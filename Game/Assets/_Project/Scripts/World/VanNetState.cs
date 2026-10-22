using System;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>What every machine knows about the van, written by the host: fuel, damage, engine, tailgate and steering.</summary>
    public struct VanNetState : INetworkSerializable, IEquatable<VanNetState>
    {
        [Flags]
        private enum Flag : byte { Engine = 1, BrokenDown = 2, RampDown = 4, Lights = 8, Repairing = 16 }

        private ushort fuel; // hundredths of a litre
        private byte damage, repair; // 0..100; repair progress 0..255
        private Flag flags;
        private sbyte steer;

        public float Fuel => fuel / 100f;
        public float Damage => damage;
        public bool EngineOn => (flags & Flag.Engine) != 0;
        public bool BrokenDown => (flags & Flag.BrokenDown) != 0;
        public bool RampDown => (flags & Flag.RampDown) != 0;
        public bool Lights => (flags & Flag.Lights) != 0;
        public bool Repairing => (flags & Flag.Repairing) != 0;
        public float RepairProgress => repair / 255f;
        /// <summary>Front wheel angle as a fraction of full lock (-1..1), for the wheels drawn on clients.</summary>
        public float Steer => steer / 127f;

        public static VanNetState Of(float fuel, float damage, bool engine, bool brokenDown, bool rampDown, bool lights,
            float steer, float repair)
        {
            Flag f = 0;
            if (engine) f |= Flag.Engine;
            if (brokenDown) f |= Flag.BrokenDown;
            if (rampDown) f |= Flag.RampDown;
            if (lights) f |= Flag.Lights;
            if (repair > 0f) f |= Flag.Repairing;
            return new VanNetState
            {
                fuel = (ushort)Mathf.Clamp(Mathf.RoundToInt(fuel * 100f), 0, ushort.MaxValue),
                damage = (byte)Mathf.Clamp(Mathf.RoundToInt(damage), 0, 100),
                repair = (byte)Mathf.Clamp(Mathf.RoundToInt(repair * 255f), 0, 255),
                flags = f,
                // Coarse on purpose: it only turns the drawn wheels, and fewer changes mean fewer sends.
                steer = (sbyte)(Mathf.RoundToInt(Mathf.Clamp(steer, -1f, 1f) * 16f) * 127 / 16),
            };
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref fuel);
            serializer.SerializeValue(ref damage);
            serializer.SerializeValue(ref repair);
            serializer.SerializeValue(ref flags);
            serializer.SerializeValue(ref steer);
        }

        public bool Equals(VanNetState o) => fuel == o.fuel && damage == o.damage && repair == o.repair && flags == o.flags && steer == o.steer;
        public override bool Equals(object obj) => obj is VanNetState o && Equals(o);
        public override int GetHashCode() => HashCode.Combine(fuel, damage, repair, flags, steer);
    }
}
