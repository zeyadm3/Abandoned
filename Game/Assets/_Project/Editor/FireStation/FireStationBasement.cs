using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's basement (y -4, under the kitchen and the laundry), down the basement stair from the ground hall:
    /// the foam store, the utility room with the breaker bank, the boiler and the standby generator, the old store where
    /// the antique hand pumper waits under the kitchen's weak floor (a heavy carry up one flight), and the archive.
    /// </summary>
    public static class FireStationBasement
    {
        private const float Y = -P.Story;

        public static void Dress(StationContext ctx)
        {
            Foam(ctx);
            Utility(ctx);
            OldStore(ctx);
            Archive(ctx);
            Transform pipes = ctx.Props.Hang("Basement_Hall", "Pipes", new Vector3(50f, Y + 3.45f, 35.4f), 0f);
            Props.C(pipes, "Pipe_A", Vector3.zero, new Vector3(.18f, 14f, .18f), ctx.Art.Steel).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Props.C(pipes, "Pipe_B", new Vector3(0f, -.25f, -.3f), new Vector3(.12f, 14f, .12f), ctx.Art.Bars).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        // Foam store: x 36-40, z 24-32; door north (x 38).
        private static void Foam(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Foam_Store");
            Material yellow = ctx.Art.Safety;
            foreach (Vector3 at in new[] { new Vector3(37f, Y, 25f), new Vector3(38.3f, Y, 25f), new Vector3(39.4f, Y, 25.2f), new Vector3(37f, Y, 26.4f), new Vector3(37f, Y, 28.6f) })
                F.Drum(p, r.Name, at, yellow);
            ctx.AddLoot(L.Apparatus, new Vector3(38.8f, Y, 27.6f), CarryClass.Pocket, CarryClass.Heavy);
        }

        // Utility room: x 44-56, z 20-32; door north (x 50). The breakers line the south wall.
        private static void Utility(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Utility_Room");
            ctx.Breakers = StationLighting.Breakers(ctx.Root, a, new Vector3(45.5f, Y, 20.12f), 0f, 1f);
            Transform boiler = p.Place(r.Name, "Boiler", new Vector3(54f, Y, 23.6f), 0f, new Vector3(1.8f, 2.4f, 1.8f));
            if (boiler != null)
            {
                Props.C(boiler, "Drum", new Vector3(0f, 1.1f, 0f), new Vector3(1.6f, 1f, 1.6f), a.DarkSteel);
                Props.B(boiler, "Base", new Vector3(0f, .08f, 0f), new Vector3(1.8f, .16f, 1.8f), a.Concrete);
                Props.C(boiler, "Flue", new Vector3(0f, 3f, 0f), new Vector3(.4f, .9f, .4f), a.Steel);
                Props.B(boiler, "Gauge", new Vector3(0f, 1.5f, .82f), new Vector3(.18f, .18f, .04f), a.White);
            }
            Transform gen = p.Place(r.Name, "Generator", new Vector3(47.5f, Y, 28.4f), 0f, new Vector3(2.6f, 1.8f, 1.4f));
            if (gen != null)
            {
                Props.B(gen, "Body", new Vector3(0f, .85f, 0f), new Vector3(2.4f, 1.5f, 1.3f), a.Red);
                Props.B(gen, "Skid", new Vector3(0f, .08f, 0f), new Vector3(2.6f, .16f, 1.4f), a.DarkSteel);
                Props.B(gen, "Panel", new Vector3(-.9f, 1f, .66f), new Vector3(.5f, .4f, .02f), a.Rubber);
                Props.C(gen, "Exhaust", new Vector3(.8f, 2.6f, 0f), new Vector3(.2f, .9f, .2f), a.Steel);
            }
            F.Workbench(p, r.Name, Props.Against(r, P.Side.East, 8.5f, .8f, out float yaw), yaw, 2f);
            ctx.Threat(new Vector3(52.4f, Y, 27.4f));
        }

        // Old store: x 56-64, z 20-32, under the kitchen's weak end; wide door north (x 58).
        private static void OldStore(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Old_Store");
            F.Shelving(p, r.Name, Props.Against(r, P.Side.West, 4f, .5f, out float yaw), yaw, 2.4f, 131, a.Leather);
            F.Crate(p, r.Name, new Vector3(62.6f, Y, 30.6f), 0f, new Vector3(1.1f, .9f, .9f));
            F.Crate(p, r.Name, new Vector3(57.4f, Y, 21f), 20f, new Vector3(1f, .7f, .8f));
            F.WallBoard(p, r.Name, new Vector3(63.88f, Y + 1.8f, 25.5f), 270f, 2f, .7f, a.Red);
            F.CeilingDamage(p, r.Name, new Vector3(61f, Y, 28.6f), P.Story - P.Slab, 12);
            ctx.AddLoot(L.Apparatus, new Vector3(60.4f, Y, 25.5f), CarryClass.Pocket, CarryClass.Huge);
            ctx.AddLoot(L.Apparatus, new Vector3(62.6f, Y, 22.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Watch, new Vector3(58.4f, Y, 28.6f), CarryClass.Pocket, CarryClass.OneHand);
        }

        // Archive: x 36-64, z 36-40; wide door south (x 50). Shelving along the north wall.
        private static void Archive(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Archive");
            for (int i = 0; i < 9; i++)
                F.Shelving(p, r.Name, Props.Against(r, P.Side.North, 2.5f + i * 3f, .5f, out float yaw), yaw, 2.4f, 140 + i, i % 3 == 0 ? ctx.Art.Paper : null);
            F.FloorLitter(p, r.Name, new Vector3(53f, Y, 37.4f), 13);
            foreach (float x in new[] { 40f, 46f, 55f, 61f })
                ctx.AddLoot(L.Watch, new Vector3(x, Y, 37.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(44f, Y, 37.6f));
        }
    }
}
