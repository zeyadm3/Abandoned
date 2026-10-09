using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>High-bay warehouse and a glazed operations wing, with real openings between departments.</summary>
    public static class HqIndustrialShell
    {
        public static void Build(Transform root)
        {
            Transform shell = Group("IndustrialCompanyBuilding", root);
            HqIndustrialMaterials.Tile(Box("WarehouseFoundation", shell, new Vector3(18f, -.18f, 14f), new Vector3(HqLayout.Width, .36f, HqLayout.Depth),
                HqIndustrialMaterials.Concrete), new Vector2(18f, 14f)).AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            Box("DispatchOfficeFloor", shell, new Vector3(30f, .012f, 5f), new Vector3(11.8f, .024f, 9.8f), HqIndustrialMaterials.Vinyl, false);
            Box("CrewRoomFloor", shell, new Vector3(31.5f, .012f, 14f), new Vector3(8.8f, .024f, 7.8f), HqIndustrialMaterials.Vinyl, false);
            Hall(shell);
            Wing(shell);
            Frames(shell);
            Roof(shell);
            MarkStatic(shell);
            HqCompanyYard.Build(root);
        }

        private static void Hall(Transform shell)
        {
            Wall(shell, "HallWest", new Vector3(0f, 0f, 14f), 28f, HqLayout.Eave, 90f);
            Wall(shell, "HallRear", new Vector3(12f, 0f, 28f), 24f, HqLayout.Eave);
            Wall(shell, "LoadingJambWest", new Vector3(1f, 0f, -.1f), 2f, HqLayout.Eave);
            Wall(shell, "LoadingJambEast", new Vector3(17f, 0f, -.1f), 14f, HqLayout.Eave);
            Wall(shell, "LoadingHeader", new Vector3(6f, 3.4f, -.1f), 8f, HqLayout.Eave - 3.4f);

            // Dispatch looks directly into receiving. The doorway opens onto its pedestrian route.
            Glazed(shell, "DispatchWarehouseView", new Vector3(24f, 0f, 3.1f), 6.2f, 3.05f, 90f);
            Glazed(shell, "DispatchDoorSide", new Vector3(24f, 0f, 9.3f), 1.4f, 3.05f, 90f);
            Wall(shell, "OfficeOverwallSouth", new Vector3(24f, 3.05f, 3.1f), 6.2f, HqLayout.Eave - 3.05f, 90f);
            Wall(shell, "OfficeOverwallNorth", new Vector3(24f, 3.05f, 9.3f), 1.4f, HqLayout.Eave - 3.05f, 90f);
            Wall(shell, "DispatchDoorHeader", new Vector3(24f, 2.75f, 7.4f), 2.4f, HqLayout.Eave - 2.75f, 90f);
            DoorFrame(shell, new Vector3(24f, 0f, 7.4f), 2.4f, 2.75f, 90f, "DISPATCH");

            Wall(shell, "ServicePierSouth", new Vector3(24f, 0f, 11.2f), 2.4f, HqLayout.Eave, 90f);
            Wall(shell, "ServicePierMiddle", new Vector3(24f, 0f, 18.2f), 5.2f, HqLayout.Eave, 90f);
            Wall(shell, "ServicePierNorth", new Vector3(24f, 0f, 26.6f), 2.8f, HqLayout.Eave, 90f);
            Wall(shell, "CrewAccessHeader", new Vector3(24f, 3.2f, 14f), 3.2f, HqLayout.Eave - 3.2f, 90f);
            Wall(shell, "WorkshopAccessHeader", new Vector3(24f, 3.2f, 23f), 4.4f, HqLayout.Eave - 3.2f, 90f);
            DoorFrame(shell, new Vector3(24f, 0f, 14f), 3.2f, 3.2f, 90f, "CREW / STORES");
            DoorFrame(shell, new Vector3(24f, 0f, 23f), 4.4f, 3.2f, 90f, "REPAIR BAY");
        }

        private static void Wing(Transform shell)
        {
            Glazed(shell, "OfficeFrontWest", new Vector3(26.4f, 0f, -.1f), 4.8f, HqLayout.WingHeight);
            Glazed(shell, "OfficeFrontEast", new Vector3(33.6f, 0f, -.1f), 4.8f, HqLayout.WingHeight);
            Wall(shell, "StaffEntranceHeader", new Vector3(30f, 2.75f, -.1f), 2.4f, 1.05f);
            DoorFrame(shell, new Vector3(30f, 0f, -.1f), 2.4f, 2.75f, 0f, "OPERATIONS / STAFF ENTRY");
            Wall(shell, "WingRear", new Vector3(30f, 0f, 28f), 12f, HqLayout.WingHeight);
            Wall(shell, "LogisticsOfficeWall", new Vector3(36f, 0f, 3.5f), 7f, HqLayout.WingHeight, 90f);
            Glazed(shell, "OfficeEastWindow", new Vector3(36f, 0f, 8.5f), 3f, HqLayout.WingHeight, 90f);
            Wall(shell, "WingEastOfficeCrewPier", new Vector3(36f, 0f, 10.5f), 1f, HqLayout.WingHeight, 90f);
            Glazed(shell, "CrewHighWindow", new Vector3(36f, 0f, 13.5f), 5f, HqLayout.WingHeight, 90f, 2.25f, .90f);
            Wall(shell, "WorkshopEastWall", new Vector3(36f, 0f, 22f), 12f, HqLayout.WingHeight, 90f);

            Wall(shell, "DispatchBackSmallPier", new Vector3(24.25f, 0f, 10f), .5f, HqLayout.WingHeight);
            Wall(shell, "DispatchBackWall", new Vector3(31.45f, 0f, 10f), 9.1f, HqLayout.WingHeight);
            Wall(shell, "DispatchServiceHeader", new Vector3(25.7f, 2.7f, 10f), 2.4f, 1.1f);
            DoorFrame(shell, new Vector3(25.7f, 0f, 10f), 2.4f, 2.7f, 0f, "CREW FACILITIES");

            Wall(shell, "CrewWestSouth", new Vector3(27f, 0f, 11.45f), 2.9f, HqLayout.WingHeight, 90f);
            Wall(shell, "CrewWestNorth", new Vector3(27f, 0f, 16.55f), 2.9f, HqLayout.WingHeight, 90f);
            Wall(shell, "CrewDoorHeader", new Vector3(27f, 2.7f, 14f), 2.2f, 1.1f, 90f);
            DoorFrame(shell, new Vector3(27f, 0f, 14f), 2.2f, 2.7f, 90f, "LOCKERS / BREAK ROOM");
            Wall(shell, "CrewWorkshopPartition", new Vector3(31.5f, 0f, 18f), 9f, HqLayout.WingHeight);
            Wall(shell, "WorkshopWestSouth", new Vector3(27f, 0f, 19.65f), 3.3f, HqLayout.WingHeight, 90f);
            Wall(shell, "WorkshopWestNorth", new Vector3(27f, 0f, 26.35f), 3.3f, HqLayout.WingHeight, 90f);
            Wall(shell, "WorkshopDoorHeader", new Vector3(27f, 3f, 23f), 3.4f, .8f, 90f);
            DoorFrame(shell, new Vector3(27f, 0f, 23f), 3.4f, 3f, 90f, "MAINTENANCE / EQUIPMENT");
        }

        private static void Frames(Transform shell)
        {
            Transform steelwork = Group("WarehousePortalFrames", shell);
            Material steel = HqIndustrialMaterials.Steel;
            foreach (float z in new[] { 1f, 6f, 11f, 16f, 21f, 27f })
            {
                foreach (float x in new[] { .35f, 23.65f })
                {
                    Box("SteelColumn", steelwork, new Vector3(x, 3.2f, z), new Vector3(.32f, 6.4f, .32f), steel);
                    Box("ColumnBase", steelwork, new Vector3(x, .13f, z), new Vector3(.68f, .26f, .68f), HqIndustrialMaterials.Concrete);
                    Box("ColumnWeb", steelwork, new Vector3(x, 3.2f, z), new Vector3(.44f, 6.4f, .07f), steel, false);
                }
                Bar("LeftRoofRafter", steelwork, new Vector3(.35f, 6.25f, z), new Vector3(12f, 8.02f, z), .25f, .32f, steel);
                Bar("RightRoofRafter", steelwork, new Vector3(12f, 8.02f, z), new Vector3(23.65f, 6.25f, z), .25f, .32f, steel);
                Bar("TrussTie", steelwork, new Vector3(.35f, 6.2f, z), new Vector3(23.65f, 6.2f, z), .16f, .16f, steel);
                for (int i = 0; i < 8; i++)
                {
                    float x = .5f + i * 2.875f;
                    float top = HqLayout.Eave + (1f - Mathf.Abs((x + 1.4f) - 12f) / 12f) * 1.8f - .18f;
                    Bar("TrussDiagonal", steelwork, new Vector3(x, 6.2f, z), new Vector3(x + 2.8f, top, z), .065f, .065f, steel);
                }
            }
            foreach (float x in new[] { 3f, 6f, 9f, 12f, 15f, 18f, 21f })
            {
                float y = HqLayout.Eave + (1f - Mathf.Abs(x - 12f) / 12f) * 1.8f - .05f;
                Box("RoofPurlin", steelwork, new Vector3(x, y, 14f), new Vector3(.11f, .11f, 28f), steel, false);
            }
        }

        private static void Roof(Transform shell)
        {
            Transform roof = Group("Roof", shell);
            float angle = Mathf.Atan2(HqLayout.Ridge - HqLayout.Eave, 12f) * Mathf.Rad2Deg;
            float length = Mathf.Sqrt(12f * 12f + 1.8f * 1.8f) + .3f;
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side < 0f ? 6f : 18f;
                Quaternion slope = Quaternion.Euler(0f, 0f, -side * angle);
                foreach (Vector2 span in new[] { new Vector2(-.25f, 5.3f), new Vector2(6.7f, 13.3f), new Vector2(14.7f, 21.3f), new Vector2(22.7f, 28.25f) })
                {
                    GameObject panel = Box("PitchedMetalRoof", roof, new Vector3(x, 7.3f, (span.x + span.y) * .5f),
                        new Vector3(length, .16f, span.y - span.x), HqIndustrialMaterials.Cladding);
                    HqIndustrialMaterials.Tile(panel, new Vector2(length * .5f, (span.y - span.x) * .5f));
                    panel.transform.localRotation = slope;
                }
                foreach (float z in new[] { 6f, 14f, 22f })
                {
                    GameObject glass = Box("RooflightBand", roof, new Vector3(x, 7.3f, z), new Vector3(length, .065f, 1.4f), HqIndustrialMaterials.Glass);
                    glass.transform.localRotation = slope;
                    foreach (float edge in new[] { -.7f, .7f })
                    {
                        GameObject frame = Box("RooflightFrame", roof, new Vector3(x, 7.34f, z + edge), new Vector3(length, .10f, .07f), HqIndustrialMaterials.Steel, false);
                        frame.transform.localRotation = slope;
                    }
                }
            }
            Box("OperationsWingRoof", roof, new Vector3(30f, HqLayout.WingHeight + .10f, 14f), new Vector3(12.3f, .20f, 28.35f), HqIndustrialMaterials.Steel);
            Box("RidgeCap", roof, new Vector3(12f, 8.22f, 14f), new Vector3(.32f, .13f, 28.5f), HqIndustrialMaterials.Steel, false);
            foreach (float z in new[] { -.11f, 28.11f })
            {
                var gable = new GameObject("GableClosure");
                gable.transform.SetParent(roof, false);
                gable.transform.localPosition = new Vector3(12f, HqLayout.Eave, z);
                gable.AddComponent<MeshFilter>().sharedMesh = GableMesh();
                gable.AddComponent<MeshRenderer>().sharedMaterial = HqIndustrialMaterials.Cladding;
            }
        }

        private static void Wall(Transform parent, string name, Vector3 floor, float length, float height, float yaw = 0f)
        {
            Transform panel = At(name, parent, floor, yaw);
            float baseHeight = floor.y < .01f ? Mathf.Min(1.2f, height) : 0f;
            if (baseHeight > 0f) HqIndustrialMaterials.Tile(Box("MasonryPlinth", panel, Vector3.up * baseHeight * .5f,
                new Vector3(length, baseHeight, .24f), HqIndustrialMaterials.Brick), new Vector2(length / 1.4f, baseHeight / 1.2f));
            if (height > baseHeight)
                HqIndustrialMaterials.Tile(Box("MetalWallPanel", panel, Vector3.up * (baseHeight + (height - baseHeight) * .5f),
                    new Vector3(length, height - baseHeight, .20f), HqIndustrialMaterials.Cladding), new Vector2(length * .5f, (height - baseHeight) * .5f));
        }

        private static void Glazed(Transform parent, string name, Vector3 floor, float length, float height, float yaw = 0f,
            float sill = .9f, float glazing = 1.95f)
        {
            Transform window = At(name, parent, floor, yaw);
            HqIndustrialMaterials.Tile(Box("BrickSill", window, Vector3.up * sill * .5f, new Vector3(length, sill, .24f),
                HqIndustrialMaterials.Brick), new Vector2(length / 1.4f, sill / 1.2f));
            float glassCentre = sill + glazing * .5f, top = sill + glazing;
            Box("GlazedPane", window, Vector3.up * glassCentre, new Vector3(length - .08f, glazing, .065f), HqIndustrialMaterials.Glass);
            if (height > top) Box("WindowHead", window, Vector3.up * (top + (height - top) * .5f),
                new Vector3(length, height - top, .22f), HqIndustrialMaterials.Cladding);
            int panes = Mathf.Max(1, Mathf.CeilToInt(length / 1.5f));
            for (int i = 0; i <= panes; i++)
                Box("WindowMullion", window, new Vector3(-length * .5f + i * length / panes, glassCentre, 0f), new Vector3(.075f, glazing + .07f, .14f), HqIndustrialMaterials.Steel, false);
            foreach (float y in new[] { sill, top })
                Box("WindowRail", window, Vector3.up * y, new Vector3(length, .065f, .14f), HqIndustrialMaterials.Steel, false);
        }

        private static void DoorFrame(Transform parent, Vector3 floor, float width, float height, float yaw, string sign)
        {
            Transform frame = At("Portal_" + sign, parent, floor, yaw);
            foreach (float side in new[] { -1f, 1f })
                Box("DoorJamb", frame, new Vector3(side * (width * .5f + .04f), height * .5f, 0f), new Vector3(.08f, height, .26f), HqIndustrialMaterials.Steel);
            Box("DoorLintel", frame, Vector3.up * (height + .02f), new Vector3(width + .16f, .08f, .26f), HqIndustrialMaterials.Steel, false);
            Box("DepartmentSign", frame, new Vector3(0f, height + .24f, -.15f), new Vector3(width + .16f, .30f, .035f), HqIndustrialMaterials.Steel, false);
            Label(frame, sign, new Vector3(0f, height + .24f, -.173f), .069f, new Color(.80f, .79f, .68f));
        }

        private static Mesh GableMesh()
        {
            PolishAssets.EnsureFolder(HqIndustrialMaterials.Folder);
            string path = HqIndustrialMaterials.Folder + "/WarehouseGable.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            mesh = new Mesh { name = "WarehouseGable" };
            mesh.vertices = new[] { new Vector3(-12.1f, 0f, 0f), new Vector3(0f, 1.8f, 0f), new Vector3(12.1f, 0f, 0f),
                new Vector3(-12.1f, 0f, 0f), new Vector3(12.1f, 0f, 0f), new Vector3(0f, 1.8f, 0f) };
            mesh.uv = new[] { Vector2.zero, new Vector2(.5f, 1f), Vector2.right, Vector2.zero, Vector2.right, new Vector2(.5f, 1f) };
            mesh.triangles = new[] { 0, 1, 2, 3, 4, 5 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
