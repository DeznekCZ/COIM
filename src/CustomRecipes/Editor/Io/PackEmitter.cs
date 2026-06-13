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

            // Header indent (whitespace before `if`/`elif`/`else`).
            string headerLine = lines[clauseHeaderLine - 1];
            string headerIndent = leadingWhitespace(headerLine);
            string bodyIndent = headerIndent + "    ";

            // Walk forward from the header looking for the body's actual
            // indentation (the first non-blank line after the header that
            // sits deeper than headerIndent). Then walk past the body to
            // find its END — the last line whose indent ≥ bodyIndent.
            // Anything below that belongs to a sibling clause or
            // outer-scope code.
            int firstBodyLine = -1;
            for (int i = clauseHeaderLine; i < lines.Length; i++) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string indent = leadingWhitespace(line);
                if (indent.Length <= headerIndent.Length) break;
                firstBodyLine = i; // 0-based
                bodyIndent = indent;
                break;
            }

            int insertAfterIdx; // 0-based index AFTER which to insert
            if (firstBodyLine < 0) {
                // Empty body (e.g. fresh `if X:` followed by `pass` only,
                // and we missed pass). Insert right after header.
                insertAfterIdx = clauseHeaderLine - 1;
            } else {
                insertAfterIdx = firstBodyLine;
                for (int i = firstBodyLine + 1; i < lines.Length; i++) {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) {
                        insertAfterIdx = i;
                        continue;
                    }
                    string indent = leadingWhitespace(line);
                    if (indent.Length >= bodyIndent.Length) {
                        insertAfterIdx = i;
                    } else {
                        break;
                    }
                }
            }

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
            string headerLine = lines[clauseHeaderLine - 1];
            string headerIndent = leadingWhitespace(headerLine);
            string bodyIndent = headerIndent + "    ";
            int firstBodyLine = -1;
            for (int i = clauseHeaderLine; i < lines.Length; i++) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string indent = leadingWhitespace(line);
                if (indent.Length <= headerIndent.Length) break;
                firstBodyLine = i;
                bodyIndent = indent;
                break;
            }
            int insertAfterIdx;
            if (firstBodyLine < 0) {
                insertAfterIdx = clauseHeaderLine - 1;
            } else {
                insertAfterIdx = firstBodyLine;
                for (int i = firstBodyLine + 1; i < lines.Length; i++) {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) { insertAfterIdx = i; continue; }
                    string indent = leadingWhitespace(line);
                    if (indent.Length >= bodyIndent.Length) {
                        insertAfterIdx = i;
                    } else {
                        break;
                    }
                }
            }

            // Render `if <cond>:\n    pass` — body indent (4 spaces) is
            // applied on top of the clause-body indent via the splice
            // helper, giving the nested `pass` an indent of bodyIndent + 4.
            string rendered = "if " + cond + ":\n    pass";
            spliceRenderedIntoClauseBody(filePath, lines, clauseHeaderLine - 1,
                insertAfterIdx, bodyIndent, rendered);
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

            string[] renderedLines = rendered.Split('\n');
            List<string> indented = new List<string>(renderedLines.Length);
            for (int i = 0; i < renderedLines.Length; i++) {
                string r = renderedLines[i];
                if (r.EndsWith("\r", StringComparison.Ordinal)) {
                    r = r.Substring(0, r.Length - 1);
                }
                if (string.IsNullOrEmpty(r)) {
                    indented.Add("");
                } else {
                    indented.Add(bodyIndent + r);
                }
            }

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

        private static string leadingWhitespace(string line) {
            int n = 0;
            while (n < line.Length && (line[n] == ' ' || line[n] == '\t')) n++;
            return line.Substring(0, n);
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

            string indent = readLeadingWhitespace(lines[startLine - 1]);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < modelOrder.Count; i++) {
                if (i > 0) sb.Append('\n'); // blank separator between defs
                string rendered = renderDefinition(modelOrder[i]);
                if (rendered == null) continue;
                if (indent.Length > 0) {
                    foreach (string ln in rendered.Split('\n')) {
                        if (ln.Length > 0) sb.Append(indent);
                        sb.Append(ln).Append('\n');
                    }
                } else {
                    sb.Append(rendered).Append('\n');
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
        public static string RenderRecipe(RecipeDef def) {
            StringBuilder sb = new StringBuilder();

            // Leading `#` comment block + optional `<var> = ` assignment
            // prefix. Mirrors PackLoader's extractAndAttachComment (which
            // captures consecutive comment lines immediately above the
            // recipe) and the assignment shape (recipe captured via
            // `<var> = build_recipe(...)` round-trips with the var prefix
            // preserved).
            appendStmtPrefix(sb, def);

            sb.Append("build_recipe(\n");

            // Capture once so every appendIdRef in this render reads the
            // same per-file variable map. Null when the def has no source
            // file context (newly-added recipes) — appendIdRef handles that.
            var vars = def.SourceFileVariables;

            sb.Append("    "); appendString(sb, def.RecipeId);   sb.Append(",\n");
            sb.Append("    "); appendString(sb, def.Name);       sb.Append(",\n");
            sb.Append("    "); appendString(sb, def.Description); sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.MachineId, vars);

            // Optional args, only emit when present.
            if (def.ResearchId != null) {
                sb.Append(",\n    research = "); appendIdRef(sb, def.ResearchId, vars);
            }
            if (def.DurationSeconds.HasValue) {
                sb.Append(",\n    duration = Duration.FromSec(").Append(def.DurationSeconds.Value).Append(")");
            }
            if (def.Ingredients != null && def.Ingredients.Count > 0) {
                sb.Append(",\n    ingredients = ");
                appendProductList(sb, def.Ingredients, vars);
            }
            if (def.Products != null && def.Products.Count > 0) {
                sb.Append(",\n    products = ");
                appendProductList(sb, def.Products, vars);
            }
            if (def.PowerPercent.HasValue) {
                sb.Append(",\n    power = ").Append(def.PowerPercent.Value);
            }

            sb.Append("\n)");
            return sb.ToString();
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
        private static string readLeadingWhitespace(string line) {
            if (string.IsNullOrEmpty(line)) return "";
            int i = 0;
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
            return i == 0 ? "" : line.Substring(0, i);
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
            List<DefBase> appended = defs.Where(d => d.SourceStartLine <= 0).ToList();
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
                string originalIndent = readLeadingWhitespace(output[startIdx]);
                string rendered = renderDefinition(def);
                if (rendered == null) continue; // unknown def kind with no raw source
                output.RemoveRange(startIdx, endIdx - startIdx + 1);
                string[] renderedLines = rendered.Split('\n');
                if (originalIndent.Length > 0) {
                    for (int i = 0; i < renderedLines.Length; i++) {
                        // Skip empty lines so we don't introduce trailing-WS
                        // noise on blank separators inside the rendered block.
                        if (renderedLines[i].Length > 0)
                            renderedLines[i] = originalIndent + renderedLines[i];
                    }
                }
                output.InsertRange(startIdx, renderedLines);
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
            if (def is ResearchDef rs)      return renderResearch(rs);
            if (def is UnlockRecipeDef ur)  return renderUnlockRecipe(ur);
            if (def is UnlockProductDef up) return renderUnlockProduct(up);
            if (def is UnlockMachineDef um) return renderUnlockMachine(um);
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
            if (def is EditMachinePortsDef ep) return renderEditMachinePorts(ep);
            if (def is BuildMachineDef bm)  return renderBuildMachine(bm);
            if (def is HousingDef hd)       return renderHousing(hd);
            if (def is SettlementDecorationDef sdd) return renderSettlementDecoration(sdd);
            if (def is SettlementFoodDef sfd)       return renderSettlementFood(sfd);
            if (def is SettlementIspDef sid)        return renderSettlementIsp(sid);
            if (def is HospitalDef hpd)             return renderHospital(hpd);
            if (def is MineTowerDef mtd)            return renderMineTower(mtd);
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
            sb.Append("    boxTypeId = "); appendIdRef(sb, def.BoxTypeId, vars); sb.Append(",\n");
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
            appendOptionalString(sb, "layout_str", value);
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
            sb.Append("    machineId            = "); appendIdRef(sb, def.MachineId, vars); sb.Append(",\n");
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
            sb.Append("    housingId       = "); appendIdRef(sb, def.HousingId, vars); sb.Append(",\n");
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
            sb.Append("    decorationId  = "); appendIdRef(sb, def.DecorationId, vars); sb.Append(",\n");
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
            sb.Append("    foodModuleId      = "); appendIdRef(sb, def.FoodModuleId, vars); sb.Append(",\n");
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
            sb.Append("    ispModuleId           = "); appendIdRef(sb, def.IspModuleId, vars); sb.Append(",\n");
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
            sb.Append("    hospitalId        = "); appendIdRef(sb, def.HospitalId, vars); sb.Append(",\n");
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
            sb.Append("    mineTowerId  = "); appendIdRef(sb, def.MineTowerId, vars); sb.Append(",\n");
            sb.Append("    source       = "); appendIdRef(sb, def.SourceId, vars);
            appendOptionalString(sb, "name",        def.Name);
            appendOptionalString(sb, "description", def.Description);
            emitLayoutStr(sb, def);
            appendOptionalIdRef (sb, "research",    def.ResearchId, vars);
            if (def.LockedOnInit.HasValue) sb.Append(",\n    lockedOnInit = ").Append(def.LockedOnInit.Value ? "True" : "False");
            sb.Append("\n)");
            return sb.ToString();
        }

        private static string renderResearchLab(ResearchLabDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("build_research_lab(\n");
            sb.Append("    researchLabId            = "); appendIdRef(sb, def.ResearchLabId, vars); sb.Append(",\n");
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
            sb.Append("    reactorId              = "); appendIdRef(sb, def.ReactorId, vars); sb.Append(",\n");
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
            sb.Append("[\n");
            for (int i = 0; i < items.Count; i++) {
                FuelPairRef p = items[i];
                sb.Append("        FuelPair(fuelIn=");
                appendIdRef(sb, p.FuelIn, vars);
                sb.Append(", spentFuelOut=");
                appendIdRef(sb, p.SpentFuelOut, vars);
                sb.Append(", durationSeconds=").Append(p.DurationSeconds);
                sb.Append(")");
                if (i < items.Count - 1) sb.Append(",");
                sb.Append("\n");
            }
            sb.Append("    ]");
        }

        // [Port(name="X", type="input", shape="IoPortShape_Pipe",
        //       position=(x, y, z), direction="+X")]
        // Position prefers the parsed X/Y/Z fields — emits as a literal
        // `(x, y, z)` tuple. Only falls back to PositionExpression when the
        // modder typed a non-tuple shape (typed-ref / Vector3i ctor / list
        // literal) that the loader couldn't reduce to integers.
        private static void appendPortList(StringBuilder sb, List<PortRef> items) {
            sb.Append("[\n");
            for (int i = 0; i < items.Count; i++) {
                PortRef p = items[i];
                sb.Append("        Port(name=");
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
                sb.Append("\n");
            }
            sb.Append("    ]");
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
            StringBuilder sb = new StringBuilder();
            appendStmtPrefix(sb, def);
            sb.Append("edit_recipe(\n");
            var vars = def.SourceFileVariables;
            sb.Append("    "); appendIdRef(sb, def.RecipeId, vars);
            if (def.DurationSeconds.HasValue) {
                sb.Append(",\n    duration = Duration.FromSec(")
                  .Append(def.DurationSeconds.Value).Append(")");
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
            }
            if (def.PowerPercent.HasValue) {
                sb.Append(",\n    power = ").Append(def.PowerPercent.Value);
            }
            sb.Append("\n)");
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
            sb.Append("    categoryId = "); appendIdRef(sb, def.CategoryId, vars); sb.Append(",\n");
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
            sb.Append("    id                  = "); appendIdRef(sb, def.GeneratorId, vars);       sb.Append(",\n");
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
            sb.Append("    productId = "); appendIdRef(sb, def.ProductId, vars); sb.Append(",\n");
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
            sb.Append("    productId = "); appendIdRef(sb, def.ProductId, vars); sb.Append(",\n");
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
            sb.Append("    productId = "); appendIdRef(sb, def.ProductId, vars); sb.Append(",\n");
            sb.Append("    name      = "); appendString(sb, def.Name);     sb.Append(",\n");
            sb.Append("    icon      = "); appendIconRef(sb, def.IconPath, def.SourceFileVariables); sb.Append(",\n");
            sb.Append("    prefab    = ").Append(def.PrefabExpression ?? "None");
            // maxTransport defaults to Quantity(3) — emit only when set.
            // We emit as a bare int; the runtime accepts both.
            appendOptionalInt   (sb, "maxTransport",                  def.MaxTransport);
            appendOptionalString(sb, "description",                   def.Description);
            appendOptionalBool  (sb, "isStorable",                    def.IsStorable);
            appendOptionalBool  (sb, "isWaste",                       def.IsWaste);
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
        private static void appendOptionalInt(StringBuilder sb, string name, int? value) {
            if (!value.HasValue) return;
            sb.Append(",\n    ").Append(name).Append(" = ").Append(value.Value);
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

        // add_unlock_recipe(research, machine, proto) — three positional ids.
        // Typed-ref vs string-literal heuristic is the same as build_recipe.
        private static string renderUnlockRecipe(UnlockRecipeDef def) {
            StringBuilder sb = new StringBuilder();
            var vars = def.SourceFileVariables;
            appendStmtPrefix(sb, def);
            sb.Append("add_unlock_recipe(\n");
            sb.Append("    "); appendIdRef(sb, def.ResearchId, vars); sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.MachineId, vars);  sb.Append(",\n");
            sb.Append("    "); appendIdRef(sb, def.RecipeId, vars);
            sb.Append("\n)");
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

        private static void appendProductList(StringBuilder sb, List<ProductRef> items,
                System.Collections.Generic.Dictionary<string, string> variables = null) {
            sb.Append("[\n");
            for (int i = 0; i < items.Count; i++) {
                ProductRef p = items[i];
                sb.Append("        Product(");
                appendIdRef(sb, p.ProductId, variables);
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
