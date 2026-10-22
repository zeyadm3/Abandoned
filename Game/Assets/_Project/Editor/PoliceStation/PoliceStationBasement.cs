using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.PoliceStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The precinct's basement (y -4), reached only by the basement stair: the boiler room, property storage (a jackpot),
    /// the generator room with the breaker bank, the four-lane firing range under the locker room's rotten shower floor,
    /// and the armoury (the rifle locker). Everything valuable down here goes up one flight and along the main hall.
    /// </summary>
    public static class PoliceStationBasement
    {
        private const float Y = -P.Story;

        public static void Dress(StationContext ctx)
        {
            Boiler(ctx);
            Property(ctx);
            Generator(ctx);
            Range(ctx);
            Armoury(ctx);
            Corridor(ctx);
        }

        // Boiler room: x 0-4, z 16-24, door north at x 2.
        private static void Boiler(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Boiler_Room");
            Transform boiler = p.Place(r.Name, "Boiler", new Vector3(2f, Y, 18.4f), 0f, new Vector3(1.8f, 2.4f, 1.8f));
            if (boiler == null) return;
            Props.C(boiler, "Drum", new Vector3(0f, 1.1f, 0f), new Vector3(1.6f, 1f, 1.6f), a.DarkSteel);
            Props.B(boiler, "Base", new Vector3(0f, .08f, 0f), new Vector3(1.8f, .16f, 1.8f), a.Concrete);
            Props.C(boiler, "Flue", new Vector3(0f, 3f, 0f), new Vector3(.4f, .9f, .4f), a.Steel);
            Props.B(boiler, "Gauge", new Vector3(0f, 1.5f, .82f), new Vector3(.18f, .18f, .04f), a.White);
        }

        // Property storage: x 8-20, z 16-24, wide door north at x 14.
        private static void Property(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Property_Storage");
            foreach (float along in new[] { 1.4f, 3.7f })
                F.Shelving(p, r.Name, Props.Against(r, P.Side.South, along, .5f, out float yaw), yaw, 2.2f, (int)(along * 10f), ctx.Art.Paper);
            foreach (float along in new[] { 2.2f, 4.8f })
                F.Shelving(p, r.Name, Props.Against(r, P.Side.East, along, .5f, out float yaw), yaw, 2.2f, (int)(along * 7f));
            ctx.Hiding.Cabinet(r, P.Side.West, 4f);
            ctx.AddLoot(L.Evidence, new Vector3(14.5f, Y, 19.4f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Evidence, new Vector3(11.4f, Y, 21.8f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Evidence, new Vector3(17.4f, Y, 22.6f), CarryClass.Pocket, CarryClass.Heavy);
        }

        // Generator room: x 20-32, z 16-24, door north at x 26. The breakers line the south wall.
        private static void Generator(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Generator_Room");
            ctx.Breakers = StationLighting.Breakers(ctx.Root, a, new Vector3(21.6f, Y, 16.12f), 0f, 1f);
            Transform gen = p.Place(r.Name, "Generator", new Vector3(29.2f, Y, 19.8f), 0f, new Vector3(2.6f, 1.8f, 1.4f));
            if (gen != null)
            {
                Props.B(gen, "Body", new Vector3(0f, .85f, 0f), new Vector3(2.4f, 1.5f, 1.3f), a.Accent);
                Props.B(gen, "Skid", new Vector3(0f, .08f, 0f), new Vector3(2.6f, .16f, 1.4f), a.DarkSteel);
                Props.B(gen, "Panel", new Vector3(-.9f, 1f, .66f), new Vector3(.5f, .4f, .02f), a.Rubber);
                Props.C(gen, "Exhaust", new Vector3(.8f, 2.6f, 0f), new Vector3(.2f, .9f, .2f), a.Steel);
            }
            F.Drum(p, r.Name, new Vector3(31f, Y, 23f), a.Red);
            F.Drum(p, r.Name, new Vector3(30.2f, Y, 23.2f), a.Red);
        }

        // Firing range: x 0-20, z 28-40; door south at x 2, east to the armoury at z 34. Shooters at the west end.
        private static void Range(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Firing_Range");
            foreach (float z in new[] { 32f, 36f })
            {
                Transform divider = p.Place(r.Name, "LaneDivider", new Vector3(3.4f, Y, z), 0f, new Vector3(2.4f, 1.9f, .08f));
                if (divider != null) Props.B(divider, "Panel", new Vector3(0f, .95f, 0f), new Vector3(2.4f, 1.9f, .08f), a.Board);
            }
            foreach (float z in new[] { 30f, 34f, 38f })
            {
                F.Table(p, r.Name, new Vector3(4.3f, Y, z), 90f, 1.2f, .5f, a.Steel);
                Transform target = p.Hang(r.Name, "Target", new Vector3(16.5f, Y, z), 270f);
                Props.B(target, "Card", new Vector3(0f, 1.5f, 0f), new Vector3(.6f, .9f, .01f), a.Paper);
                Props.B(target, "Ring", new Vector3(0f, 1.5f, .006f), new Vector3(.25f, .25f, .005f), a.Rubber);
                Props.B(target, "Carrier", new Vector3(0f, 2.6f, 0f), new Vector3(.7f, .06f, .06f), a.DarkSteel);
                Props.B(target, "Rod", new Vector3(0f, 2.3f, 0f), new Vector3(.02f, .6f, .02f), a.DarkSteel);
            }
            foreach ((float z, float w) in new[] { (30.4f, 4.2f), (37.6f, 4.2f) })
            {
                Transform stop = p.Place(r.Name, "Backstop", new Vector3(19.45f, Y, z), 270f, new Vector3(w, 2.8f, .5f));
                if (stop == null) continue;
                Props.B(stop, "Plate", new Vector3(0f, 1.4f, 0f), new Vector3(w, 2.8f, .5f), a.DarkSteel);
            }
            Transform rail = p.Hang(r.Name, "TrackRail", new Vector3(10f, Y + 3.5f, 34f), 0f);
            foreach (float z in new[] { -4f, 0f, 4f }) Props.B(rail, "Track", new Vector3(0f, 0f, z), new Vector3(12f, .06f, .08f), a.Steel);
            foreach (Vector3 at in new[] { new Vector3(8f, Y, 30f), new Vector3(8f, Y, 38f), new Vector3(12f, Y, 34f) })
                ctx.AddLoot(L.Armoury, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(12f, Y, 31f));
        }

        // Armoury: x 20-32, z 28-40; wide door south at x 26, door west at z 34.
        private static void Armoury(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Armoury");
            foreach (float along in new[] { 2.5f, 6f, 9.5f })
                Rack(p, a, r, P.Side.North, along);
            foreach (float along in new[] { 3.5f, 7.5f })
                Rack(p, a, r, P.Side.East, along);
            F.Table(p, r.Name, new Vector3(24f, Y, 33.5f), 0f, 1.8f, .9f, a.Steel);
            F.Shelving(p, r.Name, Props.Against(r, P.Side.West, 9f, .5f, out float yaw), yaw, 1.6f, 44, a.AccentDark);
            ctx.AddLoot(L.Armoury, new Vector3(27.6f, Y, 34.4f), CarryClass.Pocket, CarryClass.Heavy, true);
            ctx.AddLoot(L.Armoury, new Vector3(29.8f, Y, 30.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Armoury, new Vector3(23.5f, Y, 37.4f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // An empty gun rack: a back board, a base with slots and a top bar.
        private static void Rack(Props p, StationArt a, P.Room r, P.Side wall, float along)
        {
            Transform t = p.Place(r.Name, "GunRack", Props.Against(r, wall, along, .4f, out float yaw), yaw, new Vector3(2.4f, 1.9f, .4f));
            if (t == null) return;
            Props.B(t, "Back", new Vector3(0f, 1f, -.17f), new Vector3(2.4f, 1.8f, .04f), a.DarkWood);
            Props.B(t, "Base", new Vector3(0f, .3f, 0f), new Vector3(2.4f, .12f, .38f), a.DarkWood);
            Props.B(t, "Bar", new Vector3(0f, 1.6f, .05f), new Vector3(2.4f, .06f, .2f), a.DarkSteel);
            for (int i = 0; i < 8; i++) Props.B(t, "Slot", new Vector3(-1.05f + i * .3f, .37f, .05f), new Vector3(.02f, .04f, .2f), a.Rubber);
        }

        // The basement corridor (row 6, z 24-28): pipes under the ceiling, a monster's way in.
        private static void Corridor(StationContext ctx)
        {
            Transform pipes = ctx.Props.Hang("Basement_Corridor", "Pipes", new Vector3(16f, Y + 3.45f, 27.4f), 0f);
            Props.C(pipes, "Pipe_A", Vector3.zero, new Vector3(.18f, 16f, .18f), ctx.Art.Steel).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Props.C(pipes, "Pipe_B", new Vector3(0f, -.25f, -.3f), new Vector3(.12f, 16f, .12f), ctx.Art.Bars).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            ctx.Threat(new Vector3(16f, Y, 26f));
        }
    }
}
