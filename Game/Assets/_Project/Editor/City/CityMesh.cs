using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds and saves the city's flat geometry. The city is mostly horizontal surfaces cut to shape — ground with
    /// the river and the sea taken out of it, water, banks — so rather than place thousands of boxes and merge them
    /// afterwards, each piece is gathered as triangles here and saved as one mesh. That is what keeps a whole cell
    /// down to a handful of renderers (Docs/CITY_PLAN.md, "How it stays fast").
    /// </summary>
    public static class CityMesh
    {
        public const string Folder = "Assets/_Project/Art/Generated/City";
        /// <summary>Metres of ground per texture repeat, so surfaces read at the same size everywhere.</summary>
        public const float TexelMetres = 8f;

        /// <summary>Throws away the last run's meshes, so a rebuild never leaves orphans behind.</summary>
        public static void Reset()
        {
            if (AssetDatabase.IsValidFolder(Folder)) AssetDatabase.DeleteAsset(Folder);
            PolishAssets.EnsureFolder(Folder);
        }

        /// <summary>Triangles gathered toward one saved mesh.</summary>
        public class Piece
        {
            private readonly List<Vector3> verts = new();
            private readonly List<Vector2> uvs = new();
            private readonly List<int> tris = new();

            public int Triangles => tris.Count / 3;
            public bool Empty => tris.Count == 0;

            /// <summary>A flat patch of ground, from (x0,z0) to (x1,z1) at height y, facing up.</summary>
            public void Flat(float x0, float z0, float x1, float z1, float y) => Quad(
                new Vector3(x0, y, z0), new Vector3(x0, y, z1), new Vector3(x1, y, z1), new Vector3(x1, y, z0));

            /// <summary>Four corners in order. UVs come from the world, so nothing has to be unwrapped.</summary>
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int i = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
                foreach (Vector3 v in new[] { a, b, c, d })
                    uvs.Add(new Vector2(v.x + v.y, v.z + v.y) / TexelMetres);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
                tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
            }

            /// <summary>A wall standing between two points, from <paramref name="top"/> down to <paramref name="bottom"/>.</summary>
            public void Wall(Vector2 from, Vector2 to, float top, float bottom) => Quad(
                new Vector3(from.x, top, from.y), new Vector3(from.x, bottom, from.y),
                new Vector3(to.x, bottom, to.y), new Vector3(to.x, top, to.y));

            /// <summary>
            /// A building: four walls and a roof, turned to its street. No floor, because nothing ever sees under
            /// a block, and leaving it out keeps the base off the ground's plane where the two could flicker.
            /// </summary>
            public void Box(Vector2 centre, Vector2 size, float height, float yaw)
            {
                float rad = yaw * Mathf.Deg2Rad;
                Vector2 along = new(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 across = new(-along.y, along.x);
                Vector2 a = along * (size.x * .5f), b = across * (size.y * .5f);
                Vector2[] feet = { centre - a - b, centre - a + b, centre + a + b, centre + a - b };
                for (int i = 0; i < 4; i++)
                {
                    Vector2 from = feet[i], to = feet[(i + 1) % 4];
                    Quad(new Vector3(from.x, CityPlan.GroundY, from.y), new Vector3(from.x, height, from.y),
                         new Vector3(to.x, height, to.y), new Vector3(to.x, CityPlan.GroundY, to.y));
                }
                Quad(new Vector3(feet[0].x, height, feet[0].y), new Vector3(feet[3].x, height, feet[3].y),
                     new Vector3(feet[2].x, height, feet[2].y), new Vector3(feet[1].x, height, feet[1].y));
            }

            /// <summary>Saves what has been gathered, or null if nothing was. Over the same asset path every run.</summary>
            public Mesh Save(string name)
            {
                if (Empty) return null;
                var mesh = new Mesh { name = name };
                if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(verts);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, $"{Folder}/{name}.asset");
                return mesh;
            }
        }

        /// <summary>Hangs a saved mesh in the scene. Never casts a shadow: the city's ground and water are flat.</summary>
        public static GameObject Render(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        /// <summary>
        /// Turns a grid of solid squares into as few rectangles as possible, so a cell's ground needs a handful of
        /// box colliders instead of one per square. Rows are run together first, then equal runs stacked.
        /// </summary>
        public static List<RectInt> Rectangles(bool[,] solid)
        {
            int w = solid.GetLength(0), h = solid.GetLength(1);
            var taken = new bool[w, h];
            var found = new List<RectInt>();
            for (int z = 0; z < h; z++)
                for (int x = 0; x < w; x++)
                {
                    if (!solid[x, z] || taken[x, z]) continue;
                    int runX = x;
                    while (runX + 1 < w && solid[runX + 1, z] && !taken[runX + 1, z]) runX++;
                    int runZ = z;
                    while (runZ + 1 < h && WholeRow(solid, taken, x, runX, runZ + 1)) runZ++;
                    for (int zz = z; zz <= runZ; zz++)
                        for (int xx = x; xx <= runX; xx++) taken[xx, zz] = true;
                    found.Add(new RectInt(x, z, runX - x + 1, runZ - z + 1));
                }
            return found;
        }

        private static bool WholeRow(bool[,] solid, bool[,] taken, int x0, int x1, int z)
        {
            for (int x = x0; x <= x1; x++)
                if (!solid[x, z] || taken[x, z]) return false;
            return true;
        }
    }
}
