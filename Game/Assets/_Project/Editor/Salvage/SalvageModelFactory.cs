using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Metre-scale source geometry; the content author combines and saves the finished meshes.</summary>
    public static class SalvageModelFactory
    {
        private static readonly Dictionary<int, Mesh> cylinders = new();
        private static readonly Dictionary<int, Mesh> spheres = new();

        public static Transform Group(string name, Transform parent) => GreyboxFactory.Group(name, parent);

        public static GameObject Box(Transform parent, string name, Vector3 centre, Vector3 size,
            string finish, Vector3? euler = null)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(item.GetComponent<Collider>());
            Configure(item, parent, name, centre, size, finish, euler);
            return item;
        }

        public static GameObject Cylinder(Transform parent, string name, Vector3 centre, Vector3 size,
            string finish, Vector3? euler = null, int sides = 12)
        {
            sides = Mathf.Max(6, sides);
            if (!cylinders.TryGetValue(sides, out Mesh mesh)) cylinders[sides] = mesh = CylinderMesh(sides);
            return Part(mesh, parent, name, centre, size, finish, euler);
        }

        public static GameObject Sphere(Transform parent, string name, Vector3 centre, Vector3 size,
            string finish, Vector3? euler = null, int sides = 12)
        {
            sides = Mathf.Max(6, sides);
            if (!spheres.TryGetValue(sides, out Mesh mesh)) spheres[sides] = mesh = SphereMesh(sides);
            return Part(mesh, parent, name, centre, size, finish, euler);
        }

        public static GameObject Bar(Transform parent, string name, Vector3 a, Vector3 b,
            float diameter, string finish, int sides = 8)
        {
            GameObject item = Cylinder(parent, name, (a + b) * .5f,
                new Vector3(diameter, Vector3.Distance(a, b), diameter), finish, null, sides);
            item.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
            return item;
        }

        private static GameObject Part(Mesh mesh, Transform parent, string name, Vector3 centre,
            Vector3 size, string finish, Vector3? euler)
        {
            var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            Configure(item, parent, name, centre, size, finish, euler);
            return item;
        }

        private static void Configure(GameObject item, Transform parent, string name, Vector3 centre,
            Vector3 size, string finish, Vector3? euler)
        {
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = centre;
            item.transform.localScale = size;
            item.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
            item.GetComponent<MeshRenderer>().sharedMaterial = SalvageMaterials.Get(finish);
        }

        private static Mesh CylinderMesh(int sides)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            for (int i = 0; i <= sides; i++)
            {
                float angle = i * Mathf.PI * 2 / sides;
                float x = Mathf.Cos(angle) * .5f, z = Mathf.Sin(angle) * .5f;
                vertices.Add(new Vector3(x, -.5f, z)); uv.Add(new Vector2((float)i / sides, 0));
                vertices.Add(new Vector3(x, .5f, z)); uv.Add(new Vector2((float)i / sides, 1));
                if (i == sides) continue;
                int a = i * 2;
                triangles.AddRange(new[] { a, a + 1, a + 3, a, a + 3, a + 2 });
            }
            for (int top = 0; top < 2; top++)
            {
                int centre = vertices.Count;
                float y = top == 0 ? -.5f : .5f;
                vertices.Add(new Vector3(0, y, 0)); uv.Add(Vector2.one * .5f);
                for (int i = 0; i <= sides; i++)
                {
                    float angle = i * Mathf.PI * 2 / sides;
                    var at = new Vector3(Mathf.Cos(angle) * .5f, y, Mathf.Sin(angle) * .5f);
                    vertices.Add(at); uv.Add(new Vector2(at.x + .5f, at.z + .5f));
                    if (i == sides) continue;
                    int a = centre + i + 1;
                    triangles.AddRange(top == 0 ? new[] { centre, a, a + 1 } : new[] { centre, a + 1, a });
                }
            }
            return Mesh("ClearanceCylinder", vertices, uv, triangles);
        }

        private static Mesh SphereMesh(int sides)
        {
            int rings = Mathf.Max(3, sides / 2);
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            for (int y = 0; y <= rings; y++)
            for (int x = 0; x <= sides; x++)
            {
                float latitude = y * Mathf.PI / rings, longitude = x * Mathf.PI * 2 / sides;
                vertices.Add(new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude),
                    Mathf.Sin(latitude) * Mathf.Sin(longitude)) * .5f);
                uv.Add(new Vector2((float)x / sides, (float)y / rings));
                if (y == rings || x == sides) continue;
                int a = y * (sides + 1) + x, b = a + sides + 1;
                triangles.AddRange(new[] { a, b + 1, b, a, a + 1, b + 1 });
            }
            return Mesh("ClearanceSphere", vertices, uv, triangles);
        }

        private static Mesh Mesh(string name, List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
