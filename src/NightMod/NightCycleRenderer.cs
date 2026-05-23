using System.Reflection;
using Mafi;
using Mafi.Core;
using Mafi.Core.Environment;
using Mafi.Core.GameLoop;
using Mafi.Unity;
using Mafi.Unity.Camera;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiStatic;
using Mafi.Unity.Weather;
using NightMod.LampPosts;
using UnityEngine;

namespace NightMod;

/// <summary>
/// Visual half of the cycle. Every frame it reads the active weather's original look, blends in
/// the cycle - dimming, the warm sunrise/sunset tint and the cool night tint - and pushes the
/// result into the game's light, skybox, ambient light and the directional light's rotation. The
/// sun arcs across the sky, shadows sweep and lengthen, and the sky shifts color through the day.
///
/// The cycle position is taken from a continuous sim-time clock so the motion is perfectly smooth,
/// and weather changes are cross-faded over a few seconds instead of snapping. The game itself
/// only refreshes these during weather transitions, hence the per-frame drive. Created only when
/// the game is rendering (skipped in headless sessions).
/// </summary>
public sealed class NightCycleRenderer {

	// 20 sim steps make one in-game day (7200 ticks per year / 360 days).
	private const float SimStepsPerDay = 20f;

	// Real-time seconds a weather change is cross-faded over.
	private const float WeatherTransitionSeconds = 7f;

	// Real-time seconds the sunrise/sunset palette takes to swing from one to the other, so the
	// turn between them averages the two colors instead of snapping.
	private const float RisingBlendSeconds = 8f;

	// Sun height: high at midday, dipping just below the horizon at night so it visibly sets.
	private const float MinSunElevation = -12f;
	private const float MaxSunElevation = 80f;

	// Elevation band around the horizon over which the warm sunrise/sunset tint applies; it peaks
	// when the sun is exactly on the horizon, so the colors track the sun's real position.
	private const float DuskElevationRange = 12f;

	// How far the sun must drop below the horizon for the cool night tint to reach full strength.
	private const float NightElevationRange = 9f;

	// Sun elevations between which daylight fades: full daylight above the high mark, full night
	// below the low mark - so the world dims only as the sun gets genuinely low.
	private const float DaylightHighElevation = 18f;
	private const float DaylightLowElevation = -6f;

	// Headlights and lamp posts switch on once the scene's directional light intensity drops below
	// this. Keying off the actual light level - not the sun's angle - means the lights also come on
	// when heavy weather darkens the day, just like real automatic headlights.
	private const float LightsOnLightIntensity = 0.60f;
	private const float FullLightsOnLightIntensity = 0.30f;

	// Sky height over which the moon fades in as it rises above the horizon.
	private const float MoonFadeRange = 15f;

	// Moon height: its peak above the horizon at night, and its trough below it during the day.
	private const float MoonPeakElevation = 42f;
	private const float MoonTroughElevation = -30f;

	// Smallest lit fraction the moon is ever drawn at, so a near-new moon still shows a thin
	// crescent instead of vanishing entirely.
	private const float MinMoonLitFraction = 0.12f;

	// Cloud cover between which the stars fade: at or below the clear mark they show at full
	// strength, at or above the overcast mark they are fully washed out. The averaged cloud
	// intensity is ~0.2 in sunny weather and ~0.6 when cloudy, so this shows stars on clear nights
	// only.
	private const float StarsClearCloudIntensity = 0.25f;
	private const float StarsOvercastCloudIntensity = 0.5f;

	// In-game cycles the starfield takes to wheel one full turn about the celestial pole. Drifting
	// this slowly - rather than a whole revolution per cycle - keeps a constellation up across the
	// whole night instead of sweeping it down past the horizon soon after it rises.
	private const float StarsRotationCycles = 6f;

	// Fraction of the gap from the camera up to the skybox ceiling that the star dome radius fills.
	// The game's skybox is a flattened, depth-writing dome, so a star drawn beyond it is hidden;
	// keeping the whole dome a little inside the ceiling leaves every star in front of the skybox.
	private const float StarsDomeFill = 0.85f;

	// Star dome radius used when the skybox cannot be measured, as a fraction of the far clip plane.
	private const float StarsFallbackRadiusFraction = 0.15f;

	// Sun elevation at/below which the horizon mask casts. Set a little above the horizon so the
	// mask engages slightly before sunset - the world eases into night shadow while the sun is
	// still grazing the horizon, instead of snapping exactly at the horizon crossing.
	private const float MaskActivationElevation = 3f;

	// Strength of the warm dawn/dusk tint per element, 0..1. The sky is a single global color, so
	// its tint is kept subtle - the directional warmth comes from the sun's light.
	private const float LightDuskStrength = 0.7f;
	private const float SkyDuskStrength = 0.15f;
	private const float FogDuskStrength = 0.7f;
	private const float CloudDuskStrength = 0.35f;

	// Strength of the cool deep-night tint per element, 0..1.
	private const float LightNightStrength = 0.5f;
	private const float SkyNightStrength = 0.6f;
	private const float FogNightStrength = 1f;
	private const float CloudNightStrength = 1f;

	/// <summary>Daytime cloud color, matching the game's private SKY_CLOUDS_COLOR constant.</summary>
	private static readonly Color CloudColorDay = new Color(0.8f, 0.8f, 0.8f);

	/// <summary>Warm sunrise/sunset tint blended into light, fog and clouds at dawn and dusk.</summary>
	private static readonly Color DuskWarmColor = new Color(1f, 0.5f, 0.2f);

	/// <summary>Purple tint blended into the sky at sunset.</summary>
	private static readonly Color DuskSkyColor = new Color(0.5f, 0.28f, 0.55f);

	/// <summary>
	/// Cooler sunrise counterparts of the dusk tints. A real dawn reads bluer than a warm sunset,
	/// so while the sun is climbing these cool tones replace the orange and purple sunset ones.
	/// </summary>
	private static readonly Color DawnWarmColor = new Color(0.55f, 0.62f, 0.9f);
	private static readonly Color DawnSkyColor = new Color(0.32f, 0.42f, 0.72f);

	/// <summary>Dark, only faintly cool tint the light and sky are blended toward at deepest night.</summary>
	private static readonly Color NightColor = new Color(0.14f, 0.15f, 0.21f);

	/// <summary>Fog goes fully black at the deepest night.</summary>
	private static readonly Color FogNightColor = Color.black;

	/// <summary>Clouds go dark gray at the deepest night.</summary>
	private static readonly Color CloudNightColor = new Color(0.18f, 0.18f, 0.2f);

	/// <summary>
	/// Near-neutral ambient light the scene keeps at night - enough that terrain textures still
	/// render, but only faintly cool, so the night does not turn uniformly blue.
	/// </summary>
	private static readonly Color NightAmbientColor = new Color(0.19f, 0.2f, 0.23f);

	private static readonly FieldInfo s_lightField =
		typeof(LightController).GetField("m_light", BindingFlags.NonPublic | BindingFlags.Instance);

	private static readonly FieldInfo s_skyboxField =
		typeof(SkyboxController).GetField("m_skyBox", BindingFlags.NonPublic | BindingFlags.Instance);

	private readonly IWeatherManager m_weatherManager;
	private readonly LightController m_lightController;
	private readonly SkyboxController m_skyboxController;
	private readonly MeshRenderer m_skyboxRenderer;
	private readonly CameraController m_cameraController;
	private readonly NightCycleManager m_cycleManager;
	private readonly NightModSettings m_settings;
	private readonly Light m_sun;
	private readonly Vector3 m_originalSunAngles;
	private readonly LightShadows m_originalSunShadows;
	private readonly NightMoon m_moon;
	private readonly NightSun m_sunDisc;
	private readonly NightStars m_stars;
	private readonly NightHeadlights m_headlights;
	private readonly NightLampLights m_lampLights;
	private readonly NightHorizonMask m_horizonMask;
	private readonly NightHorizonMask m_moonMask;
	private readonly NightCursorLight m_cursorLight;
	private readonly BuildModesStripsActivator m_buildMode;
	private readonly TerrainCursor m_terrainCursor;

	private bool m_initialized;
	private float m_duskFactor;
	private float m_nightFactor;
	private float m_prevSunElevation;
	private float m_risingFactor;

	private WeatherProto m_fromWeather;
	private WeatherProto m_toWeather;
	private float m_weatherBlend;

	private bool m_ambientCaptured;
	private Color m_baseAmbientSky;
	private Color m_baseAmbientEquator;
	private Color m_baseAmbientGround;
	private float m_baseAmbientIntensity;

	public NightCycleRenderer(IGameLoopEvents gameLoopEvents, IWeatherManager weatherManager,
		LightController lightController, SkyboxController skyboxController,
		CameraController cameraController, NightCycleManager cycleManager, NightModSettings settings,
		BuildModesStripsActivator buildMode, NewInstanceOf<TerrainCursor> terrainCursor) {
		m_weatherManager = weatherManager;
		m_lightController = lightController;
		m_skyboxController = skyboxController;
		// The skybox GameObject is needed to size the star dome to nest inside it; reach it through
		// the same private-field reflection used for the directional light above.
		m_skyboxRenderer = (s_skyboxField?.GetValue(skyboxController) as GameObject)?.GetComponent<MeshRenderer>();
		if (m_skyboxRenderer == null) {
			Log.Warning("NightMod: skybox renderer not found, the star dome will use a fallback size.");
		}
		m_cameraController = cameraController;
		m_cycleManager = cycleManager;
		m_settings = settings;
		m_buildMode = buildMode;
		// The terrain cursor projects the mouse straight onto the terrain heightmap, so the
		// flashlight tracks the cursor everywhere - not only where it happens to sit over an
		// entity, as the entity picker's last-picked coordinate did. It must be activated to
		// start its per-frame recompute.
		m_terrainCursor = terrainCursor.Instance;
		m_terrainCursor.Activate();
		m_sun = s_lightField?.GetValue(lightController) as Light;
		m_originalSunAngles = m_sun != null ? m_sun.transform.eulerAngles : Vector3.zero;
		m_originalSunShadows = m_sun != null ? m_sun.shadows : LightShadows.Soft;
		if (m_sun == null) {
			Log.Warning("NightMod: directional light not found, the sun will not move.");
		}
		m_moon = new NightMoon();
		m_sunDisc = new NightSun();
		m_stars = new NightStars();
		m_headlights = new NightHeadlights();
		m_lampLights = new NightLampLights();
		m_horizonMask = new NightHorizonMask();
		m_moonMask = new NightHorizonMask();
		m_cursorLight = new NightCursorLight();
		gameLoopEvents.RenderUpdate.AddNonSaveable(this, onRenderUpdate);
		gameLoopEvents.Terminate.AddNonSaveable(this, onTerminate);
		Log.Info("NightMod: cycle renderer ready.");
	}

	private void onRenderUpdate(GameTime time) {
		if (!m_settings.Enabled) {
			if (m_initialized) {
				// Hand control back to the game: restore the sun and the scene's ambient light.
				if (m_sun != null) {
					m_sun.transform.eulerAngles = m_originalSunAngles;
					m_sun.shadows = m_originalSunShadows;
				}
				restoreAmbient();
				m_moon.Hide();
				m_sunDisc.Hide();
				m_stars.Hide();
				m_headlights.HideAll();
				m_lampLights.HideAll();
				m_horizonMask.Hide();
				m_moonMask.Hide();
				m_cursorLight.Hide();
				m_initialized = false;
			}
			return;
		}

		// Continuous cycle position from a smooth sim-time clock (frozen while the game is paused).
		int cycleDays = Mathf.Max(1, m_cycleManager.CycleLengthDays);
		double day = (time.SimStepsCount + (double)time.AbsoluteT) / SimStepsPerDay;
		// One day-night cycle is one day of the seasonal year, and the year spans 360 cycles. The
		// day-of-year is the cycle count - NOT the fast in-game calendar - so the season advances at
		// a steady one-day-per-cycle pace. This keeps the sun's arc within a single cycle clean even
		// at nordic latitudes, where day length is extremely sensitive to the date: tying it to the
		// in-game calendar (many in-game days per cycle) made the declination lurch mid-arc.
		double cycleCount = day / cycleDays;
		float phase = (float)(cycleCount % 1.0);
		// Sun position from the spherical-planet solar model: the configured latitude and the
		// day-of-year decide how high the sun climbs and how long it stays up, so winter days are
		// genuinely shorter than summer ones. 'phase' is the time of day, 0.5 = solar noon.
		double dayOfYear = cycleCount % SolarSky.DaysPerYear;
		SolarSky.SunPosition(m_settings.Latitude, dayOfYear, phase,
			out float sunElevation, out float solarAzimuth);
		// Optional override: freeze the sun at a player-set position, ignoring the cycle. The rest
		// of the pipeline (light, shadows, sky tint, the mask) follows the elevation, so this holds
		// the whole scene at a fixed time of day.
		bool overrideSun = m_settings.OverrideSunPosition;
		if (overrideSun) {
			sunElevation = (float)m_settings.OverrideSunElevation;
		}
		// Daylight tracks the sun's height: full brightness while the sun is well up, dimming only
		// once it nears the horizon - so the world does not darken while the sun is still high.
		float dayFactor = Mathf.Clamp01(
			(sunElevation - DaylightLowElevation) / (DaylightHighElevation - DaylightLowElevation));
		// The warm tint is tied to the sun itself - it peaks as the sun touches the horizon - so
		// the sunrise/sunset colors always line up with where the sun actually is.
		m_duskFactor = Mathf.Clamp01(1f - Mathf.Abs(sunElevation) / DuskElevationRange);
		// The cool night tint ramps in only once the sun is below the horizon - so it does not
		// wash out the warm dusk colors while the sun is still at the horizon.
		m_nightFactor = Mathf.Clamp01(-sunElevation / NightElevationRange);
		float minBrightness = m_settings.MinBrightness;
		float lightFactor = minBrightness + (1f - minBrightness) * dayFactor;

		// Sunrise reads cooler than sunset. Rather than snapping between the warm and cool palettes
		// when the sun turns around, a 0..1 rising factor is eased toward its target, so during the
		// change the colors pass smoothly through the average of the two. This matters most at
		// nordic latitudes, where the sun lingers near the horizon through a long dusk and the
		// sunset gives way to the sunrise while the tint is still in full view. The factor only
		// moves while the sun actually moves, so a paused sunset holds its palette.
		if (sunElevation > m_prevSunElevation) {
			m_risingFactor = Mathf.MoveTowards(m_risingFactor, 1f, Time.deltaTime / RisingBlendSeconds);
		} else if (sunElevation < m_prevSunElevation) {
			m_risingFactor = Mathf.MoveTowards(m_risingFactor, 0f, Time.deltaTime / RisingBlendSeconds);
		}
		m_prevSunElevation = sunElevation;
		Color duskWarm = Color.Lerp(DuskWarmColor, DawnWarmColor, m_risingFactor);
		Color duskSky = Color.Lerp(DuskSkyColor, DawnSkyColor, m_risingFactor);

		WeatherLook weather = blendedWeather(time);

		// The sun's light fades out as it drops to the horizon and is effectively off below it,
		// but a tiny intensity floor is always kept since fully disabling the directional light
		// breaks the game's render pipeline.
		float sunVisibility = Mathf.Clamp01((sunElevation + 3f) / 8f);
		float lightIntensity = Mathf.Max(weather.LightIntensity * lightFactor * sunVisibility, 0.01f);
		float shadows = weather.ShadowsIntensityAbs * lightFactor;
		// Light color is only hue-shifted - brightness is handled by the intensity above.
		Color lightColor = tint(weather.LightColor, duskWarm, LightDuskStrength, NightColor, LightNightStrength);
		// Sky, fog and clouds are the rendered colors themselves, so they are dimmed as well.
		Color skyColor = tint(scale(weather.SkyColor, lightFactor), duskSky, SkyDuskStrength, NightColor, SkyNightStrength);
		Color fogColor = tint(scale(weather.FogColor, lightFactor), duskWarm, FogDuskStrength, FogNightColor, FogNightStrength);
		Color cloudColor = tint(scale(CloudColorDay, lightFactor), duskWarm, CloudDuskStrength, CloudNightColor, CloudNightStrength);

		m_lightController.SetLightIntensity(lightIntensity, shadows);
		m_lightController.SetLightColor(lightColor);

		// Move the sun: it sweeps a full circle over the cycle on a smooth arc - rising from the
		// horizon at sunrise, peaking at midday and setting below the horizon at night. Its shadow
		// casting stays on at every elevation: while the sun is below the horizon its light falls
		// on the horizon mask, which casts the single map-wide shadow that darkens the night and
		// swallows the stray fragments a low sun would otherwise scatter.
		// Azimuth base comes from config (player-editable, persisted), not the captured light. When
		// the position is overridden the configured azimuth is used directly as the absolute angle.
		float sunAzimuth = overrideSun
			? (float)m_settings.OverrideSunAzimuth
			: (float)m_settings.SunAzimuth + solarAzimuth;
		if (m_sun != null) {
			m_sun.transform.eulerAngles = new Vector3(sunElevation, sunAzimuth, m_originalSunAngles.z);
			m_sun.shadows = m_originalSunShadows;
		}

		// Mod-drawn sun disc: a small circle on the sun's arc, tinted to the current sunlight
		// color, that vanishes the moment the sun reaches the horizon.
		float sunDiscVisibility = Mathf.Clamp01(sunElevation / 2f);
		m_sunDisc.Update(m_cameraController.Camera, sunElevation, sunAzimuth, sunDiscVisibility, lightColor);

		// The moon runs on its own clock, independent of the sun: its phase drifts an extra lap
		// every 30 in-game days (the lunar month), so day to day it rises and sets at its own
		// times instead of rigidly mirroring the sun. It is also faded right down during daytime.
		float monthDrift = (float)((cycleCount / SolarSky.DaysPerMonth) % 1.0);
		double dayOfMonth = dayOfYear % SolarSky.DaysPerMonth;
		SolarSky.MoonPosition(m_settings.Latitude, dayOfYear, dayOfMonth, phase,
			out float moonElevation, out float lunarAzimuth);
		float moonAzimuth = (float)m_settings.SunAzimuth + lunarAzimuth;
		float moonVisibility = Mathf.Clamp01(moonElevation / MoonFadeRange)
			* Mathf.Lerp(0.12f, 1f, 1f - dayFactor);

		// Moon phase. The lit fraction either tracks the real sun-moon angle in the sky - so the
		// shape morphs as the moon crosses overhead - or runs on the moon's own ~30-day clock; the
		// player picks which. Either way the disc turns its lit limb toward the sun.
		Vector3 sunDirection = Quaternion.Euler(-sunElevation, sunAzimuth, 0f) * Vector3.forward;

		// The horizon mask: an invisible quad re-aimed face-on to the sunlight and parked a shadow
		// distance toward the sun, so it sits between the sun and every shadow-receiving surface in
		// view. While the sun is at or below the horizon it casts one solid, map-wide shadow, so the
		// night is real occlusion. The aiming vector is read straight off the directional light -
		// '-forward' is the direction its rays arrive from. The sun disc's 'sunDirection' above is
		// only a sky-placement vector, not the lighting axis, so the mask must not use it.
		if (m_sun != null) {
			m_horizonMask.Update(m_cameraController.Camera, -m_sun.transform.forward,
				sunElevation <= MaskActivationElevation);
		} else {
			m_horizonMask.Hide();
		}

		float litFraction;
		if (m_settings.MoonPhaseFromSun) {
			Vector3 moonDirection = Quaternion.Euler(-moonElevation, moonAzimuth, 0f) * Vector3.forward;
			float cosElongation = Mathf.Clamp(Vector3.Dot(sunDirection, moonDirection), -1f, 1f);
			litFraction = (1f - cosElongation) * 0.5f;
		} else {
			litFraction = (1f - Mathf.Cos(monthDrift * 2f * Mathf.PI)) * 0.5f;
		}
		// Never let the moon fade to a fully invisible new moon - keep at least a thin crescent so
		// it always reads as a moon in the sky.
		litFraction = Mathf.Max(litFraction, MinMoonLitFraction);
		m_moon.Update(m_cameraController.Camera, moonElevation, moonAzimuth, moonVisibility,
			litFraction, sunDirection);

		// Moon horizon mask: the same invisible caster as the sun's, aimed instead at the moonlight.
		// It switches on only once the moonlight is itself a shadow-casting light. The moonlight is
		// shadowless by default, so today the moon mask stays dormant: a ShadowsOnly caster is drawn
		// by whichever directional light casts shadows - the sun - so an active moon mask would have
		// the sun cast a stray shadow of it. Gating on the moonlight keeps it harmless until the day
		// the moonlight is changed to cast shadows, when this wiring brings the mask to life.
		m_moonMask.Update(m_cameraController.Camera, m_moon.DirectionToMoon,
			m_moon.CastsShadows && moonElevation <= MaskActivationElevation);

		// Drive the scene's ambient toward a cool night ambient. With the sun set below the
		// horizon, this and the moonlight are what keep the terrain lit and visible at night.
		applyAmbient(dayFactor);

		// Headlights and lamps toggle on the scene's actual light level, not the sun's angle: once
		// the directional light has dimmed below the threshold - at dusk, or under heavy weather -
		// they switch on, and off again when it brightens back past it.
		float lightsLevel = lightIntensity < FullLightsOnLightIntensity ? 1f
			: lightIntensity < LightsOnLightIntensity ? 0.5f : 0f;

		// Vehicle headlight cones.
		if (m_settings.VehicleHeadlights) {
			m_headlights.Update(lightsLevel);
		} else {
			m_headlights.HideAll();
		}

		// Lamp post lights. Each lamp post still only casts a real light while it is powered.
		m_lampLights.Update(lightsLevel, m_cameraController.Camera);

		// Cursor flashlight: lights the spot under the cursor while the player is in build mode,
		// scaled by night darkness so it never adds a glow in daylight. The terrain cursor gives
		// the world point directly under the mouse whether or not an entity sits there; when the
		// mouse is off the terrain entirely (e.g. over the sky) it has no value and the light hides.
		if (m_terrainCursor.HasValue) {
			m_cursorLight.Update(m_terrainCursor.Tile3f.ToVector3(), m_buildMode.IsActive, 1f - dayFactor);
		} else {
			m_cursorLight.Hide();
		}

		// Sky, fog and clouds. When cloud cover is heavy the game blends the sky toward a fixed
		// bright cloud color; replicate that blend here with our night-tinted cloud color, then
		// pass cloud intensity 0 so the game does not blend toward its bright color on top.
		float cloudIntensity = 0.5f * (weather.MinCloudIntensity + weather.MaxCloudIntensity);
		Color skyWithClouds = cloudIntensity > 0.5f
			? Color.Lerp(skyColor, cloudColor, Mathf.Clamp01((cloudIntensity - 0.5f) * 3f))
			: skyColor;
		m_skyboxController.SetSkyColor(skyColor);
		m_skyboxController.SetFogColor(fogColor);
		m_skyboxController.UpdateSkyColor(skyWithClouds, 0f);

		// Starfield. The game has no night sky of its own, so the mod draws one: a dome of stars
		// that fades in as the sun sets (with dayFactor) and is washed out by cloud cover, so it
		// shows on clear nights and is gone on overcast ones. The dome drifts slowly about the
		// celestial pole - a point fixed at the observer's latitude above the horizon, on the same
		// compass bearing the sun's arc runs along - taking several cycles to wheel a full turn, so
		// a constellation stays up through the night instead of setting soon after it rises.
		float starCloudFactor = Mathf.InverseLerp(
			StarsOvercastCloudIntensity, StarsClearCloudIntensity, cloudIntensity);
		float starVisibility = (1f - dayFactor) * starCloudFactor;
		Vector3 celestialPole =
			Quaternion.Euler(-(float)m_settings.Latitude, (float)m_settings.SunAzimuth, 0f) * Vector3.forward;
		float starDrift = (float)(cycleCount / StarsRotationCycles % 1.0) * 360f;
		Quaternion starRotation = Quaternion.AngleAxis(starDrift, celestialPole);
		m_stars.Update(m_cameraController.Camera, starRotation, starDomeRadius(), starVisibility);

		m_initialized = true;
	}

	/// <summary>
	/// The weather look to render right now. When the game switches weather this cross-fades from
	/// the previous condition to the new one over <see cref="WeatherTransitionSeconds"/>, so the
	/// change eases in instead of snapping.
	/// </summary>
	private WeatherLook blendedWeather(GameTime time) {
		WeatherProto current = m_weatherManager.CurrentWeather;
		if (!m_initialized) {
			m_fromWeather = current;
			m_toWeather = current;
			m_weatherBlend = 1f;
		} else if (current != m_toWeather) {
			m_fromWeather = m_toWeather;
			m_toWeather = current;
			m_weatherBlend = 0f;
		}
		if (!time.IsGamePaused) {
			m_weatherBlend = Mathf.Min(1f, m_weatherBlend + Time.deltaTime / WeatherTransitionSeconds);
		}

		WeatherProto.Gfx from = m_fromWeather.Graphics;
		WeatherProto.Gfx to = m_toWeather.Graphics;
		float t = m_weatherBlend;
		return new WeatherLook(
			Mathf.Lerp(from.LightIntensity, to.LightIntensity, t),
			Mathf.Lerp(from.ShadowsIntensityAbs, to.ShadowsIntensityAbs, t),
			Mathf.Lerp(from.MinCloudIntensity, to.MinCloudIntensity, t),
			Mathf.Lerp(from.MaxCloudIntensity, to.MaxCloudIntensity, t),
			Color.Lerp(from.LightColor.AsColor(), to.LightColor.AsColor(), t),
			Color.Lerp(from.SkyColor.AsColor(), to.SkyColor.AsColor(), t),
			Color.Lerp(from.FogColor.AsColor(), to.FogColor.AsColor(), t));
	}

	/// <summary>
	/// Blends a base color toward the warm dawn/dusk tint and then toward the cool night tint, by
	/// the given per-element strengths and the current cycle factors.
	/// </summary>
	private Color tint(Color baseColor, Color duskColor, float duskStrength, Color nightColor,
		float nightStrength) {
		Color withDusk = Color.Lerp(baseColor, duskColor, m_duskFactor * duskStrength);
		return Color.Lerp(withDusk, nightColor, m_nightFactor * nightStrength);
	}

	/// <summary>
	/// Blends the scene's ambient light from its captured daytime values toward a functional cool
	/// night ambient. With the sun switched off below the horizon, this ambient is what guarantees
	/// the terrain textures keep rendering at night - it is a rendering floor, not a mood control.
	/// Both ambient colors and intensity are driven so it works whichever ambient mode is in use.
	/// </summary>
	private void applyAmbient(float dayFactor) {
		if (!m_ambientCaptured) {
			m_baseAmbientSky = RenderSettings.ambientLight;
			m_baseAmbientEquator = RenderSettings.ambientEquatorColor;
			m_baseAmbientGround = RenderSettings.ambientGroundColor;
			m_baseAmbientIntensity = RenderSettings.ambientIntensity;
			m_ambientCaptured = true;
		}
		RenderSettings.ambientLight = Color.Lerp(NightAmbientColor, m_baseAmbientSky, dayFactor);
		RenderSettings.ambientEquatorColor = Color.Lerp(NightAmbientColor, m_baseAmbientEquator, dayFactor);
		RenderSettings.ambientGroundColor = Color.Lerp(NightAmbientColor * 0.7f, m_baseAmbientGround, dayFactor);
		RenderSettings.ambientIntensity = Mathf.Lerp(m_baseAmbientIntensity * 0.4f, m_baseAmbientIntensity, dayFactor);
	}

	/// <summary>Restores the scene's ambient light to the values captured before the cycle ran.</summary>
	private void restoreAmbient() {
		if (!m_ambientCaptured) {
			return;
		}
		RenderSettings.ambientLight = m_baseAmbientSky;
		RenderSettings.ambientEquatorColor = m_baseAmbientEquator;
		RenderSettings.ambientGroundColor = m_baseAmbientGround;
		RenderSettings.ambientIntensity = m_baseAmbientIntensity;
	}

	/// <summary>Multiplies a color's RGB by a factor, leaving alpha untouched.</summary>
	private static Color scale(Color color, float factor) {
		return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
	}

	/// <summary>
	/// World radius for the star dome: small enough that the whole dome sits inside the game's
	/// skybox, which is a flattened, depth-writing dome - a star drawn past it is hidden, which is
	/// why a large star sphere shows only a belt of stars around the horizon. The binding limit is
	/// the vertical gap from the camera up to the skybox ceiling, since the skybox is far wider
	/// than it is tall. Falls back to a fraction of the far clip plane if the skybox is missing.
	/// </summary>
	private float starDomeRadius() {
		Camera camera = m_cameraController.Camera;
		if (m_skyboxRenderer != null) {
			Bounds bounds = m_skyboxRenderer.bounds;
			float ceilingGap = bounds.center.y + bounds.extents.y - camera.transform.position.y;
			if (ceilingGap > 1f) {
				return ceilingGap * StarsDomeFill;
			}
		}
		return camera.farClipPlane * StarsFallbackRadiusFraction;
	}

	private void onTerminate() {
		m_moon.Destroy();
		m_sunDisc.Destroy();
		m_stars.Destroy();
		m_headlights.Destroy();
		m_horizonMask.Destroy();
		m_moonMask.Destroy();
		m_cursorLight.Destroy();
		m_terrainCursor.Deactivate();
	}

	/// <summary>Weather appearance reduced to the renderer-facing values, in Unity-native types.</summary>
	private readonly struct WeatherLook {

		public readonly float LightIntensity;
		public readonly float ShadowsIntensityAbs;
		public readonly float MinCloudIntensity;
		public readonly float MaxCloudIntensity;
		public readonly Color LightColor;
		public readonly Color SkyColor;
		public readonly Color FogColor;

		public WeatherLook(float lightIntensity, float shadowsIntensityAbs, float minCloudIntensity,
			float maxCloudIntensity, Color lightColor, Color skyColor, Color fogColor) {
			LightIntensity = lightIntensity;
			ShadowsIntensityAbs = shadowsIntensityAbs;
			MinCloudIntensity = minCloudIntensity;
			MaxCloudIntensity = maxCloudIntensity;
			LightColor = lightColor;
			SkyColor = skyColor;
			FogColor = fogColor;
		}
	}
}
