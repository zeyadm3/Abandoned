using System.Text;
using Abandoned.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The name everyone sees for this player (QA B-07): the owner's Steam name, else the name they typed in
    /// Settings, else "Player N" by crew seat (1-4, reused, so a crew never shows "Player 6"). Owner-written,
    /// cleaned of markup and cut to fit; the host never trusts it for anything but display.
    /// </summary>
    public partial class NetworkPlayer
    {
        public const int MaxNameLength = 24;
        // FixedString64Bytes holds 61 UTF-8 bytes.
        private const int MaxNameBytes = 61;

        private readonly NetworkVariable<FixedString64Bytes> displayName = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        // The owner's Steam id (0 off Steam): only so others can remember their per-player voice volume.
        private readonly NetworkVariable<ulong> steamId = new(0UL,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        /// <summary>A stable key for this person across sessions (Steam id, else a chosen name; null = neither), for local preferences.</summary>
        public string Identity => steamId.Value != 0UL ? "steam." + steamId.Value
            : string.IsNullOrWhiteSpace(displayName.Value.ToString()) ? null : "name." + displayName.Value;

        /// <summary>The <see cref="Identity"/> of a connected client's player, or null.</summary>
        public static string IdentityOf(ulong clientId)
        {
            foreach (NetworkPlayer p in Spawned)
                if (p != null && p.OwnerClientId == clientId) return p.Identity;
            return null;
        }

        public string DisplayName
        {
            get
            {
                string chosen = displayName.Value.ToString();
                return string.IsNullOrWhiteSpace(chosen) ? $"Player {Mathf.Max(0, CrewSeat) + 1}" : chosen;
            }
        }

        /// <summary>The display name of a connected client's player ("Player N" when they have none yet).</summary>
        public static string NameOf(ulong clientId)
        {
            foreach (NetworkPlayer p in Spawned)
                if (p != null && p.OwnerClientId == clientId) return p.DisplayName;
            return $"Player {clientId + 1}";
        }

        private void SpawnName()
        {
            if (!IsOwner) return;
            displayName.Value = new FixedString64Bytes(Clean(PreferredName()));
            SteamBootstrap steam = SteamBootstrap.Instance;
            steamId.Value = steam != null && steam.IsAvailable ? steam.LocalSteamId : 0UL;
        }

        private static string PreferredName()
        {
            SteamBootstrap steam = SteamBootstrap.Instance;
            string steamName = steam != null && steam.IsAvailable ? steam.LocalPlayerName : "";
            return string.IsNullOrWhiteSpace(steamName) ? GameSettings.PlayerName : steamName;
        }

        /// <summary>No markup, no control characters, at most <see cref="MaxNameLength"/> characters that fit the wire.</summary>
        public static string Clean(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            var sb = new StringBuilder(raw.Length);
            foreach (char c in raw)
            {
                if (char.IsControl(c) || c == '<' || c == '>') continue;
                sb.Append(c);
            }
            string name = sb.ToString().Trim();
            if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength);
            while (name.Length > 0 && Encoding.UTF8.GetByteCount(name) > MaxNameBytes) name = name.Substring(0, name.Length - 1);
            // Never leave half a surrogate pair at the end.
            if (name.Length > 0 && char.IsHighSurrogate(name[name.Length - 1])) name = name.Substring(0, name.Length - 1);
            return name.Trim();
        }
    }
}
