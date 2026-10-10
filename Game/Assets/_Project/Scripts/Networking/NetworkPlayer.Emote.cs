using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    public enum Emote : byte { None, Point, Wave, Panic }

    /// <summary>
    /// Emotes (0.19.5): the owner picks one with its key and writes it with the server time it started; every
    /// machine's worker body plays it for a few seconds (WorkerPresentation). Purely visual.
    /// </summary>
    public partial class NetworkPlayer
    {
        public const float EmoteSeconds = 2.6f;

        private readonly NetworkVariable<byte> emote = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<double> emoteAt = new(0d, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        /// <summary>The emote playing now, and how far into it (0-1).</summary>
        public Emote CurrentEmote(out float t)
        {
            t = 0f;
            if (!IsSpawned || emote.Value == 0) return Emote.None;
            t = (float)((NetworkManager.ServerTime.Time - emoteAt.Value) / EmoteSeconds);
            return t >= 0f && t < 1f ? (Emote)emote.Value : Emote.None;
        }

        /// <summary>Owner, each frame: start an emote on its key.</summary>
        private void TickOwnerEmote()
        {
            if (!IsOwner || dead.Value || hideInput == null && (hideInput = GetComponent<Player.PlayerInputReader>()) == null) return;
            int pressed = hideInput.EmotePressed;
            if (pressed == 0) return;
            emote.Value = (byte)pressed;
            emoteAt.Value = NetworkManager.ServerTime.Time;
        }
    }
}
