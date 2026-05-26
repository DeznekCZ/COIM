using System;
using System.IO;
using System.Linq;
using System.Text;
using CustomAssets.Data.Mod;
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
        private bool m_legacyDismissedThisSession;

        // UiContext gives us the live ProtosDb the in-game proto pickers need.
        // Window dependencies are resolved via Context.Resolver.Instantiate
        // (WindowController.CreateWindow); declaring it on the constructor is
        // sufficient — no manual binding required.
        private readonly UiContext m_uiContext;

        private readonly LegacyBanner m_legacyBanner;
        private readonly Column m_treeColumn;       // children = per-file CollapsibleGroups
        private readonly Column m_statementColumn; // polymorphic right-pane content
        private readonly PackCardView m_packCard;

        public RecipeEditorWindow(UiContext uiContext) : base(WindowTitle) {
            m_uiContext = uiContext;
            MakeImmersiveFullscreen();

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
            m_packCard        = new PackCardView(onSwitchPack, onOpenDepsDialog);

            // ---- Three content panels: tree (top-left), pack info (bottom-left), editor (right).
            // Each Mafi Panel adds its own background + bolts so the regions visually
            // separate without us drawing borders by hand.
            ScrollColumn treeScroll = new ScrollColumn();
            treeScroll.Add(m_treeColumn);
            Panel treePanel = new Panel();
            treePanel.BodyAdd(treeScroll);
            treePanel.FlexGrow(1f);

            Panel packCardPanel = new Panel();
            packCardPanel.BodyAdd(m_packCard);

            ScrollColumn statementScroll = new ScrollColumn();
            statementScroll.Add(m_statementColumn);
            Panel editorPanel = new Panel();
            editorPanel.BodyAdd(statementScroll);
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

            rebuildTree();
            showEmptyStatement(m_currentModel.Recipes.Count == 0
                ? "Pack has no build_recipe() calls yet."
                : "Select a recipe from the tree.");
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

                // __init__.py CAN hold definitions (recipes/products/research)
                // — the runtime accepts content there — but the convention is
                // to keep load order in __init__.py and put definitions in
                // dedicated files. We show whatever's there and flag the
                // recommendation with a soft "tip" when content is present.
                string headerText = "📄 " + fileName +
                    "  (" + recipesInFile.Count + " recipe(s)" +
                    (isInit ? ", load order" : "") + ")";

                // __init__.py defaults expanded when it has content (so the
                // modder sees the misplaced definitions immediately) and
                // collapsed when it only carries load order. Content files
                // always default expanded.
                bool defaultExpanded = !isInit || recipesInFile.Count > 0;
                var fileGroup = new CollapsibleGroup(
                    new LocStrFormatted(headerText),
                    expanded: defaultExpanded);

                if (recipesInFile.Count > 0) {
                    var recipesGroup = new CollapsibleGroup(
                        new LocStrFormatted("Recipes (" + recipesInFile.Count + ")"),
                        expanded: true);
                    foreach (RecipeDef recipe in recipesInFile) {
                        RecipeDef captured = recipe;
                        recipesGroup.Body.Add(
                            new ButtonText(
                                new LocStrFormatted(displayLabelFor(recipe)),
                                () => onRecipeSelected(captured)));
                    }
                    fileGroup.Body.Add(recipesGroup);
                }

                if (isInit) {
                    // Always note the load-order entry point and flag the
                    // recommendation when definitions are present.
                    if (recipesInFile.Count > 0) {
                        fileGroup.Body.Add(new Label(new LocStrFormatted(
                            "⚠ Tip: prefer to keep __init__.py for load order only and " +
                            "move definitions into separate files (products, recipes, research, …).")));
                    }
                    fileGroup.Body.Add(new Label(new LocStrFormatted(
                        "Load order — edit via the 🔗 deps dialog on the pack card.")));
                } else if (recipesInFile.Count == 0) {
                    fileGroup.Body.Add(new Label(new LocStrFormatted(
                        "(no recognised statements — products/research/asset editors land later)")));
                }

                // "+ new recipe" entry point on every non-init file. The emitter
                // already handles RecipeDefs with SourceStartLine == 0 by
                // appending the rendered build_recipe(...) to the end of the
                // target file (see PackEmitter), so the new RecipeDef just
                // needs SourceFile assigned and the rest of the source-location
                // bookkeeping left at 0. We skip __init__.py because the
                // convention (and our own tip above) is to keep it for load
                // order only — modders adding recipes there would immediately
                // be told to move them, so don't surface the option.
                if (!isInit) {
                    string targetFile = file.AbsolutePath;
                    fileGroup.Body.Add(new ButtonText(
                        new LocStrFormatted("+ new recipe in " + fileName),
                        () => onAddRecipeToFile(targetFile)));
                }

                m_treeColumn.Add(fileGroup);
            }
        }

        // Append a fresh RecipeDef to the model, tagged for emission into
        // `filePath`. The id seed is "NewRecipe_<n>" where n is the recipe
        // count after insertion — gives the modder something unique they can
        // immediately rename in the editor's id field without typing over an
        // existing recipe by accident. SourceStartLine / SourceEndLine stay
        // at 0 (PackEmitter treats that as "append to file end"). The form
        // is then refreshed and the new recipe pre-selected so editing can
        // start immediately.
        private void onAddRecipeToFile(string filePath) {
            if (m_currentModel == null) return;
            int seed = m_currentModel.Recipes.Count + 1;
            string newId = "NewRecipe_" + seed;
            // Avoid collisions when files were deleted/added and counts shifted —
            // bump until we find a free id.
            while (m_currentModel.Recipes.Any(r => r.RecipeId == newId)) {
                seed++;
                newId = "NewRecipe_" + seed;
            }
            RecipeDef created = new RecipeDef {
                RecipeId        = newId,
                Name            = "New recipe",
                Description     = "",
                MachineId       = null,
                ResearchId      = null,
                DurationSeconds = null,
                PowerPercent    = null,
                SourceFile      = filePath,
                SourceStartLine = 0,
                SourceEndLine   = 0
            };
            m_currentModel.Recipes.Add(created);
            rebuildTree();
            onRecipeSelected(created);
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
            string name = string.IsNullOrEmpty(r.Name) ? "" : r.Name;
            if (string.IsNullOrEmpty(name) || name == r.RecipeId) return r.RecipeId ?? "<no id>";
            if (name.Length > 40) name = name.Substring(0, 37) + "...";
            return r.RecipeId + "  —  " + name;
        }

        private void onRecipeSelected(RecipeDef recipe) {
            m_selectedRecipe = recipe;
            rebuildStatementEditor();
        }

        // ---- Statement editor (right pane) -------------------------------------

        private void rebuildStatementEditor() {
            m_statementColumn.Clear();
            if (m_selectedRecipe == null) return;

            RecipeDef r = m_selectedRecipe;
            m_statementColumn.Add(new Label(new LocStrFormatted("Recipe — " + (r.RecipeId ?? "?"))));

            // Editable scalar fields from step C. Pickers (machine / research /
            // products) and list editing (ingredients / products) come in next
            // passes; for now they remain read-only display strings.
            m_statementColumn.Add(labeledField("id",
                new TextField()
                    .Text(r.RecipeId ?? "")
                    .OnValueChanged(v => r.RecipeId = v)));
            m_statementColumn.Add(labeledField("name",
                new TextField()
                    .Text(r.Name ?? "")
                    .OnValueChanged(v => r.Name = v)));
            m_statementColumn.Add(labeledField("description",
                new TextField()
                    .Multiline(true)
                    .Text(r.Description ?? "")
                    .SetTextAreaMinHeight(48.px())
                    .OnValueChanged(v => r.Description = v)));
            // Comment captures the `#` comment block immediately preceding the
            // build_recipe(...) call in source. Editing here writes back the
            // entire block on save (PackEmitter prepends `# ` to each line).
            // Multiline so modders can author paragraph notes without hand
            // wrapping. Stored on the RecipeDef but NOT a `build_recipe`
            // argument — purely authorial documentation.
            // Comment captures the `#` comment block immediately preceding the
            // build_recipe(...) call in source. No Placeholder() here — Mafi's
            // multiline TextField doesn't hide the hint text once a value is
            // present, so the hint overlaps the actual content (same bug that
            // bit the qty TextField; see the buildProductRow comment). The
            // label above the field is sufficient guidance.
            m_statementColumn.Add(labeledField("comment (notes shown above the recipe)",
                new TextField()
                    .Multiline(true)
                    .Text(r.Comment ?? "")
                    .SetTextAreaMinHeight(36.px())
                    .OnValueChanged(v => r.Comment = string.IsNullOrEmpty(v) ? null : v)));
            // Machine and research are now real proto pickers backed by the
            // live ProtosDb. The picker shows icon + display name + dim id and
            // opens a searchable popup on click. Selection writes back the id
            // string into the RecipeDef so the emitter preserves the change.
            // Changing the machine invalidates the port dropdowns on every
            // ingredient/product row, so we rebuild the whole form after a
            // machine change. The picker writes the new id back into the
            // RecipeDef BEFORE the callback fires here.
            m_statementColumn.Add(labeledField("machine",
                new ProtoPicker<MachineProto>(
                    m_uiContext.ProtosDb,
                    getId: () => r.MachineId,
                    setId: id => { r.MachineId = id; rebuildStatementEditor(); },
                    emptyLabel: new LocStrFormatted("(pick a machine…)"))));
            // Research uses the same generic ProtoPicker<T> as machine/product
            // (the picker no longer requires T : IProtoWithIcon — it dropped
            // ProtoPickerPopup<T> in favour of a custom FloatingColumn popup
            // for exactly this case). The custom iconPathOf delegate digs the
            // icon out of ResearchNodeProto.Graphics.Icons[0], falling back to
            // IconsProtos[0].IconPath when only the unlocked-proto icon is set.
            m_statementColumn.Add(labeledField("research",
                new ProtoPicker<ResearchNodeProto>(
                    m_uiContext.ProtosDb,
                    getId: () => r.ResearchId,
                    setId: id => r.ResearchId = id,
                    iconPathOf: researchIconPath,
                    emptyLabel: new LocStrFormatted("(no research required)"),
                    title: new LocStrFormatted("Pick research"))));
            m_statementColumn.Add(labeledField("duration (seconds, blank = default)",
                new TextField()
                    .Text(r.DurationSeconds.HasValue ? r.DurationSeconds.Value.ToString() : "")
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => r.DurationSeconds = parseNullableInt(v))));
            m_statementColumn.Add(labeledField("power (percent, blank = default)",
                new TextField()
                    .Text(r.PowerPercent.HasValue ? r.PowerPercent.Value.ToString() : "")
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => r.PowerPercent = parseNullableInt(v))));

            // Ingredient + product lists are editable. Each row carries a
            // ProductPicker (game's ProtoPickerPopup), a numeric quantity
            // field, a port dropdown driven by the recipe's machine ports,
            // and a remove button. The list header has a "+ add" button
            // that appends a default-empty row.
            MachineProto currentMachine = resolveMachine(r.MachineId);

            // Machine port layout summary, rendered in monospace right under
            // the machine picker. This is the "display for layout" the modder
            // needs to cross-reference what port letters and shapes the chosen
            // machine actually exposes, without having to open the port
            // dropdown for each row. It also serves as a sanity check when
            // validation flags a missing-port-type — the user can see at a
            // glance which types the machine does support.
            m_statementColumn.Add(buildMachinePortsView(currentMachine, r.MachineId));

            // Allocate the validation column up front so row edits (product
            // change, port change, qty change, add/remove) can clear+refill
            // it through `refreshValidation` instead of rebuilding the whole
            // form (which would lose focus on whatever field the user is
            // typing in). The column is positioned AFTER the lists below,
            // but `refreshValidation` only mutates its children, so the
            // ordering in the layout stays intact.
            Column validation = new Column();
            validation.Gap(1.pt()).PaddingTopBottom(2.pt());
            Action refreshValidation = () => populateValidationColumn(validation, r, currentMachine);
            refreshValidation();

            m_statementColumn.Add(buildProductListEditor(
                label: "ingredients",
                isInput: true,
                machine: currentMachine,
                list: r.Ingredients ?? (r.Ingredients = new System.Collections.Generic.List<ProductRef>()),
                onChanged: refreshValidation));
            m_statementColumn.Add(buildProductListEditor(
                label: "products",
                isInput: false,
                machine: currentMachine,
                list: r.Products ?? (r.Products = new System.Collections.Generic.List<ProductRef>()),
                onChanged: refreshValidation));

            m_statementColumn.Add(validation);

            if (!string.IsNullOrEmpty(r.SourceFile)) {
                m_statementColumn.Add(new Label(new LocStrFormatted(
                    "source: " + Path.GetFileName(r.SourceFile) +
                    " (lines " + r.SourceStartLine + "–" + r.SourceEndLine + ")")));
            }

            // Save + verify side-by-side. Save splices each recipe back into
            // its source .py file via PackEmitter.Save, then rescans the pack
            // so subsequent edits see fresh line ranges. Verify is the existing
            // round-trip diagnostic (load → render → re-parse → diff).
            Row actionRow = new Row {
                new ButtonText(new LocStrFormatted("Save pack to disk"), onSavePack),
                new ButtonText(new LocStrFormatted("Verify Round-Trip"), onVerifyRoundTrip)
            };
            actionRow.Gap(3.pt());
            m_statementColumn.Add(actionRow);
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
            try {
                PackEmitter.Save(m_currentModel);
                // Re-tokenise + re-parse the pack so the next edit's SourceStartLine
                // / SourceEndLine reflect post-splice line positions.
                PackRegistry.RescanPack(m_currentPack);
                // Reload the model from the refreshed AST; preserve the user's
                // selected recipe by id so the form doesn't reset.
                string previousId = m_selectedRecipe?.RecipeId;
                m_currentModel = PackLoader.Load(m_currentPack);
                m_selectedRecipe = previousId != null
                    ? m_currentModel.Recipes.FirstOrDefault(r => r.RecipeId == previousId)
                    : null;
                rebuildTree();
                if (m_selectedRecipe != null) rebuildStatementEditor();
                else showEmptyStatement("Saved. Select a recipe.");
                Log.Info("RecipeEditor: saved " + m_currentModel.Recipes.Count +
                         " recipe(s) to pack " + m_currentPack.ModId);
            } catch (Exception ex) {
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

        // Editable list of ingredient/product rows. Captured `list` is the
        // RecipeDef's actual List<ProductRef>, so add/remove mutations apply
        // directly to the model — no separate sync step needed before Save.
        // Each row's ProductPicker, qty field, and port dropdown bind through
        // closures over the captured ProductRef, so individual edits also
        // mutate the model in place.
        //
        // `isInput` drives the port dropdown's filter: ingredient rows show
        // the machine's INPUT ports; product rows show OUTPUT ports.
        //
        // Rebuilds the WHOLE list column on add/remove to keep the picker
        // popups, button states, and remove handlers in sync with the
        // current row order without juggling indexes by hand.
        private UiComponent buildProductListEditor(
                string label, bool isInput, MachineProto machine,
                System.Collections.Generic.List<ProductRef> list,
                Action onChanged) {
            Column listColumn = new Column();
            // AlignItemsStretch chain: m_statementColumn stretches its children
            // (this returned Column), which we then stretch (so listColumn
            // gets full width), which then stretches each row. Without these
            // stretches at every Column boundary, the row collapses to its
            // content and leaves the editor pane mostly empty.
            listColumn.Gap(1.pt()).AlignItemsStretch();

            // Compute the valid port names + monospace-formatted layout text
            // once for the whole list — they're shared across rows since they
            // all reference the same machine. The dropdown's option factory
            // reads back through this map (via closure) to render each option
            // with its layout in a monospace font, so users can see the port's
            // type/shape/transport-only flag in addition to the bare name char.
            System.Collections.Generic.Dictionary<string, string> portLayouts =
                collectPortLayouts(machine, isInput);

            // `onChanged` is the validation-refresh hook from rebuildStatementEditor.
            // We invoke it on every list mutation (add/remove) and pass it down to
            // each row so its inner pickers/fields can poke validation too.
            // Doing this in place beats calling rebuildStatementEditor() because a
            // full rebuild blows away keyboard focus on whatever field the user is
            // currently editing — terrible UX when typing a quantity.
            Action rebuild = null;
            rebuild = () => {
                listColumn.Clear();
                // Recompute auto-assigned port slots for every wildcard row in
                // this list. COI's Python load distributes "*" outputs 1:1 over
                // the machine's ports of matching type in order, after explicit
                // port letters are subtracted from the pool. We surface the
                // would-be-picked port next to each "*" in the row so modders
                // see what the runtime is actually wiring up.
                System.Collections.Generic.List<string> autoAssigned =
                    computeAutoAssignedPorts(list, machine, isInput);
                for (int i = 0; i < list.Count; i++) {
                    ProductRef p = list[i];
                    int captured = i;
                    string autoPort = i < autoAssigned.Count ? autoAssigned[i] : null;
                    // Two callbacks: onChanged refreshes validation only (used by
                    // the quantity field — rebuilding the list on every keystroke
                    // would yank keyboard focus). onStructureChanged ALSO rebuilds
                    // the list (used by product picker + port dropdown, whose
                    // discrete clicks don't lose focus) so the auto-assignment
                    // labels for wildcard "*" rows reflect the new shape.
                    Action onStructureChanged = () => {
                        onChanged?.Invoke();
                        rebuild();
                    };
                    // ▲▼ reorder callbacks: swap with the neighbour and rebuild.
                    // Order matters for COI's auto-distribution of "*" — moving
                    // a wildcard row down shifts which port it gets assigned —
                    // so reorders go through the structure-changed path to
                    // refresh the auto-pick labels.
                    bool canMoveUp = i > 0;
                    bool canMoveDown = i < list.Count - 1;
                    Action onMoveUp = canMoveUp ? () => {
                        ProductRef tmp = list[captured - 1];
                        list[captured - 1] = list[captured];
                        list[captured] = tmp;
                        onStructureChanged();
                    } : (Action)null;
                    Action onMoveDown = canMoveDown ? () => {
                        ProductRef tmp = list[captured + 1];
                        list[captured + 1] = list[captured];
                        list[captured] = tmp;
                        onStructureChanged();
                    } : (Action)null;
                    listColumn.Add(buildProductRow(p, portLayouts, autoPort, () => {
                        list.RemoveAt(captured);
                        rebuild();
                        onChanged?.Invoke();
                    }, onChanged, onStructureChanged, onMoveUp, onMoveDown));
                }
                listColumn.Add(new ButtonText(
                    new LocStrFormatted("+ add " + label.TrimEnd('s')),
                    () => {
                        // Default a fresh row's quantity to 1 — quantities are
                        // 1-N per the API constraint, 0 isn't meaningful.
                        list.Add(new ProductRef { Quantity = 1, Port = "*" });
                        rebuild();
                        onChanged?.Invoke();
                    }));
            };
            rebuild();

            Column wrapper = new Column {
                new Label(new LocStrFormatted(label + ":")),
                listColumn
            };
            wrapper.AlignItemsStretch();
            return wrapper;
        }

        // Single row: [Product picker — name+icon] · qty · port-dropdown · ✕ remove.
        // The picker resolves the bound id through TypedRefResolver when the
        // recipe was loaded from a typed reference (Ids.Products.X); free-form
        // string ids like "Product_LeadAcidBatteryEmpty" hit the direct lookup.
        //
        // Quantity is clamped to a minimum of 1 — 0-quantity Products don't
        // make sense for ingredients or outputs and the runtime treats them
        // as bugs.
        //
        // Port is a Dropdown<string> populated from the recipe's machine.
        // "*" always leads (means "any matching port" per the API). When the
        // recipe was loaded with a port that isn't in the machine's current
        // port list — e.g. machine was changed externally — that legacy value
        // is included as a tail option so the picker still displays it.
        private UiComponent buildProductRow(
                ProductRef p,
                System.Collections.Generic.Dictionary<string, string> portLayouts,
                string autoAssignedPort,
                Action onRemove,
                Action onChanged,
                Action onStructureChanged,
                Action onMoveUp,
                Action onMoveDown) {
            Row row = new Row();
            row.Gap(2.pt()).AlignItemsCenter();

            // Product change shifts the auto-assignment slot for every wildcard
            // row downstream (new type changes which port pool we draw from),
            // so it goes through onStructureChanged — rebuild + validate. The
            // discrete click that picks a product doesn't lose focus, so the
            // rebuild is safe here.
            row.Add(new ProtoPicker<ProductProto>(
                m_uiContext.ProtosDb,
                getId: () => p.ProductId,
                setId: id => { p.ProductId = id; onStructureChanged?.Invoke(); },
                emptyLabel: new LocStrFormatted("(pick product…)"),
                title: new LocStrFormatted("Pick product")).FlexGrow(1f));

            // Quantity — int field. Sizing the inner BetterTextField directly
            // (ForFieldSetMinWidth) is necessary because the outer TextField is
            // a wrapper with padding + glow-on-hover decoration: a Width() on
            // the wrapper leaves the actual <input> region narrower than the
            // visible box, which clipped values like "160" into "16…" and let
            // the placeholder bleed visually through short values ("2" → "2qty").
            // Removing the placeholder is also intentional — rows always
            // pre-populate Quantity (default 1 for new rows, parsed value for
            // existing), so a hint serves no purpose and only confuses.
            TextField qty = new TextField()
                .Text(p.Quantity.ToString())
                .PositiveIntegersOnly()
                .ForFieldSetMinWidth(60.px())
                .Width(80.px());
            qty.OnValueChanged(v => {
                if (int.TryParse(v, out int parsed)) {
                    int clamped = parsed < 1 ? 1 : parsed;
                    p.Quantity = clamped;
                    if (clamped != parsed) qty.Text(clamped.ToString());
                    onChanged?.Invoke();
                }
            });
            row.Add(qty);

            // Port — Dropdown<string> driven by the machine's port layout.
            // Build the displayed option set: always "*" first, then valid
            // machine ports, plus the current value when it's not already
            // listed (lets the user see + correct stale values from when
            // the machine was different).
            //
            // "VIRTUAL" is offered only when the row's product is a
            // VirtualProductProto — that's the exact constraint the runtime
            // enforces in RecipeProtoBuilder.resolvePortSelector: "VIRTUAL"
            // is valid only for virtual products. Offering it unconditionally
            // would let the modder pick an invalid combination that the game
            // rejects at registration. Conversely, hiding it from virtual
            // products would mask the only correct port selector for them.
            ProductProto resolved = resolveProduct(p.ProductId);
            bool isVirtualProduct = resolved != null
                && resolved.Type == VirtualProductProto.ProductType;
            System.Collections.Generic.List<string> options =
                new System.Collections.Generic.List<string> { "*" };
            if (portLayouts != null) {
                foreach (string n in portLayouts.Keys) options.Add(n);
            }
            if (isVirtualProduct) options.Add("VIRTUAL");
            string currentPort = string.IsNullOrEmpty(p.Port) ? "*" : p.Port;
            if (!options.Contains(currentPort)) options.Add(currentPort);

            // Capture portLayouts in a closure-backed option factory so the
            // dropdown can render each entry with full layout context (type,
            // shape, transport-only flag) in a monospace font. The bound value
            // remains the plain port-name string so the model continues to
            // round-trip identically.
            System.Collections.Generic.Dictionary<string, string> layoutsForFactory =
                portLayouts ?? new System.Collections.Generic.Dictionary<string, string>();
            // autoAssignedPort is the port the Python runtime would pick when
            // this row's port is "*" — pre-computed by computeAutoAssignedPorts
            // for the whole list, then surfaced in the wildcard option label so
            // modders see exactly where their output/input gets wired up.
            string capturedAuto = autoAssignedPort;
            Dropdown<string> portDropdown = new Dropdown<string>(
                    (option, index, isInDropdown) =>
                        portOptionFactory(option, layoutsForFactory, capturedAuto))
                .SetOptions(options)
                .MinWidth(260.px());
            portDropdown.SetValue(currentPort);
            // Port change also affects downstream wildcard rows' auto-assignment
            // (an explicit letter removes that port from the wildcard pool),
            // so route through onStructureChanged for the same reason the
            // product picker does.
            portDropdown.OnValueChanged((v, _) => {
                p.Port = (v == "*" || string.IsNullOrEmpty(v)) ? null : v;
                onStructureChanged?.Invoke();
            });
            row.Add(portDropdown);

            // ▲▼ reorder buttons appear only when a neighbour exists in that
            // direction. The "disabled but visible" alternative would leave
            // misaligned columns when the topmost row's ▲ is greyed out — a
            // visually quieter approach is to just skip the unusable arrow.
            if (onMoveUp != null) row.Add(new ButtonText(new LocStrFormatted("▲"), onMoveUp));
            if (onMoveDown != null) row.Add(new ButtonText(new LocStrFormatted("▼"), onMoveDown));
            row.Add(new ButtonText(new LocStrFormatted("✕"), onRemove));

            return row;
        }

        // Render one dropdown row in monospace so the layout columns align.
        // Layout text for known ports comes from collectPortLayouts (Name padded
        // + "IN/OUT" + shape name + optional transport-only suffix). Unknown
        // ports (wildcard "*", stale legacy values) get a one-off line.
        private static UiComponent portOptionFactory(
                string option,
                System.Collections.Generic.Dictionary<string, string> portLayouts,
                string autoAssignedPort) {
            string text;
            if (option == "*") {
                // When we know which port the runtime would auto-assign for
                // this row, surface it next to the wildcard so the modder
                // doesn't have to mentally simulate the assignment. Falls back
                // to the generic label when we couldn't compute one (no
                // machine resolved, or no ports of the row's product type).
                text = !string.IsNullOrEmpty(autoAssignedPort)
                    ? "*       (auto → " + autoAssignedPort + ")"
                    : "*       (any matching port)";
            } else if (option == "VIRTUAL") {
                // VIRTUAL is the special selector for virtual products (energy,
                // research, settler-flow). resolvePortSelector returns
                // ImmutableArray.Empty for it — i.e. no physical port — so the
                // runtime never tries to route this product through a tile.
                text = "VIRTUAL (virtual products only)";
            } else if (portLayouts != null && portLayouts.TryGetValue(option, out string layout)) {
                text = layout;
            } else {
                // Stale port preserved from the loaded recipe — flag it so the
                // user sees that the machine no longer exposes this port name.
                text = (option ?? "") + "    (not on current machine)";
            }
            return new Label(new LocStrFormatted(text)).Class(Cls.fontMonospace);
        }

        // Compute, per row, the port the Python runtime would auto-assign for
        // that row's "*" selector. Mirrors COI's load-time distribution:
        //
        //   1. First pass: collect all explicit ports already taken by rows in
        //      this list, grouped by product type. Those ports are removed
        //      from the wildcard pool so two rows don't claim the same port.
        //   2. Second pass: walk the list in order; for each row with no
        //      explicit port (Port == "*" / null / empty), pull the NEXT
        //      available port of that row's product type off the machine in
        //      port-declaration order. That port becomes the row's auto pick.
        //
        // Returns a parallel list where index i holds the would-be-assigned
        // port for `list[i]`, or null when:
        //   * the row has an explicit port already (the dropdown shows the
        //     letter directly, no auto-pick needed)
        //   * the machine couldn't be resolved
        //   * the row's product type isn't represented on the machine
        //   * we ran out of available ports of that type (recipe over-claims)
        private System.Collections.Generic.List<string> computeAutoAssignedPorts(
                System.Collections.Generic.List<ProductRef> list,
                MachineProto machine, bool isInput) {
            var result = new System.Collections.Generic.List<string>();
            if (list == null) return result;
            for (int i = 0; i < list.Count; i++) result.Add(null);
            if (machine == null) return result;

            Mafi.Collections.ImmutableCollections.ImmutableArray<Mafi.Core.Ports.Io.IoPortTemplate> ports =
                isInput ? machine.InputPorts : machine.OutputPorts;

            // typeKey → ordered list of port-name strings on the machine.
            var portsByType = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.List<string>>(StringComparer.Ordinal);
            foreach (var pt in ports) {
                if (pt.Shape == null) continue;
                string typeKey = pt.Shape.AllowedProductType.ToString();
                if (!portsByType.TryGetValue(typeKey, out var bucket)) {
                    bucket = new System.Collections.Generic.List<string>();
                    portsByType[typeKey] = bucket;
                }
                bucket.Add(pt.Name.ToString());
            }

            // First pass: subtract explicitly-taken port letters from the pool
            // for each product type. Resolve the row's ProductProto so we know
            // which type's pool to touch.
            var taken = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.HashSet<string>>(StringComparer.Ordinal);
            foreach (ProductRef pr in list) {
                if (string.IsNullOrEmpty(pr.Port) || pr.Port == "*" || pr.Port == "VIRTUAL")
                    continue;
                ProductProto proto = resolveProduct(pr.ProductId);
                if (proto == null) continue;
                string typeKey = proto.Type.ToString();
                if (!taken.TryGetValue(typeKey, out var set)) {
                    set = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                    taken[typeKey] = set;
                }
                set.Add(pr.Port);
            }

            // Second pass: walk wildcards in order, claim next free port of
            // the row's product type from the machine's port-declaration order.
            var nextIdx = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < list.Count; i++) {
                ProductRef pr = list[i];
                if (!(string.IsNullOrEmpty(pr.Port) || pr.Port == "*")) continue;
                ProductProto proto = resolveProduct(pr.ProductId);
                if (proto == null) continue;
                string typeKey = proto.Type.ToString();
                if (!portsByType.TryGetValue(typeKey, out var bucket)) continue;
                nextIdx.TryGetValue(typeKey, out int idx);
                taken.TryGetValue(typeKey, out var takenSet);
                while (idx < bucket.Count && takenSet != null && takenSet.Contains(bucket[idx])) idx++;
                if (idx < bucket.Count) {
                    result[i] = bucket[idx];
                    idx++;
                }
                nextIdx[typeKey] = idx;
            }

            return result;
        }

        // Clear + repopulate the validation column with the latest issues.
        // Called both at form-render time and on every row mutation (product
        // change, port change, qty change, add/remove) — see refreshValidation
        // wired through buildProductListEditor → buildProductRow. Doing this
        // incrementally beats rebuilding the whole statement editor since a
        // full rebuild would steal keyboard focus from whatever field the
        // user is currently editing.
        private void populateValidationColumn(Column validation, RecipeDef r, MachineProto machine) {
            validation.Clear();
            validation.Add(new Label(new LocStrFormatted("Validation:")).FontBold());
            System.Collections.Generic.List<string> issues = validateRecipeIo(r, machine);
            if (issues.Count == 0) {
                validation.Add(new Label(new LocStrFormatted("✓ All ports valid"))
                    .Color(ColorRgba.Green));
            } else {
                foreach (string msg in issues) {
                    validation.Add(new Label(new LocStrFormatted("⚠ " + msg))
                        .Color(ColorRgba.Red));
                }
            }
        }

        // Build a panel that summarizes the chosen machine's port layout.
        // Inputs and outputs are split into two sub-lists rendered in
        // monospace so the columns (port-letter / shape / transport-only)
        // align. Falls back to a "machine unresolved" notice when the model
        // references a machine id we can't find in ProtosDb — the user can
        // then go fix the picker or load the missing dependency.
        private UiComponent buildMachinePortsView(MachineProto machine, string machineId) {
            Column panel = new Column();
            panel.Gap(1.pt()).PaddingTopBottom(2.pt()).AlignItemsStretch();
            panel.Add(new Label(new LocStrFormatted("Machine ports:")).FontBold());

            if (machine == null) {
                string msg = string.IsNullOrEmpty(machineId)
                    ? "  (no machine selected)"
                    : "  (machine '" + machineId + "' could not be resolved)";
                panel.Add(new Label(new LocStrFormatted(msg))
                    .Color(ColorRgba.LightGray));
                return panel;
            }

            // Render the entity's ASCII layout grid first. EntityLayout exposes
            // the raw source string the proto was authored from (typed by the
            // game devs into MachineProto factories), where each character is a
            // tile and port letters identify connection points. Showing this
            // verbatim lets modders see WHERE on the machine a port lives —
            // including the "ports stacked on top of each other" cases the
            // single-line text summary can't convey. Each newline becomes a
            // separate monospace label so columns align across rows.
            string layoutStr = machine.Layout != null ? machine.Layout.SourceLayoutStr : null;
            if (!string.IsNullOrEmpty(layoutStr)) {
                panel.Add(new Label(new LocStrFormatted("  Layout:")));
                foreach (string line in layoutStr.Split('\n')) {
                    // TrimEnd of trailing '\r' so Windows line endings don't
                    // bloat row width with phantom whitespace.
                    string row = line.TrimEnd('\r');
                    panel.Add(new Label(new LocStrFormatted("    " + row))
                        .Class(Cls.fontMonospace));
                }
            }

            // collectPortLayouts already produces the monospace-padded lines.
            // Re-use it for both directions so the format stays in sync with
            // the dropdown rendering.
            var inputs  = collectPortLayouts(machine, isInput: true);
            var outputs = collectPortLayouts(machine, isInput: false);

            panel.Add(new Label(new LocStrFormatted("  Inputs:")));
            if (inputs.Count == 0) {
                panel.Add(new Label(new LocStrFormatted("    (none)")).Class(Cls.fontMonospace));
            } else {
                foreach (var kvp in inputs) {
                    panel.Add(new Label(new LocStrFormatted("    " + kvp.Value))
                        .Class(Cls.fontMonospace));
                }
            }
            panel.Add(new Label(new LocStrFormatted("  Outputs:")));
            if (outputs.Count == 0) {
                panel.Add(new Label(new LocStrFormatted("    (none)")).Class(Cls.fontMonospace));
            } else {
                foreach (var kvp in outputs) {
                    panel.Add(new Label(new LocStrFormatted("    " + kvp.Value))
                        .Class(Cls.fontMonospace));
                }
            }
            return panel;
        }

        // Resolve a ProductRef.ProductId to its live ProductProto. Same two-step
        // strategy as resolveMachine: direct ProtosDb hit, then typed-ref path
        // fallback through Mafi.Base.Ids reflection. Returns null when neither
        // resolves — the validator then reports "unknown product" rather than
        // a port-mismatch the user can't diagnose.
        private ProductProto resolveProduct(string productId) {
            if (string.IsNullOrEmpty(productId)) return null;
            Option<ProductProto> direct =
                m_uiContext.ProtosDb.Get<ProductProto>(new Proto.ID(productId));
            if (direct.HasValue) return direct.Value;
            string resolved = TypedRefResolver.ResolveOrNull(productId);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<ProductProto> byPath =
                    m_uiContext.ProtosDb.Get<ProductProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // Replicate the machine I/O validation that RecipeProtoBuilder runs
        // before accepting a recipe (see Mafi.Core.Factory.Recipes.RecipeProtoBuilder.verifyRecipeIo).
        //
        // The game throws ProtoBuilderException on registration if any of:
        //   * the machine has no ports of a product type the recipe needs
        //   * the machine has fewer ports of that type than the recipe asks for
        //   * (outputs only) multiple wildcard outputs of the same type would
        //     produce ambiguous routing
        // …and an exception there means the pack fails to load. Surfacing the
        // same failures in the editor lets modders correct the recipe BEFORE
        // hitting that wall.
        //
        // Differences from the game's verifyRecipeIo:
        //   * Virtual products are skipped from the recipe-side counts (matches
        //     countGroupByProductType which excludes VirtualProductProto).
        //   * If the machine has an ANY-typed port, all checks for that
        //     direction are skipped (matches the ContainsKey(ProductType.ANY)
        //     short-circuit).
        //   * Unresolved machines / products are reported as warnings rather
        //     than treated as "no matching port" — the modder may be editing
        //     while a dependency is still being added.
        //   * "Ports.Length > 1" in the ambiguity check corresponds to a port
        //     selector that matches multiple ports — in our editor that's
        //     simply Port == "*" or empty.
        private System.Collections.Generic.List<string> validateRecipeIo(
                RecipeDef r, MachineProto machine) {
            var problems = new System.Collections.Generic.List<string>();
            if (machine == null) {
                if (!string.IsNullOrEmpty(r.MachineId)) {
                    problems.Add("Machine '" + r.MachineId + "' could not be resolved — "
                                 + "port validation skipped until dependency loads.");
                }
                return problems;
            }
            validateDirection(r.Ingredients, machine.InputPorts, isInput: true, problems);
            validateDirection(r.Products,    machine.OutputPorts, isInput: false, problems);
            return problems;
        }

        private void validateDirection(
                System.Collections.Generic.List<ProductRef> refs,
                Mafi.Collections.ImmutableCollections.ImmutableArray<Mafi.Core.Ports.Io.IoPortTemplate> ports,
                bool isInput,
                System.Collections.Generic.List<string> problems) {
            if (refs == null || refs.Count == 0) return;

            // IMPORTANT: ProductType's `==` operator returns true whenever EITHER
            // side is ANY — see Mafi.Core.Products.ProductType:
            //     if (pt1.m_protoType == typeof(ProductType)
            //         || pt2.m_protoType == typeof(ProductType)) return true;
            // That makes `port.AllowedProductType == ProductType.ANY` true for
            // every port, not just ANY-typed ones, which silently broke the
            // earlier short-circuit. Using a Dictionary<ProductType,…> is also
            // unsafe because Dictionary.Equals delegates to that same operator.
            // We sidestep both pitfalls by keying everything off ProductType.
            // ToString() — which returns "ANY" for the wildcard and the proto
            // class's simple name (e.g. "GasProductProto") otherwise — and the
            // string keys collide only when product types genuinely match.

            // Group machine ports by their AllowedProductType (as string key).
            var machineCounts = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var pt in ports) {
                if (pt.Shape == null) continue;
                string key = pt.Shape.AllowedProductType.ToString();
                machineCounts.TryGetValue(key, out int existing);
                machineCounts[key] = existing + 1;
            }

            // Universal-port short-circuit: matches verifyRecipeIo's
            //     if (!valueCleared.ContainsKey(ProductType.ANY)) { … }
            // Done AFTER the populate pass — see the comment above for why we
            // can't use `==` directly on individual ports.
            if (machineCounts.ContainsKey("ANY")) return;

            // Group the recipe's products by Type — skip virtuals (the game does
            // the same in countGroupByProductType) and also skip unresolved
            // products with a dedicated warning so the modder sees the cause.
            var recipeCounts = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal);
            // Track wildcard recipe entries per product type for the output
            // ambiguity check below — a wildcard ("*" / null Port) corresponds
            // to "Ports.Length > 1" in the game's verifyRecipeIo when the
            // machine exposes >1 ports of that type.
            var wildcardCounts = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal);
            string dirNoun = isInput ? "input" : "output";
            string virtualKey = VirtualProductProto.ProductType.ToString();

            foreach (ProductRef pr in refs) {
                if (string.IsNullOrEmpty(pr.ProductId)) {
                    problems.Add("Recipe " + dirNoun + " row has no product selected.");
                    continue;
                }
                ProductProto proto = resolveProduct(pr.ProductId);
                if (proto == null) {
                    problems.Add("Recipe " + dirNoun + " '" + pr.ProductId
                                 + "' could not be resolved to a ProductProto.");
                    continue;
                }
                string protoTypeKey = proto.Type.ToString();
                if (protoTypeKey == virtualKey) {
                    // Virtual products don't consume machine ports — but if the
                    // user picked a real port (not "*"/"VIRTUAL") flag it as
                    // suspect. The game's resolvePortSelector accepts "VIRTUAL"
                    // or "*" only for virtual products.
                    string port = pr.Port;
                    if (!string.IsNullOrEmpty(port) && port != "*" && port != "VIRTUAL") {
                        problems.Add("Virtual product '" + pr.ProductId
                                     + "' has port '" + port + "' — virtual products "
                                     + "must use '*' or 'VIRTUAL'.");
                    }
                    continue;
                }
                // Non-virtual product picked the special VIRTUAL port — the
                // game's resolvePortSelector rejects that combination.
                if (pr.Port == "VIRTUAL") {
                    problems.Add("Product '" + pr.ProductId
                                 + "' has port 'VIRTUAL' but is not a virtual product.");
                    continue;
                }
                recipeCounts.TryGetValue(protoTypeKey, out int existing);
                recipeCounts[protoTypeKey] = existing + 1;
                if (string.IsNullOrEmpty(pr.Port) || pr.Port == "*") {
                    wildcardCounts.TryGetValue(protoTypeKey, out int wc);
                    wildcardCounts[protoTypeKey] = wc + 1;
                }
            }

            foreach (var kvp in recipeCounts) {
                string needed = kvp.Key;
                int need = kvp.Value;
                if (!machineCounts.TryGetValue(needed, out int have)) {
                    problems.Add("Not enough " + needed + " " + dirNoun
                                 + " ports — machine has no port for that product type.");
                    continue;
                }
                if (have < need) {
                    problems.Add("Not enough " + needed + " " + dirNoun
                                 + " ports — recipe needs " + need
                                 + ", machine has " + have + ".");
                    continue;
                }
                // NOTE: an older draft also surfaced an "ambiguous wildcard
                // routing" warning when multiple outputs of the same type used
                // port='*' on a machine with multiple ports of that type. That
                // mirrored RecipeProtoBuilder.verifyRecipeIo's literal source,
                // but in practice COI's Python load auto-distributes wildcards
                // 1:1 when count(wildcards) == count(machine ports of type),
                // so recipes like CustomRecipe_AirFilterIL_Scubbing (2 Loose
                // outputs + 2 Loose ports, all `*`) load fine. Explicit port
                // letters are only required when you genuinely want to pin
                // outputs to specific ports — flagging the wildcard pattern
                // here would produce false positives for the common, working
                // case. The wildcardCounts dict is still populated above so
                // we keep the data plumbed for future, narrower checks.
            }
        }

        // Resolve r.MachineId to its live MachineProto via ProtosDb. Tries
        // the direct id first, then falls back to TypedRefResolver for
        // typed-ref paths (Ids.Machines.*). Returns null when neither works
        // — port dropdowns then show only "*" + the current stored value.
        private MachineProto resolveMachine(string machineId) {
            if (string.IsNullOrEmpty(machineId)) return null;
            Option<MachineProto> direct =
                m_uiContext.ProtosDb.Get<MachineProto>(new Proto.ID(machineId));
            if (direct.HasValue) return direct.Value;
            string resolved = TypedRefResolver.ResolveOrNull(machineId);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<MachineProto> byPath =
                    m_uiContext.ProtosDb.Get<MachineProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // Read the valid ports off the machine and build a map from port-name
        // string (the modder-facing `port=` argument) to a monospace-aligned
        // layout-description string. Producing the layout text here — instead
        // of letting the option factory walk the PortSpec at render time —
        // means the dropdown popup only has to do dictionary lookups and string
        // formatting on a cold path.
        //
        // The layout format is fixed-width so the dropdown columns line up
        // when rendered in Cls.fontMonospace:
        //   "A | IN  | Loose"
        //   "B | IN  | Fluid    | conveyor-only"
        //
        // Columns we care about per PortSpec:
        //   Name              — single char, the port identifier
        //   Type              — Input / Output  (filtered to one direction)
        //   Shape.Strings.Name — shape display name (Loose, Fluid, Gas, …),
        //                       falls back to Shape.LayoutChar then "?"
        //   CanOnlyConnectToTransports — flagged as " | conveyor-only" suffix
        //
        // Only the direction matching `isInput` is emitted — ingredient rows
        // see inputs, product rows see outputs. Ports of type Any would belong
        // to both but the game's recipe runtime expects a direction-typed
        // match, so we skip them rather than guess.
        private static System.Collections.Generic.Dictionary<string, string> collectPortLayouts(
                MachineProto machine, bool isInput) {
            var result = new System.Collections.Generic.Dictionary<string, string>(
                StringComparer.Ordinal);
            if (machine == null) return result;
            IoPortType target = isInput ? IoPortType.Input : IoPortType.Output;
            string dirLabel = isInput ? "IN " : "OUT";
            // MachineProto.Ports yields IoPortTemplate (not PortSpec). Template
            // wraps the spec plus relative position/direction in the entity
            // layout — we currently only surface the spec fields, but the
            // position/direction is what would let us draw the multi-tile
            // layout grid the user wants long-term.
            foreach (var port in machine.Ports) {
                if (port.Type != target) continue;
                string shapeName;
                if (port.Shape != null) {
                    // Three-tier fallback so the shape column is never blank:
                    //   1) Localized display name from the shape's Strings.Name
                    //      (e.g. "Loose", "Fluid"). LocStr.Id and TranslatedString
                    //      are both string fields — either can be empty even when
                    //      the struct itself is populated, so check both.
                    //   2) The shape's proto id (e.g. "PortShape_Loose") — always
                    //      present for any registered prototype.
                    //   3) The LayoutChar (the single-char glyph in the entity's
                    //      ASCII grid) as last resort. We never want to render an
                    //      empty column; an empty cell looks like a bug to modders.
                    string translated = port.Shape.Strings.Name.TranslatedString;
                    if (!string.IsNullOrEmpty(translated)) {
                        shapeName = translated;
                    } else if (!string.IsNullOrEmpty(port.Shape.Id.Value)) {
                        shapeName = port.Shape.Id.Value;
                    } else {
                        shapeName = port.Shape.LayoutChar.ToString();
                    }
                } else {
                    shapeName = "?";
                }
                // PadRight to fixed 10-char shape column so the optional
                // transport-only suffix begins at the same x on every row.
                string padded = shapeName.Length >= 10
                    ? shapeName
                    : shapeName + new string(' ', 10 - shapeName.Length);
                string layout = port.Name + " | " + dirLabel + " | " + padded;
                // CanOnlyConnectToTransports is on PortSpec, not IoPortTemplate.
                if (port.Spec.CanOnlyConnectToTransports) layout += " | conveyor-only";
                result[port.Name.ToString()] = layout;
            }
            return result;
        }

        private static int? parseNullableInt(string s) {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return int.TryParse(s, out int i) ? i : (int?)null;
        }

        private static string formatProductRef(ProductRef p) {
            StringBuilder sb = new StringBuilder();
            sb.Append(p.ProductId ?? "?").Append(" x ").Append(p.Quantity);
            if (!string.IsNullOrEmpty(p.Port) && p.Port != "*") sb.Append(" @ ").Append(p.Port);
            return sb.ToString();
        }

        private void showEmptyStatement(string message) {
            m_statementColumn.Clear();
            m_statementColumn.Add(new Label(new LocStrFormatted(message)));
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

        // Append `fileName` to the dependencies(...) call in __init__.py — or
        // create __init__.py with a single-entry dependencies call when missing.
        // The append happens in place via line splice so any non-deps content
        // (comments, helpers) the modder added stays put.
        private void appendToLoadOrder(string definitionsDir, string fileName) {
            string initPath = Path.Combine(definitionsDir, "__init__.py");
            if (!File.Exists(initPath)) {
                File.WriteAllText(initPath,
                    "dependencies(\"" + fileName + "\")\n",
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                return;
            }
            // Find the existing dependencies() call's line range from the
            // cached AST. If none, prepend a fresh call at the top.
            int startLine = 0, endLine = 0;
            System.Collections.Generic.List<string> existing = new System.Collections.Generic.List<string>();
            bool found = false;
            foreach (LoadedFile file in m_currentPack.Files) {
                if (Path.GetFileName(file.AbsolutePath) != "__init__.py") continue;
                if (file.Ast == null) continue;
                foreach (PythonAPI.Statements.IStatement stmt in file.Ast.statements) {
                    if (!(stmt is PythonAPI.EvaluateStatement ev)) continue;
                    if (!(ev.Expression is PythonAPI.Expressions.CallExpression call)) continue;
                    if (!(call.Calle is PythonAPI.Expressions.VariableExpression v)
                        || v.Path != "dependencies") continue;
                    startLine = ev.StartLine;
                    endLine   = ev.EndLine;
                    foreach (PythonAPI.Arguments.IArgument arg in call.Arguments) {
                        if (arg is PythonAPI.Arguments.OrderedArgument
                            && arg.Expression is PythonAPI.Expressions.StringConstant s) {
                            existing.Add(s.Value);
                        }
                    }
                    found = true;
                    break;
                }
                if (found) break;
            }

            existing.Add(fileName);
            StringBuilder rendered = new StringBuilder("dependencies(");
            for (int i = 0; i < existing.Count; i++) {
                if (i > 0) rendered.Append(", ");
                rendered.Append('"').Append(existing[i]).Append('"');
            }
            rendered.Append(')');

            string[] lines = File.ReadAllLines(initPath, Encoding.UTF8);
            System.Collections.Generic.List<string> output = new System.Collections.Generic.List<string>(lines);
            if (found && startLine > 0 && endLine >= startLine && endLine <= output.Count) {
                int startIdx = startLine - 1;
                output.RemoveRange(startIdx, endLine - startIdx);
                output.InsertRange(startIdx, rendered.ToString().Split('\n'));
            } else {
                output.Insert(0, rendered.ToString());
            }
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
            if (PackRegistry.Count == 0) return;

            FloatingColumn picker = new FloatingColumn(
                FloaterPositionPolicy.ABOVE,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            // FloatingColumn renders transparent by default; Cls.panelBg gives
            // the standard recessed-dark COI panel chrome so the popup looks
            // like a real menu rather than glyphs floating over the editor.
            picker.Class(Cls.panelBg)
                  .Padding(2.pt()).Gap(1.pt())
                  .MinWidth(320.px())
                  .AlignItemsStretch();

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
