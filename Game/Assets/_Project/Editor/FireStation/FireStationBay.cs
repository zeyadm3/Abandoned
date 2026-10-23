using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.FireStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The apparatus bay (x 0-32, z 0-28): double height for its front 16 m, under the bunk rooms behind that. Engine 4 in
    /// lane 1, the haul lane (lane 2) empty and kept clear to its door, Ladder 4 (a 12 m aerial) in lane 3 and Engine 9 in
    /// lane 4 under the hose mezzanine (the mezzanine comes down on it). At the back: gear cages and a turnout rack on the
    /// west wall (cages to hide in), hose racks, SCBA cabinets, where the pole lands. The Jaws of Life (a jackpot) is here.
    /// </summary>
    public static class FireStationBay
    {
        public static void Dress(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Apparatus_Bay");
            p.KeepOut(0, new Rect(8f, 0f, 8f, 25.5f));
            Engine(p, a, r, "Engine4", 4f);
            Ladder(p, a, r);
            Engine(p, a, r, "Engine9", 28f);
            foreach (float along in new[] { 17.2f, 18.6f, 20f, 21.4f }) ctx.Hiding.Cage(r, P.Side.West, along);
            TurnoutRack(p, a, r, P.Side.West, 25.6f);
            TurnoutRack(p, a, r, P.Side.North, 18.5f);
            HoseRack(p, a, r, 10f);
            ctx.Hiding.Cabinet(r, P.Side.East, 17f);
            ctx.Hiding.Cabinet(r, P.Side.East, 26.5f);
            Markings(p, a);
            F.ExitSign(p, r.Name, new Vector3(L.HaulDoor.x, 4.85f, .26f), 0f);
            F.WallClock(p, r.Name, new Vector3(31.88f, 3.2f, 14f), 270f);
            F.Extinguisher(p, r.Name, new Vector3(.3f, 1.2f, 15f), 90f);
            F.Extinguisher(p, r.Name, new Vector3(31.7f, 1.2f, 19f), 270f);
            Transform reel = p.Hang(r.Name, "HoseReel", new Vector3(.3f, 1.4f, 13.2f), 90f);
            Props.C(reel, "Drum", new Vector3(0f, 0f, .2f), new Vector3(.7f, .12f, .7f), a.Red).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Props.C(reel, "Hose", new Vector3(0f, 0f, .2f), new Vector3(.62f, .16f, .62f), a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f)).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            F.FloorLitter(p, r.Name, new Vector3(13f, 0f, 22f), 3);

            ctx.AddLoot(L.Apparatus, new Vector3(24.6f, 0f, 18.5f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Apparatus, new Vector3(12f, 0f, 21f), CarryClass.Pocket, CarryClass.Heavy);
            foreach (Vector3 at in new[] { new Vector3(8.6f, 0f, 5f), new Vector3(15.4f, 0f, 9f), new Vector3(24f, 0f, 4f), new Vector3(14f, 0f, 18f),
                         new Vector3(6f, 0f, 20f), new Vector3(29f, 0f, 22.5f), new Vector3(24.2f, 0f, 13.5f) })
                ctx.AddLoot(L.Apparatus, at, CarryClass.Pocket, CarryClass.TwoHand);
            ctx.Threat(new Vector3(2.6f, 0f, 25.4f));
            ctx.Threat(new Vector3(26f, 0f, 21f));
        }

        // A pumper, cab to the street. One solid box; the detail is visual (StationApparatus).
        private static void Engine(Props p, StationArt a, P.Room r, string name, float x)
        {
            Transform t = p.Place(r.Name, name, new Vector3(x, 0f, 7.4f), 180f, new Vector3(2.5f, 3.2f, 9.6f));
            if (t == null) return;
            StationApparatus.Pumper(t, a, a.Red, Mathf.RoundToInt(x));
            // Chocks under the front wheels and a hose laid out of the side outlet, as it was left.
            foreach (float s in new[] { -1f, 1f })
                Props.B(t, "Chock", new Vector3(s * 1.04f, .12f, 3.95f), new Vector3(.3f, .24f, .25f), a.Safety);
        }

        // Ladder 4: a long chassis, the aerial ladder nested on top from its turntable at the back, outriggers stowed.
        private static void Ladder(Props p, StationArt a, P.Room r)
        {
            Transform t = p.Place(r.Name, "Ladder4", new Vector3(20f, 0f, 8.4f), 180f, new Vector3(2.55f, 3.5f, 12.4f));
            if (t == null) return;
            StationApparatus.LadderTruck(t, a, a.Red, 4);
        }

        private static void TurnoutRack(Props p, StationArt a, P.Room r, P.Side wall, float along)
        {
            Transform t = p.Place(r.Name, "TurnoutRack", Props.Against(r, wall, along, .7f, out float yaw), yaw, new Vector3(3.4f, 2f, .7f));
            if (t == null) return;
            Material coat = a.Plain("TurnoutCoat", new Color(.55f, .48f, .3f), .1f);
            Props.B(t, "Back", new Vector3(0f, 1f, -.32f), new Vector3(3.4f, 2f, .05f), a.DarkSteel);
            Props.B(t, "Shelf", new Vector3(0f, 1.75f, 0f), new Vector3(3.4f, .04f, .6f), a.Steel);
            for (int i = 0; i < 5; i++)
            {
                float x = -1.36f + i * .68f;
                Props.B(t, "Divider", new Vector3(x - .34f, 1f, 0f), new Vector3(.03f, 2f, .65f), a.Steel);
                if ((i + (int)along) % 4 == 3) continue;   // an empty hook: someone's gear went out on the last call
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

        private static void Markings(Props p, StationArt a)
        {
            Transform t = p.Hang("Apparatus_Bay", "LaneLines", Vector3.zero, 0f);
            foreach (float x in new[] { 8f, 16f, 24f })
                Props.B(t, "Lane", new Vector3(x, .003f, 13.5f), new Vector3(.12f, .004f, 26f), a.Safety);
            foreach (float x in L.BayDoors)
                Props.B(t, "StopLine", new Vector3(x, .003f, 1.2f), new Vector3(3.6f, .004f, .15f), a.Red);
            Props.B(t, "Drain", new Vector3(12f, .003f, 14f), new Vector3(.6f, .004f, 6f), a.DarkSteel);
            Props.B(t, "Oil", new Vector3(12f, .004f, 9f), new Vector3(2.2f, .004f, 3f), a.Oil);
            Props.B(t, "Oil", new Vector3(19.4f, .004f, 15.2f), new Vector3(1.6f, .004f, 1.2f), a.Oil);
        }
    }
}
