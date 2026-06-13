using System;
using System.Collections.Generic;
using System.Linq;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Picker for an <c>add_unlock_recipe</c>'s recipe-id argument.
    ///
    /// Recipes come from two sources:
    ///   • The current <see cref="PackModel"/> — recipes the modder is
    ///     authoring or has already authored in this pack. Stored on selection
    ///     as the bare recipe id (<c>"MyMod_BatteryAssembly"</c>) so the
    ///     emitter renders a plain string literal.
    ///   • The game's <see cref="ProtosDb"/> — every registered
    ///     <see cref="RecipeProto"/>. Stored on selection as the dotted
    ///     typed-ref path (<c>Ids.Recipes.X</c>) when one exists under
    ///     <c>Mafi.Base.Ids.Recipes</c>, so the emitter renders a Python
    ///     expression that resolves to the canonical id at pack-load time.
    ///     Falls back to the bare id when no typed-ref is registered (e.g.
    ///     recipes added by other mods that don't expose Ids constants).
    ///
    /// Visually mirrors <see cref="ProtoPicker{T}"/> for consistency with the
    /// rest of the editor: a DisplayRowWithButton trigger, FloatingColumn
    /// popup with PanelWithHeader, search field, two grouped sections.
    /// </summary>
    public sealed class RecipeIdPicker : DisplayRowWithButton {

        private const string IdsRecipesPrefix = "Ids.Recipes";
        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        private readonly Func<string> m_getId;
        private readonly Action<string> m_setId;
        private readonly Func<string, string> m_variableResolver;
        private readonly LocStrFormatted m_title;

        private readonly Column m_buttonContent;
        private FloatingColumn m_popup;
        private TextField m_popupSearch;

        public RecipeIdPicker(
                PackModel packModel,
                ProtosDb protosDb,
                Func<string> getId,
                Action<string> setId,
                LocStrFormatted? title = null,
                Func<string, string> variableResolver = null)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_packModel        = packModel;
            m_protosDb         = protosDb;
            m_getId            = getId;
            m_setId            = setId;
            m_title            = title ?? new LocStrFormatted("Pick recipe");
            m_variableResolver = variableResolver;

            Row.ClassRemove(Cls.displayFont);
            m_buttonContent = new Column();
            Row.Add(m_buttonContent);

            Btn.OnClick(openPopup);
            RefreshDisplay();
        }

        /// Refresh the trigger's content to match the currently-bound value.
        public void RefreshDisplay() {
            m_buttonContent.Clear();
            string current = m_getId();
            if (string.IsNullOrEmpty(current)) {
                m_buttonContent.Add(new Label(new LocStrFormatted("(pick recipe…)")));
                return;
            }

            // Resolve into a display name. Order: modded recipe in PackModel,
            // game recipe in ProtosDb (after typed-ref / variable resolution),
            // raw fallback to the current string for unknown ids.
            RecipeDef modded = findModdedRecipe(current);
            if (modded != null) {
                renderModdedTrigger(modded);
                return;
            }
            RecipeProto game = findGameRecipe(current);
            if (game != null) {
                renderGameTrigger(game, current);
                return;
            }
            renderRawTrigger(current);
        }

        // Trigger keeps the compact two-line shape (name on top, id below)
        // so the row fits on one editor line next to the field label. The
        // full RecipeUi preview lives in the popup, not the trigger — the
        // trigger's parent (DisplayRowWithButton) lays children
        // horizontally, so stacking the preview inside would squeeze it onto
        // the same line as the title and the result is unreadable.
        private void renderModdedTrigger(RecipeDef def) {
            Row content = new Row();
            Column labelStack = new Column {
                new Label(new LocStrFormatted(def.Name ?? def.RecipeId ?? "")).Fill(),
                new Label(new LocStrFormatted(def.RecipeId ?? "")).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Add(new Label(new LocStrFormatted("(this pack)")).TinyFontSize());
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        private void renderGameTrigger(RecipeProto proto, string storedId) {
            Row content = new Row();
            if (proto is IProtoWithIcon ip && !string.IsNullOrEmpty(ip.IconPath)) {
                content.Add(new Icon(ip.IconPath).Size(ButtonIconSize));
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

        private void renderRawTrigger(string current) {
            m_buttonContent.Add(new Label(new LocStrFormatted(current))
                .Class(Cls.fontMonospace));
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

            // (rowComponent, lowercased haystack) — null haystack on section
            // headers keeps them visible through any filter.
            List<KeyValuePair<UiComponent, string>> rows =
                new List<KeyValuePair<UiComponent, string>>();

            // Modded recipes — current pack first; the modder is more likely
            // to unlock something they just authored than a game recipe.
            List<RecipeDef> modded = m_packModel?.Recipes?
                .Where(r => !string.IsNullOrEmpty(r.RecipeId))
                .OrderBy(r => r.RecipeId, StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<RecipeDef>();
            HashSet<string> moddedIds = new HashSet<string>(
                modded.Select(r => r.RecipeId), StringComparer.Ordinal);

            if (modded.Count > 0) {
                Label header = new Label(new LocStrFormatted(
                    "This pack — " + modded.Count + " recipe(s)"));
                header.FontBold().PaddingTopBottom(2.pt());
                list.Add(header);
                rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                foreach (RecipeDef rd in modded) {
                    RecipeDef captured = rd;
                    UiComponent row = buildOptionRow(
                        thumb: null,
                        title: captured.Name ?? captured.RecipeId,
                        subtitle: captured.RecipeId,
                        chip: "(this pack)",
                        preview: RecipePreviewBuilder.BuildForModded(captured, m_protosDb),
                        onClick: () => selectValue(captured.RecipeId));
                    list.Add(row);
                    rows.Add(new KeyValuePair<UiComponent, string>(row,
                        ((captured.Name ?? "") + " " + captured.RecipeId)
                            .ToLowerInvariant()));
                }
            }

            // Game recipes from ProtosDb — skip ids already covered by the
            // modded section so the same recipe doesn't show twice.
            if (m_protosDb != null) {
                List<RecipeProto> game = m_protosDb.All<RecipeProto>()
                    .Where(p => !moddedIds.Contains(p.Id.Value))
                    .OrderBy(p => p.Strings.Name.TranslatedString,
                             StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (game.Count > 0) {
                    Label header = new Label(new LocStrFormatted(
                        "Game recipes — " + game.Count + " recipe(s)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    list.Add(header);
                    rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                    foreach (RecipeProto gp in game) {
                        RecipeProto captured = gp;
                        string typedRef = TypedRefResolver.TypedRefFor(
                            captured.Id.Value, IdsRecipesPrefix);
                        string storeAs = typedRef ?? captured.Id.Value;
                        string subtitle = typedRef ?? captured.Id.Value;
                        string modTag = readModTag(captured);

                        UiComponent row = buildOptionRow(
                            thumb: (captured is IProtoWithIcon ip
                                    && !string.IsNullOrEmpty(ip.IconPath))
                                ? new Icon(ip.IconPath).Size(OptionIconSize) : null,
                            title: captured.Strings.Name.TranslatedString,
                            subtitle: subtitle,
                            chip: modTag,
                            preview: RecipePreviewBuilder.BuildForGame(captured),
                            onClick: () => selectValue(storeAs));
                        list.Add(row);
                        rows.Add(new KeyValuePair<UiComponent, string>(row,
                            (captured.Strings.Name.TranslatedString + " "
                                + captured.Id.Value + " "
                                + (typedRef ?? "") + " "
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

        // Each option in the popup is a ButtonColumn: a vertical-flex button
        // that stacks its title row on top and the in-game RecipeUi preview
        // below at full width. Using ButtonColumn (not ButtonRow) is what
        // gives us the "preview on the next line" layout — ButtonRow lays
        // children horizontally and squeezes everything onto one row.
        private UiComponent buildOptionRow(UiComponent thumb, string title,
                string subtitle, string chip, Action onClick,
                UiComponent preview = null) {
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
            if (preview != null) {
                // The preview is a full RecipeUi card — give it some left
                // indent so it visually nests under the title row rather
                // than aligning flush with the button's left edge.
                preview.MarginLeft(4.pt());
                btn.Add(preview);
            }
            return btn;
        }

        private void selectValue(string value) {
            m_popup?.Close();
            m_setId(value);
            RefreshDisplay();
        }

        // ---- Lookups --------------------------------------------------------

        private RecipeDef findModdedRecipe(string id) {
            if (m_packModel?.Recipes == null) return null;
            foreach (RecipeDef r in m_packModel.Recipes) {
                if (r.RecipeId == id) return r;
            }
            // Variable resolution — Python source can bind a recipe to a name
            // (`myRecipe = build_recipe(...)`) and reference that name from
            // add_unlock_recipe. Run the resolver to see if it points to a
            // local recipe id.
            if (m_variableResolver != null) {
                string viaVar = m_variableResolver(id);
                if (!string.IsNullOrEmpty(viaVar) && viaVar != id) {
                    foreach (RecipeDef r in m_packModel.Recipes) {
                        if (r.RecipeId == viaVar) return r;
                    }
                }
            }
            return null;
        }

        // Three-tier resolution mirroring ProtoPicker: direct id, variable
        // resolver, then TypedRefResolver for dotted paths.
        private RecipeProto findGameRecipe(string id) {
            if (m_protosDb == null) return null;
            Option<RecipeProto> direct = m_protosDb.Get<RecipeProto>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            if (m_variableResolver != null) {
                string viaVar = m_variableResolver(id);
                if (!string.IsNullOrEmpty(viaVar) && viaVar != id) {
                    Option<RecipeProto> byVar = m_protosDb.Get<RecipeProto>(new Proto.ID(viaVar));
                    if (byVar.HasValue) return byVar.Value;
                }
            }
            string resolved = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<RecipeProto> byPath = m_protosDb.Get<RecipeProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // Provenance chip — same shape as ProtoPicker's so the recipe popup
        // looks consistent with the product/machine pickers next to it.
        private static string readModTag(RecipeProto proto) {
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
