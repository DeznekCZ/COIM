using Mafi.Core.Entities;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using ProgramableNetwork;
using System.Linq;

namespace Mafi.Unity
{
    public class TooltipView<TInspector, TEntity, TView>
        where TInspector : EntityInspector<TEntity, TView>
        where TEntity : class, IEntity
        where TView : ItemDetailWindowView
    {
        private readonly TInspector m_inspector;
        private readonly string m_id;
        private Txt content;

        public TooltipView(TInspector inspector, string id)
        {
            m_inspector = inspector;
            m_id = id;
        }

        public void SetTooltip(string tooltip, Offset? offset, IUiElement parent)
        {
            if (tooltip == null)
            {
                ClearTooltip();
                return;
            }

            content = m_inspector.Builder.NewTxt(m_id);
            content.SetTextStyle(m_inspector.Builder.Style.Panel.Text.Extend(fontSize: 15));
            content.SetText(tooltip);
            content.SetBackground(ColorRgba.Black.SetA(127));
            content.SetSize(content.GetPreferredSize(250, 600));
            content.PutToLeftTopOf(parent ?? m_inspector.GetView(), content.GetSize());
            if (offset != null)
                content.RectTransform.position += new UnityEngine.Vector3(
                    offset.Value.LeftOffset < 0
                        ? offset.Value.LeftOffset - content.GetWidth()
                        : offset.Value.LeftOffset,
                    offset.Value.TopOffset < 0
                        ? offset.Value.TopOffset - content.GetHeight()
                        : offset.Value.TopOffset + 20 * tooltip.Count(c => c == '\n'));
            else
                content.RectTransform.position -= new UnityEngine.Vector3(content.GetWidth() + 20, 20 * tooltip.Count(c => c=='\n'));
            content.SendToFront();
            content.SetParent(m_inspector.Builder.MainCanvas, true);
        }

        public void ClearTooltip()
        {
            content?.Destroy();
            content = null;
        }
    }
}