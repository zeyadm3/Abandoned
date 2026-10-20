using System.Collections.Generic;
using System.Linq;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.18.3 loot sets: six sets of existing loot as data, their catalog, RunSets on the RunState prefab.</summary>
    public static class Release0183
    {
        private const string Folder = "Assets/_Project/Data/Loot/Sets";

        private static readonly (string id, string name, string[] members, int bonus)[] Sets =
        {
            ("home_office", "Home Office", new[] { "desktop_pc", "computer_monitor", "computer_keyboard", "computer_mouse" }, 1800),
            ("kitchen", "Kitchen Counter", new[] { "toaster", "blender", "microwave", "espresso_machine" }, 1600),
            ("gallery", "Gallery Wall", new[] { "small_painting", "large_painting", "antique_vase" }, 2500),
            ("jewellery_box", "Jewellery Box", new[] { "diamond_ring", "pearl_necklace", "gold_watch" }, 3000),
            ("living_room", "Living Room", new[] { "flatscreen_tv", "bookshelf_speaker", "game_console", "table_lamp" }, 2000),
            ("checkout", "Checkout Lane", new[] { "salvage_cash_register", "salvage_card_terminal", "salvage_receipt_printer" }, 1400),
        };

        [MenuItem(ToolMenu.Features + "0.18.3 Loot Sets", priority = ToolMenu.FeaturesP + 9)]
        public static void Author()
        {
            PolishAssets.EnsureFolder(Folder);
            var byId = new Dictionary<string, LootDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition", new[] { "Assets/_Project/Data/Loot" }))
            {
                var d = AssetDatabase.LoadAssetAtPath<LootDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (d != null) byId[d.Id] = d;
            }
            var all = new List<LootSet>();
            foreach ((string id, string name, string[] members, int bonus) in Sets)
            {
                LootDefinition[] found = members.Where(byId.ContainsKey).Select(m => byId[m]).ToArray();
                if (found.Length < 2)
                {
                    Debug.LogWarning($"[Sets0183] Set {name} left out: only {found.Length} of its members exist.");
                    continue;
                }
                var set = SerializedWiring.LoadOrCreateAsset<LootSet>($"{Folder}/LootSet_{id}.asset");
                set.EditorSetup(name, found, bonus);
                EditorUtility.SetDirty(set);
                all.Add(set);
            }
            var catalog = ReleaseKit.Config<LootSetCatalog>("Assets/_Project/Data/Loot/Resources", LootSetCatalog.ResourcePath);
            catalog.EditorSet(all);
            EditorUtility.SetDirty(catalog);
            NetworkContentBuilder.CreateRunStatePrefab();
            BuildScript.ApplyPlayerSettings();
            Debug.Log($"[Sets0183] {all.Count} loot sets.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
