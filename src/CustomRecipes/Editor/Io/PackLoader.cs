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
    /// build_product_* calls, etc.) are left in the source untouched and ignored here â€”
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

            // Run AST-level validation up front so the editor can surface
            // use-before-definition warnings alongside the rest of the
            // model (the def list, etc.). Validation looks only at the
            // parsed AST and the file's source layout - independent of
            // what we extract into Definitions below.
            model.Issues = PackValidator.Validate(pack);

            foreach (LoadedFile file in pack.Files) {
                // Read the raw source lines once per file so we can extract
                // `#` comment blocks preceding each recipe AND the condition
                // text from `if/elif/else` headers when a recipe is nested
                // inside a conditional. The AST already discards comments and
                // doesn't carry a stringified form of expression trees, so
                // capturing modder intent on save/reload round-trips requires
                // going back to the source. We tolerate a read failure here
                // â€” recipes still load without comments/conditions â€” because
                // a missing file would be far more disruptive than missing
                // notes.
                string[] sourceLines = tryReadLines(file.AbsolutePath);

                // Build the per-file variable-name â†’ resolved-id map as we
                // walk. Top-level Python evaluates top-to-bottom, so an
                // assignment must appear before any reference to its name â€”
                // walking in source order is enough; no second pass needed.
                // Every Def captured from this file gets a SHARED reference
                // to this map (assigned in attachSourceLocation below) so
                // the emitter and pickers can consult it without rebuilding
                // it from PackModel each time.
                var fileVariables = new System.Collections.Generic.Dictionary<string, string>(
                    System.StringComparer.Ordinal);
                walkStatements(file.Ast.statements, file.AbsolutePath, sourceLines,
                               conditionChain: null, scopeKey: "top", model, fileVariables);
            }

            // Backfill the structured layout view on every layout-bearing def.
            // Built from the box-type library (built-ins + this pack's
            // define_box_type calls) so custom tokens parse. The model is kept
            // NON-structured (IsStructured=false) so an untouched layout
            // round-trips its raw string byte-identically until the modder
            // edits it in the visual editor.
            BoxTypeLibrary boxLib = BoxTypeLibrary.BuildFor(model);
            foreach (DefBase d in model.Definitions) {
                if (!(d is ILayoutHostDef host)) continue;
                LayoutModel layout = LayoutCodec.TryParse(host.LayoutSourceStr, boxLib);
                // Bind the structured port list to the def's own port list so
                // grid edits flow through the existing ports=/add_ports= path.
                if (host.Ports != null) layout.Ports = host.Ports;
                host.Layout = layout;
            }

            return model;
        }

        // Walk a Block.statements list collecting every build_*/add_* call we
        // see. Recipes get full typed RecipeDef objects; everything else gets
        // an UnknownDef placeholder so the tree still shows the modder what's
        // in the file, even before each definition kind has a typed editor.
        // Recurses into IfStatement bodies so definitions nested inside a
        // conditional are captured too â€” each gets the surrounding condition
        // text via the chain (joined with " and " when multiple if-levels
        // stack).
        private static void walkStatements(
                System.Collections.Generic.List<IStatement> statements,
                string sourceFile,
                string[] sourceLines,
                string conditionChain,
                string scopeKey,
                PackModel model,
                System.Collections.Generic.Dictionary<string, string> fileVariables) {
            // Run index for this scope. Increments past every if-chain so
            // defs landing after it form a fresh draggable group, with the
            // clause structure acting as a fixed boundary between them.
            int runIndex = 0;
            // Did the previous statement at this scope end a run? An
            // if-chain advances the run index, but ONLY if the surrounding
            // scope has more def-statements after it — otherwise we end up
            // with an empty trailing run. We bump runIndex lazily, right
            // before the next captured def at this scope.
            bool pendingRunBump = false;
            foreach (IStatement statement in statements) {
                // Name-binding assignments like `researchWoodgass =
                // build_research(...)` are AssignmentStatement nodes in the
                // AST, not EvaluateStatement. We treat them as if they were
                // bare calls because the captured RHS expression is what the
                // editor cares about â€” the variable name is just a Python-
                // level handle that lets later code reference the result.
                // The line range of the assignment IS the call's line range
                // for our purposes (Lexer sets StartLine to the assignment's
                // first token and EndLine to the last non-trivial token of
                // the RHS expression).
                if (statement is AssignmentStatement asn
                    && asn.Value is CallExpression asnCall) {
                    string asnCallName = calleeAsPath(asnCall.Calle);
                    if (pendingRunBump) { runIndex++; pendingRunBump = false; }
                    // Capture the Def first, then record the binding from
                    // variable-name â†’ resolved id. handleApiCall returns the
                    // added Def so we can read its primary id and stash it
                    // in fileVariables for later references in this file to
                    // resolve through.
                    DefBase added = handleApiCall(asnCallName, asnCall,
                        asn.StartLine, asn.EndLine,
                        sourceFile, sourceLines, conditionChain, scopeKey, runIndex, model, fileVariables);
                    if (added != null && !string.IsNullOrEmpty(asn.Name)) {
                        // Pin the variable name onto the def so the emitter
                        // can write the assignment back. Without this the
                        // saved file loses the `<var> = ` prefix and any
                        // downstream `material = filter_media_mat` style
                        // reference breaks on the next pack load.
                        added.VariableName = asn.Name;
                        if (!string.IsNullOrEmpty(added.DisplayId)) {
                            fileVariables[asn.Name] = added.DisplayId;
                        }
                    }
                    continue;
                }

                if (statement is EvaluateStatement ev
                    && ev.Expression is CallExpression call) {
                    string callName = calleeAsPath(call.Calle);
                    if (pendingRunBump) { runIndex++; pendingRunBump = false; }
                    handleApiCall(callName, call, ev.StartLine, ev.EndLine,
                                  sourceFile, sourceLines, conditionChain, scopeKey, runIndex, model, fileVariables);
                    continue;
                }
                // Anything else (function defs, dependencies, helper calls)
                // is left untouched â€” the tree only surfaces recognised
                // definition statements.

                if (statement is IfStatement ifs) {
                    // The lexer collapses an if/elif/else chain into the LAST
                    // clause's IfStatement, with earlier clauses linked via
                    // Parent (see Lexer.cs case PythonTokens.elsep/elif â€”
                    // tree.statements.RemoveAt then ParseElse/ParseElIf adds
                    // a fresh IfStatement that holds the previous one as
                    // Parent). So `tree.statements` only contains the trailing
                    // clause. To surface recipes in EVERY clause of an if/
                    // elif/else chain we walk the Parent chain explicitly.
                    //
                    // The condition text for each clause is read verbatim from
                    // its source header line â€” re-rendering the IExpression
                    // tree would require a full pretty-printer for every
                    // expression type (and+or, comparison, calls, etc.), far
                    // more work than reading the original characters.
                    IfStatement clause = ifs;
                    while (clause != null) {
                        string thisClause = extractConditionFromHeader(sourceLines, clause);
                        string childChain = composeConditionChain(conditionChain, thisClause);
                        if (clause.Block != null) {
                            // Each clause body gets its own scope keyed off
                            // the clause header line so drag-reorder stays
                            // within the clause. AST line numbers are stable
                            // for the duration of one load+session â€” the
                            // emitter rewrites by SourceStartLine, not by
                            // scope key, so the key only needs to be unique
                            // among scopes in the same file.
                            string clauseScope = "clause:" + clause.StartLine;
                            walkStatements(clause.Block.statements, sourceFile, sourceLines,
                                           childChain, clauseScope, model, fileVariables);
                        }
                        clause = clause.Parent;
                    }
                    // An if-chain at this scope acts as a run boundary: defs
                    // appearing AFTER the chain belong to a new draggable
                    // group, so subsequent captures bump the run index.
                    pendingRunBump = true;
                }
            }
        }

        // Pull the condition expression out of the if/elif/else header line in
        // the source file. Examples:
        //   "    if context.is_dlc_purchased('X'):"     â†’ "context.is_dlc_purchased('X')"
        //   "elif tier > 2:"                            â†’ "tier > 2"
        //   "else:"                                     â†’ "(else)"
        // The "(else)" sentinel is intentionally parenthesised so it's clear
        // it's not a Python expression â€” modders read it as "this recipe
        // lives in the else branch", not as a literal condition. Returns
        // null when sourceLines is unavailable or the header can't be parsed;
        // composeConditionChain treats null/"" as "no contribution".
        private static string extractConditionFromHeader(string[] sourceLines, IfStatement ifs) {
            if (sourceLines == null) return null;
            if (ifs.StartLine < 1 || ifs.StartLine > sourceLines.Length) return null;
            string raw = sourceLines[ifs.StartLine - 1];
            string trimmed = raw.TrimStart().TrimEnd();
            string body;
            if      (trimmed.StartsWith("elif "))    body = trimmed.Substring(5);
            else if (trimmed.StartsWith("if "))      body = trimmed.Substring(3);
            else if (trimmed.StartsWith("else"))     return "(else)";
            else return null;
            int colon = body.LastIndexOf(':');
            // Use LastIndexOf so `if d['key']:` still trims correctly. The
            // condition's content can't end with a `:` itself (would be a
            // syntax error), so the rightmost `:` is always the clause
            // terminator. Whatever's after it (single-line body, comment) is
            // dropped â€” the editor cares only about the condition itself.
            if (colon >= 0) body = body.Substring(0, colon);
            return body.Trim();
        }

        // Join nested condition clauses with " and ". The user-visible chain
        // for nested `if`s reads naturally as a single boolean expression
        // (with "(else)" appearing literally when an else clause is in the
        // chain â€” modders see the structural context even if the joined
        // string isn't valid Python). Null/empty inner contributes nothing.
        private static string composeConditionChain(string outer, string inner) {
            if (string.IsNullOrEmpty(inner)) return outer;
            if (string.IsNullOrEmpty(outer)) return inner;
            return outer + " and " + inner;
        }

        // Walk upwards from def.SourceStartLine collecting consecutive
        // `#`-prefixed lines (leading whitespace is allowed). A blank or
        // non-comment line breaks the chain â€” that's the standard "comment
        // belongs to the next statement" Python convention. The first
        // collected comment line becomes the new SourceStartLine so the
        // splice in PackEmitter covers comments + statement together.
        // Stripped form (without `#` and at most one leading space) is joined
        // with '\n' and stored on def.Comment.
        private static void extractAndAttachComment(DefBase def, string[] sourceLines) {
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

        // Join the source lines from `startLine` to `endLine` (both 1-based,
        // inclusive) into a single string with '\n' separators. Used to
        // capture the verbatim text of a build_*/add_* call so the raw-edit
        // form in the editor can show it and so PackEmitter can splice an
        // edited version back at the same line range. Returns "" on bad
        // ranges so callers don't have to null-check before assigning.
        private static string readSourceRange(string[] sourceLines, int startLine, int endLine) {
            if (sourceLines == null) return "";
            if (startLine < 1 || endLine < startLine || endLine > sourceLines.Length) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = startLine - 1; i < endLine; i++) {
                if (i > startLine - 1) sb.Append('\n');
                sb.Append(sourceLines[i]);
            }
            return sb.ToString();
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
                // Unknown name â†’ ignore. Future build_recipe params would need a model
                // bump anyway; silently dropping here is safer than throwing on every
                // experimental modder recipe.
            }
        }

        // ---- Argument-shape helpers --------------------------------------------

        /// String literal, variable name, or property-path expression â†’ canonical string.
        /// Used for IDs (recipeId / machineId / research / Ids.X.Y references).
        public static string ExpressionToId(IExpression expr) {
            switch (expr) {
                case StringConstant s:   return s.Value;
                case VariableExpression v: return v.Path;
                case PropertyExpression p: return p.Path;
                case NoneConst _:        return null;
                // CallExpression â€” e.g. RecipeProto.ID("foo") or recipe_id("foo"). Pull
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

        /// Icon / texture / path-style argument. Accepts both quoted string
        /// literals ("Assets/MyPack/foo.png") and dotted typed-ref expressions
        /// (Assets.Base.Bridges.Icons.CableStayed4_svg). The dotted form is
        /// rendered back from <see cref="renderExpressionAsText"/>, so emitter
        /// + AssetPathPicker can keep the modder's choice on round-trips. None
        /// passes through as null so the picker shows "(no asset)".
        public static string ExpressionToIconPath(IExpression expr) {
            if (expr is StringConstant s)         return s.Value;
            if (expr is NoneConst)                return null;
            if (expr is PropertyExpression p)     return p.Path;
            if (expr is VariableExpression v)     return v.Path;
            // Inline `add_texture("path")` is the common pattern for icon=
            // args in build_product_* calls. Pull the path out of the first
            // string argument so the editor sees the literal path. The
            // emitter autodetects whether to re-wrap on save by scanning
            // the model for a matching <see cref="TextureDef"/>; nothing
            // here needs to track the original wrap-or-not choice.
            if (expr is CallExpression call) {
                string callee = calleeAsPath(call.Calle);
                if (callee == "add_texture"
                        && call.Arguments != null
                        && call.Arguments.Count >= 1
                        && call.Arguments[0].Expression is StringConstant addTexArg) {
                    return addTexArg.Value;
                }
            }
            return UnparseableId;
        }

        /// Bare int â†’ int. `Quantity(N)` wrapper â†’ N. None / other â†’ null.
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

        /// `Duration(60)` â†’ 60. `Duration.FromSec(20)` â†’ 20. `Duration.FromMin(3)` â†’ 180.
        /// Bare int (`60`) â†’ 60. None â†’ null. Anything else â†’ null.
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

        /// `Percent(20)` â†’ 20. Bare int (`20`) â†’ 20. None â†’ null.
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

        /// `[Product(...), Product(...), ...]` â†’ List<ProductRef>. `None` or `[]` â†’ empty.
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

        // Per-call dispatch shared by both EvaluateStatement and
        // AssignmentStatement paths in walkStatements. Recipes route to the
        // typed RecipeDef; other recognised API calls go through
        // tryParseTypedDef for per-kind typed Defs; anything else matching
        // build_*/add_* falls back to UnknownDef + raw-source editing.
        //
        // `startLine` and `endLine` are passed in rather than read off the
        // statement so the same code path works whether the AST node is an
        // EvaluateStatement (bare call) or an AssignmentStatement (name-
        // binding form like `researchX = build_research(...)`). The line
        // numbers from either node delimit the same character range we
        // need to splice on save.
        private static DefBase handleApiCall(
                string callName,
                CallExpression call,
                int startLine,
                int endLine,
                string sourceFile,
                string[] sourceLines,
                string conditionChain,
                string scopeKey,
                int runIndex,
                PackModel model,
                System.Collections.Generic.Dictionary<string, string> fileVariables) {
            if (callName == BuildRecipeFnName) {
                RecipeDef def = parseBuildRecipe(call);
                attachSourceLocation(def, startLine, endLine, sourceFile, conditionChain, scopeKey, runIndex);
                def.SourceFileVariables = fileVariables;
                if (sourceLines != null) extractAndAttachComment(def, sourceLines);
                model.Definitions.Add(def);
                return def;
            }

            DefBase typed = tryParseTypedDef(callName, call);
            if (typed != null) {
                attachSourceLocation(typed, startLine, endLine, sourceFile, conditionChain, scopeKey, runIndex);
                typed.SourceFileVariables = fileVariables;
                if (sourceLines != null) extractAndAttachComment(typed, sourceLines);
                model.Definitions.Add(typed);
                return typed;
            }

            if (isCustomAssetsApiCall(callName)) {
                UnknownDef other = new UnknownDef {
                    CallName   = callName,
                    CapturedId = extractPrimaryId(call)
                };
                attachSourceLocation(other, startLine, endLine, sourceFile, conditionChain, scopeKey, runIndex);
                other.SourceFileVariables = fileVariables;
                if (sourceLines != null) {
                    extractAndAttachComment(other, sourceLines);
                    other.RawSource = readSourceRange(sourceLines, startLine, endLine);
                }
                model.Definitions.Add(other);
                return other;
            }
            return null;
        }

        // Dispatch by Python call name to the per-kind typed-Def parser.
        // Returns null when the call isn't one of the recognised typed kinds
        // â€” caller falls back to UnknownDef (raw-source editing) for those.
        // The recognised kinds match the most common API calls (see frequency
        // survey in the editor's design notes); product/asset kinds are
        // added incrementally.
        private static DefBase tryParseTypedDef(string callName, CallExpression call) {
            switch (callName) {
                case "build_research":      return parseBuildResearch(call);
                case "add_unlock_recipe":   return parseUnlockRecipe(call);
                case "add_unlock_product":  return parseUnlockProduct(call);
                case "add_unlock_machine":  return parseUnlockMachine(call);
                case "build_product_loose": return parseProductLoose(call);
                case "build_product_fluid": return parseProductFluid(call);
                case "build_product_unit":  return parseProductUnit(call);
                case "add_texture":         return parseTexture(call);
                case "add_loose_product_material": return parseMaterialLoose(call);
                case "add_prefab_box":      return parsePrefabBox(call);
                case "add_unit_prefab":     return parseUnitPrefab(call);
                case "add_texture_material":return parseTextureMaterial(call);
                case "add_toolbar_category":return parseToolbarCategory(call);
                case "build_generator":     return parseGenerator(call);
                case "edit_recipe":         return parseEditRecipe(call);
                case "edit_machine_ports":  return parseEditMachinePorts(call);
                case "build_machine":       return parseBuildMachine(call);
                case "build_housing":       return parseBuildHousing(call);
                case "build_settlement_decoration": return parseSettlementDecoration(call);
                case "build_settlement_food":       return parseSettlementFood(call);
                case "build_settlement_isp":        return parseSettlementIsp(call);
                case "build_hospital":              return parseHospital(call);
                case "build_mine_tower":            return parseMineTower(call);
                case "build_research_lab":          return parseResearchLab(call);
                case "build_nuclear_reactor":       return parseNuclearReactor(call);
                case "edit_nuclear_reactor_fuels":  return parseEditNuclearReactorFuels(call);
                case "edit_nuclear_reactor_fluids": return parseEditNuclearReactorFluids(call);
                case "edit_nuclear_reactor_enrichment": return parseEditNuclearReactorEnrichment(call);
                case "edit_nuclear_reactor_ports":  return parseEditNuclearReactorPorts(call);
                case "define_box_type":             return parseBoxType(call);
                case "layout_token":                return parseBoxType(call);
            }
            return null;
        }

        // define_box_type(boxTypeId, token, heightFrom, heightTo, constraint,
        //     surface, terrainMaterial, isRamp) — registers a reusable custom
        // tile type for the layout editor + runtime layout parser.
        private static BoxTypeDef parseBoxType(CallExpression call) {
            var def = new BoxTypeDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindBoxTypeNamed(def, na.Name, na.Expression);
                else bindBoxTypePositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindBoxTypeNamed(BoxTypeDef def, string name, IExpression expr) {
            switch (name) {
                case "boxTypeId":       def.BoxTypeId         = ExpressionToId(expr); break;
                case "token":           def.Token             = ExpressionToString(expr); break;
                case "heightFrom":      def.HeightFrom        = ExpressionToInt(expr) ?? 0; break;
                case "heightTo":        def.HeightTo          = ExpressionToInt(expr); break;
                case "constraint":      def.Constraint        = ExpressionToString(expr); break;
                case "surface":         def.SurfaceId         = ExpressionToIdOrNull(expr); break;
                case "terrainMaterial": def.TerrainMaterialId = ExpressionToIdOrNull(expr); break;
                case "isRamp":          def.IsRamp            = ExpressionToBool(expr); break;
            }
        }
        private static void bindBoxTypePositional(BoxTypeDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.BoxTypeId         = ExpressionToId(expr); break;
                case 1: def.Token             = ExpressionToString(expr); break;
                case 2: def.HeightFrom        = ExpressionToInt(expr) ?? 0; break;
                case 3: def.HeightTo          = ExpressionToInt(expr); break;
                case 4: def.Constraint        = ExpressionToString(expr); break;
                case 5: def.SurfaceId         = ExpressionToIdOrNull(expr); break;
                case 6: def.TerrainMaterialId = ExpressionToIdOrNull(expr); break;
                case 7: def.IsRamp            = ExpressionToBool(expr); break;
            }
        }

        // build_settlement_decoration / _food / _isp / hospital / mine_tower /
        // research_lab / nuclear_reactor — same per-arg dispatch pattern as
        // build_housing. Each parser owns its named-arg switch; positional
        // dispatch falls through to the named binder by reading the
        // constructor's argument-name array via the same index.
        private static SettlementDecorationDef parseSettlementDecoration(CallExpression call) {
            var def = new SettlementDecorationDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindSettlementDecorationNamed(def, na.Name, na.Expression);
                else bindSettlementDecorationPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindSettlementDecorationNamed(SettlementDecorationDef def, string name, IExpression expr) {
            switch (name) {
                case "decorationId":  def.DecorationId    = ExpressionToId(expr); break;
                case "source":        def.SourceId        = ExpressionToId(expr); break;
                case "name":          def.Name            = ExpressionToString(expr); break;
                case "description":   def.Description     = ExpressionToString(expr); break;
                case "upointsBonus":  def.UpointsBonus    = ExpressionToInt(expr); break;
                case "bonusRange":    def.BonusRange      = ExpressionToInt(expr); break;
                case "layout_str":    def.LayoutSourceStr = ExpressionToString(expr); break;
                case "research":      def.ResearchId      = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":  def.LockedOnInit    = ExpressionToBool(expr); break;
            }
        }
        private static void bindSettlementDecorationPositional(SettlementDecorationDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.DecorationId = ExpressionToId(expr); break;
                case 1: def.SourceId     = ExpressionToId(expr); break;
                case 2: def.Name         = ExpressionToString(expr); break;
                case 3: def.Description  = ExpressionToString(expr); break;
                case 4: def.UpointsBonus = ExpressionToInt(expr); break;
                case 5: def.BonusRange   = ExpressionToInt(expr); break;
                case 6: def.ResearchId   = ExpressionToIdOrNull(expr); break;
                case 7: def.LockedOnInit = ExpressionToBool(expr); break;
            }
        }

        private static SettlementFoodDef parseSettlementFood(CallExpression call) {
            var def = new SettlementFoodDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindSettlementFoodNamed(def, na.Name, na.Expression);
                else bindSettlementFoodPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindSettlementFoodNamed(SettlementFoodDef def, string name, IExpression expr) {
            switch (name) {
                case "foodModuleId":      def.FoodModuleId      = ExpressionToId(expr); break;
                case "source":            def.SourceId          = ExpressionToId(expr); break;
                case "name":              def.Name              = ExpressionToString(expr); break;
                case "description":       def.Description       = ExpressionToString(expr); break;
                case "buffersCount":      def.BuffersCount      = ExpressionToInt(expr); break;
                case "capacityPerBuffer": def.CapacityPerBuffer = ExpressionToInt(expr); break;
                case "layout_str":        def.LayoutSourceStr   = ExpressionToString(expr); break;
                case "research":          def.ResearchId        = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":      def.LockedOnInit      = ExpressionToBool(expr); break;
            }
        }
        private static void bindSettlementFoodPositional(SettlementFoodDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.FoodModuleId      = ExpressionToId(expr); break;
                case 1: def.SourceId          = ExpressionToId(expr); break;
                case 2: def.Name              = ExpressionToString(expr); break;
                case 3: def.Description       = ExpressionToString(expr); break;
                case 4: def.BuffersCount      = ExpressionToInt(expr); break;
                case 5: def.CapacityPerBuffer = ExpressionToInt(expr); break;
                case 6: def.ResearchId        = ExpressionToIdOrNull(expr); break;
                case 7: def.LockedOnInit      = ExpressionToBool(expr); break;
            }
        }

        private static SettlementIspDef parseSettlementIsp(CallExpression call) {
            var def = new SettlementIspDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindSettlementIspNamed(def, na.Name, na.Expression);
                else bindSettlementIspPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindSettlementIspNamed(SettlementIspDef def, string name, IExpression expr) {
            switch (name) {
                case "ispModuleId":           def.IspModuleId           = ExpressionToId(expr); break;
                case "source":                def.SourceId              = ExpressionToId(expr); break;
                case "name":                  def.Name                  = ExpressionToString(expr); break;
                case "description":           def.Description           = ExpressionToString(expr); break;
                case "computingPer100Pops":   def.ComputingPer100Pops   = ExpressionToInt(expr); break;
                case "electricityConsumedKw": def.ElectricityConsumedKw = ExpressionToInt(expr); break;
                case "layout_str":            def.LayoutSourceStr       = ExpressionToString(expr); break;
                case "research":              def.ResearchId            = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":          def.LockedOnInit          = ExpressionToBool(expr); break;
            }
        }
        private static void bindSettlementIspPositional(SettlementIspDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.IspModuleId           = ExpressionToId(expr); break;
                case 1: def.SourceId              = ExpressionToId(expr); break;
                case 2: def.Name                  = ExpressionToString(expr); break;
                case 3: def.Description           = ExpressionToString(expr); break;
                case 4: def.ComputingPer100Pops   = ExpressionToInt(expr); break;
                case 5: def.ElectricityConsumedKw = ExpressionToInt(expr); break;
                case 6: def.ResearchId            = ExpressionToIdOrNull(expr); break;
                case 7: def.LockedOnInit          = ExpressionToBool(expr); break;
            }
        }

        private static HospitalDef parseHospital(CallExpression call) {
            var def = new HospitalDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindHospitalNamed(def, na.Name, na.Expression);
                else bindHospitalPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindHospitalNamed(HospitalDef def, string name, IExpression expr) {
            switch (name) {
                case "hospitalId":        def.HospitalId        = ExpressionToId(expr); break;
                case "source":            def.SourceId          = ExpressionToId(expr); break;
                case "name":              def.Name              = ExpressionToString(expr); break;
                case "description":       def.Description       = ExpressionToString(expr); break;
                case "powerRequiredKw":   def.PowerRequiredKw   = ExpressionToInt(expr); break;
                case "buffersCount":      def.BuffersCount      = ExpressionToInt(expr); break;
                case "capacityPerBuffer": def.CapacityPerBuffer = ExpressionToInt(expr); break;
                case "suppliesPerHundredPopsPerMonth":
                    def.SuppliesPerHundredPopsPerMonth = ExpressionToInt(expr); break;
                case "layout_str":        def.LayoutSourceStr   = ExpressionToString(expr); break;
                case "research":          def.ResearchId        = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":      def.LockedOnInit      = ExpressionToBool(expr); break;
            }
        }
        private static void bindHospitalPositional(HospitalDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.HospitalId        = ExpressionToId(expr); break;
                case 1: def.SourceId          = ExpressionToId(expr); break;
                case 2: def.Name              = ExpressionToString(expr); break;
                case 3: def.Description       = ExpressionToString(expr); break;
                case 4: def.PowerRequiredKw   = ExpressionToInt(expr); break;
                case 5: def.BuffersCount      = ExpressionToInt(expr); break;
                case 6: def.CapacityPerBuffer = ExpressionToInt(expr); break;
                case 7: def.SuppliesPerHundredPopsPerMonth = ExpressionToInt(expr); break;
                case 8: def.ResearchId        = ExpressionToIdOrNull(expr); break;
                case 9: def.LockedOnInit      = ExpressionToBool(expr); break;
            }
        }

        private static MineTowerDef parseMineTower(CallExpression call) {
            var def = new MineTowerDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindMineTowerNamed(def, na.Name, na.Expression);
                else bindMineTowerPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindMineTowerNamed(MineTowerDef def, string name, IExpression expr) {
            switch (name) {
                case "mineTowerId":  def.MineTowerId     = ExpressionToId(expr); break;
                case "source":       def.SourceId        = ExpressionToId(expr); break;
                case "name":         def.Name            = ExpressionToString(expr); break;
                case "description":  def.Description     = ExpressionToString(expr); break;
                case "layout_str":   def.LayoutSourceStr = ExpressionToString(expr); break;
                case "research":     def.ResearchId      = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit": def.LockedOnInit    = ExpressionToBool(expr); break;
            }
        }
        private static void bindMineTowerPositional(MineTowerDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.MineTowerId  = ExpressionToId(expr); break;
                case 1: def.SourceId     = ExpressionToId(expr); break;
                case 2: def.Name         = ExpressionToString(expr); break;
                case 3: def.Description  = ExpressionToString(expr); break;
                case 4: def.ResearchId   = ExpressionToIdOrNull(expr); break;
                case 5: def.LockedOnInit = ExpressionToBool(expr); break;
            }
        }

        private static ResearchLabDef parseResearchLab(CallExpression call) {
            var def = new ResearchLabDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindResearchLabNamed(def, na.Name, na.Expression);
                else bindResearchLabPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindResearchLabNamed(ResearchLabDef def, string name, IExpression expr) {
            switch (name) {
                case "researchLabId":              def.ResearchLabId            = ExpressionToId(expr); break;
                case "source":                     def.SourceId                 = ExpressionToId(expr); break;
                case "name":                       def.Name                     = ExpressionToString(expr); break;
                case "description":                def.Description              = ExpressionToString(expr); break;
                case "electricityConsumedKw":      def.ElectricityConsumedKw    = ExpressionToInt(expr); break;
                case "computingConsumed":          def.ComputingConsumed        = ExpressionToInt(expr); break;
                case "durationForRecipeSeconds":   def.DurationForRecipeSeconds = ExpressionToInt(expr); break;
                case "sciencePerRecipe":           def.SciencePerRecipe         = ExpressionToInt(expr); break;
                case "unityMonthlyCost":           def.UnityMonthlyCost         = ExpressionToInt(expr); break;
                case "add_ports":                  def.AddPorts                 = ExpressionToPortList(expr); break;
                case "layout_str":                 def.LayoutSourceStr          = ExpressionToString(expr); break;
                case "research":                   def.ResearchId               = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":               def.LockedOnInit             = ExpressionToBool(expr); break;
            }
        }
        private static void bindResearchLabPositional(ResearchLabDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.ResearchLabId            = ExpressionToId(expr); break;
                case 1: def.SourceId                 = ExpressionToId(expr); break;
                case 2: def.Name                     = ExpressionToString(expr); break;
                case 3: def.Description              = ExpressionToString(expr); break;
                case 4: def.ElectricityConsumedKw    = ExpressionToInt(expr); break;
                case 5: def.ComputingConsumed        = ExpressionToInt(expr); break;
                case 6: def.DurationForRecipeSeconds = ExpressionToInt(expr); break;
                case 7: def.SciencePerRecipe         = ExpressionToInt(expr); break;
                case 8: def.UnityMonthlyCost         = ExpressionToInt(expr); break;
                case 9: def.ResearchId               = ExpressionToIdOrNull(expr); break;
                case 10: def.LockedOnInit            = ExpressionToBool(expr); break;
            }
        }

        // edit_nuclear_reactor_fuels(reactor, add_fuels=[FuelPair(...)])
        private static EditNuclearReactorFuelsDef parseEditNuclearReactorFuels(CallExpression call) {
            var def = new EditNuclearReactorFuelsDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "reactor":   def.ReactorId = ExpressionToId(named.Expression); break;
                        case "add_fuels": def.AddFuels  = ExpressionToFuelPairList(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ReactorId = ExpressionToId(arg.Expression); break;
                        case 1: def.AddFuels  = ExpressionToFuelPairList(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // edit_nuclear_reactor_fluids(reactor, coolantIn, coolantOut,
        //     coolantInPort, coolantOutPort, coolantInPortShape,
        //     coolantOutPortShape, waterInProduct, waterInQuantity,
        //     steamOutProduct, steamOutQuantity, waterInPorts,
        //     steamOutPorts)
        // Same nullable-inherit semantics as NuclearReactorDef's fluid
        // block — every field defaults null → leave the target reactor's
        // value alone.
        private static EditNuclearReactorFluidsDef parseEditNuclearReactorFluids(CallExpression call) {
            var def = new EditNuclearReactorFluidsDef();
            foreach (IArgument arg in call.Arguments) {
                if (!(arg is NamedArgument named)) continue;
                switch (named.Name) {
                    case "reactor":             def.ReactorId            = ExpressionToId(named.Expression); break;
                    case "coolantIn":           def.CoolantInId          = ExpressionToIdOrNull(named.Expression); break;
                    case "coolantOut":          def.CoolantOutId         = ExpressionToIdOrNull(named.Expression); break;
                    case "coolantInPort":       def.CoolantInPort        = ExpressionToString(named.Expression); break;
                    case "coolantOutPort":      def.CoolantOutPort       = ExpressionToString(named.Expression); break;
                    case "coolantInPortShape":  def.CoolantInPortShape   = ExpressionToString(named.Expression); break;
                    case "coolantOutPortShape": def.CoolantOutPortShape  = ExpressionToString(named.Expression); break;
                    case "waterInProduct":      def.WaterInProductId     = ExpressionToIdOrNull(named.Expression); break;
                    case "waterInQuantity":     def.WaterInQuantity      = ExpressionToInt(named.Expression); break;
                    case "steamOutProduct":     def.SteamOutProductId    = ExpressionToIdOrNull(named.Expression); break;
                    case "steamOutQuantity":    def.SteamOutQuantity     = ExpressionToInt(named.Expression); break;
                    case "waterInPorts":        def.WaterInPorts         = ExpressionToString(named.Expression); break;
                    case "steamOutPorts":       def.SteamOutPorts        = ExpressionToString(named.Expression); break;
                }
            }
            return def;
        }

        // edit_nuclear_reactor_ports(reactor, add_ports=[Port(...), ...])
        private static EditNuclearReactorPortsDef parseEditNuclearReactorPorts(CallExpression call) {
            EditNuclearReactorPortsDef def = new EditNuclearReactorPortsDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "reactor":   def.ReactorId = ExpressionToId(named.Expression); break;
                        case "add_ports": def.AddPorts  = ExpressionToPortList(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ReactorId = ExpressionToId(arg.Expression); break;
                        case 1: def.AddPorts  = ExpressionToPortList(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // edit_nuclear_reactor_enrichment(reactor, enrichment=Enrichment(...))
        private static EditNuclearReactorEnrichmentDef parseEditNuclearReactorEnrichment(CallExpression call) {
            var def = new EditNuclearReactorEnrichmentDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "reactor":    def.ReactorId  = ExpressionToId(named.Expression); break;
                        case "enrichment": def.Enrichment = ExpressionToEnrichment(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ReactorId  = ExpressionToId(arg.Expression); break;
                        case 1: def.Enrichment = ExpressionToEnrichment(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        private static NuclearReactorDef parseNuclearReactor(CallExpression call) {
            var def = new NuclearReactorDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindNuclearReactorNamed(def, na.Name, na.Expression);
                else bindNuclearReactorPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindNuclearReactorNamed(NuclearReactorDef def, string name, IExpression expr) {
            switch (name) {
                case "reactorId":              def.ReactorId              = ExpressionToId(expr); break;
                case "source":                 def.SourceId               = ExpressionToId(expr); break;
                case "name":                   def.Name                   = ExpressionToString(expr); break;
                case "description":            def.Description            = ExpressionToString(expr); break;
                case "maxPowerLevel":          def.MaxPowerLevel          = ExpressionToInt(expr); break;
                case "fuelCapacity":           def.FuelCapacity           = ExpressionToInt(expr); break;
                case "minFuelToOperate":       def.MinFuelToOperate       = ExpressionToInt(expr); break;
                case "processDurationSeconds": def.ProcessDurationSeconds = ExpressionToInt(expr); break;
                case "computingConsumed":      def.ComputingConsumed      = ExpressionToInt(expr); break;
                case "fuel_pairs":             def.FuelPairs              = ExpressionToFuelPairList(expr); break;
                case "fuelInPortShape":        def.FuelInPortShape        = ExpressionToString(expr); break;
                case "fuelOutPortShape":       def.FuelOutPortShape       = ExpressionToString(expr); break;
                case "coolantIn":              def.CoolantInId            = ExpressionToIdOrNull(expr); break;
                case "coolantOut":             def.CoolantOutId           = ExpressionToIdOrNull(expr); break;
                case "coolantInPort":          def.CoolantInPort          = ExpressionToString(expr); break;
                case "coolantOutPort":         def.CoolantOutPort         = ExpressionToString(expr); break;
                case "coolantInPortShape":     def.CoolantInPortShape     = ExpressionToString(expr); break;
                case "coolantOutPortShape":    def.CoolantOutPortShape    = ExpressionToString(expr); break;
                case "waterInProduct":         def.WaterInProductId       = ExpressionToIdOrNull(expr); break;
                case "waterInQuantity":        def.WaterInQuantity        = ExpressionToInt(expr); break;
                case "steamOutProduct":        def.SteamOutProductId      = ExpressionToIdOrNull(expr); break;
                case "steamOutQuantity":       def.SteamOutQuantity       = ExpressionToInt(expr); break;
                case "waterInPorts":           def.WaterInPorts           = ExpressionToString(expr); break;
                case "steamOutPorts":          def.SteamOutPorts          = ExpressionToString(expr); break;
                case "enrichment":             def.Enrichment             = ExpressionToEnrichment(expr); break;
                case "add_ports":              def.AddPorts               = ExpressionToPortList(expr); break;
                case "layout_str":             def.LayoutSourceStr        = ExpressionToString(expr); break;
                case "research":               def.ResearchId             = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":           def.LockedOnInit           = ExpressionToBool(expr); break;
            }
        }
        private static void bindNuclearReactorPositional(NuclearReactorDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0:  def.ReactorId              = ExpressionToId(expr); break;
                case 1:  def.SourceId               = ExpressionToId(expr); break;
                case 2:  def.Name                   = ExpressionToString(expr); break;
                case 3:  def.Description            = ExpressionToString(expr); break;
                case 4:  def.MaxPowerLevel          = ExpressionToInt(expr); break;
                case 5:  def.FuelCapacity           = ExpressionToInt(expr); break;
                case 6:  def.MinFuelToOperate       = ExpressionToInt(expr); break;
                case 7:  def.ProcessDurationSeconds = ExpressionToInt(expr); break;
                case 8:  def.ComputingConsumed      = ExpressionToInt(expr); break;
                case 9:  def.FuelPairs              = ExpressionToFuelPairList(expr); break;
                case 10: def.FuelInPortShape        = ExpressionToString(expr); break;
                case 11: def.FuelOutPortShape       = ExpressionToString(expr); break;
                case 12: def.AddPorts               = ExpressionToPortList(expr); break;
                case 13: def.LayoutSourceStr        = ExpressionToString(expr); break;
                case 14: def.ResearchId             = ExpressionToIdOrNull(expr); break;
                case 15: def.LockedOnInit           = ExpressionToBool(expr); break;
            }
        }

        /// `[FuelPair(...), FuelPair(...), ...]` → List<FuelPairRef>. None / [] → empty list.
        public static List<FuelPairRef> ExpressionToFuelPairList(IExpression expr) {
            var result = new List<FuelPairRef>();
            if (expr is NoneConst) return result;
            if (!(expr is ListExpression list)) return result;
            foreach (IExpression item in list.Items) {
                if (item is CallExpression fpCall && isCallTo(fpCall, "FuelPair")) {
                    result.Add(parseFuelPairCall(fpCall));
                }
            }
            return result;
        }

        // FuelPair(fuelIn, spentFuelOut, durationSeconds) — three required
        // args. Positional + named both supported.
        private static FuelPairRef parseFuelPairCall(CallExpression call) {
            var pr = new FuelPairRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) {
                    switch (na.Name) {
                        case "fuelIn":          pr.FuelIn          = ExpressionToId(na.Expression); break;
                        case "spentFuelOut":    pr.SpentFuelOut    = ExpressionToId(na.Expression); break;
                        case "durationSeconds": pr.DurationSeconds = ExpressionToInt(na.Expression) ?? 0; break;
                    }
                } else {
                    switch (positional) {
                        case 0: pr.FuelIn          = ExpressionToId(arg.Expression); break;
                        case 1: pr.SpentFuelOut    = ExpressionToId(arg.Expression); break;
                        case 2: pr.DurationSeconds = ExpressionToInt(arg.Expression) ?? 0; break;
                    }
                    positional++;
                }
            }
            return pr;
        }

        // Enrichment(inputProduct, inPort, outputProduct, outPort,
        //     processedPerLevel, buffersCapacity, destroyContentOnMeltdown,
        //     defaultEnrichmentStep, steps=[EnrichmentStep(...), ...])
        // Returns null for None / non-call exprs. Every field is optional
        // so the modder can author partial overrides — the runtime falls
        // back to the source's value for any null sub-field.
        public static EnrichmentRef ExpressionToEnrichment(IExpression expr) {
            if (expr == null || expr is NoneConst) return null;
            if (!(expr is CallExpression call)) return null;
            if (!isCallTo(call, "Enrichment")) return null;

            EnrichmentRef er = new EnrichmentRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) {
                    bindEnrichmentNamed(er, na.Name, na.Expression);
                } else {
                    bindEnrichmentPositional(er, positional, arg.Expression);
                    positional++;
                }
            }
            return er;
        }

        private static void bindEnrichmentNamed(EnrichmentRef er, string name, IExpression expr) {
            switch (name) {
                case "inputProduct":             er.InputProductId             = ExpressionToIdOrNull(expr); break;
                case "inPort":                   er.InPort                     = ExpressionToString(expr); break;
                case "outputProduct":            er.OutputProductId            = ExpressionToIdOrNull(expr); break;
                case "outPort":                  er.OutPort                    = ExpressionToString(expr); break;
                case "processedPerLevel":        bindProcessedPerLevel(er, expr); break;
                case "buffersCapacity":          er.BuffersCapacity            = ExpressionToInt(expr); break;
                case "destroyContentOnMeltdown": er.DestroyContentOnMeltdown   = ExpressionToBool(expr); break;
                case "defaultEnrichmentStep":    er.DefaultEnrichmentStep      = ExpressionToInt(expr); break;
                case "steps":                    er.Steps                      = ExpressionToEnrichmentStepList(expr); break;
            }
        }

        private static void bindEnrichmentPositional(EnrichmentRef er, int idx, IExpression expr) {
            switch (idx) {
                case 0: er.InputProductId           = ExpressionToIdOrNull(expr); break;
                case 1: er.InPort                   = ExpressionToString(expr); break;
                case 2: er.OutputProductId          = ExpressionToIdOrNull(expr); break;
                case 3: er.OutPort                  = ExpressionToString(expr); break;
                case 4: bindProcessedPerLevel(er, expr); break;
                case 5: er.BuffersCapacity          = ExpressionToInt(expr); break;
                case 6: er.DestroyContentOnMeltdown = ExpressionToBool(expr); break;
                case 7: er.DefaultEnrichmentStep    = ExpressionToInt(expr); break;
                case 8: er.Steps                    = ExpressionToEnrichmentStepList(expr); break;
            }
        }

        // PartialQuantity is conventionally written as either a plain int
        // (whole-number numerator with implicit /1) or a 2-tuple
        // `(numerator, denominator)`. Both shapes round-trip — the
        // emitter writes the tuple form whenever denominator != 1.
        private static void bindProcessedPerLevel(EnrichmentRef er, IExpression expr) {
            if (expr == null || expr is NoneConst) return;
            int? asInt = ExpressionToInt(expr);
            if (asInt.HasValue) {
                er.ProcessedPerLevelNumerator   = asInt.Value;
                er.ProcessedPerLevelDenominator = 1;
                return;
            }
            // Tuple shape: PyTuple(num, den) — accept both PyTuple and
            // CallExpression "PartialQuantity(num, den)" forms.
            if (expr is PyTuple tuple && tuple.ExpressionLists.Count >= 2) {
                er.ProcessedPerLevelNumerator   = ExpressionToInt(tuple.ExpressionLists[0]);
                er.ProcessedPerLevelDenominator = ExpressionToInt(tuple.ExpressionLists[1]);
                return;
            }
            if (expr is CallExpression pqCall && isCallTo(pqCall, "PartialQuantity")) {
                int p = 0;
                foreach (IArgument a in pqCall.Arguments) {
                    if (a is OrderedArgument oa) {
                        if (p == 0) er.ProcessedPerLevelNumerator = ExpressionToInt(oa.Expression);
                        else if (p == 1) er.ProcessedPerLevelDenominator = ExpressionToInt(oa.Expression);
                        p++;
                    }
                }
            }
        }

        public static List<EnrichmentStepRef> ExpressionToEnrichmentStepList(IExpression expr) {
            var result = new List<EnrichmentStepRef>();
            if (expr is NoneConst) return result;
            if (!(expr is ListExpression list)) return result;
            foreach (IExpression item in list.Items) {
                if (item is CallExpression call && isCallTo(call, "EnrichmentStep")) {
                    result.Add(parseEnrichmentStepCall(call));
                }
            }
            return result;
        }

        // EnrichmentStep(fuelMultiplierPercent, breedingRatio, steamReductionDiv)
        // — three required ints. The first is a percent value (e.g. 120
        // for 1.2× fuel) so the modder reads it as a percentage rather
        // than a fractional float.
        private static EnrichmentStepRef parseEnrichmentStepCall(CallExpression call) {
            var sr = new EnrichmentStepRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) {
                    switch (na.Name) {
                        case "fuelMultiplierPercent": sr.FuelMultiplierPercent = ExpressionToInt(na.Expression) ?? 100; break;
                        case "breedingRatio":         sr.BreedingRatio         = ExpressionToInt(na.Expression) ?? 0; break;
                        case "steamReductionDiv":     sr.SteamReductionDiv     = ExpressionToInt(na.Expression) ?? 1; break;
                    }
                } else {
                    switch (positional) {
                        case 0: sr.FuelMultiplierPercent = ExpressionToInt(arg.Expression) ?? 100; break;
                        case 1: sr.BreedingRatio         = ExpressionToInt(arg.Expression) ?? 0; break;
                        case 2: sr.SteamReductionDiv     = ExpressionToInt(arg.Expression) ?? 1; break;
                    }
                    positional++;
                }
            }
            return sr;
        }

        // build_housing(housingId, source, name, description, capacity,
        //     upointsCapacity, research, lockedOnInit)
        private static HousingDef parseBuildHousing(CallExpression call) {
            HousingDef def = new HousingDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindHousingNamed(def, named.Name, named.Expression);
                } else {
                    bindHousingPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindHousingPositional(HousingDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.HousingId       = ExpressionToId(expr); return;
                case 1: def.SourceId        = ExpressionToId(expr); return;
                case 2: def.Name            = ExpressionToString(expr); return;
                case 3: def.Description     = ExpressionToString(expr); return;
                case 4: def.Capacity        = ExpressionToInt(expr); return;
                case 5: def.UpointsCapacity = ExpressionToInt(expr); return;
                case 6: def.ResearchId      = ExpressionToIdOrNull(expr); return;
                case 7: def.LockedOnInit    = ExpressionToBool(expr); return;
            }
        }

        private static void bindHousingNamed(HousingDef def, string name, IExpression expr) {
            switch (name) {
                case "housingId":       def.HousingId        = ExpressionToId(expr); break;
                case "source":          def.SourceId         = ExpressionToId(expr); break;
                case "name":            def.Name             = ExpressionToString(expr); break;
                case "description":     def.Description      = ExpressionToString(expr); break;
                case "capacity":        def.Capacity         = ExpressionToInt(expr); break;
                case "upointsCapacity": def.UpointsCapacity  = ExpressionToInt(expr); break;
                case "layout_str":      def.LayoutSourceStr  = ExpressionToString(expr); break;
                case "research":        def.ResearchId       = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":    def.LockedOnInit     = ExpressionToBool(expr); break;
            }
        }

        // build_machine(...) — the sole machine-construction API at the
        // Python level. Captured as a typed BuildMachineDef so the call
        // name + arg surface round-trip through save without flipping
        // to an ad-hoc form.
        private static BuildMachineDef parseBuildMachine(CallExpression call) {
            BuildMachineDef def = new BuildMachineDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindBuildMachineNamed(def, named.Name, named.Expression);
                } else {
                    bindBuildMachinePositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindBuildMachinePositional(BuildMachineDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.MachineId             = ExpressionToId(expr); return;
                case 1: def.SourceId              = ExpressionToId(expr); return;
                case 2: def.Name                  = ExpressionToString(expr); return;
                case 3: def.Description           = ExpressionToString(expr); return;
                case 4: def.AddPorts              = ExpressionToPortList(expr); return;
                case 5: def.ConsumedPowerPerTickKw= ExpressionToInt(expr); return;
                case 6: def.ResearchId            = ExpressionToIdOrNull(expr); return;
                case 7: def.CopyRecipes           = ExpressionToBool(expr); return;
                case 8: def.LockedOnInit          = ExpressionToBool(expr); return;
            }
        }

        private static void bindBuildMachineNamed(BuildMachineDef def, string name, IExpression expr) {
            switch (name) {
                case "machineId":            def.MachineId              = ExpressionToId(expr); break;
                case "source":               def.SourceId               = ExpressionToId(expr); break;
                case "name":                 def.Name                   = ExpressionToString(expr); break;
                case "description":          def.Description            = ExpressionToString(expr); break;
                case "ports":                def.AddPorts               = ExpressionToPortList(expr); break;
                case "add_ports":            def.AddPorts               = ExpressionToPortList(expr); break;  // legacy alias
                case "consumedPowerPerTick": def.ConsumedPowerPerTickKw = ExpressionToInt(expr); break;
                case "research":             def.ResearchId             = ExpressionToIdOrNull(expr); break;
                case "copy_recipes":         def.CopyRecipes            = ExpressionToBool(expr); break;
                case "copy_layout":          def.CopyLayout             = ExpressionToBool(expr); break;
                case "copy_ports":           def.CopyPorts              = ExpressionToBool(expr); break;
                case "copy_graphics":        def.CopyGraphics           = ExpressionToBool(expr); break;
                case "layout_str":           def.LayoutSourceStr        = ExpressionToString(expr); break;
                case "lockedOnInit":         def.LockedOnInit           = ExpressionToBool(expr); break;
            }
        }

        // edit_machine_ports(machine, add_ports=[Port(...)])
        private static EditMachinePortsDef parseEditMachinePorts(CallExpression call) {
            EditMachinePortsDef def = new EditMachinePortsDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "machine":   def.MachineId = ExpressionToId(named.Expression); break;
                        case "add_ports": def.AddPorts  = ExpressionToPortList(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.MachineId = ExpressionToId(arg.Expression); break;
                        case 1: def.AddPorts  = ExpressionToPortList(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        /// `[Port(...), Port(...), ...]` → List<PortRef>. None / [] → empty list.
        /// Mirrors <see cref="ExpressionToProductList"/> but for Port calls.
        public static List<PortRef> ExpressionToPortList(IExpression expr) {
            List<PortRef> result = new List<PortRef>();
            if (expr is NoneConst) return result;
            if (!(expr is ListExpression list)) return result;
            foreach (IExpression item in list.Items) {
                if (item is CallExpression portCall && isCallTo(portCall, "Port")) {
                    result.Add(parsePortCall(portCall));
                }
            }
            return result;
        }

        // Port(name, type, shape, position, direction, canOnlyConnectToTransports)
        private static PortRef parsePortCall(CallExpression call) {
            PortRef pr = new PortRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindPortNamed(pr, named.Name, named.Expression);
                } else {
                    bindPortPositional(pr, positional, arg.Expression);
                    positional++;
                }
            }
            return pr;
        }

        private static void bindPortPositional(PortRef pr, int idx, IExpression expr) {
            switch (idx) {
                case 0: pr.Name      = ExpressionToString(expr); return;
                case 1: pr.Type      = ExpressionToString(expr); return;
                case 2: pr.Shape     = ExpressionToString(expr); return;
                case 3: bindPortPosition(pr, expr); return;
                case 4: pr.Direction = ExpressionToString(expr); return;
                case 5: pr.CanOnlyConnectToTransports = ExpressionToBool(expr); return;
            }
        }

        private static void bindPortNamed(PortRef pr, string name, IExpression expr) {
            switch (name) {
                case "name":      pr.Name      = ExpressionToString(expr); break;
                case "type":      pr.Type      = ExpressionToString(expr); break;
                case "shape":     pr.Shape     = ExpressionToString(expr); break;
                case "position":  bindPortPosition(pr, expr); break;
                case "direction": pr.Direction = ExpressionToString(expr); break;
                case "canOnlyConnectToTransports":
                    pr.CanOnlyConnectToTransports = ExpressionToBool(expr); break;
            }
        }

        // Walk the position expression's AST directly — no string round-trip.
        // For the common `(x, y, z)` / `(x, y)` PyTuple of NumberConstant
        // ints, drop the values straight into PortRef.PositionX/Y/Z. For
        // anything else (typed-refs like Vector3i(...) or Ids.X, named
        // constructors, list literals) fall back to capturing the raw
        // expression text so the emitter can splice it back verbatim.
        private static void bindPortPosition(PortRef pr, IExpression expr) {
            if (expr is PyTuple t) {
                int n = t.ExpressionLists?.Count ?? 0;
                if ((n == 2 || n == 3) && allIntConstants(t.ExpressionLists)) {
                    pr.PositionX = numberAsInt(t.ExpressionLists[0]);
                    pr.PositionY = numberAsInt(t.ExpressionLists[1]);
                    pr.PositionZ = n == 3 ? numberAsInt(t.ExpressionLists[2]) : 0;
                    pr.PositionExpression = null;
                    return;
                }
            }
            if (expr is ListExpression le) {
                var items = le.Items;
                int n = items?.Count ?? 0;
                if ((n == 2 || n == 3) && allIntConstants(items)) {
                    pr.PositionX = numberAsInt(items[0]);
                    pr.PositionY = numberAsInt(items[1]);
                    pr.PositionZ = n == 3 ? numberAsInt(items[2]) : 0;
                    pr.PositionExpression = null;
                    return;
                }
            }
            // Non-tuple shape — capture the raw expression text and leave
            // X/Y/Z at their defaults so the emitter prefers the raw form.
            pr.PositionExpression = renderExpressionAsText(expr);
        }

        private static bool allIntConstants(System.Collections.Generic.IReadOnlyList<IExpression> items) {
            for (int i = 0; i < items.Count; i++) {
                if (!(items[i] is NumberConstant nc) || !(nc.Value is int)) return false;
            }
            return true;
        }

        private static int numberAsInt(IExpression e) {
            return (int)((NumberConstant)e).Value;
        }

        // add_texture(path, replace=None)
        private static TextureDef parseTexture(CallExpression call) {
            TextureDef def = new TextureDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "path":    def.Path        = ExpressionToString(named.Expression); break;
                        case "replace": def.ReplacePath = ExpressionToString(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.Path        = ExpressionToString(arg.Expression); break;
                        case 1: def.ReplacePath = ExpressionToString(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // add_loose_product_material(path, albedo, normals=None, metallic=None,
        //     reference=None, tiling=1)
        private static MaterialLooseDef parseMaterialLoose(CallExpression call) {
            MaterialLooseDef def = new MaterialLooseDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindMaterialLooseNamed(def, named.Name, named.Expression);
                } else {
                    switch (positional) {
                        case 0: def.Path             = ExpressionToString(arg.Expression); break;
                        case 1: def.AlbedoExpression = renderExpressionAsText(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        private static void bindMaterialLooseNamed(MaterialLooseDef def, string name, IExpression expr) {
            switch (name) {
                case "path":      def.Path                = ExpressionToString(expr); break;
                case "albedo":    def.AlbedoExpression    = renderExpressionAsText(expr); break;
                case "normals":   def.NormalsExpression   = renderExpressionAsText(expr); break;
                case "metallic":  def.MetallicExpression  = renderExpressionAsText(expr); break;
                case "reference": def.ReferenceExpression = renderExpressionAsText(expr); break;
                case "tiling":    def.TilingExpression    = renderExpressionAsText(expr); break;
            }
        }

        // add_prefab_box(path, texture=None)
        private static PrefabBoxDef parsePrefabBox(CallExpression call) {
            PrefabBoxDef def = new PrefabBoxDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "path":    def.Path              = ExpressionToString(named.Expression); break;
                        case "texture": def.TextureExpression = renderExpressionAsText(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.Path              = ExpressionToString(arg.Expression); break;
                        case 1: def.TextureExpression = renderExpressionAsText(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // add_unit_prefab(path, albedo, normals=None, metallic=None,
        //     reference=None, width=0.5, height=0.2, depth=0.5, mesh=None,
        //     winding="ccw")
        private static UnitPrefabDef parseUnitPrefab(CallExpression call) {
            UnitPrefabDef def = new UnitPrefabDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindUnitPrefabNamed(def, named.Name, named.Expression);
                } else {
                    bindUnitPrefabPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindUnitPrefabPositional(UnitPrefabDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.Path                = ExpressionToString(expr); return;
                case 1: def.AlbedoExpression    = renderExpressionAsText(expr); return;
                case 2: def.NormalsExpression   = renderExpressionAsText(expr); return;
                case 3: def.MetallicExpression  = renderExpressionAsText(expr); return;
                case 4: def.ReferenceExpression = renderExpressionAsText(expr); return;
                case 5: def.Width               = ExpressionToDouble(expr); return;
                case 6: def.Height              = ExpressionToDouble(expr); return;
                case 7: def.Depth               = ExpressionToDouble(expr); return;
                case 8: def.MeshPath            = ExpressionToString(expr); return;
                case 9: def.Winding             = ExpressionToString(expr); return;
            }
        }

        private static void bindUnitPrefabNamed(UnitPrefabDef def, string name, IExpression expr) {
            switch (name) {
                case "path":      def.Path                = ExpressionToString(expr); break;
                case "albedo":    def.AlbedoExpression    = renderExpressionAsText(expr); break;
                case "normals":   def.NormalsExpression   = renderExpressionAsText(expr); break;
                case "metallic":  def.MetallicExpression  = renderExpressionAsText(expr); break;
                case "reference": def.ReferenceExpression = renderExpressionAsText(expr); break;
                case "width":     def.Width               = ExpressionToDouble(expr); break;
                case "height":    def.Height              = ExpressionToDouble(expr); break;
                case "depth":     def.Depth               = ExpressionToDouble(expr); break;
                case "mesh":      def.MeshPath            = ExpressionToString(expr); break;
                case "winding":   def.Winding             = ExpressionToString(expr); break;
            }
        }

        // add_texture_material(path, texture=None, reference=None, shader=None)
        private static MaterialTextureDef parseTextureMaterial(CallExpression call) {
            MaterialTextureDef def = new MaterialTextureDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "path":      def.Path                = ExpressionToString(named.Expression); break;
                        case "texture":   def.TextureExpression   = renderExpressionAsText(named.Expression); break;
                        case "reference": def.ReferenceExpression = renderExpressionAsText(named.Expression); break;
                        case "shader":    def.Shader              = ExpressionToString(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.Path                = ExpressionToString(arg.Expression); break;
                        case 1: def.TextureExpression   = renderExpressionAsText(arg.Expression); break;
                        case 2: def.ReferenceExpression = renderExpressionAsText(arg.Expression); break;
                        case 3: def.Shader              = ExpressionToString(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // edit_recipe(recipe, duration, ingredients, products, machine,
        //     research, power) — same arg surface as build_recipe minus
        // id/name/description; mutates an existing recipe at load time.
        private static EditRecipeDef parseEditRecipe(CallExpression call) {
            EditRecipeDef def = new EditRecipeDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindEditRecipeNamed(def, named.Name, named.Expression);
                } else {
                    bindEditRecipePositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindEditRecipePositional(EditRecipeDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.RecipeId        = ExpressionToId(expr); return;
                case 1: def.DurationSeconds = ExpressionToDurationSeconds(expr); return;
                case 2: def.Ingredients     = ExpressionToProductList(expr); return;
                case 3: def.Products        = ExpressionToProductList(expr); return;
                case 4: def.MachineId       = ExpressionToIdOrNull(expr); return;
                case 5: def.ResearchId      = ExpressionToIdOrNull(expr); return;
                case 6: def.PowerPercent    = ExpressionToPowerPercent(expr); return;
            }
        }

        private static void bindEditRecipeNamed(EditRecipeDef def, string name, IExpression expr) {
            switch (name) {
                case "recipe":      def.RecipeId        = ExpressionToId(expr); break;
                case "duration":    def.DurationSeconds = ExpressionToDurationSeconds(expr); break;
                case "ingredients": def.Ingredients     = ExpressionToProductList(expr); break;
                case "products":    def.Products        = ExpressionToProductList(expr); break;
                case "machine":     def.MachineId       = ExpressionToIdOrNull(expr); break;
                case "research":    def.ResearchId      = ExpressionToIdOrNull(expr); break;
                case "power":       def.PowerPercent    = ExpressionToPowerPercent(expr); break;
            }
        }

        // Bare float / int → double. None / other → null. Used for the
        // dimensional args on add_unit_prefab (width / height / depth).
        public static double? ExpressionToDouble(IExpression expr) {
            if (expr is NumberConstant n) {
                switch (n.Value) {
                    case int i:    return i;
                    case long l:   return l;
                    case float f:  return f;
                    case double d: return d;
                }
            }
            if (expr is NoneConst) return null;
            return null;
        }

        // add_toolbar_category(categoryId, name, icon, parent, entities)
        private static ToolbarCategoryDef parseToolbarCategory(CallExpression call) {
            ToolbarCategoryDef def = new ToolbarCategoryDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "categoryId": def.CategoryId         = ExpressionToId(named.Expression); break;
                        case "name":       def.Name               = ExpressionToString(named.Expression); break;
                        case "icon":       def.IconPath = ExpressionToIconPath(named.Expression); break;
                        case "parent":     def.ParentId           = ExpressionToId(named.Expression); break;
                        case "entities":   def.EntitiesExpression = renderExpressionAsText(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.CategoryId         = ExpressionToId(arg.Expression); break;
                        case 1: def.Name               = ExpressionToString(arg.Expression); break;
                        case 2: def.IconPath = ExpressionToIconPath(arg.Expression); break;
                        case 3: def.ParentId           = ExpressionToId(arg.Expression); break;
                        case 4: def.EntitiesExpression = renderExpressionAsText(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // build_generator(id, name, inputProduct, outputElectricityKw,
        //     outputProduct=None, description, source, duration,
        //     generationPriority, bufferCapacityMultiplier, research,
        //     lockedOnInit)
        private static GeneratorDef parseGenerator(CallExpression call) {
            GeneratorDef def = new GeneratorDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindGeneratorNamed(def, named.Name, named.Expression);
                } else {
                    bindGeneratorPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindGeneratorPositional(GeneratorDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.GeneratorId           = ExpressionToId(expr); break;
                case 1: def.Name                  = ExpressionToString(expr); break;
                case 2: def.InputProductExpression= renderExpressionAsText(expr); break;
                case 3: def.OutputElectricityKw   = ExpressionToInt(expr); break;
            }
        }

        private static void bindGeneratorNamed(GeneratorDef def, string name, IExpression expr) {
            switch (name) {
                case "id":                       def.GeneratorId               = ExpressionToId(expr); break;
                case "name":                     def.Name                      = ExpressionToString(expr); break;
                case "inputProduct":             def.InputProductExpression    = renderExpressionAsText(expr); break;
                case "outputElectricityKw":      def.OutputElectricityKw       = ExpressionToInt(expr); break;
                case "outputProduct":            def.OutputProductExpression   = renderExpressionAsText(expr); break;
                case "description":              def.Description               = ExpressionToString(expr); break;
                case "source":                   def.SourceId                  = ExpressionToId(expr); break;
                case "duration":                 def.DurationSeconds           = ExpressionToDurationSeconds(expr); break;
                case "generationPriority":       def.GenerationPriority        = ExpressionToInt(expr); break;
                case "bufferCapacityMultiplier": def.BufferCapacityMultiplier  = ExpressionToInt(expr); break;
                case "research":                 def.ResearchId                = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":             def.LockedOnInit              = ExpressionToBool(expr); break;
            }
        }

        // build_product_loose(productId, name, icon, material, color=None,
        //     particleColor=None, description="", isDumped=False, isStorable=False,
        //     isRecyclable=False, isWaste=False, isRough=False, pinToHomeScreen=False,
        //     maxTransport=None, prefabPath=None, dumpsAs=None, research=None)
        private static ProductLooseDef parseProductLoose(CallExpression call) {
            ProductLooseDef def = new ProductLooseDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindProductLooseNamed(def, named.Name, named.Expression);
                } else {
                    bindProductLoosePositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindProductLoosePositional(ProductLooseDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.ProductId          = ExpressionToId(expr);     break;
                case 1: def.Name               = ExpressionToString(expr); break;
                case 2: def.IconPath = ExpressionToIconPath(expr); break;
                case 3: def.MaterialExpression = renderExpressionAsText(expr); break;
                // The rest typically appear as named-args in practice.
            }
        }

        private static void bindProductLooseNamed(ProductLooseDef def, string name, IExpression expr) {
            switch (name) {
                case "productId":       def.ProductId          = ExpressionToId(expr); break;
                case "name":            def.Name               = ExpressionToString(expr); break;
                case "icon":            def.IconPath = ExpressionToIconPath(expr); break;
                case "material":        def.MaterialExpression = renderExpressionAsText(expr); break;
                case "color":           def.ColorExpression    = renderExpressionAsText(expr); break;
                case "particleColor":   def.ParticleColorExpression = renderExpressionAsText(expr); break;
                case "description":     def.Description        = ExpressionToString(expr); break;
                case "isDumped":        def.IsDumped           = ExpressionToBool(expr); break;
                case "isStorable":      def.IsStorable         = ExpressionToBool(expr); break;
                case "isRecyclable":    def.IsRecyclable       = ExpressionToBool(expr); break;
                case "isWaste":         def.IsWaste            = ExpressionToBool(expr); break;
                case "isRough":         def.IsRough            = ExpressionToBool(expr); break;
                case "pinToHomeScreen": def.PinToHomeScreen    = ExpressionToBool(expr); break;
                case "maxTransport":    def.MaxTransport       = ExpressionToInt(expr); break;
                case "prefabPath":      def.PrefabPath         = ExpressionToString(expr); break;
                case "dumpsAs":         def.DumpsAsId          = ExpressionToIdOrNull(expr); break;
                case "research":        def.ResearchId         = ExpressionToIdOrNull(expr); break;
            }
        }

        // build_product_fluid(productId, name, icon, color=None,
        //     transportColor=None, transportAccentColor=None, canBeDiscarded=True,
        //     description="", isStorable=False, isWaste=False)
        private static ProductFluidDef parseProductFluid(CallExpression call) {
            ProductFluidDef def = new ProductFluidDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindProductFluidNamed(def, named.Name, named.Expression);
                } else {
                    bindProductFluidPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindProductFluidPositional(ProductFluidDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.ProductId = ExpressionToId(expr); break;
                case 1: def.Name      = ExpressionToString(expr); break;
                case 2: def.IconPath = ExpressionToIconPath(expr); break;
            }
        }

        private static void bindProductFluidNamed(ProductFluidDef def, string name, IExpression expr) {
            switch (name) {
                case "productId":            def.ProductId                      = ExpressionToId(expr); break;
                case "name":                 def.Name                           = ExpressionToString(expr); break;
                case "icon":                 def.IconPath = ExpressionToIconPath(expr); break;
                case "color":                def.ColorExpression                = renderExpressionAsText(expr); break;
                case "transportColor":       def.TransportColorExpression       = renderExpressionAsText(expr); break;
                case "transportAccentColor": def.TransportAccentColorExpression = renderExpressionAsText(expr); break;
                case "canBeDiscarded":       def.CanBeDiscarded                 = ExpressionToBool(expr); break;
                case "description":          def.Description                    = ExpressionToString(expr); break;
                case "isStorable":           def.IsStorable                     = ExpressionToBool(expr); break;
                case "isWaste":              def.IsWaste                        = ExpressionToBool(expr); break;
            }
        }

        // build_product_unit(productId, name, icon, prefab, maxTransport=Quantity(3),
        //     description="", isStorable=False, isWaste=False, packingMode=None,
        //     allowPackingNoise=False, rotateSecondPackedItem90Degs=False, research=None)
        private static ProductUnitDef parseProductUnit(CallExpression call) {
            ProductUnitDef def = new ProductUnitDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindProductUnitNamed(def, named.Name, named.Expression);
                } else {
                    bindProductUnitPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindProductUnitPositional(ProductUnitDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.ProductId         = ExpressionToId(expr); break;
                case 1: def.Name              = ExpressionToString(expr); break;
                case 2: def.IconPath = ExpressionToIconPath(expr); break;
                case 3: def.PrefabExpression  = renderExpressionAsText(expr); break;
                case 4: def.MaxTransport      = ExpressionToInt(expr); break;
            }
        }

        private static void bindProductUnitNamed(ProductUnitDef def, string name, IExpression expr) {
            switch (name) {
                case "productId":                    def.ProductId                    = ExpressionToId(expr); break;
                case "name":                         def.Name                         = ExpressionToString(expr); break;
                case "icon":                         def.IconPath = ExpressionToIconPath(expr); break;
                case "prefab":                       def.PrefabExpression             = renderExpressionAsText(expr); break;
                case "maxTransport":                 def.MaxTransport                 = ExpressionToInt(expr); break;
                case "description":                  def.Description                  = ExpressionToString(expr); break;
                case "isStorable":                   def.IsStorable                   = ExpressionToBool(expr); break;
                case "isWaste":                      def.IsWaste                      = ExpressionToBool(expr); break;
                case "packingMode":                  def.PackingMode                  = ExpressionToString(expr); break;
                case "allowPackingNoise":            def.AllowPackingNoise            = ExpressionToBool(expr); break;
                case "rotateSecondPackedItem90Degs": def.RotateSecondPackedItem90Degs = ExpressionToBool(expr); break;
                case "research":                     def.ResearchId                   = ExpressionToIdOrNull(expr); break;
            }
        }

        // Best-effort bool extraction. `True`/`False` from Python parse as
        // BooleanConst; treat anything else (None, numbers) as false to
        // match Python's truthiness rules conservatively.
        private static bool ExpressionToBool(IExpression expr) {
            if (expr is BooleanConst b) return b.BooleanValue;
            return false;
        }

        // build_research(researchId, name, description, costs, position, icon)
        // â€” every arg is positional or named with the same string-spelled key.
        // Position is a 2-tuple (x, y); we parse the two int components.
        private static ResearchDef parseBuildResearch(CallExpression call) {
            ResearchDef def = new ResearchDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindResearchNamed(def, named.Name, named.Expression);
                } else {
                    bindResearchPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindResearchPositional(ResearchDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.ResearchId  = ExpressionToId(expr);    return;
                case 1: def.Name        = ExpressionToString(expr); return;
                case 2: def.Description = ExpressionToString(expr); return;
                case 3: bindResearchCosts(def, expr); return;
                case 4: bindResearchPosition(def, expr); return;
                case 5: def.IconPath = ExpressionToIconPath(expr); return;
            }
        }

        private static void bindResearchNamed(ResearchDef def, string name, IExpression expr) {
            switch (name) {
                case "researchId":  def.ResearchId  = ExpressionToId(expr);    break;
                case "name":        def.Name        = ExpressionToString(expr); break;
                case "description": def.Description = ExpressionToString(expr); break;
                case "costs":       bindResearchCosts(def, expr); break;
                case "position":    bindResearchPosition(def, expr); break;
                case "icon":        def.IconPath = ExpressionToIconPath(expr); break;
                // Editor-side hint pointing at the research-pack product this
                // node expects to consume (LabEquipment / LabEquipment2 /
                // LabEquipment3 / LabEquipment4). Saved by the research form's
                // ResearchPackProductPicker; round-tripped as an id string or
                // typed-ref via ExpressionToId.
                case "tier":        def.TierProductId = ExpressionToIdOrNull(expr); break;
            }
        }

        // Costs can be a plain int (cheap path) or a typed-ref expression
        // like `ResearchCostsTpl.Tier2()` (must round-trip verbatim). Both
        // forms produce a numeric-cost field that the editor can display,
        // and a raw-expression field that the emitter prefers when set.
        private static void bindResearchCosts(ResearchDef def, IExpression expr) {
            int? asInt = ExpressionToInt(expr);
            if (asInt.HasValue) {
                def.CostsAsInt = asInt;
                return;
            }
            // Non-int â€” preserve the expression text by rendering its source
            // path (callee + first arg's path if any). When we can't parse,
            // fall back to a placeholder; the typed editor surfaces a raw
            // text field for those rare cases.
            def.CostsExpression = renderExpressionAsText(expr);
        }

        private static void bindResearchPosition(ResearchDef def, IExpression expr) {
            // The parser distinguishes three shapes that all mean the same
            // (x, y) coordinate:
            //   * `(x, y)`           â€” PyTuple   (what the emitter writes)
            //   * `Vector2i(x, y)`   â€” CallExpression
            //   * `[x, y]`           â€” ListExpression (tolerated; some modders
            //                          write the position with square brackets)
            // The earlier implementation only handled CallExpression, so the
            // emitter's own `(x, y)` output round-tripped to PositionX/Y =
            // null on the next load. Each branch below pulls out the first
            // two int children; anything else (variable refs, non-int args)
            // is ignored so the emitter falls back to omitting the position
            // arg entirely rather than corrupting the source.
            if (expr is PyTuple tuple) {
                if (tuple.ExpressionLists != null && tuple.ExpressionLists.Count >= 1) {
                    def.PositionX = ExpressionToInt(tuple.ExpressionLists[0]);
                }
                if (tuple.ExpressionLists != null && tuple.ExpressionLists.Count >= 2) {
                    def.PositionY = ExpressionToInt(tuple.ExpressionLists[1]);
                }
                return;
            }
            if (expr is ListExpression list) {
                if (list.Items != null && list.Items.Count >= 1) {
                    def.PositionX = ExpressionToInt(list.Items[0]);
                }
                if (list.Items != null && list.Items.Count >= 2) {
                    def.PositionY = ExpressionToInt(list.Items[1]);
                }
                return;
            }
            if (expr is CallExpression call) {
                int positional = 0;
                foreach (IArgument arg in call.Arguments) {
                    if (arg is OrderedArgument) {
                        int? v = ExpressionToInt(arg.Expression);
                        if (positional == 0) def.PositionX = v;
                        else if (positional == 1) def.PositionY = v;
                        positional++;
                    }
                }
            }
        }

        // Render an expression as its source-like text. Used for non-trivial
        // costs (ResearchCostsTplâ€¦) AND every color field (`color`,
        // `particleColor`, `transportColor`, `transportAccentColor`), keeping
        // the modder's original wording so the form's color picker can parse
        // a tuple-literal back into RGB and the emitter can splice the same
        // text verbatim on save. PyTuple covers `(R, G, B)` â€” the canonical
        // shape COI uses for colors and the emitter writes. ListExpression is
        // tolerated for `[R, G, B]` modders who reach for square brackets.
        private static string renderExpressionAsText(IExpression expr) {
            switch (expr) {
                case VariableExpression v: return v.Path;
                case PropertyExpression p: return p.Path;
                case CallExpression c:
                    string callee = calleeAsPath(c.Calle);
                    if (string.IsNullOrEmpty(callee)) return "<unparseable>";
                    StringBuilder sb = new StringBuilder(callee);
                    sb.Append('(');
                    bool first = true;
                    // Render EVERY argument — positional via recursive
                    // renderExpressionAsText, named as `name = <expr>`.
                    // The earlier int-only filter dropped strings + named
                    // args, which turned `Product("Foo", 1)` into
                    // `Product(1)` and `Product(product="Foo", quantity=1)`
                    // into bare `Product()` on the next save round-trip.
                    foreach (IArgument a in c.Arguments) {
                        if (!first) sb.Append(", ");
                        if (a is NamedArgument na) {
                            sb.Append(na.Name).Append("=");
                            sb.Append(renderExpressionAsText(na.Expression));
                        } else {
                            sb.Append(renderExpressionAsText(a.Expression));
                        }
                        first = false;
                    }
                    sb.Append(')');
                    return sb.ToString();
                case PyTuple t:
                    return renderSequence("(", ")", t.ExpressionLists);
                case ListExpression le:
                    return renderSequence("[", "]", le.Items);
                case NumberConstant nc: return nc.Value?.ToString() ?? "0";
                case StringConstant sc: return "\"" + sc.Value + "\"";
                case BooleanConst bc:   return bc.BooleanValue ? "True" : "False";
                case NoneConst:         return "None";
                default: return "<unparseable>";
            }
        }

        // Render a tuple/list literal as "(a, b, c)" / "[a, b, c]". Each
        // element goes through renderExpressionAsText so nested calls
        // (rare for colors, common for cost expressions used elsewhere)
        // still render with the right shape. A null or empty sequence
        // produces an empty literal rather than "<unparseable>" so the
        // emitter has something safe to splice back even for edge cases.
        private static string renderSequence(string open, string close,
                System.Collections.Generic.IReadOnlyList<IExpression> items) {
            StringBuilder sb = new StringBuilder();
            sb.Append(open);
            if (items != null) {
                bool first = true;
                foreach (IExpression item in items) {
                    if (!first) sb.Append(", ");
                    sb.Append(renderExpressionAsText(item));
                    first = false;
                }
            }
            sb.Append(close);
            return sb.ToString();
        }

        // add_unlock_recipe(research, machine, proto) â€” three id args.
        private static UnlockRecipeDef parseUnlockRecipe(CallExpression call) {
            UnlockRecipeDef def = new UnlockRecipeDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "research": def.ResearchId = ExpressionToId(named.Expression); break;
                        case "machine":  def.MachineId  = ExpressionToId(named.Expression); break;
                        case "proto":    def.RecipeId   = ExpressionToId(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ResearchId = ExpressionToId(arg.Expression); break;
                        case 1: def.MachineId  = ExpressionToId(arg.Expression); break;
                        case 2: def.RecipeId   = ExpressionToId(arg.Expression); break;
                    }
                    positional++;
                }
            }
            // Unlock kinds expose DisplayId via override (composite key
            // RecipeId @ MachineId) so no separate Id assignment is needed.
            return def;
        }

        // add_unlock_product(research, product) â€” two id args.
        private static UnlockProductDef parseUnlockProduct(CallExpression call) {
            UnlockProductDef def = new UnlockProductDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "research": def.ResearchId = ExpressionToId(named.Expression); break;
                        case "product":  def.ProductId  = ExpressionToId(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ResearchId = ExpressionToId(arg.Expression); break;
                        case 1: def.ProductId  = ExpressionToId(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // add_unlock_machine(research, machine) â€” two id args.
        private static UnlockMachineDef parseUnlockMachine(CallExpression call) {
            UnlockMachineDef def = new UnlockMachineDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "research": def.ResearchId = ExpressionToId(named.Expression); break;
                        case "machine":  def.MachineId  = ExpressionToId(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ResearchId = ExpressionToId(arg.Expression); break;
                        case 1: def.MachineId  = ExpressionToId(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // Common bookkeeping that the loader stamps on every definition it
        // captures â€” source file, line range, and any wrapping condition.
        // Pulled out so RecipeDef and UnknownDef paths share one assignment
        // block instead of repeating six lines each.
        private static void attachSourceLocation(DefBase def,
                int startLine, int endLine,
                string sourceFile, string conditionChain, string scopeKey, int runIndex) {
            def.SourceFile      = sourceFile;
            def.SourceStartLine = startLine;
            def.SourceEndLine   = endLine;
            def.AstStartLine    = startLine;
            def.ScopeKey        = scopeKey;
            def.RunIndex        = runIndex;
            // conditionChain is unused after the Condition field moved off
            // DefBase — the tree walker uses the AST directly for nesting
            // visualization, and the emitter is line-range-based so the
            // wrapping `if/elif/else` source stays put. The parameter is
            // kept for now to avoid churning every call site; remove on
            // next pass through this file if it really stays unused.
            _ = conditionChain;
        }

        // Recognise calls into the CustomAssets Python API surface so we can
        // capture them as definition statements. The runtime API exposes
        // build_*, add_*, and a handful of helper functions (Product, Mat,
        // Texâ€¦); we filter on the verb-prefix to avoid capturing helper-
        // constructor calls that aren't top-level definitions.
        private static bool isCustomAssetsApiCall(string callName) {
            if (string.IsNullOrEmpty(callName)) return false;
            if (callName.StartsWith("build_") || callName.StartsWith("add_")) return true;
            // `edit_*` calls mutate existing protos; they belong to the same
            // surface as build/add and should show up in the tree.
            return callName.StartsWith("edit_");
        }

        // Pull the primary id from a build_*/add_* call's arguments. Checks,
        // in priority order:
        //   1. Named arg whose name ends with "Id" (recipeId, productId, â€¦)
        //   2. Named arg called "path" (asset registrations use this)
        //   3. First positional argument
        // The id is unwrapped via ExpressionToId so typed refs (Ids.X.Y) and
        // string literals both produce a readable id. Returns the literal
        // "<no id>" when nothing matched â€” better than null so tree labels
        // never render empty.
        private static string extractPrimaryId(CallExpression call) {
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    string n = named.Name ?? "";
                    if (n.EndsWith("Id") || n == "path") return ExpressionToId(named.Expression);
                }
            }
            foreach (IArgument arg in call.Arguments) {
                if (arg is OrderedArgument) return ExpressionToId(arg.Expression);
            }
            return "<no id>";
        }

        // Pull a display name from a build_*/add_* call's `name` named arg
        // when present, or null when the call doesn't carry one. Used for
        // tree labels alongside the id.
        private static string extractPrimaryName(CallExpression call) {
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named && named.Name == "name") {
                    return ExpressionToString(named.Expression);
                }
            }
            return null;
        }

        // ---- AST predicates ----------------------------------------------------

        private static bool isCallTo(CallExpression call, string fnName) {
            if (call.Calle is VariableExpression v) return v.Path == fnName;
            return false;
        }
    }
}
