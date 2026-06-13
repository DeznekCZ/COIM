using System.Collections.Generic;

namespace CustomAssets.Editor.Model {

    /// One entry in the layout editor's box-type palette — either a built-in
    /// COI token or a modder-defined <see cref="BoxTypeDef"/>. Carries enough
    /// to render a palette swatch, paint a tile, and round-trip the tile back
    /// to its 3-character grid token.
    public sealed class BoxTypeInfo {
        /// Palette id. For built-ins this is a stable synthetic id
        /// ("builtin:machine"); for custom types it is the
        /// <see cref="BoxTypeDef.BoxTypeId"/>.
        public string Id;

        /// 3-character grid token template. A '0' in the middle slot is a
        /// per-tile height wildcard (see <see cref="HasHeightWildcard"/>).
        public string Token;

        /// Human-facing palette label.
        public string Name;

        /// Swatch colour components (0..255). The UI converts to its colour
        /// type; keeping raw bytes avoids coupling the model to a UI library.
        public byte R;
        public byte G;
        public byte B;

        /// True for the fixed built-in COI tokens; false for modder-defined
        /// types (which the palette renders with a small "custom" affordance
        /// and which back-reference <see cref="Def"/>).
        public bool IsBuiltin;

        /// Default height applied when first painting a tile of this type.
        public int DefaultHeight = 1;

        /// The backing definition for custom types; null for built-ins.
        public BoxTypeDef Def;

        public bool HasHeightWildcard =>
            !string.IsNullOrEmpty(Token) && Token.Length == 3 && Token[1] == '0';

        public BoxTypeInfo() { }

        public BoxTypeInfo(string id, string token, string name,
                           byte r, byte g, byte b, bool isBuiltin) {
            Id = id;
            Token = token;
            Name = name;
            R = r;
            G = g;
            B = b;
            IsBuiltin = isBuiltin;
        }
    }

    /// The set of box ("tile") types available to a pack's layout editor and
    /// codec — the fixed built-in COI tokens plus every
    /// <see cref="BoxTypeDef"/> the pack defines via <c>define_box_type</c>.
    /// Built once per pack on load (see <see cref="BuildFor"/>) and consulted
    /// by <see cref="Io.LayoutCodec"/> when parsing/emitting tile tokens and
    /// by the editor when rendering the palette.
    public sealed class BoxTypeLibrary {

        /// The built-in COI tokens the editor models directly. The high
        /// 10}/10)/20) height families are intentionally omitted from the
        /// palette — the per-tile height stepper covers 1..9 and taller
        /// boxes round-trip verbatim through the raw-string fallback.
        public static readonly IReadOnlyList<BoxTypeInfo> BuiltIns = new List<BoxTypeInfo> {
            new BoxTypeInfo("builtin:machine", "{0}", "Machine box",     72,  72,  80, true),
            new BoxTypeInfo("builtin:floor",   "[0]", "Hardened floor", 120, 120, 128, true),
            new BoxTypeInfo("builtin:occupant","(0)", "Single occupant", 96,  84,  64, true),
            new BoxTypeInfo("builtin:ocean",   "~0~", "Ocean",           48,  96, 150, true),
            new BoxTypeInfo("builtin:vehicle", "_0_", "Vehicle surface", 90, 100,  78, true),
        };

        private readonly List<BoxTypeInfo> m_all = new List<BoxTypeInfo>();
        private readonly Dictionary<string, BoxTypeInfo> m_byId =
            new Dictionary<string, BoxTypeInfo>(System.StringComparer.Ordinal);
        private readonly Dictionary<char, List<BoxTypeInfo>> m_byFirstChar =
            new Dictionary<char, List<BoxTypeInfo>>();

        /// All palette entries — built-ins first, then custom types in
        /// definition order.
        public IReadOnlyList<BoxTypeInfo> All => m_all;

        public BoxTypeLibrary() {
            foreach (BoxTypeInfo b in BuiltIns) add(b);
        }

        /// Build the library for a pack: built-ins plus every
        /// <see cref="BoxTypeDef"/> found in the model.
        public static BoxTypeLibrary BuildFor(PackModel model) {
            BoxTypeLibrary lib = new BoxTypeLibrary();
            if (model?.Definitions != null) {
                foreach (DefBase d in model.Definitions) {
                    if (d is BoxTypeDef bt && !string.IsNullOrEmpty(bt.Token) && bt.Token.Length == 3) {
                        lib.AddCustom(bt);
                    }
                }
            }
            return lib;
        }

        /// Register (or replace) a custom box type at runtime — used by the
        /// "new box type" dialog so the palette refreshes immediately.
        public BoxTypeInfo AddCustom(BoxTypeDef def) {
            BoxTypeInfo info = new BoxTypeInfo(
                def.BoxTypeId, def.Token,
                string.IsNullOrEmpty(def.Name) ? def.BoxTypeId : def.Name,
                160, 140, 90, false) {
                Def = def,
                DefaultHeight = def.HeightTo.HasValue && def.HeightTo.Value > 0 ? 1 : 1,
            };
            // Replace an existing entry with the same id rather than duplicate.
            for (int i = 0; i < m_all.Count; i++) {
                if (m_all[i].Id == info.Id) {
                    m_all[i] = info;
                    reindex();
                    return info;
                }
            }
            add(info);
            return info;
        }

        public bool TryGetById(string id, out BoxTypeInfo info) {
            if (string.IsNullOrEmpty(id)) { info = null; return false; }
            return m_byId.TryGetValue(id, out info);
        }

        /// Match a 3-character grid token against the library. On success
        /// returns the box-type entry and the per-tile height parsed from the
        /// wildcard slot (1 when the token carries no wildcard).
        public bool TryMatchToken(string token3, out BoxTypeInfo info, out int height) {
            info = null;
            height = 1;
            if (string.IsNullOrEmpty(token3) || token3.Length != 3) return false;
            if (!m_byFirstChar.TryGetValue(token3[0], out List<BoxTypeInfo> candidates)) return false;
            foreach (BoxTypeInfo c in candidates) {
                if (c.Token == token3) {
                    info = c;
                    height = 1;
                    return true;
                }
                // Height-wildcard match: same first/third char, '0' middle in
                // the template, a digit 1..9 in the actual token.
                if (c.HasHeightWildcard
                        && c.Token[0] == token3[0]
                        && c.Token[2] == token3[2]
                        && token3[1] >= '1' && token3[1] <= '9') {
                    info = c;
                    height = token3[1] - '0';
                    return true;
                }
            }
            return false;
        }

        private void add(BoxTypeInfo info) {
            m_all.Add(info);
            m_byId[info.Id] = info;
            char key = info.Token[0];
            if (!m_byFirstChar.TryGetValue(key, out List<BoxTypeInfo> list)) {
                list = new List<BoxTypeInfo>();
                m_byFirstChar[key] = list;
            }
            list.Add(info);
        }

        private void reindex() {
            m_byId.Clear();
            m_byFirstChar.Clear();
            foreach (BoxTypeInfo b in m_all) {
                m_byId[b.Id] = b;
                char key = b.Token[0];
                if (!m_byFirstChar.TryGetValue(key, out List<BoxTypeInfo> list)) {
                    list = new List<BoxTypeInfo>();
                    m_byFirstChar[key] = list;
                }
                list.Add(b);
            }
        }
    }
}
