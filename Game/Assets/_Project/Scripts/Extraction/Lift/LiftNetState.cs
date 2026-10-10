using Unity.Netcode;

namespace Abandoned.Extraction
{
    /// <summary>The freight lift as the host sees it (0.16.3): where it is going, since when, and whether it gave way.</summary>
    public struct LiftNetState : INetworkSerializable, System.IEquatable<LiftNetState>
    {
        public byte From, To;
        /// <summary>Server time the trip began; 0 = parked at <see cref="From"/>.</summary>
        public double Start;
        /// <summary>Server time an overloaded trip gives way (0 = it won't), and whether it already has.</summary>
        public double SnapTime;
        public bool Broken;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref From);
            s.SerializeValue(ref To);
            s.SerializeValue(ref Start);
            s.SerializeValue(ref SnapTime);
            s.SerializeValue(ref Broken);
        }

        public bool Equals(LiftNetState o) => From == o.From && To == o.To && Start == o.Start && SnapTime == o.SnapTime && Broken == o.Broken;
    }
}
