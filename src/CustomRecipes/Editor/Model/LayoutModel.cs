using System.Collections.Generic;

namespace CustomAssets.Editor.Model {

    /// One occupied tile ("box") in a structured entity layout. Mirrors a
    /// single 3-character token in COI's ASCII layout grid (see
    /// <see cref="Io.LayoutCodec"/>). Coordinates are raw grid coordinates:
    /// <see cref="X"/> is the column (0 = left), <see cref="Y"/> is the row
    /// (0 = the TOP line of the layout string). The codec round-trips the
    /// grid verbatim — COI's parser does its own Y-flip internally, so the
    /// editor never has to.
    public sealed class LayoutTileRef {
        public int X;
        public int Y;

        /// Box height in tiles (the digit that fills the '0' wildcard slot of
        /// the token template, 1..9). Null for fixed tokens that carry no
        /// height wildcard and for origin markers.
        public int? Height;

        /// 3-character token template with the height digit normalised to the
        /// '0' wildcard slot — e.g. "{0}" (machine box), "[0]" (hardened
        /// floor), "(0)" (single occupant), or a modder-defined custom token.
        /// Null means the default machine box ("{0}").
        public string BoxTypeToken;

        /// Id of the <see cref="BoxTypeDef"/> this tile paints, when it uses a
        /// modder-defined custom box type. Null for built-in tokens. Purely a
        /// UI/back-reference convenience — the canonical value is
        /// <see cref="BoxTypeToken"/>.
        public string BoxTypeRefId;

        /// True when this cell is the layout origin marker (" * "). Origin
        /// cells carry no height/box-type; COI borrows a neighbour's spec.
        public bool IsOrigin;

        public LayoutTileRef() { }

        public LayoutTileRef(int x, int y, string boxTypeToken = null, int? height = null) {
            X = x;
            Y = y;
            BoxTypeToken = boxTypeToken;
            Height = height;
        }
    }

    /// One I/O port baked directly into the layout grid as a 3-character
    /// token (letter + shape + arrow), e.g. "B@>". These appear in the source
    /// layout strings of port-bearing entities (reactors, labs, mines) and
    /// MUST be preserved when the footprint is edited — dropping them would
    /// strip the reactor's fuel/water/steam ports. Stored in raw grid
    /// coordinates so they can be moved on the grid without any coordinate
    /// transform; COI re-derives port type/direction from tile occupancy at
    /// parse time, so emit only needs the three characters back.
    public sealed class LayoutPortCell {
        public int X;
        public int Y;

        /// Port label letter (A–Z).
        public char Name;

        /// Direction arrow: '^' '>' 'v' '<' '+'.
        public char Arrow;

        /// Port shape char: '#' '~' '\'' '@' '|' (or any modded shape char).
        public char Shape;

        /// Derived for display only (resolved against tile occupancy at parse
        /// time): "input" / "output" / "any". Not used on emit.
        public string Type;

        /// Derived for display only: "+X" / "-X" / "+Y" / "-Y". Not used on
        /// emit (COI recomputes it).
        public string Direction;

        public LayoutPortCell() { }
    }

    /// Structured representation of an entity's layout — the footprint tiles,
    /// any I/O ports embedded in the layout grid, the optional mesh/prefab
    /// override, and a verbatim fall-back string. Attached to every
    /// layout-bearing definition via <see cref="ILayoutHostDef.Layout"/>.
    ///
    /// Two modes:
    ///   â€¢ <see cref="IsStructured"/> == false (the default on load): the
    ///     model is a passive parse of <see cref="RawLayoutStr"/>. The emitter
    ///     writes the raw string back BYTE-IDENTICAL so existing hand-written
    ///     layouts never churn until the modder actively edits them.
    ///   â€¢ <see cref="IsStructured"/> == true: the modder edited the layout in
    ///     the visual editor; the emitter re-serialises <see cref="Tiles"/>
    ///     (and origin cells) through <see cref="Io.LayoutCodec.Emit"/>.
    public sealed class LayoutModel {

        /// Grid width in tiles (columns). 0 when empty / unknown.
        public int Width;

        /// Grid height in tiles (rows). 0 when empty / unknown.
        public int Height;

        /// Occupied footprint tiles, including origin markers
        /// (<see cref="LayoutTileRef.IsOrigin"/>).
        public List<LayoutTileRef> Tiles = new List<LayoutTileRef>();

        /// Ports baked into the layout grid as tokens (see
        /// <see cref="LayoutPortCell"/>). Preserved and movable in grid space;
        /// emitted back as 3-character tokens.
        public List<LayoutPortCell> PortCells = new List<LayoutPortCell>();

        /// True when <see cref="Io.LayoutCodec.TryParse"/> fully understood the
        /// source string (every token recognised). When false the layout can
        /// only be emitted verbatim from <see cref="RawLayoutStr"/> — the
        /// editor shows a "raw layout" notice rather than the visual grid.
        public bool ParsedOk;

        /// Ports parsed out of the layout grid (letter+shape+arrow tokens),
        /// with type / direction already resolved against tile occupancy the
        /// same way COI's parser resolves them. Surfaced so the visual editor
        /// can render and move them; on save the host def's own port list is
        /// the canonical home (see <see cref="Io.LayoutCodec"/>). This list is
        /// the SAME instance as the host def's add-ports list when the def has
        /// one, so edits flow through transparently.
        public List<PortRef> Ports = new List<PortRef>();

        /// Optional mesh / prefab override for the entity's visual model. A
        /// pack-relative ".obj" path, a "*.prefab" path, or an Assets.* typed
        /// ref. Null = keep the source entity's graphics. Applied at runtime
        /// via the entity proto's Gfx (see the runtime applier).
        public string MeshRef;

        /// Whether the modder has structurally edited this layout. Drives the
        /// emit path (see class docs).
        public bool IsStructured;

        /// Verbatim source layout string captured on load. Emitted unchanged
        /// while <see cref="IsStructured"/> is false; also the safety net the
        /// codec falls back to whenever it meets a token it can't model.
        public string RawLayoutStr;

        public LayoutModel() { }

        /// True when there is nothing structured to emit (no tiles and no raw
        /// string) — the host should omit layout_str entirely.
        public bool IsEmpty =>
            (Tiles == null || Tiles.Count == 0)
            && string.IsNullOrEmpty(RawLayoutStr);
    }

    /// Marker for any definition kind that owns an entity layout the visual
    /// layout editor can drive. Lets the base editor and the loader/emitter
    /// treat every entity kind uniformly without a giant type switch.
    public interface ILayoutHostDef {

        /// The opaque layout string as it appears (or will appear) in the
        /// Python <c>layout_str=</c> argument. Kept for byte-identical
        /// round-trip and as the source the structured <see cref="Layout"/> is
        /// parsed from / emitted to.
        string LayoutSourceStr { get; set; }

        /// Structured view of the layout. Null until the loader backfills it
        /// (or the editor creates one). See <see cref="LayoutModel"/>.
        LayoutModel Layout { get; set; }

        /// The def's port list, when the kind supports ports (machines, labs,
        /// reactors). Null for kinds that carry no ports (settlement modules,
        /// mines). The visual editor binds <see cref="LayoutModel.Ports"/> to
        /// this list so port edits in the grid round-trip through the existing
        /// <c>ports=</c> / <c>add_ports=</c> emit path.
        List<PortRef> Ports { get; }
    }
}
