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
        /// <summary>2026 remake: which of the eight workers (catalog index).</summary>
        public readonly byte Character;
        /// <summary>2026 remake: the outfit over the skeleton; the coverall colour tints its tint slots.</summary>
        public readonly byte Outfit;

        public CosmeticChoice(byte coverall, byte hat, byte accessory = 0, byte character = 0, byte outfit = 0)
        {
            Coverall = coverall;
            Hat = hat;
            Accessory = accessory;
            Character = character;
            Outfit = outfit;
        }

        public CosmeticChoice WithCoverall(int i) => new((byte)i, Hat, Accessory, Character, Outfit);
        public CosmeticChoice WithHat(int i) => new(Coverall, (byte)i, Accessory, Character, Outfit);
        public CosmeticChoice WithAccessory(int i) => new(Coverall, Hat, (byte)i, Character, Outfit);
        public CosmeticChoice WithCharacter(int i) => new(Coverall, Hat, Accessory, (byte)i, Outfit);
        public CosmeticChoice WithOutfit(int i) => new(Coverall, Hat, Accessory, Character, (byte)i);

        public CosmeticChoice With(CosmeticKind kind, int i) => kind switch
        {
            CosmeticKind.Coverall => WithCoverall(i),
            CosmeticKind.Hat => WithHat(i),
            CosmeticKind.Accessory => WithAccessory(i),
            CosmeticKind.Character => WithCharacter(i),
            _ => WithOutfit(i),
        };

        public int Of(CosmeticKind kind) => kind switch
        {
            CosmeticKind.Coverall => Coverall,
            CosmeticKind.Hat => Hat,
            CosmeticKind.Accessory => Accessory,
            CosmeticKind.Character => Character,
            _ => Outfit,
        };

        public bool Equals(CosmeticChoice other) => Coverall == other.Coverall && Hat == other.Hat && Accessory == other.Accessory
                                                    && Character == other.Character && Outfit == other.Outfit;
        public override bool Equals(object obj) => obj is CosmeticChoice c && Equals(c);
        public override int GetHashCode() => (((Coverall * 256 + Hat) * 256 + Accessory) * 256 + Character) * 31 + Outfit;
    }
}
