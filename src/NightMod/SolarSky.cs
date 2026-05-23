using System;
using Mafi;

namespace NightMod;

/// <summary>
/// Simplified astronomical positioning of the sun and moon. Given the observer's latitude, the
/// in-game date and the time of day, it computes the sun's elevation and azimuth from the
/// standard solar-position equations, then derives the moon as the same arc shifted by the lunar
/// month phase - so the moon drifts about one step further each in-game day and laps once per
/// in-game month.
///
/// The Mafi calendar has 30-day months and a 360-day year (see <see cref="DaysPerMonth"/> /
/// <see cref="DaysPerYear"/>). All angles are in degrees. This is pure math with no game
/// dependencies, so it can be reasoned about - and unit-tested - in isolation.
///
/// Wiring (done elsewhere, once the project builds): feed <c>SunPosition</c> / <c>MoonPosition</c>
/// the configured latitude, the in-game day-of-year and day-of-month, and a time-of-day in 0..1
/// where 0.5 is solar noon. Use the returned elevation/azimuth for the directional light and the
/// moon billboard instead of <see cref="CycleCurve.SunElevation"/> / <see cref="CycleCurve.MoonElevation"/>.
/// </summary>
public static class SolarSky {

	/// <summary>In-game days per month (Mafi calendar) - the moon's drift period.</summary>
	public const int DaysPerMonth = 30;

	/// <summary>In-game days per year (Mafi calendar) - the seasonal declination period.</summary>
	public const int DaysPerYear = 360;

	// Axial tilt: the maximum solar declination, reached at the solstices.
	private const double AxialTiltDeg = 23.45;

	// Day of the in-game year on which declination crosses zero going positive (spring equinox).
	// Placed at ~March 20 on a 30-day-month calendar (day 2*30 + 19), so day-of-year 0 - the start
	// of the year, "January" - sits just after the winter solstice: the year opens on short
	// northern-hemisphere days that lengthen into spring.
	private const int SpringDay = 79;

	private const double Deg2Rad = Math.PI / 180.0;
	private const double Rad2Deg = 180.0 / Math.PI;

	/// <summary>
	/// Sun position for the given moment. <paramref name="latitudeDeg"/> is the observer latitude
	/// (-90..90), <paramref name="dayOfYear"/> is the in-game day of year and may be fractional -
	/// pass a continuous value so the seasonal day length drifts smoothly rather than stepping -
	/// and <paramref name="timeOfDay"/> is 0..1 with 0.5 = solar noon. Outputs degrees:
	/// <paramref name="elevationDeg"/> above the horizon (negative below) and
	/// <paramref name="azimuthDeg"/> 0..360 compass-style.
	/// </summary>
	public static void SunPosition(double latitudeDeg, double dayOfYear, double timeOfDay,
		out float elevationDeg, out float azimuthDeg) {
		double declination = SolarDeclination(dayOfYear);
		computePosition(latitudeDeg, declination, timeOfDay, out elevationDeg, out azimuthDeg);
	}

	/// <summary>
	/// Moon position for the given moment. The moon follows the same arc as the sun but shifted by
	/// the lunar-month phase, so over a 30-day in-game month it drifts a full lap relative to the
	/// sun (rising noticeably later each in-game day). <paramref name="dayOfYear"/> and
	/// <paramref name="dayOfMonth"/> may be fractional - pass continuous values for smooth motion.
	/// </summary>
	public static void MoonPosition(double latitudeDeg, double dayOfYear, double dayOfMonth,
		double timeOfDay, out float elevationDeg, out float azimuthDeg) {
		double monthPhase = wrap01(dayOfMonth / DaysPerMonth);
		double moonTime = wrap01(timeOfDay + monthPhase);
		// The moon's declination stays close to the sun's; reusing it keeps the arc simple and stable.
		double declination = SolarDeclination(dayOfYear);
		computePosition(latitudeDeg, declination, moonTime, out elevationDeg, out azimuthDeg);
	}

	/// <summary>
	/// Solar declination in degrees for the given in-game day of year, which may be fractional. The
	/// sine is naturally periodic, so no day-of-year wrapping is needed.
	/// </summary>
	public static double SolarDeclination(double dayOfYear) {
		return AxialTiltDeg * Math.Sin(2.0 * Math.PI * (dayOfYear - SpringDay) / DaysPerYear);
	}

	/// <summary>
	/// Deterministic sine of the sun's elevation, for the simulation side. Every step is fixed-point
	/// (<see cref="Fix32"/> / <see cref="Fix64"/> / <see cref="AngleDegrees1f"/>) so all clients
	/// derive bit-identical solar output. Working with the sine - rather than the elevation angle -
	/// avoids inverse trig, which the fixed-point types do not provide. <paramref name="dayOfYear"/>
	/// and <paramref name="timeOfDay"/> are continuous; <paramref name="timeOfDay"/> 0.5 is solar
	/// noon. The result is in [-1, 1].
	/// </summary>
	public static Fix64 SunSineElevation(AngleDegrees1f latitude, Fix32 dayOfYear, Fix32 timeOfDay) {
		// Declination = AxialTilt * sin(360deg * (dayOfYear - SpringDay) / 360); the 360s cancel, so
		// the year angle in degrees is simply (dayOfYear - SpringDay).
		AngleDegrees1f yearAngle = AngleDegrees1f.FromDegrees(dayOfYear - Fix32.FromInt(SpringDay));
		Fix64 declinationDeg = Fix64.FromDouble(AxialTiltDeg) * yearAngle.Sin();
		AngleDegrees1f declination = AngleDegrees1f.FromDegrees(declinationDeg.ToFix32());
		// Hour angle: 0 at solar noon (timeOfDay 0.5), +/-180 degrees at midnight.
		AngleDegrees1f hourAngle = AngleDegrees1f.FromDegrees((timeOfDay - Fix32.Half) * 360);
		// sin(elevation) = sin(lat)sin(dec) + cos(lat)cos(dec)cos(hourAngle).
		Fix64 sinElevation = latitude.Sin() * declination.Sin()
			+ latitude.Cos() * declination.Cos() * hourAngle.Cos();
		return sinElevation.Clamp(-Fix64.One, Fix64.One);
	}

	/// <summary>
	/// Solar/lunar elevation and azimuth from latitude, declination and time of day, using the
	/// standard hour-angle equations.
	/// </summary>
	private static void computePosition(double latitudeDeg, double declinationDeg, double timeOfDay,
		out float elevationDeg, out float azimuthDeg) {
		double lat = latitudeDeg * Deg2Rad;
		double dec = declinationDeg * Deg2Rad;
		// Hour angle: 0 at solar noon (timeOfDay 0.5), +/-180 degrees at midnight.
		double hourAngle = (wrap01(timeOfDay) - 0.5) * 360.0 * Deg2Rad;

		double sinElevation = clamp(
			Math.Sin(lat) * Math.Sin(dec) + Math.Cos(lat) * Math.Cos(dec) * Math.Cos(hourAngle),
			-1.0, 1.0);
		double elevation = Math.Asin(sinElevation);

		double cosElevation = Math.Cos(elevation);
		double cosLat = Math.Cos(lat);
		double azimuth;
		if (cosElevation < 1e-6 || Math.Abs(cosLat) < 1e-6) {
			// Degenerate (sun at the zenith, or observer at a pole) - azimuth is undefined.
			azimuth = 0.0;
		} else {
			double cosAzimuth = clamp(
				(Math.Sin(dec) - Math.Sin(elevation) * Math.Sin(lat)) / (cosElevation * cosLat),
				-1.0, 1.0);
			azimuth = Math.Acos(cosAzimuth);
			// Before noon the body is in the eastern half; after noon, mirror it to the west.
			if (hourAngle > 0.0) {
				azimuth = 2.0 * Math.PI - azimuth;
			}
		}

		elevationDeg = (float)(elevation * Rad2Deg);
		azimuthDeg = (float)(azimuth * Rad2Deg);
	}

	/// <summary>Wraps a value into the 0..1 range.</summary>
	private static double wrap01(double value) {
		value %= 1.0;
		return value < 0.0 ? value + 1.0 : value;
	}

	private static double clamp(double value, double min, double max) {
		return value < min ? min : (value > max ? max : value);
	}
}
