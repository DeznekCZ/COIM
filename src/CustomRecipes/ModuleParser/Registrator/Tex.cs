namespace CustomAssets.ModuleParser.Registrator
{
    /// Texture-reference placeholder returned by <c>add_texture(...)</c> /
    /// <c>base_texture(...)</c>. Consumers (add_prefab_box, add_unit_prefab,
    /// add_texture_material) pattern-match on this type to discover where
    /// the underlying <c>Texture2D</c> should come from.
    internal class Tex
    {
        public string path { get; set; }

        /// When true, the path resolves against the game's <c>AssetsDb</c>
        /// at injection time â€” <c>add_texture(...)</c> sets this when the
        /// path doesn't exist as a file under the mod folder, on the
        /// assumption that the modder is referencing a base-game asset.
        /// When false, the path is a mod-local asset already loaded from
        /// disk into <c>CustomAssetManager.Alternations</c>.
        public bool loadedAsset { get; set; }
    }
}