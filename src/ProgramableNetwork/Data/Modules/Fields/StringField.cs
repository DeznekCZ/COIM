using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;

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

        public void Init(ControllerInspector inspector, ItemDetailWindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            fieldContainer.SetStackingDirection(StackContainer.Direction.LeftToRight);
            fieldContainer.SetHeight(20);

            var txt = uiBuilder
                .NewBtnGeneral("name")
                .SettingFieldNameStyle(uiBuilder)
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(40)
                .SetText(Name)
                .ToolTip(inspector, shortDesc, attached: true)
                .AppendTo(fieldContainer);

            var numberEditor = uiBuilder
                .NewTxtField("value")
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(20)
                .AppendTo(fieldContainer);

            var setButton = uiBuilder
                .NewBtnPrimary("set")
                .SetParent(fieldContainer, true)
                .SetWidth(20)
                .SetHeight(20)
                .SetText("✓")
                .SetEnabled(false)
                .AppendTo(fieldContainer);

            setButton.OnClick(() =>
            {
                string changeValue = numberEditor.GetText();
                module.Field[Id, ""] = changeValue;
                setButton.SetEnabled(false);
            });

            numberEditor.SetOnValueChangedAction(() =>
            {
                setButton.SetEnabled(true);
            });

            string value = module.Field[Id, Default];
            numberEditor.SetText(value);
        }

        public void Validate(Module module)
        {
            // nothing to do
        }
    }
}