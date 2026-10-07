using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Scenes/Mall.unity from <see cref="MallLayout"/>: every floor tile is its own unscaled root
    /// (pivot on the walking surface, scaled Visual child) so each becomes a StructuralSection; flights,
    /// walls, railings, atrium columns, the roof with its glass atrium skylight, the parking lot and lights.
    /// <see cref="MallPopulator"/> then adds the gameplay objects.
    /// </summary>
    public static class MallBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Mall.unity";
        public const string TilesGroup = "Tiles", FlightsGroup = "Flights";

        private static readonly Dictionary<string, Color> ZoneColors = new()
        {
            ["concourse"] = new Color(0.62f, 0.62f, 0.6f), ["walkway"] = new Color(0.45f, 0.58f, 0.75f),
            ["food"] = new Color(0.72f, 0.6f, 0.45f), ["electronics"] = new Color(0.45f, 0.55f, 0.62f),
            ["clothing"] = new Color(0.66f, 0.5f, 0.6f), ["jewelry"] = new Color(0.7f, 0.66f, 0.42f),
            ["office"] = new Color(0.5f, 0.52f, 0.5f), ["stock"] = new Color(0.5f, 0.46f, 0.4f),
            ["furniture"] = new Color(0.58f, 0.48f, 0.36f), ["toys"] = new Color(0.55f, 0.66f, 0.5f),
            ["cinema"] = new Color(0.45f, 0.35f, 0.42f), ["gallery"] = new Color(0.82f, 0.8f, 0.76f),
        };

        [MenuItem("Tools/Abandoned/Create Mall")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject("Mall").transform;

            int tiles = BuildTiles(Group(TilesGroup, root));
            Material steps = GetMaterial("Greybox_Stairs", new Color(0.92f, 0.55f, 0.2f));
            Material sides = GetMaterial("Greybox_Railing", new Color(0.3f, 0.3f, 0.32f));
            Transform flights = Group(FlightsGroup, root);
            foreach (Flight f in Flights) MallFlights.Build(f, flights, steps, sides);

            Material wall = GetMaterial("Greybox_Wall", new Color(0.82f, 0.81f, 0.78f));
            Material frame = GetMaterial("Greybox_DoorFrame", new Color(0.95f, 0.8f, 0.1f));
            int walls = 0;
            Transform wallRoot = Group("Walls", root);
            for (int f = 0; f < Floors; f++) walls += MallWalls.Build(wallRoot, f, wall, frame, sides);

            BuildColumns(Group("Columns", root));
            BuildRoof(Group("Roof", root));
            BuildExterior(Group("Exterior", root));
            PlaceCamera();
            MallPopulator.Populate(root);
            int props = MallProps.Place(root, root.Find(TilesGroup));
            int fixtures = BuildLights(root); // after Populate: fixtures hang from the tiles' sections
            MallProps.ParkVehicles(root.Find("Exterior"));
            BakeNavMesh(root);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            int networkObjects = NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Mall saved to {ScenePath}: {tiles} floor tiles, {Flights.Length} flights, {walls} wall/railing panels, {props} props, {fixtures} light fixtures, {networkObjects} network objects.");
        }

        public static string TileName(Vector2Int c, int floor) =>
            $"{(IsWalkwayTile(c, floor) ? "Walk" : "Floor")}_{floor}_{c.x}_{c.y}";

        private static int BuildTiles(Transform parent)
        {
            int count = 0;
            for (int f = 0; f < Floors; f++)
            {
                Transform floorGroup = Group($"Floor_{f}", parent);
                for (int x = 0; x < TilesX; x++)
                for (int z = 0; z < TilesZ; z++)
                {
                    var c = new Vector2Int(x, z);
                    Zone? zone = ZoneAt(c, f);
                    if (!zone.HasValue) continue;
                    string tag = IsWalkwayTile(c, f) ? "walkway" : zone.Value.Tag;
                    Color color = ZoneColors.TryGetValue(tag, out Color zc) ? zc : Color.gray;
                    if ((x + z) % 2 == 0) color *= 0.9f;
                    Material material = GetMaterial($"Greybox_Mall_{tag}_{(x + z) % 2}", new Color(color.r, color.g, color.b, 1f));
                    CreateTile(TileName(c, f), floorGroup, TileTopCenter(c, f), material);
                    count++;
                }
            }
            return count;
        }

        private static void CreateTile(string name, Transform parent, Vector3 topCenter, Material material)
        {
            float slab = TestBuildingBuilder.SlabThickness;
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = topCenter;
            var size = new Vector3(Tile, slab, Tile);
            var center = new Vector3(0f, -slab / 2f, 0f);
            var collider = root.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
            Box("Visual", root.transform, center, size, material, withCollider: false);
        }

        private static void BuildColumns(Transform parent)
        {
            Material material = GetMaterial("Greybox_Column", new Color(0.6f, 0.6f, 0.62f));
            float h = Floors * StoryHeight;
            foreach (Vector2Int corner in new[] { new Vector2Int(Atrium.xMin, Atrium.yMin), new Vector2Int(Atrium.xMax, Atrium.yMin),
                         new Vector2Int(Atrium.xMin, Atrium.yMax), new Vector2Int(Atrium.xMax, Atrium.yMax) })
                Box($"Column_{corner.x}_{corner.y}", parent, new Vector3(corner.x * Tile, h / 2f, corner.y * Tile),
                    new Vector3(0.6f, h, 0.6f), material);
            MarkStatic(parent);
        }

        private static void BuildRoof(Transform parent)
        {
            Material roof = GetMaterial("Greybox_Roof", new Color(0.35f, 0.35f, 0.37f));
            float y = Floors * StoryHeight, t = 0.3f;
            float w = TilesX * Tile, d = TilesZ * Tile;
            float ax0 = Atrium.xMin * Tile, ax1 = Atrium.xMax * Tile, az0 = Atrium.yMin * Tile, az1 = Atrium.yMax * Tile;
            // Four slabs around the skylight opening.
            Box("Roof_S", parent, new Vector3(w / 2f, y + t / 2f, az0 / 2f), new Vector3(w, t, az0), roof);
            Box("Roof_N", parent, new Vector3(w / 2f, y + t / 2f, (az1 + d) / 2f), new Vector3(w, t, d - az1), roof);
            Box("Roof_W", parent, new Vector3(ax0 / 2f, y + t / 2f, (az0 + az1) / 2f), new Vector3(ax0, t, az1 - az0), roof);
            Box("Roof_E", parent, new Vector3((ax1 + w) / 2f, y + t / 2f, (az0 + az1) / 2f), new Vector3(w - ax1, t, az1 - az0), roof);
            // The glass skylight: cosmetic for now (a later collapse showcase, GDD 8).
            var glass = GetMaterial("Greybox_Glass", new Color(0.6f, 0.8f, 0.9f, 0.25f));
            MakeTransparent(glass);
            Box("Skylight", parent, new Vector3((ax0 + ax1) / 2f, y + 0.05f, (az0 + az1) / 2f), new Vector3(ax1 - ax0, 0.1f, az1 - az0), glass, withCollider: false)
                .GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            MarkStatic(parent);
        }

        private static void MakeTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        private static void BuildExterior(Transform parent)
        {
            Material ground = GetMaterial("Greybox_Ground", new Color(0.36f, 0.4f, 0.32f));
            Material asphalt = GetMaterial("Greybox_Asphalt", new Color(0.16f, 0.16f, 0.17f));
            Material line = GetMaterial("Greybox_ParkingLine", new Color(0.92f, 0.92f, 0.92f));
            float w = TilesX * Tile, d = TilesZ * Tile, t = 0.5f;
            Transform slabs = Group("GroundSlabs", parent);
            Box("Ground_S", slabs, new Vector3(w / 2f, -t / 2f, -20f), new Vector3(w + 60f, t, 40f), ground);
            Box("Ground_N", slabs, new Vector3(w / 2f, -t / 2f, d + 10f), new Vector3(w + 60f, t, 20f), ground);
            Box("Ground_W", slabs, new Vector3(-15f, -t / 2f, d / 2f), new Vector3(30f, t, d), ground);
            Box("Ground_E", slabs, new Vector3(w + 15f, -t / 2f, d / 2f), new Vector3(30f, t, d), ground);

            Transform parking = Group("Parking", parent);
            Box("Asphalt", parking, new Vector3(w / 2f, -0.04f, -14f), new Vector3(w, 0.12f, 24f), asphalt);
            Box("Driveway", parking, new Vector3(w + 8f, -0.04f, d / 2f - 4f), new Vector3(14f, 0.12f, d + 8f), asphalt);
            for (int i = 0; i <= 12; i++)
                Box($"Line_{i}", parking, new Vector3(2f + i * 3.6f, 0.025f, -20f), new Vector3(0.12f, 0.01f, 5f), line, false);
            MarkStatic(parent);
        }

        // M7.2: realtime lights only (power-off runs and collapses rule out baked light); see MallFixtures.
        private static int BuildLights(Transform mall)
        {
            Transform lights = Group("Lights", mall);
            LevelAtmosphere.Apply("Mall", LevelAtmosphere.Mall, lights);
            int fixtures = MallFixtures.Place(lights, mall.Find(TilesGroup));
            MallFixtures.AddDust(lights);
            MallFixtures.AddSkylightShaft(lights, RenderSettings.sun != null ? RenderSettings.sun.transform.forward : Vector3.down);
            AmbienceBuilder.Mall(mall);
            return fixtures;
        }

        public const string NavMeshPath = "Assets/_Project/Scenes/Mall_NavMesh.asset";

        /// <summary>
        /// Baked once with every floor intact; collapses carve holes at runtime (SectionNavCarver), so
        /// there's never a runtime rebake. Players, loot and debris aren't walls.
        /// </summary>
        private static void BakeNavMesh(Transform root)
        {
            var surface = root.gameObject.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            // Every collapsible tile carries a (disabled) carving obstacle; by default the bake skips
            // anything with an obstacle, which would leave the upper floors out of the mesh.
            surface.ignoreNavMeshObstacle = false;
            surface.layerMask = ~LayerMask.GetMask(Abandoned.Core.GameLayers.Player, Abandoned.Core.GameLayers.Loot,
                Abandoned.Core.GameLayers.Debris, "Ignore Raycast");
            // The monster stays inside: nothing outside the walls, nothing on the roof (GDD: it lives in the building).
            foreach (string group in new[] { "Exterior", "Truck", "Roof" })
                root.Find(group).gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            surface.BuildNavMesh();
            AssetDatabase.DeleteAsset(NavMeshPath);
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath);
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(TilesX * Tile / 2f, 34f, -34f);
            camera.transform.LookAt(new Vector3(TilesX * Tile / 2f, 4f, TilesZ * Tile / 2f));
        }

        private static void AddToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
