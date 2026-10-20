using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Turns a built piece (thousands of boxes) into one saved mesh with a submesh per material, so the street can
    /// stand a hundred and more copies of the training house as one renderer each instead of a hundred thousand
    /// objects. Saved over the same asset every rebuild, so references keep their GUID.
    /// </summary>
    public static class TutorialMeshMerge
    {
        public const string Folder = TutorialHouseMeshes.Folder + "/Street";

        /// <summary>A saved mesh and the materials of its submeshes, in order.</summary>
        public readonly struct Merged
        {
            public readonly Mesh Mesh;
            public readonly Material[] Materials;
            public Merged(Mesh mesh, Material[] materials) { Mesh = mesh; Materials = materials; }
        }

        /// <summary>Everything drawn under <paramref name="root"/>, in its local space, saved as <paramref name="name"/>.</summary>
        public static Merged Save(Transform root, string name)
        {
            var byMaterial = new Dictionary<Material, List<CombineInstance>>();
            var order = new List<Material>();
            Matrix4x4 toRoot = root.worldToLocalMatrix;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                var renderer = filter.GetComponent<MeshRenderer>();
                if (mesh == null || renderer == null) continue;
                Material[] materials = renderer.sharedMaterials;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    Material material = materials.Length == 0 ? null : materials[Mathf.Min(sub, materials.Length - 1)];
                    if (material == null) continue;
                    if (!byMaterial.TryGetValue(material, out List<CombineInstance> list))
                    {
                        byMaterial[material] = list = new List<CombineInstance>();
                        order.Add(material);
                    }
                    list.Add(new CombineInstance { mesh = mesh, subMeshIndex = sub, transform = toRoot * filter.transform.localToWorldMatrix });
                }
            }

            // One mesh per material, then those as the submeshes of one.
            var parts = new CombineInstance[order.Count];
            for (int i = 0; i < order.Count; i++)
            {
                var part = new Mesh { indexFormat = IndexFormat.UInt32 };
                part.CombineMeshes(byMaterial[order[i]].ToArray(), true, true);
                parts[i] = new CombineInstance { mesh = part, transform = Matrix4x4.identity };
            }
            var merged = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            merged.CombineMeshes(parts, false, true);
            foreach (CombineInstance part in parts) Object.DestroyImmediate(part.mesh);
            merged.RecalculateBounds();
            return new Merged(Keep(merged, name), order.ToArray());
        }

        private static Mesh Keep(Mesh mesh, string name)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, saved);
            saved.name = name;
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(saved);
            return saved;
        }
    }
}
