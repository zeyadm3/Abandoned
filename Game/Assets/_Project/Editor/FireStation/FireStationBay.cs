using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The apparatus bay (x 0-24, z 0-28, double height): Engine 4 parked in the west lane, nose to its door; the middle
    /// lane kept clear (the haul route to the truck); the antique hand pumper's roped-off corner (the jackpot) at the back
    /// of the east lane; turnout racks on the west wall, hose racks on the north wall, two SCBA cabinets to hide in, and
    /// the hose gallery up the bay stair.
    /// </summary>
    public static class FireStationBay
    {
        public static void Dress(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Apparatus_Bay");
            p.KeepOut(0, new Rect(8f, 0f, 8f, 27f));
            Engine(p, a, r);
            for (int i = 0; i < 3; i++) TurnoutRack(p, a, r, 15f + i * 3.6f);
            HoseRack(p, a, r, 10f);
            HoseRack(p, a, r, 18.4f);
            ctx.Hiding.Cabinet(r, P.Side.North, 22.5f);
            ctx.Hiding.Cabinet(r, P.Side.East, 4f);
            Pumper(p, a);
            Markings(p, a);
            Gallery(ctx);
            F.ExitSign(p, r.Name, new Vector3(12f, 4.85f, .26f), 0f);
            F.WallClock(p, r.Name, new Vector3(23.88f, 3.2f, 14f), 270f);

            ctx.AddLoot(L.Apparatus, new Vector3(17.5f, 0f, 22.5f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Apparatus, new Vector3(14f, 0f, 12f), CarryClass.Pocket, CarryClass.Heavy);
            foreach (Vector3 at in new[] { new Vector3(7f, 0f, 15f), new Vector3(10f, 0f, 6f), new Vector3(12f, 0f, 24.6f), new Vector3(17.5f, 0f, 8f), new Vector3(7f, 0f, 2.5f) })
                ctx.AddLoot(L.Apparatus, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(2f, 0f, 25.8f));
        }

        // Engine 4: a pumper with its ladder on the roof, cab to the south. One solid box; the detail is visual.
        private static void Engine(Props p, StationArt a, P.Room r)
        {
            Transform t = p.Place(r.Name, "Engine4", new Vector3(4f, 0f, 8.6f), 180f, new Vector3(2.5f, 3.2f, 9.6f));
            if (t == null) return;
            Props.B(t, "Body", new Vector3(0f, 1.35f, -1.2f), new Vector3(2.45f, 1.9f, 6.6f), a.Red);
            Props.B(t, "Cab", new Vector3(0f, 1.55f, 3.3f), new Vector3(2.45f, 2.3f, 2.6f), a.Red);
            Props.B(t, "Windscreen", new Vector3(0f, 2.05f, 4.61f), new Vector3(2.2f, .9f, .03f), a.Glass);
            foreach (float s in new[] { -1f, 1f })
            {
                Props.B(t, "CabWindow", new Vector3(s * 1.23f, 2.05f, 3.5f), new Vector3(.02f, .8f, 1.6f), a.Glass);
                Props.B(t, "PumpPanel", new Vector3(s * 1.24f, 1.6f, 1.4f), new Vector3(.03f, 1f, 1f), a.Chrome);
                for (int g = 0; g < 3; g++) Props.C(t, "Gauge", new Vector3(s * 1.26f, 1.8f, 1.1f + g * .3f), new Vector3(.12f, .01f, .12f), a.Brass)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                for (int d = 0; d < 3; d++)
                    Props.B(t, "Shutter", new Vector3(s * 1.235f, 1.45f, -3.6f + d * 1.5f), new Vector3(.02f, 1.3f, 1.35f), a.Steel);
                foreach (float z in new[] { 3f, -2.2f, -3.5f })
                    Props.C(t, "Tyre", new Vector3(s * 1.1f, .55f, z), new Vector3(1.1f, .18f, 1.1f), a.Rubber).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Props.B(t, "LadderRail", new Vector3(s * .5f, 2.4f, -1.2f), new Vector3(.08f, .1f, 7f), a.Steel);
            }
            for (int i = 0; i < 18; i++) Props.B(t, "Rung", new Vector3(0f, 2.4f, -4.5f + i * .38f), new Vector3(1f, .04f, .04f), a.Steel);
            Props.B(t, "Bumper", new Vector3(0f, .55f, 4.72f), new Vector3(2.5f, .3f, .2f), a.Chrome);
            Props.B(t, "Grille", new Vector3(0f, 1.15f, 4.61f), new Vector3(1.4f, .6f, .02f), a.Chrome);
            Props.B(t, "LightBar", new Vector3(0f, 2.75f, 3.7f), new Vector3(1.6f, .1f, .3f), a.WarningRed);
            Props.B(t, "HoseBed", new Vector3(0f, 2.4f, -3.9f), new Vector3(2f, .3f, 1.5f), a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f));
            Props.B(t, "RearStep", new Vector3(0f, .45f, -4.6f), new Vector3(2.4f, .1f, .4f), a.Steel);
        }

        private static void TurnoutRack(Props p, StationArt a, P.Room r, float along)
        {
            Transform t = p.Place(r.Name, "TurnoutRack", Props.Against(r, P.Side.West, along, .7f, out float yaw), yaw, new Vector3(3.4f, 2f, .7f));
            if (t == null) return;
            Material coat = a.Plain("TurnoutCoat", new Color(.55f, .48f, .3f), .1f);
            Props.B(t, "Back", new Vector3(0f, 1f, -.32f), new Vector3(3.4f, 2f, .05f), a.DarkSteel);
            Props.B(t, "Shelf", new Vector3(0f, 1.75f, 0f), new Vector3(3.4f, .04f, .6f), a.Steel);
            for (int i = 0; i < 5; i++)
            {
                float x = -1.36f + i * .68f;
                Props.B(t, "Divider", new Vector3(x - .34f, 1f, 0f), new Vector3(.03f, 2f, .65f), a.Steel);
                Props.B(t, "Coat", new Vector3(x, 1.15f, 0f), new Vector3(.5f, .8f, .3f), coat);
                Props.B(t, "Trousers", new Vector3(x, .3f, .05f), new Vector3(.45f, .55f, .3f), coat);
                Props.B(t, "Boots", new Vector3(x, .15f, .2f), new Vector3(.4f, .3f, .28f), a.Rubber);
                Props.B(t, "Helmet", new Vector3(x, 1.9f, 0f), new Vector3(.35f, .22f, .4f), i % 3 == 0 ? a.White : a.Rubber);
            }
        }

        private static void HoseRack(Props p, StationArt a, P.Room r, float along)
        {
            Transform t = p.Place(r.Name, "HoseRack", Props.Against(r, P.Side.North, along, .6f, out float yaw), yaw, new Vector3(2.4f, 2f, .6f));
            if (t == null) return;
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            foreach (float x in new[] { -1.15f, 1.15f }) Props.B(t, "Upright", new Vector3(x, 1f, 0f), new Vector3(.06f, 2f, .55f), a.Red);
            for (int s = 0; s < 3; s++)
            {
                float y = .35f + s * .62f;
                Props.B(t, "Bar", new Vector3(0f, y - .25f, 0f), new Vector3(2.3f, .04f, .5f), a.Steel);
                for (int i = 0; i < 4; i++)
                    Props.C(t, "Roll", new Vector3(-.85f + i * .56f, y, 0f), new Vector3(.48f, .07f, .48f), canvas).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        // The pumper's corner: brass posts and a rope, nothing solid (the jackpot spawns inside).
        private static void Pumper(Props p, StationArt a)
        {
            Transform t = p.Hang("Apparatus_Bay", "PumperDisplay", new Vector3(17.5f, 0f, 22.5f), 0f);
            Material rope = a.Plain("VelvetRope", new Color(.45f, .05f, .06f), .2f);
            Vector2[] corners = { new(-1.5f, -2.1f), new(1.5f, -2.1f), new(1.5f, 2.1f), new(-1.5f, 2.1f) };
            for (int i = 0; i < 4; i++)
            {
                Vector2 c = corners[i], n = corners[(i + 1) % 4];
                Props.C(t, "Post", new Vector3(c.x, .45f, c.y), new Vector3(.06f, .45f, .06f), a.Brass);
                if (i == 0) continue;   // the front stays open
                Vector2 m = (c + n) * .5f;
                bool alongX = Mathf.Abs(c.y - n.y) < .01f;
                Props.B(t, "Rope", new Vector3(m.x, .82f, m.y), alongX ? new Vector3(3f, .04f, .04f) : new Vector3(.04f, .04f, 4.2f), rope);
            }
            Props.B(t, "Plinth", new Vector3(0f, .015f, 0f), new Vector3(3.2f, .03f, 4.4f), a.DarkWood);
        }

        private static void Markings(Props p, StationArt a)
        {
            Transform t = p.Hang("Apparatus_Bay", "LaneLines", Vector3.zero, 0f);
            foreach (float x in new[] { 8f, 16f })
                Props.B(t, "Lane", new Vector3(x, .003f, 13.5f), new Vector3(.12f, .004f, 26f), a.Safety);
            foreach (float x in L.BayDoors)
                Props.B(t, "StopLine", new Vector3(x, .003f, 1.2f), new Vector3(3.6f, .004f, .15f), a.Red);
            Props.B(t, "Drain", new Vector3(12f, .003f, 14f), new Vector3(.6f, .004f, 6f), a.DarkSteel);
            Props.B(t, "Oil", new Vector3(12f, .004f, 9f), new Vector3(2.2f, .004f, 3f), a.Oil);
            Props.B(t, "Oil", new Vector3(19.4f, .004f, 5f), new Vector3(1.6f, .004f, 2.4f), a.Oil);
        }

        // The hose gallery over the bay: hose rolls hung on its rail, and two short guards beside the stair's head.
        private static void Gallery(StationContext ctx)
        {
            StationArt a = ctx.Art;
            Transform g = ctx.Props.Hang("Hose_Gallery", "HoseRolls", new Vector3(20f, P.Story, 24f), 0f);
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            for (int i = 0; i < 4; i++)
                Props.C(g, "Roll", new Vector3(-.12f, .75f, -3f + i * 1.9f), new Vector3(.55f, .08f, .55f), canvas).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Transform guards = Group("StairHeadGuards", ctx.Root);
            foreach (float x in new[] { 20.25f, 23.75f })
                Box("Guard", guards, new Vector3(x, P.Story + .55f, 20f), new Vector3(.5f, 1.1f, .06f), a.Steel);
        }
    }
}
