using System.Collections.Generic;
using Abandoned.World;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A far copy of each built place for the world scene: its largest renderers (shells, roofs, yards, neighbouring
    /// buildings) merged per material into a few saved meshes, no colliders. It stands on the site while the place's
    /// own scene isn't loaded, so the places are on the map in the editor and on the horizon in game; the real scene
    /// replaces it as it streams in (<see cref="WorldRoot.ShowStandIn"/>). Baked while <see cref="WorldSurvey"/> has
    /// each scene open; the scenes are never changed.
    /// </summary>
    public static class WorldProxies
    {
        public const string Folder = "Assets/_Project/Art/Generated/World/Proxies";
        // Biggest first until this many triangles: the outside reads from afar, the fittings inside don't count.
        private const int TriangleBudget = 90000;
        private const float SmallestPiece = 2f;
        // Hidden while its scene is loaded anyway; this only drops a place too far away to cover a pixel.
        private const float CullScreenHeight = .004f;

        private static readonly Dictionary<string, List<(Mesh mesh, Material material)>> baked = new();

        public static void Clear()
        {
            baked.Clear();
            if (AssetDatabase.IsValidFolder(Folder)) AssetDatabase.DeleteAsset(Folder);
            PolishAssets.EnsureFolder(Folder);
        }

        /// <summary>Bakes the open scene's far copy (scene coordinates; placed at the site later).</summary>
        public static void Bake(Scene scene, WorldPoi p)
        {
            var hidden = new List<Transform>();
            foreach (string path in p.hideInWorld)
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    int slash = path.IndexOf('/');
                    if (root.name != (slash < 0 ? path : path.Substring(0, slash))) continue;
                    Transform t = slash < 0 ? root.transform : root.transform.Find(path.Substring(slash + 1));
                    if (t != null) hidden.Add(t);
                }
            Rect site = p.ground;
            var pieces = new List<MeshRenderer>();
            var lesserLods = new HashSet<Renderer>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (LODGroup group in root.GetComponentsInChildren<LODGroup>(true))
                {
                    LOD[] lods = group.GetLODs();
                    for (int i = 1; i < lods.Length; i++) foreach (Renderer r in lods[i].renderers) if (r != null) lesserLods.Add(r);
                }
                foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(false))
                {
                    if (!r.enabled || lesserLods.Contains(r) || r.GetComponentInParent<NetworkObject>(true) != null) continue;
                    if (!r.TryGetComponent(out MeshFilter f) || f.sharedMesh == null) continue;
                    Bounds b = r.bounds;
                    if (Mathf.Max(b.size.x, b.size.y, b.size.z) < SmallestPiece) continue;
                    // Far scenery reaching well past the site would stand in the world's woods (as WorldSurvey hides).
                    if (b.min.x < site.xMin - 60f || b.max.x > site.xMax + 60f || b.min.z < site.yMin - 60f || b.max.z > site.yMax + 60f) continue;
                    if (Under(r.transform, hidden)) continue;
                    pieces.Add(r);
                }
            }
            pieces.Sort((a, b) => b.bounds.size.sqrMagnitude.CompareTo(a.bounds.size.sqrMagnitude));

            var byMaterial = new Dictionary<Material, Builder>();
            int triangles = 0, used = 0;
            foreach (MeshRenderer r in pieces)
            {
                Mesh mesh = r.GetComponent<MeshFilter>().sharedMesh;
                Material[] materials = r.sharedMaterials;
                int count = 0;
                for (int s = 0; s < mesh.subMeshCount; s++) count += (int)mesh.GetIndexCount(s) / 3;
                if (triangles + count > TriangleBudget) continue;
                Matrix4x4 m = r.localToWorldMatrix;
                for (int s = 0; s < mesh.subMeshCount && s < materials.Length; s++)
                {
                    if (materials[s] == null) continue;
                    if (!byMaterial.TryGetValue(materials[s], out Builder builder)) byMaterial[materials[s]] = builder = new Builder();
                    builder.Add(mesh, s, m);
                }
                triangles += count;
                used++;
            }

            var result = new List<(Mesh, Material)>();
            string folder = $"{Folder}/{p.scene}";
            PolishAssets.EnsureFolder(folder);
            int n = 0;
            foreach (KeyValuePair<Material, Builder> pair in byMaterial)
            {
                Mesh mesh = pair.Value.Build($"{p.scene}_Far_{n}");
                AssetDatabase.CreateAsset(mesh, $"{folder}/{mesh.name}.asset");
                result.Add((mesh, Keep(pair.Key, folder)));
                n++;
            }
            baked[p.scene] = result;
            Debug.Log($"[WorldBuild] {p.scene}: far copy of {used} of {pieces.Count} pieces, {triangles:N0} triangles, {result.Count} materials.");
        }

        /// <summary>Puts a place's far copy on its site, under its stand-in.</summary>
        public static void Place(Transform standIn, WorldPoi p)
        {
            if (!baked.TryGetValue(p.scene, out List<(Mesh mesh, Material material)> parts) || parts.Count == 0) return;
            Transform far = GreyboxFactory.Group("FarCopy", standIn);
            far.position = p.worldPosition;
            var renderers = new List<Renderer>();
            foreach ((Mesh mesh, Material material) in parts)
            {
                var go = new GameObject(mesh.name);
                go.transform.SetParent(far, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterial = material;
                r.shadowCastingMode = ShadowCastingMode.Off;
                renderers.Add(r);
            }
            var lod = far.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(CullScreenHeight, renderers.ToArray()) });
            lod.RecalculateBounds();
        }

        private static bool Under(Transform t, List<Transform> roots)
        {
            foreach (Transform root in roots) if (t == root || t.IsChildOf(root)) return true;
            return false;
        }

        // A material made by a map's builder and saved inside its scene would vanish with the scene: keep a copy.
        private static Material Keep(Material material, string folder)
        {
            if (AssetDatabase.Contains(material)) return material;
            var copy = new Material(material) { name = material.name };
            foreach (string property in copy.GetTexturePropertyNames())
            {
                Texture texture = copy.GetTexture(property);
                if (texture != null && !AssetDatabase.Contains(texture)) copy.SetTexture(property, null);
            }
            AssetDatabase.CreateAsset(copy, AssetDatabase.GenerateUniqueAssetPath($"{folder}/{SafeName(material.name)}.mat"));
            return copy;
        }

        private static string SafeName(string name)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return string.IsNullOrEmpty(name) ? "Material" : name;
        }

        // Positions, normals and the first UVs only: what a building far away needs.
        private class Builder
        {
            private readonly List<Vector3> vertices = new(), normals = new();
            private readonly List<Vector2> uvs = new();
            private readonly List<int> triangles = new();
            private readonly List<Vector3> v = new(), nm = new();
            private readonly List<Vector2> uv = new();
            private readonly List<int> tri = new();
            private readonly Dictionary<int, int> remap = new();

            public void Add(Mesh mesh, int subMesh, Matrix4x4 m)
            {
                mesh.GetVertices(v);
                mesh.GetNormals(nm);
                mesh.GetUVs(0, uv);
                mesh.GetTriangles(tri, subMesh);
                Matrix4x4 normalMatrix = m.inverse.transpose;
                // Only the vertices this sub-mesh uses (a many-material mesh would otherwise be copied once per material).
                remap.Clear();
                foreach (int index in tri)
                {
                    if (remap.ContainsKey(index)) continue;
                    remap[index] = vertices.Count;
                    vertices.Add(m.MultiplyPoint3x4(v[index]));
                    normals.Add(index < nm.Count ? normalMatrix.MultiplyVector(nm[index]).normalized : Vector3.up);
                    uvs.Add(index < uv.Count ? uv[index] : Vector2.zero);
                }
                // A mirrored piece turns its faces inside out unless the winding is flipped too.
                bool mirrored = m.determinant < 0f;
                for (int i = 0; i < tri.Count; i += 3)
                {
                    triangles.Add(remap[tri[i]]);
                    triangles.Add(remap[mirrored ? tri[i + 2] : tri[i + 1]]);
                    triangles.Add(remap[mirrored ? tri[i + 1] : tri[i + 2]]);
                }
            }

            public Mesh Build(string name)
            {
                var mesh = new Mesh { name = name, indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
