using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity;
using System.Collections.Generic;
using System.Linq;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi;
using Mafi.Core.Entities.Dynamic;
using Mafi.Localization;
using System;
using ProgramableNetwork.Python;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.Ui.Library;
using Mafi.Core.Entities.Static;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using Mafi.Core.SaveGame;
using Mafi.Core.Input;
using UnityEngine;
using System.Xml.Serialization;
using Mafi.Unity.Ports.Io;
using Mafi.Unity.UiToolkit;

namespace ProgramableNetwork.Ui
{
	public class Sizes
	{
		public static readonly Px BLOCK_SIZE = 32.px();
		public static readonly Px IMAGE_SIZE = 28.px();
		public static readonly Px IMAGE_PADDING = 2.px();
	}

	public partial class ControllerView : Column
	{
		public static Module m_lastCreated;
		private LocStr? m_decription;
		private Category m_category;
		private readonly ControllerInspector m_controller;
		private List<IDataUpdater> m_updaters;
		private bool m_pickNew;
		private readonly Action m_refresh;

		private bool m_pickTemplateModuleInAction;
		private bool m_pickNewModuleInAction;
		private bool m_settingsDialogInAction;

		private static readonly Lyst<Color> m_colors;

		static ControllerView() {
			Color initialColor = ColorRgba.CornflowerBlue.SetA(127).ToColor(); // line color
			m_colors = [initialColor];
			for (int i = 0; i < 64; i++)
			{
				// Generate a set of 10 distinct colors by changing the hue of the initial color
				Color lastColor = IIndexableExtensions.Last(m_colors);
				Color.RGBToHSV(lastColor, out float h, out float s, out float v);
				h += 0.11f; // Change hue by 10% for each new color
				if (h > 1) {
					h -= 1; // Wrap around if hue exceeds 1
				}
				Color newColor = Color.HSVToRGB(h, s, v);
				newColor.a = initialColor.a; // Keep the same alpha value
				m_colors.Add(newColor);
			}
		}

		public ControllerView(ControllerInspector controller, Action refresh)
			: base(gap: 0) // gap is now provided by explicit channel rows we insert ourselves
		{
			m_controller = controller;
			m_updaters = new List<IDataUpdater>();
			m_refresh = refresh;
			// Padding gives the absolute-positioned cable corridors visible room above
			// and to the sides of the module rows.  Bottom is left flush with the panel.
			// Lane offsets are capped (MAX_LANE_OFFSET_PX) so cables stay inside the
			// padding zone — no overflow:hidden trickery needed.
			this.Padding(top: VIEW_PAD_TOP, right: VIEW_PAD, bottom: Px.Zero, left: VIEW_PAD);
			AddModuleImplementation(refresh);

			// Per-segment visibility is observed inside CreateConnectionPath, so no
			// global img-toggle observer is needed here anymore.
		}

		public Controller Entity => m_controller.Entity;

		public Dictionary<long, (int x, int y)> ModulePlacementCache { get; } = new Dictionary<long, (int x, int y)>();
		public ControllerInspector Inspector => m_controller;

		public Module LastCreated => m_lastCreated;

		private void AddModuleImplementation(Action refresh)
		{
			this.Observe(() => Entity).Do((entity) => {
				m_decription = null;
				m_controller.OutputConnection = null;
				m_controller.EntityHighlighterSelectable.ClearAllHighlights();
				m_updaters.Clear();

				if (entity != null)
				{
					RedrawComponents(Entity.Modules.Select(m => m.Id).ToLyst());
				}
			});
			this.Observe(WasOrderChanged, new ModuleIdComparator()).Do(RedrawComponents);
		}

		private IEnumerable<long> WasOrderChanged()
		{
			if (Entity?.Modules == null) {
				yield break;
			}
			foreach (var m in Entity.Modules)
			{
				// Encode id+row+col+ext so any change to module set, positions, OR any of the
				// three extension dimensions triggers a redraw — adding/removing extensions
				// changes Layout.GetWidth and the row's free slots have to refold around the
				// new footprint.  Cached ModuleViews persist across the redraw so the
				// per-module flicker stays minimal.
				yield return m.Id
					^ ((long)m.Row << 40)
					^ ((long)m.Column << 24)
					^ ((long)m.InputExtensionCount << 16)
					^ ((long)m.OutputExtensionCount << 8)
					^ ((long)m.DisplayExtensionCount);
			}
		}

		/// <summary>
		/// True if the given module can grow by one cell on its right side — the cell at
		/// (row, currentRightEdge) is in-grid and not occupied by any other module.  Used
		/// by the inspector and inline edge "+" button to disable Add when there's no room.
		/// </summary>
		public bool CanExtendModule(Module module)
		{
			if (module == null || Entity?.Prototype == null) {
				return false;
			}
			int rightEdge = module.Column + module.Layout.GetWidth(module);
			return IsRangeFree(module.Row, rightEdge, 1, ignore: module);
		}

		public void RedrawComponents()
		{
			if (Entity != null)
			{
				RedrawComponents(new Lyst<long>());
			}
		}

		private void RedrawComponents(Lyst<long> modules)
		{
			Clear();
			ModulePlacementCache.Clear();

			int totalRows = Entity.Prototype.Rows;
			int totalCols = Entity.Prototype.Columns;
			float bs = (float)Sizes.BLOCK_SIZE.Pixels;
			Px rowH = Sizes.BLOCK_SIZE * 4;
			Px rowW = Sizes.BLOCK_SIZE * totalCols;
			m_channelBlockHeight = 4f * bs;

			// Bucket modules by row (ignoring out-of-range rows defensively).
			var byRow = new Dictionary<int, List<Module>>();
			foreach (var m in Entity.Modules ?? new Lyst<Module>())
			{
				if (m == null || m.Prototype == null) {
					continue;
				}
				if (m.Row < 0 || m.Row >= totalRows) {
					continue;
				}
				if (!byRow.TryGetValue(m.Row, out var list)) { list = new List<Module>(); byRow[m.Row] = list; }
				list.Add(m);
			}

			// --- Pre-pass: collect every cable's routing spec -------------------------------
			// channel index 0      = top channel (above row 0).
			// channel index r + 1  = gap above row r (between row r-1 and row r), for r >= 1.
			int channelCount = totalRows + 1;
			m_cableSpecs.Clear();
			float pad = VIEW_PAD_PX;

			foreach (Module dstMod in Entity.Modules ?? new Lyst<Module>())
			{
				if (dstMod?.Prototype == null) {
					continue;
				}
				foreach (var kv in dstMod.InputModules)
				{
					Module srcMod = (Entity.Modules ?? new Lyst<Module>()).Find(m => m.Id == kv.Value.ModuleId);
					if (srcMod?.Prototype == null) {
						continue;
					}
					if (srcMod.Row < 0 || srcMod.Row >= totalRows) {
						continue;
					}
					if (dstMod.Row < 0 || dstMod.Row >= totalRows) {
						continue;
					}

					var srcOut = srcMod.GetOutputProto(kv.Value.OutputId);
					var dstIn  = dstMod.GetInputProto(kv.Key);
					if (srcOut == null || dstIn == null) {
						continue;
					}

					int srcOutCol = srcMod.GetPinColumn(kv.Value.OutputId, isOutput: true);
					int dstInCol  = dstMod.GetPinColumn(kv.Key, isOutput: false);

					// Universal rule: source side ALWAYS uses the channel below src (output is
					// at the bottom of src, so the cable drops into the channel right below).
					// Destination side ALWAYS uses the channel above dst (input is at the top
					// of dst, so the cable comes down from the channel right above).
					// When src and dst are vertically adjacent (src above dst), these two
					// channels happen to be the same — and the 3-segment direct route falls
					// out naturally; otherwise a 5-segment wrap is used.
					int srcChannel = srcMod.Row + 1;
					int dstChannel = dstMod.Row;

					m_cableSpecs.Add(new CableSpec {
						Src = srcMod, OutputId = kv.Value.OutputId,
						Dst = dstMod, InputId = kv.Key,
						SrcChannelIdx = srcChannel, DstChannelIdx = dstChannel,
						// WrapLeft assigned by AssignWrapSides() after sort — needs the full
						// cable set to balance load evenly between left and right corridors.
						WrapLeft = false,
						// ColorIndex assigned after sort below so palette ordering follows
						// (src.Row, src.Column, dst.Row, dst.Column) — same as iteration order.
						ColorIndex = 0,
						SrcXp = pad + (srcOutCol + 0.5f) * bs,
						DstXp = pad + (dstInCol + 0.5f) * bs,
					});
				}
			}

			// Order cables by (src.Row, src.Column, dst.Row, dst.Column).  This drives:
			//   - palette assignment (top-left source → first colour, deterministic),
			//   - RepaintLines draw order (z-order: later rows paint over earlier ones),
			//   - tie-breaking inside the per-channel sort when XMin ties.
			m_cableSpecs.Sort((a, b) =>
			{
				int c = a.Src.Row.CompareTo(b.Src.Row);
				if (c != 0) {
					return c;
				}
				c = a.Src.Column.CompareTo(b.Src.Column);
				if (c != 0) {
					return c;
				}
				c = a.Dst.Row.CompareTo(b.Dst.Row);
				if (c != 0) {
					return c;
				}
				return a.Dst.Column.CompareTo(b.Dst.Column);
			});

			// Palette is NOT reset between redraws — every (sourceModuleId, outputId)
			// pair gets a stable colour index for the lifetime of this view.  Without
			// this, removing/re-adding a cable could shuffle indices and the pin dot
			// (which only re-paints on connect/disconnect transitions) would end up
			// out of sync with the cable's new colour.
			foreach (var cable in m_cableSpecs)
			{
				cable.ColorIndex = ColourPaletteIndex(cable.Src.Id, cable.OutputId);
			}

			// Decide left vs right corridor for each wrapping cable, balancing load
			// between sides instead of letting every cable pick its geometric near
			// side independently (which used to bunch all cables on one corridor
			// when the modules in use clustered in one half of the layout).
			AssignWrapSides();

			// --- Sort + assign per-channel lane indices -----------------------------------
			// In each channel, group cable-ends by which side of the channel they're
			// connecting to (TOP = row above the channel, BOTTOM = row below) so
			// "shortest path" cables sit closer to the row they connect to, and within
			// each side we sort by X to keep enter/exit verticals from crossing each other.
			m_channelLaneCounts = new int[channelCount];
			AssignChannelLanes(channelCount);

			// --- Sort + assign side-corridor lane indices ---------------------------------
			AssignSideLanes();

			m_channelHeights = new float[channelCount];
			for (int i = 0; i < channelCount; i++)
			{
				m_channelHeights[i] = ComputeChannelHeight(m_channelLaneCounts[i]);
			}

			// Top channel — empty UiComponent, height observes the precomputed cable count.
			Add(new UiComponent().Width(rowW).Height(m_channelHeights[0].px()));

			for (int i = 0; i < totalRows; i++)
			{
				// Module row — full height regardless of contents (empty rows look the same as full).
				var rowElement = new Row();
				rowElement.Height(rowH);
				rowElement.Width(rowW);

				if (!byRow.TryGetValue(i, out var rowModules))
				{
					rowModules = new List<Module>(0);
				}
				rowModules.Sort((a, b) => a.Column.CompareTo(b.Column));

				int cursor = 0;
				int idx = 0;
				while (cursor < totalCols)
				{
					Module placed = null;
					while (idx < rowModules.Count && rowModules[idx].Column < cursor)
					{
						idx++;
					}
					if (idx < rowModules.Count && rowModules[idx].Column == cursor)
					{
						placed = rowModules[idx];
						idx++;
					}

					if (placed == null)
					{
						AddFreeSlot(rowElement, i, cursor);
						cursor++;
					}
					else
					{
						int width = placed.Layout.GetWidth(placed);
						if (width <= 0) {
							width = 1;
						}
						if (cursor + width > totalCols) {
							width = totalCols - cursor;
						}
						// Always build a fresh ModuleView on each redraw.  An earlier
						// caching attempt — keyed on Module.Id to preserve Observe
						// subscriptions across redraws — handed the previous
						// controller's ModuleView back when switching to a new
						// controller whose modules carried colliding legacy ids
						// (every per-controller pool started at 1), making the
						// inspector stick on the first selected controller.  Fresh
						// views per redraw are slightly less efficient but keep the
						// state correct without cross-controller bleed.
						rowElement.Add(new ModuleView(placed, this, m_controller.Context, false, () => RedrawComponents(modules)));
						ModulePlacementCache[placed.Id] = (i, cursor);
						cursor += width;
					}
				}

				Add(rowElement);

				// Channel row below this module row — height already sized to the lane count.
				Add(new UiComponent().Width(rowW).Height(m_channelHeights[i + 1].px()));
			}

			RepaintLines();
		}

		// Top edge Y (in absolute pad-relative coords) of the channel at index i.
		private float channelTopY(int channelIdx)
		{
			float y = VIEW_PAD_TOP_PX; // top padding pushes content down by this much
			for (int i = 0; i < channelIdx; i++)
			{
				y += m_channelHeights[i] + m_channelBlockHeight;
			}
			return y;
		}

		// Top edge Y of module row r.
		private float rowTopY(int row)
		{
			// Channel 0 sits before row 0; channel r+1 sits between row r-1 and row r ... so
			// row r is preceded by channels 0..r and rows 0..r-1.
			float y = VIEW_PAD_TOP_PX;
			for (int i = 0; i <= row; i++) {
				y += m_channelHeights[i];
			}
			y += row * m_channelBlockHeight;
			return y;
		}

		// Y of the n-th lane inside channel `channelIdx`.  Lanes are spaced evenly within
		// the channel (excluding CHANNEL_PADDING_PX top/bottom margins) so the wire centre
		// line always lands inside the channel UiComponent.
		// Lane = -1 is the "vertical pass-through" sentinel: cable doesn't reserve a lane
		// (zero horizontal width inside the channel) and just runs through at the centre.
		private float channelLaneY(int channelIdx, int lane)
		{
			if (m_channelHeights == null || channelIdx < 0 || channelIdx >= m_channelHeights.Length) {
				return VIEW_PAD_TOP_PX;
			}
			float chTop    = channelTopY(channelIdx);
			float chHeight = m_channelHeights[channelIdx];
			if (lane < 0) {
				return chTop + chHeight * 0.5f;
			}
			int   count    = m_channelLaneCounts[channelIdx];
			if (count <= 1) {
				return chTop + chHeight * 0.5f;
			}
			float usable = Math.Max(0f, chHeight - 2f * CHANNEL_PADDING_PX);
			float step   = Math.Min(CHANNEL_LANE_PX, usable / Math.Max(1, count - 1));
			float startY = chTop + chHeight * 0.5f - step * (count - 1) * 0.5f;
			return startY + lane * step;
		}

		// --- Layout constants -------------------------------------------------------------
		// Single source of truth for the row gap.  The Column's gap is set to 0 (we use
		// dedicated channel rows between modules instead), and channel heights are derived
		// from cable count + min/lane spacing — never hand-tuned in two places.
		private const float LINE_THICKNESS_PX = 4f;
		private const float MIN_CHANNEL_PX = 5f;       // empty channel still has at least this much room
		private const float CHANNEL_LANE_PX = 4f;      // vertical distance between two cables in the same channel
		private const float CHANNEL_PADDING_PX = 3f;   // padding inside a channel above/below the lane stack
		// Vertical fudge so the cables visually sit on the port circles.  With the
		// top-vs-side padding split, rowTopY anchors directly to the small top padding
		// and the math meets the port centres without an offset (was -10 when the top
		// padding was 56 — kept the constant in case future visual tweaks need it).
		private const float CABLE_Y_OFFSET_PX = 0f;
		// Outer padding around the ControllerView.  Sides need room for the side
		// corridors (cables wrapping outside the rows); top/bottom only need a small
		// breath because cables don't extend beyond the channel rows themselves.
		private const float VIEW_PAD_PX     = 56f; // side padding (left/right — for side corridors)
		private const float VIEW_PAD_TOP_PX = 4f;  // top padding — small, just visual breathing room
		private static readonly Px VIEW_PAD     = VIEW_PAD_PX.px();
		private static readonly Px VIEW_PAD_TOP = VIEW_PAD_TOP_PX.px();
		// Side corridor configuration.
		private const float CHANNEL_BASE_PX = 8f;
		private const float SIDE_LANE_PX = 4f;
		private const float MAX_LANE_OFFSET_PX = VIEW_PAD_PX - 6f;

		// --- Wire colours -----------------------------------------------------------------
		private const byte WIRE_ALPHA_ACTIVE = 200; // when hovered or m_showsLinks is on
		private const byte WIRE_ALPHA_IDLE = 110;   // black/idle baseline
		private const byte WIRE_BORDER_ALPHA = 60;  // border is more transparent than the fill

		// --- Per-redraw channel state -----------------------------------------------------
		// Filled by RedrawComponents.  m_channelHeights[i] is the height of channel i
		// (i = 0 is the top channel above row 0; i = r+1 is the gap above row r for r >= 0).
		// m_channelLaneCounts[i] mirrors how many lanes are reserved in channel i.  Cables
		// derive their Y from these values + cumulative row heights so the cable math and
		// the laid-out channel components can never drift apart.
		private float[] m_channelHeights;
		private int[]   m_channelLaneCounts;
		private float   m_channelBlockHeight;   // == 4 * BLOCK_SIZE (constant module row height)

		// Per-cable routing decision computed up-front in RedrawComponents.  Capturing it
		// in one place lets us sort cables within each channel before assigning lanes, so
		// the visible order minimises crossings.
		private sealed class CableSpec
		{
			public Module Src;
			public string OutputId;
			public Module Dst;
			public string InputId;
			public int    SrcChannelIdx;
			public int    DstChannelIdx;
			public int    SrcChannelLane;
			public int    DstChannelLane;
			public bool   WrapLeft;
			public int    SideLane;
			public int    ColorIndex;
			public float  SrcXp;
			public float  DstXp;
		}
		private readonly List<CableSpec> m_cableSpecs = new List<CableSpec>();

		// Stable per-(source,output) colour pool.  Allocates an index on first lookup
		// and returns the same colour forever for that pair, even if the cable is
		// removed and re-added later.  Used by pin dots and cable rendering so all
		// three views of a connection (output dot, cable, input dot) share one hue.
		public ColorRgba GetOrCreateCableColor(long sourceModuleId, string outputId)
		{
			int idx = ColourPaletteIndex(sourceModuleId, outputId ?? "");
			return cableColorFromIndex(idx);
		}

		// Converts the Unity Color stored in the palette to an opaque ColorRgba.  The
		// palette colours carry the half-transparent line alpha (used for cable fills);
		// for the port dot we want a full-saturation fill so the user can recognise
		// the matching hue.
		private static ColorRgba cableColorFromIndex(int idx)
		{
			if (idx < 0 || idx >= m_colors.Count) {
				return ColorRgba.Black;
			}
			Color c = m_colors[idx];
			return new ColorRgba(c.r, c.g, c.b, 1f);
		}

		private static float ComputeChannelHeight(int laneCount)
		{
			// Every channel keeps at least MIN_CHANNEL_PX of breathing room — even empty
			// ones — so adjacent module rows never touch.
			if (laneCount <= 0) {
				return MIN_CHANNEL_PX;
			}
			float needed = laneCount * CHANNEL_LANE_PX + 2f * CHANNEL_PADDING_PX;
			return Math.Max(MIN_CHANNEL_PX, needed);
		}

		private int ColourPaletteIndex(long sourceModuleId, string outputId)
		{
			string key = $"{sourceModuleId}.{outputId}";
			if (m_colorCombinations.Count == 0)
			{
				m_colorCombinations[key] = 0;
				return 0;
			}
			if (m_colorCombinations.TryGetValue(key, out int idx)) {
				return idx;
			}
			idx = (m_colorCombinations.Values.Max() + 1) % m_colors.Count;
			m_colorCombinations[key] = idx;
			return idx;
		}

		// For each cable end touching a channel, work out which side of the channel it
		// connects from (TOP = row above the channel, BOTTOM = row below) and the X span
		// it occupies.  Sort cables in the channel by (side, XMin) so TOP-touching ends
		// fill the upper lanes and BOTTOM-touching ends fill the lower lanes, then PACK
		// non-overlapping spans onto the same lane to minimise the total lane count
		// (channel height grows only when cables actually conflict).
		//
		// TODO: split each row channel into two sub-channels — one reserved for
		// source-side (output→corridor) horizontals, one for destination-side
		// (corridor→input) horizontals.  Today they share one channel and can
		// collide when their X spans overlap; with sub-channels, an output-end
		// and an input-end from the same channel could never visually meet, and
		// channel height for typical rows would drop to one lane each.
		private struct ChannelEntry
		{
			public CableSpec Cable;
			public bool IsSrc;
			public bool IsBoth;
			public int  Side;   // 0 = top, 1 = both/middle, 2 = bottom
			public float XMin;
			public float XMax;
		}

		private void AssignChannelLanes(int channelCount)
		{
			float layoutW = (Entity?.Prototype?.Columns ?? 0) * (float)Sizes.BLOCK_SIZE.Pixels;
			float pad     = VIEW_PAD_PX;
			float leftEdge  = pad - MAX_LANE_OFFSET_PX;          // worst-case left  side-corridor X
			float rightEdge = pad + layoutW + MAX_LANE_OFFSET_PX; // worst-case right side-corridor X

			var buckets = new List<ChannelEntry>[channelCount];
			for (int i = 0; i < channelCount; i++) {
				buckets[i] = new List<ChannelEntry>();
			}

			foreach (var c in m_cableSpecs)
			{
				if (c.SrcChannelIdx == c.DstChannelIdx)
				{
					// Adjacent-row cable — single channel, hBot spans srcXp..dstXp directly.
					float xMin = Math.Min(c.SrcXp, c.DstXp);
					float xMax = Math.Max(c.SrcXp, c.DstXp);
					addEntry(c.SrcChannelIdx, new ChannelEntry {
						Cable = c, IsSrc = true, IsBoth = true, Side = 1, XMin = xMin, XMax = xMax
					});
				}
				else
				{
					// Source side — horizontal stretches from srcXp to the side corridor.
					addSpanningEntry(c, isSrc: true,  c.SrcChannelIdx, c.SrcXp);
					// Destination side — same shape on the dst side.
					addSpanningEntry(c, isSrc: false, c.DstChannelIdx, c.DstXp);
				}
			}

			void addEntry(int channelIdx, ChannelEntry e)
			{
				if (channelIdx >= 0 && channelIdx < buckets.Length) {
					buckets[channelIdx].Add(e);
				}
			}

			void addSpanningEntry(CableSpec cable, bool isSrc, int channelIdx, float xEndpoint)
			{
				int row  = isSrc ? cable.Src.Row : cable.Dst.Row;
				int side = (row == channelIdx - 1) ? 0 : (row == channelIdx ? 2 : 1);
				float xSide = cable.WrapLeft ? leftEdge : rightEdge;
				addEntry(channelIdx, new ChannelEntry {
					Cable = cable, IsSrc = isSrc, IsBoth = false, Side = side,
					XMin = Math.Min(xEndpoint, xSide),
					XMax = Math.Max(xEndpoint, xSide),
				});
			}

			for (int ch = 0; ch < channelCount; ch++)
			{
				var list = buckets[ch];
				// Sort: top side first, then both, then bottom; within each side by XMin.
				list.Sort((a, b) =>
				{
					int s = a.Side.CompareTo(b.Side);
					return s != 0 ? s : a.XMin.CompareTo(b.XMin);
				});

				// Greedy interval packing — single global lane stack; the
				// (Side, XMin) sort order means top-side cables get tried first
				// and tend to occupy the lower-indexed (visually upper) lanes,
				// but a bottom-side cable can REUSE one of those lanes if its
				// X span doesn't overlap with what's already there.  Without
				// this lane reuse, every top-side end forces its own lane even
				// when a bottom-side end at a non-overlapping X could share it.
				// Zero-width cables (src and dst at the same X — e.g., adjacent-row direct
				// drops in the same column) don't claim a lane at all; their vertical run
				// just passes through the channel and its placement Y can be the centre.
				var laneEnds = new List<float>();
				const float ZERO_WIDTH_EPS = 0.5f;

				for (int idx = 0; idx < list.Count; idx++)
				{
					var e = list[idx];
					if (e.XMax - e.XMin < ZERO_WIDTH_EPS)
					{
						// Pure vertical cable — sentinel lane = -1 means "use channel centre".
						if (e.IsBoth)
						{
							e.Cable.SrcChannelLane = -1;
							e.Cable.DstChannelLane = -1;
						}
						else if (e.IsSrc) {
							e.Cable.SrcChannelLane = -1;
						} else {
							e.Cable.DstChannelLane = -1;
						}
						continue;
					}

					int lane = -1;
					for (int i = 0; i < laneEnds.Count; i++)
					{
						if (laneEnds[i] < e.XMin) { lane = i; break; }
					}
					if (lane < 0)
					{
						lane = laneEnds.Count;
						laneEnds.Add(e.XMax);
					}
					else
					{
						laneEnds[lane] = e.XMax;
					}
					if (e.IsBoth)
					{
						e.Cable.SrcChannelLane = lane;
						e.Cable.DstChannelLane = lane;
					}
					else if (e.IsSrc) {
						e.Cable.SrcChannelLane = lane;
					} else {
						e.Cable.DstChannelLane = lane;
					}
				}
				m_channelLaneCounts[ch] = laneEnds.Count;
			}
		}

		// Side corridors carry the long vSide segment of any 5-segment cable.  Sort cables
		// on the same side by their channel-range start, then greedy-pack non-overlapping
		// ranges onto the same lane — same idea as AssignChannelLanes uses for X spans,
		// applied to vertical channel-index spans.  Two cables on the same wrap side can
		// share a side lane iff their [min..max] channel ranges are disjoint, since their
		// vSide segments occupy disjoint Y intervals on that single corridor X.
		private void AssignSideLanes()
		{
			var left  = m_cableSpecs.Where(c => c.WrapLeft  && c.SrcChannelIdx != c.DstChannelIdx).ToList();
			var right = m_cableSpecs.Where(c => !c.WrapLeft && c.SrcChannelIdx != c.DstChannelIdx).ToList();
			Comparison<CableSpec> bySpan = (a, b) =>
			{
				int aMin = Math.Min(a.SrcChannelIdx, a.DstChannelIdx);
				int bMin = Math.Min(b.SrcChannelIdx, b.DstChannelIdx);
				int c1 = aMin.CompareTo(bMin);
				if (c1 != 0) {
					return c1;
				}
				int aMax = Math.Max(a.SrcChannelIdx, a.DstChannelIdx);
				int bMax = Math.Max(b.SrcChannelIdx, b.DstChannelIdx);
				return aMax.CompareTo(bMax);
			};
			left.Sort(bySpan);
			right.Sort(bySpan);
			packSideLanes(left);
			packSideLanes(right);
		}

		// Greedy interval packing for one side corridor.  Each lane tracks the highest
		// channel idx already claimed; a new cable reuses the first lane whose end is
		// strictly below the cable's start (so adjacent ranges still get separate lanes
		// — channel rows have height, and two cables that touch the same channel could
		// otherwise visually merge).
		private static void packSideLanes(List<CableSpec> cables)
		{
			var laneEnds = new List<int>();
			foreach (var c in cables)
			{
				int min = Math.Min(c.SrcChannelIdx, c.DstChannelIdx);
				int max = Math.Max(c.SrcChannelIdx, c.DstChannelIdx);
				int chosen = -1;
				for (int i = 0; i < laneEnds.Count; i++)
				{
					if (laneEnds[i] < min) { chosen = i; break; }
				}
				if (chosen < 0)
				{
					chosen = laneEnds.Count;
					laneEnds.Add(max);
				}
				else
				{
					laneEnds[chosen] = max;
				}
				c.SideLane = chosen;
			}
		}

		// Live UiComponents that draw connection paths. Cleared and rebuilt each RepaintLines.
		private readonly List<UiComponent> m_lineSegments = new List<UiComponent>();

		private void RepaintLines()
		{
			try
			{
				// Tear down any segments from the previous repaint.
				foreach (var s in m_lineSegments)
				{
					if (s != null && s.IsAttached) {
						s.RemoveFromHierarchy();
					}
				}
				m_lineSegments.Clear();

				// All routing decisions (channel/lane indices, wrap side, colour) were
				// computed and sorted in RedrawComponents.  Just hand them to the path builder.
				foreach (var c in m_cableSpecs)
				{
					Color uColor = m_colors[c.ColorIndex];
					ColorRgba activeColor = new ColorRgba(uColor.r, uColor.g, uColor.b, 1f).SetA(WIRE_ALPHA_ACTIVE);
					ColorRgba idleColor   = ColorRgba.Black.SetA(WIRE_ALPHA_IDLE);

					CreateConnectionPath(c.Src, c.OutputId, c.Dst, c.InputId,
						activeColor, idleColor, c.WrapLeft, c.SideLane,
						c.SrcChannelIdx, c.SrcChannelLane, c.DstChannelIdx, c.DstChannelLane);
				}
			}
			catch (Exception e)
			{
				Log.Error("[RepaintLines]");
				Log.Exception(e);
			}
		}

		// Decide left/right side corridor for every wrapping cable.  Goal: balance
		// load between corridors so cables don't all pile up on one side just
		// because the modules in use cluster geometrically — but the geometric
		// signal stays primary, so cables genuinely close to a corridor keep
		// their short detour.
		//
		// Each side's cost = (horizontal detour the cable would actually walk on
		// that side) + (cables already routed there) × LOAD_BIAS.  The detour
		// term is the SUM of both pin distances to the chosen corridor, which
		// matches the real wire length (hBot + hTop) — using the sum instead of
		// just the midpoint means a cable with one pin near the right edge pays
		// the FULL cost of dragging the other pin all the way over, so the
		// geometric preference scales with how far apart the pins are.
		//
		// LOAD_BIAS is tuned to ~2 × BLOCK_SIZE: an imbalance of N cables on a
		// side is worth N × 2 blocks of extra detour.  That alternates cables
		// near the layout centre but doesn't push edge-anchored cables across
		// the whole layout to the opposite corridor.
		private void AssignWrapSides()
		{
			if (Entity?.Prototype == null) {
				return;
			}
			float bs       = (float)Sizes.BLOCK_SIZE.Pixels;
			float pad      = VIEW_PAD_PX;
			float layoutPx = Entity.Prototype.Columns * bs;
			float leftEdge  = pad;
			float rightEdge = pad + layoutPx;
			float loadBias  = bs * 2f;

			int leftCount  = 0;
			int rightCount = 0;
			foreach (var cable in m_cableSpecs)
			{
				if (cable.SrcChannelIdx == cable.DstChannelIdx) {
					// Adjacent-row direct route — no corridor used; WrapLeft is
					// inert for routing but still feeds AssignChannelLanes' bookkeeping.
					cable.WrapLeft = (cable.SrcXp + cable.DstXp) * 0.5f < (leftEdge + rightEdge) * 0.5f;
					continue;
				}
				// Detour walked on each side = sum of both pin distances to that
				// corridor (= length of the two horizontal segments combined).
				// Bigger when the cable's farther pin is far from the corridor.
				float leftDetour  = (cable.SrcXp - leftEdge)  + (cable.DstXp - leftEdge);
				float rightDetour = (rightEdge - cable.SrcXp) + (rightEdge - cable.DstXp);
				float leftScore  = leftDetour  + leftCount  * loadBias;
				float rightScore = rightDetour + rightCount * loadBias;
				if (leftScore <= rightScore) {
					cable.WrapLeft = true;
					leftCount++;
				} else {
					cable.WrapLeft = false;
					rightCount++;
				}
			}
		}

		// Returns a darker shade of the given colour by scaling each RGB channel by `factor`,
		// with an explicit alpha so the border can be more transparent than the fill.
		private static ColorRgba darken(ColorRgba src, float factor, byte alpha)
		{
			byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(src.R * factor), 0, 255);
			byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(src.G * factor), 0, 255);
			byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(src.B * factor), 0, 255);
			return new ColorRgba(r, g, b, alpha);
		}

		// Lays down a 5-segment routing path of absolute-positioned UiComponents:
		//   src.bottom → DOWN clearance → side corridor → UP → top corridor → DOWN to dst.top
		// The path always wraps around the OUTSIDE of the module grid (top corridor + one side
		// corridor) so it never cuts through other modules.  Each segment observes module
		// positions for live updates, and observes hover/showsLinks to flip its colour between
		// the per-output hue and a flat black baseline (always visible, never hidden).
		private void CreateConnectionPath(Module src, string outputId, Module dst, string inputId,
			ColorRgba activeColor, ColorRgba idleColor, bool wrapLeft, int sideLane,
			int srcChannelIdx, int srcChannelLane, int dstChannelIdx, int dstChannelLane)
		{
			// Border tones — same hue as the fill but darker so it reads as an outline rather
			// than a flat black frame.  Alpha is kept lower than the fill so the wire stays
			// soft against the busy module grid.
			ColorRgba activeBorder = darken(activeColor, factor: 0.45f, alpha: WIRE_BORDER_ALPHA);
			ColorRgba idleBorder   = darken(idleColor,   factor: 0.45f, alpha: WIRE_BORDER_ALPHA);

			UiComponent vSrc  = makeSegment(horizontal: false, idleColor, idleBorder);
			UiComponent hBot  = makeSegment(horizontal: true,  idleColor, idleBorder);
			UiComponent vSide = makeSegment(horizontal: false, idleColor, idleBorder);
			UiComponent hTop  = makeSegment(horizontal: true,  idleColor, idleBorder);
			UiComponent vDst  = makeSegment(horizontal: false, idleColor, idleBorder);
			var segs = new (UiComponent seg, bool horizontal)[] {
				(vSrc, false), (hBot, true), (vSide, false), (hTop, true), (vDst, false)
			};

			void update()
			{
				if (src.Prototype == null || dst.Prototype == null || Entity?.Prototype == null) {
					return;
				}
				if (m_channelHeights == null || m_channelHeights.Length == 0) {
					return;
				}

				ModuleConnectorProto srcOut = src.GetOutputProto(outputId);
				ModuleConnectorProto dstIn  = dst.GetInputProto(inputId);
				if (srcOut == null || dstIn == null) {
					return;
				}

				int srcOutCol = src.GetPinColumn(outputId, isOutput: true);
				int dstInCol  = dst.GetPinColumn(inputId,  isOutput: false);

				float bs       = (float)Sizes.BLOCK_SIZE.Pixels;
				int totalCols  = Entity.Prototype.Columns;
				float layoutW  = totalCols * bs;
				float pad      = VIEW_PAD_PX;
				float yFix     = CABLE_Y_OFFSET_PX;

				// Port endpoints — Y is read straight off the layout's cumulative offsets so
				// the cable always lands on the actual rendered port circle.
				float srcXp = pad + (srcOutCol + 0.5f) * bs;
				float srcYp = rowTopY(src.Row) + 3.5f * bs + yFix;
				float dstXp = pad + (dstInCol + 0.5f) * bs;
				float dstYp = rowTopY(dst.Row) + 0.5f * bs + yFix;

				// Side corridor stacking: capped so it doesn't escape the padding zone.
				float rawSide = CHANNEL_BASE_PX + sideLane * SIDE_LANE_PX;
				float sideOffset = Math.Min(rawSide, MAX_LANE_OFFSET_PX);
				float sideCorridorX = wrapLeft ? pad - sideOffset : pad + layoutW + sideOffset;

				// In-channel Y for source and destination ends.  Uses the precomputed lane
				// counts and channel heights from RedrawComponents so the cable centre line
				// always lands inside the matching channel UiComponent.
				float srcChannelY = channelLaneY(srcChannelIdx, srcChannelLane);
				float dstChannelY = channelLaneY(dstChannelIdx, dstChannelLane);

				float t = LINE_THICKNESS_PX;
				float halfT = t * 0.5f;

				if (srcChannelIdx == dstChannelIdx && srcChannelIdx != 0)
				{
					// Both endpoints share a single inter-row channel — direct 3-segment route.
					float gapY = srcChannelY;
					placeRect(vSrc, srcXp - halfT, Math.Min(srcYp, gapY), t, Math.Abs(gapY - srcYp));
					placeRect(hBot, Math.Min(srcXp, dstXp) - halfT, gapY - halfT, Math.Abs(srcXp - dstXp) + t, t);
					placeRect(vSide, 0f, 0f, 0f, 0f);
					placeRect(hTop,  0f, 0f, 0f, 0f);
					placeRect(vDst, dstXp - halfT, Math.Min(gapY, dstYp), t, Math.Abs(dstYp - gapY));
				}
				else
				{
					// Two distinct channels OR top channel: 5-segment wrap via side corridor.
					placeRect(vSrc, srcXp - halfT, Math.Min(srcYp, srcChannelY), t, Math.Abs(srcChannelY - srcYp));
					float hBotLeft  = Math.Min(srcXp, sideCorridorX) - halfT;
					float hBotWidth = Math.Abs(sideCorridorX - srcXp) + t;
					placeRect(hBot, hBotLeft, srcChannelY - halfT, hBotWidth, t);
					placeRect(vSide, sideCorridorX - halfT, Math.Min(srcChannelY, dstChannelY), t, Math.Abs(dstChannelY - srcChannelY));
					float hTopLeft  = Math.Min(sideCorridorX, dstXp) - halfT;
					float hTopWidth = Math.Abs(dstXp - sideCorridorX) + t;
					placeRect(hTop, hTopLeft, dstChannelY - halfT, hTopWidth, t);
					placeRect(vDst, dstXp - halfT, Math.Min(dstChannelY, dstYp), t, Math.Abs(dstYp - dstChannelY));
				}
			}

			// Position observer: re-running update() any time either endpoint moves.
			vSrc.Observe(() => src.Row)
				.Observe(() => src.Column)
				.Observe(() => dst.Row)
				.Observe(() => dst.Column)
				.Do((sr, sc, dr, dc) => update());

			// Colour observer (per segment): activates only for the cable whose
			// specific pin is hovered, OR for any cable touching the module body
			// the user is currently over.  Hovering a single pin highlights only
			// that pin's wire (not all of the module's cables) so the user can
			// trace one connection at a time.  The global m_showsLinks toggle
			// still forces every wire to active.  Always visible — visibility
			// itself doesn't toggle anymore.
			foreach (var pair in segs)
			{
				var localSeg = pair.seg;
				var localHoriz = pair.horizontal;
				localSeg.Observe(() =>
				{
					if (m_controller.m_showsLinks) {
						return true;
					}
					// Pin-specific match: the hovered pin must equal this cable's
					// exact (ModuleId, PortId) endpoint, not just the same module.
					var hlOut = m_controller.m_higlightedOutput;
					if (hlOut != null) {
						return hlOut.ModuleId == src.Id && hlOut.OutputId == outputId;
					}
					var hlIn = m_controller.m_higlightedInput;
					if (hlIn != null) {
						// ModuleConnector reuses OutputId for both kinds — for
						// inputs it carries the input's id.
						return hlIn.ModuleId == dst.Id && hlIn.OutputId == inputId;
					}
					// Module-level fallback: cable hover by hovering any non-pin
					// part of the source or destination module body.
					var hovMod = m_controller.HoveredModuleGraphic;
					if (hovMod != null) {
						return hovMod.Id == src.Id || hovMod.Id == dst.Id;
					}
					return false;
				})
				.Do(active =>
				{
					localSeg.Background(active ? activeColor : idleColor);
					applyBorder(localSeg, localHoriz, active ? activeBorder : idleBorder);
				});
			}

			update(); // initial layout

			UiComponent makeSegment(bool horizontal, ColorRgba bg, ColorRgba bd)
			{
				var seg = new UiComponent()
					.Background(bg)
					.IgnoreInputPicking();
				applyBorder(seg, horizontal, bd);
				Add(seg);
				seg.BringToFront();
				m_lineSegments.Add(seg);
				return seg;
			}

			// Borders only on the long edges so two perpendicular segments meeting at
			// a corner don't stack their borders (which used to draw a "+" cross).
			void applyBorder(UiComponent c, bool horizontal, ColorRgba color)
			{
				Px b = 1.px();
				Px z = Px.Zero;
				if (horizontal) {
					c.Border(top: b, right: z, bottom: b, left: z, color: color, radius: 0);
				} else {
					c.Border(top: z, right: b, bottom: z, left: b, color: color, radius: 0);
				}
			}

			void placeRect(UiComponent c, float left, float top, float width, float height)
			{
				if (width  < 0) {
					width  = 0;
				}
				if (height < 0) {
					height = 0;
				}
				c.AbsolutePosition(top: top.px(), null, null, left: left.px());
				c.Size(width.px(), height.px());
			}
		}

		private PickNewModule m_pickNewModule;
		private PickNewModule m_pickTemplateModule;
		private int m_targetRow;
		private int m_targetColumn;
		private Dict<string, int> m_colorCombinations = [];

		private void AddFreeSlot(Row rowElement, int targetRow, int targetColumn)
		{
			Column column = rowElement.AddAndReturn(new Column())
				.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 4);

			// top filler
			column.AddAndReturn(new UiComponent())
				  .Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);

			AddHelper addHelperUI = new AddHelper(() => this);
			ModuleSlotButton button = column.AddAndReturn(new ModuleSlotButton(this, targetRow, targetColumn));
			button.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 2);
			// Hint floater (the "+ click to add" helper).  Gated on the inspector's
			// m_showHints checkbox in the modules panel header so power users can
			// silence every slot's hover hint at once without losing the click flow.
			button.Floater(() => m_controller.m_showHints ? addHelperUI.Display() : Option<UiComponent>.None);

			// bottom filler
			column.AddAndReturn(new UiComponent())
				  .Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
		}

		// --- Public slot helpers used by ModuleSlotButton ----------------------------------

		// True iff the picked module (or any module of its width) would fit in (row, col).
		public bool IsValidDropAt(int row, int col, Module picked)
		{
			if (picked == null) {
				return false;
			}
			int width = picked.Layout.GetWidth(picked);
			return IsRangeFree(row, col, width, ignore: picked);
		}

		// Drops the inspector's picked-up module at (row, col).  Returns false if no module
		// is picked up or the slot doesn't fit; on success clears PickedUpModule.
		// TODO this should be a command
		public bool TryDropPickedAt(int row, int col)
		{
			var inspector = m_controller;
			var picked = inspector.PickedUpModule;
			if (picked == null) {
				return false;
			}
			if (!TryMoveTo(picked, row, col)) {
				return false;
			}
			inspector.PickedUpModule = null;
			return true;
		}

		// Stamps a copy of the most-recently created/picked module at (row, col), preserving
		// its number/field/string data so shift-paste reproduces the original's settings.
		// TODO this should be a command
		public bool TryShiftAddAt(int row, int col)
		{
			if (m_lastCreated == null) {
				return false;
			}
			// Capture the source BEFORE TryPlaceAt — that call constructs a new
			// module and reassigns m_lastCreated to it, so without this local the
			// data-copy loop below would iterate the freshly-created (empty)
			// destination over itself and lose the original settings entirely.
			Module source = m_lastCreated;
			if (!TryPlaceAt(source.Prototype, row, col)) {
				return false;
			}
			Module placed = m_lastCreated;
			if (placed == source) {
				// Defensive: TryPlaceAt should have replaced m_lastCreated with a
				// fresh module; if it didn't, refuse to do an in-place self-write.
				return false;
			}

			placed.Prototype.ExecuteInit(placed, log: false);
			foreach (KeyValuePair<string, int> item in source.NumberData) {
				placed.NumberData[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, Fix32> item in source.FieldNumberData) {
				placed.FieldNumberData[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, string> item in source.StringData) {
				placed.StringData[item.Key] = item.Value;
			}
			// Pin extension counts roundtrip too so a copy of an extended module
			// keeps its width.  ArrayData (per-module Fix32 buffer) is replaced
			// wholesale rather than per-element so a Delay/etc. module's ring
			// buffer comes across intact.
			placed.SetInputExtensionCount(source.InputExtensionCount);
			placed.SetOutputExtensionCount(source.OutputExtensionCount);
			if (source.ArrayData != null && source.ArrayData.Length > 0)
			{
				Fix32[] copy = new Fix32[source.ArrayData.Length];
				System.Array.Copy(source.ArrayData, copy, copy.Length);
				typeof(Module).GetProperty(nameof(Module.ArrayData)).SetValue(placed, copy);
			}
			placed.Prototype.DisplayUpdate(placed);
			return true;
		}

		public void OpenAddPickerAt(int row, int col, UiComponent anchor)
		{
			if (m_pickNewModuleInAction) {
				return;
			}
			m_pickNewModuleInAction = true;
			// Cache the picker — building the full module list is expensive
			// (one PanelWithHeader + ModuleView per ModuleProto).  Per-row
			// template chips refresh themselves on every show via OnShow in
			// NewModule, so saved blueprints and reloaded Python templates
			// surface on the next open without reconstructing the whole
			// picker.
			m_pickNewModule ??= new PickNewModule(NewTr.Inspector.PickModule, NewModules());
			m_pickNewModuleInAction = false;
			m_targetRow = row;
			m_targetColumn = col;
			m_pickNewModule.Open(anchor);
		}

		// Mirror of OpenAddPickerAt for the placed-module settings dialog: orchestrates
		// the open in a single place so the click handler in ModuleView stays a thin
		// dispatcher.  Re-entry is guarded with m_settingsDialogInAction so a second
		// LMB on the same button while the dialog is mid-open doesn't double-fire.
		// Unlike the picker, we don't cache the dialog instance — its UI is bound to
		// a specific Module's fields, so a fresh dialog is built per open.
		public void OpenSettingsAt(Module module, ButtonText anchor)
		{
			new ModuleEditDialog(module, this, m_controller.Context, m_controller, m_controller.Entity.Resolver)
				.Open(anchor);
		}

		public void OpenTemplatePickerAt(int row, int col, UiComponent anchor)
		{
			if (m_pickTemplateModuleInAction) {
				return;
			}
			m_pickTemplateModuleInAction = true;
			// Rebuild every open so player-saved blueprints added since the last open
			// show up.  The Python-defined templates are stable across game runs but
			// the BlueprintsLibrary scanner needs a fresh sweep each time.
			m_pickTemplateModule = new PickNewModule(NewTr.Inspector.PickTemplate, NewTemplates());
			m_pickTemplateModuleInAction = false;
			m_targetRow = row;
			m_targetColumn = col;
			m_pickTemplateModule.Open(anchor);
		}

		private IEnumerable<AModuleProtoSelector> NewTemplates()
		{
			foreach (KeyValuePair<string, Template> item in TemplateRegistrator.GetTemplates()) {
				yield return new TemplateModule(this, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
				{
					if (TryPlaceAt(moduleProto, m_targetRow, m_targetColumn))
					{
						return (true, m_lastCreated);
					}
					else
					{
						return (false, null);
					}
				}, item);
			}

			// Also yield every player-saved module blueprint from the base game's
			// BlueprintsLibrary (entries with the [PN-Module]- title prefix).  This
			// is the runtime side of the "save as blueprint" feature — the picker
			// shows them alongside Python-defined templates so the user gets one
			// unified list of reusable configs.  DI lookup + ProtosDb deref are
			// done in a helper method (no yield in there) so we can use try/catch.
			foreach (var sel in EnumerateBlueprintSelectors())
			{
				yield return sel;
			}
		}

		private IEnumerable<AModuleProtoSelector> EnumerateBlueprintSelectors()
		{
			// Picker can be opened on a brand-new controller before its Resolver is
			// populated — guard explicitly instead of letting the resolve below NRE
			// into the catch.  Returning empty silently is the right degraded-state
			// behaviour: the regular module list still renders, blueprints just
			// don't show up until the resolver is wired (next selection / load).
			Mafi.DependencyResolver resolver = m_controller?.Entity?.Resolver;
			if (resolver == null)
			{
				return System.Linq.Enumerable.Empty<AModuleProtoSelector>();
			}
			Mafi.Core.Entities.Blueprints.BlueprintsLibrary library = null;
			Mafi.Core.Prototypes.ProtosDb protosDb = null;
			try
			{
				library = resolver.Resolve<Mafi.Core.Entities.Blueprints.BlueprintsLibrary>();
				protosDb = resolver.Resolve<Mafi.Core.Prototypes.ProtosDb>();
			}
			catch (System.Exception e)
			{
				Log.Exception(e);
			}
			if (library == null || protosDb == null)
			{
				return System.Linq.Enumerable.Empty<AModuleProtoSelector>();
			}
			return EnumerateBlueprintSelectorsCore(library, protosDb);
		}

		private IEnumerable<AModuleProtoSelector> EnumerateBlueprintSelectorsCore(
			Mafi.Core.Entities.Blueprints.BlueprintsLibrary library,
			Mafi.Core.Prototypes.ProtosDb protosDb)
		{
			foreach (var bp in ProgramableNetwork.ModuleBlueprints.EnumerateAll(library))
			{
				Mafi.Option<ModuleProto> protoOpt = ProgramableNetwork.ModuleBlueprints.ResolveStoredProto(bp, protosDb);
				if (!protoOpt.HasValue)
				{
					Log.Warning($"[ModuleBlueprints] Skipping '{bp.Name}' — module proto not found in current ProtosDb");
					continue;
				}
				yield return new BlueprintModuleSelector(this, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
				{
					if (TryPlaceAt(moduleProto, m_targetRow, m_targetColumn))
					{
						return (true, m_lastCreated);
					}
					return (false, null);
				}, bp, protoOpt.Value);
			}
		}

		private IEnumerable<AModuleProtoSelector> NewModules()
		{
			Controller controller = m_controller.Entity;
			StaticEntityProto.ID id = controller.Prototype.Id;

			foreach (ModuleProto item in controller.Context.ProtosDb
											.All<ModuleProto>()
											//.Where(p => p.IsAvailable)
											/*.Where(p => p.AllowedDevices.Any(e => e.Equals(id)))*/) {
				yield return new NewModule(this, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
				{
					if (TryPlaceAt(moduleProto, m_targetRow, m_targetColumn))
					{
						return (true, m_lastCreated);
					}
					else
					{
						return (false, null);
					}
				}, item);
			}
		}

		public bool TryPlaceAt(ModuleProto moduleProto, int targetRow, int targetColumn)
		{
			ModuleIdManager moduleIdManager = Entity.Resolver.Resolve<ModuleIdManager>();
			var module = new Module(moduleProto, Entity.Context, Entity, moduleIdManager.Allocate());
			var width = module.Layout.GetWidth(module);

			if (!IsRangeFree(targetRow, targetColumn, width, ignore: null))
			{
				moduleIdManager.Free(module.Id);
				m_controller.Context.AudioDb.InvalidOp(true).Play();
				return false;
			}

			module.Row = targetRow;
			module.Column = targetColumn;
			Entity.Modules.Add(module);
			m_lastCreated = module;
			return true;
		}

		// True iff every cell in [col, col+width) on the given row is unoccupied.
		// `ignore` lets a module be excluded from the check (used for moves).
		private bool IsRangeFree(int row, int col, int width, Module ignore)
		{
			if (Entity?.Prototype == null) {
				return false;
			}
			if (row < 0 || row >= Entity.Prototype.Rows) {
				return false;
			}
			if (col < 0 || col + width > Entity.Prototype.Columns) {
				return false;
			}

			foreach (var m in Entity.Modules)
			{
				if (m == null || m.Prototype == null) {
					continue;
				}
				if (ignore != null && m.Id == ignore.Id) {
					continue;
				}
				if (m.Row != row) {
					continue;
				}
				int mw = m.Layout.GetWidth(m);
				int mEnd = m.Column + mw;
				int end = col + width;
				if (m.Column < end && col < mEnd) {
					return false;
				}
			}
			return true;
		}

		private class ModuleIdComparator : ICollectionComparator<long, IEnumerable<long>>
		{
			public bool AreSame(IEnumerable<long> collectionC, Lyst<long> lastKnown)
			{
				if ((lastKnown == null && collectionC != null) || (lastKnown != null && collectionC == null))
				{
					return false;
				}

				Lyst<long> collection = collectionC?.ToLyst();
				if (lastKnown?.Count != collection?.Count)
				{
					return false;
				}

				int length = lastKnown.Count;
				for (int i = 0; i < length; i++)
				{
					if (collection[i] != lastKnown[i])
					{
						return false;
					}
				}

				return true;
			}
		}
	}
}
