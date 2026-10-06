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

        [field: Tooltip("Steam App ID passed to SteamClient.Init. 480 (Spacewar) during development; the real one from Steam Direct before release. Keep Game/steam_appid.txt in sync.")]
        [field: SerializeField] public uint SteamAppId { get; private set; } = DevelopmentAppId;

        [field: Tooltip("Start Steam automatically when SteamBootstrap starts (normal play). Batch mode/tests never start it without the -steam flag.")]
        [field: SerializeField] public bool InitSteamOnStart { get; private set; } = true;

        public void Validate(List<string> errors)
        {
            if (SteamAppId == 0) errors.Add($"{name}: SteamAppId must not be 0.");
        }
    }
}
