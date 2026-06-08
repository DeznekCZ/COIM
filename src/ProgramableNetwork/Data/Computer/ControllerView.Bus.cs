using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.Ui.Library;
using System;
using System.Collections.Generic;
using Mafi.Core.Syncers;
using Mafi.Unity.UiToolkit;

namespace ProgramableNetwork.Ui
{
    // Left-gutter variable-bus panel.  Rendered into a real sibling Column
    // (busPanel) that sits OUTSIDE the module grid (see RedrawComponents), so it
    // receives clicks and never overlaps the grid or its cable corridors.
    //
    // Vertical alignment is by FLOW, not absolute coordinates: busPanel stacks
    // spacers that mirror the grid's exact top-padding / channel / row heights, so
    // bus N lands in the same vertical band as module-row N — every bus respects
    // its row's vertical position automatically.
    //
    // NOTE (sub-step scope): pins are placeholder-styled (Input look) and inert
    // until 2b wires the bidirectional connect flow + signal-plan bus edges.
    public partial class ControllerView
    {
        private static readonly Px BUS_NAME_W_PX = 16;
        // Total gutter width: name column + one pin cell + a little breathing room.
        private static readonly Px BUS_GUTTER_W_PX = BUS_NAME_W_PX + Sizes.BLOCK_SIZE + 6.px();
		private static readonly Px BUS_HEIGHT = Sizes.BLOCK_SIZE * 4;

        // The bus pin (busId:pinIndex) the cursor is currently over, or null.  The bus
        // cable observers watch this so hovering a bus pin lights its wire(s) — the
        // counterpart of m_higlightedOutput/m_higlightedInput for module pins.
        private string m_hoveredBusPin;

        private static string busPinKey(long busId, int pinIndex)
        {
            return busId + ":" + pinIndex;
        }

        private void BuildBusGutter(Column leftPanel, Column rightPanel)
        {
            if (Entity?.Buses == null || m_channelHeights == null)
            {
                return;
            }
            BuildBusSide(leftPanel, ControllerBus.BusSide.Left);
            BuildBusSide(rightPanel, ControllerBus.BusSide.Right);
        }

        // Renders the buses assigned to one side into its panel, one per module-row
        // band, with a "+" in the first empty band.  Vertical alignment is by FLOW:
        // spacers mirror the grid's top-padding / channel / row heights so bus N on
        // this side sits in module-row N's band.
        private void BuildBusSide(Column panel, ControllerBus.BusSide side)
        {
            int rows = Entity.Prototype.Rows;

            List<ControllerBus> sideBuses = new List<ControllerBus>();
            foreach (ControllerBus bus in Entity.Buses)
            {
                if (bus.Side == side)
                {
                    sideBuses.Add(bus);
                }
            }

            panel.Add(busSpacer(VIEW_PAD_TOP_PX));
            panel.Add(busSpacer(m_channelHeights[0]));

            // Every row band is a visible slot: filled ones show the bus (name + pins),
            // empty ones are a clickable "+" that creates a bus on THIS side — so the
            // slot you add to picks the side.
            for (int r = 0; r < rows; r++)
            {
                if (r < sideBuses.Count)
                {
                    panel.Add(busWidget(sideBuses[r]));
                }
                else
                {
                    panel.Add(emptySlot(side));
                }
                panel.Add(busSpacer(m_channelHeights[r + 1]));
            }
        }

        // One bus: a clickable vertical name (opens BusSettingsDialog) beside a
        // column of 4 pins.  Height matches one module row so it aligns to the grid.
        // A RIGHT-side bus is mirrored — pins face the grid (inner edge), name on the
        // outer edge — so both gutters point their pins toward the modules.
        private UiComponent busWidget(ControllerBus bus)
        {
            Row band = new Row();
            band.Class(Cls.panel);
            band.Height(BUS_HEIGHT);
            band.Width(BUS_GUTTER_W_PX);

            // Name strip styled like an in-game display (glass background) but with a
            // plain MONOSPACE font instead of the 7-segment display font.  The name is
            // clamped to 7 chars and stacked vertically down the strip.
            Row nameBtn = new Row();
            nameBtn.Class(Cls.displayBg);
            nameBtn.Width(BUS_NAME_W_PX);
            nameBtn.Height(BUS_HEIGHT);
            Mafi.Unity.UiToolkit.Library.Label nameLabel =
                new Mafi.Unity.UiToolkit.Library.Label(verticalText(busNameClamped(bus.Name)).AsLoc());
            nameLabel.Class(Cls.fontMonospace, Cls.stateFg);
            nameLabel.Fill();
            nameLabel.TextAlign(Mafi.Unity.UiToolkit.Component.TextAlignment.CenterMiddle);
            nameLabel.FontSize(13);
            nameLabel.IgnoreInputPicking();
            nameBtn.Add(nameLabel);
            nameBtn.Add(new UiComponent().Class(Cls.displayGlass).IgnoreInputPicking());
            nameBtn.OnClick(() => new BusSettingsDialog(this, bus).Open(nameBtn));

            Column pins = new Column();
            for (int p = 0; p < ControllerBus.PinCount; p++)
            {
                int idx = p;
                bool connected = busPinVisual(bus, idx, out ColorRgba dotColor);
                PortPinButton pin = new PortPinButton(PortPinButton.PortKind.Input, connected)
                    .Tooltip(new LocStrFormatted(busPinTooltip(bus, idx)));
                // Paint the dot with the matching cable's hue so a bus pin and the wire
                // running to it read as the same connection (mirrors module pins).
                if (connected)
                {
                    pin.DotColor(dotColor);
                }
                pin.OnClick(() => onBusPinClick(bus, idx));
                pin.OnRightClick(() => onBusPinRightClick(bus, idx));
                // Enlarge this pin while it's the picked source (mirrors module outputs).
                pin.Observe(() => isBusPinPicked(bus, idx)).Do(open => pin.Open(open));
                // Hovering a bus pin highlights the cable(s) wired to it — the bus-side
                // counterpart of a module pin's hover highlight.
                string pinKey = busPinKey(bus.Id, idx);
                pin.OnMouseEnterLeave(
                    () => m_hoveredBusPin = pinKey,
                    () => { if (m_hoveredBusPin == pinKey) { m_hoveredBusPin = null; } });
                // Controller-type pins read from a (potentially different) controller, so
                // they are painted red to distinguish them from local input (green) pins.
                if (bus.PinTypes[idx] == ControllerBus.BusPinType.Controller)
                {
                    pin.Accent(ColorRgba.DarkRed, ColorRgba.Red);
                    pin.OnMouseEnterLeave(
                        () => highlightPinSource(bus, idx, true),
                        () => highlightPinSource(bus, idx, false));
                }
                pins.Add(pin);
            }

            if (bus.Side == ControllerBus.BusSide.Right)
            {
                // Mirrored: pins on the inner (left) edge, name on the outer (right).
                band.Add(pins);
                band.Add(nameBtn);
            }
            else
            {
                band.Add(nameBtn);
                band.Add(pins);
            }
            return band;
        }

        // An always-visible empty slot: a "+" button SIZED like the module grid's add-slot
        // (one cell wide, two blocks tall, vertically centred in the band) and pushed to
        // the gutter's OUTER edge — farther from the grid, where a bus's name strip sits —
        // so empty and filled slots line up.  Clicking it creates a bus on THIS side.
        private UiComponent emptySlot(ControllerBus.BusSide side)
        {
            BusSlotButton addBus = new BusSlotButton(this, side);
            addBus.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 2);

            Row row = new Row();
            row.Width(BUS_GUTTER_W_PX);
            if (side == ControllerBus.BusSide.Left)
            {
                // Left gutter: outer edge is the left, so the button goes first.
                row.Add(addBus);
                row.Add(new UiComponent().FlexGrow(1));
            }
            else
            {
                // Right gutter: outer edge is the right, so the button goes last.
                row.Add(new UiComponent().FlexGrow(1));
                row.Add(addBus);
            }

            Column col = new Column();
            col.Width(BUS_GUTTER_W_PX);
            col.Height(BUS_HEIGHT);
            col.Add(busSpacer(Sizes.BLOCK_SIZE)); // top filler — centres the button
            col.Add(row);
            col.Add(busSpacer(Sizes.BLOCK_SIZE)); // bottom filler
            return col;
        }

        private static UiComponent busSpacer(Px heightPx)
        {
            return new UiComponent().Height(heightPx);
        }

        // Collects every bus connection as a CableSpec onto m_cableSpecs so it shares the
        // module cables' channel- and side-lane allocation (AssignChannelLanes /
        // AssignSideLanes / AssignWrapSides) — a bus connection is just a module-pin cable
        // whose far end terminates in the left/right side corridor at a bus pin.  The
        // module side keeps the normal output/input channel; the bus side's "channel" is
        // the bus pin's band, used only to give the corridor vertical a Y range to pack.
        private void CollectBusCables(int channelCount, float pad, float bs)
        {
            if (Entity?.Buses == null || Entity.Modules == null)
            {
                return;
            }
            int rows = Entity.Prototype.Rows;

            // module OUTPUT -> Input bus pin (stored on the bus as a Local source)
            foreach (ControllerBus bus in Entity.Buses)
            {
                int busChannel = Math.Min(Math.Max(busBandRow(bus), 0), channelCount - 1);
                for (int i = 0; i < ControllerBus.PinCount; i++)
                {
                    BusPinSource src = bus.PinSources[i];
                    if (src == null || src.Kind != BusPinSource.SourceKind.LocalModule)
                    {
                        continue;
                    }
                    Module m = Entity.Modules.Find(x => x.Id == src.ModuleId);
                    if (m?.Prototype == null || m.Row < 0 || m.Row >= rows)
                    {
                        continue;
                    }
                    int col = m.GetPinColumn(src.OutputId, isOutput: true);
                    float x = pad + (col + 0.5f) * bs;
                    m_cableSpecs.Add(new CableSpec
                    {
                        Src = m, OutputId = src.OutputId, Dst = null, InputId = null,
                        IsBus = true, Bus = bus, BusPinIdx = i, BusIsSource = false,
                        SrcChannelIdx = m.Row + 1,   // module output drops into the channel below
                        DstChannelIdx = busChannel,  // bus pin band — gives the corridor a Y range
                        SrcXp = x, DstXp = x,
                    });
                }
            }

            // module INPUT <- bus pin (stored on the module as a Bus connector)
            foreach (Module m in Entity.Modules)
            {
                if (m?.Prototype == null || m.Row < 0 || m.Row >= rows)
                {
                    continue;
                }
                foreach (var kv in m.InputModules)
                {
                    if (!kv.Value.IsBus || !kv.Value.TryGetPinIndex(out int pinIdx))
                    {
                        continue;
                    }
                    ControllerBus bus = Entity.GetBusById(kv.Value.ModuleId);
                    if (bus == null || pinIdx < 0 || pinIdx >= ControllerBus.PinCount)
                    {
                        continue;
                    }
                    int busChannel = Math.Min(Math.Max(busBandRow(bus), 0), channelCount - 1);
                    int col = m.GetPinColumn(kv.Key, isOutput: false);
                    float x = pad + (col + 0.5f) * bs;
                    m_cableSpecs.Add(new CableSpec
                    {
                        Src = null, OutputId = null, Dst = m, InputId = kv.Key,
                        IsBus = true, Bus = bus, BusPinIdx = pinIdx, BusIsSource = true,
                        SrcChannelIdx = busChannel,  // bus pin band — gives the corridor a Y range
                        DstChannelIdx = m.Row,       // module input rises into the channel above
                        SrcXp = x, DstXp = x,
                    });
                }
            }
        }

        // Connected state + dot hue for a bus pin.  An Input pin fed by a module output
        // takes THAT output's cable colour (so the wire and both pin dots match); every
        // other case (a readable pin driving module inputs, or holding a remote/network
        // source) keys on (busId, pinIndex) — the same key the module-input dot and the
        // bus→input cable use, so those match too.
        private bool busPinVisual(ControllerBus bus, int idx, out ColorRgba color)
        {
            BusPinSource src = bus.PinSources[idx];
            if (src != null && src.Kind == BusPinSource.SourceKind.LocalModule)
            {
                color = GetOrCreateCableColor(src.ModuleId, src.OutputId);
                return true;
            }
            color = GetOrCreateCableColor(bus.Id, idx.ToString());
            // Connected if it has a (non-local) source feeding it, or a module reads it.
            return src != null || anyModuleReadsBusPin(bus.Id, idx);
        }

        // True if any module input on this controller reads the given bus pin.
        private bool anyModuleReadsBusPin(long busId, int pinIdx)
        {
            if (Entity?.Modules == null)
            {
                return false;
            }
            foreach (Module m in Entity.Modules)
            {
                if (m?.InputModules == null)
                {
                    continue;
                }
                foreach (var kv in m.InputModules)
                {
                    if (kv.Value.IsBus && kv.Value.ModuleId == busId
                        && kv.Value.TryGetPinIndex(out int pi) && pi == pinIdx)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Position of a bus within its own side's stack (= its row band), matching
        // BuildBusSide which renders same-side buses top-down.
        private int busBandRow(ControllerBus bus)
        {
            int band = 0;
            foreach (ControllerBus b in Entity.Buses)
            {
                if (b == bus)
                {
                    return band;
                }
                if (b.Side == bus.Side)
                {
                    band++;
                }
            }
            return band;
        }

        // Renders one bus cable using the lanes already assigned by the shared allocator
        // (c.SideLane for the corridor X, the module-side channel lane for the channel Y),
        // so it never overlaps module cables.  Shape: module port -> its channel ->
        // side corridor -> down/up to the bus pin band -> ONTO the bus pin's dot in the
        // gutter.  Drawn into the top-most content overlay (so it paints over the ports)
        // in content space.  Mirrors CreateConnectionPath's segment style (idle/active
        // colour + hover highlight).
        private void CreateBusConnectionPath(CableSpec c, ColorRgba activeColor, ColorRgba idleColor)
        {
            Module mod = c.BusModule;
            if (mod?.Prototype == null || Entity?.Prototype == null || m_channelHeights == null)
            {
                return;
            }
            bool moduleIsOutput = !c.BusIsSource;
            string pinId = moduleIsOutput ? c.OutputId : c.InputId;

            ColorRgba activeBorder = darken(activeColor, factor: 0.45f, alpha: WIRE_BORDER_ALPHA);
            ColorRgba idleBorder   = darken(idleColor,   factor: 0.45f, alpha: WIRE_BORDER_ALPHA);

            UiComponent vMod  = makeBusSeg(false, idleColor, idleBorder);
            UiComponent hMod  = makeBusSeg(true,  idleColor, idleBorder);
            UiComponent vSide = makeBusSeg(false, idleColor, idleBorder);
            UiComponent hBus  = makeBusSeg(true,  idleColor, idleBorder);
            var segs = new (UiComponent seg, bool horizontal)[]
            {
                (vMod, false), (hMod, true), (vSide, false), (hBus, true)
            };

            void update()
            {
                if (mod.Prototype == null || Entity?.Prototype == null
                    || m_channelHeights == null || m_channelHeights.Length == 0)
                {
                    return;
                }
                float bs      = (float)Sizes.BLOCK_SIZE.Pixels;
                float pad     = VIEW_PAD_PX;
                float layoutW = Entity.Prototype.Columns * bs;
                int   rows    = Entity.Prototype.Rows;
                bool  left    = c.WrapLeft;

                // Segments live in the content-row overlay, whose origin is the left edge
                // of the LEFT gutter; m_gridHost sits one gutter-width in, so every grid-
                // relative X is shifted right by the gutter width.  Y is shared (all panels
                // top-align at the content top, which is what rowTopY measures from).
                float gx = (float)BUS_GUTTER_W_PX.Pixels;

                int col = mod.GetPinColumn(pinId, isOutput: moduleIsOutput);
                float pinX = gx + pad + (col + 0.5f) * bs;
                float pinY = rowTopY(mod.Row) + (moduleIsOutput ? 3.5f : 0.5f) * bs + CABLE_Y_OFFSET_PX;

                int   moduleChannel = moduleIsOutput ? c.SrcChannelIdx  : c.DstChannelIdx;
                int   moduleLane    = moduleIsOutput ? c.SrcChannelLane : c.DstChannelLane;
                float channelY = channelLaneY(moduleChannel, moduleLane);

                float sideOffset = Math.Min(CHANNEL_BASE_PX + c.SideLane * SIDE_LANE_PX, MAX_LANE_OFFSET_PX);
                float corridorX = gx + (left ? pad - sideOffset : pad + layoutW + sideOffset);

                // The cable runs all the way onto the bus pin's dot (content space): a
                // left bus puts its pins just right of the vertical name button; a right
                // bus's gutter starts past the whole grid host (grid + both paddings).
                float portX = left
                    ? (float)BUS_NAME_W_PX.Pixels + bs * 0.5f
                    : gx + layoutW + 2f * pad + bs * 0.5f;

                int band = busBandRow(c.Bus);
                float busPinY = rowTopY(band < rows ? band : rows) + c.BusPinIdx * bs + bs * 0.5f;

                float t = LINE_THICKNESS_PX;
                float halfT = t * 0.5f;
                placeBus(vMod,  pinX - halfT, Math.Min(pinY, channelY), t, Math.Abs(channelY - pinY));
                placeBus(hMod,  Math.Min(pinX, corridorX) - halfT, channelY - halfT, Math.Abs(corridorX - pinX) + t, t);
                placeBus(vSide, corridorX - halfT, Math.Min(channelY, busPinY), t, Math.Abs(busPinY - channelY));
                placeBus(hBus,  Math.Min(corridorX, portX) - halfT, busPinY - halfT, Math.Abs(portX - corridorX) + t, t);
            }

            // Re-place when the module moves (full redraws also rebuild, but this keeps
            // the cable glued to its pin between redraws — same as module cables).
            vMod.Observe(() => mod.Row).Observe(() => mod.Column).Do((r, cc) => update());

            // Hover/showLinks colouring: highlight when global links are on, when this
            // cable's own module pin is hovered, or when the owning module body is.
            long pinModId = mod.Id;
            string pinPortId = pinId;
            string cableBusPinKey = busPinKey(c.Bus.Id, c.BusPinIdx);
            foreach (var pair in segs)
            {
                UiComponent localSeg = pair.seg;
                bool localHoriz = pair.horizontal;
                localSeg.Observe(() =>
                {
                    if (m_controller.m_showsLinks)
                    {
                        return true;
                    }
                    // Hovering this cable's bus pin lights the whole wire.
                    if (m_hoveredBusPin == cableBusPinKey)
                    {
                        return true;
                    }
                    ModuleConnector hlOut = m_controller.m_higlightedOutput;
                    if (hlOut != null && moduleIsOutput)
                    {
                        return hlOut.ModuleId == pinModId && hlOut.OutputId == pinPortId;
                    }
                    ModuleConnector hlIn = m_controller.m_higlightedInput;
                    if (hlIn != null && !moduleIsOutput)
                    {
                        return hlIn.ModuleId == pinModId && hlIn.OutputId == pinPortId;
                    }
                    Module hovMod = m_controller.HoveredModuleGraphic;
                    return hovMod != null && hovMod.Id == pinModId;
                })
                .Do(active =>
                {
                    localSeg.Background(active ? activeColor : idleColor);
                    applyBusBorder(localSeg, localHoriz, active ? activeBorder : idleBorder);
                });
            }

            update();
        }

        // Segment factory + helpers, mirroring CreateConnectionPath's locals so bus cables
        // look identical to module cables and are torn down with them via m_lineSegments.
        private UiComponent makeBusSeg(bool horizontal, ColorRgba bg, ColorRgba bd)
        {
            UiComponent seg = new UiComponent().Background(bg).IgnoreInputPicking();
            applyBusBorder(seg, horizontal, bd);
            // Drawn into the content-row overlay (topmost) so bus cables paint over the
            // bus ports in the gutters — not into m_gridHost, which the gutters cover.
            m_busCableOverlay.Add(seg);
            seg.BringToFront();
            m_lineSegments.Add(seg);
            return seg;
        }

        private static void applyBusBorder(UiComponent c, bool horizontal, ColorRgba color)
        {
            Px b = 1.px();
            Px z = Px.Zero;
            if (horizontal)
            {
                c.Border(top: b, right: z, bottom: b, left: z, color: color, radius: 0);
            }
            else
            {
                c.Border(top: z, right: b, bottom: z, left: b, color: color, radius: 0);
            }
        }

        private static void placeBus(UiComponent c, float left, float top, float width, float height)
        {
            if (width < 0)
            {
                width = 0;
            }
            if (height < 0)
            {
                height = 0;
            }
            c.AbsolutePosition(top: top.px(), null, null, left: left.px());
            c.Size(width.px(), height.px());
        }

        // Bus-pin connect, mirroring the module pick/place flow on the inspector's
        // OutputConnection:
        //   - nothing held  -> pick THIS bus pin as a source (so the next click on a
        //                      module input wires that module to read this pin).
        //   - holding a module output + this is an Input pin -> feed the pin from that
        //                      output (bus reads the module).
        //   - otherwise      -> re-pick this pin (don't lose the click).
        private void onBusPinClick(ControllerBus bus, int idx)
        {
            ModuleConnector held = m_controller.OutputConnection;
            if (held == null)
            {
                m_controller.OutputConnection = ModuleConnector.ForBusPin(bus.Id, idx);
                return;
            }
            if (bus.PinTypes[idx] == ControllerBus.BusPinType.Output && !held.IsBus)
            {
                // Held source is a module output → bus reads it.
                Entity.SetBusPinSource(bus.Id, idx, BusPinSource.Local(held.ModuleId, held.OutputId));
                m_controller.OutputConnection = null;
                RedrawComponents();
            }
            else
            {
                m_controller.OutputConnection = ModuleConnector.ForBusPin(bus.Id, idx);
            }
        }

        // Highlights (or clears) the controller a Controller-type pin reads from, while
        // the pin is hovered — so the player can see which controller it links to.
        private void highlightPinSource(ControllerBus bus, int idx, bool on)
        {
            BusPinSource src = bus.PinSources[idx];
            if (src == null || !src.IsExternalController)
            {
                return;
            }
            if (!m_controller.Context.EntitiesManager.TryGetEntity(src.ControllerId, out Controller remote))
            {
                return;
            }
            if (on)
            {
                m_controller.Context.Highlighter.Highlight(remote, ColorRgba.Green);
            }
            else
            {
                m_controller.Context.Highlighter.RemoveHighlight(remote);
            }
        }

        // True when this bus pin is the one currently held in OutputConnection.
        private bool isBusPinPicked(ControllerBus bus, int idx)
        {
            ModuleConnector held = m_controller.OutputConnection;
            return held != null
                && held.IsBus
                && held.ModuleId == bus.Id
                && held.TryGetPinIndex(out int heldPinIdx)
                && heldPinIdx == idx;
        }

        // Right-click clears the source feeding this pin (the module→bus cable).
        private void onBusPinRightClick(ControllerBus bus, int idx)
        {
            if (bus.PinSources[idx] != null)
            {
                Entity.SetBusPinSource(bus.Id, idx, null);
                RedrawComponents();
            }
        }

        // Stacks characters top-to-bottom so the bus name reads vertically down the
        // gutter.  A true -90° rotation is a follow-up refinement; this avoids the
        // untested UIElements rotate API while still giving a vertical label.
        private static string verticalText(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            return string.Join("\n", s.ToCharArray());
        }

        // Bus names are shown in a short vertical strip (~7 monospace lines tall), so the
        // displayed name is capped at 7 characters; the editor also enforces this on input.
        private static string busNameClamped(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            return name.Length <= 7 ? name : name.Substring(0, 7);
        }

        private static string busPinTooltip(ControllerBus bus, int pinIndex)
        {
            string pinName = bus.PinNames[pinIndex];
            if (string.IsNullOrEmpty(pinName))
            {
                return bus.Name + " — pin " + (pinIndex + 1);
            }
            return bus.Name + "." + pinName;
        }
    }
}
