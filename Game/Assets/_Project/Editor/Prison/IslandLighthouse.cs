using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using O = Abandoned.EditorTools.IslandKit.Opening;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The lighthouse beside the warden's house: a square white tower, 16 m of ramps climbing round a solid core in
    /// the dark (monsters can follow you up), the lamp room at the top with the lens on its pedestal and the switch
    /// that starts it, and a lean-to store at its foot holding the spare Fresnel lens, the island's heaviest prize.
    /// While the lamp turns (<see cref="LighthouseBeam"/>) two beams sweep the island and dazzle what they cross.
    /// </summary>
    public static class IslandLighthouse
    {
        private static Rect T => L.Lighthouse;
        private const float Wall = .5f, Corridor = 2f, StepRise = 1.6f, Y = L.PlateauY;

        public static void Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Transform g = Group("Lighthouse", root);
            Shell(Group("Tower", g), art);
            Ramps(Group("Ramps", g), art);
            LampRoom(Group("LampRoom", g), art, loot);
            Store(Group("LensStore", g), art, loot);
        }

        private static float In(float v, bool max) => max ? v - Wall : v + Wall;

        private static void Shell(Transform g, IslandPrisonArt art)
        {
            float h = L.LighthouseGallery - Y;
            float x0 = T.xMin, x1 = T.xMax, z0 = T.yMin, z1 = T.yMax;
            // The door at the south-west corner, where the ramps start; slit windows up every side.
            var south = new List<O> { O.Door(x0 + Wall + Corridor * .5f, 1.8f, 2.4f) };
            var north = new List<O>();
            var west = new List<O>();
            var east = new List<O>();
            for (float y = 4f; y < h - 1f; y += 4.2f)
            {
                south.Add(O.Window(T.center.x + 1.5f, .4f, y, y + 1.2f));
                north.Add(O.Window(T.center.x - 1f, .4f, y + 1f, y + 2.2f));
                west.Add(O.Window(T.center.y, .4f, y + 2f, y + 3.2f));
                east.Add(O.Window(T.center.y + 1f, .4f, y + 3f, y + 4.2f));
            }
            K.WallX(g, "South", x0, x1, z0 + Wall * .5f, Y, h, Wall, art.Facade, south.ToArray());
            K.WallX(g, "North", x0, x1, z1 - Wall * .5f, Y, h, Wall, art.Facade, north.ToArray());
            K.WallZ(g, "West", z0 + Wall, z1 - Wall, x0 + Wall * .5f, Y, h, Wall, art.Facade, west.ToArray());
            K.WallZ(g, "East", z0 + Wall, z1 - Wall, x1 - Wall * .5f, Y, h, Wall, art.Facade, east.ToArray());
            // The core the ramps wind round, solid all the way up to the lamp room's floor.
            float c0x = In(x0, false) + Corridor, c1x = In(x1, true) - Corridor, c0z = In(z0, false) + Corridor, c1z = In(z1, true) - Corridor;
            K.Span("Core", g, new Vector3(c0x, Y, c0z), new Vector3(c1x, L.LighthouseGallery, c1z), art.FacadeDark);
            // A black band at the top and the gallery's cornice: it reads as a lighthouse from across the island.
            // A black band and the gallery's cornice as strips round the outside (never a box through the tower).
            float gy = L.LighthouseGallery;
            foreach ((float o, float y0, float y1, Material m) in new[] { (.03f, gy - 2f, gy - .25f, art.Hull), (.9f, gy - .25f, gy - .02f, art.FacadeDark) })
            {
                K.Span("Ring_S", g, new Vector3(x0 - o, y0, z0 - o), new Vector3(x1 + o, y1, z0), m, false);
                K.Span("Ring_N", g, new Vector3(x0 - o, y0, z1), new Vector3(x1 + o, y1, z1 + o), m, false);
                K.Span("Ring_W", g, new Vector3(x0 - o, y0, z0), new Vector3(x0, y1, z1), m, false);
                K.Span("Ring_E", g, new Vector3(x1, y0, z0), new Vector3(x1 + o, y1, z1), m, false);
            }
            K.Railing(g, "GalleryRail_S", new Vector3(x0 - .8f, L.LighthouseGallery, z0 - .8f), new Vector3(x1 + .8f, L.LighthouseGallery, z0 - .8f), 1f, art.Steel, false);
            K.Railing(g, "GalleryRail_N", new Vector3(x0 - .8f, L.LighthouseGallery, z1 + .8f), new Vector3(x1 + .8f, L.LighthouseGallery, z1 + .8f), 1f, art.Steel, false);
            K.Railing(g, "GalleryRail_W", new Vector3(x0 - .8f, L.LighthouseGallery, z0 - .8f), new Vector3(x0 - .8f, L.LighthouseGallery, z1 + .8f), 1f, art.Steel, false);
            K.Railing(g, "GalleryRail_E", new Vector3(x1 + .8f, L.LighthouseGallery, z0 - .8f), new Vector3(x1 + .8f, L.LighthouseGallery, z1 + .8f), 1f, art.Steel, false);
            MarkStatic(g);
        }

        // Corners of the ring (south-west, south-east, north-east, north-west) and the ramps between them.
        private static Vector3 Corner(int i)
        {
            float w = In(T.xMin, false) + Corridor * .5f, e = In(T.xMax, true) - Corridor * .5f;
            float s = In(T.yMin, false) + Corridor * .5f, n = In(T.yMax, true) - Corridor * .5f;
            return (i % 4) switch { 0 => new Vector3(w, 0f, s), 1 => new Vector3(e, 0f, s), 2 => new Vector3(e, 0f, n), _ => new Vector3(w, 0f, n) };
        }

        private static int Flights => Mathf.RoundToInt((L.LighthouseGallery - Y) / StepRise);

        private static void Ramps(Transform g, IslandPrisonArt art)
        {
            for (int k = 0; k <= Flights; k++)
            {
                Vector3 c = Corner(k);
                float y = Y + k * StepRise;
                // The landing at each turn (the ground one is the floor itself).
                if (k > 0) K.Span($"Landing_{k}", g, new Vector3(c.x - Corridor * .5f, y - .3f, c.z - Corridor * .5f),
                    new Vector3(c.x + Corridor * .5f, y, c.z + Corridor * .5f), art.Board);
                if (k == Flights) break;
                Vector3 next = Corner(k + 1), dir = (next - c).normalized;
                Vector3 foot = c + dir * (Corridor * .5f) + Vector3.up * y;
                float run = (next - c).magnitude - Corridor;
                GameObject ramp = K.Ramp($"Ramp_{k}", g, foot, dir, run, StepRise, Corridor, .3f, art.Board);
                ramp.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
                // A handrail on the open (core) side: the drop is only to the turn below, but it reads.
                Vector3 coreSide = Vector3.Cross(Vector3.up, dir) * -(Corridor * .5f - .1f);
                K.Railing(g, $"Rail_{k}", foot + coreSide, foot + dir * run + Vector3.up * StepRise + coreSide, .95f, art.Steel, false);
            }
            K.Span("GroundFloor", g, new Vector3(In(T.xMin, false), Y - .02f, In(T.yMin, false)), new Vector3(In(T.xMax, true), Y + .04f, In(T.yMax, true)), art.Facade);
            MarkStatic(g);
        }

        private static void LampRoom(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float y = L.LighthouseGallery;
            float x0 = In(T.xMin, false), x1 = In(T.xMax, true), z0 = In(T.yMin, false), z1 = In(T.yMax, true);
            float ex = x1 - Corridor, sz = z0 + Corridor, nz = z1 - Corridor;
            // The floor covers the north strip and the core; the stairwell (south and east corridors) stays open, railed.
            K.Span("Floor_N", g, new Vector3(x0, y - .3f, nz), new Vector3(x1, y, z1), art.Board);
            K.Span("Floor_Core", g, new Vector3(x0, y - .3f, sz), new Vector3(ex, y, nz), art.Board);
            K.Railing(g, "WellRail_S", new Vector3(x0, y, sz), new Vector3(ex, y, sz), 1f, art.Steel);
            K.Railing(g, "WellRail_E", new Vector3(ex, y, sz), new Vector3(ex, y, nz), 1f, art.Steel);
            // The lantern: glazing all round above the tower walls, a copper cap.
            Transform lantern = Group("Lantern", g);
            const float glass = 2.6f;
            foreach ((Vector3 a, Vector3 b) in new[] { (new Vector3(T.xMin, y, T.yMin), new Vector3(T.xMax, y + glass, T.yMin + .05f)),
                (new Vector3(T.xMin, y, T.yMax - .05f), new Vector3(T.xMax, y + glass, T.yMax)),
                (new Vector3(T.xMin, y, T.yMin), new Vector3(T.xMin + .05f, y + glass, T.yMax)),
                (new Vector3(T.xMax - .05f, y, T.yMin), new Vector3(T.xMax, y + glass, T.yMax)) })
                K.Span("Glazing", lantern, a, b, art.Glass);
            for (float x = T.xMin; x <= T.xMax + .01f; x += 2f)
            {
                K.Span("Mullion", lantern, new Vector3(x - .05f, y, T.yMin - .03f), new Vector3(x + .05f, y + glass, T.yMin + .08f), art.Hull, false);
                K.Span("Mullion", lantern, new Vector3(x - .05f, y, T.yMax - .08f), new Vector3(x + .05f, y + glass, T.yMax + .03f), art.Hull, false);
            }
            K.Span("Cap", lantern, new Vector3(T.xMin - .3f, y + glass, T.yMin - .3f), new Vector3(T.xMax + .3f, y + glass + .3f, T.yMax + .3f), art.Hull);
            PolishAssets.Shape("Dome", lantern, new Vector3(T.center.x, y + glass + .3f, T.center.y), new Vector3(T.width, 2f, T.height), art.Hull, true);
            Box("Vent", lantern, new Vector3(T.center.x, y + glass + 1.6f, T.center.y), new Vector3(.5f, .8f, .5f), art.Hull, false);

            // The lens on its pedestal over the core, turning on its bath; two beams opposite each other.
            Vector3 pedestal = new((x0 + ex) * .5f, y, (sz + nz) * .5f + .4f);
            Box("Pedestal", g, pedestal + Vector3.up * .5f, new Vector3(1f, 1f, 1f), art.Hull);
            Transform head = Group("LampHead", g);
            head.position = pedestal + Vector3.up * 1.6f;
            Box("Lens", head, Vector3.zero, new Vector3(1.1f, 1.2f, 1.1f), art.Glass, false);
            Box("LensFrame", head, Vector3.zero, new Vector3(1.16f, .08f, 1.16f), art.Brass, false);
            GameObject bulb = Box("Bulb", head, Vector3.zero, new Vector3(.35f, .45f, .35f), art.Lamp, false);
            var beams = new List<Light>();
            var shown = new List<Renderer> { bulb.GetComponent<Renderer>() };
            foreach (float side in new[] { 1f, -1f })
            {
                Transform aim = Group(side > 0 ? "Beam_A" : "Beam_B", head);
                aim.localRotation = Quaternion.LookRotation(new Vector3(0f, -.07f, side));
                Light beam = aim.gameObject.AddComponent<Light>();
                beam.type = LightType.Spot;
                beam.range = LighthouseConfig.Current.Range;
                beam.spotAngle = 9f;
                beam.innerSpotAngle = 4f;
                beam.intensity = LighthouseConfig.Current.BeamIntensity;
                beam.color = new Color(1f, .95f, .82f);
                beam.shadows = LightShadows.None;
                beam.enabled = false;
                beams.Add(beam);
                // The beam you see in the fog: two crossed planes of lit air, 40 m long.
                for (int i = 0; i < 2; i++)
                {
                    GameObject shaft = Box("Shaft", aim, new Vector3(0f, 0f, 21f), new Vector3(i == 0 ? 1.8f : .01f, i == 0 ? .01f : 1.8f, 40f), art.LightShaft, false);
                    Renderer r = shaft.GetComponent<Renderer>();
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    shown.Add(r);
                }
            }
            // Off until the lamp is started (LighthouseBeam shows them): never a beam frozen in the dark.
            foreach (Renderer r in shown) r.enabled = false;
            g.gameObject.AddComponent<LighthouseBeam>().EditorSetup(L.LighthouseSector, head, beams.ToArray(), shown.ToArray());
            g.gameObject.AddComponent<PowerIndependentLight>();
            Switch(g, new Vector3(x0 + .3f, y, nz + 1f), art);
            Point(loot, "Lighthouse_LampRoom", IslandLootItems.Maritime, x0 + 2.4f, y + .05f, nz + 1f, CarryClass.Pocket, CarryClass.OneHand);
            Point(loot, "Lighthouse_Turn", IslandLootItems.Maritime, Corner(6).x, Y + 6f * StepRise + .05f, Corner(6).z, CarryClass.Pocket, CarryClass.OneHand);
        }

        // The lamp's switch: a breaker-style cabinet on the west wall, facing into the room.
        private static void Switch(Transform g, Vector3 at, IslandPrisonArt art)
        {
            Transform root = Group("LampSwitch", g);
            root.SetPositionAndRotation(at, Quaternion.LookRotation(Vector3.right));
            var size = new Vector3(.6f, .8f, .26f);
            float z = size.z * .5f + .02f, y = 1f + size.y * .5f;
            Box("Body", root, new Vector3(0f, y, z), size, art.Hull, false);
            Box("Door", root, new Vector3(0f, y, size.z + .03f), new Vector3(size.x - .06f, size.y - .06f, .02f), art.Steel, false);
            Transform pivot = Group("Handle", root);
            pivot.localPosition = new Vector3(size.x * .5f + .06f, y, size.z * .5f);
            Box("Arm", pivot, new Vector3(0f, .16f, 0f), new Vector3(.05f, .32f, .05f), art.Steel, false);
            Box("Grip", pivot, new Vector3(0f, .34f, .04f), new Vector3(.12f, .06f, .1f), art.Warning, false);
            pivot.localRotation = Quaternion.Euler(40f, 0f, 0f);
            GameObject lamp = Primitive(PrimitiveType.Sphere, "Lamp", root, new Vector3(-.2f, 1f + size.y - .15f, size.z + .06f), Vector3.one * .07f,
                GetMaterial("Greybox_BreakerLamp", new Color(.9f, .15f, .1f)), false);
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, y, z);
            solid.size = size;
            root.gameObject.AddComponent<BreakerPanel>().EditorSetupLighthouse(L.LighthouseSector, pivot, lamp.GetComponent<Renderer>());
        }

        // A lean-to store against the tower's east side; its south door is wide enough for the spare lens.
        private static void Store(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float x0 = T.xMax, x1 = T.xMax + 6f, z0 = T.yMin, z1 = T.yMin + 6f, h = 3.4f;
            K.WallX(g, "South", x0, x1, z0 + .15f, Y, h, .3f, art.Facade, O.Door(x0 + 3f, 2.8f, 2.7f));
            K.WallX(g, "North", x0, x1, z1 - .15f, Y, h, .3f, art.Facade);
            K.WallZ(g, "East", z0, z1, x1 - .15f, Y, h, .3f, art.Facade, O.Window(z0 + 3f, 1f, 1.2f, 2.2f));
            K.Span("Roof", g, new Vector3(x0, Y + h, z0 - .3f), new Vector3(x1 + .3f, Y + h + .2f, z1 + .3f), art.Rust);
            K.Span("Floor", g, new Vector3(x0, Y - .02f, z0), new Vector3(x1, Y + .04f, z1), art.Floor);
            Box("Crate_A", g, new Vector3(x1 - .9f, Y + .45f, z1 - .9f), new Vector3 (1.2f, .9f, 1.2f), art.Board);
            Box("Crate_B", g, new Vector3(x1 - .8f, Y + 1.15f, z1 - .9f), new Vector3(.8f, .5f, .8f), art.Board);
            Box("OilDrum", g, new Vector3(x0 + .7f, Y + .45f, z1 - .7f), new Vector3(.6f, .9f, .6f), art.Rust);
            Point(loot, "Lighthouse_SpareLens", IslandLootItems.Maritime, x0 + 2.8f, Y + .05f, z0 + 3.4f, CarryClass.Heavy, CarryClass.Huge, true);
            Point(loot, "Lighthouse_StoreShelf", IslandLootItems.Maritime, x1 - .8f, Y + 1.45f, z1 - .9f, CarryClass.Pocket, CarryClass.OneHand);
            MarkStatic(g);
        }

        private static void Point(List<LootPointSpec> loot, string name, string tag, float x, float y, float z, CarryClass min, CarryClass max, bool jackpot = false) =>
            loot.Add(new LootPointSpec(name, tag, new Vector3(x, y, z), min, max, jackpot));
    }
}
