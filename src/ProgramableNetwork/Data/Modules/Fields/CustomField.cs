using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
    public class CustomField : IField
    {
        private string id;
        private LocStr name;
        private LocStr shortDesc;
        private CustomFieldConstructorWithModule ui;
        private Action<CustomField> data;

        public CustomField(string id, Proto.Str strs, CustomFieldConstructor ui, Action<CustomField> data)
        {
            this.id = id;
            this.name = strs.Name;
            this.shortDesc = strs.DescShort;
            this.ui = (a,b,c,d,e) => ui(a, b, d, e);
            this.data = data;
        }

        public CustomField(string id, Proto.Str strs, CustomFieldConstructorWithModule ui, Action<CustomField> data)
        {
            this.id = id;
            this.name = strs.Name;
            this.shortDesc = strs.DescShort;
            this.ui = ui;
            this.data = data;
        }

        public string Id => id;
        public LocStr Name => name;
        public LocStr ShortDesc => shortDesc;

        // Custom fields don't have a generic backing value to format, so they never opt
        // into tooltip aggregation.  Specific custom-field consumers can wrap their data
        // in a regular field if they need this.
        public bool ShowInTooltip => false;

        public string GetTooltipValue(Module module) => "";

        public int Size => 1;
        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog)
        {
            ui.Invoke(inspector, fieldContainer, module, updateDialog, new Reference(
				(v) => uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
					module.Controller.Id, module.Id, id, v)),
				() => module.Field[id, Fix32.Zero],
				(v) => uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
					module.Controller.Id, module.Id, id, v)),
				() => module.Field[id, null]
			));
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