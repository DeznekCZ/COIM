using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using Mafi;

namespace ProgramableNetwork.Ui;

/// <summary>
/// Dropdown placement for the swap picker.  Behaves like <see cref="DropdownPositionPolicy"/>
/// (opens below the anchor, flips above / pulls back on-screen when it would overflow) but
/// anchors the panel's left edge a fixed amount to the <b>left</b> of the target.  The swap
/// picker rows are laid out as <c>[ title + description card ][ module preview ]</c>, so shifting
/// the whole panel left by the card width lines the preview column up under the module the player
/// is swapping instead of off to its right.
/// </summary>
public class SwapPickerPositionPolicy : IFloatingPositionPolicy
{
    private readonly float m_shiftLeft;

    public SwapPickerPositionPolicy(float shiftLeft)
    {
        m_shiftLeft = shiftLeft;
    }

    public void InitConstraints(UiComponent floatingPanel, UiComponent target)
    {
        // Let the panel size to its own content (previews) rather than the tiny anchor button.
        floatingPanel.MinWidth(Px.NotSet);
    }

    public UnityEngine.Vector2 GetPosition(UnityEngine.Rect boundsOfTarget,
        UnityEngine.Vector2 floatingPanelSize, UnityEngine.Vector2 screenSize)
    {
        // Vertical: below the target, flipped above if it would run off the bottom edge.
        float top = boundsOfTarget.y + boundsOfTarget.height + 3f;
        if (top + floatingPanelSize.y > screenSize.y - 4f)
        {
            top = Math.Max(0f, boundsOfTarget.y - floatingPanelSize.y - 3f);
        }

        // Horizontal: target's left shifted left by the card width, then pulled back on-screen
        // if it would overflow the right (or left) edge.
        float left = boundsOfTarget.x - m_shiftLeft;
        float rightOverflow = left + floatingPanelSize.x + 4f - screenSize.x;
        if (rightOverflow > 0f)
        {
            left -= rightOverflow + 4f;
        }
        left = Math.Max(4f, left);

        return new UnityEngine.Vector2(left, top);
    }
}
