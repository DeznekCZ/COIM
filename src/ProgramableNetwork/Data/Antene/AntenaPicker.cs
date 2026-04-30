using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
    public class AntenaPicker : Row
    {
        private readonly Action m_refresh;
        private readonly Window m_window;
        // Concrete type (not the interface) so we can reach Context.InputScheduler for
        // command dispatch — the interface only exposes selection plumbing.
        private readonly AntenaInspector m_inspector;
        private readonly FMDataBandChannel m_module;
        private readonly Fix32 m_distance;
        private Button m_selectionButton;
        private DisplayWithIcon m_btnPreview;
        private Antena entity;

        public AntenaPicker(FMDataBandChannel module, Fix32 distance, Action refresh, AntenaInspector inspector)
            : base()
        {
            m_refresh = refresh;
            m_inspector = inspector;
            m_module = module;
            m_distance = distance;

            this.Size(Sizes.BLOCK_SIZE * 4, Sizes.BLOCK_SIZE * 2);

            m_btnPreview = AddAndReturn(new DisplayWithIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png))
                .Enabled(false)
                .Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2)
                .OnClick(() => { if (entity is null) {
						return;
					}
					m_inspector.CameraController.PanTo(entity.Position2f); })
                .Margin(0)
                .Icon.Size(Percent.Eighty)
                     .Margin(0)
                     .Padding(0)
                .Parent.As<DisplayWithIcon>().Value;
            m_btnPreview.OnMouseEnter(
                (e) => { if (entity is null) {
						return;
					}
					m_inspector.EntityHighlighter.Highlight(entity, ColorRgba.LightBlue); });
            m_btnPreview.OnMouseLeave(
                (e) => { if (entity is null) {
						return;
					}
					m_inspector.EntityHighlighter.RemoveHighlight(entity); });

            m_selectionButton = AddAndReturn(new ButtonText(new Mafi.Localization.LocStrFormatted("Pick"), PickEntity))
                .Class(Cls.btn_general)
                .Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2);
            m_selectionButton.OnMouseEnter(
                (e) => { if (entity is null) {
						return;
					}
					m_inspector.EntityHighlighter.Highlight(entity, ColorRgba.LightBlue); });
            m_selectionButton.OnMouseLeave(
                (e) => { if (entity is null) {
						return;
					}
					m_inspector.EntityHighlighter.RemoveHighlight(entity); });

            m_inspector.EntitySelectionInput = null;

            SelectionChanged(m_module.Antena);
        }

        private void SelectionChanged(Antena entity)
        {
            // Route the binding through a command. Slot index is the channel's current
            // position in the owning band's redirected list at dispatch time.
            int slot = currentSlot(m_module);
            if (slot >= 0)
            {
                m_inspector.Context.InputScheduler.ScheduleInputCmd(new AntenaChannelSetFmSourceCmd(
                    m_module.OriginalDataBand.Antena.Id, slot, entity?.Id));
            }
            // Local view state still updates immediately so the picker UI is responsive
            // before the command applies.
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
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnPreview.Enabled(false);
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
                (entity) => entity.Prototype == m_module.OriginalDataBand.Antena.Prototype && entity != m_inspector.Entity,
                SelectionChanged);
        }

        private static int currentSlot(FMDataBandChannel channel)
        {
            int i = 0;
            foreach (var c in channel.OriginalDataBand.Channels)
            {
                if (object.ReferenceEquals(c, channel)) { return i; }
                i++;
            }
            return -1;
        }
    }
}