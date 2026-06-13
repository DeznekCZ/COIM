using System;
using System.Collections.Generic;
using System.Linq;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Picker for a research-id argument (recipe.research, product.research,
    /// generator.research, …). Two sources of options:
    ///
    ///   • <see cref="PackModel"/> — research nodes the modder is authoring in
    ///     the current pack. On selection the picker prefers a variable
    ///     binding when the selected research lives in the same source file
    ///     as the owner def — i.e. when the file already has
    ///     <c>researchX = build_research(...)</c> earlier, the picker stores
    ///     <c>"researchX"</c> and the emitter writes it bare (no quotes) so
    ///     the dependency reads naturally in Python. When no variable
    ///     binding exists (cross-file reference or the build_research call
    ///     wasn't bound) the picker falls back to storing the bare research
    ///     id as a quoted string.
    ///
    ///   • <see cref="ProtosDb"/> — every registered
    ///     <see cref="ResearchNodeProto"/>. Stored as the bare id (research
    ///     ids in COI's vanilla set are typically already terse — no
    ///     dotted typed-ref equivalent like recipes have).
    ///
    /// Display matches <see cref="RecipeIdPicker"/> for consistency:
    /// DisplayRowWithButton trigger, FloatingColumn popup with header search,
    /// two grouped sections, ButtonColumn rows so chip + title + sub-label
    /// stack rather than crowding onto one line.
    /// </summary>
    public sealed class ResearchIdPicker : DisplayRowWithButton {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        private readonly string m_ownerSourceFile;
        private readonly Dictionary<string, string> m_ownerVariables;
        private readonly Func<string> m_getId;
        private readonly Action<string> m_setId;
        private readonly LocStrFormatted m_title;

        private readonly Column m_buttonContent;
        private FloatingColumn m_popup;
        private TextField m_popupSearch;

        public ResearchIdPicker(
                PackModel packModel,
                ProtosDb protosDb,
                DefBase ownerDef,
                Func<string> getId,
                Action<string> setId,
                LocStrFormatted? title = null)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_packModel       = packModel;
            m_protosDb        = protosDb;
            m_ownerSourceFile = ownerDef?.SourceFile;
            m_ownerVariables  = ownerDef?.SourceFileVariables;
            m_getId           = getId;
            m_setId           = setId;
            m_title           = title ?? new LocStrFormatted("Pick research");

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
                m_buttonContent.Add(new Label(new LocStrFormatted("(no research required)")));
                return;
            }

            // Resolve order: in-file variable, then pack research id, then
            // game research id. Variable resolution mirrors the way the
            // emitter writes the value back — symmetric round-trip.
            string resolved = resolveVariableToId(current) ?? current;
            ResearchDef pack = findPackResearch(resolved);
            if (pack != null) {
                renderPackTrigger(pack, current);
                return;
            }
            ResearchNodeProto game = findGameResearch(resolved);
            if (game != null) {
                renderGameTrigger(game, current);
                return;
            }
            m_buttonContent.Add(new Label(new LocStrFormatted(current))
                .Class(Cls.fontMonospace));
        }

        private void renderPackTrigger(ResearchDef def, string storedId) {
            Row content = new Row();
            if (!string.IsNullOrEmpty(def.IconPath)) {
                content.Add(new Icon(def.IconPath).Size(ButtonIconSize));
            }
            Column labelStack = new Column {
                new Label(new LocStrFormatted(def.Name ?? def.ResearchId ?? "")).Fill(),
                new Label(new LocStrFormatted(storedId ?? def.ResearchId ?? "")).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Add(new Label(new LocStrFormatted("(this pack)")).TinyFontSize());
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        private void renderGameTrigger(ResearchNodeProto proto, string storedId) {
            Row content = new Row();
            string icon = researchIconPath(proto);
            if (!string.IsNullOrEmpty(icon)) content.Add(new Icon(icon).Size(ButtonIconSize));
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

            // (none) row first — research is optional on every consumer.
            UiComponent noneRow = buildOptionRow(
                thumb: null,
                title: "(none)",
                subtitle: "clear the current selection",
                chip: null,
                onClick: () => selectValue(null));
            list.Add(noneRow);
            rows.Add(new KeyValuePair<UiComponent, string>(noneRow, "none clear empty remove"));

            // Pack research first — modders are more likely to wire a recipe
            // to research they just authored than to vanilla research.
            List<ResearchDef> pack = m_packModel?.Definitions?
                .OfType<ResearchDef>()
                .Where(r => !string.IsNullOrEmpty(r.ResearchId))
                .OrderBy(r => r.ResearchId, StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<ResearchDef>();
            HashSet<string> packIds = new HashSet<string>(
                pack.Select(r => r.ResearchId), StringComparer.Ordinal);

            if (pack.Count > 0) {
                Label header = new Label(new LocStrFormatted(
                    "This pack — " + pack.Count + " research node(s)"));
                header.FontBold().PaddingTopBottom(2.pt());
                list.Add(header);
                rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                foreach (ResearchDef rd in pack) {
                    ResearchDef captured = rd;
                    // When the owner def lives in the same file as this
                    // research AND the file's variable map has a binding
                    // pointing at this research id, prefer the variable name
                    // — round-trips as a bare Python expression instead of a
                    // quoted string, and stays in sync with whatever name
                    // the modder chose in source. Otherwise fall back to the
                    // bare id (cross-file references can't use the local
                    // variable name).
                    string sameFileVar = pickInFileVariable(captured);
                    string storeAs = sameFileVar ?? captured.ResearchId;
                    string chip = sameFileVar != null ? "(var " + sameFileVar + ")" : "(this pack)";

                    UiComponent row = buildOptionRow(
                        thumb: !string.IsNullOrEmpty(captured.IconPath)
                            ? new Icon(captured.IconPath).Size(OptionIconSize) : null,
                        title: captured.Name ?? captured.ResearchId,
                        subtitle: captured.ResearchId,
                        chip: chip,
                        onClick: () => selectValue(storeAs));
                    list.Add(row);
                    rows.Add(new KeyValuePair<UiComponent, string>(row,
                        ((captured.Name ?? "") + " " + captured.ResearchId + " "
                            + (sameFileVar ?? "")).ToLowerInvariant()));
                }
            }

            // Game research from ProtosDb — skip ids already in the pack
            // section so the same node doesn't show twice.
            if (m_protosDb != null) {
                List<ResearchNodeProto> game = m_protosDb.All<ResearchNodeProto>()
                    .Where(p => !packIds.Contains(p.Id.Value))
                    .OrderBy(p => p.Strings.Name.TranslatedString,
                             StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (game.Count > 0) {
                    Label header = new Label(new LocStrFormatted(
                        "Game research — " + game.Count + " node(s)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    list.Add(header);
                    rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                    foreach (ResearchNodeProto gp in game) {
                        ResearchNodeProto captured = gp;
                        string modTag = readModTag(captured);
                        string icon = researchIconPath(captured);

                        UiComponent row = buildOptionRow(
                            thumb: !string.IsNullOrEmpty(icon)
                                ? new Icon(icon).Size(OptionIconSize) : null,
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

        private ResearchDef findPackResearch(string id) {
            if (m_packModel?.Definitions == null) return null;
            foreach (DefBase d in m_packModel.Definitions) {
                if (d is ResearchDef rd && rd.ResearchId == id) return rd;
            }
            return null;
        }

        private ResearchNodeProto findGameResearch(string id) {
            if (m_protosDb == null) return null;
            Option<ResearchNodeProto> direct = m_protosDb.Get<ResearchNodeProto>(
                new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef)) {
                Option<ResearchNodeProto> byPath = m_protosDb.Get<ResearchNodeProto>(
                    new Proto.ID(viaTypedRef));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // If the stored value matches a name in the owner def's per-file
        // variable map, return the resolved id; otherwise null. Symmetric
        // with the variable-name-on-selection logic in pickInFileVariable.
        private string resolveVariableToId(string maybeVariableName) {
            if (string.IsNullOrEmpty(maybeVariableName)) return null;
            if (m_ownerVariables == null) return null;
            return m_ownerVariables.TryGetValue(maybeVariableName, out string id) ? id : null;
        }

        // Reverse lookup against the owner file's variable map: given a
        // research id, return the variable name that points at it (if any).
        // Restricted to research living in the same source file so we never
        // emit a bare reference that the runtime can't resolve in scope.
        private string pickInFileVariable(ResearchDef rd) {
            if (rd == null || m_ownerVariables == null) return null;
            if (string.IsNullOrEmpty(m_ownerSourceFile)) return null;
            if (!string.Equals(rd.SourceFile, m_ownerSourceFile,
                    StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            foreach (var kvp in m_ownerVariables) {
                if (kvp.Value == rd.ResearchId) return kvp.Key;
            }
            return null;
        }

        // ResearchNodeProto isn't IProtoWithIcon — icons live on Graphics.
        // Same lookup as ProtoPicker's iconPathOf callback for research.
        private static string researchIconPath(ResearchNodeProto node) {
            if (node == null) return null;
            if (!node.Graphics.Icons.IsEmpty) return node.Graphics.Icons[0];
            if (!node.Graphics.IconsProtos.IsEmpty) return node.Graphics.IconsProtos[0].IconPath;
            return null;
        }

        private static string readModTag(ResearchNodeProto proto) {
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
