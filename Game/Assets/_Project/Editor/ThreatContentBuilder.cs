using Abandoned.Threats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Threat configs and original low-poly silhouettes. Rebuilds preserve existing AI tuning and network objects.</summary>
    public static class ThreatContentBuilder
    {
        public const string Folder = "Assets/_Project/Prefabs/Threats";
        public const string BlindOnePrefabPath = Folder + "/BlindOne.prefab";
        public const string BlindOneConfigPath = "Assets/_Project/Data/Threats/BlindOneConfig.asset";
        public const string StalkerPrefabPath = Folder + "/Stalker.prefab";
        public const string CollectorPrefabPath = Folder + "/Collector.prefab";
        public const string StalkerConfigPath = "Assets/_Project/Data/Threats/StalkerConfig.asset";
        public const string CollectorConfigPath = "Assets/_Project/Data/Threats/CollectorConfig.asset";
        public const string HunterPrefabPath = Folder + "/Hunter.prefab";
        public const string HunterConfigPath = "Assets/_Project/Data/Threats/HunterConfig.asset";

        public static void CreateOthers()
        {
            var stalkerConfig = LoadOrCreateAsset<StalkerConfig>(StalkerConfigPath);
            var collectorConfig = LoadOrCreateAsset<CollectorConfig>(CollectorConfigPath);
            var hunterConfig = LoadOrCreateAsset<HunterConfig>(HunterConfigPath);
            Body(StalkerPrefabPath, "Stalker", 2.6f, 0.3f, ThreatPresentation.Silhouette.Stalker,
                root => Set(root.AddComponent<Stalker>(), "config", stalkerConfig));
            Body(CollectorPrefabPath, "Collector", 1.3f, 0.35f, ThreatPresentation.Silhouette.Collector,
                root => Set(root.AddComponent<Collector>(), "config", collectorConfig));
            Body(HunterPrefabPath, "Hunter", 2.5f, 0.55f, ThreatPresentation.Silhouette.Hunter,
                root => root.AddComponent<Hunter>().EditorSetup(hunterConfig));
            AssetDatabase.SaveAssets();
        }

        private static GameObject Body(string path, string name, float height, float radius,
            ThreatPresentation.Silhouette silhouette, System.Action<GameObject> add)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && existing.GetComponent<Threat>() != null)
                return ThreatVisualBuilder.Upgrade(path, silhouette);

            PolishAssets.EnsureFolder(Folder);
            var root = new GameObject(name);
            root.AddComponent<NetworkObject>().DontDestroyWithOwner = true;
            var network = root.AddComponent<NetworkTransform>();
            network.SyncRotAngleX = network.SyncRotAngleZ = false;
            network.SyncScaleX = network.SyncScaleY = network.SyncScaleZ = false;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = Mathf.Max(0.3f, radius);
            agent.height = height;
            agent.acceleration = 14f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 0.3f;
            var collider = root.AddComponent<CapsuleCollider>();
            collider.height = height;
            collider.radius = radius;
            collider.center = Vector3.up * height * 0.5f;
            add(root);
            ThreatVisualBuilder.Build(root, silhouette);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }

        [MenuItem("Tools/Abandoned/Create Threat Prefabs")]
        public static GameObject CreateBlindOne()
        {
            var config = LoadOrCreateAsset<BlindOneConfig>(BlindOneConfigPath);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(BlindOnePrefabPath);
            if (existing != null && existing.GetComponent<BlindOne>() != null)
                return ThreatVisualBuilder.Upgrade(BlindOnePrefabPath, ThreatPresentation.Silhouette.BlindOne);
            GameObject prefab = Body(BlindOnePrefabPath, "BlindOne", 2.4f, 0.35f, ThreatPresentation.Silhouette.BlindOne,
                root => Set(root.AddComponent<BlindOne>(), "config", config));
            // Keep the listener's established navigation pacing rather than using another threat's defaults.
            GameObject contents = PrefabUtility.LoadPrefabContents(BlindOnePrefabPath);
            try
            {
                var agent = contents.GetComponent<NavMeshAgent>();
                agent.radius = 0.4f;
                agent.acceleration = 12f;
                agent.angularSpeed = 300f;
                agent.autoBraking = true;
                prefab = PrefabUtility.SaveAsPrefabAsset(contents, BlindOnePrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            return prefab;
        }
    }
}
