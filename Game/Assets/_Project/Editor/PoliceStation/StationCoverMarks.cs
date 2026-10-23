using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Furniture and hiding places whose one solid box is fuller than what is drawn on purpose: a table or desk is solid
    /// to the floor so nobody walks under its top between the legs, a bunk or gym rack through its open frame, a barred
    /// cell or a cabinet is one occupancy shell behind its bars or door. The rebuild's invisible-collider audit would
    /// call those invisible walls; this pass runs the audit's own coverage test (sample points against the piece's drawn
    /// parts) and marks each one that falls short with <see cref="IntentionalInvisibleCollider"/> and the reason.
    /// </summary>
    public static class StationCoverMarks
    {
        private const float Tolerance = .12f, MinCoverage = .75f;

        public static int Apply(Transform root)
        {
            Physics.SyncTransforms();
            int marked = 0;
            foreach (BoxCollider box in root.GetComponentsInChildren<BoxCollider>(true))
            {
                if (!box.enabled || box.isTrigger || box.GetComponentInParent<IntentionalInvisibleCollider>() != null) continue;
                var drawn = new List<Bounds>();
                foreach (Renderer r in box.GetComponentsInChildren<Renderer>(true))
                {
                    if (!r.enabled || r.shadowCastingMode == ShadowCastingMode.ShadowsOnly || r.sharedMaterial == null) continue;
                    Bounds b = r.bounds;
                    b.Expand(Tolerance * 2f);
                    drawn.Add(b);
                }
                if (drawn.Count == 0 || Coverage(box, drawn) >= MinCoverage) continue;
                bool hiding = box.GetComponentInParent<HidingLocker>() != null || box.GetComponentInChildren<HidingLocker>() != null;
                box.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup(hiding
                    ? "Hiding place: one solid occupancy shell behind its bars or door; the player hides through the interaction."
                    : "Furniture footprint: solid to the floor so nobody walks under or through its open frame (legs, rails, posts).");
                marked++;
            }
            return marked;
        }

        // The audit's own test: a 3 x 3 x 3 grid through the oriented box, counted inside the drawn parts' bounds.
        private static float Coverage(BoxCollider box, List<Bounds> drawn)
        {
            int inside = 0, total = 0;
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                Vector3 f = new Vector3(x, y, z) / 3f + Vector3.one / 6f - Vector3.one * .5f;
                Vector3 p = box.transform.TransformPoint(box.center + Vector3.Scale(f, box.size));
                total++;
                foreach (Bounds b in drawn)
                    if (b.Contains(p)) { inside++; break; }
            }
            return inside / (float)total;
        }
    }
}
