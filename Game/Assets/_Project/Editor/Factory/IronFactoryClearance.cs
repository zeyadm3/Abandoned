using System.Collections.Generic;
using UnityEngine;
using L = Abandoned.EditorTools.IronFactoryLayout;
using Plan = Abandoned.EditorTools.IronFactoryCatwalkPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Ground-floor space that must stay open: stair flights and their feet, ladder climb spaces, room doors,
    /// the dock lane and a walkway along the lava. (Loot and threat points are placed after the machines,
    /// around them: <see cref="IronFactoryLootPlan"/>, <see cref="IronFactoryThreatPlan"/>.) Machines and hiding places
    /// that would land in one are left out (and reported), never squeezed in.
    /// </summary>
    public sealed class IronFactoryClearance
    {
        private const float PointRadius = 1.8f;
        private const float DoorRadius = 2.2f;
        // An open lane this wide along each bank of the lava: the risky shortcut stays a shortcut.
        private const float LavaLane = 3f;

        private readonly List<(Rect area, string why)> areas = new();
        private readonly List<(Vector2 at, float radius, string why)> points = new();

        public IronFactoryClearance()
        {
            Rect c = L.ChannelMeters;
            areas.Add((new Rect(c.x - LavaLane, c.y - LavaLane, c.width + 2f * LavaLane, c.height + 2f * LavaLane), "the lava edge"));
            areas.Add((new Rect(L.DockLane.x * L.Tile, 0f, L.DockLane.width * L.Tile, L.Channel.y * L.Tile), "the dock lane"));

            foreach (Plan.Stair s in Plan.Stairs)
            {
                if (s.FromLevel != 0) continue;
                // The flight and the cell you step on from: the run's two cells plus the foot cell.
                Vector2Int a = s.Foot, b = s.Foot + s.Direction * 2;
                Vector2Int min = Vector2Int.Min(a, b), max = Vector2Int.Max(a, b);
                areas.Add((new Rect(min.x * L.Tile, min.y * L.Tile, (max.x - min.x + 1) * L.Tile, (max.y - min.y + 1) * L.Tile), s.Name));
            }
            foreach (Plan.LadderSpot l in Plan.Ladders)
            {
                if (l.Level != 1) continue;
                Vector3 edge = L.CellCentre(l.Cell) + new Vector3(l.Side.x, 0f, l.Side.y) * (L.Tile * .5f + 1f);
                points.Add((new Vector2(edge.x, edge.z), 2f, "a ladder foot"));
            }
            foreach (L.Room room in L.Rooms) AddDoors(room);
        }

        // A disc just outside each door, where you step through.
        private void AddDoors(L.Room room)
        {
            Rect r = room.Meters;
            if (room.Doors.HasFlag(L.Door.South)) points.Add((new Vector2(r.center.x, r.yMin - 1.5f), DoorRadius, room.Name + " door"));
            if (room.Doors.HasFlag(L.Door.North)) points.Add((new Vector2(r.center.x, r.yMax + 1.5f), DoorRadius, room.Name + " door"));
            if (room.Doors.HasFlag(L.Door.West)) points.Add((new Vector2(r.xMin - 1.5f, r.center.y), DoorRadius, room.Name + " door"));
            if (room.Doors.HasFlag(L.Door.East)) points.Add((new Vector2(r.xMax + 1.5f, r.center.y), DoorRadius, room.Name + " door"));
        }

        /// <summary>What a footprint would block, or null when it is clear.</summary>
        public string Problem(Bounds footprint)
        {
            var flat = new Rect(footprint.min.x, footprint.min.z, footprint.size.x, footprint.size.z);
            foreach ((Rect area, string why) in areas)
                if (flat.Overlaps(area)) return why;
            foreach ((Vector2 at, float radius, string why) in points)
            {
                Vector2 nearest = new(Mathf.Clamp(at.x, flat.xMin, flat.xMax), Mathf.Clamp(at.y, flat.yMin, flat.yMax));
                if ((nearest - at).sqrMagnitude < radius * radius) return why;
            }
            return null;
        }
    }
}
