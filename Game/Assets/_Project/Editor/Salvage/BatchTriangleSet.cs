using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Triangles of known source geometry, in a batch's own space, so exactly those can be cut back out of a
    /// combined mesh. Matching is by position (within 2 mm), never by bounds: stock resting on a shelf, or a
    /// neighbour touching it, keeps every triangle that isn't the prop's own.
    /// </summary>
    public sealed class BatchTriangleSet
    {
        private const float Grid = .004f, Tolerance = .002f;
        private readonly Dictionary<Vector3Int, List<Vector3[]>> triangles = new();

        public int Add(Mesh mesh, Matrix4x4 matrix)
        {
            Vector3[] vertices = mesh.vertices;
            int added = 0;
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                int[] indices = mesh.GetTriangles(submesh);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    var triangle = new[]
                    {
                        matrix.MultiplyPoint3x4(vertices[indices[i]]),
                        matrix.MultiplyPoint3x4(vertices[indices[i + 1]]),
                        matrix.MultiplyPoint3x4(vertices[indices[i + 2]])
                    };
                    Vector3Int key = Key((triangle[0] + triangle[1] + triangle[2]) / 3f);
                    if (!triangles.TryGetValue(key, out List<Vector3[]> list)) triangles[key] = list = new List<Vector3[]>();
                    list.Add(triangle);
                    added++;
                }
            }
            return added;
        }

        /// <summary>Removes every matching triangle from <paramref name="mesh"/> (a saved asset); returns how many.</summary>
        public int Strip(Mesh mesh)
        {
            if (mesh == null || triangles.Count == 0) return 0;
            Vector3[] vertices = mesh.vertices;
            var kept = new List<int>[mesh.subMeshCount];
            int removed = 0;
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                int[] indices = mesh.GetTriangles(submesh);
                var keep = new List<int>(indices.Length);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    if (Matches(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]]))
                    {
                        removed++;
                        continue;
                    }
                    keep.Add(indices[i]); keep.Add(indices[i + 1]); keep.Add(indices[i + 2]);
                }
                kept[submesh] = keep;
            }
            if (removed == 0) return 0;
            for (int submesh = 0; submesh < kept.Length; submesh++) mesh.SetTriangles(kept[submesh], submesh, false);
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return removed;
        }

        public bool Matches(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3Int key = Key((a + b + c) / 3f);
            // Neighbouring cells too: float rounding can put the same centroid either side of a grid line.
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            for (int z = -1; z <= 1; z++)
            {
                if (!triangles.TryGetValue(key + new Vector3Int(x, y, z), out List<Vector3[]> list)) continue;
                foreach (Vector3[] triangle in list)
                    if (Near(a, triangle) && Near(b, triangle) && Near(c, triangle)) return true;
            }
            return false;
        }

        private static bool Near(Vector3 point, Vector3[] triangle)
        {
            foreach (Vector3 corner in triangle)
                if ((corner - point).sqrMagnitude <= Tolerance * Tolerance) return true;
            return false;
        }

        private static Vector3Int Key(Vector3 point) => Vector3Int.FloorToInt(point / Grid);
    }
}
