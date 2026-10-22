using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.PoliceStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The precinct's upper floor (y 4): the records archive, the detectives' bureau, the gym over the sally port, and
    /// along the north side the evidence room (its weak floor is the map's headline), the captain's office and the break
    /// room. The hall (row 6) and the landings never fall; every room here can.
    /// </summary>
    public static class PoliceStationUpper
    {
        private const float Y = P.Story;

        public static void Dress(StationContext ctx)
        {
            Archive(ctx);
            Bureau(ctx);
            Gym(ctx);
            Evidence(ctx);
            Captain(ctx);
            BreakRoom(ctx);
            Props p = ctx.Props;
            F.WaterCooler(p, "Upper_Hall", new Vector3(.6f, Y, 21f), 90f);
            F.ExitSign(p, "Upper_Hall", new Vector3(10f, Y + 3.3f, 27.88f), 180f);
        }

        // Records archive: x 0-12, z 0-16, door north at x 2.
        private static void Archive(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Records_Archive");
            int seed = 20;
            foreach (float x in new[] { 3.2f, 6.4f, 9.6f })
            foreach (float z in new[] { 2.4f, 4.6f, 8.4f, 10.6f })
                F.Shelving(p, r.Name, new Vector3(x, Y, z), 90f, 2f, seed++);
            F.Desk(p, r.Name, new Vector3(9f, Y, 14f), 180f);
            F.Chair(p, r.Name, new Vector3(9f, Y, 13.1f), 0f);
            foreach (Vector3 at in new[] { new Vector3(1.6f, Y, 4.5f), new Vector3(4.8f, Y, 4.5f), new Vector3(8f, Y, 6.5f), new Vector3(11f, Y, 9.5f), new Vector3(5.5f, Y, 13.5f) })
                ctx.AddLoot(L.Office, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(4.8f, Y, 9.5f));
        }

        // Detectives' bureau: x 12-40, z 0-24; doors north at x 26 (wide) and 18, east to the gym at z 10.
        private static void Bureau(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Detective_Bureau");
            foreach (float z in new[] { 3f, 9.5f, 16f })
            foreach (float x in new[] { 15.5f, 19.5f, 23.5f, 28.5f, 32.5f, 36.5f })
            {
                F.Desk(p, r.Name, new Vector3(x, Y, z), 0f, (int)(x + z) % 3 != 0);
                F.Chair(p, r.Name, new Vector3(x, Y, z + .95f), 180f);
            }
            for (int i = 0; i < 5; i++)
                F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.North, 18.2f + i * .55f, .64f, out float yaw), yaw);
            F.Table(p, r.Name, Props.Against(r, P.Side.North, 2.2f, .7f, out float y2), y2, 1.6f, .7f);
            F.WallBoard(p, r.Name, new Vector3(12.12f, Y + 1.6f, 8f), 90f, 3.4f, 1.4f, ctx.Art.Board);
            F.WallClock(p, r.Name, new Vector3(39.88f, Y + 2.5f, 4f), 270f);
            foreach (float z in new[] { 6.3f, 12.8f })
            foreach (float x in new[] { 17.5f, 26f, 34.5f })
                ctx.AddLoot(L.Office, new Vector3(x, Y, z), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(26f, Y, 19.6f), CarryClass.Pocket, CarryClass.Heavy);
            ctx.AddLoot(L.Office, new Vector3(38.5f, Y, 20.5f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(14f, Y, 1.5f));
        }

        // Gym: x 40-56, z 0-24, over the sally port; doors west at z 10 and north at x 46.
        private static void Gym(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Gym");
            for (int i = 0; i < 4; i++)
                F.Treadmill(p, r.Name, Props.Against(r, P.Side.South, 3f + i * 3f, 1.8f, out float yaw), yaw);
            foreach (float x in new[] { 46f, 50f }) F.WeightBench(p, r.Name, new Vector3(x, Y, 12f), 90f);
            Transform rack = p.Place(r.Name, "DumbbellRack", Props.Against(r, P.Side.East, 12f, .6f, out float rackYaw), rackYaw, new Vector3(2.4f, .9f, .6f));
            if (rack != null)
            {
                Props.B(rack, "Frame", new Vector3(0f, .4f, 0f), new Vector3(2.4f, .06f, .55f), a.DarkSteel);
                for (int i = 0; i < 8; i++) Props.B(rack, "Dumbbell", new Vector3(-1.05f + i * .3f, .5f, 0f), new Vector3(.12f, .14f, .35f), a.Rubber);
                foreach (float x in new[] { -1.15f, 1.15f }) Props.B(rack, "Leg", new Vector3(x, .2f, 0f), new Vector3(.06f, .4f, .5f), a.DarkSteel);
            }
            Transform squat = p.Place(r.Name, "SquatRack", new Vector3(52.5f, Y, 19f), 0f, new Vector3(1.6f, 2.3f, 1.4f));
            if (squat != null)
                foreach (float x in new[] { -.75f, .75f })
                foreach (float z in new[] { -.65f, .65f })
                    Props.B(squat, "Post", new Vector3(x, 1.15f, z), new Vector3(.07f, 2.3f, .07f), a.DarkSteel);
            F.PunchingBag(p, r.Name, new Vector3(44f, Y, 15f));
            Transform mats = p.Hang(r.Name, "Mats", new Vector3(48f, Y + .01f, 5.5f), 0f);
            Props.B(mats, "Mat", Vector3.zero, new Vector3(6f, .02f, 3f), a.Rubber);
            foreach (Vector3 at in new[] { new Vector3(44f, Y, 4.5f), new Vector3(52f, Y, 5f), new Vector3(52f, Y, 16f), new Vector3(43.5f, Y, 19.5f) })
                ctx.AddLoot(L.Armoury, at, CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Evidence room: x 0-16, z 28-40, wide door south at x 6. A wire cage across it with a gap on the haul line.
        private static void Evidence(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Evidence_Room");
            foreach (float along in new[] { 4.5f, 7f, 9.5f })
            {
                F.Shelving(p, r.Name, Props.Against(r, P.Side.West, along, .5f, out float yaw), yaw, 2.4f, (int)along, ctx.Art.Paper);
                F.Shelving(p, r.Name, Props.Against(r, P.Side.East, along, .5f, out yaw), yaw, 2.4f, (int)along + 5);
            }
            F.Shelving(p, r.Name, new Vector3(4f, Y, 34.8f), 90f, 2.6f, 11);
            F.Shelving(p, r.Name, new Vector3(12f, Y, 34.8f), 90f, 2.6f, 12, ctx.Art.Paper);
            ctx.Hiding.Cabinet(r, P.Side.North, 8f);
            foreach ((float x, float w) in new[] { (2.2f, 4f), (11.8f, 8f) })
            {
                Transform cage = p.Place(r.Name, "CageWall", new Vector3(x, Y, 30.6f), 0f, new Vector3(w, 2.6f, .08f));
                if (cage == null) continue;
                Props.B(cage, "Mesh", new Vector3(0f, 1.3f, 0f), new Vector3(w, 2.6f, .02f), ctx.Art.Grating);
                Props.B(cage, "Rail", new Vector3(0f, 2.58f, 0f), new Vector3(w, .05f, .06f), ctx.Art.Steel);
            }
            F.Table(p, r.Name, new Vector3(13.5f, Y, 29.4f), 0f, 1.6f, .7f, ctx.Art.Steel);
            ctx.AddLoot(L.Evidence, new Vector3(8f, Y, 32.5f), CarryClass.Pocket, CarryClass.Huge, true);
            foreach (Vector3 at in new[] { new Vector3(2.3f, Y, 32.6f), new Vector3(13.8f, Y, 32.6f), new Vector3(8f, Y, 36.5f), new Vector3(2.3f, Y, 38.8f), new Vector3(13.8f, Y, 38.8f) })
                ctx.AddLoot(L.Evidence, at, CarryClass.Pocket, CarryClass.Heavy);
            ctx.Threat(new Vector3(13.6f, Y, 35.6f));
        }

        // Captain's office: x 16-28, z 28-40, wide door south at x 22.
        private static void Captain(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Captain_Office");
            F.Desk(p, r.Name, new Vector3(22f, Y, 35.2f), 0f, true, 2f);
            F.Chair(p, r.Name, new Vector3(22f, Y, 36.2f), 180f);
            F.Chair(p, r.Name, new Vector3(21f, Y, 33.9f), 0f, false);
            F.Chair(p, r.Name, new Vector3(23f, Y, 33.9f), 0f, false);
            F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 3f, .5f, out float yaw), yaw, 2f, 31, a.DarkWood);
            F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 9f, .5f, out yaw), yaw, 2f, 32, a.Leather);
            F.Sofa(p, r.Name, Props.Against(r, P.Side.West, 6f, .9f, out yaw), yaw, 2f, a.Leather);
            Transform flag = p.Place(r.Name, "FlagStand", new Vector3(27.3f, Y, 38.6f), 0f, new Vector3(.4f, 2.4f, .4f));
            if (flag != null)
            {
                Props.C(flag, "Pole", new Vector3(0f, 1.2f, 0f), new Vector3(.04f, 1.2f, .04f), a.Brass);
                Props.B(flag, "Cloth", new Vector3(.32f, 1.85f, 0f), new Vector3(.6f, .7f, .01f), a.Accent);
                Props.C(flag, "Base", new Vector3(0f, .03f, 0f), new Vector3(.35f, .03f, .35f), a.DarkSteel);
            }
            F.WallClock(p, r.Name, new Vector3(18f, Y + 2.5f, 28.12f), 0f);
            F.Plant(p, r.Name, new Vector3(16.6f, Y, 38.9f));
            ctx.AddLoot(L.Office, new Vector3(26f, Y, 31f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Office, new Vector3(18.5f, Y, 31f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(25.5f, Y, 36.6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Break room: x 28-44, z 28-40; door south at x 34, east at z 34.
        private static void BreakRoom(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Break_Room");
            F.Counter(p, r.Name, Props.Against(r, P.Side.North, 4f, .7f, out float yaw), yaw, 4f, .92f, ctx.Art.White);
            Transform basin = p.Hang(r.Name, "SinkBasin", new Vector3(31f, Y + .92f, 39.4f), 0f);
            Props.B(basin, "Basin", new Vector3(0f, -.06f, 0f), new Vector3(.6f, .12f, .45f), ctx.Art.Chrome);
            Props.B(basin, "Tap", new Vector3(0f, .1f, .18f), new Vector3(.04f, .2f, .04f), ctx.Art.Chrome);
            F.Fridge(p, r.Name, Props.Against(r, P.Side.North, 7f, .72f, out yaw), yaw);
            F.Stove(p, r.Name, Props.Against(r, P.Side.North, 8.4f, .68f, out yaw), yaw);
            foreach (float x in new[] { 32f, 38f })
            {
                F.Table(p, r.Name, new Vector3(x, Y, 33f), 0f, 1.4f, .9f, ctx.Art.White);
                F.Chair(p, r.Name, new Vector3(x, Y, 32f), 0f, false);
                F.Chair(p, r.Name, new Vector3(x, Y, 34.1f), 180f, false);
            }
            F.Sofa(p, r.Name, Props.Against(r, P.Side.North, 13f, .9f, out yaw), yaw, 2f);
            F.Television(p, r.Name, new Vector3(41f, Y, 36f), 0f);
            F.Vending(p, r.Name, Props.Against(r, P.Side.South, 2f, .8f, out yaw), yaw);
            foreach (Vector3 at in new[] { new Vector3(30.5f, Y, 36.5f), new Vector3(38f, Y, 30.6f), new Vector3(35.5f, Y, 35.6f) })
                ctx.AddLoot(L.Office, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(39.5f, Y, 36.9f));
        }
    }
}
