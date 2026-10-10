using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Metre-scale hard-surface source geometry combined into persistent material meshes for each truck LOD.</summary>
    public sealed class CollectionTruckGeometry
    {
        public const string Folder = "Assets/_Project/Art/Generated/CollectionTruck0157";
        public Transform Root { get; }
        public int Detail { get; }
        public CollectionTruckGeometry(Transform root, int detail) { Root = root; Detail = detail; }

        public GameObject Box(string name, Vector3 at, Vector3 size, string finish, Vector3 rotation = default)
        {
            var item = GreyboxFactory.Box(name, Root, at, size, CollectionTruckMaterials.Get(finish), false);
            item.transform.localRotation = Quaternion.Euler(rotation);
            return item;
        }

        public GameObject Cylinder(string name, Vector3 at, Vector3 size, string finish, Vector3 rotation = default)
        {
            // Source cylinders use the shared original clearance meshes, persisted in the finished combination.
            var item = SalvageModelFactory.Cylinder(Root, name, at, size, "Metal", rotation, Detail == 0 ? 24 : 12);
            item.GetComponent<MeshRenderer>().sharedMaterial = CollectionTruckMaterials.Get(finish);
            return item;
        }

        public void Bar(string name, Vector3 a, Vector3 b, float diameter, string finish)
        {
            GameObject item = Cylinder(name, (a + b) * .5f, new Vector3(diameter, Vector3.Distance(a, b), diameter), finish);
            item.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }

        public void Profile(string name, Vector2[] yz, float width, float xCenter, string finish)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
            int count = yz.Length;
            // Per-face vertices retain sharp creases and coherent metre-based UVs.
            for (int face = 0; face < count; face++)
            {
                int next = (face + 1) % count, v = vertices.Count;
                vertices.Add(new Vector3(xCenter - width / 2, yz[face].x, yz[face].y));
                vertices.Add(new Vector3(xCenter + width / 2, yz[face].x, yz[face].y));
                vertices.Add(new Vector3(xCenter + width / 2, yz[next].x, yz[next].y));
                vertices.Add(new Vector3(xCenter - width / 2, yz[next].x, yz[next].y));
                float length = Vector2.Distance(yz[face], yz[next]);
                uv.AddRange(new[] { Vector2.zero, new Vector2(width, 0), new Vector2(width, length), new Vector2(0, length) });
                tris.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
            }
            for (int side = 0; side < 2; side++)
            {
                int v = vertices.Count;
                for (int p = 0; p < count; p++)
                { vertices.Add(new Vector3(xCenter + (side == 0 ? -.5f : .5f) * width, yz[p].x, yz[p].y)); uv.Add(yz[p]); }
                for (int p = 1; p < count - 1; p++)
                    tris.AddRange(side == 0 ? new[] { v, v + p, v + p + 1 } : new[] { v, v + p + 1, v + p });
            }
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            Part(name, mesh, finish);
        }

        public void Tire(string name, Vector3 at, float width)
        {
            int rings = Detail == 0 ? 32 : Detail == 1 ? 20 : 12, sides = Detail == 0 ? 12 : 8;
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            // Elliptical section: broad tread, rounded sidewalls and a real open hub, along the truck X axle.
            for (int ring = 0; ring <= rings; ring++)
            for (int side = 0; side <= sides; side++)
            {
                float a = ring * Mathf.PI * 2 / rings, b = side * Mathf.PI * 2 / sides;
                float radius = .355f + .115f * Mathf.Cos(b);
                vertices.Add(at + new Vector3(Mathf.Sin(b) * width * .5f, Mathf.Sin(a) * radius, Mathf.Cos(a) * radius));
                uv.Add(new Vector2((float)ring / rings * 4, (float)side / sides));
                if (ring == rings || side == sides) continue;
                int v = ring * (sides + 1) + side, n = v + sides + 1;
                triangles.AddRange(new[] { v, v + 1, n + 1, v, n + 1, n });
            }
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); Part(name, mesh, "Tire");
        }

        private void Part(string name, Mesh mesh, string finish)
        {
            Transform item = GreyboxFactory.Group(name, Root);
            item.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            item.gameObject.AddComponent<MeshRenderer>().sharedMaterial = CollectionTruckMaterials.Get(finish);
        }

        public void Add(string name, Mesh mesh, string finish) => Part(name, mesh, finish);

        public Renderer[] SaveCombined(Transform target, string name)
        {
            PolishAssets.EnsureFolder(Folder + "/Meshes");
            var groups = new Dictionary<Material, List<CombineInstance>>();
            foreach (MeshFilter filter in Root.GetComponentsInChildren<MeshFilter>())
            {
                Material material = filter.GetComponent<MeshRenderer>().sharedMaterial;
                if (!groups.TryGetValue(material, out List<CombineInstance> instances)) groups[material] = instances = new();
                instances.Add(new CombineInstance { mesh = filter.sharedMesh, transform = Root.worldToLocalMatrix * filter.transform.localToWorldMatrix });
            }
            Transform holder = GreyboxFactory.Group(name, target); var renderers = new List<Renderer>();
            foreach (var pair in groups)
            {
                var combined = new Mesh { name = name + "_" + pair.Key.name, indexFormat = IndexFormat.UInt32 };
                combined.CombineMeshes(pair.Value.ToArray(), true, true); combined.RecalculateBounds();
                string path = Folder + "/Meshes/" + combined.name + ".asset";
                Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (saved == null) { AssetDatabase.CreateAsset(combined, path); saved = combined; }
                else { EditorUtility.CopySerialized(combined, saved); Object.DestroyImmediate(combined); EditorUtility.SetDirty(saved); }
                Transform batch = GreyboxFactory.Group(pair.Key.name, holder);
                batch.gameObject.AddComponent<MeshFilter>().sharedMesh = saved;
                var renderer = batch.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = pair.Key;
                if (pair.Key.renderQueue >= (int)RenderQueue.Transparent) renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderers.Add(renderer);
            }
            return renderers.ToArray();
        }
    }
}
