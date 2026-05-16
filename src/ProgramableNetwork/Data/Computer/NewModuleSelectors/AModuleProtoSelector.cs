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
        // Placement is async (the underlying cmd applies on the sim thread), so
        // tryCreate hands off the (ok, placedModule) pair via a callback fired
        // when the executor confirms the placement.  Callers' post-place work
        // (ExecuteInit, applying template Settings, blueprint extraction) goes
        // inside the callback so it runs against the real placed module instead
        // of a stale snapshot.
        protected readonly Action<ModuleProto, Action<bool, Module>> m_tryCreate;
        protected readonly Action<Module> m_onSuccess;

        protected AModuleProtoSelector(ControllerView controllerView, Action refresh, Action<Module> onSuccess, Action<ModuleProto, Action<bool, Module>> tryCreate)
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
