using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
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

        public void Init(ControllerInspector inspector, WindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
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

            bool value = module.Field[Id, Default ? 1 : 0] > 0;
            module.Field[Id] = value;

            Btn falseSelector = uiBuilder
                .NewBtnGeneral("false")
                .SetButtonStyle(!value ? uiBuilder.Style.Global.GeneralBtnActive : uiBuilder.Style.Global.GeneralBtn)
                .SetParent(fieldContainer, true)
                .SetWidth(100)
                .SetHeight(20)
                .SetText("OFF")
                .AppendTo(fieldContainer);

            Btn trueSelector = uiBuilder
                .NewBtnGeneral("true")
                .SetButtonStyle(value ? uiBuilder.Style.Global.GeneralBtnActive : uiBuilder.Style.Global.GeneralBtn)
                .SetParent(fieldContainer, true)
                .SetWidth(100)
                .SetHeight(20)
                .SetText("ON")
                .AppendTo(fieldContainer);

            trueSelector.OnClick(() =>
            {
                module.Field[Id] = 1;
                trueSelector.SetButtonStyle(uiBuilder.Style.Global.GeneralBtnActive);
                falseSelector.SetButtonStyle(uiBuilder.Style.Global.GeneralBtn);
            });

            falseSelector.OnClick(() =>
            {
                module.Field[Id] = 0;
                trueSelector.SetButtonStyle(uiBuilder.Style.Global.GeneralBtn);
                falseSelector.SetButtonStyle(uiBuilder.Style.Global.GeneralBtnActive);
            });
        }

        public void Validate(Module module)
        {
            // nothing to do
        }
    }
}