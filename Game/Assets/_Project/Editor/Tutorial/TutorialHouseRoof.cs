using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Three low gables in dark asphalt shingle, the way a ranch of the period was framed (the west wing's ridge runs
    /// north-south, the main band's and the suite's east-west), with a heavy coat of December snow, gutters hung
    /// with icicles, the brick chimney and the furnace flue. Above the ceilings: never walked on, left out of the
    /// NavMesh, so no colliders up here.
    /// </summary>
    public static class TutorialHouseRoof
    {
        // The overhang drops 30 cm at this pitch. Its underside must stay above the adjoining wing's ceiling.
        public const float Eave = L.Ceiling + .45f;
        private const float Overhang = .5f, Thickness = .14f;
        // Rise per metre of span: the low ranch pitch (about 4 in 12).
        public const float Pitch = .3f;
        /// <summary>Where the training house's chimney stands, at eave height (the street copies move theirs about).</summary>
        public static readonly Vector3 ChimneyAt = new(L.Living.center.x - 1.2f, Eave, L.Band.yMax - 1.6f);

        public static void Build(Transform root, TutorialHouseArt art)
        {
            Transform roof = Group("Roof", root);
            Gables(roof, art, full: true);
            Chimney(Group("Chimney", roof), ChimneyAt, art);
            // The furnace's flue pipe through the west wing roof.
            Primitive(PrimitiveType.Cylinder, "FurnaceFlue", roof, new Vector3(1.5f, Eave + 1.3f, TutorialHouseBasement.FurnaceZ), new Vector3(.18f, .6f, .18f), art.DarkMetal, false);
            Primitive(PrimitiveType.Cylinder, "FlueCap", roof, new Vector3(1.5f, Eave + 1.95f, TutorialHouseBasement.FurnaceZ), new Vector3(.3f, .05f, .3f), art.DarkMetal, false);
        }

        /// <summary>
        /// The house's three roofs. Without <paramref name="full"/> (a street copy seen from down the road): no
        /// icicles or attic vents.
        /// </summary>
        public static void Gables(Transform roof, TutorialHouseArt art, bool full)
        {
            Gable(roof, "WestWingRoof", L.WestWing, ridgeAlongZ: true, rise: L.WestWing.width * Pitch, Eave, art, full);
            Gable(roof, "BandRoof", L.Band, ridgeAlongZ: false, rise: L.Band.height * Pitch, Eave, art, full);
            Gable(roof, "SuiteRoof", L.Suite, ridgeAlongZ: false, rise: L.Suite.height * Pitch, Eave, art, full);
        }

        /// <summary>A shingled gable over <paramref name="r"/>, meeting the walls' top plate at <paramref name="eave"/>.</summary>
        public static void Gable(Transform parent, string name, Rect r, bool ridgeAlongZ, float rise, float eave, TutorialHouseArt art, bool full)
        {
            Transform g = Group(name, parent);
            float span = ridgeAlongZ ? r.width : r.height, length = (ridgeAlongZ ? r.height : r.width) + Overhang * 2f;
            // The planes meet the top plate at the wall line and run on past it for the overhang.
            float run = span * .5f, half = run + Overhang;
            float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float drop = rise * half / run;
            float slope = Mathf.Sqrt(half * half + drop * drop);
            Vector3 centre = new(r.center.x, eave, r.center.y);
            foreach (float side in new[] { -1f, 1f })
            {
                // Each plane runs from the ridge down past the wall to its eave, centred halfway.
                Vector3 across = ridgeAlongZ ? new Vector3(side, 0f, 0f) : new Vector3(0f, 0f, side);
                Vector3 mid = centre + across * (half * .5f) + Vector3.up * (rise - drop * .5f);
                Quaternion tilt = ridgeAlongZ ? Quaternion.Euler(0f, 0f, -side * angle) : Quaternion.Euler(side * angle, 0f, 0f);
                Vector3 size = ridgeAlongZ ? new Vector3(slope, Thickness, length) : new Vector3(length, Thickness, slope);
                Box("Shingles", g, mid, size, art.Shingles, false).transform.localRotation = tilt;
                // Snow lies thick on the field, thinning toward the eave where the gutter shed it.
                Vector3 normal = tilt * Vector3.up;
                GameObject snow = Box("SnowLoad", g, mid + normal * (Thickness * .5f + .07f) + across * .12f,
                    ridgeAlongZ ? new Vector3(slope - .3f, .14f, length - .1f) : new Vector3(length - .1f, .14f, slope - .3f), art.Snow, false);
                snow.transform.localRotation = tilt;
                // Gutter along the eave, and icicles hanging off it.
                Vector3 eaveLine = centre + across * half + Vector3.up * (rise - drop - .06f);
                Vector3 gutterSize = ridgeAlongZ ? new Vector3(.12f, .12f, length) : new Vector3(length, .12f, .12f);
                if (!full) Box("Gutter", g, eaveLine, gutterSize, art.Trim, false);
                else
                {
                    // Intersecting ranch wings have valleys, not exposed eaves inside the adjoining room.
                    int segments = Mathf.CeilToInt(length / .5f);
                    float segment = length / segments;
                    for (int i = 0; i < segments; i++)
                    {
                        float t = -length * .5f + (i + .5f) * segment;
                        Vector3 point = eaveLine + (ridgeAlongZ ? Vector3.forward : Vector3.right) * t;
                        if (InsideHouse(point)) continue;
                        Box("Gutter", g, point, ridgeAlongZ ? new Vector3(.12f, .12f, segment) : new Vector3(segment, .12f, .12f), art.Trim, false);
                    }
                }
                if (!full) continue;
                for (float t = -length * .5f + .3f; t < length * .5f; t += .55f)
                {
                    float hang = .12f + Mathf.Abs(Mathf.Sin(t * 3.1f)) * .3f;
                    Vector3 at = eaveLine + (ridgeAlongZ ? new Vector3(0f, -.06f - hang * .5f, t) : new Vector3(t, -.06f - hang * .5f, 0f));
                    if (InsideHouse(at)) continue;
                    Box("Icicle", g, at, new Vector3(.035f, hang, .035f), art.Ice, false);
                }
            }
            // Gable ends: a siding triangle at each end of the ridge, from the top plate to the ridge.
            foreach (float end in new[] { -1f, 1f })
            {
                Vector3 at = ridgeAlongZ ? new Vector3(r.center.x, eave, end > 0 ? r.yMax : r.yMin) : new Vector3(end > 0 ? r.xMax : r.xMin, eave, r.center.y);
                Quaternion turn = ridgeAlongZ ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
                TutorialHouseMeshes.Shape("GableEnd", g, TutorialHouseMeshes.Gable(), at, new Vector3(span, rise - .05f, L.ExteriorWall), art.Siding, false, turn);
                if (!full) continue;
                // A louvred attic vent near the peak.
                Vector3 vent = at + Vector3.up * (rise * .45f) + (ridgeAlongZ ? new Vector3(0f, 0f, end * .14f) : new Vector3(end * .14f, 0f, 0f));
                Box("AtticVent", g, vent, ridgeAlongZ ? new Vector3(.5f, .4f, .04f) : new Vector3(.04f, .4f, .5f), art.Trim, false);
            }
            Box("RidgeCap", g, centre + Vector3.up * (rise + .02f), ridgeAlongZ ? new Vector3(.22f, .08f, length) : new Vector3(length, .08f, .22f), art.Shingles, false);
            Box("RidgeSnow", g, centre + Vector3.up * (rise + .1f), ridgeAlongZ ? new Vector3(.3f, .1f, length - .2f) : new Vector3(length - .2f, .1f, .3f), art.Snow, false);
        }

        private static bool InsideHouse(Vector3 point)
        {
            var p = new Vector2(point.x, point.z);
            return L.WestWing.Contains(p) || L.Band.Contains(p) || L.Suite.Contains(p);
        }

        /// <summary>The brick chimney, its foot at <paramref name="at"/> (eave height, where it comes through the roof).</summary>
        public static void Chimney(Transform c, Vector3 at, TutorialHouseArt art)
        {
            Box("BrickStack", c, at + Vector3.up * 1.4f, new Vector3(.85f, 2.8f, .7f), art.Brick, false);
            Box("Crown", c, at + Vector3.up * 2.85f, new Vector3(.95f, .1f, .8f), art.Concrete, false);
            Box("CrownSnow", c, at + Vector3.up * 2.93f, new Vector3(.9f, .06f, .75f), art.Snow, false);
            Box("Flashing", c, at + Vector3.up * .7f, new Vector3(.9f, .1f, .75f), art.DarkMetal, false);
        }
    }
}
