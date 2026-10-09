using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Original continuous, weathered ridges and their forest edge; adjoining sectors share identical seam heights.</summary>
    public static class MeridianMountainMeshes
    {
        private static readonly List<Vector3> vertices = new();
        private static readonly List<Color> colors = new();
        private static readonly List<Vector2> uv = new();
        private static readonly List<int>[] triangles = { new(), new(), new() };
        private const float SectorAngle = Mathf.PI / 8f;

        public static Mesh Range(int sector, bool low)
        {
            string name = $"EnclosingMountain_{sector:00}_{(low ? "Low" : "High")}";
            Mesh existing = Load(name);
            if (existing != null) return existing;
            Clear();
            int around = low ? 12 : 24, across = low ? 14 : 28;
            for (int row = 0; row <= across; row++)
            for (int column = 0; column <= around; column++)
            {
                float angle = (sector + column / (float)around) * SectorAngle;
                float radius = Mathf.Lerp(332f, 940f, row / (float)across);
                Vector3 at = Point(angle, radius);
                vertices.Add(at);
                uv.Add(new Vector2(angle * 40f, radius * .03f));
                float variation = .87f + .13f * Mathf.Sin(angle * 31f + radius * .043f);
                colors.Add(new Color(variation, variation, variation, 1f));
            }
            for (int row = 0; row < across; row++)
            for (int column = 0; column < around; column++)
            {
                int a = row * (around + 1) + column, b = a + around + 1;
                Vector3 centre = (vertices[a] + vertices[b] + vertices[a + 1]) / 3f;
                Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[a + 1] - vertices[a]).normalized;
                int part = centre.y < 46f && normal.y > .72f ? 0 : centre.y > 82f ? 2 : 1;
                // Radius increases along rows, giving these triangles outward-facing upper surfaces.
                triangles[part].AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
            return Save(name);
        }

        public static Mesh Forest(int sector)
        {
            string name = $"MountainWoodland_{sector:00}";
            Mesh existing = Load(name);
            if (existing != null) return existing;
            Clear();
            var random = new System.Random(104729 + sector * 7919);
            for (int tree = 0; tree < 88; tree++)
            {
                float angle = (sector + (float)random.NextDouble()) * SectorAngle;
                float radius = Mathf.Lerp(352f, 450f, (float)random.NextDouble());
                Vector3 at = Point(angle, radius);
                float height = Mathf.Lerp(6.5f, 12.5f, (float)random.NextDouble());
                Cylinder(at, height * .65f, .15f, 0);
                if (tree % 4 != 0)
                {
                    Cone(at + Vector3.up * (height * .32f), height * .58f, height * .20f, 1);
                    Cone(at + Vector3.up * (height * .54f), height * .46f, height * .15f, 1);
                }
                else
                {
                    Crown(at + Vector3.up * (height * .70f), new Vector3(height * .23f, height * .31f, height * .21f), tree);
                    Crown(at + new Vector3(height * .13f, height * .56f, 0f),
                        new Vector3(height * .18f, height * .22f, height * .17f), tree + 1);
                }
            }
            return Save(name);
        }

        private static Vector3 Point(float angle, float radius) =>
            new(Mathf.Sin(angle) * radius, Height(angle, radius), Mathf.Cos(angle) * radius);

        private static float Height(float angle, float radius)
        {
            float firstCrest = 432f + Mathf.Sin(angle * 3f + .3f) * 22f + Mathf.Cos(angle * 7f) * 12f;
            float firstHeight = 77f + Mathf.Sin(angle * 5f + .8f) * 17f + Mathf.Sin(angle * 11f) * 9f;
            float farCrest = 666f + Mathf.Cos(angle * 4f + .7f) * 31f;
            float farHeight = 96f + Mathf.Sin(angle * 7f + 1.2f) * 14f;
            float hill = firstHeight * Mathf.Exp(-Mathf.Pow((radius - firstCrest) / 69f, 2f));
            float distant = farHeight * Mathf.Exp(-Mathf.Pow((radius - farCrest) / 118f, 2f));
            float valleys = 10f * Mathf.Sin(angle * 19f + radius * .022f) +
                5f * Mathf.Sin(angle * 43f - radius * .047f);
            float weathering = Mathf.Clamp01((hill + distant) / 40f) * valleys;
            float innerBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(332f, 373f, radius));
            float outerBlend = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(845f, 940f, radius));
            return Mathf.Max(0f, hill + distant + weathering) * innerBlend * outerBlend;
        }

        private static void Cone(Vector3 at, float height, float radius, int part)
        {
            const int sides = 7;
            for (int side = 0; side < sides; side++)
            {
                float first = side * Mathf.PI * 2f / sides, next = (side + 1) * Mathf.PI * 2f / sides;
                Vector3 a = at + new Vector3(Mathf.Sin(first), 0f, Mathf.Cos(first)) * radius;
                Vector3 b = at + new Vector3(Mathf.Sin(next), 0f, Mathf.Cos(next)) * radius;
                Triangle(a, b, at + Vector3.up * height, part);
            }
        }

        private static void Cylinder(Vector3 at, float height, float radius, int part)
        {
            const int sides = 5;
            for (int side = 0; side < sides; side++)
            {
                float first = side * Mathf.PI * 2f / sides, next = (side + 1) * Mathf.PI * 2f / sides;
                Vector3 a = at + new Vector3(Mathf.Sin(first), 0f, Mathf.Cos(first)) * radius;
                Vector3 b = at + new Vector3(Mathf.Sin(next), 0f, Mathf.Cos(next)) * radius;
                Triangle(a, b + Vector3.up * height, a + Vector3.up * height, part);
                Triangle(a, b, b + Vector3.up * height, part);
            }
        }

        private static void Crown(Vector3 at, Vector3 scale, int seed)
        {
            const int rings = 4, sides = 7;
            Vector3 Corner(int row, int column)
            {
                float latitude = row * Mathf.PI / rings, angle = column * Mathf.PI * 2f / sides;
                float irregular = 1f + Mathf.Sin(column * 2.7f + seed + row) * .10f;
                return at + Vector3.Scale(scale, new Vector3(Mathf.Sin(latitude) * Mathf.Sin(angle) * irregular,
                    Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Cos(angle) * irregular));
            }
            for (int row = 0; row < rings; row++)
            for (int column = 0; column < sides; column++)
            {
                Vector3 a = Corner(row, column), b = Corner(row, column + 1);
                Vector3 c = Corner(row + 1, column + 1), d = Corner(row + 1, column);
                if (row > 0) Triangle(a, c, b, 2);
                if (row < rings - 1) Triangle(a, d, c, 2);
            }
        }

        private static void Triangle(Vector3 a, Vector3 b, Vector3 c, int part)
        {
            int first = vertices.Count;
            vertices.AddRange(new[] { a, b, c });
            colors.AddRange(new[] { Color.white, Color.white, Color.white });
            uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one });
            triangles[part].AddRange(new[] { first, first + 1, first + 2 });
        }

        private static void Clear()
        {
            vertices.Clear(); colors.Clear(); uv.Clear();
            foreach (List<int> part in triangles) part.Clear();
        }

        private static Mesh Load(string name)
        {
            PolishAssets.EnsureFolder(MeridianMountainMaterials.Folder);
            return AssetDatabase.LoadAssetAtPath<Mesh>($"{MeridianMountainMaterials.Folder}/{name}.asset");
        }

        private static Mesh Save(string name)
        {
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetUVs(0, uv);
            mesh.subMeshCount = triangles.Length;
            for (int part = 0; part < triangles.Length; part++) mesh.SetTriangles(triangles[part], part);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, $"{MeridianMountainMaterials.Folder}/{name}.asset");
            return mesh;
        }
    }
}
