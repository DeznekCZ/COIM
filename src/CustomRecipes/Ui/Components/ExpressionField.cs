using System;
using System.Globalization;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components;

/// A numeric form field that can hold an expression instead of a number.
///
/// Compact by design — these sit inside dense rows (a recipe ingredient is picker +
/// amount + port + delete on one line), so the expression itself is authored in the
/// <see cref="ExpressionComposer"/> popup rather than inline:
///   • plain number  → the ordinary text box, plus an `fx` button to switch.
///   • expression    → one button showing the expression text; clicking reopens the
///                     composer. The number stays in the model as the value to fall
///                     back to when the expression is cleared.
///
/// Both states write straight through to the caller's model via the accessors, so the
/// emitter sees the change with no separate commit step.
public sealed class ExpressionField : Row
{
    private readonly Func<int?> m_getNumber;
    private readonly Action<int?> m_setNumber;
    private readonly Func<string> m_getExpression;
    private readonly Action<string> m_setExpression;
    private readonly Action m_onChanged;
    private readonly int m_minValue;
    private readonly int m_width;
    private readonly bool m_allowEmpty;

    /// <param name="minValue">Lower clamp for the plain-number path (1 for a recipe
    /// quantity, 0 for most other slots). Expressions are not clamped — their value is
    /// not known until load time.</param>
    /// <param name="allowEmpty">True for an optional slot, where clearing the box means
    /// "omit the argument" (a null number) rather than zero.</param>
    public ExpressionField(
            Func<int?> getNumber, Action<int?> setNumber,
            Func<string> getExpression, Action<string> setExpression,
            Action onChanged = null, int minValue = 0, int width = 56,
            bool allowEmpty = false)
    {
        m_allowEmpty = allowEmpty;
        m_getNumber = getNumber;
        m_setNumber = setNumber;
        m_getExpression = getExpression;
        m_setExpression = setExpression;
        m_onChanged = onChanged;
        m_minValue = minValue;
        m_width = width;

        this.Gap(1.pt()).AlignItemsCenter();
        rebuild();
    }

    private void rebuild()
    {
        Clear();
        string expression = m_getExpression();

        if (string.IsNullOrWhiteSpace(expression))
        {
            int? current = m_getNumber();
            TextField number = new TextField()
                .Text(current.HasValue ? current.Value.ToString(CultureInfo.InvariantCulture) : "")
                .AllIntegersOnly()
                .ForFieldSetMinWidth(36.px())
                .Width(m_width.px());
            number.OnValueChanged(v => {
                if (string.IsNullOrWhiteSpace(v))
                {
                    if (!m_allowEmpty)
                    {
                        return;
                    }
                    m_setNumber(null);
                    m_onChanged?.Invoke();
                    return;
                }
                if (!int.TryParse(v.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    return;
                }
                int clamped = parsed < m_minValue ? m_minValue : parsed;
                m_setNumber(clamped);
                if (clamped != parsed)
                {
                    number.Text(clamped.ToString(CultureInfo.InvariantCulture));
                }
                m_onChanged?.Invoke();
            });
            Add(number);
            Add(openButton("fx", "Use an expression instead of a fixed number — "
                + "a config field, a variable, or a calculation"));
            return;
        }

        // Expression state: show it as the field's value. Truncated for the row, with
        // the whole thing in the tooltip, since a dense row cannot widen for it.
        string shown = expression.Length > 18 ? expression.Substring(0, 17) + "…" : expression;
        ButtonText button = new ButtonText(new LocStrFormatted(shown),
            () => openComposer(this));
        button.Tooltip(new LocStrFormatted(previewTooltip(expression)));
        button.Class(Cls.fontMonospace);
        Add(button);
    }

    private ButtonText openButton(string label, string tooltip)
    {
        ButtonText button = null;
        button = new ButtonText(new LocStrFormatted(label), () => openComposer(button));
        button.Tooltip(new LocStrFormatted(tooltip));
        return button;
    }

    /// Re-read the model and re-render. Called by editors whose bound def instance is
    /// swapped underneath the control (DefEditor.Value).
    public void Refresh()
    {
        rebuild();
    }

    private void openComposer(UiComponent anchor)
    {
        ExpressionComposer.Open(anchor, m_getExpression(), m_getNumber() ?? 0, applied => {
            m_setExpression(applied);
            m_onChanged?.Invoke();
            rebuild();
        });
    }

    // What the expression works out to right now, for the button's tooltip. Falls back
    // to the raw text when it references something only known at load time.
    private static string previewTooltip(string expression)
    {
        if (ExpressionContext.TryPreview(expression, out object value, out string _))
        {
            return expression + "  (currently = " + value + ")";
        }
        return expression + "  (click to edit)";
    }
}
