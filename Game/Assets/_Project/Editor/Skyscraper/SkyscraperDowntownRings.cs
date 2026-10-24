using UnityEngine;
using S = Abandoned.EditorTools.SkyscraperDowntownStreets;
using Site = Abandoned.EditorTools.StationSite;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Two rings of mid-rise city round the downtown grid, scene-only (under <c>Exterior/Skyline</c>): where the towers
    /// stop, office blocks, walk-ups, parking decks, shops and churches on the station kit's outer rings, with every avenue
    /// and cross street of the grid running on through both ring roads to the edge (x -486..486, z -464..508). In the open
    /// world the world's own land stands here instead. The rings' ground slabs are made three metres thick, so the world's
    /// survey (which reads only thin slabs) keeps measuring the downtown grid as the site's ground.
    /// </summary>
    public static class SkyscraperDowntownRings
    {
        public const float Road = 12f, SlabDepth = 3f;

        public static void Build(Transform skyline, SkyscraperDowntownMaterials p, StationArt k, StationRingStock stock)
        {
            var spec = new StationRingSpec
            {
                Inner = SkyscraperDowntown.Ground, Road = Road, Seed = 961, Vacancy = .06f, Lot = p.Sidewalk, Yard = k.Grass, LampIntensity = 7f,
            };
            for (int m = -3; m <= 4; m++)
                foreach (int side in new[] { StationRingSpec.South, StationRingSpec.North })
                    spec.Through(side, S.StreetX(m) - S.Half, S.StreetX(m) + S.Half);
            for (int n = -3; n <= 4; n++)
                foreach (int side in new[] { StationRingSpec.West, StationRingSpec.East })
                    spec.Through(side, S.AvenueZ(n) - S.Half, S.AvenueZ(n) + S.Half);
            StationRingPools.District(stock, spec, 9600);
            StationRings.Build(skyline, Site.Exterior(skyline), k, spec, stock);
            StationRings.Settle(Site.Exterior(skyline));
            Thicken(Site.GroundSlabs(skyline));
        }

        // Each slab keeps its top and reaches down SlabDepth metres.
        private static void Thicken(Transform slabs)
        {
            foreach (Transform slab in slabs)
            {
                if (slab.GetComponent<BoxCollider>() == null) continue;
                Vector3 scale = slab.localScale, at = slab.localPosition;
                float top = at.y + scale.y * .5f;
                scale.y = SlabDepth;
                slab.localScale = scale;
                slab.localPosition = new Vector3(at.x, top - SlabDepth * .5f, at.z);
            }
        }
    }
}
