using Mafi.Unity.UiToolkit.Component;
using UnityEngine.UIElements;

namespace ProgramableNetwork.Data.Modules
{
    public class ConnectionLine : UiComponent<VisualElement>
    {
        public ConnectionLine(ControllerInspector inspector, Module module) : base(new VisualElement())
        {
            this.ObserveVisibleForRender(() => inspector.m_higlightedOutput?.ModuleId == module.Id);
        }
    }
}
