using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Vacant industrial neighbours and layered rooflines give every HQ opening a world beyond the yard.</summary>
    public static class HqDistrictBuildings
    {
        private static Material brick, cladding, steel, concrete, windows, weathering, distant;

        public static void Build(Transform parent)
        {
            // Loading authored materials first preserves the user's edits to the company building.
            brick = Existing("HQ_Architecture_Brick") ?? HqIndustrialMaterials.Brick;
            cladding = Existing("HQ_Architecture_Cladding") ?? HqIndustrialMaterials.Cladding;
            steel = Existing("HQ_Architecture_Steel") ?? HqIndustrialMaterials.Steel;
            concrete = Existing("HQ_Architecture_Concrete") ?? HqIndustrialMaterials.Concrete;
            windows = Existing("HQ_DistrictVacantWindows") ?? PolishAssets.Material("HQ_DistrictVacantWindows", new Color(.045f, .074f, .077f), .25f);
            weathering = Existing("HQ_DistrictFacadeWeathering") ?? PolishAssets.Material("HQ_DistrictFacadeWeathering", new Color(.19f, .22f, .20f), .96f);
            distant = Existing("HQ_DistrictDistantMass") ?? PolishAssets.Material("HQ_DistrictDistantMass", new Color(.27f, .32f, .33f), .94f);

            Transform district = Group("VacantIndustrialDistrictBuildings", parent);
            district.gameObject.AddComponent<WalkThroughObject>()
                .EditorSetup("Vacant estate buildings are scenery beyond the closed HQ entrances.");
            Transform neighbours = Group("NeighbouringBlocks", district);
            Warehouse(neighbours, new Vector3(14f, 0f, -76f), new Vector3(52f, 11f, 30f), 180f, 0);
            Warehouse(neighbours, new Vector3(-40f, 0f, -82f), new Vector3(30f, 9f, 28f), 180f, 1);
            Office(neighbours, new Vector3(72f, 0f, -80f), new Vector3(32f, 14.5f, 24f), 180f, 2);
            Warehouse(neighbours, new Vector3(-59f, 0f, 14f), new Vector3(46f, 9.5f, 30f), 270f, 3);
            Office(neighbours, new Vector3(86f, 0f, 11f), new Vector3(40f, 15f, 26f), 90f, 4);
            Warehouse(neighbours, new Vector3(13f, 0f, 72f), new Vector3(54f, 12f, 30f), 0f, 5);
            Warehouse(neighbours, new Vector3(87f, 0f, 77f), new Vector3(34f, 13f, 30f), 90f, 6);
            Warehouse(neighbours, new Vector3(-64f, 0f, 80f), new Vector3(36f, 11f, 30f), 270f, 7);

            Transform middle = Group("MiddleDistanceIndustrialRooflines", district);
            Transform horizon = Group("OuterDistrictRooflines", district);
            for (int i = 0; i < 12; i++)
                HorizonBlock(middle, i * 30f + 11f, 125f + i % 3 * 7f, new Vector3(29f + i % 4 * 6f, 10f + i % 4 * 3f, 22f + i % 3 * 4f), i);
            for (int i = 0; i < 16; i++)
                HorizonBlock(horizon, i * 22.5f, 188f + i % 2 * 4f, new Vector3(36f + i % 3 * 6f, 15f + i % 5 * 3.2f, 27f + i % 4 * 3f), i + 12);
            MarkStatic(district);
        }

        private static Material Existing(string name) => AssetDatabase.LoadAssetAtPath<Material>(PolishAssets.Folder + "/" + name + ".mat");

        private static Transform Block(Transform parent, Vector3 floor, float yaw, string name)
        {
            Transform block = Group(name, parent);
            block.SetLocalPositionAndRotation(floor, Quaternion.Euler(0f, yaw, 0f));
            return block;
        }

        private static GameObject Panel(string name, Transform parent, Vector3 centre, Vector3 size, Material material)
            => Box(name, parent, centre, size, material, false);

        private static void Warehouse(Transform parent, Vector3 floor, Vector3 size, float yaw, int variant)
        {
            Transform block = Block(parent, floor, yaw, "VacantWarehouse_" + variant);
            float plinth = variant % 2 == 0 ? 2.2f : 3.1f;
            HqIndustrialMaterials.Tile(Panel("MasonryWarehouseBase", block, Vector3.up * plinth * .5f,
                new Vector3(size.x, plinth, size.z), brick), new Vector2(size.x / 1.4f, plinth / 1.2f));
            HqIndustrialMaterials.Tile(Panel("CladWarehouseMass", block, Vector3.up * (plinth + (size.y - plinth) * .5f),
                new Vector3(size.x, size.y - plinth, size.z), cladding), new Vector2(size.x * .5f, (size.y - plinth) * .5f));
            float ridge = 1.8f + variant % 3 * .4f;
            PitchedRoof(block, size, ridge);
            int bays = Mathf.Max(3, Mathf.FloorToInt(size.x / 8f));
            float spacing = size.x / bays;
            for (int bay = 0; bay < bays; bay++)
            {
                float x = -size.x * .5f + spacing * (bay + .5f);
                LoadingDoor(block, new Vector3(x, 0f, -size.z * .5f), bay % 3 == 0 ? 4.1f : 3.4f, 4.1f);
                Window(block, new Vector3(x, size.y - 2.05f, -size.z * .5f), new Vector2(3.6f, 1.45f));
            }
            Panel("RaisedLoadingApron", block, new Vector3(0f, .34f, -size.z * .5f - 1.18f), new Vector3(size.x + .4f, .68f, 2.4f), concrete);
            for (int rib = 0; rib <= bays; rib++)
                Panel("FacadeVerticalSeam", block, new Vector3(-size.x * .5f + rib * spacing, (size.y + plinth) * .5f, -size.z * .5f - .025f),
                    new Vector3(.075f, size.y - plinth, .07f), steel);
            Transform side = Block(block, new Vector3(-size.x * .5f, 0f, 0f), 90f, "SideServiceElevation");
            for (int i = 0; i < 3; i++)
                Window(side, new Vector3(-size.z * .31f + i * size.z * .31f, size.y - 2.05f, 0f), new Vector2(2.8f, 1.45f));
            RoofPlant(block, size, ridge, variant);
            Weather(block, size, variant);
        }

        private static void Office(Transform parent, Vector3 floor, Vector3 size, float yaw, int variant)
        {
            Transform block = Block(parent, floor, yaw, "VacantIndustrialOffice_" + variant);
            HqIndustrialMaterials.Tile(Panel("BrickOfficeMass", block, Vector3.up * size.y * .5f, size, brick), new Vector2(size.x / 1.4f, size.y / 1.2f));
            Panel("ConcreteGroundCourse", block, new Vector3(0f, .35f, 0f), new Vector3(size.x + .04f, .7f, size.z + .04f), concrete);
            int floors = Mathf.FloorToInt(size.y / 3.3f), bays = Mathf.FloorToInt(size.x / 4.8f);
            for (int storey = 0; storey < floors; storey++)
            {
                float y = 1.8f + storey * 3.3f;
                for (int bay = 0; bay < bays; bay++)
                {
                    if (storey == 0 && (bay <= 1 || bay >= bays - 2)) continue;
                    Window(block, new Vector3(-size.x * .5f + size.x / bays * (bay + .5f), y, -size.z * .5f), new Vector2(2.5f, 1.65f));
                }
                Panel("StoreyStringCourse", block, new Vector3(0f, y + 1.32f, -size.z * .5f - .045f), new Vector3(size.x, .1f, .17f), concrete);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                Transform elevation = Block(block, new Vector3(side * size.x * .5f, 0f, 0f), side < 0f ? 90f : 270f, "OfficeSideElevation");
                for (int storey = 0; storey < floors; storey++)
                for (int bay = 0; bay < 3; bay++)
                    Window(elevation, new Vector3(-size.z * .29f + bay * size.z * .29f, 1.8f + storey * 3.3f, 0f), new Vector2(2.35f, 1.65f));
            }
            LoadingDoor(block, new Vector3(size.x * .35f, 0f, -size.z * .5f), 2.6f, 2.85f);
            Panel("ClosedStaffEntry", block, new Vector3(-size.x * .36f, 1.3f, -size.z * .5f - .08f), new Vector3(1.4f, 2.6f, .10f), steel);
            Panel("EntryCanopy", block, new Vector3(-size.x * .36f, 2.85f, -size.z * .5f - .75f), new Vector3(2.35f, .13f, 1.7f), concrete);
            FlatRoof(block, size);
            RoofPlant(block, size, 0f, variant);
            Weather(block, size, variant);
        }

        private static void Window(Transform parent, Vector3 centre, Vector2 size)
        {
            Transform window = Block(parent, centre, 0f, "RecessedDarkWindow");
            Panel("UnlitGlazing", window, new Vector3(0f, 0f, -.037f), new Vector3(size.x, size.y, .025f), windows);
            foreach (float x in new[] { -size.x * .5f - .065f, 0f, size.x * .5f + .065f })
                Panel("WindowJamb", window, new Vector3(x, 0f, -.115f), new Vector3(.09f, size.y + .18f, .19f), steel);
            Panel("WindowLintel", window, new Vector3(0f, size.y * .5f + .065f, -.12f), new Vector3(size.x + .22f, .10f, .2f), concrete);
            Panel("ProjectingSill", window, new Vector3(0f, -size.y * .5f - .07f, -.17f), new Vector3(size.x + .3f, .11f, .34f), concrete);
        }

        private static void LoadingDoor(Transform parent, Vector3 floor, float width, float height)
        {
            Transform door = Block(parent, floor, 0f, "ClosedLoadingShutter");
            Panel("DarkDoorRecess", door, new Vector3(0f, height * .5f, -.038f), new Vector3(width + .32f, height + .12f, .04f), windows);
            HqIndustrialMaterials.Tile(Panel("ClosedSteelShutter", door, new Vector3(0f, height * .5f, -.081f),
                new Vector3(width, height, .075f), steel), new Vector2(width * .5f, height * .65f));
            foreach (float x in new[] { -width * .5f - .1f, width * .5f + .1f })
                Panel("ShutterGuide", door, new Vector3(x, height * .5f, -.12f), new Vector3(.14f, height + .19f, .24f), concrete);
            Panel("ShutterHeadbox", door, new Vector3(0f, height + .17f, -.19f), new Vector3(width + .38f, .31f, .36f), steel);
            for (int slat = 0; slat < 6; slat++)
                Panel("ShutterHorizontalSeam", door, new Vector3(0f, .30f + slat * (height - .5f) / 6f, -.125f), new Vector3(width - .12f, .028f, .018f), weathering);
        }

        private static void PitchedRoof(Transform block, Vector3 size, float ridge)
        {
            float angle = Mathf.Atan2(ridge, size.x * .5f) * Mathf.Rad2Deg;
            float span = Mathf.Sqrt(size.x * size.x * .25f + ridge * ridge) + .4f;
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject roof = Panel("WarehouseRoofSlope", block, new Vector3(side * size.x * .25f, size.y + ridge * .5f, 0f),
                    new Vector3(span, .16f, size.z + .5f), cladding);
                roof.transform.localRotation = Quaternion.Euler(0f, 0f, -side * angle);
            }
            Panel("RoofRidgeCap", block, new Vector3(0f, size.y + ridge + .07f, 0f), new Vector3(.34f, .14f, size.z + .6f), steel);
            foreach (float z in new[] { -size.z * .5f - .03f, size.z * .5f + .03f })
            {
                var gable = new GameObject("WarehouseEndGable");
                gable.transform.SetParent(block, false);
                gable.transform.localPosition = new Vector3(0f, size.y, z);
                gable.transform.localScale = new Vector3(size.x, ridge * 2f, 1f);
                gable.AddComponent<MeshFilter>().sharedMesh = Gable();
                gable.AddComponent<MeshRenderer>().sharedMaterial = cladding;
            }
        }

        private static void FlatRoof(Transform block, Vector3 size)
        {
            Panel("OfficeFlatRoof", block, new Vector3(0f, size.y + .04f, 0f), new Vector3(size.x + .15f, .17f, size.z + .15f), steel);
            foreach (float x in new[] { -size.x * .5f, size.x * .5f })
                Panel("SideParapet", block, new Vector3(x, size.y + .43f, 0f), new Vector3(.22f, .86f, size.z + .22f), concrete);
            foreach (float z in new[] { -size.z * .5f, size.z * .5f })
                Panel("FrontParapet", block, new Vector3(0f, size.y + .43f, z), new Vector3(size.x + .22f, .86f, .22f), concrete);
        }

        private static void RoofPlant(Transform block, Vector3 size, float ridge, int variant)
        {
            float floor = size.y + Mathf.Max(.14f, ridge * .46f);
            Panel("VacantRoofAirHandler", block, new Vector3(size.x * .26f, floor + .62f, size.z * .14f), new Vector3(3.8f, 1.24f, 2.3f), steel);
            Panel("AirHandlerLouvre", block, new Vector3(size.x * .26f, floor + .62f, size.z * .14f - 1.165f), new Vector3(3.2f, .79f, .035f), windows);
            float chimney = 3.4f + variant % 3 * 1.2f;
            Panel("ColdIndustrialChimney", block, new Vector3(-size.x * .28f, floor + chimney * .5f, size.z * .22f), new Vector3(.88f, chimney, .88f), brick);
            Panel("ChimneyCap", block, new Vector3(-size.x * .28f, floor + chimney + .07f, size.z * .22f), new Vector3(1.08f, .14f, 1.08f), concrete);
        }

        private static void Weather(Transform block, Vector3 size, int variant)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = -size.x * .40f + i * size.x * .25f;
                float length = .9f + (i + variant) % 3 * .53f;
                Panel("FacadeRunoff", block, new Vector3(x, size.y - length * .5f - .2f, -size.z * .5f - .009f),
                    new Vector3(.18f + i % 2 * .11f, length, .014f), weathering);
            }
            Panel("LowerFacadeDampCourse", block, new Vector3(0f, .11f, -size.z * .5f - .013f), new Vector3(size.x, .22f, .015f), weathering);
        }

        private static void HorizonBlock(Transform parent, float degrees, float radius, Vector3 size, int variant)
        {
            float angle = degrees * Mathf.Deg2Rad;
            Transform block = Block(parent, new Vector3(18f + Mathf.Cos(angle) * radius, 0f, 14f + Mathf.Sin(angle) * radius), 90f - degrees, "DistantIndustrialBlock_" + variant);
            Panel("DistantWarehouseMass", block, Vector3.up * size.y * .5f, size, distant);
            Panel("DistantRoofEdge", block, Vector3.up * (size.y + .18f), new Vector3(size.x + .3f, .36f, size.z + .3f), steel);
            Panel("DarkUpperWindowBand", block, new Vector3(0f, size.y - 2.1f, -size.z * .5f - .02f), new Vector3(size.x * .76f, 1.1f, .035f), windows);
            Panel("DistantRoofPlant", block, new Vector3(size.x * .2f, size.y + .85f, 0f), new Vector3(size.x * .21f, 1.7f, size.z * .23f), distant);
            if (variant % 3 == 0)
                Panel("DistantStack", block, new Vector3(-size.x * .27f, size.y + 2.7f, size.z * .17f), new Vector3(1.25f, 5.4f, 1.25f), distant);
        }

        private static Mesh Gable()
        {
            string path = PolishAssets.Folder + "/HQ_DistrictGable.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            PolishAssets.EnsureFolder(PolishAssets.Folder);
            mesh = new Mesh { name = "HQ_DistrictGable" };
            mesh.vertices = new[] { new Vector3(-.5f, 0f, 0f), new Vector3(0f, .5f, 0f), new Vector3(.5f, 0f, 0f),
                new Vector3(-.5f, 0f, 0f), new Vector3(.5f, 0f, 0f), new Vector3(0f, .5f, 0f) };
            mesh.triangles = new[] { 0, 1, 2, 3, 4, 5 };
            mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.right, Vector2.zero, Vector2.right, Vector2.up };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
