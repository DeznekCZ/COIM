using Mafi;
using Mafi.Collections;
using Mafi.Unity;
using System.IO;
using UnityEngine;

namespace ProgramableNetwork.Data.Mod
{
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    public class CustomAssetManager
    {
        public static Dict<string, UnityEngine.Object> Alternations { get; } = new Dict<string, UnityEngine.Object>();

        public static void Clear()
        {
            Alternations.Clear();
        }

        // Load a mod-folder PNG into Alternations under its asset path so it later
        // resolves through AssetsDb (the ctor below splices Alternations into
        // AssetsDb.LoadedAssets).  Idempotent — a second call for the same path is a
        // no-op.  Used for texture assets that aren't a proto icon (e.g. the in-display
        // toggle switch), where the ControllerTemplates icon-load path doesn't apply.
        public static void LoadTexture(string modBasePath, string assetPath)
        {
            if (assetPath.IsNullOrEmpty() || Alternations.ContainsKey(assetPath))
            {
                return;
            }
            string fullPath = Path.Combine(modBasePath, assetPath);
            if (!File.Exists(fullPath))
            {
                Log.Warning($"CustomAssetManager.LoadTexture: file not found '{fullPath}'");
                return;
            }
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            if (!texture.LoadImage(File.ReadAllBytes(fullPath)))
            {
                Log.Warning($"CustomAssetManager.LoadTexture: could not decode '{assetPath}'");
                return;
            }
            texture.name = assetPath;
            Alternations[assetPath] = texture;
        }

        public CustomAssetManager(AssetsDb assets)
        {
            Dict<string, UnityEngine.Object> loadedAssets = ((Dict<string, UnityEngine.Object>)assets.LoadedAssets);

            foreach (var item in Alternations)
            {
                loadedAssets[item.Key] = item.Value;
            }
        }
    }
}