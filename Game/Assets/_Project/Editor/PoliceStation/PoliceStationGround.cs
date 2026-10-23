using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.PoliceStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The precinct's ground floor fittings, hiding places and loot and monster points, room by room (Docs/
    /// POLICE_STATION_PLAN.md). Coordinates are world metres; each room's comment gives its extent. The carry route (the
    /// main hall, row 3, and the sally port lane to the vehicle door) is kept clear before anything is placed.
    /// </summary>
    public static class PoliceStationGround
    {
        public static void Dress(StationContext ctx)
        {
            Props p = ctx.Props;
            // The haul lane across the sally port, from the hall door to the vehicle door, and the main hall itself.
            p.KeepOut(0, new Rect(40f, 6.5f, 16f, 9.5f));
            p.KeepOut(0, new Rect(0f, 12.3f, 40f, 3.4f));
            Records(ctx);
            Lobby(ctx);
            SquadRoom(ctx);
            SallyPort(ctx);
            Booking(ctx);
            Holding(ctx);
            Interview(ctx);
            Dispatch(ctx);
            Briefing(ctx);
            LockerRoom(ctx);
            Variety(ctx);
        }

        // The things that make each room its own: coat racks, bins, litter, a chair knocked over, extinguishers in the
        // halls, a radiator under a window; and in the cell block, the ceiling showing the floor above going.
        private static void Variety(StationContext ctx)
        {
            Props p = ctx.Props;
            Material coat = ctx.Art.Plain("UniformCoat", new Color(.12f, .14f, .2f), .2f);
            F.CoatRack(p, "Records", new Vector3(10.8f, 0f, 10.6f), coat);
            F.TrashCan(p, "Records", new Vector3(1f, 0f, 10.8f));
            F.FloorLitter(p, "Records", new Vector3(8f, 0f, 6.2f), 1);
            F.Radiator(p, "Lobby", new Vector3(14f, 0f, .52f), 0f);
            F.TrashCan(p, "Lobby", new Vector3(13f, 0f, 8.8f), true);
            F.CoatRack(p, "Squad_Room", new Vector3(29f, 0f, 10.8f), coat);
            F.TrashCan(p, "Squad_Room", new Vector3(36.2f, 0f, 10.9f));
            F.FloorLitter(p, "Squad_Room", new Vector3(33f, 0f, 10f), 2);
            F.Extinguisher(p, "Booking", new Vector3(28.2f, 1.2f, 25.4f), 90f);
            F.CeilingDamage(p, "Holding_Cells", new Vector3(20f, 0f, 27f), P.Story - P.Slab, 21);
            F.CeilingDamage(p, "Holding_Cells", new Vector3(24.4f, 0f, 34.4f), P.Story - P.Slab, 22);
            F.FloorLitter(p, "Holding_Cells", new Vector3(18f, 0f, 30f), 4);
            F.TrashCan(p, "Dispatch", new Vector3(51f, 0f, 26.6f));
            F.FallenChair(p, "Briefing_Room", new Vector3(43.6f, 0f, 37.9f), 30f);
            F.TrashCan(p, "Locker_Room", new Vector3(9f, 0f, 25.2f));
            F.Extinguisher(p, "Main_Hall", new Vector3(10f, 1.2f, 12.12f), 0f);
            F.Extinguisher(p, "Main_Hall", new Vector3(30f, 1.2f, 15.88f), 180f);
        }

        // Records: x 0-12, z 0-12, door north at x 6.
        private static void Records(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Records");
            for (int row = 0; row < 3; row++)
            for (int i = 0; i < 2; i++)
                F.Shelving(p, r.Name, new Vector3(3.2f + row * 3.2f, 0f, 2.6f + i * 2.2f), 90f, 2f, row * 3 + i);
            for (int i = 0; i < 4; i++)
                F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.East, 7f + i * .55f, .64f, out float yaw), yaw);
            F.Desk(p, r.Name, new Vector3(3f, 0f, 9.3f), 180f);
            F.Chair(p, r.Name, new Vector3(3f, 0f, 8.4f), 0f);
            F.Table(p, r.Name, new Vector3(8.5f, 0f, 9f), 0f, 1.6f, .8f);
            foreach (Vector3 at in new[] { new Vector3(1.6f, 0f, 3.7f), new Vector3(4.8f, 0f, 3.7f), new Vector3(8f, 0f, 3.7f), new Vector3(6f, 0f, 7.3f) })
                ctx.AddLoot(L.Office, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(10.6f, 0f, 3.7f), CarryClass.Pocket, CarryClass.Heavy);
        }

        // Lobby: x 12-28, z 0-12; street doors south at x 20, wide door north at x 22.
        private static void Lobby(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Lobby");
            p.KeepOut(0, new Rect(18f, 0f, 8f, 12f));
            F.Counter(p, r.Name, new Vector3(15f, 0f, 7.6f), 180f, 5f, 1.1f);
            // The glass screen on the counter: bullet-resistant once, cracked now.
            Transform screen = p.Hang(r.Name, "CounterScreen", new Vector3(15f, 1.1f, 7.35f), 0f);
            Props.B(screen, "Glass", new Vector3(0f, .45f, 0f), new Vector3(5f, .9f, .03f), ctx.Art.Glass);
            Props.B(screen, "Frame", new Vector3(0f, .92f, 0f), new Vector3(5.04f, .05f, .06f), ctx.Art.DarkSteel);
            F.Desk(p, r.Name, new Vector3(14.2f, 0f, 10f), 180f);
            F.Chair(p, r.Name, new Vector3(14.2f, 0f, 9.1f), 0f);
            for (int i = 0; i < 2; i++)
                F.Bench(p, r.Name, Props.Against(r, P.Side.East, 2.5f + i * 3f, .4f, out float yaw), yaw, 2f);
            F.Plant(p, r.Name, new Vector3(12.8f, 0f, .8f));
            F.Plant(p, r.Name, new Vector3(27.2f, 0f, .8f));
            F.WaterCooler(p, r.Name, new Vector3(13f, 0f, 4.5f), 90f);
            F.WallBoard(p, r.Name, new Vector3(12.12f, 1.6f, 3f), 90f, 1.6f, 1.1f, ctx.Art.Board);
            F.WallClock(p, r.Name, new Vector3(20f, 2.5f, 11.87f), 180f);
            F.ExitSign(p, r.Name, new Vector3(20f, 3.3f, .26f), 0f);
            ctx.AddLoot(L.Office, new Vector3(25.4f, 0f, 9.4f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Office, new Vector3(15f, 0f, 10.3f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(14.5f, 0f, 3.5f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Squad room: x 28-40, z 0-12; door north at x 34, door east to the sally port at z 6.
        private static void SquadRoom(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Squad_Room");
            foreach (float x in new[] { 30.5f, 34f, 37.2f })
            {
                F.Desk(p, r.Name, new Vector3(x, 0f, 2.6f), 0f);
                F.Chair(p, r.Name, new Vector3(x, 0f, 3.6f), 180f);
            }
            foreach (float x in new[] { 30.5f, 34f })
            {
                F.Desk(p, r.Name, new Vector3(x, 0f, 7.6f), 0f);
                F.Chair(p, r.Name, new Vector3(x, 0f, 8.6f), 180f);
            }
            F.Vending(p, r.Name, Props.Against(r, P.Side.North, 10.4f, .8f, out float yaw), yaw);
            F.WallBoard(p, r.Name, new Vector3(28.12f, 1.6f, 6f), 90f, 2.4f, 1.2f, ctx.Art.White);
            F.WallClock(p, r.Name, new Vector3(39.88f, 2.5f, 2.5f), 270f);
            foreach (Vector3 at in new[] { new Vector3(32.2f, 0f, 5.3f), new Vector3(35.8f, 0f, 5.3f), new Vector3(30f, 0f, 10.5f), new Vector3(37.5f, 0f, 9.2f) })
                ctx.AddLoot(L.Office, at, CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Sally port: x 40-56, z 0-20; vehicle door east (z 7-13), doors west at z 6, 14 (wide) and 18, north at x 54.
        private static void SallyPort(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Sally_Port");
            // The prisoner van, parked against the south wall, its back doors hanging open.
            Transform van = p.Place(r.Name, "PrisonerVan", new Vector3(48f, 0f, 3.3f), 90f, new Vector3(2.2f, 2.6f, 5.6f));
            if (van != null)
            {
                StationArt a = ctx.Art;
                Props.B(van, "Body", new Vector3(0f, 1.45f, -.3f), new Vector3(2.1f, 2.1f, 4.4f), a.White);
                Props.B(van, "Cab", new Vector3(0f, 1.15f, 2.3f), new Vector3(2.05f, 1.5f, 1f), a.White);
                Props.B(van, "Stripe", new Vector3(0f, 1.2f, 0f), new Vector3(2.12f, .25f, 5.4f), a.Accent);
                Props.B(van, "Windscreen", new Vector3(0f, 1.6f, 2.81f), new Vector3(1.9f, .6f, .03f), a.Glass);
                Props.B(van, "RearCage", new Vector3(0f, 1.5f, -2.52f), new Vector3(1.9f, 1.7f, .04f), a.Bars);
                foreach (float s in new[] { -1f, 1f })
                foreach (float z in new[] { -1.6f, 1.9f })
                {
                    GameObject wheel = Props.C(van, "Tyre", new Vector3(s * .95f, .38f, z), new Vector3(.76f, .14f, .76f), a.Rubber);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
                Props.B(van, "LightBar", new Vector3(0f, 2.62f, 1.9f), new Vector3(1.3f, .1f, .25f), a.Red);
            }
            F.Table(p, r.Name, Props.Against(r, P.Side.North, 3.5f, .9f, out float yaw), yaw, 2.2f, .9f, ctx.Art.Steel);
            F.Lockers(p, r.Name, Props.Against(r, P.Side.North, 7.5f, .52f, out yaw), yaw, 5);
            F.Drum(p, r.Name, new Vector3(54.6f, 0f, 1.6f));
            F.Drum(p, r.Name, new Vector3(53.8f, 0f, 1.2f), ctx.Art.Red);
            F.Crate(p, r.Name, new Vector3(42f, 0f, 1.4f), 0f, new Vector3(1.2f, .9f, 1f));
            // Oil on the floor where the cars stood.
            Transform stains = p.Hang(r.Name, "OilStains", new Vector3(47f, .003f, 11f), 0f);
            Props.B(stains, "Stain", Vector3.zero, new Vector3(2.4f, .004f, 3.2f), ctx.Art.Oil);
            F.ExitSign(p, r.Name, new Vector3(55.7f, 3.55f, 10f), 270f);
            foreach (Vector3 at in new[] { new Vector3(44.5f, 0f, 17.5f), new Vector3(52f, 0f, 3.5f), new Vector3(42.2f, 0f, 3.5f) })
                ctx.AddLoot(L.Evidence, at, CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Booking: x 28-40, z 16-28; wide door south at x 34, east doors at z 18 and 22, west door at z 22, north doors at x 30 and 38.
        private static void Booking(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Booking");
            F.Counter(p, r.Name, new Vector3(33f, 0f, 24.5f), 180f, 5f, 1.1f, ctx.Art.DarkSteel);
            F.Chair(p, r.Name, new Vector3(32f, 0f, 25.5f), 180f);
            F.Chair(p, r.Name, new Vector3(34.5f, 0f, 25.5f), 180f);
            F.Bench(p, r.Name, Props.Against(r, P.Side.South, 2.5f, .4f, out float yaw), yaw, 1.8f);
            F.Table(p, r.Name, new Vector3(37f, 0f, 25f), 0f, 1.6f, .8f, ctx.Art.Steel);
            // The mugshot wall: a height board painted on the west wall and the camera's tripod.
            F.WallBoard(p, r.Name, new Vector3(28.12f, 1.2f, 18.5f), 90f, 1.3f, 2.1f, ctx.Art.White);
            Transform tripod = p.Place(r.Name, "CameraTripod", new Vector3(31.5f, 0f, 18.5f), 270f, new Vector3(.5f, 1.5f, .5f));
            if (tripod != null)
            {
                Props.B(tripod, "Leg", new Vector3(0f, .7f, 0f), new Vector3(.04f, 1.4f, .04f), ctx.Art.DarkSteel);
                Props.B(tripod, "Head", new Vector3(0f, 1.45f, .05f), new Vector3(.2f, .15f, .3f), ctx.Art.Rubber);
            }
            ctx.AddLoot(L.Cells, new Vector3(31f, 0f, 21f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Cells, new Vector3(36.6f, 0f, 21f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Cells, new Vector3(34f, 0f, 26.6f), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Holding cells: x 12-28, z 20-40; doors south at x 22 and east at z 22. Six cells on the west wall, three on the north.
        private static void Holding(StationContext ctx)
        {
            P.Room r = L.Room("Holding_Cells");
            for (int i = 0; i < 6; i++) ctx.Hiding.Cell(r, P.Side.West, 3f + i * 3f, 2.8f);
            for (int i = 0; i < 3; i++) ctx.Hiding.Cell(r, P.Side.North, 6f + i * 3f, 2.8f);
            F.Desk(ctx.Props, r.Name, Props.Against(r, P.Side.East, 13f, .8f, out float yaw), yaw, false, 1.4f);
            F.Chair(ctx.Props, r.Name, new Vector3(26.2f, 0f, 33f), 90f);
            F.WallClock(ctx.Props, r.Name, new Vector3(27.88f, 2.5f, 27f), 270f);
            foreach (Vector3 at in new[] { new Vector3(20f, 0f, 25.5f), new Vector3(24.5f, 0f, 30.5f), new Vector3(19f, 0f, 32.5f) })
                ctx.AddLoot(L.Cells, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(22f, 0f, 29f));
        }

        // Interview: x 28-36, z 28-40, door south at x 30. Observation: x 36-40, door south at x 38. The mirror between.
        private static void Interview(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Interview");
            F.Table(p, r.Name, new Vector3(32f, 0f, 34f), 0f, 1.4f, .9f, ctx.Art.Steel);
            F.Chair(p, r.Name, new Vector3(32f, 0f, 33f), 0f, false);
            F.Chair(p, r.Name, new Vector3(32f, 0f, 35.1f), 180f, false);
            ctx.Hiding.Closet(r, P.Side.North, 6.5f);
            F.WallBoard(p, r.Name, new Vector3(35.88f, 1.5f, 34f), 270f, 2.4f, 1.1f, ctx.Art.Glass);
            ctx.AddLoot(L.Cells, new Vector3(29.8f, 0f, 37.5f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(30.5f, 0f, 31.5f));

            P.Room o = L.Room("Observation");
            F.Desk(p, o.Name, Props.Against(o, P.Side.East, 6f, .8f, out float yaw), yaw, true, 1.4f);
            F.Chair(p, o.Name, new Vector3(38.3f, 0f, 34f), 90f);
            F.WallBoard(p, o.Name, new Vector3(36.12f, 1.5f, 34f), 90f, 2.4f, 1.1f, ctx.Art.Glass);
            ctx.AddLoot(L.Cells, new Vector3(38f, 0f, 37.6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Dispatch: x 40-52, z 20-28; doors west at z 22, east at z 22, north at x 46.
        private static void Dispatch(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Dispatch");
            F.Counter(p, r.Name, Props.Against(r, P.Side.South, 6f, .7f, out float yaw), yaw, 8f, .78f, ctx.Art.DarkSteel);
            Transform consoles = p.Hang(r.Name, "RadioConsoles", new Vector3(46f, .78f, 20.55f), 0f);
            for (int i = 0; i < 4; i++)
            {
                Props.B(consoles, "Console", new Vector3(-3f + i * 2f, .2f, 0f), new Vector3(1.2f, .4f, .4f), ctx.Art.Rubber);
                Props.B(consoles, "Screen", new Vector3(-3f + i * 2f, .55f, -.05f), new Vector3(.5f, .35f, .3f), ctx.Art.DarkSteel);
            }
            F.Chair(p, r.Name, new Vector3(44f, 0f, 21.7f), 180f);
            F.Chair(p, r.Name, new Vector3(48f, 0f, 21.7f), 180f);
            F.WallBoard(p, r.Name, new Vector3(49.6f, 1.7f, 27.88f), 180f, 2.4f, 1.3f, ctx.Art.Board);
            ctx.AddLoot(L.Office, new Vector3(43.5f, 0f, 25.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(49.6f, 0f, 25.6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Briefing room: x 40-52, z 28-40; door south at x 46, east at z 34 (the janitor's closet, x 52-56, z 32-40).
        private static void Briefing(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Briefing_Room");
            foreach (float z in new[] { 31.6f, 33.6f, 35.6f })
            foreach (float x in new[] { 42.5f, 44.3f, 47.7f, 49.5f })
                F.Chair(p, r.Name, new Vector3(x, 0f, z), 0f, false);
            F.Counter(p, r.Name, new Vector3(46f, 0f, 38.4f), 180f, 1.2f, 1.15f, ctx.Art.DarkWood);
            F.WallBoard(p, r.Name, new Vector3(46f, 1.7f, 39.8f), 180f, 3.6f, 1.3f, ctx.Art.White);
            ctx.AddLoot(L.Office, new Vector3(42f, 0f, 38.3f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(50.6f, 0f, 38.3f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(46f, 0f, 36.8f));

            P.Room c = L.Room("Janitor_Closet");
            ctx.Hiding.Closet(c, P.Side.North, 2f);
            F.Shelving(p, c.Name, Props.Against(c, P.Side.East, 2.6f, .5f, out float yaw), yaw, 1.6f, 7);
            F.Drum(p, c.Name, new Vector3(53.1f, 0f, 36f), ctx.Art.Safety);
        }

        // Locker room: x 0-12, z 24-40; door south at x 2. The shower corner (x 0-8, z 32-40) is the rotten floor.
        private static void LockerRoom(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Locker_Room");
            for (int i = 0; i < 8; i++) ctx.Hiding.Locker(r, P.Side.East, 1.2f + i * .8f);
            F.Lockers(p, r.Name, Props.Against(r, P.Side.West, 4.5f, .52f, out float yaw), yaw, 6);
            F.Bench(p, r.Name, new Vector3(6.4f, 0f, 27f), 90f, 2.2f);
            // Shower stalls along the north wall, on the slab that gives way.
            for (int i = 0; i < 4; i++)
            {
                Transform stall = p.Place(r.Name, "ShowerStall", Props.Against(r, P.Side.North, 1.4f + i * 1.7f, 1.3f, out yaw), yaw, new Vector3(1.5f, 2.1f, 1.3f), false);
                if (stall == null) continue;
                Props.B(stall, "Partition", new Vector3(.73f, 1.05f, 0f), new Vector3(.04f, 2.1f, 1.3f), ctx.Art.WallTile);
                Props.B(stall, "Head", new Vector3(0f, 2.05f, -.5f), new Vector3(.12f, .06f, .2f), ctx.Art.Chrome);
                Props.B(stall, "Drain", new Vector3(0f, .006f, 0f), new Vector3(.18f, .01f, .18f), ctx.Art.DarkSteel);
            }
            for (int i = 0; i < 2; i++) F.Sink(p, r.Name, Props.Against(r, P.Side.South, 6f + i * .8f, .45f, out yaw), yaw);
            Transform wet = p.Hang(r.Name, "Puddle", new Vector3(4f, .004f, 35f), 0f);
            Props.B(wet, "Water", Vector3.zero, new Vector3(3.6f, .004f, 2.2f), ctx.Art.Wet);
            ctx.AddLoot(L.Armoury, new Vector3(6.4f, 0f, 30.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Armoury, new Vector3(3f, 0f, 28f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Armoury, new Vector3(4f, 0f, 35f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(5f, 0f, 31f));
        }
    }
}
