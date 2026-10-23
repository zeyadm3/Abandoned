using Abandoned.EditorTools;
using P = Abandoned.EditorTools.StationPlan;
using F = Abandoned.EditorTools.StationPlan.Finish;
namespace Abandoned.Hospital
{
    public static class HospitalDepartments
    {
        public static void Ground(P p)
        {
            p.AddRoom("Pharmacy", 0, 0, 2, 2, 3, "hospital_pharmacy", F.Tile);
            p.AddRoom("Reception", 0, 2, 2, 3, 3, "hospital_reception", F.Panel);
            p.AddRoom("Triage", 0, 1, 6, 1, 2, "hospital_clinical", F.Tile);
            p.AddRoom("Emergency", 0, 2, 6, 2, 2, "hospital_clinical", F.Tile);
            p.AddRoom("Radiology", 0, 0, 9, 3, 3, "hospital_clinical", F.Tile, false);
            p.AddRoom("Supplies", 0, 3, 9, 2, 3, "hospital_pharmacy", F.Block);
            p.AddRoom("Ambulance", 0, 9, 0, 5, 2, "hospital_clinical", F.Concrete, false);
            p.AddRoom("Changing", 0, 9, 3, 2, 1, "hospital_ward");
            p.AddRoom("Cafeteria", 0, 11, 3, 3, 1, "hospital_kitchen", F.Panel);
            p.AddRoom("Chapel", 0, 9, 8, 2, 2, "hospital_chapel", F.Panel);
            p.AddRoom("Kitchen", 0, 11, 8, 2, 2, "hospital_kitchen", F.Tile);
            p.AddRoom("ServiceStore", 0, 9, 11, 5, 1, "hospital_plant", F.Block);
        }
        public static void Upper(P p)
        {
            p.AddRoom("WardA", 1, 0, 2, 3, 3, "hospital_ward");
            p.AddRoom("Nurses", 1, 3, 2, 2, 3, "hospital_records", F.Panel);
            p.AddRoom("IntensiveCare", 1, 1, 6, 2, 2, "hospital_ward", F.Tile);
            p.AddRoom("Consultation", 1, 4, 6, 1, 2, "hospital_clinical");
            p.AddRoom("Laboratory", 1, 0, 9, 3, 3, "hospital_theatre", F.Tile);
            p.AddRoom("Records", 1, 3, 9, 2, 3, "hospital_records", F.Block);
            p.AddRoom("Recovery", 1, 9, 0, 4, 2, "hospital_ward");
            p.AddRoom("TheatreA", 1, 9, 8, 3, 2, "hospital_theatre", F.Tile);
            p.AddRoom("Sterile", 1, 9, 11, 3, 1, "hospital_theatre", F.Tile);
            p.AddRoom("TheatreB", 1, 12, 10, 2, 2, "hospital_theatre", F.Tile);
            p.AddRoom("Isolation", 2, 1, 6, 2, 2, "hospital_ward", F.Tile);
            p.AddRoom("WardB", 2, 0, 10, 3, 2, "hospital_ward");
            p.AddRoom("FamilyLounge", 2, 4, 6, 1, 3, "hospital_reception", F.Panel);
            p.AddRoom("Rehabilitation", 2, 10, 7, 2, 2, "hospital_clinical", F.Panel);
            p.AddRoom("EquipmentStore", 2, 10, 10, 2, 2, "hospital_theatre", F.Block);
            p.AddRoom("Paediatrics", 3, 1, 10, 2, 2, "hospital_ward");
            p.AddRoom("StaffRest", 3, 4, 7, 1, 2, "hospital_reception", F.Panel);
            p.AddRoom("PatientSuite", 3, 2, 7, 1, 2, "hospital_ward");
            p.AddRoom("Administration", 3, 10, 10, 2, 2, "hospital_records", F.Panel);
            p.AddRoom("DayCare", 3, 10, 8, 2, 1, "hospital_ward");
        }
        public static void Basement(P p)
        {
            p.AddRoom("Morgue", - 1, 1, 6, 3, 3, "hospital_morgue", F.Tile, false);
            p.AddRoom("Boiler", - 1, 5, 6, 3, 3, "hospital_plant", F.Concrete, false);
            p.AddRoom("Autopsy", - 1, 1, 10, 3, 2, "hospital_morgue", F.Tile, false);
            p.AddRoom("BasementStores", - 1, 5, 10, 3, 2, "hospital_plant", F.Block, false);
        }
    }
}
