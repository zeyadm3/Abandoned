using Abandoned.EditorTools;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
namespace Abandoned.Hospital
{
    public static class HospitalGround
    {
        public static void Dress(StationContext c)
        {
            F.Counter(c.Props, "Reception", new Vector3(11, 0, 11), 0f, 4f, 1.05f, c.Art.Trim);
            F.Desk(c.Props, "Reception", new Vector3(10, 0, 17.5f), 180f);
            F.Chair(c.Props, "Reception", new Vector3(10, 0, 16.4f), 0f);
            foreach (float z in new []
            {
                11f, 17f
            })
            {
                F.Bench(c.Props, "Reception", new Vector3(18, 0, z), 90f, 2f);
            }
            F.WallClock(c.Props, "Reception", new Vector3(12, 2.7f, 8.2f), 0f);
            F.WaterCooler(c.Props, "Reception", new Vector3(9, 0, 9), 90f);
            for (int i = 0; i <3; i++)
            {
                F.Shelving(c.Props, "Pharmacy", new Vector3(1, 0, 10 + i * 3.5f), 90f, 2.2f, i, c.Art.White);
                F.Shelving(c.Props, "Pharmacy", new Vector3(7, 0, 10 + i * 3.5f), 270f, 2.2f, i + 7, c.Art.Glass);
            }
            Cage(c);
            HospitalClinicalProps.Bed(c, "Triage", new Vector3(6, 0, 28), true);
            HospitalClinicalProps.Headwall(c, "Triage", new Vector3(4.3f, 0, 28));
            foreach (float x in new []
            {
                9.6f, 14.4f
            })
            {
                foreach (float z in new []
                {
                    25.8f, 30.2f
                })
                {
                    HospitalClinicalProps.Bed(c, "Emergency", new Vector3(x, 0, z), true);
                    HospitalClinicalProps.Curtain(c, "Emergency", new Vector3(x, 0, z + 1.5f), 2.6f);
                }
            }
            HospitalClinicalProps.Scanner(c, "Radiology", new Vector3(3.5f, 0, 39));
            F.Desk(c.Props, "Radiology", new Vector3(1.4f, 0, 46), 90f);
            F.Chair(c.Props, "Radiology", new Vector3(2.5f, 0, 46), 270f);
            F.Shelving(c.Props, "Supplies", new Vector3(19, 0, 39), 270f, 3f, 12);
            F.Shelving(c.Props, "Supplies", new Vector3(19, 0, 45), 270f, 3f, 13);
            F.Lockers(c.Props, "Ambulance", new Vector3(54.7f, 0, 4), 270f, 4);
            F.Counter(c.Props, "Ambulance", new Vector3(37.1f, 0, 4), 90f, 4f, 1f, c.Art.Steel);
            for (int i = 0; i <3; i++)
            {
                c.Hiding.Locker("Changing", new Vector3(36.9f + i * 1.8f, 0, 15.1f), 180f);
            }
            foreach (float x in new []
            {
                46f, 52f
            })
            {
                F.Table(c.Props, "Cafeteria", new Vector3(x, 0, 14), 0f, 1.4f, .8f, c.Art.White);
                F.Chair(c.Props, "Cafeteria", new Vector3(x - 1.2f, 0, 14), 90f, false);
                F.Chair(c.Props, "Cafeteria", new Vector3(x + 1.2f, 0, 14), 270f, false);
            }
            foreach (float z in new []
            {
                33.5f, 38.5f
            })
            {
                F.Bench(c.Props, "Chapel", new Vector3(39, 0, z), 0f, 3f);
            }
            HospitalServiceProps.Altar(c, "Chapel", new Vector3(39, 0, 39));
            F.Counter(c.Props, "Kitchen", new Vector3(48, 0, 39), 180f, 5f, .95f, c.Art.Chrome);
            F.Stove(c.Props, "Kitchen", new Vector3(45, 0, 33.5f), 90f);
            F.Sink(c.Props, "Kitchen", new Vector3(51, 0, 38), 270f);
            F.Fridge(c.Props, "Kitchen", new Vector3(51, 0, 33.5f), 270f);
            for (int i = 0; i <5; i++)
            {
                F.Shelving(c.Props, "ServiceStore", new Vector3(38 + i * 3.5f, 0, 47.4f), 180f, 2f, i + 50);
            }
            c.Hiding.Cabinet("Reception", new Vector3(18.6f, 0, 17.8f), 270f);
            c.Hiding.Cabinet("Radiology", new Vector3(10.6f, 0, 37.5f), 270f);
            c.Hiding.Closet("Chapel", new Vector3(42.7f, 0, 38.5f), 270f);
        }
        private static void Cage(StationContext c)
        {
            foreach (float x in new []
            {
                1.1f, 6.9f
            })
            {
                Transform t = c.Props.Place("Pharmacy", "DispensaryScreen", new Vector3(x, 0, 12), 0f, new Vector3(2.1f, 3f, .05f));
                if (t == null)
                {
                    continue;
                }
                StationProps.B(t, "Mesh", new Vector3(0, 1.5f, 0), new Vector3(2.1f, 3f, .05f), c.Art.Grating);
                StationProps.B(t, "Cap", new Vector3(0, 3f, 0), new Vector3(2.1f, .08f, .12f), c.Art.Chrome);
            }
        }
    }
}
