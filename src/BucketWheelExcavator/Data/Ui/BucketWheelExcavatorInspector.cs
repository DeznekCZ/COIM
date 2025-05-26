using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using BucketWheelExcavator.Entity;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.Ui;
using Mafi.Base;
using Mafi.Core.Syncers;
using static Mafi.Core.Prototypes.EntityCostsTpl;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;

namespace BucketWheelExcavator.Ui
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class BucketWheelExcavatorInspector : BaseInspector<Entity.BucketWheelExcavator>
    {
        private readonly Display m_bufferCount;
        private readonly Column m_touches;
        private readonly Display m_bucketCount;

        public BucketWheelExcavatorInspector(
            UiContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager
            ) : base(context)
        {
            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            AddPanelRow(
                new Label("Total zippers: ".AsLoc()),
                m_bufferCount = new Display().Value(0)
            );

            updaterBuilder
                .Observe(() => Entity)
                .Observe(() => Entity?.Queue)
                .Do((entity, queue) =>
                {
                    if (entity is null) return;
                    m_bufferCount.Value(queue.Count.ToFix32());
                });

            Panel control = AddPanel();

            Slider direction = control
                .AddAndReturn(new Slider())
                .OnValueChanged((oldValue, newValue) =>
                {
                    Entity.Direction = (360f * newValue).ToFix32();
                });
            direction.Observe(() => (Entity.Direction.ToFloat() / 360f))
                     .Do((dir) => direction.Value(dir));

            Slider arm = control
                .AddAndReturn(new Slider())
                .OnValueChanged((oldValue, newValue) =>
                {
                    if (Entity is null) return;
                    Entity.Height = (-60f * newValue + 15f).ToFix32();
                });
            arm.Observe(() => (Entity.Height.ToFloat() - 15) / -60)
               .Do((ar) => arm.Value(ar));

            Slider distance = control
                .AddAndReturn(new Slider())
                .OnValueChanged((oldValue, newValue) =>
                {
                    if (Entity is null) return;
                    Entity.Distance = (12f + 10 * newValue).ToFix32();
                });
            distance.Observe(() => (Entity.Distance.ToFloat() - 12) / 10)
                    .Do((dist) => distance.Value(dist));

            m_bucketCount = new Display().Value(0);
            m_bucketCount.Observe(() => Entity.Buckets.Count)
                         .Do((count) => m_bucketCount.Value(count));
            AddPanel(
                new Row() {
                    new Label("Touches: ".AsLoc()),
                    m_bucketCount
                },
                m_touches = new Column()
            );

            updaterBuilder
                .Observe(() => Entity?.Buckets, new Tile3fListComparator())
                .Do((buckets) =>
                {
                    m_touches.Clear();
                    foreach (var bucket in buckets) {
                        Row row = m_touches.AddAndReturn(new Row());
                        Display x = row.AddAndReturn(new Display().Value(bucket.X));
                        Display y = row.AddAndReturn(new Display().Value(bucket.Y));
                        Display z = row.AddAndReturn(new Display().Value(bucket.Z));
                    }
                });
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            //EntitySelectionInput = null;
        }
    }
}
