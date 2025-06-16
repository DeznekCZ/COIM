using Mafi;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace ProgramableNetwork.Ui
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
        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog)
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

            setButton.OnClick(() =>
            {
                setter?.Invoke();
                setButton.Enabled(false);
            });

            numberEditor.OnValueChanged((e) =>
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
                setButton.Enabled(true);
            });

            if (Default is Fix32)
            {
                numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field[Id].ToString()));
            }
            else if (Default is int)
            {
                numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field.Integer[Id].ToString()));
            }
            else if (Default is long)
            {
                numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field[Id, "0"]));
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