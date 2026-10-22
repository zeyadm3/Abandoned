using Abandoned.EditorTools;
using UnityEngine;
using B = Abandoned.EditorTools.StationProps;
using F = Abandoned.EditorTools.StationFurniture;
namespace Abandoned.Hospital
{
    public static class HospitalServiceProps
    {
        public static void MorgueBank(StationContext c, string room, Vector3 at)
        {
            Transform t = c.Props.Place(room, "MortuaryDrawerBank", at, 90f, new Vector3(3.3f, 2.5f, 1.2f));
            if (t == null)
            {
                return;
            }
            B.B(t, "InsulatedBank", new Vector3(0f, 1.25f, 0f), new Vector3(3.3f, 2.5f, 1.2f), c.Art.DarkSteel);
            for (int row = 0; row <3; row++)
            {
                for (int column = 0; column <3; column++)
                {
                    Vector3 p = new(- 1.1f + column * 1.1f, .45f + row * .8f, .61f);
                    B.B(t, "DrawerFace", p, new Vector3(1.02f, .72f, .04f), c.Art.Chrome);
                    B.B(t, "Latch", p + new Vector3(.32f, 0f, .04f), new Vector3(.1f, .2f, .06f), c.Art.Rubber);
                    B.B(t, "TagSlot", p + new Vector3(- .25f, .12f, .025f), new Vector3(.3f, .1f, .02f), c.Art.Paper);
                }
            }
        }
        public static void Boiler(StationContext c, string room, Vector3 at)
        {
            Transform t = c.Props.Place(room, "SteamBoiler", at, 0f, new Vector3(2.2f, 3f, 2f));
            if (t == null)
            {
                return;
            }
            B.C(t, "PressureDrum", new Vector3(0f, 1.3f, 0f), new Vector3(1.8f, 1.2f, 1.8f), c.Art.DarkSteel);
            B.B(t, "Burner", new Vector3(0f, .6f, - .95f), new Vector3(1f, .7f, .25f), c.Art.AccentDark);
            B.C(t, "Flue", new Vector3(0f, 3f, .25f), new Vector3(.3f, .6f, .3f), c.Art.Bars);
            foreach (float x in new []
            {
                - .5f, .5f
            })
            {
                B.B(t, "Gauge", new Vector3(x, 1.8f, - .91f), new Vector3(.3f, .3f, .04f), c.Art.White);
                B.B(t, "Needle", new Vector3(x + .04f, 1.8f, - .94f), new Vector3(.12f, .02f, .02f), c.Art.Red);
            }
        }
        public static void PharmacyCage(StationContext c, string room)
        {
            foreach (float x in new []
            {
                34.1f, 41.9f
            })
            {
                Transform t = c.Props.Place(room, "DispensaryCage", new Vector3(x, 0f, 20f), 0f, new Vector3(4.2f, 2.8f, .1f));
                if (t == null)
                {
                    continue;
                }
                B.B(t, "Mesh", new Vector3(0f, 1.4f, 0f), new Vector3(4.2f, 2.8f, .03f), c.Art.Grating);
                B.B(t, "Frame", new Vector3(0f, 2.75f, 0f), new Vector3(4.2f, .08f, .1f), c.Art.Chrome);
            }
            // A wide permanent opening keeps a heavy refrigerator extractable even after power fails.
        }
        public static void Altar(StationContext c, string room, Vector3 at)
        {
            F.Table(c.Props, room, at, 0f, 3f, 1f, c.Art.Trim);
            Transform cross = c.Props.Hang(room, "ChapelCross", at + new Vector3(0f, 2.3f, .6f), 0f);
            B.B(cross, "Stem", Vector3.zero, new Vector3(.14f, 1.4f, .09f), c.Art.Brass);
            B.B(cross, "Arms", new Vector3(0f, .25f, 0f), new Vector3(.9f, .12f, .09f), c.Art.Brass);
        }
        public static void CeilingServices(StationContext c)
        {
            foreach (int floor in new []
            {
                - 1, 0, 1
            })
            {
                float z = floor <0 ? 38f : 14f;
                Transform pipe = c.Props.Hang("Circulation", "ServiceConduit", new Vector3(floor <0 ? 16f : 28f, floor * 4f + 3.35f, z), 0f);
                B.B(pipe, "VentilationTrunk", Vector3.zero, new Vector3(floor <0 ? 26f : 50f, .25f, .5f), c.Art.Steel);
                for (int i = 0; i <12; i++)
                {
                    B.B(pipe, "DuctJoint", new Vector3(- 22f + i * 4f, 0f, 0f), new Vector3(.05f, .27f, .52f), c.Art.DarkSteel);
                }
            }
        }
    }
}
