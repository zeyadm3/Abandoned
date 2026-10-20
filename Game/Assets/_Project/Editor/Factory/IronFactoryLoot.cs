using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Makes the factory's loot playable: the definitions, their models, the network loot prefabs, the catalog,
    /// the network prefab list and the inventory icons. Runs at the start of every Iron Factory rebuild (before
    /// the scene opens: the icon renderer uses its own empty scene) and on its own from the menu. Only fills in
    /// what is missing, so it is cheap to run again.
    /// </summary>
    public static class IronFactoryLoot
    {
        [MenuItem(ToolMenu.Factory + "Add Iron Factory Loot", priority = ToolMenu.FactoryP + 20)]
        public static void Menu() => EnsureContent();

        public static void EnsureContent()
        {
            int defined = IronFactoryLootItems.CreateMissing();
            int modelled = IronFactoryLootModels.AssignMissing();
            int prefabs = 0;
            foreach (IronFactoryLootItems.Item item in IronFactoryLootItems.Items)
            {
                LootDefinition definition = LootCatalogBuilder.Load(item.Id);
                if (definition == null) continue;
                bool missing = AssetDatabase.LoadAssetAtPath<GameObject>(LootPrefabGenerator.PrefabPathFor(definition)) == null;
                // A new model needs a new prefab around it.
                if (!missing && modelled == 0) continue;
                LootPrefabGenerator.Generate(definition);
                prefabs++;
            }
            if (defined + modelled + prefabs == 0) return;
            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            if (LootIconRenderer.Render(force: false) > 0) UiContentBuilder.BuildIcons();
            AssetDatabase.SaveAssets();
            Debug.Log($"[IronFactory] Loot: {defined} new definitions, {modelled} new models, {prefabs} prefabs (re)built.");
        }
    }
}
