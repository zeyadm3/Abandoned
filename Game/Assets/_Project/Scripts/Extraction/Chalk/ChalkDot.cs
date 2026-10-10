using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>One dot of spray chalk (0.17.6): where, facing which way, and whose colour.</summary>
    public struct ChalkDot : INetworkSerializable, System.IEquatable<ChalkDot>
    {
        public Vector3 Position, Normal;
        public byte Seat;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Position);
            s.SerializeValue(ref Normal);
            s.SerializeValue(ref Seat);
        }

        public bool Equals(ChalkDot o) => Position == o.Position && Normal == o.Normal && Seat == o.Seat;
    }
}
