using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gallows Rock Penitentiary's plan as data (Docs/ISLAND_PRISON_PLAN.md): an island in a foggy bay. The dock and
    /// the boat sit at sea level on the south shore; a switchback road and a collapsible cliff stair climb 12 m to the
    /// plateau, where the cellhouse, the rec yard, the warden's house, the lighthouse and the water tower stand. Every
    /// builder reads its numbers here, so moving a block moves its walls, floors, lights, loot and markers together.
    /// Unity units: x east, z north, y up; the quay is y 0, the plateau y 12.
    /// </summary>
    public static class IslandPrisonLayout
    {
        public const string SceneName = "Prison";
        public const string DisplayName = "Gallows Rock Penitentiary";
        public const float Tile = 4f;
        public const float Slab = .25f;
        public const float Storey = 4f;

        // ---- Island levels ----------------------------------------------------------------------------
        public const float SeaY = -2.5f;
        public const float QuayY = 0f;
        public const float PlateauY = 12f;
        /// <summary>Anything whose position drops below this is in the sea: just under the surface, so you can stand at
        /// the water's edge on a rock but not wade in (the quay and the boat deck are at 0).</summary>
        public const float SeaKillTop = SeaY - .15f;

        /// <summary>The plateau's outline: cliffs on every side, the quay below its south face.</summary>
        public static readonly Rect Plateau = new(8f, 20f, 116f, 98f);
        /// <summary>The quay along the south shore under the cliff: the dock, the barracks and the guard tower.</summary>
        public static readonly Rect Quay = new(58f, 0f, 82f, 20f);
        /// <summary>Open water cut into the quay for the boat (x 74-90), and the two piers either side of it.</summary>
        public static readonly Rect Slip = new(74f, -28f, 16f, 28f);
        public static readonly Rect PierWest = new(70f, -24f, 4f, 24f);
        public static readonly Rect PierEast = new(90f, -24f, 4f, 24f);

        /// <summary>The boat: stern (open hold) against the quay edge, bow out to sea (south).</summary>
        public static readonly Vector3 BoatStern = new(82f, -1.05f, -.7f);
        public static Vector3 BoatForward => Vector3.back;

        public static IReadOnlyList<Vector3> CrewSpawns => new[]
        {
            new Vector3(77.5f, .05f, 5f), new Vector3(80.5f, .05f, 5f), new Vector3(83.5f, .05f, 5f), new Vector3(86.5f, .05f, 5f),
        };

        // ---- The road up (never collapses) --------------------------------------------------------------
        /// <summary>Leg A climbs north along the east shore from the quay (y 0) to the turn (y 6).</summary>
        public static readonly Rect RoadA = new(133f, 20f, 7f, 44f);
        public const float RoadAWidthFrom = 133.5f, RoadAWidthTo = 138.5f;
        /// <summary>The turn: a flat landing at y 6.</summary>
        public static readonly Rect RoadTurn = new(124f, 64f, 16f, 8f);
        public const float TurnY = 6f;
        /// <summary>Leg B climbs back south above leg A to the terrace by the cellhouse (y 12).</summary>
        public static readonly Rect RoadB = new(124f, 28f, 9f, 36f);
        public const float RoadBWidthFrom = 124.5f, RoadBWidthTo = 129.5f;
        public static readonly Rect Terrace = new(116f, 20f, 17f, 8f);

        // ---- The cliff stair (collapsible shortcut) -------------------------------------------------------
        public readonly struct Flight
        {
            public readonly string Name;
            public readonly Vector3 Foot;       // low end, centre of the tread line
            public readonly Vector3 Direction;  // horizontal, toward the high end
            public readonly float Run, Rise, Width;
            public readonly bool Collapsible;
            public Flight(string name, Vector3 foot, Vector3 direction, float run, float rise, float width, bool collapsible)
            { Name = name; Foot = foot; Direction = direction.normalized; Run = run; Rise = rise; Width = width; Collapsible = collapsible; }
            public Vector3 Top => Foot + Direction * Run + Vector3.up * Rise;
        }

        public readonly struct Landing
        {
            public readonly string Name;
            public readonly Rect Area;
            public readonly float Y;
            public readonly bool Collapsible;
            public Landing(string name, Rect area, float y, bool collapsible) { Name = name; Area = area; Y = y; Collapsible = collapsible; }
        }

        public static readonly Flight[] CliffFlights =
        {
            new("CliffStair_1", new Vector3(92f, QuayY, 18f), Vector3.right, 8f, 4f, 2f, true),
            new("CliffStair_2", new Vector3(100f, 4f, 15.5f), Vector3.left, 8f, 4f, 2f, true),
            new("CliffStair_3", new Vector3(92f, 8f, 18f), Vector3.right, 8f, 4f, 2f, true),
        };

        public static readonly Landing[] CliffLandings =
        {
            new("CliffLanding_1", new Rect(100f, 14.5f, 2.5f, 5f), 4f, true),
            new("CliffLanding_2", new Rect(89.5f, 14.5f, 2.5f, 5f), 8f, true),
            new("CliffLanding_Top", new Rect(100f, 17f, 2.5f, 2.6f), PlateauY, true),
        };

        // ---- The cellhouse -------------------------------------------------------------------------------
        public static readonly Rect Cellhouse = new(30f, 40f, 48f, 52f);
        public const int CellhouseTilesX = 12, CellhouseTilesZ = 13;
        public const float CellhouseEaves = 25f;
        public const float CellhouseWall = .5f;
        /// <summary>Tier heights inside: the ground floor and two gallery tiers (the runtime's 4 m storeys).</summary>
        public static readonly float[] Tiers = { PlateauY, PlateauY + 4f, PlateauY + 8f };
        /// <summary>South band: the entrance hall with both stack stair towers; admin west, library east.</summary>
        public static readonly Rect SouthBand = new(30f, 40f, 48f, 12f);
        /// <summary>North band: dining hall and kitchen, over the basement.</summary>
        public static readonly Rect NorthBand = new(30f, 80f, 48f, 12f);
        public const float BasementY = PlateauY - Storey;

        /// <summary>Main door (south, on Broadway's axis), the yard door (west), the service door (north, kitchen),
        /// the east door (to the road terrace). x or z of the opening's centre, its width and height.</summary>
        public const float MainDoorX = 54f, MainDoorWidth = 4f, MainDoorHeight = 3.6f;
        public const float YardDoorZ = 82.5f, SideDoorWidth = 2.6f, SideDoorHeight = 2.9f;
        public const float ServiceDoorX = 72f;
        public const float EastDoorZ = 46f;   // through the library's east wall

        /// <summary>A free-standing block of cells: two rows back to back either side of a pipe chase,
        /// three tiers, galleries along both faces. x is west -> east.</summary>
        public readonly struct Stack
        {
            public readonly string Name;
            public readonly float West;     // outer face of the west gallery
            public readonly bool SpineWest; // which tier-1 gallery never falls
            public Stack(string name, float west, bool spineWest) { Name = name; West = west; SpineWest = spineWest; }
            public float CellsWest => West + GalleryWidth;
            public float ChaseWest => CellsWest + CellDepth;
            public float ChaseEast => ChaseWest + ChaseWidth;
            public float CellsEast => ChaseEast + CellDepth;
            public float East => CellsEast + GalleryWidth;
            public float Centre => (CellsWest + CellsEast) * .5f;
        }

        public const float GalleryWidth = 1.6f, CellDepth = 2.6f, ChaseWidth = 1.2f, CellWidth = 2f;
        public const float StackSouth = 54f, StackNorth = 78f;   // cell rows; galleries wrap 2 m further each end
        public const int CellsPerRow = 12;                         // 24 m / 2 m
        public const int GallerySections = 6;                      // 24 m / 4 m

        public static readonly Stack[] Stacks =
        {
            new("C_Block", 39f, true),
            new("B_Block", 59.4f, false),
        };

        /// <summary>The wound: this B-block tier-2 gallery section (west side, index from the south) is already down.</summary>
        public const int FallenGallerySection = 3;
        /// <summary>Rusted sections (stack, tier, west?, index): weaker steel, the obvious gambles.</summary>
        public static readonly (int stack, int tier, bool west, int index)[] WeakGalleries =
        {
            (0, 2, false, 2), (0, 2, true, 4), (0, 1, false, 4), (1, 1, true, 1), (1, 2, false, 3), (1, 2, false, 4),
        };

        /// <summary>D-block solitary along the west wall (cells face east), A-block along the east wall (face west).</summary>
        public const float DBlockFront = 33f, ABlockFront = 75f;
        public const int SideBlockCells = 12;

        /// <summary>The basement stairwell in the dining hall's north-west corner: flight from the basement east to the floor.</summary>
        public static readonly Flight BasementStair = new("BasementStair", new Vector3(33f, BasementY, 86f), Vector3.right, 5.0f, Storey, 2.6f, false);
        public static readonly Vector2Int[] BasementStairHoles = { new(0, 11), new(1, 11) };
        /// <summary>Basement rooms west -> east: laundry, showers, boiler and stores; partitions at these x.</summary>
        public static readonly float[] BasementPartitions = { 50f, 66f };
        /// <summary>The dining hall / kitchen partition sits on the basement's second partition, so it never floats.</summary>
        public const float KitchenWest = 66f;
        public const float NorthBandCeiling = PlateauY + 5f;
        public const float RoomCeiling = PlateauY + 4f;
        public static readonly Rect Admin = new(30f, 40f, 9f, 12f);
        public static readonly Rect Library = new(69f, 40f, 9f, 12f);
        public const float BasementDoorZ = 85f;

        // ---- Outbuildings -----------------------------------------------------------------------------------
        public static readonly Rect RecYard = new(8f, 50f, 22f, 36f);
        public static readonly Rect Barracks = new(104f, 3f, 28f, 12f);
        public const int BarracksStoreys = 3;
        public static readonly Rect GuardTower = new(62f, 6f, 4f, 4f);
        public const float GuardCabinY = 9f;
        public static readonly Rect WardenHouse = new(30f, 100f, 20f, 12f);
        public static readonly Rect Lighthouse = new(58f, 103f, 8f, 8f);
        public const float LighthouseGallery = PlateauY + 16f;
        public static readonly Rect WaterTower = new(100f, 96f, 8f, 8f);
        public static readonly Rect OfficersClub = new(92f, 30f, 18f, 12f);

        /// <summary>The power sector of the lighthouse switch: one no ceiling light uses (HorrorConfig has 6).</summary>
        public const int LighthouseSector = 7;

        // ---- Bounds ------------------------------------------------------------------------------------------
        /// <summary>"Inside" for the horror director and the total collapse: the whole plateau and the road up,
        /// never the quay and the boat (the only way out stays outside it).</summary>
        public static Bounds Building => new(new Vector3(74f, 18f, 69f), new Vector3(140f, 40f, 98f));
        public static Bounds CellhouseBounds => new(new Vector3(Cellhouse.center.x, (BasementY + CellhouseEaves) * .5f, Cellhouse.center.y),
            new Vector3(Cellhouse.width, CellhouseEaves - BasementY + .5f, Cellhouse.height));
        public static Bounds SeaKill => new(new Vector3(70f, -20f, 60f), new Vector3(900f, 2f * (20f + SeaKillTop), 900f));

        // ---- Helpers -------------------------------------------------------------------------------------------
        public static Vector3 CellhouseTileCentre(Vector2Int t, float y) =>
            new(Cellhouse.xMin + (t.x + .5f) * Tile, y, Cellhouse.yMin + (t.y + .5f) * Tile);

        public static bool OverBasement(Vector2Int t) => Cellhouse.yMin + t.y * Tile >= NorthBand.yMin - .01f;

        public static bool IsBasementStairHole(Vector2Int t) => Array.IndexOf(BasementStairHoles, t) >= 0;

        public static float TierY(int tier) => Tiers[Mathf.Clamp(tier, 0, Tiers.Length - 1)];
    }
}
