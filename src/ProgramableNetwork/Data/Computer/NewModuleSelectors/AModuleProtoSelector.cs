using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
    public abstract class AModuleProtoSelector
    {
        protected readonly ControllerView m_controllerView;
        protected readonly Action m_refresh;
        protected readonly Func<ModuleProto, (bool, Module)> m_tryCreate;
        protected readonly Action<Module> m_onSuccess;

        protected AModuleProtoSelector(ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate)
        {
            m_controllerView = controllerView;
            m_refresh = refresh;
            m_tryCreate = tryCreate;
            m_onSuccess = onSuccess;
        }

        public abstract string SearchString { get; }
        public abstract Proto.Str Strings { get; }
        public abstract Proto.ID Id { get; }
        public abstract ImmutableArray<Category> Categories { get; }

        public abstract Button CreateUi();

        public abstract void Selected();
    }
}
