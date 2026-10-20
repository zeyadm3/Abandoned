using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Instantiates the original FBX kit beneath neutral placement holders; collision belongs to the furniture.</summary>
    public static class TutorialHouseModels
    {
        public static Transform Place(string key, Transform parent, Vector3 position, float yaw, TutorialHouseArt art,
            Vector3? scale = null, Material bedding = null)
        {
            string path = $"{TutorialHouseMaterials.Folder}/{key}.fbx";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException($"Missing original house model: {path}");
            var holder = new GameObject(key).transform;
            holder.SetParent(parent, false);
            holder.SetLocalPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            holder.localScale = scale ?? Vector3.one;
            var geometry = (GameObject)PrefabUtility.InstantiatePrefab(source, holder);
            geometry.name = "Geometry";
            foreach (var lod in geometry.GetComponentsInChildren<LODGroup>(true)) UnityEngine.Object.DestroyImmediate(lod);
            foreach (var collider in geometry.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
            foreach (var renderer in geometry.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material =>
                {
                    string slot = material.name.Replace("MALL_", "");
                    if (slot == "Bedding" && bedding != null) return bedding;
                    var field = typeof(TutorialHouseArt).GetField(slot);
                    if (field == null || field.GetValue(art) is not Material bound)
                        throw new InvalidOperationException($"Unbound house material: {key}/{slot}");
                    return bound;
                }).ToArray();
            Transform[] levels = geometry.GetComponentsInChildren<Transform>(true).Where(t => t.name is "LOD0" or "LOD1" or "LOD2").ToArray();
            if (levels.Length == 3)
            {
                var lod = holder.gameObject.AddComponent<LODGroup>();
                lod.SetLODs(new[] {
                    new LOD(.14f, levels.First(t => t.name == "LOD0").GetComponentsInChildren<Renderer>()),
                    new LOD(.055f, levels.First(t => t.name == "LOD1").GetComponentsInChildren<Renderer>()),
                    new LOD(.008f, levels.First(t => t.name == "LOD2").GetComponentsInChildren<Renderer>()) });
                lod.RecalculateBounds();
            }
            return holder;
        }

        public static void Replace(Transform piece, string key, TutorialHouseArt art, Vector3? scale = null, Material bedding = null)
        {
            // Keep the original collider and SectionProp owner. The training's collapsing bed still follows its floor.
            foreach (Transform child in piece.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
            Place(key, piece, Vector3.zero, 0, art, scale, bedding);
        }
    }
}
