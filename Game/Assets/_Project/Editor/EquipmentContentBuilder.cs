using System.Collections.Generic;
using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>GDD 12 gear as data: one asset per item (created when missing, never overwritten) and the catalog in a fixed order.</summary>
    public static class EquipmentContentBuilder
    {
        public const string Folder = "Assets/_Project/Data/Equipment";
        public const string CatalogPath = Folder + "/EquipmentCatalog.asset";
        public const string NoiseMakerPrefabPath = "Assets/_Project/Prefabs/Equipment/NoiseMaker.prefab";

        [MenuItem("Tools/Abandoned/Create Equipment Content")]
        public static EquipmentCatalog CreateMissing()
        {
            var items = new List<EquipmentDefinition>
            {
                Item("flashlight", "Flashlight", "Lights the way (F). Essential when the power's off.", EquipmentKind.Flashlight, 50, 1, false, new Color(1f, 0.9f, 0.5f)),
                Item("radio", "Walkie-talkie", "Hold R to talk to everyone with a radio, anywhere.", EquipmentKind.Radio, 100, 1, false, new Color(0.3f, 0.3f, 0.3f)),
                Item("hand_trolley", "Hand Trolley", "Lets one person drag Heavy items alone.", EquipmentKind.HandTrolley, 400, 1, false, new Color(0.9f, 0.4f, 0.1f)),
                Item("medkit", "Medkit", "Gets a downed crewmate back on their feet (single use).", EquipmentKind.Medkit, 300, 1, true, new Color(0.9f, 0.2f, 0.2f)),
                Item("planks", "Planks", "A plank to bridge a hole in the floor (single use).", EquipmentKind.Planks, 150, 2, true, new Color(0.6f, 0.45f, 0.25f)),
                Item("noise_maker", "Noise Maker", "Throw it: it shrieks a few seconds later and draws the Blind One (single use).", EquipmentKind.NoiseMaker, 250, 2, true, new Color(0.9f, 0.9f, 0.2f)),
                Item("stress_scanner", "Stress Scanner", "Shows how close the floor ahead is to giving way.", EquipmentKind.StressScanner, 1200, 3, false, new Color(0.2f, 0.8f, 0.9f)),
            };
            CreateNoiseMaker();
            var catalog = SerializedWiring.LoadOrCreateAsset<EquipmentCatalog>(CatalogPath);
            catalog.EditorSet(items);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        /// <summary>The thrown noise maker: host-simulated, server-authoritative transform. Created when missing.</summary>
        public static GameObject CreateNoiseMaker()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(NoiseMakerPrefabPath);
            if (existing != null && existing.GetComponent<NoiseMakerDevice>() != null) return existing;
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Equipment")) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Equipment");
            var root = new GameObject("NoiseMaker");
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = true;
            root.AddComponent<Unity.Netcode.Components.NetworkTransform>();
            var body = root.AddComponent<Rigidbody>();
            body.mass = 0.8f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            root.AddComponent<SphereCollider>().radius = 0.12f;
            root.AddComponent<NoiseMakerDevice>();
            GreyboxFactory.Primitive(PrimitiveType.Sphere, "Visual", root.transform, Vector3.zero, Vector3.one * 0.24f,
                GreyboxFactory.GetMaterial("Greybox_NoiseMaker", new Color(0.95f, 0.85f, 0.15f)), withCollider: false);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, NoiseMakerPrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }

        private static EquipmentDefinition Item(string id, string name, string description, EquipmentKind kind, int price, int level, bool consumable, Color color)
        {
            string path = $"{Folder}/Equipment_{id}.asset";
            var d = AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(path);
            if (d != null) return d;
            d = ScriptableObject.CreateInstance<EquipmentDefinition>();
            d.EditorSetup(id, name, description, kind, price, level, consumable, color);
            AssetDatabase.CreateAsset(d, path);
            return d;
        }
    }
}
