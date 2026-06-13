using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomAssets.Editor {

    /// Flat catalog of Mafi's built-in asset paths exposed as typed `const`
    /// strings on the static `Mafi.Base.Assets` class. The class is a deeply
    /// nested tree of `public static class Foo { public static class Bar {
    /// public const string Icon_svg = "Assets/...svg"; } }`; for the editor's
    /// picker we want a flat list of (typedRefName, valuePath) pairs.
    ///
    /// `typedRefName` is the dotted access path that resolves the constant in
    /// Python — e.g. `Assets.Base.Bridges.Icons.CableStayed4_svg`. Emitting
    /// this form (rather than the raw quoted path) lets the modder's source
    /// stay in step with COI's own constant identifiers, so renames/moves in
    /// game updates can be cross-referenced.
    public static class AssetsCatalog {

        public enum AssetKind {
            Any,
            Image,     // .svg / .png / .jpg / .jpeg
            Material,  // .mat
            Prefab,    // .prefab — Unity GameObject prefab asset
            Mesh,      // .obj — Wavefront mesh (only kind COI's ObjLoader reads)
        }

        public sealed class MafiAsset {
            public readonly string TypedRefName;
            public readonly string ValuePath;
            public readonly AssetKind Kind;

            public bool IsImage => Kind == AssetKind.Image;
            public bool IsMaterial => Kind == AssetKind.Material;
            public bool IsPrefab => Kind == AssetKind.Prefab;

            public MafiAsset(string typedRefName, string valuePath) {
                TypedRefName = typedRefName;
                ValuePath = valuePath;
                Kind = classifyByExtension(valuePath);
            }
        }

        private static IReadOnlyList<MafiAsset> s_all;

        public static IReadOnlyList<MafiAsset> All {
            get {
                if (s_all == null) s_all = build();
                return s_all;
            }
        }

        public static IEnumerable<MafiAsset> ImagesOnly() => OfKind(AssetKind.Image);
        public static IEnumerable<MafiAsset> MaterialsOnly() => OfKind(AssetKind.Material);
        public static IEnumerable<MafiAsset> PrefabsOnly() => OfKind(AssetKind.Prefab);

        public static IEnumerable<MafiAsset> OfKind(AssetKind kind) {
            foreach (MafiAsset a in All) {
                if (kind == AssetKind.Any || a.Kind == kind) yield return a;
            }
        }

        private static IReadOnlyList<MafiAsset> build() {
            List<MafiAsset> list = new List<MafiAsset>();
            Type root = findAssetsType();
            if (root == null) return list;
            walk(root, "Assets", list);
            return list;
        }

        // Hunt for Mafi.Base.Assets in any loaded assembly. Avoids a hard
        // reference if the Mafi.Base assembly is renamed or hot-loaded.
        private static Type findAssetsType() {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Type t;
                try {
                    t = asm.GetType("Mafi.Base.Assets", throwOnError: false);
                } catch { t = null; }
                if (t != null) return t;
            }
            return null;
        }

        // Recurse the nested static classes, collecting every `const string`
        // field. The TypedRefName is the dotted path built up from class
        // names; ValuePath is the field's literal value.
        private static void walk(Type type, string prefix, List<MafiAsset> outList) {
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo f in fields) {
                if (f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string)) {
                    string value = (string)f.GetRawConstantValue();
                    outList.Add(new MafiAsset(prefix + "." + f.Name, value));
                }
            }
            Type[] nested = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
            foreach (Type n in nested) {
                walk(n, prefix + "." + n.Name, outList);
            }
        }

        private static AssetKind classifyByExtension(string path) {
            if (string.IsNullOrEmpty(path)) return AssetKind.Any;
            if (path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                return AssetKind.Image;
            }
            if (path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase)) {
                return AssetKind.Material;
            }
            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) {
                return AssetKind.Prefab;
            }
            if (path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)) {
                return AssetKind.Mesh;
            }
            return AssetKind.Any;
        }
    }
}
