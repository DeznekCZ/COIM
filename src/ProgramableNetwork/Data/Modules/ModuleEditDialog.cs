using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;

namespace ProgramableNetwork
{
    public class ModuleEditDialog : PopupPanel
    {
        private Module m_module;
        private ControllerView m_controllerView;
        private UiContext m_uiContext;
        private ControllerInspector m_controllerInspector;

        public ModuleEditDialog(Module module, ControllerView controllerView, UiContext uiContext, ControllerInspector controllerInspector)
            : base()
        {
            m_module = module;
            m_controllerView = controllerView;
            m_uiContext = uiContext;
            m_controllerInspector = controllerInspector;

            // TODO: copy, paste, template

            foreach (IField item in module.Prototype.Fields)
            {
                item.Init(controllerInspector, controllerInspector, this, uiContext, module, () => { });
            }

            this.Open(controllerInspector);
        }
    }
}