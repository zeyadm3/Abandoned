using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Shared original paper and fabric surfaces; persistent assets survive scene reloads.</summary>
    internal static class MallRetailArtworkMesh0158
    {
        internal static Mesh Surface(string kind)
        {
            string path = MallRetailArtworkMaterials0158.Folder + "/Meshes/" + kind + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            int columns = kind == "FasciaPrint" ? 1 : 8, rows = kind == "FasciaPrint" ? 1 : 10;
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            for (int row = 0; row <= rows; row++)
            for (int column = 0; column <= columns; column++)
            {
                float u = (float)column / columns, v = (float)row / rows;
                float depth = 0;
                if (kind == "WindowPaper")
                    depth = .018f * Mathf.Pow(u, 8) * Mathf.Pow(1 - v, 8);
                else if (kind == "HangingFabric")
                    depth = Mathf.Sin(u * Mathf.PI * 3.5f) * Mathf.Sin(v * Mathf.PI) * .012f;
                vertices.Add(new Vector3(u - .5f, v - .5f, depth));
                uv.Add(new Vector2(u, v));
            }
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                int a = row * (columns + 1) + column, b = a + 1, c = a + columns + 1, d = c + 1;
                triangles.AddRange(new[] { a, b, c, b, d, c });
            }
            mesh = new Mesh { name = kind };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
