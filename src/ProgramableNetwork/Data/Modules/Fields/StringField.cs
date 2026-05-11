using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;
using Mafi;

namespace ProgramableNetwork.Ui
{
    public class StringField : IField
    {
        public string Id { get; }
        public LocStr Name { get; }
        public LocStr ShortDesc { get; }

        public string Default { get; }
        public bool Multilined { get; }
        public bool ShowInTooltip { get; }

        public StringField(string id, Proto.Str strs, string defaultValue, bool multilined = false, bool showInTooltip = false)
        {
            this.Id = id;
            this.Name = strs.Name;
            this.ShortDesc = strs.DescShort;
            this.Default = defaultValue;
            this.Multilined = multilined;
            this.ShowInTooltip = showInTooltip;
        }

        public int Size => Multilined ? 80 : 20;

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog, bool directEdit = false)
        {
            RowContainer row = fieldContainer.Row(this, module, uiContext, out _, directEdit: directEdit);

            var numberEditor = new TextField();
            numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field[Id, false]));
            // Direct-edit mode has no save button, so the editor gets the full row.
            numberEditor.Width(directEdit ? 200.px() : (200 - Sizes.BLOCK_SIZE * 1.5f));
            // Multi-line text fields grow vertically — give them ~4 rows of editing space
            // so the user sees enough context.  Single-line keeps the original BLOCK_SIZE row.
            numberEditor.Height(Multilined ? Sizes.BLOCK_SIZE * 4 : Sizes.BLOCK_SIZE);
            if (Multilined) {
                numberEditor.Multiline(true);
            }
            row.Add(numberEditor);

            if (directEdit)
            {
                numberEditor.OnValueChanged((e) =>
                {
                    module.Field[Id, false] = numberEditor.GetText();
                });
                return;
            }

            var setButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Save_svg);
            setButton.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
            setButton.Width(Sizes.BLOCK_SIZE * 1.5f);
            setButton.Height(Sizes.BLOCK_SIZE);
            setButton.Enabled(false);
            row.Add(setButton);

            setButton.OnClick(() =>
            {
                string changeValue = numberEditor.GetText();
                uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
                    module.Controller.Id, module.Id, Id, changeValue));
                setButton.Enabled(false);
            });

            numberEditor.OnValueChanged((e) =>
            {
                setButton.Enabled(true);
            });
        }

        public void Validate(Module module)
        {
            // nothing to do
        }

        public void InitData(Module module)
        {
            module.Field[Id, false] = Default;
        }

        public string GetTooltipValue(Module module)
        {
            return module.Field[Id, false] ?? "";
        }
    }
}