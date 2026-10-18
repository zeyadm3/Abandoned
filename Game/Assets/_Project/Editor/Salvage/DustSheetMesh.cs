using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A canvas dust sheet draped over a box of the given size: rounded shoulders, a skirt that flares and
    /// folds toward the floor. Pivot at the bottom centre. One saved mesh per size, in 2 cm steps.
    /// </summary>
    public static class DustSheetMesh
    {
        private const int Around = 32;
        private static readonly float[] Rings = { 0f, .16f, .5f, .84f, .97f };

        public static Mesh For(Vector3 size, string folder)
        {
            Vector3Int cm = Vector3Int.RoundToInt(size * 50f) * 2;
            cm = Vector3Int.Max(cm, new Vector3Int(10, 10, 10));
            string path = $"{folder}/DustSheet_{cm.x}x{cm.y}x{cm.z}.asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved != null) return saved;
            PolishAssets.EnsureFolder(folder);
            Mesh mesh = Build((Vector3)cm / 100f, cm.x * 73856093 ^ cm.y * 19349663 ^ cm.z * 83492791);
            mesh.name = System.IO.Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        public static Mesh Build(Vector3 size, int seed)
        {
            var random = new System.Random(seed);
            float phaseA = (float)random.NextDouble() * 6.28f, phaseB = (float)random.NextDouble() * 6.28f;
            float flare = Mathf.Min(.09f, .25f * Mathf.Min(size.x, size.z));
            float fold = Mathf.Min(.03f, .08f * Mathf.Min(size.x, size.z));
            float perimeter = 2f * (size.x + size.z);
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();

            for (int r = 0; r < Rings.Length; r++)
            {
                float t = Rings[r], below = 1f - t;
                // The cloth hangs straight from the shoulder and spreads where it meets the floor.
                float shoulder = r == Rings.Length - 1 ? .035f : 0f;
                float hx = size.x * .5f + flare * below * below - shoulder;
                float hz = size.z * .5f + flare * below * below - shoulder;
                for (int i = 0; i <= Around; i++)
                {
                    float angle = i * Mathf.PI * 2f / Around;
                    Vector2 edge = Superellipse(angle, hx, hz);
                    float wave = (Mathf.Sin(angle * 7f + phaseA) + Mathf.Sin(angle * 13f + phaseB)) * .5f;
                    float push = 1f + wave * fold * below / Mathf.Max(.05f, Mathf.Max(hx, hz));
                    vertices.Add(new Vector3(edge.x * push, t * size.y, edge.y * push));
                    uv.Add(new Vector2((float)i / Around * perimeter, t * size.y));
                }
            }
            int stride = Around + 1;
            for (int r = 0; r < Rings.Length - 1; r++)
                for (int i = 0; i < Around; i++)
                {
                    int a = r * stride + i, b = a + 1, c = a + stride, d = c + 1;
                    // Clockwise seen from outside, so the faces point out.
                    triangles.AddRange(new[] { a, c, d, a, d, b });
                }

            int top = (Rings.Length - 1) * stride, centre = vertices.Count;
            vertices.Add(new Vector3(0, size.y + .01f, 0));
            uv.Add(Vector2.zero);
            int capStart = vertices.Count;
            for (int i = 0; i <= Around; i++)
            {
                Vector3 rim = vertices[top + i];
                vertices.Add(rim);
                uv.Add(new Vector2(rim.x, rim.z));
            }
            for (int i = 0; i < Around; i++) triangles.AddRange(new[] { centre, capStart + i + 1, capStart + i });

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector2 Superellipse(float angle, float hx, float hz)
        {
            // Exponent 1/3 keeps the corners of the object underneath without sharp cloth edges.
            float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
            return new Vector2(hx * Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), 1f / 3f),
                hz * Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), 1f / 3f));
        }
    }
}
