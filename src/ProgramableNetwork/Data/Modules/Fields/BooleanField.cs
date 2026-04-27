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

        public BooleanField(string id, Proto.Str strs, bool defaultValue)
        {
            this.Id = id;
            this.Name = strs.Name;
            this.Default = defaultValue;
            this.ShortDesc = strs.DescShort;
        }


        public int Size => 20;

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog)
        {
            // input field definition
            if (Id.StartsWith("field_") && module.Prototype.Inputs.Exists(i => i.Id == Id.Substring("field_".Length)))
            {
                return;
            }

            RowContainer row = fieldContainer.Row(this, module, out bool draw);
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
                module.Field[Id] = v ? 1 : 0;
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