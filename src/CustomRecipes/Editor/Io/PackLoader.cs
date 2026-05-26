using System.Collections.Generic;
using System.IO;
using System.Text;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using PythonAPI;
using PythonAPI.Arguments;
using PythonAPI.Expressions;
using PythonAPI.Statements;

namespace CustomAssets.Editor.Io {

    /// Walks a LoadedPack's parsed ASTs and extracts every recognised `build_recipe(...)`
    /// call into a RecipeDef. Unrecognised statements (helper functions, comments,
    /// build_product_* calls, etc.) are left in the source untouched and ignored here —
    /// the round-trip strategy is "rewrite recognised line ranges only", so the loader
    /// just needs to find recognised calls and capture their line ranges.
    ///
    /// Argument extraction is best-effort: any expression we can't reduce to a literal
    /// string / int (e.g. arithmetic, function calls other than Duration(...) /
    /// Percent(...) / Product(...)) is stored as its Path (for Ids.Machines.X) or
    /// "<unparseable>" as a sentinel. The editor's UI should treat such recipes as
    /// read-only or surface them with a warning. This first cut handles the canonical
    /// shapes that 99% of modder code uses.
    public static class PackLoader {

        private const string BuildRecipeFnName     = "build_recipe";
        private const string ProductFnName         = "Product";
        private const string DurationFnName        = "Duration";
        private const string DurationFromSecName   = "Duration.FromSec";
        private const string DurationFromMinName   = "Duration.FromMin";
        private const string PercentFnName         = "Percent";
        private const string QuantityFnName        = "Quantity";
        private const string UnparseableId         = "<unparseable>";

        /// Convert a registry entry to an editable PackModel. Files in the loaded pack
        /// are walked in registration order; recipes are appended to model.Recipes in
        /// the same order the modder wrote them, preserving authorial intent.
        public static PackModel Load(LoadedPack pack) {
            PackModel model = new PackModel(pack.ModId, pack.RootPath);

            foreach (LoadedFile file in pack.Files) {
                // Read the raw source lines once per file so we can extract
                // `#` comment blocks preceding each recipe. The AST already
                // discards comments (Tokenizer converts them to newlines), so
                // capturing modder intent on save/reload round-trips requires
                // going back to the source. We tolerate a read failure here
                // — recipes still load without comments — because a missing
                // file would be far more disruptive than missing notes.
                string[] sourceLines = tryReadLines(file.AbsolutePath);

                foreach (IStatement statement in file.Ast.statements) {
                    if (!(statement is EvaluateStatement ev)) continue;
                    if (!(ev.Expression is CallExpression call)) continue;
                    if (!isCallTo(call, BuildRecipeFnName)) continue;

                    RecipeDef def = parseBuildRecipe(call);
                    def.SourceFile      = file.AbsolutePath;
                    def.SourceStartLine = ev.StartLine;
                    def.SourceEndLine   = ev.EndLine;

                    // Pull comment lines immediately above the statement and
                    // adjust SourceStartLine to point at the FIRST comment line
                    // so the emitter's splice covers comments + statement as
                    // one contiguous block. Without this adjustment, edits to
                    // the comment would write a fresh block while leaving the
                    // old comments stranded above the new build_recipe.
                    if (sourceLines != null) {
                        extractAndAttachComment(def, sourceLines);
                    }

                    model.Recipes.Add(def);
                }
            }

            return model;
        }

        // Walk upwards from def.SourceStartLine collecting consecutive
        // `#`-prefixed lines (leading whitespace is allowed). A blank or
        // non-comment line breaks the chain — that's the standard "comment
        // belongs to the next statement" Python convention. The first
        // collected comment line becomes the new SourceStartLine so the
        // splice in PackEmitter covers comments + statement together.
        // Stripped form (without `#` and at most one leading space) is joined
        // with '\n' and stored on def.Comment.
        private static void extractAndAttachComment(RecipeDef def, string[] sourceLines) {
            if (def.SourceStartLine < 2) return; // nothing above line 1
            int firstStmtIdx = def.SourceStartLine - 1; // 0-based index of statement
            int idx = firstStmtIdx - 1;
            List<string> collected = new List<string>();
            int firstCommentLine = -1;
            while (idx >= 0) {
                string raw = sourceLines[idx];
                string trimmed = raw.TrimStart();
                if (trimmed.Length == 0) break;          // blank breaks chain
                if (!trimmed.StartsWith("#")) break;    // non-comment breaks chain
                // Strip leading '#'; tolerate '# ' (one space after) gracefully.
                string body = trimmed.Substring(1);
                if (body.StartsWith(" ")) body = body.Substring(1);
                collected.Add(body);
                firstCommentLine = idx + 1; // back to 1-based
                idx--;
            }
            if (collected.Count == 0) return;
            collected.Reverse();
            def.Comment = string.Join("\n", collected);
            def.SourceStartLine = firstCommentLine;
        }

        private static string[] tryReadLines(string path) {
            try { return File.ReadAllLines(path, Encoding.UTF8); }
            catch { return null; }
        }

        // ---- build_recipe(...) parsing -----------------------------------------

        private static RecipeDef parseBuildRecipe(CallExpression call) {
            RecipeDef def = new RecipeDef();

            // Positional order for build_recipe: recipeId, name, description, machine,
            // research?, duration?, ingredients?, products?, power?. Walk all args; for
            // positional ones consume the slot at the current index, for named ones
            // resolve by name. Mirrors Python's call-binding rules.
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindNamedArg(def, named.Name, named.Expression);
                } else {
                    bindPositionalArg(def, positional, arg.Expression);
                    positional++;
                }
            }

            return def;
        }

        private static void bindPositionalArg(RecipeDef def, int index, IExpression expr) {
            switch (index) {
                case 0: def.RecipeId    = ExpressionToId(expr);     return;
                case 1: def.Name        = ExpressionToString(expr); return;
                case 2: def.Description = ExpressionToString(expr); return;
                case 3: def.MachineId   = ExpressionToId(expr);     return;
                case 4: def.ResearchId  = ExpressionToIdOrNull(expr); return;
                case 5: def.DurationSeconds = ExpressionToDurationSeconds(expr); return;
                case 6: def.Ingredients = ExpressionToProductList(expr); return;
                case 7: def.Products    = ExpressionToProductList(expr); return;
                case 8: def.PowerPercent = ExpressionToPowerPercent(expr); return;
            }
        }

        private static void bindNamedArg(RecipeDef def, string name, IExpression expr) {
            switch (name) {
                case "recipeId":    def.RecipeId    = ExpressionToId(expr); break;
                case "name":        def.Name        = ExpressionToString(expr); break;
                case "description": def.Description = ExpressionToString(expr); break;
                case "machine":     def.MachineId   = ExpressionToId(expr); break;
                case "research":    def.ResearchId  = ExpressionToIdOrNull(expr); break;
                case "duration":    def.DurationSeconds = ExpressionToDurationSeconds(expr); break;
                case "ingredients": def.Ingredients = ExpressionToProductList(expr); break;
                case "products":    def.Products    = ExpressionToProductList(expr); break;
                case "power":       def.PowerPercent = ExpressionToPowerPercent(expr); break;
                // Unknown name → ignore. Future build_recipe params would need a model
                // bump anyway; silently dropping here is safer than throwing on every
                // experimental modder recipe.
            }
        }

        // ---- Argument-shape helpers --------------------------------------------

        /// String literal, variable name, or property-path expression → canonical string.
        /// Used for IDs (recipeId / machineId / research / Ids.X.Y references).
        public static string ExpressionToId(IExpression expr) {
            switch (expr) {
                case StringConstant s:   return s.Value;
                case VariableExpression v: return v.Path;
                case PropertyExpression p: return p.Path;
                case NoneConst _:        return null;
                // CallExpression — e.g. RecipeProto.ID("foo") or recipe_id("foo"). Pull
                // the first string-constant argument out as the underlying id.
                case CallExpression c:
                    foreach (IArgument a in c.Arguments) {
                        if (a is OrderedArgument && a.Expression is StringConstant ss) {
                            return ss.Value;
                        }
                    }
                    return UnparseableId;
                default: return UnparseableId;
            }
        }

        /// Same as ExpressionToId but treats explicit None / null as a real absent value
        /// rather than UnparseableId. Used for optional ID slots like `research=None`.
        public static string ExpressionToIdOrNull(IExpression expr) {
            if (expr is NoneConst) return null;
            return ExpressionToId(expr);
        }

        public static string ExpressionToString(IExpression expr) {
            if (expr is StringConstant s) return s.Value;
            if (expr is NoneConst) return null;
            return UnparseableId;
        }

        /// Bare int → int. `Quantity(N)` wrapper → N. None / other → null.
        /// Modders typically wrap product counts in `Quantity(...)` per the
        /// CustomAssets API surface, so unwrapping here keeps the editor's
        /// numeric value matching what the modder intended.
        public static int? ExpressionToInt(IExpression expr) {
            if (expr is NumberConstant n && n.Value is int i) return i;
            if (expr is NumberConstant n2 && n2.Value is long l) return (int)l;
            if (expr is NoneConst) return null;
            // Quantity(N) wrapper used in product lists.
            if (expr is CallExpression qcall
                && qcall.Calle is VariableExpression qv
                && qv.Path == QuantityFnName) {
                foreach (IArgument a in qcall.Arguments) {
                    if (a is OrderedArgument && a.Expression is NumberConstant qn
                        && qn.Value is int qi) return qi;
                }
            }
            return null;
        }

        /// `Duration(60)` → 60. `Duration.FromSec(20)` → 20. `Duration.FromMin(3)` → 180.
        /// Bare int (`60`) → 60. None → null. Anything else → null.
        public static int? ExpressionToDurationSeconds(IExpression expr) {
            if (expr is NoneConst) return null;
            if (expr is CallExpression call) {
                // The callee may be a plain name (`Duration`) or a property
                // path (`Duration.FromSec` / `Duration.FromMin`). Branch on the
                // resolved path so all three idiomatic forms round-trip.
                string calleePath = calleeAsPath(call.Calle);
                int? raw = firstIntArg(call);
                if (calleePath == DurationFnName || calleePath == DurationFromSecName)
                    return raw;
                if (calleePath == DurationFromMinName && raw.HasValue)
                    return raw.Value * 60;
                return null;
            }
            return ExpressionToInt(expr);
        }

        /// `Percent(20)` → 20. Bare int (`20`) → 20. None → null.
        public static int? ExpressionToPowerPercent(IExpression expr) {
            if (expr is NoneConst) return null;
            if (expr is CallExpression call && isCallTo(call, PercentFnName)) {
                return firstIntArg(call);
            }
            return ExpressionToInt(expr);
        }

        // ---- AST helpers (shared) ----------------------------------------------

        private static string calleeAsPath(IExpression callee) {
            if (callee is VariableExpression v) return v.Path;
            if (callee is PropertyExpression p) return p.Path;
            return null;
        }

        private static int? firstIntArg(CallExpression call) {
            foreach (IArgument a in call.Arguments) {
                if (a is OrderedArgument && a.Expression is NumberConstant n
                    && n.Value is int i) return i;
            }
            return null;
        }

        /// `[Product(...), Product(...), ...]` → List<ProductRef>. `None` or `[]` → empty.
        public static List<ProductRef> ExpressionToProductList(IExpression expr) {
            List<ProductRef> result = new List<ProductRef>();
            if (expr is NoneConst) return result;
            if (!(expr is ListExpression list)) return result;
            foreach (IExpression item in list.Items) {
                if (item is CallExpression productCall && isCallTo(productCall, ProductFnName)) {
                    result.Add(parseProductCall(productCall));
                }
            }
            return result;
        }

        /// Product(product, quantity, port="*").
        private static ProductRef parseProductCall(CallExpression call) {
            ProductRef pr = new ProductRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "product":  pr.ProductId = ExpressionToId(named.Expression); break;
                        case "quantity": pr.Quantity  = ExpressionToInt(named.Expression) ?? 0; break;
                        case "port":     pr.Port      = ExpressionToString(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: pr.ProductId = ExpressionToId(arg.Expression); break;
                        case 1: pr.Quantity  = ExpressionToInt(arg.Expression) ?? 0; break;
                        case 2: pr.Port      = ExpressionToString(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return pr;
        }

        // ---- AST predicates ----------------------------------------------------

        private static bool isCallTo(CallExpression call, string fnName) {
            if (call.Calle is VariableExpression v) return v.Path == fnName;
            return false;
        }
    }
}
