using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CustomAssets.Data.Mod;
using Mafi;
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
    ///   * the Python load order declared in <c>Definitions/__init__.py</c>'s
    ///     top-level <c>dependencies(...)</c> call
    /// …with TextField rows the modder can edit, ▲▼ reorder, remove, and add
    /// new entries to. A Save button writes each section back to disk:
    ///   - manifest.json: round-trip via <see cref="MiniJson.Parse"/> +
    ///     <see cref="MiniJsonWriter.Write"/> so unrelated fields are preserved.
    ///   - __init__.py: splice the existing <c>dependencies(...)</c> call
    ///     range out and write a new one with the current load-order list.
    ///     If no __init__.py exists, one is created with just the call.
    ///
    /// State lives on the dialog instance (a class, not the static Open()
    /// entry from before) so edits persist between rebuilds while the dialog
    /// is open. Cancel = close without saving.
    /// </summary>
    public sealed class PackDepsDialog {

        private readonly LoadedPack m_pack;
        private readonly FloatingColumn m_dialog;
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
            m_dialog = new FloatingColumn(
                FloaterPositionPolicy.ABOVE,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: false);   // false: Save/Cancel buttons own dismissal
            // FloatingColumn doesn't carry a visible background by default —
            // editable content rendered transparently sits visually on top of
            // the tree behind it and reads as a UI bug. Cls.panelBg gives it
            // the standard COI panel chrome (recessed dark surface) so the
            // dialog looks like a real modal popup.
            m_dialog.Class(Cls.panelBg)
                    .Padding(4.pt()).Gap(3.pt()).MinWidth(440.px()).MaxHeight(640.px());

            loadCurrentState();
            m_status = new Label(new LocStrFormatted("")).TinyFontSize();

            m_dialog.Add(new Label(new LocStrFormatted(
                "Pack dependencies — " + (pack?.ModId ?? "?"))).FontBold());

            m_dialog.Add(new Label(new LocStrFormatted("Mod dependencies (manifest.json)")));
            m_dialog.Add(m_mandatoryCol);
            m_dialog.Add(new ButtonText(new LocStrFormatted("+ add mod dependency"),
                () => { m_mandatory.Add(""); refreshList(m_mandatoryCol, m_mandatory); }));

            m_dialog.Add(new Label(new LocStrFormatted("Optional mod dependencies")));
            m_dialog.Add(m_optionalCol);
            m_dialog.Add(new ButtonText(new LocStrFormatted("+ add optional dependency"),
                () => { m_optional.Add(""); refreshList(m_optionalCol, m_optional); }));

            m_dialog.Add(new Label(new LocStrFormatted(
                "Python load order (Definitions/__init__.py)")));
            m_dialog.Add(m_loadOrderCol);
            m_dialog.Add(new ButtonText(new LocStrFormatted("+ add load entry"),
                () => { m_loadOrder.Add(""); refreshList(m_loadOrderCol, m_loadOrder); }));

            // Footer: status + Save/Cancel. We use closeOnClickOutside=false on
            // the FloatingColumn so a stray click can't drop unsaved changes
            // unexpectedly — the modder must explicitly Cancel or Save.
            m_dialog.Add(m_status);
            Row footer = new Row {
                new ButtonText(new LocStrFormatted("Save"), onSave),
                new ButtonText(new LocStrFormatted("Cancel"), m_dialog.Close)
            };
            footer.Gap(3.pt());
            m_dialog.Add(footer);

            refreshList(m_mandatoryCol, m_mandatory);
            refreshList(m_optionalCol, m_optional);
            refreshList(m_loadOrderCol, m_loadOrder);
        }

        public void OpenAt(UiComponent anchor) {
            m_dialog.Open(anchor);
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

            // Python load order — read from the cached __init__.py AST.
            foreach (LoadedFile file in m_pack.Files) {
                if (Path.GetFileName(file.AbsolutePath) != "__init__.py") continue;
                if (file.Ast == null) continue;
                foreach (IStatement stmt in file.Ast.statements) {
                    if (!(stmt is EvaluateStatement ev)) continue;
                    if (!(ev.Expression is CallExpression call)) continue;
                    if (!(call.Calle is VariableExpression v) || v.Path != "dependencies") continue;
                    foreach (IArgument arg in call.Arguments) {
                        if (arg is OrderedArgument && arg.Expression is StringConstant s) {
                            m_loadOrder.Add(s.Value);
                        }
                    }
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
        private void refreshList(Column container, List<string> list) {
            container.Clear();
            for (int i = 0; i < list.Count; i++) {
                int captured = i;
                Row row = new Row().Gap(2.pt()).AlignItemsCenter();
                TextField tf = new TextField()
                    .Text(list[captured] ?? "")
                    .OnValueChanged(v => list[captured] = v ?? "");
                tf.FlexGrow(1f);
                row.Add(tf);

                if (i > 0) row.Add(new ButtonText(new LocStrFormatted("▲"), () => {
                    string tmp = list[captured - 1];
                    list[captured - 1] = list[captured];
                    list[captured] = tmp;
                    refreshList(container, list);
                }));
                if (i < list.Count - 1) row.Add(new ButtonText(new LocStrFormatted("▼"), () => {
                    string tmp = list[captured + 1];
                    list[captured + 1] = list[captured];
                    list[captured] = tmp;
                    refreshList(container, list);
                }));
                row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                    list.RemoveAt(captured);
                    refreshList(container, list);
                }));
                container.Add(row);
            }
            if (list.Count == 0) {
                container.Add(new Label(new LocStrFormatted("  (none)")).Color(ColorRgba.LightGray));
            }
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

        // Rewrite Definitions/__init__.py's dependencies(...) call to match the
        // dialog's current load-order list. Two cases:
        //   1. __init__.py exists with a dependencies(...) call → splice it.
        //   2. __init__.py missing OR no call inside it → write a fresh file
        //      (or prepend the call to the existing one).
        private void saveLoadOrder() {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return;
            string definitionsDir = Path.Combine(m_pack.RootPath, "Definitions");
            if (!Directory.Exists(definitionsDir)) {
                throw new InvalidOperationException(
                    "Definitions/ folder not found at " + definitionsDir);
            }
            string initPath = Path.Combine(definitionsDir, "__init__.py");

            string newCallBlock = renderDependenciesCall(m_loadOrder);

            if (!File.Exists(initPath)) {
                // No init file at all → create one with just the deps call.
                File.WriteAllText(initPath, newCallBlock + "\n",
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                return;
            }

            // Try to find the existing call's line range in the cached AST and
            // splice it. Fall back to prepending the call when the AST doesn't
            // carry one (init file exists but has no dependencies() yet).
            int startLine = 0, endLine = 0;
            bool foundCall = false;
            foreach (LoadedFile file in m_pack.Files) {
                if (Path.GetFileName(file.AbsolutePath) != "__init__.py") continue;
                if (file.Ast == null) continue;
                foreach (IStatement stmt in file.Ast.statements) {
                    if (!(stmt is EvaluateStatement ev)) continue;
                    if (!(ev.Expression is CallExpression call)) continue;
                    if (!(call.Calle is VariableExpression v) || v.Path != "dependencies") continue;
                    startLine = ev.StartLine;
                    endLine   = ev.EndLine;
                    foundCall = true;
                    break;
                }
                if (foundCall) break;
            }

            string[] lines = File.ReadAllLines(initPath, Encoding.UTF8);
            List<string> output = new List<string>(lines);
            if (foundCall && startLine > 0 && endLine >= startLine && endLine <= output.Count) {
                int startIdx = startLine - 1;
                output.RemoveRange(startIdx, endLine - startIdx);
                output.InsertRange(startIdx, newCallBlock.Split('\n'));
            } else {
                output.Insert(0, newCallBlock);
            }
            File.WriteAllLines(initPath, output,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        // Render a single-line dependencies("a", "b", "c") call. Stays on one
        // line because that's how the load-order convention is usually
        // authored — and one line keeps the splice range small.
        private static string renderDependenciesCall(List<string> names) {
            StringBuilder sb = new StringBuilder();
            sb.Append("dependencies(");
            for (int i = 0; i < names.Count; i++) {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(escapeStringLiteral(names[i])).Append('"');
            }
            sb.Append(')');
            return sb.ToString();
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
