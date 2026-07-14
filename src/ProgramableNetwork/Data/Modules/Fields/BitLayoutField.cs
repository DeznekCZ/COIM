using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using Mafi.Core.Syncers;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Inspector editor for the <b>Bits: encode / decode</b> modules.  Each pin maps to a
	/// contiguous (offset, length, format) window in a 32-bit word; the three ints per pin
	/// are stored in the module's <see cref="Module.ArrayData"/> (slot 3*i = offset,
	/// 3*i+1 = length, 3*i+2 = format: 0 hex / 1 int) so the per-tick action reads
	/// them without parsing a string.
	///
	/// The pin count itself is the module's normal input/output extension count — added or
	/// removed with the module's right-edge +/- pin buttons — so this editor observes that
	/// count and (re)builds one row per effective pin.  Edits go into a local mirror and
	/// schedule a <see cref="ModuleSetArrayCmd"/> with the full layout (MP-safe, exactly like
	/// the number fields' ModuleSetFix32FieldCmd); the mirror lets the offset/length fields
	/// keep focus while typing and the format button update instantly.  The read-only preview
	/// below is a colored 32-bit grid (two rows of 16): each cell is tinted by its owning pin,
	/// gaps stay dim and overlaps go red, with the owning pin's base-36 digit shown small.
	/// </summary>
	public class BitLayoutField : Column
	{
		private const int MaxBits = 32;

		// Per-pin cell colors for the 32-bit preview grid (mirrors the HTML design mock).
		private static readonly ColorRgba[] PinColors =
		{
			ColorRgba.FromHex("5b9bd5"), ColorRgba.FromHex("3fbf6f"), ColorRgba.FromHex("e0b040"), ColorRgba.FromHex("c06fd0"),
			ColorRgba.FromHex("4fb0c0"), ColorRgba.FromHex("d07f4f"), ColorRgba.FromHex("8f7fd0"), ColorRgba.FromHex("6fbf9f"),
			ColorRgba.FromHex("c05f7f"), ColorRgba.FromHex("7f9f5f"), ColorRgba.FromHex("5fa0d0"), ColorRgba.FromHex("bfa050"),
			ColorRgba.FromHex("a0709f"), ColorRgba.FromHex("60a070"), ColorRgba.FromHex("b06060"), ColorRgba.FromHex("7a86c0"),
		};
		private static readonly ColorRgba FreeColor = ColorRgba.DarkDarkGray.SetA(120);
		private static readonly ColorRgba OverlapColor = ColorRgba.Red;
		// Dark cell text so the pin-index digit reads on the light pin colors.
		private static readonly ColorRgba CellTextColor = ColorRgba.FromHex("101418");

		private readonly ControllerInspector m_inspector;
		private readonly Module m_module;
		private readonly ExtensionSide m_side;

		private readonly Column m_rows;
		private readonly Column m_previewGrid;

		// Editing buffer.  Re-seeded from the module only when the pin count changes;
		// otherwise it is the source of truth so field edits / format toggles are instant.
		private int[] m_offset = System.Array.Empty<int>();
		private int[] m_length = System.Array.Empty<int>();
		private int[] m_format = System.Array.Empty<int>();
		private TextField[] m_offsetFields = System.Array.Empty<TextField>();

		public BitLayoutField(ControllerInspector inspector, Module module, ExtensionSide side)
		{
			m_inspector = inspector;
			m_module = module;
			m_side = side;

			this.Width(420.px());
			this.Add(new Label("Per pin: off, len, format (int/hex). Little-endian, raw 32-bit.".AsLoc()));

			m_rows = this.AddAndReturn(new Column());

			Row tools = this.AddAndReturn(new Row());
			// Manage the pin count straight from the editor (in addition to the module's
			// right-edge buttons): these drive the same input/output extension count, and
			// the pinCount Observe below rebuilds the rows once the command applies.
			tools.AddAndReturn(new ButtonText("- pin".AsLoc()))
				.Height(Sizes.BLOCK_SIZE)
				.FlexGrow(1)
				.OnClick(removePin);
			tools.AddAndReturn(new ButtonText("+ pin".AsLoc()))
				.Height(Sizes.BLOCK_SIZE)
				.FlexGrow(1)
				.OnClick(addPin);
			tools.AddAndReturn(new ButtonText("Auto-pack".AsLoc()))
				.Height(Sizes.BLOCK_SIZE)
				.FlexGrow(1)
				.OnClick(autoPack);

			m_previewGrid = this.AddAndReturn(new Column());

			// Rows follow the pin (extension) count: when the player adds/removes a pin
			// with the module's right-edge buttons, this fires and rebuilds the rows.
			this.Observe(() => pinCount()).Do(_ => rebuild());
			rebuild();
		}

		private int pinCount()
		{
			return m_side == ExtensionSide.Input
				? m_module.EffectiveInputs.Count
				: m_module.EffectiveOutputs.Count;
		}

		private IReadOnlyList<ModuleConnectorProto> pins()
		{
			return m_side == ExtensionSide.Input
				? m_module.EffectiveInputs
				: m_module.EffectiveOutputs;
		}

		// Array slot readers with the (offset = index, length = 1, format = hex) default
		// that reproduces plain little-endian single bits before the layout is configured.
		private int storedOffset(int i)
		{
			Fix32[] data = m_module.ArrayData;
			return (data != null && data.Length > 3 * i) ? data[3 * i].IntegerPart : i;
		}

		private int storedLength(int i)
		{
			Fix32[] data = m_module.ArrayData;
			return (data != null && data.Length > 3 * i + 1) ? data[3 * i + 1].IntegerPart : 1;
		}

		private int storedFormat(int i)
		{
			Fix32[] data = m_module.ArrayData;
			return (data != null && data.Length > 3 * i + 2) ? data[3 * i + 2].IntegerPart : 0;
		}

		private static string fmtLabel(int f)
		{
			return f == 1 ? "int" : "hex";
		}

		private void rebuild()
		{
			int n = pinCount();

			// Only reseed the mirror from the module on a structural change (pins added or
			// removed).  A same-count rebuild (e.g. a format toggle) keeps the live mirror
			// so the just-changed value shows immediately instead of the not-yet-applied
			// command's stale module state.
			if (m_offset.Length != n)
			{
				m_offset = new int[n];
				m_length = new int[n];
				m_format = new int[n];
				for (int i = 0; i < n; i++)
				{
					m_offset[i] = storedOffset(i);
					m_length[i] = storedLength(i);
					m_format[i] = storedFormat(i);
				}
			}
			m_offsetFields = new TextField[n];

			IReadOnlyList<ModuleConnectorProto> pinList = pins();
			m_rows.Clear();
			// Two pins per row once there are more than a handful, so a full 16-pin module
			// stays ~8 rows tall instead of 16.  Small configs keep a single narrow column.
			int cols = n > 4 ? 2 : 1;
			for (int i = 0; i < n; i += cols)
			{
				Row row = m_rows.AddAndReturn(new Row());
				row.Width(Percent.Hundred);
				for (int c = 0; c < cols && i + c < n; c++)
				{
					addPinBlock(row, i + c, pinList);
				}
			}

			updatePreview();
		}

		// One compact "b#  off[ ] len[ ] [fmt]" block, appended into the given row.
		private void addPinBlock(Row row, int i, IReadOnlyList<ModuleConnectorProto> pinList)
		{
			int idx = i; // capture for the change lambdas

			// Color chip in the pin's grid color so a row can be matched to its cells.
			Label chip = row.AddAndReturn(new Label("".AsLoc()));
			chip.Background(PinColors[i % PinColors.Length]);
			chip.Width(12.px());
			chip.Height(14.px());

			// Pin label is just its index number (matches the grid digit and needs no
			// translation) rather than the internal "bN" id string.
			row.AddAndReturn(new Label(i.ToString().AsLoc())).Width(22.px());
			row.AddAndReturn(new Label("off".AsLoc())).Width(16.px());

			TextField offField = row.AddAndReturn(new TextField()).Width(32.px()).Height(Sizes.BLOCK_SIZE);
			offField.Value(new LocStrFormatted(m_offset[i].ToString()));
			offField.OnValueChanged(_ =>
			{
				if (int.TryParse(offField.GetText(), out int v))
				{
					m_offset[idx] = v;
					commit();
				}
			});
			m_offsetFields[i] = offField;

			row.AddAndReturn(new Label("len".AsLoc())).Width(16.px());

			TextField lenField = row.AddAndReturn(new TextField()).Width(32.px()).Height(Sizes.BLOCK_SIZE);
			lenField.Value(new LocStrFormatted(m_length[i].ToString()));
			lenField.OnValueChanged(_ =>
			{
				if (int.TryParse(lenField.GetText(), out int v))
				{
					m_length[idx] = v;
					commit();
				}
			});

			// Format cycles bit -> int -> hex.  Rebuild afterwards so the button relabels;
			// the mirror keeps the new value so the same-count rebuild shows it at once.
			row.AddAndReturn(new ButtonText(fmtLabel(m_format[i]).AsLoc()))
				.Width(52.px())
				.Height(Sizes.BLOCK_SIZE)
				.OnClick(() =>
				{
					m_format[idx] = (m_format[idx] + 1) % 2;
					commit();
					rebuild();
				});

			// Gap before the next column's block.
			row.AddAndReturn(new Label(" ".AsLoc())).Width(6.px());
		}

		private int extCount()
		{
			return m_side == ExtensionSide.Input
				? m_module.InputExtensionCount
				: m_module.OutputExtensionCount;
		}

		private int maxExt()
		{
			return m_side == ExtensionSide.Input
				? m_module.Prototype.MaxInputExtensions
				: m_module.Prototype.MaxOutputExtensions;
		}

		// Add / remove a pin from the tail = bump the module's extension count by one.
		// Pins are positional, so only the last one is removed (mid-list removal would
		// renumber every higher pin and shift its cables).  The executor clamps to
		// [0, max] and prunes cables on the pin that disappears.
		private void addPin()
		{
			int ext = extCount();
			if (ext >= maxExt())
			{
				return;
			}
			m_inspector.Context.InputScheduler.ScheduleInputCmd(
				new ModuleSetExtensionCountCmd(m_module.Controller.Id, m_module.Id, m_side, ext + 1));
		}

		private void removePin()
		{
			int ext = extCount();
			if (ext <= 0)
			{
				return;
			}
			m_inspector.Context.InputScheduler.ScheduleInputCmd(
				new ModuleSetExtensionCountCmd(m_module.Controller.Id, m_module.Id, m_side, ext - 1));
		}

		// Lay the pins out end-to-end (no gaps) keeping their current lengths, then push
		// the new offsets to the fields, the module Array, and the preview.
		private void autoPack()
		{
			int at = 0;
			for (int i = 0; i < m_offset.Length; i++)
			{
				m_offset[i] = at;
				at += Math.Max(1, m_length[i]);
				if (i < m_offsetFields.Length)
				{
					m_offsetFields[i].Value(new LocStrFormatted(m_offset[i].ToString()));
				}
			}
			commit();
		}

		private void commit()
		{
			int n = m_offset.Length;
			Fix32[] values = new Fix32[3 * n];
			for (int i = 0; i < n; i++)
			{
				values[3 * i] = Fix32.FromInt(m_offset[i]);
				values[3 * i + 1] = Fix32.FromInt(m_length[i]);
				values[3 * i + 2] = Fix32.FromInt(m_format[i]);
			}
			m_inspector.Context.InputScheduler.ScheduleInputCmd(
				new ModuleSetArrayCmd(m_module.Controller.Id, m_module.Id, values));
			updatePreview();
		}

		private void updatePreview()
		{
			int n = m_offset.Length;
			int[] owner = new int[MaxBits];
			for (int b = 0; b < MaxBits; b++)
			{
				owner[b] = -1;
			}
			for (int i = 0; i < n; i++)
			{
				int off = m_offset[i];
				int len = m_length[i];
				if (len < 1)
				{
					continue;
				}
				for (int b = off; b < off + len; b++)
				{
					if (b >= 0 && b < MaxBits)
					{
						owner[b] = owner[b] == -1 ? i : -2;
					}
				}
			}

			// Colored 32-bit grid (mirrors the HTML mock): two rows of 16 cells, bits
			// 0-15 on top and 16-31 below, each cell tinted by its owning pin, gaps dim,
			// overlaps red.  The cell shows the owning pin's index (matching its bN name)
			// in dark text so it reads on the light cell colors; overlaps show 'x'.
			m_previewGrid.Clear();
			Row top = m_previewGrid.AddAndReturn(new Row());
			Row bottom = m_previewGrid.AddAndReturn(new Row());
			for (int b = 0; b < MaxBits; b++)
			{
				int o = owner[b];
				ColorRgba color = o == -1 ? FreeColor : o == -2 ? OverlapColor : PinColors[o % PinColors.Length];
				string text = o == -1 ? "" : o == -2 ? "x" : o.ToString();
				Row rowFor = b < 16 ? top : bottom;
				// Call each setter as its own statement on the typed Label so the chain
				// doesn't depend on any particular fluent return type.
				Label cell = rowFor.AddAndReturn(new Label(text.AsLoc()));
				cell.TinyFontSize();
				cell.TextAlign(TextAlignment.CenterMiddle);
				cell.Color(CellTextColor);
				cell.Background(color);
				cell.Width(24.px());
				cell.Height(16.px());
			}
		}
	}
}
