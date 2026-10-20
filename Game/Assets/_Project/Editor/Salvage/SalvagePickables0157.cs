using System;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using static Abandoned.Interaction.CarryClass;
using static Abandoned.Loot.SalvageCategory;

namespace Abandoned.EditorTools
{
    /// <summary>Turns recoverable 0.15.6 stock into real loot using the established network carry pipeline.</summary>
    public static class SalvagePickables0157
    {
        public static readonly SalvageAssetSpec[] Specs =
        {
            new("receipt_printer", "Receipt Printer", Tech, 2.6f, 200, 480, OneHand, "electronics", SurfaceMaterial.Plastic, Fragility.Medium),
            new("card_terminal", "Card Payment Terminal", Tech, .55f, 120, 320, Pocket, "electronics", SurfaceMaterial.Plastic, Fragility.Medium),
            new("headphone_carton", "Boxed Headphones", Tech, .65f, 180, 480, Pocket, "electronics", SurfaceMaterial.Paper, Fragility.Medium),
            new("network_switch", "Rack Network Switch", Tech, 3.2f, 650, 1500, OneHand, "electronics", SurfaceMaterial.Metal, Fragility.Medium),
            new("showroom_sofa", "Showroom Sofa", Furniture, 72, 900, 2400, Heavy, "furniture", SurfaceMaterial.Fabric),
            new("floor_lamp", "Showroom Floor Lamp", Furniture, 5.5f, 180, 520, TwoHand, "furniture", SurfaceMaterial.Metal, Fragility.Medium),
            new("office_printer", "Commercial Office Printer", Tech, 58, 1200, 2800, Heavy, "furniture", SurfaceMaterial.Plastic, Fragility.Medium),
            new("air_compressor", "Portable Air Compressor", Hardware, 52, 1300, 2800, Heavy, "hardware", SurfaceMaterial.Metal),
            new("welding_trolley", "Welder and Gas Trolley", Hardware, 65, 1800, 3900, Heavy, "hardware", SurfaceMaterial.Metal),
            new("workshop_vacuum", "Workshop Dust Vacuum", Hardware, 12, 400, 1000, TwoHand, "hardware", SurfaceMaterial.Metal),
            new("shoe_cartons", "Footwear Stock Bundle", SalvageCategory.Material, 8, 240, 650, TwoHand, "clothing", SurfaceMaterial.Paper),
            new("cash_register", "Cash Register", Tech, 8, 600, 1500, TwoHand, "clothing", SurfaceMaterial.Metal, Fragility.Medium),
            new("extension_reel", "Extension Cable Reel", Hardware, 6, 190, 540, OneHand, "hardware", SurfaceMaterial.Metal),
            new("mobile_planning_board", "Mobile Planning Board", Furniture, 16, 250, 650, TwoHand, "concourse", SurfaceMaterial.Metal),
            new("duct_stack", "Removed Ventilation Ducts", SalvageCategory.Material, 28, 350, 850, TwoHand, "concourse", SurfaceMaterial.Metal),
            new("recovery_cart", "Loaded Recovery Cart", Hardware, 22, 450, 1100, TwoHand, "concourse", SurfaceMaterial.Metal)
        };

        public static SalvageAssetSpec Find(string id) => Specs.FirstOrDefault(spec => spec.Id == id || spec.DefinitionId == id);

        [MenuItem(ToolMenu.Loot + "Author Additional Pickable Stock (0.15.7)", priority = ToolMenu.LootP + 21)]
        public static void Author()
        {
            PolishAssets.EnsureFolder(LootCatalogBuilder.Folder);
            int authored = 0;
            foreach (SalvageAssetSpec spec in Specs)
            {
                GameObject model = SalvageExpansionModels0156.Load(spec.Id);
                if (model == null)
                    throw new InvalidOperationException($"The saved 0.15.6 model '{spec.Id}' is required to author its pickup.");

                // Reuse saved meshes and their original worn finishes; the static model itself is untouched.
                Vector3 size = SalvageAssetBuilder.PositiveSize(ModelFit.LocalBounds(model, null).size);
                string path = $"{LootCatalogBuilder.Folder}/Loot_{spec.DefinitionId}.asset";
                LootDefinition definition = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<LootDefinition>();
                    definition.EditorSetup(spec.DefinitionId, spec.Name, spec.ValueMin, spec.ValueMax,
                        spec.Weight, spec.Carry, size, spec.Fragility, spec.Surface,
                        Noise(spec), spec.Carry >= Heavy ? .75f : 1.25f,
                        PlaceholderShape.Cube, Color.white, .68f, .96f, spec.Carry >= Heavy ? 2 : 0);
                    definition.EditorSetSpawning(Tags(spec), false);
                    AssetDatabase.CreateAsset(definition, path);
                }
                definition.EditorSetModel(model, 0, size);
                AuthorCarryPoints(definition, spec.Id);
                EditorUtility.SetDirty(definition);
                LootPrefabGenerator.Generate(definition);
                authored++;
            }
            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log($"[SalvagePickables0157] Authored {authored} additional registered pickups with fitted collision, focus outlines and carry sockets.");
        }

        private static string[] Tags(SalvageAssetSpec spec) => spec.Id switch
        {
            "receipt_printer" or "card_terminal" => new[] { "electronics", "clothing", "furniture" },
            "office_printer" => new[] { "furniture", "electronics", "office" },
            "cash_register" => new[] { "clothing", "electronics", "concourse" },
            "workshop_vacuum" or "extension_reel" => new[] { "hardware", "concourse" },
            "mobile_planning_board" => new[] { "concourse", "furniture", "office" },
            "duct_stack" => new[] { "concourse", "hardware" },
            "recovery_cart" => new[] { "concourse", "furniture", "clothing" },
            _ => new[] { spec.Tag }
        };

        private static float Noise(SalvageAssetSpec spec) => spec.Carry >= Heavy ? .65f
            : spec.Surface is SurfaceMaterial.Paper or SurfaceMaterial.Fabric ? .12f
            : spec.Carry == Pocket ? .10f : .30f;

        private static void AuthorCarryPoints(LootDefinition definition, string id)
        {
            var serialized = new SerializedObject(definition);
            SerializedProperty points = serialized.FindProperty("<CarryPoints>k__BackingField");
            Vector3[] grips = definition.CarryClass >= Heavy
                ? SalvagePrefabGeometry.GripPoints(id, new Bounds(Vector3.zero, definition.Size))
                : Array.Empty<Vector3>();
            points.arraySize = grips.Length;
            for (int i = 0; i < grips.Length; i++) points.GetArrayElementAtIndex(i).vector3Value = grips[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
