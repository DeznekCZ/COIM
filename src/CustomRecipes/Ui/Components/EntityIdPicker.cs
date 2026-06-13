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
    /// Picker for any "buildable entity" id — anything that registers as a
    /// <see cref="LayoutEntityProto"/> at runtime (machines, generators,
    /// housing, hospitals, mine towers, research labs, settlement modules,
    /// nuclear reactors). Used by the <c>add_toolbar_category.entities</c>
    /// list editor so the modder can add a pack-defined building they just
    /// created OR a vanilla one without typing the id by hand.
    ///
    /// Same trigger shape + popup chrome as <see cref="MachineIdPicker"/>:
    /// pack-defined entities first (each row chips its source def kind +
    /// the variable name when in-file-bound), then game entities grouped
    /// by mod. The picker stores the variable name when one is bound in
    /// the owner def's source file; otherwise the bare id. Round-trips
    /// through the emitter so the file matches the modder's style.
    /// </summary>
    public sealed class EntityIdPicker : DisplayRowWithButton {

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

        public EntityIdPicker(
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
            m_title       = title ?? new LocStrFormatted("Pick entity");
            m_emptyLabel  = emptyLabel ?? new LocStrFormatted("(pick an entity...)");
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
            NamedDef pack = findPackEntity(resolved);
            if (pack != null) {
                renderPackTrigger(pack, current);
                return;
            }
            LayoutEntityProto game = findGameEntity(resolved);
            if (game != null) {
                renderGameTrigger(game, current);
                return;
            }
            m_buttonContent.Add(new Label(new LocStrFormatted(current))
                .Class(Cls.fontMonospace));
        }

        private void renderPackTrigger(NamedDef def, string storedId) {
            Row content = new Row();
            Column labelStack = new Column {
                new Label(new LocStrFormatted(def.Name ?? def.Id ?? "")).Fill(),
                new Label(new LocStrFormatted(storedId ?? def.Id ?? "")).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Add(new Label(new LocStrFormatted("(" + def.Kind + ")")).TinyFontSize());
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        private void renderGameTrigger(LayoutEntityProto proto, string storedId) {
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
                .Placeholder(new LocStrFormatted("search by id, name, or kind…"));
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

            // Pack entities first — modders often want to assign a
            // category to a building they just defined.
            List<NamedDef> pack = collectPackEntities();
            HashSet<string> packIds = new HashSet<string>(
                pack.Select(d => d.Id), StringComparer.Ordinal);

            if (pack.Count > 0) {
                Label header = new Label(new LocStrFormatted(
                    "This pack — " + pack.Count + " entity(ies)"));
                header.FontBold().PaddingTopBottom(2.pt());
                list.Add(header);
                rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                foreach (NamedDef def in pack) {
                    NamedDef captured = def;
                    string sameFileVar = pickInFileVariable(captured);
                    string storeAs = sameFileVar ?? captured.Id;
                    string chip = sameFileVar != null
                        ? "(var " + sameFileVar + " · " + captured.Kind + ")"
                        : "(" + captured.Kind + ")";

                    UiComponent row = buildOptionRow(
                        thumb: null,
                        title: captured.Name ?? captured.Id,
                        subtitle: captured.Id,
                        chip: chip,
                        onClick: () => selectValue(storeAs));
                    list.Add(row);
                    rows.Add(new KeyValuePair<UiComponent, string>(row,
                        ((captured.Name ?? "") + " " + captured.Id + " "
                            + captured.Kind + " "
                            + (sameFileVar ?? "")).ToLowerInvariant()));
                }
            }

            // Game entities — every LayoutEntityProto registered in the
            // proto DB, skipping ids that already appeared above so the
            // same entity doesn't show twice.
            if (m_protosDb != null) {
                List<LayoutEntityProto> game = m_protosDb.All<LayoutEntityProto>()
                    .Where(p => !packIds.Contains(p.Id.Value))
                    .OrderBy(p => p.Strings.Name.TranslatedString,
                             StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (game.Count > 0) {
                    Label header = new Label(new LocStrFormatted(
                        "Game entities — " + game.Count + " entity(ies)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    list.Add(header);
                    rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                    foreach (LayoutEntityProto gp in game) {
                        LayoutEntityProto captured = gp;
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

        // Collect every NamedDef in the pack whose runtime produces a
        // LayoutEntityProto. Recipes / products / researches are excluded
        // — they're not entities. Sorted by id so the list is stable
        // across saves.
        private List<NamedDef> collectPackEntities() {
            List<NamedDef> result = new List<NamedDef>();
            if (m_packModel?.Definitions == null) return result;
            foreach (DefBase d in m_packModel.Definitions) {
                if (d is BuildMachineDef
                        || d is GeneratorDef
                        || d is HousingDef
                        || d is SettlementDecorationDef
                        || d is SettlementFoodDef
                        || d is SettlementIspDef
                        || d is HospitalDef
                        || d is MineTowerDef
                        || d is ResearchLabDef
                        || d is NuclearReactorDef) {
                    if (d is NamedDef nd && !string.IsNullOrEmpty(nd.Id)) {
                        result.Add(nd);
                    }
                }
            }
            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private NamedDef findPackEntity(string id) {
            if (m_packModel?.Definitions == null) return null;
            foreach (DefBase d in m_packModel.Definitions) {
                if (d is NamedDef nd && nd.Id == id) {
                    // Confirm it's an entity kind (not a recipe / product).
                    if (d is BuildMachineDef
                            || d is GeneratorDef
                            || d is HousingDef
                            || d is SettlementDecorationDef
                            || d is SettlementFoodDef
                            || d is SettlementIspDef
                            || d is HospitalDef
                            || d is MineTowerDef
                            || d is ResearchLabDef
                            || d is NuclearReactorDef) {
                        return nd;
                    }
                }
            }
            return null;
        }

        private LayoutEntityProto findGameEntity(string id) {
            if (m_protosDb == null) return null;
            Option<LayoutEntityProto> direct = m_protosDb.Get<LayoutEntityProto>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef)) {
                Option<LayoutEntityProto> byPath = m_protosDb.Get<LayoutEntityProto>(
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

        private string pickInFileVariable(NamedDef nd) {
            if (nd == null) return null;
            DefBase owner = m_getOwnerDef?.Invoke();
            Dictionary<string, string> vars = owner?.SourceFileVariables;
            if (vars == null) return null;
            string ownerSourceFile = owner.SourceFile;
            if (string.IsNullOrEmpty(ownerSourceFile)) return null;
            if (!string.Equals(nd.SourceFile, ownerSourceFile,
                    StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            foreach (var kvp in vars) {
                if (kvp.Value == nd.Id) return kvp.Key;
            }
            return null;
        }

        private static string readModTag(LayoutEntityProto proto) {
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
