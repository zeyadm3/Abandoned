using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the two-story greybox test building used to develop movement, carrying and the
    /// structural system. Every floor tile and stair segment is its own unscaled root so
    /// StructuralSection can be added per piece in Milestone 2.
    /// Layout (tile grid x 0..4, z 0..3, front/south at z = 0):
    ///   atrium = tiles (1..2, 1..2), stairwell = tiles (4, 1..2) rising north.
    /// </summary>
    public static class TestBuildingBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/TestBuilding.unity";

        private const float Tile = 4f;
        private const int TilesX = 5;
        private const int TilesZ = 4;
        private const float StoryHeight = 4f;
        private const float SlabThickness = 0.3f;
        private const float UpperWallHeight = 3f;
        private const float WallThickness = 0.2f;
        private const float RailingHeight = 1f;
        private const float StairWidth = 3f;
        private const int StepsPerSegment = 10;
        private const float StepThickness = 0.4f;

        private static readonly HashSet<Vector2Int> Atrium = new()
            { new(1, 1), new(2, 1), new(1, 2), new(2, 2) };
        private static readonly HashSet<Vector2Int> Stairwell = new() { new(4, 1), new(4, 2) };
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Openings sit in the middle of the 4m panel for tile column ix on the south wall.
        private const int NarrowDoorColumn = 1;
        private const int LoadingDoorColumn = 3;
        private const int WindowColumn = 1;
        private static readonly WallOpening NarrowDoor = new(1.2f, 0f, 2.2f);   // player yes, piano no
        private static readonly WallOpening LoadingDoor = new(3.5f, 0f, 3.5f);
        private static readonly WallOpening Window = new(1.2f, 1f, 1.2f);       // future rope-point fallback route

        private static Material tileA, tileB, balcony, stairs, wall, column, railing, frame, ground, asphalt, line, truck;

        [MenuItem("Tools/Abandoned/Create Test Building")]
        public static void Create()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Create Test Building",
                    $"{ScenePath} already exists. Rebuild and overwrite it?", "Overwrite", "Cancel"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        /// <summary>Builds and saves the scene without any dialogs (used by batch mode).</summary>
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            LoadMaterials();

            Transform root = new GameObject("TestBuilding").transform;
            int groundTiles = BuildGroundFloor(Group("GroundFloor", root));
            (int upperTiles, int balconyTiles) = BuildUpperFloor(Group("UpperFloor", root));
            int stairSegments = BuildStairs(Group("Stairs", root));
            BuildColumns(Group("Columns", root));
            BuildExterior(Group("Exterior", root));
            PlaceCamera();
            TestBuildingPopulator.Populate(root);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();

            Debug.Log($"Test building saved to {ScenePath}: {groundTiles} ground tiles, " +
                      $"{upperTiles} upper tiles ({balconyTiles} balcony), {stairSegments} stair segments.");
        }

        private static void LoadMaterials()
        {
            tileA = GetMaterial("Greybox_TileA", new Color(0.55f, 0.55f, 0.55f));
            tileB = GetMaterial("Greybox_TileB", new Color(0.68f, 0.68f, 0.68f));
            balcony = GetMaterial("Greybox_Balcony", new Color(0.45f, 0.58f, 0.75f));
            stairs = GetMaterial("Greybox_Stairs", new Color(0.92f, 0.55f, 0.2f));
            wall = GetMaterial("Greybox_Wall", new Color(0.82f, 0.81f, 0.78f));
            column = GetMaterial("Greybox_Column", new Color(0.6f, 0.6f, 0.62f));
            railing = GetMaterial("Greybox_Railing", new Color(0.3f, 0.3f, 0.32f));
            frame = GetMaterial("Greybox_DoorFrame", new Color(0.95f, 0.8f, 0.1f));
            ground = GetMaterial("Greybox_Ground", new Color(0.36f, 0.4f, 0.32f));
            asphalt = GetMaterial("Greybox_Asphalt", new Color(0.16f, 0.16f, 0.17f));
            line = GetMaterial("Greybox_ParkingLine", new Color(0.92f, 0.92f, 0.92f));
            truck = GetMaterial("Greybox_Truck", new Color(0.7f, 0.22f, 0.2f));
        }

        private static int BuildGroundFloor(Transform floor)
        {
            Transform tiles = Group("Tiles", floor);
            int count = 0;
            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                // M2: these get StructuralSection with canCollapse = false; there is no basement below.
                CreateTile($"Tile_G_{x}_{z}", tiles, TileTopCenter(x, z, 0f), (x + z) % 2 == 0 ? tileA : tileB);
                count++;
            }

            BuildExteriorWalls(Group("Walls", floor), 0f, StoryHeight, isGroundStory: true);
            return count;
        }

        private static (int tiles, int balconies) BuildUpperFloor(Transform floor)
        {
            Transform tiles = Group("Tiles", floor);
            int count = 0, balconies = 0;
            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                if (Atrium.Contains(cell) || Stairwell.Contains(cell)) continue;

                bool isBalcony = Directions.Any(d => Atrium.Contains(cell + d));
                string name = isBalcony ? $"Balcony_U_{x}_{z}" : $"Tile_U_{x}_{z}";
                Material material = isBalcony ? balcony : (x + z) % 2 == 0 ? tileA : tileB;
                CreateTile(name, tiles, TileTopCenter(x, z, StoryHeight), material);
                count++;
                if (isBalcony) balconies++;
            }

            BuildRailings(Group("Railings", floor));
            BuildExteriorWalls(Group("Walls", floor), StoryHeight, UpperWallHeight, isGroundStory: false);
            return (count, balconies);
        }

        private static void CreateTile(string name, Transform parent, Vector3 topCenter, Material material)
        {
            // Unscaled root with the pivot on the walking surface; only the Visual child is scaled,
            // so M2 can swap it for the pre-fractured version without touching the root.
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = topCenter;

            var size = new Vector3(Tile, SlabThickness, Tile);
            var center = new Vector3(0f, -SlabThickness / 2f, 0f);
            var collider = root.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
            Box("Visual", root.transform, center, size, material, withCollider: false);
        }

        private static void BuildRailings(Transform railings)
        {
            float y = StoryHeight + RailingHeight / 2f;

            // Every edge between an atrium tile and a balcony tile.
            foreach (Vector2Int cell in Atrium)
            foreach (Vector2Int d in Directions)
            {
                Vector2Int neighbour = cell + d;
                if (Atrium.Contains(neighbour) || !InGrid(neighbour)) continue;
                Vector3 edge = TileTopCenter(cell.x, cell.y, 0f) + new Vector3(d.x, 0f, d.y) * (Tile / 2f);
                RailingPanel(railings, $"Railing_Atrium_{cell.x}_{cell.y}_{neighbour.x}_{neighbour.y}",
                    new Vector3(edge.x, y, edge.z), alongX: d.y != 0);
            }

            // Stairwell: west side and the south side; the north side is where the stairs arrive.
            RailingPanel(railings, "Railing_Stairwell_W1", new Vector3(4 * Tile, y, 1.5f * Tile), alongX: false);
            RailingPanel(railings, "Railing_Stairwell_W2", new Vector3(4 * Tile, y, 2.5f * Tile), alongX: false);
            RailingPanel(railings, "Railing_Stairwell_S", new Vector3(4.5f * Tile, y, 1 * Tile), alongX: true);
        }

        private static void RailingPanel(Transform parent, string name, Vector3 center, bool alongX)
        {
            Vector3 size = alongX
                ? new Vector3(Tile, RailingHeight, 0.1f)
                : new Vector3(0.1f, RailingHeight, Tile);
            Box(name, parent, center, size, railing);
        }

        private static void BuildExteriorWalls(Transform walls, float y0, float height, bool isGroundStory)
        {
            float half = WallThickness / 2f;
            float width = TilesX * Tile, depth = TilesZ * Tile;

            for (int x = 0; x < TilesX; x++)
            {
                WallOpening? opening = null;
                if (isGroundStory && x == NarrowDoorColumn) opening = NarrowDoor;
                if (isGroundStory && x == LoadingDoorColumn) opening = LoadingDoor;
                if (!isGroundStory && x == WindowColumn) opening = Window;

                WallPanel(walls, $"Wall_S_{x}", new Vector3(x * Tile, y0, -half), Vector3.right, opening);
                WallPanel(walls, $"Wall_N_{x}", new Vector3(x * Tile, y0, depth + half), Vector3.right, null);
            }
            for (int z = 0; z < TilesZ; z++)
            {
                WallPanel(walls, $"Wall_W_{z}", new Vector3(-half, y0, z * Tile), Vector3.forward, null);
                WallPanel(walls, $"Wall_E_{z}", new Vector3(width + half, y0, z * Tile), Vector3.forward, null);
            }

            // Corner posts close the gaps where the outward-offset walls meet.
            foreach (Vector3 corner in new[] { new Vector3(-half, 0, -half), new Vector3(width + half, 0, -half),
                         new Vector3(-half, 0, depth + half), new Vector3(width + half, 0, depth + half) })
                Box("Corner", walls, corner + Vector3.up * (y0 + height / 2f),
                    new Vector3(WallThickness, height, WallThickness), wall);

            MarkStatic(walls);

            void WallPanel(Transform parent, string name, Vector3 start, Vector3 direction, WallOpening? hole) =>
                GreyboxWall.Panel(parent, name, start, direction, Tile, height, WallThickness, hole, wall, frame);
        }

        private static int BuildStairs(Transform stairsGroup)
        {
            // Stairs run north through the stairwell, one 4m segment per stairwell tile.
            int segments = Stairwell.Count;
            float rise = StoryHeight / segments;
            float stepRise = rise / StepsPerSegment;
            float stepDepth = Tile / StepsPerSegment;
            float angle = Mathf.Atan2(rise, Tile) * Mathf.Rad2Deg;
            float startZ = Stairwell.Min(c => c.y) * Tile;

            for (int i = 0; i < segments; i++)
            {
                var root = new GameObject($"StairSegment_{i}").transform;
                root.SetParent(stairsGroup, false);
                root.localPosition = new Vector3(4.5f * Tile, i * rise, startZ + i * Tile);

                for (int s = 0; s < StepsPerSegment; s++)
                {
                    float stepTop = (s + 1) * stepRise;
                    Box($"Step_{s}", root, new Vector3(0f, stepTop - StepThickness / 2f, (s + 0.5f) * stepDepth),
                        new Vector3(StairWidth, StepThickness, stepDepth), stairs, withCollider: false);
                }

                // One smooth sloped collider instead of step colliders, so the CharacterController
                // and carried loot glide up instead of snagging on each step edge.
                var ramp = new GameObject("Ramp").transform;
                ramp.SetParent(root, false);
                float rad = angle * Mathf.Deg2Rad;
                const float rampThickness = 0.2f;
                Vector3 topMid = new Vector3(0f, rise / 2f + stepRise / 2f, Tile / 2f);
                ramp.localPosition = topMid - new Vector3(0f, Mathf.Cos(rad), -Mathf.Sin(rad)) * (rampThickness / 2f);
                ramp.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                ramp.gameObject.AddComponent<BoxCollider>().size =
                    new Vector3(StairWidth, rampThickness, Mathf.Sqrt(Tile * Tile + rise * rise));
            }
            return segments;
        }

        private static void BuildColumns(Transform columns)
        {
            float height = StoryHeight - SlabThickness;
            foreach (Vector3 corner in new[] { new Vector3(1, 0, 1), new Vector3(3, 0, 1), new Vector3(1, 0, 3), new Vector3(3, 0, 3) })
                Box($"Column_{corner.x}_{corner.z}", columns, corner * Tile + Vector3.up * height / 2f,
                    new Vector3(0.4f, height, 0.4f), column);
            MarkStatic(columns);
        }

        private static void BuildExterior(Transform exterior)
        {
            // Four slabs around the footprint, flush with the ground-floor tiles: no overlap, no z-fighting.
            Transform slabs = Group("GroundSlabs", exterior);
            const float t = 0.5f;
            Box("Ground_S", slabs, new Vector3(10f, -t / 2f, -15f), new Vector3(60f, t, 30f), ground);
            Box("Ground_N", slabs, new Vector3(10f, -t / 2f, 26f), new Vector3(60f, t, 20f), ground);
            Box("Ground_W", slabs, new Vector3(-10f, -t / 2f, 8f), new Vector3(20f, t, 16f), ground);
            Box("Ground_E", slabs, new Vector3(30f, -t / 2f, 8f), new Vector3(20f, t, 16f), ground);

            Transform parking = Group("Parking", exterior);
            Box("Asphalt", parking, new Vector3(10f, -0.04f, -9f), new Vector3(20f, 0.12f, 14f), asphalt);
            for (int i = 0; i <= 6; i++)
                Box($"Line_{i}", parking, new Vector3(1f + i * 3f, 0.025f, -13.5f), new Vector3(0.12f, 0.01f, 5f), line, false);

            MarkStatic(exterior);

            // Not static: M5 replaces this with the real truck, parked facing the loading door.
            Box("TruckSpot", exterior, new Vector3(LoadingDoorColumn * Tile + Tile / 2f, 1.27f, -7f),
                new Vector3(2.5f, 2.5f, 6f), truck);
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(10f, 10f, -24f);
            camera.transform.LookAt(new Vector3(10f, 2f, 8f));
        }

        private static void AddToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Vector3 TileTopCenter(int x, int z, float y) => new((x + 0.5f) * Tile, y, (z + 0.5f) * Tile);

        private static bool InGrid(Vector2Int c) => c.x >= 0 && c.x < TilesX && c.y >= 0 && c.y < TilesZ;
    }
}
