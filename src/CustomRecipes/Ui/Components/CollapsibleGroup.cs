using System;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Expand/collapse group used by the editor tree. Mirrors the &lt;details&gt;
    /// element from the HTML mockup: a clickable header row containing a chevron
    /// and a label, plus a body Column whose children render below when the
    /// group is expanded.
    ///
    /// The whole group is wrapped in a Panel (no bolts) so it gets the standard
    /// card chrome — background, border, rounded corners — that the HTML
    /// <c>.group</c> rule provided. Nested groups become nested cards visually,
    /// matching the mockup's recursive layout.
    /// </summary>
    public sealed class CollapsibleGroup : Column {

        // Right-arrow / down-arrow chevron rendered as text. UiToolkit doesn't
        // pose a strong opinion on rotated SVGs vs glyph swap, and glyph swap
        // is both cheaper at runtime and easier to style consistently with the
        // tiny default theme.
        private const string ChevronCollapsed = "▶";
        private const string ChevronExpanded  = "▼";

        /// <summary>Edge length of every square control in a group header — the
        /// collapse chevron here, and the settings / add / delete icon buttons
        /// callers append. Shared so the row reads as one strip of equal buttons
        /// instead of a chevron that is subtly the wrong shape.</summary>
        public const int ButtonSize = 28;

        /// <summary>Icon edge inside a <see cref="ButtonSize"/> box, leaving 4px
        /// of padding on each side.</summary>
        public const int IconSize = 20;

        /// <summary>Edge length of a square control on a LEAF ROW (the per-def
        /// delete button). Deliberately smaller than <see cref="ButtonSize"/>:
        /// a row must not weigh as much as the group header above it. An icon
        /// button left to size itself from icon + variant padding lands
        /// somewhere between the two and drags the whole row's height with
        /// it — which is what made a row look taller than its own header.</summary>
        public const int RowButtonSize = 22;

        /// <summary>Icon edge inside a <see cref="RowButtonSize"/> box.</summary>
        public const int RowIconSize = 14;

        /// <summary>Horizontal gap between the controls of a header or a row.
        /// 8px = the game's own <c>2.pt()</c> — note <c>pt</c> is 4× <c>px</c>
        /// in Mafi, so these are spelled in px here to keep the arithmetic
        /// below honest.</summary>
        public const int ControlGap = 8;

        /// <summary>Vertical gap between the rows inside <see cref="Body"/>.
        /// Matches <c>ObjEditor.LIST_GAP</c>, the game's own list rhythm.</summary>
        public const int RowGap = 2;

        /// <summary>Vertical gap between <see cref="Header"/> and
        /// <see cref="Body"/>. Bigger than <see cref="RowGap"/> so the header
        /// reads as a header, small enough that the first row doesn't look
        /// detached from it.</summary>
        public const int HeaderBodyGap = 4;

        /// <summary>Inner padding of the group's card.</summary>
        public const int CardPadding = 3;

        /// <summary>Width of a row's drag grip — also reserved (blank) on rows
        /// that cannot be dragged, so every label in a body starts in the same
        /// column.</summary>
        public const int GripWidth = 8;

        /// <summary>Width of a row's state marker (the ⚠ / ● glyph). Sized so
        /// the glyph is not clipped and so grip + marker + gaps add up to
        /// exactly <see cref="TextIndent"/>.</summary>
        public const int MarkerWidth = 12;

        /// <summary>X offset of the label column, measured from the card's
        /// content edge. A header spends it on the chevron plus one gap; a row
        /// spends it on grip + gap + marker + gap. Both sums are
        /// <see cref="ButtonSize"/> + <see cref="ControlGap"/>, which is what
        /// keeps a row's label directly under its group header's label.
        /// Body notes (tips, placeholders) pad by it for the same reason.</summary>
        public const int TextIndent = ButtonSize + ControlGap;

        /// <summary>Header row: the expand toggle, the label, and whatever
        /// controls the caller appends (settings, delete, …).
        ///
        /// A plain Row, NOT a button. Only the small chevron toggles the group,
        /// so the header costs about as much space as a single icon button and
        /// the rest of the row is free for controls that need their own clicks.
        /// Making the whole row the toggle target is what made a group header
        /// tower over the rows inside it.</summary>
        public readonly Row Header;

        /// <summary>The header's text element. A <see cref="ButtonText"/> when the
        /// group was constructed with an <c>onLabelClick</c>, a plain
        /// <see cref="Label"/> otherwise.</summary>
        public readonly UiComponent HeaderText;

        /// <summary>Non-null only for a selectable group. A block header stands in
        /// for the statement it describes (the `with build_recipe(…)` line, an `if`
        /// condition), so clicking it must select that def into the editor pane
        /// exactly as a tree row does — same Button.Area hover/selected styling,
        /// and exposed here so the owner can register it for selection
        /// highlighting alongside the ordinary rows.</summary>
        public readonly ButtonText HeaderButton;

        /// <summary>The container that holds the group's children. Add nested
        /// groups or leaf rows to this column directly.</summary>
        public readonly Column Body;

        private readonly Label m_chevron;
        private bool m_expanded;

        public bool IsExpanded => m_expanded;

        public CollapsibleGroup(LocStrFormatted label, bool expanded = false,
                Action onLabelClick = null) {
            m_chevron = new Label(new LocStrFormatted(expanded ? ChevronExpanded : ChevronCollapsed));

            // The toggle is its own small button — a ButtonRow wrapping the
            // glyph, because the chevron swaps text on expand/collapse and a
            // Label is what supports that cheaply.
            //
            // Explicitly square at ButtonSize, matching the add / settings /
            // delete buttons a caller appends to the header.
            //
            // Width/Height alone do NOT square a Button: they land on the outer
            // wrapper element, while the variant's padding lands on the inner one
            // that draws the button — so the visible box kept the shape
            // Cls.btn_general's asymmetric padding gave it (wider than tall) and
            // the glyph was squeezed off-centre. Zeroing the padding hands the
            // whole box to AlignItemsCenterMiddle, which centres the glyph in it.
            // Compact() drops the variant's minimum width, which would otherwise
            // push the inner element back out past the wrapper.
            ButtonRow toggle = new ButtonRow(Button.General, null);
            toggle.Compact()
                  .AlignItemsCenterMiddle()
                  .Width(ButtonSize.px())
                  .Height(ButtonSize.px())
                  .Padding(0.px())
                  .FlexShrink(0f);
            toggle.Add(m_chevron);
            toggle.OnClick(() => SetExpanded(!m_expanded));

            // MinWidth(0) + ellipsis lets a long header shrink instead of
            // forcing the whole group wider than the tree pane.
            //
            // Selectable groups use Button.Area — the same variant the tree rows
            // use — so a header behaves like the row it replaced: hover feedback,
            // Cls.selected highlight, click to open the def. Non-selectable ones
            // (file groups, panel chrome) stay a plain Label so they don't invite
            // a click that does nothing.
            if (onLabelClick != null) {
                HeaderButton = new ButtonText(Button.Area, label, onLabelClick);
                HeaderButton.FlexGrow(1f)
                            .FlexShrink(1f)
                            .MinWidth(0.px())
                            .TextAlign(TextAlignment.LeftMiddle)
                            .TextOverflow(TextOverflow.Ellipsis)
                            .Class(Cls.group);
                HeaderText = HeaderButton;
            } else {
                Label plain = new Label(label);
                plain.FlexGrow(1f)
                     .FlexShrink(1f)
                     .MinWidth(0.px())
                     .TextOverflow(TextOverflow.Ellipsis);
                HeaderText = plain;
            }

            Header = new Row();
            Header.AlignItemsCenter().Gap(ControlGap.px());
            Header.Add(toggle);
            Header.Add(HeaderText);

            // Body stretches its children horizontally so nested
            // CollapsibleGroups, inline editors (e.g. the if-condition
            // editor), and tree rows fill the available width regardless of
            // their own intrinsic size. Without it a row inside the body
            // hugs its content and the group looks half-empty on wide panes.
            Body = new Column();
            // RowGap between a block's children — the separator, nested groups
            // and run columns — matching the spacing between statements inside a
            // run so nesting doesn't visibly change the rhythm.
            Body.AlignItemsStretch().Gap(RowGap.px());

            // Wrap header + body inside a Panel so the group reads as a card
            // (background + border + corners), matching the HTML mockup's
            // .group rule. noBolts:true keeps it visually quiet — the bolts
            // would be overkill at this nesting density. FlexGrow on the
            // card lets the group claim parent height when stacked in a
            // ScrollColumn — important for nested groups containing long
            // recipe lists that would otherwise scroll inside their own
            // clipped panel.
            //
            // The card's own gap separates header from body. It used to be
            // 2.pt() — 8px, i.e. FOUR times the gap between the rows below it,
            // which read as a hole under every header.
            Panel card = new Panel(noBolts: true);
            card.BodyAdd(
                c => c.Padding(CardPadding.px()).AlignItemsStretch().Gap(HeaderBodyGap.px()),
                Header,
                Body);
            card.FlexShrink(0f);

            // The outer Column (this) holds just the card so callers can use
            // the group like any other component (Add to parents, set widths,
            // etc.) without reaching into Panel-specific APIs.
            this.AlignItemsStretch();
            Add(card);

            SetExpanded(expanded);
        }

        public CollapsibleGroup SetExpanded(bool expanded) {
            m_expanded = expanded;
            m_chevron.Value(new LocStrFormatted(expanded ? ChevronExpanded : ChevronCollapsed));
            // Visible(false) sets display:none so the body is removed from
            // layout entirely; VisibleForRender(false) hides visually but
            // children still occupy space — collapsing must do the former
            // to match the HTML <details> behaviour.
            Body.Visible(expanded);
            return this;
        }

        /// Convenience for fluent style: <c>new CollapsibleGroup(...).WithChild(...).WithChild(...)</c>.
        public CollapsibleGroup WithChild(UiComponent child) {
            Body.Add(child);
            return this;
        }
    }
}
