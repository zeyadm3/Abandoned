using Abandoned.Threats;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Additional threat content is separate from the four rebuilt launch creatures.</summary>
    public static class NewThreatContentBuilder
    {
        public const string CatalogPath = ThreatContentBuilder.CatalogPath;
        public static readonly string[] PrefabPaths = { WeightPrefabPath, ThingPrefabPath, CrawlersPrefabPath, LastHunterPrefabPath };
        public const string WeightPrefabPath = ThreatContentBuilder.Folder + "/Weight.prefab";
        public const string ThingPrefabPath = ThreatContentBuilder.Folder + "/Thing.prefab";
        public const string CrawlersPrefabPath = ThreatContentBuilder.Folder + "/Crawlers.prefab";
        public const string LastHunterPrefabPath = ThreatContentBuilder.Folder + "/LastHunter.prefab";
        [MenuItem("Tools/Abandoned/Create New Horror Threats")]
        public static void Create()
        {
            ThreatContentBuilder.Build(WeightPrefabPath, "Weight", 4.9f, 0.7f, ThreatKind.Weight, 0f, 0f, 0.85f, 1.3f,
                root => root.AddComponent<TheWeight>(), minimumDanger: 3);
            ThreatContentBuilder.Build(ThingPrefabPath, "Thing", 2.35f, 0.35f, ThreatKind.Thing, 0f, 70f, 1.4f, 5.2f,
                root => root.AddComponent<TheThing>(), minimumDanger: 4);
            ThreatContentBuilder.Build(CrawlersPrefabPath, "Crawlers", 0.9f, 0.45f, ThreatKind.Crawlers, 0f, 8f, 1.6f, 3.4f,
                root => root.AddComponent<CrawlerSwarm>(), minimumDanger: 1);
            ThreatContentBuilder.Build(LastHunterPrefabPath, "LastHunter", 3.9f, 0.65f, ThreatKind.LastHunter, 0f, 1000f, 3f, 6.6f,
                root => root.AddComponent<LastHunter>(), lethal: true, minimumDanger: 10, final: true);
            var definitions = new ThreatDefinition[8];
            string[] names = { "BlindOne", "Stalker", "Collector", "Hunter", "Weight", "Thing", "Crawlers", "LastHunter" };
            for (int i = 0; i < names.Length; i++) definitions[i] = AssetDatabase.LoadAssetAtPath<ThreatDefinition>($"{ThreatContentBuilder.DataFolder}/{names[i]}Definition.asset");
            ThreatCatalog catalog = LoadOrCreateAsset<ThreatCatalog>(ThreatContentBuilder.CatalogPath);
            catalog.EditorSetup(definitions); EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
    }
}
