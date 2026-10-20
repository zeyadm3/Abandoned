using System.Collections.Generic;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.18.2 loot quirks: flags set on existing loot definitions (data only; LootQuirks does the rest).</summary>
    public static class Release0182
    {
        private static readonly Dictionary<string, LootQuirk> Quirks = new()
        {
            ["grandfather_clock"] = LootQuirk.Rings | LootQuirk.TopHeavy,
            ["smartphone"] = LootQuirk.Rings,
            ["retro_radio"] = LootQuirk.Rings,
            ["fridge"] = LootQuirk.Leaks | LootQuirk.TopHeavy,
            ["washing_machine"] = LootQuirk.Leaks,
            ["espresso_machine"] = LootQuirk.Leaks,
            ["bonsai"] = LootQuirk.Leaks,
            ["clawfoot_bathtub"] = LootQuirk.Leaks,
            ["salvage_industrial_battery"] = LootQuirk.Leaks,
            ["salvage_air_compressor"] = LootQuirk.Leaks | LootQuirk.Rattles,
            ["salvage_tool_chest"] = LootQuirk.Rattles,
            ["salvage_shoe_cartons"] = LootQuirk.Rattles,
            ["book_stack"] = LootQuirk.Rattles,
            ["blender"] = LootQuirk.Rattles,
            ["salvage_cash_register"] = LootQuirk.Rattles,
            ["salvage_headphone_carton"] = LootQuirk.Rattles,
            ["salvage_plastic_sorting_crate"] = LootQuirk.Rattles,
            ["salvage_scaffolding_pipes"] = LootQuirk.Rattles,
            ["floor_lamp"] = LootQuirk.TopHeavy,
            ["salvage_floor_lamp"] = LootQuirk.TopHeavy,
            ["designer_lamp"] = LootQuirk.TopHeavy,
            ["marble_statue"] = LootQuirk.TopHeavy,
            ["salvage_mannequin"] = LootQuirk.TopHeavy,
            ["salvage_garment_rack"] = LootQuirk.TopHeavy,
            ["server_rack"] = LootQuirk.TopHeavy,
            ["salvage_server_rack"] = LootQuirk.TopHeavy,
            ["vending_machine"] = LootQuirk.TopHeavy,
        };

        [MenuItem(ToolMenu.Features + "0.18.2 Loot Quirks", priority = ToolMenu.FeaturesP + 8)]
        public static void Author()
        {
            int n = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition", new[] { "Assets/_Project/Data/Loot" }))
            {
                var d = AssetDatabase.LoadAssetAtPath<LootDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (d == null || !Quirks.TryGetValue(d.Id, out LootQuirk q)) continue;
                d.EditorSetQuirks(q);
                EditorUtility.SetDirty(d);
                n++;
            }
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
            Debug.Log($"[Quirks0182] {n} loot definitions have quirks.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
