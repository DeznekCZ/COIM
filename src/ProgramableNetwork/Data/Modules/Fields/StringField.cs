using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public class StringField : IField
    {
        public string Id { get; }
        private string name;
        private string shortDesc;

        public string Default { get; }

        public StringField(string id, string name, string shortDesc, string defaultValue)
        {
            this.Id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.Default = defaultValue;
        }

        public string Name => name;

        public int Size => 20;

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog)
        {
            Row row = new Row();
            row.Height(20);
            fieldContainer.Add(row);

            Label label = new Label();
            label.Value(new Mafi.Localization.LocStrFormatted(Name));
            label.Tooltip(new Mafi.Localization.LocStrFormatted(shortDesc));
            label.Size(width: 180, height: 40);
            row.Add(label);

            var numberEditor = new TextField();
            numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field[Id, false]));
            numberEditor.Width(180);
            numberEditor.Height(20);
            row.Add(numberEditor);

            var setButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Save_svg);
            setButton.Width(20);
            setButton.Height(20);
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