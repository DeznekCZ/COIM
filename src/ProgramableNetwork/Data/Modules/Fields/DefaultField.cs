using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Xml.Linq;
using Mafi.Localization;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Unity.Ui.Library;
using Mafi.Core;

namespace ProgramableNetwork
{
    public static class IFieldExtensions
    {
        public static RowContainer Row(this UiComponent fieldContainer, IField entityField, Module module, Px height)
        {
            // input field definition
            if (module.Prototype.Fields.Exists(i => i.Id == $"field_{entityField.Id}"))
            {
                Panel col = new Panel();
                col.Body.Gap(5);
                fieldContainer.Add(col);

                Row rowBool = new Row();
                col.BodyAdd(rowBool);

                Label labelSettings = new Label();
                labelSettings.Value(entityField.Name.AsLoc());
                labelSettings.Tooltip(entityField.ShortDesc.AsLoc());
                labelSettings.Width(180);
                rowBool.Add(labelSettings);

                Toggle enabled = new Toggle();
                enabled.Value(module.Field.Bool[$"field_{entityField.Id}"]);
                enabled.OnValueChanged(v => module.Field.Bool[$"field_{entityField.Id}"] = v);
                rowBool.Add(enabled);

                Display rowDisplay = new Display("NONE".AsLoc());
                rowDisplay.FlexGrow(1);
                rowDisplay.ObserveValue(() =>
                {
                    if (module.Field.Bool[$"field_{entityField.Id}"])
                        return "ON".AsLoc();
                    else if (module.InputModules.ContainsKey(entityField.Id))
                        return "WIRED".AsLoc();
                    else
                        return "NONE".AsLoc();
                });
                rowBool.Add(rowDisplay);

                PanelRow row = new PanelRow(noBolts: true);
                row.Visible(module.Field.Bool[$"field_{entityField.Id}"]);
                row.ObserveVisible(enabled, () => module.Field.Bool[$"field_{entityField.Id}"]);
                col.BodyAdd(row);

                UiComponent filler = new UiComponent();
                filler.Width(180);
                row.BodyAdd(filler);

                return row;
            }
            else
            {
                PanelRow row = new PanelRow();
                fieldContainer.Add(row);

                Label label = new Label();
                label.Value(entityField.Name.AsLoc());
                label.Tooltip(entityField.ShortDesc.AsLoc());
                label.Width(180);
                row.BodyAdd(label);

                return row;
            }
        }

        /// <summary>
        /// With default height Sizes.BLOCK_SIZE
        /// </summary>
        /// <param name="fieldContainer"></param>
        /// <param name="entityField"></param>
        /// <returns></returns>
        public static RowContainer Row(this UiComponent fieldContainer, IField entityField, Module module)
        {
            return fieldContainer.Row(entityField, module, Sizes.BLOCK_SIZE);
        }
    }
}