using Mafi;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

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

        private Action setter;

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
                .ToolTip(inspector, ShortDesc, attached: true)
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
                setter?.Invoke();
                setButton.SetEnabled(false);
            });

            numberEditor.SetOnValueChangedAction(() =>
            {
                setter = null;
                if (typeof(T) == typeof(Fix32))
                {
                    if (double.TryParse(numberEditor.GetText(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        setter = () => module.Field[Id] = value.ToFix32();
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(numberEditor.GetText(), out int value))
                    {
                        setter = () => module.Field.Integer[Id] = value;
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    if (long.TryParse(numberEditor.GetText(), out long value))
                    {
                        setter = () => module.Field[Id, false] = value.ToString();
                    }
                }
                setButton.SetEnabled(setter != null);
            });

            if (Default is Fix32)
            {
                numberEditor.SetText(module.Field[Id].ToString());
            }
            else if (Default is int)
            {
                numberEditor.SetText(module.Field.Integer[Id].ToString());
            }
            else if (Default is long)
            {
                numberEditor.SetText(module.Field[Id, "0"]);
            }
            else
            {
                Log.Error($"Invalid number type: {Default.GetType()}");
                throw new Exception($"Invalid number type: {Default.GetType()}");
            }
        }

        public void InitData(Module module)
        {
            if (Default is Fix32 f)
            {
                module.Field[Id] = f;
            }
            else if (Default is int i)
            {
                module.Field.Integer[Id] = i;
            }
            else if (Default is long l)
            {
                module.Field[Id, false] = l.ToString();
            }
            else
            {
                Log.Error($"Invalid number type: {Default.GetType()}");
                throw new Exception($"Invalid number type: {Default.GetType()}");
            }
        }

        public void Validate(Module module)
        {
        }
    }
}