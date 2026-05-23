using System.Reflection;
using Mafi;
using Mafi.Core.Environment;

namespace NightMod;

/// <summary>
/// <see cref="WeatherProto.SunIntensity"/> is a public read-only field, so the mod writes it
/// through reflection to make solar panels follow the cycle. This is the only prototype value the
/// mod changes; light and sky colors are driven per-frame by <see cref="NightCycleRenderer"/>
/// instead of being baked onto the prototype.
/// </summary>
internal static class WeatherProtoReflection {

	private static readonly FieldInfo s_sunIntensity =
		typeof(WeatherProto).GetField(nameof(WeatherProto.SunIntensity),
			BindingFlags.Public | BindingFlags.Instance);

	/// <summary>Overwrites the sun intensity of a weather condition (read by solar panels).</summary>
	public static void SetSunIntensity(WeatherProto weather, Percent value) {
		s_sunIntensity.SetValue(weather, value);
	}
}
