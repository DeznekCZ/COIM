using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Inspectors;
using BucketWheelExcavator.Entity;

namespace BucketWheelExcavator.Ui
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class BucketWheelExcavatorInspector : EntityInspector<Entity.BucketWheelExcavator, BucketWheelExcavatorView>
    {
        private readonly BucketWheelExcavatorView m_windowView;

        public BucketWheelExcavatorInspector(
            InspectorContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager
            ) : base(context)
        {
            m_windowView = new BucketWheelExcavatorView(this);
        }

        public override BucketWheelExcavatorView GetView()
        {
            return m_windowView;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            //EntitySelectionInput = null;
        }
    }
}
