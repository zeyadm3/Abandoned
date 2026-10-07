using System;
using Unity.Netcode;

namespace Abandoned.Player
{
    /// <summary>A player's outfit on the wire: catalog indices.</summary>
    public readonly struct CosmeticChoice : INetworkSerializeByMemcpy, IEquatable<CosmeticChoice>
    {
        public readonly byte Coverall;
        public readonly byte Hat;

        public CosmeticChoice(byte coverall, byte hat)
        {
            Coverall = coverall;
            Hat = hat;
        }

        public bool Equals(CosmeticChoice other) => Coverall == other.Coverall && Hat == other.Hat;
        public override bool Equals(object obj) => obj is CosmeticChoice c && Equals(c);
        public override int GetHashCode() => Coverall * 256 + Hat;
    }
}
