using Mafi;
using Mafi.Collections;
using Mafi.Unity;

namespace CustomAssets.Data.Mod
{
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    public class CustomAssetManager
    {
        public static Dict<string, UnityEngine.Object> Alternations { get; } = new Dict<string, UnityEngine.Object>();

        public static void Clear()
        {
            Alternations.Clear();
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