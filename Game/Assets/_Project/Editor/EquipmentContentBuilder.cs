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
                // M9.4 (append only: the catalog order is the network index)
                Item("crowbar", "Crowbar", "Strike the floor in front of you (use): weak floors break, sound ones crack first. Loud.", EquipmentKind.Crowbar, 350, 1, false, new Color(0.7f, 0.15f, 0.1f)),
                Item("backpack", "Backpack", "Two more pocket slots while you carry it.", EquipmentKind.Backpack, 600, 2, false, new Color(0.3f, 0.4f, 0.25f)),
                Item("support_jack", "Support Jack", "Brace the floor you stand on from below: it holds 60 % more until the next job (single use).", EquipmentKind.SupportJack, 500, 3, true, new Color(0.95f, 0.75f, 0.1f)),
                // M10 (append only)
                Item("flatbed", "Flatbed Trolley", "A flatbed and a fold-out ramp: a short crew, or one stubborn person, can drag Huge items (slowly). Works as a hand trolley too.",
                    EquipmentKind.Flatbed, 2000, 3, false, new Color(0.25f, 0.45f, 0.75f)),
                Item("rope_pulley", "Rope & Pulley", "Rig it over a hole in the floor (use, facing the hole): loot dropped down it and crewmates climbing down come down gently. Lasts the job.",
                    EquipmentKind.RopePulley, 600, 2, true, new Color(0.75f, 0.6f, 0.35f)),
                Item("bolt_cutters", "Bolt Cutters", "Cuts the padlock on a store's shutter (E on the shutter), quietly. A crowbar gets you in too, loudly.",
                    EquipmentKind.BoltCutters, 1500, 2, false, new Color(0.8f, 0.2f, 0.2f)),
            };
            CreateNoiseMaker();
            CreateSupportJack();
            var catalog = SerializedWiring.LoadOrCreateAsset<EquipmentCatalog>(CatalogPath);
            catalog.EditorSet(items);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public const string SupportJackPrefabPath = "Assets/_Project/Prefabs/Equipment/SupportJack.prefab";
        public const string PulleyPrefabPath = "Assets/_Project/Prefabs/Equipment/Pulley.prefab";

        /// <summary>The rope and pulley (M10.3): a tripod over the hole and a rope that stretches to the floor below. Created when missing.</summary>
        public static GameObject CreatePulley()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PulleyPrefabPath);
            if (existing != null && existing.GetComponent<Pulley>() != null) return existing;
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Equipment")) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Equipment");
            var root = new GameObject("Pulley");
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = true;
            Material wood = GreyboxFactory.GetMaterial("Greybox_PulleyFrame", new Color(0.55f, 0.4f, 0.22f));
            Material hemp = GreyboxFactory.GetMaterial("Greybox_Rope", new Color(0.78f, 0.68f, 0.45f));
            Material steel = GreyboxFactory.GetMaterial("Greybox_PulleyWheel", new Color(0.35f, 0.36f, 0.4f));
            // Tripod legs: three poles leaning in to an apex 1.8 m up over the hole.
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad;
                Vector3 foot = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.3f;
                Vector3 apex = Vector3.up * 1.8f;
                GameObject leg = GreyboxFactory.Primitive(PrimitiveType.Cylinder, "Leg", root.transform, (foot + apex) / 2f,
                    new Vector3(0.07f, Vector3.Distance(foot, apex) / 2f, 0.07f), wood, withCollider: false);
                leg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, apex - foot);
            }
            GreyboxFactory.Primitive(PrimitiveType.Cylinder, "Wheel", root.transform, Vector3.up * 1.75f, new Vector3(0.3f, 0.04f, 0.3f), steel, withCollider: false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject rope = GreyboxFactory.Primitive(PrimitiveType.Cylinder, "Rope", root.transform, Vector3.zero, new Vector3(0.04f, 1f, 0.04f), hemp, withCollider: false);
            var pulley = root.AddComponent<Pulley>();
            pulley.EditorSetup(rope.transform);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PulleyPrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }

        /// <summary>The support jack post (M9.4): host-placed, static; the post stretches to fit. Created when missing.</summary>
        public static GameObject CreateSupportJack()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SupportJackPrefabPath);
            if (existing != null && existing.GetComponent<SupportJack>() != null) return existing;
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Equipment")) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Equipment");
            var root = new GameObject("SupportJack");
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = true;
            Material yellow = GreyboxFactory.GetMaterial("Greybox_SupportJack", new Color(0.95f, 0.75f, 0.1f));
            GreyboxFactory.Primitive(PrimitiveType.Cylinder, "Base", root.transform, Vector3.up * 0.05f, new Vector3(0.5f, 0.05f, 0.5f), yellow, withCollider: false);
            GameObject post = GreyboxFactory.Primitive(PrimitiveType.Cylinder, "Post", root.transform, Vector3.up, new Vector3(0.18f, 1f, 0.18f), yellow, withCollider: true);
            var jack = root.AddComponent<SupportJack>();
            SerializedWiring.Set(jack, "post", post.transform);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SupportJackPrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
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
