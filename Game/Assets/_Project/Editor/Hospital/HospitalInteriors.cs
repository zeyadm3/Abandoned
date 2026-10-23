using System.Collections.Generic;
using Abandoned.EditorTools;
using Abandoned.Interaction;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalInteriors
    {
        public static List<LootPointSpec> Build(Transform root, HospitalKit kit, HospitalStructure floors)
        {
            StationContext c = floors.Context;
            foreach (StationPlan.Room room in c.Plan.Rooms)
            {
                Rect r = room.Meters;
                Vector3 main = room.Centre;
                Vector3 small = new(r.xMax - 1.6f, room.Y, r.yMin + 1.6f);
                if (room.Name == "Ambulance")
                {
                    main = new Vector3(52, 0, 4);
                    small = new Vector3(38, 0, 5);
                }
                if (room.Name == "Triage")
                {
                    main = new Vector3(5, 0, 25.5f);
                    small = new Vector3(7, 0, 30.5f);
                }
                if (room.Name == "Isolation")
                {
                    main = new Vector3(10, 8, 26);
                    small = new Vector3(10, 8, 30);
                }
                Point(c, room.LootTag, main, room.Name == "Triage" ? CarryClass.TwoHand : room.LootTag == "hospital_ward" ? CarryClass.Huge : CarryClass.Heavy);
                Point(c, room.LootTag, small, CarryClass.TwoHand);
            }
            Point(c, "hospital_imaging_prize", new Vector3(9, 0, 45), CarryClass.Huge, true);
            Point(c, "hospital_theatre_prize", new Vector3(45, 4, 37), CarryClass.Huge, true);
            c.Props.KeepOut(0, new Rect(40, 0, 8, 8));
            HospitalGround.Dress(c);
            HospitalUpper.Dress(c);
            HospitalBasement.Dress(c);
            HospitalAtrium.Dress(c);
            // Each clinical level has threat approaches in both stair cores and its cross-gallery connection.
            foreach (int f in new []
            {
                0, 1
            })
            {
                foreach (float z in new []
                {
                    22f, 34f, 42f
                })
                {
                    c.Threats.Add(new Vector3(34, f * 4, z));
                }
            }
            foreach (int f in new []
            {
                2, 3
            })
            {
                c.Threats.Add(new Vector3(14, f * 4, 38));
                c.Threats.Add(new Vector3(34, f * 4, 38));
                c.Threats.Add(new Vector3(50, f * 4, 42));
            }
            c.Threats.AddRange(new []
            {
                new Vector3(18, - 4, 38), new Vector3(30, - 4, 38), new Vector3(2, - 4, 38)
            });
            floors.BindDressing();
            return c.Loot;
        }
        private static void Point(StationContext c, string tag, Vector3 at, CarryClass max, bool prize = false)
        {
            c.AddLoot(tag, at, CarryClass.Pocket, max, prize);
            float r = prize || max == CarryClass.Huge ? 1.3f : .65f;
            c.Props.KeepOut(Mathf.RoundToInt(at.y / 4f), new Rect(at.x - r, at.z - r, r * 2, r * 2));
        }
    }
}
