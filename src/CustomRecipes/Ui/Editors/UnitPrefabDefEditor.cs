using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using CustomAssets.Editor;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_unit_prefab</c> editor — path + four texture channels (albedo /
    /// normals / metallic / material reference) + width/height/depth + mesh.
    /// Dimensions stay nullable so "default in the API" round-trips as an
    /// omitted arg rather than a redundant literal. Includes a "Generate
    /// primitive mesh…" button that opens <see cref="PrimitiveShapeDialog"/>
    /// — modders pick a stock shape (or a custom box) and the dialog writes
    /// both an .obj and a UV-template PNG into the pack's Assets/Meshes/
    /// folder, then sets MeshPath + Width/Height/Depth on the def.
    public sealed class UnitPrefabDefEditor : DefEditor<UnitPrefabDef> {

        private readonly AssetPathPicker m_albedo;
        private readonly AssetPathPicker m_normals;
        private readonly AssetPathPicker m_metallic;
        private readonly AssetPathPicker m_reference;
        private readonly AssetPathPicker m_mesh;
        private readonly LoadedPack m_pack;
        private readonly UiContext m_uiContext;

        public UnitPrefabDefEditor(LoadedPack pack, PackModel model, UiContext uiContext) {
            m_pack = pack;
            m_uiContext = uiContext;

            AddStringField("path",
                getter: d => d.Path,
                setter: (d, v) => d.Path = v);

            m_albedo = AddField("albedo (path / Tex / list)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.AlbedoExpression),
                    setPath: v => { if (value != null) value.AlbedoExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick albedo texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_albedo.RefreshDisplay());

            m_normals = AddField("normals (optional)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.NormalsExpression),
                    setPath: v => { if (value != null) value.NormalsExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick normals texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_normals.RefreshDisplay());

            m_metallic = AddField("metallic (optional)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.MetallicExpression),
                    setPath: v => { if (value != null) value.MetallicExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick metallic texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_metallic.RefreshDisplay());

            m_reference = AddField("reference (existing material to clone)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.ReferenceExpression),
                    setPath: v => { if (value != null) value.ReferenceExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Material,
                    title: new LocStrFormatted("Pick reference material"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Material)),
                onRefresh: () => m_reference.RefreshDisplay());

            AddNullableDoubleField("width (default 0.5)",
                getter: d => d.Width, setter: (d, v) => d.Width = v);
            AddNullableDoubleField("height (default 0.2)",
                getter: d => d.Height, setter: (d, v) => d.Height = v);
            AddNullableDoubleField("depth (default 0.5)",
                getter: d => d.Depth, setter: (d, v) => d.Depth = v);
            // Mesh picker — Mesh kind surfaces the seven stock primitives
            // shipped by the core CustomAssets mod as their own section
            // above pack-local .obj files. Selecting a stock shape writes
            // the canonical "Assets/Primitives/<name>.obj" path; the
            // registrator's resolver falls back to the core mod folder
            // when that path doesn't exist locally, so no per-pack copy
            // is needed. Pack-side meshes still take priority on name
            // collision, so a modder can shadow a stock primitive by
            // dropping a same-named .obj into their own pack.
            m_mesh = AddField("mesh (optional pack asset path)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.MeshPath,
                    setPath: v => { if (value != null) value.MeshPath = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: false,
                    kind: AssetsCatalog.AssetKind.Mesh,
                    title: new LocStrFormatted("Pick mesh (.obj)")),
                onRefresh: () => m_mesh.RefreshDisplay());

            // Winding override. Default ("ccw" / null) passes the .obj
            // through unchanged — correct for standard Blender / Maya / 3ds
            // Max exports. The toggle stores "cw" when on, null when off so
            // the emitter omits the arg in the default case (no redundant
            // `winding="ccw"` clutter).
            Toggle windingToggle = new Toggle(standalone: true);
            ((IComponentWithLabel)windingToggle).SetLabel(
                new LocStrFormatted("Reverse winding (.obj is CW)"));
            windingToggle.OnValueChanged(v => {
                if (value == null) return;
                value.Winding = v ? "cw" : null;
                value.Dirty = true;
            });
            AddField(
                "winding (flip if mesh renders inside-out)",
                windingToggle,
                onRefresh: () => windingToggle.Value(
                    value != null && string.Equals(value.Winding, "cw", System.StringComparison.OrdinalIgnoreCase)));

            // Primitive shape generator. Sits below the mesh path so it
            // reads as an alternative path — pick a stock shape (or a custom
            // box) instead of typing a path by hand. Custom-mesh import via
            // the path field continues to work; this is purely additive.
            AddField(
                "primitive shape (optional generator)",
                new ButtonText(
                    new LocStrFormatted("Generate primitive mesh…"),
                    openGenerator));
        }

        private void openGenerator() {
            if (value == null || m_pack == null || m_uiContext == null) return;
            PrimitiveShapeDialog.Open(m_pack, value, m_uiContext, onGenerated: () => Value(value));
        }
    }
}
