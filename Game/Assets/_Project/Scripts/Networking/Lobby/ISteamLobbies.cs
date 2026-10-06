using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abandoned.Networking
{
    /// <summary>
    /// The slice of Steam matchmaking the lobby flow needs. A seam so the flow is tested with a fake
    /// and never touches the real Steam account. Lobby ids are raw SteamID64s.
    /// </summary>
    public interface ISteamLobbies
    {
        /// <summary>A friends-only, joinable lobby; 0 when Steam refused.</summary>
        Task<ulong> CreateFriendsOnlyAsync(int maxMembers);

        /// <summary>Enters a lobby: null on success, otherwise a reason a player can read.</summary>
        Task<string> JoinAsync(ulong lobbyId);

        void Leave(ulong lobbyId);
        string GetData(ulong lobbyId, string key);
        void SetData(ulong lobbyId, string key, string value);
        void SetJoinable(ulong lobbyId, bool joinable);
        ulong Owner(ulong lobbyId);
        IReadOnlyList<LobbyMember> Members(ulong lobbyId);
        void OpenInviteOverlay(ulong lobbyId);

        /// <summary>The player accepted an invite (or chose "Join game") in the Steam overlay.</summary>
        event Action<ulong> InviteAccepted;

        /// <summary>Someone entered or left a lobby we're in.</summary>
        event Action<ulong> MembersChanged;
    }
}
