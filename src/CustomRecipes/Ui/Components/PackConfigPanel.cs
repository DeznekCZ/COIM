using System;
using System.Collections.Generic;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Io;
using CustomAssets.Ui.Editors;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components;

/// Editor for one pack's <c>config.json</c> — the fields themselves, not just their
/// values: name, kind, default, description, range/format constraints, whether a
/// player may change the field after the mod is added, and (for numbers) an
/// expression computed from the pack's other fields.
///
/// Rendered into the editor's main pane by the CFG button on the pack card — the same
/// place a definition's form appears, rather than a popup over it, so the modder edits
/// pack settings the way they edit everything else.
///
/// Saving rewrites config.json through <see cref="ConfigSchema"/>, which preserves
/// <c>$schema</c> and anything else the editor does not model.
public sealed class PackConfigPanel : Column
{
    private readonly LoadedPack m_pack;
    private readonly ConfigSchema m_schema;
    private readonly Column m_fieldsColumn = new Column();
    private readonly Label m_status;
    private readonly ButtonText m_saveBtn;

    public PackConfigPanel(LoadedPack pack)
    {
        m_pack = pack;
        m_schema = ConfigSchema.Load(pack?.RootPath);

        this.AlignItemsStretch().Gap(3.pt());

        m_status = new Label(new LocStrFormatted("")).TinyFontSize();
        m_saveBtn = new ButtonText(new LocStrFormatted("Save"), onSave);

        Add(new Label(new LocStrFormatted(
            "Config fields — " + (pack?.ModId ?? "?"))).FontBold());
        Add(new Label(new LocStrFormatted(m_schema.Existed
                ? m_schema.Path
                : m_schema.Path + "  (will be created)"))
            .TinyFontSize().Color(ColorRgba.LightGray));

        if (m_schema.LoadError != null)
        {
            // Never save over a file we could not parse — that would replace content we
            // never saw with whatever the (empty) UI holds.
            Add(new Label(new LocStrFormatted(
                    "⚠ config.json could not be read: " + m_schema.LoadError
                    + "\nFix it by hand; saving is disabled to avoid overwriting it."))
                .Color(ColorRgba.Red));
            m_saveBtn.Enabled(false);
        }

        m_fieldsColumn.AlignItemsStretch().Gap(3.pt());
        Add(m_fieldsColumn);

        Add(new ButtonText(new LocStrFormatted("+ add field"), () => {
            m_schema.Fields.Add(ConfigField.NewOfKind(nextFieldName(), ConfigFieldKind.Bool));
            rebuildFields();
        }));

        Add(new RestartNotice(
            "config.json is read when the pack loads, so changed values take effect "
            + "after the save is reloaded."));

        Add(m_status);
        Row footer = new Row { m_saveBtn };
        footer.Gap(3.pt());
        Add(footer);

        rebuildFields();
    }

    // ---- Field rows ---------------------------------------------------------

    private void rebuildFields()
    {
        m_fieldsColumn.Clear();


        if (m_schema.Fields.Count == 0)
        {
            m_fieldsColumn.Add(new Label(new LocStrFormatted(
                    "  (no fields — a pack without config.json simply has no `config` values)"))
                .Color(ColorRgba.LightGray));
        }

        for (int i = 0; i < m_schema.Fields.Count; i++)
        {
            m_fieldsColumn.Add(buildFieldCard(m_schema.Fields[i], i));
        }

    }

    private UiComponent buildFieldCard(ConfigField field, int index)
    {
        Panel card = new Panel(noBolts: true);
        Column body = new Column().AlignItemsStretch().Gap(2.pt());

        // Line 1 — name, kind, ordering, delete.
        Row head = new Row().Gap(3.pt()).AlignItemsCenter();
        TextField nameField = new TextField().Text(field.Name ?? "");
        nameField.Width(200.px());
        nameField.OnValueChanged(v => {
            field.Name = (v ?? "").Trim();
            // The name is a key in the evaluation scope, so renaming changes what other
            // fields' expressions resolve against.

        });
        head.Add(nameField);
        head.Add(buildKindPicker(field));
        head.FlexGrow(1f);

        if (index > 0)
        {
            head.Add(new ButtonText(new LocStrFormatted("▲"), () => {
                swap(index, index - 1);
                rebuildFields();
            }));
        }
        if (index < m_schema.Fields.Count - 1)
        {
            head.Add(new ButtonText(new LocStrFormatted("▼"), () => {
                swap(index, index + 1);
                rebuildFields();
            }));
        }
        head.Add(new ButtonText(new LocStrFormatted("✕"), () => {
            m_schema.Fields.RemoveAt(index);
            rebuildFields();
        }));
        body.Add(head);

        // Line 2 — the value itself. Never locked here: this dialog is where the modder
        // authors the field, so the on_add restriction (which protects PLAYERS from
        // breaking a live save) would only be in the way.
        Row valueRow = new Row().Gap(3.pt()).AlignItemsCenter();
        valueRow.Add(new Label(new LocStrFormatted("default")).TinyFontSize().Width(70.px()));
        ConfigValueField valueField = new ConfigValueField(field, null);
        valueField.FlexGrow(1f);
        valueRow.Add(valueField);
        body.Add(valueRow);

        // Line 3 — description.
        Row descRow = new Row().Gap(3.pt()).AlignItemsCenter();
        descRow.Add(new Label(new LocStrFormatted("description")).TinyFontSize().Width(70.px()));
        TextField descField = new TextField().Text(field.Description ?? "");
        descField.FlexGrow(1f);
        descField.OnValueChanged(v => field.Description = v);
        descRow.Add(descField);
        body.Add(descRow);

        // Line 4 — kind-specific constraints. A config field holds a CONSTANT: there is
        // deliberately no expression editor here. Expressions belong where a value is
        // USED — a recipe quantity, a duration, a cost — so that is where the editor
        // offers them (see ExpressionField). A hand-written `"expression"` key in
        // config.json still resolves at load time and is preserved when saving.
        if (field.Kind == ConfigFieldKind.Int || field.Kind == ConfigFieldKind.Float)
        {
            body.Add(buildRangeRow(field));
        }
        else if (field.Kind == ConfigFieldKind.String)
        {
            body.Add(buildTextConstraintsRow(field));
        }

        // Line 5 — editability. Unchecked marks the field `"editable": "on_add"`, which
        // is what locks it in the all-packs settings panel.
        Toggle editable = new Toggle(standalone: true);
        ((IComponentWithLabel)editable).SetLabel(new LocStrFormatted(
            "players can change this after the mod is added"));
        editable.Value(field.Editable == ConfigFieldEditability.Always);
        editable.OnValueChanged(v => field.Editable = v
            ? ConfigFieldEditability.Always
            : ConfigFieldEditability.OnAdd);
        editable.Tooltip(new LocStrFormatted(
            "Turn OFF for a field that decides which recipes or products get registered. "
            + "Changing one of those on an existing save can break it, so the settings "
            + "panel locks them once the pack is in a game."));
        body.Add(editable);

        card.BodyAdd(c => c.Padding(3.px()).AlignItemsStretch().Gap(2.pt()), body);
        card.FlexShrink(0f);
        return card;
    }

    private UiComponent buildKindPicker(ConfigField field)
    {
        Row line = new Row().Gap(1.pt()).AlignItemsCenter();
        List<ButtonText> buttons = new List<ButtonText>();
        (string Label, ConfigFieldKind Kind)[] kinds = {
            ("bool",  ConfigFieldKind.Bool),
            ("int",   ConfigFieldKind.Int),
            ("float", ConfigFieldKind.Float),
            ("text",  ConfigFieldKind.String)
        };

        foreach ((string label, ConfigFieldKind kind) in kinds)
        {
            ConfigFieldKind captured = kind;
            ButtonText button = null;
            button = new ButtonText(new LocStrFormatted(label), () => {
                field.ChangeKind(captured);
                // Kind drives which rows exist (range vs regex vs expression), so this
                // one does need a full rebuild.
                rebuildFields();
            });
            button.ClassIff(Cls.selected, field.Kind == kind);
            buttons.Add(button);
            line.Add(button);
        }
        return line;
    }

    private UiComponent buildRangeRow(ConfigField field)
    {
        Row row = new Row().Gap(3.pt()).AlignItemsCenter();
        row.Add(new Label(new LocStrFormatted("range")).TinyFontSize().Width(70.px()));

        row.Add(new Label(new LocStrFormatted("min")).TinyFontSize());
        TextField min = new TextField().Width(80.px());
        min.Text(field.Min.HasValue ? ConfigField.FormatNumberForUi(field.Min.Value) : "");
        min.OnValueChanged(v => {
            field.Min = EditorHelpers.ParseNullableDouble(v);

        });
        row.Add(min);

        row.Add(new Label(new LocStrFormatted("max")).TinyFontSize());
        TextField max = new TextField().Width(80.px());
        max.Text(field.Max.HasValue ? ConfigField.FormatNumberForUi(field.Max.Value) : "");
        max.OnValueChanged(v => {
            field.Max = EditorHelpers.ParseNullableDouble(v);

        });
        row.Add(max);
        row.Add(new Label(new LocStrFormatted("(blank = unbounded)"))
            .TinyFontSize().Color(ColorRgba.LightGray));
        return row;
    }

    private UiComponent buildTextConstraintsRow(ConfigField field)
    {
        Row row = new Row().Gap(3.pt()).AlignItemsCenter();
        row.Add(new Label(new LocStrFormatted("limits")).TinyFontSize().Width(70.px()));

        row.Add(new Label(new LocStrFormatted("max length")).TinyFontSize());
        TextField maxLength = new TextField().AllIntegersOnly().Width(70.px());
        maxLength.Text(field.MaxLength?.ToString() ?? "");
        maxLength.OnValueChanged(v => {
            double? parsed = EditorHelpers.ParseNullableDouble(v);
            field.MaxLength = parsed.HasValue ? (int)parsed.Value : (int?)null;
        });
        row.Add(maxLength);

        row.Add(new Label(new LocStrFormatted("regex")).TinyFontSize());
        TextField regex = new TextField();
        regex.Text(field.Regex ?? "");
        regex.FlexGrow(1f);
        regex.OnValueChanged(v => field.Regex = string.IsNullOrWhiteSpace(v) ? null : v.Trim());
        row.Add(regex);
        return row;
    }


    private void onSave()
    {
        try
        {
            if (!m_schema.Existed)
            {
                m_schema.EnsureSchemaPointer();
            }
            m_schema.Save();
            PackManifestCache.Clear();
            m_status.Value(new LocStrFormatted(
                "Saved " + m_schema.Fields.Count + " field(s). Reload the save for the pack to pick them up."));
            m_status.Color(ColorRgba.Green);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            m_status.Value(new LocStrFormatted("Not saved: " + ex.Message));
            m_status.Color(ColorRgba.Red);
        }
    }

    private void swap(int a, int b)
    {
        ConfigField tmp = m_schema.Fields[a];
        m_schema.Fields[a] = m_schema.Fields[b];
        m_schema.Fields[b] = tmp;
    }

    private string nextFieldName()
    {
        for (int i = 1; ; i++)
        {
            string candidate = "new_field" + (i == 1 ? "" : "_" + i);
            bool taken = false;
            foreach (ConfigField field in m_schema.Fields)
            {
                if (string.Equals(field.Name, candidate, StringComparison.Ordinal))
                {
                    taken = true;
                    break;
                }
            }
            if (!taken)
            {
                return candidate;
            }
        }
    }
}
