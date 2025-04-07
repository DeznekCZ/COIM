using Mafi;
using Mafi.Core;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork
{
    public class BooleanField : IField
    {
        public string Id { get; }
        private string name;
        public bool Default { get; }
        private string shortDesc;

        public BooleanField(string id, string name, string shortDesc, bool defaultValue)
        {
            this.Id = id;
            this.name = name;
            this.Default = defaultValue;
            this.shortDesc = shortDesc;
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

            bool value = module.Field.Bool[Id];

            ButtonText falseSelector = new ButtonText(new Mafi.Localization.LocStrFormatted("OFF"));
            if (value) falseSelector.Class("selected");
            falseSelector.Width(100);
            falseSelector.Height(20);
            row.Add(falseSelector);

            ButtonText trueSelector = new ButtonText(new Mafi.Localization.LocStrFormatted("ON"));
            if (!value) trueSelector.Class("selected");
            trueSelector.Width(100);
            trueSelector.Height(20);
            row.Add(trueSelector);

            trueSelector.OnClick(() =>
            {
                module.Field[Id] = 1;
                trueSelector.Class("selected");
                falseSelector.ClassRemove("selected");
            });

            falseSelector.OnClick(() =>
            {
                module.Field[Id] = 0;
                trueSelector.ClassRemove("selected");
                falseSelector.Class("selected");
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