using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IronFactoryProps.Kind;
using Kind = Abandoned.EditorTools.IronFactoryCatwalkPlan.Kind;
using L = Abandoned.EditorTools.IronFactoryLayout;
using MK = Abandoned.EditorTools.IronFactoryMachinePlan.MachineKind;
using Plan = Abandoned.EditorTools.IronFactoryMachinePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// L4: dense but readable clutter. Solid props hug what is already there (the outer walls, the machines, the
    /// rooms' walls) and never sit in an aisle's middle, on a stair, a ladder foot, a door, the dock lane, the lava
    /// edge, a machine's danger zone, in front of a hiding place, or on a loot or threat point
    /// (<see cref="IronFactoryClutterKeep"/>). Litter (broken pallets, slag) lies anywhere on the open floor and is
    /// walked over. Chains hang from the catwalks and the roof trusses (never through a walkway); cables sag along
    /// the walls between the frame columns. Seeded: every rebuild is the same.
    /// </summary>
    public static class IronFactoryClutter
    {
        private static readonly K[] WallKinds = { K.Drums, K.Drums, K.Crates, K.Crates, K.ScrapHeap, K.GasBottles, K.ToolCart, K.ToppledDrum, K.OxyTorchCart, K.BilletStack, K.SafetyBollards };
        private static readonly K[] MachineKinds = { K.Drums, K.ScrapHeap, K.ToppledDrum, K.ToolCart, K.Crates, K.OxyTorchCart, K.SlagThimble, K.BilletStack };

        public readonly struct Counts
        {
            public readonly int Solid, Litter, Chains, Cables;
            public Counts(int solid, int litter, int chains, int cables) { Solid = solid; Litter = litter; Chains = chains; Cables = cables; }
        }

        public static Counts Build(Transform root, IronFactoryArt art, IronFactoryClearance clearance,
            IEnumerable<(Plan.Machine plan, Transform root)> machines, Dictionary<Vector2Int, Kind>[] cells,
            IEnumerable<LootPointSpec> loot, IEnumerable<Vector3> threats)
        {
            Transform group = Group("Clutter", root);
            var props = new IronFactoryProps(art);
            var rng = new System.Random(5150);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            var machineList = new List<Plan.Machine>();
            foreach ((Plan.Machine plan, Transform _) in machines) machineList.Add(plan);
            var keep = new IronFactoryClutterKeep(clearance, machineList, loot, threats);
            int solid = 0, litter = 0, chains = 0, cables = 0;

            bool Try(K kind, Vector3 at, float yaw, bool inRoom)
            {
                Vector3 s = IronFactoryProps.Size(kind);
                bool turned = Mathf.Abs(Mathf.Repeat(yaw, 180f) - 90f) < 45f;
                var flat = new Rect(at.x - (turned ? s.z : s.x) * .5f, at.z - (turned ? s.x : s.z) * .5f, turned ? s.z : s.x, turned ? s.x : s.z);
                if (!keep.Fits(flat, inRoom)) return false;
                props.Build(group, kind, at, yaw, rng);
                keep.Take(flat);
                return true;
            }

            // Along the outer walls, just in front of the frame columns.
            float w = L.Width * L.Tile, d = L.Depth * L.Tile;
            for (float t = 3f; t < w - 3f; t += 3f)
            {
                if (rng.NextDouble() < .55) solid += Try(Pick(WallKinds, rng), new Vector3(t + R(-.6f, .6f), 0f, 1.9f), 0f, false) ? 1 : 0;
                if (rng.NextDouble() < .55) solid += Try(Pick(WallKinds, rng), new Vector3(t + R(-.6f, .6f), 0f, d - 1.9f), 180f, false) ? 1 : 0;
            }
            for (float t = 3f; t < d - 3f; t += 3f)
            {
                if (rng.NextDouble() < .55) solid += Try(Pick(WallKinds, rng), new Vector3(1.9f, 0f, t + R(-.6f, .6f)), 90f, false) ? 1 : 0;
                if (rng.NextDouble() < .55) solid += Try(Pick(WallKinds, rng), new Vector3(w - 1.9f, 0f, t + R(-.6f, .6f)), 270f, false) ? 1 : 0;
            }

            // Beside the quiet machines (never a press or a boiler: their striped zones stay clean and readable).
            foreach (Plan.Machine m in machineList)
            {
                if (m.Kind is MK.Press or MK.Boiler) continue;
                Vector3 s = Plan.Size(m);
                for (int i = 0; i < 2; i++)
                {
                    K kind = Pick(MachineKinds, rng);
                    Vector3 ks = IronFactoryProps.Size(kind);
                    int side = rng.Next(4);
                    var dir = side switch { 0 => Vector3.left, 1 => Vector3.right, 2 => Vector3.back, _ => Vector3.forward };
                    float reach = Mathf.Abs(dir.x) * (s.x + ks.x) * .5f + Mathf.Abs(dir.z) * (s.z + ks.z) * .5f + .4f;
                    float slide = (Mathf.Abs(dir.x) > 0f ? s.z : s.x) * R(-.35f, .35f);
                    Vector3 at = new Vector3(m.At.x, 0f, m.At.y) + dir * reach + new Vector3(dir.z, 0f, dir.x) * slide;
                    solid += Try(kind, at, dir.x != 0f ? 90f : 0f, false) ? 1 : 0;
                }
            }

            // In the rooms, along their walls.
            foreach (L.Room room in L.Rooms)
            {
                Rect r = room.Meters;
                for (float x = r.xMin + 1.5f; x < r.xMax - 1.5f; x += 3f)
                {
                    if (rng.NextDouble() < .4) solid += Try(Pick(WallKinds, rng), new Vector3(x, 0f, r.yMin + 1f), 0f, true) ? 1 : 0;
                    if (rng.NextDouble() < .4) solid += Try(Pick(WallKinds, rng), new Vector3(x, 0f, r.yMax - 1f), 180f, true) ? 1 : 0;
                }
            }

            // Litter on the open floor: walked over, so it only keeps off the machines, the lava and the stairs.
            for (int i = 0; i < 220; i++)
            {
                K kind = rng.NextDouble() < .45 ? K.BrokenPallet : K.SlagLump;
                var at = new Vector3(R(2f, w - 2f), 0f, R(2f, d - 2f));
                Vector3 s = IronFactoryProps.Size(kind);
                if (!keep.FitsLitter(new Rect(at.x - s.x * .5f, at.z - s.z * .5f, s.x, s.z))) continue;
                props.Build(group, kind, at, R(0f, 360f), rng);
                litter++;
            }

            // Chains: under tier 1's walkways (bottom 2.4 m up at the lowest) and from the trusses where nothing walks below.
            float deckUnderside = L.Tiers[0] - .16f;
            foreach (Vector2Int cell in cells[1].Keys)
                if (rng.NextDouble() < .2)
                {
                    props.Chain(group, L.CellCentre(cell, deckUnderside) + new Vector3(R(-.7f, .7f), 0f, R(-.7f, .7f)), R(1.4f, deckUnderside - 2.4f));
                    chains++;
                }
            for (float x = 12f; x < w; x += 12f)
            for (float z = 6f; z < d; z += 6f)
            {
                var cell = new Vector2Int(Mathf.FloorToInt(x / L.Tile), Mathf.FloorToInt(z / L.Tile));
                if (rng.NextDouble() > .3 || cells[1].ContainsKey(cell) || cells[2].ContainsKey(cell) || cells[3].ContainsKey(cell)) continue;
                props.Chain(group, new Vector3(x, 18.6f, z), R(3f, 7.5f));
                chains++;
            }

            // Cables sagging between the frame columns along the walls (not across the dock opening).
            for (float x = 12f; x + 12f < w; x += 12f)
            {
                if (x + 12f < L.DockX - L.DockWidth || x > L.DockX + L.DockWidth)
                { props.Cable(group, new Vector3(x, R(6.5f, 8f), 1.1f), new Vector3(x + 12f, R(6.5f, 8f), 1.1f), R(.6f, 1.4f)); cables++; }
                props.Cable(group, new Vector3(x, R(6.5f, 8f), d - 1.1f), new Vector3(x + 12f, R(6.5f, 8f), d - 1.1f), R(.6f, 1.4f));
                cables++;
            }
            for (float z = 12f; z + 12f < d; z += 12f)
            {
                props.Cable(group, new Vector3(1.1f, R(6.5f, 8f), z), new Vector3(1.1f, R(6.5f, 8f), z + 12f), R(.6f, 1.4f));
                props.Cable(group, new Vector3(w - 1.1f, R(6.5f, 8f), z), new Vector3(w - 1.1f, R(6.5f, 8f), z + 12f), R(.6f, 1.4f));
                cables += 2;
            }
            MarkStatic(group);
            return new Counts(solid, litter, chains, cables);
        }

        private static K Pick(K[] kinds, System.Random rng) => kinds[rng.Next(kinds.Length)];
    }
}
