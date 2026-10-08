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
            if (revision.intValue >= 2) return;
            // Migrate the saved asset once; subsequent rebuilds preserve the artist's inspector tuning.
            if (revision.intValue < 1)
            {
                data.FindProperty("ambientSky").colorValue = new Color(0.065f, 0.080f, 0.100f);
                data.FindProperty("ambientEquator").colorValue = new Color(0.038f, 0.049f, 0.051f);
                data.FindProperty("ambientGround").colorValue = new Color(0.018f, 0.021f, 0.018f);
                data.FindProperty("exteriorFogDensity").floatValue = 0.010f;
                data.FindProperty("interiorFogDensity").floatValue = 0.018f;
                data.FindProperty("moonIntensity").floatValue = 0.22f;
                data.FindProperty("vignette").floatValue = 0.14f;
                data.FindProperty("postExposure").floatValue = 0.2f;
            }
            data.FindProperty("fixtureIntensity").floatValue = 18f;
            data.FindProperty("fixtureRange").floatValue = 7.5f;
            data.FindProperty("fixtureAngle").floatValue = 116f;
            data.FindProperty("basementIntensityScale").floatValue = 0.8f;
            data.FindProperty("faultyPercent").intValue = 28;
            data.FindProperty("deadPercent").intValue = 10;
            data.FindProperty("maximumFixtures").intValue = 384;
            data.FindProperty("shadowLights").intValue = Mathf.Min(4, data.FindProperty("shadowLights").intValue);
            data.FindProperty("storeFixtureStride").intValue = 1;
            data.FindProperty("serviceFixtureStride").intValue = 1;
            data.FindProperty("concourseFixtureStride").intValue = 2;
            data.FindProperty("atriumFixtures").intValue = 4;
            data.FindProperty("atriumFixtureHeight").floatValue = 6.4f;
            data.FindProperty("atriumFixtureIntensity").floatValue = 42f;
            data.FindProperty("atriumFixtureRange").floatValue = 12f;
            revision.intValue = 2;
            data.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[MeridianReadability] Revision2: fixture intensity18/range7.5; dense store/food/service grid; "
                + "four skylight pendants; dead10%/faulty28%; global shadow fixtures capped4.");
        }
    }
}
