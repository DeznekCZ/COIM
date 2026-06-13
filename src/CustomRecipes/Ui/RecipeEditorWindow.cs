using System;
using System.IO;
using System.Linq;
using System.Text;
using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Io;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core;
using Mafi.Core.Factory.Machines;
using Mafi.Core.GameLoop;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.UiStatic.Toolbar;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Component.Manipulators;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using UnityEngine;

namespace CustomAssets.Ui {

    /// In-game recipe editor window. Layout follows docs/mockups/recipe-editor-layout.html:
    ///   [LegacyBanner — visible only for legacy-structure packs]
    ///   [Body Row]
    ///     [Left  30%]  Tree panel (top, scrollable) + Pack-info panel (bottom)
    ///     [Right 70%]  Editor panel (statement editor; scrollable)
    ///
    /// The three content areas (tree, pack info, editor) are wrapped in separate
    /// Mafi <c>Panel</c>s so each gets the standard window-chrome look and can be
    /// styled / resized independently. Children are added directly via
    /// <c>Body.Add(...)</c> rather than going through <c>AddBodySingle</c>, which
    /// would wrap the whole layout in one outer panel — duplicating the per-area
    /// panel chrome we want.
    public sealed class RecipeEditorWindow : Window {

        // Recipes.svg is the most thematically accurate built-in toolbar icon
        // for a recipe-editing window. Avoids the need for a custom SVG in the
        // mod's asset pipeline (which would mean an asset-bundle build step we
        // intentionally skipped — see [[project_overview]] notes). If COI ever
        // drops Recipes.svg, fall back to Sandbox.svg (the editor is still a
        // sandbox feature, so the visual association makes sense).
        private const string IconAssetPath = "Assets/Unity/UserInterface/Toolbar/Recipes.svg";
        private const float ToolbarOrder = -110f;
        public static readonly LocStrFormatted WindowTitle = new LocStrFormatted("Recipe Editor");

        [GlobalDependency(RegistrationMode.AsEverything, false, false)]
        public sealed class Controller : WindowController<RecipeEditorWindow>,
                                         IToolbarItemController,
                                         IUnityInputController,
                                         IHotReloadUi {
            private readonly SandboxManager m_sandboxManager;
            private readonly ToolbarHud m_toolbar;
            private bool m_wasRegisteredIntoToolbar;

            public bool IsVisible { get; private set; }
            public bool DeactivateShortcutsIfNotVisible => false;
            public event Action<IToolbarItemController> VisibilityChanged;

            public Controller(ControllerContext controllerContext,
                              SandboxManager sandboxManager,
                              ToolbarHud toolbar)
                : base(controllerContext, null) {
                m_sandboxManager = sandboxManager;
                m_toolbar = toolbar;
                IsVisible = sandboxManager.CanCheat;
                if (IsVisible) registerIntoToolbar();
                controllerContext.GameLoop.SyncUpdate.AddNonSaveable(this, syncUpdate);
            }

            private void registerIntoToolbar() {
                m_toolbar.AddMainMenuButton(
                    WindowTitle, this, IconAssetPath, ToolbarOrder,
                    _ => new KeyBindings(
                        ShortcutMode.Game,
                        KeyBinding.FromKeys(KbCategory.Windows,
                            KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.R),
                        KeyBinding.Empty(KbCategory.Windows)));
                m_wasRegisteredIntoToolbar = true;
            }

            private void syncUpdate(GameTime time) {
                bool canCheat = m_sandboxManager.CanCheat;
                if (IsVisible == canCheat) return;
                IsVisible = canCheat;
                if (IsVisible && !m_wasRegisteredIntoToolbar) registerIntoToolbar();
                else VisibilityChanged?.Invoke(this);
            }

            public void DisposeForHotReload() {
                Context.GameLoop.SyncUpdate.RemoveNonSaveable(this, syncUpdate);
            }
        }

        // ---- Window state ------------------------------------------------------

        private LoadedPack m_currentPack;
        private PackModel m_currentModel;
        private RecipeDef m_selectedRecipe;
        // Currently-selected non-recipe definition (or null). Holds the
        // typed Def (ResearchDef, UnlockRecipeDef, etc.) or UnknownDef
        // backing the form. Tracking it lets post-save reload re-select the
        // same definition by id so the modder doesn't lose their place.
        private DefBase m_selectedOther;
        private bool m_legacyDismissedThisSession;

        // Per-kind DefEditor cache. Constructed lazily on first selection
        // of each kind; subsequent selections of the same kind reuse the
        // existing editor via Value(...) without tearing down its UI tree.
        // Keyed by the concrete DefBase subclass type, value is the
        // editor's Column (DefEditor<T> erased to base for storage).
        private readonly System.Collections.Generic.Dictionary<System.Type, Column>
            m_editorCache = new System.Collections.Generic.Dictionary<System.Type, Column>();

        // Lookup-or-create a DefEditor<T> from m_editorCache, bind the
        // current value into it via Value(...), and add it to the statement
        // column. The editor's Value(...) returns DefEditor<T> (covariant
        // chain), so we cast on the way back to call it.
        private void showEditor<T>(T def, System.Func<Editors.DefEditor<T>> factory)
                where T : DefBase {
            System.Type key = typeof(T);
            if (!m_editorCache.TryGetValue(key, out Column cached)) {
                cached = factory();
                m_editorCache[key] = cached;
            }
            ((Editors.DefEditor<T>)cached).Value(def);
            m_statementColumn.Add(cached);
        }

        // UiContext gives us the live ProtosDb the in-game proto pickers need.
        // Window dependencies are resolved via Context.Resolver.Instantiate
        // (WindowController.CreateWindow); declaring it on the constructor is
        // sufficient — no manual binding required.
        private readonly UiContext m_uiContext;

        // Injected by DI alongside UiContext. Used to send the modder back
        // to the main menu after creating a new pack so COI rescans the
        // mods folder on the way through and the new pack loads on next
        // save-load. Constructor-injected via Context.Resolver.Instantiate
        // (no manual binding needed).
        private readonly Mafi.Unity.IMain m_main;

        private readonly LegacyBanner m_legacyBanner;
        private readonly Column m_treeColumn;       // children = per-file CollapsibleGroups
        private readonly Column m_statementColumn; // polymorphic right-pane content
        private readonly Row m_editorFooter;        // fixed Save / Duplicate / Verify row
        private readonly ButtonText m_saveBtn;
        private readonly ButtonText m_duplicateBtn;
        private readonly ButtonText m_verifyBtn;
        private DefBase m_footerTarget;             // current footer subject; null = nothing selected
        private readonly PackCardView m_packCard;

        // Per-row map for selection-state refresh. Keys are the DefBase
        // instances rendered into the tree on the current rebuildTree;
        // values are the label-button UiComponent so onRecipeSelected /
        // onOtherDefSelected can toggle Cls.selected without rebuilding
        // the whole tree. DefBase doesn't override Equals so default
        // reference equality is what we want. Cleared and repopulated by
        // buildTreeRow on every rebuildTree pass.
        private readonly System.Collections.Generic.Dictionary<DefBase, UiComponent>
            m_rowLabelByDef = new System.Collections.Generic.Dictionary<DefBase, UiComponent>();

        public RecipeEditorWindow(UiContext uiContext, Mafi.Unity.IMain main) : base(WindowTitle) {
            m_uiContext = uiContext;
            m_main      = main;
            MakeImmersiveFullscreen();

            // Warm AssetsDb's sprite cache for Mafi.Base.Assets image consts
            // the first time the editor attaches. Each AssetPathPicker would
            // otherwise hit GetSharedSprite for 800+ icons on its first popup
            // open — stutters the UI for ~1s. Doing it once at window-attach
            // time means subsequent picker opens are instant.
            RunWhenAttached(MafiAssetPrecache.Warm);

            m_legacyBanner = new LegacyBanner(onMigrateNow, onLegacyDismiss);

            m_treeColumn      = new Column();
            m_statementColumn = new Column();
            // Stretch children across the full panel width so pickers and row
            // editors fill the editor pane instead of collapsing to their
            // content. AlignItemsStretch makes each direct child (labeledField
            // Column, port-layout Column, list wrappers, validation Column)
            // span horizontally; Width(100%) on m_statementColumn itself is
            // needed because ScrollColumn (the parent) doesn't propagate its
            // own width to children by default — without it the column hugs
            // its content and the stretch chain has nothing to stretch into.
            m_statementColumn.AlignItemsStretch().Width(100.Percent());
            // Same width-propagation fix as m_statementColumn — without it
            // the per-file CollapsibleGroups in the tree hugged their
            // header text width instead of filling the left pane.
            m_treeColumn.AlignItemsStretch().Width(100.Percent());
            m_packCard        = new PackCardView(onSwitchPack, onOpenDepsDialog, onOpenTranslations);

            // ---- Three content panels: tree (top-left), pack info (bottom-left), editor (right).
            // Each Mafi Panel adds its own background + bolts so the regions visually
            // separate without us drawing borders by hand.
            ScrollColumn treeScroll = new ScrollColumn();
            treeScroll.Add(m_treeColumn);
			treeScroll.FlexGrow(1f).AlignItemsStretch();
            Panel treePanel = new Panel();
            treePanel.BodyAdd(treeScroll);
            treePanel.FlexGrow(1f);

            Panel packCardPanel = new Panel();
            packCardPanel.BodyAdd(m_packCard);

            // Editor pane = ScrollColumn body that fills the available space +
            // a fixed footer row pinned to the bottom. FlexGrow(1) on the
            // scroll makes the editor body absorb all leftover height so the
            // footer stays glued to the panel's lower edge whatever the
            // content size. Action buttons (Save / Duplicate / Verify) live
            // in m_editorFooter and are repopulated on every selection
            // change by rebuildEditorFooter.
            ScrollColumn statementScroll = new ScrollColumn();
            statementScroll.Add(m_statementColumn);
            statementScroll.FlexGrow(1f).AlignItemsStretch();

            // Footer buttons are built ONCE and stay attached for the
            // lifetime of the window — selection changes flip their
            // Enabled state via rebuildEditorFooter and stash the target
            // def in m_footerTarget so the click handlers dispatch to the
            // current selection without re-allocating buttons each pass.
            // Keeps the footer visually stable instead of flashing as the
            // modder clicks around the tree.
            m_saveBtn = new ButtonText(
                new LocStrFormatted("Save this entry"),
                () => { if (m_footerTarget != null) onSaveDef(m_footerTarget); });
            m_duplicateBtn = new ButtonText(
                new LocStrFormatted("Duplicate"),
                () => { if (m_footerTarget != null) onDuplicateDef(m_footerTarget); });
            //m_verifyBtn = new ButtonText(
            //    new LocStrFormatted("Verify Round-Trip"),
            //    onVerifyRoundTrip);

            m_editorFooter = new Row { m_saveBtn, m_duplicateBtn, /*m_verifyBtn*/ };
            m_editorFooter.Gap(3.pt())
                          .AlignItemsCenter()
                          .PaddingTopBottom(3.pt())
                          .PaddingLeftRight(4.pt());

            // Start with no selection → every button disabled but still
            // visible so the modder sees the panel's full action surface.
            rebuildEditorFooter(null, includeVerify: false);

            Column editorBody = new Column { statementScroll, m_editorFooter };
            editorBody.AlignItemsStretch().FlexGrow(1f);

            Panel editorPanel = new Panel();
            editorPanel.BodyAdd(editorBody);
            editorPanel.FlexGrow(1f);

            // Left column = tree panel (flex-grow) + pack panel (auto, bottom).
            // Clamp the width: max 30% of the window so the editor stays roomy
            // on wide screens, min 600px so file names and recipe labels stay
            // readable on smaller resolutions. AlignItemsStretch makes the two
            // child panels fill the column's width; Gap separates them visually.
            Column leftColumn = new Column { treePanel, packCardPanel };
            leftColumn.Width(30.Percent()).MinWidth(600.px())
                      .AlignItemsStretch()
                      .Gap(4.pt());

            // Right column = editor panel filling the rest. FlexGrow(1) makes
            // the column itself claim leftover horizontal space within the
            // body row; AlignItemsStretch propagates that width to the inner
            // editorPanel so the form pane fills full width rather than
            // collapsing to the natural width of its current content (which
            // was the bug producing a narrow centred strip).
            Column rightColumn = new Column { editorPanel };
            rightColumn.FlexGrow(1f)
                       .AlignItemsStretch();

            // Row's cross-axis is vertical — AlignItemsStretch lets each column
            // claim the row's full height so the tree panel can grow and push
            // the pack panel to the bottom edge of the screen. Gap(4.pt())
            // is the visual breathing room between the left and right panels.
            Row body = new Row { leftColumn, rightColumn };
            body.FlexGrow(1f).AlignItemsStretch().Gap(4.pt());

            // Body.Add directly (no AddBodySingle wrapper) so we don't get an
            // outer Panel wrapping our already-panelled content. Padding adds:
            //   - top:    clearance from the COI HUD (top toolbar/calendar)
            //   - sides:  small breathing room from screen edges
            //   - bottom: matches the side gap visually
            //   - gap:    spacing between legacy-banner (when visible) and body
            Body.Add(m_legacyBanner);
            Body.Add(body);
            Body.AlignItemsStretch()
                .PaddingTop(90.px())
                .PaddingLeftRight(10.px())
                .PaddingBottom(10.px())
                .Gap(4.pt());

            // Initial population.
            if (PackRegistry.Count > 0) {
                LoadedPack first = PackRegistry.Packs.FirstOrDefault();
                if (first != null) selectPack(first);
            } else {
                showEmptyStatement("No packs loaded. Install a CustomAssets pack and reload the save.");
            }
        }

        // ---- Pack selection ----------------------------------------------------

        private void selectPack(LoadedPack pack) {
            m_currentPack = pack;
            m_currentModel = PackLoader.Load(pack);
            m_selectedRecipe = null;
            m_packCard.SetPack(pack);

            // Show the legacy banner when the pack matches the old convention
            // (no __init__.py + inline dependencies calls) AND the modder
            // hasn't dismissed it for this session. The Dismiss flag is per-
            // session and not pack-specific, by design — once you've
            // dismissed for any pack you've seen the message and don't need
            // to be re-prompted when cycling between packs.
            bool isLegacy = PackRegistry.IsLegacyStructure(pack);
            m_legacyBanner.Visible(isLegacy && !m_legacyDismissedThisSession);

            // Surface PackValidator findings to Player.log so the user can
            // see context (file + line + message) even before opening any
            // def. The in-editor surface is the per-def warning row added
            // by onOtherDefSelected / onRecipeSelected.
            foreach (PackIssue issue in m_currentModel.Issues) {
                Log.Warning("RecipeEditor: " + Path.GetFileName(issue.SourceFile ?? "?")
                            + ":" + issue.Line + " - " + issue.Message);
            }

            rebuildTree();
            string baseMsg = m_currentModel.Recipes.Count() == 0
                ? "Pack has no build_recipe() calls yet."
                : "Select a recipe from the tree.";
            if (m_currentModel.Issues.Count > 0) {
                baseMsg += "  ⚠ " + m_currentModel.Issues.Count
                    + " validation issue(s) - see Player.log and the warning row on the affected def.";
            }
            showEmptyStatement(baseMsg);
        }

        // ---- Tree --------------------------------------------------------------

        // Rebuild the entire left tree from PackRegistry + the loaded PackModel.
        // Cheap — no disk IO, just a walk over the cached AST and the per-recipe
        // SourceFile assignments. Structure:
        //
        //   [+ new file]
        //   📄 __init__.py        — always first, regardless of file system order
        //   📄 <other files…>     — alphabetical by filename
        //     └ Recipes (n)       — leaves listed inside; click to open in editor
        //
        // Files with no recognised content (e.g. product-only files we don't
        // yet model) still appear as collapsed groups so modders can see them
        // and extend pack content later.
        private void rebuildTree() {
            m_treeColumn.Clear();
            m_rowLabelByDef.Clear();
            if (m_currentModel == null || m_currentPack == null) return;

            // "+ new file" header — scaffold flow is a follow-up todo.
            // The button is wrapped in its own Panel with 3px inner padding so
            // it sits visually separated from the file groups below, sharing
            // the same panel-chrome look as the tree, pack-info, and editor
            // panels in the outer layout.
            Panel newFilePanel = new Panel();
            newFilePanel.BodyAdd(c => c.Padding(3.px()),
                new ButtonText(new LocStrFormatted("+ new file"), onNewFile));
            m_treeColumn.Add(newFilePanel);

            // Sort files with __init__.py first, then the rest by filename
            // alphabetically. The runtime loads __init__.py first too, so the
            // tree position mirrors load order.
            var files = m_currentPack.Files
                .OrderBy(f => sortKeyFor(f.AbsolutePath), StringComparer.Ordinal)
                .ToList();

            foreach (LoadedFile file in files) {
                string fileName = Path.GetFileName(file.AbsolutePath);
                bool isInit = fileName == "__init__.py";

                var recipesInFile = m_currentModel.Recipes
                    .Where(r => r.SourceFile == file.AbsolutePath)
                    .ToList();
                var otherDefsInFile = m_currentModel.OtherDefinitions
                    .Where(d => d.SourceFile == file.AbsolutePath)
                    .ToList();

                // Index defs by (scope, run) so each Reorderable container in
                // renderStatementsTree can pull its members in current model
                // order. The model order is the SOURCE OF TRUTH for tree
                // position after a load — drag operations mutate it, so we
                // no longer key off AST line numbers (those reflect the
                // pre-drag source layout). Defs from a single file only —
                // other files have their own (scope, run) indices.
                var defsByScopeRun = new System.Collections.Generic.Dictionary<string,
                    System.Collections.Generic.List<DefBase>>(StringComparer.Ordinal);
                foreach (DefBase d in m_currentModel.Definitions) {
                    if (d.SourceFile != file.AbsolutePath) continue;
                    string key = (d.ScopeKey ?? "top") + "#" + d.RunIndex;
                    if (!defsByScopeRun.TryGetValue(key, out var list)) {
                        list = new System.Collections.Generic.List<DefBase>();
                        defsByScopeRun[key] = list;
                    }
                    list.Add(d);
                }

                string headerText = "📄 " + fileName +
                    "  (" + recipesInFile.Count + " recipe(s)"
                    + (otherDefsInFile.Count > 0 ? ", " + otherDefsInFile.Count + " other def(s)" : "")
                    + (isInit ? ", load order" : "") + ")";

                // __init__.py defaults expanded when it has content (so the
                // modder sees the misplaced definitions immediately) and
                // collapsed when it only carries load order. Content files
                // always default expanded.
                bool defaultExpanded = !isInit
                    || recipesInFile.Count > 0
                    || otherDefsInFile.Count > 0;
                var fileGroup = new CollapsibleGroup(
                    new LocStrFormatted(headerText),
                    expanded: defaultExpanded);

                // Read the source lines once per file so the conditional walk
                // can label each if/elif/else clause with its verbatim header
                // text. Tolerate read failure — falls back to a generic label
                // ("if/elif/else block") when sourceLines is null.
                string[] sourceLines = tryReadAllLines(file.AbsolutePath);

                // Structural walk — render the AST's top-level statements in
                // order, recursing into IfStatement bodies and surfacing each
                // if/elif/else clause as its own CollapsibleGroup so recipes
                // nested inside a conditional live visually under their
                // wrapper instead of being flattened into one big "Recipes"
                // group at the bottom of the file.
                if (file.Ast != null) {
                    renderStatementsTree(file.Ast.statements, fileGroup.Body,
                                         defsByScopeRun, sourceLines, file.AbsolutePath,
                                         scopeKey: "top");
                }

                if (isInit) {
                    // Always note the load-order entry point and flag the
                    // recommendation when definitions are present.
                    if (recipesInFile.Count > 0 || otherDefsInFile.Count > 0) {
                        fileGroup.Body.Add(new Label(new LocStrFormatted(
                            "⚠ Tip: prefer to keep __init__.py for load order only and " +
                            "move definitions into separate files (products, recipes, research, …).")));
                    }
                    fileGroup.Body.Add(new Label(new LocStrFormatted(
                        "Load order — edit via the 🔗 deps dialog on the pack card.")));
                } else if (recipesInFile.Count == 0 && otherDefsInFile.Count == 0) {
                    fileGroup.Body.Add(new Label(new LocStrFormatted(
                        "(no recognised statements — products/research/asset editors land later)")));
                }

                // Single "+ add definition…" button on every non-init file.
                // Opens a FloatingColumn popup with one row per DefKind so
                // the modder picks what to insert without flooding the tree
                // with 16 sibling buttons. Skipped on __init__.py since the
                // convention keeps it for load-order imports only.
                if (!isInit) {
                    string targetFile = file.AbsolutePath;
                    ButtonText addBtn = fileGroup.Body.AddAndReturn(new ButtonText(
                        new LocStrFormatted("+ add definition…"), null));
                    addBtn.OnClick(() => openAddDefPopup(addBtn, targetFile));
                }

                m_treeColumn.Add(fileGroup);
            }
        }

        // Popup launched by the "+ add definition…" button on each file
        // group's header. Lists every DefKind as a clickable row with a
        // short hint of what the call name will be. Clicking closes the
        // popup and inserts a fresh def into the target file.
        private void openAddDefPopup(UiComponent anchor, string targetFile) {
            openAddDefPickerPopup(anchor,
                title: "Add definition to " + Path.GetFileName(targetFile),
                onPick: kind => onAddDefToFile(targetFile, kind));
        }

        // Per-clause sibling — same picker, just routes the click through
        // onAddDefToClause so the def splices into the if-chain clause
        // body with proper indentation instead of the file end.
        private void openAddDefIntoClausePopup(UiComponent anchor,
                string targetFile, int clauseHeaderLine) {
            openAddDefPickerPopup(anchor,
                title: "Add definition inside clause @ line " + clauseHeaderLine
                    + " of " + Path.GetFileName(targetFile),
                onPick: kind => onAddDefToClause(targetFile, clauseHeaderLine, kind));
        }

        // Build + open the shared "pick a def kind" picker. Both the
        // file-level and per-clause "+ add definition" affordances feed
        // through here so the option list stays identical across both
        // surfaces — adding a new DefKind only needs one entry to show
        // up everywhere it's relevant.
        private void openAddDefPickerPopup(UiComponent anchor,
                string title, Action<DefKind> onPick) {
            FloatingColumn popup = new FloatingColumn(
                FloaterPositionPolicy.BELOW,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            PanelWithHeader panel = popup.AddAndReturn(new PanelWithHeader(
                    new LocStrFormatted(title)))
                .AlignItemsStretch()
                .Gap(2.pt())
                .MinWidth(360.px())
                .MaxHeight(520.px());

            ScrollColumn list = new ScrollColumn();
            list.Gap(1.pt()).MaxHeight(460.px()).AlignItemsStretch();
            panel.BodyAdd(list);

            Action<DefKind> pick = kind => {
                popup.Close();
                onPick(kind);
            };

            // Same order the toolbar would have shown — recipe-shaped first
            // (recipe, edit_recipe, research), then products, then unlocks,
            // then asset registrations. Within each group, the most common
            // is listed first.
            list.Add(new Label(new LocStrFormatted("Products")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "product (loose)",   "build_product_loose(...)",          DefKind.ProductLoose,           pick);
            addPopupRow(list, "product (fluid)",   "build_product_fluid(...)",          DefKind.ProductFluid,           pick);
            addPopupRow(list, "product (unit)",    "build_product_unit(...)",           DefKind.ProductUnit,            pick);
            list.Add(new Label(new LocStrFormatted("Recipes")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "build recipe",      "build_recipe(...)",                 DefKind.Recipe,                 pick);
            addPopupRow(list, "edit recipe",       "edit_recipe(...)",                  DefKind.EditRecipe,             pick);
            list.Add(new Label(new LocStrFormatted("Machines")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "build machine",     "build_machine(...)",                DefKind.BuildMachine,           pick);
            addPopupRow(list, "build generator",   "build_generator(...)",              DefKind.Generator,              pick);
            addPopupRow(list, "edit machine ports","edit_machine_ports(...)",           DefKind.EditMachinePorts,       pick);
            list.Add(new Label(new LocStrFormatted("Settlements")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "build housing",     "build_housing(...)",                DefKind.Housing,                pick);
            addPopupRow(list, "build decoration",  "build_settlement_decoration(...)",  DefKind.SettlementDecoration,   pick);
            addPopupRow(list, "build food module", "build_settlement_food(...)",        DefKind.SettlementFood,         pick);
            addPopupRow(list, "build ISP module",  "build_settlement_isp(...)",         DefKind.SettlementIsp,          pick);
            addPopupRow(list, "build hospital",    "build_hospital(...)",               DefKind.Hospital,               pick);
            list.Add(new Label(new LocStrFormatted("Buildings")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "build mine tower",  "build_mine_tower(...)",             DefKind.MineTower,              pick);
            addPopupRow(list, "build research lab","build_research_lab(...)",           DefKind.ResearchLab,            pick);
            addPopupRow(list, "build nuclear reactor", "build_nuclear_reactor(...)",    DefKind.NuclearReactor,         pick);
            addPopupRow(list, "edit reactor fuels",    "edit_nuclear_reactor_fuels(...)",      DefKind.EditNuclearReactorFuels,      pick);
            addPopupRow(list, "edit reactor fluids",   "edit_nuclear_reactor_fluids(...)",     DefKind.EditNuclearReactorFluids,     pick);
            addPopupRow(list, "edit reactor enrichment","edit_nuclear_reactor_enrichment(...)",DefKind.EditNuclearReactorEnrichment, pick);
            addPopupRow(list, "edit reactor ports",    "edit_nuclear_reactor_ports(...)",      DefKind.EditNuclearReactorPorts,      pick);
            list.Add(new Label(new LocStrFormatted("Research")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "research",          "build_research(...)",               DefKind.Research,               pick);
            addPopupRow(list, "unlock recipe",     "add_unlock_recipe(...)",            DefKind.UnlockRecipe,           pick);
            addPopupRow(list, "unlock product",    "add_unlock_product(...)",           DefKind.UnlockProduct,          pick);
            addPopupRow(list, "unlock machine",    "add_unlock_machine(...)",           DefKind.UnlockMachine,          pick);
            list.Add(new Label(new LocStrFormatted("Toolbars")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "toolbar category",  "add_toolbar_category(...)",         DefKind.ToolbarCategory,        pick);
            list.Add(new Label(new LocStrFormatted("Assets")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "texture",           "add_texture(...)",                  DefKind.Texture,                pick);
            addPopupRow(list, "material (loose)",  "add_loose_product_material(...)",   DefKind.MaterialLoose,          pick);
            addPopupRow(list, "material (texture)","add_texture_material(...)",         DefKind.MaterialTexture,        pick);
            addPopupRow(list, "prefab (box)",      "add_prefab_box(...)",               DefKind.PrefabBox,              pick);
            addPopupRow(list, "prefab (unit)",     "add_unit_prefab(...)",              DefKind.UnitPrefab,             pick);
            list.Add(new Label(new LocStrFormatted("Conditionals")).Class(Cls.groupHeader).PaddingTop(3.pt()));
            addPopupRow(list, "if block",          "if True:\n    pass",                DefKind.IfBlock,                pick);

            popup.Open(anchor);
        }

        private void addPopupRow(ScrollColumn list,
                string title, string subtitle, DefKind kind, Action<DefKind> onPick) {
            ButtonRow row = new ButtonRow(
                Mafi.Unity.UiToolkit.Library.Button.General,
                () => onPick(kind));
            row.Class(Cls.group);
            row.Gap(3.pt()).AlignItemsCenter().PaddingLeftRight(2.pt());
            Column stack = new Column {
                new Label(new LocStrFormatted(title)),
                new Label(new LocStrFormatted(subtitle))
                    .Class(Cls.fontMonospace).TinyFontSize()
            };
            stack.Fill();
            row.Add(stack);
            list.Add(row);
        }

        // Add a fresh def into the body of an if/elif/else clause at the
        // given header line. Mirrors onAddDefToFile but routes the splice
        // through PackEmitter.AppendDefIntoClause so the def is indented
        // to match the clause's body level.
        private void onAddDefToClause(string filePath, int clauseHeaderLine, DefKind kind) {
            if (m_currentModel == null) return;
            // Fresh nested if-block — handled by the dedicated emitter
            // splice that hand-rolls `if True:\n    pass` with the right
            // nesting indent.
            if (kind == DefKind.IfBlock) {
                try {
                    PackEmitter.AppendIfBlockIntoClause(filePath, clauseHeaderLine, "True");
                    if (m_currentPack != null) {
                        PackRegistry.RescanPack(m_currentPack);
                        m_currentModel = PackLoader.Load(m_currentPack);
                    }
                    rebuildTree();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-if-inside-clause failed - " + ex.Message);
                }
                return;
            }
            string newId = freshId(kindIdPrefix(kind));
            DefBase created = createDef(kind, newId);
            if (created == null) return;
            created.SourceFile = filePath;

            Mafi.Collections.Lyst<string> missing = created.MissingMandatoryFields();
            if (missing != null && missing.Count > 0) {
                // Same "in-memory only until required fields are filled"
                // contract as onAddDefToFile. Add to the model so it's
                // visible + editable; save runs the splice path later.
                m_currentModel.Definitions.Add(created);
                Log.Info("RecipeEditor: added '" + created.DisplayId + "' to clause @ line "
                    + clauseHeaderLine + " (in-memory only — fill "
                    + string.Join(", ", missing) + " then Save to write it).");
                rebuildTree();
                onOtherDefSelected(created);
                return;
            }

            try {
                PackEmitter.AppendDefIntoClause(filePath, clauseHeaderLine, created, m_currentModel);
                if (m_currentPack != null) {
                    PackRegistry.RescanPack(m_currentPack);
                    m_currentModel = PackLoader.Load(m_currentPack);
                }
                rebuildTree();
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: add-into-clause failed - " + ex.Message);
            }
        }

        // Append an else clause to an existing if-chain. Triggered by the
        // "+ add else clause" button on the chain's last group (only
        // rendered when the chain doesn't already end with else).
        private void onAddElseToChain(string filePath, int afterLine, string leadingIndent) {
            if (m_currentPack == null) return;
            try {
                PackEmitter.AppendElseClauseToFile(filePath, afterLine, leadingIndent ?? "");
                PackRegistry.RescanPack(m_currentPack);
                m_currentModel = PackLoader.Load(m_currentPack);
                rebuildTree();
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: add-else failed - " + ex.Message);
            }
        }

        // Read the leading-whitespace prefix of a 1-based line. Returns
        // empty string when the line is past EOF or the lines array is
        // null. Used by the "+ add else" hook to give the new clause the
        // same indent as the chain's opening if.
        private static string readLeadingIndentForLine(string[] sourceLines, int oneBasedLine) {
            if (sourceLines == null || oneBasedLine < 1 || oneBasedLine > sourceLines.Length) {
                return "";
            }
            string line = sourceLines[oneBasedLine - 1] ?? "";
            int n = 0;
            while (n < line.Length && (line[n] == ' ' || line[n] == '\t')) n++;
            return line.Substring(0, n);
        }

        // Discriminator for the per-kind "+ new" buttons. Each value maps to
        // a concrete DefBase subclass that the create flow knows how to
        // instantiate with sensible defaults.
        private enum DefKind {
            Recipe,
            EditRecipe,
            Research,
            ProductLoose,
            ProductFluid,
            ProductUnit,
            UnlockRecipe,
            UnlockProduct,
            UnlockMachine,
            Texture,
            MaterialLoose,
            MaterialTexture,
            PrefabBox,
            UnitPrefab,
            Generator,
            ToolbarCategory,
            EditMachinePorts,
            BuildMachine,
            Housing,
            SettlementDecoration,
            SettlementFood,
            SettlementIsp,
            Hospital,
            MineTower,
            ResearchLab,
            NuclearReactor,
            EditNuclearReactorFuels,
            EditNuclearReactorFluids,
            EditNuclearReactorEnrichment,
            EditNuclearReactorPorts,
            // Structural — not a typed Def. Routed through
            // PackEmitter.AppendIfBlockToFile / AppendIfBlockIntoClause
            // instead of createDef + AppendDef.
            IfBlock,
        }

        // Append a fresh Def to the model, tagged for emission into
        // `filePath`. The id seed is "New<Kind>_<n>" where n is bumped until
        // it doesn't collide with any existing def's Id — gives the modder
        // something unique they can rename in the form without typing over
        // an existing entry. SourceStartLine / SourceEndLine stay at 0,
        // which PackEmitter treats as "append rendered call to file end".
        //
        // After insertion, the tree is rebuilt and the new def is
        // pre-selected so the modder lands directly in its form. Recipes
        // open via onRecipeSelected (full recipe form); everything else
        // goes through onOtherDefSelected (the typed-form dispatcher).
        private void onAddDefToFile(string filePath, DefKind kind) {
            if (m_currentModel == null) return;
            // Structural kinds (IfBlock) don't go through createDef +
            // AppendDef — they live in the AST, not the typed model.
            // Route them to dedicated raw-text splicers instead.
            if (kind == DefKind.IfBlock) {
                try {
                    PackEmitter.AppendIfBlockToFile(filePath, "True");
                    if (m_currentPack != null) {
                        PackRegistry.RescanPack(m_currentPack);
                        m_currentModel = PackLoader.Load(m_currentPack);
                    }
                    rebuildTree();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-if failed - " + ex.Message);
                }
                return;
            }
            string newId = freshId(kindIdPrefix(kind));
            DefBase created = createDef(kind, newId);
            if (created == null) return;
            created.SourceFile      = filePath;
            created.SourceStartLine = 0;
            created.SourceEndLine   = 0;
            // Auto-assign a Python variable name so other defs in this
            // pack can reference the new entry via the variable form
            // (`research = researchX`) instead of the bare id form
            // (`research = "CustomResearch_X"`). Pure-id references fail
            // at runtime when the COI loader hasn't yet registered the
            // proto; variable references hand the in-memory object and
            // sidestep the lookup entirely. Applies to research,
            // products, textures, prefabs, materials, generators,
            // machines, recipes — every kind whose Python call returns
            // a usable object. See supportsAutoVariableName for the
            // exact whitelist.
            if (string.IsNullOrEmpty(created.VariableName)
                    && supportsAutoVariableName(created)) {
                created.VariableName = deriveFreshVarName(created);
            }
            m_currentModel.Definitions.Add(created);

            // Commit the new def to disk RIGHT NOW — UNLESS it's still
            // missing mandatory fields. Some def kinds (edit_machine_ports,
            // build_machine) carry required ids that createDef can't seed
            // because they reference other protos the modder must pick.
            // Emitting them while empty produces a syntactically-valid but
            // semantically-broken call (e.g.
            // `edit_machine_ports(machine = None)`) which then crashes the
            // mod loader on next pack reload. Hold these in memory only
            // until the modder fills the required fields, at which point
            // their next per-def or global Save runs the append path.
            string newKind = created.Kind;
            string newDisplayId = created.DisplayId;
            Mafi.Collections.Lyst<string> missing = created.MissingMandatoryFields();
            if (missing != null && missing.Count > 0) {
                Log.Info("RecipeEditor: added '" + newDisplayId + "' to "
                    + Path.GetFileName(filePath) + " (in-memory only — fill "
                    + string.Join(", ", missing) + " then Save to write it to disk).");
            } else if (m_currentPack != null) {
                try {
                    PackEmitter.AppendDef(created, m_currentModel);
                    PackRegistry.RescanPack(m_currentPack);
                    m_currentModel = PackLoader.Load(m_currentPack);
                    Log.Info("RecipeEditor: added '" + newDisplayId + "' to "
                        + Path.GetFileName(filePath));
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-new failed - " + ex.Message);
                    // Leave the in-memory def in place so the modder can
                    // still see + edit it; their next Save will retry the
                    // append path.
                }
            }

            rebuildTree();

            // The model was reloaded fresh from disk, so the `created`
            // reference no longer matches what's in m_currentModel. Find
            // the equivalent by (Kind, DisplayId) so we can select it.
            DefBase selected = m_currentModel.Definitions.FirstOrDefault(
                d => d.Kind == newKind && d.DisplayId == newDisplayId);
            if (selected is RecipeDef rd2) onRecipeSelected(rd2);
            else if (selected != null) onOtherDefSelected(selected);
            else if (created is RecipeDef rd) onRecipeSelected(rd);
            else onOtherDefSelected(created);
        }

        // Make a unique id by appending "_<n>" until nothing in the current
        // model claims it. Walks the unified Definitions list once — fine for
        // typical pack sizes (dozens of defs); a future hot-path could cache
        // a Set if it ever becomes the bottleneck.
        private string freshId(string prefix) {
            int n = 1;
            string id;
            do {
                id = prefix + "_" + n;
                n++;
            } while (m_currentModel.Definitions.Any(d => d.DisplayId == id));
            return id;
        }

        // Derive a Python variable name from the def's DisplayId so the
        // new entry rounds-trips as `<var> = <call>(...)` and can be
        // referenced by name from elsewhere in the same file. Strategy:
        //   • Strip a leading directory (asset paths like Assets/Foo
        //     read as just "foo" in the variable).
        //   • Drop a trailing file-extension (texture ids sometimes
        //     include .png).
        //   • Keep [A-Za-z_0-9] only; lower the first letter to read as
        //     camelCase; prefix a leading digit with 'v' so the result
        //     is a valid identifier.
        //   • Uniquify against every variable name already in use in
        //     the same source file — both VariableName fields on
        //     model defs AND any populated SourceFileVariables map.
        private string deriveFreshVarName(DefBase def) {
            if (def == null) {
                return null;
            }
            string seed = def.DisplayId ?? "";
            int lastSlash = seed.LastIndexOfAny(new[] { '/', '\\' });
            if (lastSlash >= 0 && lastSlash < seed.Length - 1) {
                seed = seed.Substring(lastSlash + 1);
            }
            int dot = seed.LastIndexOf('.');
            if (dot > 0) {
                seed = seed.Substring(0, dot);
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in seed) {
                if (char.IsLetter(c) || char.IsDigit(c) || c == '_') {
                    sb.Append(c);
                }
            }
            if (sb.Length == 0) {
                sb.Append('v');
            }
            if (char.IsDigit(sb[0])) {
                sb.Insert(0, 'v');
            }
            sb[0] = char.ToLowerInvariant(sb[0]);
            string baseName = sb.ToString();

            System.Collections.Generic.HashSet<string> used =
                new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            if (m_currentModel?.Definitions != null) {
                foreach (DefBase d in m_currentModel.Definitions) {
                    bool sameFile = string.Equals(d.SourceFile, def.SourceFile,
                        StringComparison.OrdinalIgnoreCase);
                    if (!sameFile) {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(d.VariableName)) {
                        used.Add(d.VariableName);
                    }
                    if (d.SourceFileVariables != null) {
                        foreach (string varName in d.SourceFileVariables.Keys) {
                            used.Add(varName);
                        }
                    }
                }
            }
            if (!used.Contains(baseName)) {
                return baseName;
            }
            for (int i = 2; i < 1000; i++) {
                string suffixed = baseName + i;
                if (!used.Contains(suffixed)) {
                    return suffixed;
                }
            }
            return baseName;
        }

        // Whether to auto-create a VariableName when adding a new def.
        // True for kinds whose Python call returns a usable object the
        // modder may want to reference elsewhere — research, products,
        // textures, materials, prefabs, generators, machines, recipes.
        // Skipped for side-effect calls (unlocks, edit_recipe,
        // edit_machine_ports, toolbar category) and structural markers
        // (if-block, unknown). The modder can still hand-edit the
        // VariableName field on the skipped kinds if they need it.
        private static bool supportsAutoVariableName(DefBase def) {
            return def is RecipeDef
                || def is ResearchDef
                || def is ProductDefBase
                || def is TextureDef
                || def is MaterialLooseDef
                || def is MaterialTextureDef
                || def is PrefabBoxDef
                || def is UnitPrefabDef
                || def is GeneratorDef
                || def is BuildMachineDef;
        }

        private static string kindIdPrefix(DefKind kind) {
            switch (kind) {
                case DefKind.Recipe:          return "NewRecipe";
                case DefKind.EditRecipe:      return "EditRecipe";
                case DefKind.Research:        return "NewResearch";
                case DefKind.ProductLoose:    return "Product_NewLoose";
                case DefKind.ProductFluid:    return "Product_NewFluid";
                case DefKind.ProductUnit:     return "Product_NewUnit";
                case DefKind.UnlockRecipe:    return "Unlock_Recipe";
                case DefKind.UnlockProduct:   return "Unlock_Product";
                case DefKind.UnlockMachine:   return "Unlock_Machine";
                case DefKind.Texture:         return "Assets/NewTexture";
                case DefKind.MaterialLoose:   return "Assets/NewLooseMat";
                case DefKind.MaterialTexture: return "Assets/NewTexMat";
                case DefKind.PrefabBox:       return "Assets/NewBoxPrefab";
                case DefKind.UnitPrefab:      return "Assets/NewUnitPrefab";
                case DefKind.Generator:       return "NewGenerator";
                case DefKind.ToolbarCategory: return "NewCategory";
                case DefKind.EditMachinePorts:return "EditMachinePorts";
                case DefKind.BuildMachine:    return "NewMachine";
                case DefKind.Housing:               return "NewHousing";
                case DefKind.SettlementDecoration:  return "NewDecoration";
                case DefKind.SettlementFood:        return "NewFoodModule";
                case DefKind.SettlementIsp:         return "NewIspModule";
                case DefKind.Hospital:              return "NewHospital";
                case DefKind.MineTower:             return "NewMineTower";
                case DefKind.ResearchLab:           return "NewResearchLab";
                case DefKind.NuclearReactor:        return "NewNuclearReactor";
                case DefKind.EditNuclearReactorFuels: return "EditReactorFuels";
                case DefKind.EditNuclearReactorFluids: return "EditReactorFluids";
                case DefKind.EditNuclearReactorEnrichment: return "EditReactorEnrichment";
                case DefKind.EditNuclearReactorPorts: return "EditReactorPorts";
                default:                            return "NewDef";
            }
        }

        // Concrete-type construction with kind-appropriate defaults. Each
        // case picks a starting shape the modder can extend in the form
        // (e.g. recipes start with no machine so the picker prompts; loose
        // products start with isStorable=true since that's the common case).
        private static DefBase createDef(DefKind kind, string id) {
            switch (kind) {
                case DefKind.Recipe:
                    return new RecipeDef { RecipeId = id, Name = "New recipe", Description = "" };
                case DefKind.EditRecipe:
                    return new EditRecipeDef { RecipeId = id };
                case DefKind.Research:
                    return new ResearchDef { ResearchId = id, Name = "New research", Description = "" };
                case DefKind.ProductLoose:
                    return new ProductLooseDef { ProductId = id, Name = "New loose product" };
                case DefKind.ProductFluid:
                    return new ProductFluidDef { ProductId = id, Name = "New fluid product" };
                case DefKind.ProductUnit:
                    return new ProductUnitDef { ProductId = id, Name = "New unit product" };
                case DefKind.UnlockRecipe:
                    // Unlock kinds compute DisplayId from their (research,
                    // machine, recipe) composite — no Id field to seed.
                    return new UnlockRecipeDef();
                case DefKind.UnlockProduct:
                    return new UnlockProductDef();
                case DefKind.UnlockMachine:
                    return new UnlockMachineDef();
                case DefKind.Texture:
                    return new TextureDef { Path = id };
                case DefKind.MaterialLoose:
                    return new MaterialLooseDef { Path = id };
                case DefKind.MaterialTexture:
                    return new MaterialTextureDef { Path = id };
                case DefKind.PrefabBox:
                    return new PrefabBoxDef { Path = id };
                case DefKind.UnitPrefab:
                    return new UnitPrefabDef { Path = id };
                case DefKind.Generator:
                    return new GeneratorDef { GeneratorId = id, Name = "New generator" };
                case DefKind.ToolbarCategory:
                    return new ToolbarCategoryDef { CategoryId = id, Name = "New category" };
                case DefKind.EditMachinePorts:
                    // No primary id field on edit_machine_ports itself —
                    // the modder picks the target machine in the editor.
                    return new EditMachinePortsDef();
                case DefKind.BuildMachine:
                    return new BuildMachineDef { MachineId = id, Name = "New machine" };
                case DefKind.Housing:
                    return new HousingDef { HousingId = id, Name = "New housing" };
                case DefKind.SettlementDecoration:
                    return new SettlementDecorationDef { DecorationId = id, Name = "New decoration" };
                case DefKind.SettlementFood:
                    return new SettlementFoodDef { FoodModuleId = id, Name = "New food module" };
                case DefKind.SettlementIsp:
                    return new SettlementIspDef { IspModuleId = id, Name = "New ISP module" };
                case DefKind.Hospital:
                    return new HospitalDef { HospitalId = id, Name = "New hospital" };
                case DefKind.MineTower:
                    return new MineTowerDef { MineTowerId = id, Name = "New mine tower" };
                case DefKind.ResearchLab:
                    return new ResearchLabDef { ResearchLabId = id, Name = "New research lab" };
                case DefKind.NuclearReactor:
                    return new NuclearReactorDef { ReactorId = id, Name = "New nuclear reactor" };
                case DefKind.EditNuclearReactorFuels:
                    // No primary id — the modder picks the target reactor in the editor.
                    return new EditNuclearReactorFuelsDef();
                case DefKind.EditNuclearReactorFluids:
                    return new EditNuclearReactorFluidsDef();
                case DefKind.EditNuclearReactorEnrichment:
                    return new EditNuclearReactorEnrichmentDef();
                case DefKind.EditNuclearReactorPorts:
                    return new EditNuclearReactorPortsDef();
                default:
                    return null;
            }
        }

        // "0_" prefix sorts before "1_" under Ordinal comparison, pinning
        // __init__.py at the top; "1_" groups everything else alphabetically.
        private static string sortKeyFor(string absolutePath) {
            string fileName = Path.GetFileName(absolutePath ?? "");
            return fileName == "__init__.py"
                ? "0_" + fileName
                : "1_" + fileName;
        }

        private static string displayLabelFor(RecipeDef r) {
            string body;
            string name = string.IsNullOrEmpty(r.Name) ? "" : r.Name;
            if (string.IsNullOrEmpty(name) || name == r.RecipeId) {
                body = r.RecipeId ?? "<no id>";
            } else {
                if (name.Length > 40) name = name.Substring(0, 37) + "...";
                body = r.RecipeId + "  —  " + name;
            }
            return prependVariableName(r, body);
        }

        // Pull the def's Python-source variable binding (set by PackLoader
        // when the call shape was `name = build_*(...)`) and prepend it as
        // `name = ` so the modder can spot which AST handle each row maps
        // to. Null/empty VariableName passes through unchanged.
        private static string prependVariableName(DefBase def, string body) {
            if (def == null || string.IsNullOrEmpty(def.VariableName)) return body;
            return def.VariableName + " = " + body;
        }

        // Render a Block.statements list as nested tree rows. Mirrors the AST
        // exactly: EvaluateStatement → if it maps to a captured RecipeDef /
        // UnknownDef, render a clickable row; IfStatement → render the full
        // if/elif/else chain (each clause becomes its own CollapsibleGroup
        // with its body's statements rendered recursively inside).
        //
        // The lexer collapses an if/elif/else chain into the LAST clause's
        // IfStatement and links earlier clauses via Parent. We rebuild the
        // chain in source order (outermost-if → last-clause) by walking the
        // Parent chain and reversing, so the tree presents clauses in the
        // order they appear in the file.
        private void renderStatementsTree(
                System.Collections.Generic.List<PythonAPI.Statements.IStatement> statements,
                UiComponent container,
                System.Collections.Generic.Dictionary<string,
                    System.Collections.Generic.List<DefBase>> defsByScopeRun,
                string[] sourceLines,
                string sourceFile,
                string scopeKey) {
            // Tree position is driven by the model's per-(scope, run) ordering,
            // not by per-statement AST lookups — that way drag operations on
            // a row simply mutate the model list and the next rebuildTree
            // call shows the new order without any source-file rewrite.
            //
            // The AST still defines the SCOPE STRUCTURE: each if-chain at
            // this scope acts as a fixed boundary that splits the surrounding
            // run into "before" and "after" halves. We walk the AST only to
            // discover those boundaries (and to render clause headers); the
            // defs themselves come from defsByScopeRun in model order.
            int currentRun = 0;
            foreach (PythonAPI.Statements.IStatement stmt in statements) {
                if (stmt is PythonAPI.Statements.IfStatement ifs) {
                    // Flush the run that ended at this if-chain BEFORE
                    // rendering the clause groups, so the clause sits
                    // visually between its before-run and after-run.
                    renderRun(container, defsByScopeRun, scopeKey, currentRun);
                    currentRun++;

                    // Walk Parent chain and reverse so we render from the
                    // outermost `if` clause downward to the final `else`.
                    System.Collections.Generic.List<PythonAPI.Statements.IfStatement> chain =
                        new System.Collections.Generic.List<PythonAPI.Statements.IfStatement>();
                    PythonAPI.Statements.IfStatement walker = ifs;
                    while (walker != null) {
                        chain.Add(walker);
                        walker = walker.Parent;
                    }
                    chain.Reverse();
                    // True when the chain already terminates with an
                    // `else` clause — Condition is null on the else
                    // IfStatement. Suppresses the "+ add else" button on
                    // the last group so we don't end up with two else
                    // clauses on the same chain.
                    bool chainHasElse = chain.Count > 0
                        && chain[chain.Count - 1].Condition == null;
                    for (int i = 0; i < chain.Count; i++) {
                        PythonAPI.Statements.IfStatement clause = chain[i];
                        string header = readSourceLineTrimmed(sourceLines, clause.StartLine)
                            ?? (clause.Condition != null ? "if/elif:" : "else:");
                        var clauseGroup = new CollapsibleGroup(
                            new LocStrFormatted("🔀 " + header), expanded: true);
                        // For `if`/`elif` clauses, synthesize an IfBlockDef so
                        // the condition becomes a clickable tree row that
                        // opens the proper right-pane editor (mode dropdown +
                        // product picker). `else` has no condition to edit, so
                        // it just hosts its children directly.
                        if (clause.Condition != null && sourceFile != null) {
                            IfBlockDef ifBlock = new IfBlockDef {
                                Condition = extractConditionFromHeaderLine(
                                    readSourceLineTrimmed(sourceLines, clause.StartLine)),
                                SourceFile = sourceFile,
                                SourceStartLine = clause.StartLine,
                                SourceEndLine = clause.StartLine,
                                AstStartLine = clause.StartLine,
                            };
                            clauseGroup.Body.Add(buildTreeRow(ifBlock));
                        }
                        if (clause.Block != null) {
                            renderStatementsTree(clause.Block.statements, clauseGroup.Body,
                                                 defsByScopeRun, sourceLines, sourceFile,
                                                 scopeKey: "clause:" + clause.StartLine);
                        }

                        // "+ add definition" inside this clause's body —
                        // splices the new def into the clause with proper
                        // body indentation (same code path the file-level
                        // popup uses, but routed through AppendDefIntoClause).
                        if (sourceFile != null) {
                            string targetFileForClause = sourceFile;
                            int clauseHeaderLine = clause.StartLine;
                            ButtonText addInClauseBtn = clauseGroup.Body.AddAndReturn(new ButtonText(
                                new LocStrFormatted("+ add definition inside this clause…"), null));
                            addInClauseBtn.OnClick(() =>
                                openAddDefIntoClausePopup(addInClauseBtn, targetFileForClause, clauseHeaderLine));
                        }

                        // "+ add else" on the LAST clause when the chain
                        // doesn't already end with one. The else's header
                        // gets the same leading indent as the chain's
                        // opening if.
                        bool isLastClause = i == chain.Count - 1;
                        if (isLastClause && !chainHasElse && sourceFile != null) {
                            PythonAPI.Statements.IfStatement openingIf = chain[0];
                            int afterLine = clause.EndLine > 0 ? clause.EndLine : clause.StartLine;
                            string leadingIndent = readLeadingIndentForLine(sourceLines, openingIf.StartLine);
                            string targetFileForElse = sourceFile;
                            clauseGroup.Body.Add(new ButtonText(
                                new LocStrFormatted("+ add else clause"),
                                () => onAddElseToChain(targetFileForElse, afterLine, leadingIndent)));
                        }

                        container.Add(clauseGroup);
                    }
                }
                // Every other statement (EvaluateStatement, AssignmentStatement,
                // FunctionDef, …) is non-clausal and contributes nothing to
                // the run boundary — the captured defs are sourced through
                // defsByScopeRun, so we don't render here. Pure helper code
                // (no captured def) passes through invisibly because it
                // never made it into the model.
            }
            // Flush the final run (everything at this scope after the last
            // if-chain, OR the only run when no clauses exist at all).
            renderRun(container, defsByScopeRun, scopeKey, currentRun);
        }

        // Pull the (scope, run) bucket from defsByScopeRun in current model
        // order and add a row per def into a dedicated Column that hosts a
        // Reorderable manipulator on each row. The Column is the "drag
        // arena" for this run — Reorderable confines each row's drag to
        // its parent.contentContainer, so wrapping the run in its own
        // Column is what keeps reorder strictly within (scope, run).
        // No-op when the bucket is empty (a leading/trailing if-chain
        // leaves its adjacent run with zero defs).
        private void renderRun(
                UiComponent container,
                System.Collections.Generic.Dictionary<string,
                    System.Collections.Generic.List<DefBase>> defsByScopeRun,
                string scopeKey,
                int runIndex) {
            string key = (scopeKey ?? "top") + "#" + runIndex;
            if (!defsByScopeRun.TryGetValue(key, out var defs) || defs.Count == 0) return;

            Column runColumn = new Column();
            // Stretch each row to the run-column's full width so flex-
            // distributed children (drag handle | label | trash) get
            // proper bounds. Without this the row sizes to its content,
            // which pushes long-label rows past the tree panel's right
            // edge.
            runColumn.AlignItemsStretch();
            // Keep a reference to the def list so the drag callback can
            // translate visual order changes into in-memory reorderings of
            // model.Definitions. The list captured here is the same one
            // referenced by defsByScopeRun, so it mutates in place and
            // future rebuildTree calls pick up the new order without any
            // extra plumbing.
            System.Collections.Generic.List<DefBase> defsRef = defs;
            foreach (DefBase def in defsRef) {
                UiComponent row = buildTreeRow(def, onReordered: (oldIdx, newIdx) =>
                    onRunReordered(defsRef, oldIdx, newIdx));
                runColumn.Add(row);
            }
            container.Add(runColumn);
        }

        // Splice a new condition into the if/elif header line. Preserves the
        // original line's leading indent so a nested-if's header doesn't lose
        // its column position. Keyword (if vs elif) is re-detected from the
        // header line so the editor doesn't need to carry it. RescanPack +
        // reload propagates the new condition into the AST + PackModel so the
        // tree refreshes.
        private void saveIfCondition(IfBlockDef def, string newCondition) {
            if (def == null) return;
            if (m_currentPack == null) return;
            string sourceFile = def.SourceFile;
            int headerLine = def.SourceStartLine;
            if (string.IsNullOrEmpty(sourceFile) || headerLine < 1) return;
            try {
                string[] lines = File.ReadAllLines(sourceFile, Encoding.UTF8);
                if (headerLine > lines.Length) return;
                string original = lines[headerLine - 1];
                int j = 0;
                while (j < original.Length && (original[j] == ' ' || original[j] == '\t')) j++;
                string indent = original.Substring(0, j);
                string body = original.Substring(j);
                string keyword = body.StartsWith("elif ") ? "elif"
                                : body.StartsWith("if ")   ? "if"
                                : null;
                if (keyword == null) return;
                string trimmed = (newCondition ?? "").Trim();
                lines[headerLine - 1] = indent + keyword + " " + trimmed + ":";
                File.WriteAllLines(sourceFile, lines,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                PackRegistry.RescanPack(m_currentPack);
                m_currentModel = PackLoader.Load(m_currentPack);
                // Selection by recipe id survives the reload via the
                // existing onSavePack pattern; reuse that mechanism by
                // calling rebuildTree + leaving selection alone.
                rebuildTree();
                Log.Info("RecipeEditor: condition updated in " + Path.GetFileName(sourceFile)
                         + ":" + headerLine);
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: condition save failed — " + ex.Message);
            }
        }

        // One clickable row for a captured definition. Recipes route through
        // onRecipeSelected (full editor); other defs through onOtherDefSelected
        // (read-only placeholder until typed forms land).
        // Single tree row: [drag handle] [● dirty] [label button] [🗑 trash]
        //
        //  * Label-button click selects the def into the right pane. Selected
        //    row carries Cls.selected so applySelectionHighlight can flip the
        //    style without rebuilding the tree.
        //  * Trash button: plain click pops a confirm-popup; Shift+LMB skips
        //    the popup (power-user shortcut). Both paths funnel through
        //    onDeleteDefFromFile so the deletion behaviour stays in one place.
        //  * Drag handle is the Reorderable's grip; the manipulator is added
        //    to the row itself, so dropping rearranges the run's children in
        //    place. onReordered translates the new DOM order into a mutation
        //    of the run's def list (which is the same instance the model
        //    holds via Definitions filtered by ScopeKey/RunIndex).
        //
        //  IfBlockDef rows are clause-header editors, not run members: they
        //  skip the drag handle and trash icon to keep clauses fixed.
        private UiComponent buildTreeRow(DefBase def, System.Action<int, int> onReordered = null) {
            DefBase capturedDef = def;
            bool isClauseHeader = def is IfBlockDef;

            // Label / select button â€” Button.Area variant has built-in
            // hover + selected effects (Mafi docstring: "Suitable for
            // lists / menus"), so Cls.selected on the label visually
            // pops the highlighted row without us styling it ourselves.
            // Cls.group groups the row's hover/selected effect with its
            // siblings in the same draggable run.
            string labelText = def is RecipeDef rd ? displayLabelFor(rd) : displayLabelForDef(def);
            ButtonText selectBtn;
            if (def is RecipeDef rec) {
                RecipeDef cap = rec;
                selectBtn = new ButtonText(Button.Area, new LocStrFormatted(labelText),
                    () => onRecipeSelected(cap));
            } else {
                selectBtn = new ButtonText(Button.Area, new LocStrFormatted(labelText),
                    () => onOtherDefSelected(capturedDef));
            }
            selectBtn.FlexGrow(1f)
                     .FlexShrink(1f)
                     // MinWidth(0) lets flexbox shrink the button below its
                     // intrinsic content width — without it, a long label
                     // pushes the row past the panel's right edge (the
                     // overflow bug the modder hit on the coal-liquification
                     // pack). Combined with TextOverflow.Ellipsis the label
                     // truncates with "..." while the trash button stays
                     // visible.
                     .MinWidth(0.px())
                     .TextAlign(Mafi.Unity.UiToolkit.Component.TextAlignment.LeftMiddle)
                     .TextOverflow(Mafi.Unity.UiToolkit.Component.TextOverflow.Ellipsis)
                     .Class(Cls.group);
            selectBtn.ClassRootIff(Cls.selected, isCurrentlySelected(def));
            m_rowLabelByDef[def] = selectBtn;

            // Clause headers fall back to a plain row — no drag, no delete.
            if (isClauseHeader) return selectBtn;

            // Drag handle (left grip column). Reorderable confines drag to
            // its target.parent.contentContainer, so this row's container
            // (one Column per (scope, run)) is the drag arena.
            Column dragHandle = new Column();
            dragHandle.Class(Cls.dragHandle)
                      .AlignSelfStretch()
                      .Width(8.px());

            // Dirty marker. Hidden when clean; an orange "â—" when the def
            // has uncommitted in-memory changes (field edit or reorder).
            Label dirtyMarker = new Label(new LocStrFormatted(def.Dirty ? "â—" : ""));
            dirtyMarker.Width(10.px()).Color(ColorRgba.Orange);

            // Trash button. AttachConfirmationInline wraps OnClick with a
            // floating confirm popup; the MouseDown listener short-circuits
            // it when Shift+LMB is held so power-users can bypass the
            // popup. StopImmediatePropagation prevents the underlying
            // Clickable manipulator from then firing OnClick.
            ButtonIcon trash = new ButtonIcon(
                    Button.Danger,
                    "Assets/Unity/UserInterface/General/Trash128.png")
                .IconSize(14.px())
                .Tooltip(new LocStrFormatted("Delete (Shift+click to skip confirm)"));
            trash.AttachConfirmationInline(
                new LocStrFormatted("Delete"),
                () => new LocStrFormatted("Delete '"
                    + (def.DisplayId ?? def.Kind) + "'?"),
                () => onDeleteDefFromFile(capturedDef));
            trash.RootElement.RegisterCallback<UnityEngine.UIElements.MouseDownEvent>(evt => {
                if (evt.button == 0 && evt.shiftKey) {
                    onDeleteDefFromFile(capturedDef);
                    evt.StopImmediatePropagation();
                }
            });

            Row row = new Row();
            row.AlignItemsCenter().Gap(2.pt());
            row.Add(dragHandle);
            row.Add(dirtyMarker);
            row.Add(selectBtn);
            row.Add(trash);

            // Reorderable manipulator. drag-handle is the grip; OnOrderChanged
            // fires after drop with the row's new container index.
            if (onReordered != null) {
                Reorderable reorderable = new Reorderable(dragHandle.RootElement);
                System.Action<int, int> capturedCallback = onReordered;
                reorderable.OnOrderChanged += (oldIdx, newIdx) => capturedCallback(oldIdx, newIdx);
                row.AddManipulator(reorderable);
            }
            return row;
        }

        // Called by Reorderable after a successful drop. Mutates the run's
        // def list in place so the next rebuildTree (triggered here) shows
        // the new order. Marks every def in the run Dirty so the modder can
        // tell which file needs a save — reorder doesn't change individual
        // field values but it WILL change the on-disk layout, so until Save
        // runs the in-memory state diverges from the file.
        private void onRunReordered(System.Collections.Generic.List<DefBase> runDefs,
                int oldIdx, int newIdx) {
            if (runDefs == null) return;
            if (oldIdx < 0 || oldIdx >= runDefs.Count) return;
            if (newIdx < 0 || newIdx >= runDefs.Count) return;
            if (oldIdx == newIdx) return;
            DefBase moved = runDefs[oldIdx];
            runDefs.RemoveAt(oldIdx);
            runDefs.Insert(newIdx, moved);
            foreach (DefBase d in runDefs) d.Dirty = true;
            // model.Definitions also needs to mirror the new order so
            // PackEmitter's eventual region-rewrite sees it. We walk the
            // run's defs in their NEW order and reassign them to the same
            // set of indices in model.Definitions that they originally
            // occupied â€” that keeps cross-run defs untouched.
            if (m_currentModel != null) {
                System.Collections.Generic.List<int> indices =
                    new System.Collections.Generic.List<int>(runDefs.Count);
                for (int i = 0; i < m_currentModel.Definitions.Count; i++) {
                    if (runDefs.Contains(m_currentModel.Definitions[i])) {
                        indices.Add(i);
                    }
                }
                if (indices.Count == runDefs.Count) {
                    for (int i = 0; i < indices.Count; i++) {
                        m_currentModel.Definitions[indices[i]] = runDefs[i];
                    }
                }
            }
            rebuildTree();
        }

        // True when this def is what the right pane currently shows. Used
        // by buildTreeRow to seed the Cls.selected class on the row's
        // label button so the modder can scan the tree and see which row
        // is loaded into the editor.
        private bool isCurrentlySelected(DefBase def) {
            if (def is RecipeDef rd) return ReferenceEquals(m_selectedRecipe, rd);
            return ReferenceEquals(m_selectedOther, def);
        }

        // Toggle Cls.selected on each tracked row's label button to match
        // the current m_selectedRecipe / m_selectedOther values. Cheaper
        // than rebuilding the tree on every click; called from the two
        // selection handlers below.
        private void applySelectionHighlight() {
            foreach (var kvp in m_rowLabelByDef) {
                if (kvp.Value == null) continue;
                kvp.Value.ClassRootIff(Cls.selected, isCurrentlySelected(kvp.Key));
            }
        }

        // Read line `n` (1-based) from the source-line array, returning the
        // trimmed text. Returns null when the line index is out of range or
        // the array is null. Used as the if/elif/else clause label.
        private static string readSourceLineTrimmed(string[] sourceLines, int oneBasedLine) {
            if (sourceLines == null) return null;
            if (oneBasedLine < 1 || oneBasedLine > sourceLines.Length) return null;
            return sourceLines[oneBasedLine - 1].Trim();
        }

        // Extract the condition expression text from an `if <expr>:` /
        // `elif <expr>:` header line (already trimmed). Returns "" when the
        // line doesn't match either keyword — the editor still opens with an
        // empty Custom field so the modder can type one in.
        private static string extractConditionFromHeaderLine(string trimmedHeader) {
            if (string.IsNullOrEmpty(trimmedHeader)) return "";
            string keyword = trimmedHeader.StartsWith("elif ") ? "elif"
                            : trimmedHeader.StartsWith("if ")   ? "if"
                            : null;
            if (keyword == null) return "";
            string body = trimmedHeader.Substring(keyword.Length + 1);
            int colon = body.LastIndexOf(':');
            if (colon >= 0) body = body.Substring(0, colon);
            return body.Trim();
        }

        // Best-effort file read; the caller falls back to a generic clause
        // label when this returns null.
        private static string[] tryReadAllLines(string path) {
            try { return File.ReadAllLines(path, Encoding.UTF8); }
            catch { return null; }
        }

        private void onRecipeSelected(RecipeDef recipe) {
            m_selectedRecipe = recipe;
            m_selectedOther  = null;
            applySelectionHighlight();
            m_statementColumn.Clear();
            if (recipe == null) return;

            // Mirrors onOtherDefSelected layout: bold header → comment →
            // editor body → source label → Save-this-entry. Keeping the
            // dispatcher in charge of header/comment/source/save means
            // RecipeDefEditor focuses purely on the recipe-specific
            // fields, matching every other typed editor.
            m_statementColumn.Add(new Label(new LocStrFormatted(recipe.Kind + " — "
                + (string.IsNullOrEmpty(recipe.DisplayId) ? "<no id>" : recipe.DisplayId)))
                .FontBold());

            RecipeDef commentTarget = recipe;
            m_statementColumn.Add(labeledField("comment (notes shown above the recipe)",
                new TextField()
                    .Multiline(true)
                    .Text(recipe.Comment ?? "")
                    .SetTextAreaMinHeight(36.px())
                    .OnValueChanged(v => commentTarget.Comment = string.IsNullOrEmpty(v) ? null : v)));

            // Variable-name field — when set the emitter writes
            // `<name> = build_recipe(...)` so downstream code that
            // references this recipe by variable still resolves on the
            // next pack load. Blank means "emit as a bare expression
            // statement" (the typical recipe shape).
            m_statementColumn.Add(labeledField("variable name (assigns the def to this Python variable; blank = none)",
                new TextField()
                    .Text(recipe.VariableName ?? "")
                    .OnValueChanged(v => commentTarget.VariableName = string.IsNullOrEmpty(v) ? null : v)));

            showEditor<RecipeDef>(recipe,
                () => new Editors.RecipeDefEditor(m_uiContext.ProtosDb, m_currentModel));

            if (!string.IsNullOrEmpty(recipe.SourceFile)) {
                m_statementColumn.Add(new Label(new LocStrFormatted(
                    "source: " + Path.GetFileName(recipe.SourceFile) +
                    " (lines " + recipe.SourceStartLine + "–" + recipe.SourceEndLine + ")")));
            }
            appendIssueRowsForDef(recipe);

            rebuildEditorFooter(recipe, includeVerify: true);
        }

        // Tree label for a non-recipe definition: "[<kind>] <id>  —  <name>"
        // when a display name is present, "[<kind>] <id>" otherwise. The
        // kind tag is bracketed so the eye can scan a file's contents and
        // distinguish recipes from product/research/asset entries at a
        // glance.
        private static string displayLabelForDef(DefBase def) {
            string id = string.IsNullOrEmpty(def.DisplayId) ? "<no id>" : def.DisplayId;
            string label = "[" + def.Kind + "] " + id;
            string name = def.DisplayName;
            if (!string.IsNullOrEmpty(name) && name != id) {
                if (name.Length > 40) label += "  —  " + name.Substring(0, 37) + "...";
                else label += "  —  " + name;
            }
            return prependVariableName(def, label);
        }

        // Selection handler for non-recipe definitions. Dispatches per Def
        // kind: typed Defs (ResearchDef, UnlockRecipeDef, UnlockProductDef,
        // UnlockMachineDef) render a per-kind form so the modder gets proper
        // pickers and validation; UnknownDef falls back to the raw-source
        // multiline editor so unrecognised kinds remain editable.
        //
        // m_selectedOther tracks any non-recipe selection (typed or unknown)
        // so onSavePack can re-select the same definition by id after reload.
        private void onOtherDefSelected(DefBase def) {
            m_selectedRecipe = null;
            m_selectedOther  = def;
            applySelectionHighlight();
            m_statementColumn.Clear();
            m_statementColumn.Add(new Label(new LocStrFormatted(def.Kind + " — "
                + (string.IsNullOrEmpty(def.DisplayId) ? "<no id>" : def.DisplayId))).FontBold());

            // Comment editor — shared across all Def kinds since it lives on
            // DefBase. Multi-line, no placeholder (see TextField placeholder
            // quirk memory).
            m_statementColumn.Add(labeledField("comment (notes shown above the statement)",
                new TextField()
                    .Multiline(true)
                    .Text(def.Comment ?? "")
                    .SetTextAreaMinHeight(36.px())
                    .OnValueChanged(v => def.Comment = string.IsNullOrEmpty(v) ? null : v)));

            // Variable-name field — when set the emitter writes
            // `<name> = <call>(...)` so downstream code that references
            // this def by variable (e.g. `material = filter_media_mat`)
            // still resolves on next load. Blank means "emit as a bare
            // expression statement". Skipped for IfBlockDef since `if`
            // clauses aren't assignable expressions in Python.
            if (!(def is IfBlockDef)) {
                m_statementColumn.Add(labeledField("variable name (assigns the def to this Python variable; blank = none)",
                    new TextField()
                        .Text(def.VariableName ?? "")
                        .OnValueChanged(v => def.VariableName = string.IsNullOrEmpty(v) ? null : v)));
            }

            // Per-kind body. Typed editors follow the `DefEditor<T>` pattern:
            // constructed once and cached, rebinds via `Value(...)` on each
            // selection. The legacy `buildEditRecipeForm` stays in place
            // because it depends on the recipe form's product-list editor
            // helpers, which haven't been migrated yet. UnknownDef falls
            // back to the raw-source multiline editor.
            //
            // Wrapped in try/catch so a typed editor's constructor blowing
            // up (missing field, null-deref in a binding) surfaces a visible
            // banner + full stack in Player.log instead of leaving the right
            // pane silently empty.
            try {
                if      (def is IfBlockDef ib)         showEditor<IfBlockDef>(ib, () => new Editors.IfBlockDefEditor(m_uiContext.ProtosDb, saveIfCondition));
                else if (def is ResearchDef rs)        showEditor<ResearchDef>(rs, () => new Editors.ResearchDefEditor(m_currentPack, m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockRecipeDef ur)    showEditor<UnlockRecipeDef>(ur, () => new Editors.UnlockRecipeDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockProductDef up)   showEditor<UnlockProductDef>(up, () => new Editors.UnlockProductDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockMachineDef um)   showEditor<UnlockMachineDef>(um, () => new Editors.UnlockMachineDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is ProductLooseDef pl)    showEditor<ProductLooseDef>(pl, () => new Editors.ProductLooseDefEditor(m_currentPack, m_currentModel, m_uiContext.ProtosDb));
                else if (def is ProductFluidDef pf)    showEditor<ProductFluidDef>(pf, () => new Editors.ProductFluidDefEditor(m_currentPack, m_currentModel, m_uiContext.ProtosDb));
                else if (def is ProductUnitDef pu)     showEditor<ProductUnitDef>(pu, () => new Editors.ProductUnitDefEditor(m_currentPack, m_currentModel, m_uiContext.ProtosDb));
                else if (def is TextureDef tx)         showEditor<TextureDef>(tx, () => new Editors.TextureDefEditor(m_currentPack));
                else if (def is MaterialLooseDef ml)   showEditor<MaterialLooseDef>(ml, () => new Editors.MaterialLooseDefEditor(m_currentPack, m_currentModel));
                else if (def is MaterialTextureDef mt) showEditor<MaterialTextureDef>(mt, () => new Editors.MaterialTextureDefEditor(m_currentPack, m_currentModel));
                else if (def is PrefabBoxDef pb)       showEditor<PrefabBoxDef>(pb, () => new Editors.PrefabBoxDefEditor(m_currentPack, m_currentModel));
                else if (def is UnitPrefabDef upr)     showEditor<UnitPrefabDef>(upr, () => new Editors.UnitPrefabDefEditor(m_currentPack, m_currentModel, m_uiContext));
                else if (def is ToolbarCategoryDef tc) showEditor<ToolbarCategoryDef>(tc, () => new Editors.ToolbarCategoryDefEditor(m_currentPack, m_currentModel, m_uiContext.ProtosDb));
                else if (def is GeneratorDef gd)       showEditor<GeneratorDef>(gd, () => new Editors.GeneratorDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditRecipeDef er)      showEditor<EditRecipeDef>(er, () => new Editors.EditRecipeDefEditor(m_uiContext.ProtosDb, m_currentModel));
                else if (def is EditMachinePortsDef emp) showEditor<EditMachinePortsDef>(emp, () => new Editors.EditMachinePortsDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is BuildMachineDef bmd)   showEditor<BuildMachineDef>(bmd, () => new Editors.BuildMachineDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is HousingDef hd)         showEditor<HousingDef>(hd, () => new Editors.HousingDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is SettlementDecorationDef sdd) showEditor<SettlementDecorationDef>(sdd, () => new Editors.SettlementDecorationDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is SettlementFoodDef sfd) showEditor<SettlementFoodDef>(sfd, () => new Editors.SettlementFoodDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is SettlementIspDef sid)  showEditor<SettlementIspDef>(sid, () => new Editors.SettlementIspDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is HospitalDef hpd)       showEditor<HospitalDef>(hpd, () => new Editors.HospitalDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is MineTowerDef mtd)      showEditor<MineTowerDef>(mtd, () => new Editors.MineTowerDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is ResearchLabDef rld)    showEditor<ResearchLabDef>(rld, () => new Editors.ResearchLabDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is NuclearReactorDef nrd) showEditor<NuclearReactorDef>(nrd, () => new Editors.NuclearReactorDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditNuclearReactorFuelsDef enrf) showEditor<EditNuclearReactorFuelsDef>(enrf, () => new Editors.EditNuclearReactorFuelsDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditNuclearReactorFluidsDef enrfl) showEditor<EditNuclearReactorFluidsDef>(enrfl, () => new Editors.EditNuclearReactorFluidsDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditNuclearReactorPortsDef enrp) showEditor<EditNuclearReactorPortsDef>(enrp, () => new Editors.EditNuclearReactorPortsDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditNuclearReactorEnrichmentDef enren) showEditor<EditNuclearReactorEnrichmentDef>(enren, () => new Editors.EditNuclearReactorEnrichmentDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnknownDef other)      buildRawSourceForm(other);
                else m_statementColumn.Add(new Label(new LocStrFormatted(
                    "(no editor for this definition kind yet)"))
                    .Color(ColorRgba.LightGray));
            } catch (Exception editorEx) {
                Log.Exception(editorEx);
                Log.Warning("RecipeEditor: editor for kind '" + def.Kind
                    + "' threw â€” " + editorEx.GetType().Name + ": " + editorEx.Message);
                m_statementColumn.Add(new Label(new LocStrFormatted(
                    "(editor crashed â€” see Player.log: " + editorEx.GetType().Name
                    + ": " + editorEx.Message + ")"))
                    .Color(ColorRgba.Red));
            }

            if (!string.IsNullOrEmpty(def.SourceFile)) {
                m_statementColumn.Add(new Label(new LocStrFormatted(
                    "source: " + Path.GetFileName(def.SourceFile) +
                    " (lines " + def.SourceStartLine + "–" + def.SourceEndLine + ")")));
            }
            appendIssueRowsForDef(def);

            // Action buttons (Save, Duplicate) live in the editor's footer
            // — see rebuildEditorFooter. The footer is anchored to the
            // bottom of the editor pane so it stays visible while the
            // scrollable body above scrolls. Per-entry save splices ONLY
            // this def back into its source file so a single edit doesn't
            // drag every other dirty entry in the model along with it.
            rebuildEditorFooter(def, includeVerify: false);
        }

        // Remove the def from the in-memory model so the next save splices
        // it out of source. Source-file rewriting is line-range-based —
        // PackEmitter detects the missing entry and deletes the
        // corresponding lines from the .py file. Clears the current
        // selection (the just-deleted def shouldn't stay editable) and
        // rebuilds the tree so the row disappears immediately.
        private void onDeleteDefFromFile(DefBase def) {
            if (m_currentModel == null || def == null) return;
            bool wasOnDisk = !string.IsNullOrEmpty(def.SourceFile) && def.SourceStartLine > 0;
            string sourceFileName = Path.GetFileName(def.SourceFile ?? "");
            string displayId = def.DisplayId ?? def.Kind;

            m_currentModel.Definitions.Remove(def);
            if (ReferenceEquals(m_selectedOther, def)) m_selectedOther = null;
            if (def is RecipeDef rd && ReferenceEquals(m_selectedRecipe, rd))
                m_selectedRecipe = null;

            // Commit the deletion to disk RIGHT NOW. The earlier "save
            // later" flow had a bug: onSavePack only re-emits defs still
            // in the model, so the deleted def's lines lingered on disk
            // and the next reload pulled it back into the model.
            if (wasOnDisk && m_currentPack != null) {
                try {
                    PackEmitter.DeleteDef(def);
                    PackRegistry.RescanPack(m_currentPack);
                    m_currentModel = PackLoader.Load(m_currentPack);
                    Log.Info("RecipeEditor: deleted '" + displayId + "' from "
                        + sourceFileName);
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: delete failed - " + ex.Message);
                }
            }

            rebuildTree();
            showEmptyStatement("Deleted '" + displayId + "'"
                + (wasOnDisk ? (" from " + sourceFileName + ".") : "."));
        }

        // Append every PackValidator issue whose source file + line range
        // overlaps this def to the statement column as a red warning row.
        // Currently the only check is use-before-definition; rendering as
        // one row per issue keeps the surface usable when a single def
        // references several undefined variables.
        private void appendIssueRowsForDef(DefBase def) {
            if (m_currentModel?.Issues == null || def == null) return;
            string defFile = def.SourceFile;
            if (string.IsNullOrEmpty(defFile)) return;
            foreach (PackIssue issue in m_currentModel.Issues) {
                if (!string.Equals(issue.SourceFile, defFile, StringComparison.Ordinal)) continue;
                // Use-before-def issues are flagged at the reference line.
                // Show every issue whose line falls inside (or at) this
                // def's source range so the modder sees them in-context.
                if (issue.Line < def.SourceStartLine || issue.Line > def.SourceEndLine) continue;
                m_statementColumn.Add(new Label(new LocStrFormatted("⚠ " + issue.Message))
                    .Color(ColorRgba.Red));
            }
        }


        // Fallback raw-text editor for UnknownDef (definition kinds we don't
        // have typed forms for yet). PackEmitter splices the edited text
        // back at the original line range on save.
        private void buildRawSourceForm(UnknownDef other) {
            if (other.RawSource != null) {
                m_statementColumn.Add(new Label(new LocStrFormatted(
                    "Raw source — edit and Save to splice the new text back at the original line range:")));
                TextField raw = new TextField()
                    .Multiline(true)
                    .Text(other.RawSource)
                    .SetTextAreaMinHeight(220.px())
                    .OnValueChanged(v => other.RawSource = v ?? "");
                raw.Class(Cls.fontMonospace);
                m_statementColumn.Add(raw);
            } else {
                m_statementColumn.Add(new Label(new LocStrFormatted(
                    "(no captured source — definition came from a non-file source)"))
                    .Color(ColorRgba.LightGray));
            }
        }


        // Persist a single entry. Splices ONLY this def back into its source
        // file via PackEmitter.SaveDef so unrelated edits in other entries
        // stay in memory until the modder explicitly saves them too. Other
        // entries in the same file keep their Dirty markers; the saved
        // entry's Dirty flag clears. RescanPack runs so subsequent edits on
        // this def see fresh line ranges after the splice shifted them.
        private void onSaveDef(DefBase def) {
            if (def == null || m_currentPack == null) {
                Log.Info("RecipeEditor: SaveDef clicked but no def/pack selected.");
                return;
            }
            // Refuse to write a new def while mandatory fields are still
            // empty. Without this guard the emitter would happily produce
            // a `<call>(machine = None, ...)` line which crashes the mod
            // loader on next pack reload. The modder sees the missing
            // fields in the log + the in-memory def stays editable.
            Mafi.Collections.Lyst<string> missing = def.MissingMandatoryFields();
            if (missing != null && missing.Count > 0) {
                DiagnosticTrace.Step($"onSaveDef[{def.Kind}/{def.DisplayId}]: blocked — missing {string.Join(",", missing)}");
                Log.Warning("RecipeEditor: cannot save '" + (def.DisplayId ?? def.Kind)
                    + "' — required fields still empty: " + string.Join(", ", missing));
                return;
            }
            // Freshly added defs without a source range yet can't go through
            // the single-entry path â€” they need the appended-defs flow that
            // PackEmitter.Save runs. Fall back to onSavePack in that case.
            if (string.IsNullOrEmpty(def.SourceFile) || def.SourceStartLine <= 0) {
                DiagnosticTrace.Step($"onSaveDef[{def.Kind}/{def.DisplayId}]: no SourceFile/StartLine → onSavePack");
                onSavePack();
                return;
            }
            DiagnosticTrace.Step($"onSaveDef[{def.Kind}/{def.DisplayId}]: BEGIN file={Path.GetFileName(def.SourceFile)} lines={def.SourceStartLine}-{def.SourceEndLine}");
            try {
                DiagnosticTrace.Step("onSaveDef: PackEmitter.SaveDef BEGIN");
                PackEmitter.SaveDef(def, m_currentModel);
                DiagnosticTrace.Step("onSaveDef: PackEmitter.SaveDef END");
                DiagnosticTrace.Step("onSaveDef: PackRegistry.RescanPack BEGIN");
                PackRegistry.RescanPack(m_currentPack);
                DiagnosticTrace.Step("onSaveDef: PackRegistry.RescanPack END");
                string previousRecipeId = m_selectedRecipe?.RecipeId;
                string previousOtherKind = m_selectedOther?.Kind;
                string previousOtherId   = m_selectedOther?.DisplayId;
                DiagnosticTrace.Step("onSaveDef: PackLoader.Load BEGIN");
                m_currentModel = PackLoader.Load(m_currentPack);
                DiagnosticTrace.Step("onSaveDef: PackLoader.Load END");
                m_selectedRecipe = previousRecipeId != null
                    ? m_currentModel.Recipes.FirstOrDefault(r => r.RecipeId == previousRecipeId)
                    : null;
                m_selectedOther = (previousOtherKind != null && previousOtherId != null)
                    ? m_currentModel.OtherDefinitions
                        .FirstOrDefault(d => d.Kind == previousOtherKind && d.DisplayId == previousOtherId)
                    : null;
                DiagnosticTrace.Step("onSaveDef: rebuildTree BEGIN");
                rebuildTree();
                DiagnosticTrace.Step("onSaveDef: rebuildTree END");
                if (m_selectedRecipe != null) {
                    DiagnosticTrace.Step($"onSaveDef: onRecipeSelected[{m_selectedRecipe.RecipeId}] BEGIN");
                    onRecipeSelected(m_selectedRecipe);
                    DiagnosticTrace.Step("onSaveDef: onRecipeSelected END");
                } else if (m_selectedOther != null) {
                    DiagnosticTrace.Step($"onSaveDef: onOtherDefSelected[{m_selectedOther.Kind}/{m_selectedOther.DisplayId}] BEGIN");
                    onOtherDefSelected(m_selectedOther);
                    DiagnosticTrace.Step("onSaveDef: onOtherDefSelected END");
                }
                Log.Info("RecipeEditor: saved entry '"
                    + (def.DisplayId ?? def.Kind) + "' in "
                    + Path.GetFileName(def.SourceFile));
            } catch (Exception ex) {
                DiagnosticTrace.Step($"onSaveDef: THREW {ex.GetType().Name}: {ex.Message}");
                Log.Exception(ex);
                Log.Warning("RecipeEditor: SaveDef failed â€” " + ex.Message);
            }
        }

        // Persist edits made in the form. The new emitter (Duration.FromSec /
        // Quantity / typed-ref heuristic for IDs) plus the lexer's corrected
        // EndLine make this round-trip cleanly: load → edit → save → reload
        // produces the same RecipeDef values. Exceptions surface in Player.log;
        // a status banner in the editor is a follow-up.
        private void onSavePack() {
            if (m_currentModel == null || m_currentPack == null) {
                Log.Info("RecipeEditor: Save clicked but no pack/model selected.");
                return;
            }
            DiagnosticTrace.Step($"onSavePack[{m_currentPack.ModId}]: BEGIN recipes={m_currentModel.Recipes.Count()} other={m_currentModel.OtherDefinitions.Count()}");
            try {
                DiagnosticTrace.Step("onSavePack: PackEmitter.Save BEGIN");
                PackEmitter.Save(m_currentModel);
                DiagnosticTrace.Step("onSavePack: PackEmitter.Save END");
                // Re-tokenise + re-parse the pack so the next edit's
                // SourceStartLine / SourceEndLine reflect post-splice line
                // positions.
                DiagnosticTrace.Step("onSavePack: PackRegistry.RescanPack BEGIN");
                PackRegistry.RescanPack(m_currentPack);
                DiagnosticTrace.Step("onSavePack: PackRegistry.RescanPack END");
                // Preserve selection across reload — recipes by RecipeId,
                // other definitions by (CallName, Id). If neither matches the
                // form clears, but typically Save → reload → same selection
                // so the modder can keep editing without a click.
                string previousRecipeId = m_selectedRecipe?.RecipeId;
                string previousOtherKind = m_selectedOther?.Kind;
                string previousOtherId   = m_selectedOther?.DisplayId;
                DiagnosticTrace.Step("onSavePack: PackLoader.Load BEGIN");
                m_currentModel = PackLoader.Load(m_currentPack);
                DiagnosticTrace.Step("onSavePack: PackLoader.Load END");
                m_selectedRecipe = previousRecipeId != null
                    ? m_currentModel.Recipes.FirstOrDefault(r => r.RecipeId == previousRecipeId)
                    : null;
                // Re-select the same non-recipe definition by (Kind, DisplayId).
                // Kind matches across both typed Defs (ResearchDef.Kind ==
                // "research", etc.) and UnknownDef (Kind == CallName), so
                // one comparison handles both cases.
                m_selectedOther = (previousOtherKind != null && previousOtherId != null)
                    ? m_currentModel.OtherDefinitions
                        .FirstOrDefault(d => d.Kind == previousOtherKind && d.DisplayId == previousOtherId)
                    : null;
                DiagnosticTrace.Step("onSavePack: rebuildTree BEGIN");
                rebuildTree();
                DiagnosticTrace.Step("onSavePack: rebuildTree END");
                if (m_selectedRecipe != null) {
                    DiagnosticTrace.Step($"onSavePack: onRecipeSelected[{m_selectedRecipe.RecipeId}] BEGIN");
                    onRecipeSelected(m_selectedRecipe);
                    DiagnosticTrace.Step("onSavePack: onRecipeSelected END");
                } else if (m_selectedOther != null) {
                    DiagnosticTrace.Step($"onSavePack: onOtherDefSelected[{m_selectedOther.Kind}/{m_selectedOther.DisplayId}] BEGIN");
                    onOtherDefSelected(m_selectedOther);
                    DiagnosticTrace.Step("onSavePack: onOtherDefSelected END");
                } else {
                    showEmptyStatement("Saved. Select a recipe.");
                }
                Log.Info("RecipeEditor: saved " + m_currentModel.Recipes.Count()
                         + " recipe(s) + " + m_currentModel.OtherDefinitions.Count()
                         + " other def(s) to pack " + m_currentPack.ModId);
            } catch (Exception ex) {
                DiagnosticTrace.Step($"onSavePack: THREW {ex.GetType().Name}: {ex.Message}");
                Log.Exception(ex);
                Log.Warning("RecipeEditor: save failed — " + ex.Message);
            }
        }

        // Icon-path resolver for ResearchNodeProto. The proto type doesn't
        // implement IProtoWithIcon directly (icons live on a Gfx sub-struct),
        // so we hand-pick the first available source: Graphics.Icons[0] (the
        // canonical research icon array) first, then the unlocked-proto icon
        // from IconsProtos[0] as fallback. Empty/null result tells the picker
        // to render the row without a leading icon component.
        private static string researchIconPath(ResearchNodeProto node) {
            if (node?.Graphics == null) return null;
            if (!node.Graphics.Icons.IsEmpty) return node.Graphics.Icons[0];
            if (!node.Graphics.IconsProtos.IsEmpty) return node.Graphics.IconsProtos[0].IconPath;
            return null;
        }


        private static UiComponent labeledField(string label, UiComponent field) {
            // The container Column needs AlignItemsStretch so the inner field
            // fills horizontally; m_statementColumn's stretch propagates to
            // this Column, but propagation stops at the next Column unless we
            // re-stretch. The label naturally sizes to its text.
            Column col = new Column {
                new Label(new LocStrFormatted(label)),
                field
            };
            col.AlignItemsStretch();
            return col;
        }


        private void showEmptyStatement(string message) {
            m_statementColumn.Clear();
            m_statementColumn.Add(new Label(new LocStrFormatted(message)));
            // Empty pane → no def to act on. The footer stays visible but
            // every button is disabled so the modder sees the available
            // actions even when nothing is selected (Save / Duplicate
            // / Verify all greyed out).
            rebuildEditorFooter(null, includeVerify: false);
        }

        // ---- Editor footer + Duplicate --------------------------------------

        // Update the editor's bottom action bar for the currently-selected
        // def. Buttons stay attached to the footer for the window's
        // lifetime; this method only flips their Enabled state and stashes
        // the new target in m_footerTarget so the persistent click
        // handlers dispatch to the right def. The footer lives outside
        // the scrollable body so it stays visible whatever the editor's
        // content height is — modders editing long forms no longer have
        // to scroll to find Save.
        //
        // Verify Round-Trip is only meaningful for recipes (the verifier
        // round-trips the recipe form through the emitter and reloads it),
        // so it's disabled for every other selection. The button stays
        // visible in the footer either way so the panel doesn't reflow
        // between selections.
        private void rebuildEditorFooter(DefBase target, bool includeVerify) {
            m_footerTarget = target;
            if (m_saveBtn != null)      m_saveBtn.Enabled(target != null);
            if (m_duplicateBtn != null) m_duplicateBtn.Enabled(target != null);
            if (m_verifyBtn != null)    m_verifyBtn.Enabled(target != null && includeVerify);
        }

        // Deep-clone the selected def into a fresh entry, slot it into the
        // model right after the original, and select it so the modder lands
        // straight in its editor. The clone keeps SourceFile (so the new
        // entry stays in the same file) but resets SourceStartLine/EndLine
        // to 0 — PackEmitter treats that as "append at file end" on next
        // save, which is what we want for a freshly minted entry.
        //
        // Primary id is renamed to "<origId>_copy" (uniquified with a
        // numeric suffix if needed) so the new def doesn't collide with
        // the original in the model's id-keyed lookups. Unlock kinds use
        // a composite id and don't have a single rename target — those
        // duplicate verbatim and the modder edits the composite parts in
        // the form.
        private void onDuplicateDef(DefBase src) {
            if (m_currentModel == null || src == null) return;

            DefBase copy = cloneDef(src);
            if (copy == null) return;

            // Reset bookkeeping that should not transfer.
            copy.SourceStartLine = 0;
            copy.SourceEndLine   = 0;
            copy.AstStartLine    = 0;
            copy.Dirty           = true;

            string baseId = src.DisplayId;
            string proposed = string.IsNullOrEmpty(baseId) ? null : baseId + "_copy";
            if (!string.IsNullOrEmpty(proposed)) {
                proposed = uniquifyId(proposed);
                setPrimaryId(copy, proposed);
            }

            int idx = m_currentModel.Definitions.IndexOf(src);
            if (idx >= 0) m_currentModel.Definitions.Insert(idx + 1, copy);
            else          m_currentModel.Definitions.Add(copy);

            rebuildTree();
            if (copy is RecipeDef rd) onRecipeSelected(rd);
            else                      onOtherDefSelected(copy);
        }

        // Reflection-based field copy. We deliberately walk public fields
        // (not properties) — every DefBase subclass exposes its state as
        // plain public fields, and PackEmitter / PackLoader already round-
        // trip on that shape, so anything not visible here also wouldn't
        // round-trip through save/load. List<T> values get a fresh list
        // (with element-level clones for ProductRef so the two defs don't
        // share mutable rows); dictionaries are passed by reference because
        // SourceFileVariables is intentionally shared across every def in
        // the same file.
        private static DefBase cloneDef(DefBase src) {
            if (src == null) return null;
            System.Type t = src.GetType();
            DefBase copy;
            try {
                copy = (DefBase)System.Activator.CreateInstance(t);
            } catch (System.Exception ex) {
                Log.Warning("Duplicate: cannot instantiate '" + t.Name + "' — " + ex.Message);
                return null;
            }

            const System.Reflection.BindingFlags publicInstance =
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance;

            // Walk the entire inheritance chain so DefBase / NamedDef
            // fields (Comment, SourceFile, Id, Name, …) are copied too —
            // GetFields(publicInstance) on a derived type already returns
            // inherited public fields, but we keep this explicit so the
            // intent is obvious.
            foreach (System.Reflection.FieldInfo f in t.GetFields(publicInstance)) {
                if (f.IsInitOnly) continue;
                object v = f.GetValue(src);
                if (v == null) { f.SetValue(copy, null); continue; }
                System.Type ft = f.FieldType;
                if (ft.IsGenericType
                        && ft.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>)) {
                    System.Collections.IList srcList = (System.Collections.IList)v;
                    System.Collections.IList newList =
                        (System.Collections.IList)System.Activator.CreateInstance(ft);
                    foreach (object item in srcList) newList.Add(cloneItem(item));
                    f.SetValue(copy, newList);
                } else {
                    f.SetValue(copy, v);
                }
            }
            return copy;
        }

        // Element-level clone for items inside cloned List<T> fields.
        // ProductRef is the only mutable element type that actually appears
        // in the def models today (Ingredients / Products lists); strings
        // and value types are immutable so a reference copy is correct.
        private static object cloneItem(object item) {
            switch (item) {
                case ProductRef p: return new ProductRef(p.ProductId, p.Quantity, p.Port);
                case PortRef pr:   return new PortRef {
                    Name                       = pr.Name,
                    Type                       = pr.Type,
                    Shape                      = pr.Shape,
                    PositionX                  = pr.PositionX,
                    PositionY                  = pr.PositionY,
                    PositionZ                  = pr.PositionZ,
                    PositionExpression         = pr.PositionExpression,
                    Direction                  = pr.Direction,
                    CanOnlyConnectToTransports = pr.CanOnlyConnectToTransports,
                };
                case FuelPairRef fp: return new FuelPairRef(fp.FuelIn, fp.SpentFuelOut, fp.DurationSeconds);
                default:           return item;
            }
        }

        // Per-kind id-field write. createDef writes these on construction;
        // here we mirror the same set so a duplicate gets a unique id in
        // the field the model uses for lookups / display. Composite-id
        // kinds (unlocks) have no single primary id to rewrite — they
        // duplicate verbatim and the modder distinguishes the copy by
        // editing the composite parts directly.
        private static void setPrimaryId(DefBase def, string newId) {
            switch (def) {
                case RecipeDef r:          r.RecipeId    = newId; break;
                case EditRecipeDef e:      e.RecipeId    = newId; break;
                case ResearchDef rs:       rs.ResearchId = newId; break;
                case ProductLooseDef pl:   pl.ProductId  = newId; break;
                case ProductFluidDef pf:   pf.ProductId  = newId; break;
                case ProductUnitDef pu:    pu.ProductId  = newId; break;
                case TextureDef tx:        tx.Path       = newId; break;
                case MaterialLooseDef ml:  ml.Path       = newId; break;
                case MaterialTextureDef mt:mt.Path       = newId; break;
                case PrefabBoxDef pb:      pb.Path       = newId; break;
                case UnitPrefabDef upr:    upr.Path      = newId; break;
                case GeneratorDef gd:      gd.GeneratorId = newId; break;
                case ToolbarCategoryDef tc:tc.CategoryId  = newId; break;
                case BuildMachineDef bmd:  bmd.MachineId  = newId; break;
                case HousingDef hd:        hd.HousingId   = newId; break;
                case SettlementDecorationDef sdd: sdd.DecorationId = newId; break;
                case SettlementFoodDef sfd:       sfd.FoodModuleId = newId; break;
                case SettlementIspDef sid:        sid.IspModuleId  = newId; break;
                case HospitalDef hpd:             hpd.HospitalId   = newId; break;
                case MineTowerDef mtd:            mtd.MineTowerId  = newId; break;
                case ResearchLabDef rld:          rld.ResearchLabId= newId; break;
                case NuclearReactorDef nrd:       nrd.ReactorId    = newId; break;
            }
        }

        // Append _2, _3, … to <baseId> until no existing def claims the
        // result. Same shape as freshId but seeded from an arbitrary
        // candidate string instead of a kind prefix.
        private string uniquifyId(string baseId) {
            if (m_currentModel == null) return baseId;
            string candidate = baseId;
            int n = 1;
            while (m_currentModel.Definitions.Any(d => d.DisplayId == candidate)) {
                n++;
                candidate = baseId + "_" + n;
            }
            return candidate;
        }

        // ---- Action handlers (most are placeholders until their flows land) ---

        // Create a fresh Python file under the current pack's Definitions/ folder
        // and append its filename to the load order in __init__.py. We pick a
        // unique "new_file_<n>.py" name rather than popping an input prompt —
        // a prompt would need a modal Window infrastructure we don't have yet,
        // and the modder can rename the file on disk afterwards.
        //
        // The new file ships with a standard import header so adding a recipe
        // immediately works. PackRegistry is rescanned afterwards so the tree
        // shows the new file as an empty group ready for + new recipe.
        private void onNewFile() {
            if (m_currentPack == null) {
                Log.Info("RecipeEditor: + new file clicked but no pack selected.");
                return;
            }
            string definitionsDir = Path.Combine(m_currentPack.RootPath ?? "", "Definitions");
            if (!Directory.Exists(definitionsDir)) {
                Log.Warning("RecipeEditor: Definitions/ folder missing at "
                            + definitionsDir + " — cannot create file.");
                return;
            }

            // Pick a unique name: bump suffix until we don't collide with
            // anything on disk (covers the case where the modder created files
            // outside the editor that aren't yet in PackRegistry).
            int seed = 1;
            string fileName;
            string fullPath;
            do {
                fileName = "new_file_" + seed + ".py";
                fullPath = Path.Combine(definitionsDir, fileName);
                seed++;
            } while (File.Exists(fullPath) && seed < 1000);

            try {
                // Stub content matches what ModBuilder generates for empty
                // definition files — imports are enough for the modder to
                // start writing build_recipe / build_product_loose calls
                // without hunting the API surface in another file.
                string stub =
                    "from Mafi import Duration, Quantity\n" +
                    "from Mafi.Base import Assets, Ids\n" +
                    "from CustomAssets import add_loose_product_material, add_texture, "
                    + "build_product_loose, build_recipe, Product\n" +
                    "\n";
                File.WriteAllText(fullPath, stub,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                // Append to __init__.py's load order. If __init__ doesn't
                // exist yet, create one with just this file in the deps call.
                appendToLoadOrder(definitionsDir, fileName);

                // Rescan + rebuild the tree so the new file appears as its own
                // CollapsibleGroup, primed for + new recipe.
                PackRegistry.RescanPack(m_currentPack);
                m_currentModel = PackLoader.Load(m_currentPack);
                rebuildTree();
                Log.Info("RecipeEditor: created '" + fullPath + "'");
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: new-file create failed — " + ex.Message);
            }
        }

        // Append `import <module>` to __init__.py — the canonical form for
        // declaring load order in this pack convention (Batteries pack is
        // the reference). `import` is sugar for dependencies("<module>")
        // via LocalImportStatement, so the runtime resolves it the same
        // way but the source reads as one line per file instead of a
        // monolithic dependencies(...) call that grows wider every time.
        //
        // Module names are bare (no .py): CustomAssetRegistrator appends
        // the extension itself when resolving each dep, so passing
        // "foo.py" would resolve to "foo.py.py" and abort the pack with
        // FileNotFoundException. Strip the extension defensively so
        // callers can pass either form.
        //
        // Existing dependencies(...) calls and other top-level content
        // (comments, helpers, prior imports) are left untouched — the new
        // line is appended at the end of the file. Idempotent: a second
        // call with the same module is a no-op once the import line is
        // already present.
        private void appendToLoadOrder(string definitionsDir, string fileName) {
            string moduleName = fileName != null && fileName.EndsWith(".py", StringComparison.OrdinalIgnoreCase)
                ? fileName.Substring(0, fileName.Length - 3)
                : fileName;
            if (string.IsNullOrEmpty(moduleName)) return;

            string initPath = Path.Combine(definitionsDir, "__init__.py");
            string importLine = "import " + moduleName;

            if (!File.Exists(initPath)) {
                File.WriteAllText(initPath,
                    importLine + "\n",
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                return;
            }

            // Skip the append if the import is already declared — covers
            // re-runs from rescans and accidental double-clicks. Match
            // exact "import <module>" (trimmed) so a substring of a longer
            // module name doesn't fool the check.
            string[] lines = File.ReadAllLines(initPath, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++) {
                if (lines[i].Trim() == importLine) return;
            }

            // Append at the end. `import` lines stack densely in the
            // reference layout (Batteries pack), no blank separator
            // needed.
            System.Collections.Generic.List<string> output =
                new System.Collections.Generic.List<string>(lines);
            output.Add(importLine);
            File.WriteAllLines(initPath, output,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private void onSwitchPack() {
            // Open a FloatingColumn popup above the switch button listing
            // every registered pack as a uniform list of rows. Each row is
            // a ButtonRow holding [32px thumbnail | mod-id label | current
            // marker], stretched to the popup's width via AlignItemsStretch
            // so all entries share the same shape — like a real list rather
            // than text-shrunk buttons. Click selects and closes; click-
            // outside also closes.

            FloatingColumn picker = new FloatingColumn(
                FloaterPositionPolicy.ABOVE,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            // FloatingColumn renders transparent + borderless by default;
            // Cls.panel ties bg + border + bolts into one chrome that matches
            // every other popup in the editor (proto picker, deps dialog).
            picker.Class(Cls.panel)
                  .Padding(2.pt()).Gap(1.pt())
                  .MinWidth(320.px())
                  .AlignItemsStretch();

            // "+ Create new pack" entry at the top of the picker. Closes
            // the picker first, then opens NewPackDialog as a standalone
            // movable Window (same shape as TranslationsDialog) — the
            // modder can drag it around while picking the id without it
            // being clipped against the pack-card popup region.
            picker.Add(new ButtonText(new LocStrFormatted("+ Create new pack"), () => {
                picker.Close();
                NewPackDialog.Open(m_uiContext, m_main);
            }));

            foreach (LoadedPack pack in PackRegistry.Packs) {
                LoadedPack captured = pack;
                bool isCurrent = m_currentPack != null && pack.ModId == m_currentPack.ModId;

                ButtonRow row = new ButtonRow(Button.General, () => {
                    picker.Close();
                    selectPack(captured);
                });
                row.Height(48.px()).Gap(2.pt()).AlignItemsCenter();

                // Thumbnail (real or fallback) so the entry's visual matches
                // the pack-card thumbnail in the editor.
                Texture2D tex = PackThumbnailCache.TryGet(pack);
                if (tex != null) row.Add(new Img(tex).Width(36.px()).Height(36.px()));
                else row.Add(new Img(PackThumbnailCache.FallbackIconPath).Width(36.px()).Height(36.px()));

                // Two-line label stack: display name from manifest on top,
                // ModId in a smaller dim font underneath so modders see both
                // identities (what they read in COI vs the underlying string
                // they reference from Python).
                Label nameLabel = new Label(
                    new LocStrFormatted(PackManifestCache.DisplayName(pack)));
                Label idLabel = new Label(new LocStrFormatted(pack.ModId)).TinyFontSize();
                Column labelStack = new Column { nameLabel, idLabel };
                labelStack.FlexGrow(1f);
                row.Add(labelStack);

                // Trailing marker for the active pack so the user sees at a
                // glance which one they're on.
                if (isCurrent) row.Add(new Label(new LocStrFormatted("•")));

                picker.Add(row);
            }

            picker.Open(m_packCard.SwitchButton);
        }

        private void onOpenDepsDialog() {
            if (m_currentPack == null) return;
            // Anchor above the pack card so the dialog appears in the same
            // visual region as the trigger button. closeOnClickOutside is
            // handled by FloatingColumn itself.
            PackDepsDialog.Open(m_currentPack, m_packCard);
        }

        // TT button on the pack card opens the translations editor scoped to
        // the current pack. The dialog is its own movable Window (rather
        // than a popup anchored to the card) so the modder can keep it open
        // alongside the main recipe form and switch between the two while
        // translating. Each language is saved to a separate file under
        // <pack>/Localization/<lang>.json.
        private void onOpenTranslations() {
            if (m_currentPack == null || m_currentModel == null) return;
            TranslationsDialog.Open(m_currentPack, m_currentModel, m_uiContext);
        }

        private void onMigrateNow() {
            if (m_currentPack == null) {
                Log.Info("RecipeEditor: Migrate clicked but no pack selected.");
                return;
            }
            try {
                LegacyMigrator.Report report = LegacyMigrator.Migrate(m_currentPack);
                if (!report.Migrated) {
                    Log.Info("RecipeEditor: migration skipped — " + report.Error);
                    return;
                }
                // Re-scan + re-load so the tree reflects the new __init__.py and
                // the slimmed-down per-file ASTs. Selection is dropped because
                // the underlying recipes' line ranges shifted; the modder can
                // re-pick from the rebuilt tree.
                PackRegistry.RescanPack(m_currentPack);
                m_currentModel = PackLoader.Load(m_currentPack);
                m_selectedRecipe = null;
                rebuildTree();
                showEmptyStatement("Migrated to __init__.py — wrote "
                    + report.CollectedNames.Count + " deps, "
                    + "spliced " + report.AffectedFiles.Count + " file(s). "
                    + (report.Error != null ? "Warning: " + report.Error : ""));
                // Hide the banner so the modder doesn't keep seeing it after a
                // successful migration. Banner reappears next session if the
                // pack is still legacy (it won't be, post-migration).
                m_legacyBanner.Visible(false);
                Log.Info("RecipeEditor: migrated pack '" + m_currentPack.ModId
                    + "' — collected " + report.CollectedNames.Count + " deps");
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: migration failed — " + ex.Message);
            }
        }

        private void onLegacyDismiss() {
            m_legacyDismissedThisSession = true;
            m_legacyBanner.Visible(false);
        }

        private void onVerifyRoundTrip() {
            try {
                RoundTripTester.Report r = RoundTripTester.RunOnRegisteredPacks();
                Log.Info("RecipeEditor: round-trip " + r.OverallPass +
                         " (" + r.Passed + "/" + r.RecipesChecked + " OK, " + r.Failed + " failed)");
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
