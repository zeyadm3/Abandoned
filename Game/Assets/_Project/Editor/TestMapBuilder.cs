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
    /// Builds the two-story test building used to develop movement, carrying and the
    /// structural system. Every floor tile and stair segment is its own unscaled root so
    /// StructuralSection can be added per piece in Milestone 2.
    /// Layout (tile grid x 0..4, z 0..3, front/south at z = 0):
    ///   atrium = tiles (1..2, 1..2), stairwell = tiles (4, 1..2) rising north.
    /// Since 2026-10-22 it is also the graphics showcase (TestMapShowcase): the same colliders, finished as a real
    /// warehouse, with a gallery of the crew and monsters beside it.
    /// </summary>
    public static class TestMapBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/TestMap.unity";

        public const float Tile = 4f;
        public const int TilesX = 5;
        public const int TilesZ = 4;
        public const float StoryHeight = 4f;
        public const float SlabThickness = 0.3f;
        public const float UpperWallHeight = 3f;
        public const float WallThickness = 0.2f;
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
        private static readonly WallOpening Glazed = new(1.8f, .9f, 1.5f);      // upstairs windows, glass in them

        private static Material concrete, timber, stairs, blockWall, plasterWall, column, asphalt, line;

        [MenuItem(ToolMenu.TestMap + "Rebuild Test Map", priority = ToolMenu.TestMapP)]
        public static void Create()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Create Test Map",
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

            Transform root = new GameObject("TestMap").transform;
            int groundTiles = BuildGroundFloor(Group("GroundFloor", root));
            (int upperTiles, int balconyTiles) = BuildUpperFloor(Group("UpperFloor", root));
            int stairSegments = BuildStairs(Group("Stairs", root));
            BuildColumns(Group("Columns", root));
            BuildExterior(Group("Exterior", root));
            PlaceCamera();
            TestMapPopulator.Populate(root);
            TestMapShowcase.Dress(root);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            int networkObjects = NetworkObjectIds.StampScene(scene);
            TestMapShowcase.Finish(scene, root);
            AssetDatabase.SaveAssets();

            Debug.Log($"Test map saved to {ScenePath}: {groundTiles} ground tiles, " +
                      $"{upperTiles} upper tiles ({balconyTiles} balcony), {stairSegments} stair segments, {networkObjects} network objects.");
        }

        private static void LoadMaterials()
        {
            ShowcaseMaterials.Reset();
            concrete = ShowcaseMaterials.ConcreteFloor;
            timber = ShowcaseMaterials.WoodFloor;
            stairs = ShowcaseMaterials.Tread;
            blockWall = ShowcaseMaterials.BlockWall;
            plasterWall = ShowcaseMaterials.PlasterWall;
            column = ShowcaseMaterials.CastConcrete;
            asphalt = ShowcaseMaterials.Asphalt;
            line = ShowcaseMaterials.LinePaint;
        }

        private static int BuildGroundFloor(Transform floor)
        {
            Transform tiles = Group("Tiles", floor);
            int count = 0;
            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                // M2: these get StructuralSection with canCollapse = false; there is no basement below.
                CreateTile($"Tile_G_{x}_{z}", tiles, TileTopCenter(x, z, 0f), concrete);
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
                Transform tile = CreateTile(name, tiles, TileTopCenter(x, z, StoryHeight), timber);
                ShowcaseStructure.Joists(tile.Find("Visual"), Tile, SlabThickness);
                count++;
                if (isBalcony) balconies++;
            }

            BuildRailings(Group("Railings", floor));
            BuildExteriorWalls(Group("Walls", floor), StoryHeight, UpperWallHeight, isGroundStory: false);
            return (count, balconies);
        }

        private static Transform CreateTile(string name, Transform parent, Vector3 topCenter, Material material)
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
            return root.transform;
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
            // The solid panel stays exactly where it was (nobody falls through); what you see is an open steel rail.
            Vector3 size = alongX
                ? new Vector3(Tile, RailingHeight, 0.1f)
                : new Vector3(0.1f, RailingHeight, Tile);
            Transform panel = Group(name, parent);
            panel.localPosition = center;
            panel.gameObject.AddComponent<BoxCollider>().size = size;
            ShowcaseStructure.Railing(panel, Tile, RailingHeight, alongX);
        }

        private static void BuildExteriorWalls(Transform walls, float y0, float height, bool isGroundStory)
        {
            float half = WallThickness / 2f;
            float width = TilesX * Tile, depth = TilesZ * Tile;

            Material inside = isGroundStory ? blockWall : plasterWall;
            const ShowcaseWalls.Kind none = ShowcaseWalls.Kind.Window;
            for (int x = 0; x < TilesX; x++)
            {
                WallOpening? opening = null;
                ShowcaseWalls.Kind kind = none;
                if (isGroundStory && x == NarrowDoorColumn) (opening, kind) = (NarrowDoor, ShowcaseWalls.Kind.Door);
                if (isGroundStory && x == LoadingDoorColumn) (opening, kind) = (LoadingDoor, ShowcaseWalls.Kind.LoadingDoor);
                if (!isGroundStory && x == WindowColumn) (opening, kind) = (Window, ShowcaseWalls.Kind.OpenWindow);

                WallPanel(walls, $"Wall_S_{x}", new Vector3(x * Tile, y0, -half), Vector3.right, Vector3.back, opening, kind);
                WallPanel(walls, $"Wall_N_{x}", new Vector3(x * Tile, y0, depth + half), Vector3.right, Vector3.forward,
                    isGroundStory ? null : Glazed, none);
            }
            for (int z = 0; z < TilesZ; z++)
            {
                // Upstairs, the middle two bays of each side wall are glazed (the west offices, the east stairwell).
                WallOpening? side = !isGroundStory && z is 1 or 2 ? Glazed : null;
                WallPanel(walls, $"Wall_W_{z}", new Vector3(-half, y0, z * Tile), Vector3.forward, Vector3.left, side, none);
                WallPanel(walls, $"Wall_E_{z}", new Vector3(width + half, y0, z * Tile), Vector3.forward, Vector3.right, side, none);
            }

            // Corner posts close the gaps where the outward-offset walls meet; brick blocks close the skin outside them.
            foreach (Vector3 corner in new[] { new Vector3(-half, 0, -half), new Vector3(width + half, 0, -half),
                         new Vector3(-half, 0, depth + half), new Vector3(width + half, 0, depth + half) })
                Box("Corner", walls, corner + Vector3.up * (y0 + height / 2f),
                    new Vector3(WallThickness, height, WallThickness), inside);
            ShowcaseWalls.Corners(walls, Vector3.zero, new Vector3(width, 0f, depth), y0, height, WallThickness);
            if (isGroundStory) ShowcaseWalls.StringCourse(walls, Vector3.zero, new Vector3(width, 0f, depth), StoryHeight, WallThickness);

            MarkStatic(walls);

            void WallPanel(Transform parent, string name, Vector3 start, Vector3 direction, Vector3 outward, WallOpening? hole,
                ShowcaseWalls.Kind kind) =>
                ShowcaseWalls.Panel(parent, name, start, direction, outward, Tile, height, WallThickness, hole, kind, inside);
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

                Transform steps = Group("Visual", root);
                for (int s = 0; s < StepsPerSegment; s++)
                {
                    float stepTop = (s + 1) * stepRise;
                    Box($"Step_{s}", steps, new Vector3(0f, stepTop - StepThickness / 2f, (s + 0.5f) * stepDepth),
                        new Vector3(StairWidth, StepThickness, stepDepth), stairs, withCollider: false);
                }
                ShowcaseStructure.StairSides(steps, Tile, rise, StairWidth, stepRise);

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
            // The ground round the building is the showcase's terrain (ShowcaseTerrain), 3 cm under the slab.
            Transform parking = Group("Parking", exterior);
            Box("Asphalt", parking, new Vector3(10f, -0.04f, -9f), new Vector3(20f, 0.12f, 14f), asphalt);
            for (int i = 0; i <= 6; i++)
                Box($"Line_{i}", parking, new Vector3(1f + i * 3f, 0.025f, -13.5f), new Vector3(0.12f, 0.01f, 5f), line, false);

            MarkStatic(exterior);
            // The loading lane in front of the big door (x 12..16) is left empty; the red truck block is gone.
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
