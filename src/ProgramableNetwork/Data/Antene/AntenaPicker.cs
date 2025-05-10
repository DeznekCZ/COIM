using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork
{
    public class AntenaPicker : UiComponent
    {
        private readonly Action m_refresh;
        private readonly Window m_window;
        private readonly ISelectionInspector<Antena, AntenaSelector, Antena> m_inspector;
        private readonly FMDataBandChannel m_module;
        private readonly string m_dataName;
        private readonly Fix32 m_distance;
        private readonly Action<Antena> m_selected;
        private readonly AntenaProto m_prototype;
        private readonly UiComponent m_btnPreviewHolder;
        private Button m_selectionButton;
        private ButtonIcon m_btnPreview;
        private Antena entity;

        public AntenaPicker(AntenaProto antenaProto, FMDataBandChannel module, Action<Antena> selected, Fix32 distance, Action refresh, Window parentWindow, AntenaInspector inspector)
            : base(new UnityEngine.UIElements.VisualElement())
        {
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspector = inspector;
            m_module = module;
            m_distance = distance;
            m_selected = selected;
            m_prototype = antenaProto;

            m_btnPreviewHolder = new UiComponent();
            m_btnPreviewHolder.Size(Sizes.BLOCK_SIZE * 4, Sizes.BLOCK_SIZE);
            Add(m_btnPreviewHolder);

            m_btnPreview = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
            m_btnPreview.Enabled(false);
            m_btnPreview.OnClick(
                () => { if (entity is null) return; m_inspector.CameraController.PanTo(entity.Position2f); });
            m_btnPreview.OnMouseEnter(
                (e) => { if (entity is null) return; m_inspector.EntityHighlighter.Highlight(entity, ColorRgba.LightBlue); });
            m_btnPreview.OnMouseLeave(
                (e) => { if (entity is null) return; m_inspector.EntityHighlighter.RemoveHighlight(entity); });
            m_selectionButton.OnMouseEnter(
                (e) => { if (entity is null) return; m_inspector.EntityHighlighter.Highlight(entity, ColorRgba.LightBlue); });
            m_selectionButton.OnMouseLeave(
                (e) => { if (entity is null) return; m_inspector.EntityHighlighter.RemoveHighlight(entity); });
            m_btnPreviewHolder.Add(m_btnPreview);

            m_selectionButton = new ButtonText(new Mafi.Localization.LocStrFormatted("Pick"), PickEntity);
            m_selectionButton.Class(Cls.btn_general);
            m_selectionButton.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
            m_selectionButton.OnMouseEnter(
                (e) => m_inspector.EntityHighlighter.Highlight(entity, ColorRgba.LightBlue));
            m_selectionButton.OnMouseLeave(
                (e) => m_inspector.EntityHighlighter.RemoveHighlight(entity));
            Add(m_selectionButton);

            //this.SetHeight(40);

            m_window.OnCloseStart += onHide;

            //m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtn);
            m_inspector.EntitySelectionInput = null;

            SelectionChanged(m_module.Antena);
        }

        private void SelectionChanged(Antena entity)
        {
            m_module.Antena = entity;
            if (entity != null)
            {
                m_btnPreview.Enabled(true);
                m_btnPreview.Icon.Value(entity.GetIcon());
                m_btnPreview.OnClick(
                    () => m_inspector.CameraController.PanTo(entity.Position2f));
                m_btnPreview.OnMouseEnter(
                    (e) => m_inspector.EntityHighlighter.Highlight(entity, ColorRgba.LightBlue));
                m_btnPreview.OnMouseLeave(
                    (e) => m_inspector.EntityHighlighter.RemoveHighlight(entity));
            }
            else
            {
                m_btnPreview = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnPreview.Enabled(false);
                m_selectionButton = new ButtonText(new Mafi.Localization.LocStrFormatted("Pick"), PickEntity);
            }
            m_selectionButton.ClassRemove(Cls.bold);
        }

        private void onHide(Window w)
        {
            m_window.OnCloseStart -= onHide;
            m_inspector.EntitySelectionInput = null;
        }

        private void PickEntity()
        {
            m_selectionButton.Class(Cls.bold);
            m_inspector.EntitySelectionInput = new AntenaSelector(
                m_distance,
                m_refresh,
                (entity) => entity.Prototype == m_prototype && entity != m_inspector.Entity,
                SelectionChanged);
        }
    }
}