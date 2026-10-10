using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Thin vehicle panels with real openings, rounded profiles and metre-scale UV coordinates.</summary>
    public static class CollectionTruckCabMesh
    {
        public static void Surface(CollectionTruckGeometry g, string name, string finish, int across, int along,
            Func<float, float, Vector3> point, bool reverse = false)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            for (int row = 0; row <= along; row++)
            for (int column = 0; column <= across; column++)
            {
                float u = column / (float)across, v = row / (float)along;
                Vector3 p = point(u, v); vertices.Add(p);
                uv.Add(new Vector2(Vector3.Distance(point(0, v), p), Vector3.Distance(point(u, 0), p)));
                if (column == across || row == along) continue;
                int a = row * (across + 1) + column, b = a + across + 1;
                if (reverse) triangles.AddRange(new[] { a, b + 1, b, a, a + 1, b + 1 });
                else triangles.AddRange(new[] { a, b, b + 1, a, b + 1, a + 1 });
            }
            Add(g, name, finish, vertices, uv, triangles);
        }

        public static void Side(CollectionTruckGeometry g, string name, Vector2[] yz, float x, float thickness, string finish)
        {
            var points = new Vector3[yz.Length];
            for (int i = 0; i < yz.Length; i++) points[i] = new Vector3(x, yz[i].x, yz[i].y);
            Polygon(g, name, points, yz, Vector3.right * thickness, finish);
        }

        public static void FrontRounded(CollectionTruckGeometry g, string name, Vector3 center,
            Vector3 size, float radius, string finish, int corners = 4)
        {
            var points = new List<Vector3>(); var xy = new List<Vector2>();
            float hx = size.x * .5f, hy = size.y * .5f;
            radius = Mathf.Min(radius, Mathf.Min(hx, hy));
            for (int corner = 0; corner < 4; corner++)
            {
                float angle = corner * Mathf.PI * .5f;
                float sx = corner == 0 || corner == 3 ? 1 : -1;
                float sy = corner < 2 ? 1 : -1;
                Vector2 origin = new Vector2(sx * (hx - radius), sy * (hy - radius));
                for (int segment = 0; segment <= corners; segment++)
                {
                    float a = angle + segment / (float)corners * Mathf.PI * .5f;
                    Vector2 p = origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    points.Add(center + new Vector3(p.x, p.y, -size.z * .5f)); xy.Add(p);
                }
            }
            Polygon(g, name, points.ToArray(), xy.ToArray(), Vector3.forward * size.z, finish);
        }

        public static void Quad(CollectionTruckGeometry g, string name, string finish,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool twoSided = false)
        {
            var points = new List<Vector3> { a, b, c, d };
            float width = Vector3.Distance(a, b), height = Vector3.Distance(b, c);
            var uv = new List<Vector2> { Vector2.zero, new Vector2(width, 0), new Vector2(width, height), new Vector2(0, height) };
            var triangles = new List<int> { 0, 1, 2, 0, 2, 3 };
            if (twoSided)
            {
                points.AddRange(new[] { a, b, c, d }); uv.AddRange(uv.ToArray());
                triangles.AddRange(new[] { 4, 6, 5, 4, 7, 6 });
            }
            Add(g, name, finish, points, uv, triangles);
        }

        public static void Ring(CollectionTruckGeometry g, string name, string finish, Vector3[] outer, Vector3[] inner)
        {
            var points = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            float distance = 0;
            for (int i = 0; i < outer.Length; i++)
            {
                int next = (i + 1) % outer.Length, v = points.Count;
                float length = Vector3.Distance(outer[i], outer[next]);
                points.AddRange(new[] { outer[i], inner[i], inner[next], outer[next] });
                uv.AddRange(new[] { new Vector2(distance, 0), new Vector2(distance, .06f),
                    new Vector2(distance + length, .06f), new Vector2(distance + length, 0) });
                triangles.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3,
                    v + 4, v + 6, v + 5, v + 4, v + 7, v + 6 });
                points.AddRange(new[] { outer[i], inner[i], inner[next], outer[next] });
                uv.AddRange(new[] { new Vector2(distance, 0), new Vector2(distance, .06f),
                    new Vector2(distance + length, .06f), new Vector2(distance + length, 0) });
                distance += length;
            }
            Add(g, name, finish, points, uv, triangles);
        }

        private static void Polygon(CollectionTruckGeometry g, string name, Vector3[] points, Vector2[] plane,
            Vector3 extrusion, string finish)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            int count = points.Length;
            vertices.AddRange(points); uv.AddRange(plane);
            foreach (Vector3 p in points) vertices.Add(p + extrusion);
            uv.AddRange(plane);
            List<int> face = Triangulate(plane);
            // Projected coordinates use opposite basis handedness for side and front faces.
            Vector3 normal = Vector3.zero;
            for (int i = 0; i < count; i++) normal += Vector3.Cross(points[i], points[(i + 1) % count]);
            bool sideFlip = Vector3.Dot(normal, extrusion) > 0;
            for (int i = 0; i < face.Count; i += 3)
            {
                int a = face[i], b = face[i + 1], c = face[i + 2];
                Vector3 n = Vector3.Cross(points[b] - points[a], points[c] - points[a]);
                if (Vector3.Dot(n, extrusion) > 0) (b, c) = (c, b);
                triangles.AddRange(new[] { a, b, c, a + count, c + count, b + count });
            }
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count, v = vertices.Count;
                vertices.AddRange(new[] { points[i], points[i] + extrusion, points[next] + extrusion, points[next] });
                float length = Vector3.Distance(points[i], points[next]);
                uv.AddRange(new[] { Vector2.zero, new Vector2(extrusion.magnitude, 0),
                    new Vector2(extrusion.magnitude, length), new Vector2(0, length) });
                triangles.AddRange(sideFlip ? new[] { v, v + 2, v + 1, v, v + 3, v + 2 }
                    : new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
            }
            Add(g, name, finish, vertices, uv, triangles);
        }

        private static List<int> Triangulate(Vector2[] outline)
        {
            var polygon = new List<int>(); var result = new List<int>(); float area = 0;
            for (int i = 0; i < outline.Length; i++)
            {
                polygon.Add(i); Vector2 next = outline[(i + 1) % outline.Length];
                area += outline[i].x * next.y - next.x * outline[i].y;
            }
            if (area < 0) polygon.Reverse();
            int limit = outline.Length * outline.Length;
            while (polygon.Count > 2 && limit-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < polygon.Count; i++)
                {
                    int a = polygon[(i + polygon.Count - 1) % polygon.Count], b = polygon[i], c = polygon[(i + 1) % polygon.Count];
                    if (Cross(outline[b] - outline[a], outline[c] - outline[b]) <= .000001f) continue;
                    bool occupied = false;
                    foreach (int p in polygon)
                    {
                        if (p == a || p == b || p == c) continue;
                        if (Cross(outline[b] - outline[a], outline[p] - outline[a]) >= -.000001f &&
                            Cross(outline[c] - outline[b], outline[p] - outline[b]) >= -.000001f &&
                            Cross(outline[a] - outline[c], outline[p] - outline[c]) >= -.000001f)
                        { occupied = true; break; }
                    }
                    if (occupied) continue;
                    result.AddRange(new[] { a, b, c }); polygon.RemoveAt(i); clipped = true; break;
                }
                if (!clipped) throw new InvalidOperationException("Truck panel outline must be a simple non-intersecting polygon.");
            }
            return result;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private static void Add(CollectionTruckGeometry g, string name, string finish,
            List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
        {
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            Transform item = GreyboxFactory.Group(name, g.Root);
            item.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            item.gameObject.AddComponent<MeshRenderer>().sharedMaterial = CollectionTruckMaterials.Get(finish);
        }
    }
}
