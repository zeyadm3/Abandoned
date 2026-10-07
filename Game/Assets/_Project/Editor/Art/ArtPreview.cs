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

        /// <summary>Every hat on a player-sized capsule in a row (unsaved scene); returns the row's width.</summary>
        public static float BuildHatLineup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var catalog = AssetDatabase.LoadAssetAtPath<Abandoned.Player.CosmeticCatalog>(CosmeticsBuilder.CatalogPath);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            float x = 0f;
            for (int i = 0; i < catalog.Hats.Count; i++, x += 1.1f)
            {
                var suit = catalog.Coverall(i % catalog.Coveralls.Count);
                GameObject body = GreyboxFactory.Primitive(PrimitiveType.Capsule, $"Body_{i}", null, new Vector3(x, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.6f),
                    GreyboxFactory.GetMaterial($"Preview_Coverall_{suit.Id}", suit.Color), withCollider: false);
                if (catalog.Hats[i].HatPrefab != null)
                    Object.Instantiate(catalog.Hats[i].HatPrefab, body.transform.TransformPoint(Vector3.up) - Vector3.up * 0.04f, Quaternion.identity);
            }
            floor.transform.position = new Vector3(x / 2f, 0f, 0f);
            return x;
        }

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
