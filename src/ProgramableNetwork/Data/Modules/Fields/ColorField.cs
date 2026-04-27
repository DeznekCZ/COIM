using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Globalization;
using Mafi.Unity.Ui.Library;

namespace ProgramableNetwork.Ui
{
    public class ColorField : IField
    {
        public ColorField(string id, Proto.Str strs, int defaultValue)
			: this(id, strs, new ColorRgba(defaultValue))
        {
        }

        public ColorField(string id, Proto.Str strs, ColorRgba defaultValue)
        {
            Id = id;
            Name = strs.Name;
            Default = defaultValue;
            ShortDesc = strs.DescShort;
        }

        public string Id { get; }
        public LocStr Name { get; }
        public LocStr ShortDesc { get; }

        public int Size => 20;

        public ColorRgba Default { get; }

		public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog)
        {
            RowContainer row = fieldContainer.Row(this, module, out _, useFiller: false);

            var colorPicker = new RgbColorPicker();
            colorPicker.Value(module.Field[Id, Default.AsFix32].AsColorRgba);
            colorPicker.Width(200 - Sizes.BLOCK_SIZE * 1.5f);
            colorPicker.Height(Px.Auto);
            row.Add(colorPicker);

            var setButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Save_svg);
            setButton.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
            setButton.Width(Sizes.BLOCK_SIZE * 1.5f);
            setButton.Height(Sizes.BLOCK_SIZE);
            setButton.Enabled(false);
            row.Add(setButton);

			colorPicker.OnColorChanged((c) => setButton.Enabled(true));
            setButton.OnClick(() =>
            {
                module.Field[Id] = colorPicker.GetColor().AsFix32;
                setButton.Enabled(false);
            });
        }

        public void InitData(Module module)
        {
			module.Field[Id] = Default.AsFix32;
        }

        public void Validate(Module module)
        {
        }
    }
}