using Mafi;
using Mafi.Core;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
    public class BooleanField : IField
    {
        public string Id { get; }
        public LocStr Name { get; }
        public bool Default { get; }
        public LocStr ShortDesc { get; }
        public bool ShowInTooltip { get; }

        public BooleanField(string id, Proto.Str strs, bool defaultValue, bool showInTooltip = false)
        {
            this.Id = id;
            this.Name = strs.Name;
            this.Default = defaultValue;
            this.ShortDesc = strs.DescShort;
            this.ShowInTooltip = showInTooltip;
        }


        public int Size => 20;

        public string GetTooltipValue(Module module)
        {
            return module.Field.Bool[Id] ? "true" : "false";
        }

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog, bool directEdit = false)
        {
            // input field definition
            if (Id.StartsWith("field_") && module.Prototype.Inputs.Exists(i => i.Id == Id.Substring("field_".Length)))
            {
                return;
            }

            RowContainer row = fieldContainer.Row(this, module, uiContext, out bool draw, directEdit: directEdit);
            if (!draw) {
				return;
			}

			bool value = module.Field.Bool[Id];

            Toggle toggle = new Toggle()
                .Value(value)
                .Width(200)
                .Height(Sizes.BLOCK_SIZE);
            row.Add(toggle);

            toggle.OnValueChanged((v) =>
            {
                if (directEdit) {
                    module.Field[Id] = v ? Fix32.One : Fix32.Zero;
                } else {
                    uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
                        module.Controller.Id, module.Id, Id, v ? Fix32.One : Fix32.Zero));
                }
            });
        }

        public void Validate(Module module)
        {
            // nothing to do
        }

        public void InitData(Module module)
        {
            module.Field[Id] = Default ? Fix32.One : Fix32.Zero;
        }
    }
}