using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Authored arrival court, abandoned parking and independent street lighting.</summary>
    public static class MeridianParkingBuilder
    {
        private const string MaterialFolder = "Assets/_Project/Art/Generated/MeridianParking";
        public static Vector3 ArrivalVanPosition => MallLayout.ArrivalVehicle;
        public static Vector3 ArrivalCrewPoint => ArrivalVanPosition + new Vector3(2.6f, .05f, 1.2f);
        private static Material paint, crewPaint, glow;

        public static void Build(Transform parking)
        {
            MeridianPresentationConfig config = MeridianPresentationConfigBuilder.Load();
            paint = Material("FadedBayPaint", "Ceiling", new Color(.96f, .92f, .72f));
            crewPaint = Material("CrewTeal", "StorefrontTrim", new Color(.16f, .44f, .42f));
            glow = Material("StreetLampGlow", "FixtureGlow", Color.white);
            glow.SetColor("_EmissionColor", config.ParkingLampColor * 2f);
            glow.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(glow);
            // End asphalt at the front wall plane so stone court/setback slabs do not fight its surface.
            Ground(parking, "ArrivalLot", new Vector3(34f, -.2f, -18f), new Vector3(92f, .4f, 36f), new Vector2(23f, 9f));
            Ground(parking, "UtilityRoad", new Vector3(68f, -.2f, 24f), new Vector3(24f, .4f, 48f), new Vector2(6f, 12f));
            Bays(Group("ParkingBays", parking));
            Islands(Group("CurbedIslands", parking));
            Arrival(Group("CrewArrival", parking));
            AbandonedVehicles(Group("EarlierVehicles", parking));
            Markings(Group("ArrivalAndServiceDirections", parking));
            StreetLights(Group("IndependentStreetFeed", parking), config);
            // All decorations are non-blocking; only the visible asphalt carries solid collision.
            MarkStatic(parking);
        }

        private static void Ground(Transform parent, string name, Vector3 at, Vector3 size, Vector2 tiling)
        {
            Material asphalt = Material(name + "Asphalt", "Asphalt", Color.white);
            asphalt.SetTextureScale("_BaseMap", tiling);
            GeneratedMaterialRepair.Apply(asphalt);
            GameObject slab = Box(name, parent, at, size, asphalt);
            slab.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
        }

        private static void Bays(Transform parent)
        {
            foreach (float row in new[] { -29f, -17f })
            for (int index = 0; index < 23; index++)
            {
                float x = -1.2f + index * 3.2f;
                // The entrance/crew approach remains an eight-metre pedestrian gap through both rows.
                if (x > 22f && x < 33f) continue;
                Transform bay = Group($"Bay_{row}_{index}", parent);
                Paint("LeftLine", bay, new Vector3(x - 1.53f, .007f, row), new Vector3(.075f, .007f, 5.8f));
                Paint("RightLine", bay, new Vector3(x + 1.53f, .007f, row), new Vector3(.075f, .007f, 5.8f));
                Paint("RearLine", bay, new Vector3(x, .007f, row - 2.9f), new Vector3(3.1f, .007f, .075f));
                Box("WheelStop", bay, new Vector3(x, .09f, row + 2.5f), new Vector3(1.65f, .18f, .25f), MeridianArt.Material("Concrete"), false);
            }
            Arrow(parent, new Vector3(8f, .012f, -23f), Vector3.right);
            Arrow(parent, new Vector3(44f, .012f, -23f), Vector3.right);
            Arrow(parent, new Vector3(72f, .012f, -23f), Vector3.forward);
        }

        private static void Islands(Transform parent)
        {
            Island(parent, new Vector3(6f, 0f, -3.6f), new Vector2(11f, 3f));
            Island(parent, new Vector3(48f, 0f, -3.6f), new Vector2(10f, 3f));
            Island(parent, new Vector3(-5.7f, 0f, -22f), new Vector2(3f, 12f));
            Box("SouthBoundaryCurb", parent, new Vector3(36f, .10f, -34.8f), new Vector3(88f, .20f, .24f), MeridianArt.Material("FacadeStone"), false);
        }

        private static void Island(Transform parent, Vector3 at, Vector2 size)
        {
            Transform island = Group("NeglectedLandscapeIsland", parent);
            island.localPosition = at;
            Box("RaisedSoil", island, new Vector3(0f, .10f, 0f), new Vector3(size.x, .20f, size.y), MeridianArt.Material("Moss"), false);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("KerbLong", island, new Vector3(0f, .12f, side * size.y / 2f), new Vector3(size.x + .24f, .24f, .24f), MeridianArt.Material("FacadeStone"), false);
                Box("KerbEnd", island, new Vector3(side * size.x / 2f, .12f, 0f), new Vector3(.24f, .24f, size.y), MeridianArt.Material("FacadeStone"), false);
            }
            for (int i = 0; i < 3; i++)
                MeridianArt.Place("Overgrowth", island, new Vector3((i - 1) * size.x * .27f, .21f, .15f), i * 71f,
                    new Vector3(.75f, .70f, .75f));
        }

        private static void Arrival(Transform parent)
        {
            Vehicle(parent, "CrewArrivalVan", Vehicles.Van, ArrivalVanPosition, 0f, new Vector3(2.3f, 2.45f, 5.7f));
            Paint("CrewWaitingLine", parent, new Vector3(21.5f, .008f, -16.4f), new Vector3(.10f, .007f, 3.5f));
        }

        private static void AbandonedVehicles(Transform parent)
        {
            Vehicle(parent, "DustyDeliveryVan", Vehicles.Delivery, new Vector3(8.4f, 0f, -29f), 1f, new Vector3(2.1f, 2.55f, 5.3f));
            Vehicle(parent, "OldTenantVan", Vehicles.Van, new Vector3(46.8f, 0f, -17f), -3f, new Vector3(2.15f, 2.3f, 5.2f));
            Vehicle(parent, "AbandonedBoxTruck", Vehicles.Truck, new Vector3(62.8f, 0f, -29f), 5f, new Vector3(2.35f, 2.8f, 5.8f));
        }

        private static Transform Vehicle(Transform parent, string name, string model, Vector3 at, float yaw, Vector3 size)
        {
            Transform holder = Group(name, parent);
            holder.localPosition = at;
            GameObject proxy = Vehicles.Park(holder, model, holder.position, yaw, size);
            foreach (Collider collider in holder.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            proxy.name = "NonBlockingVehiclePlacement";
            return holder;
        }

        private static void Markings(Transform parent)
        {
            // Keep both the actual dock approach x56..62/z31.8..36.2 and cab envelope free.
            for (int i = 0; i < 8; i++)
                Paint("EntranceCrossing", parent, new Vector3(28f, .008f, -14f + i), new Vector3(6f, .007f, .40f));
            Arrow(parent, new Vector3(28f, .012f, -4f), Vector3.forward);
            Arrow(parent, new Vector3(43f, .012f, -10f), Vector3.right);
            Arrow(parent, new Vector3(66f, .012f, -10f), Vector3.right);
            foreach (float z in new[] { 5f, 21f, 42f }) Arrow(parent, new Vector3(73f, .012f, z), Vector3.forward);
            foreach (float z in new[] { 0f, 8f, 16f, 24f })
                Paint("ServiceLaneEdge", parent, new Vector3(76.2f, .008f, z + 2f), new Vector3(.11f, .007f, 2.8f));
            Sign(parent, new Vector3(52f, 1.5f, -5.4f), Vector3.back, "COLLECTION >", 2.2f, .30f, crewPaint);
            Box("DirectionsPost", parent, new Vector3(52f, .73f, -5.4f), new Vector3(.09f, 1.46f, .09f), MeridianArt.Material("FixtureTrim"), false);
        }

        private static void Arrow(Transform parent, Vector3 at, Vector3 forward)
        {
            Transform arrow = Group("FadedDriveArrow", parent);
            arrow.SetLocalPositionAndRotation(at, Quaternion.LookRotation(forward));
            Paint("Shaft", arrow, new Vector3(0f, 0f, -.4f), new Vector3(.20f, .007f, 2f));
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject head = Paint("ArrowHead", arrow, new Vector3(side * .38f, 0f, .56f), new Vector3(.20f, .007f, 1.15f));
                head.transform.localRotation = Quaternion.Euler(0f, side * -45f, 0f);
            }
        }

        private static void StreetLights(Transform parent, MeridianPresentationConfig config)
        {
            Vector3[] poles = { new(14f, 0f, -8f), new(38f, 0f, -5f), new(51f, 0f, -29f), new(78f, 0f, 14f), new(78f, 0f, 42f) };
            for (int i = 0; i < poles.Length; i++)
            {
                Transform pole = Group("ParkingUtilityPole_" + i, parent);
                pole.localPosition = poles[i];
                Primitive(PrimitiveType.Cylinder, "SteelPole", pole, new Vector3(0f, 2.7f, 0f), new Vector3(.16f, 2.7f, .16f), MeridianArt.Material("FixtureTrim"), false);
                Box("Footplate", pole, new Vector3(0f, .035f, 0f), new Vector3(.36f, .07f, .36f), MeridianArt.Material("EscalatorSteel"), false);
                Box("LampArm", pole, new Vector3(0f, 5.35f, .48f), new Vector3(.11f, .12f, 1.1f), MeridianArt.Material("FixtureTrim"), false);
                Box("LampHousing", pole, new Vector3(0f, 5.28f, 1.04f), new Vector3(.52f, .14f, .85f), MeridianArt.Material("FixtureTrim"), false);
                Box("StreetFeedDiffuser", pole, new Vector3(0f, 5.20f, 1.04f), new Vector3(.43f, .025f, .72f), glow, false);
                Light lamp = Group("ParkingUtilityLamp_" + i, pole).gameObject.AddComponent<Light>();
                lamp.transform.localPosition = new Vector3(0f, 5.12f, 1.04f);
                lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                lamp.type = LightType.Spot; lamp.spotAngle = 115f; lamp.innerSpotAngle = 85f;
                lamp.intensity = config.ParkingLampIntensity; lamp.range = config.ParkingLampRange;
                lamp.color = config.ParkingLampColor; lamp.shadows = LightShadows.None;
                // In the shadow budget with the ceiling lights: the lamps nearest you throw the van's and the crew's shadows.
                lamp.gameObject.AddComponent<Abandoned.Structure.ShadowBudgetLight>();
            }
        }

        private static GameObject Paint(string name, Transform parent, Vector3 at, Vector3 size)
        {
            GameObject stripe = Box(name, parent, at, size, paint, false);
            stripe.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return stripe;
        }

        private static void Sign(Transform parent, Vector3 at, Vector3 front, string text, float width, float height, Material face)
        {
            MeridianDressingSigns.Sign(parent, at, front, text, width, height);
            Transform sign = parent.GetChild(parent.childCount - 1);
            sign.Find("Fascia").GetComponent<Renderer>().sharedMaterial = face;
            sign.GetComponentInChildren<TextMesh>().color = new Color(.91f, .92f, .78f);
        }

        private static Material Material(string name, string source, Color color)
        {
            PolishAssets.EnsureFolder(MaterialFolder);
            string path = $"{MaterialFolder}/{name}.mat";
            Material result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null) { result = new Material(MeridianArt.Material(source)); AssetDatabase.CreateAsset(result, path); }
            result.CopyPropertiesFromMaterial(MeridianArt.Material(source));
            result.SetColor("_BaseColor", color); result.name = name; result.enableInstancing = true;
            GeneratedMaterialRepair.Apply(result);
            return result;
        }
    }
}
