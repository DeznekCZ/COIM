using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Serialization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Linq;
using System.Reflection;
using static Mafi.Unity.Assets.Unity;

namespace ProgramableNetwork.Ui
{
    internal class ColorPicker : FloatingColumn
    {
        private readonly Func<Controller> m_controllerSelector;
        private readonly PanelWithHeader m_panel;
        private readonly Slider m_sliderR;
        private readonly Slider m_sliderG;
        private readonly Slider m_sliderB;
        private readonly Icon m_colorPreviewIcon;
        private readonly ButtonIcon m_apply;
        private ColorRgba m_previewColor;

        public ColorPicker(Func<Controller> controllerSelector)
            : base(new DropdownPositionPolicy(), true, false, true)
        {
            this.m_controllerSelector = controllerSelector;

            Add(m_panel = new PanelWithHeader("Color".AsLoc()));

            Label label = (Label)m_panel.GetType()
                .GetField("m_title", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(m_panel);
            label.TextAlign(TextAlignment.CenterMiddle);
            label.Fill();

            m_panel.Header.Gap(5);

            m_panel.Header.Add(m_colorPreviewIcon = new Icon(UserInterface.General.Circle_svg));
            this.Observe(() => m_previewColor)
                .Do(color => m_colorPreviewIcon.Color(color));

            m_panel.Header.Add(m_apply = new ButtonIcon(UserInterface.General.Save_svg));
            // TODO command
            m_apply.OnClick(() => m_controllerSelector().SetColor(m_previewColor));

            m_sliderR = new Slider();
            m_sliderB = new Slider();
            m_sliderG = new Slider();

            m_panel.Body.Gap(5);
            m_panel.Body.Add(
                new Row
                {
                    new Label("Red".AsLoc()).Width(100),
                    m_sliderR.Width(250)
                             .Range(0, 255)
                },
                new Row
                {
                    new Label("Green".AsLoc()).Width(100),
                    m_sliderG.Width(250)
                             .Range(0, 255)
                },
                new Row
                {
                    new Label("Blue".AsLoc()).Width(100),
                    m_sliderB.Width(250)
                             .Range(0, 255)
                });

            m_sliderR.OnValueChanged((_,value) => m_previewColor = m_previewColor.SetR((byte)(int)value.Max(0).Min(255)));
            m_sliderG.OnValueChanged((_,value) => m_previewColor = m_previewColor.SetG((byte)(int)value.Max(0).Min(255)));
            m_sliderB.OnValueChanged((_,value) => m_previewColor = m_previewColor.SetB((byte)(int)value.Max(0).Min(255)));

            OnOpen += ColorPicker_OnOpen;
        }

        private void ColorPicker_OnOpen(FloatingColumn obj)
        {
            m_previewColor = m_controllerSelector().Color;
            m_sliderR.Value(m_previewColor.R);
            m_sliderG.Value(m_previewColor.G);
            m_sliderB.Value(m_previewColor.B);
        }
    }
}