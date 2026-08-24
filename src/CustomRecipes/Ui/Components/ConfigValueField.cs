using System;
using CustomAssets.Editor.Io;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components;

/// One config.json field rendered as an editable VALUE: a toggle for a bool, a text
/// field for everything else, with the field's description, its range/format hint,
/// and any validation error underneath.
///
/// Shared by both config surfaces — <see cref="PackConfigPanel"/> (where the modder
/// also edits the schema around it) and <see cref="ModSettingsPanel"/> (where a
/// player only changes values) — so a value edited in either place is validated by
/// exactly the same rules.
///
/// Locking: a field marked <see cref="ConfigFieldEditability.OnAdd"/> renders
/// read-only with an explicit unlock button, because changing it on a game that
/// already has the pack is what breaks saves. The unlock is per-field and per-open;
/// it never persists.
public sealed class ConfigValueField : Column
{
    private readonly ConfigField m_field;
    private readonly Action m_onChanged;
    private readonly Column m_holder = new Column();
    private readonly Label m_error;
    private readonly bool m_lockable;
    private readonly string m_lockReason;

    private bool m_unlocked;

    /// Validation message for the current value, or null when it is fine. The owning
    /// dialog reads this to gate its Save button.
    public string Error { get; private set; }

    /// <param name="lockable">True in the settings panel, where an on_add field must
    /// not be changed casually. False in the schema editor, where the modder is
    /// authoring the field itself.</param>
    public ConfigValueField(ConfigField field, Action onChanged, bool lockable = false,
            string lockReason = null)
    {
        m_field = field;
        m_onChanged = onChanged;
        m_lockable = lockable && field.Editable == ConfigFieldEditability.OnAdd;
        m_lockReason = lockReason
            ?? "Only meant to be set when the mod is added to a game. Changing it now can break this save.";

        m_error = new Label(new LocStrFormatted("")).TinyFontSize().Color(ColorRgba.Red);
        m_error.Visible(false);

        this.AlignItemsStretch().Gap(1.px());
        m_holder.AlignItemsStretch().Gap(1.px());
        Add(m_holder);
        Add(m_error);
        rebuild();
    }

    /// Re-read the underlying field (kind or constraints changed elsewhere) and
    /// re-render the control.
    public void Refresh()
    {
        rebuild();
    }

    private void rebuild()
    {
        m_holder.Clear();

        bool locked = m_lockable && !m_unlocked;

        Row line = new Row().Gap(3.pt()).AlignItemsCenter();
        Label name = new Label(new LocStrFormatted(m_field.Name ?? ""));
        name.MinWidth(180.px()).FlexShrink(0f);
        line.Add(name);
        line.Add(buildControl(locked));

        if (m_lockable)
        {
            // Spelled out rather than a padlock glyph — the game's UI font carries only
            // the handful of symbols already used elsewhere in this editor, and an
            // unavailable one renders as a missing-glyph box (see PackCardView).
            // Visible in both states so the field always reads as "this one is special",
            // not merely "this one is greyed out".
            ButtonText lockBtn = new ButtonText(
                new LocStrFormatted(locked ? "locked" : "unlocked"),
                () => {
                    m_unlocked = !m_unlocked;
                    rebuild();
                });
            lockBtn.Tooltip(new LocStrFormatted(
                locked ? m_lockReason + " Click to change it anyway." : m_lockReason));
            line.Add(lockBtn);
        }

        m_holder.Add(line);

        if (!string.IsNullOrEmpty(m_field.Description))
        {
            m_holder.Add(new Label(new LocStrFormatted(m_field.Description))
                .TinyFontSize().Color(ColorRgba.LightGray));
        }

        string hint = m_field.ConstraintHint();
        if (m_lockable)
        {
            hint = string.IsNullOrEmpty(hint)
                ? "set when adding the mod"
                : hint + " · set when adding the mod";
        }
        if (!string.IsNullOrEmpty(hint))
        {
            m_holder.Add(new Label(new LocStrFormatted(hint))
                .TinyFontSize().Color(locked ? ColorRgba.Orange : ColorRgba.LightGray));
        }

        // A value that was already invalid on load (a hand-edited file out of its own
        // range) must show immediately, not only after the first keystroke.
        setError(m_field.ValidateValue());
    }

    private UiComponent buildControl(bool locked)
    {
        if (m_field.Kind == ConfigFieldKind.Bool)
        {
            Toggle toggle = new Toggle(standalone: true);
            toggle.Value(m_field.Value is bool b && b);
            toggle.OnValueChanged(v => {
                m_field.Value = v;
                setError(null);
                m_onChanged?.Invoke();
            });
            toggle.Enabled(!locked);
            return toggle;
        }

        TextField text = new TextField();
        if (m_field.Kind == ConfigFieldKind.Int)
        {
            // Keeps letters out of an int field; range is still checked on parse,
            // since AllIntegersOnly knows nothing about min/max.
            text.AllIntegersOnly();
        }
        text.Text(m_field.ValueText());
        if (m_field.Kind == ConfigFieldKind.String)
        {
            // Text can be arbitrarily long, so it takes the rest of the row. A number
            // gets a fixed box — a 600px-wide field holding "0" reads as a mistake.
            text.FlexGrow(1f);
        }
        else
        {
            text.Width(120.px());
        }
        text.Enabled(!locked);
        text.OnValueChanged(v => {
            if (m_field.TryParseValue(v, out object parsed, out string error))
            {
                m_field.Value = parsed;
                setError(null);
            }
            else
            {
                // Keep the invalid text on screen — retyping it for the modder is
                // worse than letting Save stay blocked until they fix it.
                setError(error);
            }
            m_onChanged?.Invoke();
        });
        return text;
    }

    private void setError(string error)
    {
        Error = error;
        m_error.Value(new LocStrFormatted(error ?? ""));
        m_error.Visible(!string.IsNullOrEmpty(error));
    }
}
