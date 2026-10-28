using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// What is solid around a box, for the rebuild's questions: is this stretch of wall face buried in a neighbour, is
    /// there a floor under it, a ceiling over it, sky above it. Boxes drawn by the map (any upright box renderer) are
    /// answered exactly; everything else solid (mesh colliders, terrain) through physics. Invisible box colliders are
    /// ignored on purpose: they are blockers, not surfaces.
    /// </summary>
    public sealed class ArchSpace
    {
        private const float Cell = 4f;
        private readonly Dictionary<Vector3Int, List<ArchBox>> grid = new();
        private readonly HashSet<Collider> own = new();
        private readonly List<ArchBox> scratch = new();
        private int stamp;
        private readonly Dictionary<ArchBox, int> seen = new();

        public ArchSpace(IEnumerable<ArchBox> boxes)
        {
            foreach (ArchBox b in boxes)
            {
                Bounds w = b.WorldBounds;
                Vector3Int lo = CellOf(w.min), hi = CellOf(w.max);
                for (int x = lo.x; x <= hi.x; x++)
                for (int y = lo.y; y <= hi.y; y++)
                for (int z = lo.z; z <= hi.z; z++)
                {
                    var key = new Vector3Int(x, y, z);
                    if (!grid.TryGetValue(key, out List<ArchBox> list)) grid[key] = list = new List<ArchBox>();
                    list.Add(b);
                }
                foreach (Collider c in b.Transform.GetComponents<Collider>()) own.Add(c);
            }
            Physics.SyncTransforms();
        }

        private static Vector3Int CellOf(Vector3 p) =>
            new(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.y / Cell), Mathf.FloorToInt(p.z / Cell));

        private List<ArchBox> Near(Bounds area)
        {
            scratch.Clear();
            stamp++;
            Vector3Int lo = CellOf(area.min), hi = CellOf(area.max);
            for (int x = lo.x; x <= hi.x; x++)
            for (int y = lo.y; y <= hi.y; y++)
            for (int z = lo.z; z <= hi.z; z++)
            {
                if (!grid.TryGetValue(new Vector3Int(x, y, z), out List<ArchBox> list)) continue;
                foreach (ArchBox b in list)
                {
                    if (seen.TryGetValue(b, out int s) && s == stamp) continue;
                    seen[b] = stamp;
                    scratch.Add(b);
                }
            }
            return scratch;
        }

        /// <summary>Something solid at the point (other than <paramref name="except"/>).</summary>
        public bool Solid(Vector3 p, ArchBox except = null)
        {
            foreach (ArchBox b in Near(new Bounds(p, Vector3.one * .02f)))
                if (b != except && b.Contains(p)) return true;
            foreach (Collider c in Physics.OverlapSphere(p, .01f, ~0, QueryTriggerInteraction.Ignore))
                if (!own.Contains(c) && c is not BoxCollider) return true;
            return false;
        }

        /// <summary>The stretches of the segment a→b (t in 0..1) that are open, i.e. not inside any other box. Physics
        /// geometry is sampled every 10 cm.</summary>
        public List<Vector2> Open(Vector3 a, Vector3 b, ArchBox except)
        {
            var covered = new List<Vector2>();
            var area = new Bounds(a, Vector3.zero);
            area.Encapsulate(b);
            area.Expand(.05f);
            foreach (ArchBox box in Near(area))
                if (box != except && box.Clip(a, b, out float t0, out float t1) && t1 - t0 > 1e-4f) covered.Add(new Vector2(t0, t1));
            float length = Vector3.Distance(a, b);
            int samples = Mathf.Max(2, Mathf.CeilToInt(length / .1f));
            for (int i = 0; i < samples; i++)
            {
                float t = (i + .5f) / samples;
                foreach (Collider c in Physics.OverlapSphere(Vector3.Lerp(a, b, t), .01f, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (own.Contains(c) || c is BoxCollider) continue;
                    covered.Add(new Vector2(t - .5f / samples, t + .5f / samples));
                    break;
                }
            }
            covered.Sort((x, y) => x.x.CompareTo(y.x));
            var open = new List<Vector2>();
            float at = 0f;
            foreach (Vector2 c in covered)
            {
                if (c.x > at) open.Add(new Vector2(at, Mathf.Min(c.x, 1f)));
                at = Mathf.Max(at, c.y);
                if (at >= 1f) break;
            }
            if (at < 1f) open.Add(new Vector2(at, 1f));
            open.RemoveAll(o => (o.y - o.x) * length < .03f);
            return open;
        }

        /// <summary>Fraction of the segment a→b that is open.</summary>
        public float OpenShare(Vector3 a, Vector3 b, ArchBox except)
        {
            float sum = 0f;
            foreach (Vector2 o in Open(a, b, except)) sum += o.y - o.x;
            return sum;
        }

        /// <summary>Anything within <paramref name="distance"/> along the ray (boxes exactly, other geometry by physics).</summary>
        public bool Hit(Vector3 origin, Vector3 direction, float distance, ArchBox except)
        {
            Vector3 end = origin + direction.normalized * distance;
            var area = new Bounds(origin, Vector3.zero);
            area.Encapsulate(end);
            foreach (ArchBox b in Near(area))
                if (b != except && b.Clip(origin, end, out _, out _)) return true;
            foreach (RaycastHit h in Physics.RaycastAll(origin, direction, distance, ~0, QueryTriggerInteraction.Ignore))
                if (!own.Contains(h.collider) && h.collider is not BoxCollider) return true;
            return false;
        }
    }
}
