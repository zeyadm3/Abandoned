using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Original ridgelines, conifers and pitched roofs; distant scenery has no physics.</summary>
    public static class BurleySchoolLandscape
    {
        private const string Folder = BurleySchoolArt.Folder + "/LandscapeMeshes";

        public static void Ground(Transform parent, BurleySchoolArt art)
        {
            Transform ground = Group("CampusGround", parent);
            // The service basement is an actual hole in the terrain, including its stair footprint.
            Slab("SouthGround", new Rect(-70f, -70f, 340f, 150f));
            Slab("NorthGround", new Rect(-70f, 92f, 340f, 138f));
            Slab("WestGround", new Rect(-70f, 80f, 114f, 12f));
            Slab("EastGround", new Rect(68f, 80f, 202f, 12f));
            void Slab(string name, Rect r) => Box(name, ground, new Vector3(r.center.x, -.35f, r.center.y),
                new Vector3(r.width, .5f, r.height), art.Snow);
        }

        public static void Ridge(Transform parent, string name, Vector3 at, Vector3 size, float yaw, int seed, BurleySchoolArt art)
        {
            Transform ridge = Group(name, parent);
            ridge.position = new Vector3(at.x, -.2f, at.z);
            ridge.rotation = Quaternion.Euler(0f, yaw, 0f);
            Mesh mesh = Asset(name + "_Ridge_v2", () => BuildRidge(size, seed));
            ridge.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            ridge.gameObject.AddComponent<MeshRenderer>().sharedMaterials = new[] { art.Soil, art.Snow };
        }

        public static void Pine(Transform parent, string name, Vector3 at, float scale, BurleySchoolArt art)
        {
            Transform tree = Group(name, parent);
            tree.position = at;
            tree.localScale = Vector3.one * scale;
            Primitive(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, 3.7f, 0f), new Vector3(.36f, 3.7f, .36f), art.OakWood);
            Mesh cone = Asset("ConiferCrown_v2", Cone);
            for (int tier = 0; tier < 5; tier++)
            {
                float radius = 2.4f - tier * .4f;
                float y = 1.4f + tier * 1.35f;
                Shape("NeedleBough_" + tier, tree, new Vector3(0f, y, 0f), new Vector3(radius, 2.8f, radius), cone, art.PineFoliage);
                Shape("SnowCap_" + tier, tree, new Vector3(.05f, y + .75f, -.03f), new Vector3(radius * .76f, 2.12f, radius * .76f), cone, art.Snow);
            }
        }

        public static void Roof(Transform parent, BurleySchoolArt art)
        {
            Mesh roof = Asset("CraftsmanGable_v2", Gable);
            Shape("PitchedRoof", parent, new Vector3(0f, 6.4f, 0f), new Vector3(11.4f, 2.6f, 9.4f), roof, art.RoofGravel);
            // Two sloping snow planes follow the pitches instead of a flat box above the house.
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject snow = Box("RoofSnow_" + side, parent, new Vector3(side * 2.9f, 7.78f, 0f),
                    new Vector3(6.35f, .14f, 9.55f), art.Snow, false);
                snow.transform.localRotation = Quaternion.Euler(0f, 0f, -side * 24.5f);
            }
        }

        private static GameObject Shape(string name, Transform parent, Vector3 at, Vector3 size, Mesh mesh, Material material)
        {
            Transform item = Group(name, parent);
            item.localPosition = at;
            item.localScale = size;
            item.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            item.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            return item.gameObject;
        }

        private static Mesh Asset(string name, Func<Mesh> make)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            mesh = make();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Mesh BuildRidge(Vector3 size, int seed)
        {
            const int nx = 36, nz = 10;
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var heights = new List<float>();
            for (int z = 0; z <= nz; z++)
            for (int x = 0; x <= nx; x++)
            {
                float u = x / (float)nx, v = z / (float)nz;
                float spine = .36f + .16f * Mathf.Sin(u * 11f + seed);
                float profile = Mathf.Clamp01(1f - Mathf.Abs(v - spine) / .66f);
                float crest = .45f + .55f * Mathf.PerlinNoise(u * 9f + seed, 1.9f + seed * .7f);
                float taper = Mathf.Pow(Mathf.Sin(u * Mathf.PI), .5f);
                float h = profile * crest * taper;
                vertices.Add(new Vector3((u - .5f) * size.x, h * size.y, (v - .5f) * size.z));
                uv.Add(new Vector2(u * size.x / 20f, v * size.z / 20f));
                heights.Add(h);
            }
            var rock = new List<int>();
            var snow = new List<int>();
            for (int z = 0; z < nz; z++)
            for (int x = 0; x < nx; x++)
            {
                int a = z * (nx + 1) + x, b = a + 1, c = a + nx + 1, d = c + 1;
                List<int> triangles = (heights[a] + heights[b] + heights[c] + heights[d]) * .25f > .48f ? snow : rock;
                triangles.AddRange(new[] { a, c, b, b, c, d });
            }
            var mesh = new Mesh { subMeshCount = 2 };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(rock, 0);
            mesh.SetTriangles(snow, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh Cone()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uv = new List<Vector2>();
            const int sides = 12;
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides, b = (i + 1) * Mathf.PI * 2f / sides;
                int first = vertices.Count;
                vertices.AddRange(new[] { new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), Vector3.up,
                    new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) });
                uv.AddRange(new[] { new Vector2(i / (float)sides, 0f), new Vector2((i + .5f) / sides, 1f), new Vector2((i + 1f) / sides, 0f) });
                triangles.AddRange(new[] { first, first + 1, first + 2 });
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh Gable()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { new Vector3(-.5f, 0f, -.5f), new Vector3(.5f, 0f, -.5f), new Vector3(0f, 1f, -.5f),
                new Vector3(-.5f, 0f, .5f), new Vector3(.5f, 0f, .5f), new Vector3(0f, 1f, .5f) };
            mesh.triangles = new[] { 0, 2, 1, 3, 4, 5, 0, 3, 5, 0, 5, 2, 1, 2, 5, 1, 5, 4, 0, 1, 4, 0, 4, 3 };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.zero, Vector2.right, Vector2.up };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
