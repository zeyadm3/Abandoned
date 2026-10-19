using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using O = Abandoned.EditorTools.IslandKit.Opening;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The island between and around the buildings, so the outside reads as a place that worked: the cellhouse's
    /// pilasters, downpipes, roof plant and graffiti; the prison wall along the north-west brink with its guard post and
    /// wire; the gatehouse where the road comes onto the plateau; the ruined powerhouse with its tall brick chimney (a
    /// landmark from the boat) and the power line strung on poles from it to every building; sheds, a rusted pickup,
    /// and below the north point the remains of the old jetty, which you can climb down the slope to and walk out on.
    /// </summary>
    public static class IslandExterior
    {
        private const float Y = L.PlateauY;
        public static readonly Rect Powerhouse = new(10f, 22f, 14f, 12f);

        public static void Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Transform g = Group("Exterior", root);
            CellhouseOutside(Group("CellhouseOutside", g), art);
            PrisonWall(Group("PrisonWall", g), art);
            Gatehouse(Group("Gatehouse", g), art, loot);
            PowerhouseRuin(Group("Powerhouse", g), art, loot);
            Sheds(Group("Sheds", g), art, loot);
            Jetty(Group("NorthJetty", g), art, loot);
            MarkStatic(g);
            PowerLine(Group("PowerLine", root), art);
        }

        private static void Point(List<LootPointSpec> loot, string name, string tag, Vector3 at, CarryClass min, CarryClass max) =>
            loot.Add(new LootPointSpec(name, tag, at, min, max, false));

        // Pilasters between the windows, downpipes at the corners, roof plant, a flagpole, faded graffiti low down.
        private static void CellhouseOutside(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            float top = L.CellhouseEaves + .5f;
            bool NearDoor(float at, float door, float width) => Mathf.Abs(at - door) < width * .5f + 1.2f;
            for (float x = c.xMin; x <= c.xMax + .01f; x += 8f)
            {
                if (!NearDoor(x, L.MainDoorX, L.MainDoorWidth)) K.Span("Pilaster_S", g, new Vector3(x - .35f, Y, c.yMin - .45f), new Vector3(x + .35f, top, c.yMin), art.FacadeDark);
                if (!NearDoor(x, L.ServiceDoorX, L.SideDoorWidth)) K.Span("Pilaster_N", g, new Vector3(x - .35f, Y, c.yMax), new Vector3(x + .35f, top, c.yMax + .45f), art.FacadeDark);
            }
            for (float z = c.yMin + 8f; z < c.yMax - .01f; z += 8f)
            {
                if (!NearDoor(z, L.YardDoorZ, L.SideDoorWidth)) K.Span("Pilaster_W", g, new Vector3(c.xMin - .45f, Y, z - .35f), new Vector3(c.xMin, top, z + .35f), art.FacadeDark);
                if (!NearDoor(z, L.EastDoorZ, L.SideDoorWidth)) K.Span("Pilaster_E", g, new Vector3(c.xMax, Y, z - .35f), new Vector3(c.xMax + .45f, top, z + .35f), art.FacadeDark);
            }
            foreach ((float x, float z) in new[] { (c.xMin - .6f, c.yMin - .6f), (c.xMax + .6f, c.yMin - .6f), (c.xMin - .6f, c.yMax + .6f), (c.xMax + .6f, c.yMax + .6f) })
            {
                Primitive(PrimitiveType.Cylinder, "Downpipe", g, new Vector3(x, (Y + top) * .5f, z), new Vector3(.18f, (top - Y) * .5f, .18f), art.Rust, false);
                Box("Hopper", g, new Vector3(x, top - .3f, z), new Vector3(.4f, .4f, .4f), art.Rust, false);
            }
            // Roof: the parapet rail, ventilators, two tanks on stilts, a flagpole over the door.
            float roof = L.CellhouseEaves + .4f;
            foreach ((Vector3 a, Vector3 b) in new[] { (new Vector3(c.xMin, roof, c.yMin + .3f), new Vector3(c.xMax, roof, c.yMin + .3f)),
                (new Vector3(c.xMin, roof, c.yMax - .3f), new Vector3(c.xMax, roof, c.yMax - .3f)),
                (new Vector3(c.xMin + .3f, roof, c.yMin), new Vector3(c.xMin + .3f, roof, c.yMax)), (new Vector3(c.xMax - .3f, roof, c.yMin), new Vector3(c.xMax - .3f, roof, c.yMax)) })
                K.Railing(g, "RoofRail", a, b, 1f, art.Rust, false);
            foreach ((float x, float z) in new[] { (34f, 46f), (42f, 70f), (72f, 52f), (74f, 84f), (36f, 88f) })
            {
                Box("Vent", g, new Vector3(x, roof + .7f, z), new Vector3(1.2f, 1.4f, 1.2f), art.Steel, false);
                Box("VentCap", g, new Vector3(x, roof + 1.5f, z), new Vector3(1.6f, .2f, 1.6f), art.Steel, false);
            }
            foreach (float x in new[] { 36f, 72f })
            {
                foreach (float dx in new[] { -1.2f, 1.2f })
                foreach (float dz in new[] { -1.2f, 1.2f })
                    Box("TankLeg", g, new Vector3(x + dx, roof + 1.5f, 62f + dz), new Vector3(.15f, 3f, .15f), art.Rust, false);
                Primitive(PrimitiveType.Cylinder, "Tank", g, new Vector3(x, roof + 4.2f, 62f), new Vector3(3.2f, 1.6f, 3.2f), art.Rust, false);
            }
            Box("Flagpole", g, new Vector3(L.MainDoorX + 6f, roof + 4f, c.yMin + 1f), new Vector3(.12f, 8f, .12f), art.Steel, false);
            // A fuse box and conduit by the east door; a bench by the main door.
            Box("FuseBox", g, new Vector3(c.xMax + .2f, Y + 1.4f, L.EastDoorZ + 3f), new Vector3(.25f, .8f, .6f), art.Steel, false);
            Box("Conduit", g, new Vector3(c.xMax + .12f, (Y + 1.8f + top) * .5f, L.EastDoorZ + 3f), new Vector3(.08f, top - Y - 1.8f, .08f), art.Steel, false);
            foreach ((Renderer r, int _) in Each(g)) r.shadowCastingMode = r.bounds.size.y > 4f ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        private static IEnumerable<(Renderer, int)> Each(Transform g)
        {
            int i = 0;
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) yield return (r, i++);
        }

        // The wall along the north-west brink beyond the yard, with a gap where the warden's garden path runs, a guard
        // post at its corner and a coil of wire along the top.
        private static void PrisonWall(Transform g, IslandPrisonArt art)
        {
            const float h = 3.6f, t = .5f;
            float x = L.Plateau.xMin + 1.4f;
            // It stops where the plateau's north-west corner rounds away into the slope.
            float end = L.RecYard.yMax + 14f;
            K.Span("Wall_W", g, new Vector3(x - t * .5f, Y, L.RecYard.yMax), new Vector3(x + t * .5f, Y + h, end), art.Facade);
            foreach ((Vector3 a, Vector3 b) in new[] { (new Vector3(x, Y + h + .25f, L.RecYard.yMax), new Vector3(x, Y + h + .25f, end)) })
            {
                Vector3 d = b - a;
                GameObject coil = Primitive(PrimitiveType.Cylinder, "Wire", g, (a + b) * .5f, new Vector3(.5f, d.magnitude * .5f, .5f), art.Bars, false);
                coil.transform.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
            }
            // The guard post: a small concrete box on legs, windows all round, a lamp.
            var post = new Vector3(x + 2.2f, Y, end - 2f);
            foreach (float dx in new[] { -1.4f, 1.4f })
            foreach (float dz in new[] { -1.4f, 1.4f })
                Box("PostLeg", g, post + new Vector3(dx, 2.5f, dz), new Vector3(.3f, 5f, .3f), art.FacadeDark);
            K.Span("PostFloor", g, post + new Vector3(-1.7f, 5f, -1.7f), post + new Vector3(1.7f, 5.25f, 1.7f), art.FacadeDark);
            foreach ((Vector3 a, Vector3 b) in new[] { (new Vector3(-1.7f, 5.25f, -1.7f), new Vector3(1.7f, 6.3f, -1.5f)), (new Vector3(-1.7f, 5.25f, 1.5f), new Vector3(1.7f, 6.3f, 1.7f)),
                (new Vector3(-1.7f, 5.25f, -1.7f), new Vector3(-1.5f, 6.3f, 1.7f)), (new Vector3(1.5f, 5.25f, -1.7f), new Vector3(1.7f, 6.3f, 1.7f)) })
                K.Span("PostParapet", g, post + a, post + b, art.Facade, false);
            K.Span("PostRoof", g, post + new Vector3(-2f, 7.5f, -2f), post + new Vector3(2f, 7.7f, 2f), art.Rust, false);
            foreach (float dx in new[] { -1.6f, 1.6f })
                Box("PostMullion", g, post + new Vector3(dx, 6.9f, -1.6f), new Vector3(.1f, 1.2f, .1f), art.Rust, false);
        }

        // Where the road comes onto the plateau: two gate piers, the gate leaves swung back, a guard booth.
        private static void Gatehouse(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float x = L.Terrace.xMin - 4f, z0 = L.Plateau.yMin + 1.4f, z1 = L.Plateau.yMin + 7.6f;
            Box("Pier_S", g, new Vector3(x, Y + 1.6f, z0), new Vector3(.8f, 3.2f, .8f), art.FacadeDark);
            Box("Pier_N", g, new Vector3(x, Y + 1.6f, z1), new Vector3(.8f, 3.2f, .8f), art.FacadeDark);
            K.RotBox("Leaf_S", g, new Vector3(x - 1.2f, Y + 1.2f, z0 + .8f), new Vector3(.06f, 2.2f, 2.8f), Quaternion.Euler(0f, 62f, 0f), art.Bars);
            K.RotBox("Leaf_N", g, new Vector3(x - 1.4f, Y + 1.2f, z1 - .6f), new Vector3(.06f, 2.2f, 2.8f), Quaternion.Euler(0f, -55f, 3f), art.Bars);
            // East of the gate (the officers' club fills the ground to the west).
            var booth = new Rect(x + .6f, z1 + .8f, 3f, 3f);
            K.WallX(g, "Booth_S", booth.xMin, booth.xMax, booth.yMin + .1f, Y, 2.6f, .2f, art.Facade, O.Window(booth.center.x, 1.6f, 1f, 2f));
            K.WallX(g, "Booth_N", booth.xMin, booth.xMax, booth.yMax - .1f, Y, 2.6f, .2f, art.Facade);
            K.WallZ(g, "Booth_W", booth.yMin, booth.yMax, booth.xMin + .1f, Y, 2.6f, .2f, art.Facade, O.Door(booth.center.y, 1.2f, 2.1f));
            K.WallZ(g, "Booth_E", booth.yMin, booth.yMax, booth.xMax - .1f, Y, 2.6f, .2f, art.Facade, O.Window(booth.center.y, 1.6f, 1f, 2f));
            K.Span("Booth_Roof", g, new Vector3(booth.xMin - .3f, Y + 2.6f, booth.yMin - .3f), new Vector3(booth.xMax + .3f, Y + 2.8f, booth.yMax + .3f), art.Rust);
            Box("Booth_Desk", g, new Vector3(booth.center.x, Y + .45f, booth.yMin + .6f), new Vector3(2f, .9f, .6f), art.DarkWood);
            Point(loot, "Gatehouse_Desk", IslandLootItems.Armoury, new Vector3(booth.center.x, Y + .95f, booth.yMin + .6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // The old powerhouse in the plateau's south-west corner: walls broken to different heights, the roof gone, the
        // dead generator inside, transformers outside, and its chimney, 30 m of brick you can see from the boat.
        private static void PowerhouseRuin(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Rect r = Powerhouse;
            const float t = .45f;
            int n = 0;
            void Broken(string name, Vector3 a, Vector3 b, bool alongX, params O[] openings)
            {
                // Each 2 m of wall stands to its own height: the fire and the wind took the rest.
                float len = alongX ? b.x - a.x : b.z - a.z;
                for (float s = 0f; s < len - .01f; s += 2f)
                {
                    float run = Mathf.Min(2f, len - s), top = 3.4f + K.Hash(n++, 501) * 4f;
                    float mid = s + run * .5f;
                    bool open = false;
                    foreach (O o in openings)
                        if (Mathf.Abs((alongX ? a.x : a.z) + mid - o.At) < o.Width * .5f) open = true;
                    if (open) { if (top > 3.2f) K.Span(name + "_Lintel", g, Off(a, alongX, s, 2.9f), Off(a, alongX, s + run, top, t), art.Brick); continue; }
                    K.Span(name, g, Off(a, alongX, s, 0f), Off(a, alongX, s + run, top, t), art.Brick);
                }
            }
            Broken("Wall_S", new Vector3(r.xMin, Y, r.yMin), new Vector3(r.xMax, Y, r.yMin), true, O.Door(r.center.x + 2f, 3f, 2.9f));
            Broken("Wall_N", new Vector3(r.xMin, Y, r.yMax - t), new Vector3(r.xMax, Y, r.yMax - t), true, O.Door(r.xMax - 3f, 2.4f, 2.9f));
            Broken("Wall_W", new Vector3(r.xMin, Y, r.yMin), new Vector3(r.xMin, Y, r.yMax), false);
            Broken("Wall_E", new Vector3(r.xMax - t, Y, r.yMin), new Vector3(r.xMax - t, Y, r.yMax), false, O.Door(r.center.y, 2.8f, 2.9f));
            K.Span("Floor", g, new Vector3(r.xMin, Y, r.yMin), new Vector3(r.xMax, Y + .03f, r.yMax), art.Floor, false);
            // The generator on its plinth and a fallen roof truss.
            K.Span("GenPlinth", g, new Vector3(r.xMin + 3f, Y, r.yMin + 4f), new Vector3(r.xMin + 9f, Y + .5f, r.yMin + 7.5f), art.FacadeDark);
            Primitive(PrimitiveType.Cylinder, "Generator", g, new Vector3(r.xMin + 6f, Y + 1.6f, r.yMin + 5.75f), new Vector3(2.4f, 2.6f, 2.4f), art.Rust)
                .transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            K.RotBox("FallenTruss", g, new Vector3(r.center.x, Y + .9f, r.yMax - 2.5f), new Vector3(r.width - 1f, .3f, .3f), Quaternion.Euler(0f, 8f, 14f), art.Rust);
            K.Patch("Ash", g, new Vector3(r.center.x, Y + .06f, r.center.y), new Vector2(r.width - 3f, r.height - 3f), 0f, art.Scorch);
            // The chimney at the north-west corner, banded near the top.
            var stack = new Vector3(r.xMin + 1.6f, Y, r.yMax + 1.6f);
            K.Span("Chimney", g, stack + new Vector3(-1.3f, 0f, -1.3f), stack + new Vector3(1.3f, 30f, 1.3f), art.Brick);
            K.Span("ChimneyBand", g, stack + new Vector3(-1.45f, 27f, -1.45f), stack + new Vector3(1.45f, 28f, 1.45f), art.FacadeDark, false);
            K.Span("ChimneyCap", g, stack + new Vector3(-1.5f, 30f, -1.5f), stack + new Vector3(1.5f, 30.4f, 1.5f), art.FacadeDark, false);
            Box("ChimneyLadder", g, stack + new Vector3(1.35f, 15f, 0f), new Vector3(.05f, 30f, .5f), art.Rust, false);
            // Transformers outside the east wall, in a broken fence.
            foreach (float z in new[] { r.yMin + 2f, r.yMin + 5f })
            {
                Box("Transformer", g, new Vector3(r.xMax + 2.2f, Y + .9f, z), new Vector3(1.4f, 1.8f, 1.2f), art.Steel);
                for (int i = 0; i < 3; i++) Box("Insulator", g, new Vector3(r.xMax + 1.8f + i * .4f, Y + 2.05f, z), new Vector3(.1f, .3f, .1f), art.Porcelain, false);
            }
            Point(loot, "Powerhouse_Generator", "workshop", new Vector3(r.xMin + 4f, Y + .55f, r.yMin + 8.6f), CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Powerhouse_Corner", IslandLootItems.Armoury, new Vector3(r.xMax - 1.5f, Y + .05f, r.yMax - 1.5f), CarryClass.OneHand, CarryClass.Heavy);
        }

        private static Vector3 Off(Vector3 a, bool alongX, float s, float y, float t = 0f) =>
            alongX ? new Vector3(a.x + s, a.y + y, a.z + t) : new Vector3(a.x + t, a.y + y, a.z + s);

        private static void Sheds(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            foreach ((Vector3 at, float yaw, string name) in new[] { (new Vector3(84f, Y, 104f), 10f, "Shed_North"), (new Vector3(112.6f, Y, 35.5f), 90f, "Shed_East") })
            {
                Transform shed = Group(name, g);
                shed.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                Box("Back", shed, new Vector3(0f, 1.3f, 1.4f), new Vector3(4f, 2.6f, .12f), art.Board);
                Box("Side_L", shed, new Vector3(-2f, 1.3f, 0f), new Vector3(.12f, 2.6f, 2.8f), art.Board);
                Box("Side_R", shed, new Vector3(2f, 1.3f, 0f), new Vector3(.12f, 2.6f, 2.8f), art.Board);
                Box("Front_L", shed, new Vector3(-1.5f, 1.3f, -1.4f), new Vector3(1f, 2.6f, .12f), art.Board);
                Box("Front_R", shed, new Vector3(1.5f, 1.3f, -1.4f), new Vector3(1f, 2.6f, .12f), art.Board);
                K.RotBox("Roof", shed, new Vector3(0f, 2.8f, 0f), new Vector3(4.4f, .08f, 3.4f), Quaternion.Euler(8f, 0f, 0f), art.Rust, false);
                K.RotBox("Door", shed, new Vector3(-.9f, 1.05f, -1.9f), new Vector3(1.1f, 2.1f, .05f), Quaternion.Euler(0f, 70f, 0f), art.Board, false);
                Box("Shelf", shed, new Vector3(0f, .9f, 1f), new Vector3(3.6f, .05f, .5f), art.Board, false);
                Point(loot, name, "workshop", shed.TransformPoint(new Vector3(.4f, .95f, .7f)), CarryClass.Pocket, CarryClass.OneHand);
            }
            // A pickup left where it died on the south lawn: rust, flat tyres, the bonnet up.
            Transform car = Group("Pickup", g);
            car.SetPositionAndRotation(new Vector3(100f, Y, 23f), Quaternion.Euler(0f, 104f, 2f));
            var carMod = car.gameObject.AddComponent<NavMeshModifier>();
            carMod.overrideArea = true;
            carMod.area = 1;
            Box("Bed", car, new Vector3(0f, .85f, -1.2f), new Vector3(1.9f, .6f, 2.2f), art.Rust);
            Box("Cab", car, new Vector3(0f, 1.25f, .6f), new Vector3(1.9f, 1.3f, 1.5f), art.Rust);
            Box("Bonnet", car, new Vector3(0f, .95f, 1.9f), new Vector3(1.8f, .55f, 1.2f), art.Rust);
            K.RotBox("BonnetLid", car, new Vector3(0f, 1.6f, 1.7f), new Vector3(1.7f, .04f, 1.1f), Quaternion.Euler(-60f, 0f, 0f), art.Rust, false);
            foreach (float x in new[] { -.95f, .95f })
            foreach (float z in new[] { -1.6f, 1.7f })
                Primitive(PrimitiveType.Cylinder, "Wheel", car, new Vector3(x, .3f, z), new Vector3(.7f, .1f, .7f), art.Bars, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Box("Glass", car, new Vector3(0f, 1.55f, 1.36f), new Vector3(1.7f, .6f, .02f), art.Glass, false);
            Point(loot, "Pickup_Bed", IslandLootItems.Maritime, car.TransformPoint(new Vector3(0f, 1.2f, -1.2f)), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Below the north point, where the slope reaches the water: the old jetty's piles marching out to sea and the
        // stretch of deck still standing, low over the water.
        private static void Jetty(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float x = L.Lighthouse.center.x - 12f;
            float shore = L.Plateau.yMax;
            while (shore < L.Plateau.yMax + 120f && IslandLandform.Height(x, shore) > L.SeaY + .4f) shore += 1f;
            float deckY = L.SeaY + 1.5f;
            // The landing: a concrete apron where the slope meets the water, then the deck running north.
            K.Span("Apron", g, new Vector3(x - 2.5f, deckY - 1f, shore - 4f), new Vector3(x + 2.5f, deckY, shore + 1f), art.FacadeDark);
            for (float z = shore + 1f; z < shore + 26f; z += 3f)
            {
                float h = K.Hash((int)z, 601);
                foreach (float dx in new[] { -1.4f, 1.4f })
                    Primitive(PrimitiveType.Cylinder, "Pile", g, new Vector3(x + dx, deckY - 3f, z), new Vector3(.4f, 3.2f + h, .4f), art.DarkWood, false);
                if (z < shore + 14f || h > .6f)
                {
                    GameObject plank = K.Span("Deck", g, new Vector3(x - 1.6f, deckY - .2f, z), new Vector3(x + 1.6f, deckY, z + 3f), art.Pier);
                    plank.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
                    if (z > shore + 8f) plank.transform.rotation = Quaternion.Euler(h * 6f - 3f, 0f, h * 8f - 4f);
                }
            }
            for (float z = shore + 26f; z < shore + 46f; z += 3f)
                foreach (float dx in new[] { -1.4f, 1.4f })
                    if (K.Hash((int)(z * 3 + dx), 602) > .35f)
                        Primitive(PrimitiveType.Cylinder, "Pile", g, new Vector3(x + dx, L.SeaY - 1f, z), new Vector3(.4f, 2.2f, .4f), art.DarkWood, false);
            Box("Bollard", g, new Vector3(x + 1.2f, deckY + .3f, shore + 9f), new Vector3(.3f, .6f, .3f), art.Rust, false);
            Point(loot, "Jetty_End", IslandLootItems.Maritime, new Vector3(x, deckY + .05f, shore + 11f), CarryClass.Pocket, CarryClass.Heavy);
        }

        // Poles from the powerhouse to every building, three sagging wires between each pair, a lamp on some.
        private static void PowerLine(Transform g, IslandPrisonArt art)
        {
            var routes = new[]
            {
                new[] { new Vector2(25.5f, 30f), new Vector2(40f, 31f), new Vector2(60f, 32f), new Vector2(78f, 34f), new Vector2(90f, 27f), new Vector2(104f, 28.6f), new Vector2(122f, 40f) },
                new[] { new Vector2(40f, 31f), new Vector2(27.5f, 47f) },
                new[] { new Vector2(80.5f, 94f), new Vector2(66f, 98f), new Vector2(54.5f, 98.5f), new Vector2(44f, 97.5f) },
                new[] { new Vector2(80.5f, 94f), new Vector2(96f, 95f), new Vector2(110f, 92f), new Vector2(120f, 76f), new Vector2(121f, 56f) },
            };
            const float height = 8.5f;
            var poles = new HashSet<Vector2>();
            foreach (Vector2[] route in routes)
            {
                for (int i = 0; i < route.Length; i++)
                {
                    Vector2 p = route[i];
                    if (poles.Add(p))
                    {
                        float lean = K.Hash((int)p.x, (int)p.y) * 6f - 3f;
                        Transform pole = Group("Pole", g);
                        pole.SetPositionAndRotation(new Vector3(p.x, IslandLandform.Height(p.x, p.y), p.y), Quaternion.Euler(lean, 0f, lean * .5f));
                        Box("Post", pole, new Vector3(0f, height * .5f, 0f), new Vector3(.22f, height, .22f), art.DarkWood);
                        Box("Crossarm", pole, new Vector3(0f, height - .4f, 0f), new Vector3(1.8f, .12f, .12f), art.DarkWood, false);
                        for (int k = -1; k <= 1; k++) Box("Insulator", pole, new Vector3(k * .75f, height - .25f, 0f), new Vector3(.08f, .2f, .08f), art.Porcelain, false);
                    }
                    if (i == 0) continue;
                    Vector2 q = route[i - 1];
                    Vector3 a = new(q.x, IslandLandform.Height(q.x, q.y) + height - .2f, q.y), b = new(p.x, IslandLandform.Height(p.x, p.y) + height - .2f, p.y);
                    Vector3 across = Vector3.Cross(Vector3.up, (b - a).normalized);
                    for (int k = -1; k <= 1; k++)
                    {
                        // Some spans have come down: one wire hangs to the ground.
                        bool down = k == 1 && K.Hash((int)(a.x + b.x), (int)(a.z + b.z)) < .25f;
                        Sag(g, a + across * (k * .75f), down ? new Vector3(b.x, IslandLandform.Height(b.x, b.z) + .1f, b.z) + across * (k * .75f) : b + across * (k * .75f), down ? .3f : 1.4f, art.Bars);
                    }
                }
            }
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) if (r.name == "Wire") r.shadowCastingMode = ShadowCastingMode.Off;
            MarkStatic(g);
        }

        private static void Sag(Transform g, Vector3 a, Vector3 b, float sag, Material m)
        {
            const int segments = 6;
            Vector3 prev = a;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * (sag * 4f * t * (1f - t));
                Vector3 d = p - prev;
                GameObject wire = Box("Wire", g, (p + prev) * .5f, new Vector3(.025f, .025f, d.magnitude), m, false);
                wire.transform.rotation = Quaternion.LookRotation(d.normalized);
                prev = p;
            }
        }
    }
}
