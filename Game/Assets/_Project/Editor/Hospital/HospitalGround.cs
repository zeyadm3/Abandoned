using Abandoned.EditorTools;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using B = Abandoned.EditorTools.StationProps;
namespace Abandoned.Hospital
{
    public static class HospitalGround
    {
        public static void Dress(StationContext c)
        {
            StationProps p = c.Props;
            F.Counter(p, "Reception", new Vector3(4f, 0f, 8f), 90f, 5f);
            F.Desk(p, "Reception", new Vector3(3f, 0f, 10.5f), 180f);
            F.Chair(p, "Reception", new Vector3(3f, 0f, 9.5f), 0f);
            for (int i = 0; i <3; i++)
            {
                F.Bench(p, "Reception", new Vector3(12f, 0f, 3f + i * 3f), 90f, 2f);
            }
            F.WaterCooler(p, "Reception", new Vector3(1f, 0f, 2f), 90f);
            F.WallBoard(p, "Reception", new Vector3(.22f, 1.8f, 5f), 90f, 2f, 1f, c.Art.Board);
            F.WallClock(p, "Reception", new Vector3(6f, 2.7f, .22f), 0f);
            foreach (float x in new []
            {
                18.7f, 24.5f
            })
            {
                HospitalClinicalProps.Bed(c, "Triage", new Vector3(x, 0f, 7.5f), true);
                HospitalClinicalProps.Curtain(c, "Triage", new Vector3(x, 0f, 9.2f), 3.5f);
                HospitalClinicalProps.Headwall(c, "Triage", new Vector3(x, 0f, 11.7f));
            }
            F.Counter(p, "Triage", new Vector3(18f, 0f, 2.3f), 90f, 2f, 1f, c.Art.White);
            foreach (float x in new []
            {
                6.8f, 12.5f
            })
            {
                foreach (float z in new []
                {
                    19.5f, 28f
                })
                {
                    HospitalClinicalProps.Bed(c, "Emergency", new Vector3(x, 0f, z), true);
                    HospitalClinicalProps.Curtain(c, "Emergency", new Vector3(x, 0f, z + 1.6f), 3.3f);
                }
            }
            HospitalClinicalProps.Scanner(c, "Radiology", new Vector3(19.2f, 0f, 21f));
            F.Desk(p, "Radiology", new Vector3(24f, 0f, 18f), 0f);
            F.Chair(p, "Radiology", new Vector3(24f, 0f, 19f), 180f);
            F.FilingCabinet(p, "Radiology", new Vector3(26.9f, 0f, 28.8f), 270f);
            HospitalServiceProps.PharmacyCage(c, "Pharmacy");
            foreach (float z in new []
            {
                23f, 26f, 29f
            })
            {
                F.Shelving(p, "Pharmacy", new Vector3(33f, 0f, z), 90f, 2.2f,(int) z, c.Art.White);
                F.Shelving(p, "Pharmacy", new Vector3(43f, 0f, z), 270f, 2.2f,(int) z + 3, c.Art.Glass);
            }
            F.Counter(p, "Pharmacy", new Vector3(38f, 0f, 30.8f), 180f, 5f, 1f, c.Art.White);
            foreach (float z in new []
            {
                18.5f, 22f, 27f, 30f
            })
            {
                F.Shelving(p, "Supplies", new Vector3(45f, 0f, z), 90f, 2f,(int) z, c.Art.Paper);
            }
            F.Lockers(p, "Ambulance", new Vector3(54.8f, 0f, 7f), 270f, 6);
            F.Counter(p, "Ambulance", new Vector3(34f, 0f, 5f), 90f, 4f, 1f, c.Art.Steel);
            // The actual loading path is reserved before any prop is placed.
            Transform roll = p.Hang("Ambulance", "OpenRollerDoor", new Vector3(44f, 3.5f, .25f), 0f);
            B.B(roll, "Drum", Vector3.zero, new Vector3(6.4f, .5f, .5f), c.Art.DarkSteel);
            foreach (float x in new []
            {
                40.7f, 47.3f
            })
            {
                B.B(roll, "Guide", new Vector3(x - 44f, - 1.8f, 0f), new Vector3(.1f, 3.4f, .12f), c.Art.Safety);
            }
            Social(c);
        }
        private static void Social(StationContext c)
        {
            StationProps p = c.Props;
            foreach (float z in new []
            {
                39f, 42f, 45f
            })
            {
                foreach (float x in new []
                {
                    4f, 12f
                })
                {
                    F.Bench(p, "Chapel", new Vector3(x, 0f, z), 0f, 4f);
                }
            }
            HospitalServiceProps.Altar(c, "Chapel", new Vector3(8f, 0f, 46.4f));
            F.Shelving(p, "Chapel", new Vector3(1f, 0f, 45f), 90f, 2f, 13, c.Art.Leather);
            foreach (float x in new []
            {
                19f, 25f
            })
            {
                foreach (float z in new []
                {
                    40f, 45f
                })
                {
                    F.Table(p, "Cafeteria", new Vector3(x, 0f, z), 0f, 1.5f, .9f, c.Art.White);
                    F.Chair(p, "Cafeteria", new Vector3(x, 0f, z - 1f), 0f, false);
                    F.Chair(p, "Cafeteria", new Vector3(x, 0f, z + 1f), 180f, false);
                }
            }
            F.Vending(p, "Cafeteria", new Vector3(17f, 0f, 46f), 90f);
            F.Counter(p, "Kitchen", new Vector3(38f, 0f, 46.8f), 180f, 8f, .92f, c.Art.Chrome);
            F.Stove(p, "Kitchen", new Vector3(33f, 0f, 39f), 90f);
            F.Fridge(p, "Kitchen", new Vector3(33f, 0f, 41f), 90f);
            F.Sink(p, "Kitchen", new Vector3(42.8f, 0f, 45f), 270f);
            F.Shelving(p, "Kitchen", new Vector3(42.8f, 0f, 39f), 270f, 2f, 55, c.Art.Porcelain);
            for (int i = 0; i <5; i++)
            {
                c.Hiding.Locker("Changing", new Vector3(45.3f, 0f, 38f + i * 1.7f), 90f);
            }
            F.Bench(p, "Changing", new Vector3(48f, 0f, 44f), 90f, 3f);
            F.Sink(p, "Changing", new Vector3(54.8f, 0f, 45f), 270f);
        }
    }
}
