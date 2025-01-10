using Mafi.Core.Entities;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using ProgramableNetwork;

namespace Mafi.Unity
{
    public class TooltipView<TInspector, TEntity, TView>
        where TInspector : EntityInspector<TEntity, TView>
        where TEntity : class, IEntity
        where TView : ItemDetailWindowView
    {
        private readonly TInspector m_inspector;
        private Txt content;

        public TooltipView(TInspector inspector, string id, bool isOnMainCanvas = true)
        {
            m_inspector = inspector;
            content = m_inspector.Builder.NewTxt(id)
                .SetText("")
                .SetSize(0, 0);
        }

        public void SetTooltip(string tooltip, Offset? offset, IUiElement parent)
        {
            content.SetText(tooltip);
            content.SetBackground(ColorRgba.Black);
            content.SetSize(content.GetPreferredSize(250, 600));
            content.PutToLeftTopOf(parent ?? m_inspector.GetView(), content.GetSize());
            if (offset != null)
                content.RectTransform.position += new UnityEngine.Vector3(
                    offset.Value.LeftOffset < 0
                        ? offset.Value.LeftOffset - content.GetWidth()
                        : offset.Value.LeftOffset,
                    offset.Value.TopOffset < 0
                        ? offset.Value.TopOffset - content.GetHeight()
                        : offset.Value.TopOffset);
            else
                content.RectTransform.position -= new UnityEngine.Vector3(content.GetWidth() + 20, 0);
            content.SendToFront();
            content.SetParent(m_inspector.Builder.MainCanvas, true);
        }

        public void ClearTooltip()
        {
            content.SetText("");
            content.SetSize(0, 0);
        }
    }
}