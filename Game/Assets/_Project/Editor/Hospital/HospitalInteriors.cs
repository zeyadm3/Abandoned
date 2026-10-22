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
            // Reserve real loot and turning space first. Furniture cannot silently claim those bays later.
            foreach (StationPlan.Room room in c.Plan.Rooms)
            {
                Rect r = room.Meters;
                Point(c, room.LootTag, room.Centre, CarryClass.Pocket, room.LootTag == "hospital_ward" ? CarryClass.Huge : CarryClass.Heavy);
                Point(c, room.LootTag, new Vector3(r.xMax - 1.8f, room.Y, r.yMin + 1.8f), CarryClass.Pocket, CarryClass.TwoHand);
            }
            Point(c, "hospital_imaging_prize", new Vector3(25f, 0f, 28f), CarryClass.Huge, CarryClass.Huge, true);
            Point(c, "hospital_theatre_prize", new Vector3(40f, 4f, 7f), CarryClass.Huge, CarryClass.Huge, true);
            c.Props.KeepOut(0, new Rect(40f, 0f, 8f, 12f));
            HospitalGround.Dress(c);
            HospitalUpper.Dress(c);
            HospitalBasement.Dress(c);
            HospitalServiceProps.CeilingServices(c);
            c.Threats.AddRange(new []
            {
                new Vector3(30f, 0f, 28f), new Vector3(30f, 0f, 42f), new Vector3(54f, 0f, 18f), new Vector3(18f, 0f, 34f), new Vector3(42f, 0f, 34f), new Vector3(2f, 0f, 26f), new Vector3(18f, 4f, 26f), new Vector3(42f, 4f, 26f), new Vector3(30f, 4f, 42f), new Vector3(54f, 4f, 34f), new Vector3(10f, 4f, 34f), new Vector3(30f, 4f, 10f), new Vector3(18f, - 4f, 38f), new Vector3(30f, - 4f, 38f), new Vector3(2f, - 4f, 38f)
            });
            floors.BindDressing();
            return c.Loot;
        }
        private static void Point(StationContext c, string tag, Vector3 at, CarryClass min, CarryClass max, bool prize = false)
        {
            c.AddLoot(tag, at, min, max, prize);
            float radius = prize || max == CarryClass.Huge ? 1.3f : .7f;
            c.Props.KeepOut(Mathf.RoundToInt(at.y / 4f), new Rect(at.x - radius, at.z - radius, radius * 2f, radius * 2f));
        }
    }
}
