using Abandoned.Threats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Rebuilds the original four threats with custom Blender rigs; preserves their GDD navigation and behavior configs.</summary>
    public static class ThreatContentBuilder
    {
        public const string Folder = "Assets/_Project/Prefabs/Threats";
        public const string DataFolder = "Assets/_Project/Data/Threats";
        public const string CatalogPath = DataFolder + "/ThreatCatalog.asset";
        public const string BlindOnePrefabPath = Folder + "/BlindOne.prefab";
        public const string BlindOneConfigPath = DataFolder + "/BlindOneConfig.asset";
        public const string StalkerPrefabPath = Folder + "/Stalker.prefab";
        public const string CollectorPrefabPath = Folder + "/Collector.prefab";
        public const string StalkerConfigPath = DataFolder + "/StalkerConfig.asset";
        public const string CollectorConfigPath = DataFolder + "/CollectorConfig.asset";
        public const string HunterPrefabPath = Folder + "/Hunter.prefab";
        public const string HunterConfigPath = DataFolder + "/HunterConfig.asset";
        public static void CreateOthers()
        {
            PolishAssets.EnsureFolder(DataFolder);
            var stalker = LoadOrCreateAsset<StalkerConfig>(StalkerConfigPath);
            var collector = LoadOrCreateAsset<CollectorConfig>(CollectorConfigPath);
            var hunter = LoadOrCreateAsset<HunterConfig>(HunterConfigPath);
            Build(StalkerPrefabPath, "Stalker", 3.1f, 0.3f, ThreatKind.Stalker, 1f, 85f, 2.4f, 6.5f,
                root => Set(root.AddComponent<Stalker>(), "config", stalker));
            Build(CollectorPrefabPath, "Collector", 1.5f, 0.35f, ThreatKind.Collector, 0.65f, 0f, 2.4f, 5.5f,
                root => Set(root.AddComponent<Collector>(), "config", collector));
            Build(HunterPrefabPath, "Hunter", 2.75f, 0.55f, ThreatKind.Hunter, 0.4f, 1000f, 1.8f, 5f,
                root => root.AddComponent<Hunter>().EditorSetup(hunter), true);
            AssetDatabase.SaveAssets();
        }
        public static GameObject Build(string path, string name, float height, float radius, ThreatKind kind, float weight,
            float damage, float walk, float chase, System.Action<GameObject> add, bool lethal = false, int minimumDanger = 0, bool final = false)
        {
            PolishAssets.EnsureFolder(Folder); PolishAssets.EnsureFolder(DataFolder);
            ThreatDefinition definition = LoadOrCreateAsset<ThreatDefinition>($"{DataFolder}/{name}Definition.asset");
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject prefab;
            if (existing != null && existing.GetComponent<Threat>() != null)
                prefab = ThreatRigBuilder.Upgrade(path, name, definition);
            else
            {
                var root = new GameObject(name);
                root.AddComponent<NetworkObject>().DontDestroyWithOwner = true;
                var network = root.AddComponent<NetworkTransform>();
                network.SyncRotAngleX = network.SyncRotAngleZ = false;
                network.SyncScaleX = network.SyncScaleY = network.SyncScaleZ = false;
                var agent = root.AddComponent<NavMeshAgent>();
                agent.radius = Mathf.Max(0.3f, radius); agent.height = height;
                agent.acceleration = 14f; agent.angularSpeed = 360f; agent.stoppingDistance = 0.3f;
                var collider = root.AddComponent<CapsuleCollider>();
                collider.height = height; collider.radius = radius; collider.center = Vector3.up * height * 0.5f;
                add(root); root.GetComponent<Threat>().EditorSetupDefinition(definition);
                ThreatRigBuilder.Install(root, name);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Object.DestroyImmediate(root);
            }
            NetworkObjectIds.StampPrefab(prefab);
            definition.EditorSetup(kind, kind == ThreatKind.Weight ? "The Weight" : kind == ThreatKind.Thing ? "The Thing" : kind == ThreatKind.LastHunter ? "The Last Hunter" : name == "BlindOne" ? "Blind One" : name,
                prefab.GetComponent<NetworkObject>(), weight, minimumDanger, damage, walk, chase, lethal, final);
            EditorUtility.SetDirty(definition);
            return prefab;
        }
        [MenuItem("Tools/Abandoned/Create Threat Prefabs")]
        public static GameObject CreateBlindOne()
        {
            PolishAssets.EnsureFolder(DataFolder);
            var config = LoadOrCreateAsset<BlindOneConfig>(BlindOneConfigPath);
            GameObject prefab = Build(BlindOnePrefabPath, "BlindOne", 2.65f, 0.35f, ThreatKind.BlindOne, 1.4f, 65f, 1.6f, 4.2f,
                root => Set(root.AddComponent<BlindOne>(), "config", config));
            AssetDatabase.SaveAssets();
            return prefab;
        }
    }
}
