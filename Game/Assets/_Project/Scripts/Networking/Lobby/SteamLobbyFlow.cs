using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The Steam lobby rules, free of Unity objects so they are tested with fakes: hosting over Steam
    /// opens a friends-only lobby (max = MaxPlayers) tagged with our build's compatibility key and the
    /// host's SteamID; friends join through the overlay's invite or "Join game"; joining checks the
    /// version before connecting NGO to the lobby's host over the Facepunch (Steam relay) transport.
    /// The lobby is only the meeting point: NGO's connection approval still has the final say.
    /// </summary>
    public sealed class SteamLobbyFlow
    {
        public const string VersionKey = "abandoned_version";
        public const string HostKey = "abandoned_host";

        private static readonly IReadOnlyList<LobbyMember> NoMembers = Array.Empty<LobbyMember>();

        private readonly ISteamLobbies lobbies;
        private readonly ILobbySession session;
        private bool creating;

        public SteamLobbyFlow(ISteamLobbies lobbies, ILobbySession session)
        {
            this.lobbies = lobbies ?? throw new ArgumentNullException(nameof(lobbies));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            lobbies.InviteAccepted += OnInviteAccepted;
            lobbies.MembersChanged += OnMembersChanged;
        }

        /// <summary>The lobby we're in (0 = none).</summary>
        public ulong LobbyId { get; private set; }
        public bool InLobby => LobbyId != 0UL;
        public bool Joining { get; private set; }
        public string LastError { get; private set; } = string.Empty;
        public IReadOnlyList<LobbyMember> Members => InLobby ? lobbies.Members(LobbyId) : NoMembers;

        public event Action Changed;

        public void Detach()
        {
            lobbies.InviteAccepted -= OnInviteAccepted;
            lobbies.MembersChanged -= OnMembersChanged;
            LeaveLobby();
        }

        /// <summary>Host side: called when hosting started. Only Steam sessions get a lobby.</summary>
        public async Task OnHostStarted()
        {
            if (!session.UsesSteamTransport || !session.IsHost || InLobby || creating) return;
            creating = true;
            ulong id;
            try
            {
                id = await lobbies.CreateFriendsOnlyAsync(session.MaxPlayers);
            }
            finally
            {
                creating = false;
            }
            if (id == 0UL)
            {
                Fail(LobbyMessages.CreateFailed);
                return;
            }
            // Hosting may have stopped while Steam was creating the lobby: don't leave an empty one behind.
            if (!session.IsRunning || !session.IsHost)
            {
                lobbies.Leave(id);
                return;
            }
            lobbies.SetData(id, VersionKey, session.CompatibilityKey);
            lobbies.SetData(id, HostKey, session.LocalSteamId.ToString());
            LobbyId = id;
            LastError = string.Empty;
            Debug.Log($"[Steam] Hosting friends-only lobby {id} (max {session.MaxPlayers}).");
            Changed?.Invoke();
        }

        /// <summary>The session ended (left, kicked, host gone, failed to connect): leave the lobby too.</summary>
        public void OnSessionStopped()
        {
            if (Joining) return;
            LeaveLobby();
        }

        /// <summary>
        /// Between runs the host keeps the lobby open; during a run nobody may join (players join only
        /// at HQ), so the run flow closes it and reopens it back at HQ.
        /// </summary>
        public void SetJoinable(bool joinable)
        {
            if (InLobby && session.IsHost) lobbies.SetJoinable(LobbyId, joinable);
        }

        public void OpenInviteOverlay()
        {
            if (InLobby) lobbies.OpenInviteOverlay(LobbyId);
        }

        /// <summary>Joins a friend's lobby (invite, "Join game" or +connect_lobby). Returns whether NGO started connecting.</summary>
        public async Task<bool> JoinAsync(ulong lobbyId)
        {
            if (Joining) return Fail(LobbyMessages.Busy);
            if (session.IsRunning || InLobby) return Fail(LobbyMessages.AlreadyInGame);
            Joining = true;
            LastError = string.Empty;
            Changed?.Invoke();
            try
            {
                string error = await lobbies.JoinAsync(lobbyId);
                if (error != null) return Fail(error);
                // The player may have started hosting (and opened their own lobby) while Steam was answering.
                if (session.IsRunning || InLobby)
                {
                    lobbies.Leave(lobbyId);
                    return Fail(LobbyMessages.AlreadyInGame);
                }

                string hostKey = lobbies.GetData(lobbyId, VersionKey);
                if (!VersionInfo.AreCompatible(hostKey, session.CompatibilityKey))
                {
                    lobbies.Leave(lobbyId);
                    return Fail(LobbyMessages.VersionMismatch(hostKey, session.CompatibilityKey));
                }

                ulong host = ulong.TryParse(lobbies.GetData(lobbyId, HostKey), out ulong id) && id != 0UL ? id : lobbies.Owner(lobbyId);
                if (host == 0UL)
                {
                    lobbies.Leave(lobbyId);
                    return Fail(LobbyMessages.NoHost);
                }

                LobbyId = lobbyId;
                if (!session.JoinSteamHost(host, out string joinError))
                {
                    LeaveLobby();
                    return Fail(joinError);
                }
                Debug.Log($"[Steam] Joined lobby {lobbyId}; connecting to host {host}.");
                return true;
            }
            finally
            {
                Joining = false;
                Changed?.Invoke();
            }
        }

        private void LeaveLobby()
        {
            if (!InLobby) return;
            lobbies.Leave(LobbyId);
            Debug.Log($"[Steam] Left lobby {LobbyId}.");
            LobbyId = 0UL;
            Changed?.Invoke();
        }

        private async void OnInviteAccepted(ulong lobbyId)
        {
            try
            {
                await JoinAsync(lobbyId);
            }
            catch (Exception e)
            {
                Fail($"Couldn't join the Steam lobby ({e.Message}).");
            }
        }

        private void OnMembersChanged(ulong lobbyId)
        {
            if (lobbyId == LobbyId) Changed?.Invoke();
        }

        private bool Fail(string message)
        {
            LastError = message ?? string.Empty;
            Debug.LogWarning($"[Steam] {LastError}");
            Changed?.Invoke();
            return false;
        }
    }
}
