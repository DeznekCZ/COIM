using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Inspectors;
using WindPower.Entity;

namespace WindPower.Ui
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class WindTurbineInspector : EntityInspector<WindTurbine, WindTurbineView>
    {
        private readonly WindTurbineView m_windowView;

        public WindTurbineInspector(
            InspectorContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager
            ) : base(context)
        {
            m_windowView = new WindTurbineView(this);
        }

        public override WindTurbineView GetView()
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
