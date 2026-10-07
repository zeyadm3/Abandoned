using System;
using Unity.Netcode;

namespace Abandoned.Equipment
{
    /// <summary>How many of one catalog item the company owns (replicated list entry).</summary>
    public struct OwnedGear : INetworkSerializable, IEquatable<OwnedGear>
    {
        public byte Index;
        public short Count;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Index);
            s.SerializeValue(ref Count);
        }

        public bool Equals(OwnedGear o) => Index == o.Index && Count == o.Count;
    }
}
