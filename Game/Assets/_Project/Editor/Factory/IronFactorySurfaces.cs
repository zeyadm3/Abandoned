using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// World-scale UVs for the Iron Factory's box geometry. The builders make everything from scaled cubes, whose
    /// UVs run 0-1 on every face whatever its size: a 120 m wall would smear one texture across its whole length
    /// (the "blank box" look). This pass swaps each tiled cube for a cube mesh whose UVs are its real size in
    /// metres over the material's repeat, so brick is brick-sized everywhere. The meshes are shared assets, one
    /// per size (rounded to 10 cm), so the scene stays small. Colliders are untouched.
    /// </summary>
    public static class IronFactorySurfaces
    {
        private const string Folder = "Assets/_Project/Art/Factory/UvBoxes";
        // Below this (every side), a part keeps the plain cube: the stretch doesn't show, and it saves meshes.
        private const float MinSize = .9f;

        public static int Apply(Transform root, Dictionary<Material, Vector2> tiling)
        {
            PolishAssets.EnsureFolder(Folder);
            var cache = new Dictionary<string, Mesh>();
            int swapped = 0;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null || mesh.name != "Cube" || AssetDatabase.GetAssetPath(mesh) != "Library/unity default resources") continue;
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial == null || !tiling.TryGetValue(renderer.sharedMaterial, out Vector2 repeat)) continue;
                Vector3 s = Abs(filter.transform.lossyScale);
                if (s.x < MinSize && s.y < MinSize && s.z < MinSize) continue;
                filter.sharedMesh = Box(cache, Round(s), repeat);
                swapped++;
            }
            AssetDatabase.SaveAssets();
            return swapped;
        }

        private static Mesh Box(Dictionary<string, Mesh> cache, Vector3 size, Vector2 repeat)
        {
            string key = $"UvBox_{size.x:0.0}_{size.y:0.0}_{size.z:0.0}_{repeat.x:0.00}_{repeat.y:0.00}".Replace(',', '.');
            if (cache.TryGetValue(key, out Mesh cached)) return cached;
            string path = $"{Folder}/{key}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = Build(size, repeat);
                mesh.name = key;
                AssetDatabase.CreateAsset(mesh, path);
            }
            cache[key] = mesh;
            return mesh;
        }

        // A unit cube (like Unity's) whose UVs are face size / repeat: u along the face's width, v up (or along z
        // on the top and bottom).
        private static Mesh Build(Vector3 size, Vector2 repeat)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            void Face(Vector3 normal, Vector3 uAxis, Vector3 vAxis, float uSize, float vSize)
            {
                int start = vertices.Count;
                Vector3 centre = normal * .5f;
                for (int i = 0; i < 4; i++)
                {
                    float a = i == 0 || i == 3 ? -.5f : .5f, b = i < 2 ? -.5f : .5f;
                    vertices.Add(centre + uAxis * a + vAxis * b);
                    normals.Add(normal);
                    uvs.Add(new Vector2((a + .5f) * uSize / repeat.x, (b + .5f) * vSize / repeat.y));
                }
                // Unity draws clockwise triangles as front faces: corners 0-1-2 run clockwise seen from outside
                // when u x v points along the normal.
                bool aligned = Vector3.Dot(Vector3.Cross(uAxis, vAxis), normal) > 0f;
                triangles.AddRange(aligned ? new[] { start, start + 1, start + 2, start, start + 2, start + 3 }
                                           : new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            }
            Face(Vector3.right, Vector3.back, Vector3.up, size.z, size.y);
            Face(Vector3.left, Vector3.forward, Vector3.up, size.z, size.y);
            Face(Vector3.forward, Vector3.right, Vector3.up, size.x, size.y);
            Face(Vector3.back, Vector3.left, Vector3.up, size.x, size.y);
            Face(Vector3.up, Vector3.right, Vector3.forward, size.x, size.z);
            Face(Vector3.down, Vector3.right, Vector3.back, size.x, size.z);
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        private static Vector3 Round(Vector3 v) => new(Mathf.Round(v.x * 10f) / 10f, Mathf.Round(v.y * 10f) / 10f, Mathf.Round(v.z * 10f) / 10f);
    }
}
