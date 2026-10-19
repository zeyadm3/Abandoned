using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Reusable smooth tailored forms, with UVs and shared per-joint meshes for the salvage crew.</summary>
    public static class WorkerVisualMeshes
    {
        private const string Folder = "Assets/_Project/Art/Custom/Characters/Meshes";
        private static readonly Dictionary<string, Mesh> Forms = new();

        public static GameObject Form(string name, Transform parent, Vector3 at, Vector3 size, Material material, string profile = "Cloth")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localScale = size;
            go.AddComponent<MeshFilter>().sharedMesh = Profile(profile);
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        public static void Beam(string name, Transform parent, Vector3 from, Vector3 to, float width, Material material, float depth = -1f)
        {
            Vector3 d = to - from;
            GameObject go = Form(name, parent, (from + to) * .5f, new Vector3(width, d.magnitude, depth > 0f ? depth : width), material, "Tube");
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        public static Mesh Profile(string name)
        {
            if (Forms.TryGetValue(name, out Mesh cached) && cached != null) return cached;
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/CrewForm_{name}_V2.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                // Each ring is height, half-width and half-depth. Broad planar cloth panels meet rounded sides.
                Vector3[] rings = name switch
                {
                    "Jacket" => new[] { new Vector3(-.5f, .40f, .43f), new Vector3(-.40f, .46f, .48f), new Vector3(-.1f, .49f, .48f),
                        new Vector3(.26f, .5f, .46f), new Vector3(.42f, .43f, .41f), new Vector3(.5f, .28f, .29f) },
                    "Boot" => new[] { new Vector3(-.5f, .43f, .47f), new Vector3(-.38f, .5f, .5f), new Vector3(-.02f, .49f, .48f),
                        new Vector3(.22f, .40f, .38f), new Vector3(.5f, .34f, .30f) },
                    "Head" => new[] { new Vector3(-.5f, .20f, .28f), new Vector3(-.37f, .34f, .4f), new Vector3(-.12f, .47f, .46f),
                        new Vector3(.18f, .49f, .48f), new Vector3(.4f, .40f, .4f), new Vector3(.5f, .15f, .2f) },
                    "ViewSleeve" => new[] { new Vector3(-.5f, .5f, .5f), new Vector3(-.35f, .49f, .48f), new Vector3(.1f, .43f, .43f),
                        new Vector3(.35f, .37f, .39f), new Vector3(.5f, .34f, .35f) },
                    "Tube" => new[] { new Vector3(-.5f, .47f, .47f), new Vector3(-.43f, .5f, .5f), new Vector3(.43f, .5f, .5f), new Vector3(.5f, .47f, .47f) },
                    "Pad" => new[] { new Vector3(-.5f, .32f, .32f), new Vector3(-.35f, .48f, .48f), new Vector3(.35f, .48f, .48f), new Vector3(.5f, .32f, .32f) },
                    _ => new[] { new Vector3(-.5f, .35f, .38f), new Vector3(-.4f, .42f, .43f), new Vector3(-.18f, .48f, .5f),
                        new Vector3(.18f, .5f, .49f), new Vector3(.4f, .43f, .44f), new Vector3(.5f, .38f, .4f) },
                };
                mesh = Make(name, rings);
                AssetDatabase.CreateAsset(mesh, path);
            }
            Forms[name] = mesh;
            return mesh;
        }

        private static Mesh Make(string name, Vector3[] rings)
        {
            const int sides = 24;
            bool rounded = name is "Head" or "Tube";
            float exponent = rounded ? 1f : .66f;
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            for (int y = 0; y < rings.Length; y++)
            for (int i = 0; i <= sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                float x = Mathf.Sign(Mathf.Cos(a)) * Mathf.Pow(Mathf.Abs(Mathf.Cos(a)), exponent) * rings[y].y;
                float z = Mathf.Sign(Mathf.Sin(a)) * Mathf.Pow(Mathf.Abs(Mathf.Sin(a)), exponent) * rings[y].z;
                vertices.Add(new Vector3(x, rings[y].x, z));
                uv.Add(new Vector2(i / (float)sides, rings[y].x + .5f));
                if (y == rings.Length - 1 || i == sides) continue;
                int p = y * (sides + 1) + i, q = p + sides + 1;
                triangles.AddRange(new[] { p, q, p + 1, p + 1, q, q + 1 });
            }
            int bottom = vertices.Count;
            vertices.Add(new Vector3(0f, rings[0].x, 0f)); uv.Add(new Vector2(.5f, .5f));
            int top = vertices.Count;
            vertices.Add(new Vector3(0f, rings[rings.Length - 1].x, 0f)); uv.Add(new Vector2(.5f, .5f));
            for (int i = 0; i < sides; i++)
            {
                triangles.AddRange(new[] { bottom, i, i + 1 });
                int t = (rings.Length - 1) * (sides + 1) + i;
                triangles.AddRange(new[] { top, t + 1, t });
            }
            var mesh = new Mesh { name = "CrewForm_" + name };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            return mesh;
        }

        public static void Merge(Transform group, string key)
        {
            PolishAssets.EnsureFolder(Folder);
            var pieces = new List<MeshFilter>(group.GetComponentsInChildren<MeshFilter>());
            var materials = new Dictionary<Material, List<CombineInstance>>();
            foreach (MeshFilter piece in pieces)
            {
                var renderer = piece.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial == null || piece.sharedMesh == null) continue;
                if (!materials.TryGetValue(renderer.sharedMaterial, out var list)) materials[renderer.sharedMaterial] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = piece.sharedMesh, transform = group.worldToLocalMatrix * piece.transform.localToWorldMatrix });
            }
            foreach (var pair in materials)
            {
                string path = $"{Folder}/Crew_{key}_{pair.Key.name}.asset";
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = "Crew_" + key + "_" + pair.Key.name }; AssetDatabase.CreateAsset(mesh, path); }
                else mesh.Clear();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);
                var part = new GameObject(pair.Key.name);
                part.transform.SetParent(group, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
            foreach (MeshFilter piece in pieces) if (piece != null) Object.DestroyImmediate(piece.gameObject);
        }
    }
}
