using System.Collections.Generic;
using System.Text;
using CustomAssets.Editor.Model;

namespace CustomAssets.Editor.Io {

    /// Converts between COI's ASCII tile-grid layout strings and the editor's
    /// structured <see cref="LayoutModel"/>. Pure C# (no Mafi runtime types)
    /// so it round-trips entirely editor-side and is self-testable via
    /// <see cref="SelfTest"/>.
    ///
    /// The grammar mirrors <c>EntityLayoutParser</c>: a rectangular grid of
    /// 3-character tokens, one per tile, rows joined with '\n'. Recognised
    /// tokens:
    ///   â€¢ "   " (3 spaces)        â†’ empty cell
    ///   â€¢ " * "                   â†’ origin marker
    ///   â€¢ box token (e.g. "{3}", "[1]", "(2)", "~0~", or a custom token)
    ///                              â†’ a footprint tile; a digit in the middle
    ///                                slot is the per-tile height
    ///   â€¢ port token (letter + shape char + arrow, any order, e.g. "B@>")
    ///                              â†’ an I/O port baked into the grid
    /// Anything else makes <see cref="TryParse"/> fall back to a verbatim,
    /// non-structured model so a layout is NEVER corrupted by an unmodelled
    /// token.
    public static class LayoutCodec {

        private const string EmptyToken  = "   ";
        private const string OriginToken = " * ";
        private static readonly char[] PortDirections = { '^', '>', 'v', '<', '+' };

        // ---- Public API --------------------------------------------------------

        /// Parse a layout string into a structured model. Always returns a
        /// model (never null): on success <see cref="LayoutModel.ParsedOk"/> is
        /// true and the tiles/ports are populated; on any unrecognised token or
        /// malformed grid it is false and only <see cref="LayoutModel.RawLayoutStr"/>
        /// is meaningful. <see cref="LayoutModel.IsStructured"/> is left false
        /// either way — callers flip it to true only once the modder edits the
        /// layout, so untouched layouts round-trip byte-identically.
        public static LayoutModel TryParse(string layoutStr, BoxTypeLibrary lib) {
            LayoutModel model = new LayoutModel { RawLayoutStr = layoutStr };
            if (string.IsNullOrEmpty(layoutStr)) {
                model.ParsedOk = false;
                return model;
            }

            // Jagged-but-token-aligned rows are accepted by padding them with
            // empty cells first; only rows that aren't a multiple of 3 chars
            // (which PadRectangular leaves alone) still fail the width check
            // below and fall back to verbatim passthrough.
            string normalized = PadRectangular(layoutStr).Replace("\r\n", "\n").Replace("\r", "\n");
            string[] lines = normalized.Split('\n');
            // Drop a single trailing empty line that a final '\n' produces, but
            // keep genuinely blank interior rows (they'd be all-spaces tokens).
            int lineCount = lines.Length;
            while (lineCount > 0 && lines[lineCount - 1].Length == 0) lineCount--;
            if (lineCount == 0) { model.ParsedOk = false; return model; }

            int width = lines[0].Length / 3;
            for (int r = 0; r < lineCount; r++) {
                string line = lines[r];
                if (line.Length % 3 != 0 || line.Length / 3 != width || line.IndexOf('\t') >= 0) {
                    model.ParsedOk = false;
                    return model;
                }
            }
            if (width == 0) { model.ParsedOk = false; return model; }

            // First pass: occupancy (which cells are box tiles), so port type /
            // direction can be derived the way COI's parser derives them.
            bool[,] occupied = new bool[width, lineCount];
            for (int r = 0; r < lineCount; r++) {
                for (int c = 0; c < width; c++) {
                    string tok = lines[r].Substring(c * 3, 3);
                    if (tok == EmptyToken || tok == OriginToken) continue;
                    if (lib.TryMatchToken(tok, out _, out _)) occupied[c, r] = true;
                }
            }

            for (int r = 0; r < lineCount; r++) {
                for (int c = 0; c < width; c++) {
                    string tok = lines[r].Substring(c * 3, 3);
                    if (tok == EmptyToken) continue;
                    if (tok == OriginToken) {
                        model.Tiles.Add(new LayoutTileRef(c, r) { IsOrigin = true });
                        continue;
                    }
                    if (lib.TryMatchToken(tok, out BoxTypeInfo info, out int height)) {
                        model.Tiles.Add(new LayoutTileRef(c, r, info.Token,
                            info.HasHeightWildcard ? height : (int?)null) {
                            BoxTypeRefId = info.IsBuiltin ? null : info.Id,
                        });
                        continue;
                    }
                    if (tryParsePortToken(tok, out char name, out char arrow, out char shape)) {
                        LayoutPortCell cell = new LayoutPortCell {
                            X = c, Y = r, Name = name, Arrow = arrow, Shape = shape,
                        };
                        resolvePortTypeDir(occupied, width, lineCount, cell);
                        model.PortCells.Add(cell);
                        continue;
                    }
                    // Unrecognised token — bail to verbatim passthrough.
                    model.Tiles.Clear();
                    model.PortCells.Clear();
                    model.ParsedOk = false;
                    return model;
                }
            }

            model.Width = width;
            model.Height = lineCount;
            model.ParsedOk = true;
            return model;
        }

        /// Serialise a structured model back to a COI layout string. When the
        /// model is not structured (the modder never opened the grid editor)
        /// the raw source string is returned byte-identical. Returns null when
        /// there is nothing to emit.
        public static string Emit(LayoutModel model, BoxTypeLibrary lib) {
            if (model == null) return null;
            if (!model.IsStructured) return model.RawLayoutStr;
            if ((model.Tiles == null || model.Tiles.Count == 0)
                    && (model.PortCells == null || model.PortCells.Count == 0)) {
                return null;
            }

            // Normalise to a 0-based grid covering every tile + port cell.
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (LayoutTileRef t in model.Tiles) {
                if (t.X < minX) minX = t.X;
                if (t.Y < minY) minY = t.Y;
                if (t.X > maxX) maxX = t.X;
                if (t.Y > maxY) maxY = t.Y;
            }
            if (model.PortCells != null) {
                foreach (LayoutPortCell p in model.PortCells) {
                    if (p.X < minX) minX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }
            if (minX == int.MaxValue) return null;

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            string[,] grid = new string[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    grid[x, y] = EmptyToken;

            foreach (LayoutTileRef t in model.Tiles) {
                grid[t.X - minX, t.Y - minY] = t.IsOrigin ? OriginToken : tileToken(t);
            }
            if (model.PortCells != null) {
                foreach (LayoutPortCell p in model.PortCells) {
                    grid[p.X - minX, p.Y - minY] = portToken(p);
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) sb.Append(grid[x, y]);
                if (y < height - 1) sb.Append('\n');
            }
            return sb.ToString();
        }

        /// Pad jagged rows with whole empty cells ("   ") so every row is the
        /// same length. COI's EntityLayoutParser hard-fails on rows of unequal
        /// length ("Length ... of line ... does not match layout line length"),
        /// and jagged rows arise naturally when a port token widens one row
        /// past the others (base-game SourceLayoutStr values do this too, so a
        /// cloned layout can be jagged without the modder touching it). Rows
        /// are only ever extended, never trimmed. A layout that is already
        /// rectangular is returned unchanged — byte-identical, so untouched
        /// well-formed layouts never churn. One whose rows are not
        /// token-aligned (some length % 3 != 0) is also returned unchanged:
        /// padding cannot repair a malformed token and the verbatim fallback
        /// should keep carrying it as-is.
        public static string PadRectangular(string layoutStr) {
            if (string.IsNullOrEmpty(layoutStr)) {
                return layoutStr;
            }

            string[] lines = layoutStr.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            int lineCount = lines.Length;
            while (lineCount > 0 && lines[lineCount - 1].Length == 0) {
                lineCount--;
            }

            int maxLen = 0;
            bool jagged = false;
            for (int i = 0; i < lineCount; i++) {
                if (lines[i].Length % 3 != 0) {
                    return layoutStr;
                }
                if (lines[i].Length != lines[0].Length) {
                    jagged = true;
                }
                if (lines[i].Length > maxLen) {
                    maxLen = lines[i].Length;
                }
            }
            if (!jagged) {
                return layoutStr;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lineCount; i++) {
                sb.Append(lines[i]);
                sb.Append(' ', maxLen - lines[i].Length);
                if (i < lineCount - 1) {
                    sb.Append('\n');
                }
            }
            return sb.ToString();
        }

        // ---- Token helpers -----------------------------------------------------

        /// Build the 3-char token for a box tile: substitute the per-tile
        /// height digit into the token template's '0' wildcard slot.
        private static string tileToken(LayoutTileRef t) {
            string template = string.IsNullOrEmpty(t.BoxTypeToken) ? "{0}" : t.BoxTypeToken;
            if (template.Length != 3) return "{0}";
            if (template[1] == '0') {
                int h = t.Height ?? 1;
                if (h < 1) h = 1;
                if (h > 9) h = 9; // taller boxes use the 10}/20) families (verbatim path)
                return new string(new[] { template[0], (char)('0' + h), template[2] });
            }
            return template;
        }

        private static string portToken(LayoutPortCell p) {
            // COI accepts the three chars in any order; emit name + shape + arrow
            // (the convention used by the base game's layout strings).
            return new string(new[] { p.Name, p.Shape, p.Arrow });
        }

        /// True when the token is a port token: exactly one A–Z, one direction
        /// arrow, and one other ("shape") char.
        private static bool tryParsePortToken(string tok, out char name, out char arrow, out char shape) {
            name = '\0';
            arrow = '\0';
            shape = '\0';
            for (int i = 0; i < 3; i++) {
                char ch = tok[i];
                if (ch >= 'A' && ch <= 'Z') {
                    if (name != '\0') return false;
                    name = ch;
                } else if (isPortDirection(ch)) {
                    if (arrow != '\0') return false;
                    arrow = ch;
                } else {
                    if (shape != '\0') return false;
                    shape = ch;
                }
            }
            return name != '\0' && arrow != '\0' && shape != '\0';
        }

        private static bool isPortDirection(char c) {
            for (int i = 0; i < PortDirections.Length; i++)
                if (PortDirections[i] == c) return true;
            return false;
        }

        // Resolve display type/direction the way EntityLayoutParser.tryGetPortType
        // does: look at which neighbouring cell holds a box tile.
        private static void resolvePortTypeDir(bool[,] occ, int w, int h, LayoutPortCell p) {
            bool At(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && occ[x, y];
            int x0 = p.X, y0 = p.Y;
            switch (p.Arrow) {
                case '<':
                case '>':
                    if (At(x0 - 1, y0)) {
                        p.Direction = "+X";
                        p.Type = p.Arrow == '>' ? "output" : "input";
                    } else if (At(x0 + 1, y0)) {
                        p.Direction = "-X";
                        p.Type = p.Arrow == '>' ? "input" : "output";
                    }
                    break;
                case '^':
                case 'v':
                    if (At(x0, y0 - 1)) {
                        p.Direction = "+Y";
                        p.Type = p.Arrow == 'v' ? "output" : "input";
                    } else if (At(x0, y0 + 1)) {
                        p.Direction = "-Y";
                        p.Type = p.Arrow == 'v' ? "input" : "output";
                    }
                    break;
                case '+':
                    p.Type = "any";
                    if (At(x0 - 1, y0)) p.Direction = "+X";
                    else if (At(x0 + 1, y0)) p.Direction = "-X";
                    else if (At(x0, y0 - 1)) p.Direction = "+Y";
                    else if (At(x0, y0 + 1)) p.Direction = "-Y";
                    break;
            }
        }

        // ---- Port shape <-> char (used by the UI overlay for add-ports) --------

        public static char ShapeIdToChar(string shapeId) {
            if (string.IsNullOrEmpty(shapeId)) return '#';
            switch (shapeId) {
                case "IoPortShape_FlatConveyor":          return '#';
                case "IoPortShape_LooseMaterialConveyor": return '~';
                case "IoPortShape_MoltenMetalChannel":    return '\'';
                case "IoPortShape_Pipe":                  return '@';
                case "IoPortShape_Shaft":                 return '|';
                default:                                  return '#';
            }
        }

        public static string ShapeCharToId(char shapeChar) {
            switch (shapeChar) {
                case '#':  return "IoPortShape_FlatConveyor";
                case '~':  return "IoPortShape_LooseMaterialConveyor";
                case '\'': return "IoPortShape_MoltenMetalChannel";
                case '@':  return "IoPortShape_Pipe";
                case '|':  return "IoPortShape_Shaft";
                default:   return "IoPortShape_FlatConveyor";
            }
        }

        public static char DirectionToArrow(string direction) {
            switch (direction) {
                case "+X": return '>';
                case "-X": return '<';
                case "+Y": return '^';
                case "-Y": return 'v';
                default:   return '+';
            }
        }

        public static string ArrowToDirection(char arrow) {
            switch (arrow) {
                case '>': return "+X";
                case '<': return "-X";
                case '^': return "+Y";
                case 'v': return "-Y";
                default:  return null;
            }
        }

        // ---- Self test ---------------------------------------------------------

        /// Round-trips a handful of representative layout strings through
        /// TryParse -> (mark structured) -> Emit and checks byte-equality.
        /// Returns null on success, or the first failure message. Wired from
        /// RoundTripTester so it runs alongside the recipe round-trip checks.
        public static string SelfTest() {
            BoxTypeLibrary lib = new BoxTypeLibrary();
            string[] cases = {
                "{1}",
                "{2}{2}{2}\n{2}{2}{2}",
                "[1][1]\n[1][1]",
                "   {3}   \n{3}{3}{3}\n   {3}   ",
                "B@>{6}{6}{6}Y@<",
                " * {1}\n{1}{1}",
            };
            foreach (string c in cases) {
                LayoutModel m = TryParse(c, lib);
                if (!m.ParsedOk) return "parse failed: <" + c + ">";
                m.IsStructured = true;
                string outp = Emit(m, lib);
                if (outp != c) return "round-trip mismatch:\n  in : <" + c + ">\n  out: <" + outp + ">";
            }

            // Jagged rows (a port overflowing the footprint) are padded with
            // whole empty cells — by PadRectangular directly, and by a
            // structured parse -> emit round-trip.
            string jagged = "{1}{1}B@>\n{1}{1}";
            string padded = "{1}{1}B@>\n{1}{1}   ";
            string padOut = PadRectangular(jagged);
            if (padOut != padded) {
                return "PadRectangular mismatch:\n  in : <" + jagged + ">\n  out: <" + padOut + ">";
            }
            LayoutModel jm = TryParse(jagged, lib);
            if (!jm.ParsedOk) return "parse failed on jagged layout: <" + jagged + ">";
            jm.IsStructured = true;
            string jaggedOut = Emit(jm, lib);
            if (jaggedOut != padded) {
                return "jagged round-trip mismatch:\n  in : <" + jagged + ">\n  out: <" + jaggedOut + ">";
            }
            // Already-rectangular and non-token-aligned inputs pass through
            // byte-identical (padding must never introduce churn there).
            if (PadRectangular(padded) != padded) return "PadRectangular churned a rectangular layout";
            if (PadRectangular("{1}{1\n{1}") != "{1}{1\n{1}") return "PadRectangular touched a malformed layout";
            return null;
        }
    }
}
