using System.Collections.Generic;
using Abandoned.World;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gathers what the ground, road and massing builders made into one object per cell, and gives each cell the
    /// far copy of itself that <see cref="CityStreamer"/> shows at a distance (Docs/CITY_PLAN.md step 5).
    ///
    /// The far copy is the cell's own meshes combined into a single mesh of a single material. The city is cheap in
    /// triangles and expensive in draw calls — a cell near the van is four to six renderers — so what the distance
    /// has to save is calls, not geometry: one per cell instead of six, and nothing at all past the fog.
    /// </summary>
    public static class CityCells
    {
        private static readonly Dictionary<Vector2Int, List<Transform>> parts = new();

        public static void Reset() => parts.Clear();

        /// <summary>A builder saying "this object belongs to that cell". Called as each cell is made.</summary>
        public static void Register(Vector2Int cell, Transform part)
        {
            if (!parts.TryGetValue(cell, out List<Transform> list)) parts[cell] = list = new List<Transform>();
            list.Add(part);
        }

        /// <summary>
        /// Regroups every registered part, bakes the far copies and wires up the streamer. Returns how many cells
        /// there are and how many triangles the far copies came to.
        /// </summary>
        public static (int cells, int farTriangles) Finish(Transform cityRoot, Material far)
        {
            Transform group = GreyboxFactory.Group("Cells", cityRoot);
            var centres = new List<Vector3>();
            var nearObjects = new List<GameObject>();
            var farObjects = new List<GameObject>();
            int triangles = 0;

            foreach (KeyValuePair<Vector2Int, List<Transform>> entry in parts)
            {
                Vector2Int at = entry.Key;
                Transform cell = GreyboxFactory.Group($"Cell_{at.x}_{at.y}", group);
                Transform near = GreyboxFactory.Group("Near", cell);
                foreach (Transform part in entry.Value) part.SetParent(near, true);

                (GameObject farCopy, int tris) = Far(cell, near, at, far);
                triangles += tris;
                centres.Add(new Vector3((at.x + .5f) * CityPlan.Cell, 0f, (at.y + .5f) * CityPlan.Cell));
                nearObjects.Add(near.gameObject);
                farObjects.Add(farCopy);
            }

            CityStreamer streamer = cityRoot.gameObject.AddComponent<CityStreamer>();
            streamer.EditorSetup(centres.ToArray(), nearObjects.ToArray(), farObjects.ToArray());
            parts.Clear();
            return (centres.Count, triangles);
        }

        // One mesh for the whole cell. Every piece is already in world coordinates on an identity transform, so
        // the combine needs no matrices and the result sits exactly where the near copy does.
        private static (GameObject copy, int triangles) Far(Transform cell, Transform near, Vector2Int at, Material material)
        {
            var combine = new List<CombineInstance>();
            int vertices = 0;
            foreach (MeshFilter filter in near.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                combine.Add(new CombineInstance { mesh = filter.sharedMesh, transform = Matrix4x4.identity });
                vertices += filter.sharedMesh.vertexCount;
            }
            if (combine.Count == 0) return (null, 0);

            var mesh = new Mesh { name = $"Far_{at.x}_{at.y}" };
            if (vertices > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(combine.ToArray(), true, false);
            mesh.RecalculateBounds();
            UnityEditor.AssetDatabase.CreateAsset(mesh, $"{CityMesh.Folder}/Far_{at.x}_{at.y}.asset");

            GameObject copy = CityMesh.Render("Far", cell, mesh, material);
            // Off until the streamer asks for it: left on, it would draw through the near copy it is a copy of,
            // which is both a waste and two faces in the same plane.
            copy.SetActive(false);
            return (copy, mesh.triangles.Length / 3);
        }
    }
}
