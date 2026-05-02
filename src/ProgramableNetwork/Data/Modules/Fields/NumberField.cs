using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Globalization;

namespace ProgramableNetwork.Ui
{
    public class NumberField<T> : IField
    {
        public NumberField(string id, Proto.Str strs, T defaultValue, bool showInTooltip = false)
        {
            Id = id;
            Name = strs.Name;
            Default = defaultValue;
            ShortDesc = strs.DescShort;
            ShowInTooltip = showInTooltip;
        }

        public string Id { get; }
        public LocStr Name { get; }
        public LocStr ShortDesc { get; }
        public bool ShowInTooltip { get; }

        public int Size => 20;

        public T Default { get; }

        public string GetTooltipValue(Module module)
        {
            if (typeof(T) == typeof(Fix32))    return module.Field[Id].ToString();
            if (typeof(T) == typeof(int))      return module.Field.Integer[Id].ToString();
            if (typeof(T) == typeof(long))     return module.Field[Id, "0"];
            if (typeof(T) == typeof(HexInt32)) return module.Field[Id].RawValue.ToString("X");
            return module.Field[Id].ToString();
        }

        private Action setter;
        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog)
        {
            RowContainer row = fieldContainer.Row(this, module, uiContext, out _);

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
                uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
                    module.Controller.Id, module.Id, Id, changeValue));
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
                        setter = () => uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
                            module.Controller.Id, module.Id, Id, value.ToFix32()));
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(numberEditor.GetText(), out int value))
                    {
                        setter = () => uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
                            module.Controller.Id, module.Id, Id, Fix32.FromInt(value)));
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    if (long.TryParse(numberEditor.GetText(), out long value))
                    {
                        setter = () => uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
                            module.Controller.Id, module.Id, Id, value.ToString()));
                    }
                }
                else if (typeof(T) == typeof(HexInt32)) {
					if (uint.TryParse(numberEditor.GetText(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
					{
						setter = () => uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
							module.Controller.Id, module.Id, Id, Fix32.FromRaw((int)value)));
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
            else if (Default is HexInt32)
            {
                numberEditor.Value(new Mafi.Localization.LocStrFormatted(module.Field[Id].RawValue.ToString("X")));
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
            else if (Default is HexInt32 h)
            {
                module.Field[Id] = Fix32.FromRaw(h.Value);
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

	public struct HexInt32 {
		public int Value;
	}
}