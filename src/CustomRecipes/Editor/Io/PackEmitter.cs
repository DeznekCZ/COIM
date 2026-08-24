using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomAssets.Editor.Model;
using Mafi.Collections;

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

        /// Write every recipe + raw-edited definition in the model back to
        /// its source file. Throws if any RecipeDef.SourceFile is null/empty
        /// (caller must assign new recipes to a target file first).
        ///
        /// Two kinds of splices happen per file:
        ///   1. RecipeDef → RenderRecipe(def) replaces the original line range.
        ///   2. UnknownDef → def.RawSource (the raw-edit form's text)
        ///      replaces the original line range. Both share the same
        ///      indent-preservation logic so wrapping in if/elif/else stays
        ///      intact.
        ///
        /// Both kinds are bundled per file and applied in DESCENDING start-line
        /// order so earlier splices don't invalidate later ranges.
        public static void Save(PackModel model) {
            foreach (RecipeDef def in model.Recipes) {
                if (string.IsNullOrEmpty(def.SourceFile)) {
                    throw new InvalidOperationException(
                        $"Recipe '{def.RecipeId}' has no SourceFile assigned. " +
                        "Assign one before saving (typically a Definitions/*.py path " +
                        "under the pack root).");
                }
            }

            // Group all DefBase entries (RecipeDef + UnknownDef) by file.
            var byFile = new Dictionary<string, List<DefBase>>(StringComparer.Ordinal);
            foreach (RecipeDef r in model.Recipes) {
                if (!byFile.TryGetValue(r.SourceFile, out var bucket)) {
                    bucket = new List<DefBase>();
                    byFile[r.SourceFile] = bucket;
                }
                bucket.Add(r);
            }
            foreach (DefBase other in model.OtherDefinitions) {
                if (string.IsNullOrEmpty(other.SourceFile)) continue; // appended-only path
                if (!byFile.TryGetValue(other.SourceFile, out var bucket)) {
                    bucket = new List<DefBase>();
                    byFile[other.SourceFile] = bucket;
                }
                bucket.Add(other);
            }

            // Park the model so per-kind renderers can autodetect texture
            // registrations without taking a model parameter (see
            // appendIconRef). Cleared in finally so a thrown rewrite
            // doesn't leak the reference into a later, unrelated emit.
            s_currentSaveModel = model;
            try {
                foreach (var kvp in byFile) {
                    rewriteFile(kvp.Key, kvp.Value);
                }
            } finally {
                s_currentSaveModel = null;
            }
        }

        /// Splice ONLY this one def back into its source file â€” unless the
        /// def's (scope, run) has been reordered in memory vs. on-disk, in
        /// which case the whole run gets a region rewrite so the new order
        /// lands. Single-entry edits stay surgical; reorder is committed
        /// as one unit because in-place line splicing can't shuffle ranges
        /// around each other.
        ///
        /// The def must already have a SourceFile + non-zero
        /// SourceStartLine â€” newly added defs (SourceStartLine == 0) still
        /// require the full <see cref="Save"/> path so the appended-defs
        /// flow runs.
        /// Physically remove <paramref name="def"/>'s line range from its
        /// source file. Used by the editor's per-def delete affordance so
        /// the deletion is committed to disk immediately — the previous
        /// "remove from model + wait for Save" flow had a bug where
        /// onSavePack only RE-EMITS defs still in the model and never
        /// touched the deleted def's lines, so a Save after Delete would
        /// see the def still on disk and re-load it into the model.
        ///
        /// Requires a non-zero SourceStartLine — newly-added defs that
        /// haven't been flushed to disk yet can be dropped from the
        /// in-memory model without involving this method.
        public static void DeleteDef(DefBase def) {
            if (def == null) return;
            if (string.IsNullOrEmpty(def.SourceFile)) return;
            if (def.SourceStartLine <= 0) return;
            if (!File.Exists(def.SourceFile)) return;

            string[] lines = File.ReadAllLines(def.SourceFile);
            int startIdx = def.SourceStartLine - 1;
            int endIdx   = def.SourceEndLine   - 1;
            if (startIdx < 0 || endIdx >= lines.Length || startIdx > endIdx) return;

            List<string> output = new List<string>(lines);
            output.RemoveRange(startIdx, endIdx - startIdx + 1);
            // Conservative blank-line cleanup: if the deletion left two
            // adjacent blank lines (one ABOVE the removed block and the
            // first one BELOW), collapse to one so successive deletes
            // don't accrete blank-line clusters. Skipped when only one
            // side has a blank — those came from the surrounding file
            // and we don't own them.
            if (startIdx > 0 && startIdx < output.Count
                    && string.IsNullOrWhiteSpace(output[startIdx - 1])
                    && string.IsNullOrWhiteSpace(output[startIdx])) {
                output.RemoveAt(startIdx);
            }
            File.WriteAllLines(def.SourceFile, output);
        }

        /// Render <paramref name="def"/> and append it to its source file
        /// with a blank-line separator. Used by the editor's "+ new"
        /// affordance so a freshly-added def lands on disk immediately
        /// instead of waiting for a later Save call. After invocation the
        /// caller should RescanPack + reload the PackModel so the
        /// returned def gets proper SourceStartLine/EndLine values from
        /// the new on-disk layout.
        public static void AppendDef(DefBase def, PackModel model) {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(def.SourceFile)) {
                throw new InvalidOperationException(
                    "AppendDef requires a SourceFile on the def.");
            }
            // Defensive: AppendDef is invoked only from the editor's
            // "+ add" affordance these days, which already filters on
            // MissingMandatoryFields. Belt-and-braces check here so any
            // future call site can't accidentally write a draft to disk.
            var miss = def.MissingMandatoryFields();
            if (miss != null && miss.Count > 0) {
                throw new InvalidOperationException(
                    "AppendDef refused: '" + (def.DisplayId ?? def.Kind)
                    + "' is missing required field(s): "
                    + string.Join(", ", miss));
            }
            // SourceStartLine == 0 routes the def through rewriteFile's
            // "appended" branch (line 417), which renders + tacks onto
            // the end of the file with a blank-line separator.
            def.SourceStartLine = 0;
            def.SourceEndLine = 0;

            // Make sure the target file exists; rewriteFile reads from
            // it before writing back. Creating an empty file is the
            // minimum safe state for a brand-new Definitions/foo.py.
            if (!File.Exists(def.SourceFile)) {
                File.WriteAllText(def.SourceFile, "");
            }

            s_currentSaveModel = model;
            try {
                rewriteFile(def.SourceFile, new List<DefBase> { def });
            } finally {
                s_currentSaveModel = null;
            }
        }

        /// Append a fresh top-level `if &lt;condition&gt;:\n    pass\n` block
        /// to the bottom of <paramref name="filePath"/>. Creates the file
        /// when it doesn't exist (matches AppendDef's behaviour for a
        /// brand-new Definitions/foo.py). The body is just `pass` so the
        /// modder can immediately add a definition inside the clause via
        /// the per-clause "+ add definition" button.
        ///
        /// Caller is responsible for RescanPack + reload — same contract
        /// as AppendDef so the editor refreshes against the new on-disk
        /// state.
        public static void AppendIfBlockToFile(string filePath, string condition) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath));
            }
            string cond = string.IsNullOrWhiteSpace(condition) ? "True" : condition.Trim();
            if (!File.Exists(filePath)) {
                File.WriteAllText(filePath, "");
            }
            // Match the trailing-newline / blank-separator convention
            // that rewriteFile's "appended" branch uses for typed defs.
            string existing = File.ReadAllText(filePath);
            StringBuilder sb = new StringBuilder(existing.Length + 64);
            sb.Append(existing);
            if (existing.Length > 0 && !existing.EndsWith("\n", StringComparison.Ordinal)) {
                sb.Append('\n');
            }
            // Single blank-line separator before the new block so the
            // file reads cleanly when stacked under another statement.
            if (existing.Length > 0) sb.Append('\n');
            sb.Append("if ").Append(cond).Append(":\n");
            sb.Append("    pass\n");
            File.WriteAllText(filePath, sb.ToString());
        }

        /// Append a fresh `with edit_recipe(recipe):\n    pass` block to a file.
        /// Mirrors AppendIfBlockToFile — the block is written with a `pass` body so
        /// it is valid Python immediately; the editor then swaps the recipe id via
        /// the header editor and adds sub-actions with AppendDefIntoClause (which
        /// replaces the `pass`). <paramref name="recipeId"/> is a placeholder the
        /// modder replaces; it is emitted as a quoted string, matching how an
        /// unresolved recipe id renders elsewhere.
        public static void AppendEditBlockToFile(string filePath, string recipeId) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath));
            }
            string id = string.IsNullOrWhiteSpace(recipeId) ? "RecipeToEdit" : recipeId.Trim();
            if (!File.Exists(filePath)) {
                File.WriteAllText(filePath, "");
            }
            string existing = File.ReadAllText(filePath);
            StringBuilder sb = new StringBuilder(existing.Length + 64);
            sb.Append(existing);
            if (existing.Length > 0 && !existing.EndsWith("\n", StringComparison.Ordinal)) {
                sb.Append('\n');
            }
            if (existing.Length > 0) sb.Append('\n');
            EditRecipeDef seed = new EditRecipeDef { RecipeId = id, EmitAsWithBlock = true };
            sb.Append(RenderEditRecipeBlockHeader(seed)).Append('\n');
            File.WriteAllText(filePath, sb.ToString());
        }

        /// Append `else:\n    pass\n` to an if/elif-chain at
        /// <paramref name="afterLine"/> (the chain's last clause's
        /// last line). <paramref name="leadingIndent"/> is the
        /// whitespace prefix the chain's `if` header started with —
        /// the new `else:` header gets the same indent so a nested
        /// if-chain doesn't lose its column position.
        public static void AppendElseClauseToFile(string filePath,
                int afterLine, string leadingIndent) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (afterLine < 1) {
                throw new InvalidOperationException(
                    "AppendElseClauseToFile requires afterLine > 0.");
            }
            string indent = leadingIndent ?? "";
            string bodyIndent = indent + "    ";
            string[] lines = File.ReadAllLines(filePath);
            if (afterLine > lines.Length) afterLine = lines.Length;

            // Detect the body indent that the if-chain already uses by
            // looking at the line AFTER the header within the existing
            // chain — if it's indented more than the header, that's the
            // chain's body indent. Use that for `pass` so the new else's
            // body lines up with sibling clauses' body lines.
            int peek = Math.Max(0, afterLine - 1);
            for (int i = peek; i >= 0; i--) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string trimmed = line.TrimStart();
                if (trimmed.Length == line.Length) break; // unindented sibling
                string thisIndent = line.Substring(0, line.Length - trimmed.Length);
                if (thisIndent.Length > indent.Length) {
                    bodyIndent = thisIndent;
                    break;
                }
            }

            List<string> result = new List<string>(lines.Length + 2);
            for (int i = 0; i < afterLine; i++) result.Add(lines[i]);
            result.Add(indent + "else:");
            result.Add(bodyIndent + "pass");
            for (int i = afterLine; i < lines.Length; i++) result.Add(lines[i]);
            File.WriteAllLines(filePath, result,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        /// Splice <paramref name="def"/> into the body of an if-chain
        /// clause that starts at <paramref name="clauseHeaderLine"/>
        /// (1-based). The def is rendered via the same per-kind renderer
        /// the regular AppendDef path uses, then every emitted line is
        /// re-indented to match the clause's body indent so the inserted
        /// statement nests under the clause header.
        ///
        /// The new def's SourceFile + SourceStartLine/EndLine are NOT
        /// updated here — the caller is expected to RescanPack + reload
        /// so the freshly-parsed AST drives subsequent edits, mirroring
        /// AppendDef's contract.
        public static void AppendDefIntoClause(string filePath,
                int clauseHeaderLine, DefBase def, PackModel model) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (clauseHeaderLine < 1) {
                throw new InvalidOperationException(
                    "AppendDefIntoClause requires clauseHeaderLine > 0.");
            }
            var miss = def.MissingMandatoryFields();
            if (miss != null && miss.Count > 0) {
                throw new InvalidOperationException(
                    "AppendDefIntoClause refused: '" + (def.DisplayId ?? def.Kind)
                    + "' is missing required field(s): " + string.Join(", ", miss));
            }

            string[] lines = File.ReadAllLines(filePath);
            if (clauseHeaderLine > lines.Length) {
                throw new InvalidOperationException(
                    "AppendDefIntoClause: clauseHeaderLine " + clauseHeaderLine
                    + " is past end of " + filePath);
            }

            findClauseBody(lines, clauseHeaderLine, out string bodyIndent,
                out int insertAfterIdx);

            // Render the def with the existing per-kind dispatcher. Park
            // the model so renderers can resolve cross-def references via
            // appendIconRef etc. Same convention as AppendDef.
            s_currentSaveModel = model;
            string rendered;
            try {
                rendered = renderDefinition(def);
            } finally {
                s_currentSaveModel = null;
            }
            if (string.IsNullOrEmpty(rendered)) {
                throw new InvalidOperationException(
                    "AppendDefIntoClause: no renderer for '"
                    + (def?.Kind ?? "(null)") + "'");
            }

            spliceRenderedIntoClauseBody(filePath, lines, clauseHeaderLine - 1,
                insertAfterIdx, bodyIndent, rendered);
        }

        /// Splice a fresh `if &lt;condition&gt;:\n    pass` block into an
        /// existing if-chain clause's body. Same indentation contract as
        /// <see cref="AppendDefIntoClause"/> — bodyIndent inherits from the
        /// host clause, the inner `pass` uses bodyIndent+4 so the new
        /// nested if reads as proper Python.
        public static void AppendIfBlockIntoClause(string filePath,
                int clauseHeaderLine, string condition) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (clauseHeaderLine < 1) {
                throw new InvalidOperationException(
                    "AppendIfBlockIntoClause requires clauseHeaderLine > 0.");
            }
            string cond = string.IsNullOrWhiteSpace(condition) ? "True" : condition.Trim();

            string[] lines = File.ReadAllLines(filePath);
            if (clauseHeaderLine > lines.Length) {
                throw new InvalidOperationException(
                    "AppendIfBlockIntoClause: clauseHeaderLine " + clauseHeaderLine
                    + " is past end of " + filePath);
            }
            findClauseBody(lines, clauseHeaderLine, out string bodyIndent,
                out int insertAfterIdx);

            // Render `if <cond>:\n    pass` — body indent (4 spaces) is
            // applied on top of the clause-body indent via the splice
            // helper, giving the nested `pass` an indent of bodyIndent + 4.
            string rendered = "if " + cond + ":\n    pass";
            spliceRenderedIntoClauseBody(filePath, lines, clauseHeaderLine - 1,
                insertAfterIdx, bodyIndent, rendered);
        }

        /// Splice a fresh `with edit_recipe(recipe):\n    pass` block into an
        /// existing clause body. Same contract as AppendIfBlockIntoClause — the
        /// header+pass renders at column 0 and the splice helper shifts it to the
        /// clause's body indent, so the nested `pass` sits at bodyIndent + 4.
        public static void AppendEditBlockIntoClause(string filePath,
                int clauseHeaderLine, string recipeId) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (clauseHeaderLine < 1) {
                throw new InvalidOperationException(
                    "AppendEditBlockIntoClause requires clauseHeaderLine > 0.");
            }
            string[] lines = File.ReadAllLines(filePath);
            if (clauseHeaderLine > lines.Length) {
                throw new InvalidOperationException(
                    "AppendEditBlockIntoClause: clauseHeaderLine " + clauseHeaderLine
                    + " is past end of " + filePath);
            }
            findClauseBody(lines, clauseHeaderLine, out string bodyIndent,
                out int insertAfterIdx);

            EditRecipeDef seed = new EditRecipeDef {
                RecipeId = string.IsNullOrWhiteSpace(recipeId) ? "RecipeToEdit" : recipeId.Trim(),
                EmitAsWithBlock = true,
            };
            spliceRenderedIntoClauseBody(filePath, lines, clauseHeaderLine - 1,
                insertAfterIdx, bodyIndent, RenderEditRecipeBlockHeader(seed));
        }

        /// Locate where a new statement goes inside a block clause's body:
        /// the body's actual indent, and the 0-based index AFTER which to
        /// insert (i.e. the end of the existing body).
        ///
        /// Three walks. First over the HEADER itself, which is one statement but
        /// may occupy many physical lines when its call arguments wrap (see
        /// <see cref="blockHeaderEndIdx"/>) — the body starts after its last
        /// line, never after its first. Then forward from there for the first
        /// non-blank line deeper than the header — that line's indent IS the
        /// body indent, however the file happens to be indented. Then on past
        /// the body for its last line: anything at or deeper than the body
        /// indent still belongs to it (blank lines included, so a trailing
        /// blank stays inside rather than pushing the insert above it); the
        /// first shallower line belongs to a sibling clause or outer scope and
        /// ends the walk.
        ///
        /// With no body at all (a bare `if X:` whose `pass` we didn't see) the
        /// insert point is the header's last line and the indent falls back to
        /// header + 4.
        private static void findClauseBody(string[] lines, int clauseHeaderLine,
                out string bodyIndent, out int insertAfterIdx) {
            string headerIndent = leadingWhitespace(lines[clauseHeaderLine - 1]);
            bodyIndent = headerIndent + "    ";
            int headerEndIdx = blockHeaderEndIdx(lines, clauseHeaderLine - 1);

            int firstBodyLine = -1;
            for (int i = headerEndIdx + 1; i < lines.Length; i++) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string indent = leadingWhitespace(line);
                if (indent.Length <= headerIndent.Length) break;
                firstBodyLine = i; // 0-based
                bodyIndent = indent;
                break;
            }

            if (firstBodyLine < 0) {
                insertAfterIdx = headerEndIdx;
                return;
            }

            insertAfterIdx = firstBodyLine;
            for (int i = firstBodyLine + 1; i < lines.Length; i++) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) {
                    insertAfterIdx = i;
                    continue;
                }
                if (leadingWhitespace(line).Length >= bodyIndent.Length) {
                    insertAfterIdx = i;
                } else {
                    break;
                }
            }
        }

        /// 0-based index of the LAST line of the block header that STARTS at
        /// <paramref name="headerIdx"/>. A header is one statement, but it can
        /// occupy many physical lines whenever its call arguments wrap:
        ///
        /// <code>
        /// with build_recipe(
        ///     "Recipe_X",
        ///     …
        /// ) as recipe_x:
        /// </code>
        ///
        /// Every one of those continuation lines is indented deeper than the
        /// header, so a scan that assumed a one-line header claimed them as the
        /// block's body and spliced new statements INTO the argument list,
        /// producing `bind_recipe(…)` calls between `products = […]` and the
        /// closing `)`. Since the resulting file no longer parses as the block
        /// it was, the next reload didn't recognise the binding as written and
        /// appended it again — one more copy per save.
        ///
        /// The header ends at the first line where every bracket it opened has
        /// closed AND the code ends with the block-opening `:`. Comments and
        /// string literals are skipped so a `#` or a bracket inside a quoted id
        /// can't move the boundary. A header we can't terminate (unbalanced
        /// brackets, an unclosed triple-quote) falls back to its first line,
        /// which is the pre-existing behaviour.
        private static int blockHeaderEndIdx(string[] lines, int headerIdx) {
            if (headerIdx < 0 || headerIdx >= lines.Length) return headerIdx;
            int depth = 0;
            for (int i = headerIdx; i < lines.Length; i++) {
                string code = stripCommentsAndCountBrackets(lines[i], ref depth);
                if (depth > 0) continue;
                if (code.TrimEnd().EndsWith(":", StringComparison.Ordinal)) {
                    return i;
                }
            }
            return headerIdx;
        }

        // One line of Python with its trailing `#` comment removed, updating
        // <paramref name="depth"/> by the brackets the line opens and closes.
        // Quoted spans are copied through verbatim and contribute neither
        // brackets nor a comment start, which is what lets an id like
        // "Recipe_(X)" or a comment marker inside a string sit in a header
        // without confusing the span scan.
        private static string stripCommentsAndCountBrackets(string line, ref int depth) {
            if (string.IsNullOrEmpty(line)) return "";
            StringBuilder code = new StringBuilder(line.Length);
            char quote = '\0';
            for (int i = 0; i < line.Length; i++) {
                char c = line[i];
                if (quote != '\0') {
                    code.Append(c);
                    if (c == '\\' && i + 1 < line.Length) {
                        code.Append(line[++i]);
                        continue;
                    }
                    if (c == quote) quote = '\0';
                    continue;
                }
                if (c == '"' || c == '\'') {
                    quote = c;
                    code.Append(c);
                    continue;
                }
                if (c == '#') break;
                if (c == '(' || c == '[' || c == '{') {
                    depth++;
                } else if (c == ')' || c == ']' || c == '}') {
                    if (depth > 0) depth--;
                }
                code.Append(c);
            }
            return code.ToString();
        }

        // Shared insertion logic — re-indent the rendered text to bodyIndent,
        // splice into the file replacing a trailing `<bodyIndent>pass` body
        // if present. Used by both typed-def and fresh-if-block clause
        // splices. lines = current file contents (pre-read so callers can
        // do their own pre-validation against them). clauseHeaderIdx +
        // insertAfterIdx are 0-based.
        private static void spliceRenderedIntoClauseBody(string filePath,
                string[] lines, int clauseHeaderIdx, int insertAfterIdx,
                string bodyIndent, string rendered) {
            int passReplaceIdx = -1;
            for (int i = insertAfterIdx; i > clauseHeaderIdx; i--) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimEnd() == bodyIndent + "pass") {
                    passReplaceIdx = i;
                }
                break;
            }

            List<string> indented = indentBlock(rendered, bodyIndent);

            List<string> output = new List<string>(lines.Length + indented.Count + 2);
            for (int i = 0; i < lines.Length; i++) {
                if (i == passReplaceIdx) continue;
                output.Add(lines[i]);
                if (i == insertAfterIdx) {
                    if (!string.IsNullOrWhiteSpace(lines[i])) {
                        output.Add("");
                    }
                    output.AddRange(indented);
                }
            }
            File.WriteAllLines(filePath, output,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        /// The spaces/tabs a line starts with — the unit every splice works in,
        /// since a statement is rendered at column 0 and then shifted to match
        /// its surroundings. Null/empty → "".
        private static string leadingWhitespace(string line) {
            if (string.IsNullOrEmpty(line)) return "";
            int n = 0;
            while (n < line.Length && (line[n] == ' ' || line[n] == '\t')) n++;
            return n == 0 ? "" : line.Substring(0, n);
        }

        /// Shift a rendered statement (always produced at column 0) to sit at
        /// <paramref name="indent"/>. The single place that knows the two rules
        /// every splice path shares:
        ///
        ///   - blank lines stay blank — prefixing them would write trailing
        ///     whitespace on the separators between defs;
        ///   - a stray '\r' is dropped, so a CRLF-captured RawSource can't leave
        ///     a bare CR in the middle of a rewritten line.
        ///
        /// An empty indent still round-trips through here, so callers no longer
        /// need the `if (indent.Length > 0)` branch they each used to carry.
        private static List<string> indentBlock(string rendered, string indent) {
            string[] renderedLines = rendered.Split('\n');
            List<string> result = new List<string>(renderedLines.Length);
            foreach (string line in renderedLines) {
                string r = line;
                if (r.EndsWith("\r", StringComparison.Ordinal)) {
                    r = r.Substring(0, r.Length - 1);
                }
                result.Add(r.Length == 0 ? "" : indent + r);
            }
            return result;
        }

        public static void SaveDef(DefBase def, PackModel model) {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(def.SourceFile)) {
                throw new InvalidOperationException(
                    "SaveDef requires a SourceFile â€” newly added defs without one " +
                    "must go through Save() so the appended-defs path runs.");
            }
            if (def.SourceStartLine <= 0) {
                throw new InvalidOperationException(
                    "SaveDef requires SourceStartLine > 0 â€” a freshly added def " +
                    "with no source range yet must go through Save() first.");
            }

            // Collect every other def sharing this def's (file, scope, run).
            // Order matters: the list reflects current MODEL order, while
            // sorting by SourceStartLine recovers the ON-DISK order. If they
            // disagree the user dragged something and we need a region
            // rewrite, not an in-place splice.
            string file  = def.SourceFile;
            string scope = def.ScopeKey ?? "top";
            int    run   = def.RunIndex;
            var runDefsByModelOrder = new System.Collections.Generic.List<DefBase>();
            foreach (DefBase d in model.Definitions) {
                if (d.SourceFile == file
                        && (d.ScopeKey ?? "top") == scope
                        && d.RunIndex == run
                        && d.SourceStartLine > 0) {
                    runDefsByModelOrder.Add(d);
                }
            }
            var runDefsByDiskOrder = new System.Collections.Generic.List<DefBase>(runDefsByModelOrder);
            runDefsByDiskOrder.Sort((a, b) => a.SourceStartLine.CompareTo(b.SourceStartLine));

            bool reordered = false;
            for (int i = 0; i < runDefsByModelOrder.Count; i++) {
                if (!ReferenceEquals(runDefsByModelOrder[i], runDefsByDiskOrder[i])) {
                    reordered = true;
                    break;
                }
            }

            // Park the model so per-kind renderers (which don't take a
            // model parameter) can autodetect texture registrations via
            // appendIconRef. Cleared in finally so a thrown rewrite doesn't
            // leak the reference into a later, unrelated emit.
            s_currentSaveModel = model;
            try {
                if (!reordered) {
                    rewriteFile(file,
                        new System.Collections.Generic.List<DefBase> { def });
                    def.Dirty = false;
                    return;
                }

                // Region rewrite: replace the lines [min..max] covering every
                // def in the run with each def re-rendered in MODEL ORDER,
                // separated by a single blank line. Free helper code or
                // floating comments that happened to live between defs IS
                // dropped â€” per the design discussion, the model is the source
                // of truth and intermediate text isn't preserved across a
                // reorder.
                rewriteRegion(file, runDefsByModelOrder, runDefsByDiskOrder);
                foreach (DefBase d in runDefsByModelOrder) d.Dirty = false;
            } finally {
                s_currentSaveModel = null;
            }
        }

        // Splice a (file, scope, run) region with its defs emitted in
        // model order. Reads the file, finds the bounding line range
        // from the run's disk-order members, replaces that range with
        // newly rendered content, and writes the file back. Indent is
        // taken from the FIRST def's original first line so a nested
        // run inside an if/elif/else body re-emits with matching
        // indentation.
        private static void rewriteRegion(string file,
                System.Collections.Generic.List<DefBase> modelOrder,
                System.Collections.Generic.List<DefBase> diskOrder) {
            if (modelOrder.Count == 0) return;
            int startLine = diskOrder[0].SourceStartLine;
            int endLine   = diskOrder[diskOrder.Count - 1].SourceEndLine;
            string[] lines = File.ReadAllLines(file);
            if (startLine < 1 || endLine > lines.Length || startLine > endLine) return;

            string indent = leadingWhitespace(lines[startLine - 1]);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < modelOrder.Count; i++) {
                if (i > 0) sb.Append('\n'); // blank separator between defs
                string rendered = renderDefinition(modelOrder[i]);
                if (rendered == null) continue;
                foreach (string ln in indentBlock(rendered, indent)) {
                    sb.Append(ln).Append('\n');
                }
            }

            var output = new System.Collections.Generic.List<string>(lines);
            output.RemoveRange(startLine - 1, endLine - startLine + 1);
            string newBlock = sb.ToString();
            // Trim the trailing newline we added in the loop above so the
            // splice doesn't insert an extra blank at the bottom of the
            // region. Split on '\n' includes a final empty string when the
            // text ends with '\n'; .TrimEnd avoids that.
            string[] newLines = newBlock.TrimEnd('\n').Split('\n');
            output.InsertRange(startLine - 1, newLines);
            File.WriteAllLines(file, output);
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
        /// Renders a recipe. A recipe WITH machine bindings emits as one
        /// `with build_recipe(...) [as var]:` block whose body is the
        /// `bind_recipe(...)` calls — a single source range, which is what the
        /// line-range splice pipeline expects. A recipe with no bindings emits
        /// as a plain `build_recipe(...)` statement.
        ///
        /// A LEGACY recipe (inline `machine=`/`duration=`) is migrated here: its
        /// machine becomes a binding and the call loses the inline machine, so
        /// opening an old pack and saving upgrades it to the split format.
        public static string RenderRecipe(RecipeDef def) {
            // Bindings this recipe owns. Split into those already written to the
            // file (they own their own line ranges and splice themselves — that is
            // what makes them independently saveable, deletable and reorderable)
            // and those still PENDING, which have no range yet and are written
            // here, as the body of the block being opened.
            //
            // Writing the pending ones is not optional: a legacy recipe's machine
            // and duration live on exactly such a binding, so a header-only render
            // would drop them on the very save that migrates the recipe.
            List<BindRecipeDef> owned = s_currentSaveModel != null
                ? s_currentSaveModel.BindingsOf(def).ToList()
                : new List<BindRecipeDef>();
            List<BindRecipeDef> pending = owned.Where(b => b.IsPendingInOwner).ToList();

            // A `with` block must have a body. If the last binding was removed,
            // fall back to the plain statement instead of writing a header with
            // nothing under it. Outside a save pass there is no model to consult,
            // so trust EmitAsWithBlock and keep the header-only preview.
            bool hasBody = owned.Count > 0 || s_currentSaveModel == null;
            if (!def.EmitAsWithBlock || !hasBody) {
                // No machines attached — plain statement (keeps the `<var> = `
                // assignment prefix so downstream references still resolve).
                StringBuilder plain = new StringBuilder();
                appendStmtPrefix(plain, def);
                appendBuildRecipeCall(plain, def, suppressLegacyMachine: false);
                return plain.ToString();
            }

            StringBuilder sb = new StringBuilder();
            PyWriter w = new PyWriter(sb);
            // In the `with` form the variable moves from a `<var> = ` prefix to
            // the `as <var>` clause, so only the comment block is prefixed here.
            appendCommentLines(sb, def.Comment);
            sb.Append("with ");
            appendBuildRecipeCall(sb, def, suppressLegacyMachine: true);
            if (!string.IsNullOrEmpty(def.VariableName)) {
                sb.Append(" as ").Append(def.VariableName);
            }
            sb.Append(":");

            // Body: the pending bindings, in context form (the recipe is implicit
            // inside the block, so only the machine is emitted). The writer supplies
            // the one level of indentation — each binding is rendered at column 0
            // and every newline inside it picks up the body indent.
            PyWriter body = w.Indent();
            foreach (BindRecipeDef bind in pending) {
                body.Append("\n").Append(renderBindRecipe(bind, contextForm: true));
            }
            return w.ToString();
        }

        /// The `build_recipe(...)` call itself (no statement prefix, no `with`).
        /// `suppressLegacyMachine` drops the inline machine/duration/research —
        /// used by the `with` form where those live on the bindings instead.
        private static void appendBuildRecipeCall(StringBuilder sb, RecipeDef def,
                bool suppressLegacyMachine) {
            // `w` sits at the statement's own column; `arg` one level in, so every
            // "\n" it emits is followed by the argument indent. The four spaces
            // that used to be typed after each newline come from `arg` now.
            PyWriter w = new PyWriter(sb);
            PyWriter arg = w.Indent();

            // Capture once so every appendIdRef in this render reads the
            // same per-file variable map. Null when the def has no source
            // file context (newly-added recipes) — appendIdRef handles that.
            var vars = def.SourceFileVariables;

            arg.Append("build_recipe(\n"); appendString(sb, def.RecipeId);
            arg.Append(",\n");             appendString(sb, def.Name);
            arg.Append(",\n");             appendString(sb, def.Description);

            // LEGACY one-shot form: machine / research / duration are emitted
            // only for a machine-bearing recipe that is NOT being wrapped in a
            // `with` block (where they migrate onto the bindings instead).
            if (!suppressLegacyMachine && !string.IsNullOrEmpty(def.MachineId)) {
                arg.Append(",\nmachine = "); appendIdRef(sb, def.MachineId, vars);
                if (def.ResearchId != null) {
                    arg.Append(",\nresearch = "); appendIdRef(sb, def.ResearchId, vars);
                    if (!def.UnlockMachine) {
                        arg.Append(",\nunlock_machine = False");
                    }
                }
                if (def.DurationSeconds.HasValue || !string.IsNullOrWhiteSpace(def.DurationExpression)) {
                    arg.Append(",\nduration = Duration.FromSec(");
                    sb.Append(durationArg(def.DurationSeconds, def.DurationExpression)).Append(")");
                }
            }
            if (def.Ingredients != null && def.Ingredients.Count > 0) {
                arg.Append(",\ningredients = ");
                appendProductList(arg, def.Ingredients, vars);
            }
            if (def.Products != null && def.Products.Count > 0) {
                arg.Append(",\nproducts = ");
                appendProductList(arg, def.Products, vars);
            }
            if (def.PowerPercent.HasValue) {
                arg.Append(",\npower = ");
                sb.Append(def.PowerPercent.Value);
            }
            // Always a list, even for a single entry: the emitted form should not
            // change shape when a second superseded id is added later.
            if (def.Replaces != null && def.Replaces.Count > 0) {
                StringBuilder replaced = new StringBuilder("[");
                for (int i = 0; i < def.Replaces.Count; i++) {
                    if (i > 0) replaced.Append(", ");
                    appendString(replaced, def.Replaces[i]);
                }
                replaced.Append("]");
                arg.Append(",\nreplaces = ").Append(replaced.ToString());
            }

            w.Append("\n)");
        }

        // IDs with a dot in them are emitted verbatim (typed references like
        // `Ids.Machines.X`). IDs without a dot but matching a known file-
        // scoped variable name are also emitted bare (so a modder's
        // `researchWoodgass = build_research(...)` followed by
        // `add_unlock_product(researchWoodgass, "X")` round-trips exactly).
        // Anything else is a plain string literal. Null → Python None.
        //
        // The `variables` parameter is the per-file map of variable name →
        // resolved id that PackLoader builds. Pass `def.SourceFileVariables`
        // from each render call. A null map is treated as "no known
        // variables" — the heuristic falls through to dot-vs-string.
        private static void appendIdRef(StringBuilder sb, string id,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            if (id == null || id == UnparseableSentinel) { sb.Append("None"); return; }
            if (variables != null && variables.ContainsKey(id)) { sb.Append(id); return; }
            if (id.IndexOf('.') >= 0) { sb.Append(id); return; }
            appendString(sb, id);
        }

        // Primary-id emit for calls that CREATE a new proto (build_housing,
        // build_machine, build_product_*, add_crop, …). The call's own id
        // must never resolve through the per-file variable map: when a
        // variable shares the id's spelling — most commonly the def's own
        // `<var> = build_*(...)` binding — appendIdRef would emit the bare
        // identifier and the call would read its own not-yet-assigned
        // variable, a NameError at pack load. Dotted typed-refs still pass
        // through verbatim.
        private static void appendNewId(StringBuilder sb, string id) {
            appendIdRef(sb, id, null);
        }

        // Asset-path emit. Two value shapes round-trip through the editor's
        // AssetPathPicker: filesystem paths inside the pack (e.g.
        // "Assets/MyPack/foo.png") and typed-ref constants on Mafi.Base.Assets
        // (e.g. Assets.Base.Bridges.Icons.CableStayed4_svg). The latter is a
        // dotted Python expression that resolves at pack-load time to the
        // underlying string, so it must NOT be quoted; the former is a literal
        // string containing `/` which is not a valid Python identifier and so
        // must be quoted. Discriminator: presence of any character outside
        // [a-zA-Z0-9._] means "path string" → quote. Pure identifier-shaped
        // input with at least one dot is treated as a dotted typed-ref and
        // written verbatim. Null → emits None (the API default for optional
        // icon args).
        // Autodetect-aware icon emit. Decides between three forms:
        //   â€¢ <c>None</c>                       when value is null
        //   â€¢ bare identifier / typed-ref       when value is a dotted ref
        //                                       (Ids.X / Assets.Foo) OR a
        //                                       known variable name in the
        //                                       per-file <paramref name="variables"/>
        //                                       map (e.g. <c>texture_X = add_texture(â€¦)</c>
        //                                       earlier in the file)
        //   â€¢ bare quoted string                when the model already has
        //                                       a top-level <see cref="TextureDef"/>
        //                                       registering this path
        //   â€¢ <c>add_texture("path")</c> wrap   otherwise, so the texture is
        //                                       loaded inline as a side-effect
        //                                       of this assignment
        //
        // The model is read from <see cref="s_currentSaveModel"/>, which is
        // set by <see cref="Save"/> / <see cref="SaveDef"/> for the duration
        // of a write pass. When null (defensive â€” a renderer called outside
        // a save) we fall back to bare-string emission so we never wrap
        // something that wasn't requested.
        private static void appendIconRef(StringBuilder sb, string value,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            if (value == null || value == UnparseableSentinel) { sb.Append("None"); return; }
            if (isDottedRef(value)) { sb.Append(value); return; }
            if (variables != null && variables.ContainsKey(value)) {
                sb.Append(value); return;
            }
            if (s_currentSaveModel != null
                    && isPathRegisteredAsTexture(s_currentSaveModel, value)) {
                appendString(sb, value);
                return;
            }
            if (s_currentSaveModel == null) {
                // Outside an active save pass we can't autodetect â€” keep
                // the conservative bare-string form so a callerâ€“driven
                // preview doesn't accidentally inject add_texture() calls.
                appendString(sb, value);
                return;
            }
            sb.Append("add_texture(");
            appendString(sb, value);
            sb.Append(")");
        }

        // True when the pack already has a top-level <c>add_texture(path)</c>
        // call captured as a <see cref="TextureDef"/>. Used by
        // <see cref="appendIconRef"/> to skip the inline wrap when the
        // texture is already registered elsewhere in the pack.
        private static bool isPathRegisteredAsTexture(PackModel model, string path) {
            if (model?.Definitions == null || string.IsNullOrEmpty(path)) return false;
            foreach (DefBase d in model.Definitions) {
                if (d is TextureDef tx && tx.Path == path) return true;
            }
            return false;
        }

        // Per-save-pass model reference. Set by Save / SaveDef so the
        // per-kind renderers (which don't take a model parameter) can
        // autodetect texture registrations without threading the model
        // through every call site. [ThreadStatic] keeps it isolated so a
        // parallel Save on another thread doesn't see a stale value.
        [System.ThreadStatic]
        private static PackModel s_currentSaveModel;

        private static bool isDottedRef(string s) {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) {
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_') return false;
            }
            return s.IndexOf('.') >= 0;
        }

        // appendOptionalIdRef counterpart that respects the variables map.
        // Same signature shape as appendOptionalString/Bool/Int for use in
        // the product/research/generator renderers.
        private static void appendOptionalIdRef(StringBuilder sb, string name, string id,
                System.Collections.Generic.Dictionary<string, string> variables) {
            if (isMissingOrSentinel(id)) return;
            sb.Append(",\n    ").Append(name).Append(" = ");
            appendIdRef(sb, id, variables);
        }

        // Slice off the leading whitespace prefix from a source line. Returns
        // empty when the line starts at column 0 or is null. Treats both ' '
        // and '\t' as whitespace so packs authored with tabs round-trip with
        // their original indent style preserved.
        /// Delete a whole block statement — its header line(s) AND everything
        /// indented under it. Used for `if`/`else` chains and `with` blocks, which
        /// have no single def whose line range covers them.
        ///
        /// <paramref name="endLine"/> is the caller's idea of the last line (for an
        /// if-CHAIN that's the final clause's end, so deleting the `if` takes its
        /// `elif`/`else` with it — leaving them behind would be a syntax error).
        /// The span is then extended past any further deeper-indented or blank
        /// lines, so a trailing body line can't be orphaned.
        public static void DeleteBlock(string filePath, int headerLine, int endLine) {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;
            string[] lines = File.ReadAllLines(filePath);
            int startIdx = headerLine - 1;
            if (startIdx < 0 || startIdx >= lines.Length) return;
            int lastIdx = blockExtentEndIdx(lines, startIdx, endLine);

            List<string> output = new List<string>(lines);
            output.RemoveRange(startIdx, lastIdx - startIdx + 1);
            File.WriteAllLines(filePath, output);
        }

        /// Move a whole block (its header plus everything indented under it) so
        /// that it sits immediately BEFORE <paramref name="insertBeforeLine"/>.
        /// Line numbers are all 1-based and expressed in the CURRENT file, before
        /// the move.
        ///
        /// The block's lines are relocated verbatim. That is only correct when
        /// the destination is at the same nesting depth as the source — which is
        /// exactly the case the tree offers, since a drag is confined to siblings
        /// within one scope. Re-indenting is therefore deliberately not done: a
        /// block moved among its own siblings keeps its indentation by definition,
        /// and rewriting it would risk disturbing the bodies inside it.
        public static void MoveBlock(string filePath, int headerLine, int endLine,
                int insertBeforeLine) {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;
            string[] lines = File.ReadAllLines(filePath);
            int startIdx = headerLine - 1;
            if (startIdx < 0 || startIdx >= lines.Length) return;
            int lastIdx = blockExtentEndIdx(lines, startIdx, endLine);
            int count = lastIdx - startIdx + 1;

            List<string> moved = new List<string>(count);
            for (int i = startIdx; i <= lastIdx; i++) moved.Add(lines[i]);

            List<string> output = new List<string>(lines);
            output.RemoveRange(startIdx, count);

            // Translate the destination into post-removal coordinates. A target
            // below the block shifts up by everything we just cut; a target
            // inside the block is meaningless (it moved with it) so the block
            // stays where it was.
            int insertIdx = insertBeforeLine - 1;
            if (insertIdx > lastIdx) insertIdx -= count;
            else if (insertIdx > startIdx) insertIdx = startIdx;
            if (insertIdx < 0) insertIdx = 0;
            if (insertIdx > output.Count) insertIdx = output.Count;

            output.InsertRange(insertIdx, moved);
            File.WriteAllLines(filePath, output);
        }

        /// Last line INDEX (0-based) belonging to the block whose header is at
        /// <paramref name="startIdx"/>. The parser's own end line is the floor;
        /// past it we keep absorbing lines that are blank or indented deeper than
        /// the header, because those are still the block's body (trailing blank
        /// lines and comment tails included).
        ///
        /// Shared by delete and move so the two can never disagree about where a
        /// block ends — a mismatch there would either strand body lines behind or
        /// swallow the following statement.
        private static int blockExtentEndIdx(string[] lines, int startIdx, int endLine) {
            string headerIndent = leadingWhitespace(lines[startIdx]);
            int lastIdx = Math.Max(startIdx, Math.Min(endLine, lines.Length) - 1);
            for (int i = lastIdx + 1; i < lines.Length; i++) {
                if (string.IsNullOrWhiteSpace(lines[i])) { lastIdx = i; continue; }
                if (leadingWhitespace(lines[i]).Length > headerIndent.Length) { lastIdx = i; continue; }
                break;
            }
            return lastIdx;
        }

        /// True when a def lives inside a block body (if-clause or `with`), keyed
        /// as "block:&lt;headerLine&gt;". Such a def is only ever
        /// written by splicing into that block, never by an end-of-file append.
        private static bool isBlockScoped(DefBase def) {
            string key = def?.ScopeKey;
            return !string.IsNullOrEmpty(key) && key.StartsWith("block:");
        }

        // ---- File rewrite ------------------------------------------------------

        private static void rewriteFile(string path, List<DefBase> defs) {
            string[] lines = File.ReadAllLines(path);
            List<string> output = new List<string>(lines);

            // Separate definitions that were loaded from this file (have a
            // line range) from brand-new ones that need appending. Sort
            // existing by start line DESCENDING so splices from bottom to top
            // don't invalidate earlier ranges.
            List<DefBase> existing = defs.Where(d => d.SourceStartLine > 0).ToList();
            // A pending def that belongs INSIDE a block (keyed "block:<headerLine>")
            // must never be appended at end-of-file: that would silently move it out
            // of its block. Those go through AppendDefIntoClause instead, driven by
            // the save flow, so they are excluded here and stay in memory until then.
            //
            // A pending BINDING is excluded for the same reason by a different
            // route: it has no scope key at all, because the block it belongs to is
            // its recipe's `with` body — which RenderRecipe writes for it.
            List<DefBase> appended = defs
                .Where(d => d.SourceStartLine <= 0
                            && !isBlockScoped(d)
                            && !(d is BindRecipeDef b && b.IsPendingInOwner))
                .ToList();
            existing.Sort((a, b) => b.SourceStartLine.CompareTo(a.SourceStartLine));

            foreach (DefBase def in existing) {
                int startIdx = def.SourceStartLine - 1;
                int endIdx   = def.SourceEndLine   - 1;
                if (startIdx < 0 || endIdx >= output.Count || startIdx > endIdx) {
                    // Defensive: line range is corrupt (file changed under us?).
                    // Skip this definition rather than corrupt the file.
                    continue;
                }
                // Preserve the original line's leading whitespace so a
                // definition nested inside an `if/elif/else` body re-emits
                // with matching indentation.
                string originalIndent = leadingWhitespace(output[startIdx]);
                string rendered = renderDefinition(def);
                if (rendered == null) continue; // unknown def kind with no raw source
                output.RemoveRange(startIdx, endIdx - startIdx + 1);
                output.InsertRange(startIdx, indentBlock(rendered, originalIndent));
            }

            if (appended.Count > 0) {
                // Filter out incomplete drafts. Emitting them would write
                // a syntactically-valid but semantically-broken call like
                // `edit_machine_ports(machine = None)` which crashes the
                // Python runtime on next pack reload. The in-memory def
                // stays in the model so the modder can finish editing it;
                // it appends on the next save once mandatory fields are set.
                Lyst<DefBase> ready = new Lyst<DefBase>();
                foreach (DefBase def in appended) {
                    var miss = def.MissingMandatoryFields();
                    if (miss != null && miss.Count > 0) {
                        Mafi.Log.Warning("PackEmitter: skipping append of '"
                            + (def.DisplayId ?? def.Kind)
                            + "' — required fields still empty: "
                            + string.Join(", ", miss));
                        continue;
                    }
                    ready.Add(def);
                }
                if (ready.Count > 0 && output.Count > 0 && !string.IsNullOrEmpty(output[output.Count - 1])) {
                    output.Add(""); // blank separator before appended block
                }
                foreach (DefBase def in ready) {
                    string rendered = renderDefinition(def);
                    if (rendered == null) continue;
                    output.AddRange(rendered.Split('\n'));
                    output.Add("");
                }
            }

            File.WriteAllLines(path, output);
        }

        // Per-kind dispatch for splice-time rendering. Typed Defs get
        // canonical Python; UnknownDef falls back to the verbatim
        // RawSource that the raw-edit form bound to. Returns null when we
        // can't emit (unknown subclass, missing raw text) so the caller
        // knows to skip the splice.
        private static string renderDefinition(DefBase def) {
            if (def is RecipeDef r)         return RenderRecipe(r);
            // A binding that belongs to a recipe sits INSIDE that recipe's `with`
            // block, where the recipe is implicit — so it must re-emit in context
            // form (machine only). Without this an individual save would rewrite
            // `bind_recipe(machine, …)` as `bind_recipe(recipe, machine, …)`,
            // changing the file on a no-op save. A standalone binding has no
            // owner and does name its recipe explicitly.
            if (def is BindRecipeDef br)    return renderBindRecipe(br, contextForm: br.IsContextForm || br.OwnerRecipe != null);
            if (def is ResearchDef rs)      return renderResearch(rs);
            if (def is UnlockRecipeDef ur)  return renderUnlockRecipe(ur);
            if (def is MigrateRecipeDef mr) return renderMigrateRecipe(mr);
            if (def is UnlockProductDef up) return renderUnlockProduct(up);
            if (def is UnlockMachineDef um) return renderUnlockMachine(um);
            if (def is UnlockEntityDef ue)  return renderUnlockEntity(ue);
            if (def is RemoveUnlockDef rmu) return renderRemoveUnlock(rmu);
            if (def is ProductLooseDef pl)  return renderProductLoose(pl);
            if (def is ProductFluidDef pf)  return renderProductFluid(pf);
            if (def is ProductUnitDef pu)   return renderProductUnit(pu);
            if (def is TextureDef tx)       return renderTexture(tx);
            if (def is MaterialLooseDef ml) return renderMaterialLoose(ml);
            if (def is PrefabBoxDef pb)     return renderPrefabBox(pb);
            if (def is UnitPrefabDef upr)   return renderUnitPrefab(upr);
            if (def is MaterialTextureDef mt) return renderTextureMaterial(mt);
            if (def is ToolbarCategoryDef tc) return renderToolbarCategory(tc);
            if (def is GeneratorDef gd)     return renderGenerator(gd);
            if (def is EditRecipeDef er)    return renderEditRecipe(er);
            if (def is RecipeProductActionDef pa) return renderProductAction(pa);
            if (def is UnbindRecipeDef ubr) return renderUnbindRecipe(ubr);
            if (def is EditMachinePortsDef ep) return renderEditMachinePorts(ep);
            if (def is EditEntityCostsDef ec) return renderEditEntityCosts(ec);
            if (def is BuildMachineDef bm)  return renderBuildMachine(bm);
            if (def is HousingDef hd)       return renderHousing(hd);
            if (def is SettlementDecorationDef sdd) return renderSettlementDecoration(sdd);
            if (def is SettlementFoodDef sfd)       return renderSettlementFood(sfd);
            if (def is SettlementIspDef sid)        return renderSettlementIsp(sid);
            if (def is HospitalDef hpd)             return renderHospital(hpd);
            if (def is MineTowerDef mtd)            return renderMineTower(mtd);
            if (def is FarmDef fmd)                 return renderFarm(fmd);
            if (def is CropDef crd)                 return renderCrop(crd);
            if (def is EditCropDef ecd)             return renderEditCrop(ecd);
            if (def is ResearchLabDef rld)          return renderResearchLab(rld);
            if (def is NuclearReactorDef nrd)       return renderNuclearReactor(nrd);
            if (def is EditNuclearReactorFuelsDef enrf) return renderEditNuclearReactorFuels(enrf);
            if (def is EditNuclearReactorFluidsDef enrfl) return renderEditNuclearReactorFluids(enrfl);
            if (def is EditNuclearReactorEnrichmentDef enren) return renderEditNuclearReactorEnrichment(enren);
            if (def is EditNuclearReactorPortsDef enrp) return renderEditNuclearReactorPorts(enrp);
            if (def is BoxTypeDef bt)       return renderBoxType(bt);
            if (def is UnknownDef u)        return string.IsNullOrEmpty(u.RawSource) ? null : u.RawSource;
            return null;
        }

        // define_box_type(boxTypeId, token, heightFrom, heightTo, constraint,
        //     surface, terrainMaterial, isRamp) — only id + token are
        // required; every other arg emits only when set so a simple custom
        // box stays a 2-arg call.
        private static string renderBoxType(BoxTypeDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("define_box_type(\n");
            sb.Append("    boxTypeId = "); appendNewId(sb, def.BoxTypeId); sb.Append(",\n");
            sb.Append("    token     = "); appendString(sb, def.Token);
            if (def.HeightFrom != 0) sb.Append(",\n    heightFrom = ").Append(def.HeightFrom);
            appendOptionalInt   (sb, "heightTo",        def.HeightTo);
            appendOptionalString(sb, "constraint",      def.Constraint);
            appendOptionalIdRef (sb, "surface",         def.SurfaceId, vars);
            appendOptionalIdRef (sb, "terrainMaterial", def.TerrainMaterialId, vars);
            if (def.IsRamp) sb.Append(",\n    isRamp    = True");
            sb.Append("\n)");
            return sb.ToString();
        }

        // Shared layout_str emit for every ILayoutHostDef. When the modder has
        // structurally edited the layout in the visual editor the grid is
        // re-serialised through LayoutCodec; otherwise the captured raw string
        // is written back byte-identical (so untouched layouts never churn).
        private static void emitLayoutStr(StringBuilder sb, ILayoutHostDef host) {
            string value;
            if (host.Layout != null && host.Layout.IsStructured) {
                BoxTypeLibrary lib = s_currentSaveModel != null
                    ? BoxTypeLibrary.BuildFor(s_currentSaveModel)
                    : new BoxTypeLibrary();
                value = LayoutCodec.Emit(host.Layout, lib);
            } else {
                value = host.LayoutSourceStr;
            }
            // COI's EntityLayoutParser rejects grids whose rows differ in
            // length, so whatever path produced the string (structured emit,
            // raw-text editing, or a jagged SourceLayoutStr captured from a
            // cloned game proto), rows are padded to a rectangle on the way
            // to the file.
            appendOptionalString(sb, "layout_str", LayoutCodec.PadRectangular(value));
        }

        // edit_machine_ports(machine, add_ports=[Port(...), ...])
        private static string renderEditMachinePorts(EditMachinePortsDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_machine_ports(\n");
            sb.Append("    machine   = "); appendIdRef(sb, def.MachineId, vars);
            if (def.AddPorts != null && def.AddPorts.Count > 0) {
                sb.Append(",\n    add_ports = ");
                appendPortList(sb, def.AddPorts);
            }
            if (def.AutoSelectRecipes.HasValue) {
                sb.Append(",\n    auto_select_recipes = ").Append(def.AutoSelectRecipes.Value ? "True" : "False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_crop(crop, ...) — every rate emits only when the modder set it,
        // so a call that just doubles a yield stays two arguments long.
        private static string renderEditCrop(EditCropDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_crop(\n");
            sb.Append("    crop = "); appendIdRef(sb, def.CropId, vars);
            if (!string.IsNullOrEmpty(def.ProductProducedId) && def.ProductProducedQuantity.HasValue) {
                sb.Append(",\n    productProduced = Product(");
                appendIdRef(sb, def.ProductProducedId, vars);
                sb.Append(", Quantity(").Append(def.ProductProducedQuantity.Value).Append("))");
            }
            appendOptionalInt(sb, "multiplyYieldPercent",             def.MultiplyYieldPercent);
            appendOptionalInt(sb, "growthDurationDays",               def.GrowthDurationDays);
            appendOptionalInt(sb, "consumedWaterPerDay",              def.ConsumedWaterPerDay);
            appendOptionalInt(sb, "consumedFertilityPercentPerDay",   def.ConsumedFertilityPercentPerDay);
            appendOptionalInt(sb, "minFertilityToStartGrowthPercent", def.MinFertilityToStartGrowthPercent);
            appendOptionalInt(sb, "surviveWithNoWaterDays",           def.SurviveWithNoWaterDays);
            if (def.RequiresGreenhouse.HasValue) {
                sb.Append(",\n    requiresGreenhouse = ")
                  .Append(def.RequiresGreenhouse.Value ? "True" : "False");
            }
            if (def.PlantByDefault.HasValue) {
                sb.Append(",\n    plantByDefault = ")
                  .Append(def.PlantByDefault.Value ? "True" : "False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_entity_costs(entity, workers, maintenance, ..., products,
        // multiplyPercent). Only `entity` is unconditional; every other
        // argument emits solely when the modder set it, so a call that only
        // retunes the worker count stays a two-argument call.
        //
        // Emission order mirrors the editor form: staffing and upkeep first,
        // then the build-material list last (it is the tallest argument, and
        // burying the scalars under it makes the call hard to skim).
        private static string renderEditEntityCosts(EditEntityCostsDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_entity_costs(\n");
            sb.Append("    entity = "); appendIdRef(sb, def.EntityId, vars);
            appendOptionalInt   (sb, "workers",                   def.Workers);
            appendOptionalDouble(sb, "maintenance",               def.Maintenance);
            appendOptionalIdRef (sb, "maintenanceProduct",        def.MaintenanceProductId, vars);
            appendOptionalInt   (sb, "maintenanceBufferMonths",   def.MaintenanceBufferMonths);
            appendOptionalInt   (sb, "initialMaintenancePercent", def.InitialMaintenancePercent);
            appendOptionalInt   (sb, "priority",                  def.Priority);
            if (def.Products != null && def.Products.Count > 0) {
                sb.Append(",\n    products = ");
                appendProductList(sb, def.Products, vars);
            }
            appendOptionalInt   (sb, "multiplyPercent",           def.MultiplyPercent);
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_machine(...) — sole machine-construction renderer. Lives as
        // its own renderer so the call name in the emitted source matches
        // the model's BuildMachineDef capture (so a modder who wrote
        // build_machine(...) gets build_machine(...) back on save).
        private static string renderBuildMachine(BuildMachineDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_machine(\n");
            sb.Append("    machineId            = "); appendNewId(sb, def.MachineId); sb.Append(",\n");
            sb.Append("    source               = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",        def.Name);
            appendOptionalString(sb, "description", def.Description);
            if (def.AddPorts != null && def.AddPorts.Count > 0) {
                sb.Append(",\n    ports                = ");
                appendPortList(sb, def.AddPorts);
            }
            appendOptionalInt   (sb, "consumedPowerPerTick", def.ConsumedPowerPerTickKw);
            appendOptionalIdRef (sb, "research",             def.ResearchId, vars);
            if (!def.CopyRecipes) {
                sb.Append(",\n    copy_recipes         = False");
            }
            // Three copy_* flags only emit when overridden from the
            // default True — keeps the file readable for the common
            // "clone everything" case while still round-tripping the
            // modder's explicit opt-outs. A structurally-authored layout
            // forces copy_layout=False so the authored footprint wins over
            // the source clone.
            bool structuredLayout = def.Layout != null && def.Layout.IsStructured;
            if (!def.CopyLayout || structuredLayout) {
                sb.Append(",\n    copy_layout          = False");
            }
            if (!def.CopyPorts) {
                sb.Append(",\n    copy_ports           = False");
            }
            if (!def.CopyGraphics) {
                sb.Append(",\n    copy_graphics        = False");
            }
            // Only emitted when the modder overrode the inherited flag —
            // blank stays absent so the common "inherit from source" case
            // keeps the call terse.
            if (def.AutoSelectRecipes.HasValue) {
                sb.Append(",\n    auto_select_recipes  = ").Append(def.AutoSelectRecipes.Value ? "True" : "False");
            }
            emitLayoutStr(sb, def);
            if (def.LockedOnInit.HasValue) {
                sb.Append(",\n    lockedOnInit         = ").Append(def.LockedOnInit.Value ? "True" : "False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_housing(housingId, source, name, description, capacity,
        //     upointsCapacity, research, lockedOnInit) — clones an existing
        // SettlementHousingModuleProto. Only `housingId` + `source` are
        // required; every other optional arg is emitted only when set so
        // a vanilla-clone with no overrides stays a 2-arg call.
        private static string renderHousing(HousingDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_housing(\n");
            sb.Append("    housingId       = "); appendNewId(sb, def.HousingId); sb.Append(",\n");
            sb.Append("    source          = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",            def.Name);
            appendOptionalString(sb, "description",     def.Description);
            appendOptionalInt   (sb, "capacity",        def.Capacity);
            appendOptionalInt   (sb, "upointsCapacity", def.UpointsCapacity);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",        def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) {
                sb.Append(",\n    lockedOnInit    = ").Append(def.LockedOnInit.Value ? "True" : "False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // Renderers for the per-type build_* settlement / building clones.
        // Each is structurally identical: required id + source, then
        // optional name / description / typed numeric overrides, then
        // optional research + lockedOnInit. Helpers from earlier (
        // appendIdRef / appendOptionalString / appendOptionalInt /
        // appendOptionalIdRef) do the heavy lifting.

        private static string renderSettlementDecoration(SettlementDecorationDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_settlement_decoration(\n");
            sb.Append("    decorationId  = "); appendNewId(sb, def.DecorationId); sb.Append(",\n");
            sb.Append("    source        = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",         def.Name);
            appendOptionalString(sb, "description",  def.Description);
            appendOptionalInt   (sb, "upointsBonus", def.UpointsBonus);
            appendOptionalInt   (sb, "bonusRange",   def.BonusRange);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",     def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit  = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderSettlementFood(SettlementFoodDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_settlement_food(\n");
            sb.Append("    foodModuleId      = "); appendNewId(sb, def.FoodModuleId); sb.Append(",\n");
            sb.Append("    source            = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",              def.Name);
            appendOptionalString(sb, "description",       def.Description);
            appendOptionalInt   (sb, "buffersCount",      def.BuffersCount);
            appendOptionalInt   (sb, "capacityPerBuffer", def.CapacityPerBuffer);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",          def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit      = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderSettlementIsp(SettlementIspDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_settlement_isp(\n");
            sb.Append("    ispModuleId           = "); appendNewId(sb, def.IspModuleId); sb.Append(",\n");
            sb.Append("    source                = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",                  def.Name);
            appendOptionalString(sb, "description",           def.Description);
            appendOptionalInt   (sb, "computingPer100Pops",   def.ComputingPer100Pops);
            appendOptionalInt   (sb, "electricityConsumedKw", def.ElectricityConsumedKw);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",              def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit          = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderHospital(HospitalDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_hospital(\n");
            sb.Append("    hospitalId        = "); appendNewId(sb, def.HospitalId); sb.Append(",\n");
            sb.Append("    source            = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",              def.Name);
            appendOptionalString(sb, "description",       def.Description);
            appendOptionalInt   (sb, "powerRequiredKw",   def.PowerRequiredKw);
            appendOptionalInt   (sb, "buffersCount",      def.BuffersCount);
            appendOptionalInt   (sb, "capacityPerBuffer", def.CapacityPerBuffer);
            appendOptionalInt   (sb, "suppliesPerHundredPopsPerMonth", def.SuppliesPerHundredPopsPerMonth);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",          def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit      = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderMineTower(MineTowerDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_mine_tower(\n");
            sb.Append("    mineTowerId  = "); appendNewId(sb, def.MineTowerId); sb.Append(",\n");
            sb.Append("    source       = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",        def.Name);
            appendOptionalString(sb, "description", def.Description);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",    def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_farm(farmId, source, name, description,
        //     yieldMultiplierPercent, demandsMultiplierPercent,
        //     fertilityReplenishPercent, waterCollected,
        //     waterEvaporationPerDay, hasIrrigationAndFertilizerSupport,
        //     isGreenhouse, research, lockedOnInit)
        private static string renderFarm(FarmDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_farm(\n");
            sb.Append("    farmId                        = "); appendNewId(sb, def.FarmId); sb.Append(",\n");
            sb.Append("    source                        = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",                       def.Name);
            appendOptionalString(sb, "description",                def.Description);
            appendOptionalInt   (sb, "yieldMultiplierPercent",     def.YieldMultiplierPercent);
            appendOptionalInt   (sb, "demandsMultiplierPercent",   def.DemandsMultiplierPercent);
            appendOptionalInt   (sb, "fertilityReplenishPercent",  def.FertilityReplenishPercent);
            if (!string.IsNullOrEmpty(def.WaterCollectedProductId) && def.WaterCollectedQuantity.HasValue) {
                sb.Append(",\n    waterCollected                = Product(");
                appendIdRef(sb, def.WaterCollectedProductId, vars);
                sb.Append(", Quantity(").Append(def.WaterCollectedQuantity.Value).Append("))");
            }
            appendOptionalInt   (sb, "waterEvaporationPerDay",     def.WaterEvaporationPerDay);
            if (def.HasIrrigationAndFertilizerSupport.HasValue) {
                sb.Append(",\n    hasIrrigationAndFertilizerSupport = ")
                  .Append(def.HasIrrigationAndFertilizerSupport.Value ? "True" : "False");
            }
            if (def.IsGreenhouse.HasValue) {
                sb.Append(",\n    isGreenhouse                  = ")
                  .Append(def.IsGreenhouse.Value ? "True" : "False");
            }
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",                   def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) {
                sb.Append(",\n    lockedOnInit                  = ").Append(def.LockedOnInit.Value ? "True" : "False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // add_crop(cropId, name, productProduced, consumedWaterPerDay,
        //     consumedFertilityPercentPerDay, minFertilityToStartGrowthPercent,
        //     growthDurationDays, surviveWithNoWaterDays, icon, prefab,
        //     requiresGreenhouse, plantByDefault, description, research,
        //     farms) — OR clone_crop(cropId, source, …same tail…) when
        // <see cref="CropDef.SourceId"/> is set.
        private static string renderCrop(CropDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            bool isClone = !string.IsNullOrEmpty(def.SourceId);
            sb.Append(isClone ? "clone_crop(\n" : "add_crop(\n");
            sb.Append("    cropId                          = "); appendNewId(sb, def.CropId);
            if (isClone) {
                sb.Append(",\n    source                          = "); appendIdRef(sb, def.SourceId, vars);
            }
            appendOptionalString(sb, "name",                             def.Name);
            appendOptionalString(sb, "description",                      def.Description);
            if (def.ProductProduced != null && !string.IsNullOrEmpty(def.ProductProduced.ProductId)) {
                sb.Append(",\n    productProduced                 = Product(");
                appendIdRef(sb, def.ProductProduced.ProductId, vars);
                sb.Append(", Quantity(").Append(def.ProductProduced.Quantity).Append("))");
            }
            appendOptionalInt   (sb, "consumedWaterPerDay",              def.ConsumedWaterPerDay);
            appendOptionalInt   (sb, "consumedFertilityPercentPerDay",   def.ConsumedFertilityPercentPerDay);
            appendOptionalInt   (sb, "minFertilityToStartGrowthPercent", def.MinFertilityToStartGrowthPercent);
            appendOptionalInt   (sb, "growthDurationDays",               def.GrowthDurationDays);
            appendOptionalInt   (sb, "surviveWithNoWaterDays",           def.SurviveWithNoWaterDays);
            if (!string.IsNullOrEmpty(def.IconPath)) {
                sb.Append(",\n    icon                            = ");
                appendIconRef(sb, def.IconPath, vars);
            }
            appendOptionalString(sb, "prefab",                           def.PrefabPath);
            if (def.RequiresGreenhouse.HasValue) {
                sb.Append(",\n    requiresGreenhouse              = ")
                  .Append(def.RequiresGreenhouse.Value ? "True" : "False");
            }
            if (def.PlantByDefault.HasValue) {
                sb.Append(",\n    plantByDefault                  = ")
                  .Append(def.PlantByDefault.Value ? "True" : "False");
            }
            appendOptionalIdRef (sb, "research",                         def.ResearchId, vars);
            if (def.Farms != null && def.Farms.Count > 0) {
                sb.Append(",\n    farms                           = [");
                for (int i = 0; i < def.Farms.Count; i++) {
                    if (i > 0) sb.Append(", ");
                    appendIdRef(sb, def.Farms[i], vars);
                }
                sb.Append("]");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderResearchLab(ResearchLabDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_research_lab(\n");
            sb.Append("    researchLabId            = "); appendNewId(sb, def.ResearchLabId); sb.Append(",\n");
            sb.Append("    source                   = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",                     def.Name);
            appendOptionalString(sb, "description",              def.Description);
            appendOptionalInt   (sb, "electricityConsumedKw",    def.ElectricityConsumedKw);
            appendOptionalInt   (sb, "computingConsumed",        def.ComputingConsumed);
            appendOptionalInt   (sb, "durationForRecipeSeconds", def.DurationForRecipeSeconds);
            appendOptionalInt   (sb, "sciencePerRecipe",         def.SciencePerRecipe);
            appendOptionalInt   (sb, "unityMonthlyCost",         def.UnityMonthlyCost);
            if (def.AddPorts != null && def.AddPorts.Count > 0) {
                sb.Append(",\n    add_ports                = ");
                appendPortList(sb, def.AddPorts);
            }
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",                 def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit             = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_nuclear_reactor_fuels(reactor, add_fuels=[FuelPair(...)]) — both args required.
        private static string renderEditNuclearReactorFuels(EditNuclearReactorFuelsDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_nuclear_reactor_fuels(\n");
            sb.Append("    reactor   = "); appendIdRef(sb, def.ReactorId, vars); sb.Append(",\n");
            sb.Append("    add_fuels = ");
            if (def.AddFuels != null && def.AddFuels.Count > 0) {
                appendFuelPairList(sb, def.AddFuels, vars);
            } else {
                sb.Append("[]");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderNuclearReactor(NuclearReactorDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_nuclear_reactor(\n");
            sb.Append("    reactorId              = "); appendNewId(sb, def.ReactorId); sb.Append(",\n");
            sb.Append("    source                 = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",                   def.Name);
            appendOptionalString(sb, "description",            def.Description);
            appendOptionalInt   (sb, "maxPowerLevel",          def.MaxPowerLevel);
            appendOptionalInt   (sb, "fuelCapacity",           def.FuelCapacity);
            appendOptionalInt   (sb, "minFuelToOperate",       def.MinFuelToOperate);
            appendOptionalInt   (sb, "processDurationSeconds", def.ProcessDurationSeconds);
            appendOptionalInt   (sb, "computingConsumed",      def.ComputingConsumed);
            if (def.FuelPairs != null && def.FuelPairs.Count > 0) {
                sb.Append(",\n    fuel_pairs             = ");
                appendFuelPairList(sb, def.FuelPairs, vars);
            }
            appendOptionalString(sb, "fuelInPortShape",        def.FuelInPortShape);
            appendOptionalString(sb, "fuelOutPortShape",       def.FuelOutPortShape);
            // Coolant / water / steam overrides — every field is optional;
            // null / empty inherits from the source reactor at runtime.
            appendOptionalIdRef (sb, "coolantIn",              def.CoolantInId, vars);
            appendOptionalIdRef (sb, "coolantOut",             def.CoolantOutId, vars);
            appendOptionalString(sb, "coolantInPort",          def.CoolantInPort);
            appendOptionalString(sb, "coolantOutPort",         def.CoolantOutPort);
            appendOptionalString(sb, "coolantInPortShape",     def.CoolantInPortShape);
            appendOptionalString(sb, "coolantOutPortShape",    def.CoolantOutPortShape);
            appendOptionalIdRef (sb, "waterInProduct",         def.WaterInProductId, vars);
            appendOptionalInt   (sb, "waterInQuantity",        def.WaterInQuantity);
            appendOptionalIdRef (sb, "steamOutProduct",        def.SteamOutProductId, vars);
            appendOptionalInt   (sb, "steamOutQuantity",       def.SteamOutQuantity);
            appendOptionalString(sb, "waterInPorts",           def.WaterInPorts);
            appendOptionalString(sb, "steamOutPorts",          def.SteamOutPorts);
            // Enrichment / breeding override.
            if (def.Enrichment != null) {
                sb.Append(",\n    enrichment             = ");
                appendEnrichment(sb, def.Enrichment, vars);
            }
            if (def.AddPorts != null && def.AddPorts.Count > 0) {
                sb.Append(",\n    add_ports              = ");
                appendPortList(sb, def.AddPorts);
            }
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",               def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit           = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_nuclear_reactor_ports(reactor, add_ports=[Port(...), ...])
        private static string renderEditNuclearReactorPorts(EditNuclearReactorPortsDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_nuclear_reactor_ports(\n");
            sb.Append("    reactor   = "); appendIdRef(sb, def.ReactorId, vars);
            if (def.AddPorts != null && def.AddPorts.Count > 0) {
                sb.Append(",\n    add_ports = ");
                appendPortList(sb, def.AddPorts);
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_nuclear_reactor_fluids(reactor, coolantIn=..., ...) —
        // patches the coolant/water/steam fields on an existing reactor.
        // Same null-inherit semantics as build_nuclear_reactor's fluid
        // block, so the emitter omits unset fields rather than write
        // `coolantIn = None`.
        private static string renderEditNuclearReactorFluids(EditNuclearReactorFluidsDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_nuclear_reactor_fluids(\n");
            sb.Append("    reactor             = "); appendIdRef(sb, def.ReactorId, vars);
            appendOptionalIdRef (sb, "coolantIn",           def.CoolantInId, vars);
            appendOptionalIdRef (sb, "coolantOut",          def.CoolantOutId, vars);
            appendOptionalString(sb, "coolantInPort",       def.CoolantInPort);
            appendOptionalString(sb, "coolantOutPort",      def.CoolantOutPort);
            appendOptionalString(sb, "coolantInPortShape",  def.CoolantInPortShape);
            appendOptionalString(sb, "coolantOutPortShape", def.CoolantOutPortShape);
            appendOptionalIdRef (sb, "waterInProduct",      def.WaterInProductId, vars);
            appendOptionalInt   (sb, "waterInQuantity",     def.WaterInQuantity);
            appendOptionalIdRef (sb, "steamOutProduct",     def.SteamOutProductId, vars);
            appendOptionalInt   (sb, "steamOutQuantity",    def.SteamOutQuantity);
            appendOptionalString(sb, "waterInPorts",        def.WaterInPorts);
            appendOptionalString(sb, "steamOutPorts",       def.SteamOutPorts);
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_nuclear_reactor_enrichment(reactor, enrichment=Enrichment(...))
        private static string renderEditNuclearReactorEnrichment(EditNuclearReactorEnrichmentDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("edit_nuclear_reactor_enrichment(\n");
            sb.Append("    reactor    = "); appendIdRef(sb, def.ReactorId, vars);
            if (def.Enrichment != null) {
                sb.Append(",\n    enrichment = ");
                appendEnrichment(sb, def.Enrichment, vars);
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // Emit an Enrichment(...) call literal. Every field is optional;
        // null / empty / "inherit" sub-fields are omitted from the
        // output so partial overrides round-trip cleanly. Step list
        // renders inline; empty list → "steps = []".
        private static void appendEnrichment(StringBuilder sb, EnrichmentRef er,
                System.Collections.Generic.Dictionary<string, string> vars) {
            sb.Append("Enrichment(");
            bool first = true;
            if (!string.IsNullOrEmpty(er.InputProductId)) {
                appendEnrichKv(sb, ref first, "inputProduct"); appendIdRef(sb, er.InputProductId, vars);
            }
            if (!string.IsNullOrEmpty(er.InPort)) {
                appendEnrichKv(sb, ref first, "inPort"); appendString(sb, er.InPort);
            }
            if (!string.IsNullOrEmpty(er.OutputProductId)) {
                appendEnrichKv(sb, ref first, "outputProduct"); appendIdRef(sb, er.OutputProductId, vars);
            }
            if (!string.IsNullOrEmpty(er.OutPort)) {
                appendEnrichKv(sb, ref first, "outPort"); appendString(sb, er.OutPort);
            }
            if (er.ProcessedPerLevelNumerator.HasValue) {
                appendEnrichKv(sb, ref first, "processedPerLevel");
                int num = er.ProcessedPerLevelNumerator.Value;
                int den = er.ProcessedPerLevelDenominator ?? 1;
                if (den == 1) {
                    sb.Append(num);
                } else {
                    sb.Append('(').Append(num).Append(", ").Append(den).Append(')');
                }
            }
            if (er.BuffersCapacity.HasValue) {
                appendEnrichKv(sb, ref first, "buffersCapacity"); sb.Append(er.BuffersCapacity.Value);
            }
            if (er.DestroyContentOnMeltdown.HasValue) {
                appendEnrichKv(sb, ref first, "destroyContentOnMeltdown");
                sb.Append(er.DestroyContentOnMeltdown.Value ? "True" : "False");
            }
            if (er.DefaultEnrichmentStep.HasValue) {
                appendEnrichKv(sb, ref first, "defaultEnrichmentStep"); sb.Append(er.DefaultEnrichmentStep.Value);
            }
            if (er.Steps != null && er.Steps.Count > 0) {
                appendEnrichKv(sb, ref first, "steps");
                sb.Append("[");
                for (int i = 0; i < er.Steps.Count; i++) {
                    EnrichmentStepRef s = er.Steps[i];
                    sb.Append("EnrichmentStep(fuelMultiplierPercent=").Append(s.FuelMultiplierPercent)
                      .Append(", breedingRatio=").Append(s.BreedingRatio)
                      .Append(", steamReductionDiv=").Append(s.SteamReductionDiv).Append(")");
                    if (i < er.Steps.Count - 1) sb.Append(", ");
                }
                sb.Append("]");
            }
            sb.Append(")");
        }

        private static void appendEnrichKv(StringBuilder sb, ref bool first, string key) {
            if (!first) sb.Append(", ");
            sb.Append(key).Append("=");
            first = false;
        }

        // [FuelPair(fuelIn, spentFuelOut, durationSeconds), ...] — list
        // literal rendering for the build_nuclear_reactor.fuel_pairs arg.
        // Each entry's product ids go through appendIdRef so vanilla
        // typed-refs (Ids.Products.UraniumFuelRod) render bare and
        // modder-authored ids ("Product_X") get quoted.
        private static void appendFuelPairList(StringBuilder sb, List<FuelPairRef> items,
                System.Collections.Generic.Dictionary<string, string> vars) {
            appendFuelPairList(new PyWriter(sb, PyWriter.IndentUnit), items, vars);
        }

        private static void appendFuelPairList(PyWriter w, List<FuelPairRef> items,
                System.Collections.Generic.Dictionary<string, string> vars) {
            StringBuilder sb = w.Buffer;
            PyWriter item = w.Indent();
            w.Append("[");
            for (int i = 0; i < items.Count; i++) {
                FuelPairRef p = items[i];
                item.Append("\n");
                sb.Append("FuelPair(fuelIn=");
                appendIdRef(sb, p.FuelIn, vars);
                sb.Append(", spentFuelOut=");
                appendIdRef(sb, p.SpentFuelOut, vars);
                sb.Append(", durationSeconds=").Append(p.DurationSeconds);
                sb.Append(")");
                if (i < items.Count - 1) sb.Append(",");
            }
            w.Append("\n]");
        }

        // [Port(name="X", type="input", shape="IoPortShape_Pipe",
        //       position=(x, y, z), direction="+X")]
        // Position prefers the parsed X/Y/Z fields — emits as a literal
        // `(x, y, z)` tuple. Only falls back to PositionExpression when the
        // modder typed a non-tuple shape (typed-ref / Vector3i ctor / list
        // literal) that the loader couldn't reduce to integers.
        private static void appendPortList(StringBuilder sb, List<PortRef> items) {
            appendPortList(new PyWriter(sb, PyWriter.IndentUnit), items);
        }

        private static void appendPortList(PyWriter w, List<PortRef> items) {
            StringBuilder sb = w.Buffer;
            PyWriter item = w.Indent();
            w.Append("[");
            for (int i = 0; i < items.Count; i++) {
                PortRef p = items[i];
                item.Append("\n");
                sb.Append("Port(name=");
                appendString(sb, p.Name);
                sb.Append(", type=");
                appendString(sb, p.Type);
                sb.Append(", shape=");
                appendString(sb, p.Shape);
                sb.Append(", position=");
                if (string.IsNullOrEmpty(p.PositionExpression)) {
                    sb.Append("(").Append(p.PositionX)
                      .Append(", ").Append(p.PositionY)
                      .Append(", ").Append(p.PositionZ)
                      .Append(")");
                } else {
                    sb.Append(p.PositionExpression);
                }
                sb.Append(", direction=");
                appendString(sb, p.Direction);
                if (p.CanOnlyConnectToTransports) {
                    sb.Append(", canOnlyConnectToTransports=True");
                }
                sb.Append(")");
                if (i < items.Count - 1) sb.Append(",");
            }
            w.Append("\n]");
        }

        // add_texture(path, replace=None) — single-line when no replace,
        // multi-line otherwise.
        private static string renderTexture(TextureDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            if (string.IsNullOrEmpty(def.ReplacePath)) {
                sb.Append("add_texture(");
                appendString(sb, def.Path);
                sb.Append(")");
            } else {
                sb.Append("add_texture(\n");
                sb.Append("    path    = "); appendString(sb, def.Path);        sb.Append(",\n");
                sb.Append("    replace = "); appendString(sb, def.ReplacePath);
                sb.Append("\n)");
            }
            return sb.ToString();
        }

        // add_loose_product_material(path, albedo, normals=None,
        //     metallic=None, reference=None, tiling=1)
        private static string renderMaterialLoose(MaterialLooseDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("add_loose_product_material(\n");
            sb.Append("    path   = "); appendString(sb, def.Path); sb.Append(",\n");
            sb.Append("    albedo = ").Append(def.AlbedoExpression ?? "None");
            appendOptionalRaw(sb, "normals",   def.NormalsExpression);
            appendOptionalRaw(sb, "metallic",  def.MetallicExpression);
            appendOptionalRaw(sb, "reference", def.ReferenceExpression);
            appendOptionalRaw(sb, "tiling",    def.TilingExpression);
            sb.Append("\n)");
            return sb.ToString();
        }

        // add_prefab_box(path, texture=None)
        private static string renderPrefabBox(PrefabBoxDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            if (string.IsNullOrEmpty(def.TextureExpression)) {
                sb.Append("add_prefab_box(");
                appendString(sb, def.Path);
                sb.Append(")");
            } else {
                sb.Append("add_prefab_box(\n");
                sb.Append("    path    = "); appendString(sb, def.Path); sb.Append(",\n");
                sb.Append("    texture = ").Append(def.TextureExpression);
                sb.Append("\n)");
            }
            return sb.ToString();
        }

        // add_unit_prefab(path, albedo, normals, metallic, reference,
        //     width, height, depth, mesh, winding) — registers a unit-prefab
        // asset. Albedo is required; the rest are optional and emitted only
        // when set. `winding` is the .obj fan-triangulation mode ("ccw" / "cw");
        // omitted means default ("ccw" pass-through).
        private static string renderUnitPrefab(UnitPrefabDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("add_unit_prefab(\n");
            sb.Append("    path   = "); appendString(sb, def.Path); sb.Append(",\n");
            sb.Append("    albedo = ").Append(def.AlbedoExpression ?? "None");
            appendOptionalRaw(sb, "normals",   def.NormalsExpression);
            appendOptionalRaw(sb, "metallic",  def.MetallicExpression);
            appendOptionalRaw(sb, "reference", def.ReferenceExpression);
            if (def.Width.HasValue)  sb.Append(",\n    width    = ").Append(formatDouble(def.Width.Value));
            if (def.Height.HasValue) sb.Append(",\n    height   = ").Append(formatDouble(def.Height.Value));
            if (def.Depth.HasValue)  sb.Append(",\n    depth    = ").Append(formatDouble(def.Depth.Value));
            appendOptionalString(sb, "mesh",    def.MeshPath);
            appendOptionalString(sb, "winding", def.Winding);
            sb.Append("\n)");
            return sb.ToString();
        }

        // add_texture_material(path, texture, reference, shader). All three
        // optional tails are named. Texture / reference are raw expressions
        // (the picker emits quoted paths or dotted typed-refs via
        // AssetExpression.Render); shader is a plain string literal.
        private static string renderTextureMaterial(MaterialTextureDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("add_texture_material(\n");
            sb.Append("    path = "); appendString(sb, def.Path);
            appendOptionalRaw   (sb, "texture",   def.TextureExpression);
            appendOptionalRaw   (sb, "reference", def.ReferenceExpression);
            appendOptionalString(sb, "shader",    def.Shader);
            sb.Append("\n)");
            return sb.ToString();
        }

        // edit_recipe(recipe, duration, ingredients, products, machine,
        //     research, power). Same body shape as build_recipe but no
        // name/description and the recipe id can be a typed-ref to a vanilla
        // proto (Ids.Recipes.X), so appendIdRef handles both shapes.
        private static string renderEditRecipe(EditRecipeDef def) {
            // Block form: the header carries only the recipe; every change is a
            // sub-action statement in the body, each splicing itself by scope like
            // an if-clause's statements. renderDefinition re-renders the HEADER ONLY
            // — its source range covers just the `with edit_recipe(...):` line, and
            // the body statements own their own ranges. (Block CREATION, which needs
            // a `pass` body, goes through editEmitBlockHeader below, not here.)
            if (def.EmitAsWithBlock) {
                StringBuilder blk = new StringBuilder();
                appendCommentLines(blk, def.Comment);
                blk.Append(editBlockHeaderLine(def));
                return blk.ToString();
            }

            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("edit_recipe(\n");
            var vars = def.SourceFileVariables;
            sb.Append("    "); appendIdRef(sb, def.RecipeId, vars);
            if (def.DurationSeconds.HasValue || !string.IsNullOrWhiteSpace(def.DurationExpression)) {
                sb.Append(",\n    duration = Duration.FromSec(")
                  .Append(durationArg(def.DurationSeconds, def.DurationExpression)).Append(")");
            }
            if (def.Ingredients != null && def.Ingredients.Count > 0) {
                sb.Append(",\n    ingredients = ");
                appendProductList(sb, def.Ingredients, vars);
            }
            if (def.Products != null && def.Products.Count > 0) {
                sb.Append(",\n    products = ");
                appendProductList(sb, def.Products, vars);
            }
            if (def.MachineId != null) {
                sb.Append(",\n    machine = "); appendIdRef(sb, def.MachineId, vars);
            }
            if (def.ResearchId != null) {
                sb.Append(",\n    research = "); appendIdRef(sb, def.ResearchId, vars);
                if (!def.UnlockMachine) {
                    sb.Append(",\n    unlock_machine = False");
                }
            }
            if (def.PowerPercent.HasValue) {
                sb.Append(",\n    power = ").Append(def.PowerPercent.Value);
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // Just the `with edit_recipe(recipe) [as v]:` header line (no comment, no
        // body). Shared by renderEditRecipe (re-render) and the block-creation path.
        private static string editBlockHeaderLine(EditRecipeDef def) {
            StringBuilder sb = new StringBuilder();
            sb.Append("with edit_recipe(");
            appendIdRef(sb, def.RecipeId, def.SourceFileVariables);
            sb.Append(")");
            if (!string.IsNullOrEmpty(def.VariableName)) {
                sb.Append(" as ").Append(def.VariableName);
            }
            sb.Append(":");
            return sb.ToString();
        }

        /// Render a NEW `with edit_recipe(recipe):` block with a `pass` body, for
        /// first insertion into a file. The editor then adds sub-actions into it
        /// with AppendDefIntoClause, which replaces the `pass`. Mirrors the
        /// `if <cond>:\n    pass` shape AppendIfBlockIntoClause writes.
        public static string RenderEditRecipeBlockHeader(EditRecipeDef def) {
            StringBuilder sb = new StringBuilder();
            PyWriter w = new PyWriter(sb);
            appendCommentLines(sb, def.Comment);
            sb.Append(editBlockHeaderLine(def));
            w.Indent().Append("\npass");
            return w.ToString();
        }

        // set_ingredient / set_product / remove_ingredient / remove_product — a
        // sub-action inside a `with edit_recipe(...)` body. Rendered at column 0;
        // the block splice shifts it into place. The recipe is implicit (the
        // enclosing block supplies it), so only the product (+ quantity for a set)
        // is emitted.
        // Rendered on ONE line: these verbs take one or two short operands, so the
        // multi-line argument style the bigger calls use would only add noise. A
        // single line also matches how a modder naturally writes them, keeping the
        // load→save round-trip byte-identical.
        private static string renderProductAction(RecipeProductActionDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            string fn = (def.IsRemoval ? "remove_" : "set_")
                + (def.IsInput ? "ingredient" : "product");
            sb.Append(fn).Append("(");
            appendIdRef(sb, def.ProductId, vars);
            if (!def.IsRemoval) {
                sb.Append(", Quantity(").Append(def.Quantity ?? 0).Append(")");
            }
            sb.Append(")");
            return sb.ToString();
        }

        // unbind_recipe(machine, research=...) — context form (recipe implicit),
        // one line for the same reason as the product actions above.
        private static string renderUnbindRecipe(UnbindRecipeDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("unbind_recipe(");
            appendIdRef(sb, def.MachineId, vars);
            if (def.ResearchId != null) {
                sb.Append(", research = "); appendIdRef(sb, def.ResearchId, vars);
            }
            sb.Append(")");
            return sb.ToString();
        }

        // Format a double using invariant culture so a `1.5` width arg never
        // emits as `1,5` on locales where the comma is the decimal separator
        // — that's a Python syntax error.
        private static string formatDouble(double v) {
            return v.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Sentinel value PackLoader stores when it couldn't parse a field's
        // expression (typed-refs nested in expressions, novel AST shapes,
        // etc. — see PackLoader.UnparseableId / renderExpressionAsText).
        // Without this guard the emitter would happily round-trip
        // `<unparseable>` back into the .py file as a literal string,
        // breaking the next mod load with a runtime error like
        // "Requested value '<unparseable>' was not found." The fix:
        // treat the sentinel as if the field were missing, so the
        // affected arg either skips emission (optional fields) or
        // becomes None (required fields). The modder loses the original
        // expression they wrote — there's nothing we can do about that
        // until we capture raw source text per field — but at least the
        // file stays loadable.
        private const string UnparseableSentinel = "<unparseable>";
        private static bool isMissingOrSentinel(string v) {
            return string.IsNullOrEmpty(v) || v == UnparseableSentinel;
        }

        // add_toolbar_category(categoryId, name, icon, parent, entities)
        private static string renderToolbarCategory(ToolbarCategoryDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("add_toolbar_category(\n");
            var vars = def.SourceFileVariables;
            sb.Append("    categoryId = "); appendNewId(sb, def.CategoryId); sb.Append(",\n");
            sb.Append("    name       = "); appendString(sb, def.Name);     sb.Append(",\n");
            sb.Append("    icon       = "); appendIconRef(sb, def.IconPath, def.SourceFileVariables); sb.Append(",\n");
            sb.Append("    parent     = "); appendIdRef(sb, def.ParentId, vars);  sb.Append(",\n");
            sb.Append("    entities   = ").Append(def.EntitiesExpression ?? "[]");
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_generator(...). Source has a meaningful default
        // ("DieselGeneratorT2") so we only emit it when the modder chose
        // something different — preserves round-trip on calls that omitted
        // the arg.
        private static string renderGenerator(GeneratorDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_generator(\n");
            sb.Append("    id                  = "); appendNewId(sb, def.GeneratorId);             sb.Append(",\n");
            sb.Append("    name                = "); appendString(sb, def.Name);                    sb.Append(",\n");
            sb.Append("    inputProduct        = ").Append(def.InputProductExpression ?? "None");  sb.Append(",\n");
            sb.Append("    outputElectricityKw = ").Append(def.OutputElectricityKw ?? 0);
            appendOptionalRaw   (sb, "outputProduct",            def.OutputProductExpression);
            appendOptionalString(sb, "description",              def.Description);
            // Source defaults to DieselGeneratorT2 — emit only when changed.
            if (!string.IsNullOrEmpty(def.SourceId) && def.SourceId != "DieselGeneratorT2") {
                sb.Append(",\n    source              = "); appendIdRef(sb, def.SourceId, vars);
            }
            if (def.DurationSeconds.HasValue) {
                sb.Append(",\n    duration            = Duration.FromSec(")
                  .Append(def.DurationSeconds.Value).Append(")");
            }
            appendOptionalInt   (sb, "generationPriority",       def.GenerationPriority);
            appendOptionalInt   (sb, "bufferCapacityMultiplier", def.BufferCapacityMultiplier);
            appendOptionalIdRef (sb, "research",                 def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) {
                sb.Append(",\n    lockedOnInit        = ").Append(def.LockedOnInit.Value ? "True" : "False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_product_loose(...). Required positionals first (productId,
        // name, icon, material), then optionals as named args — emit only
        // when present so the file doesn't accrete explicit defaults that
        // weren't in the original source.
        private static string renderProductLoose(ProductLooseDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_product_loose(\n");
            sb.Append("    productId = "); appendNewId(sb, def.ProductId); sb.Append(",\n");
            sb.Append("    name      = "); appendString(sb, def.Name);     sb.Append(",\n");
            sb.Append("    icon      = "); appendIconRef(sb, def.IconPath, def.SourceFileVariables); sb.Append(",\n");
            sb.Append("    material  = ").Append(def.MaterialExpression ?? "None");
            appendOptionalString(sb, "description",   def.Description);
            appendOptionalRaw   (sb, "color",         def.ColorExpression);
            appendOptionalRaw   (sb, "particleColor", def.ParticleColorExpression);
            appendOptionalBool  (sb, "isDumped",        def.IsDumped);
            appendOptionalBool  (sb, "isStorable",      def.IsStorable);
            appendOptionalBool  (sb, "isRecyclable",    def.IsRecyclable);
            appendOptionalBool  (sb, "isWaste",         def.IsWaste);
            appendOptionalBoolOrNull(sb, "isLocked",    def.IsLocked);
            appendOptionalBool  (sb, "isRough",         def.IsRough);
            appendOptionalBool  (sb, "pinToHomeScreen", def.PinToHomeScreen);
            appendOptionalInt   (sb, "maxTransport",    def.MaxTransport);
            appendOptionalString(sb, "prefabPath",      def.PrefabPath);
            appendOptionalIdRef (sb, "dumpsAs",         def.DumpsAsId, vars);
            appendOptionalIdRef (sb, "research",        def.ResearchId, vars);
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_product_fluid(...).
        private static string renderProductFluid(ProductFluidDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_product_fluid(\n");
            sb.Append("    productId = "); appendNewId(sb, def.ProductId); sb.Append(",\n");
            sb.Append("    name      = "); appendString(sb, def.Name);     sb.Append(",\n");
            sb.Append("    icon      = "); appendIconRef(sb, def.IconPath, def.SourceFileVariables);
            appendOptionalRaw   (sb, "color",                def.ColorExpression);
            appendOptionalRaw   (sb, "transportColor",       def.TransportColorExpression);
            appendOptionalRaw   (sb, "transportAccentColor", def.TransportAccentColorExpression);
            // canBeDiscarded defaults to True in the API; only emit when it
            // was explicitly set to False so we don't bloat round-trips.
            if (!def.CanBeDiscarded) sb.Append(",\n    canBeDiscarded = False");
            appendOptionalString(sb, "description", def.Description);
            appendOptionalBool  (sb, "isStorable",  def.IsStorable);
            appendOptionalBool  (sb, "isWaste",     def.IsWaste);
            appendOptionalBoolOrNull(sb, "isLocked", def.IsLocked);
            sb.Append("\n)");
            return sb.ToString();
        }

        // build_product_unit(...). Unit-product specific: prefab + packing
        // arguments.
        private static string renderProductUnit(ProductUnitDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_product_unit(\n");
            sb.Append("    productId = "); appendNewId(sb, def.ProductId); sb.Append(",\n");
            sb.Append("    name      = "); appendString(sb, def.Name);     sb.Append(",\n");
            sb.Append("    icon      = "); appendIconRef(sb, def.IconPath, def.SourceFileVariables); sb.Append(",\n");
            sb.Append("    prefab    = ").Append(def.PrefabExpression ?? "None");
            // maxTransport defaults to Quantity(3) — emit only when set.
            // We emit as a bare int; the runtime accepts both.
            appendOptionalInt   (sb, "maxTransport",                  def.MaxTransport);
            appendOptionalString(sb, "description",                   def.Description);
            appendOptionalBool  (sb, "isStorable",                    def.IsStorable);
            appendOptionalBool  (sb, "isWaste",                       def.IsWaste);
            appendOptionalBoolOrNull(sb, "isLocked",                  def.IsLocked);
            appendOptionalString(sb, "packingMode",                   def.PackingMode);
            appendOptionalBool  (sb, "allowPackingNoise",             def.AllowPackingNoise);
            appendOptionalBool  (sb, "rotateSecondPackedItem90Degs",  def.RotateSecondPackedItem90Degs);
            appendOptionalIdRef (sb, "research",                      def.ResearchId, vars);
            sb.Append("\n)");
            return sb.ToString();
        }

        // ---- Per-arg helpers --------------------------------------------
        //
        // The product renderers above all share the same "emit only when
        // set" rule for optional args, so they go through these helpers to
        // keep each call site to a single line. Each helper handles the
        // leading ",\n    name = " formatting too.

        private static void appendOptionalString(StringBuilder sb, string name, string value) {
            if (isMissingOrSentinel(value)) return;
            sb.Append(",\n    ").Append(name).Append(" = ");
            appendString(sb, value);
        }
        private static void appendOptionalRaw(StringBuilder sb, string name, string rawExpr) {
            if (isMissingOrSentinel(rawExpr)) return;
            sb.Append(",\n    ").Append(name).Append(" = ").Append(rawExpr);
        }
        private static void appendOptionalBool(StringBuilder sb, string name, bool value) {
            // Only emit `name = True`; `False` is the API default for every
            // bool product arg, so omitting is what keeps the file stable.
            if (!value) return;
            sb.Append(",\n    ").Append(name).Append(" = True");
        }
        // Tri-state counterpart for bool args whose runtime default is NOT
        // simply False — `isLocked` defaults to "locked when a research is
        // set". For those, an explicit False is meaningful and omitting is a
        // third, distinct state, so null is the only thing we may drop.
        private static void appendOptionalBoolOrNull(StringBuilder sb, string name, bool? value) {
            if (!value.HasValue) return;
            sb.Append(",\n    ").Append(name).Append(" = ").Append(value.Value ? "True" : "False");
        }
        private static void appendOptionalInt(StringBuilder sb, string name, int? value) {
            if (!value.HasValue) return;
            sb.Append(",\n    ").Append(name).Append(" = ").Append(value.Value);
        }
        // Fractional counterpart of appendOptionalInt. Formatted with the
        // INVARIANT culture on purpose: on a comma-decimal locale the default
        // ToString() would emit `maintenance = 4,0`, which the Python parser
        // reads as two arguments rather than one fractional number.
        private static void appendOptionalDouble(StringBuilder sb, string name, double? value) {
            if (!value.HasValue) return;
            sb.Append(",\n    ").Append(name).Append(" = ")
              .Append(value.Value.ToString("0.0###", System.Globalization.CultureInfo.InvariantCulture));
        }
        // Backwards-compat shim — kept for callers that pre-date variable
        // round-trip and don't yet have a SourceFileVariables map to pass.
        // Forwards to the variable-aware overload with a null map (falls
        // back to the dot/string heuristic, exactly the old behaviour).
        private static void appendOptionalIdRef(StringBuilder sb, string name, string id) {
            appendOptionalIdRef(sb, name, id, (System.Collections.Generic.Dictionary<string, string>)null);
        }

        // Emit build_research(...) canonical form. Optional args are emitted
        // only when present so the file doesn't accrete explicit defaults
        // that weren't in the original source. Comments precede the call;
        // PackEmitter.rewriteFile already preserves the original line's
        // leading indent so a research call nested inside an `if` clause
        // re-emits at the right depth.
        private static string renderResearch(ResearchDef def) {
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("build_research(\n");
            sb.Append("    "); appendString(sb, def.ResearchId);   sb.Append(",\n");
            sb.Append("    "); appendString(sb, def.Name);          sb.Append(",\n");
            sb.Append("    "); appendString(sb, def.Description);
            // costs — bare int when we have one; raw expression text when
            // the modder used a typed-ref shape we preserve verbatim.
            if (def.CostsAsInt.HasValue) {
                sb.Append(",\n    costs = ").Append(def.CostsAsInt.Value);
            } else if (!string.IsNullOrEmpty(def.CostsExpression)) {
                sb.Append(",\n    costs = ").Append(def.CostsExpression);
            }
            // position — only emit when at least one component is non-null.
            // Tuple form (X, Y) matches the API default signature.
            if (def.PositionX.HasValue || def.PositionY.HasValue) {
                int x = def.PositionX ?? 0;
                int y = def.PositionY ?? 0;
                sb.Append(",\n    position = (").Append(x).Append(", ").Append(y).Append(")");
            }
            // parents — the tech-tree wiring. Emitted before icon so the call
            // reads in the same order the API declares its arguments.
            if (def.Parents != null && def.Parents.Count > 0) {
                sb.Append(",\n    parents = [");
                for (int i = 0; i < def.Parents.Count; i++) {
                    if (i > 0) sb.Append(", ");
                    appendIdRef(sb, def.Parents[i], def.SourceFileVariables);
                }
                sb.Append("]");
            }
            if (!string.IsNullOrEmpty(def.IconPath)) {
                sb.Append(",\n    icon = "); appendIconRef(sb, def.IconPath, def.SourceFileVariables);
            }
            // tier — research-pack product hint set via the editor's
            // ResearchPackProductPicker. Emitted as appendIdRef so the
            // common typed-ref shape (Ids.Products.LabEquipment2) round-trips
            // bare while a plain "LabEquipment2" string-literal gets quoted.
            if (!string.IsNullOrEmpty(def.TierProductId)) {
                sb.Append(",\n    tier = ");
                appendIdRef(sb, def.TierProductId, def.SourceFileVariables);
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // add_unlock_recipe(research, machine, recipe) — three positional ids,
        // plus `unlock_machine = False` when the node should NOT hand over the
        // machine along with the recipe. Written only in that direction: true is
        // the API default, so spelling it out on every unlock would be noise.
        // Typed-ref vs string-literal heuristic is the same as build_recipe.
        private static string renderUnlockRecipe(UnlockRecipeDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("add_unlock_recipe(\n");
            sb.Append("    "); appendIdRef(sb, def.ResearchId, vars); sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.MachineId, vars);  sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.RecipeId, vars);
            if (!def.UnlockMachine) {
                sb.Append(",\n    unlock_machine = False");
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // migrate_recipe(old, new, since) — tombstone for a removed recipe id.
        // Named args throughout: the call reads as prose ("old X becomes new Y")
        // and `old`/`new` are easy to transpose positionally, which would silently
        // migrate the wrong direction. `old` is always a quoted literal — it names
        // a recipe that no longer exists, so it can never be a file variable.
        private static string renderMigrateRecipe(MigrateRecipeDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("migrate_recipe(\n");
            sb.Append("    old = ");  appendString(sb, def.OldRecipeId); sb.Append(",\n");
            sb.Append("    new = ");  appendIdRef(sb, def.NewRecipeId, vars);
            if (!string.IsNullOrWhiteSpace(def.Since)) {
                sb.Append(",\n    since = ");
                appendString(sb, def.Since);
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // bind_recipe(recipe, machine, duration, ports, multiplier,
        // minPartialUtilization, research) — attaches a recipe to a machine.
        // recipe + machine are positional; everything else is an optional named
        // arg emitted only when meaningfully set (so the file doesn't accrete
        // explicit defaults). `ports` reuses the Product(...) list shape.
        /// <paramref name="contextForm"/>: inside a `with build_recipe(...)`
        /// block the recipe is implicit, so only the machine is emitted as the
        /// leading positional. Outside a block both are emitted.
        private static string renderBindRecipe(BindRecipeDef def, bool contextForm = false) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            // Rendered at column 0 — the splice layer shifts the whole statement
            // into its block. `arg` supplies the argument indent after each "\n".
            PyWriter w = new PyWriter(sb);
            PyWriter arg = w.Indent();
            arg.Append("bind_recipe(\n");
            if (!contextForm) {
                appendIdRef(sb, def.RecipeId, vars);
                arg.Append(",\n");
            }
            appendIdRef(sb, def.MachineId, vars);
            if (def.DurationSeconds.HasValue || !string.IsNullOrWhiteSpace(def.DurationExpression)) {
                arg.Append(",\nduration = Duration.FromSec(");
                sb.Append(durationArg(def.DurationSeconds, def.DurationExpression)).Append(")");
            }
            // Always emit the port map — a binding is self-describing about how
            // every product routes to a machine port (the editor seeds it fully).
            arg.Append(",\nports = ");
            appendPortMapList(arg, def.Ports ?? new List<PortMapRef>(), vars);
            if (def.Multiplier.HasValue && def.Multiplier.Value != 1) {
                arg.Append(",\nmultiplier = ");
                sb.Append(def.Multiplier.Value);
            }
            if (def.MinPartialUtilizationPercent.HasValue) {
                arg.Append(",\nminPartialUtilization = ");
                sb.Append(def.MinPartialUtilizationPercent.Value);
            }
            if (def.ResearchId != null) {
                arg.Append(",\nresearch = "); appendIdRef(sb, def.ResearchId, vars);
                // Only meaningful with a research node — a binding without one
                // wires no unlock at all, so the flag would be dead text.
                if (!def.UnlockMachine) {
                    arg.Append(",\nunlock_machine = False");
                }
            }
            w.Append("\n)");
            return sb.ToString();
        }

        // add_unlock_product(research, product).
        private static string renderUnlockProduct(UnlockProductDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("add_unlock_product(\n");
            sb.Append("    "); appendIdRef(sb, def.ResearchId, vars); sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.ProductId, vars);
            sb.Append("\n)");
            return sb.ToString();
        }

        // add_unlock_machine(research, machine).
        private static string renderUnlockMachine(UnlockMachineDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("add_unlock_machine(\n");
            sb.Append("    "); appendIdRef(sb, def.ResearchId, vars); sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.MachineId, vars);
            sb.Append("\n)");
            return sb.ToString();
        }

        // add_unlock_entity(research, entity). Named rather than positional on
        // the second argument so a reader can tell it apart from
        // add_unlock_machine at a glance.
        private static string renderUnlockEntity(UnlockEntityDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("add_unlock_entity(\n");
            sb.Append("    research = "); appendIdRef(sb, def.ResearchId, vars); sb.Append(",\n");
            sb.Append("    entity   = "); appendIdRef(sb, def.EntityId, vars);
            sb.Append("\n)");
            return sb.ToString();
        }

        // remove_unlock(research, target, machine=…). Named args throughout —
        // `target` accepts any kind of proto, so spelling it out is what tells a
        // reader whether a bare id is the thing being removed or the machine it
        // is scoped to. `machine` is emitted only when the removal is scoped.
        private static string renderRemoveUnlock(RemoveUnlockDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("remove_unlock(\n");
            sb.Append("    research = "); appendIdRef(sb, def.ResearchId, vars); sb.Append(",\n");
            sb.Append("    target   = "); appendIdRef(sb, def.TargetId, vars);
            if (!string.IsNullOrEmpty(def.MachineId)) {
                sb.Append(",\n    machine  = ");
                appendIdRef(sb, def.MachineId, vars);
            }
            sb.Append("\n)");
            return sb.ToString();
        }

        // Shared comment-block emitter. Mirrors what RenderRecipe does inline:
        // every line of def.Comment becomes `# <line>` (or bare `#` for empty
        // lines) above the call, separated by '\n'. Pulled out so each typed
        // renderer above doesn't repeat the loop.
        private static void appendCommentLines(StringBuilder sb, string comment) {
            if (string.IsNullOrEmpty(comment)) return;
            foreach (string line in comment.Split('\n')) {
                string body = line.TrimEnd('\r');
                if (body.Length == 0) sb.Append("#\n");
                else sb.Append("# ").Append(body).Append('\n');
            }
        }

        // Statement prefix shared by every renderer: comment block first
        // (each comment line ends with its own '\n'), then a Python
        // assignment prefix `<VariableName> = ` when the def was originally
        // captured from `<var> = <call>(...)`. The renderer's existing
        // `sb.Append("<callName>(")` follows immediately after, producing
        // `<comments>\n<var> = <callName>(...)`.
        //
        // Replaces the older `appendCommentLines(sb, def.Comment)` boilerplate
        // at the head of every renderer; centralising both behaviours here
        // means a future change to the assignment shape (e.g. typed
        // annotations) lands in one place.
        private static void appendStmtPrefix(StringBuilder sb, DefBase def) {
            appendCommentLines(sb, def.Comment);
            if (!string.IsNullOrEmpty(def.VariableName)) {
                sb.Append(def.VariableName).Append(" = ");
            }
        }

        // ---- Render helpers ----------------------------------------------------

        private static void appendString(StringBuilder sb, string value) {
            if (value == null || value == UnparseableSentinel) {
                // Treat the loader's "couldn't parse" sentinel as missing
                // rather than emitting `"<unparseable>"` as a literal —
                // see [[isMissingOrSentinel]] for full rationale.
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

        /// Shim for callers that still build on a raw StringBuilder. A list is
        /// always an ARGUMENT, i.e. one level inside its call, so that is the
        /// depth the writer starts at.
        // Text for a `Duration.FromSec(...)` argument. An expression wins over the
        // number: the int is only what the loader could parse, so re-emitting it would
        // replace `config.smelt_seconds` with whatever it happened to fall back to.
        // Both are kept in the model, so clearing the expression restores the number.
        private static string durationArg(int? seconds, string expression) {
            return string.IsNullOrWhiteSpace(expression)
                ? (seconds ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)
                : expression.Trim();
        }

        private static void appendProductList(StringBuilder sb, List<ProductRef> items,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            appendProductList(new PyWriter(sb, PyWriter.IndentUnit), items, variables);
        }

        private static void appendProductList(PyWriter w, List<ProductRef> items,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            StringBuilder sb = w.Buffer;
            PyWriter item = w.Indent();
            w.Append("[");
            for (int i = 0; i < items.Count; i++) {
                ProductRef p = items[i];
                // The writer supplies the item indent after each newline, so the
                // entry itself starts at "Product(" — no hand-counted spaces.
                item.Append("\n");
                sb.Append("Product(");
                appendIdRef(sb, p.ProductId, variables);
                // Quantity(N) wrapper matches modder convention. The loader
                // unwraps it back into the int field, so a load → edit → save
                // cycle preserves the same code form.
                //
                // An EXPRESSION wins over the number: `Quantity(config.batch)` must
                // come back out as it went in, not as the 0 the int field holds for
                // something it could not parse.
                sb.Append(", Quantity(")
                  .Append(string.IsNullOrWhiteSpace(p.QuantityExpression)
                      ? p.Quantity.ToString()
                      : p.QuantityExpression.Trim())
                  .Append(")");
                if (!string.IsNullOrEmpty(p.Port) && p.Port != "*") {
                    sb.Append(", ");
                    appendString(sb, p.Port);
                }
                sb.Append(")");
                if (i < items.Count - 1) sb.Append(",");
            }
            w.Append("\n]");
        }

        // bind_recipe's `ports` — a list of PortMap(product, "X") (no quantity).
        private static void appendPortMapList(StringBuilder sb, List<PortMapRef> items,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            appendPortMapList(new PyWriter(sb, PyWriter.IndentUnit), items, variables);
        }

        private static void appendPortMapList(PyWriter w, List<PortMapRef> items,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            StringBuilder sb = w.Buffer;
            if (items == null || items.Count == 0) { sb.Append("[]"); return; }
            PyWriter item = w.Indent();
            w.Append("[");
            for (int i = 0; i < items.Count; i++) {
                PortMapRef p = items[i];
                item.Append("\n");
                sb.Append("PortMap(");
                appendIdRef(sb, p.ProductId, variables);
                sb.Append(", ");
                appendString(sb, string.IsNullOrEmpty(p.Port) ? "*" : p.Port);
                sb.Append(")");
                if (i < items.Count - 1) sb.Append(",");
            }
            w.Append("\n]");
        }
    }
}
