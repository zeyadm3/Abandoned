using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A = Abandoned.EditorTools.BurleySchoolArchitecture;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Pine Hollow's brick school envelope, open entrance, internal room walls and winter roofscape.</summary>
    public static class BurleySchoolBuilding
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform building = Group("BuildingShell", root);
            BuildExteriorShell(Group("ExteriorWalls", building), art);
            BuildEntrancePortico(Group("MainEntrancePortico", building), art);
            BuildSmokestack(Group("HistoricSmokestack", building), art);
            BuildRoofsAndHVAC(Group("Roofs", building), Group("RooftopHVAC", building), art);
            BuildInteriorDivisions(Group("InteriorPartitions", building), art);
            BuildWinterAccents(Group("WinterAccents", building), art);
            MarkStatic(building);
        }

        private static void BuildExteriorShell(Transform parent, BurleySchoolArt art)
        {
            const float h = 8.2f;
            WallDoor(parent, "WestWing_South", new Vector3(12f, 0f, 20f), 24f, h, false, 12f, 2.4f, art.Brick);
            A.WindowWall(parent, "WestWing_West", new Vector3(0f, 0f, 38f), 36f, h, true, art);
            Box("WestWing_North", parent, new Vector3(12f, h * 0.5f, 56f), new Vector3(24f, h, L.WallThickness), art.Brick);

            // Front classrooms now have actual windows instead of a featureless brick curtain.
            A.WindowWall(parent, "Center_SouthFacade_W", new Vector3(34.2f, 0f, 20f), 20.4f, h, false, art);
            A.WindowWall(parent, "Center_SouthFacade_E", new Vector3(57.8f, 0f, 20f), 20.4f, h, false, art);
            Box("EntranceUpperWall", parent, new Vector3(46f, 5.75f, 20f), new Vector3(3.2f, 4.9f, L.WallThickness), art.Brick);

            A.WindowWall(parent, "North_WestCourt", new Vector3(24f, 0f, 68f), 24f, h, true, art);
            A.WindowWall(parent, "North_CafeteriaBack", new Vector3(34f, 0f, 80f), 20f, h, false, art);
            WallDoor(parent, "North_KitchenBoilerWall", new Vector3(56f, 0f, 80f), 24f, 4f, false, 50f, 2.8f, art.BrickDark);
            A.WindowWall(parent, "Library_North", new Vector3(56f, 4f, 80f), 24f, 4.2f, false, art);
            A.WindowWall(parent, "NorthWing_EastReturn", new Vector3(68f, 0f, 74f), 12f, h, true, art);

            A.Clerestory(parent, "EastWing_South", new Vector3(84f, 0f, 20f), 32f, h, false, art);
            A.Clerestory(parent, "EastWing_East", new Vector3(100f, 0f, 44f), 48f, h, true, art);
            WallDoor(parent, "EastWing_North", new Vector3(84f, 0f, 68f), 32f, h, false, 84f, 2.8f, art.Brick);

            // The service floor sits over the sunken plant room. Its retaining walls start at basement level.
            Box("Boiler_East", parent, new Vector3(68f, 0.25f, 86f), new Vector3(L.WallThickness, 7.5f, 12f), art.BrickDark);
            Box("Boiler_West", parent, new Vector3(44f, 0.25f, 86f), new Vector3(L.WallThickness, 7.5f, 12f), art.BrickDark);
            Box("Boiler_NorthRetainingWall", parent, new Vector3(56f, -1.75f, 92f), new Vector3(24f, 3.5f, L.WallThickness), art.Concrete);
            // The old central opening led straight into the smokestack's foundation.
            WallDoor(parent, "Boiler_North", new Vector3(56f, 0f, 92f), 24f, 4f, false, 62f, 2.6f, art.BrickDark);

            (string name, Vector3 at, float length, bool alongZ)[] cornices =
            {
                ("WestFront", new Vector3(12f, 8.25f, 20f), 24f, false),
                ("AcademicFront", new Vector3(46f, 8.25f, 20f), 44f, false),
                ("GymFront", new Vector3(84f, 8.25f, 20f), 32f, false),
                ("WestOuter", new Vector3(0f, 8.25f, 38f), 36f, true),
                ("AuditoriumRear", new Vector3(12f, 8.25f, 56f), 24f, false),
                ("NorthWest", new Vector3(24f, 8.25f, 68f), 24f, true),
                ("NorthRear", new Vector3(46f, 8.25f, 80f), 44f, false),
                ("NorthEast", new Vector3(68f, 8.25f, 74f), 12f, true),
                ("GymOuter", new Vector3(100f, 8.25f, 44f), 48f, true),
                ("GymRear", new Vector3(84f, 8.25f, 68f), 32f, false),
                ("PlantRear", new Vector3(56f, 4.35f, 92f), 24f, false)
            };
            foreach (var c in cornices) A.Cornice(parent, c.name, c.at, c.length, c.alongZ, art);

            // Pilasters and string courses give the brick wings a legible institutional rhythm.
            foreach (float x in new[] { 0f, 24f, 40f, 52f, 68f, 100f })
            {
                Box($"FrontPilaster_{x}", parent, new Vector3(x, 4.05f, 19.8f), new Vector3(0.75f, 8.1f, 0.5f), art.BrickDark);
                Box($"PilasterBase_{x}", parent, new Vector3(x, 0.22f, 19.7f), new Vector3(1f, 0.44f, 0.68f), art.Limestone);
                Box($"PilasterCap_{x}", parent, new Vector3(x, 7.65f, 19.7f), new Vector3(1f, 0.22f, 0.65f), art.Limestone, false);
            }
            Box("AcademicStringCourseW", parent, new Vector3(34.2f, 4.02f, 19.82f), new Vector3(20.4f, 0.16f, 0.14f), art.Limestone, false);
            Box("AcademicStringCourseE", parent, new Vector3(57.8f, 4.02f, 19.82f), new Vector3(20.4f, 0.16f, 0.14f), art.Limestone, false);
        }

        private static void BuildEntrancePortico(Transform parent, BurleySchoolArt art)
        {
            // Keep the centre at the building's floor level; the old raised platform ended in a drop at the doorway.
            Box("PorticoPaving", parent, new Vector3(46f, -0.06f, 17.8f), new Vector3(10.2f, 0.12f, 4.4f), art.Limestone);
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 5; col++)
                Box($"PavingJoint_{row}_{col}", parent, new Vector3(42.1f + col * 1.95f, 0.003f, 16.2f + row * 1.45f),
                    new Vector3(1.88f, 0.008f, 1.38f), art.Concrete, false);

            foreach (float x in new[] { 42.4f, 44.2f, 47.8f, 49.6f })
            {
                Transform column = Group($"Column_{x}", parent);
                column.localPosition = new Vector3(x, 0f, 16.8f);
                Box("Plinth", column, new Vector3(0f, 0.18f, 0f), new Vector3(0.7f, 0.36f, 0.7f), art.Limestone);
                A.Cylinder(column, "BaseMoulding", new Vector3(0f, 0.4f, 0f), 0.33f, 0.12f, art.Limestone);
                A.Cylinder(column, "Shaft", new Vector3(0f, 3.8f, 0f), 0.24f, 6.7f, art.Limestone, true);
                A.Cylinder(column, "Necking", new Vector3(0f, 7.18f, 0f), 0.29f, 0.12f, art.Limestone);
                Box("Capital", column, new Vector3(0f, 7.36f, 0f), new Vector3(0.76f, 0.25f, 0.76f), art.Limestone, false);
            }
            Transform entablature = Group("EntranceEntablature", parent);
            entablature.localPosition = new Vector3(46f, 7.8f, 18.1f);
            Box("Architrave", entablature, Vector3.zero, new Vector3(9.2f, 0.6f, 3.8f), art.Limestone);
            Box("Cornice", entablature, new Vector3(0f, 0.4f, 0f), new Vector3(9.6f, 0.2f, 4f), art.Limestone, false);
            Transform pediment = Group("Pediment", entablature);
            pediment.localPosition = new Vector3(0f, 0.52f, -1.35f);
            A.Pediment(pediment, art);
            Label(entablature, "HistoricSchoolTitle", "PINE HOLLOW MIDDLE SCHOOL", new Vector3(0f, 0f, -1.92f),
                Quaternion.identity, 0.15f, new Color(0.22f, 0.2f, 0.18f));

            Transform entrance = Group("EntranceDoors", parent);
            entrance.localPosition = new Vector3(46f, 0f, 20f);
            foreach (float side in new[] { -1f, 1f })
            {
                Box(side < 0f ? "JambL" : "JambR", entrance, new Vector3(side * 1.7f, 1.65f, -0.08f),
                    new Vector3(0.2f, 3.3f, 0.3f), art.Limestone);
                Transform leaf = Group(side < 0f ? "ProppedDoorL" : "ProppedDoorR", entrance);
                leaf.localPosition = new Vector3(side * 1.48f, 0f, 0.06f);
                leaf.localRotation = Quaternion.Euler(0f, side * 90f, 0f);
                Box("Leaf", leaf, new Vector3(side * -0.7f, 1.25f, 0f), new Vector3(1.4f, 2.5f, 0.08f), art.LockerGreen);
                Box("UpperGlazing", leaf, new Vector3(side * -0.7f, 1.83f, -0.05f), new Vector3(1.06f, 0.84f, 0.03f), art.WindowGlass, false);
                Box("KickPlate", leaf, new Vector3(side * -0.7f, 0.22f, -0.05f), new Vector3(1.3f, 0.3f, 0.025f), art.MetalSteel, false);
                Box("PushBar", leaf, new Vector3(side * -0.7f, 1.08f, -0.1f), new Vector3(1.08f, 0.055f, 0.05f), art.GoldAccent, false);
            }
            Box("TransomFrame", entrance, new Vector3(0f, 2.58f, 0f), new Vector3(3.2f, 0.12f, 0.18f), art.MetalSteel, false);
            Box("TransomGlass", entrance, new Vector3(0f, 2.96f, 0f), new Vector3(3.2f, 0.64f, 0.05f), art.WindowGlass);
            Box("Lintel", entrance, new Vector3(0f, 3.34f, 0f), new Vector3(3.6f, 0.16f, 0.35f), art.Limestone);
            Box("SchoolPlaque", parent, new Vector3(50.6f, 1.65f, 19.65f), new Vector3(1.1f, 0.7f, 0.12f), art.LockerGreen, false);
            Label(parent, "SchoolPlaqueText", "PINE HOLLOW\nMIDDLE SCHOOL", new Vector3(50.6f, 1.65f, 19.58f), Quaternion.identity, 0.085f, art.GoldAccent.GetColor("_BaseColor"));
            Label(parent, "Cornerstone", "1951", new Vector3(40f, 0.85f, 19.5f), Quaternion.identity, 0.1f, new Color(0.2f, 0.19f, 0.17f));
        }

        private static void BuildSmokestack(Transform parent, BurleySchoolArt art)
        {
            Transform stack = Group("Smokestack", parent);
            stack.localPosition = L.SmokestackPosition;
            Box("FoundationPlinth", stack, new Vector3(0f, 0.5f, 0f), new Vector3(4.8f, 1f, 4.8f), art.Concrete);
            A.Cylinder(stack, "BrickPedestal", new Vector3(0f, 2.5f, 0f), 2.1f, 3f, art.BrickDark, true);
            // Short diminishing courses produce a round, subtly tapering industrial chimney silhouette.
            for (int i = 0; i < 18; i++)
            {
                float radius = Mathf.Lerp(1.8f, 1.25f, i / 17f);
                A.Cylinder(stack, $"ShaftCourse_{i:00}", new Vector3(0f, 4.5f + i, 0f), radius, 1.02f, art.Brick, true);
            }
            A.Cylinder(stack, "CrownBrick", new Vector3(0f, 22.1f, 0f), 1.45f, 0.55f, art.BrickDark);
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI / 12f;
                float b = (i + 1) * Mathf.PI / 12f;
                Vector3 p = new(Mathf.Sin(a) * 1.48f, 22.5f, Mathf.Cos(a) * 1.48f);
                Vector3 q = new(Mathf.Sin(b) * 1.48f, 22.5f, Mathf.Cos(b) * 1.48f);
                A.Rod(stack, $"CrownRim_{i}", p, q, 0.18f, art.Concrete);
                A.Rod(stack, $"CrownSnow_{i}", p + Vector3.up * 0.14f, q + Vector3.up * 0.14f, 0.11f, art.Snow);
            }
            A.Cylinder(stack, "DarkFlue", new Vector3(0f, 22.32f, 0f), 1.2f, 0.06f, art.BrickDark);
            for (int i = 0; i < 42; i++)
                A.Rod(stack, $"LadderRung_{i:00}", new Vector3(-0.28f, 3f + i * 0.45f, -1.98f), new Vector3(0.28f, 3f + i * 0.45f, -1.98f), 0.025f, art.MetalSteel);
            A.Rod(stack, "LadderRailL", new Vector3(-0.3f, 2.8f, -1.98f), new Vector3(-0.3f, 22f, -1.98f), 0.035f, art.MetalSteel);
            A.Rod(stack, "LadderRailR", new Vector3(0.3f, 2.8f, -1.98f), new Vector3(0.3f, 22f, -1.98f), 0.035f, art.MetalSteel);
            A.Rod(stack, "LightningRod", new Vector3(0.9f, 22.5f, 0.9f), new Vector3(0.9f, 24.5f, 0.9f), 0.035f, art.GoldAccent);
        }

        private static void BuildRoofsAndHVAC(Transform roofs, Transform hvac, BurleySchoolArt art)
        {
            (string name, Vector3 at, Vector2 size)[] wings =
            {
                ("WestWing", new Vector3(12f, 8.15f, 38f), new Vector2(24f, 36f)),
                ("CenterWing", new Vector3(46f, 8.15f, 36f), new Vector2(44f, 32f)),
                ("NorthWing", new Vector3(46f, 8.15f, 66f), new Vector2(44f, 28f)),
                ("EastWing", new Vector3(84f, 8.15f, 44f), new Vector2(32f, 48f)),
                ("BoilerWing", new Vector3(56f, 4.15f, 86f), new Vector2(24f, 12f))
            };
            foreach (var wing in wings)
            {
                Box("Roof_" + wing.name, roofs, wing.at, new Vector3(wing.size.x, 0.3f, wing.size.y), art.RoofGravel);
                Box("Snow_" + wing.name, roofs, wing.at + Vector3.up * 0.22f,
                    new Vector3(wing.size.x - 0.4f, 0.14f, wing.size.y - 0.4f), art.Snow, false);
                for (int seam = 1; seam < 4; seam++)
                    Box($"ThawStrip_{wing.name}_{seam}", roofs, wing.at + new Vector3(-wing.size.x * 0.5f + seam * wing.size.x / 4f, 0.3f, 0f),
                        new Vector3(0.09f, 0.015f, wing.size.y - 0.8f), art.RoofGravel, false);
            }
            Vector3[] positions = { new(36f, 9.15f, 32f), new(55f, 9.15f, 36f), new(36f, 9.15f, 64f),
                new(80f, 9.15f, 36f), new(80f, 9.15f, 54f), new(12f, 9.15f, 42f) };
            for (int i = 0; i < positions.Length; i++)
            {
                Transform unit = Group($"HVAC_Unit_{i}", hvac);
                unit.localPosition = positions[i];
                Box("RoofCurb", unit, new Vector3(0f, -0.68f, 0f), new Vector3(3.5f, 0.24f, 2.6f), art.BrickDark);
                Box("Chassis", unit, Vector3.zero, new Vector3(3.2f, 1.2f, 2.2f), art.MetalSteel);
                for (int louver = 0; louver < 7; louver++)
                    Box($"IntakeLouver_{louver}", unit, new Vector3(0f, -0.42f + louver * 0.13f, -1.12f),
                        new Vector3(2.8f, 0.035f, 0.04f), art.BrickDark, false);
                A.Cylinder(unit, "FanWell", new Vector3(0.65f, 0.64f, 0f), 0.7f, 0.08f, art.BrickDark);
                for (int blade = 0; blade < 4; blade++)
                {
                    GameObject fan = Box($"FanBlade_{blade}", unit, new Vector3(0.65f, 0.7f, 0f), new Vector3(1.2f, 0.025f, 0.11f), art.MetalSteel, false);
                    fan.transform.localRotation = Quaternion.Euler(0f, blade * 45f, 0f);
                }
                Box("WindwardSnowCap", unit, new Vector3(-0.92f, 0.67f, 0f), new Vector3(1.32f, 0.16f, 2.18f), art.Snow, false);
                Box("DuctElbow", unit, new Vector3(2f, -0.1f, 0f), new Vector3(1.2f, 0.8f, 0.85f), art.MetalSteel);
                Box("DuctDrop", unit, new Vector3(2.45f, -0.42f, 0f), new Vector3(0.4f, 0.72f, 0.85f), art.MetalSteel);
            }
        }

        private static void BuildInteriorDivisions(Transform parent, BurleySchoolArt art)
        {
            WallDoor(parent, "Div_West_Center", new Vector3(24f, 0f, 36f), 32f, 8.2f, true, 36f, 2.8f, art.PlasterWall);
            WallDoor(parent, "Div_Center_East", new Vector3(68f, 0f, 36f), 32f, 8.2f, true, 36f, 2.8f, art.PlasterWall);
            WallDoor(parent, "Div_Kitchen_Gym", new Vector3(68f, 0f, 60f), 16f, 8.2f, true, 60f, 2.4f, art.PlasterWall);
            for (int story = 0; story < 2; story++)
            {
                float y = story * L.StoryHeight;
                WallDoor(parent, $"Div_Center_North_{story}", new Vector3(46f, y, 52f), 44f, 4f, false, 46f, 3.6f, art.PlasterWall);
                foreach (float x in new[] { 32f, 60f })
                foreach (float z in new[] { 32f, 40f })
                    WallDoor(parent, $"RoomHallWall_{story}_{x}_{z}", new Vector3(x, y, z), 16f, 4f, false, x, 1.8f, art.PlasterWall);
                foreach (float x in new[] { 40f, 52f })
                {
                    WallDoor(parent, $"LobbyRoomSide_{story}_{x}", new Vector3(x, y, 26f), 12f, 4f, true, 26f, 1.8f, art.PlasterWall);
                    WallDoor(parent, $"NorthRoomSide_{story}_{x}", new Vector3(x, y, 46f), 12f, 4f, true, 46f, 1.8f, art.PlasterWall);
                }
            }
            // Close the short shared edge behind the auditorium without putting glass into an adjoining room.
            Box("AuditoriumLibraryReturn", parent, new Vector3(24f, 4.1f, 54f), new Vector3(0.24f, 8.2f, 4f), art.PlasterWall);
        }

        private static void BuildWinterAccents(Transform parent, BurleySchoolArt art)
        {
            Box("Drift_SouthFacade_W", parent, new Vector3(29f, 0.22f, 19.3f), new Vector3(6f, 0.44f, 1.1f), art.SnowDrift, false);
            Box("Drift_SouthFacade_E", parent, new Vector3(64f, 0.3f, 19.3f), new Vector3(7f, 0.6f, 1.2f), art.SnowDrift, false);
            Box("Drift_GymEastWall", parent, new Vector3(100.7f, 0.4f, 44f), new Vector3(1.4f, 0.8f, 24f), art.SnowDrift, false);
            Box("Drift_NorthWingBack", parent, new Vector3(34f, 0.3f, 80.7f), new Vector3(18f, 0.6f, 1.2f), art.SnowDrift, false);
            for (int i = 0; i < 14; i++)
            {
                float x = 41.7f + i * 0.65f;
                float h = 0.18f + (Mathf.Sin(i * 2.7f) + 1f) * 0.13f;
                A.Rod(parent, $"PorticoIcicle_{i}", new Vector3(x, 7.5f, 16.18f), new Vector3(x, 7.5f - h, 16.18f), 0.025f, art.FrostIce);
            }
            foreach (Vector3 foot in new[] { new Vector3(0.28f, 0f, 20.3f), new Vector3(24.25f, 0f, 79.7f), new Vector3(99.72f, 0f, 20.3f), new Vector3(99.72f, 0f, 67.7f) })
            {
                Transform drain = Group($"RainwaterPipe_{foot.x}_{foot.z}", parent);
                drain.localPosition = foot;
                A.Cylinder(drain, "Downpipe", new Vector3(0f, 3.9f, 0f), 0.075f, 7.8f, art.MetalSteel);
                for (int strap = 0; strap < 4; strap++)
                    Box($"WallStrap_{strap}", drain, new Vector3(0f, 1f + strap * 1.8f, 0f), new Vector3(0.22f, 0.04f, 0.18f), art.BrickDark, false);
            }
        }

        private static void WallDoor(Transform parent, string name, Vector3 foot, float length, float height,
            bool alongZ, float doorPosition, float doorWidth, Material material)
        {
            Transform wall = Group(name, parent);
            wall.localPosition = foot;
            wall.localRotation = Quaternion.Euler(0f, alongZ ? -90f : 0f, 0f);
            float door = doorPosition - (alongZ ? foot.z : foot.x);
            const float openingHeight = 2.4f;
            float leftLength = door - doorWidth * 0.5f + length * 0.5f;
            float rightLength = length * 0.5f - door - doorWidth * 0.5f;
            if (leftLength > 0.01f)
                Box("WallL", wall, new Vector3(-length * 0.5f + leftLength * 0.5f, height * 0.5f, 0f),
                    new Vector3(leftLength, height, L.WallThickness), material);
            if (rightLength > 0.01f)
                Box("WallR", wall, new Vector3(length * 0.5f - rightLength * 0.5f, height * 0.5f, 0f),
                    new Vector3(rightLength, height, L.WallThickness), material);
            if (height > openingHeight)
                Box("Lintel", wall, new Vector3(door, openingHeight + (height - openingHeight) * 0.5f, 0f),
                    new Vector3(doorWidth, height - openingHeight, L.WallThickness), material);
        }
    }
}
