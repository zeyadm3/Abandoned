using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abandoned.Networking;

namespace Abandoned.Tests
{
    /// <summary>In-memory Steam lobbies (copy of the EditMode fake; test assemblies don't share code).</summary>
    public sealed class FakeSteamLobbies : ISteamLobbies
    {
        public sealed class FakeLobby
        {
            public ulong Owner;
            public int MaxMembers;
            public bool FriendsOnly = true;
            public bool Joinable = true;
            public readonly Dictionary<string, string> Data = new();
            public readonly List<LobbyMember> Members = new();
        }

        public readonly Dictionary<ulong, FakeLobby> Lobbies = new();
        public readonly List<ulong> Left = new();
        public readonly List<ulong> InvitesOpened = new();
        public ulong LocalId = 76561190000000001UL;
        public string LocalName = "Me";
        public bool RefuseCreate;
        /// <summary>Set: the next create waits for this to complete (Steam being slow).</summary>
        public TaskCompletionSource<bool> CreateGate;
        public string JoinError;
        private ulong nextId = 109775240000000001UL;

        public event Action<ulong> InviteAccepted;
        public event Action<ulong> MembersChanged;

        public FakeLobby Add(ulong owner, string ownerName, int max = 4)
        {
            var lobby = new FakeLobby { Owner = owner, MaxMembers = max };
            lobby.Members.Add(new LobbyMember(owner, ownerName));
            Lobbies[nextId++] = lobby;
            return lobby;
        }

        public ulong IdOf(FakeLobby lobby) => Lobbies.First(p => p.Value == lobby).Key;

        public async Task<ulong> CreateFriendsOnlyAsync(int maxMembers)
        {
            if (CreateGate != null) await CreateGate.Task;
            if (RefuseCreate) return 0UL;
            FakeLobby lobby = Add(LocalId, LocalName, maxMembers);
            return IdOf(lobby);
        }

        public Task<string> JoinAsync(ulong lobbyId)
        {
            if (JoinError != null) return Task.FromResult(JoinError);
            if (!Lobbies.TryGetValue(lobbyId, out FakeLobby lobby)) return Task.FromResult(LobbyMessages.Gone);
            if (lobby.Members.Count >= lobby.MaxMembers) return Task.FromResult(LobbyMessages.Full);
            lobby.Members.Add(new LobbyMember(LocalId, LocalName));
            return Task.FromResult<string>(null);
        }

        public void Leave(ulong lobbyId)
        {
            Left.Add(lobbyId);
            if (Lobbies.TryGetValue(lobbyId, out FakeLobby lobby)) lobby.Members.RemoveAll(m => m.SteamId == LocalId);
        }

        public string GetData(ulong lobbyId, string key) =>
            Lobbies.TryGetValue(lobbyId, out FakeLobby l) && l.Data.TryGetValue(key, out string v) ? v : string.Empty;

        public void SetData(ulong lobbyId, string key, string value) => Lobbies[lobbyId].Data[key] = value;
        public void SetJoinable(ulong lobbyId, bool joinable) => Lobbies[lobbyId].Joinable = joinable;
        public ulong Owner(ulong lobbyId) => Lobbies.TryGetValue(lobbyId, out FakeLobby l) ? l.Owner : 0UL;
        public IReadOnlyList<LobbyMember> Members(ulong lobbyId) =>
            Lobbies.TryGetValue(lobbyId, out FakeLobby l) ? l.Members.ToList() : new List<LobbyMember>();
        public void OpenInviteOverlay(ulong lobbyId) => InvitesOpened.Add(lobbyId);

        public void AcceptInvite(ulong lobbyId) => InviteAccepted?.Invoke(lobbyId);

        public void MemberJoins(ulong lobbyId, ulong id, string name)
        {
            Lobbies[lobbyId].Members.Add(new LobbyMember(id, name));
            MembersChanged?.Invoke(lobbyId);
        }
    }
}
