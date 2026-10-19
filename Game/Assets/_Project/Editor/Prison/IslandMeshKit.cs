using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Repeated fittings (cell bars, bunks, toilets, tables, carts) are modelled once from boxes, merged into one
    /// mesh per material and saved under Art/Prison/Meshes, then placed as plain renderers. Two hundred cells then
    /// cost a few shared meshes instead of thousands of objects. Colliders are never part of a kit: the caller adds
    /// the few boxes that matter (a bunk, a toilet, the bars of a shut door).
    /// </summary>
    public static class IslandMeshKit
    {
        private const string Folder = IslandPrisonArt.Folder + "/Meshes";
        private static readonly Dictionary<string, List<(Mesh mesh, Material material)>> Cache = new();
        private static IReadOnlyDictionary<Material, Vector2> surfaceTiling;

        /// <summary>Clears the per-build cache (meshes are rebuilt in place each rebuild, keeping their GUIDs).</summary>
        public static void Reset()
        {
            Cache.Clear();
            surfaceTiling = null;
            IslandPrisonSurfaces.Reset();
        }

        public static void ConfigureSurfaces(IReadOnlyDictionary<Material, Vector2> tiling) => surfaceTiling = tiling;

        /// <summary>The kit called <paramref name="name"/>, built by <paramref name="model"/> the first time it is asked
        /// for in this build (boxes under the given root, in the kit's own space).</summary>
        public static List<(Mesh mesh, Material material)> Get(string name, Action<Transform> model)
        {
            if (Cache.TryGetValue(name, out List<(Mesh, Material)> kit)) return kit;
            PolishAssets.EnsureFolder(Folder);
            var root = new GameObject("Kit_" + name);
            kit = new List<(Mesh, Material)>();
            try
            {
                model(root.transform);
                // Once parts are merged the whole-scene cube pass cannot see their individual dimensions.
                IslandPrisonSurfaces.Apply(root.transform, surfaceTiling);
                var groups = new Dictionary<Material, List<CombineInstance>>();
                foreach (MeshFilter part in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    Material m = part.GetComponent<Renderer>().sharedMaterial;
                    if (m == null || part.sharedMesh == null) continue;
                    if (!groups.TryGetValue(m, out List<CombineInstance> list)) groups[m] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = part.sharedMesh, transform = root.transform.worldToLocalMatrix * part.transform.localToWorldMatrix });
                }
                foreach (KeyValuePair<Material, List<CombineInstance>> pair in groups)
                {
                    string path = $"{Folder}/{name}_{pair.Key.name}.asset";
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (mesh == null) { mesh = new Mesh { name = name + "_" + pair.Key.name }; AssetDatabase.CreateAsset(mesh, path); }
                    else mesh.Clear();
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                    mesh.RecalculateTangents();
                    mesh.RecalculateBounds();
                    EditorUtility.SetDirty(mesh);
                    kit.Add((mesh, pair.Key));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            Cache[name] = kit;
            return kit;
        }

        /// <summary>Places a kit: one child renderer per material under a new holder at the given pose.</summary>
        public static Transform Place(List<(Mesh mesh, Material material)> kit, string name, Transform parent, Vector3 position,
            Quaternion rotation, bool castShadows = true)
        {
            var holder = new GameObject(name).transform;
            holder.SetParent(parent, false);
            holder.SetPositionAndRotation(position, rotation);
            foreach ((Mesh mesh, Material material) in kit)
            {
                var part = new GameObject(material.name);
                part.transform.SetParent(holder, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = part.AddComponent<MeshRenderer>();
                r.sharedMaterial = material;
                if (!castShadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return holder;
        }

        /// <summary>A box collider in a holder's own space (centre and size).</summary>
        public static BoxCollider Solid(Transform holder, Vector3 centre, Vector3 size)
        {
            var c = holder.gameObject.AddComponent<BoxCollider>();
            c.center = centre;
            c.size = size;
            return c;
        }
    }
}
