using System;
using Unity.Netcode;

namespace Abandoned.Extraction
{
    /// <summary>What every player sees of the run: phase, haul vs quota, cargo and the clocks (server time).</summary>
    public struct RunNetState : INetworkSerializable, IEquatable<RunNetState>
    {
        public RunPhase Phase;
        public int Quota, Haul, Seed;
        public float CargoVolume, CargoCapacity;
        public double WindowEnd, HonkEnd;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Phase);
            s.SerializeValue(ref Quota);
            s.SerializeValue(ref Haul);
            s.SerializeValue(ref Seed);
            s.SerializeValue(ref CargoVolume);
            s.SerializeValue(ref CargoCapacity);
            s.SerializeValue(ref WindowEnd);
            s.SerializeValue(ref HonkEnd);
        }

        public bool Overloaded => CargoVolume > CargoCapacity;

        public bool Equals(RunNetState o) => Phase == o.Phase && Quota == o.Quota && Haul == o.Haul && Seed == o.Seed &&
                                             CargoVolume.Equals(o.CargoVolume) && CargoCapacity.Equals(o.CargoCapacity) &&
                                             WindowEnd.Equals(o.WindowEnd) && HonkEnd.Equals(o.HonkEnd);
    }
}
