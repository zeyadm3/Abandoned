using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's upper floor (y 4): the bunk room, the captain's quarters, the bathroom, the fitness room (weak,
    /// over the kitchen) and the storage loft (old timber over the hose workshop, the weakest floor here). The hall and
    /// the hose gallery never fall; the pole is in the hall (<see cref="FireStationTower"/>).
    /// </summary>
    public static class FireStationUpper
    {
        private const float Y = P.Story;

        public static void Dress(StationContext ctx)
        {
            Bunks(ctx);
            Captain(ctx);
            Bathroom(ctx);
            Fitness(ctx);
            Loft(ctx);
        }

        // Bunk room: x 28-48, z 0-16; doors west at z 6 and 14, north to the bathroom at x 42.
        private static void Bunks(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Bunk_Room");
            Material blanket = a.Plain("Blanket", new Color(.42f, .1f, .08f), .1f);
            foreach (float along in new[] { 2.5f, 6f, 9.5f, 13f, 16.5f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.South, along, 2.05f, out float yaw), yaw, false, blanket);
            foreach (float along in new[] { 3.5f, 7f, 10.5f, 17.5f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.North, along, 2.05f, out float yaw), yaw, false, blanket);
            foreach (float along in new[] { 4.25f, 7.75f, 11.25f, 14.75f })
                F.Crate(p, r.Name, Props.Against(r, P.Side.South, along, .45f, out float yaw), yaw, new Vector3(.45f, .55f, .45f));
            for (int i = 0; i < 4; i++) ctx.Hiding.Locker(r, P.Side.West, 8.6f + i * .8f);
            F.WallClock(p, r.Name, new Vector3(38f, Y + 2.5f, 15.88f), 180f);
            foreach (Vector3 at in new[] { new Vector3(32.2f, Y, 5f), new Vector3(39.5f, Y, 5f), new Vector3(46f, Y, 8f), new Vector3(35.2f, Y, 11f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(40f, Y, 9.2f));
        }

        // Captain's quarters: x 28-36, z 16-24; door west at z 18. The wardrobe is a hiding place.
        private static void Captain(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Captain_Quarters");
            F.Bunk(p, r.Name, Props.Against(r, P.Side.North, 5f, 2.05f, out float yaw), yaw, false);
            F.Desk(p, r.Name, Props.Against(r, P.Side.East, 2.5f, .8f, out yaw), yaw, false, 1.4f);
            ctx.Hiding.Closet(r, P.Side.South, 5.5f);
            F.Sofa(p, r.Name, Props.Against(r, P.Side.West, 6.2f, .9f, out yaw), yaw, 1.1f, ctx.Art.Leather);
            ctx.AddLoot(L.Quarters, new Vector3(30.6f, Y, 20.2f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(33.6f, Y, 20.6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Bathroom: x 36-48, z 16-24; doors south and north at x 42. Showers on the east wall, toilets and sinks west.
        private static void Bathroom(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Bathroom");
            for (int i = 0; i < 3; i++)
            {
                Transform stall = p.Place(r.Name, "ShowerStall", Props.Against(r, P.Side.East, 1.6f + i * 1.7f, 1.3f, out float yaw), yaw, new Vector3(1.5f, 2.1f, 1.3f), false);
                if (stall == null) continue;
                Props.B(stall, "Partition", new Vector3(.73f, 1.05f, 0f), new Vector3(.04f, 2.1f, 1.3f), a.WallTile);
                Props.B(stall, "Head", new Vector3(0f, 2.05f, -.5f), new Vector3(.12f, .06f, .2f), a.Chrome);
            }
            foreach (float along in new[] { 1.4f, 3f })
                F.Toilet(p, r.Name, Props.Against(r, P.Side.North, along, .7f, out float yaw), yaw);
            foreach (float along in new[] { 3f, 4f, 5f })
                F.Sink(p, r.Name, Props.Against(r, P.Side.West, along, .45f, out float yaw), yaw);
            Transform wet = p.Hang(r.Name, "Puddle", new Vector3(45.5f, Y + .004f, 20f), 0f);
            Props.B(wet, "Water", Vector3.zero, new Vector3(2.4f, .004f, 3f), a.Wet);
        }

        // Fitness room: x 28-48, z 24-36; doors west at z 30, south to the bathroom at x 42.
        private static void Fitness(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Fitness_Room");
            foreach (float along in new[] { 3f, 6f, 9f })
                F.Treadmill(p, r.Name, Props.Against(r, P.Side.North, along, 1.8f, out float yaw), yaw);
            F.WeightBench(p, r.Name, new Vector3(45.6f, Y, 29.5f), 90f);
            F.WeightBench(p, r.Name, new Vector3(45.6f, Y, 33f), 90f);
            F.PunchingBag(p, r.Name, new Vector3(40f, Y, 31f));
            Transform mats = p.Hang(r.Name, "Mats", new Vector3(36f, Y + .01f, 28.5f), 0f);
            Props.B(mats, "Mat", Vector3.zero, new Vector3(5f, .02f, 3f), ctx.Art.Rubber);
            foreach (Vector3 at in new[] { new Vector3(35.5f, Y, 31.5f), new Vector3(44f, Y, 26.6f), new Vector3(42.5f, Y, 34.6f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(39.6f, Y, 33.8f));
        }

        // Storage loft: x 8-24, z 28-36; doors south to the hose gallery (x 22) and east to the hall (z 34). Old timber.
        private static void Loft(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Storage_Loft");
            int seed = 70;
            foreach (Vector3 at in new[] { new Vector3(9.6f, Y, 29.4f), new Vector3(10.8f, Y, 34.8f), new Vector3(14.2f, Y, 34.9f), new Vector3(17.6f, Y, 29.3f) })
                F.Crate(p, r.Name, at, (seed++ % 4) * 22.5f, new Vector3(1.1f, .8f, .9f));
            F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 12f, .5f, out float yaw), yaw, 2.4f, seed, a.Board);
            Transform wheel = p.Place(r.Name, "OldWagonWheel", new Vector3(9.4f, Y, 32.3f), 90f, new Vector3(1.3f, 1.35f, .2f));
            if (wheel != null)
            {
                Props.C(wheel, "Rim", new Vector3(0f, .66f, 0f), new Vector3(1.3f, .05f, 1.3f), a.DarkWood).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Props.B(wheel, "Spoke_A", new Vector3(0f, .66f, 0f), new Vector3(1.2f, .05f, .05f), a.DarkWood);
                Props.B(wheel, "Spoke_B", new Vector3(0f, .66f, 0f), new Vector3(.05f, 1.2f, .05f), a.DarkWood);
            }
            foreach (Vector3 at in new[] { new Vector3(12.5f, Y, 31f), new Vector3(16.2f, Y, 32.6f), new Vector3(20.4f, Y, 31.4f) })
                ctx.AddLoot(L.Watch, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(14.4f, Y, 29.6f));
        }
    }
}
