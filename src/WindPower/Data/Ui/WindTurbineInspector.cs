using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
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
            ShortcutsManager shortcutsManager
            ) : base(context)
        {

        }
    }
}
