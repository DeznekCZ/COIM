using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Core.Mods;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using PythonAPI;
using PythonAPI.Arguments;
using PythonAPI.Expressions;
using PythonAPI.Statements;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Editable pack-dependencies dialog. Surfaces both:
    ///   * manifest.json's <c>mod_dependencies</c> + <c>optional_mod_dependencies</c>
    ///   * the Python load order declared in <c>Definitions/__init__.py</c>
    ///     via either <c>import &lt;name&gt;</c> lines (the current convention)
    ///     or a legacy <c>dependencies(...)</c> call (read for backward compat)
    /// …with TextField rows the modder can edit, ▲▼ reorder, remove, and add
    /// new entries to. A Save button writes each section back to disk:
    ///   - manifest.json: round-trip via <see cref="MiniJson.Parse"/> +
    ///     <see cref="MiniJsonWriter.Write"/> so unrelated fields are preserved.
    ///   - __init__.py: one <c>import &lt;name&gt;</c> per line in the dialog's
    ///     order. Any legacy <c>dependencies(...)</c> call gets spliced out so
    ///     the file ends up in the canonical shape after a save. Files in the
    ///     dialog's list that are missing from disk are still written (they
    ///     might be added later); files on disk that aren't in the list are
    ///     left to the modder's discretion — the dialog is the source of truth.
    ///
    /// State lives on the dialog instance (a class, not the static Open()
    /// entry from before) so edits persist between rebuilds while the dialog
    /// is open. Cancel = close without saving.
    /// </summary>
    public sealed class PackDepsDialog {

        private readonly LoadedPack m_pack;
		private readonly FloatingColumn m_holder;
        private readonly Panel m_dialog;
        private readonly List<string> m_mandatory = new List<string>();
        private readonly List<string> m_optional = new List<string>();
        private readonly List<string> m_loadOrder = new List<string>();

        // Columns we clear+repopulate when rows are added/removed/reordered.
        // Keeping them as instance fields lets each list section refresh in
        // place without rebuilding the whole dialog (which would close it).
        private readonly Column m_mandatoryCol = new Column();
        private readonly Column m_optionalCol = new Column();
        private readonly Column m_loadOrderCol = new Column();
        private readonly Label m_status;

		/// <summary>Convenience entry point matching the earlier static Open()
        /// signature so callers don't have to track instances themselves.</summary>
        public static void Open(LoadedPack pack, UiComponent anchor) {
            new PackDepsDialog(pack).OpenAt(anchor);
        }

        public PackDepsDialog(LoadedPack pack) {
            m_pack = pack;
            m_holder = new FloatingColumn(
                FloaterPositionPolicy.ABOVE,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: false);   // false: Save/Cancel buttons own dismissal
            // FloatingColumn doesn't carry a visible background or border by
            // default — editable content rendered transparently sits visually
            // on top of the tree behind it and reads as a UI bug. Cls.panel
            // ties bg + border + bolts into one chrome match for all our
            // popups (proto picker, pack picker, this dialog).
            // AlignItemsStretch makes each section (label, editable lists,
            // action row) span the full popup width instead of hugging its
            // content.
			m_dialog = m_holder.AddAndReturn(new Panel())
                    .AlignItemsStretch()
                    .Padding(4.pt()).Gap(3.pt()).MinWidth(440.px()).MaxHeight(640.px());

            loadCurrentState();
            m_status = new Label(new LocStrFormatted("")).TinyFontSize();

            m_dialog.Add(new Label(new LocStrFormatted(
                "Pack dependencies — " + (pack?.ModId ?? "?"))).FontBold());

            m_dialog.Add(new Label(new LocStrFormatted("Mod dependencies (manifest.json)")));
            m_dialog.Add(m_mandatoryCol);
            m_dialog.Add(new ButtonText(new LocStrFormatted("+ add mod dependency"),
                () => { m_mandatory.Add(""); refreshList(m_mandatoryCol, m_mandatory, showModPicker: true); }));

            m_dialog.Add(new Label(new LocStrFormatted("Optional mod dependencies")));
            m_dialog.Add(m_optionalCol);
            m_dialog.Add(new ButtonText(new LocStrFormatted("+ add optional dependency"),
                () => { m_optional.Add(""); refreshList(m_optionalCol, m_optional, showModPicker: true); }));

            m_dialog.Add(new Label(new LocStrFormatted(
                "Python load order (Definitions/__init__.py)")));
            m_dialog.Add(m_loadOrderCol);
            m_dialog.Add(new ButtonText(new LocStrFormatted("+ add load entry"),
                () => { m_loadOrder.Add(""); refreshList(m_loadOrderCol, m_loadOrder, showModPicker: false); }));

            // Footer: status + Save/Cancel. We use closeOnClickOutside=false on
            // the FloatingColumn so a stray click can't drop unsaved changes
            // unexpectedly — the modder must explicitly Cancel or Save.
            m_dialog.Add(m_status);
            Row footer = new Row {
                new ButtonText(new LocStrFormatted("Save"), onSave),
                new ButtonText(new LocStrFormatted("Cancel"), m_holder.Close)
            };
            footer.Gap(3.pt());
            m_dialog.Add(footer);

            refreshList(m_mandatoryCol, m_mandatory, showModPicker: true);
            refreshList(m_optionalCol, m_optional, showModPicker: true);
            refreshList(m_loadOrderCol, m_loadOrder, showModPicker: false);
        }

        public void OpenAt(UiComponent anchor) {
			m_holder.Open(anchor);
        }

        // ---- Initial state ---------------------------------------------------

        private void loadCurrentState() {
            if (m_pack == null) return;
            // Manifest.json — same two-array surface as the read-only version.
            string manifestPath = Path.Combine(m_pack.RootPath ?? "", "manifest.json");
            if (File.Exists(manifestPath)) {
                try {
                    object root = MiniJson.Parse(File.ReadAllText(manifestPath));
                    if (root is Dictionary<string, object> dict) {
                        appendStringsFromArray(dict, "mod_dependencies",          m_mandatory);
                        appendStringsFromArray(dict, "optional_mod_dependencies", m_optional);
                    }
                } catch (Exception ex) {
                    Log.Warning("PackDepsDialog: manifest read failed — " + ex.Message);
                }
            }

            // Python load order — read from the cached __init__.py AST. Two
            // shapes are supported on input so packs migrating from the
            // legacy convention can still be edited without first running
            // LegacyMigrator:
            //   • `import <name>` lines — each adds <name> to the order.
            //   • a single `dependencies("a.py", "b.py", …)` call — each
            //     positional string-literal arg goes in.
            //
            // Every name is normalized through stripPyExtension before being
            // tracked, so `dependencies("a.py")` and `import a` (and the
            // sibling-file scan that produces "a.py") all collapse to a
            // single "a" entry. Without normalization a pack with both an
            // `import a` line and a `dependencies("a.py", …)` call would
            // show two rows for the same file in the dialog — the bug
            // reported when migration ran on a pre-migrated pack.
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LoadedFile file in m_pack.Files) {
                if (Path.GetFileName(file.AbsolutePath) != "__init__.py") continue;
                if (file.Ast == null) continue;
                foreach (IStatement stmt in file.Ast.statements) {
                    if (stmt is LocalImportStatement imp) {
                        string name = imp.moduleName;
                        if (string.IsNullOrEmpty(name)) continue;
                        string norm = stripPyExtension(name);
                        if (seen.Add(norm)) m_loadOrder.Add(norm);
                        continue;
                    }
                    if (stmt is EvaluateStatement ev
                            && ev.Expression is CallExpression call
                            && call.Calle is VariableExpression v
                            && v.Path == "dependencies") {
                        foreach (IArgument arg in call.Arguments) {
                            if (arg is OrderedArgument && arg.Expression is StringConstant s) {
                                string norm = stripPyExtension(s.Value);
                                if (seen.Add(norm)) m_loadOrder.Add(norm);
                            }
                        }
                    }
                }
            }

            // Auto-include every sibling .py file that isn't yet listed.
            // Without this, files that the modder created on disk but never
            // mentioned in dependencies() / imports would silently drop out of
            // the load order — bug the user hit when consolidating.
            string defsDir = Path.Combine(m_pack.RootPath ?? "", "Definitions");
            if (Directory.Exists(defsDir)) {
                try {
                    List<string> extras = new List<string>();
                    foreach (string p in Directory.EnumerateFiles(defsDir, "*.py",
                            SearchOption.TopDirectoryOnly)) {
                        string fname = Path.GetFileName(p);
                        if (string.Equals(fname, "__init__.py", StringComparison.OrdinalIgnoreCase)) continue;
                        string norm = stripPyExtension(fname);
                        if (seen.Add(norm)) extras.Add(norm);
                    }
                    extras.Sort(StringComparer.OrdinalIgnoreCase);
                    m_loadOrder.AddRange(extras);
                } catch (Exception ex) {
                    Log.Warning("PackDepsDialog: failed to enumerate '"
                                + defsDir + "' — " + ex.Message);
                }
            }
        }

        private static void appendStringsFromArray(
                Dictionary<string, object> root, string key, List<string> sink) {
            if (!root.TryGetValue(key, out object raw)) return;
            if (!(raw is List<object> list)) return;
            foreach (object item in list) {
                if (item is string s) sink.Add(s);
            }
        }

        // ---- Row rendering ---------------------------------------------------

        // Re-render an editable list section in place. Each row gets a TextField
        // bound to the underlying List<string> by index, plus ▲▼✕ buttons.
        // The whole section is rebuilt on every mutation so the index-bound
        // closures stay correct after add/remove/reorder.
        private void refreshList(Column container, List<string> list, bool showModPicker) {
            container.Clear();
            for (int i = 0; i < list.Count; i++) {
                int captured = i;
                Row row = new Row().Gap(2.pt()).AlignItemsCenter();
                TextField tf = new TextField()
                    .Text(list[captured] ?? "")
                    .OnValueChanged(v => list[captured] = v ?? "");
                tf.FlexGrow(1f);
                row.Add(tf);

                // Loaded-mods picker for mod-dependency rows. Mafi's
                // ModsLoader holds every loaded mod with its id + version,
                // so we can offer them as a one-click fill instead of the
                // modder hand-typing `Mafi.Base>=0.8.4`. Load-order rows
                // skip this — those reference Python module names inside
                // the pack, not external mods.
                if (showModPicker) {
                    ButtonText pickBtn = null;
                    pickBtn = new ButtonText(new LocStrFormatted("📋"), () => {
                        openLoadedModPicker(pickBtn, picked => {
                            tf.Text(picked);
                            list[captured] = picked;
                        });
                    });
                    pickBtn.Tooltip(new LocStrFormatted("Pick from currently loaded mods"));
                    row.Add(pickBtn);
                }

                if (i > 0) row.Add(new ButtonText(new LocStrFormatted("▲"), () => {
                    string tmp = list[captured - 1];
                    list[captured - 1] = list[captured];
                    list[captured] = tmp;
                    refreshList(container, list, showModPicker);
                }));
                if (i < list.Count - 1) row.Add(new ButtonText(new LocStrFormatted("▼"), () => {
                    string tmp = list[captured + 1];
                    list[captured + 1] = list[captured];
                    list[captured] = tmp;
                    refreshList(container, list, showModPicker);
                }));
                row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                    list.RemoveAt(captured);
                    refreshList(container, list, showModPicker);
                }));
                container.Add(row);
            }
            if (list.Count == 0) {
                container.Add(new Label(new LocStrFormatted("  (none)")).Color(ColorRgba.LightGray));
            }
        }

        // Delegate to the shared LoadedModPicker so PackDepsDialog and
        // NewPackDialog share one picker implementation. The picker is
        // searchable + scrollable; selection writes the canonical
        // `<id>>=<version>` spec into the row's TextField.
        private void openLoadedModPicker(UiComponent anchor, Action<string> onPicked) {
            LoadedModPicker.OpenAnchored(anchor, onPicked);
        }

        // ---- Save -----------------------------------------------------------

        private void onSave() {
            try {
                trimEmpties(m_mandatory);
                trimEmpties(m_optional);
                trimEmpties(m_loadOrder);
                saveManifest();
                saveLoadOrder();
                m_status.Value(new LocStrFormatted("Saved."));
            } catch (Exception ex) {
                Log.Exception(ex);
                m_status.Value(new LocStrFormatted("Save failed: " + ex.Message));
            }
        }

        // Strip out empty entries the modder may have left around when they
        // clicked "+ add" but never filled the field. Saving them as empty
        // string literals would produce a malformed manifest / a load-order
        // entry that loads nothing.
        private static void trimEmpties(List<string> list) {
            for (int i = list.Count - 1; i >= 0; i--) {
                if (string.IsNullOrWhiteSpace(list[i])) list.RemoveAt(i);
                else list[i] = list[i].Trim();
            }
        }

        // Round-trip manifest.json through MiniJson — preserves any keys we
        // don't surface in the dialog (display_name, description, etc.). When
        // the file doesn't exist we don't create one; manifests are authored
        // separately and creating from scratch needs more than just two arrays.
        private void saveManifest() {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return;
            string manifestPath = Path.Combine(m_pack.RootPath, "manifest.json");
            if (!File.Exists(manifestPath)) {
                throw new InvalidOperationException(
                    "manifest.json not found at " + manifestPath);
            }
            object root = MiniJson.Parse(File.ReadAllText(manifestPath));
            if (!(root is Dictionary<string, object> dict)) {
                throw new InvalidOperationException(
                    "manifest.json root is not a JSON object");
            }
            dict["mod_dependencies"]          = toStringList(m_mandatory);
            dict["optional_mod_dependencies"] = toStringList(m_optional);
            File.WriteAllText(manifestPath, MiniJsonWriter.Write(dict),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static List<object> toStringList(List<string> src) {
            List<object> sink = new List<object>(src.Count);
            foreach (string s in src) sink.Add(s);
            return sink;
        }

        // Rewrite Definitions/__init__.py to match the dialog's load-order
        // list using one `import <name>` per line. Strategy:
        //   1. Read the existing file (if any).
        //   2. Sweep the lines, identifying every load-order line that we
        //      will replace: top-level `import …` lines AND every line that
        //      belongs to a top-level `dependencies(…)` call (including
        //      its continuations across multiple lines).
        //   3. Remove those lines and insert the fresh imports block at the
        //      position of the first removed line — preserves leading file
        //      comments / module docstring and any other non-load-order code
        //      below.
        // We do this via text patterns rather than the cached AST because
        // <see cref="LocalImportStatement"/> doesn't currently carry source-
        // line metadata, and the text shapes ("starts with `import ` after
        // optional leading whitespace") are unambiguous at the top level.
        private void saveLoadOrder() {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return;
            string definitionsDir = Path.Combine(m_pack.RootPath, "Definitions");
            if (!Directory.Exists(definitionsDir)) {
                throw new InvalidOperationException(
                    "Definitions/ folder not found at " + definitionsDir);
            }
            string initPath = Path.Combine(definitionsDir, "__init__.py");

            string importsBlock = renderImports(m_loadOrder);

            if (!File.Exists(initPath)) {
                File.WriteAllText(initPath, importsBlock,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                return;
            }

            string[] lines = File.ReadAllLines(initPath, Encoding.UTF8);
            HashSet<int> toRemove = new HashSet<int>();
            int firstRemovedLine = -1;
            bool inDepsCall = false;

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                string trimmed = line.TrimStart();
                if (inDepsCall) {
                    toRemove.Add(i);
                    // Closing paren ends the call. Naive — assumes the `)` we
                    // see is the closing one, which holds for the legacy
                    // shape `dependencies("a", "b")` and its multi-line
                    // variants. Anything fancier (nested calls inside
                    // dependencies()) wasn't supported by the legacy framework
                    // either so we don't need to handle it here.
                    if (line.Contains(")")) inDepsCall = false;
                    continue;
                }
                if (trimmed.StartsWith("import ", StringComparison.Ordinal)) {
                    if (firstRemovedLine < 0) firstRemovedLine = i;
                    toRemove.Add(i);
                    continue;
                }
                if (trimmed.StartsWith("dependencies(", StringComparison.Ordinal)) {
                    if (firstRemovedLine < 0) firstRemovedLine = i;
                    toRemove.Add(i);
                    // Multi-line if the open paren isn't closed on the same line.
                    if (!line.Contains(")")) inDepsCall = true;
                    continue;
                }
            }

            List<string> output = new List<string>(lines.Length + 8);
            int insertAt = firstRemovedLine >= 0 ? -1 : 0;
            bool inserted = false;
            for (int i = 0; i < lines.Length; i++) {
                if (toRemove.Contains(i)) {
                    if (!inserted && firstRemovedLine == i) {
                        // Inject the fresh imports at the first removal site
                        // so they take the place of the prior load-order
                        // block in document order.
                        output.AddRange(importsBlock.Split(new[] { '\n' },
                            StringSplitOptions.None));
                        // The split emits a trailing empty string because
                        // importsBlock ends in '\n'; trim it to avoid an
                        // accidental blank line between the imports and the
                        // next preserved line.
                        if (output.Count > 0
                                && string.IsNullOrEmpty(output[output.Count - 1])) {
                            output.RemoveAt(output.Count - 1);
                        }
                        inserted = true;
                    }
                    continue;
                }
                output.Add(lines[i]);
            }
            if (!inserted) {
                // Init file had no load-order lines at all — drop the imports
                // at the top so they run before any other code.
                output.InsertRange(0,
                    importsBlock.Split(new[] { '\n' }, StringSplitOptions.None));
                if (output.Count > 0
                        && string.IsNullOrEmpty(output[output.Count - 1])) {
                    output.RemoveAt(output.Count - 1);
                }
            }
            File.WriteAllLines(initPath, output,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        // Each name in m_loadOrder still carries its `.py` suffix from the
        // legacy convention. Strip it so the generated `import <name>` lines
        // are valid Python (modules don't use the extension).
        private static string renderImports(List<string> names) {
            StringBuilder sb = new StringBuilder();
            foreach (string name in names) {
                if (string.IsNullOrWhiteSpace(name)) continue;
                sb.Append("import ").Append(stripPyExtension(name)).Append('\n');
            }
            return sb.ToString();
        }

        private static string stripPyExtension(string name) {
            if (string.IsNullOrEmpty(name)) return name;
            if (name.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                return name.Substring(0, name.Length - 3);
            }
            return name;
        }

        private static string escapeStringLiteral(string value) {
            if (value == null) return "";
            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value) {
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n");  break;
                    default:   sb.Append(c);       break;
                }
            }
            return sb.ToString();
        }
    }
}
