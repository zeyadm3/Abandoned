using System.Collections.Generic;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Player;
using Abandoned.Threats;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What the mall must keep clear when objects are made solid (0.14.0): stairs, escalators and haul ramps
    /// with their approaches, doorways, entrances and fallback exits, the truck's approach, spawn points for
    /// players, monsters and loot, and a lane down the middle of every permanent carry-route tile and the
    /// balcony ring. <see cref="SolidObjectsPass"/> leaves an object walk-through, and reports it, rather
    /// than put a box in any of these. <see cref="Reachability"/> then checks the baked NavMesh still
    /// reaches every doorway and monster spawn.
    /// </summary>
    public sealed class MallRouteClearance
    {
        // Free width (m) kept through the middle of a carry-route tile: a crew with a Huge item, or the trolley.
        private const float Lane = 2.4f;
        // How far (m) in from each side of a doorway something fixed to the wall may stand.
        private const float DoorSide = 0.4f;
        // Narrower than this (m) both ways is a post or a column: it never closes a four-metre tile.
        private const float Post = 0.8f;

        private readonly List<(Bounds box, string what)> boxes = new();
        private readonly List<(Vector3 at, float radius, string what)> points = new();
        private readonly List<Vector3> doorSides = new();
        private Bounds truck;
        private bool hasTruck;

        public MallRouteClearance(Transform mall)
        {
            foreach (MallLayout.Flight f in MallLayout.AllFlights)
            {
                Vector3 along = new(Mathf.Abs(f.Dir.x), 0f, Mathf.Abs(f.Dir.y)), across = new(along.z, 0f, along.x);
                Vector3 centre = f.Foot + new Vector3(f.Dir.x, 0f, f.Dir.y) * (f.Length * 0.5f) + Vector3.up * (f.Rise * 0.5f + 1.1f);
                // Two metres of approach at each end, and the full width with a hand's breadth to spare.
                boxes.Add((new Bounds(centre, along * (f.Length + 4f) + across * (f.Width + 0.5f) + Vector3.up * (Mathf.Abs(f.Rise) + 2.4f)),
                    $"the way onto {f.Name}"));
            }
            foreach (MallLayout.Door d in MallLayout.Doors)
            {
                Vector3 a = MallLayout.TileTopCenter(d.A, d.Floor), b = MallLayout.TileTopCenter(d.B, d.Floor);
                Vector3 through = (b - a).normalized, along = new(Mathf.Abs(through.x), 0f, Mathf.Abs(through.z)), across = new(along.z, 0f, along.x);
                float width = d.Wide ? MallWalls.WideWidth : 1.8f;
                Vector3 middle = (a + b) * 0.5f;
                // The lane through the door and a metre and a half either side of it. Not the full opening: what
                // hugs the wall beside a door (a storefront bay on the next panel, or round the corner) is not in the way.
                float lane = Mathf.Max(1.2f, width - DoorSide * 2f);
                boxes.Add((new Bounds(middle + Vector3.up * 1.25f, along * 3f + across * lane + Vector3.up * 2.5f), "a doorway"));
                doorSides.Add(middle + through * 1.2f);
                doorSides.Add(middle - through * 1.2f);
            }
            foreach (var exit in MallLayout.Exterior)
                points.Add((MallLayout.ExteriorPosition(exit), exit.window ? 1.5f : 2.6f, exit.window ? "a fallback exit" : "an entrance"));
            foreach (ThreatSpawnPoint p in mall.GetComponentsInChildren<ThreatSpawnPoint>(true))
                points.Add((p.transform.position, 1.6f, "a monster's spawn point"));
            foreach (LootSpawnPoint p in mall.GetComponentsInChildren<LootSpawnPoint>(true))
                points.Add((p.transform.position, p.MaxClass >= CarryClass.Heavy ? 1.35f : 0.8f, "a loot spawn point"));
            foreach (PlayerSpawnPoint p in Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None))
                points.Add((p.transform.position, 1.5f, "a player spawn point"));
            TruckCargo cargo = mall.GetComponentInChildren<TruckCargo>(true);
            if (cargo == null) return;
            truck = ModelFit.RendererBounds(cargo.gameObject);
            hasTruck = true;
        }

        /// <summary>What a solid box there would be in the way of, or null when it is in nobody's way.</summary>
        public string Conflict(Bounds box)
        {
            foreach ((Bounds keep, string what) in boxes)
                if (keep.Intersects(box)) return $"it would stand in {what}";
            foreach ((Vector3 at, float radius, string what) in points)
            {
                if (at.y < box.min.y - 0.6f || at.y > box.max.y + 0.6f) continue; // another storey
                Vector3 nearest = box.ClosestPoint(at);
                if (new Vector2(nearest.x - at.x, nearest.z - at.z).sqrMagnitude < radius * radius) return $"it would crowd {what}";
            }
            if (hasTruck)
            {
                // Round the truck, not the truck itself: its own wheels and lamps are part of it.
                Bounds near = truck, own = truck;
                near.Expand(6f);
                own.Expand(0.4f);
                if (near.Intersects(box) && !(own.Contains(box.min) && own.Contains(box.max))) return "it would stand in the truck's approach";
            }
            int floor = Mathf.RoundToInt(box.min.y / MallLayout.StoryHeight);
            if (floor < -1 || floor >= MallLayout.Floors) return null;
            // A post is walked round, not blocked by: the entrance canopy's columns stand on carry-route tiles.
            if (Mathf.Max(box.size.x, box.size.z) < Post) return null;
            int x0 = Mathf.FloorToInt(box.min.x / MallLayout.Tile), x1 = Mathf.FloorToInt(box.max.x / MallLayout.Tile);
            int z0 = Mathf.FloorToInt(box.min.z / MallLayout.Tile), z1 = Mathf.FloorToInt(box.max.z / MallLayout.Tile);
            for (int x = x0; x <= x1; x++)
            for (int z = z0; z <= z1; z++)
            {
                var cell = new Vector2Int(x, z);
                if (!MallLayout.InGrid(cell)) continue;
                bool route = MallLayout.IsProtected(cell, floor) || (MallLayout.Ring.Contains(cell) && !MallLayout.Atrium.Contains(cell));
                if (!route) continue;
                var lane = new Bounds(MallLayout.TileTopCenter(cell, floor) + Vector3.up * 1.2f, new Vector3(Lane, 2.4f, Lane));
                if (lane.Intersects(box)) return "it would stand in the middle of a carry route";
            }
            return null;
        }

        /// <summary>
        /// After the NavMesh bake: both sides of every doorway and every monster spawn must still reach the
        /// entrance. Reported, never fatal (a store sealed by its own design would show up here too).
        /// </summary>
        public int Reachability()
        {
            if (!NavMesh.SamplePosition(MallLayout.EntranceTarget, out NavMeshHit entrance, 4f, NavMesh.AllAreas))
            {
                Debug.LogWarning("[MallValidation] Mall: reachability not checked, no NavMesh at the entrance.");
                return 0;
            }
            var targets = new List<(Vector3 at, string what)>();
            foreach (Vector3 side in doorSides) targets.Add((side, "one side of a doorway"));
            foreach ((Vector3 at, float radius, string what) in points)
                if (what == "a monster's spawn point") targets.Add((at, what));
            int cut = 0;
            var path = new NavMeshPath();
            foreach ((Vector3 at, string what) in targets)
            {
                bool reached = NavMesh.SamplePosition(at, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)
                               && NavMesh.CalculatePath(hit.position, entrance.position, NavMesh.AllAreas, path)
                               && path.status == NavMeshPathStatus.PathComplete;
                if (reached) continue;
                cut++;
                Debug.LogWarning($"[MallValidation] Mall: {what} at {at} can't be reached from the entrance on the NavMesh.");
            }
            Debug.Log($"[MallValidation] Mall: reachability: {targets.Count} points checked, {cut} cut off.");
            return cut;
        }
    }
}
