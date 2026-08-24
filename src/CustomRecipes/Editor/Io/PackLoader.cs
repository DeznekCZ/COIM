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
        private const string EditRecipeFnName      = "edit_recipe";
        private const string BindRecipeFnName      = "bind_recipe";
        private const string ProductFnName         = "Product";
        private const string PortMapFnName         = "PortMap";
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
                System.Collections.Generic.Dictionary<string, string> fileVariables,
                // Non-null while walking the body of a `with build_recipe(...)`
                // block: bind_recipe(...) statements there omit the recipe (context
                // form) and belong to this recipe.
                RecipeDef contextRecipe = null,
                // Non-null while walking a `with edit_recipe(recipe):` body. Carries
                // just the recipe id — the edit sub-actions (set_/remove_ingredient,
                // set_/remove_product, bind_recipe, unbind_recipe) omit the recipe
                // and refer to this one. Unlike contextRecipe there is no owner def
                // to link: the body renders by scope, not by ownership.
                string contextEditRecipeId = null) {
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
                        sourceFile, sourceLines, conditionChain, scopeKey, runIndex, model, fileVariables,
                        contextRecipe, contextEditRecipeId);
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
                                  sourceFile, sourceLines, conditionChain, scopeKey, runIndex, model, fileVariables,
                        contextRecipe, contextEditRecipeId);
                    continue;
                }
                // `with <expr> [as var]:` — a block statement, handled exactly like
                // an if-clause: the HEADER becomes its own def occupying only the
                // header lines, and the BODY is walked recursively under its own
                // scope key. That keeps every body statement independently
                // addressable (own source range ⇒ own save / delete / reorder) and
                // lets blocks nest to any depth in either order.
                if (statement is WithStatement ws) {
                    if (pendingRunBump) { runIndex++; pendingRunBump = false; }

                    // `with build_recipe(...)` — the header is a recipe definition.
                    // `with edit_recipe(...)` — the header is an edit definition.
                    // Either makes the body a context: build binds bind_recipe to
                    // the recipe; edit routes its sub-actions to the edited recipe.
                    RecipeDef ownerRecipe = null;
                    string ownerEditRecipeId = null;
                    string hdrName = ws.ContextExpr is CallExpression hc
                        ? calleeAsPath(hc.Calle) : null;
                    if (hdrName == EditRecipeFnName) {
                        CallExpression editCall = (CallExpression)ws.ContextExpr;
                        EditRecipeDef ownerEdit = parseEditRecipe(editCall);
                        ownerEdit.VariableName = ws.AsName;
                        ownerEdit.EmitAsWithBlock = true;
                        attachSourceLocation(ownerEdit, ws.StartLine,
                            withHeaderEndLine(ws), sourceFile, conditionChain,
                            "blockheader:" + ws.StartLine, 0);
                        ownerEdit.SourceFileVariables = fileVariables;
                        if (sourceLines != null) extractAndAttachComment(ownerEdit, sourceLines);
                        model.Definitions.Add(ownerEdit);
                        ownerEditRecipeId = ownerEdit.RecipeId;
                        if (!string.IsNullOrEmpty(ws.AsName)
                                && !string.IsNullOrEmpty(ownerEdit.RecipeId)) {
                            fileVariables[ws.AsName] = ownerEdit.RecipeId;
                        }
                    } else if (ws.ContextExpr is CallExpression hdrCall
                            && calleeAsPath(hdrCall.Calle) == BuildRecipeFnName) {
                        ownerRecipe = parseBuildRecipe(hdrCall);
                        // The `as <var>` clause plays the role the `<var> = `
                        // assignment prefix plays for a plain statement.
                        ownerRecipe.VariableName = ws.AsName;
                        ownerRecipe.EmitAsWithBlock = true;

                        // Range covers the HEADER ONLY (`with build_recipe(...):`),
                        // not the body — body statements splice themselves.
                        //
                        // Scope is the block's own "blockheader:<line>" rather than the
                        // enclosing run: the tree renders this row inside the block's
                        // group (walking the AST), so it must NOT also be picked up as
                        // an ordinary member of the surrounding run or it would render
                        // twice, in two different places.
                        attachSourceLocation(ownerRecipe, ws.StartLine,
                            withHeaderEndLine(ws), sourceFile, conditionChain,
                            "blockheader:" + ws.StartLine, 0);
                        ownerRecipe.SourceFileVariables = fileVariables;
                        if (sourceLines != null) extractAndAttachComment(ownerRecipe, sourceLines);
                        model.Definitions.Add(ownerRecipe);

                        if (!string.IsNullOrEmpty(ws.AsName)
                                && !string.IsNullOrEmpty(ownerRecipe.RecipeId)) {
                            fileVariables[ws.AsName] = ownerRecipe.RecipeId;
                        }
                    }

                    if (ws.Block != null) {
                        walkStatements(ws.Block.statements, sourceFile, sourceLines,
                                       conditionChain, "block:" + ws.StartLine, model, fileVariables,
                                       contextRecipe: ownerRecipe,
                                       contextEditRecipeId: ownerEditRecipeId);
                    }
                    // Like an if-chain, a block ends the current run.
                    pendingRunBump = true;
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
                            string clauseScope = "block:" + clause.StartLine;
                            // An `if` nested inside a `with build_recipe(...)` body is
                            // still in that recipe's context, so bind_recipe(...) calls
                            // inside the clause keep binding to it.
                            walkStatements(clause.Block.statements, sourceFile, sourceLines,
                                           childChain, clauseScope, model, fileVariables,
                                           contextRecipe, contextEditRecipeId);
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
                case 5: def.DurationSeconds = ExpressionToDurationSeconds(expr);
                        def.DurationExpression = ExpressionToDurationText(expr); return;
                case 6: def.Ingredients = ExpressionToProductList(expr); return;
                case 7: def.Products    = ExpressionToProductList(expr); return;
                case 8: def.PowerPercent = ExpressionToPowerPercent(expr); return;
                // 9 is `replaces`; 10 is `unlock_machine` — both realistically
                // only ever written by name, but the slots must line up with the
                // Constructor's argument array either way.
                case 9: def.Replaces    = ExpressionToIdList(expr); return;
                case 10: def.UnlockMachine = ExpressionToUnlockMachine(expr); return;
            }
        }

        private static void bindNamedArg(RecipeDef def, string name, IExpression expr) {
            switch (name) {
                case "recipeId":    def.RecipeId    = ExpressionToId(expr); break;
                case "name":        def.Name        = ExpressionToString(expr); break;
                case "description": def.Description = ExpressionToString(expr); break;
                case "machine":     def.MachineId   = ExpressionToId(expr); break;
                case "research":    def.ResearchId  = ExpressionToIdOrNull(expr); break;
                case "duration":    def.DurationSeconds = ExpressionToDurationSeconds(expr);
                                    def.DurationExpression = ExpressionToDurationText(expr); break;
                case "ingredients": def.Ingredients = ExpressionToProductList(expr); break;
                case "products":    def.Products    = ExpressionToProductList(expr); break;
                case "power":       def.PowerPercent = ExpressionToPowerPercent(expr); break;
                case "replaces":    def.Replaces    = ExpressionToIdList(expr); break;
                case "unlock_machine": def.UnlockMachine = ExpressionToUnlockMachine(expr); break;
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

        /// Source text for a quantity slot that is NOT a plain number, or null when it
        /// is one (the caller already has the number) or cannot be printed.
        /// `Quantity(config.batch)` â†’ `config.batch`, matching how ExpressionToInt
        /// unwraps the Quantity(...) call — the model stores the inner value either way.
        ///
        /// Without this the loader dropped every non-literal argument and the emitter
        /// wrote the int back, silently turning `Quantity(config.batch)` into
        /// `Quantity(0)` the first time the recipe was saved.
        public static string ExpressionToQuantityText(IExpression expr) {
            if (expr == null || expr is NoneConst) return null;
            if (expr is CallExpression qcall
                && qcall.Calle is VariableExpression qv
                && qv.Path == QuantityFnName) {
                foreach (IArgument a in qcall.Arguments) {
                    if (a is OrderedArgument) return ExpressionPrinter.PrintIfNotLiteral(a.Expression);
                }
                return null;
            }
            return ExpressionPrinter.PrintIfNotLiteral(expr);
        }

        /// Source text for a duration slot that is not a plain number, expressed in
        /// SECONDS so it can be re-emitted as `Duration.FromSec(<text>)`. A minutes
        /// form keeps its meaning by being scaled: `Duration.FromMin(config.x)` â†’
        /// `(config.x) * 60`.
        public static string ExpressionToDurationText(IExpression expr) {
            if (expr == null || expr is NoneConst) return null;
            if (expr is CallExpression call) {
                string calleePath = calleeAsPath(call.Calle);
                foreach (IArgument a in call.Arguments) {
                    if (!(a is OrderedArgument)) continue;
                    string text = ExpressionPrinter.PrintIfNotLiteral(a.Expression);
                    if (text == null) return null;
                    if (calleePath == DurationFnName || calleePath == DurationFromSecName) {
                        return text;
                    }
                    if (calleePath == DurationFromMinName) {
                        return "(" + text + ") * 60";
                    }
                    return null;
                }
                return null;
            }
            return ExpressionPrinter.PrintIfNotLiteral(expr);
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

        /// `["a", "b"]` or a bare `"a"` → id list. Used by build_recipe's `replaces`,
        /// which accepts either shape (a single supersession is common enough that
        /// requiring a one-element list would be noise). `None` → empty.
        public static List<string> ExpressionToIdList(IExpression expr) {
            List<string> result = new List<string>();
            if (expr is NoneConst) return result;
            if (expr is ListExpression list) {
                foreach (IExpression item in list.Items) {
                    string id = ExpressionToId(item);
                    if (!string.IsNullOrWhiteSpace(id)) result.Add(id);
                }
                return result;
            }
            string single = ExpressionToId(expr);
            if (!string.IsNullOrWhiteSpace(single)) result.Add(single);
            return result;
        }

        /// `[PortMap(product, port), …]` → PortMapRef list. Used by bind_recipe's
        /// `ports` argument (quantity-free, unlike ExpressionToProductList).
        public static List<PortMapRef> ExpressionToPortMapList(IExpression expr) {
            List<PortMapRef> result = new List<PortMapRef>();
            if (expr is NoneConst) return result;
            if (!(expr is ListExpression list)) return result;
            foreach (IExpression item in list.Items) {
                if (item is CallExpression pmCall && isCallTo(pmCall, PortMapFnName)) {
                    result.Add(parsePortMapCall(pmCall));
                }
            }
            return result;
        }

        /// PortMap(product, port).
        private static PortMapRef parsePortMapCall(CallExpression call) {
            PortMapRef pm = new PortMapRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "product": pm.ProductId = ExpressionToId(named.Expression); break;
                        case "port":    pm.Port      = ExpressionToString(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: pm.ProductId = ExpressionToId(arg.Expression); break;
                        case 1: pm.Port      = ExpressionToString(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return pm;
        }

        /// Product(product, quantity, port="*").
        private static ProductRef parseProductCall(CallExpression call) {
            ProductRef pr = new ProductRef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "product":  pr.ProductId = ExpressionToId(named.Expression); break;
                        case "quantity": pr.Quantity  = ExpressionToInt(named.Expression) ?? 0;
                                         pr.QuantityExpression = ExpressionToQuantityText(named.Expression); break;
                        case "port":     pr.Port      = ExpressionToString(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: pr.ProductId = ExpressionToId(arg.Expression); break;
                        case 1: pr.Quantity  = ExpressionToInt(arg.Expression) ?? 0;
                                pr.QuantityExpression = ExpressionToQuantityText(arg.Expression); break;
                        case 2: pr.Port      = ExpressionToString(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return pr;
        }

        /// Convert a legacy one-shot `build_recipe(machine=…, duration=…, research=…)`
        /// into the split model AT LOAD TIME, so everything downstream (editor,
        /// emitter) only ever sees the new shape and the next save writes a
        /// `with build_recipe(...):` block.
        ///
        /// The inline machine becomes a single binding carrying the duration, the
        /// research, and the per-`Product` port pins — the ports move onto the
        /// binding the same way the machine does, because in the split model ports
        /// belong to the (recipe, machine) pair, not to the recipe. The recipe's
        /// own `Product` entries therefore lose their `port` here, which is also
        /// why the recipe form no longer shows a port column.
        ///
        /// <see cref="RecipeDef.LoadedAsLegacy"/> is set so the editor can warn
        /// that the FILE is still in the old format until it is saved.
        private static void migrateLegacyRecipe(RecipeDef def, PackModel model, string sourceFile) {
            if (def == null || string.IsNullOrEmpty(def.MachineId)) return;

            def.LoadedAsLegacy = true;
            // The recipe becomes a `with` block on the next save; its binding is a
            // PENDING statement (no source range of its own yet) that the save flow
            // inserts into the newly written block body.
            def.EmitAsWithBlock = true;

            BindRecipeDef bind = new BindRecipeDef {
                OwnerRecipe         = def,
                RecipeId            = def.RecipeId,
                MachineId           = def.MachineId,
                DurationSeconds     = def.DurationSeconds,
                DurationExpression  = def.DurationExpression,
                ResearchId          = def.ResearchId,
                UnlockMachine       = def.UnlockMachine,
                SourceFile          = sourceFile,
                SourceFileVariables = def.SourceFileVariables,
                // NO ScopeKey/RunIndex: a pending owned binding has no place in
                // any run. It is reachable only through OwnerRecipe, drawn only
                // inside its recipe's group, and written only as part of the
                // recipe's `with` body. Copying the recipe's scope here is what
                // made it render a second time as a loose sibling.
                Ports               = new List<PortMapRef>(),
            };

            foreach (List<ProductRef> side in new[] { def.Ingredients, def.Products }) {
                if (side == null) continue;
                foreach (ProductRef p in side) {
                    if (!string.IsNullOrEmpty(p.Port) && p.Port != "*"
                            && !string.IsNullOrEmpty(p.ProductId)) {
                        bind.Ports.Add(new PortMapRef(p.ProductId, p.Port));
                    }
                    p.Port = null;   // ports now live on the binding
                }
            }

            model?.Definitions.Add(bind);
            def.MachineId       = null;
            def.DurationSeconds = null;
            def.ResearchId      = null;
            def.UnlockMachine   = true;   // back to the neutral default; it lives on the binding now
        }

        /// Last line of a `with …:` block HEADER — i.e. the line carrying the
        /// colon, which is the line before the first body statement. The recipe
        /// def occupies only this range so its body statements can splice
        /// themselves independently. Falls back to the block's end for an empty
        /// body (the grammar shouldn't produce one, but a `pass` body would).
        private static int withHeaderEndLine(WithStatement ws) {
            int firstBody = firstStatementLine(ws.Block);
            return firstBody > ws.StartLine ? firstBody - 1 : ws.EndLine;
        }

        /// Source line of the first statement in a block, or 0 when unknown.
        /// IStatement has no common line member, so match the kinds the parser
        /// can actually produce inside a block body.
        private static int firstStatementLine(Block block) {
            if (block == null) return 0;
            foreach (IStatement s in block.statements) {
                if (s is EvaluateStatement e)   return e.StartLine;
                if (s is AssignmentStatement a) return a.StartLine;
                if (s is IfStatement i)         return i.StartLine;
                if (s is WithStatement w)       return w.StartLine;
            }
            return 0;
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
                System.Collections.Generic.Dictionary<string, string> fileVariables,
                RecipeDef contextRecipe = null,
                string contextEditRecipeId = null) {
            // Inside a `with edit_recipe(recipe):` body the recipe is implicit, so
            // the sub-action verbs carry only their own operands. Handle them
            // before the generic dispatch so they never fall through to UnknownDef.
            if (contextEditRecipeId != null) {
                DefBase editChild = parseEditSubAction(callName, call, contextEditRecipeId);
                if (editChild != null) {
                    attachSourceLocation(editChild, startLine, endLine, sourceFile,
                        conditionChain, scopeKey, runIndex);
                    editChild.SourceFileVariables = fileVariables;
                    if (sourceLines != null) extractAndAttachComment(editChild, sourceLines);
                    model.Definitions.Add(editChild);
                    return editChild;
                }
            }

            if (callName == BuildRecipeFnName) {
                RecipeDef def = parseBuildRecipe(call);
                attachSourceLocation(def, startLine, endLine, sourceFile, conditionChain, scopeKey, runIndex);
                def.SourceFileVariables = fileVariables;
                if (sourceLines != null) extractAndAttachComment(def, sourceLines);
                migrateLegacyRecipe(def, model, sourceFile);
                model.Definitions.Add(def);
                return def;
            }

            // Inside a `with build_recipe(...)` body the recipe is implicit, so the
            // call's first positional is the MACHINE. Parse it in context form and
            // link it to the owning recipe; elsewhere it's a standalone binding that
            // names its recipe explicitly.
            if (callName == BindRecipeFnName && contextRecipe != null) {
                BindRecipeDef bind = parseBindRecipe(call, contextRecipe.RecipeId);
                bind.OwnerRecipe = contextRecipe;
                attachSourceLocation(bind, startLine, endLine, sourceFile, conditionChain, scopeKey, runIndex);
                bind.SourceFileVariables = fileVariables;
                if (sourceLines != null) extractAndAttachComment(bind, sourceLines);
                model.Definitions.Add(bind);
                return bind;
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
                case "bind_recipe":         return parseBindRecipe(call);
                case "add_unlock_recipe":   return parseUnlockRecipe(call);
                case "migrate_recipe":      return parseMigrateRecipe(call);
                case "add_unlock_product":  return parseUnlockProduct(call);
                case "add_unlock_machine":  return parseUnlockMachine(call);
                case "add_unlock_entity":   return parseUnlockEntity(call);
                case "remove_unlock":       return parseRemoveUnlock(call);
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
                case "edit_entity_costs":   return parseEditEntityCosts(call);
                case "build_machine":       return parseBuildMachine(call);
                case "build_housing":       return parseBuildHousing(call);
                case "build_settlement_decoration": return parseSettlementDecoration(call);
                case "build_settlement_food":       return parseSettlementFood(call);
                case "build_settlement_isp":        return parseSettlementIsp(call);
                case "build_hospital":              return parseHospital(call);
                case "build_mine_tower":            return parseMineTower(call);
                case "build_farm":                  return parseFarm(call);
                case "add_crop":                    return parseAddCrop(call);
                case "clone_crop":                  return parseCloneCrop(call);
                case "edit_crop":                   return parseEditCrop(call);
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

        // bind_recipe(recipe, machine, duration, ports, multiplier,
        // minPartialUtilization, research) → BindRecipeDef. Positional order
        // mirrors the Python signature; `ports` is a Product(...) list where
        // only product + port matter.
        /// <paramref name="contextRecipeId"/> non-null ⇒ the call sits inside a
        /// `with build_recipe(...)` block, so the recipe is implicit and the
        /// positionals shift by one (the first is the MACHINE).
        private static BindRecipeDef parseBindRecipe(CallExpression call, string contextRecipeId = null) {
            BindRecipeDef def = new BindRecipeDef();
            bool contextForm = contextRecipeId != null;
            def.IsContextForm = contextForm;
            if (contextForm) def.RecipeId = contextRecipeId;
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    bindBindRecipeNamed(def, named.Name, named.Expression);
                } else {
                    bindBindRecipePositional(def, contextForm ? positional + 1 : positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        // The sub-actions that live inside `with edit_recipe(recipe):`. bind_recipe
        // reuses the context-form binding parser; the rest are edit-specific.
        // Returns null for a call that is not an edit sub-action, so the caller
        // falls through to the ordinary dispatch (e.g. a build_research nested in
        // an edit block is still a build_research).
        private static DefBase parseEditSubAction(string callName, CallExpression call,
                string contextRecipeId) {
            switch (callName) {
                case BindRecipeFnName: {
                    // Context form: recipe implicit, first positional is the machine.
                    BindRecipeDef bind = parseBindRecipe(call, contextRecipeId);
                    return bind;
                }
                case "set_ingredient":    return parseProductAction(call, contextRecipeId, isInput: true,  isRemoval: false);
                case "set_product":       return parseProductAction(call, contextRecipeId, isInput: false, isRemoval: false);
                case "remove_ingredient": return parseProductAction(call, contextRecipeId, isInput: true,  isRemoval: true);
                case "remove_product":    return parseProductAction(call, contextRecipeId, isInput: false, isRemoval: true);
                case "unbind_recipe":     return parseUnbindRecipe(call, contextRecipeId);
            }
            return null;
        }

        // set_ingredient(product, quantity) / set_product(product, quantity) /
        // remove_ingredient(product) / remove_product(product). The removal forms
        // take a single product positional; the set forms add a quantity (bare int
        // or Quantity(N)).
        private static RecipeProductActionDef parseProductAction(CallExpression call,
                string contextRecipeId, bool isInput, bool isRemoval) {
            RecipeProductActionDef def = new RecipeProductActionDef {
                RecipeId  = contextRecipeId,
                IsInput   = isInput,
                IsRemoval = isRemoval,
            };
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "product":  def.ProductId = ExpressionToId(named.Expression); break;
                        case "quantity": def.Quantity  = ExpressionToInt(named.Expression); break;
                    }
                } else {
                    if (positional == 0) def.ProductId = ExpressionToId(arg.Expression);
                    else if (positional == 1 && !isRemoval) def.Quantity = ExpressionToInt(arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        // unbind_recipe(machine, research=None) in context form.
        private static UnbindRecipeDef parseUnbindRecipe(CallExpression call, string contextRecipeId) {
            UnbindRecipeDef def = new UnbindRecipeDef { RecipeId = contextRecipeId };
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "machine":  def.MachineId  = ExpressionToId(named.Expression); break;
                        case "research": def.ResearchId = ExpressionToIdOrNull(named.Expression); break;
                    }
                } else {
                    if (positional == 0) def.MachineId = ExpressionToId(arg.Expression);
                    else if (positional == 1) def.ResearchId = ExpressionToIdOrNull(arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindBindRecipePositional(BindRecipeDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.RecipeId  = ExpressionToId(expr); return;
                case 1: def.MachineId = ExpressionToId(expr); return;
                case 2: def.DurationSeconds = ExpressionToDurationSeconds(expr);
                        def.DurationExpression = ExpressionToDurationText(expr); return;
                case 3: def.Ports = ExpressionToPortMapList(expr); return;
                case 4: def.Multiplier = ExpressionToInt(expr); return;
                case 5: def.MinPartialUtilizationPercent = ExpressionToPowerPercent(expr); return;
                case 6: def.ResearchId = ExpressionToIdOrNull(expr); return;
                case 7: def.UnlockMachine = ExpressionToUnlockMachine(expr); return;
            }
        }

        private static void bindBindRecipeNamed(BindRecipeDef def, string name, IExpression expr) {
            switch (name) {
                case "recipe":     def.RecipeId  = ExpressionToId(expr); break;
                case "machine":    def.MachineId = ExpressionToId(expr); break;
                case "duration":   def.DurationSeconds = ExpressionToDurationSeconds(expr);
                                   def.DurationExpression = ExpressionToDurationText(expr); break;
                case "ports":      def.Ports = ExpressionToPortMapList(expr); break;
                case "multiplier": def.Multiplier = ExpressionToInt(expr); break;
                case "minPartialUtilization": def.MinPartialUtilizationPercent = ExpressionToPowerPercent(expr); break;
                case "research":   def.ResearchId = ExpressionToIdOrNull(expr); break;
                case "unlock_machine": def.UnlockMachine = ExpressionToUnlockMachine(expr); break;
            }
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

        // build_farm(farmId, source, name, description,
        //     yieldMultiplierPercent, demandsMultiplierPercent,
        //     fertilityReplenishPercent, waterCollected,
        //     waterEvaporationPerDay, hasIrrigationAndFertilizerSupport,
        //     isGreenhouse, research, lockedOnInit)
        private static FarmDef parseFarm(CallExpression call) {
            var def = new FarmDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindFarmNamed(def, na.Name, na.Expression);
                else bindFarmPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        private static void bindFarmNamed(FarmDef def, string name, IExpression expr) {
            switch (name) {
                case "farmId":                  def.FarmId                  = ExpressionToId(expr); break;
                case "source":                  def.SourceId                = ExpressionToId(expr); break;
                case "name":                    def.Name                    = ExpressionToString(expr); break;
                case "description":             def.Description             = ExpressionToString(expr); break;
                case "yieldMultiplierPercent":  def.YieldMultiplierPercent  = ExpressionToInt(expr); break;
                case "demandsMultiplierPercent":def.DemandsMultiplierPercent= ExpressionToInt(expr); break;
                case "fertilityReplenishPercent":def.FertilityReplenishPercent = ExpressionToInt(expr); break;
                case "waterCollected":          bindFarmWaterCollected(def, expr); break;
                case "waterEvaporationPerDay":  def.WaterEvaporationPerDay  = ExpressionToInt(expr); break;
                case "hasIrrigationAndFertilizerSupport":
                    def.HasIrrigationAndFertilizerSupport = ExpressionToBool(expr); break;
                case "isGreenhouse":            def.IsGreenhouse            = ExpressionToBool(expr); break;
                case "layout_str":              def.LayoutSourceStr         = ExpressionToString(expr); break;
                case "research":                def.ResearchId              = ExpressionToIdOrNull(expr); break;
                case "lockedOnInit":            def.LockedOnInit            = ExpressionToBool(expr); break;
            }
        }
        private static void bindFarmPositional(FarmDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0:  def.FarmId                    = ExpressionToId(expr); break;
                case 1:  def.SourceId                  = ExpressionToId(expr); break;
                case 2:  def.Name                      = ExpressionToString(expr); break;
                case 3:  def.Description               = ExpressionToString(expr); break;
                case 4:  def.YieldMultiplierPercent    = ExpressionToInt(expr); break;
                case 5:  def.DemandsMultiplierPercent  = ExpressionToInt(expr); break;
                case 6:  def.FertilityReplenishPercent = ExpressionToInt(expr); break;
                case 7:  bindFarmWaterCollected(def, expr); break;
                case 8:  def.WaterEvaporationPerDay    = ExpressionToInt(expr); break;
                case 9:  def.HasIrrigationAndFertilizerSupport = ExpressionToBool(expr); break;
                case 10: def.IsGreenhouse              = ExpressionToBool(expr); break;
                case 11: def.ResearchId                = ExpressionToIdOrNull(expr); break;
                case 12: def.LockedOnInit              = ExpressionToBool(expr); break;
            }
        }
        // waterCollected accepts a Product(...) call — reuses the recipe
        // Product wrapper so modders don't learn a second literal shape.
        // Both halves land on the FarmDef; the runtime converts to
        // PartialProductQuantity at registration time.
        private static void bindFarmWaterCollected(FarmDef def, IExpression expr) {
            if (expr is CallExpression productCall && isCallTo(productCall, ProductFnName)) {
                ProductRef p = parseProductCall(productCall);
                def.WaterCollectedProductId = p.ProductId;
                def.WaterCollectedQuantity  = p.Quantity;
            }
        }

        // edit_crop(crop, productProduced, multiplyYieldPercent,
        //     growthDurationDays, consumedWaterPerDay,
        //     consumedFertilityPercentPerDay, minFertilityToStartGrowthPercent,
        //     surviveWithNoWaterDays, requiresGreenhouse, plantByDefault).
        // Argument names match add_crop / clone_crop exactly so a modder who
        // knows those already knows this one; only `crop` is required.
        private static EditCropDef parseEditCrop(CallExpression call) {
            EditCropDef def = new EditCropDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments)
            {
                if (arg is NamedArgument named)
                {
                    bindEditCropArg(def, named.Name, named.Expression);
                }
                else
                {
                    bindEditCropPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindEditCropPositional(EditCropDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: bindEditCropArg(def, "crop",                             expr); return;
                case 1: bindEditCropArg(def, "productProduced",                  expr); return;
                case 2: bindEditCropArg(def, "multiplyYieldPercent",             expr); return;
                case 3: bindEditCropArg(def, "growthDurationDays",               expr); return;
                case 4: bindEditCropArg(def, "consumedWaterPerDay",              expr); return;
                case 5: bindEditCropArg(def, "consumedFertilityPercentPerDay",   expr); return;
                case 6: bindEditCropArg(def, "minFertilityToStartGrowthPercent", expr); return;
                case 7: bindEditCropArg(def, "surviveWithNoWaterDays",           expr); return;
                case 8: bindEditCropArg(def, "requiresGreenhouse",               expr); return;
                case 9: bindEditCropArg(def, "plantByDefault",                   expr); return;
            }
        }

        private static void bindEditCropArg(EditCropDef def, string name, IExpression expr) {
            switch (name) {
                case "crop":            def.CropId = ExpressionToId(expr); break;
                case "productProduced": bindEditCropProduct(def, expr); break;
                case "multiplyYieldPercent":             def.MultiplyYieldPercent             = ExpressionToInt(expr); break;
                case "growthDurationDays":               def.GrowthDurationDays               = ExpressionToInt(expr); break;
                case "consumedWaterPerDay":              def.ConsumedWaterPerDay              = ExpressionToInt(expr); break;
                case "consumedFertilityPercentPerDay":   def.ConsumedFertilityPercentPerDay   = ExpressionToInt(expr); break;
                case "minFertilityToStartGrowthPercent": def.MinFertilityToStartGrowthPercent = ExpressionToInt(expr); break;
                case "surviveWithNoWaterDays":           def.SurviveWithNoWaterDays           = ExpressionToInt(expr); break;
                case "requiresGreenhouse":               def.RequiresGreenhouse               = ExpressionToBoolOrNull(expr); break;
                case "plantByDefault":                   def.PlantByDefault                   = ExpressionToBoolOrNull(expr); break;
            }
        }

        // Same Product(...) wrapper add_crop uses for productProduced, split
        // across the two flat model fields.
        private static void bindEditCropProduct(EditCropDef def, IExpression expr) {
            if (expr is CallExpression productCall && isCallTo(productCall, ProductFnName))
            {
                ProductRef p = parseProductCall(productCall);
                def.ProductProducedId       = p.ProductId;
                def.ProductProducedQuantity = p.Quantity;
            }
        }

        // add_crop(cropId, name, productProduced, consumedWaterPerDay,
        //     consumedFertilityPercentPerDay, minFertilityToStartGrowthPercent,
        //     growthDurationDays, surviveWithNoWaterDays, icon, prefab,
        //     requiresGreenhouse, plantByDefault, description, research,
        //     farms)
        private static CropDef parseAddCrop(CallExpression call) {
            var def = new CropDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) bindCropNamed(def, na.Name, na.Expression);
                else bindCropPositional(def, positional++, arg.Expression);
            }
            return def;
        }
        // clone_crop(cropId, source, …other tunables inherited from add_crop…) —
        // shares CropDef with add_crop; the only difference is SourceId
        // being set. Emitter picks the write function accordingly.
        private static CropDef parseCloneCrop(CallExpression call) {
            var def = new CropDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument na) {
                    if (na.Name == "source") def.SourceId = ExpressionToId(na.Expression);
                    else bindCropNamed(def, na.Name, na.Expression);
                } else {
                    // clone_crop's positional order is (cropId, source, …),
                    // shifting every non-source add_crop position by +1
                    // starting at index 2. Handle explicitly for the first
                    // two, then pass through to the shared add_crop binder
                    // (positional-1) for the rest.
                    switch (positional) {
                        case 0: def.CropId  = ExpressionToId(arg.Expression); break;
                        case 1: def.SourceId = ExpressionToId(arg.Expression); break;
                        default:
                            // Positional index 2 in clone_crop = index 1 in add_crop
                            // (`name`), etc. Reuse the shared binder.
                            bindCropPositional(def, positional - 1, arg.Expression);
                            break;
                    }
                    positional++;
                }
            }
            return def;
        }
        private static void bindCropNamed(CropDef def, string name, IExpression expr) {
            switch (name) {
                case "cropId":                          def.CropId                          = ExpressionToId(expr); break;
                case "source":                          def.SourceId                        = ExpressionToId(expr); break;
                case "name":                            def.Name                            = ExpressionToString(expr); break;
                case "description":                     def.Description                     = ExpressionToString(expr); break;
                case "productProduced":                 def.ProductProduced                 = expressionToSingleProduct(expr); break;
                case "consumedWaterPerDay":             def.ConsumedWaterPerDay             = ExpressionToInt(expr); break;
                case "consumedFertilityPercentPerDay":  def.ConsumedFertilityPercentPerDay  = ExpressionToInt(expr); break;
                case "minFertilityToStartGrowthPercent":def.MinFertilityToStartGrowthPercent= ExpressionToInt(expr); break;
                case "growthDurationDays":              def.GrowthDurationDays              = ExpressionToInt(expr); break;
                case "surviveWithNoWaterDays":          def.SurviveWithNoWaterDays          = ExpressionToInt(expr); break;
                case "icon":                            def.IconPath                        = ExpressionToIconPath(expr); break;
                case "prefab":                          def.PrefabPath                      = ExpressionToString(expr); break;
                case "requiresGreenhouse":              def.RequiresGreenhouse              = ExpressionToBool(expr); break;
                case "plantByDefault":                  def.PlantByDefault                  = ExpressionToBool(expr); break;
                case "research":                        def.ResearchId                      = ExpressionToIdOrNull(expr); break;
                case "farms":                           def.Farms                           = expressionToIdList(expr); break;
            }
        }
        private static void bindCropPositional(CropDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0:  def.CropId                          = ExpressionToId(expr); break;
                case 1:  def.Name                            = ExpressionToString(expr); break;
                case 2:  def.ProductProduced                 = expressionToSingleProduct(expr); break;
                case 3:  def.ConsumedWaterPerDay             = ExpressionToInt(expr); break;
                case 4:  def.ConsumedFertilityPercentPerDay  = ExpressionToInt(expr); break;
                case 5:  def.MinFertilityToStartGrowthPercent= ExpressionToInt(expr); break;
                case 6:  def.GrowthDurationDays              = ExpressionToInt(expr); break;
                case 7:  def.SurviveWithNoWaterDays          = ExpressionToInt(expr); break;
                case 8:  def.IconPath                        = ExpressionToIconPath(expr); break;
                case 9:  def.PrefabPath                      = ExpressionToString(expr); break;
                case 10: def.RequiresGreenhouse              = ExpressionToBool(expr); break;
                case 11: def.PlantByDefault                  = ExpressionToBool(expr); break;
                case 12: def.Description                     = ExpressionToString(expr); break;
                case 13: def.ResearchId                      = ExpressionToIdOrNull(expr); break;
                case 14: def.Farms                           = expressionToIdList(expr); break;
            }
        }
        // Product(...) call → single ProductRef. Reuses parseProductCall
        // so a hand-typed named-arg form still round-trips. Non-Product
        // expressions (None, bare id) return null — the runtime treats
        // that as "no product produced" (cover crop).
        private static ProductRef expressionToSingleProduct(IExpression expr) {
            if (expr is CallExpression productCall && isCallTo(productCall, ProductFnName)) {
                return parseProductCall(productCall);
            }
            return null;
        }
        // `[Ids.X, "Y", z]` → list of string ids/variable-names. Reused
        // by CropDef.Farms (which lists farm ids the crop is restricted
        // to). Empty / None / non-list → empty list. Each entry stored
        // verbatim so typed-refs and bare identifiers round-trip.
        private static List<string> expressionToIdList(IExpression expr) {
            List<string> result = new List<string>();
            if (expr is NoneConst) return result;
            if (!(expr is ListExpression list)) return result;
            foreach (IExpression item in list.Items) {
                string id = ExpressionToIdOrNull(item);
                if (!string.IsNullOrEmpty(id)) result.Add(id);
            }
            return result;
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
                case 9: def.AutoSelectRecipes     = ExpressionToBoolOrNull(expr); return;
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
                case "auto_select_recipes":  def.AutoSelectRecipes      = ExpressionToBoolOrNull(expr); break;
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
                        case "machine":             def.MachineId         = ExpressionToId(named.Expression); break;
                        case "add_ports":           def.AddPorts          = ExpressionToPortList(named.Expression); break;
                        case "auto_select_recipes": def.AutoSelectRecipes = ExpressionToBoolOrNull(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.MachineId         = ExpressionToId(arg.Expression); break;
                        case 1: def.AddPorts          = ExpressionToPortList(arg.Expression); break;
                        case 2: def.AutoSelectRecipes = ExpressionToBoolOrNull(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // edit_entity_costs(entity, workers, maintenance, maintenanceProduct,
        // maintenanceBufferMonths, initialMaintenancePercent, priority,
        // products, multiplyPercent). Only `entity` is required; every other
        // argument is a per-facet override that stays null when absent.
        //
        // Argument order is deliberate: the staffing/upkeep facets come
        // first because they read as properties OF the entity, and the
        // build-material list — the longest, most-edited argument — comes
        // last so it never buries the short scalars above it.
        private static EditEntityCostsDef parseEditEntityCosts(CallExpression call) {
            EditEntityCostsDef def = new EditEntityCostsDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments)
            {
                if (arg is NamedArgument named)
                {
                    bindEntityCostsArg(def, named.Name, named.Expression);
                }
                else
                {
                    bindEntityCostsPositional(def, positional, arg.Expression);
                    positional++;
                }
            }
            return def;
        }

        private static void bindEntityCostsPositional(EditEntityCostsDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: bindEntityCostsArg(def, "entity",                    expr); return;
                case 1: bindEntityCostsArg(def, "workers",                   expr); return;
                case 2: bindEntityCostsArg(def, "maintenance",               expr); return;
                case 3: bindEntityCostsArg(def, "maintenanceProduct",        expr); return;
                case 4: bindEntityCostsArg(def, "maintenanceBufferMonths",   expr); return;
                case 5: bindEntityCostsArg(def, "initialMaintenancePercent", expr); return;
                case 6: bindEntityCostsArg(def, "priority",                  expr); return;
                case 7: bindEntityCostsArg(def, "products",                  expr); return;
                case 8: bindEntityCostsArg(def, "multiplyPercent",           expr); return;
            }
        }

        private static void bindEntityCostsArg(EditEntityCostsDef def, string name, IExpression expr) {
            switch (name) {
                case "entity":                    def.EntityId                 = ExpressionToId(expr); break;
                case "products":                  def.Products                 = ExpressionToProductList(expr); break;
                case "multiplyPercent":           def.MultiplyPercent          = ExpressionToInt(expr); break;
                case "workers":                   def.Workers                  = ExpressionToInt(expr); break;
                case "priority":                  def.Priority                 = ExpressionToInt(expr); break;
                case "maintenance":               def.Maintenance              = ExpressionToDouble(expr); break;
                case "maintenanceProduct":        def.MaintenanceProductId     = ExpressionToId(expr); break;
                case "maintenanceBufferMonths":   def.MaintenanceBufferMonths  = ExpressionToInt(expr); break;
                case "initialMaintenancePercent": def.InitialMaintenancePercent = ExpressionToInt(expr); break;
            }
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
                case 7: def.UnlockMachine   = ExpressionToUnlockMachine(expr); return;
            }
        }

        private static void bindEditRecipeNamed(EditRecipeDef def, string name, IExpression expr) {
            switch (name) {
                case "recipe":      def.RecipeId        = ExpressionToId(expr); break;
                case "duration":    def.DurationSeconds = ExpressionToDurationSeconds(expr);
                                    def.DurationExpression = ExpressionToDurationText(expr); break;
                case "ingredients": def.Ingredients     = ExpressionToProductList(expr); break;
                case "products":    def.Products        = ExpressionToProductList(expr); break;
                case "machine":     def.MachineId       = ExpressionToIdOrNull(expr); break;
                case "research":    def.ResearchId      = ExpressionToIdOrNull(expr); break;
                case "power":       def.PowerPercent    = ExpressionToPowerPercent(expr); break;
                case "unlock_machine": def.UnlockMachine = ExpressionToUnlockMachine(expr); break;
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
                case "isLocked":        def.IsLocked           = ExpressionToBoolOrNull(expr); break;
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
                case "isLocked":             def.IsLocked                       = ExpressionToBoolOrNull(expr); break;
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
                case "isLocked":                     def.IsLocked                     = ExpressionToBoolOrNull(expr); break;
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

        // Tri-state bool parse: True/False → the value, None / anything else
        // → null. Used by optional flags whose "absent" state is meaningful
        // (e.g. auto_select_recipes, where blank means "inherit").
        private static bool? ExpressionToBoolOrNull(IExpression expr) {
            if (expr is BooleanConst b) return b.BooleanValue;
            return null;
        }

        /// `unlock_machine` reads TRUE for anything that is not a literal
        /// `False`. The argument defaults to true at runtime, and the emitter
        /// writes it out only when false — so misreading an expression the
        /// editor cannot evaluate (`unlock_machine = config.grant_machine`) as
        /// false would spell `unlock_machine = False` into the file on the next
        /// save and quietly change what the node unlocks. Defaulting the
        /// un-evaluable case to true keeps the emit a no-op instead.
        private static bool ExpressionToUnlockMachine(IExpression expr) {
            return ExpressionToBoolOrNull(expr) ?? true;
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

        // Positional order MUST match the `build_research` Constructor's
        // Arguments array in CustomAssetRegistrator — that array is what the
        // runtime binds positional arguments against. When the two disagree the
        // editor reads an argument as one thing and the game reads it as
        // another, and saving rewrites the call into whatever the editor
        // believed. (They did disagree at slots 3 and 5: the editor called them
        // costs/icon, the runtime difficulty/parents.)
        private static void bindResearchPositional(ResearchDef def, int idx, IExpression expr) {
            switch (idx) {
                case 0: def.ResearchId  = ExpressionToId(expr);    return;
                case 1: def.Name        = ExpressionToString(expr); return;
                case 2: def.Description = ExpressionToString(expr); return;
                case 3: bindResearchCosts(def, expr); return;
                case 4: bindResearchPosition(def, expr); return;
                case 5: def.Parents = ExpressionToIdList(expr); return;
                case 6: def.IconPath = ExpressionToIconPath(expr); return;
            }
        }

        private static void bindResearchNamed(ResearchDef def, string name, IExpression expr) {
            switch (name) {
                case "researchId":  def.ResearchId  = ExpressionToId(expr);    break;
                case "name":        def.Name        = ExpressionToString(expr); break;
                case "description": def.Description = ExpressionToString(expr); break;
                case "costs":       bindResearchCosts(def, expr); break;
                // `difficulty` is the name the API docs used for the same
                // value; the runtime reads `costs`. Accept both so a pack
                // written against either spelling survives a round-trip.
                case "difficulty":  bindResearchCosts(def, expr); break;
                case "position":    bindResearchPosition(def, expr); break;
                case "parents":     def.Parents = ExpressionToIdList(expr); break;
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

        // add_unlock_recipe(research, machine, recipe, unlock_machine=False).
        // `proto` is accepted as an alias for `recipe` because older stubs
        // spelled it that way; the runtime Constructor only knows `recipe`, so
        // reading both here keeps the editor from dropping a name it can see.
        private static UnlockRecipeDef parseUnlockRecipe(CallExpression call) {
            UnlockRecipeDef def = new UnlockRecipeDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "research": def.ResearchId = ExpressionToId(named.Expression); break;
                        case "machine":  def.MachineId  = ExpressionToId(named.Expression); break;
                        case "recipe":
                        case "proto":    def.RecipeId   = ExpressionToId(named.Expression); break;
                        case "unlock_machine": def.UnlockMachine = ExpressionToUnlockMachine(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ResearchId = ExpressionToId(arg.Expression); break;
                        case 1: def.MachineId  = ExpressionToId(arg.Expression); break;
                        case 2: def.RecipeId   = ExpressionToId(arg.Expression); break;
                        case 3: def.UnlockMachine = ExpressionToUnlockMachine(arg.Expression); break;
                    }
                    positional++;
                }
            }
            // Unlock kinds expose DisplayId via override (composite key
            // RecipeId @ MachineId) so no separate Id assignment is needed.
            return def;
        }

        // migrate_recipe(old, new, since) â€” two ids plus an optional version string.
        // `old` is deliberately read as a raw id: it names a recipe that no longer
        // exists, so it never resolves to a variable or a live proto.
        private static MigrateRecipeDef parseMigrateRecipe(CallExpression call) {
            MigrateRecipeDef def = new MigrateRecipeDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "old":   def.OldRecipeId = ExpressionToId(named.Expression); break;
                        case "new":   def.NewRecipeId = ExpressionToId(named.Expression); break;
                        case "since": def.Since       = ExpressionToString(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.OldRecipeId = ExpressionToId(arg.Expression); break;
                        case 1: def.NewRecipeId = ExpressionToId(arg.Expression); break;
                        case 2: def.Since       = ExpressionToString(arg.Expression); break;
                    }
                    positional++;
                }
            }
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

        // add_unlock_entity(research, entity) — two id args. Same shape as
        // parseUnlockMachine; kept separate because the second argument has a
        // different name and the two calls stay distinct kinds in the model.
        private static UnlockEntityDef parseUnlockEntity(CallExpression call) {
            UnlockEntityDef def = new UnlockEntityDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments)
            {
                if (arg is NamedArgument named)
                {
                    switch (named.Name) {
                        case "research": def.ResearchId = ExpressionToId(named.Expression); break;
                        case "entity":   def.EntityId   = ExpressionToId(named.Expression); break;
                    }
                }
                else
                {
                    switch (positional) {
                        case 0: def.ResearchId = ExpressionToId(arg.Expression); break;
                        case 1: def.EntityId   = ExpressionToId(arg.Expression); break;
                    }
                    positional++;
                }
            }
            return def;
        }

        // remove_unlock(research, target, machine) — two required ids plus the
        // optional machine scope. One parser for what the add_unlock_* family
        // needs four of, since removal matches by id and never cares which kind
        // of proto the target is.
        private static RemoveUnlockDef parseRemoveUnlock(CallExpression call) {
            RemoveUnlockDef def = new RemoveUnlockDef();
            int positional = 0;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named) {
                    switch (named.Name) {
                        case "research": def.ResearchId = ExpressionToId(named.Expression); break;
                        case "target":   def.TargetId   = ExpressionToId(named.Expression); break;
                        case "machine":  def.MachineId  = ExpressionToId(named.Expression); break;
                    }
                } else {
                    switch (positional) {
                        case 0: def.ResearchId = ExpressionToId(arg.Expression); break;
                        case 1: def.TargetId   = ExpressionToId(arg.Expression); break;
                        case 2: def.MachineId  = ExpressionToId(arg.Expression); break;
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
            // `bind_recipe` attaches a recipe to a machine — a top-level
            // definition statement in its own right.
            if (callName.StartsWith("bind_")) return true;
            // `migrate_recipe` is a tombstone for a removed recipe — a definition
            // statement too, and one that must never be dropped on a rewrite.
            if (callName.StartsWith("migrate_")) return true;
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
