using Abandoned.EditorTools;
using UnityEngine;
using B = Abandoned.EditorTools.StationProps;
namespace Abandoned.Hospital
{
    public static class HospitalClinicalProps
    {
        public static void Bed(StationContext c, string room, Vector3 at, bool critical = false)
        {
            Transform t = c.Props.Place(room, critical ? "ICUBed" : "WardBed", at, 0f, new Vector3(1.2f, 1.3f, 2.4f));
            if (t == null)
            {
                return;
            }
            StationArt a = c.Art;
            B.B(t, "Chassis", new Vector3(0f, .45f, 0f), new Vector3(1.1f, .12f, 2.3f), a.Chrome);
            B.B(t, "Mattress", new Vector3(0f, .7f, 0f), new Vector3(1f, .25f, 2.1f), a.Mattress);
            B.B(t, "Blanket", new Vector3(0f, .84f, - .4f), new Vector3(1.01f, .04f, 1.15f), critical ? a.White : a.Fabric);
            B.B(t, "Pillow", new Vector3(0f, .88f, .75f), new Vector3(.7f, .12f, .36f), a.White);
            foreach (float x in new []
            {
                - .5f, .5f
            })
            {
                B.B(t, "SideRail", new Vector3(x, 1f, 0f), new Vector3(.04f, .06f, 1.5f), a.Chrome);
                foreach (float z in new []
                {
                    - .9f, .9f
                })
                {
                    B.B(t, "Leg", new Vector3(x, .24f, z), new Vector3(.06f, .48f, .06f), a.Steel);
                    B.C(t, "Caster", new Vector3(x, .08f, z), new Vector3(.14f, .05f, .14f), a.Rubber);
                }
            }
            B.B(t, "Headboard", new Vector3(0f, 1.05f, 1.12f), new Vector3(1.15f, .5f, .06f), a.Accent);
            if (! critical)
            {
                return;
            }
            B.B(t, "MonitorArm", new Vector3(.65f, 1.3f, .75f), new Vector3(.05f, 1f, .05f), a.Chrome);
            B.B(t, "BedsideMonitor", new Vector3(.65f, 1.85f, .75f), new Vector3(.55f, .35f, .3f), a.White);
            B.B(t, "MonitorFace", new Vector3(.65f, 1.85f, .91f), new Vector3(.45f, .25f, .01f), a.Glass);
        }
        public static void Curtain(StationContext c, string room, Vector3 at, float width)
        {
            Transform t = c.Props.Hang(room, "TriageCurtain", at, 0f);
            B.B(t, "CeilingTrack", new Vector3(0f, 3.45f, 0f), new Vector3(width, .06f, .06f), c.Art.Chrome);
            // Gathered cloth only covers the rear of a bay, leaving both ends open for carrying.
            for (int i = 0; i <12; i++)
            {
                B.B(t, "Pleat", new Vector3(- width * .5f + .06f + i * width / 12f, 2.1f, i % 2 * .035f), new Vector3(width / 12f, 2.4f, .025f), c.Art.Fabric);
            }
        }
        public static void Theatre(StationContext c, string room, Vector3 at)
        {
            Transform t = c.Props.Place(room, "BoltedOperatingTable", at, 0f, new Vector3(1.2f, 1.2f, 2.4f));
            if (t == null)
            {
                return;
            }
            B.B(t, "Pedestal", new Vector3(0f, .45f, 0f), new Vector3(.45f, .9f, .8f), c.Art.Chrome);
            B.B(t, "Table", new Vector3(0f, 1f, 0f), new Vector3(.9f, .18f, 2.2f), c.Art.Rubber);
            foreach (float z in new []
            {
                - .8f, 0f, .8f
            })
            {
                B.B(t, "PadJoint", new Vector3(0f, 1.1f, z), new Vector3(.88f, .02f, .04f), c.Art.White);
            }
            Transform boom = c.Props.Hang(room, "SurgicalLightBoom", at + Vector3.up * 3.3f, 0f);
            B.B(boom, "Arm", Vector3.zero, new Vector3(2.4f, .1f, .1f), c.Art.Chrome);
            foreach (float x in new []
            {
                - .9f, .9f
            })
            {
                B.C(boom, "Reflector", new Vector3(x, - .4f, 0f), new Vector3(.6f, .09f, .6f), c.Art.White);
                B.C(boom, "Lens", new Vector3(x, - .5f, 0f), new Vector3(.5f, .01f, .5f), c.Art.Lamp);
            }
        }
        public static void Scanner(StationContext c, string room, Vector3 at)
        {
            Transform t = c.Props.Place(room, "BoltedMRIMagnet", at, 0f, new Vector3(3.2f, 3f, 2f));
            if (t == null)
            {
                return;
            }
            for (int i = 0; i <18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                GameObject segment = B.B(t, "MagnetSegment", new Vector3(Mathf.Cos(angle) * 1.18f, 1.5f + Mathf.Sin(angle) * 1.18f, 0f), new Vector3(.45f, .45f, 1.8f), c.Art.White);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            }
            B.B(t, "ServicePlinth", new Vector3(0f, .14f, 0f), new Vector3(3f, .28f, 1.8f), c.Art.DarkSteel);
            Transform table = c.Props.Place(room, "ScannerPatientTable", at + Vector3.forward * 2.7f, 0f, new Vector3(.75f, .9f, 2.5f));
            if (table != null)
            {
                B.B(table, "Support", new Vector3(0f, .4f, 0f), new Vector3(.4f, .8f, 1.8f), c.Art.Chrome);
                B.B(table, "Pad", new Vector3(0f, .86f, 0f), new Vector3(.72f, .08f, 2.5f), c.Art.Rubber);
            }
        }
        public static void Headwall(StationContext c, string room, Vector3 at)
        {
            Transform t = c.Props.Hang(room, "MedicalHeadwall", at, 0f);
            B.B(t, "Panel", new Vector3(0f, 1.6f, 0f), new Vector3(1.8f, .35f, .1f), c.Art.Accent);
            for (int i = 0; i <5; i++)
            {
                B.B(t, "GasOutlet", new Vector3(- .6f + i * .3f, 1.6f, - .065f), new Vector3(.1f, .12f, .04f), i % 2 == 0 ? c.Art.Chrome : c.Art.Safety);
            }
        }
    }
}
