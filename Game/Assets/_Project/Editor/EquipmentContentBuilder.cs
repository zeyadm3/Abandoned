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

        [MenuItem(ToolMenu.Data + "Create Equipment Content", priority = ToolMenu.DataP + 1)]
        public static EquipmentCatalog CreateMissing()
        {
            // The catalog order is each item's index on the network within one build, so any order works; saves keep ids.
            // 2026-10-22: flare, floodlight, spray chalk, flatbed, backpack, moving blanket, crash mat, rope & pulley and
            // support jack were taken out of the game (user's call).
            var items = new List<EquipmentDefinition>
            {
                Item("flashlight", "Flashlight", "Lights the way (F). Essential when the power's off.", EquipmentKind.Flashlight, 400, 1, false, new Color(1f, 0.9f, 0.5f)),
                Item("radio", "Walkie-talkie", "Hold R to talk to everyone with a radio, anywhere.", EquipmentKind.Radio, 600, 1, false, new Color(0.3f, 0.3f, 0.3f)),
                Item("hand_trolley", "Hand Trolley", "Lets one person, or a crew too small to lift it, drag a Huge item (slowly).", EquipmentKind.HandTrolley, 2500, 1, false, new Color(0.9f, 0.4f, 0.1f)),
                Item("medkit", "Medkit", "Treat yourself or an injured crewmate for 65 HP. A nearby downed crewmate gets up with 50 HP (single use).", EquipmentKind.Medkit, 1500, 1, true, new Color(0.9f, 0.2f, 0.2f)),
                Item("planks", "Planks", "A plank to bridge a hole in the floor (single use).", EquipmentKind.Planks, 800, 2, true, new Color(0.6f, 0.45f, 0.25f)),
                Item("noise_maker", "Noise Maker", "Throw it: it shrieks a few seconds later and draws the Blind One (single use).", EquipmentKind.NoiseMaker, 1200, 2, true, new Color(0.9f, 0.9f, 0.2f)),
                Item("stress_scanner", "Stress Scanner", "Shows how close the floor ahead is to giving way.", EquipmentKind.StressScanner, 8000, 3, false, new Color(0.2f, 0.8f, 0.9f)),
                // M9.4
                Item("crowbar", "Crowbar", "Strike the floor in front of you (use): weak floors break, sound ones crack first. Loud.", EquipmentKind.Crowbar, 2000, 1, false, new Color(0.7f, 0.15f, 0.1f)),
                // M10
                Item("bolt_cutters", "Bolt Cutters", "Cuts the padlock on a store's shutter (E on the shutter), quietly. A crowbar gets you in too, loudly.",
                    EquipmentKind.BoltCutters, 3500, 2, false, new Color(0.8f, 0.2f, 0.2f)),
                Item("motion_detector", "Motion Detector", "In hand: pings anything moving within 22 m, and which way it is. The Stalker hates it.",
                    EquipmentKind.MotionDetector, 9000, 4, false, new Color(0.3f, 0.85f, 0.4f)),
                Item("night_vision", "Night Vision", "In hand, use to switch on: see in the dark for two minutes of battery (it recharges while off).",
                    EquipmentKind.NightVision, 14000, 6, false, new Color(0.35f, 0.75f, 0.3f)),
                Item("battery", "Flashlight Battery", "Replaces a used flashlight cell. Keep your flashlight in the other slot and use (single use).",
                    EquipmentKind.Battery, 500, 1, true, new Color(0.5f, 0.56f, 0.42f)),
                // 0.17+
                Item("sledgehammer", "Sledgehammer", "Swing it at the floor (use): a few blows break a sound section, through its warning. Make your own hole to drop loot through. Very loud.",
                    EquipmentKind.Sledgehammer, 3000, 2, false, new Color(0.45f, 0.42f, 0.4f)),
                Item("glow_sticks", "Glow Sticks", "Throw one (use): a dim green light that burns for ten minutes. Marks a room, a route or a hole (single use).",
                    EquipmentKind.GlowSticks, 150, 1, true, new Color(0.3f, 1f, 0.4f)),
                Item("lockpick", "Lockpick Set", "E on a locked shutter: picks the padlock in silence, slowly. Bolt cutters are quicker, a crowbar quicker still and loud.",
                    EquipmentKind.Lockpick, 1800, 2, false, new Color(0.6f, 0.6f, 0.65f)),
                Item("thermal_camera", "Thermal Camera", "In hand, use to switch on: anything warm in sight burns bright through dark and fog. A minute of battery; it recharges while off.",
                    EquipmentKind.ThermalCamera, 12000, 5, false, new Color(1f, 0.5f, 0.15f)),
                Item("drone", "Scout Drone", "Use to launch and fly it (you stand still meanwhile); use again to bring it home. A minute and a half of battery. Monsters hear it.",
                    EquipmentKind.Drone, 16000, 6, false, new Color(0.3f, 0.35f, 0.4f)),
                // @@gear
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
            if (d != null)
            {
                if (id == "medkit")
                {
                    d.EditorSetup(id, name, description, kind, d.Price, d.UnlockLevel, consumable, d.Color);
                    EditorUtility.SetDirty(d);
                }
                return d;
            }
            d = ScriptableObject.CreateInstance<EquipmentDefinition>();
            d.EditorSetup(id, name, description, kind, price, level, consumable, color);
            AssetDatabase.CreateAsset(d, path);
            return d;
        }
    }
}
