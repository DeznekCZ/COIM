using System;
using System.Collections.Generic;
using System.Globalization;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components;

/// Popup for authoring the expression behind a numeric field — a recipe quantity, a
/// duration, a cost.
///
/// Two ways in, because neither alone is enough:
///   • Compose by clicking: a chip per config field of this pack, the operators, and
///     an "insert number" box. Nothing to memorise, and the field names are right
///     there instead of in another window.
///   • Type it: the same text box the chips write into stays fully editable, so
///     anything the dialect accepts can be written by hand — including names the
///     composer knows nothing about, such as a variable defined earlier in the file.
///
/// A live preview evaluates the text against this pack's config values through the
/// same evaluator the game uses at load time, so a mistake shows up here rather than
/// as a broken pack.
public sealed class ExpressionComposer
{
    private static readonly string[] Operators = { "+", "-", "*", "/", "(", ")" };

    private readonly FloatingColumn m_holder;
    private readonly Panel m_dialog;
    private readonly TextField m_text;
    private readonly Label m_preview;
    private readonly Action<string> m_onApply;
    private readonly string m_numberLabel;

    private string m_current;

    /// <param name="current">Existing expression, or null when the field is a plain
    /// number.</param>
    /// <param name="numberFallback">The field's current numeric value, offered as the
    /// "back to a plain number" option.</param>
    /// <param name="onApply">Receives the new expression, or null to go back to the
    /// plain number.</param>
    public static void Open(UiComponent anchor, string current, int numberFallback,
            Action<string> onApply)
    {
        new ExpressionComposer(current, numberFallback, onApply).OpenAt(anchor);
    }

    private ExpressionComposer(string current, int numberFallback, Action<string> onApply)
    {
        m_current = current ?? "";
        m_onApply = onApply;
        m_numberLabel = numberFallback.ToString(CultureInfo.InvariantCulture);

        m_holder = new FloatingColumn(
            FloaterPositionPolicy.ABOVE,
            keepOpenOnHover: false,
            openAfterDelay: false,
            closeOnClickOutside: false);
        m_dialog = m_holder.AddAndReturn(new Panel())
            .AlignItemsStretch()
            .Padding(4.pt()).Gap(3.pt()).MinWidth(460.px()).MaxHeight(560.px());

        m_dialog.Add(new Label(new LocStrFormatted("Expression")).FontBold());
        m_dialog.Add(new Label(new LocStrFormatted(
                "Used instead of the number. Written into the pack source as you see it "
                + "here, and evaluated by the game when the pack loads."))
            .TinyFontSize().Color(ColorRgba.LightGray));

        m_text = new TextField().Text(m_current);
        m_text.FlexGrow(1f);
        m_text.OnValueChanged(v => {
            m_current = v ?? "";
            refreshPreview();
        });
        m_dialog.Add(m_text);

        m_preview = new Label(new LocStrFormatted("")).TinyFontSize();
        m_dialog.Add(m_preview);

        m_dialog.Add(new Label(new LocStrFormatted("Config fields")).TinyFontSize());
        m_dialog.Add(buildFieldChips());

        m_dialog.Add(new Label(new LocStrFormatted("Operators & numbers")).TinyFontSize());
        m_dialog.Add(buildOperatorRow());

        Row footer = new Row {
            new ButtonText(new LocStrFormatted("Use expression"), () => {
                m_onApply?.Invoke(string.IsNullOrWhiteSpace(m_current) ? null : m_current.Trim());
                m_holder.Close();
            }),
            // Clearing is the way back to an ordinary number; the field keeps the last
            // numeric value, so this never leaves the slot empty.
            new ButtonText(new LocStrFormatted("Use number " + m_numberLabel), () => {
                m_onApply?.Invoke(null);
                m_holder.Close();
            }),
            new ButtonText(new LocStrFormatted("Cancel"), m_holder.Close)
        };
        footer.Gap(3.pt());
        m_dialog.Add(footer);

        refreshPreview();
    }

    public void OpenAt(UiComponent anchor)
    {
        m_holder.Open(anchor);
    }

    private UiComponent buildFieldChips()
    {
        Row row = new Row();
        row.Wrap().Gap(2.px(), 2.px()).AlignItemsCenter();

        bool any = false;
        foreach (string name in ExpressionContext.ConfigFieldNames)
        {
            string captured = name;
            row.Add(new ButtonText(new LocStrFormatted(name), () => append("config." + captured)));
            any = true;
        }
        if (!any)
        {
            row.Add(new Label(new LocStrFormatted(
                    "(this pack has no config.json fields — you can still reference a "
                    + "variable defined earlier in the file)"))
                .TinyFontSize().Color(ColorRgba.LightGray));
        }
        return row;
    }

    private UiComponent buildOperatorRow()
    {
        Row row = new Row();
        row.Wrap().Gap(2.px(), 2.px()).AlignItemsCenter();

        foreach (string op in Operators)
        {
            string captured = op;
            row.Add(new ButtonText(new LocStrFormatted(op), () => append(captured)));
        }

        TextField number = new TextField().AllIntegersOnly().Width(70.px());
        row.Add(number);
        row.Add(new ButtonText(new LocStrFormatted("insert number"), () => {
            string value = (number.GetText() ?? "").Trim();
            if (value.Length > 0)
            {
                append(value);
            }
        }));
        return row;
    }

    // Chips append rather than replace: composing is incremental, and a click that
    // wiped a half-typed expression would be worse than one extra space to delete.
    private void append(string token)
    {
        m_current = string.IsNullOrWhiteSpace(m_current) ? token : m_current.TrimEnd() + " " + token;
        m_text.Text(m_current);
        refreshPreview();
    }

    private void refreshPreview()
    {
        if (string.IsNullOrWhiteSpace(m_current))
        {
            m_preview.Value(new LocStrFormatted("empty — the field will use its number"));
            m_preview.Color(ColorRgba.LightGray);
            return;
        }

        if (ExpressionContext.TryPreview(m_current, out object value, out string error))
        {
            m_preview.Value(new LocStrFormatted("= " + value));
            m_preview.Color(ColorRgba.Green);
            return;
        }

        // A name the composer cannot resolve is not necessarily wrong: a pack variable
        // (`base_amount = 4` earlier in the file) is perfectly legal here and only
        // resolves at load time. Say that plainly instead of crying error.
        bool unknownName = error != null
            && (error.Contains("is not defined") || error.Contains("no field"));
        m_preview.Value(new LocStrFormatted(
            unknownName
                ? "cannot preview here (" + error + ") — fine if it is a variable from this file"
                : "⚠ " + error));
        m_preview.Color(unknownName ? ColorRgba.LightGray : ColorRgba.Orange);
    }
}
