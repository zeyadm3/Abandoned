using System;
using Unity.Netcode;

namespace Abandoned.Equipment
{
    /// <summary>A player's two hand slots (catalog indices, -1 = empty), which one is in hand, and the flashlight.</summary>
    public struct EquipState : INetworkSerializable, IEquatable<EquipState>
    {
        public sbyte Slot0, Slot1;
        public byte Active;
        public bool LightOn;

        public static EquipState Empty => new() { Slot0 = -1, Slot1 = -1 };

        public int this[int slot] => slot == 0 ? Slot0 : Slot1;

        public EquipState With(int slot, int index)
        {
            EquipState s = this;
            if (slot == 0) s.Slot0 = (sbyte)index;
            else s.Slot1 = (sbyte)index;
            return s;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Slot0);
            s.SerializeValue(ref Slot1);
            s.SerializeValue(ref Active);
            s.SerializeValue(ref LightOn);
        }

        public bool Equals(EquipState o) => Slot0 == o.Slot0 && Slot1 == o.Slot1 && Active == o.Active && LightOn == o.LightOn;
    }
}
