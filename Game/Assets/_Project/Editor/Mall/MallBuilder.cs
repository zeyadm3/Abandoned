using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
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
            CustomMallArt.Prepare();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject("Mall").transform;
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(new Bounds(new Vector3(28f, 4f, 24f), new Vector3(56f, 16f, 48f)));

            int tiles = BuildTiles(Group(TilesGroup, root));
            Material sides = PolishAssets.Material("Mall_Railing", new Color(0.25f, 0.29f, 0.29f), 0.65f);
            Transform flights = Group(FlightsGroup, root);
            foreach (Flight f in Flights) MallFlights.Build(f, flights);

            Material wall = PolishAssets.Material("Mall_PaintedPlaster", new Color(0.32f, 0.34f, 0.30f), texture: PolishAssets.Texture("PlasterAge"));
            Material frame = PolishAssets.Material("Mall_DoorTrim", new Color(0.35f, 0.4f, 0.39f));
            int walls = 0;
            Transform wallRoot = Group("Walls", root);
            for (int f = 0; f < Floors; f++) walls += MallWalls.Build(wallRoot, f, wall, frame, sides);

            BuildColumns(Group("Columns", root));
            BuildRoof(Group("Roof", root));
            BuildExterior(Group("Exterior", root));
            MallBasementBuilder.Build(root);
            PlaceCamera();
            MallPopulator.Populate(root);
            BuildCeilings(root);
            int props = MallProps.Place(root, root.Find(TilesGroup));
            MallDecayBuilder.Place(root);
            int fixtures = BuildLights(root); // after Populate: fixtures hang from the tiles' sections
            MallProps.ParkVehicles(root.Find("Exterior"));
            MallMysteryBuilder.Place(root);
            BakeNavMesh(root);
            int shutters = MallShutters.Place(root); // after the bake: runtime obstacles, not walls
            // The rebuild log is the report: stairs a player or monster can't take, invisible walls.
            MallFlightValidator.Run(root);
            InvisibleColliderAudit.Run(root, "Mall");

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            int networkObjects = NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Mall saved to {ScenePath}: {tiles} floor tiles, {Flights.Length} flights, {walls} wall/railing panels, {props} props, {fixtures} light fixtures, {shutters} shutters, {networkObjects} network objects, occlusion not baked.");
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
                    // Neutral laminate/stone keeps structural tint/cracks readable; zone colour remains a quiet undertone.
                    color = Color.Lerp(new Color(0.49f, 0.48f, 0.43f), color, 0.24f);
                    Material material = PolishAssets.Material($"Mall_Floor_{tag}_{(x + z) % 2}", new Color(color.r, color.g, color.b, 1f),
                        texture: PolishAssets.Texture("MallTile", tiles: true));
                    CreateTile(TileName(c, f), floorGroup, TileTopCenter(c, f), material);
                    count++;
                }
            }
            return count;
        }

        private static void CreateTile(string name, Transform parent, Vector3 topCenter, Material material)
        {
            float slab = TestMapBuilder.SlabThickness;
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = topCenter;
            var size = new Vector3(Tile, slab, Tile);
            var center = new Vector3(0f, -slab / 2f, 0f);
            var collider = root.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
            GameObject visual = CustomMallArt.Place("floor", root.transform, Vector3.zero, Quaternion.identity, new Vector3(1f, slab / .18f, 1f));
            visual.name = "Visual";
        }

        private static void BuildColumns(Transform parent)
        {
            Material material = GetMaterial("Greybox_Column", new Color(0.6f, 0.6f, 0.62f));
            float h = Floors * StoryHeight;
            foreach (Vector2Int corner in new[] { new Vector2Int(Atrium.xMin, Atrium.yMin), new Vector2Int(Atrium.xMax, Atrium.yMin),
                         new Vector2Int(Atrium.xMin, Atrium.yMax), new Vector2Int(Atrium.xMax, Atrium.yMax) })
            {
                Box($"Column_{corner.x}_{corner.y}", parent, new Vector3(corner.x * Tile, h / 2f, corner.y * Tile), new Vector3(.86f,h,.86f),material).GetComponent<Renderer>().enabled=false;
                for(int f=0;f<Floors;f++) CustomMallArt.Place("pillar",parent,new Vector3(corner.x*Tile,f*StoryHeight,corner.y*Tile),Quaternion.identity,new Vector3(1f,StoryHeight/3.82f,1f));
            }
            MarkStatic(parent);
        }

        private static void BuildRoof(Transform parent)
        {
            float y = Floors * StoryHeight, width = TilesX * Tile, depth = TilesZ * Tile;
            // One opaque deck backs every ceiling, including the dirty skylight. Colliders alone do not occlude the sky.
            Box("RoofDeck", parent, new Vector3(width * .5f, y + .15f, depth * .5f),
                new Vector3(width + ThicknessOverlap, .3f, depth + ThicknessOverlap), CustomMallArt.Material("Concrete"));
            float atriumWidth = Atrium.width * Tile, atriumDepth = Atrium.height * Tile;
            Vector3 middle = new(Atrium.center.x * Tile, y - .025f, Atrium.center.y * Tile);
            Box("SealedSkylight", parent, middle, new Vector3(atriumWidth, .08f, atriumDepth), CustomMallArt.Material("Glass"), false);
            for (int i = 0; i <= Atrium.width; i++)
                Box("SkylightRibX", parent, new Vector3(Atrium.xMin * Tile + i * Tile, y - .12f, middle.z),
                    new Vector3(.08f, .2f, atriumDepth), CustomMallArt.Material("Metal"), false);
            for (int i = 0; i <= Atrium.height; i++)
                Box("SkylightRibZ", parent, new Vector3(middle.x, y - .12f, Atrium.yMin * Tile + i * Tile),
                    new Vector3(atriumWidth, .2f, .08f), CustomMallArt.Material("Metal"), false);
            MarkStatic(parent);
        }

        private const float ThicknessOverlap = .22f;

        private static void BuildCeilings(Transform mall)
        {
            Transform ceilings = Group("Ceilings", mall);
            Transform tiles = mall.Find(TilesGroup);
            for (int floor = -1; floor < Floors; floor++)
            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                bool basement = floor == -1;
                if (basement && (x < 10 || z < 6)) continue;
                StructuralSection support = null;
                if (floor + 1 < Floors)
                {
                    if (IsVoid(cell, floor + 1)) continue;
                    support = tiles.Find($"Floor_{floor + 1}/{TileName(cell, floor + 1)}")?.GetComponent<StructuralSection>();
                    if (support == null) continue;
                }
                else if (Atrium.Contains(cell)) continue;
                float level = FloorY(floor + 1) - TestMapBuilder.SlabThickness - .025f;
                Transform ceiling = Group($"Ceiling_{floor}_{x}_{z}", ceilings);
                ceiling.localPosition = new Vector3((x + .5f) * Tile, level, (z + .5f) * Tile);
                // Broken mineral panels reveal an opaque concrete underside, never open sky or an unrelated store.
                Box("SlabUnderside", ceiling, Vector3.up * .055f, new Vector3(Tile, .06f, Tile), CustomMallArt.Material("Concrete"), false);
                bool damaged = (x * 17 + z * 31 + floor * 7) % 23 == 0;
                CustomMallArt.Place(damaged ? "CeilingSagging" : "CeilingGrid", ceiling, Vector3.zero);
                if (support != null) ceiling.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            }
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
