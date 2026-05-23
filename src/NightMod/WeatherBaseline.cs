using Mafi;
using Mafi.Core.Environment;

namespace NightMod;

/// <summary>
/// Captures the original <see cref="WeatherProto.SunIntensity"/> of one weather condition, taken
/// once when the mod loads. The cycle scales sun intensity down from this baseline so solar panels
/// follow the day-night cycle, and restores it from here if the mod is disabled.
///
/// Light and sky colors are not snapshotted - those are read live from the (untouched) prototype
/// and re-lit per frame by <see cref="NightCycleRenderer"/>.
/// </summary>
public readonly struct WeatherBaseline {

	/// <summary>The weather condition this baseline belongs to.</summary>
	public readonly WeatherProto Weather;

	/// <summary>Original sun intensity that solar panels read (<see cref="WeatherProto.SunIntensity"/>).</summary>
	public readonly Percent SunIntensity;

	public WeatherBaseline(WeatherProto weather) {
		Weather = weather;
		SunIntensity = weather.SunIntensity;
	}
}
