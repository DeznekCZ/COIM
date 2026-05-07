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
	/// MouseUpEvent so left/right + Shift / Alt are read uniformly without polling Input.
	///
	/// Click semantics (modes were retired):
	///   - LMB           → open Add picker
	///   - SHIFT + LMB   → paste from clipboard (last-created module)
	///   - ALT + LMB     → drop the picked-up module here (if any, and slot fits)
	///   - RMB           → open Templates picker
	/// "Disabled" visual is reserved for the case where a module is picked up and this
	/// slot wouldn't fit it — every other state is interactable.
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

			// Slot interactability follows the pickup state:
			//  - Nothing picked up → always interactable (LMB picker, Shift+LMB paste, RMB templates).
			//  - Picked up + slot fits → still interactable (Alt+LMB drop, plus the
			//    other actions remain so the player can paste or pick from a slot
			//    even mid-move).  We just dim slots that wouldn't accept the
			//    drop so the visual cue points at valid drop targets.
			this.Observe(() => view.Inspector.PickedUpModule)
				.Do(picked =>
				{
					m_enabled = picked == null || m_view.IsValidDropAt(m_row, m_col, picked);
					applyVisual();
				});

			applyVisual();
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
					if (inspector.PickedUpModule != null)
					{
						// Drop the picked-up module on this slot — Alt+LMB picked it up
						// from the placed module's symbol button; plain LMB on the
						// destination slot completes the move.  Slots that don't fit
						// are already greyed out via m_enabled (see Observe in the
						// ctor that combines PickedUpModule + IsValidDropAt), so we
						// only get here when the drop is valid.  No modifier required.
						if (!m_view.TryDropPickedAt(m_row, m_col))
						{
							inspector.Context.AudioDb.InvalidOp(true).Play();
						}
					}
					else if (evt.shiftKey)
					{
						// Paste from the "last created" clipboard at this slot.
						if (!m_view.TryShiftAddAt(m_row, m_col))
						{
							inspector.Context.AudioDb.InvalidOp(true).Play();
						}
					}
					else
					{
						m_view.OpenAddPickerAt(m_row, m_col, this);
					}
					evt.StopPropagation();
					break;
				}
				case 1: // right click — open templates picker
				{
					m_view.OpenTemplatePickerAt(m_row, m_col, this);
					evt.StopPropagation();
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
