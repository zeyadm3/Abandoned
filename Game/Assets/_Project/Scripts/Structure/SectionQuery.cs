using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>The structural section right under a point (a player's feet), for HUD warnings and tips.</summary>
    public static class SectionQuery
    {
        private static readonly RaycastHit[] Hits = new RaycastHit[4];

        public static StructuralSection Under(Vector3 feet, float reach = 1.5f)
        {
            int mask = 1 << Mathf.Max(0, GameLayers.StructureLayer);
            int count = Physics.RaycastNonAlloc(feet + Vector3.up * 0.3f, Vector3.down, Hits, reach, mask, QueryTriggerInteraction.Ignore);
            StructuralSection best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                StructuralSection s = Hits[i].collider.GetComponentInParent<StructuralSection>();
                if (s == null || s.IsCollapsed || Hits[i].distance >= bestDistance) continue;
                best = s;
                bestDistance = Hits[i].distance;
            }
            return best;
        }
    }
}
