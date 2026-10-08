using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MeridianPresentationConfigBuilder
    {
        public const string Path = "Assets/_Project/Data/Structure/MeridianPresentationConfig.asset";

        public static MeridianPresentationConfig Load()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Structure");
            MeridianPresentationConfig config = SerializedWiring.LoadOrCreateAsset<MeridianPresentationConfig>(Path);
            MigrateReadability(config);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void MigrateReadability(MeridianPresentationConfig config)
        {
            var data = new SerializedObject(config);
            SerializedProperty revision = data.FindProperty("readabilityRevision");
            if (revision.intValue >= 1) return;
            // Migrate the saved asset once; subsequent rebuilds preserve the artist's inspector tuning.
            data.FindProperty("fixtureIntensity").floatValue = 8.4f;
            data.FindProperty("fixtureRange").floatValue = 9.5f;
            data.FindProperty("ambientSky").colorValue = new Color(0.065f, 0.080f, 0.100f);
            data.FindProperty("ambientEquator").colorValue = new Color(0.038f, 0.049f, 0.051f);
            data.FindProperty("ambientGround").colorValue = new Color(0.018f, 0.021f, 0.018f);
            data.FindProperty("exteriorFogDensity").floatValue = 0.010f;
            data.FindProperty("interiorFogDensity").floatValue = 0.018f;
            data.FindProperty("moonIntensity").floatValue = 0.22f;
            data.FindProperty("vignette").floatValue = 0.14f;
            data.FindProperty("postExposure").floatValue = 0.2f;
            revision.intValue = 1;
            data.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[MeridianReadability] Migrated powered fixtures 2.1→8.4, range 8.5→9.5; "
                + "moon 0.09→0.22; exterior/interior fog 0.014/0.028→0.010/0.018; exposure +0.2 EV.");
        }
    }
}
