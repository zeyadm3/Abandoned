using Abandoned.Structure;
using UnityEditor;

namespace Abandoned.EditorTools
{
    public static class MeridianPresentationConfigBuilder
    {
        public const string Path = "Assets/_Project/Data/Structure/MeridianPresentationConfig.asset";

        public static MeridianPresentationConfig Load()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Structure");
            MeridianPresentationConfig config = SerializedWiring.LoadOrCreateAsset<MeridianPresentationConfig>(Path);
            EditorUtility.SetDirty(config);
            return config;
        }
    }
}
