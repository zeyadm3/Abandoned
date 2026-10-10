using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    public enum PingKind : byte { Here, Loot, Monster, WeakFloor }

    /// <summary>One ping (0.19.0): what was marked, where, by whose seat, until when; it follows a target object if it has one.</summary>
    public struct PingMark : INetworkSerializable, System.IEquatable<PingMark>
    {
        public Vector3 Position;
        public PingKind Kind;
        public byte Seat;
        public ulong Target; // NetworkObjectId, 0 = a fixed spot
        public double Until;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Position);
            s.SerializeValue(ref Kind);
            s.SerializeValue(ref Seat);
            s.SerializeValue(ref Target);
            s.SerializeValue(ref Until);
        }

        public bool Equals(PingMark o) => Position == o.Position && Kind == o.Kind && Seat == o.Seat && Target == o.Target && Until == o.Until;
    }
}
