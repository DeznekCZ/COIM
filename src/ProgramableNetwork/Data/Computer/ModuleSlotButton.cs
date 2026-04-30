using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Empty-slot "+" button used in <see cref="ControllerView"/>'s module grid.
	/// Custom <see cref="UiComponent"/> on purpose so it does NOT inherit any of the base
	/// game's button classes — visual state (idle / hover / disabled) is controlled here
	/// via Background + Border + Label colour, and click routing goes through a single
	/// MouseUpEvent so left/right/shift+left are handled uniformly without polling Input.
	///
	/// All click-time decisions (which mode is active, what to do with the click) live
	/// in this class; the consumer only hands over (view, row, column) and the button
	/// dispatches into <see cref="ControllerView"/>'s public slot helpers.
	/// </summary>
	public class ModuleSlotButton : UiComponent
	{
		// Visual palette — local so the slot looks distinct from base-game buttons.
		private static readonly ColorRgba COLOR_BG_IDLE     = ColorRgba.DarkDarkGray.SetA(110);
		private static readonly ColorRgba COLOR_BG_HOVER    = ColorRgba.CornflowerBlue.SetA(160);
		private static readonly ColorRgba COLOR_BG_DISABLED = ColorRgba.Black.SetA(50);
		private static readonly ColorRgba COLOR_BD_IDLE     = ColorRgba.Gray.SetA(180);
		private static readonly ColorRgba COLOR_BD_HOVER    = ColorRgba.White.SetA(220);
		private static readonly ColorRgba COLOR_BD_DISABLED = ColorRgba.Black.SetA(80);
		private static readonly ColorRgba COLOR_TX_IDLE     = ColorRgba.LightGray;
		private static readonly ColorRgba COLOR_TX_HOVER    = ColorRgba.White;
		private static readonly ColorRgba COLOR_TX_DISABLED = ColorRgba.Gray.SetA(120);

		// Just enough state to do the slot's job — no controller, no module references.
		private readonly ControllerView m_view;
		private readonly int m_row;
		private readonly int m_col;

		private readonly Label m_label;
		private bool m_enabled = true;
		private bool m_hovered;

		public ModuleSlotButton(ControllerView view, int row, int column)
		{
			m_view = view;
			m_row = row;
			m_col = column;

			// Plain UiComponent doesn't pick up the IFlexComponent layout extensions, so
			// reach for the Unity flex style directly to centre the "+" label.
			RootElement.style.alignItems     = UnityEngine.UIElements.Align.Center;
			RootElement.style.justifyContent = UnityEngine.UIElements.Justify.Center;

			m_label = AddAndReturn(new Label("+".AsLoc()))
				.TextAlign(TextAlignment.CenterMiddle)
				.FontSize(20)
				.Color(COLOR_TX_IDLE);

			this.OnMouseEnterLeave(
				() => { m_hovered = true;  applyVisual(); },
				() => { m_hovered = false; applyVisual(); });

			this.RegisterCallback<UnityEngine.UIElements.MouseUpEvent>(handleMouseUp);

			// Slot lock-down per current mode, observed live:
			//  - Edit: never interactable.
			//  - Add: always interactable (open picker / shift-paste / right-click templates).
			//  - Move + nothing picked up: locked, nothing to drop here.
			//  - Move + picked-up: only if the picked module would actually fit here.
			this.Observe(() => view.Inspector.Mode)
				.Observe(() => view.Inspector.PickedUpModule)
				.Do((mode, picked) =>
				{
					m_enabled = computeEnabled(mode, picked);
					applyVisual();
				});

			applyVisual();
		}

		private bool computeEnabled(ControllerEditMode mode, Module picked)
		{
			switch (mode)
			{
				case ControllerEditMode.Add:  return true;
				case ControllerEditMode.Move: return picked != null && m_view.IsValidDropAt(m_row, m_col, picked);
				case ControllerEditMode.Edit:
				default: return false;
			}
		}

		private void handleMouseUp(UnityEngine.UIElements.MouseUpEvent evt)
		{
			var inspector = m_view.Inspector;
			switch (evt.button)
			{
				case 0: // left click
				{
					if (!m_enabled)
					{
						inspector.Context.AudioDb.InvalidOp(true).Play();
						evt.StopPropagation();
						return;
					}
					if (inspector.Mode == ControllerEditMode.Move)
					{
						if (!m_view.TryDropPickedAt(m_row, m_col))
						{
							inspector.Context.AudioDb.InvalidOp(true).Play();
						}
					}
					else // Add mode (Edit is already filtered out by m_enabled)
					{
						if (evt.shiftKey)
						{
							if (!m_view.TryShiftAddAt(m_row, m_col))
							{
								inspector.Context.AudioDb.InvalidOp(true).Play();
							}
						}
						else
						{
							m_view.OpenAddPickerAt(m_row, m_col, this);
						}
					}
					evt.StopPropagation();
					break;
				}
				case 1: // right click — templates (Add mode only)
				{
					if (inspector.Mode == ControllerEditMode.Add)
					{
						m_view.OpenTemplatePickerAt(m_row, m_col, this);
						evt.StopPropagation();
					}
					break;
				}
			}
		}

		private void applyVisual()
		{
			ColorRgba bg, bd, tx;
			if (!m_enabled)         { bg = COLOR_BG_DISABLED; bd = COLOR_BD_DISABLED; tx = COLOR_TX_DISABLED; }
			else if (m_hovered)     { bg = COLOR_BG_HOVER;    bd = COLOR_BD_HOVER;    tx = COLOR_TX_HOVER;    }
			else                    { bg = COLOR_BG_IDLE;     bd = COLOR_BD_IDLE;     tx = COLOR_TX_IDLE;     }
			this.Background(bg);
			this.Border(all: 1.px(), color: bd, radius: 4);
			m_label.Color(tx);
		}
	}
}
