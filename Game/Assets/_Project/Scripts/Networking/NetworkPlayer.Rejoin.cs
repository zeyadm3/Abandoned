using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Rejoining at the HQ (0.23.5). Host: once a newcomer's identity (Steam id or chosen name) arrives, a player
    /// who dropped earlier in this session gets their old seat back (number, colour, spawn) if it's free.
    /// Own machine: the address or SteamID64 it joined is kept for the main menu's "Rejoin your crew" button.
    /// Joining stays HQ-only.
    /// </summary>
    public partial class NetworkPlayer
    {
        public const string LastJoinKey = "join.last";
        private bool reclaimChecked, joinRemembered;

        private void TickRejoin()
        {
            if (IsServer && !reclaimChecked && !IsOwner && Identity != null)
            {
                reclaimChecked = true;
                SpawnSlots seats = NetworkBootstrap.Resolve(null)?.Slots;
                if (seats != null && seats.TryReclaim(OwnerClientId, Identity, out int seat) && crewSeat.Value != seat)
                {
                    crewSeat.Value = seat;
                    Debug.Log($"[Session] {DisplayName} is back: seat {seat + 1} kept for them.");
                }
            }
            if (IsOwner && !IsServer && !joinRemembered)
            {
                joinRemembered = true;
                string target = NetworkBootstrap.Resolve(null)?.LastJoinTarget;
                if (!string.IsNullOrEmpty(target)) Core.Prefs.SetString(LastJoinKey, target);
            }
        }
    }
}
