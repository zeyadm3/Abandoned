using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the exterior brick architectural envelope, U-shaped wings, entrance portico,
    /// roofs with HVAC infrastructure, and the landmark 22m historic brick smokestack
    /// for Jackson P. Burley Middle School (1951 Stripped Classical style).
    /// </summary>
    public static class BurleySchoolBuilding
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform building = Group("BuildingShell", root);
            Transform exteriorWalls = Group("ExteriorWalls", building);
            Transform roofs = Group("Roofs", building);
            Transform hvac = Group("RooftopHVAC", building);
            Transform smokestack = Group("HistoricSmokestack", building);
            Transform portico = Group("MainEntrancePortico", building);
            Transform partitions = Group("InteriorPartitions", building);

            // ---- 1. Exterior Walls ----
            BuildExteriorShell(exteriorWalls, art);

            // ---- 2. Main Entrance Portico ----
            BuildEntrancePortico(portico, art);

            // ---- 3. Historic Brick Smokestack (Iconic Burley Landmark) ----
            BuildSmokestack(smokestack, art);

            // ---- 4. Flat Roofs & HVAC Infrastructure ----
            BuildRoofsAndHVAC(roofs, hvac, art);

            // ---- 5. Interior Wing Division Walls ----
            BuildInteriorDivisions(partitions, art);

            MarkStatic(building);
        }

        private static void BuildExteriorShell(Transform parent, BurleySchoolArt art)
        {
            const float wallH = 8.2f;
            const float t = L.WallThickness;

            // --- West Wing (Auditorium) Exterior Walls ---
            // South wall (with secondary exit door)
            WallDoor(parent, "WestWing_South", new Vector3(12f, 0f, 20f), 24f, wallH, false, 12f, 2.4f, art.Brick);
            // West outer wall (facing Henry Ave / Booker St)
            WallWindowed(parent, "WestWing_West", new Vector3(0f, 0f, 38f), 36f, wallH, true, art);
            // North outer wall
            WallSolid(parent, "WestWing_North", new Vector3(12f, wallH * 0.5f, 56f), new Vector3(24f, wallH, t), art.Brick);

            // --- Center Wing (Academic Core) Exterior Walls ---
            // South facade (Front entrance wall facing Rose Hill Drive)
            WallDoor(parent, "Center_SouthFacade_W", new Vector3(33f, 0f, 20f), 18f, wallH, false, 33f, 0f, art.Brick);
            // Main Entrance opening at X: 46 (handled by Portico)
            WallDoor(parent, "Center_SouthFacade_E", new Vector3(58f, 0f, 20f), 18f, wallH, false, 58f, 0f, art.Brick);

            // --- North Wing (Cafeteria & Library) Exterior Walls ---
            // West wall (facing inner light court)
            WallWindowed(parent, "North_WestCourt", new Vector3(24f, 0f, 66f), 28f, wallH, true, art);
            // North back wall
            WallWindowed(parent, "North_NorthBack", new Vector3(44f, 0f, 80f), 40f, wallH, false, art);

            // --- East Wing (Gymnasium) Exterior Walls ---
            // South wall (facing front approach & outdoor courts)
            WallSolid(parent, "EastWing_South", new Vector3(82f, wallH * 0.5f, 20f), new Vector3(28f, wallH, t), art.Brick);
            // East outer wall (facing outdoor basketball courts & running track)
            WallClerestory(parent, "EastWing_East", new Vector3(96f, 0f, 44f), 48f, wallH, true, art);
            // North wall (facing parking lot, with gym exit double doors)
            WallDoor(parent, "EastWing_North", new Vector3(82f, 0f, 68f), 28f, wallH, false, 82f, 2.8f, art.Brick);

            // --- Boiler Room Service Wing Exterior Walls ---
            WallSolid(parent, "Boiler_East", new Vector3(68f, 2f, 84f), new Vector3(t, 4f, 16f), art.BrickDark);
            WallDoor(parent, "Boiler_North", new Vector3(56f, 0f, 92f), 24f, 4f, false, 52f, 2.6f, art.BrickDark);
            WallSolid(parent, "Boiler_West", new Vector3(44f, 2f, 84f), new Vector3(t, 4f, 16f), art.BrickDark);

            // Parapet Coping (Limestone trim running along the top of all roof edges)
            Box("Coping_West", parent, new Vector3(0f, wallH + 0.1f, 38f), new Vector3(0.45f, 0.22f, 36.4f), art.Limestone, false);
            Box("Coping_SouthWest", parent, new Vector3(12f, wallH + 0.1f, 20f), new Vector3(24.4f, 0.22f, 0.45f), art.Limestone, false);
            Box("Coping_CenterSouth", parent, new Vector3(46f, wallH + 0.1f, 20f), new Vector3(44.4f, 0.22f, 0.45f), art.Limestone, false);
            Box("Coping_EastSouth", parent, new Vector3(82f, wallH + 0.1f, 20f), new Vector3(28.4f, 0.22f, 0.45f), art.Limestone, false);
            Box("Coping_EastOuter", parent, new Vector3(96f, wallH + 0.1f, 44f), new Vector3(0.45f, 0.22f, 48.4f), art.Limestone, false);
            Box("Coping_NorthWingBack", parent, new Vector3(44f, wallH + 0.1f, 80f), new Vector3(40.4f, 0.22f, 0.45f), art.Limestone, false);
        }

        private static void BuildEntrancePortico(Transform parent, BurleySchoolArt art)
        {
            // Concrete entry stairs leading up to entrance (from loop level y=-0.1 to school slab y=0.0)
            for (int s = 0; s < 3; s++)
            {
                float z = 17.5f + s * 0.8f;
                Box($"EntryStep_{s}", parent, new Vector3(46f, -0.1f + s * 0.05f, z), new Vector3(10f - s * 0.4f, 0.1f, 0.8f), art.Limestone);
            }

            // Stripped Classical portico frame
            Box("PorticoStepPlaza", parent, new Vector3(46f, -0.05f, 19.5f), new Vector3(9.2f, 0.12f, 1.8f), art.Concrete);
            Box("PorticoPillar_L", parent, new Vector3(42.2f, 3.2f, 19.8f), new Vector3(0.85f, 6.4f, 0.85f), art.Limestone);
            Box("PorticoPillar_R", parent, new Vector3(49.8f, 3.2f, 19.8f), new Vector3(0.85f, 6.4f, 0.85f), art.Limestone);

            // Overhead Entablature & Pediment Header
            Box("PorticoEntablature", parent, new Vector3(46f, 6.6f, 19.7f), new Vector3(10.5f, 1.2f, 1.2f), art.Limestone);
            Box("PorticoBrickHeader", parent, new Vector3(46f, 7.6f, 19.8f), new Vector3(9.6f, 0.8f, 0.6f), art.Brick);

            // School Nameplate
            Box("SchoolSignPlate", parent, new Vector3(46f, 6.6f, 19.05f), new Vector3(9.8f, 0.95f, 0.08f), art.LockerGreen);
            Box("SchoolSignGoldBorder", parent, new Vector3(46f, 6.6f, 19.02f), new Vector3(10.0f, 1.05f, 0.04f), art.GoldAccent);

            // Text Label
            Label(parent, "SchoolTitle", "JACKSON P. BURLEY MIDDLE SCHOOL", new Vector3(46f, 6.8f, 18.98f),
                Quaternion.Euler(0f, 180f, 0f), 0.14f, art.GoldAccent.GetColor("_BaseColor"));
            Label(parent, "SchoolMotto", "HOME OF THE BEARS  //  EST. 1951", new Vector3(46f, 6.35f, 18.98f),
                Quaternion.Euler(0f, 180f, 0f), 0.09f, Color.white);

            // Double Entry Glass Doors
            Box("EntryDoorLeft", parent, new Vector3(44.8f, 1.4f, 20.0f), new Vector3(1.6f, 2.8f, 0.08f), art.MetalSteel);
            Box("EntryDoorLeftGlass", parent, new Vector3(44.8f, 1.6f, 20.0f), new Vector3(1.2f, 2.0f, 0.1f), art.WindowGlass, false);
            Box("EntryDoorRight", parent, new Vector3(47.2f, 1.4f, 20.0f), new Vector3(1.6f, 2.8f, 0.08f), art.MetalSteel);
            Box("EntryDoorRightGlass", parent, new Vector3(47.2f, 1.6f, 20.0f), new Vector3(1.2f, 2.0f, 0.1f), art.WindowGlass, false);
        }

        private static void BuildSmokestack(Transform parent, BurleySchoolArt art)
        {
            Vector3 basePos = L.SmokestackPosition;
            const float h = L.SmokestackHeight;

            // Square brick foundation base
            Box("SmokestackFoundation", parent, basePos + Vector3.up * 2f, new Vector3(4.8f, 4f, 4.8f), art.BrickDark);
            Box("SmokestackBasePlinth", parent, basePos + Vector3.up * 4.15f, new Vector3(5.2f, 0.3f, 5.2f), art.Limestone);

            // Octagonal tapering chimney stack
            const int tiers = 9;
            for (int i = 0; i < tiers; i++)
            {
                float y = 4.3f + i * 1.9f;
                float t = i / (float)tiers;
                float diam = Mathf.Lerp(4.2f, 2.5f, t);
                Box($"ChimneyTier_{i}", parent, basePos + Vector3.up * (y + 0.95f), new Vector3(diam, 1.9f, diam), art.Brick);
            }

            // Top Crown & Concrete Chimney Ring
            Box("ChimneyCrown", parent, basePos + Vector3.up * (h - 0.4f), new Vector3(3.2f, 0.8f, 3.2f), art.Limestone);
            Box("ChimneyFlueHole", parent, basePos + Vector3.up * h, new Vector3(1.8f, 0.2f, 1.8f), art.BrickDark, false);

            // Iron maintenance ladder climbing the smokestack
            for (float ly = 1.0f; ly < h - 1.0f; ly += 0.8f)
            {
                Box($"LadderRung_{ly:00}", parent, basePos + new Vector3(0f, ly, -2.5f), new Vector3(0.55f, 0.04f, 0.08f), art.MetalSteel);
            }
        }

        private static void BuildRoofsAndHVAC(Transform roofs, Transform hvac, BurleySchoolArt art)
        {
            const float roofY = 8.25f;

            // Roof slabs over each section
            Box("Roof_WestWing", roofs, new Vector3(12f, roofY, 38f), new Vector3(24f, 0.3f, 36f), art.RoofGravel);
            Box("Roof_CenterWing", roofs, new Vector3(46f, roofY, 36f), new Vector3(44f, 0.3f, 32f), art.RoofGravel);
            Box("Roof_NorthWing", roofs, new Vector3(44f, roofY, 66f), new Vector3(40f, 0.3f, 28f), art.RoofGravel);
            Box("Roof_Gymnasium", roofs, new Vector3(82f, roofY + 0.4f, 44f), new Vector3(28f, 0.35f, 48f), art.RoofGravel);
            Box("Roof_BoilerRoom", roofs, new Vector3(56f, 4.15f, 84f), new Vector3(24f, 0.3f, 16f), art.RoofGravel);

            // Rooftop HVAC Industrial Units
            Vector3[] hvacLocs =
            {
                new(16f, roofY + 0.9f, 32f),
                new(36f, roofY + 0.9f, 28f),
                new(54f, roofY + 0.9f, 42f),
                new(42f, roofY + 0.9f, 70f),
                new(76f, roofY + 1.2f, 36f),
                new(88f, roofY + 1.2f, 54f),
            };

            for (int i = 0; i < hvacLocs.Length; i++)
            {
                Transform unit = Group($"HVAC_Unit_{i}", hvac);
                unit.position = hvacLocs[i];
                Box("Chassis", unit, Vector3.zero, new Vector3(3.2f, 1.8f, 2.2f), art.MetalSteel);
                Box("FanGrille", unit, new Vector3(0f, 0.95f, 0f), new Vector3(1.6f, 0.15f, 1.6f), art.BrickDark, false);
                Box("DuctExhaust", unit, new Vector3(1.8f, 0f, 0f), new Vector3(0.6f, 1.4f, 1.2f), art.Limestone);
            }

            // Rooftop galvanized ducts connecting across the central academic roof
            Box("MainRoofDuct", hvac, new Vector3(46f, roofY + 0.45f, 38f), new Vector3(18f, 0.7f, 0.7f), art.MetalSteel, false);
        }

        private static void BuildInteriorDivisions(Transform parent, BurleySchoolArt art)
        {
            const float h = L.StoryHeight;

            // Ground Floor Main Hallway dividing walls (running East-West along Z: 32 and Z: 38)
            // South classroom corridor wall
            WallWithOpening(parent, "HallWall_South_W", new Vector3(33f, 0f, 32f), 18f, h, false, 30f, 2.2f, art.PlasterWall);
            WallWithOpening(parent, "HallWall_South_E", new Vector3(57f, 0f, 32f), 18f, h, false, 54f, 2.2f, art.PlasterWall);

            // North classroom / cafeteria corridor wall
            WallWithOpening(parent, "HallWall_North_W", new Vector3(33f, 0f, 38f), 18f, h, false, 34f, 2.4f, art.PlasterWall);
            WallWithOpening(parent, "HallWall_North_E", new Vector3(57f, 0f, 38f), 18f, h, false, 58f, 2.4f, art.PlasterWall);

            // Gymnasium partition wall (X: 68, separating Academic Wing from Gym)
            // Includes double entrance doors to the Gymnasium at Z: 35
            WallWithOpening(parent, "Gym_DivisionWall_Ground", new Vector3(68f, 0f, 35f), 18f, h, true, 35f, 3.2f, art.Brick);
            WallSolid(parent, "Gym_DivisionWall_Upper", new Vector3(68f, h * 1.5f, 44f), new Vector3(L.WallThickness, h, 48f), art.Brick);

            // Auditorium partition wall (X: 24, separating Academic Wing from Auditorium)
            // Includes double entrance doors to Auditorium at Z: 35
            WallWithOpening(parent, "Auditorium_DivisionWall_Ground", new Vector3(24f, 0f, 35f), 18f, h, true, 35f, 3.2f, art.Brick);
            WallSolid(parent, "Auditorium_DivisionWall_Upper", new Vector3(24f, h * 1.5f, 38f), new Vector3(L.WallThickness, h, 36f), art.Brick);

            // Cafeteria / Kitchen partition wall (Ground floor Z: 52)
            WallWithOpening(parent, "Cafeteria_EntryWall", new Vector3(44f, 0f, 52f), 40f, h, false, 44f, 4.0f, art.PlasterWall);
        }

        // ---- Wall Geometry Helpers ----
        private static void WallSolid(Transform parent, string name, Vector3 center, Vector3 size, Material mat) =>
            Box(name, parent, center, size, mat);

        private static void WallDoor(Transform parent, string name, Vector3 wallBase, float length, float height,
            bool alongZ, float doorCenter, float doorWidth, Material mat)
        {
            if (doorWidth <= 0.01f)
            {
                WallSolid(parent, name, wallBase + Vector3.up * (height * 0.5f),
                    alongZ ? new Vector3(L.WallThickness, height, length) : new Vector3(length, height, L.WallThickness), mat);
                return;
            }
            WallWithOpening(parent, name, wallBase, length, height, alongZ, doorCenter, doorWidth, mat);
        }

        private static void WallWindowed(Transform parent, string name, Vector3 wallBase, float length, float height, bool alongZ, BurleySchoolArt art)
        {
            // Institutional facade with repeating mid-century classroom windows
            Transform holder = Group(name, parent);
            int bays = Mathf.Max(2, Mathf.FloorToInt(length / 6f));
            float bayW = length / bays;

            for (int b = 0; b < bays; b++)
            {
                float offset = (b + 0.5f) * bayW - length * 0.5f;
                Vector3 bayCenter = wallBase + (alongZ ? Vector3.forward : Vector3.right) * ((b + 0.5f) * bayW);

                // Lower brick spandrel (y: 0 to 1.1m)
                Box($"Spandrel_{b}", holder, bayCenter + Vector3.up * 0.55f,
                    alongZ ? new Vector3(L.WallThickness, 1.1f, bayW) : new Vector3(bayW, 1.1f, L.WallThickness), art.Brick);

                // Limestone Window Sill
                Box($"Sill_{b}", holder, bayCenter + Vector3.up * 1.15f,
                    alongZ ? new Vector3(L.WallThickness + 0.1f, 0.1f, bayW) : new Vector3(bayW, 0.1f, L.WallThickness + 0.1f), art.Limestone);

                // Window Glass Bay (y: 1.2m to 2.8m for Ground Floor)
                Box($"Glass_G_{b}", holder, bayCenter + Vector3.up * 2.0f,
                    alongZ ? new Vector3(0.06f, 1.6f, bayW - 0.4f) : new Vector3(bayW - 0.4f, 1.6f, 0.06f), art.WindowGlass, false);

                // Mid-floor brick band (y: 2.8m to 4.9m)
                Box($"MidBand_{b}", holder, bayCenter + Vector3.up * 3.85f,
                    alongZ ? new Vector3(L.WallThickness, 2.1f, bayW) : new Vector3(bayW, 2.1f, L.WallThickness), art.Brick);

                // Upper Floor Window Glass (y: 4.9m to 6.5m)
                Box($"Glass_U_{b}", holder, bayCenter + Vector3.up * 5.7f,
                    alongZ ? new Vector3(0.06f, 1.6f, bayW - 0.4f) : new Vector3(bayW - 0.4f, 1.6f, 0.06f), art.WindowGlass, false);

                // Upper brick frieze / parapet (y: 6.5m to 8.2m)
                Box($"Parapet_{b}", holder, bayCenter + Vector3.up * 7.35f,
                    alongZ ? new Vector3(L.WallThickness, 1.7f, bayW) : new Vector3(bayW, 1.7f, L.WallThickness), art.Brick);
            }
        }

        private static void WallClerestory(Transform parent, string name, Vector3 wallBase, float length, float height, bool alongZ, BurleySchoolArt art)
        {
            // High clerestory windows for the Gymnasium wall
            Transform holder = Group(name, parent);
            // Lower solid brick wall (0 to 5.5m)
            Box("GymLowerWall", holder, wallBase + Vector3.up * 2.75f + (alongZ ? Vector3.forward : Vector3.right) * (length * 0.5f),
                alongZ ? new Vector3(L.WallThickness, 5.5f, length) : new Vector3(length, 5.5f, L.WallThickness), art.Brick);

            // Clerestory Windows (5.5m to 7.2m)
            Box("ClerestoryGlass", holder, wallBase + Vector3.up * 6.35f + (alongZ ? Vector3.forward : Vector3.right) * (length * 0.5f),
                alongZ ? new Vector3(0.08f, 1.7f, length - 1.2f) : new Vector3(length - 1.2f, 1.7f, 0.08f), art.WindowGlass, false);

            // Parapet Cap (7.2m to 8.2m)
            Box("GymParapet", holder, wallBase + Vector3.up * 7.7f + (alongZ ? Vector3.forward : Vector3.right) * (length * 0.5f),
                alongZ ? new Vector3(L.WallThickness, 1.0f, length) : new Vector3(length, 1.0f, L.WallThickness), art.Brick);
        }

        private static void WallWithOpening(Transform parent, string name, Vector3 wallCenter, float length, float height,
            bool alongZ, float openingCenter, float openingWidth, Material material)
        {
            float start = (alongZ ? wallCenter.z : wallCenter.x) - length * 0.5f;
            float relative = Mathf.Clamp(openingCenter - start, openingWidth * 0.5f, length - openingWidth * 0.5f);
            float left = Mathf.Max(0f, relative - openingWidth * 0.5f);
            float right = Mathf.Max(0f, length - relative - openingWidth * 0.5f);
            Vector3 axis = alongZ ? Vector3.forward : Vector3.right;

            if (left > 0.01f)
            {
                Vector3 c = wallCenter - axis * (length * 0.5f - left * 0.5f);
                Box(name + "_L", parent, c + Vector3.up * (height * 0.5f),
                    alongZ ? new Vector3(L.WallThickness, height, left) : new Vector3(left, height, L.WallThickness), material);
            }
            if (right > 0.01f)
            {
                Vector3 c = wallCenter + axis * (length * 0.5f - right * 0.5f);
                Box(name + "_R", parent, c + Vector3.up * (height * 0.5f),
                    alongZ ? new Vector3(L.WallThickness, height, right) : new Vector3(right, height, L.WallThickness), material);
            }
            float lintel = Mathf.Max(0.1f, height - 2.8f);
            if (lintel > 0.01f)
            {
                Vector3 c = wallCenter + axis * (relative - length * 0.5f) + Vector3.up * (2.8f + lintel * 0.5f);
                Box(name + "_Lintel", parent, c,
                    alongZ ? new Vector3(L.WallThickness, lintel, openingWidth) : new Vector3(openingWidth, lintel, L.WallThickness), material);
            }
        }

        private static void Label(Transform parent, string name, string text, Vector3 at, Quaternion rotation, float size, Color color)
        {
            var label = new GameObject(name).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.position = at;
            label.transform.rotation = rotation;
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = size;
            label.fontSize = 54;
            label.color = color;
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>(BurleySchoolArt.FontPath));
        }
    }
}
