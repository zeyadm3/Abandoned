using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>The depot's surrounding estate and wooded horizon, authored without changing its interior mood.</summary>
    public static class HqDistrictLandscape
    {
        public static void Build(Transform parent)
        {
            Transform district = Group("IndustrialEstateLandscape", parent);
            Material earth = DistrictMaterial("HQ_District_Earth", new Color(.22f, .24f, .20f));
            Material asphalt = DistrictMaterial("HQ_District_Asphalt", new Color(.105f, .118f, .12f));
            Material pavement = DistrictMaterial("HQ_District_Pavement", new Color(.29f, .31f, .30f));
            Material paint = DistrictMaterial("HQ_District_RoadPaint", new Color(.55f, .53f, .39f));
            Material bark = DistrictMaterial("HQ_District_Bark", new Color(.22f, .18f, .13f));
            Material foliage = DistrictMaterial("HQ_District_Leaves", new Color(.19f, .26f, .18f));
            Material faded = DistrictMaterial("HQ_District_FadedLeaves", new Color(.25f, .30f, .20f));
            Material steel = DistrictMaterial("HQ_District_PoleSteel", new Color(.20f, .23f, .23f), .65f);
            Material cable = DistrictMaterial("HQ_District_Cable", new Color(.025f, .03f, .032f));
            Material lamp = DistrictMaterial("HQ_District_LampGlass", new Color(.72f, .72f, .52f), emission: .9f);

            // One continuous support surface closes the visible world beneath the existing raised apron.
            Box("DistrictGround", district, new Vector3(18f, -.57f, -8f), new Vector3(640f, .20f, 640f), earth);
            Transform scenery = Group("EstateScenery", district);
            scenery.gameObject.AddComponent<WalkThroughObject>()
                .EditorSetup("Background district scenery lies beyond the HQ access boundary.");
            Roads(scenery, asphalt, pavement, paint);
            Utilities(scenery, bark, steel, cable, lamp);
            Vegetation(scenery, new[] { bark, foliage, faded }, earth);
            foreach (Transform item in district.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(item.gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }

        private static Material DistrictMaterial(string name, Color color, float roughness = .8f, float emission = 0f)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>($"{PolishAssets.Folder}/{name}.mat");
            return existing != null ? existing : PolishAssets.Material(name, color, roughness, emission: emission);
        }

        private static void Roads(Transform parent, Material asphalt, Material pavement, Material paint)
        {
            Transform roads = Group("EstateRoads", parent);
            Box("EstateStreet", roads, new Vector3(18f, -.42f, -42f), new Vector3(540f, .10f, 11.5f), asphalt, false);
            Box("ServiceLane", roads, new Vector3(54f, -.42f, 53f), new Vector3(7f, .10f, 190f), asphalt, false);
            Box("ApronConnection", roads, new Vector3(18f, -.42f, -33f), new Vector3(18f, .10f, 8f), asphalt, false);
            Box("StreetSouthWalk", roads, new Vector3(18f, -.35f, -49.7f), new Vector3(540f, .18f, 3.8f), pavement, false);
            Box("StreetSouthKerb", roads, new Vector3(18f, -.31f, -47.92f), new Vector3(540f, .25f, .24f), pavement, false);
            NorthWalk(-252f, 9f);
            NorthWalk(27f, 49.5f);
            NorthWalk(58.5f, 288f);
            Box("ServiceWalkEast", roads, new Vector3(59f, -.35f, 52.5f), new Vector3(3f, .18f, 177f), pavement, false);
            Box("ServiceKerbEast", roads, new Vector3(57.62f, -.31f, 52.5f), new Vector3(.24f, .25f, 177f), pavement, false);
            for (float x = -236f; x < 280f; x += 10f)
                Box("FadedLaneDash", roads, new Vector3(x, -.365f, -42f), new Vector3(3.1f, .007f, .12f), paint, false);
            foreach (float x in new[] { 9.15f, 26.85f })
                Box("DrivewayEdge", roads, new Vector3(x, -.365f, -33f), new Vector3(.10f, .007f, 7.5f), paint, false);

            void NorthWalk(float from, float to)
            {
                float centre = (from + to) * .5f, length = to - from;
                Box("StreetNorthWalk", roads, new Vector3(centre, -.35f, -34.3f), new Vector3(length, .18f, 3.8f), pavement, false);
                Box("StreetNorthKerb", roads, new Vector3(centre, -.31f, -36.08f), new Vector3(length, .25f, .24f), pavement, false);
            }
        }

        private static void Utilities(Transform parent, Material wood, Material steel, Material cable, Material glow)
        {
            Transform utilities = Group("EstateUtilities", parent);
            Vector3 previous = Vector3.zero;
            for (int i = 0; i < 7; i++)
            {
                Vector3 at = new(-130f + i * 60f, -.45f, -53.5f);
                Pole(at, 0f);
                if (i > 0) WireSpan("StreetSpan_" + i, previous, at, Vector3.forward);
                previous = at;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 at = new(61f, -.45f, 8f + i * 60f);
                Pole(at, 90f);
                if (i > 0) WireSpan("ServiceSpan_" + i, previous, at, Vector3.right);
                previous = at;
            }
            for (int i = 0; i < 6; i++)
                StreetLamp(new Vector3(-18f + i * 48f, -.45f, -50.8f), 0f, i < 3);
            StreetLamp(new Vector3(58.8f, -.45f, 36f), 90f, true);
            StreetLamp(new Vector3(58.8f, -.45f, 96f), 90f, false);

            void Pole(Vector3 at, float yaw)
            {
                Transform pole = Group("UtilityPole", utilities);
                pole.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                Primitive(PrimitiveType.Cylinder, "TimberPole", pole, Vector3.up * 4f, new Vector3(.24f, 4f, .24f), wood, false);
                Box("CrossArm", pole, new Vector3(0f, 7.7f, 0f), new Vector3(.15f, .16f, 1.7f), steel, false);
                foreach (float z in new[] { -.60f, 0f, .60f })
                    Primitive(PrimitiveType.Cylinder, "Insulator", pole, new Vector3(0f, 7.88f, z), new Vector3(.10f, .11f, .10f), steel, false);
            }

            void WireSpan(string name, Vector3 from, Vector3 to, Vector3 cross)
            {
                for (int wire = -1; wire <= 1; wire++)
                {
                    Vector3 start = from + Vector3.up * 8f + cross * (wire * .60f);
                    Vector3 end = to + Vector3.up * 8f + cross * (wire * .60f);
                    Mesh cableMesh = HqDistrictLandscapeMeshes.Cable(name + "_" + wire, end - start);
                    MeshObject(name + "_" + wire, utilities, cableMesh, new[] { cable }, start, Quaternion.identity, Vector3.one);
                }
            }

            void StreetLamp(Vector3 at, float yaw, bool lit)
            {
                Transform holder = Group("StreetLamp", utilities);
                holder.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                Primitive(PrimitiveType.Cylinder, "SteelColumn", holder, Vector3.up * 3.9f, new Vector3(.16f, 3.9f, .16f), steel, false);
                Box("LampArm", holder, new Vector3(0f, 7.70f, 1f), new Vector3(.095f, .095f, 2f), steel, false);
                Box("LampHood", holder, new Vector3(0f, 7.65f, 2f), new Vector3(.42f, .14f, .75f), steel, false);
                Box("LampDiffuser", holder, new Vector3(0f, 7.565f, 2f), new Vector3(.34f, .018f, .63f), lit ? glow : steel, false);
                if (!lit) return;
                var light = new GameObject("StreetPool").AddComponent<Light>();
                light.transform.SetParent(holder, false);
                light.transform.SetLocalPositionAndRotation(new Vector3(0f, 7.51f, 2f), Quaternion.Euler(90f, 0f, 0f));
                light.type = LightType.Spot;
                light.color = new Color(.90f, .91f, .77f);
                light.intensity = 24f;
                light.range = 17f;
                light.spotAngle = 88f;
                light.innerSpotAngle = 58f;
                light.shadows = LightShadows.None;
                LevelStoreys.LightEverything(light);
                foreach (Renderer renderer in holder.GetComponentsInChildren<Renderer>())
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void Vegetation(Transform parent, Material[] trees, Material earth)
        {
            Transform belts = Group("TreeBeltsAndHorizon", parent);
            Patch("WestSiteTrees", new Rect(-27f, 2f, 10f, 37f), 12);
            Patch("EastServiceTrees", new Rect(62.5f, -9f, 2.5f, 52f), 10);
            Patch("RearBoundaryTrees", new Rect(-24f, 37f, 88f, 7f), 19);
            Patch("WestStreetVerge", new Rect(-122f, -31.5f, 96f, 8f), 17);
            Patch("EastStreetVerge", new Rect(73f, -31.5f, 105f, 8f), 17);
            Patch("SouthStreetTrees", new Rect(-126f, -59f, 132f, 4f), 15);
            Patch("SouthStreetTreesEast", new Rect(61f, -59f, 119f, 4f), 15);
            Patch("WestScrub", new Rect(-28f, -18f, 12f, 15f), 20, true);
            Patch("ServiceScrub", new Rect(62f, 6f, 4f, 29f), 18, true);
            Patch("RearScrub", new Rect(-12f, 41f, 48f, 4f), 22, true);

            Mesh berm = HqDistrictLandscapeMeshes.Berm();
            for (int sector = 0; sector < 12; sector++)
            {
                float angle = sector * 30f, radians = angle * Mathf.Deg2Rad;
                Vector3 at = new(18f + Mathf.Sin(radians) * 282f, -.49f, -8f + Mathf.Cos(radians) * 282f);
                float width = 168f, depth = 88f, height = 15f + sector % 4 * 3.4f;
                Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
                MeshObject("WoodedBerm_" + sector, belts, berm, new[] { earth }, at, rotation, new Vector3(width, height, depth));
                Mesh woodland = HqDistrictLandscapeMeshes.Plants("HorizonTrees_" + sector,
                    new Rect(-68f, -12f, 136f, 24f), 23, false, new Vector3(width, height, depth));
                MeshObject("HorizonTrees_" + sector, belts, woodland, trees, at, rotation, Vector3.one);
            }

            void Patch(string name, Rect area, int count, bool shrubs = false)
            {
                Mesh mesh = HqDistrictLandscapeMeshes.Plants(name, area, count, shrubs, Vector3.zero);
                MeshObject(name, belts, mesh, trees, new Vector3(0f, -.46f, 0f), Quaternion.identity, Vector3.one);
            }
        }

        private static void MeshObject(string name, Transform parent, Mesh mesh, Material[] materials,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Transform item = Group(name, parent);
            item.SetLocalPositionAndRotation(position, rotation);
            item.localScale = scale;
            item.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = item.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
