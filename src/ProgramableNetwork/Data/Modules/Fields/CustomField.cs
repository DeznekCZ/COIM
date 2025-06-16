using Mafi;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
    public class CustomField : IField
    {
        private string id;
        private string name;
        private string shortDesc;
        private CustomFieldConstructorWithModule ui;
        private Action<CustomField> data;

        public CustomField(string id, string name, string shortDesc, CustomFieldConstructor ui, Action<CustomField> data)
        {
            this.id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.ui = (a,b,c,d,e) => ui(a, b, d, e);
            this.data = data;
        }

        public CustomField(string id, string name, string shortDesc, CustomFieldConstructorWithModule ui, Action<CustomField> data)
        {
            this.id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.ui = ui;
            this.data = data;
        }

        public string Id => id;
        public string Name => name;
        public string ShortDesc => shortDesc;

        public int Size => 1;
        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog)
        {
            ui.Invoke(inspector, fieldContainer, module, updateDialog, new Reference((v) => module.Field[id] = v, () => module.Field[id, Fix32.Zero]));
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

    public delegate void CustomFieldConstructor(ControllerInspector Inspector, UiComponent Container, Action Refresh, Reference reference);
    public delegate void CustomFieldConstructorWithModule(ControllerInspector Inspector, UiComponent Container, Module module, Action Refresh, Reference reference);
}