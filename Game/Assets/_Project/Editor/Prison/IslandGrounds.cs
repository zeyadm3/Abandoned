using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using O = Abandoned.EditorTools.IslandKit.Opening;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The plateau between the buildings. The recreation yard west of the cellhouse (high walls, concrete bleachers,
    /// a ball-field backstop, its west side open to the cliff behind a low broken parapet), the water tower (a
    /// landmark from everywhere), the burned officers' club (roofless walls, good cover, a little loot), the low
    /// concrete parapet round the cliff edge with stretches fallen into the sea, and the old fence lines.
    /// </summary>
    public static class IslandGrounds
    {
        private const float Y = L.PlateauY;

        public static void Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Transform g = Group("Grounds", root);
            Yard(Group("RecYard", g), art, loot);
            WaterTower(Group("WaterTower", g), art);
            OfficersClub(Group("OfficersClub", g), art, loot);
            Parapet(Group("CliffParapet", g), art);
            Fences(Group("Fences", g), art);
            Outdoors(g, art, loot);
            MarkStatic(g);
        }

        private static void Point(List<LootPointSpec> loot, string name, string tag, float x, float z, CarryClass min, CarryClass max, float y = Y + .05f, bool jackpot = false) =>
            loot.Add(new LootPointSpec(name, tag, new Vector3(x, y, z), min, max, jackpot));

        private static void Yard(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Rect r = L.RecYard;
            const float h = 6f, t = .6f;
            // North and south walls; the east side is the cellhouse; the west is the cliff.
            K.WallX(g, "North", r.xMin, r.xMax, r.yMax - t * .5f, Y, h, t, art.Facade);
            K.WallX(g, "South", r.xMin, r.xMax, r.yMin + t * .5f, Y, h, t, art.Facade, O.Door(r.xMax - 4f, 3f, 3.2f));
            K.Span("Coping_N", g, new Vector3(r.xMin, Y + h, r.yMax - t - .1f), new Vector3(r.xMax, Y + h + .2f, r.yMax + .1f), art.FacadeDark, false);
            K.Span("Coping_S", g, new Vector3(r.xMin, Y + h, r.yMin - .1f), new Vector3(r.xMax, Y + h + .2f, r.yMin + t + .1f), art.FacadeDark, false);
            // Concrete bleachers stepping up the north wall: a landmark you can stand on.
            for (int i = 0; i < 5; i++)
                K.Span($"Bleacher_{i}", g, new Vector3(r.xMin + 3f, Y, r.yMax - t - 5f + i), new Vector3(r.xMax - 4f, Y + .45f * (i + 1), r.yMax - t - 4f + i), art.FacadeDark);
            // The backstop in the south-west corner: posts and a rusted mesh screen.
            Transform back = Group("Backstop", g);
            foreach ((float x, float z) in new[] { (r.xMin + 2f, r.yMin + 7f), (r.xMin + 5f, r.yMin + 2.5f), (r.xMin + 8.5f, r.yMin + 1.5f) })
                Box("Post", back, new Vector3(x, Y + 2.5f, z), new Vector3(.12f, 5f, .12f), art.Rust);
            K.RotBox("Screen_A", back, new Vector3(r.xMin + 3.5f, Y + 2.5f, r.yMin + 4.75f), new Vector3(.02f, 4.6f, 5.4f), Quaternion.Euler(0f, -34f, 0f), art.Bars);
            K.RotBox("Screen_B", back, new Vector3(r.xMin + 6.75f, Y + 2.5f, r.yMin + 2f), new Vector3(.02f, 4.6f, 3.6f), Quaternion.Euler(0f, -74f, 0f), art.Bars);
            // Worn base paths and a home plate, all floor-level.
            Point(loot, "Yard_Bleachers", IslandLootItems.Contraband, r.center.x, r.yMax - t - 2.4f, CarryClass.Pocket, CarryClass.OneHand, Y + 1.4f);
            Point(loot, "Yard_Plate", IslandLootItems.Contraband, r.xMin + 5f, r.yMin + 6f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Yard_Corner", IslandLootItems.Armoury, r.xMax - 2f, r.yMin + 3f, CarryClass.OneHand, CarryClass.Heavy);
        }

        private static void WaterTower(Transform g, IslandPrisonArt art)
        {
            Rect r = L.WaterTower;
            const float legs = 18f;
            foreach (float x in new[] { r.xMin, r.xMax })
            foreach (float z in new[] { r.yMin, r.yMax })
                Box("Leg", g, new Vector3(x, Y + legs * .5f, z), new Vector3(.45f, legs, .45f), art.Rust);
            for (float y = 4f; y < legs; y += 4.5f)
            {
                K.Span("Ring_S", g, new Vector3(r.xMin, Y + y, r.yMin - .1f), new Vector3(r.xMax, Y + y + .2f, r.yMin + .1f), art.Rust, false);
                K.Span("Ring_N", g, new Vector3(r.xMin, Y + y, r.yMax - .1f), new Vector3(r.xMax, Y + y + .2f, r.yMax + .1f), art.Rust, false);
                K.Span("Ring_W", g, new Vector3(r.xMin - .1f, Y + y, r.yMin), new Vector3(r.xMin + .1f, Y + y + .2f, r.yMax), art.Rust, false);
                K.Span("Ring_E", g, new Vector3(r.xMax - .1f, Y + y, r.yMin), new Vector3(r.xMax + .1f, Y + y + .2f, r.yMax), art.Rust, false);
            }
            GameObject tank = Primitive(PrimitiveType.Cylinder, "Tank", g, new Vector3(r.center.x, Y + legs + 3.5f, r.center.y), new Vector3(r.width + 3f, 3.5f, r.height + 3f), art.Rust, false);
            tank.name = "Tank";
            PolishAssets.Shape("Cap", g, new Vector3(r.center.x, Y + legs + 7f, r.center.y), new Vector3(r.width + 3f, 2.2f, r.height + 3f), art.Rust, true);
            Box("Riser", g, new Vector3(r.center.x, Y + legs * .5f, r.center.y), new Vector3(.7f, legs, .7f), art.Rust);
            K.Span("Walkway", g, new Vector3(r.xMin - 2f, Y + legs - .1f, r.yMin - 2f), new Vector3(r.xMax + 2f, Y + legs, r.yMax + 2f), art.Plate, false);
        }

        // Roofless, burned walls with empty doorways and windows; a chimney; a fallen roof truss inside.
        private static void OfficersClub(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Rect r = L.OfficersClub;
            const float h = 4.5f, t = .35f;
            K.WallX(g, "South", r.xMin, r.xMax, r.yMin + t * .5f, Y, h, t, art.BurntBrick, O.Door(r.center.x, 2.4f, 2.6f),
                O.Window(r.xMin + 3f, 1.6f, 1f, 2.8f), O.Window(r.xMax - 3f, 1.6f, 1f, 2.8f));
            K.WallX(g, "North", r.xMin, r.xMax, r.yMax - t * .5f, Y, h * .7f, t, art.BurntBrick, O.Window(r.center.x - 3f, 1.6f, 1f, 2.8f), O.Door(r.xMax - 3f, 2f, 2.4f));
            K.WallZ(g, "West", r.yMin, r.yMax, r.xMin + t * .5f, Y, h, t, art.BurntBrick, O.Window(r.center.y, 1.6f, 1f, 2.8f));
            K.WallZ(g, "East", r.yMin, r.yMax, r.xMax - t * .5f, Y, h * .55f, t, art.BurntBrick, O.Door(r.center.y, 2.4f, 2.4f));
            K.Span("Chimney", g, new Vector3(r.xMin + .3f, Y, r.yMax - 2.5f), new Vector3(r.xMin + 1.6f, Y + 7f, r.yMax - 1f), art.Brick);
            K.RotBox("FallenTruss", g, new Vector3(r.center.x + 2f, Y + .12f, r.center.y), new Vector3(.3f, .24f, 11f), Quaternion.Euler(0f, 70f, 0f), art.DarkWood);
            K.Patch("Ash", g, new Vector3(r.center.x, Y + .03f, r.center.y), new Vector2(r.width - 3f, r.height - 3f), 0f, art.Scorch);
            Box("Bar", g, new Vector3(r.xMax - 2.5f, Y + .55f, r.center.y + 2f), new Vector3(.8f, 1.1f, 4f), art.DarkWood);
            Point(loot, "Club_Bar", IslandLootItems.Warden, r.xMax - 2.5f, r.center.y + 1f, CarryClass.Pocket, CarryClass.OneHand, Y + 1.15f);
            Point(loot, "Club_Floor", IslandLootItems.Maritime, r.xMin + 4f, r.center.y - 1f, CarryClass.OneHand, CarryClass.TwoHand);
            Point(loot, "Club_Hearth", IslandLootItems.Warden, r.xMin + 2.6f, r.yMax - 2f, CarryClass.TwoHand, CarryClass.Heavy);
        }

        // A knee-high concrete parapet round the cliff top, broken in places: the edge is honest, but it can be walked off.
        private static void Parapet(Transform g, IslandPrisonArt art)
        {
            Rect p = L.Plateau;
            var edges = new List<(Vector3 a, Vector3 b)>
            {
                (new Vector3(p.xMin + .3f, Y, L.RecYard.yMax), new Vector3(p.xMin + .3f, Y, p.yMax - .3f)),
                (new Vector3(p.xMin + .3f, Y, p.yMin + .3f), new Vector3(p.xMin + .3f, Y, L.RecYard.yMin)),
                (new Vector3(p.xMin + .3f, Y, p.yMax - .3f), new Vector3(p.xMax - .3f, Y, p.yMax - .3f)),
                (new Vector3(p.xMax - .3f, Y, 31f), new Vector3(p.xMax - .3f, Y, p.yMax - .3f)),
                (new Vector3(p.xMin + .3f, Y, p.yMin + .3f), new Vector3(88f, Y, p.yMin + .3f)),
                (new Vector3(104f, Y, p.yMin + .3f), new Vector3(L.Terrace.xMin, Y, p.yMin + .3f)),
            };
            // The yard's own cliff side (inside its walls).
            edges.Add((new Vector3(p.xMin + .3f, Y, L.RecYard.yMin + .6f), new Vector3(p.xMin + .3f, Y, L.RecYard.yMax - .6f)));
            int n = 0;
            foreach ((Vector3 a, Vector3 b) in edges)
            {
                Vector3 d = b - a;
                float len = d.magnitude;
                Vector3 dir = d / len;
                for (float s = 0f; s < len - .5f; s += 3f)
                {
                    float h = K.Hash(n++, 141);
                    if (h < .22f) continue;   // fallen away here
                    // Only where the plateau's edge really is (its north corners round away into the slope).
                    Vector3 probe = a + dir * (s + Mathf.Min(3f, len - s) * .5f);
                    if (IslandLandform.Height(probe.x, probe.z) < L.PlateauY - .05f) continue;
                    float run = Mathf.Min(3f, len - s) - .05f;
                    Vector3 mid = a + dir * (s + run * .5f);
                    float tall = h > .85f ? .3f : .55f;
                    Vector3 size = Mathf.Abs(dir.x) > .5f ? new Vector3(run, tall, .35f) : new Vector3(.35f, tall, run);
                    K.WorldBox("Parapet", g, mid + Vector3.up * tall * .5f, size, art.FacadeDark);
                }
            }
        }

        // Old chain-link fence lines on rusted posts, mostly down: they break the plateau up for the eye, not for movement.
        private static void Fences(Transform g, IslandPrisonArt art)
        {
            foreach ((Vector3 a, Vector3 b) in new[]
            {
                (new Vector3(80f, Y, 96f), new Vector3(118f, Y, 96f)), (new Vector3(84f, Y, 50f), new Vector3(84f, Y, 88f)),
            })
            {
                Vector3 d = b - a;
                float len = d.magnitude;
                Vector3 dir = d / len;
                for (float s = 0f; s <= len; s += 3f)
                {
                    float h = K.Hash((int)(a.x + s), (int)a.z);
                    GameObject post = Box("FencePost", g, a + dir * s + Vector3.up * 1.2f, new Vector3(.08f, 2.4f, .08f), art.Rust);
                    if (h < .3f) post.transform.rotation = Quaternion.Euler(h * 120f, 0f, 30f);
                    if (h > .55f && s + 3f <= len)
                    {
                        GameObject mesh = Box("FenceMesh", g, a + dir * (s + 1.5f) + Vector3.up * 1.2f,
                            Mathf.Abs(dir.x) > .5f ? new Vector3(3f, 2.2f, .02f) : new Vector3(.02f, 2.2f, 3f), art.Bars);
                        if (h > .85f) mesh.transform.rotation = Quaternion.Euler(Mathf.Abs(dir.x) > .5f ? 78f : 0f, 0f, Mathf.Abs(dir.x) > .5f ? 0f : 78f);
                    }
                }
            }
        }

        // Things the island left lying about outside: benches, a handcart, coils of rope, a capsized dinghy.
        private static void Outdoors(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            foreach ((float x, float z, float yaw) in new[] { (46f, 30f, 0f), (64f, 30f, 0f), (88f, 60f, 90f), (26f, 94f, 30f) })
            {
                Transform bench = Group("Bench", g);
                bench.SetPositionAndRotation(new Vector3(x, Y, z), Quaternion.Euler(0f, yaw, 0f));
                Box("Seat", bench, new Vector3(0f, .45f, 0f), new Vector3(1.8f, .06f, .45f), art.DarkWood);
                Box("Back", bench, new Vector3(0f, .8f, -.2f), new Vector3(1.8f, .4f, .05f), art.DarkWood, false);
                Box("Legs", bench, new Vector3(0f, .22f, 0f), new Vector3(1.6f, .44f, .3f), art.Rust, false);
            }
            Transform cart = Group("HandCart", g);
            cart.SetPositionAndRotation(new Vector3(70f, Y, 96f), Quaternion.Euler(0f, 25f, 0f));
            Box("Bed", cart, new Vector3(0f, .6f, 0f), new Vector3(1.2f, .5f, 2f), art.DarkWood);
            Primitive(PrimitiveType.Cylinder, "Wheel", cart, new Vector3(.7f, .4f, .3f), new Vector3(.8f, .04f, .8f), art.Rust, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            K.RotBox("Dinghy", g, new Vector3(112f, Y + .4f, 88f), new Vector3(1.6f, .8f, 4f), Quaternion.Euler(0f, 30f, 180f), art.Hull);
            Point(loot, "Plateau_Dinghy", IslandLootItems.Maritime, 110f, 85.5f, CarryClass.OneHand, CarryClass.Heavy);
            Point(loot, "Plateau_Cart", IslandLootItems.Contraband, 71.5f, 98f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Plateau_WaterTower", IslandLootItems.Maritime, L.WaterTower.center.x + 3f, L.WaterTower.center.y, CarryClass.OneHand, CarryClass.TwoHand);
            Point(loot, "Plateau_Terrace", IslandLootItems.Armoury, L.Terrace.center.x - 2f, L.Terrace.center.y, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Plateau_SouthLawn", IslandLootItems.Contraband, 18f, 25f, CarryClass.Pocket, CarryClass.OneHand);
            Point(loot, "Plateau_NorthLawn", IslandLootItems.Maritime, 92f, 112f, CarryClass.OneHand, CarryClass.TwoHand);
            Point(loot, "Quay_Bollards", IslandLootItems.Maritime, 96f, 2.5f, CarryClass.Pocket, CarryClass.TwoHand, .05f);
            Point(loot, "Quay_West", IslandLootItems.Maritime, 61f, 14f, CarryClass.OneHand, CarryClass.Heavy, .05f);
            Point(loot, "Pier_East", IslandLootItems.Maritime, 92f, -18f, CarryClass.Pocket, CarryClass.TwoHand, .05f);
        }
    }
}
