using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

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
		colorPicker.OnColorChanged(setColorToEntity); // TODO command for multiplayer

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
		Entity.SetProperty("colorOn.R", v.R);
		Entity.SetProperty("colorOn.G", v.G);
		Entity.SetProperty("colorOn.B", v.B);
		Entity.SetProperty("colorOff.R", v.R.Min(100));
		Entity.SetProperty("colorOff.G", v.G.Min(100));
		Entity.SetProperty("colorOff.B", v.B.Min(100));
	}
}
