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
    /// <summary>Starting and ending sessions: the transport, hosting, joining, leaving, and the status line.</summary>
    public sealed partial class NetworkBootstrap
    {
        /// <summary>Chooses the transport for the next session. Refused while one is running.</summary>
        public bool SelectTransport(TransportMode mode)
        {
            if (IsRunning) return Fail(Loc.Mark("Disconnect before switching transport."));
            transportChosen = true;
            ApplyTransport(mode);
            return true;
        }

        private void ApplyTransport(TransportMode mode)
        {
            Transport = mode;
            networkManager.NetworkConfig.NetworkTransport = mode == TransportMode.Steam
                ? facepunchTransport
                : unityTransport;
            StateChanged?.Invoke();
        }

        // A Steam build (release or demo) plays over Steam once Steam is up, unless the player or a launch flag
        // picked a transport: hosting over Direct IP there opens no lobby, so "Invite friends" would do nothing,
        // and listening on a UDP port asks for a firewall exception. Dev and shareable builds keep the config's
        // default (Unity Transport) for MPPM and LAN testing.
        private void PreferSteamInSteamBuilds()
        {
            if (transportChosen || !sceneSession || networkManager == null || IsRunning || Transport == TransportMode.Steam) return;
            SteamBootstrap steam = SteamBootstrap.Instance;
            if (steam == null || !steam.IsAvailable || !Launch.IsPublicBuild) return;
            transportChosen = true;
            ApplyTransport(TransportMode.Steam);
        }

        /// <summary>Hosts with the configured port (solo play is hosting with nobody else).</summary>
        public bool StartHost() => StartHost(config.Port, config.ListenAddress);

        public bool StartHost(ushort port, string listenAddress)
        {
            if (!CanStart()) return false;
            if (Transport == TransportMode.Steam)
            {
                if (!EnsureSteam()) return false;
            }
            else
            {
                unityTransport.SetConnectionData(true, LoopbackAddress, port, listenAddress);
            }

            slots.Clear();
            LastError = string.Empty;
            JoinTarget = string.Empty;
            BeginSession();
            if (!networkManager.StartHost())
                return Fail(Transport == TransportMode.UnityTransport
                    ? Loc.Wire("Couldn't host on port {0}. Is another game already using it?", port)
                    : Loc.Mark("Couldn't host over Steam."));
            Debug.Log($"[Net] Hosting over {Transport}{(Transport == TransportMode.UnityTransport ? $" on port {HostPort}" : "")}.");
            StateChanged?.Invoke();
            return true;
        }

        /// <summary>0.23.5: what this machine last tried to join (address:port or SteamID64), for "Rejoin your crew".</summary>
        public string LastJoinTarget { get; private set; }

        /// <summary>Joins a host: "address[:port]" for Unity Transport, a SteamID64 for Steam.</summary>
        public bool StartClient(string address, ushort port = 0)
        {
            LastJoinTarget = port != 0 ? $"{address}:{port}" : address;
            if (!CanStart()) return false;
            if (string.IsNullOrWhiteSpace(address)) return Fail(Loc.Mark("Enter an address to join."));
            NetworkLaunchArgs.SplitAddress(address, out string host, out ushort parsedPort);
            if (port == 0) port = parsedPort != 0 ? parsedPort : config.Port;

            if (Transport == TransportMode.Steam)
            {
                if (!ulong.TryParse(host, out ulong steamId) || steamId == 0)
                    return Fail(Loc.Mark("For Steam, enter the host's SteamID64 (invites arrive with the lobby)."));
                facepunchTransport.targetSteamId = steamId;
                JoinTarget = $"Steam {steamId}";
                if (!EnsureSteam()) return false;
            }
            else
            {
                unityTransport.SetConnectionData(true, host, port);
                JoinTarget = $"{host}:{port}";
            }

            LastError = string.Empty;
            BeginSession();
            if (!networkManager.StartClient()) return Fail(Loc.Wire("Couldn't start joining {0}.", JoinTarget));
            Debug.Log($"[Net] Joining {JoinTarget} over {Transport}.");
            StateChanged?.Invoke();
            return true;
        }

        public void Disconnect()
        {
            if (networkManager == null || !networkManager.IsListening) return;
            leavingOnPurpose = true;
            TellClientsTheHostLeft();
            networkManager.Shutdown();
            StateChanged?.Invoke();
        }

        private void BeginSession()
        {
            networkManager.NetworkConfig.ConnectionData = ConnectionGate.Payload(CompatibilityKey);
            wasInSession = leavingOnPurpose = false;
            SessionEndNotice.Clear();
        }

        // Without a reason clients can only guess between "host quit" and "network died".
        private void TellClientsTheHostLeft()
        {
            if (networkManager == null || !networkManager.IsListening || !networkManager.IsServer) return;
            foreach (ulong id in networkManager.ConnectedClientsIds.ToArray())
                if (id != NetworkManager.ServerClientId) networkManager.DisconnectClient(id, SessionMessages.HostLeft);
        }

        /// <summary>One line for menus and the F1 view.</summary>
        public string Status
        {
            get
            {
                if (networkManager == null) return Loc.T("No NetworkManager");
                if (networkManager.ShutdownInProgress) return Loc.T("Disconnecting...");
                if (!networkManager.IsListening) return Loc.T("Offline");
                if (networkManager.IsHost)
                    return Transport == TransportMode.UnityTransport
                        ? Loc.T("Hosting ({0}/{1}) over {2}, port {3}", networkManager.ConnectedClientsIds.Count, config.MaxPlayers, Transport, HostPort)
                        : Loc.T("Hosting ({0}/{1}) over {2}", networkManager.ConnectedClientsIds.Count, config.MaxPlayers, Transport);
                return networkManager.IsConnectedClient ? Loc.T("Connected to {0}", JoinTarget) : Loc.T("Connecting to {0}...", JoinTarget);
            }
        }

        private bool CanStart()
        {
            if (networkManager == null || config == null) return Fail(Loc.Mark("Network setup is incomplete."));
            if (networkManager.IsListening || networkManager.ShutdownInProgress) return Fail(Loc.Mark("Already in a session; disconnect first."));
            return true;
        }

        private bool EnsureSteam()
        {
            SteamBootstrap steam = SteamBootstrap.Instance != null ? SteamBootstrap.Instance : SteamBootstrap.Create(config);
            if (steam.IsAvailable || steam.TryInitialize()) return true;
            return Fail(string.IsNullOrEmpty(steam.LastError) ? SteamErrorMessages.NotRunning : steam.LastError);
        }
    }
}
