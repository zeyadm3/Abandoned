using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>A coherent arrival car park: two parking banks flank one pedestrian spine and one collection point.</summary>
    public static class MeridianArrivalCourt
    {
        private const string Folder = "Assets/_Project/Art/Generated/MeridianArrival";
        private static Material paint, paving, curb, dark, soil, trunk, leaves, leavesDark, glow;

        public static void Build(Transform parking)
        {
            Prepare();
            Ground(parking);
            Promenade(Group("MainEntrancePromenade", parking));
            Bays(Group("OrderedParkingBanks", parking));
            Planting(Group("LandscapeIslands", parking));
            VehiclesAndCrew(Group("ArrivalAndAbandonedCars", parking));
            Lighting(Group("CarParkLighting", parking));
            MarkStatic(parking);
        }

        private static void Prepare()
        {
            paint = Material("WornIvoryPaint", "Ceiling", new Color(.82f, .79f, .66f));
            paving = Material("ForecourtPavers", "FacadeStone", new Color(.69f, .65f, .55f));
            curb = Material("CarParkCurbs", "Concrete", new Color(.61f, .59f, .52f));
            dark = Material("ParkingSteel", "FixtureTrim", new Color(.16f, .18f, .17f));
            soil = Material("IslandSoil", "Concrete", new Color(.19f, .18f, .14f));
            trunk = Material("ParkingTreeBark", "Wood", new Color(.29f, .26f, .20f));
            leaves = Material("ParkingTreeFoliage", "Leaves", new Color(.27f, .32f, .20f));
            leavesDark = Material("ParkingTreeDarkFoliage", "Leaves", new Color(.18f, .23f, .15f));
            glow = Material("ParkingLampDiffuser", "FixtureGlow", new Color(.93f, .89f, .70f));
            glow.SetColor("_EmissionColor", new Color(1.25f, 1.09f, .69f));
            glow.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(glow);
        }

        private static void Ground(Transform parent)
        {
            Material asphalt = Material("CarParkAsphalt", "Asphalt", new Color(.89f, .90f, .86f));
            asphalt.SetTextureScale("_BaseMap", new Vector2(24f, 10f));
            GameObject slab = Box("ArrivalLot", parent, new Vector3(34f, -.2f, -19.75f),
                new Vector3(96f, .4f, 39.5f), asphalt);
            slab.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            foreach (float x in new[] { -13.4f, 81.4f })
                Box("OuterParkingCurb", parent, new Vector3(x, .09f, -20f), new Vector3(.24f, .18f, 39f), curb);
            Box("SouthParkingCurb", parent, new Vector3(34f, .09f, -38.8f), new Vector3(95f, .18f, .24f), curb);
        }

        private static void Promenade(Transform parent)
        {
            // Both vehicles unload into this wide uninterrupted path; the mall has no alternate door.
            Box("PedestrianSpine", parent, new Vector3(28f, .008f, -20f), new Vector3(8f, .016f, 39.8f), paving, false);
            Box("EntryPlaza", parent, new Vector3(28f, .009f, -4f), new Vector3(26f, .018f, 8f), paving, false);
            Material joint = Material("PaverJoints", "FixtureTrim", new Color(.26f, .27f, .25f));
            for (int strip = 0; strip < 20; strip++)
                Stripe("PromenadePaverJoint", parent, new Vector3(28f, .018f, -1f - strip * 2f),
                    new Vector3(8f, .003f, .025f), joint);
            foreach (float x in new[] { 24.02f, 31.98f })
                Stripe("PedestrianSpineBorder", parent, new Vector3(x, .018f, -20f), new Vector3(.10f, .003f, 39.5f), joint);
            for (int strip = 0; strip < 10; strip++)
                Stripe("ForecourtPaverJoint", parent, new Vector3(16.7f + strip * 2.5f, .02f, -4f),
                    new Vector3(.025f, .003f, 7.8f), joint);
            foreach (float x in new[] { 22f, 34f })
            {
                Bollard(parent, new Vector3(x, 0f, -7.8f));
                Bollard(parent, new Vector3(x, 0f, -1.3f));
            }
            // The open plaza joins the truck ramp at x36 without posts or raised kerbs in its haul lane.
            for (int stripe = 0; stripe < 7; stripe++)
                Stripe("VehicleAislePedestrianCrossing", parent, new Vector3(28f, .025f, -20.4f - stripe * .9f),
                    new Vector3(7.4f, .007f, .37f), paint);
        }

        private static void Bays(Transform parent)
        {
            foreach (float row in new[] { -14.8f, -31.2f })
            {
                Bank(-10f, 8, row);
                Bank(44.5f, 12, row);
            }
            // These adjacent service spaces read as one crew staging area, not a distant loading dock.
            ServiceSpace("CrewArrivalSpace", new Vector3(19f, .008f, -15f), 4f, 9f);
            ServiceSpace("CompanyCollectionSpace", new Vector3(36f, .008f, -15f), 6.2f, 14f);
            Arrow(parent, new Vector3(1f, .009f, -23f), 90f);
            Arrow(parent, new Vector3(59f, .009f, -23f), -90f);
            foreach (float x in new[] { 15.1f, 41.3f })
                for (int dash = 0; dash < 7; dash++)
                    Stripe("DriveAisleEdge", parent, new Vector3(x, .011f, -20.5f - dash * .8f), new Vector3(.09f, .007f, .45f), paint);

            void Bank(float start, int count, float z)
            {
                for (int i = 0; i < count; i++)
                {
                    float x = start + i * 3f;
                    Transform bay = Group($"ParkingBay_{z}_{i}_{start}", parent);
                    foreach (float side in new[] { -1f, 1f })
                        Stripe("BaySideStripe", bay, new Vector3(x + side * 1.46f, .008f, z), new Vector3(.075f, .007f, 5.9f), paint);
                    Stripe("BayBackStripe", bay, new Vector3(x, .008f, z - 2.95f), new Vector3(2.96f, .007f, .075f), paint);
                    Box("ParkingWheelStop", bay, new Vector3(x, .075f, z + 2.35f), new Vector3(1.65f, .15f, .25f), curb);
                }
            }
            void ServiceSpace(string name, Vector3 at, float width, float length)
            {
                foreach (float side in new[] { -1f, 1f })
                    Stripe(name + "Edge", parent, at + Vector3.right * (side * width / 2f), new Vector3(.10f, .007f, length), paint);
                Stripe(name + "Rear", parent, at + Vector3.back * length / 2f, new Vector3(width, .007f, .10f), paint);
            }
        }

        private static void Planting(Transform parent)
        {
            Island(new Vector3(2f, 0f, -6f), new Vector2(24f, 3.3f), 4);
            Island(new Vector3(61f, 0f, -6f), new Vector2(34f, 3.3f), 5);
            Island(new Vector3(-8f, 0f, -37f), new Vector2(10f, 2.6f), 2);
            Island(new Vector3(58f, 0f, -37f), new Vector2(35f, 2.6f), 5);
            foreach (float x in new[] { 12.9f, 43.1f }) Bench(parent, new Vector3(x, 0f, -2.3f));

            void Island(Vector3 at, Vector2 size, int trees)
            {
                Transform island = Group("CurbedTreeIsland", parent);
                island.localPosition = at;
                Box("RaisedPlantingBed", island, new Vector3(0f, .12f, 0f), new Vector3(size.x, .24f, size.y), soil);
                foreach (float side in new[] { -1f, 1f })
                {
                    Box("PlanterLongKerb", island, new Vector3(0f, .15f, side * size.y / 2f), new Vector3(size.x + .22f, .30f, .22f), curb);
                    Box("PlanterEndKerb", island, new Vector3(side * size.x / 2f, .15f, 0f), new Vector3(.22f, .30f, size.y), curb);
                }
                Mesh mesh = HqDistrictLandscapeMeshes.Plants($"MallArrivalTrees_{size.x}_{trees}",
                    new Rect(-size.x * .40f, -.25f, size.x * .8f, .50f), trees, false, Vector3.zero);
                Transform canopy = Group("ParkingIslandTrees", island);
                canopy.localPosition = Vector3.up * .24f;
                canopy.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                canopy.gameObject.AddComponent<MeshRenderer>().sharedMaterials = new[] { trunk, leaves, leavesDark };
                for (int tree = 0; tree < trees; tree++)
                    MeridianArt.Place("Overgrowth", island, new Vector3((tree + .5f) / trees * size.x - size.x / 2f, .25f, .4f),
                        tree * 31f, new Vector3(.6f, .65f, .6f));
            }
        }

        private static void VehiclesAndCrew(Transform parent)
        {
            Vehicle("CrewArrivalVan", Vehicles.Van, MallLayout.ArrivalVehicle, 0f, new Vector3(2.3f, 2.45f, 5.7f));
            Vehicle("ForgottenTenantDelivery", Vehicles.Delivery, new Vector3(-7f, 0f, -14.8f), 0f, new Vector3(2.1f, 2.35f, 5.2f));
            Vehicle("DustyTenantVan", Vehicles.Van, new Vector3(5f, 0f, -31.2f), 180f, new Vector3(2.1f, 2.3f, 5.2f));
            Vehicle("AbandonedStoreDelivery", Vehicles.Delivery, new Vector3(62.5f, 0f, -31.2f), 180f, new Vector3(2.15f, 2.4f, 5.2f));
            Vehicle("OldNightGuardVan", Vehicles.Van, new Vector3(74.5f, 0f, -14.8f), 0f, new Vector3(2.1f, 2.3f, 5.2f));

            void Vehicle(string name, string model, Vector3 at, float yaw, Vector3 size)
            {
                Transform holder = Group(name, parent);
                GameObject proxy = Vehicles.Park(holder, model, at, yaw, size);
                proxy.name = "SolidParkedVehicle";
            }
        }

        private static void Lighting(Transform parent)
        {
            MeridianPresentationConfig config = MeridianPresentationConfigBuilder.Load();
            Vector3[] positions = { new(-1f, 0f, -6f), new(52f, 0f, -6f), new(-8f, 0f, -37f), new(60f, 0f, -37f) };
            foreach (Vector3 at in positions)
            {
                Transform pole = Group("ParkingDoubleLampPole", parent);
                pole.localPosition = at;
                Primitive(PrimitiveType.Cylinder, "SteelLightPole", pole, new Vector3(0f, 3.5f, 0f), new Vector3(.18f, 3.5f, .18f), dark);
                Box("LightPoleConcreteFoot", pole, new Vector3(0f, .12f, 0f), new Vector3(.42f, .24f, .42f), curb);
                Box("DoubleLampCrossArm", pole, new Vector3(0f, 6.90f, 0f), new Vector3(3.1f, .13f, .12f), dark, false);
                foreach (float side in new[] { -1f, 1f })
                {
                    Box("ParkingLampHousing", pole, new Vector3(side * 1.35f, 6.83f, 0f), new Vector3(.78f, .18f, 1.05f), dark, false);
                    Box("ParkingLampDiffuser", pole, new Vector3(side * 1.35f, 6.72f, 0f), new Vector3(.66f, .04f, .92f), glow, false);
                }
                Light light = Group("CarParkLightPool", pole).gameObject.AddComponent<Light>();
                light.transform.localPosition = new Vector3(0f, 6.70f, 0f);
                light.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                light.type = LightType.Spot; light.spotAngle = 125f; light.innerSpotAngle = 95f;
                light.intensity = config.ParkingLampIntensity; light.range = config.ParkingLampRange;
                light.color = config.ParkingLampColor; light.shadows = LightShadows.None;
                light.gameObject.AddComponent<ShadowBudgetLight>();
            }
        }

        private static void Bench(Transform parent, Vector3 at)
        {
            Transform bench = Group("EntryCourtBench", parent);
            bench.localPosition = at;
            foreach (float x in new[] { -1.1f, 1.1f })
                Box("BenchSupport", bench, new Vector3(x, .23f, 0f), new Vector3(.10f, .46f, .64f), dark);
            for (int slat = 0; slat < 5; slat++)
                Box("BenchSeatSlat", bench, new Vector3(0f, .48f, -.25f + slat * .125f), new Vector3(2.8f, .07f, .095f), MeridianRetailMaterials.Wood);
            foreach (float y in new[] { .72f, .93f })
                Box("BenchBackSlat", bench, new Vector3(0f, y, .34f), new Vector3(2.8f, .15f, .065f), MeridianRetailMaterials.Wood);
            foreach (float x in new[] { -1.1f, 1.1f })
                Box("BenchBackSupport", bench, new Vector3(x, .69f, .34f), new Vector3(.07f, .67f, .07f), dark);
        }

        private static void Bollard(Transform parent, Vector3 at)
        {
            Primitive(PrimitiveType.Cylinder, "EntranceBollard", parent, at + Vector3.up * .42f, new Vector3(.18f, .42f, .18f), dark);
            Primitive(PrimitiveType.Cylinder, "BollardReflector", parent, at + Vector3.up * .76f, new Vector3(.19f, .035f, .19f), paint, false);
        }

        private static void Arrow(Transform parent, Vector3 at, float yaw)
        {
            Transform arrow = Group("DriveAisleArrow", parent);
            arrow.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Stripe("ArrowStem", arrow, new Vector3(0f, 0f, -.35f), new Vector3(.16f, .007f, 1.7f), paint);
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject arm = Stripe("ArrowHead", arrow, new Vector3(side * .27f, 0f, .55f), new Vector3(.16f, .007f, .8f), paint);
                arm.transform.localRotation = Quaternion.Euler(0f, side * -45f, 0f);
            }
        }

        private static GameObject Stripe(string name, Transform parent, Vector3 at, Vector3 size, Material material)
        {
            GameObject stripe = Box(name, parent, at, size, material, false);
            stripe.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return stripe;
        }

        private static Material Material(string name, string source, Color color)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(MeridianArt.Material(source)) { name = name, enableInstancing = true };
            material.SetColor("_BaseColor", color);
            material.SetColor("_EmissionColor", Color.black); material.DisableKeyword("_EMISSION");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
