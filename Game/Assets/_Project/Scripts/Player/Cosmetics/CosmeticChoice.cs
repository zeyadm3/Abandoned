using System;
using Unity.Netcode;

namespace Abandoned.Player
{
    /// <summary>A player's outfit on the wire: catalog indices.</summary>
    public readonly struct CosmeticChoice : INetworkSerializeByMemcpy, IEquatable<CosmeticChoice>
    {
        public readonly byte Coverall;
        public readonly byte Hat;
        /// <summary>Goggles, a mask, a rucksack... (M10.6); 0 = none.</summary>
        public readonly byte Accessory;

        public CosmeticChoice(byte coverall, byte hat, byte accessory = 0)
        {
            Coverall = coverall;
            Hat = hat;
            Accessory = accessory;
        }

        public CosmeticChoice WithCoverall(int i) => new((byte)i, Hat, Accessory);
        public CosmeticChoice WithHat(int i) => new(Coverall, (byte)i, Accessory);
        public CosmeticChoice WithAccessory(int i) => new(Coverall, Hat, (byte)i);

        public bool Equals(CosmeticChoice other) => Coverall == other.Coverall && Hat == other.Hat && Accessory == other.Accessory;
        public override bool Equals(object obj) => obj is CosmeticChoice c && Equals(c);
        public override int GetHashCode() => (Coverall * 256 + Hat) * 256 + Accessory;
    }
}
