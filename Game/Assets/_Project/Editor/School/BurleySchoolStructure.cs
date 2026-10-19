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
    /// Builds the physical and structural floor systems for Jackson P. Burley Middle School.
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
            for (float x = 0f; x < 96f; x += L.Tile)
            for (float z = 20f; z < 80f; z += L.Tile)
            {
                // Verify within building wings
                bool inWest = L.WestWing.Contains(new Vector2(x + 2f, z + 2f));
                bool inCenter = L.CenterWing.Contains(new Vector2(x + 2f, z + 2f));
                bool inNorth = L.NorthWing.Contains(new Vector2(x + 2f, z + 2f));
                bool inEast = L.EastWing.Contains(new Vector2(x + 2f, z + 2f));
                if (!inWest && !inCenter && !inNorth && !inEast) continue;

                Vector2Int cell = new(Mathf.RoundToInt(x / L.Tile), Mathf.RoundToInt(z / L.Tile));
                // Skip stairwell cutouts
                if (cell == new Vector2Int(11, 9) || cell == new Vector2Int(11, 10)) continue; // Central stair
                if (cell == new Vector2Int(13, 19) || cell == new Vector2Int(13, 20)) continue; // Basement stair

                Material floorMat = inEast ? art.GymWood : inWest ? (z >= 48f ? art.StageWood : art.Terrazzo) : art.Terrazzo;
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

                // Hallways are protected; classrooms and science labs have some weak sections that can collapse
                bool isHallway = (z >= 32f && z < 40f) || (x >= 40f && x < 48f);
                bool isWeak = !isHallway && ((cell.x + cell.y) % 4 == 0);

                Material floorMat = (z >= 52f) ? art.FloorTile : art.FloorTile;
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

            // 3. Basement / Boiler Room (y = -3.5m)
            for (float x = 48f; x < 68f; x += L.Tile)
            for (float z = 76f; z < 92f; z += L.Tile)
            {
                Vector2Int cell = new(Mathf.RoundToInt(x / L.Tile), Mathf.RoundToInt(z / L.Tile));
                Transform tile = CreateTile(basementGroup, $"Tile_B_{cell.x}_{cell.y}", new Vector3(x + L.Tile * 0.5f, -3.5f, z + L.Tile * 0.5f), art.Concrete, SurfaceMaterial.Concrete);
                result.Pieces.Add(new TilePiece { Transform = tile, Floor = -1, Cell = cell, Collapsible = false });
            }

            // 4. Stairs
            // Central Stairwell (Ground to Upper Hall)
            Transform mainStair = BuildStair(flights, "CentralAcademicStair", new Vector3(46f, 0f, 36f), Vector3.forward, L.StoryHeight, 3.2f, art.MetalSteel);
            result.Stairs.Add(mainStair);

            // West Wing Secondary Stair
            Transform westStair = BuildStair(flights, "WestWingStair", new Vector3(26f, 0f, 22f), Vector3.forward, L.StoryHeight, 2.8f, art.MetalSteel);
            result.Stairs.Add(westStair);

            // Boiler Room Basement Stair
            Transform boilerStair = BuildStair(flights, "BoilerServiceStair", new Vector3(54f, -3.5f, 76f), Vector3.back, 3.5f, 2.5f, art.MetalSteel);
            result.Stairs.Add(boilerStair);

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

        private static Transform CreateTile(Transform parent, string name, Vector3 position, Material material, SurfaceMaterial surface)
        {
            Transform tile = Group(name, parent);
            tile.position = position;
            BoxCollider collider = tile.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.down * (L.SlabThickness * 0.5f);
            collider.size = new Vector3(L.Tile, L.SlabThickness, L.Tile);
            Box("Visual", tile, collider.center, collider.size, material, withCollider: false);
            tile.gameObject.AddComponent<SurfaceTag>().EditorSet(surface);
            return tile;
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
                    new Vector3(0.12f, 1.1f, slope), material, withCollider: true);
                rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            }

            var link = root.gameObject.AddComponent<NavMeshLink>();
            link.startPoint = Vector3.back * 0.8f;
            link.endPoint = Vector3.forward * (length + 0.8f) + Vector3.up * rise;
            link.width = width - 0.35f;
            link.bidirectional = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return root;
        }
    }
}
