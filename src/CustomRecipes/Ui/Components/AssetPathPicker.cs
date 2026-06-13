using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using UnityEngine;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Picker for an icon/texture path argument. Two sources of options:
    ///
    ///   • Files in the current pack's <c>Assets/</c> folder (recursive scan,
    ///     only image-like extensions). Selected value is the relative
    ///     in-bundle path string, e.g. <c>"Assets/MyPack/foo.png"</c>.
    ///   • Typed constants on <c>Mafi.Base.Assets</c> whose value ends in an
    ///     image extension. Selected value is the dotted reference name
    ///     (<c>Assets.Base.Bridges.Icons.CableStayed4_svg</c>) — emitted by
    ///     <see cref="Editor.Io.PackEmitter"/> without quotes so Python
    ///     resolves it to the underlying const at pack-load time. This keeps
    ///     packs in step with whatever the game's own identifier currently
    ///     resolves to, instead of pinning a raw path string that could drift
    ///     between updates.
    ///
    /// Mirrors <see cref="ProtoPicker{T}"/> structurally so the editor reads
    /// as one cohesive set of pickers — DisplayRowWithButton trigger, floating
    /// popup with PanelWithHeader, search box, scroll list, thumbnail rows.
    /// </summary>
    public sealed class AssetPathPicker : DisplayRowWithButton {

        private static readonly Px ButtonIconSize = 32.px();
        private static readonly Px OptionIconSize = 32.px();

        private readonly LoadedPack m_pack;
        private readonly Func<string> m_getPath;
        private readonly Action<string> m_setPath;
        private readonly bool m_allowMafiAssets;
        private readonly AssetsCatalog.AssetKind m_kind;
        private readonly LocStrFormatted m_title;

        // Optional callback yielding Defs whose VariableName can be picked
        // as a value for this field. Caller decides the filter (file scope,
        // matching kind, exclude self). Null â†’ no variables section is
        // shown in the popup. Re-invoked on every popup build so a
        // value-swap on the editor surfaces an up-to-date list.
        private readonly Func<IEnumerable<DefBase>> m_variableCandidates;

        private readonly Column m_buttonContent;

        // Popup is built lazily on first click and then reused. Building the
        // 800+ option rows + Mafi Icon thumbnails is the expensive part — each
        // Icon triggers AssetsDb.GetSharedSprite on attach, and the sprites
        // get cached in AssetsDb's own m_spriteCache. After the first build,
        // subsequent opens hit the same VisualElements with everything already
        // attached and pre-loaded; the popup just toggles its visibility via
        // FloatingColumn.Open/Close.
        private FloatingColumn m_popup;
        private TextField m_popupSearch;

        public AssetPathPicker(
                LoadedPack pack,
                Func<string> getPath,
                Action<string> setPath,
                bool allowMafiAssets = true,
                AssetsCatalog.AssetKind kind = AssetsCatalog.AssetKind.Image,
                LocStrFormatted? title = null,
                Func<IEnumerable<DefBase>> variableCandidates = null)
                : base(Mafi.Unity.UiToolkit.Library.Button.General) {

            m_pack               = pack;
            m_getPath            = getPath;
            m_setPath            = setPath;
            m_allowMafiAssets    = allowMafiAssets;
            m_kind               = kind;
            m_title              = title ?? new LocStrFormatted("Pick asset");
            m_variableCandidates = variableCandidates;

            // Same display-row class hygiene as ProtoPicker — strip the
            // LCD/digital font so file paths render in the normal UI font.
            Row.ClassRemove(Cls.displayFont);

            m_buttonContent = new Column();
            Row.Add(m_buttonContent);

            Btn.OnClick(openPopup);
            RefreshDisplay();
        }

        /// Re-resolve the current value and rebuild the trigger's content.
        public void RefreshDisplay() {
            m_buttonContent.Clear();
            string current = m_getPath();
            if (string.IsNullOrEmpty(current)) {
                m_buttonContent.Add(new Label(new LocStrFormatted("(no asset)")));
                return;
            }

            Row content = new Row();
            UiComponent thumb = buildThumbForValue(current, ButtonIconSize);
            if (thumb != null) content.Add(thumb);
            Column labelStack = new Column {
                new Label(new LocStrFormatted(displayNameForValue(current))).Fill(),
                new Label(new LocStrFormatted(current)).TinyFontSize().Fill()
            };
            labelStack.Fill();
            content.Add(labelStack);
            content.Gap(2.pt()).AlignItemsCenter().FlexGrow(1f);
            m_buttonContent.Add(content);
        }

        // ---- Popup ----------------------------------------------------------

        // Reusable storage for the popup's "Pack variables" section. The
        // outer popup is cached; this column is cleared+repopulated on
        // every open so a value-swap on the editor (different def
        // selected) re-asks variableCandidates and shows the right list.
        private Column m_varsSection;
        private List<KeyValuePair<UiComponent, string>> m_varsRows;
        private List<KeyValuePair<UiComponent, string>> m_rowsHaystack;

        // Reverse index from "stored value" â†’ option row so we can
        // visually flag and scroll to whichever option matches the
        // current value when the popup opens. Cleared/rebuilt by the
        // same code paths that populate the popup; the variables
        // section uses it too via rebuildVariablesSection.
        private Dictionary<string, UiComponent> m_rowsByValue;
        private ScrollColumn m_listScroll;
        private UiComponent m_lastHighlightedRow;

        private void openPopup() {
            if (m_popup != null) {
                // Cached popup â€” clear any previous search filter so the full
                // list is visible again, then reopen at the trigger button.
                // All rows stay attached; AssetsDb's sprite cache makes the
                // Icon thumbnails render instantly the second time around.
                if (m_popupSearch != null) m_popupSearch.Text("");
                rebuildVariablesSection();
                m_popup.Open(Btn);
                highlightAndScrollToCurrent();
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
            m_listScroll = list;

            // Each entry: (UiComponent row, lowercased haystack). Section
            // headers get null haystacks so they never hide on filter — keeps
            // visual grouping intact even with strict needles.
            List<KeyValuePair<UiComponent, string>> rows =
                new List<KeyValuePair<UiComponent, string>>();
            m_rowsHaystack = rows;
            m_rowsByValue = new Dictionary<string, UiComponent>(StringComparer.Ordinal);

            // "(none)" first — many icon-like args are optional (e.g. a
            // recipe's research field clears unlock requirements when empty).
            // Selecting writes null so PackEmitter omits the arg / writes None.
            ButtonRow noneRow = buildOptionRow(
                thumb: null,
                title: "(none)",
                subtitle: "clear the current selection",
                onClick: () => selectValue(popup, null));
            list.Add(noneRow);
            rows.Add(new KeyValuePair<UiComponent, string>(noneRow, "none clear empty"));

            // Pack variables section first â€” modders working on a file
            // typically want to wire something to a variable they just
            // defined ABOVE the current statement, so the most relevant
            // candidates should be the top of the list. Populated lazily
            // by rebuildVariablesSection so a def-value swap re-asks the
            // candidate list. The section's own header is part of its
            // column so the whole block disappears (or stays empty) when
            // there are no variables to show.
            m_varsSection = new Column();
            m_varsSection.AlignItemsStretch();
            list.Add(m_varsSection);
            m_varsRows = new List<KeyValuePair<UiComponent, string>>();
            rebuildVariablesSection();

            // Stock primitive shapes section. Sourced from the core
            // CustomAssets mod's Assets/Primitives folder (populated at
            // startup by PrimitiveStockGenerator). Sits above the pack
            // files so modders see the seven ready-made shapes (wide_box,
            // tall_box, flat_plate, cylinder_h/v, container_heap,
            // custom_box) before scrolling into per-pack files. Limited
            // to the mesh kind — they're .obj files and don't fit the
            // image / material / prefab pickers.
            if (m_kind == AssetsCatalog.AssetKind.Mesh) {
                List<string> stockPaths = scanStockPrimitives();
                if (stockPaths.Count > 0) {
                    Label stockHeader = new Label(new LocStrFormatted(
                        "Primitive shapes — " + stockPaths.Count + " (CustomAssets stock)"));
                    stockHeader.FontBold().PaddingTopBottom(2.pt());
                    list.Add(stockHeader);
                    rows.Add(new KeyValuePair<UiComponent, string>(stockHeader, null));

                    foreach (string absPath in stockPaths) {
                        string captured = absPath;
                        string fileName = Path.GetFileNameWithoutExtension(absPath);
                        string storedValue = "Assets/Primitives/" + Path.GetFileName(absPath);
                        string stackTag = stockStackingTag(fileName);
                        ButtonRow row = buildOptionRow(
                            thumb: buildStockMeshThumb(captured, OptionIconSize),
                            title: fileName + (string.IsNullOrEmpty(stackTag) ? "" : "  " + stackTag),
                            subtitle: storedValue,
                            onClick: () => selectValue(popup, storedValue));
                        list.Add(row);
                        rows.Add(new KeyValuePair<UiComponent, string>(row,
                            (fileName + " " + storedValue + " primitive stock").ToLowerInvariant()));
                        m_rowsByValue[storedValue] = row;
                    }
                }
            }

            // Pack files next.
            List<string> packPaths = scanPackAssetPaths();
            if (packPaths.Count > 0) {
                Label header = new Label(new LocStrFormatted(
                    "This pack — " + packPaths.Count + " file(s)"));
                header.FontBold().PaddingTopBottom(2.pt());
                list.Add(header);
                rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                foreach (string path in packPaths) {
                    string captured = path;
                    ButtonRow row = buildOptionRow(
                        thumb: buildPackFileThumb(captured, OptionIconSize),
                        title: Path.GetFileName(captured),
                        subtitle: captured,
                        onClick: () => selectValue(popup, captured));
                    list.Add(row);
                    rows.Add(new KeyValuePair<UiComponent, string>(row,
                        (Path.GetFileName(captured) + " " + captured).ToLowerInvariant()));
                    m_rowsByValue[captured] = row;
                }
            }

            if (m_allowMafiAssets) {
                List<AssetsCatalog.MafiAsset> mafiAssets = AssetsCatalog.OfKind(m_kind).ToList();
                if (mafiAssets.Count > 0) {
                    Label header = new Label(new LocStrFormatted(
                        "Mafi built-in — " + mafiAssets.Count + " asset(s)"));
                    header.FontBold().PaddingTopBottom(2.pt());
                    list.Add(header);
                    rows.Add(new KeyValuePair<UiComponent, string>(header, null));

                    foreach (AssetsCatalog.MafiAsset a in mafiAssets) {
                        AssetsCatalog.MafiAsset captured = a;
                        ButtonRow row = buildOptionRow(
                            thumb: new Icon(captured.ValuePath).Size(OptionIconSize),
                            title: shortNameOf(captured.TypedRefName),
                            subtitle: captured.TypedRefName,
                            onClick: () => selectValue(popup, captured.TypedRefName));
                        list.Add(row);
                        rows.Add(new KeyValuePair<UiComponent, string>(row,
                            (captured.TypedRefName + " " + captured.ValuePath).ToLowerInvariant()));
                        m_rowsByValue[captured.TypedRefName] = row;
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
            highlightAndScrollToCurrent();
        }

        // Flag the option row whose stored value matches the picker's
        // current value with Cls.selected, and scroll the popup list so
        // that row is visible. Both operations are deferred via the
        // panel's scheduler so they run AFTER UI Toolkit has laid out
        // the popup (worldBound is otherwise NaN and ScrollTo no-ops).
        // Re-runs on every open so a value change between two clicks
        // moves the highlight + scroll target accordingly.
        private void highlightAndScrollToCurrent() {
            if (m_listScroll == null || m_rowsByValue == null) return;
            // Drop the previous highlight regardless of whether the
            // current value still has a row â€” otherwise a stale row
            // keeps the highlight after the user clears the picker.
            if (m_lastHighlightedRow != null) {
                m_lastHighlightedRow.ClassRootIff(Cls.selected, false);
                m_lastHighlightedRow = null;
            }
            string current = m_getPath();
            if (string.IsNullOrEmpty(current)) return;
            if (!m_rowsByValue.TryGetValue(current, out UiComponent row) || row == null) return;
            row.ClassRootIff(Cls.selected, true);
            m_lastHighlightedRow = row;
            // Defer the actual scroll â€” the row's RootElement.worldBound
            // is only valid after the panel has laid out, which happens
            // after Open() returns on the next frame.
            var rootEl = row.RootElement;
            if (rootEl == null) return;
            var sv = m_listScroll.RootElement as UnityEngine.UIElements.ScrollView;
            if (sv == null) return;
            sv.schedule.Execute(() => {
                try { sv.ScrollTo(rootEl); } catch { /* element may have detached */ }
            });
        }

        private ButtonRow buildOptionRow(UiComponent thumb, string title, string subtitle, Action onClick) {
            ButtonRow row = new ButtonRow(
                Mafi.Unity.UiToolkit.Library.Button.General, onClick);
            row.Class(Cls.group);
            row.Gap(3.pt()).AlignItemsCenter().PaddingLeftRight(2.pt());
            if (thumb != null) row.Add(thumb);
            Column labelStack = new Column {
                new Label(new LocStrFormatted(title ?? "")),
                new Label(new LocStrFormatted(subtitle ?? "")).TinyFontSize()
            };
            labelStack.Fill();
            row.Add(labelStack);
            return row;
        }

        private void selectValue(FloatingColumn popup, string value) {
            popup.Close();
            m_setPath(value);
            RefreshDisplay();
        }

        // ---- Sources --------------------------------------------------------

        // Scan the pack's `Assets/` folder for files matching the configured
        // <see cref="AssetsCatalog.AssetKind"/>. The relative path
        // (forward-slashed, starting with `Assets/`) is both the on-disk file
        // location and the in-bundle key that mod scripts pass to API calls,
        // so the modder doesn't have to mentally translate between the two.
        private List<string> scanPackAssetPaths() {
            List<string> result = new List<string>();
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return result;
            string assetsRoot = Path.Combine(m_pack.RootPath, "Assets");
            if (!Directory.Exists(assetsRoot)) return result;
            try {
                foreach (string file in Directory.EnumerateFiles(
                        assetsRoot, "*.*", SearchOption.AllDirectories)) {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (!extensionMatchesKind(ext)) continue;
                    string rel = file.Substring(m_pack.RootPath.Length)
                        .TrimStart(Path.DirectorySeparatorChar, '/')
                        .Replace('\\', '/');
                    result.Add(rel);
                }
            } catch (Exception ex) {
                Log.Warning("AssetPathPicker: failed to scan " + assetsRoot + " — " + ex.Message);
            }
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        // Enumerate stock .obj primitives shipped under the core
        // CustomAssets mod. Returns absolute paths (the option row's
        // thumb lookup needs the on-disk path; the stored value the
        // option writes is the same relative "Assets/Primitives/..."
        // string CustomAssetRegistrator's fallback resolver recognises).
        // Returns an empty list when the directory doesn't exist yet —
        // happens on the first frame before RegisterData has run.
        private List<string> scanStockPrimitives() {
            List<string> result = new List<string>();
            string dir = CustomAssets.Data.Mod.PrimitiveStockGenerator.TargetDir;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return result;
            try {
                foreach (string file in Directory.EnumerateFiles(dir, "*.obj", SearchOption.TopDirectoryOnly)) {
                    result.Add(file);
                }
            } catch (Exception ex) {
                Log.Warning("AssetPathPicker: failed to scan stock primitives at "
                    + dir + " — " + ex.Message);
            }
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        // Render a 3D wireframe thumbnail for a stock primitive. Uses
        // the same MeshThumbnailCache the pack-file thumbs do, keyed by
        // absolute path so both code paths share one cached texture
        // per .obj on disk.
        private UiComponent buildStockMeshThumb(string absPath, Px size) {
            Texture2D tex = CustomAssets.Ui.MeshThumbnailCache.LoadObj(absPath);
            if (tex == null) return new Icon(PackThumbnailCache.FallbackIconPath).Size(size);
            return new Img(tex).Width(size).Height(size);
        }

        // Pull the StacksWell flag for a stock primitive from
        // PrimitiveCatalog. Matches by DefaultBaseName (== filename
        // without extension). Returns "[stacks]" / "[irregular]" or
        // empty when the file isn't catalogued (custom-generated meshes
        // dropped into the folder by hand).
        private static string stockStackingTag(string fileNameNoExt) {
            foreach (var preset in CustomAssets.Ui.Primitives.PrimitiveCatalog.All) {
                if (string.Equals(preset.DefaultBaseName, fileNameNoExt, StringComparison.OrdinalIgnoreCase)) {
                    return preset.StacksWell ? "[stacks]" : "[irregular]";
                }
            }
            return "";
        }

        private bool extensionMatchesKind(string ext) {
            switch (m_kind) {
                case AssetsCatalog.AssetKind.Image:
                    return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".svg";
                case AssetsCatalog.AssetKind.Material:
                    return ext == ".mat";
                case AssetsCatalog.AssetKind.Prefab:
                    // Pack-side prefabs ship as Unity .prefab files; .obj
                    // meshes feed into a unit prefab but aren't selectable
                    // AS the prefab itself (only as the mesh input to
                    // add_unit_prefab). Keep the filter strict so the
                    // dropdown doesn't list mesh files alongside real
                    // prefabs.
                    return ext == ".prefab";
                case AssetsCatalog.AssetKind.Mesh:
                    return ext == ".obj";
                default:
                    return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".svg"
                        || ext == ".mat" || ext == ".prefab" || ext == ".obj";
            }
        }

        // ---- Thumbnails -----------------------------------------------------

        // Thumbnail for the trigger / option preview. PNG/JPG paths inside the
        // pack are loaded via the on-disk cache (works even before COI has
        // registered the asset bundle); dotted typed-refs use Icon so the
        // game's own asset pipeline renders them.
        private UiComponent buildThumbForValue(string value, Px size) {
            if (string.IsNullOrEmpty(value)) return null;
            if (isDottedRef(value)) {
                string assetPath = resolveMafiAssetPath(value);
                if (!string.IsNullOrEmpty(assetPath)) {
                    return new Icon(assetPath).Size(size);
                }
                return null;
            }
            return buildPackFileThumb(value, size);
        }

        private UiComponent buildPackFileThumb(string relPath, Px size) {
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) return null;
            // Defensive guard: PackLoader stores "<unparseable>" when an
            // expression (e.g. inline `add_texture(...)` as the icon arg)
            // can't be reduced to a literal path. Path.GetExtension below
            // throws ArgumentException on '<' / '>', so bail out with the
            // fallback thumbnail instead of taking down the editor.
            if (containsInvalidPathChars(relPath)) {
                return new Icon(PackThumbnailCache.FallbackIconPath).Size(size);
            }
            string abs = Path.Combine(m_pack.RootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
            // SVGs aren't decodable via Texture2D.LoadImage. For pack-side
            // SVGs we'd need NGSvgImporter or similar; rather than ship the
            // dependency, fall back to a placeholder.
            string ext = Path.GetExtension(abs).ToLowerInvariant();
            if (ext == ".svg") {
                return new Icon(PackThumbnailCache.FallbackIconPath).Size(size);
            }
            // Wavefront OBJ — parse + render an axonometric flat-shaded
            // preview through MeshThumbnailCache so mesh assets get a real
            // thumbnail instead of the fallback icon. Cache is shared
            // across pickers, so reopening the popup is instant after the
            // first render.
            if (ext == ".obj") {
                Texture2D meshTex = MeshThumbnailCache.LoadObj(abs);
                if (meshTex == null) return new Icon(PackThumbnailCache.FallbackIconPath).Size(size);
                return new Img(meshTex).Width(size).Height(size);
            }
            Texture2D tex = AssetThumbnailCache.LoadPng(abs);
            if (tex == null) return new Icon(PackThumbnailCache.FallbackIconPath).Size(size);
            return new Img(tex).Width(size).Height(size);
        }

        // ---- Helpers --------------------------------------------------------

        // Rebuild the "Pack variables" section in place. Drops any rows
        // populated on a previous open (including their entries in the
        // shared haystack list so the search filter stays consistent),
        // re-asks variableCandidates, and adds a row per candidate.
        // Click on a variable row stores the variable's name as the value
        // â€” PackEmitter writes that as a bare identifier so Python
        // resolves through the binding at pack-load time.
        private void rebuildVariablesSection() {
            if (m_varsSection == null) return;
            // Remove the variable rows from the haystack so the filter
            // doesn't try to toggle visibility on detached elements.
            if (m_varsRows != null && m_rowsHaystack != null) {
                foreach (var kvp in m_varsRows) m_rowsHaystack.Remove(kvp);
                m_varsRows.Clear();
            }
            // Drop the variable-row entries from the value index so a
            // stale variable name (renamed, deleted, etc.) doesn't keep
            // pointing at a detached row.
            if (m_rowsByValue != null) {
                List<string> toRemove = new List<string>();
                foreach (var kvp in m_rowsByValue) {
                    if (kvp.Value != null && kvp.Value.RootElement != null
                            && kvp.Value.RootElement.parent == m_varsSection.RootElement) {
                        toRemove.Add(kvp.Key);
                    }
                }
                foreach (string k in toRemove) m_rowsByValue.Remove(k);
            }
            m_varsSection.Clear();

            if (m_variableCandidates == null) return;
            List<DefBase> candidates;
            try { candidates = m_variableCandidates().ToList(); }
            catch { return; }
            if (candidates == null || candidates.Count == 0) return;

            Label header = new Label(new LocStrFormatted(
                "Pack variables â€” " + candidates.Count + " in this file"));
            header.FontBold().PaddingTopBottom(2.pt());
            m_varsSection.Add(header);
            if (m_rowsHaystack != null) {
                m_rowsHaystack.Add(new KeyValuePair<UiComponent, string>(header, null));
                m_varsRows.Add(new KeyValuePair<UiComponent, string>(header, null));
            }

            foreach (DefBase def in candidates) {
                if (def == null || string.IsNullOrEmpty(def.VariableName)) continue;
                string varName = def.VariableName;
                string kind    = def.Kind ?? "";
                string subtitle = !string.IsNullOrEmpty(def.DisplayId)
                    ? "[" + kind + "] " + def.DisplayId
                    : "[" + kind + "]";
                ButtonRow row = buildOptionRow(
                    thumb: variableThumb(def),
                    title: varName + "  (variable)",
                    subtitle: subtitle,
                    onClick: () => selectValue(m_popup, varName));
                m_varsSection.Add(row);
                if (m_rowsHaystack != null) {
                    var entry = new KeyValuePair<UiComponent, string>(row,
                        (varName + " " + subtitle).ToLowerInvariant());
                    m_rowsHaystack.Add(entry);
                    m_varsRows.Add(entry);
                }
                if (m_rowsByValue != null) m_rowsByValue[varName] = row;
            }
        }

        // Best-effort thumbnail for a variable-bound def: textures know
        // their on-disk path so we render it through buildPackFileThumb;
        // anything else falls back to the picker's default missing-image
        // icon so the row still aligns visually with the other sections.
        private UiComponent variableThumb(DefBase def) {
            if (def is TextureDef tx && !string.IsNullOrEmpty(tx.Path)) {
                return buildPackFileThumb(tx.Path, OptionIconSize);
            }
            return new Icon(PackThumbnailCache.FallbackIconPath).Size(OptionIconSize);
        }

        private static bool isDottedRef(string s) {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) {
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_') return false;
            }
            return s.IndexOf('.') >= 0;
        }

        // True if the string contains any character that Windows rejects in
        // a path (< > " | ? * plus control chars). Used to short-circuit
        // Path.GetExtension before it throws ArgumentException on the
        // PackLoader's "<unparseable>" sentinel.
        private static bool containsInvalidPathChars(string s) {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in Path.GetInvalidPathChars()) {
                if (s.IndexOf(c) >= 0) return true;
            }
            // Path.GetInvalidPathChars on .NET Framework doesn't include
            // '<', '>', '"', '|', '?', '*' â€” those are file-name invalid
            // but pass the path check on some runtimes. Check them too so
            // the "<unparseable>" sentinel triggers the guard reliably.
            return s.IndexOf('<') >= 0
                || s.IndexOf('>') >= 0
                || s.IndexOf('"') >= 0
                || s.IndexOf('|') >= 0
                || s.IndexOf('?') >= 0
                || s.IndexOf('*') >= 0;
        }

        // Look up the underlying asset path for a dotted typed-ref so the
        // trigger thumbnail can be loaded via the game's AssetsDb.
        private static string resolveMafiAssetPath(string typedRef) {
            foreach (AssetsCatalog.MafiAsset a in AssetsCatalog.All) {
                if (a.TypedRefName == typedRef) return a.ValuePath;
            }
            return null;
        }

        // Tail segment of a dotted typed-ref ("...CableStayed4_svg" →
        // "CableStayed4_svg"). Picker rows show this as the headline name
        // since the full dotted path is also rendered as the subtitle.
        private static string shortNameOf(string dotted) {
            if (string.IsNullOrEmpty(dotted)) return dotted;
            int i = dotted.LastIndexOf('.');
            return i < 0 ? dotted : dotted.Substring(i + 1);
        }

        // Display name for the trigger button. For pack files we show the
        // filename; for typed-refs we show the constant name.
        private static string displayNameForValue(string value) {
            if (isDottedRef(value)) return shortNameOf(value);
            int slash = value.LastIndexOf('/');
            return slash < 0 ? value : value.Substring(slash + 1);
        }
    }
}
