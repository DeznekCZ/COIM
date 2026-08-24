using System;
using System.Collections.Generic;
using System.IO;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using UnityEngine;

namespace CustomAssets.Ui.Components;

/// <summary>
/// Reusable "pick a thumbnail image" control: a path field, a browse button
/// that lists every image already sitting inside a loaded pack, a live
/// preview, and a hint line that calls out SVG → PNG conversion before the
/// modder commits.
///
/// The control only *collects* a source path — it never touches disk. The
/// owning dialog decides when to commit, because the two callers differ:
/// <see cref="NewPackDialog"/> has no pack folder until the scaffolder has
/// run, while <see cref="PackDepsPanel"/> writes on Save. Call
/// <see cref="ApplyTo"/> at that point.
///
/// COI runs fullscreen without an OS file dialog available to a mod, so
/// "browse" means scanning the mods folder rather than opening a native
/// picker. Anything outside a pack can still be used by pasting an absolute
/// path into the field.
/// </summary>
public sealed class ThumbnailPicker {

    // How deep a pack is scanned for candidate images, and how many rows the
    // browse popup will show. Packs with a big Assets/ tree (icons per
    // product, per machine) can hold hundreds of PNGs; the cap keeps the
    // popup from turning into an unusable wall of rows.
    private const int MaxBrowseResults = 400;

    private readonly TextField m_pathField;
    private readonly Column m_previewHolder;
    private readonly Label m_hint;
    private string m_path;

    /// The control's root component — add this to the owning dialog's body.
    public Column Root { get; }

    /// Currently selected source path, or empty when the modder hasn't
    /// picked anything. Trimmed; never null.
    public string SourcePath {
        get { return (m_path ?? "").Trim(); }
    }

    public bool HasSelection {
        get { return SourcePath.Length > 0; }
    }

    /// <param name="initialPath">Pre-filled source path, e.g. a pack's
    /// existing Thumbnail.png when editing rather than creating.</param>
    public ThumbnailPicker(string initialPath = null) {
        m_path = initialPath ?? "";

        m_pathField = new TextField().Text(m_path);
        // Mafi's TextField keeps its placeholder visible even when a value
        // is present, so a pre-filled field must not get one — the two
        // texts overlap and read as corruption.
        if (m_path.Length == 0) {
            m_pathField.Placeholder(new LocStrFormatted("path to a .png / .jpg / .svg image"));
        }
        m_pathField.OnValueChanged(v => {
            m_path = v ?? "";
            refresh();
        });
        m_pathField.FlexGrow(1f);

        ButtonText browseBtn = null;
        browseBtn = new ButtonText(new LocStrFormatted("browse…"), () => {
            openBrowser(browseBtn);
        });
        browseBtn.Tooltip(new LocStrFormatted(
            "Pick an image from a loaded pack. For an image elsewhere, paste its full path into the field."));

        ButtonText clearBtn = new ButtonText(new LocStrFormatted("✕"), () => {
            m_path = "";
            m_pathField.Text("");
            refresh();
        });
        clearBtn.Tooltip(new LocStrFormatted("Clear the selection"));

        Row pathRow = new Row { m_pathField, browseBtn, clearBtn };
        pathRow.Gap(2.pt()).AlignItemsCenter();

        m_previewHolder = new Column();
        m_previewHolder.Gap(1.pt());
        m_hint = new Label(new LocStrFormatted("")).TinyFontSize();

        Row previewRow = new Row { m_previewHolder, m_hint };
        previewRow.Gap(3.pt()).AlignItemsCenter();

        Root = new Column { pathRow, previewRow };
        Root.AlignItemsStretch().Gap(2.pt());

        refresh();
    }

    /// <summary>Commit the current selection into
    /// <paramref name="packRoot"/>. Returns null when nothing is selected —
    /// a thumbnail is optional, so "no selection" is a success, not an
    /// error, and callers should treat null as "nothing to do".</summary>
    public ThumbnailWriter.Result ApplyTo(string packRoot) {
        if (!HasSelection) {
            return null;
        }
        return ThumbnailWriter.Apply(packRoot, SourcePath);
    }

    // ---- Preview + hint -----------------------------------------------------

    private void refresh() {
        m_previewHolder.Clear();
        string path = SourcePath;

        if (path.Length == 0) {
            m_hint.Color(ColorRgba.LightGray);
            m_hint.Value(new LocStrFormatted(
                "Optional. Without one, the pack shows COI's generic mod icon."));
            return;
        }
        if (!ThumbnailWriter.IsSupportedSource(path)) {
            m_hint.Color(ColorRgba.Red);
            m_hint.Value(new LocStrFormatted("Unsupported file type — use .png, .jpg, or .svg."));
            return;
        }
        if (!File.Exists(path)) {
            m_hint.Color(ColorRgba.Orange);
            m_hint.Value(new LocStrFormatted("No file at that path yet."));
            return;
        }

        if (ThumbnailWriter.IsSvg(path)) {
            // No preview for vectors: rendering one would mean rasterizing
            // on every keystroke. The conversion notice is the point here
            // anyway — it tells the modder what committing will do.
            m_hint.Color(ColorRgba.LightGray);
            m_hint.Value(new LocStrFormatted(
                "SVG — will be converted to a " + ThumbnailWriter.ThumbnailSize + "×"
                + ThumbnailWriter.ThumbnailSize + " PNG, and the .svg kept in the pack for rebuilds."));
            return;
        }

        Texture2D tex = AssetThumbnailCache.LoadPng(path);
        if (tex == null) {
            m_hint.Color(ColorRgba.Orange);
            m_hint.Value(new LocStrFormatted("Could not read that image."));
            return;
        }
        m_previewHolder.Add(new Img(tex).Width(64.px()).Height(64.px()));
        m_hint.Color(ColorRgba.LightGray);
        m_hint.Value(new LocStrFormatted(
            "Copied into the pack as " + ThumbnailWriter.ThumbnailFileName + "."));
    }

    // ---- Browse popup -------------------------------------------------------

    // Searchable list of every supported image found inside a loaded pack.
    // Same PanelWithHeader + search + ScrollColumn shape as LoadedModPicker
    // so the two pickers feel identical.
    private void openBrowser(UiComponent anchor) {
        FloatingColumn picker = new FloatingColumn(
            FloaterPositionPolicy.ABOVE,
            keepOpenOnHover: false,
            openAfterDelay: false,
            closeOnClickOutside: true);
        LocStrFormatted title = new LocStrFormatted("Pick a thumbnail image");
        PanelWithHeader panel = picker.AddAndReturn(new PanelWithHeader(title))
            .AlignItemsStretch()
            .Gap(2.pt())
            .MinWidth(460.px())
            .MaxHeight(560.px());
        panel.Header.Clear();
        panel.Header.Add(new Label(title).FontBold());

        TextField search = new TextField()
            .Placeholder(new LocStrFormatted("search by file or pack name…"));
        panel.Header.Add(search);

        ScrollColumn list = new ScrollColumn();
        list.Gap(1.pt()).MaxHeight(480.px()).AlignItemsStretch();
        panel.BodyAdd(list);

        List<KeyValuePair<UiComponent, string>> rows =
            new List<KeyValuePair<UiComponent, string>>();

        List<Candidate> candidates = collectCandidates();
        foreach (Candidate candidate in candidates) {
            ButtonRow rowBtn = new ButtonRow(Button.General, () => {
                picker.Close();
                m_path = candidate.AbsolutePath;
                m_pathField.Text(candidate.AbsolutePath);
                refresh();
            });
            rowBtn.Gap(2.pt()).AlignItemsCenter();

            // SVG rows get no raster preview (same reason as the main
            // preview) — a text marker keeps the row height consistent.
            Texture2D tex = ThumbnailWriter.IsSvg(candidate.AbsolutePath)
                ? null
                : AssetThumbnailCache.LoadPng(candidate.AbsolutePath);
            if (tex != null) {
                rowBtn.Add(new Img(tex).Width(36.px()).Height(36.px()));
            } else {
                rowBtn.Add(new Label(new LocStrFormatted("SVG"))
                    .TinyFontSize().Color(ColorRgba.LightGray).Width(36.px()));
            }

            Column labels = new Column {
                new Label(new LocStrFormatted(candidate.FileName)).Fill(),
                new Label(new LocStrFormatted(candidate.PackId + "   " + candidate.RelativePath))
                    .TinyFontSize().Fill(),
            };
            labels.FlexGrow(1f);
            rowBtn.Add(labels);
            list.Add(rowBtn);
            rows.Add(new KeyValuePair<UiComponent, string>(rowBtn,
                (candidate.FileName + " " + candidate.PackId + " "
                    + candidate.RelativePath).ToLowerInvariant()));
        }

        if (candidates.Count == 0) {
            list.Add(new Label(new LocStrFormatted(
                "(no .png / .jpg / .svg images found in any loaded pack)"))
                .Color(ColorRgba.LightGray));
        }

        search.OnValueChanged(v => {
            string needle = (v ?? "").Trim().ToLowerInvariant();
            if (needle.Length == 0) {
                foreach (KeyValuePair<UiComponent, string> kvp in rows) {
                    kvp.Key.Visible(true);
                }
                return;
            }
            foreach (KeyValuePair<UiComponent, string> kvp in rows) {
                kvp.Key.Visible(kvp.Value != null && kvp.Value.Contains(needle));
            }
        });
        search.FocusOnShow();

        picker.Open(anchor);
    }

    private static List<Candidate> collectCandidates() {
        List<Candidate> result = new List<Candidate>();
        foreach (LoadedPack pack in PackRegistry.Packs) {
            if (pack == null || string.IsNullOrEmpty(pack.RootPath)) {
                continue;
            }
            if (!Directory.Exists(pack.RootPath)) {
                continue;
            }
            try {
                foreach (string file in Directory.EnumerateFiles(
                        pack.RootPath, "*.*", SearchOption.AllDirectories)) {
                    if (!ThumbnailWriter.IsSupportedSource(file)) {
                        continue;
                    }
                    result.Add(new Candidate {
                        AbsolutePath = file,
                        FileName = Path.GetFileName(file),
                        PackId = pack.ModId ?? "?",
                        RelativePath = makeRelative(pack.RootPath, file),
                    });
                    if (result.Count >= MaxBrowseResults) {
                        Log.Warning("ThumbnailPicker: browse list capped at "
                            + MaxBrowseResults + " images.");
                        return result;
                    }
                }
            } catch (Exception ex) {
                // A single unreadable pack folder shouldn't empty the whole
                // list — skip it and keep scanning the rest.
                Log.Warning("ThumbnailPicker: could not scan '" + pack.RootPath
                    + "' - " + ex.Message);
            }
        }
        return result;
    }

    private static string makeRelative(string root, string file) {
        string trimmedRoot = root.TrimEnd(Path.DirectorySeparatorChar, '/');
        if (file.StartsWith(trimmedRoot, StringComparison.OrdinalIgnoreCase)
                && file.Length > trimmedRoot.Length + 1) {
            return file.Substring(trimmedRoot.Length + 1);
        }
        return file;
    }

    private sealed class Candidate {
        public string AbsolutePath;
        public string FileName;
        public string PackId;
        public string RelativePath;
    }
}
