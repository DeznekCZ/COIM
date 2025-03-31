using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Products;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using WindPower.Entity;
using Mafi.Unity.UserInterface.Components;

namespace WindPower.Ui
{
    public class WindTurbineView : StaticEntityInspectorBase<WindTurbine>
    {
        private WindTurbineInspector m_inspector;
        private QuantityBar m_powerBufferSlider;

        public WindTurbineView(WindTurbineInspector WindTurbineInspector)
            :base(WindTurbineInspector)
        {
            this.m_inspector = WindTurbineInspector;
        }

        protected override WindTurbine Entity => m_inspector.SelectedEntity;
        protected override void AddCustomItems(StackContainer itemContainer)
        {
            //MakeScrollableWithHeightLimit();
            base.AddCustomItems(itemContainer);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            Txt parent = AddSectionTitle(itemContainer, Tr.Power, Tr.Power, null);
            m_powerBufferSlider = new QuantityBar(Builder);
            m_powerBufferSlider.AppendTo(itemContainer);
            m_powerBufferSlider.SetColor(ColorRgba.DarkGreen);
            m_powerBufferSlider.SetHeight(40);
            updaterBuilder
                .Observe(() => Entity.StoredPower)
                .Observe(() => Entity.TargetPower)
                .Observe(() => Entity.WindPower)
                .Observe(() => Entity.Prototype)
                .Do((Percent storedPower, Percent targetPower, Percent windPower, WindTurbineProto proto) =>
                {
                    m_powerBufferSlider.SetColor(
                        windPower > Percent.Hundred ?
                        ColorRgba.DarkRed :
                        windPower < Percent.FromFloat(0.25f) ?
                        ColorRgba.DarkYellow :
                        ColorRgba.DarkGreen);

                    int step = (int)(storedPower.ToFloat() * proto.GeneratedPower.Value);
                    m_powerBufferSlider.UpdateValues(storedPower,
                        (step > 900 ? ((step.ToFix32() / 1000).ToStringRounded(1) + " MW") : (step.ToStringCached() + " kW")) +
                        ($" Wind: {windPower.ToStringRounded()}")
                    );
                });

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
        }
    }
}