using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The training house at 14 Larch Court, a 1970s ranch in a Michigan suburb, from the user's floor plan
    /// (2026-10-20). Redone 2026-10-21 at about 1.6x the first build because it was too tight ("you get stuck
    /// everywhere"): every doorway is 1.8 m (the NavMesh agent's 0.5 m radius leaves 0.8 m to walk through; the old
    /// 1 m doors were closed to it, which is why the floor arrows cut through walls), the hallway is 2.4 m, the
    /// ceilings 3 m, and nothing hinged stands out into a room. Same rooms and the same order as the plan: two
    /// bathrooms (the hall bath between the two small bedrooms, the ensuite only through the master), the open
    /// kitchen / dining / living band, the laundry alcove, the patio with the cellar stairs, a basement under the west
    /// wing. Metres; +X east, +Z north; the house floor is y = 0 and the yard 12 cm lower. Rects are
    /// (x min, z min, width along x, depth along z).
    /// </summary>
    public static class TutorialHouseLayout
    {
        public const string SceneName = "Tutorial";
        public const string ScenePath = "Assets/_Project/Scenes/Tutorial.unity";

        // ---- Heights and widths ----
        public const float Floor = 0f;
        public const float Grade = -0.12f;
        public const float Ceiling = 3f;
        public const float Slab = 0.25f;
        public const float BasementFloor = -2.4f;
        public const float ExteriorWall = 0.26f;
        public const float InteriorWall = 0.14f;
        public const float DoorHeight = 2.3f;

        // ---- West wing: the south bedroom, the hall bath and hallway, the north bedroom ----
        public static readonly Rect WestWing = new(0f, 0f, 8f, 17.5f);
        public static readonly Rect BedroomB = new(0f, 0f, 8f, 6.5f);              // south
        // The floor that gives way: the middle and west of the south bedroom, the bed and nightstand on it. The door
        // strip (north) and the closet side (east) hold.
        public static readonly Rect BedroomBPatch = Rect.MinMaxRect(.2f, .2f, 5.4f, 5.2f);
        public static readonly Rect HallBath = new(0f, 6.5f, 5.6f, 4.5f);
        public static readonly Rect Hallway = new(5.6f, 6.5f, 2.4f, 4.5f);
        public static readonly Rect BedroomA = new(0f, 11f, 8f, 6.5f);             // north
        // Reach-in closets against the bedrooms' east walls (faces west), and along the master's north wall.
        public static readonly Rect ClosetA = new(6.9f, 13.6f, .96f, 3f);
        public static readonly Rect ClosetB = new(6.9f, .6f, .96f, 3f);

        // ---- The open band: kitchen, dining, living ----
        public const float BandSouth = 6.5f, BandNorth = 14f, WingEast = 8f, HouseEast = 26.5f;
        public static readonly Rect Band = Rect.MinMaxRect(WingEast, BandSouth, HouseEast, BandNorth);
        public static readonly Rect Kitchen = Rect.MinMaxRect(8f, BandSouth, 14f, BandNorth);
        public static readonly Rect Dining = Rect.MinMaxRect(14f, BandSouth, 19.5f, BandNorth);
        public static readonly Rect Living = Rect.MinMaxRect(19.5f, BandSouth, HouseEast, BandNorth);

        // ---- The suite: laundry alcove off the dining room, the ensuite, the master ----
        public static readonly Rect Suite = Rect.MinMaxRect(14f, 0f, HouseEast, BandSouth);
        public static readonly Rect Laundry = Rect.MinMaxRect(14f, 4.5f, 17.5f, BandSouth);
        public static readonly Rect Ensuite = Rect.MinMaxRect(14f, 0f, 17.5f, 4.5f);
        public static readonly Rect Master = Rect.MinMaxRect(17.5f, 0f, HouseEast, BandSouth);
        public static readonly Rect MasterCloset = Rect.MinMaxRect(22.6f, 5.43f, 26.2f, 6.39f);

        // ---- Outside ----
        public static readonly Rect Patio = Rect.MinMaxRect(WingEast, 0f, 14f, BandSouth);
        public static readonly Rect Porch = Rect.MinMaxRect(HouseEast, 8f, 30f, 12.5f);
        public static readonly Rect Driveway = Rect.MinMaxRect(29f, 6f, 62f, 14.5f);
        public const float StreetX = 62f, StreetWidth = 8f;
        /// <summary>The yard the house stands in: the ground here is in the NavMesh (the guide's arrows walk it).</summary>
        public static readonly Rect Lot = Rect.MinMaxRect(-15f, -15f, 60f, 33f);

        // ---- The basement (west wing, plus a strip under the patio and kitchen for the cellar stairs) ----
        public static readonly Rect BasementMain = WestWing;
        public static readonly Rect BasementStrip = Rect.MinMaxRect(WingEast, 0f, 10.5f, 12f);
        // The bulkhead: an outside stair in the patio, top at the south end, going down northward toward the house.
        public static readonly Rect Bulkhead = Rect.MinMaxRect(8.8f, .6f, 10.4f, 5.4f);

        // ---- Openings: (from, to) along the wall ----
        public const float HallwayOpeningZ0 = 6.8f, HallwayOpeningZ1 = 9.2f;    // hallway into the kitchen (x = 8)
        public const float FrontDoorZ = 10.25f, FrontDoorWidth = 1.8f;          // living room's east wall (x = 26.5)
        // The patio slider, well east of the cellar stairs; it opens on its east half (the fixed pane is on the west).
        public const float SliderX0 = 10.6f, SliderX1 = 13.6f;                  // kitchen's south wall (z = 6.5)
        public const float LaundryDoorX0 = 14.3f, LaundryDoorX1 = 17.2f;        // bypass doors (z = 6.5)
        public const float MasterDoorX0 = 19.9f, MasterDoorX1 = 21.7f;          // master off the living room (z = 6.5)
        public const float EnsuiteDoorZ0 = 1.4f, EnsuiteDoorZ1 = 3.2f;          // ensuite off the master (x = 17.5)
        public const float BathDoorZ0 = 8.4f, BathDoorZ1 = 10.2f;               // hall bath off the hallway (x = 5.6)
        public const float BedroomDoorX0 = 5.9f, BedroomDoorX1 = 7.7f;          // both bedrooms off the hallway's ends

        // ---- The truck and the crew ----
        // The truck's ramp foot lands just past the porch's ramp (TruckBuilder: a 4.4 m ramp off the back).
        public static readonly Vector3 TruckRear = new(35.2f, Grade, FrontDoorZ);
        public static readonly Vector3 TruckForward = Vector3.right;
        // On the front lawn by the street, facing the house: the walk up is the first lesson.
        public static readonly Vector3[] CrewSpawns =
        {
            new(52f, Grade, 16.6f), new(53.1f, Grade, 16.6f), new(54.2f, Grade, 16.6f), new(55.3f, Grade, 16.6f),
        };
        public static readonly Vector3 VanPosition = new(64.6f, Grade, 18.5f);
        // The kit crates beside the truck's ramp (hand trolley, medkit).
        public static readonly Vector3 TrolleyCrate = new(31.6f, Grade, 12.9f);
        public static readonly Vector3 MedkitCrate = new(32.8f, Grade, 12.9f);

        // ---- The house's bounding box (atmosphere, building bounds) ----
        public static readonly Bounds Building = new(new Vector3(13.25f, .7f, 8.75f), new Vector3(27.2f, 7.6f, 18.2f));

        public static Vector3 Centre(Rect r, float y = Floor) => new(r.center.x, y, r.center.y);

        /// <summary>A box over a rect: centre and size, from height y0 to y1.</summary>
        public static Bounds Box(Rect r, float y0, float y1) => new(new Vector3(r.center.x, (y0 + y1) * 0.5f, r.center.y), new Vector3(r.width, y1 - y0, r.height));
    }
}
