using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Metre-scale UVs for prison cubes, including the thin parts inside a merged fitting.</summary>
    public static class IslandPrisonSurfaces
    {
        private const string Folder = IslandPrisonArt.Folder + "/SurfaceMeshes";
        private static readonly Dictionary<string, Mesh> Cache = new();

        public static void Reset() => Cache.Clear();

        public static int Apply(Transform root, IReadOnlyDictionary<Material, Vector2> tiling)
        {
            if (tiling == null) return 0;
            PolishAssets.EnsureFolder(Folder);
            int changed = 0;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null || (mesh.name != "Cube" && mesh.name != "Cylinder") ||
                    AssetDatabase.GetAssetPath(mesh) != "Library/unity default resources") continue;
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial == null || !tiling.TryGetValue(renderer.sharedMaterial, out Vector2 repeat)) continue;
                Vector3 scale = filter.transform.lossyScale;
                var size = new Vector3(Precision(scale.x), Precision(scale.y), Precision(scale.z));
                filter.sharedMesh = mesh.name == "Cylinder" ? Cylinder(mesh, size, repeat) : Box(size, repeat);
                changed++;
            }
            return changed;
        }

        // Ten-centimetre rounding turns a 2 cm bar into zero-width UVs and degenerate normal-map tangents.
        private static float Precision(float value) => Mathf.Max(.001f, Mathf.Round(Mathf.Abs(value) * 1000f) / 1000f);

        private static Mesh Box(Vector3 size, Vector2 repeat)
        {
            string key = string.Format(CultureInfo.InvariantCulture, "PrisonUV_{0:0.000}_{1:0.000}_{2:0.000}_{3:0.000}_{4:0.000}",
                size.x, size.y, size.z, repeat.x, repeat.y);
            if (Cache.TryGetValue(key, out Mesh existing) && existing != null) return existing;
            string path = $"{Folder}/{key}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = Build(size, repeat);
                mesh.name = key;
                AssetDatabase.CreateAsset(mesh, path);
            }
            Cache[key] = mesh;
            return mesh;
        }

        private static Mesh Build(Vector3 size, Vector2 repeat)
        {
            var vertices = new List<Vector3>(24);
            var normals = new List<Vector3>(24);
            var uv = new List<Vector2>(24);
            var triangles = new List<int>(36);
            void Face(Vector3 normal, Vector3 across, Vector3 up, float width, float height)
            {
                int first = vertices.Count;
                for (int i = 0; i < 4; i++)
                {
                    float x = i == 0 || i == 3 ? -.5f : .5f, y = i < 2 ? -.5f : .5f;
                    vertices.Add(normal * .5f + across * x + up * y);
                    normals.Add(normal);
                    uv.Add(new Vector2((x + .5f) * width / repeat.x, (y + .5f) * height / repeat.y));
                }
                bool forward = Vector3.Dot(Vector3.Cross(across, up), normal) > 0f;
                triangles.AddRange(forward ? new[] { first, first + 1, first + 2, first, first + 2, first + 3 }
                    : new[] { first, first + 2, first + 1, first, first + 3, first + 2 });
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
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh Cylinder(Mesh source, Vector3 scale, Vector2 repeat)
        {
            string key = string.Format(CultureInfo.InvariantCulture, "PrisonPipeUV_{0:0.000}_{1:0.000}_{2:0.000}_{3:0.000}_{4:0.000}",
                scale.x, scale.y, scale.z, repeat.x, repeat.y);
            if (Cache.TryGetValue(key, out Mesh existing) && existing != null) return existing;
            string path = $"{Folder}/{key}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = Object.Instantiate(source);
                mesh.name = key;
                Vector2[] uv = mesh.uv;
                Vector3[] normals = mesh.normals;
                float circumference = Mathf.PI * (scale.x + scale.z) * .5f;
                for (int i = 0; i < uv.Length; i++)
                {
                    // Unity's cylinder is two metres high before scaling; its caps use their own planar islands.
                    uv[i] = Mathf.Abs(normals[i].y) < .5f
                        ? Vector2.Scale(uv[i], new Vector2(circumference / repeat.x, scale.y * 2f / repeat.y))
                        : Vector2.Scale(uv[i], new Vector2(scale.x / repeat.x, scale.z / repeat.y));
                }
                mesh.uv = uv;
                mesh.RecalculateTangents();
                AssetDatabase.CreateAsset(mesh, path);
            }
            Cache[key] = mesh;
            return mesh;
        }
    }
}
