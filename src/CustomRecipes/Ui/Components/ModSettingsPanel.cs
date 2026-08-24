using System;
using System.Collections.Generic;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Io;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components;

/// Settings for EVERY loaded pack — one list showing the config values of all mods
/// built on CustomAssets, so a player never has to open config.json by hand.
///
/// Values only: this panel never adds, removes or re-types a field (that is
/// <see cref="PackConfigPanel"/>, the pack author's tool). It writes the same
/// `default` slot the runtime reads, into each pack's own config.json.
///
/// Two safety rails, because a config value decides what a running game contains:
///   • A standing warning that changing a mod's settings can break a save.
///   • Fields the author marked <c>"editable": "on_add"</c> render locked, since
///     those gate which recipes and products exist; a deliberate per-field unlock
///     is required to touch one.
///
/// Hosted by <see cref="ModSettingsWindow"/>, which is a normal toolbar window —
/// available in an ordinary game, not only in the sandbox-gated pack editor.
public sealed class ModSettingsPanel : Column
{
    private const string BreakageWarning =
        "⚠ Changing a mod's settings affects a game that is already running on the old "
        + "values. A setting that turns content on or off can leave a save referring to "
        + "recipes, products or machines that no longer get registered — which can break "
        + "that save. Prefer changing settings before adding a mod to a game, back up a "
        + "save you care about, and treat locked fields as unsafe to change here.";

    private readonly LoadedPack m_expandPack;

    // One tab per activated mod that exposes settings, plus the body showing the
    // selected mod's fields. Tabs rather than a stack of expanders: a game can have a
    // dozen packs, and a player looking for one mod's settings should not have to
    // scroll past everyone else's.
    private readonly Row m_tabRow = new Row();
    private readonly Column m_packsColumn = new Column();
    private readonly Label m_status;
    private readonly Label m_summary;

    private PackEntry m_selected;

    // One entry per loaded pack. Dirty is set by any value edit so Save touches only
    // the packs the player actually changed.
    private readonly List<PackEntry> m_entries = new List<PackEntry>();

    /// <param name="expandPack">Pack whose group starts expanded — the one the modder
    /// is working on when opened from the editor. Null expands nothing.</param>
    public ModSettingsPanel(LoadedPack expandPack = null)
    {
        m_expandPack = expandPack;
        m_status = new Label(new LocStrFormatted("")).TinyFontSize();
        m_summary = new Label(new LocStrFormatted("")).TinyFontSize().Color(ColorRgba.LightGray);

        this.AlignItemsStretch().Gap(3.pt());

        Add(new Label(new LocStrFormatted(BreakageWarning)).TinyFontSize().Color(ColorRgba.Orange));

        // Wrapped so a long mod list becomes several rows of tabs instead of running
        // off the edge of the window.
        m_tabRow.Wrap().Gap(2.px(), 2.px()).AlignItemsCenter();
        Add(m_tabRow);

        // The selected mod's fields live in their own panel directly under the tab
        // strip, so the tab reads as a tab: a strip of buttons with a body attached to
        // it, rather than free-floating rows on the window background.
        ScrollColumn scroll = new ScrollColumn();
        m_packsColumn.AlignItemsStretch().Gap(3.pt());
        scroll.Add(m_packsColumn);
        scroll.FlexGrow(1f).AlignItemsStretch();

        Panel tabBody = new Panel(noBolts: true);
        tabBody.BodyAdd(c => c.AlignItemsStretch().Padding(4.px()).Gap(2.pt()), scroll);
        tabBody.FlexGrow(1f);
        Add(tabBody);

        Add(m_summary);
        Add(m_status);

        Row footer = new Row {
            new ButtonText(new LocStrFormatted("Save changed packs"), onSave),
            // config.json can also be edited by hand or by the pack author's dialog
            // while this panel is open; the window is created once and reused, so an
            // explicit reload is the honest way to pick that up.
            new ButtonText(new LocStrFormatted("Reload from disk"), Reload)
        };
        footer.Gap(3.pt());
        Add(footer);

        Reload();
    }

    /// Re-read every pack's config.json and rebuild the tabs, discarding unsaved edits.
    public void Reload()
    {
        m_entries.Clear();
        m_selected = null;

        int configurable = 0;
        foreach (LoadedPack pack in PackRegistry.Packs)
        {
            PackEntry entry = new PackEntry(pack, ConfigSchema.Load(pack.RootPath));
            m_entries.Add(entry);
            if (entry.HasSettings)
            {
                configurable++;
                // Open on the pack the caller cares about, else the first configurable
                // one, so the panel never opens on an empty body.
                bool preferred = m_expandPack != null && ReferenceEquals(pack, m_expandPack);
                if (m_selected == null || preferred)
                {
                    m_selected = entry;
                }
            }
        }

        m_summary.Value(new LocStrFormatted(
            "ⓘ " + configurable + " of " + m_entries.Count + " activated mod(s) expose settings. "
            + "Values are read when a pack loads, so changes take effect after the save is reloaded."));
        m_status.Value(new LocStrFormatted(""));

        rebuildTabs();
        showSelected();
    }

    // One tab per activated mod that actually has settings. A mod with no config.json
    // gets no tab — it has nothing to show, and listing it would only make the real
    // ones harder to find; the summary line above still counts it.
    private void rebuildTabs()
    {
        m_tabRow.Clear();
        List<ButtonText> tabs = new List<ButtonText>();

        foreach (PackEntry entry in m_entries)
        {
            if (!entry.HasSettings)
            {
                continue;
            }
            PackEntry captured = entry;
            ButtonText tab = null;
            tab = new ButtonText(new LocStrFormatted(PackManifestCache.DisplayName(entry.Pack)), () => {
                m_selected = captured;
                foreach (ButtonText other in tabs)
                {
                    other.ClassIff(Cls.selected, ReferenceEquals(other, tab));
                }
                showSelected();
            });
            tab.Tooltip(new LocStrFormatted(entry.Pack.ModId));
            tab.ClassIff(Cls.selected, ReferenceEquals(entry, m_selected));
            tabs.Add(tab);
            m_tabRow.Add(tab);
        }

        if (tabs.Count == 0)
        {
            m_tabRow.Add(new Label(new LocStrFormatted(
                    m_entries.Count == 0
                        ? "(no mods loaded — install a CustomAssets pack and reload the save)"
                        : "(none of the loaded mods expose settings)"))
                .TinyFontSize().Color(ColorRgba.LightGray));
        }
    }

    // Render the selected mod's fields. Edits mutate the ConfigField objects held by
    // that pack's schema, so switching tabs and coming back keeps unsaved changes —
    // only the controls are rebuilt.
    private void showSelected()
    {
        m_packsColumn.Clear();
        if (m_selected == null)
        {
            return;
        }

        PackEntry entry = m_selected;
        entry.Values.Clear();

        m_packsColumn.Add(new Label(new LocStrFormatted(entry.Pack.ModId))
            .TinyFontSize().Color(ColorRgba.LightGray));

        if (entry.Schema.LoadError != null)
        {
            m_packsColumn.Add(new Label(new LocStrFormatted(
                    "⚠ config.json could not be read: " + entry.Schema.LoadError))
                .TinyFontSize().Color(ColorRgba.Red));
            return;
        }

        Dictionary<string, object> computed = entry.Schema.EvaluateValues(
            out Dictionary<string, string> expressionErrors);

        foreach (ConfigField field in entry.Schema.Fields)
        {
            ConfigValueField valueField = new ConfigValueField(
                field,
                onChanged: () => entry.Dirty = true,
                lockable: true);
            m_packsColumn.Add(valueField);
            entry.Values.Add(valueField);

            if (field.HasExpression)
            {
                // A computed field is not a knob the player turns: show what it works out
                // to, and leave the editable box above it as the fallback it really is.
                bool failed = expressionErrors.ContainsKey(field.Name ?? "");
                string text = failed
                    ? "computed: " + field.Expression + " ⚠ " + expressionErrors[field.Name ?? ""]
                        + " — the value above is used instead"
                    : "computed: " + field.Expression + " = "
                        + (computed.TryGetValue(field.Name ?? "", out object v) ? v : "?");
                m_packsColumn.Add(new Label(new LocStrFormatted(text))
                    .TinyFontSize().Color(failed ? ColorRgba.Orange : ColorRgba.Green));
            }
        }
    }

    private void onSave()
    {
        List<string> saved = new List<string>();
        List<string> failed = new List<string>();

        foreach (PackEntry entry in m_entries)
        {
            if (!entry.Saveable || !entry.Dirty)
            {
                continue;
            }
            string invalid = entry.FirstError();
            if (invalid != null)
            {
                failed.Add(entry.Pack.ModId + ": " + invalid);
                continue;
            }
            try
            {
                entry.Schema.Save();
                entry.Dirty = false;
                saved.Add(entry.Pack.ModId);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                failed.Add(entry.Pack.ModId + ": " + ex.Message);
            }
        }

        if (saved.Count == 0 && failed.Count == 0)
        {
            m_status.Value(new LocStrFormatted("Nothing changed."));
            m_status.Color(ColorRgba.LightGray);
            return;
        }
        if (failed.Count == 0)
        {
            m_status.Value(new LocStrFormatted(
                "Saved: " + string.Join(", ", saved) + ". Reload the save to apply."));
            m_status.Color(ColorRgba.Green);
            return;
        }
        m_status.Value(new LocStrFormatted(
            (saved.Count > 0 ? "Saved: " + string.Join(", ", saved) + ". " : "")
            + "Not saved — " + string.Join("; ", failed)));
        m_status.Color(ColorRgba.Red);
    }

    /// One pack's row state: its schema, the value controls built for it, and whether
    /// the player touched any of them.
    private sealed class PackEntry
    {
        public readonly LoadedPack Pack;
        public readonly ConfigSchema Schema;
        public readonly List<ConfigValueField> Values = new List<ConfigValueField>();

        public bool Dirty;

        public PackEntry(LoadedPack pack, ConfigSchema schema)
        {
            Pack = pack;
            Schema = schema;
        }

        /// Whether this mod has anything to show: a config.json that parsed and defines
        /// at least one field. A pack that failed to parse still gets a tab so the
        /// player sees WHY it has no settings, but it is never written back to.
        public bool HasSettings =>
            Schema.LoadError != null || (Schema.Existed && Schema.Fields.Count > 0);

        /// False for a pack with nothing writable — no config.json, no fields, or a file
        /// we failed to parse and must not overwrite.
        public bool Saveable =>
            Schema.LoadError == null && Schema.Existed && Schema.Fields.Count > 0;

        /// First validation problem among this pack's value controls, or null. Checked
        /// before Save so an out-of-range value never lands in config.json.
        public string FirstError()
        {
            foreach (ConfigValueField field in Values)
            {
                if (!string.IsNullOrEmpty(field.Error))
                {
                    return field.Error;
                }
            }
            return Schema.Validate();
        }
    }
}
