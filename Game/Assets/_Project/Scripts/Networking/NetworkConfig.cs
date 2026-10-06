using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>Networking and Steam settings shared by every scene (one asset in Data/Networking).</summary>
    [CreateAssetMenu(menuName = "Abandoned/Networking/Network Config", fileName = "NetworkConfig")]
    public class NetworkConfig : ScriptableObject, IValidatable
    {
        /// <summary>Valve's public test app (Spacewar). Fine for development; the real App ID replaces it before release.</summary>
        public const uint DevelopmentAppId = 480;

        [field: Header("Steam")]
        [field: Tooltip("Steam App ID passed to SteamClient.Init. 480 (Spacewar) during development; the real one from Steam Direct before release. Keep Game/steam_appid.txt in sync.")]
        [field: SerializeField] public uint SteamAppId { get; private set; } = DevelopmentAppId;

        [field: Tooltip("Start Steam automatically when SteamBootstrap starts (normal play). Batch mode/tests never start it without the -steam flag.")]
        [field: SerializeField] public bool InitSteamOnStart { get; private set; } = true;

        [field: Header("Session")]
        [field: Tooltip("Transport selected when a scene starts. Steam needs Steam running (lobby flow in M3.5).")]
        [field: SerializeField] public TransportMode DefaultTransport { get; private set; } = TransportMode.UnityTransport;

        [field: Tooltip("Players per session, host included (GDD: 1-4).")]
        [field: SerializeField, Range(1, 8)] public int MaxPlayers { get; private set; } = 4;

        [field: Tooltip("Network ticks per second (NGO NetworkConfig.TickRate). Every machine must use the same value.")]
        [field: SerializeField, Range(10, 60)] public int TickRate { get; private set; } = 30;

        [field: Header("Unity Transport (direct IP)")]
        [field: Tooltip("UDP port the host listens on and clients connect to.")]
        [field: SerializeField] public ushort Port { get; private set; } = 7777;

        [field: Tooltip("Address pre-filled in the Join field.")]
        [field: SerializeField] public string DefaultJoinAddress { get; private set; } = "127.0.0.1";

        [field: Tooltip("Address the host listens on. 0.0.0.0 accepts LAN players; 127.0.0.1 only this machine.")]
        [field: SerializeField] public string ListenAddress { get; private set; } = "0.0.0.0";

        [field: Tooltip("Per-attempt connect timeout (ms).")]
        [field: SerializeField, Min(100)] public int ConnectTimeoutMs { get; private set; } = 1000;

        [field: Tooltip("Attempts before a join gives up (attempts x timeout = how long 'Connecting...' lasts).")]
        [field: SerializeField, Min(1)] public int MaxConnectAttempts { get; private set; } = 10;

        [field: Header("Editor")]
        [field: Tooltip("Pressing Play in the editor starts hosting straight away, so solo testing needs no clicks. Never fires for Multiplayer Play Mode virtual players or when launched with -connect/-client.")]
        [field: SerializeField] public bool AutoHostInEditor { get; private set; } = true;

        public void Validate(List<string> errors)
        {
            if (SteamAppId == 0) errors.Add($"{name}: SteamAppId must not be 0.");
            if (Port == 0) errors.Add($"{name}: Port must not be 0 (0 is only for automated tests).");
            if (MaxPlayers < 1) errors.Add($"{name}: MaxPlayers must be at least 1.");
            if (string.IsNullOrWhiteSpace(ListenAddress)) errors.Add($"{name}: ListenAddress is empty.");
        }
    }
}
