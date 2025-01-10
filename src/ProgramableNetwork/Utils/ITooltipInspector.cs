using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using System;

namespace Mafi.Unity
{
    public interface ITooltipInspector
    {
        UiBuilder Builder { get; }
        void OnMouseIn(string tooltip, Offset? offset, IUiElement parent);
        void OnMouseOut();
    }
}