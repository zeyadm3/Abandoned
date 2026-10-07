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
