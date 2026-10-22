using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// TestMap showcase structure: what a two-storey timber-and-steel warehouse is actually made of. Joists under every
    /// upper floor tile (inside its Visual, so they go when it collapses), steel girders on the columns, a concrete roof
    /// on steel beams, skirting, open steel railings and stair stringers. None of it changes a collider the game uses:
    /// railings keep their solid panel, everything added here is visual only unless it says otherwise.
    /// </summary>
    public static class ShowcaseStructure
    {
        private const float JoistWidth = .075f, JoistDepth = .22f, JoistSpacing = 1f;
        public const float UnderJoists = 4f - .3f - JoistDepth; // ground floor ceiling height under the joists (3.48)

        /// <summary>Timber joists under an upper tile, as children of its scaled Visual box (centre, size given).</summary>
        public static void Joists(Transform visual, float tile, float slab)
        {
            Vector3 scale = visual.localScale;
            float y = -slab / 2f - JoistDepth / 2f; // below the slab, relative to the Visual's centre
            for (float x = -tile / 2f + JoistSpacing / 2f; x < tile / 2f; x += JoistSpacing)
            {
                GameObject joist = Box($"Joist_{x:0.0}", visual, Vector3.zero, Vector3.one, ShowcaseMaterials.Joist, false);
                joist.transform.localPosition = new Vector3(x / scale.x, y / scale.y, 0f);
                joist.transform.localScale = new Vector3(JoistWidth / scale.x, JoistDepth / scale.y, (tile - .01f) / scale.z);
            }
        }

        /// <summary>Steel girders under the upper floor's tile seams, carried by the columns (visual only).</summary>
        public static void Girders(Transform parent, float tile)
        {
            const float depth = .3f, width = .18f;
            float y = UnderJoists - depth / 2f;
            Material steel = ShowcaseMaterials.Steel;
            // z = 4 and 12 run the length of the floor beside the atrium; z = 8 crosses only where there is floor.
            Box("Girder_z4", parent, new Vector3(2f * tile, y, tile), new Vector3(4f * tile, depth, width), steel, false);
            Box("Girder_z12", parent, new Vector3(2f * tile, y, 3f * tile), new Vector3(4f * tile, depth, width), steel, false);
            Box("Girder_z8_W", parent, new Vector3(tile / 2f, y, 2f * tile), new Vector3(tile, depth, width), steel, false);
            Box("Girder_z8_E", parent, new Vector3(3.5f * tile, y, 2f * tile), new Vector3(tile, depth, width), steel, false);
            MarkStatic(parent);
        }

        /// <summary>The roof slab (solid) on steel beams and purlins, over the whole footprint and its brick skin.</summary>
        public static void Roof(Transform parent, Vector3 min, Vector3 max, float y, float overhang)
        {
            Vector3 size = max - min + new Vector3(overhang * 2f, 0f, overhang * 2f);
            Box("RoofSlab", parent, new Vector3((min.x + max.x) / 2f, y + .175f, (min.z + max.z) / 2f), new Vector3(size.x, .35f, size.z), ShowcaseMaterials.Roof);
            Material steel = ShowcaseMaterials.Steel;
            for (float x = min.x + 4f; x < max.x - .1f; x += 4f)
                Box($"RoofBeam_{x:0}", parent, new Vector3(x, y - .14f, (min.z + max.z) / 2f), new Vector3(.16f, .28f, max.z - min.z), steel, false);
            for (float z = min.z + 4f; z < max.z - .1f; z += 4f)
                Box($"Purlin_{z:0}", parent, new Vector3((min.x + max.x) / 2f, y - .1f, z), new Vector3(max.x - min.x, .2f, .1f), steel, false);
            MarkStatic(parent);
        }

        /// <summary>A 12 cm skirting board along an inside wall face, owned by the tile it stands on.</summary>
        public static void Skirting(Transform parent, Vector3 from, Vector3 to, Vector3 inward, StructuralSection support)
        {
            const float tall = .12f, thick = .015f, gap = .0025f;
            Vector3 along = to - from;
            Vector3 centre = (from + to) / 2f + inward * (gap + thick / 2f) + Vector3.up * tall / 2f;
            Vector3 size = Mathf.Abs(along.x) > Mathf.Abs(along.z)
                ? new Vector3(Mathf.Abs(along.x), tall, thick)
                : new Vector3(thick, tall, Mathf.Abs(along.z));
            GameObject board = Box("Skirting", parent, centre, size, ShowcaseMaterials.Skirting, false);
            if (support != null) board.AddComponent<SectionProp>().EditorSetup(support);
        }

        /// <summary>Open steel guard rail: posts, top and middle rails and a toe board, inside an invisible solid panel.</summary>
        public static void Railing(Transform panel, float length, float height, bool alongX)
        {
            Material paint = ShowcaseMaterials.SafetyRail;
            Vector3 Along(float a, float y, float across = 0f) => alongX ? new Vector3(a, y, across) : new Vector3(across, y, a);
            Vector3 Size(float a, float y, float across) => alongX ? new Vector3(a, y, across) : new Vector3(across, y, a);
            float bottom = -height / 2f, rail = length - .06f; // rails stop short of the ends, so corners don't overlap
            Box("TopRail", panel, Along(0f, height / 2f - .025f), Size(rail, .05f, .05f), paint, false);
            Box("MidRail", panel, Along(0f, 0f), Size(rail, .04f, .035f), paint, false);
            Box("ToeBoard", panel, Along(0f, bottom + .05f), Size(rail, .1f, .02f), paint, false);
            for (float a = -length / 2f + .03f; a < length / 2f - .1f; a += 1f)
                Box($"Post_{a:0.0}", panel, Along(a, bottom + (height - .05f) / 2f), Size(.045f, height - .05f, .045f), paint, false);
        }

        /// <summary>Steel stringers and handrails on both sides of one straight stair segment (in its root's space).</summary>
        public static void StairSides(Transform visual, float run, float rise, float width, float stepRise)
        {
            float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float slope = Mathf.Sqrt(run * run + rise * rise);
            Quaternion tilt = Quaternion.Euler(-angle, 0f, 0f);
            Material steel = ShowcaseMaterials.Steel, paint = ShowcaseMaterials.SafetyRail;
            float Pitch(float z) => stepRise + z * rise / run; // the line through the step nosings
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (width / 2f + .04f);
                GameObject stringer = Box(side < 0 ? "Stringer_L" : "Stringer_R", visual, new Vector3(x, Pitch(run / 2f) - .2f, run / 2f),
                    new Vector3(.08f, .32f, slope), steel, false);
                stringer.transform.localRotation = tilt;
                float rx = side * (width / 2f + .11f);
                GameObject handrail = Box(side < 0 ? "Handrail_L" : "Handrail_R", visual, new Vector3(rx, Pitch(run / 2f) + .9f, run / 2f),
                    new Vector3(.05f, .05f, slope), paint, false);
                handrail.transform.localRotation = tilt;
                for (float z = .3f; z < run; z += 1f)
                    Box($"Baluster_{z:0.0}", visual, new Vector3(rx, Pitch(z) + .35f, z), new Vector3(.04f, 1.1f, .04f), paint, false);
            }
        }

        /// <summary>A painted floor stripe 3 mm above the floor (never on its plane).</summary>
        public static void Stripe(Transform parent, Vector3 from, Vector3 to, float width, Material paint)
        {
            Vector3 along = to - from;
            Vector3 size = Mathf.Abs(along.x) > Mathf.Abs(along.z) ? new Vector3(Mathf.Abs(along.x), .002f, width) : new Vector3(width, .002f, Mathf.Abs(along.z));
            GameObject stripe = Box("Stripe", parent, (from + to) / 2f + Vector3.up * .004f, size, paint, false);
            stripe.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
