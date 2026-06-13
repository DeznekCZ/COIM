using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Generic Proto picker used throughout the recipe editor.
    ///
    /// Built on a custom <see cref="FloatingColumn"/> popup rather than the
    /// game's <c>ProtoPickerPopup&lt;T&gt;</c> so the same class can serve protos
    /// that DON'T implement <c>IProtoWithIcon</c> (notably
    /// <c>ResearchNodeProto</c>, which keeps its icon paths on a Gfx sub-struct).
    /// Callers supply <paramref name="iconPathOf"/> — for protos that satisfy
    /// IProtoWithIcon, this is just <c>p =&gt; p.IconPath</c>; for research it
    /// digs into <c>Graphics.Icons[0]</c> / <c>IconsProtos[0].IconPath</c>.
    ///
    /// Visual goal: every proto field in the recipe form reads as the same
    /// picker shape — DisplayRowWithButton (recessed bg + chevron) on the trigger
    /// side, and an icon + display name + dim id rendering on the popup options.
    /// A search field at the top of the popup filters on substring (name + id).
    /// </summary>
    public sealed class ProtoPicker<T> : DisplayRowWithButton where T : Proto {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly ProtosDb m_protosDb;
        private readonly Func<string> m_getId;
        private readonly Action<string> m_setId;
        private readonly Func<T, bool> m_filter;
        private readonly Func<T, string> m_iconPathOf;
        private readonly LocStrFormatted m_emptyLabel;
        private readonly LocStrFormatted m_title;
        // Optional pre-lookup hook. When the bound id doesn't resolve in
        // ProtosDb directly (e.g. it's a Python variable name like
        // "researchWoodgass" rather than the registered proto id), the
        // picker calls this delegate to translate the value first. Callers
        // typically pass a lambda that reads DefBase.SourceFileVariables.
        private readonly Func<string, string> m_variableResolver;
        // Show a "(none)" option at the top of the popup for optional fields
        // (e.g. a recipe's research, a product's research). Selecting it
        // writes null through setId so the emitter omits the arg / writes
        // None and the field reads as "no research required" afterwards.
        private readonly bool m_allowNone;

        // Content holder inside the inherited Row. Rebuilt on selection change
        // so the rest of the display chrome (recessed bg, chevron) stays put.
        private readonly Column m_buttonContent;

        public ProtoPicker(
                ProtosDb protosDb,
                Func<string> getId,
                Action<string> setId,
                Func<T, bool> filter = null,
                Func<T, string> iconPathOf = null,
                LocStrFormatted? emptyLabel = null,
                LocStrFormatted? title = null,
                Func<string, string> variableResolver = null,
                bool allowNone = false)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_protosDb         = protosDb;
            m_getId            = getId;
            m_setId            = setId;
            m_filter           = filter;
            m_iconPathOf       = iconPathOf ?? defaultIconPath;
            m_emptyLabel       = emptyLabel ?? new LocStrFormatted("(pick…)");
            m_title            = title ?? new LocStrFormatted("Pick");
            m_variableResolver = variableResolver;
            m_allowNone        = allowNone;

            // Strip the LCD/digital "displayFont" class that DisplayRowWithButton
            // applies to its inner Row — that font is meant for numeric readouts
            // (KW, m³, dates) and makes localized product names like "ZPLODINY"
            // unreadable in their decorative form. We keep the recessed display
            // background (Cls.displayBg on `this`) for visual consistency with
            // other proto references — only the font is reverted to the normal
            // UI font that the rest of the editor uses.
            Row.ClassRemove(Cls.displayFont);

            m_buttonContent = new Column();
            Row.Add(m_buttonContent);

            Btn.OnClick(openPopup);
            RefreshDisplay();
        }

        /// <summary>Re-resolve the current id and update the trigger button's
        /// display. Call after the bound id changes externally (e.g. another
        /// recipe is selected in the tree).</summary>
        public void RefreshDisplay() {
            m_buttonContent.Clear();
            T current = lookupCurrent();
            if (current == null) {
                m_buttonContent.Add(new Label(m_emptyLabel));
                return;
            }
            // Two-line stack — name on top, dim id below — mirroring the
            // option rows in the popup. Earlier the trigger laid everything
            // on a single line which competed with qty/port for horizontal
            // space and the long display id (Product_FilterMediaIronLime…)
            // pushed neighbours far to the right. Stacking lets the labels
            // wrap naturally and the row reads as one compact card.
            Row content = new Row();
            string icon = m_iconPathOf(current);
            if (!string.IsNullOrEmpty(icon)) content.Add(new Icon(icon).Size(ButtonIconSize));
            Column labelStack = new Column {
                new Label(current.Strings.Name).Fill(),
                new Label(new LocStrFormatted(current.Id.Value)).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        // ---- Popup ----------------------------------------------------------

        // Build + open the FloatingColumn popup fresh on each click so the
        // search filter starts empty and the option list reflects the latest
        // ProtosDb contents (new protos can appear after the editor opens if
        // another mod registers them later).
        private void openPopup() {
            FloatingColumn popup = new FloatingColumn(
                FloaterPositionPolicy.BELOW,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            // FloatingColumn renders transparent + borderless by default —
            // give it Cls.panel (the unified panel chrome that ties bg, border,
            // and bolts together) so the picker reads as a real popup instead
            // of floating glyphs over the form behind it. AlignItemsStretch
            // propagates width to every child added below — without it the
            // option rows hug their content (icon + name + chip) and leave a
            // gap on the right, while the search field stretches via its own
            // FlexGrow chain.
            PanelWithHeader panel = popup.AddAndReturn(new PanelWithHeader(m_title))
				.AlignItemsStretch()
				.Gap(2.pt())
				.MinWidth(380.px())
				.MaxHeight(500.px());
            panel.Header.Clear();
			panel.Header.Add(new Label(m_title).FontBold());

            // Snapshot the filtered source once per open. Ordering by display
            // name produces a stable, intuitive list — id-order would mix
            // alphabetically and feel arbitrary to modders.
            IEnumerable<T> sourceEnum = m_protosDb.All<T>();
            if (m_filter != null) sourceEnum = sourceEnum.Where(m_filter);
            List<T> source = sourceEnum
                .OrderBy(n => n.Strings.Name.TranslatedString,
                         StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Search box: filters the already-built option rows by name or id
            // substring. We store the rows separately so the filter only
            // toggles visibility instead of rebuilding the popup body, which
            // would close + reopen the floating panel.
            TextField search = new TextField()
                .Placeholder(new LocStrFormatted("search…"));
			panel.Header.Add(search);

            ScrollColumn list = new ScrollColumn();
            // AlignItemsStretch on the inner list makes each ButtonRow fill
            // the popup width — without it the rows hug their content (icon +
            // labelStack + mod chip) and leave a visible empty strip on the
            // right side of the popup. The popup's own AlignItemsStretch makes
            // the ScrollColumn itself reach edge-to-edge; this propagates that
            // further to the items inside.
            list.Gap(1.pt()).MaxHeight(420.px()).AlignItemsStretch();
			panel.BodyAdd(list);

            // Map row → lowercased haystack (name + id + mod) so search
            // comparisons stay cheap. We avoid allocating new strings per
            // keypress by pre-computing the haystack once during initial build.
            List<KeyValuePair<UiComponent, string>> rows = new List<KeyValuePair<UiComponent, string>>();

            // Optional "(none)" row for clearing the field — only added when
            // the caller opted in via allowNone. The haystack includes
            // synonyms so a search for "clear" / "empty" / "remove" still
            // surfaces it.
            if (m_allowNone) {
                ButtonRow noneRow = new ButtonRow(
                    Mafi.Unity.UiToolkit.Library.Button.General,
                    () => {
                        popup.Close();
                        m_setId(null);
                        RefreshDisplay();
                    });
                noneRow.Class(Cls.group);
                noneRow.Gap(3.pt()).AlignItemsCenter().PaddingLeftRight(2.pt());
                Column noneLabel = new Column {
                    new Label(new LocStrFormatted("(none)")),
                    new Label(new LocStrFormatted("clear the current selection")).TinyFontSize()
                };
                noneLabel.Fill();
                noneRow.Add(noneLabel);
                list.Add(noneRow);
                rows.Add(new KeyValuePair<UiComponent, string>(noneRow, "none clear empty remove"));
            }

            foreach (T option in source) {
                T captured = option;
                ButtonRow row = new ButtonRow(
                    Mafi.Unity.UiToolkit.Library.Button.General,
                    () => {
                        popup.Close();
                        m_setId(captured.Id.Value);
                        RefreshDisplay();
                    });
				row.Class(Cls.group);
                row.Gap(3.pt()).AlignItemsCenter().PaddingLeftRight(2.pt());
                string icon = m_iconPathOf(option);
                if (!string.IsNullOrEmpty(icon))
                    row.Add(new Icon(icon).Size(OptionIconSize));
                Column labelStack = new Column {
                    new Label(option.Strings.Name),
                    new Label(new LocStrFormatted(option.Id.Value)).TinyFontSize()
                };
                labelStack.Fill();
                row.Add(labelStack);

                // Provenance chip — short tag showing which mod registered the
                // proto. IProto.Mod is populated by the prototype registry, so
                // we can read it without building any cross-pack index. Empty
                // or null Mod is shown as "(unknown)" rather than skipped so
                // modders can see where the gap is.
                string modTag = readModTag(option);
                if (!string.IsNullOrEmpty(modTag)) {
                    row.Add(new Label(new LocStrFormatted(modTag)).TinyFontSize());
                }
                list.Add(row);

                string haystack = (option.Strings.Name.TranslatedString + "\n"
                                   + option.Id.Value + "\n" + (modTag ?? ""))
                    .ToLowerInvariant();
                rows.Add(new KeyValuePair<UiComponent, string>(row, haystack));
            }

            search.OnValueChanged(v => {
                string needle = (v ?? "").Trim().ToLowerInvariant();
                if (needle.Length == 0) {
                    foreach (var kvp in rows) kvp.Key.Visible(true);
                    return;
                }
                foreach (var kvp in rows) {
                    kvp.Key.Visible(kvp.Value.Contains(needle));
                }
            });
			search.FocusOnShow();

            popup.Open(Btn);
		}

        // ---- Lookup ---------------------------------------------------------

        // Two-pass id → proto resolution:
        //   1. Direct ProtosDb.Get on the stored id (works for "Product_X"-style
        //      strings written as Python string literals).
        //   2. Fallback via TypedRefResolver for dotted typed-ref paths like
        //      "Ids.Machines.AssemblyElectrified" → "AssemblyElectrified".
        private T lookupCurrent() {
            string id = m_getId();
            if (string.IsNullOrEmpty(id)) return null;
            // Three-tier resolution:
            //   1. Direct ProtosDb hit — handles "Product_X"/"Recipe_X" ids.
            //   2. Variable resolver — turns a Python variable name (e.g.
            //      "researchWoodgass") into the underlying registered id by
            //      consulting the def's per-file SourceFileVariables map.
            //      Falls through when the resolver returns null/unchanged.
            //   3. TypedRefResolver — converts dotted typed-ref paths like
            //      "Ids.Machines.Foo" to the underlying proto id by
            //      reflecting through Mafi.Base.Ids.
            Option<T> direct = m_protosDb.Get<T>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            if (m_variableResolver != null) {
                string viaVar = m_variableResolver(id);
                if (!string.IsNullOrEmpty(viaVar) && viaVar != id) {
                    Option<T> byVar = m_protosDb.Get<T>(new Proto.ID(viaVar));
                    if (byVar.HasValue) return byVar.Value;
                }
            }
            string resolved = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<T> byPath = m_protosDb.Get<T>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // Default icon resolver: works for any proto that implements
        // IProtoWithIcon. Callers passing a Proto type without that interface
        // (e.g. ResearchNodeProto) must supply their own iconPathOf.
        private static string defaultIconPath(T proto) {
            return proto is IProtoWithIcon ip ? ip.IconPath : null;
        }

        // Read the proto's owning mod name as a short chip. IProto.Mod is the
        // IMod instance that registered the prototype; its Manifest has both
        // a stable Id and a localized DisplayName. We prefer DisplayName when
        // present (more readable in the popup) and fall back to Id. Wraps in
        // parentheses to set the chip apart from the proto's own id label.
        private static string readModTag(T proto) {
            if (proto?.Mod == null) return null;
            var manifest = proto.Mod.Manifest;
            if (manifest == null) return null;
            string label = !string.IsNullOrEmpty(manifest.DisplayName)
                ? manifest.DisplayName
                : manifest.Id;
            return string.IsNullOrEmpty(label) ? null : "(" + label + ")";
        }
    }
}
