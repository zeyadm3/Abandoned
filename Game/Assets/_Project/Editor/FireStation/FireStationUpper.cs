using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's upper floor (y 4). Over the back of the bay: bunk rooms A and B on a weak floor (the pole goes down
    /// from B, its closet beside it), the hose mezzanine (weak, over Engine 9) and the storage loft (old timber). Over the
    /// quarters: officers' quarters, the bathroom, the training room, the fitness room (weak), the radio room, the chief's
    /// quarters and the attic. The hall (column 8) and the landing never fall. Under each weak floor the ceiling shows it.
    /// </summary>
    public static class FireStationUpper
    {
        private const float Y = P.Story;

        public static void Dress(StationContext ctx)
        {
            BunkA(ctx);
            BunkB(ctx);
            Mezzanine(ctx);
            Loft(ctx);
            Officers(ctx);
            Bathroom(ctx);
            Training(ctx);
            Fitness(ctx);
            Radio(ctx);
            Chief(ctx);
            Attic(ctx);
            Warnings(ctx);
            Props p = ctx.Props;
            F.ExitSign(p, "Upper_Hall", new Vector3(34f, Y + 2.9f, 39.74f), 180f);
            F.Extinguisher(p, "Upper_Hall", new Vector3(32.18f, Y + 1.2f, 20f), 90f);
            F.WaterCooler(p, "Upper_Hall", new Vector3(35.4f, Y, 2.2f), 270f);
        }

        private static Material Blanket(StationArt a) => a.Plain("Blanket", new Color(.42f, .1f, .08f), .1f);

        // Bunk room A: x 0-16, z 16-28, over the bay; doors east to B (z 22) and north to the loft (x 10).
        private static void BunkA(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Bunk_Room_A");
            foreach (float along in new[] { 2.5f, 5.5f, 8.5f, 11.5f, 14f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.South, along, 2.05f, out float yaw), yaw, false, Blanket(ctx.Art));
            foreach (float along in new[] { 2.5f, 5.5f, 13.5f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.North, along, 2.05f, out float yaw), yaw, false, Blanket(ctx.Art));
            for (int i = 0; i < 3; i++) ctx.Hiding.Locker(r, P.Side.West, 5f + i * .8f);
            F.FloorLitter(p, r.Name, new Vector3(9f, Y, 22.5f), 8);
            foreach (Vector3 at in new[] { new Vector3(7f, Y, 21.5f), new Vector3(12.5f, Y, 22.5f), new Vector3(4f, Y, 23.4f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(6f, Y, 23f));
        }

        // Bunk room B: x 16-32, z 16-28, over the bay; the pole at (22, 22); doors west to A, south to the mezzanine (x 30),
        // east to the hall (z 22). The pole-drop closet stands behind the pole.
        private static void BunkB(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Bunk_Room_B");
            foreach (float along in new[] { 1.8f, 5f, 9f, 11.8f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.South, along, 2.05f, out float yaw), yaw, false, Blanket(ctx.Art));
            foreach (float along in new[] { 2f, 10f, 13f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.North, along, 2.05f, out float yaw), yaw, false, Blanket(ctx.Art));
            ctx.Hiding.Closet(r, P.Side.North, 6f);
            for (int i = 0; i < 3; i++) ctx.Hiding.Locker(r, P.Side.East, 8f + i * .8f);
            F.WallClock(p, r.Name, new Vector3(24f, Y + 2.6f, 16.12f), 0f);
            foreach (Vector3 at in new[] { new Vector3(19.5f, Y, 21f), new Vector3(25f, Y, 22.6f), new Vector3(28f, Y, 21.2f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(27.6f, Y, 24f));
        }

        // The hose mezzanine: x 28-32, z 0-16, railed on the bay side, open to the upper hall. Weak.
        private static void Mezzanine(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            const string room = "Hose_Mezzanine";
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            Transform rolls = p.Hang(room, "HoseRolls", new Vector3(28f, Y, 8f), 0f);
            for (int i = 0; i < 6; i++)
                Props.C(rolls, "Roll", new Vector3(-.12f, .75f, -6.5f + i * 2.6f), new Vector3(.55f, .08f, .55f), canvas).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            F.Crate(p, room, new Vector3(30.6f, Y, 2.6f), 0f, new Vector3(1.2f, .8f, 1f));
            F.Crate(p, room, new Vector3(30.4f, Y, 7.2f), 15f, new Vector3(1f, .6f, .8f));
            F.Shelving(p, room, new Vector3(31.4f, Y, 11f), 270f, 2.4f, 101, canvas);
            ctx.AddLoot(L.Apparatus, new Vector3(29.8f, Y, 4.8f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Apparatus, new Vector3(29.8f, Y, 13.8f), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Storage loft: x 8-32, z 28-40, over the workshop and the SCBA room; doors south to bunk A (x 10) and east to the hall (z 34).
        private static void Loft(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Storage_Loft");
            int seed = 70;
            foreach (Vector3 at in new[] { new Vector3(12.5f, Y, 30.5f), new Vector3(15f, Y, 37.6f), new Vector3(21f, Y, 30.2f), new Vector3(27f, Y, 38f) })
                F.Crate(p, r.Name, at, (seed++ % 4) * 22.5f, new Vector3(1.1f, .8f, .9f));
            F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 10f, .5f, out float yaw), yaw, 2.4f, seed, a.Board);
            F.Bunk(p, r.Name, new Vector3(24.5f, Y, 34f), 90f, true);
            Transform wheel = p.Place(r.Name, "OldWagonWheel", new Vector3(9.4f, Y, 34f), 90f, new Vector3(1.3f, 1.35f, .2f));
            if (wheel != null)
            {
                Props.C(wheel, "Rim", new Vector3(0f, .66f, 0f), new Vector3(1.3f, .05f, 1.3f), a.DarkWood).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Props.B(wheel, "Spoke_A", new Vector3(0f, .66f, 0f), new Vector3(1.2f, .05f, .05f), a.DarkWood);
                Props.B(wheel, "Spoke_B", new Vector3(0f, .66f, 0f), new Vector3(.05f, 1.2f, .05f), a.DarkWood);
            }
            F.FloorLitter(p, r.Name, new Vector3(18f, Y, 34.5f), 9);
            foreach (Vector3 at in new[] { new Vector3(12f, Y, 34f), new Vector3(18f, Y, 31.6f), new Vector3(26.4f, Y, 31f), new Vector3(29.4f, Y, 37.6f), new Vector3(17.4f, Y, 38.6f) })
                ctx.AddLoot(L.Watch, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(20f, Y, 36.6f));
        }

        // Officers' quarters: x 36-48, z 0-12; doors west to the hall (z 6) and east to the bathroom (z 6).
        private static void Officers(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Officers_Quarters");
            foreach (float along in new[] { 2.5f, 6f, 9.5f })
                F.Bunk(p, r.Name, Props.Against(r, P.Side.South, along, 2.05f, out float yaw), yaw, false);
            for (int i = 0; i < 2; i++) ctx.Hiding.Locker(r, P.Side.North, 3f + i * .8f);
            F.Desk(p, r.Name, Props.Against(r, P.Side.North, 9f, .8f, out float dy), dy, false, 1.4f);
            F.Chair(p, r.Name, new Vector3(45f, Y, 10.4f), 0f);
            F.CoatRack(p, r.Name, new Vector3(47.4f, Y, 9.6f), ctx.Art.Fabric);
            ctx.AddLoot(L.Quarters, new Vector3(40f, Y, 6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(44f, Y, 7f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Bathroom: x 48-64, z 0-12; doors west (z 6) and north to the fitness room (x 54).
        private static void Bathroom(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Bathroom");
            foreach (float along in new[] { 2.5f, 4.2f, 5.9f, 7.6f })
            {
                Transform stall = p.Place(r.Name, "ShowerStall", Props.Against(r, P.Side.East, along, 1.3f, out float yaw), yaw, new Vector3(1.5f, 2.1f, 1.3f), false);
                if (stall == null) continue;
                Props.B(stall, "Partition", new Vector3(.73f, 1.05f, 0f), new Vector3(.04f, 2.1f, 1.3f), a.WallTile);
                Props.B(stall, "Head", new Vector3(0f, 2.05f, -.5f), new Vector3(.12f, .06f, .2f), a.Chrome);
            }
            foreach (float along in new[] { 9f, 11f }) F.Toilet(p, r.Name, Props.Against(r, P.Side.North, along, .7f, out float yaw), yaw);
            foreach (float along in new[] { 2f, 3f }) F.Sink(p, r.Name, Props.Against(r, P.Side.West, along, .45f, out float yaw), yaw);
            Transform wet = p.Hang(r.Name, "Puddle", new Vector3(60f, Y + .004f, 5f), 0f);
            Props.B(wet, "Water", Vector3.zero, new Vector3(2.4f, .004f, 3f), a.Wet);
        }

        // Training room: x 36-48, z 12-24; doors west (z 18) and east to the fitness room (z 18).
        private static void Training(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Training_Room");
            foreach (float z in new[] { 15f, 17f, 19f })
            foreach (float x in new[] { 38f, 40f, 44f, 46f })
                F.Chair(p, r.Name, new Vector3(x, Y, z), 0f, false);
            F.WallBoard(p, r.Name, new Vector3(42f, Y + 1.7f, 23.88f), 180f, 3.4f, 1.3f, a.White);
            Transform dummy = p.Place(r.Name, "CprDummy", new Vector3(42f, Y, 21.6f), 90f, new Vector3(.5f, .3f, 1.6f));
            if (dummy != null)
            {
                Props.B(dummy, "Torso", new Vector3(0f, .12f, .2f), new Vector3(.45f, .22f, .7f), a.Plain("DummySkin", new Color(.8f, .68f, .58f), .3f));
                Props.B(dummy, "Head", new Vector3(0f, .12f, .7f), new Vector3(.22f, .2f, .25f), a.Plain("DummySkin", new Color(.8f, .68f, .58f), .3f));
                Props.B(dummy, "Legs", new Vector3(0f, .08f, -.45f), new Vector3(.38f, .14f, .7f), a.AccentDark);
            }
            ctx.AddLoot(L.Watch, new Vector3(42f, Y, 13.2f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Watch, new Vector3(47f, Y, 22.6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Fitness room: x 48-64, z 12-24, weak; doors west (z 18) and south to the bathroom (x 54).
        private static void Fitness(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Fitness_Room");
            foreach (float along in new[] { 9f, 12f })
                F.Treadmill(p, r.Name, Props.Against(r, P.Side.South, along, 1.8f, out float yaw), yaw);
            F.WeightBench(p, r.Name, new Vector3(52f, Y, 20f), 90f);
            F.WeightBench(p, r.Name, new Vector3(60f, Y, 20f), 90f);
            F.PunchingBag(p, r.Name, new Vector3(56f, Y, 17f));
            Transform mats = p.Hang(r.Name, "Mats", new Vector3(55f, Y + .01f, 15f), 0f);
            Props.B(mats, "Mat", Vector3.zero, new Vector3(4f, .02f, 3f), ctx.Art.Rubber);
            foreach (Vector3 at in new[] { new Vector3(50f, Y, 14f), new Vector3(62f, Y, 22.6f), new Vector3(56f, Y, 22.6f) })
                ctx.AddLoot(L.Quarters, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(56f, Y, 20.4f));
        }

        // Radio room: x 40-48, z 24-32; doors north to the attic (x 42) and east to the chief (z 26).
        private static void Radio(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Radio_Room");
            F.Counter(p, r.Name, Props.Against(r, P.Side.West, 4f, .7f, out float yaw), yaw, 3f, .8f, a.DarkSteel);
            Transform sets = p.Hang(r.Name, "RadioSets", new Vector3(40.6f, Y + .8f, 28f), 90f);
            for (int i = 0; i < 3; i++) Props.B(sets, "Set", new Vector3(-1f + i, .18f, 0f), new Vector3(.8f, .36f, .4f), a.Rubber);
            F.Chair(p, r.Name, new Vector3(41.7f, Y, 28f), 270f);
            F.WallBoard(p, r.Name, new Vector3(44f, Y + 1.7f, 24.12f), 0f, 2.4f, 1.2f, a.Board);
            ctx.AddLoot(L.Watch, new Vector3(45.6f, Y, 29.6f), CarryClass.Pocket, CarryClass.TwoHand);
        }

        // Chief's quarters: x 48-64, z 24-32; doors west to the radio room (z 26), north to the attic (x 54). The wardrobe hides.
        private static void Chief(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Chief_Quarters");
            F.Bunk(p, r.Name, Props.Against(r, P.Side.North, 9f, 2.05f, out float yaw), yaw, false, a.AccentDark);
            F.Desk(p, r.Name, Props.Against(r, P.Side.East, 4f, .8f, out yaw), yaw, true, 1.4f);
            ctx.Hiding.Closet(r, P.Side.South, 12f);
            F.Bookcase(p, r.Name, Props.Against(r, P.Side.South, 4f, .38f, out yaw), yaw, 1.4f, 41);
            F.Sofa(p, r.Name, new Vector3(51f, Y, 29.6f), 90f, 1.1f, a.Leather);
            ctx.AddLoot(L.Quarters, new Vector3(55f, Y, 27f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Quarters, new Vector3(61f, Y, 30.4f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Attic storage: x 40-64, z 32-40; doors from the landing (z 34), the radio room (x 42) and the chief (x 54).
        private static void Attic(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Attic_Storage");
            int seed = 120;
            foreach (Vector3 at in new[] { new Vector3(45f, Y, 37.5f), new Vector3(49f, Y, 38.3f), new Vector3(58f, Y, 37f), new Vector3(62.6f, Y, 38f) })
                F.Crate(p, r.Name, at, (seed++ % 3) * 30f, new Vector3(1.1f, .8f, .9f));
            F.Sofa(p, r.Name, new Vector3(51f, Y, 34.6f), 0f, 2f, a.White);
            F.Bookcase(p, r.Name, Props.Against(r, P.Side.North, 13f, .38f, out float yaw), yaw, 1.2f, 57);
            F.Bunk(p, r.Name, new Vector3(60.5f, Y, 34f), 90f, true);
            F.FloorLitter(p, r.Name, new Vector3(55f, Y, 34.5f), 11);
            foreach (Vector3 at in new[] { new Vector3(44f, Y, 34.6f), new Vector3(53.6f, Y, 37.6f), new Vector3(57f, Y, 34.2f) })
                ctx.AddLoot(L.Watch, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(55.4f, Y, 36.6f));
        }

        // Under each weak floor: a tile hanging down, a cable, plaster dust on the floor. The floor above shows it first.
        private static void Warnings(StationContext ctx)
        {
            Props p = ctx.Props;
            const float ceiling = P.Story - P.Slab;
            F.CeilingDamage(p, "Apparatus_Bay", new Vector3(5.2f, 0f, 24f), ceiling, 2);
            F.CeilingDamage(p, "Apparatus_Bay", new Vector3(18.6f, 0f, 19.6f), ceiling, 3);
            F.CeilingDamage(p, "Apparatus_Bay", new Vector3(30.6f, 0f, 14f), ceiling, 5);
            F.CeilingDamage(p, "Workshop", new Vector3(14f, 0f, 34f), ceiling, 4);
            F.CeilingDamage(p, "SCBA_Room", new Vector3(24f, 0f, 33f), ceiling, 6);
            F.CeilingDamage(p, "Day_Room", new Vector3(57f, 0f, 15.4f), ceiling, 7);
        }
    }
}
