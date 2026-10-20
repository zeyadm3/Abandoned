using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The two shapes cubes can't make for the house: a gable (triangular prism, for the roof ends) and a cone (snowy
    /// evergreens, the tree in the living room). Saved once as assets so rebuilds keep their references.
    /// </summary>
    public static class TutorialHouseMeshes
    {
        public const string Folder = "Assets/_Project/Art/Tutorial/Meshes";

        /// <summary>A unit gable: 1 wide on X (-0.5..0.5), 1 high on Y (0..1, apex at the top), 1 deep on Z.</summary>
        public static Mesh Gable() => Asset("TutorialGable", () =>
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            Vector3 a = new(-.5f, 0f, -.5f), b = new(.5f, 0f, -.5f), c = new(0f, 1f, -.5f);
            Vector3 d = new(-.5f, 0f, .5f), e = new(.5f, 0f, .5f), f = new(0f, 1f, .5f);
            Tri(v, t, a, c, b);       // front
            Tri(v, t, d, e, f);       // back
            Quad(v, t, a, d, f, c);   // left slope
            Quad(v, t, b, c, f, e);   // right slope
            Quad(v, t, a, b, e, d);   // base
            return Build(v, t);
        });

        /// <summary>A unit cone: radius 0.5 at y = 0, apex at y = 1, twelve sides.</summary>
        public static Mesh Cone() => Asset("TutorialCone", () =>
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            const int sides = 12;
            Vector3 apex = Vector3.up;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                Vector3 p0 = new(Mathf.Cos(a0) * .5f, 0f, Mathf.Sin(a0) * .5f), p1 = new(Mathf.Cos(a1) * .5f, 0f, Mathf.Sin(a1) * .5f);
                Tri(v, t, p0, apex, p1);
                Tri(v, t, p1, Vector3.zero, p0);
            }
            return Build(v, t);
        });

        public static GameObject Shape(string name, Transform parent, Mesh mesh, Vector3 localPosition, Vector3 scale, Material material,
            bool collider = true, Quaternion? rotation = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            return go;
        }

        private static void Tri(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
        }

        private static void Quad(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Tri(v, t, a, b, c);
            Tri(v, t, a, c, d);
        }

        private static Mesh Build(List<Vector3> v, List<int> t)
        {
            var mesh = new Mesh();
            mesh.SetVertices(v);
            mesh.SetTriangles(t, 0);
            var uv = new Vector2[v.Count];
            for (int i = 0; i < v.Count; i++) uv[i] = new Vector2(v[i].x + v[i].z, v[i].y);
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh Asset(string name, System.Func<Mesh> make)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;
            Mesh mesh = make();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
