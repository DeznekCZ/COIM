using System;
using System.Collections.Generic;
using System.Linq;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Entities;
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
    ///
    /// By default the game-side list is restricted to
    /// <see cref="LayoutEntityProto"/> — that's what a toolbar category can
    /// hold. Pass <c>includeAllEntities</c> to widen it to every
    /// <see cref="EntityProto"/>, which additionally surfaces the dynamic
    /// entities (trucks, excavators, locomotives, cargo wagons, ships).
    /// <c>edit_entity_costs</c> uses the wide form since <c>Costs</c> lives
    /// on the shared base and applies to all of them.
    /// </summary>
    public sealed class EntityIdPicker : DisplayRowWithButton {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        /// Rows materialised per scheduler tick. Small enough that a tick stays
        /// well inside a frame, large enough that a few thousand entities finish
        /// streaming in well under a second.
        private const int RenderChunkSize = 40;

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        private readonly Func<DefBase> m_getOwnerDef;
        private readonly Func<string> m_getId;
        private readonly Action<string> m_setId;
        private readonly LocStrFormatted m_title;
        private readonly LocStrFormatted m_emptyLabel;
        private readonly bool m_allowNone;
        private readonly bool m_includeAllEntities;

        private readonly Column m_buttonContent;
        private FloatingColumn m_popup;
        private TextField m_popupSearch;
        private ScrollColumn m_list;
        private Label m_status;

        /// Every candidate, rebuilt on each popup open. Data only — no UI.
        private List<Entry> m_entries = new List<Entry>();
        /// The subset matching the current search box contents.
        private List<Entry> m_filtered = new List<Entry>();
        /// How much of <see cref="m_filtered"/> has been turned into rows.
        private int m_renderedCount;
        /// Section header most recently emitted, so headers are written once as
        /// the chunks stream past a section boundary.
        private string m_lastSection;
        /// In-flight chunked render, paused when the filter changes or finishes.
        private UnityEngine.UIElements.IVisualElementScheduledItem m_fill;
        /// Guards the search field's change event while we reset its text
        /// ourselves, so reopening doesn't filter twice.
        private bool m_suppressFilterEvent;

        public EntityIdPicker(
                PackModel packModel,
                ProtosDb protosDb,
                Func<string> getId,
                Action<string> setId,
                Func<DefBase> getOwnerDef = null,
                LocStrFormatted? title = null,
                LocStrFormatted? emptyLabel = null,
                bool allowNone = false,
                bool includeAllEntities = false)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_packModel   = packModel;
            m_protosDb    = protosDb;
            m_getOwnerDef = getOwnerDef;
            m_getId       = getId;
            m_setId       = setId;
            m_title       = title ?? new LocStrFormatted("Pick entity");
            m_emptyLabel  = emptyLabel ?? new LocStrFormatted("(pick an entity...)");
            m_allowNone   = allowNone;
            m_includeAllEntities = includeAllEntities;

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
            EntityProto game = findGameEntity(resolved);
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

        private void renderGameTrigger(EntityProto proto, string storedId) {
            Row content = new Row();
            string iconPath = iconPathOf(proto);
            if (!string.IsNullOrEmpty(iconPath)) {
                content.Add(new Icon(iconPath).Size(ButtonIconSize));
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
        //
        // The option list is DATA first, UI second. An earlier version built one
        // ButtonColumn (plus an Icon, which pulls a sprite through AssetsDb) for
        // every candidate up front — fine for the couple of hundred layout
        // entities the toolbar editor picks from, far too slow once
        // includeAllEntities widened the pool to every EntityProto in the game.
        // Filtering made it worse: it toggled Visible on every prebuilt row, and
        // SetVisible walks the row's whole subtree, so each keystroke swept
        // thousands of elements.
        //
        // Now: collect lightweight Entry records once per open (no UI, no
        // sprites), then materialise rows for the CURRENT filter only, a chunk
        // per frame via the UI Toolkit scheduler. The popup is usable on the
        // first frame and fills in behind you, and a filter change just cancels
        // the in-flight fill and starts a new one.

        private void openPopup() {
            if (m_popup == null) {
                buildPopupShell();
            }
            // Rebuilt per open: the pack model can gain or lose definitions
            // between openings, and this is cheap precisely because it makes no
            // UI.
            m_entries = buildEntries();

            m_suppressFilterEvent = true;
            try {
                m_popupSearch?.Text("");
            } finally {
                m_suppressFilterEvent = false;
            }
            applyFilter("");
            m_popup.Open(Btn);
        }

        private void buildPopupShell() {
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

            m_status = new Label(new LocStrFormatted("")).TinyFontSize().Color(ColorRgba.LightGray);
            panel.Header.Add(m_status);

            m_list = new ScrollColumn();
            m_list.Gap(1.pt()).MaxHeight(480.px()).AlignItemsStretch();
            panel.BodyAdd(m_list);

            search.OnValueChanged(v => {
                if (m_suppressFilterEvent) {
                    return;
                }
                applyFilter(v);
            });
            search.FocusOnShow();
        }

        // ---- Option data -----------------------------------------------------

        /// Flatten every candidate into a render-ready record. Deliberately does
        /// NOT touch the UI: this runs on every popup open, and the whole point
        /// is that it stays cheap enough to do so.
        private List<Entry> buildEntries() {
            List<Entry> entries = new List<Entry>();

            if (m_allowNone) {
                entries.Add(new Entry {
                    StoreAs  = null,
                    Title    = "(none)",
                    Subtitle = "clear the current selection",
                    Haystack = "none clear empty remove",
                });
            }

            // Pack entities first — modders often want to assign a category to a
            // building they just defined.
            List<NamedDef> pack = collectPackEntities();
            HashSet<string> packIds = new HashSet<string>(
                pack.Select(d => d.Id), StringComparer.Ordinal);

            foreach (NamedDef def in pack) {
                string sameFileVar = pickInFileVariable(def);
                string chip = sameFileVar != null
                    ? "(var " + sameFileVar + " · " + def.Kind + ")"
                    : "(" + def.Kind + ")";
                entries.Add(new Entry {
                    StoreAs  = sameFileVar ?? def.Id,
                    Title    = def.Name ?? def.Id,
                    Subtitle = def.Id,
                    Chip     = chip,
                    Section  = "This pack",
                    Haystack = ((def.Name ?? "") + " " + def.Id + " " + def.Kind + " "
                        + (sameFileVar ?? "")).ToLowerInvariant(),
                });
            }

            if (m_protosDb == null) {
                return entries;
            }

            // Game entities. Each one is read inside a try/catch: a single proto
            // with unexpected Strings or Mod metadata used to be able to throw
            // mid-loop and silently truncate everything after it, which looks
            // exactly like "the list is missing entries".
            IEnumerable<EntityProto> candidates = m_includeAllEntities
                ? m_protosDb.All<EntityProto>()
                : m_protosDb.All<LayoutEntityProto>().Cast<EntityProto>();

            List<Entry> gameEntries = new List<Entry>();
            foreach (EntityProto proto in candidates) {
                try {
                    if (proto == null || packIds.Contains(proto.Id.Value)) {
                        continue;
                    }
                    string id = proto.Id.Value;
                    string name = displayNameOf(proto) ?? id;
                    string modTag = readModTag(proto);
                    gameEntries.Add(new Entry {
                        StoreAs  = id,
                        Title    = name,
                        Subtitle = id,
                        Chip     = modTag,
                        IconPath = iconPathOf(proto),
                        Section  = "Game entities",
                        Haystack = (name + " " + id + " " + (modTag ?? "")).ToLowerInvariant(),
                    });
                } catch (Exception ex) {
                    Log.Warning("EntityIdPicker: skipping a proto that could not be read — "
                        + ex.GetType().Name + ": " + ex.Message);
                }
            }
            gameEntries.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
            entries.AddRange(gameEntries);
            return entries;
        }

        // Proto.Strings is populated by the game for everything player-facing,
        // but this picker also sees internal protos — fall back rather than
        // throw, so one odd entry can't take the list down with it.
        private static string displayNameOf(EntityProto proto) {
            try {
                string translated = proto.Strings.Name.TranslatedString;
                return string.IsNullOrEmpty(translated) ? null : translated;
            } catch (Exception) {
                return null;
            }
        }

        // ---- Incremental rendering -------------------------------------------

        private void applyFilter(string rawNeedle) {
            if (m_list == null) {
                return;
            }
            m_fill?.Pause();
            m_fill = null;
            m_list.Clear();
            m_renderedCount = 0;
            m_lastSection = null;

            string needle = (rawNeedle ?? "").Trim().ToLowerInvariant();
            m_filtered = needle.Length == 0
                ? m_entries
                : m_entries.Where(e => e.Haystack != null && e.Haystack.Contains(needle)).ToList();

            // First chunk synchronously so the popup is never empty on the frame
            // it opens; the rest streams in.
            renderNextChunk();
            if (m_renderedCount < m_filtered.Count) {
                m_fill = m_list.RootElement.schedule.Execute(() => {
                    renderNextChunk();
                    if (m_renderedCount >= m_filtered.Count) {
                        m_fill?.Pause();
                        m_fill = null;
                    }
                }).Every(1);
            }
            updateStatus();
        }

        private void renderNextChunk() {
            int end = Math.Min(m_renderedCount + RenderChunkSize, m_filtered.Count);
            for (int i = m_renderedCount; i < end; i++) {
                Entry entry = m_filtered[i];
                if (entry.Section != null && entry.Section != m_lastSection) {
                    m_lastSection = entry.Section;
                    Label header = new Label(new LocStrFormatted(
                        entry.Section + " — " + countInSection(entry.Section) + " entity(ies)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    m_list.Add(header);
                }
                Entry captured = entry;
                m_list.Add(buildOptionRow(
                    thumb: !string.IsNullOrEmpty(captured.IconPath)
                        ? new Icon(captured.IconPath).Size(OptionIconSize) : null,
                    title: captured.Title,
                    subtitle: captured.Subtitle,
                    chip: captured.Chip,
                    onClick: () => selectValue(captured.StoreAs)));
            }
            m_renderedCount = end;
            updateStatus();
        }

        private int countInSection(string section) {
            int count = 0;
            for (int i = 0; i < m_filtered.Count; i++) {
                if (m_filtered[i].Section == section) {
                    count++;
                }
            }
            return count;
        }

        // Always states the true match count, so a list still streaming in reads
        // as "loading" rather than as "these are all the entities there are".
        private void updateStatus() {
            if (m_status == null) {
                return;
            }
            int total = m_filtered?.Count ?? 0;
            string text = m_renderedCount >= total
                ? total + " match(es)"
                : "showing " + m_renderedCount + " of " + total + "… (keep typing to narrow)";
            m_status.Value(new LocStrFormatted(text));
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
                        || d is FarmDef
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

        private EntityProto findGameEntity(string id) {
            if (m_protosDb == null) return null;
            Option<EntityProto> direct = m_protosDb.Get<EntityProto>(new Proto.ID(id));
            if (direct.HasValue && isSelectable(direct.Value)) return direct.Value;
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef)) {
                Option<EntityProto> byPath = m_protosDb.Get<EntityProto>(
                    new Proto.ID(viaTypedRef));
                if (byPath.HasValue && isSelectable(byPath.Value)) return byPath.Value;
            }
            return null;
        }

        // Narrow-mode pickers only ever store a LayoutEntityProto id, so a
        // dynamic entity resolved by id must not render as a valid selection
        // there — otherwise the trigger would show a truck for a field that
        // cannot hold one.
        private bool isSelectable(EntityProto proto) {
            return m_includeAllEntities || proto is LayoutEntityProto;
        }

        // IconPath is declared per-family rather than on EntityProto — layout
        // entities expose it directly, dynamic ones via IProtoWithIcon — so go
        // through the interface and accept that some protos simply have none.
        private static string iconPathOf(EntityProto proto) {
            if (proto is IProtoWithIcon withIcon) return withIcon.IconPath;
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

        private static string readModTag(EntityProto proto) {
            if (proto?.Mod == null) return null;
            var manifest = proto.Mod.Manifest;
            if (manifest == null) return null;
            string label = !string.IsNullOrEmpty(manifest.DisplayName)
                ? manifest.DisplayName
                : manifest.Id;
            return string.IsNullOrEmpty(label) ? null : "(" + label + ")";
        }

        /// One render-ready option. Holds only strings, so building the full
        /// candidate list costs nothing compared with building its UI.
        private sealed class Entry {
            public string StoreAs;
            public string Title;
            public string Subtitle;
            public string Chip;
            public string IconPath;
            /// Group header this entry sits under; null for the "(none)" row.
            public string Section;
            /// Pre-lowercased search text.
            public string Haystack;
        }
    }
}
