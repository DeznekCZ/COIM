using System;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Core.Prototypes;
using Mafi.Core.Mods;

namespace ProgramableNetwork.Ui
{
    public abstract class AModuleProtoSelector
    {
        protected readonly Controller m_controller;
        protected readonly ControllerView m_controllerView;
        protected readonly Action m_refresh;
        protected readonly Func<ModuleProto, (bool, Module)> m_tryCreate;
        protected readonly Action<Module> m_onSuccess;

        protected AModuleProtoSelector(Controller controller, ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate)
        {
            m_controller = controller;
            m_controllerView = controllerView;
            m_refresh = refresh;
            m_tryCreate = tryCreate;
            m_onSuccess = onSuccess;
        }

        public abstract string SearchString { get; }
        public abstract Proto.Str Strings { get; }
        public abstract Proto.ID Id { get; }

        public abstract Button CreateUi();

        public abstract void Selected();
    }
}
