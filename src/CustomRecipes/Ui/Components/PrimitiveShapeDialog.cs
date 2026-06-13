using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Primitives;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;
using MafiTextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace CustomAssets.Ui.Components {

    /// Picker + generator dialog for the unit-prefab primitive shape library.
    /// Modder picks a preset (or "Custom"), tweaks dimensions if applicable,
    /// previews the UV wireframe template, and on "Generate" the dialog
    /// writes <c>Assets/Meshes/&lt;name&gt;.obj</c> + <c>&lt;name&gt;_uv.png</c>
    /// into the loaded pack and pushes width/height/depth + mesh path back
    /// into the bound <see cref="UnitPrefabDef"/>.
    ///
    /// Output location follows the rule "load from shared (the in-code preset
    /// library), save only to pack" — the user picks from a shared catalog
    /// and the result lands in their pack folder, never in CustomAssetPack.
    public sealed class PrimitiveShapeDialog : Window {

        private const int PreviewPixelSize = 256;
        private const int PngPixelSize = 1024;

        private readonly LoadedPack m_pack;
        private readonly UnitPrefabDef m_def;
        private readonly Action m_onGenerated;

        private readonly Column m_presetList;
        private readonly TextField m_widthField;
        private readonly TextField m_heightField;
        private readonly TextField m_depthField;
        private readonly TextField m_baseNameField;
        private readonly Label m_stackingLabel;
        private readonly Label m_descLabel;
        private readonly Label m_statusLabel;
        private readonly Column m_previewHolder;
        private readonly ButtonText m_generateBtn;

        private PrimitivePreset m_selected;
        private Texture2D m_previewTex;
        private Texture2D m_meshPreviewTex;

        public static void Open(LoadedPack pack, UnitPrefabDef def, UiContext ctx, Action onGenerated) {
            new PrimitiveShapeDialog(pack, def, onGenerated).Open(ctx.UiRoot);
        }

        public PrimitiveShapeDialog(LoadedPack pack, UnitPrefabDef def, Action onGenerated)
            : base(new LocStrFormatted("Generate primitive mesh")) {
            m_pack = pack;
            m_def = def;
            m_onGenerated = onGenerated;

            MakeMovable();
            WindowSize(820.px(), 640.px());

            // Sidebar (preset list) + main column (params + preview).
            m_presetList = new Column();
            m_presetList.Gap(2.px()).AlignItemsStretch().Width(220.px());
            ScrollColumn presetScroll = new ScrollColumn();
            presetScroll.Add(m_presetList);
            presetScroll.AlignItemsStretch().FlexGrow(0f);

            Column main = new Column();
            main.Gap(4.px()).AlignItemsStretch().FlexGrow(1f);

            m_descLabel = new Label(new LocStrFormatted("")).TinyFontSize();
            m_stackingLabel = new Label(new LocStrFormatted("")).TinyFontSize();

            m_widthField  = numericField();
            m_heightField = numericField();
            m_depthField  = numericField();
            m_baseNameField = new TextField()
                .Placeholder(new LocStrFormatted("filename (without extension)"));

            m_widthField.OnValueChanged(_ => onParamsChanged());
            m_heightField.OnValueChanged(_ => onParamsChanged());
            m_depthField.OnValueChanged(_ => onParamsChanged());

            Row dimsRow = new Row {
                labeled("width (X)",  m_widthField),
                labeled("height (Y)", m_heightField),
                labeled("depth (Z)",  m_depthField),
            };
            dimsRow.Gap(6.px()).AlignItemsCenter();

            // Preview holder hosts a Row of [3D mesh thumbnail | UV layout]
            // so the modder sees both the resulting shape and the texture
            // template that maps onto it. Row width = 2 × preview + gap.
            m_previewHolder = new Column();
            m_previewHolder.AlignItemsStretch();

            m_statusLabel = new Label(new LocStrFormatted("Pick a preset to start.")).TinyFontSize();
            m_generateBtn = new ButtonText(new LocStrFormatted("Generate into pack"), onGenerate);

            Row footer = new Row {
                m_generateBtn,
                new ButtonText(new LocStrFormatted("Close"), Close),
            };
            footer.Gap(6.px());

            main.Add(m_descLabel);
            main.Add(m_stackingLabel);
            main.Add(dimsRow);
            main.Add(labeled("filename (.obj / _uv.png)", m_baseNameField));
            main.Add(new Label(new LocStrFormatted(
                "3D shape (left) + UV template (right):")).TinyFontSize());
            main.Add(m_previewHolder);
            main.Add(m_statusLabel);
            main.Add(footer);

            Row body = new Row { presetScroll, main };
            body.Gap(8.px()).AlignItemsStretch();
            Body.Add(body);
            Body.AlignItemsStretch().PaddingTop(60.px()).PaddingLeftRight(8.px())
                .PaddingBottom(8.px());

            buildPresetButtons();
            // Auto-select the first preset so the dialog isn't blank on open.
            selectPreset(PrimitiveCatalog.All.Count > 0 ? PrimitiveCatalog.All[0] : null);
        }

        // ---- Preset list ----------------------------------------------------

        private void buildPresetButtons() {
            m_presetList.Clear();
            foreach (PrimitivePreset preset in PrimitiveCatalog.All) {
                PrimitivePreset captured = preset;
                string label = preset.DisplayName
                    + (preset.StacksWell ? "  [stacks]" : "  [irregular]");
                ButtonText btn = new ButtonText(new LocStrFormatted(label),
                    () => selectPreset(captured));
                btn.TextAlign(MafiTextAlignment.LeftMiddle).FlexGrow(0f);
                m_presetList.Add(btn);
            }
        }

        private void selectPreset(PrimitivePreset preset) {
            m_selected = preset;
            if (preset == null) {
                m_descLabel.Value(new LocStrFormatted(""));
                m_stackingLabel.Value(new LocStrFormatted(""));
                return;
            }
            m_descLabel.Value(new LocStrFormatted(preset.Description));
            m_stackingLabel.Value(new LocStrFormatted(
                preset.StacksWell
                    ? "Stacking: this shape packs cleanly inside a COI tile."
                    : "Stacking: this shape's silhouette may look irregular when stacked."));

            // Fields prefill from the preset; the user can still tweak any
            // dimension on any preset (a "wide box but slightly shorter"
            // workflow). Custom is just the preset whose defaults invite
            // editing — there's no hard read-only on the other presets.
            m_widthField.Text(formatDim(preset.Width));
            m_heightField.Text(formatDim(preset.Height));
            m_depthField.Text(formatDim(preset.Depth));

            m_baseNameField.Text(preset.DefaultBaseName ?? preset.Id);

            updatePreview();
        }

        private void onParamsChanged() {
            // Any preset's dimensions can be tweaked — re-render the preview
            // so the modder sees the effect immediately.
            if (m_selected != null) updatePreview();
        }

        // ---- Preview --------------------------------------------------------

        private void updatePreview() {
            disposePreview();
            m_previewHolder.Clear();
            if (m_selected == null) return;

            (double w, double h, double d) = currentDims();
            if (!(w > 0) || !(h > 0) || !(d > 0)) {
                m_previewHolder.Add(new Label(new LocStrFormatted(
                    "Need positive width/height/depth.")).TinyFontSize());
                return;
            }

            ShapeResult shape;
            try {
                shape = m_selected.Build(w, h, d);
            } catch (Exception ex) {
                Log.Warning("PrimitiveShapeDialog: build failed — " + ex.Message);
                m_previewHolder.Add(new Label(new LocStrFormatted(
                    "Build failed: " + ex.Message)).TinyFontSize());
                return;
            }

            // Two side-by-side previews: an axonometric flat-shaded 3D view
            // of the generated mesh (left) and the UV template (right).
            // Either renderer can fail independently (e.g. degenerate
            // geometry on extreme dimensions) — render each into its own
            // try/catch so a bad mesh doesn't drop the UV view too.
            Row previewRow = new Row();
            previewRow.Gap(8.px()).AlignItemsCenter();

            m_meshPreviewTex = PrimitiveWireframe.RenderMesh3DTexture(shape.Mesh, PreviewPixelSize);
            if (m_meshPreviewTex != null) {
                previewRow.Add(
                    new Img(m_meshPreviewTex)
                        .Width(PreviewPixelSize.px())
                        .Height(PreviewPixelSize.px()));
            }

            m_previewTex = PrimitiveWireframe.RenderTexture(shape, PreviewPixelSize);
            if (m_previewTex != null) {
                previewRow.Add(
                    new Img(m_previewTex)
                        .Width(PreviewPixelSize.px())
                        .Height(PreviewPixelSize.px()));
            }

            if (m_meshPreviewTex == null && m_previewTex == null) {
                m_previewHolder.Add(new Label(new LocStrFormatted(
                    "Preview unavailable.")).TinyFontSize());
            } else {
                m_previewHolder.Add(previewRow);
            }
        }

        private void disposePreview() {
            if (m_previewTex != null) {
                UnityEngine.Object.Destroy(m_previewTex);
                m_previewTex = null;
            }
            if (m_meshPreviewTex != null) {
                UnityEngine.Object.Destroy(m_meshPreviewTex);
                m_meshPreviewTex = null;
            }
        }

        // ---- Generate -------------------------------------------------------

        private void onGenerate() {
            if (m_selected == null) {
                m_statusLabel.Value(new LocStrFormatted("Pick a preset first."));
                return;
            }
            if (m_pack == null || string.IsNullOrEmpty(m_pack.RootPath)) {
                m_statusLabel.Value(new LocStrFormatted("No pack is loaded — open one first."));
                return;
            }
            (double w, double h, double d) = currentDims();
            if (!(w > 0) || !(h > 0) || !(d > 0)) {
                m_statusLabel.Value(new LocStrFormatted("Width / height / depth must be positive."));
                return;
            }

            string baseName = (m_baseNameField.GetText() ?? "").Trim();
            if (string.IsNullOrEmpty(baseName)) {
                baseName = m_selected.DefaultBaseName ?? m_selected.Id;
            }
            baseName = sanitiseName(baseName);

            try {
                string meshDir = Path.Combine(m_pack.RootPath, "Assets", "Meshes");
                Directory.CreateDirectory(meshDir);
                string objAbs = uniquePath(Path.Combine(meshDir, baseName + ".obj"));
                string finalBase = Path.GetFileNameWithoutExtension(objAbs);
                string pngAbs = Path.Combine(meshDir, finalBase + "_uv.png");

                ShapeResult shape = m_selected.Build(w, h, d);
                shape.Mesh.Name = finalBase;
                File.WriteAllText(objAbs, shape.Mesh.ToObj());

                byte[] pngBytes = PrimitiveWireframe.RenderPng(shape, PngPixelSize);
                if (pngBytes != null) {
                    File.WriteAllBytes(pngAbs, pngBytes);
                }

                // Drop any cached mesh thumbnail for the path we just
                // wrote so the AssetPathPicker re-renders the updated
                // geometry next time it builds a thumb. uniquePath above
                // already steered us off existing files in most cases;
                // the invalidate covers the unusual overwrite path.
                CustomAssets.Ui.MeshThumbnailCache.Invalidate(objAbs);

                string packRel = "Assets/Meshes/" + finalBase + ".obj";
                if (m_def != null) {
                    m_def.MeshPath = packRel;
                    m_def.Width = w;
                    m_def.Height = h;
                    m_def.Depth = d;
                    m_def.Dirty = true;
                }
                m_onGenerated?.Invoke();
                m_statusLabel.Value(new LocStrFormatted(
                    "Wrote " + Path.GetFileName(objAbs)
                    + (pngBytes != null ? " + " + Path.GetFileName(pngAbs) : "")
                    + " under Assets/Meshes/."));
            } catch (Exception ex) {
                Log.Exception(ex);
                m_statusLabel.Value(new LocStrFormatted("Generate failed: " + ex.Message));
            }
        }

        // ---- Helpers --------------------------------------------------------

        private (double w, double h, double d) currentDims() {
            double w = parse(m_widthField.GetText(),  m_selected != null ? m_selected.Width  : 0);
            double h = parse(m_heightField.GetText(), m_selected != null ? m_selected.Height : 0);
            double d = parse(m_depthField.GetText(),  m_selected != null ? m_selected.Depth  : 0);
            return (w, h, d);
        }

        private static double parse(string s, double fallback) {
            if (string.IsNullOrWhiteSpace(s)) return fallback;
            if (double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v)) {
                return v;
            }
            return fallback;
        }

        private static string formatDim(double v) {
            return v.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static TextField numericField() {
            TextField f = new TextField();
            f.Width(80.px());
            return f;
        }

        private static Column labeled(string label, UiComponent inner) {
            Column c = new Column {
                new Label(new LocStrFormatted(label)).TinyFontSize(),
                inner,
            };
            c.Gap(1.px()).AlignItemsStretch();
            return c;
        }

        /// Avoid clobbering an existing file: append _1, _2, … until the
        /// chosen path is free. Returns the chosen absolute path.
        private static string uniquePath(string preferredAbs) {
            if (!File.Exists(preferredAbs)) return preferredAbs;
            string dir = Path.GetDirectoryName(preferredAbs);
            string baseName = Path.GetFileNameWithoutExtension(preferredAbs);
            string ext = Path.GetExtension(preferredAbs);
            for (int i = 1; i < 1000; i++) {
                string candidate = Path.Combine(dir, baseName + "_" + i + ext);
                if (!File.Exists(candidate)) return candidate;
            }
            return preferredAbs; // give up; overwrite
        }

        private static string sanitiseName(string s) {
            char[] invalid = Path.GetInvalidFileNameChars();
            char[] arr = s.ToCharArray();
            for (int i = 0; i < arr.Length; i++) {
                if (Array.IndexOf(invalid, arr[i]) >= 0 || arr[i] == ' ') arr[i] = '_';
            }
            return new string(arr);
        }
    }
}
