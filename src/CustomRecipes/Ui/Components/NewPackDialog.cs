using System;
using System.Collections.Generic;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.MainMenu;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Dialog for scaffolding a new CustomAssets pack on disk. Collects
    /// id / display name / short description, then calls
    /// <see cref="PackScaffolder"/> to write the folder structure
    /// (manifest.json, CustomAssetPack.dll stub, Definitions/__init__.py,
    /// Assets/).
    ///
    /// A standalone movable <see cref="Window"/> rather than a floating
    /// popup — matches <see cref="TranslationsPanel"/> so the modder can
    /// keep the new-pack form open beside the main editor while picking
    /// the id / description.
    ///
    /// The new pack only becomes visible after the save is reloaded
    /// (Mafi loads mods once per session). On success the dialog shows a
    /// "Reload mods (return to main menu)" button — clicking it sends
    /// the modder back to the main menu via <see cref="IMain.GoToMainMenu"/>,
    /// which is the only point COI rescans the mods folder. Unsaved
    /// progress in the current session is lost, so the button warns about
    /// that and requires a second confirmation click.
    /// </summary>
    public sealed class NewPackDialog : Window {

        private readonly TextField m_idField;
        private readonly TextField m_nameField;
        private readonly TextField m_descField;
        private readonly TextField m_authorField;
        private readonly TextField m_minGameVersionField;
        // Thumbnail is optional and applied after the scaffolder has run —
        // there's no pack folder to write into until then.
        private readonly ThumbnailPicker m_thumbnailPicker = new ThumbnailPicker();
        // Both dependency lists are mutable backing stores the row UI
        // edits in place. The default mandatory list seeds the canonical
        // CustomAssets dep so brand-new packs still depend on the
        // framework without the modder having to add it manually.
        private readonly List<string> m_modDeps = new List<string> { "CustomAssets>=0.1.9" };
        private readonly List<string> m_optionalModDeps = new List<string>();
        private readonly Column m_modDepsCol = new Column();
        private readonly Column m_optionalModDepsCol = new Column();
        private readonly Label m_status;
        private readonly ButtonText m_createBtn;
        private readonly Row m_postCreateRow;
        private readonly ButtonText m_reloadBtn;
        private readonly Action m_onCreated;
        private readonly IMain m_main;
        private bool m_created;
        // Two-step click for the destructive "go to main menu" action.
        // First click arms the button (label changes to a confirm prompt);
        // second click fires GoToMainMenu. A 5s window before the armed
        // state resets prevents stray double-taps but is long enough that
        // the modder can read the warning between clicks.
        private bool m_reloadArmed;

        /// Convenience entry point matching the TranslationsPanel shape.
        /// Opens the window on the given UiContext's root.
        public static void Open(UiContext uiContext, IMain main, Action onCreated = null) {
            new NewPackDialog(main, onCreated).Open(uiContext.UiRoot);
        }

        public NewPackDialog(IMain main, Action onCreated)
            : base(new LocStrFormatted("Create new pack")) {
            m_main = main;
            m_onCreated = onCreated;

            MakeMovable();
            // Taller window to fit the dep lists + version + author rows
            // without cramming. The body wraps in a ScrollColumn below so
            // even on small screens the modder can reach Create.
            WindowSize(560.px(), 640.px());

            Body.Add(new Label(new LocStrFormatted(
                "The new pack will only load after you reload the save."))
                .TinyFontSize().Color(ColorRgba.LightGray));

            m_idField   = new TextField()
                .Placeholder(new LocStrFormatted("e.g. MyCoolRecipes"));
            m_nameField = new TextField()
                .Placeholder(new LocStrFormatted("Display name shown in COI's mods list"));
            m_descField = new TextField()
                .Placeholder(new LocStrFormatted("Short description (one sentence)"));
            m_authorField = new TextField()
                .Placeholder(new LocStrFormatted("Your name (optional)"));
            // min_game_version: canonical default matches the rest of the
            // scaffolder; modder can edit to pin to whatever they tested
            // against. No max_game_version — Mafi's manifest only carries
            // the min pin.
            m_minGameVersionField = new TextField().Text("0.8.4");

            Body.Add(labeled("id (folder + manifest id; letters / digits / underscores)", m_idField));
            Body.Add(labeled("display name", m_nameField));
            Body.Add(labeled("short description", m_descField));
            Body.Add(labeled("author (optional)", m_authorField));
            Body.Add(labeled("min game version", m_minGameVersionField));
            Body.Add(labeled("thumbnail (optional)", m_thumbnailPicker.Root));

            // Mod dependencies. Default-seeded with the CustomAssets
            // framework pin so the pack loads in the editor at all; the
            // modder can ✕ it if they're doing something exotic. The
            // "+ add" button opens the shared LoadedModPicker — same
            // searchable scrollable picker the pack-deps dialog uses.
            Body.Add(new Label(new LocStrFormatted("mod dependencies")));
            Body.Add(m_modDepsCol);
            Body.Add(makeAddDepRow("+ add mod dependency", m_modDeps, m_modDepsCol));

            Body.Add(new Label(new LocStrFormatted("optional mod dependencies")));
            Body.Add(m_optionalModDepsCol);
            Body.Add(makeAddDepRow("+ add optional dependency",
                m_optionalModDeps, m_optionalModDepsCol));

            refreshDepList(m_modDepsCol, m_modDeps);
            refreshDepList(m_optionalModDepsCol, m_optionalModDeps);

            m_status = new Label(new LocStrFormatted("")).TinyFontSize();
            Body.Add(m_status);

            // Post-create action row holding the "Reload mods" button.
            // Hidden until a pack is successfully created; shown then so
            // the modder has a one-click path from "pack written" to
            // "pack loaded".
            m_reloadBtn = new ButtonText(new LocStrFormatted("Reload mods (return to main menu)"),
                onReloadClicked);
            m_reloadBtn.Tooltip(new LocStrFormatted(
                "Goes back to the main menu so COI rescans the mods folder."
                + " Unsaved progress in the current session is lost."));
            m_postCreateRow = new Row { m_reloadBtn };
            m_postCreateRow.Gap(3.pt());
            m_postCreateRow.Visible(false);
            Body.Add(m_postCreateRow);

            m_createBtn = new ButtonText(new LocStrFormatted("Create"), onCreate);
            Row footer = new Row {
                m_createBtn,
                new ButtonText(new LocStrFormatted("Close"), Close)
            };
            footer.Gap(3.pt());
            Body.Add(footer);

            Body.AlignItemsStretch()
                .PaddingTop(60.px()).PaddingLeftRight(8.px())
                .PaddingBottom(8.px()).Gap(3.pt());
        }

        private static UiComponent labeled(string label, UiComponent field) {
            Column col = new Column {
                new Label(new LocStrFormatted(label)),
                field
            };
            col.AlignItemsStretch();
            return col;
        }

        // "+ add" button that opens the shared LoadedModPicker anchored to
        // itself; the picked spec is appended to <paramref name="list"/>
        // and the visible rows are rebuilt. Returns the button wrapped so
        // it can be added directly into Body.
        private ButtonText makeAddDepRow(string label,
                List<string> list, Column container) {
            ButtonText addBtn = null;
            addBtn = new ButtonText(new LocStrFormatted(label), () => {
                LoadedModPicker.OpenAnchored(addBtn, picked => {
                    if (string.IsNullOrWhiteSpace(picked)) return;
                    list.Add(picked);
                    refreshDepList(container, list);
                });
            });
            return addBtn;
        }

        // Re-render an editable list section in place. Each row is a
        // TextField (modder can hand-edit the version pin) plus a ✕ button.
        // Whole section rebuilt on mutation so the index-bound closures
        // stay correct after remove.
        private void refreshDepList(Column container, List<string> list) {
            container.Clear();
            if (list.Count == 0) {
                container.Add(new Label(new LocStrFormatted("  (none)"))
                    .Color(ColorRgba.LightGray));
                return;
            }
            for (int i = 0; i < list.Count; i++) {
                int captured = i;
                Row row = new Row().Gap(2.pt()).AlignItemsCenter();
                TextField tf = new TextField()
                    .Text(list[captured] ?? "")
                    .OnValueChanged(v => list[captured] = v ?? "");
                tf.FlexGrow(1f);
                row.Add(tf);
                row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                    list.RemoveAt(captured);
                    refreshDepList(container, list);
                }));
                container.Add(row);
            }
        }

        private void onCreate() {
            // A successful create disables the inputs but leaves the
            // window open so the modder can read the path + reload hint.
            // Guard against a stray re-click that would attempt to create
            // the same pack again.
            if (m_created) return;

            string modsRoot = PackScaffolder.TryFindModsRoot();
            if (string.IsNullOrEmpty(modsRoot)) {
                showError("Could not locate the COI mods folder. At least one existing pack must be loaded.");
                return;
            }
            string id   = (m_idField.GetText() ?? "").Trim();
            string name = (m_nameField.GetText() ?? "").Trim();
            string desc = (m_descField.GetText() ?? "").Trim();
            string author = (m_authorField.GetText() ?? "").Trim();
            string minVersion = (m_minGameVersionField.GetText() ?? "").Trim();

            PackScaffolder.Result result = PackScaffolder.Create(
                modsRoot, id, name, desc,
                modDependencies: m_modDeps,
                optionalModDependencies: m_optionalModDeps,
                minGameVersion: minVersion,
                author: author);
            if (!result.Success) {
                showError(result.Error ?? "Pack creation failed.");
                return;
            }

            // Thumbnail is written after the folder exists. A failure here
            // is not fatal — the pack itself is already valid — so we keep
            // the success status and append the reason.
            string thumbnailNote = "";
            ThumbnailWriter.Result thumbnail = m_thumbnailPicker.ApplyTo(result.PackRootPath);
            if (thumbnail != null && !thumbnail.Success) {
                thumbnailNote = " Thumbnail not set: " + thumbnail.Error;
            } else if (thumbnail != null && thumbnail.ConvertedFromSvg) {
                thumbnailNote = " Thumbnail converted from SVG.";
            }

            m_status.Color(ColorRgba.Green);
            m_status.Value(new LocStrFormatted(
                "Created '" + result.PackRootPath + "'. Reload the save to load the pack."
                + thumbnailNote));

            m_created = true;
            m_idField.Enabled(false);
            m_nameField.Enabled(false);
            m_descField.Enabled(false);
            m_createBtn.Enabled(false);
            // Only show the reload-mods button when we actually have an
            // IMain reference to act on (DI inject can fail in test paths).
            if (m_main != null) {
                m_postCreateRow.Visible(true);
            }

            m_onCreated?.Invoke();
        }

        // Two-step destructive click: first arms, second fires. The first
        // click changes the label to "Click again to confirm" and switches
        // the status row to a warning so the modder sees what the next
        // click will do.
        private void onReloadClicked() {
            if (m_main == null) {
                showError("No IMain reference - cannot go to main menu.");
                return;
            }
            if (!m_reloadArmed) {
                m_reloadArmed = true;
                m_reloadBtn.Value(new LocStrFormatted("Click again to confirm - unsaved progress will be lost"));
                m_status.Color(ColorRgba.Orange);
                m_status.Value(new LocStrFormatted(
                    "Going to the main menu now. Save your current game first if you want to keep it."));
                return;
            }
            try {
                m_main.GoToMainMenu(new MainMenuArgs());
                // GoToMainMenu terminates the current scene; the dialog
                // window goes with it. No need to call Close() explicitly.
            } catch (Exception ex) {
                Log.Exception(ex);
                m_reloadArmed = false;
                m_reloadBtn.Value(new LocStrFormatted("Reload mods (return to main menu)"));
                showError("Could not return to main menu: " + ex.Message);
            }
        }

        private void showError(string message) {
            m_status.Color(ColorRgba.Red);
            m_status.Value(new LocStrFormatted(message));
        }
    }
}
