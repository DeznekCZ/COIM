using Mafi.Unity;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;

namespace ProgramableNetwork
{
    public class CustomField : IField
    {
        private string id;
        private string name;
        private string shortDesc;
        private Action<CustomField> ui;
        private Func<int> size;

        public CustomField(string id, string name, string shortDesc, Func<int> size, Action<CustomField> ui)
        {
            this.id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.ui = ui;
            this.size = size;
        }

        public string Name => name;
        public string ShortDesc => shortDesc;

        public int Size => size();

        public ITooltipInspector Inspector { get; private set; }
        public UiBuilder Builder { get; private set; }
        public StackContainer Container { get; private set; }
        public Action Refresh { get; private set; }
        public Reference Reference { get; private set; }

        public void Init(ControllerInspector inspector, WindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            Inspector = inspector;
            Builder = uiBuilder;
            Container = fieldContainer;
            Refresh = updateDialog;
            Reference = new Reference((v) => module.Field[id] = v, () => module.Field[id, 0]);
            ui.Invoke(this);
        }

        public void Validate(Module module)
        {
            // do nothing
        }
    }
}