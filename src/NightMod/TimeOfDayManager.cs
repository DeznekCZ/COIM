using System;
using Mafi;
using Mafi.Core.Simulation;

namespace NightMod;

/// <summary>
/// Single deterministic source of truth for "what time of day is it" on the simulation side.
/// Anything that has to behave differently by day and night - currently the lamp posts deciding
/// whether to draw electricity - asks this service instead of recomputing the cycle itself.
///
/// <para>It is intentionally a thin placeholder: it derives the cycle phase from the sim-step
/// count and the cycle length, the same way <see cref="NightCycleRenderer"/> does, so it is
/// deterministic and headless-safe. The fuller time-of-day model (astronomical sun position,
/// HUD-controlled time) is planned alongside the <see cref="NightCycleManager"/> rework - this
/// manager is the seam it will be resolved through later, so callers do not need to change.</para>
/// </summary>
[GlobalDependency(RegistrationMode.AsSelf)]
public sealed class TimeOfDayManager {

	// One in-game day is 20 sim steps, matching NightCycleRenderer's timeline constant.
	private const float SimStepsPerDay = 20f;

	// Below this daylight amount the cycle counts as night.
	private const float NightDayFactorThreshold = 0.5f;

	// Sun elevations between which daylight fades, matching NightCycleRenderer's constants. Used to
	// derive day/night from a frozen sun when the player overrides the sun position.
	private const double DaylightLowElevation = -6.0;
	private const double DaylightHighElevation = 18.0;

	private readonly ISimLoopEvents m_simLoopEvents;
	private readonly NightCycleManager m_cycleManager;
	private readonly NightMod m_mod;

	public TimeOfDayManager(ISimLoopEvents simLoopEvents, NightCycleManager cycleManager, NightMod mod) {
		m_simLoopEvents = simLoopEvents;
		m_cycleManager = cycleManager;
		m_mod = mod;
	}

	/// <summary>
	/// Current position in the day-night cycle, 0..1 (0 = start of daytime). Computed purely from
	/// the deterministic sim-step count, so every client and a headless host agree.
	/// </summary>
	public float Phase {
		get {
			int cycleDays = Math.Max(1, m_cycleManager.CycleLengthDays);
			double day = m_simLoopEvents.CurrentStep.Value / (double)SimStepsPerDay;
			return (float)(day / cycleDays % 1.0);
		}
	}

	/// <summary>
	/// Daylight amount right now: 1 in full daytime, 0 in deep night. When the player has frozen
	/// the sun with the "override sun position" setting, this is derived from that fixed elevation
	/// instead of the cycle clock - so lamp posts follow the overridden sun exactly as the renderer
	/// does, rather than lighting on the cycle's own schedule while the sun sits still.
	/// </summary>
	public float DayFactor {
		get {
			if (m_mod.JsonConfig.GetBool(NightModSettings.OverrideSunPositionKey)) {
				double elevation = m_mod.JsonConfig.GetDouble(NightModSettings.OverrideSunElevationKey);
				double factor = (elevation - DaylightLowElevation)
					/ (DaylightHighElevation - DaylightLowElevation);
				return (float)(factor < 0.0 ? 0.0 : (factor > 1.0 ? 1.0 : factor));
			}
			return CycleCurve.DayFactor(Phase);
		}
	}

	/// <summary>Darkness amount right now: 0 in full daytime, 1 in deep night.</summary>
	public float NightLevel => 1f - DayFactor;

	/// <summary>Whether it is currently night - the window in which lamp posts should be lit.</summary>
	public bool IsNight => DayFactor < NightDayFactorThreshold;

	/// <summary>Applies when the visual effects of the day-night cycle should be active.</summary>
	public bool IsVisual => !m_mod.JsonConfig.GetBool(NightModSettings.AffectSolarKey)
		|| !m_mod.JsonConfig.GetBool(NightModSettings.EnabledKey);
}
