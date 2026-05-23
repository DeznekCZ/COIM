using System;
using Mafi.Core.Mods;

namespace NightMod;

/// <summary>
/// Strongly-typed view over the mod's <c>config.json</c>. A single instance is created by
/// <see cref="NightMod"/> and registered with the dependency resolver so the cycle services
/// can read the player's settings. All values are read lazily, so changes made in the in-game
/// settings UI are picked up immediately.
/// </summary>
public sealed class NightModSettings {

	public const string EnabledKey = "enabled";
	public const string CycleLengthMinutesKey = "cycle_length_minutes";
	public const string MinBrightnessKey = "min_brightness";
	public const string AffectSolarKey = "affect_solar";
	public const string VehicleHeadlightsKey = "vehicle_headlights";
	public const string LatitudeKey = "latitude";
	public const string LongitudeKey = "longitude";
	public const string SunAzimuthKey = "sun_azimuth";
	public const string MoonPhaseFromSunKey = "moon_phase_from_sun";
	public const string OverrideSunPositionKey = "override_sun_position";
	public const string OverrideSunElevationKey = "override_sun_elevation";
	public const string OverrideSunAzimuthKey = "override_sun_azimuth";
	public const string SolarCutoffElevationKey = "solar_cutoff_elevation";

	private readonly ModJsonConfig m_config;

	/// <summary>Master on/off switch for the whole cycle.</summary>
	public bool Enabled => m_config.GetBool(EnabledKey);

	/// <summary>Real-time length of one full day-night-day cycle, in minutes (at least 0.1).</summary>
	public double CycleLengthMinutes => Math.Max(0.1, m_config.GetDouble(CycleLengthMinutesKey));

	/// <summary>Darkness floor at the deepest night, clamped to the 0..1 range.</summary>
	public float MinBrightness {
		get {
			double value = m_config.GetDouble(MinBrightnessKey);
			if (value < 0.0) value = 0.0;
			if (value > 1.0) value = 1.0;
			return (float)value;
		}
	}

	/// <summary>Whether the cycle also dims solar panel output.</summary>
	public bool AffectSolar => m_config.GetBool(AffectSolarKey);

	/// <summary>Whether vehicles cast headlight cones on the ground from sundown to sunrise.</summary>
	public bool VehicleHeadlights => m_config.GetBool(VehicleHeadlightsKey);

	/// <summary>Observer latitude (-90..90) used to place the sun and moon.</summary>
	public double Latitude => Math.Max(-90.0, Math.Min(90.0, m_config.GetDouble(LatitudeKey)));

	/// <summary>Observer longitude (-180..180); reserved for future time-zone offsetting.</summary>
	public double Longitude => Math.Max(-180.0, Math.Min(180.0, m_config.GetDouble(LongitudeKey)));

	/// <summary>Compass direction (degrees) the sun's arc is oriented along.</summary>
	public double SunAzimuth => m_config.GetDouble(SunAzimuthKey);

	/// <summary>
	/// When true the moon's lit fraction tracks the real sun-moon angle in the sky; when false it
	/// runs on the moon's own ~30-day phase cycle. The lit limb faces the sun in either case.
	/// </summary>
	public bool MoonPhaseFromSun => m_config.GetBool(MoonPhaseFromSunKey);

	/// <summary>
	/// When true the sun ignores the day-night cycle and is frozen at <see cref="OverrideSunElevation"/>
	/// / <see cref="OverrideSunAzimuth"/>.
	/// </summary>
	public bool OverrideSunPosition => m_config.GetBool(OverrideSunPositionKey);

	/// <summary>Fixed sun elevation (-90..90 degrees) used while the position is overridden.</summary>
	public double OverrideSunElevation =>
		Math.Max(-90.0, Math.Min(90.0, m_config.GetDouble(OverrideSunElevationKey)));

	/// <summary>Fixed sun compass direction (degrees) used while the position is overridden.</summary>
	public double OverrideSunAzimuth => m_config.GetDouble(OverrideSunAzimuthKey);

	/// <summary>
	/// Sun elevation (degrees) at or below which solar panels produce no power. Clamped to stay
	/// below the full-power elevation so the solar ramp can never collapse.
	/// </summary>
	public double SolarCutoffElevation =>
		Math.Max(-10.0, Math.Min(18.0, m_config.GetDouble(SolarCutoffElevationKey)));

	/// <summary>The underlying JSON config, for hosting in the mod's own settings window.</summary>
	public ModJsonConfig Config => m_config;

	/// <summary>Raised when the player changes any setting in the in-game settings UI.</summary>
	public event Action<string> OnValueChanged {
		add => m_config.OnValueChanged += value;
		remove => m_config.OnValueChanged -= value;
	}

	public NightModSettings(ModJsonConfig config) {
		m_config = config;
	}
}
