using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Io;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Translations editor for a pack, rendered into the editor's main pane by the
    /// TT button on the pack card — the same area a definition's form uses.
    ///
    /// Workflow:
    ///   1. Modder enters a language code (e.g. "cs", "de", "fr").
    ///   2. Clicks Scan — pulls every translatable string from the pack:
    ///      the mod's own display name and descriptions (manifest.json), its
    ///      config field descriptions (config.json), and every definition's
    ///      name + description — then loads any existing
    ///      <c>&lt;pack&gt;/Translations/&lt;langCode&gt;.json</c>.
    ///   3. Edits per-row translations.
    ///   4. Clicks Save — writes the flat-dict JSON to disk. Each language
    ///      is its own file, so updating one doesn't disturb others.
    ///
    /// The runtime side (loading the JSON at pack-load time) is a separate
    /// follow-up — this window is the authoring side.
    /// </summary>
    public sealed class TranslationsPanel : Column {

        private readonly LoadedPack m_pack;
        private readonly PackModel m_model;
        private readonly TextField m_langField;
        private readonly Column m_rowsColumn;
        private readonly Label m_status;
        private readonly Dictionary<string, string> m_translations =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<KeyValuePair<string, string>> m_strings =
            new List<KeyValuePair<string, string>>();

        public TranslationsPanel(LoadedPack pack, PackModel model) {
            m_pack = pack;
            m_model = model;

            // No window chrome of its own: the editor renders this into its main pane,
            // beside the tree, the same way it renders a definition's form.
            this.AlignItemsStretch().Gap(3.pt());
            Add(new Label(new LocStrFormatted(
                "Translations — " + (pack?.ModId ?? "?"))).FontBold());

            // Language code + Scan row. Scan rebuilds the list from the
            // current model and merges any saved translations for the
            // chosen language so unfinished work resumes cleanly.
            m_langField = new TextField()
                .Placeholder(new LocStrFormatted("e.g. cs, de, fr…"))
                .Width(120.px());
            Row langRow = new Row {
                new Label(new LocStrFormatted("Language code:")),
                m_langField,
                new ButtonText(new LocStrFormatted("Scan pack strings"), onScan)
            };
            langRow.Gap(3.pt()).AlignItemsCenter();
            Add(langRow);

            m_status = new Label(new LocStrFormatted(
                "Enter a language code and click 'Scan pack strings' to load entries."));
            m_status.TinyFontSize();
            Add(m_status);

            // No ScrollColumn of its own — the editor pane this sits in already scrolls,
            // and a nested scroll inside one collapses to zero height.
            m_rowsColumn = new Column();
            m_rowsColumn.Gap(1.pt()).AlignItemsStretch();
            Add(m_rowsColumn);

            Row footer = new Row {
                new ButtonText(new LocStrFormatted("Save translations"), onSave)
            };
            footer.Gap(3.pt());
            Add(footer);
        }

        // ---- Scan ------------------------------------------------------

        private void onScan() {
            string lang = (m_langField.GetText() ?? "").Trim();
            if (string.IsNullOrEmpty(lang)) {
                m_status.Value(new LocStrFormatted("Enter a language code first."));
                return;
            }

            m_strings.Clear();
            m_translations.Clear();

            addManifestStrings();
            addConfigStrings();

            foreach (RecipeDef r in m_model.Recipes) {
                string baseKey = "recipe." + (r.RecipeId ?? "<no_id>");
                addString(baseKey + ".name",        r.Name);
                addString(baseKey + ".description", r.Description);
            }
            foreach (DefBase d in m_model.Definitions) {
                if (d is RecipeDef) continue; // already covered by the recipe loop above
                string baseKey = d.Kind.Replace(" ", "_").Replace("(", "").Replace(")", "")
                                  + "." + (string.IsNullOrEmpty(d.DisplayId) ? "<no_id>" : d.DisplayId);
                addString(baseKey + ".name", d.DisplayName);
                switch (d) {
                    case ResearchDef rs:      addString(baseKey + ".description", rs.Description); break;
                    case ProductLooseDef pl:  addString(baseKey + ".description", pl.Description); break;
                    case ProductFluidDef pf:  addString(baseKey + ".description", pf.Description); break;
                    case ProductUnitDef pu:   addString(baseKey + ".description", pu.Description); break;
                    case GeneratorDef gd:     addString(baseKey + ".description", gd.Description); break;
                }
            }

            string existingPath = languageFilePath(lang);
            if (File.Exists(existingPath)) {
                try {
                    object parsed = MiniJson.Parse(File.ReadAllText(existingPath));
                    if (parsed is Dictionary<string, object> dict) {
                        foreach (var kvp in dict) {
                            if (kvp.Value is string s) m_translations[kvp.Key] = s;
                        }
                    }
                } catch (Exception ex) {
                    Log.Warning("TranslationsPanel: failed to load existing '"
                                + existingPath + "' — " + ex.Message);
                }
            }

            renderRows();
            m_status.Value(new LocStrFormatted(
                "Loaded " + m_strings.Count + " string(s)" +
                (File.Exists(existingPath)
                    ? " (with existing translations from " + Path.GetFileName(existingPath) + ")"
                    : " — no existing file for '" + lang + "'")));
        }

        private void addString(string key, string source) {
            if (string.IsNullOrEmpty(source)) return;
            m_strings.Add(new KeyValuePair<string, string>(key, source));
        }

        // The pack's OWN presentation strings — what a player reads in the mod manager
        // before any of its content exists. Scanned from manifest.json rather than the
        // model, which only carries definitions.
        private void addManifestStrings() {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return;
            string manifestPath = Path.Combine(m_pack.RootPath, "manifest.json");
            if (!File.Exists(manifestPath)) return;
            try {
                object root = MiniJson.Parse(File.ReadAllText(manifestPath));
                if (!(root is Dictionary<string, object> dict)) return;
                addString("mod.display_name",       dict.TryGetValue("display_name", out object n) ? n as string : null);
                addString("mod.description_short",  dict.TryGetValue("description_short", out object s) ? s as string : null);
                addString("mod.description_long",   dict.TryGetValue("description_long", out object l) ? l as string : null);
            } catch (Exception ex) {
                Log.Warning("TranslationsPanel: manifest read failed — " + ex.Message);
            }
        }

        // Config field descriptions: the labels a player reads in the Pack Settings
        // panel. Keyed by field name so a renamed field simply gets a new key rather
        // than silently inheriting the old field's translation.
        private void addConfigStrings() {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return;
            ConfigSchema schema = ConfigSchema.Load(m_pack.RootPath);
            if (!schema.Existed || schema.LoadError != null) return;
            foreach (ConfigField field in schema.Fields) {
                if (string.IsNullOrEmpty(field.Name)) continue;
                addString("config." + field.Name + ".description", field.Description);
            }
        }

        // ---- Row rendering --------------------------------------------

        private void renderRows() {
            m_rowsColumn.Clear();
            foreach (var kvp in m_strings) {
                string capturedKey = kvp.Key;
                string source = kvp.Value;
                m_translations.TryGetValue(capturedKey, out string existing);

                Column rowCol = new Column();
                rowCol.Gap(1.pt()).PaddingTopBottom(1.pt()).AlignItemsStretch();
                rowCol.Add(new Label(new LocStrFormatted(capturedKey))
                    .Class(Cls.fontMonospace).TinyFontSize());
                rowCol.Add(new Label(new LocStrFormatted("src: " + source))
                    .Color(ColorRgba.LightGray));
                rowCol.Add(new TextField()
                    .Text(existing ?? "")
                    .OnValueChanged(v => m_translations[capturedKey] = v ?? ""));
                m_rowsColumn.Add(rowCol);
            }
        }

        // ---- Save -----------------------------------------------------

        private void onSave() {
            string lang = (m_langField.GetText() ?? "").Trim();
            if (string.IsNullOrEmpty(lang)) {
                m_status.Value(new LocStrFormatted("Enter a language code before saving."));
                return;
            }
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) {
                m_status.Value(new LocStrFormatted("No pack selected — cannot save."));
                return;
            }
            try {
                string dir = Path.Combine(m_pack.RootPath, "Translations");
                Directory.CreateDirectory(dir);
                string outPath = Path.Combine(dir, lang + ".json");

                Dictionary<string, object> jsonRoot = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var kvp in m_translations) {
                    if (!string.IsNullOrWhiteSpace(kvp.Value)) jsonRoot[kvp.Key] = kvp.Value;
                }
                File.WriteAllText(outPath, MiniJsonWriter.Write(jsonRoot),
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                m_status.Value(new LocStrFormatted(
                    "Saved " + jsonRoot.Count + " entries to " + outPath));
            } catch (Exception ex) {
                Log.Exception(ex);
                m_status.Value(new LocStrFormatted("Save failed: " + ex.Message));
            }
        }

        private string languageFilePath(string lang) {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return null;
            return Path.Combine(m_pack.RootPath, "Translations", lang + ".json");
        }
    }
}
