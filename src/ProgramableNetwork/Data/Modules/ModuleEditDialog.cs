using Mafi;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System.Net.Configuration;

namespace ProgramableNetwork
{
    public class ModuleEditDialog : Window
    {
        private Module m_module;
        private ControllerView m_controllerView;
        private UiContext m_uiContext;
        private ControllerInspector m_controllerInspector;

        public ModuleEditDialog(Module module, ControllerView controllerView, UiContext uiContext, ControllerInspector controllerInspector)
            : base(module.Prototype.Strings.Name)
        {
            m_module = module;
            m_controllerView = controllerView;
            m_uiContext = uiContext;
            m_controllerInspector = controllerInspector;

            ScrollColumn settings = new ScrollColumn();
            settings.Gap(5.px());
            settings.Height(400.px());
            settings.Width(400.px());
            Body.Add(settings);

            // TODO: copy, paste, template

            foreach (IField item in module.Prototype.Fields)
            {
                item.Init(controllerInspector, controllerInspector, settings, uiContext, module, () => { });
            }

            this.Height(450.px());
            this.Width(420.px());
            this.OpenIn(controllerInspector);
            //this.AbsolutePositionCenterMiddle();
        }
    }
}