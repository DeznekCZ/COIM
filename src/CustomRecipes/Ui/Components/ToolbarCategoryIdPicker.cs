using System;
using System.Collections.Generic;
using System.Linq;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Picker for a toolbar-category id (<c>add_toolbar_category.parent</c>,
    /// any future call that takes a category). Two sources of options:
    ///
    ///   • <see cref="PackModel"/> — <see cref="ToolbarCategoryDef"/>
    ///     entries in the current pack. When the owner def lives in the
    ///     same source file AND that file's variable map has a binding
    ///     pointing at the selected category, the picker stores the
    ///     variable name; otherwise it stores the bare category id.
    ///     Round-tripping through the emitter writes the variable form
    ///     (bare identifier) when set so the file matches the modder's
    ///     authored style.
    ///
    ///   • <see cref="ProtosDb"/> — every registered
    ///     <see cref="ToolbarCategoryProto"/>. Stored as the bare id.
    ///
    /// Near-verbatim copy of <see cref="MachineIdPicker"/>: same trigger
    /// shape, same popup chrome, same in-file-variable resolution. The
    /// type substitution is mechanical so future generic-typed pickers
    /// for other "pack def + game proto" hybrid surfaces can follow the
    /// same template.
    /// </summary>
    public sealed class ToolbarCategoryIdPicker : DisplayRowWithButton {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        private readonly Func<DefBase> m_getOwnerDef;
        private readonly Func<string> m_getId;
        private readonly Action<string> m_setId;
        private readonly LocStrFormatted m_title;
        private readonly LocStrFormatted m_emptyLabel;
        private readonly bool m_allowNone;

        private readonly Column m_buttonContent;
        private FloatingColumn m_popup;
        private TextField m_popupSearch;

        public ToolbarCategoryIdPicker(
                PackModel packModel,
                ProtosDb protosDb,
                Func<string> getId,
                Action<string> setId,
                Func<DefBase> getOwnerDef = null,
                LocStrFormatted? title = null,
                LocStrFormatted? emptyLabel = null,
                bool allowNone = false)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_packModel   = packModel;
            m_protosDb    = protosDb;
            m_getOwnerDef = getOwnerDef;
            m_getId       = getId;
            m_setId       = setId;
            m_title       = title ?? new LocStrFormatted("Pick toolbar category");
            m_emptyLabel  = emptyLabel ?? new LocStrFormatted("(pick a category...)");
            m_allowNone   = allowNone;

            Row.ClassRemove(Cls.displayFont);
            m_buttonContent = new Column();
            Row.Add(m_buttonContent);

            Btn.OnClick(openPopup);
            RefreshDisplay();
        }

        public void RefreshDisplay() {
            m_buttonContent.Clear();
            string current = m_getId();
            if (string.IsNullOrEmpty(current)) {
                m_buttonContent.Add(new Label(m_emptyLabel));
                return;
            }
            string resolved = resolveVariableToId(current) ?? current;
            ToolbarCategoryDef pack = findPackCategory(resolved);
            if (pack != null) {
                renderPackTrigger(pack, current);
                return;
            }
            ToolbarCategoryProto game = findGameCategory(resolved);
            if (game != null) {
                renderGameTrigger(game, current);
                return;
            }
            // Unknown id (typo, or a category from another mod not loaded
            // right now) — surface it verbatim in monospace so the modder
            // can spot the issue.
            m_buttonContent.Add(new Label(new LocStrFormatted(current))
                .Class(Cls.fontMonospace));
        }

        private void renderPackTrigger(ToolbarCategoryDef def, string storedId) {
            Row content = new Row();
            Column labelStack = new Column {
                new Label(new LocStrFormatted(def.Name ?? def.CategoryId ?? "")).Fill(),
                new Label(new LocStrFormatted(storedId ?? def.CategoryId ?? "")).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Add(new Label(new LocStrFormatted("(this pack)")).TinyFontSize());
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        private void renderGameTrigger(ToolbarCategoryProto proto, string storedId) {
            Row content = new Row();
            if (!string.IsNullOrEmpty(proto.IconPath)) {
                content.Add(new Icon(proto.IconPath).Size(ButtonIconSize));
            }
            Column labelStack = new Column {
                new Label(proto.Strings.Name).Fill(),
                new Label(new LocStrFormatted(storedId ?? proto.Id.Value)).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        // ---- Popup ----------------------------------------------------------

        private void openPopup() {
            if (m_popup != null) {
                if (m_popupSearch != null) m_popupSearch.Text("");
                m_popup.Open(Btn);
                return;
            }
            FloatingColumn popup = new FloatingColumn(
                FloaterPositionPolicy.BELOW,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            m_popup = popup;
            PanelWithHeader panel = popup.AddAndReturn(new PanelWithHeader(m_title))
                .AlignItemsStretch()
                .Gap(2.pt())
                .MinWidth(420.px())
                .MaxHeight(560.px());
            panel.Header.Clear();
            panel.Header.Add(new Label(m_title).FontBold());

            TextField search = new TextField()
                .Placeholder(new LocStrFormatted("search…"));
            m_popupSearch = search;
            panel.Header.Add(search);

            ScrollColumn list = new ScrollColumn();
            list.Gap(1.pt()).MaxHeight(480.px()).AlignItemsStretch();
            panel.BodyAdd(list);

            List<KeyValuePair<UiComponent, string>> rows =
                new List<KeyValuePair<UiComponent, string>>();

            if (m_allowNone) {
                UiComponent noneRow = buildOptionRow(
                    thumb: null,
                    title: "(none)",
                    subtitle: "clear the current selection",
                    chip: null,
                    onClick: () => selectValue(null));
                list.Add(noneRow);
                rows.Add(new KeyValuePair<UiComponent, string>(noneRow, "none clear empty remove"));
            }

            // Pack categories first — modders are likely to nest a
            // category they just defined in their own pack underneath
            // another freshly-defined one.
            List<ToolbarCategoryDef> pack = m_packModel?.Definitions?
                .OfType<ToolbarCategoryDef>()
                .Where(c => !string.IsNullOrEmpty(c.CategoryId))
                .OrderBy(c => c.CategoryId, StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<ToolbarCategoryDef>();
            HashSet<string> packIds = new HashSet<string>(
                pack.Select(c => c.CategoryId), StringComparer.Ordinal);

            if (pack.Count > 0) {
                Label header = new Label(new LocStrFormatted(
                    "This pack — " + pack.Count + " category(ies)"));
                header.FontBold().PaddingTopBottom(2.pt());
                list.Add(header);
                rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                foreach (ToolbarCategoryDef tc in pack) {
                    ToolbarCategoryDef captured = tc;
                    string sameFileVar = pickInFileVariable(captured);
                    string storeAs = sameFileVar ?? captured.CategoryId;
                    string chip = sameFileVar != null ? "(var " + sameFileVar + ")" : "(this pack)";

                    UiComponent row = buildOptionRow(
                        thumb: null,
                        title: captured.Name ?? captured.CategoryId,
                        subtitle: captured.CategoryId,
                        chip: chip,
                        onClick: () => selectValue(storeAs));
                    list.Add(row);
                    rows.Add(new KeyValuePair<UiComponent, string>(row,
                        ((captured.Name ?? "") + " " + captured.CategoryId + " "
                            + (sameFileVar ?? "")).ToLowerInvariant()));
                }
            }

            // Game categories from ProtosDb — skip ids already in the
            // pack section so the same category doesn't show twice.
            if (m_protosDb != null) {
                List<ToolbarCategoryProto> game = m_protosDb.All<ToolbarCategoryProto>()
                    .Where(p => !packIds.Contains(p.Id.Value))
                    .OrderBy(p => p.Strings.Name.TranslatedString,
                             StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (game.Count > 0) {
                    Label header = new Label(new LocStrFormatted(
                        "Game categories — " + game.Count + " category(ies)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    list.Add(header);
                    rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                    foreach (ToolbarCategoryProto gp in game) {
                        ToolbarCategoryProto captured = gp;
                        string modTag = readModTag(captured);

                        UiComponent row = buildOptionRow(
                            thumb: !string.IsNullOrEmpty(captured.IconPath)
                                ? new Icon(captured.IconPath).Size(OptionIconSize) : null,
                            title: captured.Strings.Name.TranslatedString,
                            subtitle: captured.Id.Value,
                            chip: modTag,
                            onClick: () => selectValue(captured.Id.Value));
                        list.Add(row);
                        rows.Add(new KeyValuePair<UiComponent, string>(row,
                            (captured.Strings.Name.TranslatedString + " "
                                + captured.Id.Value + " "
                                + (modTag ?? "")).ToLowerInvariant()));
                    }
                }
            }

            search.OnValueChanged(v => {
                string needle = (v ?? "").Trim().ToLowerInvariant();
                if (needle.Length == 0) {
                    foreach (var kvp in rows) kvp.Key.Visible(true);
                    return;
                }
                foreach (var kvp in rows) {
                    if (kvp.Value == null) { kvp.Key.Visible(true); continue; }
                    kvp.Key.Visible(kvp.Value.Contains(needle));
                }
            });
            search.FocusOnShow();

            popup.Open(Btn);
        }

        private UiComponent buildOptionRow(UiComponent thumb, string title,
                string subtitle, string chip, Action onClick) {
            ButtonColumn btn = new ButtonColumn(
                Mafi.Unity.UiToolkit.Library.Button.General, onClick);
            btn.Class(Cls.group);
            btn.Gap(1.pt()).AlignItemsStretch().PaddingLeftRight(2.pt())
               .PaddingTopBottom(2.pt());

            Row titleRow = new Row();
            titleRow.Gap(3.pt()).AlignItemsCenter();
            if (thumb != null) titleRow.Add(thumb);
            Column labelStack = new Column {
                new Label(new LocStrFormatted(title ?? "")),
                new Label(new LocStrFormatted(subtitle ?? "")).TinyFontSize()
            };
            labelStack.Fill();
            titleRow.Add(labelStack);
            if (!string.IsNullOrEmpty(chip)) {
                titleRow.Add(new Label(new LocStrFormatted(chip)).TinyFontSize());
            }
            btn.Add(titleRow);
            return btn;
        }

        private void selectValue(string value) {
            m_popup?.Close();
            m_setId(value);
            RefreshDisplay();
        }

        // ---- Lookups --------------------------------------------------------

        private ToolbarCategoryDef findPackCategory(string id) {
            if (m_packModel?.Definitions == null) return null;
            foreach (DefBase d in m_packModel.Definitions) {
                if (d is ToolbarCategoryDef tc && tc.CategoryId == id) return tc;
            }
            return null;
        }

        private ToolbarCategoryProto findGameCategory(string id) {
            if (m_protosDb == null) return null;
            Option<ToolbarCategoryProto> direct = m_protosDb.Get<ToolbarCategoryProto>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef)) {
                Option<ToolbarCategoryProto> byPath = m_protosDb.Get<ToolbarCategoryProto>(
                    new Proto.ID(viaTypedRef));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        private string resolveVariableToId(string maybeVariableName) {
            if (string.IsNullOrEmpty(maybeVariableName)) return null;
            DefBase owner = m_getOwnerDef?.Invoke();
            Dictionary<string, string> vars = owner?.SourceFileVariables;
            if (vars == null) return null;
            return vars.TryGetValue(maybeVariableName, out string id) ? id : null;
        }

        private string pickInFileVariable(ToolbarCategoryDef tc) {
            if (tc == null) return null;
            DefBase owner = m_getOwnerDef?.Invoke();
            Dictionary<string, string> vars = owner?.SourceFileVariables;
            if (vars == null) return null;
            string ownerSourceFile = owner.SourceFile;
            if (string.IsNullOrEmpty(ownerSourceFile)) return null;
            if (!string.Equals(tc.SourceFile, ownerSourceFile,
                    StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            foreach (var kvp in vars) {
                if (kvp.Value == tc.CategoryId) return kvp.Key;
            }
            return null;
        }

        private static string readModTag(ToolbarCategoryProto proto) {
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
