using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gallows Rock's island and the quay the boat leaves from (Docs/CITY_PLAN.md step 6). The prison has been a
    /// planned site since 2026-10-19 for one reason — the old world had no coast to put it off — and the drawing's
    /// sea is what finally gives it one.
    ///
    /// This is only the ground the island stands on and the mainland end of the crossing. The prison's own landform,
    /// cliffs and buildings live in its scene (Editor/Prison), which streams in over this pad the way every other
    /// map streams in over its own.
    /// </summary>
    public static class CityIsland
    {
        /// <summary>The pad the prison scene stands on, and the rock skirt that carries it down to the water.</summary>
        private const float PadHalf = 300f, SkirtSteps = 6f, SkirtReach = 120f;
        /// <summary>The quay: how far it stands out from the shore, and how wide.</summary>
        private const float QuayOut = 70f, QuayHalfWidth = 26f;

        /// <summary>Builds the island and the quay. Returns how far the crossing is, in metres.</summary>
        public static float Build(Transform parent, Material rock, Material paving)
        {
            Transform group = GreyboxFactory.Group("GallowsRock", parent);
            CityPlan.Site prison = Prison();
            Vector2 at = prison.Centre;

            var land = new CityMesh.Piece();
            land.Flat(at.x - PadHalf, at.y - PadHalf, at.x + PadHalf, at.y + PadHalf, CityPlan.GroundY);
            // A stepped rock skirt rather than a cliff: the island reads as land from the quay, and from the boat
            // the lowest steps are under the water line anyway.
            for (int i = 0; i < SkirtSteps; i++)
            {
                float out0 = PadHalf + SkirtReach * (i / SkirtSteps);
                float out1 = PadHalf + SkirtReach * ((i + 1f) / SkirtSteps);
                float y0 = Mathf.Lerp(CityPlan.GroundY, CityPlan.SeaY - 3f, i / SkirtSteps);
                float y1 = Mathf.Lerp(CityPlan.GroundY, CityPlan.SeaY - 3f, (i + 1f) / SkirtSteps);
                Ring(land, at, out0, out1, y0, y1);
            }
            CityMesh.Render("Island", group, land.Save("Island_GallowsRock"), rock);

            var collision = new GameObject("Island_Collision");
            collision.transform.SetParent(group, false);
            BoxCollider box = collision.AddComponent<BoxCollider>();
            box.center = new Vector3(at.x, CityPlan.GroundY - .25f, at.y);
            box.size = new Vector3(PadHalf * 2f, .5f, PadHalf * 2f);

            float crossing = Quay(group, at, paving);
            Debug.Log($"[CityIsland] Gallows Rock at ({at.x:0}, {at.y:0}), {PadHalf * 2f:0} m of ground, " +
                      $"{crossing:0} m from the quay.");
            return crossing;
        }

        // The mainland end: a quay pushed out from the nearest point of the coast, facing the island.
        private static float Quay(Transform group, Vector2 island, Material paving)
        {
            Vector2 shore = NearestShore(island);
            Vector2 toward = (island - shore).normalized;
            Vector2 across = new(-toward.y, toward.x);
            Vector2 head = shore + toward * QuayOut;

            var piece = new CityMesh.Piece();
            piece.Quad(At(shore - across * QuayHalfWidth), At(head - across * QuayHalfWidth),
                       At(head + across * QuayHalfWidth), At(shore + across * QuayHalfWidth));
            CityMesh.Render("Quay", group, piece.Save("Island_Quay"), paving);

            var collision = new GameObject("Quay_Collision");
            collision.transform.SetParent(group, false);
            collision.transform.rotation = Quaternion.LookRotation(new Vector3(toward.x, 0f, toward.y), Vector3.up);
            BoxCollider box = collision.AddComponent<BoxCollider>();
            Vector2 middle = (shore + head) * .5f;
            box.center = collision.transform.InverseTransformPoint(
                new Vector3(middle.x, CityPlan.GroundY - .25f, middle.y));
            box.size = new Vector3(QuayHalfWidth * 2f, .5f, QuayOut);

            var marker = new GameObject("BoatDock").transform;
            marker.SetParent(group, false);
            marker.SetPositionAndRotation(new Vector3(head.x, CityPlan.GroundY, head.y),
                                          Quaternion.LookRotation(new Vector3(toward.x, 0f, toward.y), Vector3.up));
            return Vector2.Distance(head, island);
        }

        // The point on the drawn coastline closest to the island, so the quay is never round a headland from it.
        private static Vector2 NearestShore(Vector2 island)
        {
            Vector2[] coast = CityPlan.Coast;
            Vector2 best = coast[0];
            float closest = float.MaxValue;
            for (int i = 0; i < coast.Length - 1; i++)
            {
                Vector2 a = coast[i], ab = coast[i + 1] - a;
                float t = Mathf.Clamp01(Vector2.Dot(island - a, ab) / Mathf.Max(ab.sqrMagnitude, .001f));
                Vector2 on = a + ab * t;
                float d = Vector2.Distance(on, island);
                if (d >= closest) continue;
                closest = d;
                best = on;
            }
            return best;
        }

        private static void Ring(CityMesh.Piece piece, Vector2 at, float out0, float out1, float y0, float y1)
        {
            Vector2[] inner = Square(at, out0), outer = Square(at, out1);
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                piece.Quad(new Vector3(inner[i].x, y0, inner[i].y), new Vector3(outer[i].x, y1, outer[i].y),
                           new Vector3(outer[j].x, y1, outer[j].y), new Vector3(inner[j].x, y0, inner[j].y));
            }
        }

        private static Vector2[] Square(Vector2 at, float half) => new[]
        {
            at + new Vector2(-half, -half), at + new Vector2(-half, half),
            at + new Vector2(half, half), at + new Vector2(half, -half),
        };

        private static CityPlan.Site Prison()
        {
            foreach (CityPlan.Site s in CityPlan.Sites)
                if (s.Scene == "Prison") return s;
            return CityPlan.Sites[0];
        }

        private static Vector3 At(Vector2 p) => new(p.x, CityPlan.GroundY, p.y);
    }
}
