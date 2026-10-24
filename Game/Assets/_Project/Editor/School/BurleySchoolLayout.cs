using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Master spatial coordinates, room boundaries, athletic fields, and road networks for
    /// Pine Hollow Middle School (fictional; a Blue Ridge town), matching the campus footprint in winter.
    /// </summary>
    public static class BurleySchoolLayout
    {
        public const string SceneName = "School";
        public const string ScenePath = "Assets/_Project/Scenes/School.unity";

        // ---- Grid & Dimensions ----
        public const float Tile = 4.0f;
        public const float StoryHeight = 4.0f;
        public const float SlabThickness = 0.25f;
        public const float WallThickness = 0.24f;

        // Overall Building Bounds (for interior atmosphere & horror volume)
        public static readonly Bounds Building = new(new Vector3(50f, 2.25f, 56f), new Vector3(101f, 12.5f, 73f));

        // ---- Wings Footprints (in World Coordinates) ----
        // West Wing: Auditorium, Stage, Backstage, Dressing Rooms
        public static readonly Rect WestWing = new(0f, 20f, 24f, 36f);

        // Center Academic Wing: 2 Stories (Ground & Upper)
        // Ground: Lobby, Main Administration, Attendance, Nurse Clinic, Classrooms 101 & 102, Restrooms
        // Upper: Science Labs 201 & 202, Classrooms 203 & 204, Teacher Lounge
        public static readonly Rect CenterWing = new(24f, 20f, 44f, 32f);

        // North Wing: Cafeteria & Kitchen (Ground), Library & Media Center (Upper)
        public static readonly Rect NorthWing = new(24f, 52f, 44f, 28f);

        // East Wing: Double-height Gymnasium, Bleachers, Basketball Court, Athletic Locker Rooms
        public static readonly Rect EastWing = new(68f, 20f, 32f, 48f);

        // Rear Service & Boiler Room (Basement / Utility)
        public static readonly Rect BoilerWing = new(44f, 76f, 24f, 16f);

        // Historic Brick Smokestack Position (behind Boiler Room)
        public static readonly Vector3 SmokestackPosition = new(56f, 0f, 92f);
        public const float SmokestackHeight = 22.0f;

        // ---- Exterior & Athletic Facilities ----
        // Front Entrance Drop-off Loop & Rose Hill Drive
        public static readonly Vector3 FrontEntrance = new(46f, 0f, 20f);
        public static readonly Vector3 TruckPosition = new(46f, 0f, -4f);
        public static readonly Vector3 TruckForward = Vector3.forward;

        // Northeast Parking Lot (Staff & Visitors)
        public static readonly Rect ParkingLot = new(102f, 46f, 28f, 42f);

        // Outdoor Basketball Courts (East of School)
        public static readonly Rect OutdoorBasketballCourt = new(104f, 12f, 28f, 24f);

        // Oval Running Track & Football / Soccer Field (oriented North-South parallel to Madison Ave). Authored round
        // z 14 and moved TrackShift north so the oval clears Rose Hill Drive; these bounds are where they stand now.
        public const float TrackShift = 38f;
        public static readonly Rect AthleticTrackBounds = new(136f, -36f + TrackShift, 80f, 104f);
        public static readonly Rect FootballFieldBounds = new(152f, -20f + TrackShift, 48f, 72f);

        // Baseball Diamond (North of Parking Lot towards Hollow Creek Park)
        public static readonly Vector3 BaseballHomePlate = new(118f, 0f, 108f);
        public const float BaseballBaseDistance = 27.4f; // 90 feet in metres

        // Hollow Creek Park (Rolling snowy parkland to the north/northeast)
        public static readonly Rect ParkBounds = new(80f, 136f, 150f, 80f);

        // ---- Surrounding Streets ----
        // Rose Hill Drive: across front (Z: -20 to -8)
        public const float RoseHillZ = -14f;
        public const float RoadWidth = 9.0f;

        // Madison Avenue: running along eastern edge past track (X: 236 to 246)
        public const float MadisonAveX = 240f;

        // Henry Avenue: running along northwest side (X: -24 to -12)
        public const float HenryAveX = -18f;

        // Concord Avenue: intersecting Rose Hill Dr opposite school (X: 22 to 32)
        public const float ConcordAveX = 26f;
    }
}
