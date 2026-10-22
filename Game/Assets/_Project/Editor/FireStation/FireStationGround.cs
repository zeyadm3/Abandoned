using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's ground floor behind the bay (Docs/FIRE_STATION_PLAN.md): the watch office with its alarm board,
    /// the station office, the day room (the jukebox), the kitchen and its long table, the turnout room's lockers, the
    /// laundry, the hose workshop with the breakers, and the hall the pole comes down into.
    /// </summary>
    public static class FireStationGround
    {
        public static void Dress(StationContext ctx)
        {
            Watch(ctx);
            Office(ctx);
            DayRoom(ctx);
            Kitchen(ctx);
            Turnout(ctx);
            Laundry(ctx);
            Workshop(ctx);
            Props p = ctx.Props;
            F.ExitSign(p, "Hall", new Vector3(26f, 2.75f, 35.74f), 180f);
            F.WaterCooler(p, "Hall", new Vector3(27.4f, 0f, 24.6f), 270f);
        }

        // Watch office: x 24-36, z 0-8; doors to the bay (west, z 6), the hall (north, x 26), the office (east, z 2), the street (south, x 30).
        private static void Watch(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Watch_Office");
            F.WallBoard(p, r.Name, new Vector3(24.12f, 1.7f, 2.4f), 90f, 3f, 1.5f, a.DarkWood);
            Transform lamps = p.Hang(r.Name, "AlarmLamps", new Vector3(24.16f, 1.7f, 2.4f), 90f);
            for (int i = 0; i < 12; i++)
                Props.B(lamps, "Lamp", new Vector3(-1.2f + (i % 6) * .48f, .35f - (i / 6) * .55f, .045f), new Vector3(.12f, .12f, .02f), i % 5 == 0 ? a.WarningRed : a.Brass);
            F.Counter(p, r.Name, new Vector3(31f, 0f, 5.5f), 180f, 3.5f, .95f, a.DarkWood);
            Transform radio = p.Hang(r.Name, "RadioDesk", new Vector3(31f, .95f, 5.5f), 180f);
            Props.B(radio, "Console", new Vector3(-.8f, .18f, 0f), new Vector3(1f, .36f, .4f), a.DarkSteel);
            Props.B(radio, "Mic", new Vector3(.4f, .15f, .1f), new Vector3(.06f, .3f, .06f), a.Chrome);
            Props.B(radio, "Log", new Vector3(.9f, .02f, 0f), new Vector3(.3f, .03f, .4f), a.Paper);
            F.Chair(p, r.Name, new Vector3(31f, 0f, 6.6f), 180f);
            F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.North, 7f, .64f, out float yaw), yaw);
            F.WallClock(p, r.Name, new Vector3(30f, 2.4f, 7.88f), 180f);
            ctx.AddLoot(L.Watch, new Vector3(34f, 0f, 6.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Watch, new Vector3(27.4f, 0f, 2.6f), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Station office: x 36-48, z 0-8; doors west (z 2) and north (x 42).
        private static void Office(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Station_Office");
            F.Desk(p, r.Name, new Vector3(42f, 0f, 4.2f), 0f, true, 1.8f);
            F.Chair(p, r.Name, new Vector3(42f, 0f, 5.15f), 180f);
            F.Chair(p, r.Name, new Vector3(41.2f, 0f, 3.1f), 0f, false);
            for (int i = 0; i < 3; i++) F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.East, 1.5f + i * .55f, .64f, out float yaw), yaw);
            F.Shelving(p, r.Name, Props.Against(r, P.Side.West, 6f, .5f, out float y2), y2, 1.6f, 51, a.DarkWood);
            F.WallBoard(p, r.Name, new Vector3(45f, 1.7f, 7.88f), 180f, 2.4f, 1.2f, a.Board);
            ctx.AddLoot(L.Watch, new Vector3(38.6f, 0f, 2.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Watch, new Vector3(46f, 0f, 6.3f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Day room: x 28-48, z 8-20; wide door west (z 14), south to the office (x 42), north to the kitchen (x 38).
        private static void DayRoom(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Day_Room");
            Material cover = a.Plain("SofaVinyl", new Color(.35f, .2f, .14f), .3f);
            F.Television(p, r.Name, Props.Against(r, P.Side.East, 6f, .45f, out float yaw), yaw);
            F.Sofa(p, r.Name, new Vector3(42.4f, 0f, 14f), 90f, 2.4f, cover);
            F.Sofa(p, r.Name, new Vector3(45f, 0f, 18.3f), 180f, 2f, cover);
            F.Sofa(p, r.Name, new Vector3(45f, 0f, 9.7f), 0f, 1.1f, cover);
            F.Table(p, r.Name, new Vector3(44.6f, 0f, 14f), 90f, 1f, .6f, a.DarkWood);
            Transform pool = p.Place(r.Name, "PoolTable", new Vector3(33.5f, 0f, 11.8f), 0f, new Vector3(2.5f, .85f, 1.4f));
            if (pool != null)
            {
                Props.B(pool, "Bed", new Vector3(0f, .74f, 0f), new Vector3(2.5f, .16f, 1.4f), a.DarkWood);
                Props.B(pool, "Felt", new Vector3(0f, .825f, 0f), new Vector3(2.3f, .01f, 1.2f), a.Plain("Felt", new Color(.08f, .3f, .14f), .05f));
                foreach (float x in new[] { -1.1f, 1.1f })
                foreach (float z in new[] { -.55f, .55f })
                    Props.B(pool, "Leg", new Vector3(x, .33f, z), new Vector3(.14f, .66f, .14f), a.DarkWood);
            }
            F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 15f, .5f, out float y2), y2, 1.8f, 61, a.DarkWood);
            F.Plant(p, r.Name, new Vector3(28.8f, 0f, 8.8f));
            F.WallClock(p, r.Name, new Vector3(28.12f, 2.4f, 10f), 90f);
            ctx.AddLoot(L.Quarters, new Vector3(31.5f, 0f, 18.2f), CarryClass.Pocket, CarryClass.Huge, true);
            foreach (Vector3 at in new[] { new Vector3(36.5f, 0f, 9.6f), new Vector3(39f, 0f, 16.6f), new Vector3(46.6f, 0f, 12f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(36.5f, 0f, 17f));
        }

        // Kitchen: x 28-48, z 20-28; doors west (z 22), south (x 38), north to the laundry (x 42).
        private static void Kitchen(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Kitchen");
            F.Table(p, r.Name, new Vector3(37f, 0f, 24.2f), 0f, 6f, 1.1f, a.Wood);
            foreach (float x in new[] { 35f, 37f, 39f })
            {
                F.Chair(p, r.Name, new Vector3(x, 0f, 23.1f), 0f, false);
                F.Chair(p, r.Name, new Vector3(x, 0f, 25.3f), 180f, false);
            }
            F.Counter(p, r.Name, Props.Against(r, P.Side.East, 4f, .7f, out float yaw), yaw, 6f, .92f, a.White);
            F.Fridge(p, r.Name, Props.Against(r, P.Side.North, 15.5f, .72f, out yaw), yaw);
            F.Fridge(p, r.Name, Props.Against(r, P.Side.North, 16.4f, .72f, out yaw), yaw);
            F.Stove(p, r.Name, Props.Against(r, P.Side.North, 18.4f, .68f, out yaw), yaw);
            Transform hood = p.Hang(r.Name, "RangeHood", new Vector3(46.4f, 2.2f, 27.6f), 180f);
            Props.B(hood, "Hood", Vector3.zero, new Vector3(1f, .5f, .6f), a.Steel);
            ctx.AddLoot(L.Quarters, new Vector3(31f, 0f, 26.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(44f, 0f, 22f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(31f, 0f, 21.6f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(32.5f, 0f, 24f));
        }

        // Turnout room: x 28-36, z 28-36; door west (z 30). Lockers to hide in on the north and east walls.
        private static void Turnout(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Turnout_Room");
            for (int i = 0; i < 6; i++) ctx.Hiding.Locker(r, P.Side.North, 1.6f + i * .8f);
            for (int i = 0; i < 3; i++) ctx.Hiding.Locker(r, P.Side.East, 1.3f + i * .8f);
            F.Bench(p, r.Name, new Vector3(31.5f, 0f, 31f), 0f, 2.2f);
            ctx.AddLoot(L.Tower, new Vector3(30f, 0f, 32.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Tower, new Vector3(33.4f, 0f, 30f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Laundry and decontamination: x 36-48, z 28-36; door south (x 42).
        private static void Laundry(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Laundry");
            for (int i = 0; i < 3; i++)
            {
                Transform washer = p.Place(r.Name, "Washer", Props.Against(r, P.Side.North, 2f + i * 1f, .75f, out float yaw), yaw, new Vector3(.85f, 1f, .75f));
                if (washer == null) continue;
                Props.B(washer, "Body", new Vector3(0f, .5f, 0f), new Vector3(.85f, 1f, .75f), a.White);
                Props.C(washer, "Door", new Vector3(0f, .55f, .38f), new Vector3(.5f, .01f, .5f), a.Glass).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Transform extractor = p.Place(r.Name, "GearExtractor", Props.Against(r, P.Side.East, 6f, 1.2f, out float ey), ey, new Vector3(1.4f, 1.6f, 1.2f));
            if (extractor != null)
            {
                Props.B(extractor, "Body", new Vector3(0f, .8f, 0f), new Vector3(1.4f, 1.6f, 1.2f), a.Steel);
                Props.C(extractor, "Drum", new Vector3(0f, .9f, .61f), new Vector3(.9f, .01f, .9f), a.DarkSteel).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            F.Sink(p, r.Name, Props.Against(r, P.Side.East, 2f, .45f, out float sy), sy, a.Steel);
            ctx.Hiding.Closet(r, P.Side.West, 6f);
            ctx.AddLoot(L.Quarters, new Vector3(42f, 0f, 31.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(44.6f, 0f, 30.2f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(40.5f, 0f, 32.5f));
        }

        // Hose workshop: x 8-24, z 28-36; wide door south to the bay (x 14), door east to the hall (z 30). The breakers.
        private static void Workshop(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Hose_Workshop");
            ctx.Breakers = StationLighting.Breakers(ctx.Root, a, new Vector3(17f, 0f, 35.78f), 180f, 1f);
            F.Table(p, r.Name, Props.Against(r, P.Side.North, 13f, .9f, out float yaw), yaw, 2.4f, .9f, a.Steel);
            Transform rack = p.Place(r.Name, "HoseTestRack", Props.Against(r, P.Side.West, 4f, .6f, out float ry), ry, new Vector3(3f, 1.4f, .6f));
            if (rack != null)
            {
                Props.B(rack, "Frame", new Vector3(0f, .7f, 0f), new Vector3(3f, .06f, .55f), a.Red);
                foreach (float x in new[] { -1.45f, 1.45f }) Props.B(rack, "Leg", new Vector3(x, .35f, 0f), new Vector3(.06f, .7f, .5f), a.Red);
                Props.B(rack, "Hose", new Vector3(0f, .8f, 0f), new Vector3(2.8f, .1f, .14f), a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f));
            }
            Transform compressor = p.Place(r.Name, "AirCompressor", new Vector3(11f, 0f, 30.6f), 90f, new Vector3(1.4f, 1.5f, 2f));
            if (compressor != null)
            {
                Props.B(compressor, "Body", new Vector3(0f, .7f, 0f), new Vector3(1.4f, 1.4f, 2f), a.Accent);
                Props.C(compressor, "Tank", new Vector3(0f, 1.2f, -.4f), new Vector3(.45f, .5f, .45f), a.Steel);
                for (int i = 0; i < 4; i++) Props.C(compressor, "FillValve", new Vector3(-.5f + i * .33f, 1.45f, .7f), new Vector3(.08f, .06f, .08f), a.Brass);
            }
            ctx.Hiding.Cabinet(r, P.Side.East, 6f);
            ctx.AddLoot(L.Apparatus, new Vector3(17f, 0f, 32f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Apparatus, new Vector3(12f, 0f, 34f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(19.5f, 0f, 30.8f));
        }
    }
}
