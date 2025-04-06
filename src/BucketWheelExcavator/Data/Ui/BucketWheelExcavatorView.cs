using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Products;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using BucketWheelExcavator.Entity;
using Mafi.Unity.UserInterface.Components;
using System;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface.Style;

namespace BucketWheelExcavator.Ui
{
    public class BucketWheelExcavatorView : EntityInspectorBase<Entity.BucketWheelExcavator>
    {
        private BucketWheelExcavatorInspector m_inspector;
        private QuantityBar m_powerBufferSlider;
        private Slidder baseAngle;
        private Slidder armAngle;
        private Slidder distance;

        public BucketWheelExcavatorView(BucketWheelExcavatorInspector windTurbineInspector)
            :base(windTurbineInspector)
        {
            this.m_inspector = windTurbineInspector;
        }

        protected override Entity.BucketWheelExcavator Entity => m_inspector.SelectedEntity;
        protected override void AddCustomItems(StackContainer itemContainer)
        {
            //MakeScrollableWithHeightLimit();
            base.AddCustomItems(itemContainer);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            baseAngle = Builder.NewSlider("baseAngle")
                .SimpleSlider(new SliderStyle()
                {
                    BgColor = ColorRgba.Black,
                    FillColor = ColorRgba.White,
                    HandleColor = ColorRgba.Gray,
                    HandleWidth = 2,
                    BgSprite = new IconsPaths().WhiteBgBlueBorder
                })
                .OnValueChange((v) => { }, (v) =>
                {
                    if (Entity is null) return;
                    Entity.Direction = (360f * v).ToFix32();
                })
                .AppendTo(itemContainer);

            updaterBuilder
                .Observe(() => Entity)
                .Observe(() => Entity?.Direction)
                .DoWhen(
                    (e, v) => !(e is null) && !(v is null),
                    (e, v) => baseAngle.SetValue(v.Value.ToFloat() / 360)
                );

            armAngle = Builder.NewSlider("armAngle")
                .SimpleSlider(new SliderStyle()
                {
                    BgColor = ColorRgba.Black,
                    FillColor = ColorRgba.White,
                    HandleColor = ColorRgba.Gray,
                    HandleWidth = 2,
                    BgSprite = new IconsPaths().WhiteBgBlueBorder
                })
                .OnValueChange((v) => { }, (v) =>
                {
                    if (Entity is null) return;
                    Entity.Height = (-60f * v + 15f).ToFix32();
                })
                .AppendTo(itemContainer);

            updaterBuilder
                .Observe(() => Entity)
                .Observe(() => Entity?.Height)
                .DoWhen(
                    (e, v) => !(e is null) && !(v is null),
                    (e, v) => armAngle.SetValue((v.Value.ToFloat() - 15) / -60)
                );

            distance = Builder.NewSlider("distance")
                .SimpleSlider(new SliderStyle()
                {
                    BgColor = ColorRgba.Black,
                    FillColor = ColorRgba.White,
                    HandleColor = ColorRgba.Gray,
                    HandleWidth = 2,
                    BgSprite = new IconsPaths().WhiteBgBlueBorder
                })
                .OnValueChange((v) => { }, (v) =>
                {
                    if (Entity is null) return;
                    Entity.Distance = (12f + 10 * v).ToFix32();
                })
                .AppendTo(itemContainer);

            updaterBuilder
                .Observe(() => Entity)
                .Observe(() => Entity?.Distance)
                .DoWhen(
                    (e, v) => !(e is null) && !(v is null),
                    (e, v) => distance.SetValue((v.Value.ToFloat() - 12) / 10)
                );

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
        }
    }
}