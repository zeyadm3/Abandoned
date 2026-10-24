using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the physical and structural floor systems for Pine Hollow Middle School.
    /// Manages ground slabs, two-story academic wing floors, weak collapsible second-story tiles,
    /// and stairwells with NavMesh links and keep-clear corridors.
    /// </summary>
    public static class BurleySchoolStructure
    {
        public sealed class TilePiece
        {
            public Transform Transform;
            public int Floor; // -1 basement, 0 ground, 1 upper
            public Vector2Int Cell;
            public bool Collapsible;
            public float Health = 1f;
            public float Capacity = 1f;
        }

        public sealed class Result
        {
            public readonly List<TilePiece> Pieces = new();
            public readonly List<Transform> Stairs = new();
            public StructureSimulation Simulation;
        }

        public static Result Build(Transform root, BurleySchoolArt art)
        {
            var result = new Result();
            Transform levels = Group("Levels", root);
            Transform groundGroup = Group("GroundFloor", levels);
            Transform upperGroup = Group("UpperFloor", levels);
            Transform basementGroup = Group("Basement", levels);
            Transform flights = Group("Flights", root);

            // 1. Ground Floor (y = 0)
            // Academic center, Auditorium, Cafeteria, Gymnasium
            for (float x = 0f; x < 100f; x += L.Tile)
            for (float z = 20f; z < 80f; z += L.Tile)
            {
                // Only the actual wing footprint receives a slab.
                bool inWest = L.WestWing.Contains(new Vector2(x + 2f, z + 2f));
                bool inCenter = L.CenterWing.Contains(new Vector2(x + 2f, z + 2f));
                bool inNorth = L.NorthWing.Contains(new Vector2(x + 2f, z + 2f));
                bool inEast = L.EastWing.Contains(new Vector2(x + 2f, z + 2f));
                if (!inWest && !inCenter && !inNorth && !inEast) continue;

                Vector2Int cell = new(Mathf.RoundToInt(x / L.Tile), Mathf.RoundToInt(z / L.Tile));
                // Ground stays continuous under the stair; openings belong to the floor above it.

                Material floorMat = inEast ? art.GymWood : inWest ? (z >= 48f ? art.StageWood : art.Terrazzo) : (z >= 52f ? art.FloorTile : art.Terrazzo);
                SurfaceMaterial surfaceMat = inEast ? SurfaceMaterial.Wood : inWest && z >= 48f ? SurfaceMaterial.Wood : SurfaceMaterial.Concrete;

                Transform tile = CreateTile(groundGroup, $"Tile_0_{cell.x}_{cell.y}", new Vector3(x + L.Tile * 0.5f, 0f, z + L.Tile * 0.5f), floorMat, surfaceMat);
                result.Pieces.Add(new TilePiece { Transform = tile, Floor = 0, Cell = cell, Collapsible = false });
            }

            // 2. Upper Floor (y = StoryHeight = 4.0m)
            // Only over Academic Center and North Wing (Library). Gymnasium and Auditorium are double-height.
            for (float x = 24f; x < 68f; x += L.Tile)
            for (float z = 20f; z < 80f; z += L.Tile)
            {
                Vector2Int cell = new(Mathf.RoundToInt(x / L.Tile), Mathf.RoundToInt(z / L.Tile));
                // Central stairwell hole
                if (cell == new Vector2Int(11, 9) || cell == new Vector2Int(11, 10)) continue;
                if (cell.x == 16 && (cell.y == 11 || cell.y == 12)) continue;

                // Hallways are protected; classrooms and science labs have some weak sections that can collapse
                bool isHallway = (z >= 32f && z < 40f) || (x >= 40f && x < 52f) ||
                                 (x >= 60f && z >= 40f && z < 56f);
                bool isWeak = !isHallway && ((cell.x + cell.y) % 4 == 0);

                Material floorMat = art.FloorTile;
                Transform tile = CreateTile(upperGroup, $"Tile_1_{cell.x}_{cell.y}", new Vector3(x + L.Tile * 0.5f, L.StoryHeight, z + L.Tile * 0.5f), floorMat, SurfaceMaterial.Concrete);

                result.Pieces.Add(new TilePiece
                {
                    Transform = tile,
                    Floor = 1,
                    Cell = cell,
                    Collapsible = isWeak,
                    Health = isWeak ? 0.65f : 1.0f,
                    Capacity = isWeak ? 0.55f : 1.0f
                });
            }

            // 3. Basement / Boiler Room (y = -3.5m, Z: 80 to 92)
            for (float x = 44f; x < 68f; x += L.Tile)
            for (float z = 80f; z < 92f; z += L.Tile)
            {
                Vector2Int cell = new(Mathf.RoundToInt(x / L.Tile), Mathf.RoundToInt(z / L.Tile));
                Transform tile = CreateTile(basementGroup, $"Tile_B_{cell.x}_{cell.y}", new Vector3(x + L.Tile * 0.5f, -3.5f, z + L.Tile * 0.5f), art.Concrete, SurfaceMaterial.Concrete);
                result.Pieces.Add(new TilePiece { Transform = tile, Floor = -1, Cell = cell, Collapsible = false });
            }

            // A real utility lobby/ceiling, with the service stair cut out of the upper slab.
            for (float x = 44f; x < 68f; x += L.Tile)
            for (float z = 80f; z < 92f; z += L.Tile)
            {
                if (x == 48f && z < 88f) continue;
                Vector2Int cell = new(Mathf.RoundToInt(x / L.Tile), Mathf.RoundToInt(z / L.Tile));
                Transform tile = CreateTile(groundGroup, $"Tile_0_{cell.x}_{cell.y}", new Vector3(x + 2f, 0f, z + 2f), art.Concrete, SurfaceMaterial.Concrete);
                result.Pieces.Add(new TilePiece { Transform = tile, Floor = 0, Cell = cell });
            }

            // The second flight ends halfway through a grid cell; a half-depth landing meets its ramp.
            Transform eastLanding = CreateTile(upperGroup, "EastStairLanding", new Vector3(66f, 4f, 51f), art.FloorTile,
                SurfaceMaterial.Concrete, new Vector2(4f, 2f));
            result.Pieces.Add(new TilePiece { Transform = eastLanding, Floor = 1, Cell = new Vector2Int(16, 12) });

            // 4. Stairs
            // Central Stairwell (Ground to Upper Hall, X: 46, Z: 36 to 44)
            Transform mainStair = BuildStair(flights, "CentralAcademicStair", new Vector3(46f, 0f, 36f), Vector3.forward, L.StoryHeight, 3.0f, art.MetalSteel);
            result.Stairs.Add(mainStair);

            Transform eastStair = BuildStair(flights, "EastReturnStair", new Vector3(66f, 0f, 42f), Vector3.forward, L.StoryHeight, 2.6f, art.MetalSteel);
            result.Stairs.Add(eastStair);

            // Boiler Room Basement Stair (Basement to Ground, X: 50, Z: 88 to 80)
            Transform boilerStair = BuildStair(flights, "BoilerServiceStair", new Vector3(50f, -3.5f, 88f), Vector3.back, 3.5f, 2.6f, art.MetalSteel);
            result.Stairs.Add(boilerStair);

            BuildOpeningGuard(flights, art, "AcademicVoidGuard", new Vector3(46f, 4f, 40f), 4f, 8f, openNorth: true);
            BuildOpeningGuard(flights, art, "EastVoidGuard", new Vector3(66f, 4f, 47f), 4f, 6f, openNorth: true);
            BuildOpeningGuard(flights, art, "ServiceVoidGuard", new Vector3(50f, 0f, 84f), 4f, 8f, openNorth: false);

            // Bind to Structural Gameplay
            var setup = new StructureSceneSetup();
            foreach (TilePiece piece in result.Pieces)
            {
                setup.AddSection(piece.Transform, SectionType.Floor, piece.Collapsible, piece.Health, piece.Capacity);
                if (piece.Collapsible)
                    SectionNavCarver.EditorAdd(piece.Transform.gameObject, new Vector3(L.Tile, L.SlabThickness, L.Tile));
                else
                    piece.Transform.gameObject.AddComponent<SectionKeepClear>();
            }

            foreach (Transform stair in result.Stairs)
                setup.AddSection(stair, SectionType.Stair, false, fractured: false);

            result.Simulation = setup.AddSimulation(stability: 0.76f, seed: 1951);
            return result;
        }

        private static Transform CreateTile(Transform parent, string name, Vector3 position, Material material, SurfaceMaterial surface, Vector2? footprint = null)
        {
            Transform tile = Group(name, parent);
            tile.position = position;
            BoxCollider collider = tile.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.down * (L.SlabThickness * 0.5f);
            Vector2 span = footprint ?? new Vector2(L.Tile, L.Tile);
            collider.size = new Vector3(span.x, L.SlabThickness, span.y);
            Box("Visual", tile, collider.center, collider.size, material, withCollider: false);
            tile.gameObject.AddComponent<SurfaceTag>().EditorSet(surface);
            return tile;
        }

        private static void BuildOpeningGuard(Transform parent, BurleySchoolArt art, string name, Vector3 center,
            float width, float length, bool openNorth)
        {
            Transform guard = Group(name, parent);
            guard.position = center;
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (width * .5f - .08f);
                Box($"TopRail_{side}", guard, new Vector3(x, 1.05f, 0f), new Vector3(.08f, .08f, length), art.LockerGreen, false);
                Box($"MidRail_{side}", guard, new Vector3(x, .55f, 0f), new Vector3(.05f, .05f, length), art.MetalSteel, false);
                // One invisible, continuous barrier keeps carried salvage from clipping through the balusters.
                var barrier = Group($"Barrier_{side}", guard).gameObject.AddComponent<BoxCollider>();
                barrier.center = new Vector3(x, .55f, 0f);
                barrier.size = new Vector3(.1f, 1.1f, length);
                for (int p = 0; p <= Mathf.CeilToInt(length); p++)
                    Box($"Post_{side}_{p}", guard, new Vector3(x, .5f, -length * .5f + length * p / Mathf.CeilToInt(length)),
                        new Vector3(.055f, 1f, .055f), art.MetalSteel, false);
            }
            float closedEnd = (openNorth ? -1f : 1f) * length * .5f;
            Box("EndGuard", guard, new Vector3(0f, .55f, closedEnd), new Vector3(width, 1.1f, .08f), art.LockerGreen);
        }

        public static Transform BuildStair(Transform parent, string name, Vector3 foot, Vector3 direction, float rise, float width, Material material)
        {
            const float length = 8f;
            const int steps = 16;
            Transform root = Group(name, parent);
            root.position = foot;
            root.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            Transform visual = Group("Visual", root);
            float angle = Mathf.Atan2(rise, length) * Mathf.Rad2Deg;
            float slope = Mathf.Sqrt(length * length + rise * rise);

            Transform ramp = Group("Ramp", root);
            ramp.localPosition = new Vector3(0f, rise * 0.5f, length * 0.5f);
            ramp.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(width, 0.25f, slope);

            for (int i = 0; i < steps; i++)
            {
                float t = (i + 0.5f) / steps;
                Box($"Step_{i:00}", visual, new Vector3(0f, rise * t - 0.10f, length * t),
                    new Vector3(width, 0.20f, length / steps), material, withCollider: false);
            }

            foreach (float side in new[] { -1f, 1f })
            {
                GameObject rail = Box(side < 0 ? "Rail_L" : "Rail_R", visual,
                    new Vector3(side * (width * 0.5f + 0.08f), rise * 0.5f + 0.55f, length * 0.5f),
                    new Vector3(0.15f, 1.2f, slope), material, withCollider: true);
                rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            }

            var link = root.gameObject.AddComponent<NavMeshLink>();
            link.startPoint = Vector3.back * 0.75f;
            link.endPoint = Vector3.forward * (length + 0.75f) + Vector3.up * rise;
            link.width = width - 0.5f;
            link.bidirectional = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return root;
        }
    }
}
