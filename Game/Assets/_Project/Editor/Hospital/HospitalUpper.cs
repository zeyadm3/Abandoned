using Abandoned.EditorTools;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
namespace Abandoned.Hospital
{
    public static class HospitalUpper
    {
        public static void Dress(StationContext c)
        {
            Ward(c, "WardA", new []
            {
                3f, 12f
            }, new []
            {
                3f, 8.5f
            }, false);
            Ward(c, "WardB", new []
            {
                19f, 24f
            }, new []
            {
                3f, 8.5f
            }, false);
            Ward(c, "IntensiveCare", new []
            {
                7f, 13f
            }, new []
            {
                21f
            }, true);
            Ward(c, "Recovery", new []
            {
                47f, 53f
            }, new []
            {
                21f
            }, false);
            Ward(c, "Isolation", new []
            {
                3f, 12f
            }, new []
            {
                39f, 45f
            }, true);
            Ward(c, "Paediatrics", new []
            {
                19f, 24f
            }, new []
            {
                40f, 45f
            }, false);
            foreach (string name in new []
            {
                "TheatreA", "TheatreB"
            })
            {
                StationPlan.Room room = c.Plan.FindRoom(name);
                float x = room.Meters.xMin;
                HospitalClinicalProps.Theatre(c, name, new Vector3(x + 3f, 4f, 6f));
                F.Sink(c.Props, name, new Vector3(x + 1f, 4f, 1.5f), 90f);
                F.Counter(c.Props, name, new Vector3(x + 7f, 4f, 10.8f), 180f, 6f, .95f, c.Art.Chrome);
                c.Hiding.Cabinet(name, new Vector3(x + 1.3f, 4f, 9.5f), 90f);
            }
            Work(c, "Nurses", true);
            Work(c, "Sterile", false);
            Work(c, "Laboratory", false);
            Work(c, "Records", false);
            Work(c, "Consultation", true);
            Work(c, "Administration", true);
            Work(c, "EquipmentStore", false);
            F.Sofa(c.Props, "StaffRest", new Vector3(35f, 4f, 30f), 90f, 2f);
            F.Counter(c.Props, "StaffRest", new Vector3(41f, 4f, 31.2f), 180f, 3f, .9f, c.Art.White);
            F.WaterCooler(c.Props, "StaffRest", new Vector3(33f, 4f, 29f), 90f);
            // Children's furniture differs in silhouette without copying any loose toy's loot model.
            for (int i = 0; i <5; i++)
            {
                Transform mural = c.Props.Hang("Paediatrics", "PaintedWallTile", new Vector3(16.22f, 5.6f, 38.5f + i * 1.6f), 90f);
                StationProps.B(mural, "ColourTile", Vector3.zero, new Vector3(.7f, .7f, .02f), i % 2 == 0 ? c.Art.Safety : c.Art.Accent);
            }
        }
        private static void Ward(StationContext c, string name, float [] xs, float [] zs, bool critical)
        {
            foreach (float x in xs)
            {
                foreach (float z in zs)
                {
                    HospitalClinicalProps.Bed(c, name, new Vector3(x, 4f, z), critical);
                    HospitalClinicalProps.Headwall(c, name, new Vector3(x, 4f, z + 1.4f));
                    if (! critical)
                    {
                        HospitalClinicalProps.Curtain(c, name, new Vector3(x, 4f, z + 1.6f), 2.8f);
                    }
                }
            }
            StationPlan.Room room = c.Plan.FindRoom(name);
            c.Hiding.Closet(room, StationPlan.Side.North, 2.5f);
            F.Sink(c.Props, name, new Vector3(room.Meters.xMax - 1f, 4f, room.Meters.yMax - 1.2f), 270f);
        }
        private static void Work(StationContext c, string name, bool office)
        {
            StationPlan.Room room = c.Plan.FindRoom(name);
            Rect r = room.Meters;
            if (office)
            {
                for (float x = r.xMin + 2f; x <r.xMax - 2f; x += 3f)
                {
                    F.Desk(c.Props, name, new Vector3(x, 4f, r.yMax - 1.1f), 180f);
                    F.Chair(c.Props, name, new Vector3(x, 4f, r.yMax - 2.1f), 0f);
                }
            }
            else
            {
                for (float x = r.xMin + 2f; x <r.xMax - 1f; x += 2.5f)
                {
                    F.Shelving(c.Props, name, new Vector3(x, 4f, r.yMax - .6f), 180f, 2f,(int) x, name == "Records" ? c.Art.Paper : c.Art.White);
                }
            }
            if (r.height> 6f)
            {
                c.Hiding.Cabinet(room, StationPlan.Side.West, 3f);
                F.Table(c.Props, name, new Vector3(r.xMin + 3f, 4f, r.yMin + 4f), 0f, 2f, 1f, c.Art.Chrome);
            }
        }
    }
}
