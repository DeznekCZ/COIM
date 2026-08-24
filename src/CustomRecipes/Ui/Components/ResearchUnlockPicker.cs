using System;
using System.Collections.Generic;
using System.Linq;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.UnlockingTree;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Picker for <c>remove_unlock.target</c>. Unlike every other id picker in
    /// the editor it does NOT browse the whole prototype database: the only
    /// things worth removing from a research node are the ones that node
    /// currently unlocks, so the options ARE that node's unlock list.
    ///
    /// Two sources, both keyed off whichever research the def currently names:
    ///
    ///   • <see cref="ProtosDb"/> — the live node's <c>Units</c>, i.e. what the
    ///     game had after the last load (vanilla unlocks plus whatever any
    ///     loaded pack already added).
    ///
    ///   • <see cref="PackModel"/> — <c>add_unlock_*</c> definitions in THIS
    ///     pack pointing at the same node. They may not be in the protos db yet
    ///     (a def added since the last game start), and a pack that adds an
    ///     unlock in one file and removes it in another is a real thing.
    ///
    /// Picking a recipe unlock also writes the machine it was unlocked on, so
    /// the emitted call is scoped to that one (recipe, machine) pair rather
    /// than dropping the recipe from every machine the node lists it under.
    /// </summary>
    public sealed class ResearchUnlockPicker : DisplayRowWithButton {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        private readonly Func<DefBase> m_getOwnerDef;
        private readonly Func<string> m_getResearchId;
        private readonly Func<string> m_getTargetId;
        private readonly Action<string> m_setTargetId;
        private readonly Action<string> m_setMachineId;
        private readonly Action m_onSelected;
        private readonly LocStrFormatted m_title;
        private readonly LocStrFormatted m_emptyLabel;

        private readonly Column m_buttonContent;
        private FloatingColumn m_popup;
        private TextField m_popupSearch;
        private ScrollColumn m_list;
        private Label m_status;
        private bool m_suppressFilterEvent;

        /// Status line shown instead of the match count when there is nothing to
        /// count (no research picked yet, node not registered, node empty). Set
        /// by <see cref="buildEntries"/>, read by <see cref="renderEntries"/>.
        private string m_statusOverride;

        /// <param name="setMachineId">Receives the machine a picked recipe
        /// unlock was bound to, or null for every other unlock kind — a product
        /// or entity unlock has no machine to scope by, and leaving a stale one
        /// behind would silently narrow the removal to nothing.</param>
        /// <param name="onSelected">Fired after a pick so the form can refresh
        /// the machine field, which this picker writes to as a side effect.</param>
        public ResearchUnlockPicker(
                PackModel packModel,
                ProtosDb protosDb,
                Func<DefBase> getOwnerDef,
                Func<string> getResearchId,
                Func<string> getTargetId,
                Action<string> setTargetId,
                Action<string> setMachineId,
                Action onSelected = null,
                LocStrFormatted? title = null,
                LocStrFormatted? emptyLabel = null)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_packModel     = packModel;
            m_protosDb      = protosDb;
            m_getOwnerDef   = getOwnerDef;
            m_getResearchId = getResearchId;
            m_getTargetId   = getTargetId;
            m_setTargetId   = setTargetId;
            m_setMachineId  = setMachineId;
            m_onSelected    = onSelected;
            m_title         = title ?? new LocStrFormatted("Pick what to remove");
            m_emptyLabel    = emptyLabel ?? new LocStrFormatted("(pick what to remove…)");

            Row.ClassRemove(Cls.displayFont);
            m_buttonContent = new Column();
            Row.Add(m_buttonContent);

            Btn.OnClick(openPopup);
            RefreshDisplay();
        }

        public void RefreshDisplay() {
            m_buttonContent.Clear();
            string current = m_getTargetId();
            if (string.IsNullOrEmpty(current)) {
                m_buttonContent.Add(new Label(m_emptyLabel));
                return;
            }

            string resolved = resolveVariableToId(current) ?? current;
            Proto proto = findProto(resolved);
            Row content = new Row();
            string icon = iconPathOf(proto);
            if (!string.IsNullOrEmpty(icon)) {
                content.Add(new Icon(icon).Size(ButtonIconSize));
            }
            Column labelStack = new Column {
                new Label(new LocStrFormatted(displayNameOf(proto) ?? resolved)).Fill(),
                new Label(new LocStrFormatted(current)).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        // ---- Popup ----------------------------------------------------------

        private void openPopup() {
            if (m_popup == null) {
                buildPopupShell();
            }
            // Rebuilt per open — the def's research argument can change between
            // openings, and the whole option list hangs off it.
            List<Entry> entries = buildEntries();

            m_suppressFilterEvent = true;
            try {
                m_popupSearch?.Text("");
            } finally {
                m_suppressFilterEvent = false;
            }
            renderEntries(entries, "");
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
                .Placeholder(new LocStrFormatted("search by id or name…"));
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
                renderEntries(buildEntries(), v);
            });
            search.FocusOnShow();
        }

        // A research node lists at most a few dozen unlocks, so this renders in
        // one pass — none of the chunked streaming EntityIdPicker needs for its
        // several-thousand-proto list.
        private void renderEntries(List<Entry> entries, string rawNeedle) {
            if (m_list == null) {
                return;
            }
            m_list.Clear();

            string needle = (rawNeedle ?? "").Trim().ToLowerInvariant();
            List<Entry> shown = needle.Length == 0
                ? entries
                : entries.Where(e => e.Haystack != null && e.Haystack.Contains(needle)).ToList();

            string lastSection = null;
            foreach (Entry entry in shown) {
                if (entry.Section != null && entry.Section != lastSection) {
                    lastSection = entry.Section;
                    m_list.Add(new Label(new LocStrFormatted(entry.Section))
                        .FontBold().PaddingTopBottom(2.pt()));
                }
                Entry captured = entry;
                m_list.Add(buildOptionRow(
                    thumb: !string.IsNullOrEmpty(captured.IconPath)
                        ? new Icon(captured.IconPath).Size(OptionIconSize) : null,
                    title: captured.Title,
                    subtitle: captured.Subtitle,
                    chip: captured.Chip,
                    onClick: () => selectValue(captured.StoreAs, captured.MachineId)));
            }

            if (m_status != null) {
                m_status.Value(new LocStrFormatted(
                    m_statusOverride ?? (shown.Count + " unlock(s)")));
            }
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

        private void selectValue(string targetId, string machineId) {
            m_popup?.Close();
            m_setTargetId(targetId);
            // Always written, including the null case: a leftover machine from a
            // previously picked recipe unlock would scope a product removal to a
            // machine and quietly match nothing.
            m_setMachineId?.Invoke(machineId);
            RefreshDisplay();
            m_onSelected?.Invoke();
        }

        // ---- Option data -----------------------------------------------------

        private List<Entry> buildEntries() {
            List<Entry> entries = new List<Entry>();
            m_statusOverride = null;

            string researchId = resolveResearchId();
            if (string.IsNullOrEmpty(researchId)) {
                m_statusOverride = "pick the research node first — its unlocks are the options";
                return entries;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            ResearchNodeProto node = findGameResearch(researchId);
            if (node != null) {
                foreach (IUnlockNodeUnit unit in node.Units) {
                    try {
                        Entry entry = entryForUnit(unit);
                        if (entry == null || !seen.Add(entry.StoreAs + "@" + (entry.MachineId ?? ""))) {
                            continue;
                        }
                        entry.Section = "Currently unlocked by " + researchId;
                        entries.Add(entry);
                    } catch (Exception ex) {
                        Log.Warning("ResearchUnlockPicker: skipping an unreadable unlock on '"
                            + researchId + "' — " + ex.GetType().Name + ": " + ex.Message);
                    }
                }
            }

            foreach (Entry packEntry in packAdditionsFor(researchId)) {
                if (!seen.Add(packEntry.StoreAs + "@" + (packEntry.MachineId ?? ""))) {
                    continue;
                }
                entries.Add(packEntry);
            }

            if (entries.Count == 0) {
                m_statusOverride = node == null
                    ? "'" + researchId + "' is not registered yet — nothing to list"
                    : "'" + researchId + "' unlocks nothing";
            }
            return entries;
        }

        // The one thing a unit puts on the node. Deliberately not
        // ProtoUnlock.UnlockedProtos: a RecipeUnlock lists every ingredient and
        // product of its recipe there, which would offer a recipe's inputs as
        // removable "unlocks" they never were.
        private Entry entryForUnit(IUnlockNodeUnit unit) {
            switch (unit) {
                case RecipeUnlock recipeUnlock: {
                    string id = recipeUnlock.Proto.Id.Value;
                    string machine = recipeUnlock.MachineProto.Id.Value;
                    return new Entry {
                        StoreAs   = id,
                        MachineId = machine,
                        Title     = displayNameOf(recipeUnlock.Proto) ?? id,
                        Subtitle  = id,
                        Chip      = "(recipe on " + machine + ")",
                        IconPath  = iconPathOf(recipeUnlock.MachineProto),
                        Haystack  = ((displayNameOf(recipeUnlock.Proto) ?? "") + " " + id + " "
                            + machine + " recipe").ToLowerInvariant(),
                    };
                }
                case ProductUnlock productUnlock:
                    return entryForProto(productUnlock.Proto, "product");
                case ProtoWithIconUnlock protoUnlock:
                    return entryForProto(protoUnlock.Proto as Proto, "entity");
                case ProtoUnlock plain:
                    // Plain ProtoUnlock(proto) — a single entry, and it IS the
                    // target. Anything wider is a unit we can't name, so skip it
                    // rather than guess.
                    return plain.UnlockedProtos.Length == 1
                        ? entryForProto(plain.UnlockedProtos[0] as Proto, "unlock")
                        : null;
                default:
                    // Units that unlock no proto at all (vehicle-limit increases
                    // and friends) — remove_unlock has no way to name them.
                    return null;
            }
        }

        private Entry entryForProto(Proto proto, string kindLabel) {
            if (proto == null) {
                return null;
            }
            string id = proto.Id.Value;
            string name = displayNameOf(proto) ?? id;
            return new Entry {
                StoreAs  = id,
                Title    = name,
                Subtitle = id,
                Chip     = "(" + kindLabel + ")",
                IconPath = iconPathOf(proto),
                Haystack = (name + " " + id + " " + kindLabel).ToLowerInvariant(),
            };
        }

        // add_unlock_* definitions in this pack that target the same node. Their
        // effect may not be in the protos db yet (added since the last game
        // start), and removing something the same pack adds elsewhere is a
        // legitimate ordering trick.
        private List<Entry> packAdditionsFor(string researchId) {
            List<Entry> entries = new List<Entry>();
            if (m_packModel?.Definitions == null) {
                return entries;
            }
            foreach (DefBase def in m_packModel.Definitions) {
                string defResearch = null;
                string target = null;
                string machine = null;
                string kind = null;
                if (def is UnlockRecipeDef ur) {
                    defResearch = ur.ResearchId;
                    target = ur.RecipeId;
                    machine = ur.MachineId;
                    kind = "recipe";
                } else if (def is UnlockProductDef up) {
                    defResearch = up.ResearchId;
                    target = up.ProductId;
                    kind = "product";
                } else if (def is UnlockMachineDef um) {
                    defResearch = um.ResearchId;
                    target = um.MachineId;
                    kind = "machine";
                } else if (def is UnlockEntityDef ue) {
                    defResearch = ue.ResearchId;
                    target = ue.EntityId;
                    kind = "entity";
                }
                if (target == null) {
                    continue;
                }
                string resolvedResearch = resolveVariableFor(def, defResearch) ?? defResearch;
                if (!string.Equals(resolvedResearch, researchId, StringComparison.Ordinal)) {
                    continue;
                }
                string resolvedTarget = resolveVariableFor(def, target) ?? target;
                Proto proto = findProto(resolvedTarget);
                entries.Add(new Entry {
                    StoreAs   = resolvedTarget,
                    MachineId = machine == null ? null : (resolveVariableFor(def, machine) ?? machine),
                    Title     = displayNameOf(proto) ?? resolvedTarget,
                    Subtitle  = resolvedTarget,
                    Chip      = "(this pack adds it as " + kind + ")",
                    IconPath  = iconPathOf(proto),
                    Section   = "Added by this pack",
                    Haystack  = (resolvedTarget + " " + kind + " "
                        + (displayNameOf(proto) ?? "")).ToLowerInvariant(),
                });
            }
            return entries;
        }

        // ---- Lookups --------------------------------------------------------

        /// The research the def points at, as a bare id: through the owner
        /// file's variable map first (the argument is routinely a variable), then
        /// through the typed-ref resolver.
        private string resolveResearchId() {
            string raw = m_getResearchId?.Invoke();
            if (string.IsNullOrEmpty(raw)) {
                return null;
            }
            string viaVariable = resolveVariableToId(raw);
            if (!string.IsNullOrEmpty(viaVariable)) {
                return viaVariable;
            }
            string viaTypedRef = TypedRefResolver.ResolveOrNull(raw);
            return string.IsNullOrEmpty(viaTypedRef) ? raw : viaTypedRef;
        }

        private ResearchNodeProto findGameResearch(string id) {
            if (m_protosDb == null || string.IsNullOrEmpty(id)) {
                return null;
            }
            Option<ResearchNodeProto> direct = m_protosDb.Get<ResearchNodeProto>(new Proto.ID(id));
            return direct.HasValue ? direct.Value : null;
        }

        private Proto findProto(string id) {
            if (m_protosDb == null || string.IsNullOrEmpty(id)) {
                return null;
            }
            Option<Proto> direct = m_protosDb.Get<Proto>(new Proto.ID(id));
            if (direct.HasValue) {
                return direct.Value;
            }
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef)) {
                Option<Proto> byPath = m_protosDb.Get<Proto>(new Proto.ID(viaTypedRef));
                if (byPath.HasValue) {
                    return byPath.Value;
                }
            }
            return null;
        }

        private string resolveVariableToId(string maybeVariableName) {
            return resolveVariableFor(m_getOwnerDef?.Invoke(), maybeVariableName);
        }

        private static string resolveVariableFor(DefBase def, string maybeVariableName) {
            if (def == null || string.IsNullOrEmpty(maybeVariableName)) {
                return null;
            }
            Dictionary<string, string> vars = def.SourceFileVariables;
            if (vars == null) {
                return null;
            }
            return vars.TryGetValue(maybeVariableName, out string id) ? id : null;
        }

        private static string iconPathOf(IProto proto) {
            try {
                if (proto is IProtoWithIcon withIcon) {
                    return withIcon.IconPath;
                }
            } catch (Exception) {
                // An unreadable icon must never cost us the row itself.
            }
            return null;
        }

        // Proto.Strings is populated for everything player-facing, but this
        // picker also sees internal protos and protos other mods registered —
        // fall back rather than throw.
        private static string displayNameOf(IProto proto) {
            if (proto == null) {
                return null;
            }
            try {
                string translated = proto.Strings.Name.TranslatedString;
                return string.IsNullOrEmpty(translated) ? null : translated;
            } catch (Exception) {
                return null;
            }
        }

        /// One render-ready option. Strings only, so rebuilding the list on
        /// every popup open (and every keystroke) stays cheap.
        private sealed class Entry {
            public string StoreAs;
            /// Machine to scope a recipe removal by; null for every other kind.
            public string MachineId;
            public string Title;
            public string Subtitle;
            public string Chip;
            public string IconPath;
            public string Section;
            public string Haystack;
        }
    }
}
