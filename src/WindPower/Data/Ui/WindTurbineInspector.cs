using Mafi;
using Mafi.Core;
using Mafi.Core.Environment;
using Mafi.Core.Products;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using WindPower.Entity;

namespace WindPower.Ui
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class WindTurbineInspector : BaseInspector<WindTurbine>
    {
        public WindTurbineInspector(
            UiContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager,
            WeatherManager weatherManager
            ) : base(context)
        {
            Icon weatherIcon = new Icon().Large();
            ProgressBarPercentInline bar = new ProgressBarPercentInline().AlignSelfStretch().MarginBottom(1.pt());
            ProductProto orThrow = base.Context.ProtosDb.GetOrThrow<ProductProto>(IdsCore.Products.Electricity);
            DisplayWithIcon outputDisplayAct = new DisplayWithIcon().IconValue(orThrow);
            Label capacityLabel = new Label().InfoIconPosition(Label.InfoIconPos.None)/*.FloaterInteractive(() => powerMultFloater)*/;

            //PropertyModifiersFloater powerMultFloater = new PropertyModifiersFloater(context, IdsCore.PropertyIds.SolarPowerMultiplier);
            //powerMultFloater.AddBaseVal(() => Entity.Prototype.OutputElectricity.ToString());

            AddPanelWithHeader(new Row(2.pt())
            {
                new Column(2.pt())
                {
                    new Row(3.pt())
                    {
                        weatherIcon,
                        new Icon(Assets.Unity.UserInterface.General.Transform128_png).Small(),
                        new Row(1.pt())
                        {
                            outputDisplayAct,
                            new Label("/".AsLoc()),
                            capacityLabel
                        }
                    }.MarginLeftRight(8.pt()),
                    bar
                }
            }.AlignSelfCenter()).Title(Tr.Production);

            this.Observe(() => Entity?.StoredPower ?? Percent.Zero)
                .Observe(() => Entity?.Prototype.GeneratedPower ?? Electricity.Zero)
                .Do((wind, generable) =>
                {
                    bar.Value(wind.Min(Percent.Hundred));
                    outputDisplayAct.Value(generable.ScaledBy(wind).Value);
                    if (wind == Percent.Hundred)
                    {
                        bar.Color(ColorRgba.Red);
                        outputDisplayAct.State(DisplayState.Danger);
                    }
                    else if (wind > Percent.Eighty)
                    {
                        bar.Color(ColorRgba.Yellow);
                        outputDisplayAct.State(DisplayState.Warning);
                    }
                    else if (wind > Percent.Twenty)
                    {
                        bar.Color(ColorRgba.Green);
                        outputDisplayAct.State(DisplayState.Neutral);
                    }
                    else if (wind == Percent.Zero)
                    {
                        bar.Color(ColorRgba.Green);
                        outputDisplayAct.State(DisplayState.Positive);
                    }
                    else
                    {
                        bar.Color(ColorRgba.Green);
                        outputDisplayAct.State(DisplayState.Positive);
                    }
                });

            this.Observe(() => Entity?.Prototype?.GeneratedPower)
                .Do(maxGenerated =>
                {
                    capacityLabel.Value(maxGenerated?.Value ?? 0);
                });

            this.Observe(() => weatherManager.CurrentWeather).Do((WeatherProto weather) =>
            {
                weatherIcon.ColorIff(Theme.PrimaryColor, weather.SunIntensity.IsNearHundred);
                weatherIcon.Value(weather.Graphics.IconPath, null);
                weatherIcon.Tooltip(weather.Strings.Name);
            });
        }
    }
}
