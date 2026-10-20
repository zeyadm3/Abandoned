using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using A = Abandoned.EditorTools.BurleySchoolArchitecture;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Persistent planar track meshes and open diamond wirework, rather than solid fence sheets.</summary>
    public static class BurleySchoolAthleticsGeometry
    {
        private const string MeshFolder = BurleySchoolArt.Folder + "/Meshes";

        public static void StadiumBand(Transform parent, string name, Vector3 at, float innerRadius,
            float outerRadius, float straightLength, Material material)
        {
            string key = $"Stadium_{Mathf.RoundToInt(innerRadius * 100f)}_{Mathf.RoundToInt(outerRadius * 100f)}_{Mathf.RoundToInt(straightLength * 100f)}";
            PolishAssets.EnsureFolder(MeshFolder);
            string path = MeshFolder + "/" + key + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                const int halfSegments = 40;
                const int count = (halfSegments + 1) * 2;
                var vertices = new Vector3[count * 2];
                var uv = new Vector2[count * 2];
                var triangles = new int[count * 6];
                for (int half = 0; half < 2; half++)
                for (int i = 0; i <= halfSegments; i++)
                {
                    int index = half * (halfSegments + 1) + i;
                    float angle = (-90f + half * 180f + i * 180f / halfSegments) * Mathf.Deg2Rad;
                    Vector3 radial = new(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                    Vector3 centre = Vector3.forward * ((half == 0 ? 1f : -1f) * straightLength * 0.5f);
                    vertices[index * 2] = centre + radial * innerRadius;
                    vertices[index * 2 + 1] = centre + radial * outerRadius;
                    uv[index * 2] = new Vector2(vertices[index * 2].x, vertices[index * 2].z) * 0.2f;
                    uv[index * 2 + 1] = new Vector2(vertices[index * 2 + 1].x, vertices[index * 2 + 1].z) * 0.2f;
                }
                for (int i = 0; i < count; i++)
                {
                    int n = (i + 1) % count;
                    int t = i * 6;
                    triangles[t] = i * 2;
                    triangles[t + 1] = i * 2 + 1;
                    triangles[t + 2] = n * 2 + 1;
                    triangles[t + 3] = i * 2;
                    triangles[t + 4] = n * 2 + 1;
                    triangles[t + 5] = n * 2;
                }
                mesh = new Mesh { name = key, vertices = vertices, uv = uv, triangles = triangles };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, path);
            }
            Transform band = Group(name, parent);
            band.localPosition = at;
            band.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            band.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        public static void FenceRun(Transform parent, string name, Vector3 start, Vector3 end, float height, BurleySchoolArt art)
        {
            Vector3 direction = (end - start).normalized;
            float length = Vector3.Distance(start, end);
            Transform run = Group(name, parent);
            run.localPosition = start;
            run.localRotation = Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up), Vector3.up);
            string key = $"Wire_{Mathf.RoundToInt(length * 100f)}_{Mathf.RoundToInt(height * 100f)}";
            PolishAssets.EnsureFolder(MeshFolder);
            string path = MeshFolder + "/" + key + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var triangles = new List<int>();
                for (float offset = -length; offset <= length + height; offset += 0.28f)
                {
                    float a = Mathf.Max(0f, -offset);
                    float b = Mathf.Min(length, height - offset);
                    if (b > a) AddWire(vertices, normals, triangles, new Vector2(a, a + offset), new Vector2(b, b + offset));
                    a = Mathf.Max(0f, offset - height);
                    b = Mathf.Min(length, offset);
                    if (b > a) AddWire(vertices, normals, triangles, new Vector2(a, offset - a), new Vector2(b, offset - b));
                }
                mesh = new Mesh { name = key };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, path);
            }
            Transform panel = Group("DiamondWire", run);
            panel.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            panel.gameObject.AddComponent<MeshRenderer>().sharedMaterial = art.FenceChainlink;
            BoxCollider collider = panel.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(length * 0.5f, height * 0.5f, 0f);
            collider.size = new Vector3(length, height, 0.05f);
            int posts = Mathf.Max(1, Mathf.CeilToInt(length / 3f));
            for (int i = 0; i <= posts; i++)
            {
                float x = length * i / posts;
                A.Cylinder(run, $"Post_{i}", new Vector3(x, height * 0.5f, 0f), 0.055f, height + 0.1f, art.MetalSteel, true);
                A.Cylinder(run, $"SnowCap_{i}", new Vector3(x, height + 0.075f, 0f), 0.065f, 0.08f, art.Snow);
            }
            A.Rod(run, "TopRail", new Vector3(0f, height - 0.08f, 0f), new Vector3(length, height - 0.08f, 0f), 0.035f, art.MetalSteel);
            A.Rod(run, "BottomRail", new Vector3(0f, 0.1f, 0f), new Vector3(length, 0.1f, 0f), 0.025f, art.MetalSteel);
        }

        private static void AddWire(List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector2 a, Vector2 b)
        {
            Vector2 d = (b - a).normalized;
            Vector2 n = new Vector2(-d.y, d.x) * 0.006f;
            Vector2[] corners = { a - n, b - n, b + n, a + n };
            int first = vertices.Count;
            for (int face = 0; face < 2; face++)
            for (int i = 0; i < 4; i++)
            {
                vertices.Add(new Vector3(corners[i].x, corners[i].y, 0f));
                normals.Add(face == 0 ? Vector3.forward : Vector3.back);
            }
            triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3,
                first + 4, first + 6, first + 5, first + 4, first + 7, first + 6 });
        }
    }
}
