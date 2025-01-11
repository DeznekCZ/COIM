using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;

namespace ProgramableNetwork
{
    public class NumberField<T> : IField
    {
        public NumberField(string id, string name, string shortDesc, T defaultValue)
        {
            Id = id;
            Name = name;
            Default = defaultValue;
            ShortDesc = shortDesc;
        }

        public string Id { get; }
        public string Name { get; }
        public string ShortDesc { get; }

        public int Size => 20;

        public T Default { get; }

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
                .ToolTip(inspector, ShortDesc, attached: true)
                .AppendTo(fieldContainer);

            var numberEditor = uiBuilder
                .NewTxtField("value")
                .SetParent(fieldContainer, true)
                .SetWidth(200)
                .SetHeight(20)
                .SetText("0")
                .AppendTo(fieldContainer);

            numberEditor.SetOnValueChangedAction(() =>
            {
                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(numberEditor.GetText(), out int value))
                    {
                        module.Field[Id] = value;
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    if (long.TryParse(numberEditor.GetText(), out long value))
                    {
                        module.Field[Id] = value.ToString();
                    }
                }
            });

            if (typeof(T) == typeof(int))
            {
                int value = module.Field[Id, (int)(object)Default];
                numberEditor.SetText(value.ToString());
                module.Field[Name] = value;
            }
            else if (typeof(T) == typeof(long))
            {
                string value = module.Field[Id, ((long)(object)Default).ToString()];
                numberEditor.SetText(value);
                module.Field[Name] = value;
            }
        }

        public void Validate(Module module)
        {
            // nothing to do
        }
    }
}