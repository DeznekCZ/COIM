namespace NightMod;

/// <summary>
/// Shapes the day-night cycle. One full cycle is split into four phases - daytime, sunset, night,
/// sunrise - in a 5 : 1 : 5 : 1 ratio, so the sunrise and sunset color transitions are short while
/// daytime and night are long. At the default 4-minute cycle that works out to 100 s daytime,
/// 20 s sunset, 100 s night, 20 s sunrise.
///
/// Used by both <see cref="NightCycleManager"/> (deterministic, for solar power) and
/// <see cref="NightCycleRenderer"/> (visuals) so the two always agree.
/// </summary>
public static class CycleCurve {

	// Phase boundaries of the four segments within one cycle (phase runs 0..1, 0 = daytime start).
	private const float DaytimeEnd = 5f / 12f;
	private const float SunsetEnd = 6f / 12f;
	private const float NightEnd = 11f / 12f;

	/// <summary>
	/// Daylight amount at the given cycle phase: 1 through daytime, 0 through night, easing
	/// between the two over the short sunset and sunrise windows.
	/// </summary>
	public static float DayFactor(float phase) {
		if (phase < DaytimeEnd) {
			return 1f;
		}
		if (phase < SunsetEnd) {
			return 1f - smoothStep((phase - DaytimeEnd) / (SunsetEnd - DaytimeEnd));
		}
		if (phase < NightEnd) {
			return 0f;
		}
		return smoothStep((phase - NightEnd) / (1f - NightEnd));
	}

	/// <summary>
	/// Sunrise/sunset warmth at the given cycle phase: a 0..1 bump confined to the two short
	/// transition windows, and zero throughout daytime and night.
	/// </summary>
	public static float DuskFactor(float phase) {
		if (phase >= DaytimeEnd && phase < SunsetEnd) {
			return bump((phase - DaytimeEnd) / (SunsetEnd - DaytimeEnd));
		}
		if (phase >= NightEnd) {
			return bump((phase - NightEnd) / (1f - NightEnd));
		}
		return 0f;
	}

	/// <summary>
	/// Sun height at the given cycle phase. Unlike <see cref="DayFactor"/> (which is flat through
	/// daytime), this is a smooth arc: the sun rises from the horizon at sunrise, climbs to
	/// <paramref name="peakElevation"/> at midday, sets back to the horizon at sunset, and dips to
	/// <paramref name="troughElevation"/> (below the horizon) through the night.
	/// </summary>
	public static float SunElevation(float phase, float peakElevation, float troughElevation) {
		if (phase >= SunsetEnd && phase < NightEnd) {
			// Night: the sun is below the horizon, arcing down to its trough and back.
			return troughElevation * bump((phase - SunsetEnd) / (NightEnd - SunsetEnd));
		}
		// Sunrise, daytime and sunset: the sun arcs above the horizon, peaking at midday.
		float fromSunrise = (phase - NightEnd + 1f) % 1f;
		return peakElevation * bump(fromSunrise / (1f - (NightEnd - SunsetEnd)));
	}

	/// <summary>
	/// Moon height at the given cycle phase - the mirror of <see cref="SunElevation"/>: the moon
	/// arcs above the horizon through the night and stays below it during the day.
	/// </summary>
	public static float MoonElevation(float phase, float peakElevation, float troughElevation) {
		if (phase >= SunsetEnd && phase < NightEnd) {
			// Night: the moon is up, arcing to its peak and back.
			return peakElevation * bump((phase - SunsetEnd) / (NightEnd - SunsetEnd));
		}
		// Daytime: the moon is below the horizon.
		float fromNightEnd = (phase - NightEnd + 1f) % 1f;
		return troughElevation * bump(fromNightEnd / (1f - (NightEnd - SunsetEnd)));
	}

	/// <summary>Smooth 0..1 ease (3t^2 - 2t^3), with the input clamped.</summary>
	private static float smoothStep(float t) {
		t = t < 0f ? 0f : (t > 1f ? 1f : t);
		return t * t * (3f - 2f * t);
	}

	/// <summary>0 -> 1 -> 0 bump over a 0..1 input (a half sine), with the input clamped.</summary>
	private static float bump(float t) {
		t = t < 0f ? 0f : (t > 1f ? 1f : t);
		return (float)System.Math.Sin(System.Math.PI * t);
	}
}
