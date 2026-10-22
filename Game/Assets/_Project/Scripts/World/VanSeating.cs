using System;
using Unity.Netcode;

namespace Abandoned.World
{
    /// <summary>Who sits where in the van, written by the host: one client id per seat (seat 0 drives).</summary>
    public struct VanSeating : INetworkSerializable, IEquatable<VanSeating>
    {
        public const int Count = 4;
        public const ulong Empty = ulong.MaxValue;

        private ulong s0, s1, s2, s3;

        public static VanSeating None => new() { s0 = Empty, s1 = Empty, s2 = Empty, s3 = Empty };

        public ulong this[int seat]
        {
            get => seat switch { 0 => s0, 1 => s1, 2 => s2, 3 => s3, _ => Empty };
            set
            {
                switch (seat)
                {
                    case 0: s0 = value; break;
                    case 1: s1 = value; break;
                    case 2: s2 = value; break;
                    case 3: s3 = value; break;
                }
            }
        }

        /// <summary>The seat this client sits in, or -1.</summary>
        public int SeatOf(ulong client)
        {
            for (int i = 0; i < Count; i++) if (this[i] == client) return i;
            return -1;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref s0);
            serializer.SerializeValue(ref s1);
            serializer.SerializeValue(ref s2);
            serializer.SerializeValue(ref s3);
        }

        public bool Equals(VanSeating o) => s0 == o.s0 && s1 == o.s1 && s2 == o.s2 && s3 == o.s3;
        public override bool Equals(object obj) => obj is VanSeating o && Equals(o);
        public override int GetHashCode() => HashCode.Combine(s0, s1, s2, s3);
    }
}
