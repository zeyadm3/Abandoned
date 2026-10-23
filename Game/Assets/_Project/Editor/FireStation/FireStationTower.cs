using System.Collections.Generic;
using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Hole = Abandoned.EditorTools.StationWalls.Hole;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The hose tower (8 x 12 m, brick, to 20 m) on the cells the plan leaves custom, and the fire pole. Inside, four steel
    /// flights switch back up the west and east halves to landings every 4 m and the bell deck at 16 m, open arches all
    /// round it. Between the flights a 1 m shaft runs from the roof beam to the floor: the hoses hang in it to dry, and it
    /// is the quick way down for anything you can let go of. Landings are Walkway sections: the 12 m landing and the two
    /// halves of the bell deck can fall; the 4, 8 and 16 m landings (where the flights meet) never do. Monsters use the
    /// flights like any stair. The pole is a fixed ladder through the pierced slab in bunk room B.
    /// </summary>
    public static class FireStationTower
    {
        public const string GroupName = "HoseTower";
        private const float Wall = P.ShellThickness, Half = Wall * .5f;
        private const float X0 = 0f, X1 = 8f, Z0 = 28f, Z1 = 40f, ZS = 30f, ZN = 38f, GapW = 3.5f, GapE = 4.5f;

        private readonly struct Landing
        {
            public readonly string Name;
            public readonly Rect Area;
            public readonly float Y, Health, Capacity;
            public readonly bool Falls;
            public Landing(string name, Rect area, float y, bool falls, float health = 1f, float capacity = 1f)
            { Name = name; Area = area; Y = y; Falls = falls; Health = health; Capacity = capacity; }
        }

        private static readonly Landing[] Landings =
        {
            new("Landing_N4", Rect.MinMaxRect(X0 + Half, ZN, X1 - Half, Z1 - Half), 4f, false),
            new("Landing_S8", Rect.MinMaxRect(X0 + Half, Z0 + Half, X1 - Half, ZS), 8f, false),
            new("Landing_N12", Rect.MinMaxRect(X0 + Half, ZN, X1 - Half, Z1 - Half), 12f, true, .7f, .6f),
            new("Landing_S16", Rect.MinMaxRect(X0 + Half, Z0 + Half, X1 - Half, ZS), 16f, false),
            new("BellDeck_W", Rect.MinMaxRect(X0 + Half, ZS, GapW, Z1 - Half), 16f, true, .6f, .5f),
            new("BellDeck_N", Rect.MinMaxRect(GapW, ZN, X1 - Half, Z1 - Half), 16f, true, .6f, .5f),
        };

        public static void Build(StationContext ctx)
        {
            StationArt a = ctx.Art;
            Transform tower = Group(GroupName, ctx.Root);
            Shell(tower, a);
            Flights(ctx, tower);
            Transform group = Group("Landings", tower);
            foreach (Landing l in Landings)
            {
                Transform t = Slab(group, l, a);
                ctx.Setup.AddSection(t, SectionType.Walkway, l.Falls, l.Health, l.Capacity, fractured: false);
                if (l.Falls) SectionNavCarver.EditorAdd(t.gameObject, new Vector3(l.Area.width, P.Slab, l.Area.height));
                else t.gameObject.AddComponent<SectionKeepClear>();
            }
            Rails(group, a);
            Hoses(tower, a);
            Lamps(tower, a);
            // Keep the tower's doors clear of the bay's and the workshop's fittings.
            ctx.Props.KeepOut(0, new Rect(4.2f, 25.2f, 3.6f, 2.8f));
            ctx.Props.KeepOut(0, new Rect(8f, 32.6f, 2.6f, 2.8f));

            ctx.AddLoot(L.Tower, new Vector3(6.2f, 0f, 33f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Tower, new Vector3(6.2f, 0f, 36.4f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Tower, new Vector3(1.3f, 4f, 39f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Tower, new Vector3(1.3f, 8f, 29.1f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Tower, new Vector3(1.3f, 12f, 39f), CarryClass.Pocket, CarryClass.TwoHand);
            // The bell deck: the bronze bell (a heavy carry down four flights) and the odd bit of brass.
            ctx.AddLoot(L.Tower, new Vector3(1.8f, L.TowerDeck, 34f), CarryClass.Pocket, CarryClass.Heavy);
            ctx.AddLoot(L.Tower, new Vector3(6.4f, L.TowerDeck, 39f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Tower, new Vector3(1.6f, L.TowerDeck, 29.1f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(6.2f, 0f, 30.6f));
            ctx.Threat(new Vector3(1.6f, L.TowerDeck, 37.5f));
            Pole(ctx);
        }

        private static void Shell(Transform tower, StationArt a)
        {
            Transform shell = Group("Shell", tower);
            float top = L.TowerTop + .8f;
            var slits = new List<float> { 2f, 6f, 10f, 14f };
            List<Hole> Openings(float a0, float a1, bool slitsToo, float above = -1f)
            {
                var list = new List<Hole>();
                if (slitsToo) foreach (float y in slits) if (y > above) list.Add(new Hole((a0 + a1) * .5f, .6f, y, y + 1.4f, true));
                // The bell deck's arches, open to the air.
                list.Add(new Hole(a0 + (a1 - a0) * .25f, 2f, L.TowerDeck + 1.1f, L.TowerDeck + 3.3f));
                list.Add(new Hole(a0 + (a1 - a0) * .75f, 2f, L.TowerDeck + 1.1f, L.TowerDeck + 3.3f));
                return list;
            }
            StationWalls.Run(shell, "West", false, X0, Z0, Z1 + Half, 0f, top, Wall, a.Brick, Openings(Z0, Z1, true), a);
            StationWalls.Run(shell, "North", true, Z1, X0 + Half, X1, 0f, 9.05f, Wall, a.Brick, new List<Hole>(), a);
            StationWalls.Run(shell, "NorthUpper", true, Z1, X0 + Half, X1 + Half, 9.05f, top, Wall, a.Brick, Openings(X0, X1, true, 9f), a);
            var east = Openings(Z0, Z1, true, 8.5f);
            east.Add(new Hole(34f, P.DoorWidth, 0f, P.DoorHeight));
            StationWalls.Run(shell, "East", false, X1, Z0 + Half, Z1 - Half, 0f, top, Wall, a.Brick, east, a);
            var south = Openings(X0, X1, true, 8.5f);
            south.Add(new Hole(6f, 2.4f, 0f, 3f));
            StationWalls.Run(shell, "South", true, Z0, X0 + Half, X1 - Half, 0f, top, Wall, a.Brick, south, a);
            Box("Roof", shell, new Vector3((X0 + X1) * .5f, L.TowerTop - P.Slab * .5f, (Z0 + Z1) * .5f), new Vector3(X1 - X0 - Wall, P.Slab, Z1 - Z0 - Wall), a.Roof);
            Box("Coping_N", shell, new Vector3(4f, top + .06f, Z1), new Vector3(8.6f, .12f, Wall + .14f), a.Trim, false);
            Box("Coping_S", shell, new Vector3(4f, top + .06f, Z0), new Vector3(8.6f, .12f, Wall + .14f), a.Trim, false);
            Box("Coping_W", shell, new Vector3(X0, top + .06f, 34f), new Vector3(Wall + .14f, .12f, 11.36f), a.Trim, false);
            Box("Coping_E", shell, new Vector3(X1, top + .06f, 34f), new Vector3(Wall + .14f, .12f, 11.36f), a.Trim, false);
            Box("Band", shell, new Vector3(-Half - .04f, L.TowerDeck + .5f, 34f), new Vector3(.1f, .3f, 12.4f), a.Trim, false);
            Primitive(PrimitiveType.Cylinder, "Flagpole", shell, new Vector3(4f, top + 2f, 34f), new Vector3(.08f, 2f, .08f), a.Chrome, false);
            MarkStatic(shell);
        }

        // Up the west half to the north landings, back down... up the east half to the south ones: four flights, 16 m.
        private static void Flights(StationContext ctx, Transform tower)
        {
            Transform group = Group("Flights", tower);
            var flights = new[]
            {
                new P.Flight("Flight_A1", 0, new Vector3(2f, 0f, ZS), Vector3.forward),
                new P.Flight("Flight_B1", 0, new Vector3(6f, 4f, ZN), Vector3.back),
                new P.Flight("Flight_A2", 0, new Vector3(2f, 8f, ZS), Vector3.forward),
                new P.Flight("Flight_B2", 0, new Vector3(6f, 12f, ZN), Vector3.back),
            };
            foreach (P.Flight f in flights)
                ctx.Setup.AddSection(StationFloors.Stair(group, f, ctx.Art), SectionType.Stair, false, fractured: false);
            // Only the first flight stands on the floor: fill under it so nobody walks under its low end.
            StationFloors.UnderFill(Group("UnderStairs", tower), flights[0], ctx.Art);
        }

        private static Transform Slab(Transform group, Landing l, StationArt a)
        {
            Transform t = Group(l.Name, group);
            t.position = new Vector3(l.Area.center.x, l.Y, l.Area.center.y);
            var size = new Vector3(l.Area.width, P.Slab, l.Area.height);
            var solid = t.gameObject.AddComponent<BoxCollider>();
            solid.center = Vector3.down * (P.Slab * .5f);
            solid.size = size;
            Transform visual = Group("Visual", t);
            Box("Grating", visual, solid.center, size, a.Grating, false);
            Box("Beam", visual, new Vector3(0f, -P.Slab - .1f, 0f), new Vector3(size.x, .2f, .12f), a.DarkSteel, false);
            t.gameObject.AddComponent<Abandoned.Core.SurfaceTag>().EditorSet(Abandoned.Core.SurfaceMaterial.Metal);
            return t;
        }

        // Guards where a landing or the bell deck meets the shaft or the stairwell.
        private static void Rails(Transform group, StationArt a)
        {
            Transform rails = Group("Rails", group);
            foreach (float y in new[] { 4f, 8f, 12f, 16f })
            {
                float z = Mathf.Approximately(y % 8f, 4f) ? ZN : ZS;
                Rail(rails, new Vector3((GapW + GapE) * .5f, y, z), new Vector3(GapE - GapW, 1.1f, .06f), a);
            }
            Rail(rails, new Vector3(GapW, L.TowerDeck, (ZS + ZN) * .5f), new Vector3(.06f, 1.1f, ZN - ZS), a);
            Rail(rails, new Vector3((GapW + X1 - Half) * .5f, L.TowerDeck, ZN), new Vector3(X1 - Half - GapW, 1.1f, .06f), a);
        }

        private static void Rail(Transform parent, Vector3 foot, Vector3 size, StationArt a)
        {
            GameObject guard = Box("Guard", parent, foot + Vector3.up * .55f, size, a.Steel);
            guard.GetComponent<MeshRenderer>().enabled = false;
            Box("TopRail", parent, foot + Vector3.up * 1.07f, new Vector3(Mathf.Max(size.x, .06f), .06f, Mathf.Max(size.z, .06f)), a.Safety, false);
            Box("MidRail", parent, foot + Vector3.up * .55f, new Vector3(Mathf.Max(size.x, .04f), .04f, Mathf.Max(size.z, .04f)), a.Steel, false);
        }

        // Canvas hoses hanging in the shaft from a beam under the roof.
        private static void Hoses(Transform tower, StationArt a)
        {
            Transform group = Group("Hoses", tower);
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            float x = (GapW + GapE) * .5f;
            Box("Beam", group, new Vector3(x, L.TowerTop - .55f, 34f), new Vector3(.2f, .2f, Z1 - Z0 - Wall), a.DarkSteel, false);
            foreach (float z in new[] { 31f, 33f, 35f, 37f })
            {
                float length = 15.5f + Mathf.Abs(z - 34f) * .4f;
                Box("Hose", group, new Vector3(x, L.TowerTop - .65f - length * .5f, z), new Vector3(.1f, length, .22f), canvas, false);
                Box("Coupling", group, new Vector3(x, L.TowerTop - .65f - length, z), new Vector3(.14f, .18f, .14f), a.Brass, false);
            }
        }

        // A caged lamp under each landing and the roof, on the building's power like every ceiling light.
        private static void Lamps(Transform tower, StationArt a)
        {
            Transform group = Group("Lamps", tower);
            Vector3[] spots = { new(2f, 3.55f, 39f), new(6f, 7.55f, 29.1f), new(2f, 11.55f, 39f), new(6f, 15.55f, 29.1f), new(2f, L.TowerTop - .6f, 34f) };
            for (int i = 0; i < spots.Length; i++)
            {
                Transform holder = Group("Bulb_" + i, group);
                holder.position = spots[i];
                Box("Cage", holder, new Vector3(0f, .1f, 0f), new Vector3(.2f, .2f, .2f), a.DarkSteel, false);
                GameObject glow = Primitive(PrimitiveType.Sphere, "Bulb", holder, Vector3.zero, Vector3.one * .14f, a.Lamp, false);
                Transform aim = Group("Light", holder);
                aim.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = aim.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 7f;
                lamp.spotAngle = 140f;
                lamp.intensity = 3f;
                lamp.color = new Color(1f, .82f, .6f);
                lamp.shadows = LightShadows.None;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(), i == 2, i == 4, 4141 + i);
            }
        }

        // The pole: a ladder in the pierced slab's opening (you climb on facing south, toward the bunk room's floor), a
        // brass pole down its middle, a guard on three sides above and a mat where you land in the bay.
        private static void Pole(StationContext ctx)
        {
            StationArt a = ctx.Art;
            Transform group = Group("FirePole", ctx.Root);
            Vector3 c = P.CellCentre(L.PoleCell, 1);
            float h = L.PoleHole * .5f;
            // The climb volume only: the brass pole is all you see.
            Transform ladder = Group("PoleLadder", group);
            ladder.SetPositionAndRotation(new Vector3(c.x, 0f, c.z - h), Quaternion.LookRotation(Vector3.forward));
            ladder.gameObject.AddComponent<Abandoned.Core.Ladder>().EditorSetup(P.Story);
            Primitive(PrimitiveType.Cylinder, "Pole", group, new Vector3(c.x, 3.3f, c.z), new Vector3(.09f, 3.3f, .09f), a.Brass, false);
            Primitive(PrimitiveType.Cylinder, "Collar", group, new Vector3(c.x, 6.55f, c.z), new Vector3(.3f, .05f, .3f), a.Brass, false);
            Primitive(PrimitiveType.Cylinder, "Mat", group, new Vector3(c.x, .03f, c.z), new Vector3(1.4f, .03f, 1.4f), a.Rubber, false);
            float y = P.FloorY(1) + .55f;
            foreach ((Vector3 at, Vector3 size) in new[]
            {
                (new Vector3(c.x - h - .03f, y, c.z), new Vector3(.06f, 1.1f, L.PoleHole + .06f)),
                (new Vector3(c.x + h + .03f, y, c.z), new Vector3(.06f, 1.1f, L.PoleHole + .06f)),
                (new Vector3(c.x, y, c.z + h + .03f), new Vector3(L.PoleHole + .12f, 1.1f, .06f)),
            })
                Box("Guard", group, at, size, a.Brass);
        }
    }
}
