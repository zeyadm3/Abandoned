using System.Collections.Generic;
using Abandoned.Interaction;
using UnityEngine;
using Items = Abandoned.EditorTools.IronFactoryLootItems;
using Kind = Abandoned.EditorTools.IronFactoryCatwalkPlan.Kind;
using L = Abandoned.EditorTools.IronFactoryLayout;
using Machines = Abandoned.EditorTools.IronFactoryMachinePlan;
using Walks = Abandoned.EditorTools.IronFactoryCatwalkPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where the factory's salvage can turn up: beside the machines (what each makes or holds), in the rooms
    /// (records in the offices, tools in the shops), along the catwalks (light things only: weight up there
    /// cracks the steel), and three jackpot sites, one of them the protected bridge over the lava. Every point
    /// is checked clear of machines, hiding places, stairs and the lava, so nothing spawns inside anything.
    /// </summary>
    public static class IronFactoryLootPlan
    {
        private readonly struct Rule
        {
            public readonly string Tag;
            public readonly CarryClass Min, Max;
            public Rule(string tag, CarryClass min, CarryClass max) { Tag = tag; Min = min; Max = max; }
        }

        private static readonly (Vector3 at, string tag)[] Jackpots =
        {
            (new Vector3(62f, L.Tiers[0], 50f), Items.Machinery),   // mid-bridge over the lava, tier 1 (a spine: never falls)
            (new Vector3(18f, 0f, 44f), Items.Foundry),
            (new Vector3(106f, 0f, 30f), Items.Machinery),
        };

        public static List<LootPointSpec> Points(IEnumerable<(Machines.Machine plan, Transform root)> built,
            Dictionary<Vector2Int, Kind>[] cells, out int rejected)
        {
            var blockers = new List<Rect>();
            var machineList = new List<Machines.Machine>();
            foreach ((Machines.Machine plan, Transform _) in built)
            {
                machineList.Add(plan);
                blockers.Add(Grow(Footprint(plan), .5f));
            }
            foreach (Machines.Spot s in Machines.Spots)
            {
                Bounds b = Machines.SpotBounds(s);
                blockers.Add(Grow(new Rect(b.min.x, b.min.z, b.size.x, b.size.z), .4f));
            }
            Rect c = L.ChannelMeters;
            blockers.Add(Grow(c, 1f));
            foreach (Walks.Stair st in Walks.Stairs)
            {
                if (st.FromLevel != 0) continue;
                foreach (Vector2Int r in st.RunCells) blockers.Add(new Rect(r.x * L.Tile, r.y * L.Tile, L.Tile, L.Tile));
            }

            var points = new List<LootPointSpec>();
            int left = 0;
            void Add(Vector3 at, Rule rule, bool ground, bool jackpot = false)
            {
                if (ground && !Clear(at, blockers)) { left++; return; }
                points.Add(new LootPointSpec($"factory_loot_{points.Count:000}", rule.Tag, at, rule.Min, rule.Max, jackpot));
            }

            foreach ((Vector3 at, string tag) in Jackpots) Add(at, new Rule(tag, CarryClass.Pocket, CarryClass.Huge), at.y < .1f, true);

            // Beside each machine: the first clear side.
            foreach (Machines.Machine m in machineList)
            {
                if (!MachineRule(m.Kind, out Rule rule)) continue;
                Vector3 size = Machines.Size(m);
                float gap = rule.Max == CarryClass.Huge ? 1.8f : 1.1f;
                foreach (Vector3 dir in new[] { Vector3.left, Vector3.right, Vector3.back, Vector3.forward })
                {
                    float reach = Mathf.Abs(dir.x) * size.x * .5f + Mathf.Abs(dir.z) * size.z * .5f + gap;
                    Vector3 at = new Vector3(m.At.x, 0f, m.At.y) + dir * reach;
                    if (!Clear(at, blockers) || InRoom(at)) continue;
                    Add(at, rule, true);
                    break;
                }
            }

            // Rooms: a loose grid, 6 m apart, kept off the walls.
            foreach (L.Room room in L.Rooms)
            {
                Rule rule = RoomRule(room.Name);
                Rect r = room.Meters;
                for (float x = r.xMin + 2f; x < r.xMax - 1.5f; x += 6f)
                for (float z = r.yMin + 2f; z < r.yMax - 1.5f; z += 6f)
                    Add(new Vector3(x, 0f, z), rule, true);
            }

            // Catwalks: about one cell in four, light salvage only, never on the narrow bridge.
            for (int level = 1; level < cells.Length; level++)
                foreach (KeyValuePair<Vector2Int, Kind> cell in cells[level])
                {
                    if (cell.Value == Kind.Narrow || (cell.Key.x * 7 + cell.Key.y * 3 + level) % 4 != 0) continue;
                    Vector3 at = L.CellCentre(cell.Key, Walks.Height(level));
                    if (InPulpit(at)) continue;
                    CarryClass max = level == 1 ? CarryClass.TwoHand : CarryClass.OneHand;
                    string tag = level == 3 || (cell.Key.x + cell.Key.y) % 2 == 0 ? Items.Workshop : Items.Foundry;
                    Add(at, new Rule(tag, CarryClass.Pocket, max), false);
                }
            rejected = left;
            return points;
        }

        private static bool MachineRule(Machines.MachineKind kind, out Rule rule)
        {
            switch (kind)
            {
                case Machines.MachineKind.Press:
                case Machines.MachineKind.Hopper: rule = new Rule(Items.Machinery, CarryClass.Pocket, CarryClass.Huge); return true;
                case Machines.MachineKind.Boiler: rule = new Rule(Items.Machinery, CarryClass.Pocket, CarryClass.Heavy); return true;
                case Machines.MachineKind.MillStand: rule = new Rule(Items.Foundry, CarryClass.Pocket, CarryClass.Heavy); return true;
                case Machines.MachineKind.MouldRow: rule = new Rule(Items.Foundry, CarryClass.Pocket, CarryClass.TwoHand); return true;
                case Machines.MachineKind.Pallet: rule = new Rule(Items.Foundry, CarryClass.OneHand, CarryClass.Heavy); return true;
                case Machines.MachineKind.Lathe:
                case Machines.MachineKind.Mill: rule = new Rule(Items.Workshop, CarryClass.Pocket, CarryClass.TwoHand); return true;
                default: rule = default; return false;
            }
        }

        private static Rule RoomRule(string room) => room switch
        {
            "Foremen_Offices" or "Admin" or "Weighbridge_Office" => new Rule(Items.Records, CarryClass.Pocket, CarryClass.Heavy),
            "Foundry_Stores" => new Rule(Items.Foundry, CarryClass.Pocket, CarryClass.Heavy),
            "Pattern_Shop" => new Rule(Items.Workshop, CarryClass.Pocket, CarryClass.TwoHand),
            _ => new Rule(Items.Workshop, CarryClass.Pocket, CarryClass.OneHand),
        };

        private static Rect Footprint(Machines.Machine m)
        {
            Vector3 s = Machines.Size(m);
            return new Rect(m.At.x - s.x * .5f, m.At.y - s.z * .5f, s.x, s.z);
        }

        private static Rect Grow(Rect r, float by) => new(r.x - by, r.y - by, r.width + 2f * by, r.height + 2f * by);

        private static bool Clear(Vector3 at, List<Rect> blockers)
        {
            var p = new Vector2(at.x, at.z);
            if (p.x < 1.5f || p.y < 1.5f || p.x > L.Width * L.Tile - 1.5f || p.y > L.Depth * L.Tile - 1.5f) return false;
            foreach (Rect b in blockers) if (b.Contains(p)) return false;
            return true;
        }

        private static bool InRoom(Vector3 at)
        {
            foreach (L.Room room in L.Rooms) if (room.Meters.Contains(new Vector2(at.x, at.z))) return true;
            return false;
        }

        private static bool InPulpit(Vector3 at) => at.y > 8f && at.x >= 95f && at.x <= 103f && at.z >= 71f && at.z <= 77f;
    }
}
