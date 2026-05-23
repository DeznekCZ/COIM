using System;
using System.Linq;
using System.Reflection;
using Mafi;
using Mafi.Core;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;
using UnityEngine.UIElements;

namespace NightMod;

/// <summary>
/// Standard Window controller for <see cref="NightCycleWindow"/>. Registered via DI; its
/// constructor also docks the day-night HUD bar into the status bar, just left of the calendar /
/// date panel in the top-right corner. The bar shows the whole cycle as a color gradient -
/// night, sunrise, day, sunset - with a pointer marking the current time. Clicking it calls
/// <c>ActivateSelf()</c> to open the window.
/// </summary>
[GlobalDependency(RegistrationMode.AsSelf)]
public sealed class NightCycleWindowController : WindowController<NightCycleWindow> {

	// 20 sim steps make one in-game day.
	private const float SimStepsPerDay = 20f;

	// Bar size. It is docked flush to the left of the date panel; the width is the free space we
	// claim from the status bar there.
	private const int BarWidthPx = 275;
	private const int BarHeightPx = 12;

	// Number of solid-color slices the cycle gradient is built from - enough for a smooth fade.
	private const int GradientSegments = 64;

	private static readonly Color DayColor = new Color(0.55f, 0.78f, 1f);
	private static readonly Color NightColor = new Color(0.1f, 0.13f, 0.32f);
	private static readonly Color DuskColor = new Color(1f, 0.5f, 0.2f);
	private static readonly Color PointerColor = new Color(1f, 1f, 1f, 0.75f);

	private readonly UiContext m_uiContext;
	private readonly NightCycleManager m_cycleManager;
	private readonly NightModSettings m_settings;
	private UiComponent m_statusBar;
	private PanelRow m_bar;
	private UiComponent m_fill;
	private VisualElement m_pointer;
	private VisualElement[] m_segments;

	public NightCycleWindowController(ControllerContext controllerContext, UiContext uiContext,
		NightCycleManager cycleManager, NightModSettings settings)
		: base(controllerContext, ControllerConfig.InspectorWindow) {
		m_uiContext = uiContext;
		m_cycleManager = cycleManager;
		m_settings = settings;

		// Dock the HUD bar once the status bar exists.
		uiContext.UiRoot.Schedule
			.Execute(() => {
				m_statusBar = getLayer(uiContext.UiRoot, UiLayer.STATUS_BAR)
					.FirstOrDefault(c => c.RootElement.name == "StatusBarContainer");
			})
			.Every(1000)
			.Until(() => {
				if (m_statusBar != null) {
					initHudBar();
					return true;
				}
				return false;
			});
	}

	protected override NightCycleWindow CreateWindow() {
		return new NightCycleWindow(m_settings.Config);
	}

	/// <summary>Builds the day-night bar and docks it just left of the calendar/date panel.</summary>
	private void initHudBar() {
		// The cycle gradient lives in a child that fills the clickable panel, so the panel frame
		// stays visible and the gradient + pointer sit inside it.
		m_fill = new UiComponent().Fill();
		m_bar = new PanelRow(noBolts: true)
			.PanelStyleHud()
			.BodyAdd(c => c.Padding(left: 3, right: 3, top: 3, bottom: 3), m_fill);
		m_bar.Width(BarWidthPx.px()).Height(BarHeightPx.px());
		m_bar.OnClick((ClickEvent _) => ActivateSelf());

		buildGradient();

		try {
			// Dock into CalendarControls (the top-right date/speed panel) and pin the bar flush to
			// its left edge, bottom-aligned with the date row. right=100% places the bar's right
			// edge at the panel's left edge, so the bar takes the free space beside the date panel.
			UiComponent calendar = m_statusBar.First(c => c.RootElement.name == "CalendarControls");
			calendar.Add(m_bar);
			calendar.OverflowVisible();
			// right=100% places the bar's right edge at the panel's left edge; the top offset is
			// applied directly since it cannot be mixed with a percent in one AbsolutePosition call.
			m_bar.AbsolutePosition(top: 40.px(), left: 0.px());
		} catch (Exception e) {
			// Fall back to absolute positioning if the calendar panel cannot be found.
			Log.Error("NightMod: HUD docking failed - " + e.Message);
			m_uiContext.UiRoot.AddComponent(m_bar);
			m_bar.AbsolutePosition(top: 110.px(), right: 10.px());
		}

		m_uiContext.GameLoopEvents.RenderUpdate.AddNonSaveable(this, onRenderUpdate);
	}

	/// <summary>
	/// Fills the bar with the cycle gradient (a row of solid-color slices) and adds the fixed
	/// pointer over its center. The slices are recolored every frame so the colors scroll; the
	/// pointer stays put.
	/// </summary>
	private void buildGradient() {
		VisualElement root = m_fill.RootElement;
		root.style.flexDirection = FlexDirection.Row;
		root.style.overflow = Overflow.Hidden;

		m_segments = new VisualElement[GradientSegments];
		for (int i = 0; i < GradientSegments; i++) {
			VisualElement segment = new VisualElement();
			segment.style.flexGrow = 1f;
			root.Add(segment);
			m_segments[i] = segment;
		}

		// The pointer: a thin vertical marker fixed at the center of the bar. The cycle colors
		// scroll beneath it, so the color under the pointer is always the current moment.
		m_pointer = new VisualElement();
		m_pointer.style.position = Position.Absolute;
		m_pointer.style.top = 0;
		m_pointer.style.bottom = 0;
		m_pointer.style.width = 5;
		m_pointer.style.backgroundColor = PointerColor;
		m_pointer.style.left = Length.Percent(50f);
		m_pointer.style.translate = new Translate(Length.Percent(-50f), 0f);
		root.Add(m_pointer);
	}

	/// <summary>
	/// Scrolls the cycle colors so the current moment sits under the fixed center pointer. The bar
	/// always shows the whole day-night cycle, rotated so "now" is in the middle.
	/// </summary>
	private void onRenderUpdate(GameTime time) {
		int cycleDays = Math.Max(1, m_cycleManager.CycleLengthDays);
		double day = (time.SimStepsCount + (double)time.AbsoluteT) / SimStepsPerDay;
		// The day-of-year is the cycle count - one cycle is one day of the 360-cycle seasonal year -
		// matching the renderer, so the gradient shows the same smooth, steady seasonal day length.
		double cycleCount = day / cycleDays;
		float phase = (float)(cycleCount % 1.0);
		double dayOfYear = cycleCount % SolarSky.DaysPerYear;
		for (int i = 0; i < m_segments.Length; i++) {
			float segmentPhase = phase + (i + 0.5f) / GradientSegments - 0.5f;
			segmentPhase -= Mathf.Floor(segmentPhase);
			m_segments[i].style.backgroundColor = cycleColor(segmentPhase, dayOfYear);
		}
	}

	/// <summary>
	/// The cycle color at a given time-of-day phase. It runs the same seasonal solar model as the
	/// world, so the gradient shows the real day length for the configured latitude and in-game
	/// date - the day band narrows in winter and widens in summer.
	/// </summary>
	private Color cycleColor(float phase, double dayOfYear) {
		float elevation;
		if (m_settings.OverrideSunPosition) {
			// Sun frozen by the override - the bar shows one flat color, the current sky.
			elevation = (float)m_settings.OverrideSunElevation;
		} else {
			SolarSky.SunPosition(m_settings.Latitude, dayOfYear, phase, out elevation, out _);
		}
		// Daylight ramps from -6 deg to +18 deg elevation; the dusk tint peaks at the horizon.
		float dayFactor = Mathf.Clamp01((elevation + 6f) / 24f);
		float duskFactor = Mathf.Clamp01(1f - Mathf.Abs(elevation) / 12f);
		Color color = Color.Lerp(NightColor, DayColor, dayFactor);
		return Color.Lerp(color, DuskColor, duskFactor * 0.75f);
	}

	/// <summary>Reaches the (non-public) UI layer container on the root.</summary>
	private static UiComponent getLayer(UiRoot root, UiLayer layer) {
		MethodInfo method = typeof(UiRoot).GetMethod("getOrCreateContainer",
			BindingFlags.Instance | BindingFlags.NonPublic);
		return (UiComponent)method.Invoke(root, new object[] { layer });
	}
}
