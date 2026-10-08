using System;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Player;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>The host's door (approval) and what happens as machines connect, leave, or the session stops or fails.</summary>
    public sealed partial class NetworkBootstrap
    {
        private void OnApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string blocked = request.ClientNetworkId != NetworkManager.ServerClientId ? JoinBlocker?.Invoke() : null;
            int slot = -1;
            string reason = blocked;
            if (blocked != null || !ConnectionGate.Admit(request.ClientNetworkId, request.Payload, CompatibilityKey, slots, config.MaxPlayers,
                    out slot, out reason))
            {
                response.Approved = false;
                response.Reason = reason;
                Debug.Log($"[Net] Refused client {request.ClientNetworkId}: {reason}");
                return;
            }
            Pose pose = PlayerSpawnPoint.PoseFor(slot);
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = pose.position;
            response.Rotation = pose.rotation;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (clientId == networkManager.LocalClientId) wasInSession = true;
            StateChanged?.Invoke();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (networkManager.IsServer)
            {
                slots.Release(clientId);
            }
            else if (clientId == networkManager.LocalClientId || clientId == NetworkManager.ServerClientId)
            {
                string reason = SessionMessages.FromHost(networkManager.DisconnectReason);
                LastError = reason.Length > 0 ? reason
                    : wasInSession || networkManager.IsConnectedClient ? SessionMessages.LostHost
                    : SessionMessages.CouldNotJoin(JoinTarget, VersionInfo.Display);
            }
            StateChanged?.Invoke();
        }

        private void OnStopped(bool wasHost)
        {
            slots?.Clear();
            bool ended = wasInSession && !quitting;
            wasInSession = false;
            StateChanged?.Invoke();
            if (ended) SessionEnded?.Invoke(leavingOnPurpose, leavingOnPurpose ? string.Empty : LastError);
        }

        private void OnApplicationQuit() => TellClientsTheHostLeft();

        private void OnTransportFailure()
        {
            LastError = "The network transport failed; the session ended.";
            StateChanged?.Invoke();
        }

        private bool Fail(string message)
        {
            LastError = message;
            Debug.LogWarning($"[Net] {message}");
            StateChanged?.Invoke();
            return false;
        }
    }
}
