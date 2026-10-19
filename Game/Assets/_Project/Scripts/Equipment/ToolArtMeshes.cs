using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Reusable formed shells and turned-metal shapes with broad faces and narrow bevel highlights.</summary>
    public static class ToolArtMeshes
    {
        private static Mesh box, cylinder, ring;
        public static Mesh Box => box != null ? box : box = BevelBox();
        public static Mesh Cylinder => cylinder != null ? cylinder : cylinder = TurnedCylinder();
        public static Mesh Ring => ring != null ? ring : ring = Washer();

        private static Mesh BevelBox()
        {
            const float inner = .42f, outer = .5f;
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            void Face(params Vector3[] points)
            {
                Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;
                Vector3 centre = Vector3.zero;
                foreach (Vector3 point in points) centre += point;
                if (Vector3.Dot(normal, centre) < 0f) { System.Array.Reverse(points); normal = -normal; }
                int start = vertices.Count;
                foreach (Vector3 point in points)
                {
                    vertices.Add(point); normals.Add(normal);
                    Vector3 abs = new(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
                    uv.Add(abs.x > abs.y && abs.x > abs.z ? new Vector2(point.z + .5f, point.y + .5f)
                        : abs.y > abs.z ? new Vector2(point.x + .5f, point.z + .5f) : new Vector2(point.x + .5f, point.y + .5f));
                }
                for (int i = 1; i < points.Length - 1; i++) triangles.AddRange(new[] { start, start + i, start + i + 1 });
            }
            foreach (float sign in new[] { -1f, 1f })
            {
                Face(new(sign * outer, -inner, -inner), new(sign * outer, inner, -inner), new(sign * outer, inner, inner), new(sign * outer, -inner, inner));
                Face(new(-inner, sign * outer, -inner), new(inner, sign * outer, -inner), new(inner, sign * outer, inner), new(-inner, sign * outer, inner));
                Face(new(-inner, -inner, sign * outer), new(inner, -inner, sign * outer), new(inner, inner, sign * outer), new(-inner, inner, sign * outer));
            }
            foreach (float a in new[] { -1f, 1f })
            foreach (float b in new[] { -1f, 1f })
            {
                Face(new(a * outer, b * inner, -inner), new(a * inner, b * outer, -inner), new(a * inner, b * outer, inner), new(a * outer, b * inner, inner));
                Face(new(a * outer, -inner, b * inner), new(a * inner, -inner, b * outer), new(a * inner, inner, b * outer), new(a * outer, inner, b * inner));
                Face(new(-inner, a * outer, b * inner), new(-inner, a * inner, b * outer), new(inner, a * inner, b * outer), new(inner, a * outer, b * inner));
                foreach (float c in new[] { -1f, 1f })
                    Face(new(a * outer, b * inner, c * inner), new(a * inner, b * outer, c * inner), new(a * inner, b * inner, c * outer));
            }
            return Create("Equipment_BevelBox", vertices, normals, uv, triangles);
        }

        private static Mesh TurnedCylinder()
        {
            const int segments = 24;
            float[] heights = { -.5f, -.43f, .43f, .5f }, radii = { .43f, .5f, .5f, .43f };
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            for (int ringIndex = 0; ringIndex < heights.Length; ringIndex++)
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 radial = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices.Add(radial * radii[ringIndex] + Vector3.up * heights[ringIndex]);
                normals.Add((radial + Vector3.up * (ringIndex == 0 ? -.7f : ringIndex == 3 ? .7f : 0f)).normalized);
                uv.Add(new Vector2(i / (float)segments, heights[ringIndex] + .5f));
                if (ringIndex == 0 || i == segments) continue;
                int a = (ringIndex - 1) * (segments + 1) + i, b = a + 1, c = ringIndex * (segments + 1) + i, d = c + 1;
                triangles.AddRange(new[] { a, c, b, b, c, d });
            }
            for (int end = 0; end < 2; end++)
            {
                float y = end == 0 ? -.5f : .5f;
                int centre = vertices.Count;
                vertices.Add(Vector3.up * y); normals.Add(end == 0 ? Vector3.down : Vector3.up); uv.Add(Vector2.one * .5f);
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    Vector3 radial = new(Mathf.Cos(angle) * .43f, 0f, Mathf.Sin(angle) * .43f);
                    vertices.Add(radial + Vector3.up * y); normals.Add(end == 0 ? Vector3.down : Vector3.up); uv.Add(new Vector2(radial.x + .5f, radial.z + .5f));
                    if (i == segments) continue;
                    triangles.AddRange(end == 0 ? new[] { centre, centre + i + 1, centre + i + 2 } : new[] { centre, centre + i + 2, centre + i + 1 });
                }
            }
            return Create("Equipment_TurnedCylinder", vertices, normals, uv, triangles);
        }

        private static Mesh Washer()
        {
            const int segments = 24;
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            foreach (int strip in new[] { 0, 1, 2, 3 })
            {
                int start = vertices.Count;
                for (int i = 0; i <= segments; i++)
                for (int edge = 0; edge < 2; edge++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float radius = strip < 2 ? edge == 0 ? .34f : .5f : strip == 2 ? .5f : .34f;
                    float y = strip < 2 ? strip == 0 ? -.5f : .5f : edge == 0 ? -.5f : .5f;
                    Vector3 radial = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    vertices.Add(radial * radius + Vector3.up * y);
                    normals.Add(strip == 0 ? Vector3.down : strip == 1 ? Vector3.up : strip == 2 ? radial : -radial);
                    uv.Add(new Vector2(i / (float)segments, edge));
                    if (i == segments || edge != 0) continue;
                    int a = start + i * 2, b = a + 1, c = a + 2, d = a + 3;
                    bool reverse = strip is 0 or 2;
                    triangles.AddRange(reverse ? new[] { a, b, c, b, d, c } : new[] { a, c, b, b, c, d });
                }
            }
            return Create("Equipment_BoredRing", vertices, normals, uv, triangles);
        }

        private static Mesh Create(string name, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv, List<int> triangles)
        {
            var mesh = new Mesh { name = name, hideFlags = HideFlags.DontSave };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents(); mesh.RecalculateBounds();
            return mesh;
        }

        public static void Reset()
        {
            foreach (Mesh mesh in new[] { box, cylinder, ring })
                if (mesh != null)
                {
                    if (Application.isPlaying) Object.Destroy(mesh);
                    else Object.DestroyImmediate(mesh);
                }
            box = cylinder = ring = null;
        }
    }
}
