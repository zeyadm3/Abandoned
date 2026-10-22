using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Which sides of a broken floor were still held when it fell: by the intact floor next to it or by a wall. A held
    /// side keeps a broken lip and the bars that tore out of it; a free side (an atrium edge, a hole already there)
    /// has nothing to hang from. Read from the colliders around the hole at the moment of the break, which every
    /// machine has from the same replicated state.
    /// </summary>
    public sealed class CollapseEdges
    {
        /// <summary>Out of the hole, per side: +x, -x, +z, -z.</summary>
        public static readonly Vector3[] Outward = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        public readonly bool[] Held = new bool[4];
        public readonly StructuralSection[] Neighbour = new StructuralSection[4];
        public Bounds Hole { get; private set; }
        public bool Any => Held[0] || Held[1] || Held[2] || Held[3];

        private static readonly Collider[] Hits = new Collider[16];

        public static CollapseEdges Find(StructuralSection section)
        {
            var edges = new CollapseEdges { Hole = section.SurfaceBounds };
            Bounds hole = edges.Hole;
            if (hole.size.x < 0.5f || hole.size.z < 0.5f) return edges;
            int mask = (1 << 0) | (1 << Mathf.Max(0, GameLayers.StructureLayer));
            float midSlab = hole.max.y - Mathf.Min(hole.size.y, 0.3f) * 0.5f;
            for (int side = 0; side < 4; side++)
            {
                Vector3 o = Outward[side];
                Vector3 edge = new(hole.center.x + o.x * hole.extents.x, midSlab, hole.center.z + o.z * hole.extents.z);
                // A thin box just past the edge, most of its length: floor or wall there means this side was held.
                Vector3 half = o.x != 0f ? new Vector3(0.08f, 0.06f, hole.extents.z * 0.6f) : new Vector3(hole.extents.x * 0.6f, 0.06f, 0.08f);
                int count = Physics.OverlapBoxNonAlloc(edge + o * 0.15f, half, Hits, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    Collider c = Hits[i];
                    if (c.transform.IsChildOf(section.transform)) continue;
                    if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic) continue; // loose things hold nothing
                    if (c.GetComponentInParent<RubbleMound>() != null) continue;
                    edges.Held[side] = true;
                    StructuralSection neighbour = c.GetComponentInParent<StructuralSection>();
                    if (neighbour != null && !neighbour.IsCollapsed) edges.Neighbour[side] = neighbour;
                    if (edges.Neighbour[side] != null) break;
                }
            }
            return edges;
        }

        /// <summary>The held side a point is within <paramref name="band"/> of (the nearest), or -1.</summary>
        public int SideNear(Vector3 point, float band)
        {
            Bounds hole = Hole;
            int best = -1;
            float bestDistance = band;
            for (int side = 0; side < 4; side++)
            {
                if (!Held[side]) continue;
                Vector3 o = Outward[side];
                float distance = o.x > 0f ? hole.max.x - point.x : o.x < 0f ? point.x - hole.min.x
                    : o.z > 0f ? hole.max.z - point.z : point.z - hole.min.z;
                if (distance < 0f || distance >= bestDistance) continue;
                bestDistance = distance;
                best = side;
            }
            return best;
        }
    }
}
