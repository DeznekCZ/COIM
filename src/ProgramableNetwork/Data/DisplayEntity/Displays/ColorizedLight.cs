using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using Mafi.Core;

namespace ProgramableNetwork.Ui.DisplayEntity.Displays;

public class ColorizedLightInspector(Data.DisplayEntity.DisplayEntity entity) {
	private ColorRgba[] s_favoriteColors = [
		ColorRgba.Red,
		ColorRgba.Yellow,
		ColorRgba.Green,
		ColorRgba.Blue,
		ColorRgba.White
	];

	public Data.DisplayEntity.DisplayEntity Entity { get; } = entity;
	// Set lazily by Create(...) on subclasses so the color picker (which lives on the
	// base) can reach IInputScheduler without forcing a constructor change.
	protected Mafi.Unity.Ui.UiContext UiContext { get; set; }

	protected UiComponent GetColorPickerComponent() {
		Column floater = new Column().Padding(5).Gap(5);

		RgbColorPicker colorPicker = floater.AddAndReturn(new RgbColorPicker()
			.LaterText<RgbColorPicker>(() => NewTr.Inspector.LightColor, floater, (cp, v) => cp.Title(v)));
		colorPicker.Observe(() => new ColorRgba(
					Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
					Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
					Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
				))
			.Do(c => colorPicker.Value(c));
		colorPicker.OnColorChanged(setColorToEntity);

		Row favoriteColorsRow = floater.AddAndReturn(new Row().Gap(5));
		foreach (ColorRgba favorite in s_favoriteColors) {
			ButtonIcon favoriteButton = favoriteColorsRow.AddAndReturn(new ButtonIcon(Assets.Unity.UserInterface.General.Circle_svg));
			favoriteButton.Icon.Color(favorite);
			favoriteButton.OnClick(() => {
				colorPicker.Value(favorite);
				setColorToEntity(favorite);
			});
		}
			
		ButtonIcon colorButton = new ButtonIcon(Assets.Unity.UserInterface.General.Circle_svg);
		colorButton.Observe(() => new ColorRgba(
					Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
					Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
					Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
				))
			.Do(color => colorButton.Icon.Color(color));
		colorButton.FloaterInteractive(floater);

		return colorButton;
	}

	private void setColorToEntity(ColorRgba v) {
		// One command for all six channels — the executor derives the off colour from the on
		// colour, so hosts/clients agree on a single atomic colour change per click/drag tick.
		// Direct fallback only fires when there is no scheduler at all (e.g. tests).
		var scheduler = UiContext?.InputScheduler;
		if (scheduler == null) {
			Entity.SetProperty("colorOn.R", v.R);
			Entity.SetProperty("colorOn.G", v.G);
			Entity.SetProperty("colorOn.B", v.B);
			Entity.SetProperty("colorOff.R", v.R.Min(100));
			Entity.SetProperty("colorOff.G", v.G.Min(100));
			Entity.SetProperty("colorOff.B", v.B.Min(100));
			return;
		}
		scheduler.ScheduleInputCmd(new DisplayEntitySetLightColorCmd(Entity.Id, v));
	}
}
