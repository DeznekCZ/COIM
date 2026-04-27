using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork.Ui
{
    public class StringField : IField
    {
        public string Id { get; }
        public LocStr Name { get; }
        public LocStr ShortDesc { get; }

        public string Default { get; }

        public StringField(string id, Proto.Str strs, string defaultValue)
        {
            this.Id = id;
            this.Name = strs.Name;
            this.ShortDesc = strs.DescShort;
            this.Default = defaultValue;
        }

        public int Size => 20;

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog)
        {
            RowContainer row = fieldContainer.Row(this, module, out _);

            var numberEditor = new TextField();
            numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field[Id, false]));
            numberEditor.Width(200 - Sizes.BLOCK_SIZE * 1.5f);
            numberEditor.Height(Sizes.BLOCK_SIZE);
            row.Add(numberEditor);

            var setButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Save_svg);
            setButton.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
            setButton.Width(Sizes.BLOCK_SIZE * 1.5f);
            setButton.Height(Sizes.BLOCK_SIZE);
            setButton.Enabled(false);
            row.Add(setButton);

            setButton.OnClick(() =>
            {
                string changeValue = numberEditor.GetText();
                module.Field[Id, false] = changeValue;
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
    }
}