using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Empty-slot "+" button for the variable-bus gutter — the bus-side counterpart of
	/// <see cref="ModuleSlotButton"/>.  Custom <see cref="UiComponent"/> on purpose so it
	/// shares the module slot's visual language (idle / hover background + border + label
	/// colour) instead of the base game's button classes, and routes its click through a
	/// single MouseUpEvent.
	///
	/// Click semantics:
	///   - LMB → create a new bus on THIS gutter's side (the slot's side becomes the
	///           bus's side, exactly like the old inline "+" did).
	/// The slot is always interactable — unlike module slots there is no pick-up/drop
	/// state to gate on — so there is no disabled visual.
	/// </summary>
	public class BusSlotButton : UiComponent
	{
		// Mirror ModuleSlotButton's palette so the two slot kinds read as siblings.
		private static readonly ColorRgba COLOR_BG_IDLE  = ColorRgba.DarkDarkGray.SetA(110);
		private static readonly ColorRgba COLOR_BG_HOVER = ColorRgba.CornflowerBlue.SetA(160);
		private static readonly ColorRgba COLOR_BD_IDLE  = ColorRgba.Gray.SetA(180);
		private static readonly ColorRgba COLOR_BD_HOVER = ColorRgba.White.SetA(220);
		private static readonly ColorRgba COLOR_TX_IDLE  = ColorRgba.LightGray;
		private static readonly ColorRgba COLOR_TX_HOVER = ColorRgba.White;

		private readonly ControllerView m_view;
		private readonly ControllerBus.BusSide m_side;

		private readonly Label m_label;
		private bool m_hovered;

		public BusSlotButton(ControllerView view, ControllerBus.BusSide side)
		{
			m_view = view;
			m_side = side;

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

			applyVisual();
		}

		private void handleMouseUp(UnityEngine.UIElements.MouseUpEvent evt)
		{
			if (evt.button != 0)
			{
				return;
			}
			Controller entity = m_view.Entity;
			entity.CreateBus("bus" + (entity.Buses.Count + 1), m_side);
			m_view.RedrawComponents();
			evt.StopPropagation();
		}

		private void applyVisual()
		{
			this.Background(m_hovered ? COLOR_BG_HOVER : COLOR_BG_IDLE);
			this.Border(all: 1.px(), color: m_hovered ? COLOR_BD_HOVER : COLOR_BD_IDLE, radius: 4);
			m_label.Color(m_hovered ? COLOR_TX_HOVER : COLOR_TX_IDLE);
		}
	}
}
