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
            var catalog = SerializedWiring.LoadOrCreateAsset<EquipmentCatalog>(CatalogPath);
            catalog.EditorSet(items);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
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
