using Abandoned.EditorTools;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
namespace Abandoned.Hospital
{
    public static class HospitalUpper
    {
        public static void Dress(StationContext c)
        {
            foreach (StationPlan.Room r in c.Plan.Rooms)
            {
                if (r.Floor<1)
                {
                    continue;
                }
                if (r.LootTag == "hospital_ward")
                {
                    Ward(c, r);
                }
                else if (r.Name == "TheatreA" || r.Name == "TheatreB")
                {
                    Theatre(c, r);
                }
                else if (r.Name == "FamilyLounge" || r.Name == "StaffRest")
                {
                    Lounge(c, r);
                }
                else if (r.Name == "Rehabilitation")
                {
                    Rehab(c, r);
                }
                else
                {
                    Work(c, r);
                }
            }
        }
        private static void Ward(StationContext c, StationPlan.Room r)
        {
            Rect a = r.Meters;
            bool intensive = r.Name == "IntensiveCare" || r.Name == "Isolation";
            for (float x = a.xMin + 1.7f; x <a.xMax - 1f; x += 4.5f)
            {
                for (float z = a.yMin + 2f; z <a.yMax - 1f; z += 5f)
                {
                    HospitalClinicalProps.Bed(c, r.Name, new Vector3(x, r.Y, z), intensive);
                    HospitalClinicalProps.Headwall(c, r.Name, new Vector3(x, r.Y, z + 1.35f));
                }
            }
            F.Sink(c.Props, r.Name, new Vector3(a.xMax - .8f, r.Y, a.yMax - 1), 270f);
            c.Hiding.Closet(r, StationPlan.Side.North, 1.5f);
            if (r.Name != "Paediatrics")
            {
                return;
            }
            for (int i = 0; i <6; i++)
            {
                Transform art = c.Props.Hang(r.Name, "MosaicWall", new Vector3(a.xMin + .18f, r.Y + 1.7f, a.yMin + .7f + i * .8f), 90f);
                StationProps.B(art, "Colour", Vector3.zero, new Vector3(.65f, .65f, .04f), i % 2 == 0 ? c.Art.Safety : c.Art.Accent);
            }
        }
        private static void Theatre(StationContext c, StationPlan.Room r)
        {
            Rect a = r.Meters;
            HospitalClinicalProps.Theatre(c, r.Name, new Vector3(a.xMin + 2.8f, r.Y, a.yMin + 3));
            F.Sink(c.Props, r.Name, new Vector3(a.xMin + .9f, r.Y, a.yMin + 1.3f), 90f);
            F.Counter(c.Props, r.Name, new Vector3(a.xMax - 1.1f, r.Y, a.yMax - 1), 180f, 2f, .95f, c.Art.Chrome);
            c.Hiding.Cabinet(r, StationPlan.Side.West, a.height - 1.6f);
        }
        private static void Work(StationContext c, StationPlan.Room r)
        {
            Rect a = r.Meters;
            bool office = r.LootTag == "hospital_records";
            for (float x = a.xMin + 1.3f; x <a.xMax - 1f; x += 2.8f)
            {
                if (office)
                {
                    F.Desk(c.Props, r.Name, new Vector3(x, r.Y, a.yMax - 1), 180f, true, 1.4f);
                    F.Chair(c.Props, r.Name, new Vector3(x, r.Y, a.yMax - 2), 0f);
                }
                else
                {
                    F.Shelving(c.Props, r.Name, new Vector3(x, r.Y, a.yMax - .6f), 180f, 1.8f,(int) x, c.Art.White);
                }
            }
            if (a.height> 6)
            {
                c.Hiding.Cabinet(r, StationPlan.Side.West, 2f);
            }
            if (r.Name == "Laboratory")
            {
                F.Counter(c.Props, r.Name, new Vector3(a.xMin + .8f, r.Y, a.yMin + 3), 90f, 4f, .95f, c.Art.Chrome);
                F.Sink(c.Props, r.Name, new Vector3(a.xMax - 1, r.Y, a.yMax - 1), 270f);
            }
        }
        private static void Lounge(StationContext c, StationPlan.Room r)
        {
            Rect a = r.Meters;
            foreach (float z in new []
            {
                a.yMin + 1.3f, a.yMax - 1.3f
            })
            {
                F.Sofa(c.Props, r.Name, new Vector3(a.xMin + 1.1f, r.Y, z), 90f, 1.8f);
            }
            F.WaterCooler(c.Props, r.Name, new Vector3(a.xMax - .7f, r.Y, a.yMax - .8f), 270f);
        }
        private static void Rehab(StationContext c, StationPlan.Room r)
        {
            Rect a = r.Meters;
            Transform rails = c.Props.Place(r.Name, "ParallelTherapyBars", new Vector3(a.xMin + 1.8f, r.Y, a.yMin + 2.5f), 0f, new Vector3(1.6f, 1.1f, 2.8f), false);
            if (rails != null)
            {
                foreach (float x in new []
                {
                    - .65f, .65f
                })
                {
                    StationProps.B(rails, "Rail", new Vector3(x, 1f, 0), new Vector3(.06f, .06f, 2.8f), c.Art.Chrome);
                    foreach (float z in new []
                    {
                        - 1.2f, 1.2f
                    })
                    {
                        StationProps.B(rails, "Post", new Vector3(x, .5f, z), new Vector3(.06f, 1f, .06f), c.Art.Steel);
                    }
                }
            }
            F.Bench(c.Props, r.Name, new Vector3(a.xMin + 1, r.Y, a.yMax - 1), 0f, 2f);
            F.WallBoard(c.Props, r.Name, new Vector3(a.xMax - .18f, r.Y + 1.8f, a.yMin + 1), 270f, 1.3f, .8f, c.Art.Board);
        }
    }
}
