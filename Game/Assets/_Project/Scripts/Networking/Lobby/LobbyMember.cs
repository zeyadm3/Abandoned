namespace Abandoned.Networking
{
    /// <summary>One person in a Steam lobby, as the member list shows them.</summary>
    public readonly struct LobbyMember
    {
        public ulong SteamId { get; }
        public string Name { get; }

        public LobbyMember(ulong steamId, string name)
        {
            SteamId = steamId;
            Name = name ?? string.Empty;
        }
    }
}
