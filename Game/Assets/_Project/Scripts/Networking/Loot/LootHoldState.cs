using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>
    /// Who has a loot item, as the host decided it. Every machine mirrors this onto its own copies of
    /// the item and the holder's player, so remote players' carried weight and hidden pocket items match.
    /// </summary>
    public struct LootHoldState : INetworkSerializeByMemcpy
    {
        public LootHoldMode Mode;
        /// <summary>NetworkObjectId of the holder's player object (unused when <see cref="Mode"/> is Free).</summary>
        public ulong HolderObjectId;

        public static LootHoldState Free => default;

        public static LootHoldState For(LootHoldMode mode, ulong holderObjectId) =>
            mode == LootHoldMode.Free ? Free : new LootHoldState { Mode = mode, HolderObjectId = holderObjectId };

        public bool IsHeldBy(ulong holderObjectId) => Mode != LootHoldMode.Free && HolderObjectId == holderObjectId;

        public override string ToString() => Mode == LootHoldMode.Free ? "free" : $"{Mode.ToString().ToLowerInvariant()} by #{HolderObjectId}";
    }
}
