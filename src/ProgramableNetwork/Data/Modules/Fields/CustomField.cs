using Mafi;
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
        private Action<CustomField> data;
        private Func<int> size;

        public CustomField(string id, string name, string shortDesc, Func<int> size, Action<CustomField> ui, Action<CustomField> data)
        {
            this.id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.ui = ui;
            this.data = data;
            this.size = size;
        }

        public string Id => id;
        public string Name => name;
        public string ShortDesc => shortDesc;

        public int Size => size();

        public ITooltipInspector Inspector { get; private set; }
        public UiBuilder Builder { get; private set; }
        public StackContainer Container { get; private set; }
        public Action Refresh { get; private set; }
        public Reference Reference { get; private set; }

        public void Init(ControllerInspector inspector, ItemDetailWindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            Inspector = inspector;
            Builder = uiBuilder;
            Container = fieldContainer;
            Refresh = updateDialog;
            Reference = new Reference((v) => module.Field[id] = v, () => module.Field[id, Fix32.Zero]);
            ui.Invoke(this);
        }

        public void InitData(Module module)
        {
            data.Invoke(this);
        }

        public void Validate(Module module)
        {
            // do nothing
        }
    }
}