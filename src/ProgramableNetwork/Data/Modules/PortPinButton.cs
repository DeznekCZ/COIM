using Mafi;
using Mafi.Unity.UiToolkit.Component;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Custom port-pin button used in ModuleView's input/output rows.  Replaces the
	/// previous <c>ButtonText("○"/"◎")</c> approach so the circle glyph is rendered as a
	/// real shape (border-radius round element) rather than a Unicode char floated inside
	/// a rectangular text button — fixing the off-centre alignment that the text-glyph
	/// variant suffered from.
	///
	/// Inherits from <see cref="UiComponent"/> directly (no base-game button class) so
	/// the visual language stays self-contained:
	///   - cell background = transparent at rest, port-kind colour on hover,
	///   - inner circle = "connected" indicator (filled vs ring),
	///   - "open" state = the port is a live next-click target → circle enlarges.
	/// Connection highlights live exclusively on the cables now (they already convey
	/// "this output → that input"), so the cell colour does NOT respond to the
	/// selected/highlighted state.  Click routing, tooltip, observers, and
	/// enabled-state are all standard UiComponent extensions, so call sites keep
	/// their familiar <c>.OnClick / .OnRightClick / .Observe / .Tooltip / .ObserveEnabled</c>
	/// shape.  Drive the visual via <see cref="Connected"/> and <see cref="Open"/>.
	/// </summary>
	public class PortPinButton : UiComponent
	{
		public enum PortKind { Input, Output }

		// Cell colours.  The opaque parts of the visual are the pinhole (border +
		// fill) and the cables themselves — the cell stays transparent so the
		// underlying row colour shows through.  Hover flashes the port-kind colour
		// briefly to confirm the cursor target.
		private static readonly ColorRgba INPUT_BG_IDLE   = ColorRgba.Empty; // transparent
		private static readonly ColorRgba INPUT_BG_HOVER  = ColorRgba.Green;
		private static readonly ColorRgba OUTPUT_BG_IDLE  = ColorRgba.Empty; // transparent
		private static readonly ColorRgba OUTPUT_BG_HOVER = ColorRgba.Red;

		// Border colour is always black.  The pinhole stays opaque even when the
		// cell behind it is transparent — unconnected pins fill the disc with black
		// (reads as a "plug hole"), connected pins paint the matching cable colour
		// via DotColor() so the wire's hue is recognisable at the endpoint.
		private static readonly ColorRgba RING_COLOR        = ColorRgba.Black;
		private static readonly ColorRgba DOT_DEFAULT_FILL  = ColorRgba.Black;

		// Closed = baseline circle (universal idle look for both inputs and outputs).
		// Open   = enlarged circle when this pin is a live next-click target — only
		//          inputs use this state (when an output has been picked); outputs
		//          stay closed and look identical to idle inputs.
		private const int DOT_PX_CLOSED = 14;
		private const int DOT_PX_OPEN   = 20;
		private const int RING_PX       = 2;

		private readonly PortKind m_kind;
		private readonly UiComponent m_dot;
		private bool m_connected;
		private bool m_open;
		private bool m_hovered;
		private ColorRgba m_dotFill = DOT_DEFAULT_FILL;

		public PortPinButton(PortKind kind, bool connected)
		{
			m_kind = kind;
			m_connected = connected;

			// Plain UiComponent doesn't pick up the IFlexComponent layout extensions;
			// reach for the Unity flex style directly to centre the inner dot.
			RootElement.style.alignItems     = UnityEngine.UIElements.Align.Center;
			RootElement.style.justifyContent = UnityEngine.UIElements.Justify.Center;
			this.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);

			m_dot = AddAndReturn(new UiComponent());
			m_dot.IgnoreInputPicking();

			this.OnMouseEnterLeave(
				() => { m_hovered = true;  applyVisual(); },
				() => { m_hovered = false; applyVisual(); });

			applyVisual();
		}

		// Switches the inner indicator between ring (unconnected) and filled dot (connected).
		public PortPinButton Connected(bool value)
		{
			if (m_connected == value) {
				return this;
			}
			m_connected = value;
			applyVisual();
			return this;
		}

		// Marks this pin as a live next-click target — the circle enlarges so the
		// user can scan the row and see which pins are accepting input right now.
		public PortPinButton Open(bool value)
		{
			if (m_open == value) {
				return this;
			}
			m_open = value;
			applyVisual();
			return this;
		}

		// Sets the inner-circle fill colour — paint with the matching cable's hue so
		// the user can trace a wire back to its endpoint at a glance.  Only takes
		// effect while <see cref="Connected"/> is true; unconnected ports always show
		// a hollow ring regardless of fill colour.
		public PortPinButton DotColor(ColorRgba color)
		{
			if (m_dotFill.Rgba == color.Rgba) {
				return this;
			}
			m_dotFill = color;
			applyVisual();
			return this;
		}

		private void applyVisual()
		{
			ColorRgba bgIdle  = m_kind == PortKind.Input ? INPUT_BG_IDLE  : OUTPUT_BG_IDLE;
			ColorRgba bgHover = m_kind == PortKind.Input ? INPUT_BG_HOVER : OUTPUT_BG_HOVER;
			this.Background(m_hovered ? bgHover : bgIdle);

			int dotPx     = m_open ? DOT_PX_OPEN : DOT_PX_CLOSED;
			int dotRadius = dotPx / 2;
			m_dot.Size(dotPx.px(), dotPx.px());
			// Border is always a black ring; the fill is opaque too.  Unconnected
			// ports default to black (looks like an empty plug hole); connected
			// ports paint with the cable colour set via DotColor().
			m_dot.Background(m_connected ? m_dotFill : DOT_DEFAULT_FILL);
			m_dot.Border(all: RING_PX.px(), color: RING_COLOR, radius: dotRadius);
		}
	}
}
