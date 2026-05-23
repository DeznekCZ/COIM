using Mafi;
using Mafi.Core.Mods;
using Mafi.Localization;
using Mafi.Unity.InputControl.GameMenu.Settings;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine.UIElements;

namespace NightMod;

/// <summary>
/// The mod's window, opened by clicking the HUD cycle bar. Managed by the standard
/// <see cref="NightCycleWindowController"/>. It hosts the mod's light/cycle settings as an editable
/// panel bound to <c>config.json</c>, reflowed into two columns so the window stays compact.
/// </summary>
public sealed class NightCycleWindow : Window {

	public NightCycleWindow(ModJsonConfig config) : base("Night Mod".AsLoc()) {
		WindowWidth(760.px());
		WindowMaxHeight(80.Percent());
		MakeMovable();

		// Host the JSON settings panel, but reflow its single column of fields into two columns.
		// The panel is a plain vertical stack, so switching its root to a wrapping row and giving
		// each field half the width lays the settings out side by side.
		ModJsonConfigPanel panel = new ModJsonConfigPanel(config);
		VisualElement root = panel.RootElement;
		root.style.flexDirection = FlexDirection.Row;
		root.style.flexWrap = Wrap.Wrap;
		foreach (VisualElement child in root.Children()) {
			child.style.width = Length.Percent(50f);
		}
		AddPanel(panel);
	}
}
