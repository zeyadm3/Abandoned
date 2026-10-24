using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Architectural dimensions, coordinate layouts, and level boundaries for the Skyscraper Restaurant.
    /// Apex occupies two six-metre stories above the 404m arrival floor.
    /// </summary>
    public static class SkyscraperLayout
    {
        public const string SceneName = "Skyscraper";
        public const string DisplayName = "Cloudline Tower & Apex Restaurant";
        public const string ScenePath = "Assets/_Project/Scenes/Skyscraper.unity";

        // Vertical floor elevations
        public const float GroundY = 0f;
        public const float LobbyHeight = 6.0f;
        public const float MechanicalY = 200.0f;
        public const float MechanicalHeight = 5.0f;
        public const float RestaurantY = 404.0f;
        public const float RestaurantHeight = 6.0f;
        public const float UpperRestaurantY = RestaurantY + RestaurantHeight;
        public const float RoofY = UpperRestaurantY + RestaurantHeight;
        public const float SlabThickness = 0.4f;

        public const float FloodedY=120f;
        public const float OfficeY=280f;

        // Footprint limits
        public const float TowerHalfWidth = 18f; // x from -18 to +18 (36m wide)
        public const float TowerBackZ = 22f;     // North curtain wall
        public const float TowerFrontZ = -10f;   // South lobby entrance
        public const float PlazaSouthZ = -34f;   // Van forecourt limit

        // Elevator Shaft Core (x: -2.5 to 2.5, z: -2.5 to 2.5)
        public static readonly Vector3 ShaftCenter = new(0f, (RestaurantY+4f)*.5f, 0f);
        public static readonly Vector3 ShaftSize = new(4.6f, RestaurantY+4f, 4.6f);
        public static readonly Vector3 CabDeckSize = new(3.4f, 0.2f, 3.4f);

        // Fire Stairwell Core (x: -15 to -9, z: -3 to +3)
        public static readonly Vector3 StairwellCenter = new(-12f, (RoofY+3f)*.5f, 1f);
        public static readonly Vector3 StairwellSize = new(7.2f, RoofY+3f, 12f);

        // Van & Extraction Truck Location in the street forecourt
        public static readonly Vector3 TruckRear = new(0f, 0f, -22f);
        public static readonly Vector3 TruckForward = Vector3.forward;

        // Player spawn points on the plaza pavement behind the van
        public static readonly IReadOnlyList<Vector3> CrewSpawns = new[]
        {
            new Vector3(-2.2f, 0.05f, -25.0f),
            new Vector3( 2.2f, 0.05f, -25.0f),
            new Vector3(-1.0f, 0.05f, -26.5f),
            new Vector3( 1.0f, 0.05f, -26.5f)
        };

        // Overall building volume for atmosphere, occlusion, and horror pacing
        public static readonly Bounds BuildingBounds = new(new Vector3(0f, RoofY*.5f+2f, 0f), new Vector3(44f, RoofY+8f, 46f));
        public static readonly Bounds RestaurantBounds = new(new Vector3(0f, (RestaurantY+RoofY)*.5f, 6f), new Vector3(38f, RoofY-RestaurantY+2f, 34f));
        public static readonly Bounds MechanicalBounds = new(new Vector3(0f, MechanicalY+2.5f, 5f), new Vector3(38f, 7f, 30f));
    }
}
