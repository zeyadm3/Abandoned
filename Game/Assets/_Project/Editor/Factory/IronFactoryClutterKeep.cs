using System.Collections.Generic;
using UnityEngine;
using L = Abandoned.EditorTools.IronFactoryLayout;
using MK = Abandoned.EditorTools.IronFactoryMachinePlan.MachineKind;
using Plan = Abandoned.EditorTools.IronFactoryMachinePlan;
using Walks = Abandoned.EditorTools.IronFactoryCatwalkPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What clutter must keep off (L4), as plan-view rectangles and points: the machines, the presses' and boilers'
    /// danger zones, the hiding places and the space in front of them, the blast furnace, the rooms (from outside)
    /// and their doorways (from inside), every loot and threat point, the shared clearance (stairs, ladder feet,
    /// outside doors, dock lane, lava edge), and the clutter already placed, with a gap so the aisles stay open.
    /// </summary>
    public sealed class IronFactoryClutterKeep
    {
        private const float Gap = .5f;
        private readonly IronFactoryClearance clearance;
        private readonly List<Rect> hard = new(), litter = new(), taken = new();
        private readonly List<(Vector2 at, float radius)> points = new(), roomDoors = new();

        public IronFactoryClutterKeep(IronFactoryClearance clearance, IEnumerable<Plan.Machine> machines,
            IEnumerable<LootPointSpec> loot, IEnumerable<Vector3> threats)
        {
            this.clearance = clearance;
            foreach (Plan.Machine m in machines)
            {
                Vector3 s = Plan.Size(m);
                var foot = new Rect(m.At.x - s.x * .5f, m.At.y - s.z * .5f, s.x, s.z);
                hard.Add(Grow(foot, .3f));
                litter.Add(Grow(foot, .1f));
                // The striped danger zones (see IronFactoryPower): a press's behind it, a boiler's all round.
                if (m.Kind == MK.Press) hard.Add(Grow(new Rect(foot.x, foot.yMax, foot.width, 1.6f), .5f));
                if (m.Kind == MK.Boiler) hard.Add(Grow(foot, 2f));
            }
            foreach (Plan.Spot spot in Plan.Spots)
            {
                Bounds b = Plan.SpotBounds(spot);
                var foot = new Rect(b.min.x, b.min.z, b.size.x, b.size.z);
                hard.Add(Grow(foot, 1.3f));   // and the floor in front of it, where you climb in and out
                litter.Add(Grow(foot, .2f));
            }
            hard.Add(IronFactoryBlastFurnace.Footprint);
            litter.Add(IronFactoryBlastFurnace.Footprint);
            Rect c = L.ChannelMeters;
            litter.Add(Grow(c, 1.5f));
            foreach (Walks.Stair st in Walks.Stairs)
                if (st.FromLevel == 0)
                    foreach (Vector2Int r in st.RunCells) litter.Add(new Rect(r.x * L.Tile, r.y * L.Tile, L.Tile, L.Tile));
            foreach (LootPointSpec p in loot) points.Add((new Vector2(p.At.x, p.At.z), 1.2f));
            foreach (Vector3 t in threats) points.Add((new Vector2(t.x, t.z), 1.6f));
            foreach (L.Room room in L.Rooms)
            {
                Rect r = room.Meters;
                if (room.Doors.HasFlag(L.Door.South)) roomDoors.Add((new Vector2(r.center.x, r.yMin), 2.2f));
                if (room.Doors.HasFlag(L.Door.North)) roomDoors.Add((new Vector2(r.center.x, r.yMax), 2.2f));
                if (room.Doors.HasFlag(L.Door.West)) roomDoors.Add((new Vector2(r.xMin, r.center.y), 2.2f));
                if (room.Doors.HasFlag(L.Door.East)) roomDoors.Add((new Vector2(r.xMax, r.center.y), 2.2f));
            }
        }

        public bool Fits(Rect flat, bool inRoom)
        {
            if (!InsideHall(flat, .4f)) return false;
            foreach (Rect b in hard) if (b.Overlaps(flat)) return false;
            foreach (Rect b in taken) if (Grow(b, Gap).Overlaps(flat)) return false;
            if (clearance.Problem(new Bounds(new Vector3(flat.center.x, .5f, flat.center.y), new Vector3(flat.width, 1f, flat.height))) != null) return false;
            foreach ((Vector2 at, float radius) in points) if (Near(flat, at, radius)) return false;
            bool insideSome = false;
            foreach (L.Room room in L.Rooms)
            {
                Rect r = room.Meters;
                if (inRoom && Grow(r, -.3f).Contains(flat.min) && Grow(r, -.3f).Contains(flat.max)) insideSome = true;
                else if (!inRoom && Grow(r, .3f).Overlaps(flat)) return false;
            }
            if (inRoom)
            {
                if (!insideSome) return false;
                foreach ((Vector2 at, float radius) in roomDoors) if (Near(flat, at, radius)) return false;
            }
            return true;
        }

        public bool FitsLitter(Rect flat)
        {
            if (!InsideHall(flat, .5f)) return false;
            foreach (Rect b in litter) if (b.Overlaps(flat)) return false;
            foreach (Rect b in taken) if (b.Overlaps(flat)) return false;
            return true;
        }

        public void Take(Rect flat) => taken.Add(flat);

        private static bool InsideHall(Rect r, float margin) =>
            r.xMin > margin && r.yMin > margin && r.xMax < L.Width * L.Tile - margin && r.yMax < L.Depth * L.Tile - margin;

        private static bool Near(Rect r, Vector2 p, float radius)
        {
            var nearest = new Vector2(Mathf.Clamp(p.x, r.xMin, r.xMax), Mathf.Clamp(p.y, r.yMin, r.yMax));
            return (nearest - p).sqrMagnitude < radius * radius;
        }

        private static Rect Grow(Rect r, float by) => new(r.x - by, r.y - by, r.width + 2f * by, r.height + 2f * by);
    }
}
