using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuild step (0.14.0, user: "you cannot pass through any object"): every visible object with volume
    /// that a player can reach gets collision, whichever builder placed it and whatever it is. It runs
    /// after a level is dressed and before its NavMesh is baked, so monsters path round the same things.
    ///
    /// The colliders follow the model, not its bounding box: the mesh is read and only the parts of its
    /// footprint that have geometry at body height become solid. A counter becomes one box; an entrance
    /// canopy becomes two columns and stays open underneath.
    ///
    /// Left alone: flat things (litter, decals, kerbs lower than a step), thin things (posters, sign faces),
    /// things above head height, soft vegetation, structural sections (they own their collision), anything
    /// that moves, and anything already inside a collider. An object that would block a route the level
    /// must keep clear is left walk-through, marked <see cref="WalkThroughObject"/> and reported.
    /// <see cref="Audit"/> re-checks the finished level and lists whatever can still be walked through.
    /// </summary>
    public static class SolidObjectsPass
    {
        // Lower than this is stepped over, not walked through (the same height fitted furniture used in 0.13.5).
        private const float MinHeight = 0.3f;
        private const float MinThickness = 0.08f;
        // Whose lowest point is higher than this above its floor is overhead: nobody walks into it.
        private const float Reach = 2.1f;
        // An object with this share of its volume already inside colliders is already solid.
        private const float Covered = 0.5f;
        private const float Storey = 4f;
        // Footprint grid (m), and how full the footprint must be for one plain box to be the honest shape.
        private const float Cell = 0.2f, SolidFill = 0.7f;
        private const int MaxCells = 96, MaxBoxes = 10, MaxTriangles = 60000;

        // Plants give way; a box round a patch of weeds would be an invisible wall.
        private static readonly HashSet<string> Soft = new() { "Overgrowth", "LeafScatter" };
        // Top-level groups nobody can stand in.
        private static readonly string[] OutOfReach = { "Roof" };

        /// <summary>Makes everything solid. <paramref name="routeConflict"/> names what a box would block, or returns null.</summary>
        public static int Apply(Transform level, string levelName, System.Func<Bounds, string> routeConflict = null) =>
            Guarded(level, levelName, true, routeConflict);

        /// <summary>Lists what can still be walked through. Reported, never fatal.</summary>
        public static int Audit(Transform level, string levelName) => Guarded(level, levelName, false, null);

        // A model this pass can't make sense of must not cost the whole rebuild: whatever was made solid before
        // the failure stays solid, and the failure is reported.
        private static int Guarded(Transform level, string levelName, bool fix, System.Func<Bounds, string> routeConflict)
        {
            try
            {
                return Scan(level, levelName, fix, routeConflict);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"[MallValidation] {levelName}: the solid objects {(fix ? "pass" : "audit")} stopped early; see the exception above.");
                return 0;
            }
        }

        private static int Scan(Transform level, string levelName, bool fix, System.Func<Bounds, string> routeConflict)
        {
            Physics.SyncTransforms();
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            int made = 0, boxes = 0, kept = 0, open = 0, intentional = 0;
            var names = new SortedDictionary<string, int>();
            foreach (GameObject item in Items(level))
            {
                var marker = item.GetComponentInParent<WalkThroughObject>();
                if (marker != null)
                {
                    intentional++;
                    if (!fix) Debug.Log($"[MallValidation] {levelName}: intentional walk-through object {Path(item.transform)}: {marker.Reason}");
                    continue;
                }
                if (!Wants(item, level, mask, out Renderer[] parts, out Bounds local)) continue;
                // Mostly air as a whole doesn't mean walk-through: an open structure (the entrance canopy) is solid
                // where its columns are. What counts is whether the boxes it should have are there.
                List<Bounds> shapes = Shapes(item.transform, parts, local);
                shapes.RemoveAll(shape => Coverage(item.transform, shape, mask) >= Covered);
                if (shapes.Count == 0) continue;
                if (!fix)
                {
                    open++;
                    Debug.LogWarning($"[MallValidation] {levelName}: walk-through object {Path(item.transform)} at {item.transform.position}.");
                    continue;
                }
                // Each box on its own: a canopy's columns stand either side of the entrance, the gap between isn't theirs.
                string blocked = null;
                if (routeConflict != null)
                    foreach (Bounds shape in shapes)
                    {
                        blocked = routeConflict(World(item.transform, new List<Bounds> { shape }));
                        if (blocked != null) break;
                    }
                if (blocked != null)
                {
                    item.AddComponent<WalkThroughObject>().EditorSetup(blocked);
                    kept++;
                    Debug.LogWarning($"[MallValidation] {levelName}: left walk-through, {blocked}: {Path(item.transform)} at {item.transform.position}.");
                    continue;
                }
                foreach (Bounds shape in shapes)
                {
                    var box = item.AddComponent<BoxCollider>();
                    box.center = shape.center;
                    box.size = shape.size;
                }
                made++;
                boxes += shapes.Count;
                names[item.name] = names.TryGetValue(item.name, out int seen) ? seen + 1 : 1;
            }
            if (fix)
                Debug.Log($"[MallValidation] {levelName}: solid objects: {made} made solid ({boxes} boxes), " +
                          $"{kept} left walk-through to keep a route clear, {intentional} marked walk-through. Made solid: " +
                          (names.Count == 0 ? "nothing" : string.Join(", ", names.OrderBy(n => n.Key).Select(n => $"{n.Key} x{n.Value}"))) + ".");
            else
                Debug.Log($"[MallValidation] {levelName}: walk-through objects: {open} problem(s), {intentional} intentional.");
            return fix ? made : open;
        }

        // Placed models (a holder with a "Geometry" child, from MeridianArt.Place and CustomMallArt.Place),
        // then every other mesh on its own (primitives and greybox pieces).
        private static List<GameObject> Items(Transform level)
        {
            var holders = new HashSet<Transform>();
            foreach (Transform t in level.GetComponentsInChildren<Transform>())
                if (t.name == "Geometry" && t.parent != null && t.parent != level) holders.Add(t.parent);
            var items = holders.Select(h => h.gameObject).ToList();
            foreach (MeshRenderer r in level.GetComponentsInChildren<MeshRenderer>())
            {
                if (!r.enabled) continue;
                bool placed = false;
                for (Transform t = r.transform; t != null && !placed; t = t.parent) placed = holders.Contains(t);
                if (!placed) items.Add(r.gameObject);
            }
            return items;
        }

        private static bool Wants(GameObject item, Transform level, int mask, out Renderer[] parts, out Bounds local)
        {
            parts = null;
            local = default;
            Transform t = item.transform;
            if (item.layer == GameLayers.DebrisLayer || item.layer == LayerMask.NameToLayer("Ignore Raycast")) return false;
            if (Soft.Contains(item.name)) return false;
            if (item.GetComponentInParent<StructuralSection>() != null) return false;
            if (item.GetComponentInParent<Rigidbody>() != null || item.GetComponentInParent<NetworkObject>() != null) return false;
            for (Transform p = t; p != null && p != level; p = p.parent)
                if (p.parent == level && OutOfReach.Contains(p.name)) return false;
            // A mirrored model can't carry a box collider; none of the builders mirror, so this is only a guard.
            Vector3 scale = t.lossyScale;
            if (scale.x <= 0f || scale.y <= 0f || scale.z <= 0f) return false;

            parts = Parts(item);
            if (parts.Length == 0) return false;
            local = LocalBounds(parts, t);
            Vector3 size = Vector3.Scale(local.size, scale);
            if (Mathf.Min(size.x, Mathf.Min(size.y, size.z)) < MinThickness) return false;
            Bounds world = World(t, new List<Bounds> { local });
            if (world.size.y < MinHeight) return false;
            if (world.min.y - FloorBelow(world.min.y) > Reach) return false;
            return Coverage(t, local, mask) < Covered;
        }

        // The most detailed level of a model with LODs; otherwise every mesh it has.
        private static Renderer[] Parts(GameObject item)
        {
            if (item.TryGetComponent(out LODGroup group))
            {
                LOD[] lods = group.GetLODs();
                if (lods.Length > 0 && lods[0].renderers != null)
                    return lods[0].renderers.Where(r => r != null && r.enabled).ToArray();
            }
            if (item.transform.Find("Geometry") != null)
                return item.GetComponentsInChildren<MeshRenderer>().Where(r => r.enabled).Cast<Renderer>().ToArray();
            return item.TryGetComponent(out MeshRenderer own) && own.enabled ? new Renderer[] { own } : System.Array.Empty<Renderer>();
        }

        private static float FloorBelow(float y) => Mathf.Floor((y + 0.35f) / Storey) * Storey;

        // Share of a 3 x 3 x 3 grid of points through the object that already sit inside a collider.
        private static float Coverage(Transform t, Bounds local, int mask)
        {
            int inside = 0;
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                Vector3 f = new Vector3(x, y, z) / 3f + Vector3.one / 6f - Vector3.one * 0.5f;
                if (Physics.CheckSphere(t.TransformPoint(local.center + Vector3.Scale(f, local.size)), 0.03f, mask, QueryTriggerInteraction.Ignore))
                    inside++;
            }
            return inside / 27f;
        }

        /// <summary>
        /// The object's collision as boxes in its own space. The mesh is rasterised onto a grid over its
        /// footprint; a cell counts when the model has geometry there at body height. A mostly full
        /// footprint is one box round the whole model; otherwise the full cells are merged into a few boxes.
        /// </summary>
        private static List<Bounds> Shapes(Transform t, Renderer[] parts, Bounds local)
        {
            var whole = new List<Bounds> { local };
            // Tipped-over models and unreadable ones keep the plain box.
            if (Vector3.Dot(t.up, Vector3.up) < 0.99f) return whole;
            Vector3 scale = t.lossyScale;
            float bottom = t.TransformPoint(local.min).y;
            float reachTop = local.min.y + (FloorBelow(bottom) + Reach + 0.05f - bottom) / scale.y;

            for (float cell = Cell; cell <= Cell * 4.01f; cell *= 2f)
            {
                float cx = cell / scale.x, cz = cell / scale.z;
                int nx = Mathf.Max(1, Mathf.CeilToInt(local.size.x / cx)), nz = Mathf.Max(1, Mathf.CeilToInt(local.size.z / cz));
                if (nx > MaxCells || nz > MaxCells) continue;
                var full = new bool[nx, nz];
                var top = new float[nx, nz];
                if (!Rasterise(t, parts, local, cx, cz, reachTop, full, top)) return whole;
                FillInside(full, top, nx, nz, local.max.y);
                int count = 0;
                foreach (bool cellFull in full) if (cellFull) count++;
                if (count == 0 || count >= nx * nz * SolidFill) return whole;
                List<Bounds> merged = Merge(full, top, nx, nz, local, cx, cz);
                if (merged.Count <= MaxBoxes) return merged;
            }
            return whole;
        }

        // Marks the cells under every triangle, sampling each triangle finer than the grid.
        private static bool Rasterise(Transform t, Renderer[] parts, Bounds local, float cx, float cz, float reachTop,
            bool[,] full, float[,] top)
        {
            int nx = full.GetLength(0), nz = full.GetLength(1);
            float step = Mathf.Min(cx, cz) * 0.5f;
            foreach (Renderer part in parts)
            {
                if (!part.TryGetComponent(out MeshFilter filter) || filter.sharedMesh == null) return false;
                Vector3[] vertices = filter.sharedMesh.vertices;
                int[] triangles = filter.sharedMesh.triangles;
                if (vertices.Length == 0 || triangles.Length == 0 || triangles.Length / 3 > MaxTriangles) return false;
                Matrix4x4 toItem = t.worldToLocalMatrix * part.transform.localToWorldMatrix;
                for (int i = 0; i < vertices.Length; i++) vertices[i] = toItem.MultiplyPoint3x4(vertices[i]);
                for (int i = 0; i + 2 < triangles.Length; i += 3)
                {
                    Vector3 a = vertices[triangles[i]], u = vertices[triangles[i + 1]] - a, v = vertices[triangles[i + 2]] - a;
                    int steps = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(u.magnitude, v.magnitude) / step), 1, 240);
                    for (int p = 0; p <= steps; p++)
                    for (int q = 0; q <= steps - p; q++)
                    {
                        Vector3 point = a + u * (p / (float)steps) + v * (q / (float)steps);
                        int x = Mathf.Clamp(Mathf.FloorToInt((point.x - local.min.x) / cx), 0, nx - 1);
                        int z = Mathf.Clamp(Mathf.FloorToInt((point.z - local.min.z) / cz), 0, nz - 1);
                        top[x, z] = Mathf.Max(top[x, z], point.y - local.min.y);
                        if (point.y <= reachTop) full[x, z] = true;
                    }
                }
            }
            return true;
        }

        // A hollow shape (a cabinet's four sides) is solid all the way through: whatever empty cells can't
        // be reached from the edge of the grid are inside it.
        private static void FillInside(bool[,] full, float[,] top, int nx, int nz, float height)
        {
            var outside = new bool[nx, nz];
            var open = new Stack<Vector2Int>();
            void Visit(int x, int z)
            {
                if (x < 0 || z < 0 || x >= nx || z >= nz || outside[x, z] || full[x, z]) return;
                outside[x, z] = true;
                open.Push(new Vector2Int(x, z));
            }
            for (int x = 0; x < nx; x++) { Visit(x, 0); Visit(x, nz - 1); }
            for (int z = 0; z < nz; z++) { Visit(0, z); Visit(nx - 1, z); }
            while (open.Count > 0)
            {
                Vector2Int c = open.Pop();
                Visit(c.x + 1, c.y);
                Visit(c.x - 1, c.y);
                Visit(c.x, c.y + 1);
                Visit(c.x, c.y - 1);
            }
            float tallest = 0f;
            foreach (float h in top) tallest = Mathf.Max(tallest, h);
            for (int x = 0; x < nx; x++)
            for (int z = 0; z < nz; z++)
            {
                if (full[x, z] || outside[x, z]) continue;
                full[x, z] = true;
                top[x, z] = tallest;
            }
        }

        // Greedy rectangles: grow each along x, then along z while the whole row is full.
        private static List<Bounds> Merge(bool[,] full, float[,] top, int nx, int nz, Bounds local, float cx, float cz)
        {
            var used = new bool[nx, nz];
            var boxes = new List<Bounds>();
            for (int z = 0; z < nz; z++)
            for (int x = 0; x < nx; x++)
            {
                if (!full[x, z] || used[x, z]) continue;
                int w = 1;
                while (x + w < nx && full[x + w, z] && !used[x + w, z]) w++;
                int d = 1;
                while (z + d < nz && Enumerable.Range(x, w).All(i => full[i, z + d] && !used[i, z + d])) d++;
                float height = 0f;
                for (int i = x; i < x + w; i++)
                for (int j = z; j < z + d; j++)
                {
                    used[i, j] = true;
                    height = Mathf.Max(height, top[i, j]);
                }
                Vector3 min = new(local.min.x + x * cx, local.min.y, local.min.z + z * cz);
                Vector3 max = new(Mathf.Min(local.max.x, local.min.x + (x + w) * cx), local.min.y + Mathf.Max(height, 0.01f),
                    Mathf.Min(local.max.z, local.min.z + (z + d) * cz));
                var box = new Bounds();
                box.SetMinMax(min, max);
                boxes.Add(box);
            }
            return boxes;
        }

        private static Bounds LocalBounds(Renderer[] parts, Transform space)
        {
            var result = new Bounds();
            bool first = true;
            foreach (Renderer part in parts)
            {
                Bounds own = part.localBounds;
                Matrix4x4 toSpace = space.worldToLocalMatrix * part.transform.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = toSpace.MultiplyPoint3x4(own.center + Vector3.Scale(own.extents, Corner(corner)));
                    if (first) { result = new Bounds(point, Vector3.zero); first = false; }
                    else result.Encapsulate(point);
                }
            }
            return result;
        }

        private static Bounds World(Transform t, List<Bounds> shapes)
        {
            var result = new Bounds();
            bool first = true;
            foreach (Bounds shape in shapes)
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = t.TransformPoint(shape.center + Vector3.Scale(shape.extents, Corner(corner)));
                    if (first) { result = new Bounds(point, Vector3.zero); first = false; }
                    else result.Encapsulate(point);
                }
            return result;
        }

        private static Vector3 Corner(int i) => new((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f);

        private static string Path(Transform t)
        {
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            return path;
        }
    }
}
