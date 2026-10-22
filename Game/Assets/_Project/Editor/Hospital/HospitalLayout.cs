using UnityEngine;

namespace Abandoned.Hospital
{
    public static class HospitalLayout
    {
        public const string ScenePath = "Assets/_Project/Scenes/Hospital.unity";
        public const float Tile=4f, Storey=4f, Width=48f, Depth=40f;
        public static readonly Bounds Bounds = new(new Vector3(24,4,20),new Vector3(58,8,42));
        public static readonly string[,] Rooms = {
            {"RECEPTION", "PHARMACY", "EMERGENCY", "IMAGING"},
            {"WARD A", "WARD B", "SURGERY", "LAB AND RECORDS"}
        };
        public static readonly Vector3[] Crew = { new(20,0,-18),new(22,0,-18),new(26,0,-18),new(28,0,-18) };
        public static bool Protected(int x,int z) => x>=5 && x<=6 || z>=4 && z<=5;
        public static string Tag(int floor,int side,int bay) => floor==0
            ? (side==0 ? (bay==0 ? "hospital_reception" : "hospital_pharmacy") : "hospital_clinical")
            : (side==0 ? "hospital_ward" : (bay==0 ? "hospital_theatre" : "hospital_records"));
    }
}
