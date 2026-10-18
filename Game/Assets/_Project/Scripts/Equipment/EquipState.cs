using System;
using Unity.Netcode;

namespace Abandoned.Equipment
{
    /// <summary>Tools in four shared pockets (catalog indices, -1 = no tool), active pocket and flashlight.</summary>
    public struct EquipState : INetworkSerializable, IEquatable<EquipState>
    {
        public const int SlotCount = 4;
        public sbyte Slot0, Slot1, Slot2, Slot3;
        public byte Active;
        public bool LightOn;

        public static EquipState Empty => new() { Slot0 = -1, Slot1 = -1, Slot2 = -1, Slot3 = -1 };

        public int this[int slot] => slot switch { 0 => Slot0, 1 => Slot1, 2 => Slot2, 3 => Slot3, _ => -1 };

        public EquipState With(int slot, int index)
        {
            EquipState s = this;
            switch (slot)
            {
                case 0: s.Slot0 = (sbyte)index; break;
                case 1: s.Slot1 = (sbyte)index; break;
                case 2: s.Slot2 = (sbyte)index; break;
                case 3: s.Slot3 = (sbyte)index; break;
            }
            return s;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Slot0);
            s.SerializeValue(ref Slot1);
            s.SerializeValue(ref Slot2);
            s.SerializeValue(ref Slot3);
            s.SerializeValue(ref Active);
            s.SerializeValue(ref LightOn);
        }

        public bool Equals(EquipState o) => Slot0 == o.Slot0 && Slot1 == o.Slot1 && Slot2 == o.Slot2 && Slot3 == o.Slot3 && Active == o.Active && LightOn == o.LightOn;
    }
}
