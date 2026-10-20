using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Makes the island's loot playable: the definitions, their models, the network loot prefabs, the catalog, the
    /// network prefab list and the inventory icons (the Iron Factory's steps, for the island's items). Runs at the start
    /// of every island rebuild (before the scene opens: the icon renderer uses its own empty scene) and on its own from
    /// the menu. Only fills in what is missing, so it is cheap to run again.
    /// </summary>
    public static class IslandLoot
    {
        [MenuItem(ToolMenu.Prison + "Add Island Prison Loot", priority = ToolMenu.PrisonP + 20)]
        public static void Menu() => EnsureContent();

        public static void EnsureContent()
        {
            int defined = IslandLootItems.CreateMissing();
            int modelled = IslandLootModels.AssignMissing();
            int prefabs = 0;
            foreach (IslandLootItems.Item item in IslandLootItems.Items)
            {
                LootDefinition definition = LootCatalogBuilder.Load(item.Id);
                if (definition == null) continue;
                bool missing = AssetDatabase.LoadAssetAtPath<GameObject>(LootPrefabGenerator.PrefabPathFor(definition)) == null;
                if (!missing && modelled == 0) continue;
                LootPrefabGenerator.Generate(definition);
                prefabs++;
            }
            if (defined + modelled + prefabs == 0) return;
            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            if (LootIconRenderer.Render(force: false) > 0) UiContentBuilder.BuildIcons();
            AssetDatabase.SaveAssets();
            Debug.Log($"[IslandPrison] Loot: {defined} new definitions, {modelled} new models, {prefabs} prefabs (re)built.");
        }
    }
}
