using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the 44-metre concrete fire stair tower connecting Ground (0m) to Apex Restaurant (44m).
    /// Features 11 continuous switchback stair flights, structural landings, railings, and heavy fire exit doors.
    /// </summary>
    public static class SkyscraperStairwell
    {
        public const int FlightCount = 11;
        public const float StoryRise = 4.0f;

        public static List<Transform> Build(Transform root, SkyscraperArt art, out List<Transform> collapsibleLandings)
        {
            Transform stairwell = Group("StairwellTower", root);
            collapsibleLandings = new List<Transform>();
            var allLandings = new List<Transform>();

            // 1. Concrete Stairwell Enclosure Core Walls (x: -15 to -9, z: -3 to +3, y: 0 to 45m)
            float h = L.RestaurantY + 1.5f;
            float midY = h * 0.5f;
            Box("StairWall_West", stairwell, new Vector3(-15.1f, midY, 0f), new Vector3(0.3f, h, 6.4f), art.Concrete);
            Box("StairWall_North", stairwell, new Vector3(-12f, midY, 3.1f), new Vector3(6.4f, h, 0.3f), art.Concrete);
            Box("StairWall_South", stairwell, new Vector3(-12f, midY, -3.1f), new Vector3(6.4f, h, 0.3f), art.Concrete);

            // East wall with doorways at Ground (0m), Mechanical (20m), and Restaurant (44m)
            Box("StairWall_East_Base", stairwell, new Vector3(-8.9f, 10f, 0f), new Vector3(0.3f, 20f, 6.4f), art.Concrete);
            Box("StairWall_East_Upper", stairwell, new Vector3(-8.9f, 32f, 0f), new Vector3(0.3f, 24f, 6.4f), art.Concrete);

            // 2. Build 11 Continuous Switchback Flights
            for (int i = 0; i < FlightCount; i++)
            {
                float baseY = i * StoryRise;
                Transform storyGroup = Group($"StairStory_{i:00}", stairwell);

                // Main landing at South side (z: -1.8 to -3.0, x: -14.8 to -9.2)
                Transform mainLanding = Group($"Landing_Main_{i}", storyGroup);
                mainLanding.position = new Vector3(-12f, baseY, -2.4f);
                BoxCollider colMain = mainLanding.gameObject.AddComponent<BoxCollider>();
                colMain.center = new Vector3(0f, -0.15f, 0f);
                colMain.size = new Vector3(5.6f, 0.3f, 1.2f);
                Box("Visual", mainLanding, colMain.center, colMain.size, art.Concrete, false);
                mainLanding.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
                allLandings.Add(mainLanding);

                // Landing 6 (y = 24m) is weakened concrete that can crack under heavy loot loads
                if (i == 6) collapsibleLandings.Add(mainLanding);

                // Half landing at North side (z: 1.8 to 3.0, x: -14.8 to -9.2, y = baseY + 2.0m)
                Transform halfLanding = Group($"Landing_Half_{i}", storyGroup);
                halfLanding.position = new Vector3(-12f, baseY + 2.0f, 2.4f);
                BoxCollider colHalf = halfLanding.gameObject.AddComponent<BoxCollider>();
                colHalf.center = new Vector3(0f, -0.15f, 0f);
                colHalf.size = new Vector3(5.6f, 0.3f, 1.2f);
                Box("Visual", halfLanding, colHalf.center, colHalf.size, art.Concrete, false);
                halfLanding.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
                allLandings.Add(halfLanding);

                // Flight 1: Ascending Northward from Main Landing to Half Landing (x: -13.6m)
                BuildRampFlight(storyGroup, art, new Vector3(-13.6f, baseY + 1.0f, 0f),
                    new Vector3(2.4f, 2.0f, 3.6f), true);

                // Flight 2: Ascending Southward from Half Landing to Next Main Landing (x: -10.4m)
                BuildRampFlight(storyGroup, art, new Vector3(-10.4f, baseY + 3.0f, 0f),
                    new Vector3(2.4f, 2.0f, 3.6f), false);

                // Central Divider Handrail
                Box("DividerRail", storyGroup, new Vector3(-12.0f, baseY + 2.0f, 0f),
                    new Vector3(0.08f, 3.6f, 3.4f), art.Steel);
            }

            // Top Final Landing at Restaurant Sky Lobby (y = 44m)
            Transform topLanding = Group("Landing_TopSummit", stairwell);
            topLanding.position = new Vector3(-12f, L.RestaurantY, 0f);
            BoxCollider topCol = topLanding.gameObject.AddComponent<BoxCollider>();
            topCol.center = new Vector3(0f, -0.15f, 0f);
            topCol.size = new Vector3(5.6f, 0.3f, 6.0f);
            Box("Visual", topLanding, topCol.center, topCol.size, art.Concrete, false);
            topLanding.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            allLandings.Add(topLanding);

            // 3. Fire Exit Doors (Heavy Steel Doors with Panic Bars)
            BuildFireDoor(stairwell, art, new Vector3(-8.9f, L.GroundY, -2.4f), "Ground Fire Exit");
            BuildFireDoor(stairwell, art, new Vector3(-8.9f, L.MechanicalY, -2.4f), "Mechanical Fire Exit");
            BuildFireDoor(stairwell, art, new Vector3(-8.9f, L.RestaurantY, -2.4f), "Restaurant Fire Exit");

            return allLandings;
        }

        private static void BuildRampFlight(Transform parent, SkyscraperArt art, Vector3 center, Vector3 size, bool northUp)
        {
            Transform ramp = Group(northUp ? "Flight_NorthUp" : "Flight_SouthUp", parent);
            ramp.position = center;

            // Ramp angle matching 2.0m rise over 3.6m run (~29 degrees)
            float angle = Mathf.Atan2(size.y, size.z) * Mathf.Rad2Deg;
            if (!northUp) angle = -angle;

            float diagonalLength = Mathf.Sqrt(size.y * size.y + size.z * size.z);
            GameObject slab = Box("TreadSlab", ramp, Vector3.zero, new Vector3(size.x, 0.25f, diagonalLength), art.Concrete);
            slab.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);

            // Outer Handrail
            float railX = northUp ? -size.x * 0.45f : size.x * 0.45f;
            GameObject rail = Box("Handrail", ramp, new Vector3(railX, 0.9f, 0f), new Vector3(0.06f, 0.06f, diagonalLength), art.Steel, false);
            rail.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private static void BuildFireDoor(Transform parent, SkyscraperArt art, Vector3 at, string name)
        {
            Transform door = Group(name, parent);
            door.position = at;
            Box("Frame", door, new Vector3(0f, 1.25f, 0f), new Vector3(0.35f, 2.5f, 1.2f), art.Steel);
            Box("DoorLeaf", door, new Vector3(0f, 1.25f, 0f), new Vector3(0.08f, 2.4f, 1.0f), art.Steel);
            Box("PushBar", door, new Vector3(-0.06f, 1.0f, 0f), new Vector3(0.05f, 0.08f, 0.8f), art.Brass);
            Box("ExitSign", door, new Vector3(0f, 2.6f, 0f), new Vector3(0.06f, 0.25f, 0.6f), art.RedEmergencyEmissive);
        }
    }
}
