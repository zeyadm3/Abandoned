using System;
using Unity.Netcode;

namespace Abandoned.Extraction
{
    /// <summary>What every player sees of the run: phase, haul vs quota, cargo and the clocks (server time).</summary>
    public struct RunNetState : INetworkSerializable, IEquatable<RunNetState>
    {
        public RunPhase Phase;
        public int Quota, Haul, Seed;
        /// <summary>Danger level (M5.6): rises over the run, sharply after the window closes.</summary>
        public byte Danger;
        /// <summary>From the contract: the window's length (s), the payout bonus, lights off.</summary>
        public float Window, Bonus;
        public bool PowerOff;
        /// <summary>M9.2 modifiers every machine shows: after dark, a storm.</summary>
        public bool Night, Storm;
        public float CargoVolume, CargoCapacity;
        public double WindowEnd, HonkEnd;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Phase);
            s.SerializeValue(ref Quota);
            s.SerializeValue(ref Haul);
            s.SerializeValue(ref Seed);
            s.SerializeValue(ref Danger);
            s.SerializeValue(ref Window);
            s.SerializeValue(ref Bonus);
            s.SerializeValue(ref PowerOff);
            s.SerializeValue(ref Night);
            s.SerializeValue(ref Storm);
            s.SerializeValue(ref CargoVolume);
            s.SerializeValue(ref CargoCapacity);
            s.SerializeValue(ref WindowEnd);
            s.SerializeValue(ref HonkEnd);
        }

        public bool Overloaded => CargoVolume > CargoCapacity;

        public bool Equals(RunNetState o) => Phase == o.Phase && Quota == o.Quota && Haul == o.Haul && Seed == o.Seed && Danger == o.Danger && Window.Equals(o.Window) && Bonus.Equals(o.Bonus) && PowerOff == o.PowerOff && Night == o.Night && Storm == o.Storm &&
                                             CargoVolume.Equals(o.CargoVolume) && CargoCapacity.Equals(o.CargoCapacity) &&
                                             WindowEnd.Equals(o.WindowEnd) && HonkEnd.Equals(o.HonkEnd);
    }
}
