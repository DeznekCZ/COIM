using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomAssets.Editor.Model;

namespace CustomAssets.Editor.Io {

    /// Writes a PackModel back to disk by SPLICING into each source file rather than
    /// regenerating it. The strategy:
    ///
    ///   1. Group recipes by SourceFile.
    ///   2. For each file:
    ///        - Read its current text from disk (source of truth at save time).
    ///        - For every recipe that was loaded from this file (has SourceStartLine
    ///          / SourceEndLine > 0), build a "replacement plan": delete the original
    ///          line range, insert the canonical render of the (possibly edited) recipe
    ///          at the same position.
    ///        - Apply replacements from BOTTOM to TOP so earlier line numbers stay valid.
    ///        - Append any NEW recipes (SourceStartLine == 0) at end of file.
    ///        - Write the file back.
    ///   3. Recipes whose SourceFile is null (new and not yet assigned) require a
    ///      target file — caller must set SourceFile (e.g. Definitions/recipes.py)
    ///      before save. Save() throws if any unassigned recipes remain.
    ///
    /// Comments, imports, helper functions, build_product_* calls, dependencies(...)
    /// declarations — anything we don't model — pass through untouched because we
    /// only modify the specific line ranges of recognised build_recipe(...) statements.
    ///
    /// Limitations of this first cut:
    ///   - Deletion isn't supported yet. A recipe in the model is always re-emitted;
    ///     to remove a recipe the caller must edit the file by hand or invoke
    ///     DeleteRecipeFromFile separately.
    ///   - IDs are always emitted as string literals ("Product_X"), never as
    ///     Ids.Products.X. Simpler, unambiguous, and always valid — even when the
    ///     original source used a typed reference. Modders who care about typed
    ///     references can re-symbolise by hand.
    public static class PackEmitter {

        /// Write every recipe in the model back to its source file. Throws if any
        /// RecipeDef.SourceFile is null/empty (caller must assign new recipes to a
        /// target file first).
        public static void Save(PackModel model) {
            foreach (RecipeDef def in model.Recipes) {
                if (string.IsNullOrEmpty(def.SourceFile)) {
                    throw new InvalidOperationException(
                        $"Recipe '{def.RecipeId}' has no SourceFile assigned. " +
                        "Assign one before saving (typically a Definitions/*.py path " +
                        "under the pack root).");
                }
            }

            IEnumerable<IGrouping<string, RecipeDef>> byFile =
                model.Recipes.GroupBy(r => r.SourceFile);
            foreach (IGrouping<string, RecipeDef> group in byFile) {
                rewriteFile(group.Key, group.ToList());
            }
        }

        /// Render one recipe as canonical Python. Public so the editor can preview
        /// the text before save (and tests can assert exact output).
        ///
        /// Emission conventions:
        ///   - IDs containing a `.` are written verbatim as typed references
        ///     (e.g. `Ids.Machines.AssemblyElectrified`). IDs without a `.`
        ///     are written as string literals (e.g. `"Product_X"`). This
        ///     heuristic matches the CustomAssets-pack convention: vanilla
        ///     refs use `Ids.*` dotted paths, mod-defined IDs are bare names
        ///     emitted as quoted string literals.
        ///   - Duration emits as `Duration.FromSec(N)` — most idiomatic.
        ///   - Product quantities emit as `Quantity(N)` — matches modder
        ///     convention and is what the loader expects to round-trip.
        ///   - Power emits as a bare int (no `Percent(...)` wrapper) so packs
        ///     that don't import `Percent` from Mafi keep working.
        public static string RenderRecipe(RecipeDef def) {
            StringBuilder sb = new StringBuilder();

            // Leading `#` comment block. Mirrors PackLoader's extractAndAttachComment
            // which captures consecutive comment lines immediately above a recipe.
            // Empty/null Comment emits nothing; multi-line Comments emit one
            // `# <line>` per source newline so the file stays readable. We
            // preserve blank comment lines as bare `#` (no trailing space) so
            // round-trip is exact for recipes with hand-formatted note blocks.
            if (!string.IsNullOrEmpty(def.Comment)) {
                foreach (string line in def.Comment.Split('\n')) {
                    string body = line.TrimEnd('\r');
                    if (body.Length == 0) sb.Append("#\n");
                    else sb.Append("# ").Append(body).Append('\n');
                }
            }

            sb.Append("build_recipe(\n");

            sb.Append("    "); appendString(sb, def.RecipeId);   sb.Append(",\n");
            sb.Append("    "); appendString(sb, def.Name);       sb.Append(",\n");
            sb.Append("    "); appendString(sb, def.Description); sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.MachineId);

            // Optional args, only emit when present.
            if (def.ResearchId != null) {
                sb.Append(",\n    research = "); appendIdRef(sb, def.ResearchId);
            }
            if (def.DurationSeconds.HasValue) {
                sb.Append(",\n    duration = Duration.FromSec(").Append(def.DurationSeconds.Value).Append(")");
            }
            if (def.Ingredients != null && def.Ingredients.Count > 0) {
                sb.Append(",\n    ingredients = ");
                appendProductList(sb, def.Ingredients);
            }
            if (def.Products != null && def.Products.Count > 0) {
                sb.Append(",\n    products = ");
                appendProductList(sb, def.Products);
            }
            if (def.PowerPercent.HasValue) {
                sb.Append(",\n    power = ").Append(def.PowerPercent.Value);
            }

            sb.Append("\n)");
            return sb.ToString();
        }

        // IDs with a dot in them are emitted verbatim (typed references like
        // `Ids.Machines.X`). IDs without a dot are emitted as string literals.
        // Null becomes the Python None literal.
        private static void appendIdRef(StringBuilder sb, string id) {
            if (id == null) { sb.Append("None"); return; }
            if (id.IndexOf('.') >= 0) { sb.Append(id); return; }
            appendString(sb, id);
        }

        // ---- File rewrite ------------------------------------------------------

        private static void rewriteFile(string path, List<RecipeDef> recipes) {
            string[] lines = File.ReadAllLines(path);
            List<string> output = new List<string>(lines);

            // Separate recipes that were loaded from this file (have a line range) from
            // brand-new recipes that need appending. Sort existing ones by start line
            // DESCENDING so splices from bottom to top don't invalidate the line
            // numbers of earlier (smaller-line-number) replacements.
            List<RecipeDef> existing = recipes.Where(r => r.SourceStartLine > 0).ToList();
            List<RecipeDef> appended = recipes.Where(r => r.SourceStartLine <= 0).ToList();
            existing.Sort((a, b) => b.SourceStartLine.CompareTo(a.SourceStartLine));

            foreach (RecipeDef def in existing) {
                // Line numbers are 1-based, inclusive. Convert to 0-based slice indices.
                int startIdx = def.SourceStartLine - 1;
                int endIdx   = def.SourceEndLine   - 1;
                if (startIdx < 0 || endIdx >= output.Count || startIdx > endIdx) {
                    // Defensive: line range is corrupt (file changed under us?). Skip
                    // this recipe rather than corrupt the file. The editor should
                    // surface a "reload required" warning in this case — out of scope
                    // for this first cut.
                    continue;
                }
                output.RemoveRange(startIdx, endIdx - startIdx + 1);
                string rendered = RenderRecipe(def);
                output.InsertRange(startIdx, rendered.Split('\n'));
            }

            if (appended.Count > 0) {
                if (output.Count > 0 && !string.IsNullOrEmpty(output[output.Count - 1])) {
                    output.Add(""); // blank separator before appended block
                }
                foreach (RecipeDef def in appended) {
                    output.AddRange(RenderRecipe(def).Split('\n'));
                    output.Add("");
                }
            }

            File.WriteAllLines(path, output);
        }

        // ---- Render helpers ----------------------------------------------------

        private static void appendString(StringBuilder sb, string value) {
            if (value == null) {
                sb.Append("None");
                return;
            }
            sb.Append('"');
            foreach (char c in value) {
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n");  break;
                    case '\t': sb.Append("\\t");  break;
                    case '\r': sb.Append("\\r");  break;
                    default:   sb.Append(c); break;
                }
            }
            sb.Append('"');
        }

        private static void appendProductList(StringBuilder sb, List<ProductRef> items) {
            sb.Append("[\n");
            for (int i = 0; i < items.Count; i++) {
                ProductRef p = items[i];
                sb.Append("        Product(");
                appendIdRef(sb, p.ProductId);
                // Quantity(N) wrapper matches modder convention. The loader
                // unwraps it back into the int field, so a load → edit → save
                // cycle preserves the same code form.
                sb.Append(", Quantity(").Append(p.Quantity).Append(")");
                if (!string.IsNullOrEmpty(p.Port) && p.Port != "*") {
                    sb.Append(", ");
                    appendString(sb, p.Port);
                }
                sb.Append(")");
                if (i < items.Count - 1) sb.Append(",");
                sb.Append("\n");
            }
            sb.Append("    ]");
        }
    }
}
