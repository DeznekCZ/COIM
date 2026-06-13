using System.Collections.Generic;
using System.Text;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_toolbar_category</c> editor — category id / name / icon /
    /// parent (typed picker) / entities (structured list of entity ids,
    /// falls back to raw expression when the source doesn't parse cleanly).
    ///
    /// The parent field uses <see cref="ToolbarCategoryIdPicker"/> so a
    /// modder can nest under another pack-defined category (referenced by
    /// the in-file variable that bound it) without typing the id.
    ///
    /// The entities field is a list of <see cref="EntityIdPicker"/> rows
    /// — same in-file-variable resolution so a fresh BuildMachineDef in
    /// the same .py file shows up as a chip-tagged variable selection.
    /// When the loaded expression isn't a flat list of identifiers (e.g.
    /// the modder hand-authored a list comprehension), the editor falls
    /// back to a monospace multi-line text area so the source still
    /// round-trips.
    public sealed class ToolbarCategoryDefEditor : NamedDefEditor<ToolbarCategoryDef> {

        private readonly AssetPathPicker m_icon;
        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_model;

        // Parsed-out entity list, edited as rows beneath. Rebuilt against
        // value.EntitiesExpression every Value() swap; each mutation
        // immediately serialises back into the def's expression so the
        // emitter sees the latest state.
        private readonly Column m_entitiesHolder = new Column();
        private readonly List<string> m_entries = new List<string>();
        // When the loaded expression isn't a flat list literal we toggle
        // to a raw multi-line edit so the modder can still see + edit it.
        // Otherwise the structured list rows are the primary surface.
        private bool m_useRawMode;

        public ToolbarCategoryDefEditor(LoadedPack pack, PackModel model, ProtosDb protosDb) {
            m_model = model;
            m_protosDb = protosDb;

            AddIdField("categoryId");
            AddNameField();

            m_icon = AddField("icon (pack asset or Mafi typed-ref)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.IconPath,
                    setPath: v => { if (value != null) value.IconPath = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: true,
                    title: new LocStrFormatted("Pick toolbar icon"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, CustomAssets.Editor.AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_icon.RefreshDisplay());

            // Parent picker — pack categories (chipped by variable when
            // bound in the same file) PLUS every vanilla / mod-shipped
            // ToolbarCategoryProto. allowNone because a top-level
            // category genuinely has no parent.
            Column parentHolder = new Column();
            parentHolder.AlignItemsStretch();
            AddField("parent (existing or pack-defined category)",
                parentHolder, onRefresh: () => {
                    parentHolder.Clear();
                    if (value == null) return;
                    parentHolder.Add(new ToolbarCategoryIdPicker(
                        m_model, m_protosDb,
                        getId: () => value?.ParentId,
                        setId: id => { if (value != null) value.ParentId = string.IsNullOrEmpty(id) ? null : id; },
                        getOwnerDef: () => value,
                        title: new LocStrFormatted("Pick parent category"),
                        allowNone: true));
                });

            AddField("entities (buildings shown under this category)",
                m_entitiesHolder, onRefresh: rebuildEntities);
        }

        // ---- Entities list editor ------------------------------------------

        private void rebuildEntities() {
            m_entitiesHolder.Clear();
            m_entries.Clear();
            if (value == null) return;

            // Try to parse the stored expression as a flat list literal.
            // Anything fancier (comprehensions, function calls returning a
            // list, ternaries) goes through the raw fallback.
            string expr = value.EntitiesExpression;
            m_useRawMode = !tryParseFlatList(expr, m_entries);

            if (m_useRawMode) {
                renderRawMode();
            } else {
                renderListMode();
            }
        }

        private void renderRawMode() {
            // Same monospace textarea the editor had before — so a
            // hand-authored Python expression stays editable verbatim.
            // The "switch to list" link only appears when the current
            // expression is empty or `[]`, so the modder can opt back
            // into the structured editor without losing real content.
            TextField raw = new TextField()
                .Multiline(true)
                .Class(Cls.fontMonospace)
                .SetTextAreaMinHeight(60.px())
                .Text(value.EntitiesExpression ?? "[]")
                .OnValueChanged(v => {
                    if (value != null) value.EntitiesExpression = string.IsNullOrEmpty(v) ? "[]" : v;
                });
            m_entitiesHolder.Add(raw);
            m_entitiesHolder.Add(new Label(new LocStrFormatted(
                "(raw expression — the source isn't a flat list literal)"))
                .TinyFontSize().Color(ColorRgba.LightGray));
            string current = (value.EntitiesExpression ?? "").Trim();
            if (current.Length == 0 || current == "[]") {
                m_entitiesHolder.Add(new ButtonText(
                    new LocStrFormatted("Switch to list editor"),
                    () => {
                        value.EntitiesExpression = "[]";
                        rebuildEntities();
                    }));
            }
        }

        private void renderListMode() {
            for (int i = 0; i < m_entries.Count; i++) {
                int captured = i;
                Row row = new Row().Gap(2.pt()).AlignItemsCenter();
                EntityIdPicker picker = new EntityIdPicker(
                    m_model, m_protosDb,
                    getId: () => captured < m_entries.Count ? m_entries[captured] : null,
                    setId: id => {
                        if (captured >= m_entries.Count) return;
                        m_entries[captured] = id ?? "";
                        commitEntries();
                    },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick entity for category"));
                picker.FlexGrow(1f);
                row.Add(picker);
                row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                    if (captured < m_entries.Count) {
                        m_entries.RemoveAt(captured);
                        commitEntries();
                        rebuildEntities();
                    }
                }));
                m_entitiesHolder.Add(row);
            }
            if (m_entries.Count == 0) {
                m_entitiesHolder.Add(new Label(new LocStrFormatted("  (no entities yet)"))
                    .Color(ColorRgba.LightGray));
            }
            m_entitiesHolder.Add(new ButtonText(
                new LocStrFormatted("+ add entity"),
                () => {
                    m_entries.Add("");
                    commitEntries();
                    rebuildEntities();
                }));
            m_entitiesHolder.Add(new ButtonText(
                new LocStrFormatted("Edit as raw expression"),
                () => {
                    // Force-switch to raw mode by committing the current
                    // entries as a list expression and re-rendering as
                    // raw — useful when the modder wants to hand-craft
                    // a comprehension or include a typed-ref pattern
                    // the list editor doesn't model.
                    commitEntries();
                    m_useRawMode = true;
                    m_entitiesHolder.Clear();
                    renderRawMode();
                }).Tooltip(new LocStrFormatted(
                    "Switch this category's entities to a raw Python expression you can hand-edit.")));
        }

        // Serialize m_entries into the def's EntitiesExpression in the
        // canonical "[a, b, c]" shape. Entries that are blank after
        // trimming are dropped — a "+ add entity" click with no follow-up
        // selection mustn't produce `[, ]` in the output file.
        private void commitEntries() {
            if (value == null) return;
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            bool first = true;
            foreach (string raw in m_entries) {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string entry = raw.Trim();
                if (!first) sb.Append(", ");
                sb.Append(entry);
                first = false;
            }
            sb.Append(']');
            value.EntitiesExpression = sb.ToString();
            value.Dirty = true;
        }

        // ---- Flat-list parser ----------------------------------------------

        // Parse expressions shaped like `[entry, entry, entry]` into the
        // individual entries (each verbatim — typed-refs, string literals,
        // bare identifiers all stay as-written). Returns false when the
        // input isn't a flat list literal, in which case the caller falls
        // back to raw-text mode.
        //
        // Bracket / paren / brace depth tracking lets us skip commas
        // INSIDE nested constructors like `Ids.Buildings.X` (no nesting),
        // `SomeCall(a, b)`, or `[nested, list]`. We don't model strings
        // beyond noticing them — but Python identifiers / typed-refs /
        // bare strings inside the list typically don't carry commas, so
        // this is enough for the cases the modder actually writes.
        private static bool tryParseFlatList(string expr, List<string> sink) {
            if (string.IsNullOrWhiteSpace(expr)) {
                // Empty / missing → empty list, structured mode works.
                return true;
            }
            string trimmed = expr.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '[' || trimmed[trimmed.Length - 1] != ']') {
                return false;
            }
            string body = trimmed.Substring(1, trimmed.Length - 2);
            int depth = 0;
            int start = 0;
            bool inString = false;
            char stringQuote = '\0';
            for (int i = 0; i < body.Length; i++) {
                char c = body[i];
                if (inString) {
                    if (c == '\\' && i + 1 < body.Length) { i++; continue; }
                    if (c == stringQuote) inString = false;
                    continue;
                }
                if (c == '"' || c == '\'') { inString = true; stringQuote = c; continue; }
                if (c == '(' || c == '[' || c == '{') { depth++; continue; }
                if (c == ')' || c == ']' || c == '}') {
                    if (depth == 0) return false; // unbalanced — bail
                    depth--;
                    continue;
                }
                if (c == ',' && depth == 0) {
                    string entry = body.Substring(start, i - start).Trim();
                    if (entry.Length > 0) sink.Add(entry);
                    start = i + 1;
                }
            }
            if (depth != 0 || inString) return false;
            string tail = body.Substring(start).Trim();
            if (tail.Length > 0) sink.Add(tail);
            return true;
        }
    }
}
