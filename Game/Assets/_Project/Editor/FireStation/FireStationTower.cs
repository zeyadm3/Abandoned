using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.FireStationLayout;
using Hole = Abandoned.EditorTools.StationWalls.Hole;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The hose tower (an 8 x 8 m brick shaft to 18 m on the cells the plan leaves custom) and the fire pole. The tower's
    /// three steel platforms (5, 10, 15 m) alternate between its north and south halves and are joined by fixed ladders
    /// (<see cref="Ladder"/>: players only, hands empty), so nothing heavy goes up; a 0.9 m slot along the east wall,
    /// where the hoses hang, is the way loot comes down: dropped. Platforms are Walkway sections; the top two can fall.
    /// The pole is a ladder through the pierced slab in the upper hall, a brass pole down its middle.
    /// </summary>
    public static class FireStationTower
    {
        public const string GroupName = "HoseTower";
        private const float Wall = P.ShellThickness, Half = Wall * .5f;
        private const float X0 = 0f, X1 = 8f, Z0 = 28f, Z1 = 36f, ZMid = 32f, SlotX = 6.9f;

        public static void Build(StationContext ctx)
        {
            StationArt a = ctx.Art;
            Transform tower = Group(GroupName, ctx.Root);
            Shell(tower, a);
            var platforms = new List<(Transform t, int level)>();
            for (int i = 0; i < L.Platforms.Length; i++) platforms.Add((Platform(tower, a, i), i));
            Ladders(tower, a);
            Hoses(tower, a);
            Bulbs(tower, a);
            foreach ((Transform t, int level) in platforms)
            {
                // The first platform never falls; the rusted top one is the weakest.
                bool falls = level > 0;
                float health = level == 2 ? .6f : .8f, capacity = level == 2 ? .5f : .7f;
                ctx.Setup.AddSection(t, SectionType.Walkway, falls, falls ? health : 1f, falls ? capacity : 1f, fractured: false);
                if (!falls) t.gameObject.AddComponent<SectionKeepClear>();
            }
            // Keep the tower's door and the foot of the first ladder clear of the bay's fittings.
            ctx.Props.KeepOut(0, new Rect(4.2f, 25.2f, 3.6f, 2.8f));
            ctx.Props.KeepOut(0, new Rect(.5f, 28.2f, 6.2f, 3.8f));

            ctx.AddLoot(L.Tower, new Vector3(4.6f, 0f, 30f), CarryClass.Pocket, CarryClass.Heavy, true);
            ctx.AddLoot(L.Tower, new Vector3(1.5f, 0f, 34.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Tower, new Vector3(5.6f, 0f, 34.6f), CarryClass.Pocket, CarryClass.TwoHand);
            // Up top only what can be dropped down the slot.
            foreach ((float y, float z) in new[] { (L.Platforms[0], 34.4f), (L.Platforms[1], 29.8f), (L.Platforms[2], 34.4f) })
            {
                ctx.AddLoot(L.Tower, new Vector3(1.4f, y, z), CarryClass.Pocket, CarryClass.OneHand);
                ctx.AddLoot(L.Tower, new Vector3(5.2f, y, z + (z > ZMid ? .6f : -.4f)), CarryClass.Pocket, CarryClass.OneHand);
            }
            Pole(ctx);
        }

        // Four brick walls to the top, slit windows at each platform, the door into the bay, a roof and a parapet.
        private static void Shell(Transform tower, StationArt a)
        {
            Transform shell = Group("Shell", tower);
            float top = L.TowerTop + .8f;
            var slits = new List<Hole>();
            foreach (float y in L.Platforms) slits.Add(new Hole(0f, .6f, y + .9f, y + 2.3f, true));
            List<Hole> At(float centre)
            {
                var list = new List<Hole>();
                foreach (Hole h in slits) list.Add(new Hole(centre, h.Width, h.Bottom, h.Top, true));
                return list;
            }
            // West (outside), its corner closing the north wall's end.
            StationWalls.Run(shell, "West", false, X0, Z0, Z1 + Half, 0f, top, Wall, a.Brick, At(32f), a);
            // North (outside): below the main building's parapet it stops at the facade, above it it closes the corner.
            StationWalls.Run(shell, "North", true, Z1, X0 + Half, X1, 0f, 9.05f, Wall, a.Brick, new List<Hole>(), a);
            StationWalls.Run(shell, "NorthUpper", true, Z1, X0 + Half, X1 + Half, 9.05f, top, Wall, a.Brick, At(4f), a);
            // East (the workshop and the loft below the roof, outside above it) and south (the bay, with the door).
            StationWalls.Run(shell, "East", false, X1, Z0 + Half, Z1 - Half, 0f, top, Wall, a.Brick, new List<Hole>(), a);
            var south = new List<Hole> { new(6f, 2.8f, 0f, 3f) };
            foreach (Hole h in At(3f)) if (h.Bottom > 8.2f) south.Add(h);
            StationWalls.Run(shell, "South", true, Z0, X0 + Half, X1 - Half, 0f, top, Wall, a.Brick, south, a);
            Box("Roof", shell, new Vector3((X0 + X1) * .5f, L.TowerTop - P.Slab * .5f, (Z0 + Z1) * .5f), new Vector3(X1 - X0 - Wall, P.Slab, Z1 - Z0 - Wall), a.Roof);
            Box("Coping_N", shell, new Vector3(4f, top + .06f, Z1), new Vector3(8.6f, .12f, Wall + .14f), a.Trim, false);
            Box("Coping_S", shell, new Vector3(4f, top + .06f, Z0), new Vector3(8.6f, .12f, Wall + .14f), a.Trim, false);
            Box("Coping_W", shell, new Vector3(X0, top + .06f, 32f), new Vector3(Wall + .14f, .12f, 7.36f), a.Trim, false);
            Box("Coping_E", shell, new Vector3(X1, top + .06f, 32f), new Vector3(Wall + .14f, .12f, 7.36f), a.Trim, false);
            Primitive(PrimitiveType.Cylinder, "Flagpole", shell, new Vector3(4f, top + 2f, 32f), new Vector3(.08f, 2f, .08f), a.Chrome, false);
            MarkStatic(shell);
        }

        private static Transform Platform(Transform tower, StationArt a, int level)
        {
            Transform group = tower.Find("Platforms");
            if (group == null) group = Group("Platforms", tower);
            bool north = level % 2 == 0;
            float y = L.Platforms[level];
            float z0 = north ? ZMid : Z0 + Half, z1 = north ? Z1 - Half : ZMid;
            float x0 = X0 + Half, x1 = SlotX;
            Transform t = Group($"Platform_{level}", group);
            t.position = new Vector3((x0 + x1) * .5f, y, (z0 + z1) * .5f);
            var size = new Vector3(x1 - x0, P.Slab, z1 - z0);
            var solid = t.gameObject.AddComponent<BoxCollider>();
            solid.center = Vector3.down * (P.Slab * .5f);
            solid.size = size;
            Transform visual = Group("Visual", t);
            Box("Grating", visual, solid.center, size, a.Grating, false);
            Box("Beam_A", visual, new Vector3(0f, -P.Slab - .1f, -size.z * .5f + .1f), new Vector3(size.x, .2f, .12f), a.DarkSteel, false);
            Box("Beam_B", visual, new Vector3(0f, -P.Slab - .1f, size.z * .5f - .1f), new Vector3(size.x, .2f, .12f), a.DarkSteel, false);
            t.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);

            // Rails along the open edges (the slot side and the middle), gaps where a ladder arrives.
            float edgeZ = north ? z0 : z1;
            var gaps = new List<float>();
            if (level == 0) gaps.Add(2f);
            if (level == 1) gaps.Add(4f);
            if (level == 2) gaps.Add(6f);
            float from = x0;
            foreach (float g in gaps)
            {
                RailX(visual, t.position, from, g - .75f, edgeZ, y, a);
                from = g + .75f;
            }
            RailX(visual, t.position, from, x1, edgeZ, y, a);
            RailZ(visual, t.position, x1, z0, z1, y, a);
            return t;
        }

        private static void RailX(Transform parent, Vector3 origin, float x0, float x1, float z, float y, StationArt a)
        {
            if (x1 - x0 < .1f) return;
            var c = new Vector3((x0 + x1) * .5f, y + .55f, z) - origin;
            Box("Rail", parent, c + Vector3.up * .5f, new Vector3(x1 - x0, .06f, .06f), a.Safety, false);
            Box("RailMid", parent, c, new Vector3(x1 - x0, .04f, .04f), a.Steel, false);
            GameObject guard = Box("RailGuard", parent, c, new Vector3(x1 - x0, 1.1f, .06f), a.Steel);
            guard.GetComponent<MeshRenderer>().enabled = false;
        }

        private static void RailZ(Transform parent, Vector3 origin, float x, float z0, float z1, float y, StationArt a)
        {
            var c = new Vector3(x, y + .55f, (z0 + z1) * .5f) - origin;
            Box("Rail", parent, c + Vector3.up * .5f, new Vector3(.06f, .06f, z1 - z0), a.Safety, false);
            Box("RailMid", parent, c, new Vector3(.04f, .04f, z1 - z0), a.Steel, false);
            GameObject guard = Box("RailGuard", parent, c, new Vector3(.06f, 1.1f, z1 - z0), a.Steel);
            guard.GetComponent<MeshRenderer>().enabled = false;
        }

        // Ground to the first platform, then up, alternating faces of the middle line (z 32).
        private static void Ladders(Transform tower, StationArt a)
        {
            Transform group = Group("Ladders", tower);
            (float x, float y, bool facesSouth)[] spots = { (2f, 0f, true), (4f, L.Platforms[0], false), (6f, L.Platforms[1], true) };
            for (int i = 0; i < spots.Length; i++)
            {
                (float x, float y, bool south) = spots[i];
                float rise = L.Platforms[i] - y;
                Transform ladder = Group($"Ladder_{i}", group);
                ladder.SetPositionAndRotation(new Vector3(x, y, ZMid), Quaternion.LookRotation(south ? Vector3.back : Vector3.forward));
                LadderVisual(ladder, rise, a);
                ladder.gameObject.AddComponent<Ladder>().EditorSetup(rise);
            }
            MarkStatic(group);
        }

        private static void LadderVisual(Transform ladder, float rise, StationArt a)
        {
            float stile = rise + 1.1f;
            foreach (float x in new[] { -.25f, .25f })
                Box(x < 0 ? "Stile_L" : "Stile_R", ladder, new Vector3(x, stile * .5f, .06f), new Vector3(.06f, stile, .06f), a.Safety, false);
            for (float y = .3f; y < rise; y += .3f)
                Box("Rung", ladder, new Vector3(0f, y, .06f), new Vector3(.5f, .04f, .04f), a.Steel, false);
        }

        // Canvas hoses hanging to dry in the slot by the east wall, from a beam under the roof.
        private static void Hoses(Transform tower, StationArt a)
        {
            Transform group = Group("Hoses", tower);
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            float x = (SlotX + X1 - Half) * .5f;
            Box("Beam", group, new Vector3(x, L.TowerTop - .6f, 32f), new Vector3(.2f, .2f, Z1 - Z0 - Wall), a.DarkSteel, false);
            foreach (float z in new[] { 29.4f, 30.9f, 33.1f, 34.6f })
            {
                float length = 13.5f + Mathf.Abs(z - 32f) * .5f;
                Box("Hose", group, new Vector3(x, L.TowerTop - .7f - length * .5f, z), new Vector3(.1f, length, .22f), canvas, false);
                Box("Coupling", group, new Vector3(x, L.TowerTop - .7f - length, z), new Vector3(.14f, .18f, .14f), a.Brass, false);
            }
        }

        // A bare lamp under each platform and under the roof, on the building's power like every ceiling light.
        private static void Bulbs(Transform tower, StationArt a)
        {
            Transform group = Group("Lamps", tower);
            float[] heights = { L.Platforms[0] - .5f, L.Platforms[1] - .5f, L.Platforms[2] - .5f, L.TowerTop - .6f };
            float[] zs = { 34f, 30f, 34f, 30f };
            for (int i = 0; i < heights.Length; i++)
            {
                Transform holder = Group("Bulb_" + i, group);
                holder.position = new Vector3(3.5f, heights[i], zs[i]);
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
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(), i == 2, i == 3, 4141 + i);
            }
        }

        // The fire pole: a ladder in the pierced slab's opening (you climb on facing south, toward the hall slab), a brass
        // pole down its middle, a guard on three sides up top and a mat where you land.
        private static void Pole(StationContext ctx)
        {
            StationArt a = ctx.Art;
            Transform group = Group("FirePole", ctx.Root);
            Vector3 c = P.CellCentre(L.PoleCell, 1);
            float h = L.PoleHole * .5f;
            Transform ladder = Group("PoleLadder", group);
            ladder.SetPositionAndRotation(new Vector3(c.x, 0f, c.z - h), Quaternion.LookRotation(Vector3.forward));
            ladder.gameObject.AddComponent<Ladder>().EditorSetup(P.Story);
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
