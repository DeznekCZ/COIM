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
        /// Swap in a freshly loaded model. ALWAYS go through this rather than
        /// assigning m_currentModel directly.
        ///
        /// The per-kind editor cache below captures the model each editor was
        /// CONSTRUCTED with, and a save+reload replaces the model wholesale. A
        /// cached editor left holding the previous one keeps reading and MUTATING
        /// a model nothing else references: "+ add machine" appended its new
        /// binding to the dead model, so the tree (which reads the live one) never
        /// showed it and the save never wrote it. Dropping the cache forces every
        /// editor to be rebuilt against the model that is actually current.
        private void setCurrentModel(PackModel model) {
            setCurrentModel(model, null);
        }

        /// <paramref name="justWritten"/>: defs that were persisted immediately
        /// before this reload. They now exist in the file, so the fresh model
        /// already contains its own instances of them and the outgoing ones must
        /// NOT be carried over — that would show each of them twice.
        private void setCurrentModel(PackModel model,
                System.Collections.Generic.ICollection<DefBase> justWritten) {
            PackModel previous = m_currentModel;
            m_currentModel = model;
            m_editorCache.Clear();
            if (previous != null && model != null) {
                carryOverUnwritten(previous, model, justWritten);
            }
        }

        /// Move in-memory-only definitions from the outgoing model into the
        /// freshly loaded one.
        ///
        /// PackLoader.Load rebuilds the model purely from what is on DISK, so
        /// anything not yet written simply vanishes when a reload happens. And
        /// reloads happen for reasons unrelated to the entry being edited — most
        /// often flushCompletedNewDefs auto-writing some OTHER def. The visible
        /// effect was a brand-new entry disappearing the moment it was clicked.
        ///
        /// An incomplete entry is exactly the one that most needs to survive: it
        /// can't be written yet (mandatory fields still empty), so memory is the
        /// only place it exists until the modder finishes filling it in.
        private void carryOverUnwritten(PackModel previous, PackModel fresh,
                System.Collections.Generic.ICollection<DefBase> justWritten) {
            foreach (DefBase def in previous.Definitions) {
                if (!isUnwritten(def)) continue;                       // came from a file
                if (justWritten != null && justWritten.Contains(def)) continue;

                // Already on disk under a different instance? Then this one is a
                // spent draft, not a pending entry.
                //
                // A def's SourceStartLine is NOT updated when it gets written —
                // the write is always followed by a reload that replaces the
                // instance — so "unwritten" stops being true of the OBJECT the
                // moment its content reaches the file. Carrying it over anyway
                // re-added an entry that already existed, and because the stale
                // instance never stopped looking unwritten it came back on every
                // later reload too. This check is what keeps the carry-over from
                // resurrecting saved and deleted entries.
                if (existsIn(fresh, def)) continue;

                // A binding's owner link points into the OLD model, whose recipe
                // instances are now garbage. Re-point it at the equivalent recipe
                // in the fresh model, or the binding is orphaned: it would render
                // nowhere and never be written as part of any block.
                if (def is BindRecipeDef bind && bind.OwnerRecipe != null) {
                    RecipeDef owner = bind.OwnerRecipe;
                    RecipeDef relinked = fresh.Recipes.FirstOrDefault(
                        r => r.RecipeId == owner.RecipeId && r.SourceFile == owner.SourceFile);
                    if (relinked == null) continue;   // owner is gone — drop the orphan
                    bind.OwnerRecipe = relinked;
                    bind.SourceFileVariables = relinked.SourceFileVariables;
                    relinked.EmitAsWithBlock = true;
                    // The reload may have turned the owner INTO a block (its
                    // previous save wrote one), which changes where this binding
                    // belongs — see RecipeDef.TryGetBlockHeaderLine.
                    bind.ScopeKey = relinked.TryGetBlockHeaderLine(out int headerLine)
                        ? "block:" + headerLine
                        : null;
                }
                fresh.Definitions.Add(def);
            }
        }

        /// True when <paramref name="model"/> already holds a definition that
        /// represents the same thing as <paramref name="candidate"/> — same kind,
        /// same identity, same file.
        ///
        /// Identity has to come from DisplayId rather than object reference: a
        /// reload builds entirely new instances, so the freshly-parsed copy of a
        /// def shares nothing with the in-memory draft it came from. An
        /// INCOMPLETE draft has no meaningful DisplayId yet, but it also cannot
        /// have been written, so it never matches anything here and is always
        /// carried over — which is the behaviour that matters.
        private static bool existsIn(PackModel model, DefBase candidate) {
            if (model?.Definitions == null || candidate == null) return false;
            string id = candidate.DisplayId;
            if (string.IsNullOrEmpty(id)) return false;
            foreach (DefBase other in model.Definitions) {
                if (ReferenceEquals(other, candidate)) continue;
                if (other.Kind == candidate.Kind
                        && other.DisplayId == id
                        && string.Equals(other.SourceFile, candidate.SourceFile,
                                StringComparison.Ordinal)) {
                    return true;
                }
            }
            return false;
        }

        private void showEditor<T>(T def, System.Func<Editors.DefEditor<T>> factory)
                where T : DefBase {
            System.Type key = typeof(T);
            if (!m_editorCache.TryGetValue(key, out Column cached)) {
                cached = factory();
                // Subscribe ONCE, at construction — the cached editor is
                // reused for every def of this kind, so subscribing on each
                // showEditor call would stack duplicate handlers.
                ((Editors.DefEditor<T>)cached).DefEdited += refreshRowMarker;
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

        // Companion to m_rowLabelByDef holding each row's state marker (the
        // ⚠ / ● glyph left of the label). Kept separate so a field edit can
        // repaint just the marker via refreshRowMarker instead of running a
        // full rebuildTree on every keystroke.
        private readonly System.Collections.Generic.Dictionary<DefBase, Label>
            m_rowMarkerByDef = new System.Collections.Generic.Dictionary<DefBase, Label>();

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
            // header text width instead of filling the left pane. The gap
            // separates the "+ new file" panel and the per-file cards, which
            // otherwise stack edge to edge and read as one block.
            m_treeColumn.AlignItemsStretch()
                        .Width(100.Percent())
                        .Gap(CollapsibleGroup.HeaderBodyGap.px());
            m_packCard        = new PackCardView(onSwitchPack, onOpenDepsDialog, onOpenTranslations,
                                                 onOpenConfigDialog);

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
            // Point the expression controls at this pack's config.json so the fx
            // composer offers its fields and can preview what an expression evaluates
            // to. Cleared and re-read on every pack switch.
            ExpressionContext.SetPack(pack?.RootPath);
            setCurrentModel(PackLoader.Load(pack));
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
            m_rowMarkerByDef.Clear();
            if (m_currentModel == null || m_currentPack == null) return;

            // "+ new file" header — scaffold flow is a follow-up todo.
            // The button is wrapped in its own Panel with 3px inner padding so
            // it sits visually separated from the file groups below, sharing
            // the same panel-chrome look as the tree, pack-info, and editor
            // panels in the outer layout.
            Panel newFilePanel = new Panel();
            newFilePanel.BodyAdd(c => c.Padding(CollapsibleGroup.CardPadding.px()),
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
                    // A binding still waiting to be written into its recipe's
                    // block is not a member of any run — its owner's group is the
                    // only thing that draws it. Without this it would appear a
                    // second time as a loose sibling of the recipe.
                    if (d is BindRecipeDef pending && pending.IsPendingInOwner) continue;
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

                // "+ add definition…" lives in the header, next to the collapse
                // chevron — the same place a `with` block or an `if` clause keeps
                // its add button. As a body row it scrolled away from the file it
                // belonged to on long files, and it moved every time the file's
                // contents changed length.
                //
                // __init__.py gets one too: the load-order-only convention is a
                // recommendation, not a constraint — the runtime executes defs
                // there fine, and small packs are often simpler as one file. The
                // tip label below still nudges toward splitting.
                fileGroup.Header.Add(buildFileAddButton(file.AbsolutePath));

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
                        fileGroup.Body.Add(buildBodyNote(
                            "⚠ Tip: prefer to keep __init__.py for load order only and " +
                            "move definitions into separate files (products, recipes, research, …)."));
                    }
                    fileGroup.Body.Add(buildBodyNote(
                        "Load order — edit via the 🔗 deps dialog on the pack card."));
                } else if (recipesInFile.Count == 0 && otherDefsInFile.Count == 0) {
                    fileGroup.Body.Add(buildBodyNote(
                        "(no recognised statements — products/research/asset editors land later)"));
                }

                m_treeColumn.Add(fileGroup);
            }
        }

        /// A prose line inside a group's body — the load-order pointer, the
        /// "nothing recognised here" placeholder, the __init__.py tip.
        ///
        /// Indented to the same column as the labels of the rows around it: as a
        /// plain Label it started at the card's left edge, a good half-inch left
        /// of everything else in the body, which is what made a file group with
        /// one note in it look mis-laid.
        private static Label buildBodyNote(string text) {
            Label note = new Label(new LocStrFormatted(text));
            note.TinyFontSize()
                .PaddingLeft(CollapsibleGroup.TextIndent.px())
                .Color(ColorRgba.LightGray);
            return note;
        }

        /// "Add a definition to this file" button for a file group's header —
        /// the file-level counterpart of buildBlockAddButton, sharing its icon and
        /// square sizing so a file header and a block header read identically.
        /// The button anchors its own popup.
        private UiComponent buildFileAddButton(string targetFile) {
            ButtonIcon add = squareHeaderIcon(new ButtonIcon(Button.General,
                    "Assets/Unity/UserInterface/General/Plus.svg")
                .Tooltip(new LocStrFormatted("Add a definition to "
                    + Path.GetFileName(targetFile))));
            add.OnClick(() => openAddDefPopup(add, targetFile));
            return add;
        }

        // Popup launched by the "+ add definition…" button on each file
        // group's header. Lists every DefKind as a clickable row with a
        // short hint of what the call name will be. Clicking closes the
        // popup and inserts a fresh def into the target file.
        private void openAddDefPopup(UiComponent anchor, string targetFile) {
            openAddDefPickerPopup(anchor,
                title: "Add definition to " + Path.GetFileName(targetFile),
                onPick: kind => onAddDefToFile(targetFile, kind),
                onNewFile: () => openNewFileNamePrompt(anchor));
        }

        // Per-clause sibling — same picker, just routes the click through
        // onAddDefToClause so the def splices into the if-chain clause
        // body with proper indentation instead of the file end.
        /// Trash button for a whole BLOCK (an `if`/`elif`/`else` clause or a `with`
        /// block). A block has no single def whose line range covers it — the header
        /// def, when there is one, spans only the header lines — so deleting it goes
        /// through PackEmitter.DeleteBlock with the block's full extent instead of
        /// the usual per-def path, which would leave the body behind.
        private UiComponent buildBlockTrash(string filePath, int headerLine, int endLine,
                string describe) {
            ButtonIcon trash = squareHeaderIcon(new ButtonIcon(
                    Button.Danger,
                    "Assets/Unity/UserInterface/General/Trash128.png")
                .Tooltip(new LocStrFormatted("Delete " + describe
                    + " and everything inside it (Shift+click to skip confirm)")));
            trash.AttachConfirmationInline(
                new LocStrFormatted("Delete"),
                () => new LocStrFormatted("Delete " + describe + " and everything inside it?"),
                () => onDeleteBlock(filePath, headerLine, endLine, describe));
            trash.RootElement.RegisterCallback<UnityEngine.UIElements.MouseDownEvent>(evt => {
                if (evt.button == 0 && evt.shiftKey) {
                    onDeleteBlock(filePath, headerLine, endLine, describe);
                    evt.StopImmediatePropagation();
                }
            });
            return trash;
        }

        /// Track a selectable block header the same way buildTreeRow tracks an
        /// ordinary row, so applySelectionHighlight flips its highlight on
        /// selection without a tree rebuild. A block header stands in for a
        /// statement that has no row of its own; without this it would be the one
        /// selectable thing in the tree that never looked selected.
        private void registerHeaderSelection(CollapsibleGroup group, DefBase def) {
            if (group?.HeaderButton == null || def == null) return;
            group.HeaderButton.ClassRootIff(Cls.selected, isCurrentlySelected(def));
            m_rowLabelByDef[def] = group.HeaderButton;
        }

        /// Settings button for a block's own definition — the `with build_recipe(…)`
        /// recipe, or an `if` clause's condition. Opens the same right-pane editor
        /// the def's tree row used to.
        ///
        /// It replaces that row: a block header plus a full-width row for the very
        /// statement the header already displays was two lines saying one thing,
        /// and it stopped the group from shrinking. The header now carries the
        /// text; this button carries the click.
        private UiComponent buildDefSettingsButton(DefBase def, string describe) {
            ButtonIcon settings = squareHeaderIcon(new ButtonIcon(
                    Button.General,
                    "Assets/Unity/UserInterface/General/Configure.svg")
                .Tooltip(new LocStrFormatted("Edit " + describe)));
            settings.OnClick(() => onDefRowClicked(def));
            return settings;
        }

        /// Pin a header icon button to the same square box as the group's collapse
        /// chevron. ButtonIcon sizes itself from icon + padding, which lands close
        /// to but not exactly on the chevron's box — enough that a header read as a
        /// row of mismatched controls. Both ends now come from CollapsibleGroup's
        /// constants, so they can only drift together.
        ///
        private static ButtonIcon squareHeaderIcon(ButtonIcon button) {
            return sizeIconButton(button,
                CollapsibleGroup.ButtonSize, CollapsibleGroup.IconSize);
        }

        /// The leaf-row counterpart of <see cref="squareHeaderIcon"/>: same idea,
        /// one size down, for the controls that sit on a def row rather than on a
        /// group header.
        private static ButtonIcon squareRowIcon(ButtonIcon button) {
            return sizeIconButton(button,
                CollapsibleGroup.RowButtonSize, CollapsibleGroup.RowIconSize);
        }

        /// Pin an icon button to an exact square box.
        ///
        /// Both halves are needed, because a Button is TWO elements and the
        /// fluent API splits across them: Width/Height go to the outer wrapper
        /// (UiComponent.SetSizeInternal → Element), while Padding, background and
        /// border go to the inner one that actually draws the button
        /// (UiComponentDecorated.GetElementForPaddingInternal → InnerElement).
        /// The shadowed variants (General, Danger, …) give the inner element
        /// flexGrow(1), so it fills whatever box the wrapper has — and then the
        /// variant's own asymmetric USS padding eats the icon from the inside.
        /// Sizing alone shrank the row's delete icon to a sliver; padding alone
        /// left the box whatever shape the variant felt like.
        ///
        /// Symmetric padding of (box - icon) / 2 makes the inner content box
        /// exactly icon-sized, so the icon is centred by construction — no
        /// alignment call, which a ButtonIcon couldn't take anyway (it is not an
        /// IFlexComponent).
        private static ButtonIcon sizeIconButton(ButtonIcon button, int box, int icon) {
            return button
                .Compact()
                .IconSize(icon.px())
                .Width(box.px())
                .Height(box.px())
                .Padding(((box - icon) / 2).px())
                .FlexShrink(0f);
        }

        /// Delete a block from the file, then reload so the tree reflects it. Unlike
        /// a def delete there is nothing to remove from the model by hand — the
        /// reload rebuilds it from the rewritten source.
        private void onDeleteBlock(string filePath, int headerLine, int endLine, string describe) {
            if (m_currentPack == null || string.IsNullOrEmpty(filePath)) return;
            try {
                PackEmitter.DeleteBlock(filePath, headerLine, endLine);
                PackRegistry.RescanPack(m_currentPack);
                setCurrentModel(PackLoader.Load(m_currentPack));
                m_selectedRecipe = null;
                m_selectedOther = null;
                rebuildTree();
                showEmptyStatement("Deleted " + describe + " from "
                    + Path.GetFileName(filePath) + ".");
                Log.Info("RecipeEditor: deleted block @ line " + headerLine
                    + " in " + Path.GetFileName(filePath));
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: block delete failed — " + ex.Message);
            }
        }

        /// Section header inside a block: the group's name at the left, a thin rule
        /// filling the middle, and the add button at the right end. It sits ABOVE
        /// the statements it introduces (a recipe's "Bindings" divider goes between
        /// the recipe row and its first binding), so the rule reads as the boundary
        /// between the block's own header and its contents.
        ///
        /// The button hands itself to the callback as the popup anchor, so the
        /// picker opens against the separator rather than somewhere arbitrary.
        private static UiComponent buildSectionSeparator(string label) {
            Row row = new Row();
            // Indented to the label column so the caption starts where the rows
            // it introduces start, and the rule fills what's left of the width.
            row.AlignItemsCenter()
               .Gap(CollapsibleGroup.ControlGap.px())
               .PaddingTopBottom(CollapsibleGroup.RowGap.px())
               .PaddingLeft(CollapsibleGroup.TextIndent.px());

            if (!string.IsNullOrEmpty(label)) {
                row.Add(new Label(new LocStrFormatted(label)).TinyFontSize());
            }

            Column rule = new Column();
            rule.Height(1.px()).FlexGrow(1f).Background(new ColorRgba(96, 96, 104, 255));
            row.Add(rule);
            return row;
        }

        /// "Add a statement inside this block" button, for a block's HEADER —
        /// alongside settings and delete, so every block control sits in one place
        /// and the separator below stays a plain divider.
        private UiComponent buildBlockAddButton(string targetFile, int blockHeaderLine,
                string describe, bool isEditBlock = false) {
            ButtonIcon add = squareHeaderIcon(new ButtonIcon(Button.General,
                    "Assets/Unity/UserInterface/General/Plus.svg")
                .Tooltip(new LocStrFormatted("Add "
                    + (isEditBlock ? "a change inside " : "a definition inside ") + describe)));
            // An edit block accepts only its own sub-action verbs, so it gets a
            // focused picker of those six rather than the full def list.
            if (isEditBlock) {
                add.OnClick(() => openEditSubActionPopup(add, targetFile, blockHeaderLine));
            } else {
                add.OnClick(() => openAddDefIntoClausePopup(add, targetFile, blockHeaderLine));
            }
            return add;
        }

        // The six verbs valid inside a `with edit_recipe(...)` block.
        private enum EditSubAction {
            SetIngredient,
            SetProduct,
            RemoveIngredient,
            RemoveProduct,
            BindMachine,
            UnbindMachine,
        }

        // Focused picker listing only the edit sub-actions. Anchored to the block's
        // own add button (so which block it targets is obvious), it mirrors the
        // clause add popup but with a fixed, short option list.
        private void openEditSubActionPopup(UiComponent anchor, string targetFile,
                int blockHeaderLine) {
            FloatingColumn popup = new FloatingColumn(
                FloaterPositionPolicy.BELOW,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            PanelWithHeader panel = popup.AddAndReturn(new PanelWithHeader(
                    new LocStrFormatted("add change")))
                .AlignItemsStretch()
                .Gap(2.pt())
                .MinWidth(300.px());

            ScrollColumn list = new ScrollColumn();
            list.Gap(1.pt()).AlignItemsStretch();
            panel.BodyAdd(list);

            // entries unused for filtering here (short fixed list), but the shared
            // row builder wants the collection — a throwaway satisfies it.
            var entries = new System.Collections.Generic.List<PickerEntry>();
            (EditSubAction kind, string label, string hint)[] options = {
                (EditSubAction.SetIngredient,    "set ingredient amount", "set_ingredient(product, quantity)"),
                (EditSubAction.SetProduct,       "set product amount",    "set_product(product, quantity)"),
                (EditSubAction.RemoveIngredient, "remove ingredient",     "remove_ingredient(product)"),
                (EditSubAction.RemoveProduct,    "remove product",        "remove_product(product)"),
                (EditSubAction.BindMachine,      "add machine",           "bind_recipe(machine, ...)"),
                (EditSubAction.UnbindMachine,    "remove machine",        "unbind_recipe(machine)"),
            };
            foreach ((EditSubAction kind, string label, string hint) opt in options) {
                EditSubAction captured = opt.kind;
                addPopupActionRow(list, entries, opt.label, opt.hint, () => {
                    popup.Close();
                    onAddSubActionToEdit(targetFile, blockHeaderLine, captured);
                });
            }

            popup.Open(anchor);
        }

        // Create + insert one edit sub-action into a `with edit_recipe(...)` block.
        // Mirrors onAddDefToClause: pending (missing required fields) → into the
        // model under the block scope so the modder finishes it in the form; ready
        // → spliced straight into the block body.
        private void onAddSubActionToEdit(string filePath, int blockHeaderLine,
                EditSubAction kind) {
            if (m_currentModel == null) return;

            // The edit's recipe id — the sub-actions carry it for display and
            // validation. Resolve it from the block header def so every child of
            // this block agrees on which recipe it patches.
            string recipeId = null;
            foreach (DefBase d in m_currentModel.Definitions) {
                if (d is EditRecipeDef ed
                        && ed.TryGetBlockHeaderLine(out int hl) && hl == blockHeaderLine) {
                    recipeId = ed.RecipeId;
                    break;
                }
            }

            DefBase created = createSubAction(kind, recipeId);
            if (created == null) return;
            created.SourceFile = filePath;

            Mafi.Collections.Lyst<string> missing = created.MissingMandatoryFields();
            if (missing != null && missing.Count > 0) {
                created.ScopeKey = blockScopeKey(blockHeaderLine);
                m_currentModel.Definitions.Add(created);
                rebuildTree();
                onOtherDefSelected(created);
                return;
            }

            try {
                PackEmitter.AppendDefIntoClause(filePath, blockHeaderLine, created, m_currentModel);
                if (m_currentPack != null) {
                    PackRegistry.RescanPack(m_currentPack);
                    setCurrentModel(PackLoader.Load(m_currentPack));
                }
                rebuildTree();
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: add-sub-action failed - " + ex.Message);
            }
        }

        private static DefBase createSubAction(EditSubAction kind, string recipeId) {
            switch (kind) {
                case EditSubAction.SetIngredient:
                    return new RecipeProductActionDef { RecipeId = recipeId, IsInput = true,  IsRemoval = false };
                case EditSubAction.SetProduct:
                    return new RecipeProductActionDef { RecipeId = recipeId, IsInput = false, IsRemoval = false };
                case EditSubAction.RemoveIngredient:
                    return new RecipeProductActionDef { RecipeId = recipeId, IsInput = true,  IsRemoval = true };
                case EditSubAction.RemoveProduct:
                    return new RecipeProductActionDef { RecipeId = recipeId, IsInput = false, IsRemoval = true };
                case EditSubAction.BindMachine:
                    // A context-form binding: recipe is implicit in the block, so it
                    // carries the edit's recipe id but renders machine-only. No
                    // OwnerRecipe (the context is an EditRecipeDef, not a RecipeDef),
                    // so IsContextForm is what tells the emitter to omit the recipe.
                    return new BindRecipeDef { RecipeId = recipeId, IsContextForm = true };
                case EditSubAction.UnbindMachine:
                    return new UnbindRecipeDef { RecipeId = recipeId };
            }
            return null;
        }

        /// Scope key for the body of a block — an `if`, an `else`, or a `with`.
        /// They're all just "statements indented under a header", and the header
        /// LINE already says which one it is, so there is a single key format and
        /// nothing has to pass a block kind around.
        private static string blockScopeKey(int headerLine) {
            return "block:" + headerLine;
        }

        private void openAddDefIntoClausePopup(UiComponent anchor,
                string targetFile, int blockHeaderLine) {
            // Short title: the popup opens anchored to the block's own add
            // button, so which block and file it targets is already obvious from
            // where it appeared. Spelling it out only made the popup wide.
            openAddDefPickerPopup(anchor,
                title: "add",
                onPick: kind => onAddDefToClause(targetFile, blockHeaderLine, kind));
        }

        // Build + open the shared "pick a def kind" picker. Both the
        // file-level and per-clause "+ add definition" affordances feed
        // through here so the option list stays identical across both
        // surfaces — adding a new DefKind only needs one entry to show
        // up everywhere it's relevant.
        private void openAddDefPickerPopup(UiComponent anchor,
                string title, Action<DefKind> onPick) {
            openAddDefPickerPopup(anchor, title, onPick, onNewFile: null);
        }

        // Full form. `onNewFile`, when non-null, adds a "new file" row at the
        // top of the picker — used by the file-level surface so the modder can
        // spin up a fresh module (and its __init__.py import) from the same
        // place they add definitions. The clause-level surface passes null:
        // creating a file from inside an if-block body would be nonsensical.
        private void openAddDefPickerPopup(UiComponent anchor,
                string title, Action<DefKind> onPick, Action onNewFile) {
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
                .MaxHeight(560.px());

            // Search field lives in the header so it stays pinned while the
            // list below scrolls. The option list is ~35 entries across 9
            // sections — long enough that scroll-hunting is slower than
            // typing two characters.
            TextField search = new TextField()
                .Placeholder(new LocStrFormatted("search definitions…"));
            panel.Header.Add(search);

            ScrollColumn list = new ScrollColumn();
            list.Gap(1.pt()).MaxHeight(460.px()).AlignItemsStretch();
            panel.BodyAdd(list);

            // Filter model: entries in visual order, each carrying a
            // pre-lowercased haystack. A null haystack marks a section
            // header — headers have no text of their own worth matching, and
            // are shown or hidden based on whether any row under them
            // survived the filter (see applyFilter below).
            var entries = new System.Collections.Generic.List<PickerEntry>();

            Action<DefKind> pick = kind => {
                popup.Close();
                onPick(kind);
            };

            if (onNewFile != null) {
                addPopupHeader(list, entries, "File");
                addPopupActionRow(list, entries, "new definition file…",
                    "creates <name>.py + `import <name>` in __init__.py",
                    () => {
                        popup.Close();
                        onNewFile();
                    });
            }

            // Same order the toolbar would have shown — recipe-shaped first
            // (recipe, edit_recipe, research), then products, then unlocks,
            // then asset registrations. Within each group, the most common
            // is listed first.
            addPopupHeader(list, entries, "Products");
            addPopupRow(list, entries, "product (loose)",   "build_product_loose(...)",          DefKind.ProductLoose,           pick);
            addPopupRow(list, entries, "product (fluid)",   "build_product_fluid(...)",          DefKind.ProductFluid,           pick);
            addPopupRow(list, entries, "product (unit)",    "build_product_unit(...)",           DefKind.ProductUnit,            pick);
            addPopupHeader(list, entries, "Recipes");
            addPopupRow(list, entries, "build recipe",      "build_recipe(...)",                 DefKind.Recipe,                 pick);
            addPopupRow(list, entries, "bind recipe",       "bind_recipe(...) — attach to machine", DefKind.BindRecipe,          pick);
            addPopupRow(list, entries, "edit recipe",       "edit_recipe(...)",                  DefKind.EditRecipe,             pick);
            addPopupRow(list, entries, "edit recipe (block)","with edit_recipe(...): — patch with sub-actions", DefKind.EditRecipeBlock, pick);
            addPopupRow(list, entries, "migrate recipe",    "migrate_recipe(...) — remap a removed recipe id", DefKind.MigrateRecipe, pick);
            addPopupHeader(list, entries, "Machines");
            addPopupRow(list, entries, "build machine",     "build_machine(...)",                DefKind.BuildMachine,           pick);
            addPopupRow(list, entries, "build generator",   "build_generator(...)",              DefKind.Generator,              pick);
            addPopupRow(list, entries, "edit machine ports","edit_machine_ports(...)",           DefKind.EditMachinePorts,       pick);
            addPopupHeader(list, entries, "Settlements");
            addPopupRow(list, entries, "build housing",     "build_housing(...)",                DefKind.Housing,                pick);
            addPopupRow(list, entries, "build decoration",  "build_settlement_decoration(...)",  DefKind.SettlementDecoration,   pick);
            addPopupRow(list, entries, "build food module", "build_settlement_food(...)",        DefKind.SettlementFood,         pick);
            addPopupRow(list, entries, "build ISP module",  "build_settlement_isp(...)",         DefKind.SettlementIsp,          pick);
            addPopupRow(list, entries, "build hospital",    "build_hospital(...)",               DefKind.Hospital,               pick);
            addPopupHeader(list, entries, "Buildings");
            addPopupRow(list, entries, "build mine tower",  "build_mine_tower(...)",             DefKind.MineTower,              pick);
            addPopupRow(list, entries, "build farm",        "build_farm(...)",                   DefKind.Farm,                   pick);
            addPopupRow(list, entries, "add crop",          "add_crop(...) / clone_crop(...)",   DefKind.Crop,                   pick);
            addPopupRow(list, entries, "edit crop",         "edit_crop(...) — retune an existing crop's rates", DefKind.EditCrop, pick);
            addPopupRow(list, entries, "build research lab","build_research_lab(...)",           DefKind.ResearchLab,            pick);
            addPopupRow(list, entries, "build nuclear reactor", "build_nuclear_reactor(...)",    DefKind.NuclearReactor,         pick);
            addPopupRow(list, entries, "edit reactor fuels",    "edit_nuclear_reactor_fuels(...)",      DefKind.EditNuclearReactorFuels,      pick);
            addPopupRow(list, entries, "edit reactor fluids",   "edit_nuclear_reactor_fluids(...)",     DefKind.EditNuclearReactorFluids,     pick);
            addPopupRow(list, entries, "edit reactor enrichment","edit_nuclear_reactor_enrichment(...)",DefKind.EditNuclearReactorEnrichment, pick);
            addPopupRow(list, entries, "edit reactor ports",    "edit_nuclear_reactor_ports(...)",      DefKind.EditNuclearReactorPorts,      pick);
            addPopupHeader(list, entries, "Research");
            addPopupRow(list, entries, "research",          "build_research(...)",               DefKind.Research,               pick);
            addPopupRow(list, entries, "unlock recipe",     "add_unlock_recipe(...)",            DefKind.UnlockRecipe,           pick);
            addPopupRow(list, entries, "unlock product",    "add_unlock_product(...)",           DefKind.UnlockProduct,          pick);
            // `unlock machine` is deliberately NOT offered here — add_unlock_entity
            // supersedes it and takes machines too. The DefKind, editor and
            // round-trip for add_unlock_machine all stay in place so existing
            // packs that use it keep loading, saving and editing unchanged.
            addPopupRow(list, entries, "unlock entity",     "add_unlock_entity(...) — machine, building, vehicle or train car", DefKind.UnlockEntity, pick);
            addPopupRow(list, entries, "remove unlock",     "remove_unlock(...) — take a product, machine, entity or recipe back off a node", DefKind.RemoveUnlock, pick);
            addPopupHeader(list, entries, "Toolbars");
            addPopupRow(list, entries, "toolbar category",  "add_toolbar_category(...)",         DefKind.ToolbarCategory,        pick);
            // Its own group rather than sitting under "Machines": Costs lives
            // on EntityProto, so this call re-prices vehicles and train cars
            // just as readily as machines, and filing it under Machines would
            // hide that.
            addPopupHeader(list, entries, "Balancing");
            addPopupRow(list, entries, "edit entity costs",
                "edit_entity_costs(...) — re-price a machine, building, vehicle or train car",
                DefKind.EditEntityCosts, pick);
            addPopupHeader(list, entries, "Assets");
            addPopupRow(list, entries, "texture",           "add_texture(...)",                  DefKind.Texture,                pick);
            addPopupRow(list, entries, "material (loose)",  "add_loose_product_material(...)",   DefKind.MaterialLoose,          pick);
            addPopupRow(list, entries, "material (texture)","add_texture_material(...)",         DefKind.MaterialTexture,        pick);
            addPopupRow(list, entries, "prefab (box)",      "add_prefab_box(...)",               DefKind.PrefabBox,              pick);
            addPopupRow(list, entries, "prefab (unit)",     "add_unit_prefab(...)",              DefKind.UnitPrefab,             pick);
            addPopupHeader(list, entries, "Conditionals");
            addPopupRow(list, entries, "if block",          "if True:\n    pass",                DefKind.IfBlock,                pick);

            search.OnValueChanged(v => applyPickerFilter(entries, v));
            search.FocusOnShow();

            popup.Open(anchor);
        }

        // Show/hide picker entries against the typed needle. Rows match on
        // their own haystack (title + subtitle + kind name); headers are
        // resolved in a second backward pass so a section whose rows all
        // filtered out doesn't leave a dangling caption behind.
        private static void applyPickerFilter(
                System.Collections.Generic.List<PickerEntry> entries, string value) {
            string needle = (value ?? "").Trim().ToLowerInvariant();
            bool showAll = needle.Length == 0;

            foreach (PickerEntry entry in entries) {
                if (entry.Haystack == null) {
                    continue;
                }
                entry.Component.Visible(showAll || entry.Haystack.Contains(needle));
            }

            // Walk backwards so each header sees whether any row between it
            // and the next header stayed visible.
            bool sectionHasVisibleRow = false;
            for (int i = entries.Count - 1; i >= 0; i--) {
                PickerEntry entry = entries[i];
                if (entry.Haystack == null) {
                    entry.Component.Visible(showAll || sectionHasVisibleRow);
                    sectionHasVisibleRow = false;
                    continue;
                }
                if (showAll || entry.Haystack.Contains(needle)) {
                    sectionHasVisibleRow = true;
                }
            }
        }

        private static void addPopupHeader(ScrollColumn list,
                System.Collections.Generic.List<PickerEntry> entries, string text) {
            Label header = new Label(new LocStrFormatted(text))
                .Class(Cls.groupHeader).PaddingTop(3.pt());
            list.Add(header);
            entries.Add(new PickerEntry(header, null));
        }

        private void addPopupRow(ScrollColumn list,
                System.Collections.Generic.List<PickerEntry> entries,
                string title, string subtitle, DefKind kind, Action<DefKind> onPick) {
            // Include the enum name in the haystack so searching "editrecipe"
            // or "unlock" finds the row even when the display title words it
            // differently from the Python call.
            addPopupRowCore(list, entries, title, subtitle,
                extraHaystack: kind.ToString(),
                onClick: () => onPick(kind));
        }

        // Non-DefKind picker row (e.g. "new definition file…") — same shape
        // as a def row so the list reads uniformly, but its click runs an
        // arbitrary action instead of creating a definition.
        private void addPopupActionRow(ScrollColumn list,
                System.Collections.Generic.List<PickerEntry> entries,
                string title, string subtitle, Action onClick) {
            addPopupRowCore(list, entries, title, subtitle,
                extraHaystack: null, onClick: onClick);
        }

        private void addPopupRowCore(ScrollColumn list,
                System.Collections.Generic.List<PickerEntry> entries,
                string title, string subtitle, string extraHaystack, Action onClick) {
            ButtonRow row = new ButtonRow(
                Mafi.Unity.UiToolkit.Library.Button.General,
                () => onClick());
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

            string haystack = (title + " " + subtitle + " " + (extraHaystack ?? ""))
                .ToLowerInvariant();
            entries.Add(new PickerEntry(row, haystack));
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
                        setCurrentModel(PackLoader.Load(m_currentPack));
                    }
                    rebuildTree();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-if-inside-clause failed - " + ex.Message);
                }
                return;
            }
            // A nested `with edit_recipe(...)` block inside a clause: render the
            // whole header+pass block and splice it in, same as a fresh if-block.
            if (kind == DefKind.EditRecipeBlock) {
                try {
                    PackEmitter.AppendEditBlockIntoClause(filePath, clauseHeaderLine,
                        freshId("RecipeToEdit"));
                    if (m_currentPack != null) {
                        PackRegistry.RescanPack(m_currentPack);
                        setCurrentModel(PackLoader.Load(m_currentPack));
                    }
                    rebuildTree();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-edit-block-inside-clause failed - " + ex.Message);
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
                //
                // The scope key MUST match what PackLoader assigns to defs inside
                // this block ("block:<headerLine>") — the tree buckets rows by
                // (ScopeKey ?? "top"), so without it the pending def renders at the
                // top of the file instead of inside the clause it was added to.
                // onSaveDef reads the same key back to append it in the right place.
                created.ScopeKey = blockScopeKey(clauseHeaderLine);
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
                    setCurrentModel(PackLoader.Load(m_currentPack));
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
                setCurrentModel(PackLoader.Load(m_currentPack));
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
            BindRecipe,
            EditRecipe,
            EditRecipeBlock,
            MigrateRecipe,
            Research,
            ProductLoose,
            ProductFluid,
            ProductUnit,
            UnlockRecipe,
            UnlockProduct,
            UnlockMachine,
            UnlockEntity,
            RemoveUnlock,
            Texture,
            MaterialLoose,
            MaterialTexture,
            PrefabBox,
            UnitPrefab,
            Generator,
            ToolbarCategory,
            EditMachinePorts,
            EditEntityCosts,
            BuildMachine,
            Housing,
            SettlementDecoration,
            SettlementFood,
            SettlementIsp,
            Hospital,
            MineTower,
            Farm,
            Crop,
            EditCrop,
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
            // Commit any previously-added entry that has since been filled
            // in, so adding a second entry doesn't strand the first one in
            // memory. Safe to run before createDef — it only touches defs
            // that are already complete.
            flushCompletedNewDefs();
            // Structural kinds (IfBlock) don't go through createDef +
            // AppendDef — they live in the AST, not the typed model.
            // Route them to dedicated raw-text splicers instead.
            if (kind == DefKind.IfBlock) {
                try {
                    PackEmitter.AppendIfBlockToFile(filePath, "True");
                    if (m_currentPack != null) {
                        PackRegistry.RescanPack(m_currentPack);
                        setCurrentModel(PackLoader.Load(m_currentPack));
                    }
                    rebuildTree();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-if failed - " + ex.Message);
                }
                return;
            }
            // A `with edit_recipe(...)` block is structural too — it needs a `pass`
            // body to be valid Python, which the normal AppendDef path (header-only
            // render) doesn't provide. Write it whole, then the modder picks the
            // recipe on the header and adds sub-actions.
            if (kind == DefKind.EditRecipeBlock) {
                try {
                    PackEmitter.AppendEditBlockToFile(filePath, freshId("RecipeToEdit"));
                    if (m_currentPack != null) {
                        PackRegistry.RescanPack(m_currentPack);
                        setCurrentModel(PackLoader.Load(m_currentPack));
                    }
                    rebuildTree();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: add-edit-block failed - " + ex.Message);
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
                    setCurrentModel(PackLoader.Load(m_currentPack));
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
                case DefKind.BindRecipe:      return "Bind_Recipe";
                case DefKind.EditRecipe:      return "EditRecipe";
                case DefKind.Research:        return "NewResearch";
                case DefKind.ProductLoose:    return "Product_NewLoose";
                case DefKind.ProductFluid:    return "Product_NewFluid";
                case DefKind.ProductUnit:     return "Product_NewUnit";
                case DefKind.UnlockRecipe:    return "Unlock_Recipe";
                case DefKind.UnlockProduct:   return "Unlock_Product";
                case DefKind.UnlockMachine:   return "Unlock_Machine";
                case DefKind.UnlockEntity:    return "Unlock_Entity";
                case DefKind.RemoveUnlock:    return "Remove_Unlock";
                case DefKind.Texture:         return "Assets/NewTexture";
                case DefKind.MaterialLoose:   return "Assets/NewLooseMat";
                case DefKind.MaterialTexture: return "Assets/NewTexMat";
                case DefKind.PrefabBox:       return "Assets/NewBoxPrefab";
                case DefKind.UnitPrefab:      return "Assets/NewUnitPrefab";
                case DefKind.Generator:       return "NewGenerator";
                case DefKind.ToolbarCategory: return "NewCategory";
                case DefKind.EditMachinePorts:return "EditMachinePorts";
                case DefKind.EditEntityCosts: return "EditEntityCosts";
                case DefKind.BuildMachine:    return "NewMachine";
                case DefKind.Housing:               return "NewHousing";
                case DefKind.SettlementDecoration:  return "NewDecoration";
                case DefKind.SettlementFood:        return "NewFoodModule";
                case DefKind.SettlementIsp:         return "NewIspModule";
                case DefKind.Hospital:              return "NewHospital";
                case DefKind.MineTower:             return "NewMineTower";
                case DefKind.Farm:                  return "NewFarm";
                case DefKind.Crop:                  return "NewCrop";
                case DefKind.EditCrop:              return "EditCrop";
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
                case DefKind.BindRecipe:
                    // Composite DisplayId (recipe @ machine) — nothing to seed;
                    // the modder picks the recipe + machine in the form.
                    return new BindRecipeDef();
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
                case DefKind.MigrateRecipe:
                    // Composite DisplayId (old → new); both ids are filled in by
                    // the modder, so there is nothing sensible to seed.
                    return new MigrateRecipeDef();
                case DefKind.UnlockRecipe:
                    // Unlock kinds compute DisplayId from their (research,
                    // machine, recipe) composite — no Id field to seed.
                    return new UnlockRecipeDef();
                case DefKind.UnlockProduct:
                    return new UnlockProductDef();
                case DefKind.UnlockMachine:
                    return new UnlockMachineDef();
                case DefKind.UnlockEntity:
                    return new UnlockEntityDef();
                case DefKind.RemoveUnlock:
                    // Composite DisplayId (target @ machine) — the modder picks
                    // the node and then what to take off it, so nothing to seed.
                    return new RemoveUnlockDef();
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
                case DefKind.EditEntityCosts:
                    // Same shape — the target entity is picked in the editor,
                    // so there's no id to seed here.
                    return new EditEntityCostsDef();
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
                case DefKind.Farm:
                    return new FarmDef { FarmId = id, Name = "New farm" };
                case DefKind.Crop:
                    return new CropDef { CropId = id, Name = "New crop" };
                case DefKind.EditCrop:
                    // No primary id — the modder picks the target crop in the editor.
                    return new EditCropDef();
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
                string scopeKey,
                int itemIndexOffset = 0) {
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
            // Sibling items at this scope, in the order their components are added
            // to `container`. Blocks are draggable, and Reorderable reports an
            // index among the container's children — so this list must stay
            // exactly parallel to those children, including skipping runs that
            // rendered nothing. onBlockReordered turns an index pair into the
            // source line a block is moved in front of.
            var items = new System.Collections.Generic.List<LineSpan>();

            int currentRun = 0;
            foreach (PythonAPI.Statements.IStatement stmt in statements) {
                // `with <expr>:` block — same treatment as an if-chain: flush the
                // run that ended here, then render the block as a group whose body
                // is this method applied recursively under the block's scope. That
                // recursion is what places `with:`-scoped statements (a recipe's
                // machine bindings, or an `if` nested in the body) INSIDE the block
                // instead of leaving them to scatter into the enclosing run.
                if (stmt is PythonAPI.Statements.WithStatement ws) {
                    addItem(items, renderRun(container, defsByScopeRun, scopeKey, currentRun));
                    currentRun++;

                    // The block header IS the recipe's row: it carries the recipe's
                    // own label rather than the raw `with build_recipe(` source
                    // line, which says nothing about which recipe this is. The def
                    // is parked under its own "blockheader:<line>" scope so only
                    // this branch renders it, and it opens from the settings button
                    // instead of a separate row restating the same thing.
                    string headerKey = "blockheader:" + ws.StartLine + "#0";
                    DefBase headerDef = null;
                    if (defsByScopeRun.TryGetValue(headerKey, out var headerDefs)
                            && headerDefs.Count > 0) {
                        headerDef = headerDefs[0];
                    }
                    // The header can front either a `with build_recipe(...)` recipe
                    // or a `with edit_recipe(...)` edit block — different icon,
                    // wording and body-divider, same machinery.
                    bool isEditBlock = headerDef is EditRecipeDef;
                    string headerIcon = isEditBlock ? "✎ " : "🔗 ";
                    string headerText =
                        headerDef is RecipeDef headerRecipe ? displayLabelFor(headerRecipe)
                        : headerDef is EditRecipeDef headerEdit ? "edit " + (headerEdit.RecipeId ?? "<recipe>")
                        : (readSourceLineTrimmed(sourceLines, ws.StartLine) ?? "with …:");
                    string blockNoun = isEditBlock ? "this edit block" : "this recipe block";
                    // The header IS the def's row — there is no separate tree row
                    // for a `with`-block header — so clicking it selects the def,
                    // like every other statement. The settings button stays as the
                    // explicit affordance.
                    DefBase capturedHeaderDef = headerDef;
                    var withGroup = new CollapsibleGroup(
                        new LocStrFormatted(headerIcon + headerText), expanded: true,
                        onLabelClick: headerDef != null
                            ? () => onDefRowClicked(capturedHeaderDef)
                            : (System.Action)null);

                    if (headerDef != null) {
                        registerHeaderSelection(withGroup, headerDef);
                        withGroup.Header.Add(buildDefSettingsButton(headerDef,
                            isEditBlock ? "this edit" : "this recipe"));
                    }

                    // Add + delete sit together at the right end, so every block
                    // header reads the same way: toggle, label, settings, add,
                    // delete. Deleting the block takes the recipe AND its bindings —
                    // the header's own range covers only the header lines, so the
                    // block-level delete lives here.
                    if (sourceFile != null) {
                        withGroup.Header.Add(buildBlockAddButton(
                            sourceFile, ws.StartLine, blockNoun, isEditBlock));
                        withGroup.Header.Add(buildBlockTrash(sourceFile, ws.StartLine,
                            ws.EndLine > 0 ? ws.EndLine : ws.StartLine, blockNoun));
                    }

                    // Plain divider between the header and its body — bindings for a
                    // recipe, changes for an edit.
                    if (sourceFile != null) {
                        withGroup.Body.Add(buildSectionSeparator(
                            isEditBlock ? "Changes" : "Bindings"));
                    }

                    if (ws.Block != null) {
                        // The separator above already occupies a child slot, so
                        // the body's statements start one index further in.
                        renderStatementsTree(ws.Block.statements, withGroup.Body,
                                             defsByScopeRun, sourceLines, sourceFile,
                                             scopeKey: blockScopeKey(ws.StartLine),
                                             itemIndexOffset: sourceFile != null ? 1 : 0);
                    }

                    int withEnd = ws.EndLine > 0 ? ws.EndLine : ws.StartLine;
                    container.Add(wrapBlockWithGrip(withGroup, sourceFile, items,
                        new LineSpan(ws.StartLine, withEnd), itemIndexOffset));
                    continue;
                }

                if (stmt is PythonAPI.Statements.IfStatement ifs) {
                    // Flush the run that ended at this if-chain BEFORE
                    // rendering the clause groups, so the clause sits
                    // visually between its before-run and after-run.
                    addItem(items, renderRun(container, defsByScopeRun, scopeKey, currentRun));
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

                    // Every clause of the chain goes into ONE parent component.
                    // An `elif`/`else` is only legal directly after its `if`, so
                    // the chain has to be indivisible in the tree too: as separate
                    // container children the clauses were separate drop targets,
                    // and dropping a block between them would have written a
                    // stranded `elif` — a syntax error. One child also means one
                    // sibling slot and one grip, so the chain moves as a unit.
                    Column chainColumn = new Column();
                    chainColumn.AlignItemsStretch().Gap(CollapsibleGroup.RowGap.px());

                    for (int i = 0; i < chain.Count; i++) {
                        PythonAPI.Statements.IfStatement clause = chain[i];
                        string header = readSourceLineTrimmed(sourceLines, clause.StartLine)
                            ?? (clause.Condition != null ? "if/elif:" : "else:");
                        // For `if`/`elif` clauses, synthesize an IfBlockDef so the
                        // condition can be edited — it opens from the header itself
                        // (click to select, or the settings button) rather than a
                        // row below, which just restated the header. `else` has no
                        // condition, so its header is not selectable.
                        IfBlockDef ifBlock = null;
                        if (clause.Condition != null && sourceFile != null) {
                            ifBlock = new IfBlockDef {
                                Condition = extractConditionFromHeaderLine(
                                    readSourceLineTrimmed(sourceLines, clause.StartLine)),
                                SourceFile = sourceFile,
                                SourceStartLine = clause.StartLine,
                                SourceEndLine = clause.StartLine,
                                AstStartLine = clause.StartLine,
                            };
                        }

                        IfBlockDef capturedIfBlock = ifBlock;
                        var clauseGroup = new CollapsibleGroup(
                            new LocStrFormatted("🔀 " + header), expanded: true,
                            onLabelClick: ifBlock != null
                                ? () => onDefRowClicked(capturedIfBlock)
                                : (System.Action)null);

                        if (ifBlock != null) {
                            registerHeaderSelection(clauseGroup, ifBlock);
                            clauseGroup.Header.Add(buildDefSettingsButton(ifBlock, "this condition"));
                        }

                        // Add + delete last, matching the `with` block header order.
                        // Removing the OPENING `if` has to take the rest of the
                        // chain with it — a stranded `elif`/`else` is a syntax
                        // error — so it spans to the last clause's end; an
                        // `elif`/`else` removes only itself.
                        if (sourceFile != null) {
                            PythonAPI.Statements.IfStatement lastClause = chain[chain.Count - 1];
                            bool isOpeningIf = i == 0;
                            int delFrom = clause.StartLine;
                            int delTo = isOpeningIf
                                ? (lastClause.EndLine > 0 ? lastClause.EndLine : lastClause.StartLine)
                                : (clause.EndLine > 0 ? clause.EndLine : clause.StartLine);
                            clauseGroup.Header.Add(buildBlockAddButton(
                                sourceFile, clause.StartLine, "this clause"));
                            clauseGroup.Header.Add(buildBlockTrash(sourceFile, delFrom, delTo,
                                isOpeningIf && chain.Count > 1
                                    ? "this if/else chain"
                                    : "this " + (clause.Condition != null ? "clause" : "else")));
                        }

                        // Same shape as a recipe's "Bindings" divider — a labelled
                        // rule introducing the statements the clause guards. The
                        // add button for them lives on the header, beside delete.
                        if (sourceFile != null) {
                            clauseGroup.Body.Add(buildSectionSeparator("Statements"));
                        }
                        if (clause.Block != null) {
                            // Offset by the separator, as in the `with` body above.
                            renderStatementsTree(clause.Block.statements, clauseGroup.Body,
                                                 defsByScopeRun, sourceLines, sourceFile,
                                                 scopeKey: blockScopeKey(clause.StartLine),
                                                 itemIndexOffset: sourceFile != null ? 1 : 0);
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

                        chainColumn.Add(clauseGroup);
                    }

                    // The chain is a single sibling: one grip, one span covering
                    // the opening `if` through the last clause's end, so a drag
                    // carries the `elif`s and `else` along with it.
                    PythonAPI.Statements.IfStatement chainLast = chain[chain.Count - 1];
                    int chainStart = chain[0].StartLine;
                    int chainEnd = chainLast.EndLine > 0
                        ? chainLast.EndLine : chainLast.StartLine;
                    container.Add(wrapBlockWithGrip(chainColumn, sourceFile, items,
                        new LineSpan(chainStart, chainEnd), itemIndexOffset));
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
            addItem(items, renderRun(container, defsByScopeRun, scopeKey, currentRun));
        }

        // Record a rendered sibling. Null means the run rendered nothing, so it
        // must NOT take a slot — the list has to stay parallel to the container's
        // real children for drag indices to mean anything.
        private static void addItem(
                System.Collections.Generic.List<LineSpan> items, LineSpan span) {
            if (span != null) items.Add(span);
        }

        /// Wrap a block group in a row carrying a drag grip, and register it as a
        /// sibling item.
        ///
        /// The grip lives on the WRAPPER, beside the group and spanning its
        /// height, rather than inside the group's header — Reorderable reports an
        /// index within the dragged element's container, so the element it
        /// manipulates has to be the direct child of `container`, and a grip on
        /// the header would also fight the header's own buttons.
        private UiComponent wrapBlockWithGrip(UiComponent group, string sourceFile,
                System.Collections.Generic.List<LineSpan> items, LineSpan span,
                int itemIndexOffset) {
            items.Add(span);

            // Nothing to move a block relative to without a file to rewrite.
            if (string.IsNullOrEmpty(sourceFile)) return group;

            Column grip = new Column();
            grip.Class(Cls.dragHandle)
                .AlignSelfStretch()
                .Width(CollapsibleGroup.GripWidth.px())
                .FlexShrink(0f);

            Row row = new Row();
            row.AlignItemsStretch().Gap(CollapsibleGroup.ControlGap.px());
            row.Add(grip);
            row.Add(group.FlexGrow(1f));

            string capturedFile = sourceFile;
            var capturedItems = items;
            int offset = itemIndexOffset;
            Reorderable reorder = new Reorderable(grip.RootElement);
            // Reorderable counts EVERY child of the container, including any the
            // caller placed before the statements (a nested block's body opens
            // with its section separator). Subtract those to get back to an index
            // into `items`.
            reorder.OnOrderChanged += (oldIdx, newIdx) =>
                onBlockReordered(capturedFile, capturedItems, oldIdx - offset, newIdx - offset);
            row.AddManipulator(reorder);
            return row;
        }

        /// Translate a block drag into a source-file move.
        ///
        /// `items` is this scope's siblings in render order, so the drop target is
        /// whatever sits at the new index: dragging DOWN means "after that item",
        /// dragging UP means "before it". The move is committed to the file
        /// immediately and the pack reloaded, because a block's position is
        /// structure — unlike a within-run reorder, there is no model list whose
        /// order could stand in for it until the next save.
        private void onBlockReordered(string sourceFile,
                System.Collections.Generic.List<LineSpan> items, int oldIdx, int newIdx) {
            if (m_currentPack == null || oldIdx == newIdx) return;
            if (oldIdx < 0 || oldIdx >= items.Count) return;
            if (newIdx < 0 || newIdx >= items.Count) return;

            LineSpan moved = items[oldIdx];
            LineSpan target = items[newIdx];
            if (moved == null || target == null) {
                // A run made entirely of not-yet-written entries has no position
                // on disk to move against. Refuse rather than guess.
                Log.Warning("RecipeEditor: cannot move that block yet — a neighbouring "
                    + "entry has not been written to the file. Save first, then move it.");
                return;
            }

            int insertBeforeLine = newIdx > oldIdx ? target.End + 1 : target.Start;
            try {
                PackEmitter.MoveBlock(sourceFile, moved.Start, moved.End, insertBeforeLine);
                PackRegistry.RescanPack(m_currentPack);
                setCurrentModel(PackLoader.Load(m_currentPack));
                rebuildTree();
                Log.Info("RecipeEditor: moved block at line " + moved.Start
                    + " before line " + insertBeforeLine
                    + " in " + Path.GetFileName(sourceFile));
            } catch (Exception ex) {
                Log.Exception(ex);
                Log.Warning("RecipeEditor: block move failed — " + ex.Message);
            }
        }

        // Pull the (scope, run) bucket from defsByScopeRun in current model
        // order and add a row per def into a dedicated Column that hosts a
        // Reorderable manipulator on each row. The Column is the "drag
        // arena" for this run — Reorderable confines each row's drag to
        // its parent.contentContainer, so wrapping the run in its own
        // Column is what keeps reorder strictly within (scope, run).
        // No-op when the bucket is empty (a leading/trailing if-chain
        // leaves its adjacent run with zero defs).
        /// Returns the run's source line span, or null when it added nothing.
        ///
        /// The caller needs both facts to keep a per-scope list of sibling items
        /// aligned with the container's actual children: Reorderable reports an
        /// index among those children, so a run that renders nothing must not
        /// occupy a slot, and one that does has to contribute the line range a
        /// block dragged next to it will be positioned against.
        private LineSpan renderRun(
                UiComponent container,
                System.Collections.Generic.Dictionary<string,
                    System.Collections.Generic.List<DefBase>> defsByScopeRun,
                string scopeKey,
                int runIndex) {
            string key = (scopeKey ?? "top") + "#" + runIndex;
            if (!defsByScopeRun.TryGetValue(key, out var defs) || defs.Count == 0) return null;

            Column runColumn = new Column();
            // Stretch each row to the run-column's full width so flex-
            // distributed children (drag handle | label | trash) get
            // proper bounds. Without this the row sizes to its content,
            // which pushes long-label rows past the tree panel's right
            // edge.
            // Gap rather than per-row padding: the run column is the Reorderable
            // arena, so spacing declared here applies uniformly to the rows being
            // dragged and doesn't change any row's own hit area.
            runColumn.AlignItemsStretch().Gap(CollapsibleGroup.RowGap.px());
            // Keep a reference to the def list so the drag callback can
            // translate visual order changes into in-memory reorderings of
            // model.Definitions. The list captured here is the same one
            // referenced by defsByScopeRun, so it mutates in place and
            // future rebuildTree calls pick up the new order without any
            // extra plumbing.
            System.Collections.Generic.List<DefBase> defsRef = defs;
            foreach (DefBase def in defsRef) {
                System.Action<int, int> reorderCb = (oldIdx, newIdx) =>
                    onRunReordered(defsRef, oldIdx, newIdx);

                // A recipe that owns machine bindings is emitted as a
                // `with build_recipe(...):` block, so render it the same way an
                // if-clause is rendered: a CollapsibleGroup whose header is the
                // block statement and whose body holds the recipe row itself plus
                // one row per binding.
                //
                // The whole block moves as one unit, so the grip lives on a wrapper
                // Row beside the group (spanning its full height) rather than on the
                // recipe row inside it. Reorderable reports an index within the
                // dragged element's container, so the element it manipulates must be
                // the direct runColumn child — that's this wrapper. Every def still
                // contributes exactly one runColumn child, so indices stay aligned
                // with defsRef. Putting the grip on the group's Header instead would
                // fight the header's click-to-collapse.
                // A recipe that is already a `with` block IN THE FILE is rendered by
                // the AST walk above (its header lives under "blockheader:<line>" and
                // never reaches a run). The group below is only for a recipe that has
                // no block on disk yet — a newly added one, or a legacy recipe whose
                // migration hasn't been saved — so its pending bindings still show
                // under it instead of floating loose.
                // PENDING bindings only. One that already exists in the file has
                // its own range and "block:<line>" scope, and is drawn by the AST
                // walk inside that block — listing it here as well is exactly the
                // double-draw this guard exists to prevent.
                System.Collections.Generic.List<BindRecipeDef> ownBindings =
                    def is RecipeDef rcp && m_currentModel != null
                        ? m_currentModel.BindingsOf(rcp)
                            .Where(b => b.IsPendingInOwner)
                            .ToList()
                        : null;
                if (def is RecipeDef recipeWithBinds
                        && ownBindings != null && ownBindings.Count > 0) {
                    string withHeader = "with " + prependVariableName(recipeWithBinds,
                        string.IsNullOrEmpty(recipeWithBinds.RecipeId)
                            ? "<no id>" : recipeWithBinds.RecipeId) + ":";
                    var withGroup = new CollapsibleGroup(
                        new LocStrFormatted("🔗 " + withHeader), expanded: true);
                    // Inner rows are not individually reorderable — no grip on them.
                    withGroup.Body.Add(buildTreeRow(def));
                    foreach (BindRecipeDef bind in ownBindings) {
                        withGroup.Body.Add(buildTreeRow(bind));
                    }

                    Column blockGrip = new Column();
                    blockGrip.Class(Cls.dragHandle)
                             .AlignSelfStretch()
                             .Width(CollapsibleGroup.GripWidth.px())
                             .FlexShrink(0f);

                    Row blockRow = new Row();
                    blockRow.AlignItemsStretch().Gap(CollapsibleGroup.ControlGap.px());
                    blockRow.Add(blockGrip);
                    blockRow.Add(withGroup.FlexGrow(1f));

                    Reorderable blockReorder = new Reorderable(blockGrip.RootElement);
                    blockReorder.OnOrderChanged += (oldIdx, newIdx) => reorderCb(oldIdx, newIdx);
                    blockRow.AddManipulator(blockReorder);

                    runColumn.Add(blockRow);
                    continue;
                }

                runColumn.Add(buildTreeRow(def, reorderCb));
            }
            container.Add(runColumn);

            // Span across the defs that actually exist on disk. Pending ones have
            // no line yet and simply don't extend the range; if the whole run is
            // pending the span is unknown, and a drop against it is refused
            // rather than guessed at.
            int start = int.MaxValue, end = 0;
            foreach (DefBase def in defsRef) {
                if (def.SourceStartLine <= 0) continue;
                if (def.SourceStartLine < start) start = def.SourceStartLine;
                if (def.SourceEndLine > end) end = def.SourceEndLine;
            }
            return start <= end ? new LineSpan(start, end) : null;
        }

        /// A contiguous range of source lines occupied by one sibling in the tree
        /// — a run of statements, or a whole block. Used to turn a drag's index
        /// change into the line a block should be moved in front of.
        private sealed class LineSpan {
            public readonly int Start;
            public readonly int End;

            public LineSpan(int start, int end) {
                Start = start;
                End = end;
            }
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
                setCurrentModel(PackLoader.Load(m_currentPack));
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
            selectBtn = new ButtonText(Button.Area, new LocStrFormatted(labelText),
                () => onDefRowClicked(capturedDef));
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
            //
            // The column is added to EVERY row, grip texture or not: a row that
            // can't be dragged (a binding inside a `with` group) still has to
            // start its label in the same place as its draggable siblings, or
            // the body's label column goes ragged.
            Column dragHandle = new Column();
            dragHandle.AlignSelfStretch()
                      .Width(CollapsibleGroup.GripWidth.px())
                      .FlexShrink(0f);
            if (onReordered != null) dragHandle.Class(Cls.dragHandle);

            // State marker. Three states, most severe first:
            //   ⚠ red    — mandatory fields still empty (an "uninitialized"
            //              entry; it can't be written to the pack file yet).
            //   ● orange — complete but with uncommitted in-memory changes.
            //   (blank)  — clean and on disk.
            //
            // Fixed box, centred glyph, no shrink: ⚠ is wider than the 10px the
            // marker used to get, so it spilled over the label beside it, and a
            // shrinkable marker moved the label column around as rows changed
            // state.
            Label dirtyMarker = new Label(new LocStrFormatted(markerTextFor(def)));
            dirtyMarker.Width(CollapsibleGroup.MarkerWidth.px())
                       .FlexShrink(0f)
                       .TextAlign(Mafi.Unity.UiToolkit.Component.TextAlignment.CenterMiddle)
                       .Color(markerColorFor(def));
            dirtyMarker.Tooltip(new LocStrFormatted(markerTooltipFor(def)));
            m_rowMarkerByDef[def] = dirtyMarker;

            // Trash button. AttachConfirmationInline wraps OnClick with a
            // floating confirm popup; the MouseDown listener short-circuits
            // it when Shift+LMB is held so power-users can bypass the
            // popup. StopImmediatePropagation prevents the underlying
            // Clickable manipulator from then firing OnClick.
            ButtonIcon trash = squareRowIcon(new ButtonIcon(
                    Button.Danger,
                    "Assets/Unity/UserInterface/General/Trash128.png")
                .Tooltip(new LocStrFormatted("Delete (Shift+click to skip confirm)")));
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
            row.AlignItemsCenter().Gap(CollapsibleGroup.ControlGap.px());
            // grip + gap + marker + gap == CollapsibleGroup.TextIndent, so the
            // label lands in the same column as the group header's label above
            // it. Rows that aren't reorderable (a recipe inside a `with` group —
            // the GROUP carries the grip — or a binding, which is ordered by its
            // recipe) keep the width but not the grip texture.
            row.Add(dragHandle);
            row.Add(dirtyMarker);
            row.Add(selectBtn);
            row.Add(trash);

            // Reorderable manipulator. drag-handle is the grip; OnOrderChanged
            // fires after drop with the DRAGGED COMPONENT's new index inside its
            // container, so the manipulated element must be a direct child of the
            // run's Column — which this row is. A recipe rendered as a `with` group
            // is NOT: it gets wrapped, and the wrapper carries its own grip and
            // manipulator instead (see renderStatementsTree).
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
            // A definition selection and a pack-level pane are mutually exclusive views
            // of the main area, so lighting a tree row must un-light the pane buttons.
            if (m_selectedRecipe != null || m_selectedOther != null) {
                m_packCard?.SetActivePane(PackCardView.PackPane.None);
            }
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
            appendMissingFieldRowsForDef(recipe);

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
                // onBindingsChanged: bindings are listed in the TREE as children of
                // their recipe, so adding/removing one has to refresh it.
                () => new Editors.RecipeDefEditor(m_uiContext.ProtosDb, m_currentModel,
                    onBindingsChanged: rebuildTree));

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
            // bind_recipe rows read as a sub-item of the recipe they follow: an
            // indented "↳ machine (duration)" so a recipe's machines line up as
            // a visual sublist without restructuring the run/drag tree.
            if (def is BindRecipeDef brd) {
                string mach = string.IsNullOrEmpty(brd.MachineId) ? "<pick machine>" : brd.MachineId;
                string rcp  = string.IsNullOrEmpty(brd.RecipeId) ? "" : "  [" + brd.RecipeId + "]";
                string dur  = brd.DurationSeconds.HasValue ? " (" + brd.DurationSeconds.Value + "s)" : "";
                return prependVariableName(def, "    ↳ bind → " + mach + dur + rcp);
            }
            // edit_recipe sub-actions read as indented verbs under the edit block,
            // mirroring how bind rows read under a recipe.
            if (def is RecipeProductActionDef pa) {
                string prod = string.IsNullOrEmpty(pa.ProductId) ? "<pick product>" : pa.ProductId;
                string arrow = pa.IsRemoval ? " ✕ remove " : " ✎ set ";
                string noun = pa.IsInput ? "ingredient" : "product";
                string qty = !pa.IsRemoval && pa.Quantity.HasValue ? " = " + pa.Quantity.Value : "";
                return prependVariableName(def, "    ↳" + arrow + noun + " " + prod + qty);
            }
            if (def is UnbindRecipeDef ubd) {
                string mach = string.IsNullOrEmpty(ubd.MachineId) ? "<pick machine>" : ubd.MachineId;
                return prependVariableName(def, "    ↳ ✕ unbind → " + mach);
            }
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
            appendMissingFieldRowsForDef(def);

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
                else if (def is BindRecipeDef brd)     showEditor<BindRecipeDef>(brd, () => new Editors.BindRecipeDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is MigrateRecipeDef mrd)  showEditor<MigrateRecipeDef>(mrd, () => new Editors.MigrateRecipeDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockRecipeDef ur)    showEditor<UnlockRecipeDef>(ur, () => new Editors.UnlockRecipeDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockProductDef up)   showEditor<UnlockProductDef>(up, () => new Editors.UnlockProductDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockMachineDef um)   showEditor<UnlockMachineDef>(um, () => new Editors.UnlockMachineDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is UnlockEntityDef ue)    showEditor<UnlockEntityDef>(ue, () => new Editors.UnlockEntityDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is RemoveUnlockDef rmu)   showEditor<RemoveUnlockDef>(rmu, () => new Editors.RemoveUnlockDefEditor(m_currentModel, m_uiContext.ProtosDb));
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
                else if (def is RecipeProductActionDef rpa) showEditor<RecipeProductActionDef>(rpa, () => new Editors.RecipeProductActionDefEditor(m_uiContext.ProtosDb));
                else if (def is UnbindRecipeDef ubr)   showEditor<UnbindRecipeDef>(ubr, () => new Editors.UnbindRecipeDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditMachinePortsDef emp) showEditor<EditMachinePortsDef>(emp, () => new Editors.EditMachinePortsDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditEntityCostsDef eec) showEditor<EditEntityCostsDef>(eec, () => new Editors.EditEntityCostsDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is BuildMachineDef bmd)   showEditor<BuildMachineDef>(bmd, () => new Editors.BuildMachineDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is HousingDef hd)         showEditor<HousingDef>(hd, () => new Editors.HousingDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is SettlementDecorationDef sdd) showEditor<SettlementDecorationDef>(sdd, () => new Editors.SettlementDecorationDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is SettlementFoodDef sfd) showEditor<SettlementFoodDef>(sfd, () => new Editors.SettlementFoodDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is SettlementIspDef sid)  showEditor<SettlementIspDef>(sid, () => new Editors.SettlementIspDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is HospitalDef hpd)       showEditor<HospitalDef>(hpd, () => new Editors.HospitalDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is MineTowerDef mtd)      showEditor<MineTowerDef>(mtd, () => new Editors.MineTowerDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is FarmDef fmd)           showEditor<FarmDef>(fmd, () => new Editors.FarmDefEditor(m_currentModel, m_uiContext.ProtosDb));
                else if (def is CropDef crd)           showEditor<CropDef>(crd, () => new Editors.CropDefEditor(m_currentPack, m_currentModel, m_uiContext.ProtosDb));
                else if (def is EditCropDef ecd)       showEditor<EditCropDef>(ecd, () => new Editors.EditCropDefEditor(m_currentModel, m_uiContext.ProtosDb));
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
                    setCurrentModel(PackLoader.Load(m_currentPack));
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

        // ---- Uninitialized-entry detection -------------------------------
        //
        // A def is "uninitialized" when DefBase.MissingMandatoryFields still
        // reports empty required arguments. Such a def is deliberately NOT
        // written to the pack file (emitting `edit_machine_ports(machine =
        // None)` crashes the mod loader on the next reload), so it lives in
        // memory only until the modder fills the gaps. These helpers make
        // that state visible instead of leaving it to Player.log.

        private static bool isIncomplete(DefBase def) {
            Mafi.Collections.Lyst<string> missing = def?.MissingMandatoryFields();
            return missing != null && missing.Count > 0;
        }

        // True when the def has never reached the pack file. SourceStartLine
        // is the sentinel: onAddDefToFile seeds freshly-created defs with 0
        // and the loader always assigns a 1-based line to anything it read
        // from disk.
        private static bool isUnwritten(DefBase def) {
            return def != null && def.SourceStartLine <= 0;
        }

        private static string markerTextFor(DefBase def) {
            if (isIncomplete(def)) return "⚠";
            return def.Dirty ? "●" : "";
        }

        private static ColorRgba markerColorFor(DefBase def) {
            return isIncomplete(def) ? ColorRgba.Red : ColorRgba.Orange;
        }

        private static string markerTooltipFor(DefBase def) {
            Mafi.Collections.Lyst<string> missing = def?.MissingMandatoryFields();
            if (missing != null && missing.Count > 0) {
                return "Not written to the pack yet — still missing: "
                    + string.Join(", ", missing);
            }
            return def != null && def.Dirty ? "Unsaved changes" : "";
        }

        // Repaint one row's marker in place. Called on every field edit so
        // the ⚠ clears the instant the last required field is filled,
        // without the cost (and lost text-field focus) of a rebuildTree.
        private void refreshRowMarker(DefBase def) {
            if (def == null) return;
            if (!m_rowMarkerByDef.TryGetValue(def, out Label marker)) return;
            marker.Value(new LocStrFormatted(markerTextFor(def)));
            marker.Color(markerColorFor(def));
            marker.Tooltip(new LocStrFormatted(markerTooltipFor(def)));
        }

        // Red banner listing what an incomplete def still needs, plus a note
        // on what happens once it's complete. Rendered at the TOP of the
        // form (before the per-kind body) so it's the first thing seen.
        private void appendMissingFieldRowsForDef(DefBase def) {
            Mafi.Collections.Lyst<string> missing = def?.MissingMandatoryFields();
            if (missing == null || missing.Count == 0) return;
            m_statementColumn.Add(new Label(new LocStrFormatted(
                    "⚠ incomplete — required: " + string.Join(", ", missing)))
                .Color(ColorRgba.Red).FontBold());
            m_statementColumn.Add(new Label(new LocStrFormatted(isUnwritten(def)
                    ? "This entry exists in the editor only. It is written to "
                      + "the pack file as soon as every required field is filled."
                    : "Saving is blocked until the required fields are filled — "
                      + "the on-disk copy keeps its previous contents."))
                .Color(ColorRgba.LightGray).TinyFontSize());
        }

        // Write out every in-memory-only def that has since become complete.
        // This is what makes a newly-added entry reach the mod without the
        // modder having to press Save: the flush runs whenever they navigate
        // away from the entry or add another one.
        //
        // Deliberately NOT run on each keystroke — AppendDef is followed by a
        // RescanPack + full PackLoader.Load, which replaces every DefBase
        // instance and would tear the form out from under the field being
        // typed into.
        //
        // Returns true when at least one def was appended (so the caller
        // knows m_currentModel has been replaced and its DefBase references
        // are stale).
        private bool flushCompletedNewDefs() {
            if (m_currentModel == null || m_currentPack == null) return false;
            System.Collections.Generic.List<DefBase> pending =
                m_currentModel.Definitions.Where(
                    d => isUnwritten(d)
                        && !(d is IfBlockDef)
                        && !string.IsNullOrEmpty(d.SourceFile)
                        // A binding whose recipe is not a block on disk YET has
                        // nowhere of its own to go: it is written as the body of
                        // the `with` block the recipe's own save opens (see
                        // PackEmitter.RenderRecipe). Auto-appending it here would
                        // land it at end-of-file, outside the block.
                        && !(d is BindRecipeDef ob && ob.IsPendingInOwner)
                        && !isIncomplete(d)).ToList();
            if (pending.Count == 0) return false;

            // Track exactly which ones reached disk. Everything else that is
            // still unwritten — including anything that threw below — has to
            // survive the reload, so it must NOT be in this set.
            System.Collections.Generic.List<DefBase> written =
                new System.Collections.Generic.List<DefBase>();
            foreach (DefBase def in pending) {
                try {
                    // A def that belongs inside a block splices into it; only a
                    // top-level def goes through the end-of-file append.
                    if (tryGetBlockHeaderLine(def, out int blockLine)) {
                        PackEmitter.AppendDefIntoClause(
                            def.SourceFile, blockLine, def, m_currentModel);
                    } else {
                        PackEmitter.AppendDef(def, m_currentModel);
                    }
                    written.Add(def);
                    Log.Info("RecipeEditor: auto-wrote new entry '"
                        + (def.DisplayId ?? def.Kind) + "' to "
                        + Path.GetFileName(def.SourceFile)
                        + " (all required fields now filled).");
                } catch (Exception ex) {
                    Log.Exception(ex);
                    Log.Warning("RecipeEditor: auto-write of '"
                        + (def.DisplayId ?? def.Kind) + "' failed — " + ex.Message
                        + ". It stays in memory; use Save to retry.");
                }
            }
            if (written.Count == 0) return false;

            PackRegistry.RescanPack(m_currentPack);
            setCurrentModel(PackLoader.Load(m_currentPack), written);
            return true;
        }

        // Tree-row click entry point. Flushes any now-complete new entries
        // first, then re-resolves the clicked def against the (possibly
        // reloaded) model before handing off to the per-kind form. Without
        // the re-resolve the form would bind to an orphaned DefBase whose
        // edits no longer reach m_currentModel.
        private void onDefRowClicked(DefBase def) {
            if (flushCompletedNewDefs()) {
                string kind = def.Kind;
                string id   = def.DisplayId;
                def = m_currentModel.Definitions.FirstOrDefault(
                    d => d.Kind == kind && d.DisplayId == id) ?? def;
                rebuildTree();
            }
            if (def is RecipeDef recipe) onRecipeSelected(recipe);
            else onOtherDefSelected(def);
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
        // Read the if-clause header line back out of a def's ScopeKey. PackLoader
        // keys every block body as "block:&lt;headerLine&gt;" and onAddDefToClause stamps
        // the same shape onto pending defs, so this recovers where a not-yet-written
        // def is supposed to land. False for top-level defs.
        private static bool tryGetBlockHeaderLine(DefBase def, out int headerLine) {
            headerLine = 0;
            string key = def?.ScopeKey;
            if (string.IsNullOrEmpty(key)) return false;
            // Every block body — `if`, `else`, `with` — is keyed "block:<headerLine>".
            // A pending def in one must be spliced into that block, never appended
            // at end-of-file (which would silently move it out of the block).
            const string prefix = "block:";
            if (!key.StartsWith(prefix)) return false;
            return int.TryParse(key.Substring(prefix.Length), out headerLine) && headerLine > 0;
        }

        /// Pre-save load-order gate. Returns true when it is safe to write.
        ///
        /// <paramref name="def"/> null = whole-pack save, so every violation
        /// counts. Otherwise only the ones ORIGINATING from that def block the
        /// write; violations elsewhere are still logged so the modder can see
        /// the pack is not clean, but they don't stop an unrelated edit.
        ///
        /// The violations are also pushed into <see cref="PackModel.Issues"/> so
        /// the per-def red rows (appendIssueRowsForDef) surface them in place,
        /// rather than the modder having to find them in Player.log.
        private bool checkReferenceOrder(DefBase def) {
            if (m_currentModel == null || m_currentPack == null) return true;
            System.Collections.Generic.List<PackReferenceValidator.Violation> all;
            try {
                all = PackReferenceValidator.Validate(m_currentModel, m_currentPack);
            } catch (Exception ex) {
                // A validator bug must never make the editor unable to save.
                Log.Exception(ex);
                return true;
            }
            if (all.Count == 0) return true;

            foreach (PackReferenceValidator.Violation v in all) {
                PackIssue issue = v.ToIssue();
                bool alreadyListed = m_currentModel.Issues.Any(
                    i => i.SourceFile == issue.SourceFile
                         && i.Line == issue.Line
                         && i.Message == issue.Message);
                if (!alreadyListed) m_currentModel.Issues.Add(issue);
            }

            System.Collections.Generic.List<PackReferenceValidator.Violation> blocking = def == null
                ? all
                : all.Where(v => ReferenceEquals(v.Source, def)).ToList();

            foreach (PackReferenceValidator.Violation v in all) {
                string prefix = blocking.Contains(v) ? "cannot save — " : "also in this pack — ";
                Log.Warning("RecipeEditor: " + prefix + PackReferenceValidator.Describe(v));
            }
            if (blocking.Count == 0) return true;

            DiagnosticTrace.Step(
                $"checkReferenceOrder[{def?.Kind ?? "pack"}]: blocked — {blocking.Count} forward reference(s)");
            rebuildTree();
            return false;
        }

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
            // Refuse a forward reference by bare id: this def naming another
            // definition of THIS pack that is created later in load order. The
            // runtime has no forward references, so writing it produces a pack
            // that fails to load. Only violations this def is responsible for
            // block it — the rest of the pack is reported as issues, not held
            // against the entry being saved.
            if (!checkReferenceOrder(def)) return;
            // Freshly added defs without a source range yet can't go through
            // the single-entry path â€” they need the appended-defs flow that
            // PackEmitter.Save runs. Fall back to onSavePack in that case.
            if (string.IsNullOrEmpty(def.SourceFile) || def.SourceStartLine <= 0) {
                // …unless it was added INSIDE an if-clause. PackEmitter.Save appends
                // new defs at the end of the file, which would silently move it out
                // of the clause it belongs to, so route those through the
                // clause-aware splice instead (same call onAddDefToClause makes once
                // the mandatory fields are filled).
                if (!string.IsNullOrEmpty(def.SourceFile)
                        && tryGetBlockHeaderLine(def, out int clauseLine)) {
                    DiagnosticTrace.Step($"onSaveDef[{def.Kind}/{def.DisplayId}]: pending in clause @ {clauseLine} → AppendDefIntoClause");
                    try {
                        PackEmitter.AppendDefIntoClause(def.SourceFile, clauseLine, def, m_currentModel);
                        PackRegistry.RescanPack(m_currentPack);
                        setCurrentModel(PackLoader.Load(m_currentPack));
                        rebuildTree();
                    } catch (Exception ex) {
                        Log.Exception(ex);
                        Log.Warning("RecipeEditor: save-into-clause failed — " + ex.Message);
                    }
                    return;
                }
                DiagnosticTrace.Step($"onSaveDef[{def.Kind}/{def.DisplayId}]: no SourceFile/StartLine → onSavePack");
                // The per-def gate above already passed for THIS def. Don't let
                // the pack-wide gate re-run and block the modder's edit over a
                // pre-existing problem somewhere else in the pack.
                onSavePack(alreadyValidated: true);
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
                setCurrentModel(PackLoader.Load(m_currentPack));
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
        /// <paramref name="alreadyValidated"/> is set by the single-def save path,
        /// which has already run the gate scoped to the def being written.
        private void onSavePack(bool alreadyValidated = false) {
            if (m_currentModel == null || m_currentPack == null) {
                Log.Info("RecipeEditor: Save clicked but no pack/model selected.");
                return;
            }
            // Whole-pack gate: a save that writes every definition must not leave
            // any of them naming a prototype this pack only creates later.
            if (!alreadyValidated && !checkReferenceOrder(null)) return;
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
                setCurrentModel(PackLoader.Load(m_currentPack));
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
                case FarmDef fmd:                 fmd.FarmId       = newId; break;
                case CropDef crd:                 crd.CropId       = newId; break;
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

            createDefinitionFile(definitionsDir, fileName);
        }

        // Prompt for a module name, then create the file. Reached from the
        // "new definition file…" row in the add-definition picker — the
        // toolbar's "+ new file" button skips this and auto-names instead.
        //
        // Anchored on the same component the picker was, so the prompt opens
        // where the modder's attention already is.
        private void openNewFileNamePrompt(UiComponent anchor) {
            if (m_currentPack == null) {
                Log.Info("RecipeEditor: new-file requested but no pack selected.");
                return;
            }
            string definitionsDir = Path.Combine(m_currentPack.RootPath ?? "", "Definitions");
            if (!Directory.Exists(definitionsDir)) {
                Log.Warning("RecipeEditor: Definitions/ folder missing at "
                            + definitionsDir + " — cannot create file.");
                return;
            }

            FloatingColumn popup = new FloatingColumn(
                FloaterPositionPolicy.BELOW,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            PanelWithHeader panel = popup.AddAndReturn(new PanelWithHeader(
                    new LocStrFormatted("New definition file")))
                .AlignItemsStretch()
                .Gap(2.pt())
                .MinWidth(340.px());

            TextField nameField = new TextField()
                .Placeholder(new LocStrFormatted("module name, e.g. products"));
            panel.BodyAdd(nameField);
            panel.BodyAdd(new Label(new LocStrFormatted(
                    "Creates Definitions/<name>.py and appends `import <name>` "
                    + "to __init__.py so it loads."))
                .Class(Cls.fontMonospace).TinyFontSize());

            Label error = new Label(new LocStrFormatted(""));
            error.Visible(false);
            panel.BodyAdd(error);

            // PanelWithHeader.BodyAdd returns the PANEL (for chaining), not the
            // child — so build the button first and add it.
            ButtonText create = new ButtonText(new LocStrFormatted("create"), null);
            panel.BodyAdd(create);
            create.OnClick(() => {
                // TextField exposes its current text via GetText(); `Value` is the
                // IComponentWithText SETTER extension, hence the method-group error.
                string fileName = normalizeModuleFileName(nameField.GetText());
                if (fileName == null) {
                    // Label has no Text(...) setter — text is set through the
                    // IComponentWithText `Value(LocStrFormatted)` extension.
                    error.Value(new LocStrFormatted(
                        "⚠ Use letters, digits and underscores only; must not start with a digit."));
                    error.Visible(true);
                    return;
                }
                if (File.Exists(Path.Combine(definitionsDir, fileName))) {
                    error.Value(new LocStrFormatted("⚠ " + fileName + " already exists."));
                    error.Visible(true);
                    return;
                }
                popup.Close();
                createDefinitionFile(definitionsDir, fileName);
            });

            nameField.FocusOnShow();
            popup.Open(anchor);
        }

        // Coerce user input into a legal Python module file name, or null if
        // it can't be. A trailing ".py" is accepted and stripped so both
        // "products" and "products.py" work. The identifier rules match what
        // the lexer accepts for `import <name>` — a file the runtime can't
        // import is worse than a rejected keystroke.
        private static string normalizeModuleFileName(string raw) {
            string name = (raw ?? "").Trim();
            if (name.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                name = name.Substring(0, name.Length - 3);
            }
            if (name.Length == 0) {
                return null;
            }
            if (char.IsDigit(name[0])) {
                return null;
            }
            for (int i = 0; i < name.Length; i++) {
                char c = name[i];
                if (!char.IsLetterOrDigit(c) && c != '_') {
                    return null;
                }
            }
            if (name == "__init__") {
                return null;
            }
            return name + ".py";
        }

        // Write the stub file, register it in __init__.py's load order, and
        // reload the pack so the tree shows it. Shared by the auto-named
        // toolbar flow and the named prompt above.
        private void createDefinitionFile(string definitionsDir, string fileName) {
            string fullPath = Path.Combine(definitionsDir, fileName);
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
                setCurrentModel(PackLoader.Load(m_currentPack));
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

        // The three pack-level panes (dependencies / translations / config fields) all
        // render into the SAME main pane a definition's form uses, rather than floating
        // over it: they are editors like any other, they need the room, and a popup
        // covering the tree made it impossible to check a definition while editing one.
        //
        // Clicking the button that is already showing rebuilds its pane from disk,
        // which is also the discard path for unsaved edits.
        private void onOpenDepsDialog() {
            if (m_currentPack == null) return;
            showPackPane(PackCardView.PackPane.Deps,
                () => new PackDepsPanel(m_currentPack));
        }

        // CFG on the pack card — this pack's config.json fields. The player-facing
        // settings across all packs live in their own toolbar window
        // (ModSettingsWindow), because this editor is sandbox-gated.
        private void onOpenConfigDialog() {
            if (m_currentPack == null) return;
            showPackPane(PackCardView.PackPane.Config,
                () => new PackConfigPanel(m_currentPack));
        }

        // TT on the pack card — the translations editor for the current pack. Each
        // language is saved to its own file under <pack>/Localization/<lang>.json.
        private void onOpenTranslations() {
            if (m_currentPack == null || m_currentModel == null) return;
            showPackPane(PackCardView.PackPane.Translations,
                () => new TranslationsPanel(m_currentPack, m_currentModel));
        }

        // Render a pack-level pane into the editor area and light its button. The tree
        // selection is dropped: what the pane shows is now the subject, so leaving a
        // recipe row highlighted would claim otherwise. The footer's Save/Duplicate act
        // on a DEFINITION, so they go disabled — each pane carries its own Save.
        private void showPackPane(PackCardView.PackPane pane, Func<UiComponent> build) {
            m_selectedRecipe = null;
            m_selectedOther  = null;
            applySelectionHighlight();

            m_statementColumn.Clear();
            m_statementColumn.Add(build());
            rebuildEditorFooter(null, includeVerify: false);
            m_packCard.SetActivePane(pane);
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
                setCurrentModel(PackLoader.Load(m_currentPack));
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

        // One row in the add-definition picker, paired with the lower-cased text
        // the search box matches against. A null Haystack marks a SECTION HEADER
        // — headers never match the needle themselves; applyPickerFilter decides
        // their visibility from whether any row in their section survived.
        private sealed class PickerEntry {
            public readonly UiComponent Component;
            public readonly string Haystack;

            public PickerEntry(UiComponent component, string haystack) {
                Component = component;
                Haystack = haystack;
            }
        }
    }
}
