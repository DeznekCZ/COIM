using System;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Environment;
using Mafi.Core.Prototypes;
using Mafi.Core.Simulation;

namespace NightMod;

/// <summary>
/// Drives the day-night cycle on the simulation side. Once per in-game day it scales each weather
/// condition's sun intensity so solar panels follow the cycle - that is the only prototype value
/// it changes, and the only part of the cycle that must be deterministic. It also publishes the
/// cycle length so <see cref="NightCycleRenderer"/> can place the visuals on the same timeline.
///
/// Registered as a global dependency so it runs in both rendered and headless games, keeping
/// solar power identical on every client.
/// </summary>
[GlobalDependency(RegistrationMode.AsSelf)]
public sealed class NightCycleManager {

	// Sun elevation (degrees) at which solar output reaches full power. The cutoff (no power)
	// elevation is player-configurable; see NightModSettings.SolarCutoffElevation, clamped below this.
	private const int SolarFullElevation = 20;

	private readonly ICalendar m_calendar;
	private readonly NightModSettings m_settings;
	private readonly Lyst<WeatherBaseline> m_baselines;

	/// <summary>Length of one full day-night cycle in in-game days. Read by the renderer.</summary>
	public int CycleLengthDays { get; private set; }

	public NightCycleManager(ProtosDb protosDb, ICalendar calendar, NightModSettings settings) {
		m_calendar = calendar;
		m_settings = settings;
		m_baselines = new Lyst<WeatherBaseline>();
		foreach (WeatherProto weather in protosDb.All<WeatherProto>()) {
			m_baselines.Add(new WeatherBaseline(weather));
		}
		CycleLengthDays = 1;

		// NewDay is a deterministic sim-thread event, so solar power stays in lock-step across
		// clients. AddNonSaveable keeps this service out of the save file - it is recreated on
		// every load and re-subscribes here, so the subscription is always fresh.
		calendar.NewDay.AddNonSaveable(this, onNewDay);
		settings.OnValueChanged += onSettingChanged;

		applyCurrentState();
		Log.Info($"NightMod: cycle manager ready, tracking {m_baselines.Count} weather conditions.");
	}

	private void onNewDay() {
		applyCurrentState();
	}

	private void onSettingChanged(string parameterName) {
		applyCurrentState();
	}

	/// <summary>Recomputes the cycle length and re-scales every weather's sun intensity.</summary>
	private void applyCurrentState() {
		CycleLengthDays = cycleLengthInDays();
		if (!m_settings.Enabled) {
			restoreBaselines();
			return;
		}

		int totalDays = m_calendar.CurrentDate.RelGameDate.TotalDays;
		int dayInCycle = totalDays % CycleLengthDays;

		// Solar output is the cycle's one piece of shared game state, so it is computed entirely in
		// deterministic fixed-point (Fix32 / Fix64 / AngleDegrees1f) - every client derives
		// bit-identical panel power. The day-of-year is the (fractional) cycle count - one day-night
		// cycle is one day of a 360-cycle seasonal year - and the time of day is the cycle position.
		Fix64 sinElevation;
		if (m_settings.OverrideSunPosition) {
			// A player-set sun override freezes solar output to match the fixed sun in the world.
			sinElevation = AngleDegrees1f
				.FromDegrees(Fix32.FromDouble(m_settings.OverrideSunElevation)).Sin();
		} else {
			AngleDegrees1f latitude =
				AngleDegrees1f.FromDegrees(Fix32.FromDouble(m_settings.Latitude));
			Fix32 dayOfYear = Fix32.FromFraction(totalDays, CycleLengthDays);
			Fix32 timeOfDay = Fix32.FromFraction(dayInCycle, CycleLengthDays);
			sinElevation = SolarSky.SunSineElevation(latitude, dayOfYear, timeOfDay);
		}

		// Solar output ramps from none at the cutoff elevation to full at the high mark. The ramp
		// runs on the sine of the elevation (no inverse trig), keeping it deterministic.
		Fix64 sinCutoff = AngleDegrees1f
			.FromDegrees(Fix32.FromDouble(m_settings.SolarCutoffElevation)).Sin();
		Fix64 sinFull = AngleDegrees1f.FromDegrees(Fix32.FromInt(SolarFullElevation)).Sin();
		Percent dayFactor =
			Percent.FromRatio(sinElevation - sinCutoff, sinFull - sinCutoff).Clamp0To100();

		foreach (WeatherBaseline baseline in m_baselines) {
			// Sun intensity drops all the way to zero at midnight so solar panels read near-zero
			// power during the night - the renderer's brightness floor does not apply here.
			Percent sunIntensity = m_settings.AffectSolar
				? dayFactor.Apply(baseline.SunIntensity)
				: baseline.SunIntensity;
			WeatherProtoReflection.SetSunIntensity(baseline.Weather, sunIntensity);
		}
	}

	/// <summary>Restores the original sun intensity of every weather condition.</summary>
	private void restoreBaselines() {
		foreach (WeatherBaseline baseline in m_baselines) {
			WeatherProtoReflection.SetSunIntensity(baseline.Weather, baseline.SunIntensity);
		}
	}

	/// <summary>
	/// Length of one full cycle in in-game days. The player configures the cycle in real-time
	/// minutes; this converts that through the game's fixed real-to-game time ratio
	/// (<see cref="ICalendar.DurationToRelTime"/>) so the cycle stays deterministic - keeping solar
	/// power in sync across clients - while still lasting the requested real duration at normal
	/// game speed.
	/// </summary>
	private int cycleLengthInDays() {
		Duration realDuration = Duration.FromMin(m_settings.CycleLengthMinutes);
		return Math.Max(1, m_calendar.DurationToRelTime(realDuration).TotalDays);
	}
}
