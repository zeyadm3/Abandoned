using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Original static trees, bushes, rolling berms and sagging utility wires for the HQ district.</summary>
    public static class HqDistrictLandscapeMeshes
    {
        private const string Folder = "Assets/_Project/Art/Generated/HqDistrictLandscape";
        private static readonly List<Vector3> vertices = new();
        private static readonly List<Vector2> uv = new();
        private static readonly List<int>[] faces = { new(), new(), new() };

        public static Mesh Plants(string name, Rect area, int count, bool shrubs, Vector3 berm)
        {
            Mesh existing = Load(name);
            if (existing != null) return existing;
            Clear();
            int seed = 17;
            foreach (char c in name) seed = unchecked(seed * 31 + c);
            var random = new System.Random(seed & int.MaxValue);
            for (int plant = 0; plant < count; plant++)
            {
                float x = Mathf.Lerp(area.xMin, area.xMax, (float)random.NextDouble());
                float z = Mathf.Lerp(area.yMin, area.yMax, (float)random.NextDouble());
                float ground = berm == Vector3.zero ? 0f : BermHeight(x / berm.x, z / berm.z) * berm.y;
                Vector3 at = new(x, ground, z);
                float size = Mathf.Lerp(.70f, 1.25f, (float)random.NextDouble());
                if (shrubs)
                {
                    Ellipsoid(at + Vector3.up * .7f * size, new Vector3(1.15f, .90f, .95f) * size, 1 + plant % 2, plant);
                    continue;
                }
                if (berm != Vector3.zero) size *= 1.25f;
                Vector3 trunkTop = at + new Vector3(.16f * size, 4.8f * size, .12f * size);
                Tube(at, trunkTop, .16f * size, .075f * size, 0, 6);
                Tube(at + Vector3.up * 2.4f * size, at + new Vector3(-1.45f, 5.3f, .25f) * size,
                    .085f * size, .035f * size, 0, 5);
                Tube(at + Vector3.up * 3f * size, at + new Vector3(1.35f, 6.1f, -.35f) * size,
                    .075f * size, .028f * size, 0, 5);
                Ellipsoid(at + new Vector3(.15f, 5.6f, .1f) * size, new Vector3(2.1f, 2.75f, 1.85f) * size, 1, plant);
                Ellipsoid(at + new Vector3(-1.25f, 4.8f, .30f) * size, new Vector3(1.65f, 1.9f, 1.5f) * size, 2, plant + 3);
                Ellipsoid(at + new Vector3(1.15f, 5.65f, -.25f) * size, new Vector3(1.45f, 2f, 1.55f) * size, 1, plant + 7);
            }
            return Save(name, 3);
        }

        public static Mesh Berm()
        {
            const string name = "RollingBerm";
            Mesh existing = Load(name);
            if (existing != null) return existing;
            Clear();
            const int across = 20, deep = 12;
            for (int row = 0; row <= deep; row++)
            for (int column = 0; column <= across; column++)
            {
                float x = column / (float)across - .5f, z = row / (float)deep - .5f;
                vertices.Add(new Vector3(x, BermHeight(x, z), z));
                uv.Add(new Vector2(x + .5f, z + .5f));
            }
            for (int row = 0; row < deep; row++)
            for (int column = 0; column < across; column++)
            {
                int a = row * (across + 1) + column, b = a + across + 1;
                faces[0].AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
            return Save(name, 1);
        }

        public static Mesh Cable(string name, Vector3 end)
        {
            Mesh existing = Load(name);
            if (existing != null) return existing;
            Clear();
            Vector3 previous = Vector3.zero;
            const int segments = 14;
            for (int segment = 1; segment <= segments; segment++)
            {
                float t = segment / (float)segments;
                Vector3 point = end * t + Vector3.down * (Mathf.Sin(t * Mathf.PI) * 1.10f);
                Tube(previous, point, .025f, .025f, 0, 4);
                previous = point;
            }
            return Save(name, 1);
        }

        private static float BermHeight(float x, float z)
        {
            float edge = Mathf.Clamp01(1f - x * x * 4f) * Mathf.Clamp01(1f - z * z * 4f);
            float rolling = .75f + .14f * Mathf.Sin(x * 12f + .4f) + .10f * Mathf.Cos(z * 9f - x * 6f);
            return edge * rolling;
        }

        private static void Ellipsoid(Vector3 at, Vector3 radius, int part, int seed)
        {
            const int rings = 5, sides = 8;
            Vector3 Point(int ring, int side)
            {
                float latitude = Mathf.PI * ring / rings, longitude = Mathf.PI * 2f * side / sides;
                float irregular = 1f + Mathf.Sin(side * 2.3f + seed + ring * 1.7f) * .095f;
                return at + new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude) * radius.x * irregular,
                    Mathf.Cos(latitude) * radius.y, Mathf.Sin(latitude) * Mathf.Sin(longitude) * radius.z * irregular);
            }
            for (int ring = 0; ring < rings; ring++)
            for (int side = 0; side < sides; side++)
            {
                Vector3 a = Point(ring, side), b = Point(ring, side + 1);
                Vector3 c = Point(ring + 1, side + 1), d = Point(ring + 1, side);
                if (ring > 0) Triangle(a, b, c, part);
                if (ring < rings - 1) Triangle(a, c, d, part);
            }
        }

        private static void Tube(Vector3 start, Vector3 end, float lower, float upper, int part, int sides)
        {
            Vector3 axis = (end - start).normalized;
            Vector3 right = Vector3.Cross(axis, Mathf.Abs(axis.y) > .9f ? Vector3.forward : Vector3.up).normalized;
            Vector3 forward = Vector3.Cross(right, axis).normalized;
            Vector3 Ring(float angle) => right * Mathf.Cos(angle) + forward * Mathf.Sin(angle);
            for (int side = 0; side < sides; side++)
            {
                Vector3 a = Ring(side * Mathf.PI * 2f / sides), b = Ring((side + 1) * Mathf.PI * 2f / sides);
                Triangle(start + a * lower, end + a * upper, end + b * upper, part);
                Triangle(start + a * lower, end + b * upper, start + b * lower, part);
                Triangle(end, end + b * upper, end + a * upper, part);
            }
        }

        private static void Triangle(Vector3 a, Vector3 b, Vector3 c, int part)
        {
            int first = vertices.Count;
            vertices.AddRange(new[] { a, b, c });
            uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one });
            faces[part].AddRange(new[] { first, first + 1, first + 2 });
        }

        private static void Clear()
        {
            vertices.Clear();
            uv.Clear();
            foreach (List<int> part in faces) part.Clear();
        }

        private static Mesh Load(string name)
        {
            PolishAssets.EnsureFolder(Folder);
            return AssetDatabase.LoadAssetAtPath<Mesh>(Folder + "/" + name + ".asset");
        }

        private static Mesh Save(string name, int parts)
        {
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.subMeshCount = parts;
            for (int part = 0; part < parts; part++) mesh.SetTriangles(faces[part], part);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, Folder + "/" + name + ".asset");
            return mesh;
        }
    }
}
