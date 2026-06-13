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

        /// <summary>Clickable header row. ButtonRow gives us click handling and
        /// keyboard activation without registering raw UIElements callbacks
        /// against protected component internals.</summary>
        public readonly ButtonRow Header;

        /// <summary>The label component inside the header, exposed so callers can
        /// update text reactively (e.g. when the underlying child count changes).</summary>
        public readonly Label HeaderLabel;

        /// <summary>The container that holds the group's children. Add nested
        /// groups or leaf rows to this column directly.</summary>
        public readonly Column Body;

        private readonly Label m_chevron;
        private bool m_expanded;

        public bool IsExpanded => m_expanded;

        public CollapsibleGroup(LocStrFormatted label, bool expanded = false) {
            m_chevron    = new Label(new LocStrFormatted(expanded ? ChevronExpanded : ChevronCollapsed))
                            .Width(14.px());
            HeaderLabel  = new Label(label).FlexGrow(1f);
            Header       = new ButtonRow(Button.General, null);
            // Header centres chevron + label + any extra controls callers
            // add (e.g. a per-clause edit button) on the cross-axis, and
            // gives the label FlexGrow so it claims the slack instead of
            // letting the right edge crowd against the chevron.
            Header.AlignItemsCenter().Gap(2.pt());
            Header.Add(m_chevron);
            Header.Add(HeaderLabel);
            // Assign the click handler after construction so the toggle closes
            // over this instance correctly. OnClick fluently returns the button.
            Header.OnClick(() => SetExpanded(!m_expanded));

            // Body stretches its children horizontally so nested
            // CollapsibleGroups, inline editors (e.g. the if-condition
            // editor), and tree rows fill the available width regardless of
            // their own intrinsic size. Without it a row inside the body
            // hugs its content and the group looks half-empty on wide panes.
            Body = new Column();
            Body.AlignItemsStretch().Gap(1.pt());

            // Wrap header + body inside a Panel so the group reads as a card
            // (background + border + corners), matching the HTML mockup's
            // .group rule. noBolts:true keeps it visually quiet — the bolts
            // would be overkill at this nesting density. FlexGrow on the
            // card lets the group claim parent height when stacked in a
            // ScrollColumn — important for nested groups containing long
            // recipe lists that would otherwise scroll inside their own
            // clipped panel.
            Panel card = new Panel(noBolts: true);
            card.BodyAdd(c => c.Padding(3.px()).AlignItemsStretch().Gap(2.pt()),
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
