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
        public string Name { get; }
        public bool Default { get; }
        public string ShortDesc { get; }

        public BooleanField(string id, string name, string shortDesc, bool defaultValue)
        {
            this.Id = id;
            this.Name = name;
            this.Default = defaultValue;
            this.ShortDesc = shortDesc;
        }


        public int Size => 20;

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog)
        {
            Row row = fieldContainer.Row(this);

            bool value = module.Field.Bool[Id];

            ButtonText falseSelector = new ButtonText(new Mafi.Localization.LocStrFormatted("OFF"));
            if (!value) falseSelector.Class("selected");
            falseSelector.Width(100);
            falseSelector.Height(Sizes.BLOCK_SIZE);
            row.Add(falseSelector);

            ButtonText trueSelector = new ButtonText(new Mafi.Localization.LocStrFormatted("ON"));
            if (value) trueSelector.Class("selected");
            trueSelector.Width(100);
            trueSelector.Height(Sizes.BLOCK_SIZE);
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