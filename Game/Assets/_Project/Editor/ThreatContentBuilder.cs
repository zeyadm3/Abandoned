using Abandoned.Threats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Threat prefabs and configs. The Blind One: a tall, pale, eyeless greybox figure on a NavMesh agent.</summary>
    public static class ThreatContentBuilder
    {
        public const string Folder = "Assets/_Project/Prefabs/Threats";
        public const string BlindOnePrefabPath = Folder + "/BlindOne.prefab";
        public const string BlindOneConfigPath = "Assets/_Project/Data/Threats/BlindOneConfig.asset";
        public const string StalkerPrefabPath = Folder + "/Stalker.prefab";
        public const string CollectorPrefabPath = Folder + "/Collector.prefab";
        public const string StalkerConfigPath = "Assets/_Project/Data/Threats/StalkerConfig.asset";
        public const string CollectorConfigPath = "Assets/_Project/Data/Threats/CollectorConfig.asset";

        /// <summary>The Stalker (tall, thin, dark) and the Collector (small, hunched, with a sack). Created when missing.</summary>
        public static void CreateOthers()
        {
            var stalkerConfig = LoadOrCreateAsset<StalkerConfig>(StalkerConfigPath);
            var collectorConfig = LoadOrCreateAsset<CollectorConfig>(CollectorConfigPath);
            Body(StalkerPrefabPath, "Stalker", 2.6f, 0.3f, new Color(0.08f, 0.08f, 0.1f), root =>
            {
                Set(root.AddComponent<Stalker>(), "config", stalkerConfig);
                Transform v = Group("Visual", root.transform);
                Material m = GetMaterial("Greybox_Stalker", new Color(0.08f, 0.08f, 0.1f));
                Primitive(PrimitiveType.Capsule, "Body", v, Vector3.up * 1.2f, new Vector3(0.38f, 1.2f, 0.3f), m, withCollider: false);
                Primitive(PrimitiveType.Sphere, "Head", v, Vector3.up * 2.45f, new Vector3(0.3f, 0.36f, 0.3f), m, withCollider: false);
                foreach (float side in new[] { -1f, 1f })
                    Primitive(PrimitiveType.Capsule, "Arm", v, new Vector3(side * 0.3f, 1.1f, 0f), new Vector3(0.1f, 0.95f, 0.1f), m, withCollider: false);
                Primitive(PrimitiveType.Sphere, "Eyes", v, new Vector3(0f, 2.5f, 0.13f), new Vector3(0.18f, 0.04f, 0.04f), GetMaterial("Greybox_StalkerEyes", new Color(0.9f, 0.9f, 0.85f)), withCollider: false);
            });
            Body(CollectorPrefabPath, "Collector", 1.3f, 0.35f, new Color(0.42f, 0.36f, 0.28f), root =>
            {
                Set(root.AddComponent<Collector>(), "config", collectorConfig);
                Transform v = Group("Visual", root.transform);
                Material m = GetMaterial("Greybox_Collector", new Color(0.42f, 0.36f, 0.28f));
                Primitive(PrimitiveType.Capsule, "Body", v, Vector3.up * 0.65f, new Vector3(0.6f, 0.6f, 0.55f), m, withCollider: false);
                Primitive(PrimitiveType.Sphere, "Head", v, new Vector3(0f, 1.1f, 0.2f), new Vector3(0.32f, 0.3f, 0.32f), m, withCollider: false);
                Primitive(PrimitiveType.Sphere, "Sack", v, new Vector3(0f, 0.9f, -0.35f), new Vector3(0.55f, 0.5f, 0.45f), GetMaterial("Greybox_Sack", new Color(0.55f, 0.5f, 0.35f)), withCollider: false);
            });
        }

        private static void Body(string path, string name, float height, float radius, Color color, System.Action<GameObject> add)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && existing.GetComponent<Threat>() != null) return;
            var root = new GameObject(name);
            root.AddComponent<NetworkObject>().DontDestroyWithOwner = true;
            var nt = root.AddComponent<NetworkTransform>();
            nt.SyncRotAngleX = false;
            nt.SyncRotAngleZ = false;
            nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = Mathf.Max(0.3f, radius);
            agent.height = height;
            agent.acceleration = 14f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 0.3f;
            var body = root.AddComponent<CapsuleCollider>();
            body.height = height;
            body.radius = radius;
            body.center = Vector3.up * height / 2f;
            add(root);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
        }
        private const float Height = 2.4f;

        [MenuItem("Tools/Abandoned/Create Threat Prefabs")]
        public static GameObject CreateBlindOne()
        {
            var config = LoadOrCreateAsset<BlindOneConfig>(BlindOneConfigPath);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(BlindOnePrefabPath);
            if (existing != null && existing.GetComponent<BlindOne>() != null) return existing;

            var root = new GameObject("BlindOne");
            var no = root.AddComponent<NetworkObject>();
            no.DontDestroyWithOwner = true;
            var nt = root.AddComponent<NetworkTransform>();
            nt.SyncRotAngleX = false;
            nt.SyncRotAngleZ = false;
            nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.4f;
            agent.height = Height;
            agent.acceleration = 12f;
            agent.angularSpeed = 300f;
            agent.stoppingDistance = 0.3f;
            agent.autoBraking = true;
            Set(root.AddComponent<BlindOne>(), "config", config);
            var body = root.AddComponent<CapsuleCollider>();
            body.height = Height;
            body.radius = 0.35f;
            body.center = Vector3.up * Height / 2f;

            Material skin = GetMaterial("Greybox_BlindOne", new Color(0.86f, 0.84f, 0.8f));
            Material mouth = GetMaterial("Greybox_BlindOneMouth", new Color(0.15f, 0.05f, 0.05f));
            Transform visual = Group("Visual", root.transform);
            Primitive(PrimitiveType.Capsule, "Body", visual, Vector3.up * 1.05f, new Vector3(0.55f, 1.05f, 0.4f), skin, withCollider: false);
            Primitive(PrimitiveType.Sphere, "Head", visual, Vector3.up * 2.1f, new Vector3(0.42f, 0.5f, 0.45f), skin, withCollider: false);
            Box("Mouth", visual, new Vector3(0f, 2.0f, 0.21f), new Vector3(0.22f, 0.05f, 0.04f), mouth, withCollider: false);
            foreach (float side in new[] { -1f, 1f })
                Primitive(PrimitiveType.Capsule, "Arm", visual, new Vector3(side * 0.42f, 1.0f, 0.05f), new Vector3(0.14f, 0.75f, 0.14f), skin, withCollider: false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BlindOnePrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            AssetDatabase.SaveAssets();
            return prefab;
        }
    }
}
