using System;
using System.Collections.Generic;
using CustomAssets.Ui;
using Mafi;
using Mafi.Core.Mods;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using UnityEngine;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Searchable picker over currently-loaded mods. Opens a
    /// PanelWithHeader anchored to a UiComponent with a search box in the
    /// header (filters across id + display name + version) and a
    /// ScrollColumn body so long mod lists scroll cleanly. Selecting a row
    /// calls back with the canonical <c>&lt;id&gt;&gt;=&lt;version&gt;</c>
    /// spec the manifest expects.
    ///
    /// Centralizes what PackDepsPanel used to inline so the new-pack
    /// dialog (and any future dialog needing a mod picker) can drop the
    /// affordance in with a single call.
    /// </summary>
    public static class LoadedModPicker {

        public static void OpenAnchored(UiComponent anchor, Action<string> onPicked) {
            FloatingColumn picker = new FloatingColumn(
                FloaterPositionPolicy.ABOVE,
                keepOpenOnHover: false,
                openAfterDelay: false,
                closeOnClickOutside: true);
            LocStrFormatted title = new LocStrFormatted("Pick a loaded mod");
            PanelWithHeader panel = picker.AddAndReturn(new PanelWithHeader(title))
                .AlignItemsStretch()
                .Gap(2.pt())
                .MinWidth(420.px())
                .MaxHeight(560.px());
            panel.Header.Clear();
            panel.Header.Add(new Label(title).FontBold());

            TextField search = new TextField()
                .Placeholder(new LocStrFormatted("search by id or name…"));
            panel.Header.Add(search);

            ScrollColumn list = new ScrollColumn();
            list.Gap(1.pt()).MaxHeight(480.px()).AlignItemsStretch();
            panel.BodyAdd(list);

            // Each row paired with a pre-lowercased haystack for filtering.
            // Mirrors MachineIdPicker / PackDepsPanel so search semantics
            // stay consistent across the editor.
            List<KeyValuePair<UiComponent, string>> rows =
                new List<KeyValuePair<UiComponent, string>>();

            int added = 0;
            foreach (LoadedModData mod in ModsLoader.LoadedAndFailedMods) {
                if (mod?.Manifest == null) continue;
                ModManifest m = mod.Manifest;
                if (string.IsNullOrEmpty(m.Id)) continue;
                string version = m.Version != null ? m.Version.ToString() : "";
                string spec = string.IsNullOrEmpty(version) ? m.Id : (m.Id + ">=" + version);
                string displayName = string.IsNullOrEmpty(m.DisplayName) ? m.Id : m.DisplayName;
                ButtonRow rowBtn = new ButtonRow(Button.General, () => {
                    picker.Close();
                    onPicked(spec);
                });
                rowBtn.Gap(2.pt()).AlignItemsCenter();
                // Thumbnail (real or fallback) — mirrors the pack switcher
                // row shape so modders see the same visual treatment for
                // a given mod across both surfaces.
                Texture2D tex = PackThumbnailCache.TryGet(mod);
                if (tex != null) {
                    rowBtn.Add(new Img(tex).Width(36.px()).Height(36.px()));
                } else {
                    rowBtn.Add(new Img(PackThumbnailCache.FallbackIconPath)
                        .Width(36.px()).Height(36.px()));
                }
                Column labels = new Column {
                    new Label(new LocStrFormatted(displayName)).Fill(),
                    new Label(new LocStrFormatted(m.Id
                        + (string.IsNullOrEmpty(version) ? "" : "   v" + version)))
                        .TinyFontSize().Fill(),
                };
                labels.FlexGrow(1f);
                rowBtn.Add(labels);
                list.Add(rowBtn);
                rows.Add(new KeyValuePair<UiComponent, string>(rowBtn,
                    (displayName + " " + m.Id + " " + version).ToLowerInvariant()));
                added++;
            }
            if (added == 0) {
                list.Add(new Label(new LocStrFormatted("(no loaded mods)"))
                    .Color(ColorRgba.LightGray));
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

            picker.Open(anchor);
        }
    }
}
