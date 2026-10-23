using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's ground floor behind and beside the bay (Docs/FIRE_STATION_PLAN.md): the watch office and its alarm
    /// board, the captain's office, records, the turnout room's lockers, the day room (the jukebox), the kitchen's long
    /// table, the laundry's carts, the restroom, the workshop and the SCBA refill room (the thermal imaging camera), and
    /// the hall the crew runs down to the bay.
    /// </summary>
    public static class FireStationGround
    {
        public static void Dress(StationContext ctx)
        {
            Watch(ctx);
            Captain(ctx);
            Records(ctx);
            Turnout(ctx);
            DayRoom(ctx);
            Kitchen(ctx);
            Laundry(ctx);
            Restroom(ctx);
            Workshop(ctx);
            Scba(ctx);
            Props p = ctx.Props;
            F.ExitSign(p, "Hall", new Vector3(L.RearDoorX, 2.75f, 39.74f), 180f);
            F.WaterCooler(p, "Hall", new Vector3(35.4f, 0f, 16.4f), 270f);
            F.Extinguisher(p, "Hall", new Vector3(32.18f, 1.2f, 30f), 90f);
            F.WallBoard(p, "Hall", new Vector3(35.88f, 1.6f, 11f), 270f, 1.4f, 1f, ctx.Art.Board);
        }

        // Watch office: x 32-44, z 0-8; doors to the bay (west, z 6), the hall (north, x 34), the captain (east, z 2), the street (x 38).
        private static void Watch(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Watch_Office");
            F.WallBoard(p, r.Name, new Vector3(32.12f, 1.7f, 2.4f), 90f, 3f, 1.5f, a.DarkWood);
            Transform lamps = p.Hang(r.Name, "AlarmLamps", new Vector3(32.16f, 1.7f, 2.4f), 90f);
            for (int i = 0; i < 12; i++)
                Props.B(lamps, "Lamp", new Vector3(-1.2f + (i % 6) * .48f, .35f - (i / 6) * .55f, .045f), new Vector3(.12f, .12f, .02f), i % 5 == 0 ? a.WarningRed : a.Brass);
            F.Counter(p, r.Name, new Vector3(40.4f, 0f, 5.5f), 180f, 3.2f, .95f, a.DarkWood);
            Transform radio = p.Hang(r.Name, "RadioDesk", new Vector3(40.4f, .95f, 5.5f), 180f);
            Props.B(radio, "Console", new Vector3(-.8f, .18f, 0f), new Vector3(1f, .36f, .4f), a.DarkSteel);
            Props.B(radio, "Mic", new Vector3(.4f, .15f, .1f), new Vector3(.06f, .3f, .06f), a.Chrome);
            Props.B(radio, "Log", new Vector3(.9f, .02f, 0f), new Vector3(.3f, .03f, .4f), a.Paper);
            F.Chair(p, r.Name, new Vector3(40.4f, 0f, 6.6f), 180f);
            F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.North, 9.6f, .64f, out float yaw), yaw);
            F.WallClock(p, r.Name, new Vector3(38f, 2.4f, 7.88f), 180f);
            F.Extinguisher(p, r.Name, new Vector3(43.82f, 1.2f, 6.5f), 270f);
            ctx.AddLoot(L.Watch, new Vector3(42.6f, 0f, 6.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Watch, new Vector3(35.4f, 0f, 2.6f), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Captain's office: x 44-52, z 0-8; doors west (z 2) and north to the day room (x 50).
        private static void Captain(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Captain_Office");
            F.Desk(p, r.Name, new Vector3(48f, 0f, 4.2f), 0f, true, 1.6f);
            F.Chair(p, r.Name, new Vector3(48f, 0f, 5.15f), 180f);
            F.Chair(p, r.Name, new Vector3(47.2f, 0f, 3.1f), 0f, false);
            F.Bookcase(p, r.Name, Props.Against(r, P.Side.East, 4f, .38f, out float yaw), yaw, 1.4f, 7);
            F.CoatRack(p, r.Name, new Vector3(44.9f, 0f, 6.9f), a.Plain("TurnoutCoat", new Color(.55f, .48f, .3f), .1f));
            F.WallBoard(p, r.Name, new Vector3(48f, 1.7f, .2f), 0f, 1.2f, .8f, a.White);
            ctx.AddLoot(L.Watch, new Vector3(50.6f, 0f, 2.2f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Watch, new Vector3(46f, 0f, 1.6f), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Records: x 52-64, z 0-8; door north to the day room (x 58).
        private static void Records(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Records_Room");
            F.Shelving(p, r.Name, new Vector3(55f, 0f, 3.4f), 90f, 2.4f, 81);
            F.Shelving(p, r.Name, new Vector3(61f, 0f, 3.4f), 90f, 2.4f, 82, ctx.Art.Paper);
            for (int i = 0; i < 3; i++) F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.North, 1.5f + i * .55f, .64f, out float yaw), yaw);
            F.Copier(p, r.Name, Props.Against(r, P.Side.East, 6.3f, .7f, out float cy), cy);
            F.FloorLitter(p, r.Name, new Vector3(58f, 0f, 3.5f), 5);
            ctx.AddLoot(L.Watch, new Vector3(58f, 0f, 2f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Watch, new Vector3(53.4f, 0f, 5.8f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Turnout room: x 36-44, z 8-20; door west to the hall (z 14). Lockers to hide in, open racks of gear.
        private static void Turnout(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Turnout_Room");
            for (int i = 0; i < 6; i++) ctx.Hiding.Locker(r, P.Side.East, 2f + i * .8f);
            ctx.Hiding.Cage(r, P.Side.East, 9.8f);
            F.Bench(p, r.Name, new Vector3(39.6f, 0f, 14f), 90f, 2.6f);
            F.Lockers(p, r.Name, Props.Against(r, P.Side.North, 4f, .52f, out float yaw), yaw, 7);
            F.Lockers(p, r.Name, Props.Against(r, P.Side.South, 4f, .52f, out yaw), yaw, 7);
            ctx.AddLoot(L.Apparatus, new Vector3(38f, 0f, 10.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Apparatus, new Vector3(38f, 0f, 17.4f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Day room: x 44-64, z 8-20; doors south to the captain (x 50) and records (x 58), north to the hall (x 46, wide)
        // and the kitchen (x 54).
        private static void DayRoom(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Day_Room");
            Material cover = a.Plain("SofaVinyl", new Color(.35f, .2f, .14f), .3f);
            F.Television(p, r.Name, Props.Against(r, P.Side.East, 6f, .45f, out float yaw), yaw);
            F.Sofa(p, r.Name, new Vector3(59.4f, 0f, 14f), 90f, 2.4f, cover);
            F.Sofa(p, r.Name, new Vector3(61.5f, 0f, 18.3f), 180f, 2f, cover);
            F.Sofa(p, r.Name, new Vector3(61.5f, 0f, 9.7f), 0f, 1.1f, cover);
            F.Table(p, r.Name, new Vector3(61.6f, 0f, 14f), 90f, 1f, .6f, a.DarkWood);
            Transform pool = p.Place(r.Name, "PoolTable", new Vector3(50f, 0f, 13.8f), 0f, new Vector3(2.5f, .85f, 1.4f));
            if (pool != null)
            {
                Props.B(pool, "Bed", new Vector3(0f, .74f, 0f), new Vector3(2.5f, .16f, 1.4f), a.DarkWood);
                Props.B(pool, "Felt", new Vector3(0f, .825f, 0f), new Vector3(2.3f, .01f, 1.2f), a.Plain("Felt", new Color(.08f, .3f, .14f), .05f));
                foreach (float x in new[] { -1.1f, 1.1f })
                foreach (float z in new[] { -.55f, .55f })
                    Props.B(pool, "Leg", new Vector3(x, .33f, z), new Vector3(.14f, .66f, .14f), a.DarkWood);
            }
            F.Bookcase(p, r.Name, Props.Against(r, P.Side.West, 6f, .38f, out float by), by, 1.6f, 23);
            F.FallenChair(p, r.Name, new Vector3(54.5f, 0f, 11f), 40f);
            F.TrashCan(p, r.Name, new Vector3(56.6f, 0f, 18.8f), true);
            ctx.AddLoot(L.Quarters, new Vector3(46.2f, 0f, 10.4f), CarryClass.Pocket, CarryClass.Huge);
            foreach (Vector3 at in new[] { new Vector3(53f, 0f, 9.6f), new Vector3(55.4f, 0f, 16.6f), new Vector3(52.4f, 0f, 17.4f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(56.4f, 0f, 13.4f));
        }

        // Kitchen: x 48-64, z 20-32; doors west to the hall (z 26), south to the day room (x 54), north to the restroom (x 54).
        private static void Kitchen(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Kitchen");
            F.Table(p, r.Name, new Vector3(56f, 0f, 25.5f), 0f, 6f, 1.1f, a.Wood);
            foreach (float x in new[] { 54f, 56f, 58f })
            {
                F.Chair(p, r.Name, new Vector3(x, 0f, 24.4f), 0f, false);
                F.Chair(p, r.Name, new Vector3(x, 0f, 26.6f), 180f, false);
            }
            F.Counter(p, r.Name, Props.Against(r, P.Side.East, 6f, .7f, out float yaw), yaw, 7f, .92f, a.White);
            F.Fridge(p, r.Name, Props.Against(r, P.Side.North, 9.5f, .72f, out yaw), yaw);
            F.Fridge(p, r.Name, Props.Against(r, P.Side.North, 10.4f, .72f, out yaw), yaw);
            F.Stove(p, r.Name, Props.Against(r, P.Side.North, 12.5f, .68f, out yaw), yaw);
            Transform hood = p.Hang(r.Name, "RangeHood", new Vector3(60.5f, 2.2f, 31.6f), 180f);
            Props.B(hood, "Hood", Vector3.zero, new Vector3(1f, .5f, .6f), a.Steel);
            F.TrashCan(p, r.Name, new Vector3(49f, 0f, 30.8f));
            ctx.AddLoot(L.Quarters, new Vector3(50.5f, 0f, 22f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(50.6f, 0f, 29.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(61f, 0f, 22.4f), CarryClass.Pocket, CarryClass.Heavy);
            ctx.Threat(new Vector3(51.2f, 0f, 27.6f));
        }

        // Laundry and decontamination: x 36-52, z 32-40; doors west to the hall (z 34) and south to the hall (x 46).
        private static void Laundry(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Laundry");
            for (int i = 0; i < 3; i++)
            {
                Transform washer = p.Place(r.Name, "Washer", Props.Against(r, P.Side.North, 3f + i * 1f, .75f, out float yaw), yaw, new Vector3(.85f, 1f, .75f));
                if (washer == null) continue;
                Props.B(washer, "Body", new Vector3(0f, .5f, 0f), new Vector3(.85f, 1f, .75f), a.White);
                Props.C(washer, "Door", new Vector3(0f, .55f, .38f), new Vector3(.5f, .01f, .5f), a.Glass).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Transform extractor = p.Place(r.Name, "GearExtractor", Props.Against(r, P.Side.North, 8.6f, 1.2f, out float ey), ey, new Vector3(1.4f, 1.6f, 1.2f));
            if (extractor != null)
            {
                Props.B(extractor, "Body", new Vector3(0f, .8f, 0f), new Vector3(1.4f, 1.6f, 1.2f), a.Steel);
                Props.C(extractor, "Drum", new Vector3(0f, .9f, .61f), new Vector3(.9f, .01f, .9f), a.DarkSteel).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            ctx.Hiding.Cart(r, P.Side.South, 3f);
            ctx.Hiding.Cart(r, P.Side.South, 13f);
            ctx.Hiding.Closet(r, P.Side.East, 4f);
            F.Sink(p, r.Name, Props.Against(r, P.Side.North, 11.4f, .45f, out float sy), sy, a.Steel);
            ctx.AddLoot(L.Quarters, new Vector3(43f, 0f, 36f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(47.4f, 0f, 37.4f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(42.4f, 0f, 35.4f));
        }

        // Restroom: x 52-64, z 32-40; door south to the kitchen (x 54).
        private static void Restroom(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Restroom");
            foreach (float along in new[] { 6f, 8f, 10f })
            {
                F.Toilet(p, r.Name, Props.Against(r, P.Side.North, along, .7f, out float yaw), yaw);
                Transform stall = p.Hang(r.Name, "StallWall", new Vector3(52f + along + 1f, 0f, 38.6f), 0f);
                Props.B(stall, "Panel", new Vector3(0f, 1f, 0f), new Vector3(.04f, 1.8f, 1.6f), a.WallTile);
            }
            foreach (float along in new[] { 3.5f, 4.5f }) F.Sink(p, r.Name, Props.Against(r, P.Side.West, along, .45f, out float yaw), yaw);
            Transform wet = p.Hang(r.Name, "Puddle", new Vector3(58f, .004f, 35f), 0f);
            Props.B(wet, "Water", Vector3.zero, new Vector3(3f, .004f, 2f), a.Wet);
        }

        // Workshop: x 8-20, z 28-40; wide door south to the bay (x 14), the tower's door in its west wall (z 34).
        private static void Workshop(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Workshop");
            F.Workbench(p, r.Name, Props.Against(r, P.Side.North, 6f, .8f, out float yaw), yaw, 2.6f);
            Transform rack = p.Place(r.Name, "HoseTestRack", Props.Against(r, P.Side.East, 6f, .6f, out float ry), ry, new Vector3(3f, 1.4f, .6f));
            if (rack != null)
            {
                Props.B(rack, "Frame", new Vector3(0f, .7f, 0f), new Vector3(3f, .06f, .55f), a.Red);
                foreach (float x in new[] { -1.45f, 1.45f }) Props.B(rack, "Leg", new Vector3(x, .35f, 0f), new Vector3(.06f, .7f, .5f), a.Red);
                Props.B(rack, "Hose", new Vector3(0f, .8f, 0f), new Vector3(2.8f, .1f, .14f), a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f));
            }
            F.Crate(p, r.Name, new Vector3(10.4f, 0f, 30.6f), 0f, new Vector3(1f, 1f, .6f));
            F.Drum(p, r.Name, new Vector3(18.8f, 0f, 29.6f), a.Red);
            ctx.Hiding.Cabinet(r, P.Side.North, 10.2f);
            ctx.AddLoot(L.Apparatus, new Vector3(13f, 0f, 32f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Apparatus, new Vector3(16f, 0f, 36.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Apparatus, new Vector3(10.6f, 0f, 36.8f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(16.6f, 0f, 33.4f));
        }

        // SCBA refill room: x 20-32, z 28-40; wide door south to the bay (x 26), door east to the hall (z 34).
        private static void Scba(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("SCBA_Room");
            Transform cascade = p.Place(r.Name, "CascadeBank", Props.Against(r, P.Side.North, 3f, .7f, out float yaw), yaw, new Vector3(2.6f, 2f, .7f));
            if (cascade != null)
            {
                for (int i = 0; i < 4; i++) Props.C(cascade, "Cylinder", new Vector3(-.9f + i * .6f, .95f, 0f), new Vector3(.45f, .9f, .45f), a.Safety);
                Props.B(cascade, "Manifold", new Vector3(0f, 1.95f, 0f), new Vector3(2.6f, .08f, .1f), a.Brass);
            }
            Transform station = p.Place(r.Name, "FillStation", Props.Against(r, P.Side.West, 6f, .9f, out float sy), sy, new Vector3(1.6f, 1.8f, .9f));
            if (station != null)
            {
                Props.B(station, "Cabinet", new Vector3(0f, .9f, 0f), new Vector3(1.6f, 1.8f, .9f), a.Accent);
                Props.B(station, "Shield", new Vector3(0f, 1.1f, .46f), new Vector3(1.3f, .9f, .02f), a.Glass);
                for (int i = 0; i < 3; i++) Props.C(station, "Gauge", new Vector3(-.4f + i * .4f, 1.65f, .46f), new Vector3(.14f, .01f, .14f), a.White).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 8.5f, .5f, out float shy), shy, 2f, 91, a.Rubber);
            F.Table(p, r.Name, new Vector3(26f, 0f, 35f), 0f, 1.8f, .8f, a.Steel);
            ctx.Hiding.Cage(r, P.Side.North, 10.8f);
            ctx.AddLoot(L.Apparatus, new Vector3(26f, 0f, 31.6f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Apparatus, new Vector3(29.6f, 0f, 30.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Apparatus, new Vector3(23f, 0f, 36.8f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(28.6f, 0f, 36.4f));
        }
    }
}
