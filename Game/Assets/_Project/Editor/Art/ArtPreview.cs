using System.Linq;
using Abandoned.Core;
using Abandoned.Loot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A throwaway scene with every loot prefab lined up on a floor, for checking art at a glance
    /// (batch screenshots, or Tools/Abandoned/Art/Loot Lineup in the editor). Never saved.
    /// </summary>
    public static class ArtPreview
    {
        [MenuItem("Tools/Abandoned/Art/Loot Lineup (unsaved scene)")]
        public static void OpenLootLineup() => BuildLootLineup();

        /// <summary>Builds the lineup; returns its width (m), for framing a camera.</summary>
        public static float BuildLootLineup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var catalog = AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath);
            var entries = catalog.Entries.Where(e => e.definition != null && e.prefab != null)
                .OrderBy(e => e.definition.Size.x).ToList();
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.localScale = new Vector3(8f, 1f, 2f);
            float x = 0f;
            foreach (LootCatalog.Entry e in entries)
            {
                Vector3 size = e.definition.Size;
                x += size.x * 0.5f;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(e.prefab);
                go.transform.position = new Vector3(x, size.y * 0.5f, 0f);
                x += size.x * 0.5f + 0.3f;
            }
            floor.transform.position = new Vector3(x * 0.5f, 0f, 0f);
            return x;
        }
    }
}
