using Abandoned.Extraction;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The machinery floor as data: the dead machines that make the ground floor a maze of cover under the
    /// catwalks, and the hiding places. Positions are metres on the plant floor (Docs/IRON_FACTORY_PLAN.md
    /// for the zones). Anything that would block a stair, ladder, door, the dock lane, the lava edge or a
    /// loot/threat point is left out at build time with a warning (<see cref="IronFactoryClearance"/>).
    /// </summary>
    public static class IronFactoryMachinePlan
    {
        public enum MachineKind : byte { Press, MillStand, RollerTable, Hopper, MouldRow, Lathe, Mill, Boiler, Pallet, Conveyor }

        public readonly struct Machine
        {
            public readonly MachineKind Kind;
            public readonly Vector2 At;
            /// <summary>Turned 90 degrees: its length runs along x instead of z (tables, conveyors) or vice versa.</summary>
            public readonly bool Turned;
            /// <summary>Tables and conveyors: their length; 0 = the kind's own.</summary>
            public readonly float Length;
            public Machine(MachineKind kind, float x, float z, bool turned = false, float length = 0f)
            { Kind = kind; At = new Vector2(x, z); Turned = turned; Length = length; }
        }

        public readonly struct Spot
        {
            public readonly HideSpotKind Kind;
            /// <summary>On the floor at the back of the spot (against a wall or a machine).</summary>
            public readonly Vector2 Back;
            /// <summary>The way its door faces.</summary>
            public readonly Vector2 Out;
            public Spot(HideSpotKind kind, float x, float z, Vector2 facing) { Kind = kind; Back = new Vector2(x, z); Out = facing; }
        }

        /// <summary>Footprint (x, z at no turn) and height of each kind.</summary>
        public static Vector3 Size(MachineKind kind) => kind switch
        {
            MachineKind.Press => new Vector3(4f, 4.2f, 3f),
            MachineKind.MillStand => new Vector3(3f, 3.6f, 2.4f),
            MachineKind.RollerTable => new Vector3(1.4f, .9f, 5f),
            MachineKind.Hopper => new Vector3(3.6f, 4.4f, 3.6f),
            MachineKind.MouldRow => new Vector3(4f, .7f, 1.4f),
            MachineKind.Lathe => new Vector3(3.2f, 1.5f, 1.2f),
            MachineKind.Mill => new Vector3(2f, 2.4f, 2f),
            MachineKind.Boiler => new Vector3(3f, 3.8f, 3f),
            MachineKind.Pallet => new Vector3(2f, 1.2f, 2f),
            _ => new Vector3(1.2f, 1f, 6f),
        };

        public static Vector3 Size(Machine m)
        {
            Vector3 s = Size(m.Kind);
            if (m.Length > 0f) s.z = m.Length;
            return m.Turned ? new Vector3(s.z, s.y, s.x) : s;
        }

        private static readonly Vector2 North = Vector2.up, South = Vector2.down, East = Vector2.right, West = Vector2.left;

        public static readonly Machine[] Machines =
        {
            // ---- West bank: the rolling mill line splits it, with two gaps to cross -------------------
            new(MachineKind.MillStand, 26f, 26f), new(MachineKind.RollerTable, 26f, 31f, length: 7.6f),
            new(MachineKind.MillStand, 26f, 36f), new(MachineKind.RollerTable, 26f, 39.6f, length: 4.8f),
            new(MachineKind.RollerTable, 26f, 48.4f, length: 4.8f), new(MachineKind.MillStand, 26f, 52f),
            new(MachineKind.RollerTable, 26f, 55.6f, length: 4.8f), new(MachineKind.RollerTable, 26f, 64.4f, length: 4.8f),
            new(MachineKind.MillStand, 26f, 68f), new(MachineKind.RollerTable, 26f, 72.6f, length: 6.8f),
            // Ore handling against the west wall, presses and casting moulds toward the lava.
            new(MachineKind.Hopper, 8f, 60f), new(MachineKind.Hopper, 8f, 72f),
            new(MachineKind.Press, 36f, 30f), new(MachineKind.Press, 36f, 56f), new(MachineKind.Press, 40f, 78f),
            new(MachineKind.MouldRow, 43f, 34f), new(MachineKind.MouldRow, 43f, 40f),
            new(MachineKind.MouldRow, 43f, 58f), new(MachineKind.MouldRow, 43f, 66f),
            new(MachineKind.Boiler, 20f, 78f),

            // ---- East bank: the machine shop's lathe aisles, presses and the cross conveyor --------------
            new(MachineKind.Lathe, 86f, 26f), new(MachineKind.Lathe, 86f, 32f), new(MachineKind.Lathe, 86f, 38f),
            new(MachineKind.Lathe, 86f, 44f), new(MachineKind.Lathe, 86f, 50f),
            new(MachineKind.Lathe, 96f, 26f), new(MachineKind.Lathe, 96f, 32f), new(MachineKind.Lathe, 96f, 38f),
            new(MachineKind.Lathe, 96f, 44f), new(MachineKind.Lathe, 96f, 50f),
            new(MachineKind.Mill, 100f, 28f), new(MachineKind.Mill, 100f, 48f),
            new(MachineKind.Press, 86f, 62f), new(MachineKind.Press, 96f, 62f),
            new(MachineKind.Conveyor, 82.5f, 70f, turned: true, length: 9f), new(MachineKind.Conveyor, 94.5f, 70f, turned: true, length: 7f),
            new(MachineKind.Hopper, 112f, 26f), new(MachineKind.Hopper, 114f, 76f),
            new(MachineKind.Boiler, 80f, 80f),

            // ---- Dispatch floor and north concourse: stacked ingots and parts -------------------------
            new(MachineKind.Pallet, 24f, 8f), new(MachineKind.Pallet, 28f, 15f), new(MachineKind.Pallet, 46f, 12f),
            new(MachineKind.Pallet, 74f, 12f), new(MachineKind.Pallet, 90f, 6f), new(MachineKind.Pallet, 106f, 16f),
            new(MachineKind.Pallet, 110f, 10f), new(MachineKind.Pallet, 48f, 92f), new(MachineKind.Pallet, 66f, 94f),
        };

        public static readonly Spot[] Spots = BuildSpots();

        private static Spot[] BuildSpots()
        {
            var spots = new System.Collections.Generic.List<Spot>();
            // Worker lockers: two banks on the locker room's north wall, one on its west wall, and two short
            // banks by the dock for the crew that loaded the trains.
            const float pitch = .82f;
            for (int i = 0; i < 6; i++) spots.Add(new Spot(HideSpotKind.Locker, 3f + i * pitch, 99.8f, South));
            for (int i = 0; i < 6; i++) spots.Add(new Spot(HideSpotKind.Locker, 18f + i * pitch, 99.8f, South));
            for (int i = 0; i < 4; i++) spots.Add(new Spot(HideSpotKind.Locker, .2f, 88f + i * pitch, East));
            for (int i = 0; i < 3; i++) spots.Add(new Spot(HideSpotKind.Locker, 30f + i * pitch, .2f, North));
            for (int i = 0; i < 3; i++) spots.Add(new Spot(HideSpotKind.Locker, 92f + i * pitch, .2f, North));
            // Maintenance bays under the front of the presses and one mill stand.
            spots.Add(new Spot(HideSpotKind.Machine, 36f, 28.5f, South));
            spots.Add(new Spot(HideSpotKind.Machine, 36f, 54.5f, South));
            spots.Add(new Spot(HideSpotKind.Machine, 40f, 76.5f, South));
            spots.Add(new Spot(HideSpotKind.Machine, 86f, 60.5f, South));
            spots.Add(new Spot(HideSpotKind.Machine, 96f, 60.5f, South));
            spots.Add(new Spot(HideSpotKind.Machine, 24.5f, 52f, West));
            // Cold annealing furnaces by the ore hoppers and in the north-east corner.
            spots.Add(new Spot(HideSpotKind.Furnace, 13f, 26f, East));
            spots.Add(new Spot(HideSpotKind.Furnace, 13f, 56f, East));
            spots.Add(new Spot(HideSpotKind.Furnace, 13f, 66f, East));
            spots.Add(new Spot(HideSpotKind.Furnace, 100f, 80f, South));
            // Electrical cabinets in the breaker hall and on the east wall.
            spots.Add(new Spot(HideSpotKind.Cabinet, .2f, 39.5f, East));
            spots.Add(new Spot(HideSpotKind.Cabinet, .2f, 42f, East));
            spots.Add(new Spot(HideSpotKind.Cabinet, 6f, 47.78f, South));
            spots.Add(new Spot(HideSpotKind.Cabinet, 8.5f, 47.78f, South));
            spots.Add(new Spot(HideSpotKind.Cabinet, 119.8f, 18f, West));
            spots.Add(new Spot(HideSpotKind.Cabinet, 119.8f, 20.5f, West));
            return spots.ToArray();
        }

        /// <summary>Width across the door, height, depth from the back to the door.</summary>
        public static Vector3 SpotSize(HideSpotKind kind) => kind switch
        {
            HideSpotKind.Machine => new Vector3(1.2f, 2f, 1f),
            HideSpotKind.Furnace => new Vector3(2.2f, 2.6f, 2f),
            HideSpotKind.Cabinet => new Vector3(1f, 2.1f, .6f),
            _ => new Vector3(.8f, 2f, .7f),
        };

        /// <summary>World footprint of a spot (for the clearance checks).</summary>
        public static Bounds SpotBounds(Spot s)
        {
            Vector3 size = SpotSize(s.Kind);
            Vector3 back = new(s.Back.x, 0f, s.Back.y), outward = new(s.Out.x, 0f, s.Out.y);
            Vector3 centre = back + outward * size.z * .5f + Vector3.up * size.y * .5f;
            bool facesZ = Mathf.Abs(s.Out.y) > .5f;
            return new Bounds(centre, facesZ ? new Vector3(size.x, size.y, size.z) : new Vector3(size.z, size.y, size.x));
        }
    }
}
