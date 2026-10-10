using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Saved material/LOD batches per supporting slab, retaining independent collapse behavior.</summary>
    public static class SalvageDensityBatcher
    {
        public const string Folder = SalvageAssetBuilder.ArtFolder + "/Density0155b/Meshes";

        private sealed class Cell
        {
            public string Name;
            public StructuralSection Support;
            public readonly Dictionary<Material, List<CombineInstance>>[] Levels =
                { new(), new(), new() };
        }

        public static int Build(Transform root, string meshFolder = Folder)
        {
            PolishAssets.EnsureFolder(meshFolder);
            var meshLevels = new Dictionary<Renderer, int>();
            LODGroup[] sourceGroups = root.GetComponentsInChildren<LODGroup>(true);
            foreach (LODGroup group in sourceGroups)
            {
                LOD[] lods = group.GetLODs();
                for (int i = 0; i < lods.Length; i++)
                    foreach (Renderer renderer in lods[i].renderers)
                        if (renderer != null) meshLevels[renderer] = Mathf.Min(i, 2);
            }
            var cells = new Dictionary<string, Cell>();
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter filter in filters)
            {
                Mesh mesh = filter.sharedMesh;
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                var binding = filter.GetComponentInParent<SectionProp>();
                if (mesh == null || renderer == null || binding == null || binding.Section == null) continue;
                Transform zone = binding.transform;
                while (zone.parent != null && zone.parent != root) zone = zone.parent;
                string key = zone.name + "_" + binding.Section.name;
                if (!cells.TryGetValue(key, out Cell cell))
                    cells[key] = cell = new Cell { Name = key, Support = binding.Section };
                Matrix4x4 toCell = Matrix4x4.Translate(-cell.Support.transform.position) * filter.transform.localToWorldMatrix;
                int level = meshLevels.TryGetValue(renderer, out int selected) ? selected : -1;
                for (int lod = 0; lod < 3; lod++)
                {
                    if (level >= 0 && level != lod) continue;
                    // Flat scraps keep their near silhouette but disappear in the distant batch.
                    if (level < 0 && lod == 2 && renderer.bounds.size.y < .05f) continue;
                    for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                    {
                        if (renderer.sharedMaterials.Length == 0) continue;
                        Material material = renderer.sharedMaterials[Mathf.Min(submesh, renderer.sharedMaterials.Length - 1)];
                        if (material == null) continue;
                        if (!cell.Levels[lod].TryGetValue(material, out List<CombineInstance> instances))
                            cell.Levels[lod][material] = instances = new List<CombineInstance>();
                        instances.Add(new CombineInstance { mesh = mesh, subMeshIndex = submesh, transform = toCell });
                    }
                }
                // The light-weight source holder retains collider, label and section references.
                Object.DestroyImmediate(renderer);
                Object.DestroyImmediate(filter);
            }
            foreach (LODGroup group in sourceGroups) Object.DestroyImmediate(group);
            Transform batches = GreyboxFactory.Group("SectionRenderBatches", root);
            foreach (Cell cell in cells.Values.OrderBy(c => c.Name, System.StringComparer.Ordinal))
            {
                Transform holder = GreyboxFactory.Group(cell.Name, batches);
                holder.position = cell.Support.transform.position;
                holder.gameObject.AddComponent<SectionProp>().EditorSetup(cell.Support);
                holder.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Render-only stock batch; source holders retain fitted collision.");
                var lods = new LOD[3];
                for (int lod = 0; lod < 3; lod++)
                {
                    Transform level = GreyboxFactory.Group("LOD" + lod, holder);
                    var renderers = new List<Renderer>();
                    foreach (var pair in cell.Levels[lod].OrderBy(p => p.Key.name, System.StringComparer.Ordinal))
                    {
                        string name = cell.Name + "_LOD" + lod + "_" + pair.Key.name;
                        var combined = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
                        combined.CombineMeshes(pair.Value.ToArray(), true, true);
                        combined.RecalculateBounds();
                        string path = meshFolder + "/" + name + ".asset";
                        Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if (saved != null)
                        {
                            EditorUtility.CopySerialized(combined, saved);
                            Object.DestroyImmediate(combined);
                            EditorUtility.SetDirty(saved);
                        }
                        else { saved = combined; AssetDatabase.CreateAsset(saved, path); }
                        Transform draw = GreyboxFactory.Group(pair.Key.name, level);
                        draw.gameObject.AddComponent<MeshFilter>().sharedMesh = saved;
                        MeshRenderer renderer = draw.gameObject.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = pair.Key;
                        renderers.Add(renderer);
                    }
                    lods[lod] = new LOD(lod == 0 ? .32f : lod == 1 ? .12f : .025f, renderers.ToArray());
                }
                var group = holder.gameObject.AddComponent<LODGroup>();
                group.SetLODs(lods);
                group.RecalculateBounds();
            }
            return cells.Count;
        }
    }
}
