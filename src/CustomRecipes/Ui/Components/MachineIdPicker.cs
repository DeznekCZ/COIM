using System;
using System.Collections.Generic;
using System.Linq;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Picker for a machine-id argument (<c>recipe.machine</c>,
    /// <c>edit_recipe.machine</c>, …). Two sources of options:
    ///
    ///   • <see cref="PackModel"/> — <see cref="BuildMachineDef"/> entries
    ///     in the current pack. When the owner def lives in the same source
    ///     file AND that file's variable map has a binding pointing at the
    ///     selected machine, the picker stores the variable name; otherwise
    ///     it stores the bare machine id. Round-tripping through the
    ///     emitter writes the variable form (bare identifier) when set so
    ///     the file matches the modder's authored style.
    ///
    ///   • <see cref="ProtosDb"/> — every registered
    ///     <see cref="MachineProto"/>. Stored as the bare id.
    ///
    /// Modeled on <see cref="ResearchIdPicker"/>: DisplayRowWithButton
    /// trigger, FloatingColumn popup with header search, grouped pack-then-
    /// game sections, ButtonColumn rows so chip + title + sub-label stack
    /// cleanly without crowding.
    /// </summary>
    public sealed class MachineIdPicker : DisplayRowWithButton {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        // Owner-def accessor is a callback so the picker can be constructed
        // once at editor-build time and still see the currently-bound def
        // when it needs to resolve variable bindings. Mirrors the
        // AssetPathPicker pattern — the editor surfaces stable picker
        // instances and the bound value (re-read through this lambda)
        // supplies the per-call context. Null is allowed: the picker still
        // works against pack + game machines, just without variable-
        // binding support.
        private readonly Func<DefBase> m_getOwnerDef;
        private readonly Func<string> m_getId;
        private readonly Action<string> m_setId;
        private readonly LocStrFormatted m_title;
        private readonly LocStrFormatted m_emptyLabel;
        private readonly bool m_allowNone;

        private readonly Column m_buttonContent;
        private FloatingColumn m_popup;
        private TextField m_popupSearch;

        public MachineIdPicker(
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
            m_title       = title ?? new LocStrFormatted("Pick machine");
            m_emptyLabel  = emptyLabel ?? new LocStrFormatted("(pick a machine...)");
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

            // Resolve order: in-file variable, then pack machine id, then
            // game machine id (with typed-ref fallback). Mirrors the
            // emitter so what we display is what gets written back.
            string resolved = resolveVariableToId(current) ?? current;
            BuildMachineDef pack = findPackMachine(resolved);
            if (pack != null) {
                renderPackTrigger(pack, current);
                return;
            }
            MachineProto game = findGameMachine(resolved);
            if (game != null) {
                renderGameTrigger(game, current);
                return;
            }
            m_buttonContent.Add(new Label(new LocStrFormatted(current))
                .Class(Cls.fontMonospace));
        }

        private void renderPackTrigger(BuildMachineDef def, string storedId) {
            Row content = new Row();
            Column labelStack = new Column {
                new Label(new LocStrFormatted(def.Name ?? def.MachineId ?? "")).Fill(),
                new Label(new LocStrFormatted(storedId ?? def.MachineId ?? "")).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Add(new Label(new LocStrFormatted("(this pack)")).TinyFontSize());
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        private void renderGameTrigger(MachineProto proto, string storedId) {
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

            // Pack machines first — modders are likely to reference a
            // machine they just defined in their own pack.
            List<BuildMachineDef> pack = m_packModel?.Definitions?
                .OfType<BuildMachineDef>()
                .Where(b => !string.IsNullOrEmpty(b.MachineId))
                .OrderBy(b => b.MachineId, StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<BuildMachineDef>();
            HashSet<string> packIds = new HashSet<string>(
                pack.Select(b => b.MachineId), StringComparer.Ordinal);

            if (pack.Count > 0) {
                Label header = new Label(new LocStrFormatted(
                    "This pack — " + pack.Count + " machine(s)"));
                header.FontBold().PaddingTopBottom(2.pt());
                list.Add(header);
                rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                foreach (BuildMachineDef bm in pack) {
                    BuildMachineDef captured = bm;
                    // Same in-file variable rule the ResearchIdPicker uses:
                    // when owner + this BuildMachineDef live in the same
                    // source file AND the file has a variable bound to
                    // captured.MachineId, prefer that variable name so the
                    // emitter writes it as a bare identifier. Cross-file
                    // references can't use the local variable name — bare id
                    // is the only correct form there.
                    string sameFileVar = pickInFileVariable(captured);
                    string storeAs = sameFileVar ?? captured.MachineId;
                    string chip = sameFileVar != null ? "(var " + sameFileVar + ")" : "(this pack)";

                    UiComponent row = buildOptionRow(
                        thumb: null,
                        title: captured.Name ?? captured.MachineId,
                        subtitle: captured.MachineId,
                        chip: chip,
                        onClick: () => selectValue(storeAs));
                    list.Add(row);
                    rows.Add(new KeyValuePair<UiComponent, string>(row,
                        ((captured.Name ?? "") + " " + captured.MachineId + " "
                            + (sameFileVar ?? "")).ToLowerInvariant()));
                }
            }

            // Game machines from ProtosDb — skip ids already in the pack
            // section so the same machine doesn't show twice.
            if (m_protosDb != null) {
                List<MachineProto> game = m_protosDb.All<MachineProto>()
                    .Where(p => !packIds.Contains(p.Id.Value))
                    .OrderBy(p => p.Strings.Name.TranslatedString,
                             StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (game.Count > 0) {
                    Label header = new Label(new LocStrFormatted(
                        "Game machines — " + game.Count + " machine(s)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    list.Add(header);
                    rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                    foreach (MachineProto gp in game) {
                        MachineProto captured = gp;
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

        private BuildMachineDef findPackMachine(string id) {
            if (m_packModel?.Definitions == null) return null;
            foreach (DefBase d in m_packModel.Definitions) {
                if (d is BuildMachineDef bm && bm.MachineId == id) return bm;
            }
            return null;
        }

        private MachineProto findGameMachine(string id) {
            if (m_protosDb == null) return null;
            Option<MachineProto> direct = m_protosDb.Get<MachineProto>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef)) {
                Option<MachineProto> byPath = m_protosDb.Get<MachineProto>(
                    new Proto.ID(viaTypedRef));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // If the stored value matches a name in the current owner def's
        // per-file variable map, return the resolved id; otherwise null.
        // Re-reads m_getOwnerDef each call so a Value() swap on the
        // hosting editor picks up the new file's variables.
        private string resolveVariableToId(string maybeVariableName) {
            if (string.IsNullOrEmpty(maybeVariableName)) return null;
            DefBase owner = m_getOwnerDef?.Invoke();
            Dictionary<string, string> vars = owner?.SourceFileVariables;
            if (vars == null) return null;
            return vars.TryGetValue(maybeVariableName, out string id) ? id : null;
        }

        // Reverse lookup against the current owner file's variable map:
        // given a machine id, return the variable name that points at it
        // (if any). Restricted to machines living in the same source file
        // so we never emit a bare reference that the runtime can't resolve
        // in scope.
        private string pickInFileVariable(BuildMachineDef bm) {
            if (bm == null) return null;
            DefBase owner = m_getOwnerDef?.Invoke();
            Dictionary<string, string> vars = owner?.SourceFileVariables;
            if (vars == null) return null;
            string ownerSourceFile = owner.SourceFile;
            if (string.IsNullOrEmpty(ownerSourceFile)) return null;
            if (!string.Equals(bm.SourceFile, ownerSourceFile,
                    StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            foreach (var kvp in vars) {
                if (kvp.Value == bm.MachineId) return kvp.Key;
            }
            return null;
        }

        private static string readModTag(MachineProto proto) {
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
