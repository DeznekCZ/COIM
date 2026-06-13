using System;
using System.Collections.Generic;
using System.IO;
using CustomAssets.Editor;
using CustomAssets.Editor.Io;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using UnityEngine;

namespace CustomAssets.Ui.Editors {

    /// In-game visual layout editor embedded by every entity editor (machines,
    /// settlement modules, mines, labs, reactors). Renders the footprint as a
    /// top-down 2D grid where tile "boxes" (and any ports baked into the layout
    /// string) can be painted, moved, and erased, shows the entity mesh as a
    /// flat axonometric thumbnail with a replace control, and offers a palette
    /// of built-in + modder-defined box types.
    ///
    /// The panel drives the def's <see cref="LayoutModel"/> in pure grid
    /// (column/row) coordinates and round-trips through <see cref="LayoutCodec"/>;
    /// machine add-port editing stays in the existing port grid (which works in
    /// COI relative coordinates), so this panel never has to reconcile the two
    /// coordinate spaces.
    public sealed class LayoutEditorPanel : Column {

        private enum Tool { Paint, Move, Erase }

        private const int CellPx = 24;
        private const int GapPx = 1;

        private static readonly ColorRgba EmptyColor   = new ColorRgba(28, 28, 32, 255);
        private static readonly ColorRgba OriginColor  = new ColorRgba(150, 120, 60, 255);
        private static readonly ColorRgba PortColor    = new ColorRgba(58, 132, 64, 255);
        private static readonly ColorRgba BorderColor  = ColorRgba.DarkGray;
        private static readonly ColorRgba SelectColor  = new ColorRgba(230, 220, 120, 255);

        private readonly PackModel m_model;
        private readonly ProtosDb m_protosDb;
        private readonly ILayoutHostDef m_host;
        private readonly LayoutModel m_layout;
        private readonly BoxTypeLibrary m_lib;
        private readonly Action m_onChanged;

        private Tool m_tool = Tool.Paint;
        private BoxTypeInfo m_selectedBox;
        private int m_paintHeight = 1;
        private (int x, int y)? m_moveSource;

        private readonly Column m_thumbHolder    = new Column();
        private readonly Column m_paletteHolder  = new Column();
        private readonly Column m_toolHolder     = new Column();
        private readonly Column m_gridHolder     = new Column();
        private readonly Column m_inspectorHolder= new Column();

        public LayoutEditorPanel(PackModel model, ProtosDb protosDb,
                ILayoutHostDef host, Action onChanged) {
            m_model = model;
            m_protosDb = protosDb;
            m_host = host;
            m_onChanged = onChanged;
            m_lib = BoxTypeLibrary.BuildFor(model);
            m_selectedBox = m_lib.All.Count > 0 ? m_lib.All[0] : null;

            // Materialise the structured layout if the loader hasn't (e.g. a
            // brand-new def). Stays non-structured until the modder edits.
            if (host.Layout == null) {
                host.Layout = LayoutCodec.TryParse(host.LayoutSourceStr, m_lib);
            }
            m_layout = host.Layout;

            this.AlignItemsStretch().Gap(4.px());
            build();
        }

        // ---- Construction ------------------------------------------------------

        private void build() {
            // Raw fallback: the source string carries tokens the codec can't
            // model. Offer a plain text editor so the layout is still editable
            // rather than silently locked.
            if (!m_layout.ParsedOk && !string.IsNullOrEmpty(m_layout.RawLayoutStr)) {
                Add(new Label(new LocStrFormatted(
                        "This layout uses tokens the visual editor can't model yet — editing raw text."))
                    .Color(ColorRgba.LightGray).TinyFontSize());
                TextField raw = new TextField().Multiline(true).SetTextAreaMinHeight(120.px());
                raw.Class(Cls.fontMonospace);
                raw.Text(m_host.LayoutSourceStr ?? "");
                raw.OnValueChanged(v => {
                    m_host.LayoutSourceStr = string.IsNullOrEmpty(v) ? null : v;
                    m_layout.RawLayoutStr = m_host.LayoutSourceStr;
                    m_layout.IsStructured = false;
                    m_onChanged?.Invoke();
                });
                Add(raw);
                return;
            }

            // Mesh thumbnail + replace.
            m_thumbHolder.AlignItemsStretch();
            Add(new Label(new LocStrFormatted("mesh")).Class(Cls.groupHeader));
            Add(m_thumbHolder);
            rebuildThumb();

            // Box-type palette.
            m_paletteHolder.AlignItemsStretch();
            Add(new Label(new LocStrFormatted("box types")).Class(Cls.groupHeader));
            Add(m_paletteHolder);
            rebuildPalette();

            // Tool mode + grid (grid is self-evident, no header).
            m_toolHolder.AlignItemsStretch();
            Add(m_toolHolder);
            rebuildToolbar();

            m_gridHolder.AlignItemsStretch();
            Add(m_gridHolder);
            m_inspectorHolder.AlignItemsStretch();
            Add(m_inspectorHolder);
            rebuildGrid();
            rebuildInspector();
        }

        // ---- Mesh thumbnail ----------------------------------------------------

        private void rebuildThumb() {
            m_thumbHolder.Clear();
            Row row = new Row().Gap(8.px());
            row.AlignItemsCenter();

            string meshRef = m_layout.MeshRef;
            UiComponent thumb = buildMeshThumb(meshRef);
            row.Add(thumb);

            // Replace control. Uses the pack's AssetPathPicker when the
            // LoadedPack is available, else a plain path field.
            if (PackRegistry.TryGet(m_model.ModId, out LoadedPack pack)) {
                row.Add(new AssetPathPicker(
                    pack,
                    getPath: () => m_layout.MeshRef,
                    setPath: v => {
                        m_layout.MeshRef = string.IsNullOrEmpty(v) ? null : v;
                        m_layout.IsStructured = true;
                        m_onChanged?.Invoke();
                        rebuildThumb();
                    },
                    kind: AssetsCatalog.AssetKind.Mesh,
                    title: new LocStrFormatted("Replace mesh / prefab")));
            } else {
                TextField path = new TextField();
                path.Class(Cls.fontMonospace);
                path.Text(meshRef ?? "");
                path.OnValueChanged(v => {
                    m_layout.MeshRef = string.IsNullOrEmpty(v) ? null : v;
                    m_layout.IsStructured = true;
                    m_onChanged?.Invoke();
                });
                row.Add(path);
            }
            m_thumbHolder.Add(row);
        }

        private UiComponent buildMeshThumb(string meshRef) {
            const int size = 96;
            if (!string.IsNullOrEmpty(meshRef)
                    && meshRef.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)) {
                string abs = Path.IsPathRooted(meshRef)
                    ? meshRef
                    : Path.Combine(m_model.RootPath ?? "", meshRef);
                try {
                    Texture2D tex = MeshThumbnailCache.LoadObj(abs);
                    if (tex != null) return new Img(tex).Width(size.px()).Height(size.px());
                } catch (Exception) {
                    // fall through to placeholder
                }
            }
            Column ph = new Column();
            ph.Size(size.px(), size.px()).Background(EmptyColor).Border(1.px(), BorderColor, 2)
              .AlignItemsCenter();
            ph.Add(new Label(new LocStrFormatted(string.IsNullOrEmpty(meshRef) ? "(source mesh)" : "(no preview)"))
                .Color(ColorRgba.LightGray).TinyFontSize());
            return ph;
        }

        // ---- Palette -----------------------------------------------------------

        private void rebuildPalette() {
            m_paletteHolder.Clear();
            Row row = new Row().Gap(4.px());
            foreach (BoxTypeInfo info in m_lib.All) {
                row.Add(buildSwatch(info));
            }
            row.Add(buildNewBoxTypeButton());
            m_paletteHolder.Add(row);
        }

        private UiComponent buildSwatch(BoxTypeInfo info) {
            Column cell = new Column();
            cell.Size(28.px(), 28.px())
                .Background(new ColorRgba(info.R, info.G, info.B, (byte)255))
                .Border((m_selectedBox == info ? 2 : 1).px(),
                        m_selectedBox == info ? SelectColor : BorderColor, 2)
                .AlignItemsCenter();
            cell.Add(new Label(new LocStrFormatted(info.Token[0].ToString()))
                .Class(Cls.fontMonospace).Color(ColorRgba.White).FontBold());
            cell.Tooltip(new LocStrFormatted(info.Name + "  (" + info.Token + ")"
                + (info.IsBuiltin ? "" : "  [custom]")));
            cell.OnMouseDown(evt => {
                if (evt.button != 0) return;
                m_selectedBox = info;
                m_tool = Tool.Paint;
                rebuildPalette();
                rebuildToolbar();
                evt.StopPropagation();
            });
            return cell;
        }

        private UiComponent buildNewBoxTypeButton() {
            ButtonText btn = null;
            btn = new ButtonText(new LocStrFormatted(" + type "), () => openNewBoxTypeDialog(btn));
            return btn;
        }

        // ---- Toolbar -----------------------------------------------------------

        private void rebuildToolbar() {
            m_toolHolder.Clear();
            Row row = new Row().Gap(4.px());
            row.AlignItemsCenter();
            row.Add(toolButton("Paint", Tool.Paint));
            row.Add(toolButton("Move",  Tool.Move));
            row.Add(toolButton("Erase", Tool.Erase));

            // Height stepper for the paint tool.
            row.Add(new Label(new LocStrFormatted("   height:")).TinyFontSize());
            row.Add(new ButtonText(new LocStrFormatted(" - "), () => {
                if (m_paintHeight > 1) { m_paintHeight--; rebuildToolbar(); }
            }));
            row.Add(new Label(new LocStrFormatted(m_paintHeight.ToString()))
                .Class(Cls.fontMonospace).FontBold());
            row.Add(new ButtonText(new LocStrFormatted(" + "), () => {
                if (m_paintHeight < 9) { m_paintHeight++; rebuildToolbar(); }
            }));

            if (m_tool == Tool.Move) {
                row.Add(new Label(new LocStrFormatted(
                        m_moveSource.HasValue
                            ? "   click destination cell"
                            : "   click a tile to move"))
                    .Color(ColorRgba.LightGray).TinyFontSize());
            }
            m_toolHolder.Add(row);
        }

        private UiComponent toolButton(string label, Tool tool) {
            return new ButtonText(
                new LocStrFormatted(m_tool == tool ? "[" + label + "]" : " " + label + " "),
                () => { m_tool = tool; m_moveSource = null; rebuildToolbar(); rebuildGrid(); });
        }

        // ---- Grid --------------------------------------------------------------

        private void rebuildGrid() {
            m_gridHolder.Clear();

            // Occupancy + lookups keyed by packed (col,row).
            Dictionary<long, LayoutTileRef> tileAt = new Dictionary<long, LayoutTileRef>();
            foreach (LayoutTileRef t in m_layout.Tiles) tileAt[key(t.X, t.Y)] = t;
            Dictionary<long, LayoutPortCell> portAt = new Dictionary<long, LayoutPortCell>();
            if (m_layout.PortCells != null) {
                foreach (LayoutPortCell p in m_layout.PortCells) portAt[key(p.X, p.Y)] = p;
            }

            // Bounds: occupied cells padded by 1 so there's room to paint. A
            // blank layout shows a small default canvas to start from.
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (LayoutTileRef t in m_layout.Tiles) { expand(ref minX, ref minY, ref maxX, ref maxY, t.X, t.Y); }
            if (m_layout.PortCells != null)
                foreach (LayoutPortCell p in m_layout.PortCells) expand(ref minX, ref minY, ref maxX, ref maxY, p.X, p.Y);
            if (minX == int.MaxValue) { minX = 0; minY = 0; maxX = 4; maxY = 4; }
            else { minX--; minY--; maxX++; maxY++; }

            Column grid = new Column().Gap(GapPx.px());
            for (int row = minY; row <= maxY; row++) {
                Row tileRow = new Row().Gap(GapPx.px());
                for (int col = minX; col <= maxX; col++) {
                    tileAt.TryGetValue(key(col, row), out LayoutTileRef tile);
                    portAt.TryGetValue(key(col, row), out LayoutPortCell port);
                    tileRow.Add(buildCell(col, row, tile, port));
                }
                grid.Add(tileRow);
            }
            m_gridHolder.Add(grid);
        }

        private UiComponent buildCell(int col, int row, LayoutTileRef tile, LayoutPortCell port) {
            ColorRgba bg;
            if (port != null) bg = PortColor;
            else if (tile != null && tile.IsOrigin) bg = OriginColor;
            else if (tile != null) bg = boxColor(tile);
            else bg = EmptyColor;

            bool selected = m_moveSource.HasValue && m_moveSource.Value.x == col && m_moveSource.Value.y == row;

            Column cell = new Column();
            cell.Size(CellPx.px(), CellPx.px())
                .Background(bg)
                .Border((selected ? 2 : 1).px(), selected ? SelectColor : BorderColor, 2)
                .AlignItemsCenter();

            if (port != null) {
                cell.Add(new Label(new LocStrFormatted(port.Arrow.ToString()))
                    .Class(Cls.fontMonospace).Color(ColorRgba.White).TinyFontSize());
                cell.Add(new Label(new LocStrFormatted(port.Name.ToString()))
                    .Class(Cls.fontMonospace).Color(ColorRgba.White).FontBold());
            } else if (tile != null && tile.IsOrigin) {
                cell.Add(new Label(new LocStrFormatted("*"))
                    .Class(Cls.fontMonospace).Color(ColorRgba.White).FontBold());
            } else if (tile != null) {
                cell.Add(new Label(new LocStrFormatted(tile.Height.HasValue ? tile.Height.Value.ToString() : "■"))
                    .Class(Cls.fontMonospace).Color(ColorRgba.White).TinyFontSize());
            }

            cell.OnMouseDown(evt => {
                if (evt.button != 0) return;
                onCellClick(col, row, tile, port);
                evt.StopPropagation();
            });
            return cell;
        }

        private void onCellClick(int col, int row, LayoutTileRef tile, LayoutPortCell port) {
            switch (m_tool) {
                case Tool.Paint:
                    if (m_selectedBox == null) return;
                    if (port != null) return; // don't paint over a port cell
                    if (tile == null) {
                        tile = new LayoutTileRef(col, row);
                        m_layout.Tiles.Add(tile);
                    }
                    tile.IsOrigin = false;
                    tile.BoxTypeToken = m_selectedBox.Token;
                    tile.BoxTypeRefId = m_selectedBox.IsBuiltin ? null : m_selectedBox.Id;
                    tile.Height = m_selectedBox.HasHeightWildcard ? m_paintHeight : (int?)null;
                    markChanged();
                    break;

                case Tool.Erase:
                    if (port != null) { m_layout.PortCells.Remove(port); markChanged(); break; }
                    if (tile != null) { m_layout.Tiles.Remove(tile); markChanged(); }
                    break;

                case Tool.Move:
                    if (!m_moveSource.HasValue) {
                        if (tile != null || port != null) {
                            m_moveSource = (col, row);
                            rebuildToolbar();
                            rebuildGrid();
                        }
                        return;
                    }
                    int sx = m_moveSource.Value.x, sy = m_moveSource.Value.y;
                    m_moveSource = null;
                    if (sx == col && sy == row) { rebuildToolbar(); rebuildGrid(); return; }
                    moveCell(sx, sy, col, row);
                    markChanged();
                    rebuildToolbar();
                    break;
            }
        }

        private void moveCell(int sx, int sy, int dx, int dy) {
            // Destination must be free.
            foreach (LayoutTileRef t in m_layout.Tiles) if (t.X == dx && t.Y == dy) return;
            if (m_layout.PortCells != null)
                foreach (LayoutPortCell p in m_layout.PortCells) if (p.X == dx && p.Y == dy) return;
            foreach (LayoutTileRef t in m_layout.Tiles) {
                if (t.X == sx && t.Y == sy) { t.X = dx; t.Y = dy; return; }
            }
            if (m_layout.PortCells != null)
                foreach (LayoutPortCell p in m_layout.PortCells) {
                    if (p.X == sx && p.Y == sy) { p.X = dx; p.Y = dy; return; }
                }
        }

        // ---- Inspector ---------------------------------------------------------

        private void rebuildInspector() {
            m_inspectorHolder.Clear();
            int tiles = m_layout.Tiles?.Count ?? 0;
            int ports = m_layout.PortCells?.Count ?? 0;
            m_inspectorHolder.Add(new Label(new LocStrFormatted(
                    $"{tiles} tile(s), {ports} embedded port(s)  ·  Paint / Move (2 clicks) / Erase"))
                .Color(ColorRgba.LightGray).TinyFontSize());
        }

        // ---- New box type dialog ----------------------------------------------

        private void openNewBoxTypeDialog(UiComponent anchor) {
            FloatingColumn popup = new FloatingColumn(
                FloaterPositionPolicy.BELOW, keepOpenOnHover: false,
                openAfterDelay: false, closeOnClickOutside: true);
            Column body = new Column().Gap(4.px()).Padding(8.px());
            body.Add(new Label(new LocStrFormatted("New box type")).Class(Cls.groupHeader));

            TextField idField = new TextField();
            idField.Text("CustomBox");
            body.Add(labeled("boxTypeId", idField));

            TextField tokenField = new TextField().Class(Cls.fontMonospace);
            tokenField.Text("=0=");
            body.Add(labeled("token (3 chars; '0' = height wildcard)", tokenField));

            Label err = new Label(new LocStrFormatted("")).Color(new ColorRgba(220, 120, 120, 255)).TinyFontSize();
            body.Add(err);

            body.Add(new ButtonText(new LocStrFormatted("Create"), () => {
                string id = (idField.GetText() ?? "").Trim();
                string token = tokenField.GetText() ?? "";
                if (string.IsNullOrEmpty(id) || token.Length != 3) {
                    err.Value(new LocStrFormatted("Need an id and a 3-character token."));
                    return;
                }
                char f = token[0];
                if ((f >= 'A' && f <= 'Z') || f == '^' || f == '>' || f == 'v' || f == '<' || f == '+') {
                    err.Value(new LocStrFormatted("Token's first char collides with a port name/arrow."));
                    return;
                }
                BoxTypeDef def = new BoxTypeDef { BoxTypeId = id, Token = token };
                def.SourceFile = m_host is DefBase hb ? hb.SourceFile : null;
                def.Dirty = true;
                m_model.Definitions.Add(def);
                m_selectedBox = m_lib.AddCustom(def);
                m_tool = Tool.Paint;
                rebuildPalette();
                rebuildToolbar();
                popup.Close();
            }));

            popup.Add(body);
            popup.Open(anchor);
        }

        // ---- Helpers -----------------------------------------------------------

        private void markChanged() {
            m_layout.IsStructured = true;
            m_onChanged?.Invoke();
            rebuildGrid();
            rebuildInspector();
        }

        private ColorRgba boxColor(LayoutTileRef tile) {
            if (m_lib.TryMatchToken(tile.BoxTypeToken ?? "{0}", out BoxTypeInfo info, out _)
                    || (tile.BoxTypeRefId != null && m_lib.TryGetById(tile.BoxTypeRefId, out info))) {
                return new ColorRgba(info.R, info.G, info.B, (byte)255);
            }
            return new ColorRgba(72, 72, 80, 255);
        }

        private static Column labeled(string label, UiComponent field) {
            Column col = new Column().Gap(2.px());
            col.AlignItemsStretch();
            col.Add(new Label(new LocStrFormatted(label)).TinyFontSize());
            col.Add(field);
            return col;
        }

        private static long key(int x, int y) {
            return ((long)(uint)x << 32) | (uint)y;
        }

        private static void expand(ref int minX, ref int minY, ref int maxX, ref int maxY, int x, int y) {
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }
    }
}
