using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace Abandoned.Networking
{
    /// <summary>Steam lobbies through Facepunch.Steamworks. Only created once Steam is initialised.</summary>
    public sealed class FacepunchSteamLobbies : ISteamLobbies, IDisposable
    {
        public event Action<ulong> InviteAccepted;
        public event Action<ulong> MembersChanged;

        public FacepunchSteamLobbies()
        {
            SteamFriends.OnGameLobbyJoinRequested += OnJoinRequested;
            SteamMatchmaking.OnLobbyMemberJoined += OnMemberChanged;
            SteamMatchmaking.OnLobbyMemberLeave += OnMemberChanged;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnMemberChanged;
        }

        public void Dispose()
        {
            SteamFriends.OnGameLobbyJoinRequested -= OnJoinRequested;
            SteamMatchmaking.OnLobbyMemberJoined -= OnMemberChanged;
            SteamMatchmaking.OnLobbyMemberLeave -= OnMemberChanged;
            SteamMatchmaking.OnLobbyMemberDisconnected -= OnMemberChanged;
        }

        public async Task<ulong> CreateFriendsOnlyAsync(int maxMembers)
        {
            Lobby? created = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
            if (!created.HasValue) return 0UL;
            Lobby lobby = created.Value;
            lobby.SetFriendsOnly();
            lobby.SetJoinable(true);
            return lobby.Id.Value;
        }

        public async Task<string> JoinAsync(ulong lobbyId)
        {
            RoomEnter result = await new Lobby(lobbyId).Join();
            return result == RoomEnter.Success ? null : LobbyMessages.ForRoomEnter(result.ToString());
        }

        public void Leave(ulong lobbyId) => new Lobby(lobbyId).Leave();
        public string GetData(ulong lobbyId, string key) => new Lobby(lobbyId).GetData(key) ?? string.Empty;
        public void SetData(ulong lobbyId, string key, string value) => new Lobby(lobbyId).SetData(key, value);
        public void SetJoinable(ulong lobbyId, bool joinable) => new Lobby(lobbyId).SetJoinable(joinable);
        public ulong Owner(ulong lobbyId) => new Lobby(lobbyId).Owner.Id.Value;

        public IReadOnlyList<LobbyMember> Members(ulong lobbyId) =>
            new Lobby(lobbyId).Members.Select(f => new LobbyMember(f.Id.Value, f.Name)).ToList();

        public void OpenInviteOverlay(ulong lobbyId) => SteamFriends.OpenGameInviteOverlay(lobbyId);

        private void OnJoinRequested(Lobby lobby, SteamId friend) => InviteAccepted?.Invoke(lobby.Id.Value);
        private void OnMemberChanged(Lobby lobby, Friend member) => MembersChanged?.Invoke(lobby.Id.Value);
    }
}
