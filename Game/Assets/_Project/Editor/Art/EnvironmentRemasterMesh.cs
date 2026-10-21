using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Geometry is authored in world metres, then saved in a handful of spatial chunks for culling.</summary>
    public sealed class EnvironmentRemasterMesh
    {
        private readonly List<Vector3> vertices = new();
        private readonly List<Vector3> normals = new();
        private readonly List<Vector2> uv = new();
        private readonly List<int> triangles = new();
        public int Triangles => triangles.Count / 3;

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            int at = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            for (int i = 0; i < 4; i++) normals.Add(normal);
            uv.Add(Vector2.zero); uv.Add(new Vector2(Vector3.Distance(a, b), 0f));
            uv.Add(new Vector2(Vector3.Distance(a, b), Vector3.Distance(b, c))); uv.Add(new Vector2(0f, Vector3.Distance(b, c)));
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) >= 0f)
            { triangles.Add(at); triangles.Add(at + 1); triangles.Add(at + 2); triangles.Add(at); triangles.Add(at + 2); triangles.Add(at + 3); }
            else
            { triangles.Add(at); triangles.Add(at + 2); triangles.Add(at + 1); triangles.Add(at); triangles.Add(at + 3); triangles.Add(at + 2); }
        }

        public void Ribbon(Vector3 start, Vector3 end, Vector3 outward, float bottom, float top, float depth)
        {
            Vector3 a = start + outward * depth + Vector3.up * bottom;
            Vector3 b = end + outward * depth + Vector3.up * bottom;
            Quad(a, b, b + Vector3.up * (top - bottom), a + Vector3.up * (top - bottom), outward);
        }

        public void Skirting(Vector3 start, Vector3 end, Vector3 outward, float height)
        {
            // Sloped cap catches a narrow highlight; face and top have true geometry instead of a painted line.
            var profile = new[] { new Vector2(.002f, .003f), new Vector2(.02f, .003f), new Vector2(.02f, height - .023f),
                new Vector2(.009f, height), new Vector2(.002f, height) };
            for (int i = 0; i < profile.Length; i++)
            {
                Vector2 p = profile[i], q = profile[(i + 1) % profile.Length];
                Vector3 a = start + outward * p.x + Vector3.up * p.y;
                Vector3 b = end + outward * p.x + Vector3.up * p.y;
                Vector3 c = end + outward * q.x + Vector3.up * q.y;
                Vector3 d = start + outward * q.x + Vector3.up * q.y;
                Vector3 normal = (outward * (q.y - p.y) - Vector3.up * (q.x - p.x)).normalized;
                Quad(a, b, c, d, normal);
            }
            Vector2 centre = Vector2.zero;
            foreach (Vector2 p in profile) centre += p / profile.Length;
            Vector3 tangent = (end - start).normalized;
            foreach (bool last in new[] { false, true })
            {
                Vector3 at = last ? end : start, normal = last ? tangent : -tangent;
                for (int i = 0; i < profile.Length; i++)
                {
                    Vector2 p = profile[i], q = profile[(i + 1) % profile.Length];
                    Triangle(at + outward * centre.x + Vector3.up * centre.y,
                        at + outward * p.x + Vector3.up * p.y, at + outward * q.x + Vector3.up * q.y, normal);
                }
            }
        }

        private void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            int at = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            normals.Add(normal); normals.Add(normal); normals.Add(normal);
            uv.Add(Vector2.zero); uv.Add(new Vector2(Vector3.Distance(a, b), 0f)); uv.Add(new Vector2(0f, Vector3.Distance(a, c)));
            triangles.Add(at);
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) >= 0f) { triangles.Add(at + 1); triangles.Add(at + 2); }
            else { triangles.Add(at + 2); triangles.Add(at + 1); }
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name, indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        public Mesh Save(string path, string name)
        {
            Mesh mesh = Build(name);
            Mesh old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (old == null) { AssetDatabase.CreateAsset(mesh, path); return mesh; }
            EditorUtility.CopySerialized(mesh, old);
            EditorUtility.SetDirty(old);
            Object.DestroyImmediate(mesh);
            return old;
        }
    }
}
