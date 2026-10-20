using UnityEngine;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where the furniture the lessons put salvage on stands (floor centre and facing), shared by the rooms
    /// (<see cref="TutorialHouseRooms"/>) and the salvage spots (<see cref="TutorialHouseGameplay"/>), so a lamp is
    /// always on its side table. Everything stands against a wall with at least 1.3 m clear in front: the hauling
    /// routes (hallway to front door along the band's south half, the fridge from the kitchen's north wall, the clock
    /// from the north bedroom down the hallway) keep 2 m or more.
    /// </summary>
    public static class TutorialHouseFittings
    {
        private static float Inside(float wall, float sign, float depth, bool exterior = false) =>
            wall + sign * ((exterior ? L.ExteriorWall : L.InteriorWall) * .5f + depth * .5f + .02f);

        // ---- Living room (sofa on the north wall facing the TV on the south wall) ----
        public static readonly Vector3 Sofa = new(23f, L.Floor, Inside(L.BandNorth, -1f, .9f, true));
        public static readonly Vector3 SideTable = new(21.35f, L.Floor, Inside(L.BandNorth, -1f, .45f, true));
        public static readonly Vector3 CoffeeTable = new(23f, L.Floor, 11.7f);
        public static readonly Vector3 ArmchairWest = new(21.2f, L.Floor, 11.6f), ArmchairEast = new(24.8f, L.Floor, 11.6f);
        public static readonly Vector3 TvStand = new(23.2f, L.Floor, Inside(L.BandSouth, 1f, .45f));
        public const float SideTableTop = .6f, CoffeeTableTop = .42f, ArmchairSeat = .49f, TvStandTop = .5f, SofaSeat = .52f;

        // ---- Kitchen (counters on the north and west walls, an island well clear of both) ----
        public static readonly Vector3 CounterNorth = new(10.15f, L.Floor, Inside(L.BandNorth, -1f, .62f, true));
        public const float CounterNorthLength = 4.1f, CounterTop = .92f;
        public static readonly Vector3 CounterWest = new(Inside(L.WingEast, 1f, .62f), L.Floor, 11.85f);
        public const float CounterWestLength = 1.3f;
        public static readonly Vector3 Stove = new(Inside(L.WingEast, 1f, .62f), L.Floor, 12.9f);
        public static readonly Vector3 Fridge = new(12.95f, L.Floor, Inside(L.BandNorth, -1f, .76f, true));
        public static readonly Vector3 Island = new(11f, L.Floor, 10.4f);
        public const float IslandLength = 2f;

        // ---- Dining room ----
        public static readonly Vector3 DiningTable = new(16.75f, L.Floor, 10.4f);
        public static readonly Vector3 Sideboard = new(16.75f, L.Floor, Inside(L.BandNorth, -1f, .5f, true));
        public const float DiningTableTop = .76f, SideboardTop = .85f;

        // ---- Laundry alcove (machines on its south wall, the shelf above them) ----
        public static readonly Vector3 Washer = new(14.8f, L.Floor, Inside(L.Laundry.yMin, 1f, .7f));
        public static readonly Vector3 Dryer = new(15.7f, L.Floor, Inside(L.Laundry.yMin, 1f, .7f));
        public static readonly Vector3 LaundryShelf = new(15.75f, 1.3f, Inside(L.Laundry.yMin, 1f, .3f));

        // ---- North bedroom (the grandfather clock on the west wall, clear run to the hallway door) ----
        public static readonly Vector3 Clock = new(Inside(0f, 1f, .5f, true), L.Floor, 12.6f);
        public static readonly Vector3 TwinBed = new(2.2f, L.Floor, Inside(L.WestWing.yMax, -1f, 2.1f, true));
        public static readonly Vector3 NightstandA = new(3.3f, L.Floor, Inside(L.WestWing.yMax, -1f, .4f, true));
        public const float NightstandTop = .55f;

        // ---- South bedroom (bed and nightstand on the rotten patch, against the west wall) ----
        public static readonly Vector3 QueenBedB = new(Inside(0f, 1f, 2.1f, true), L.Floor, 2.9f);
        public static readonly Vector3 NightstandB = new(Inside(0f, 1f, .4f, true), L.Floor, 1.65f);

        // ---- Ensuite and master ----
        public static readonly Vector3 EnsuiteVanity = new(15.9f, L.Floor, Inside(L.Laundry.yMin, -1f, .54f));
        public const float VanityTop = .86f;
        public static readonly Vector3 MasterDresser = new(20.3f, L.Floor, Inside(0f, 1f, .5f, true));
        public const float DresserTop = .85f;
        public static readonly Vector3 MasterBed = new(Inside(L.HouseEast, -1f, 2.1f, true), L.Floor, 3f);
    }
}
