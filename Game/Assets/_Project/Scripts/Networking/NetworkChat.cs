using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Text chat (UI step 10) for crew without a microphone: the owner sends a line to the host, which checks
    /// it (length, rate) and passes it on. The living hear everyone alive; ghosts' lines only reach other
    /// ghosts (GDD 11: the dead can't help). On the player object; UI.ChatView shows and sends.
    /// </summary>
    public class NetworkChat : NetworkBehaviour
    {
        public const int MaxLength = 120;
        private const float MinInterval = 0.4f;

        private NetworkPlayer player;
        private float nextAllowed;

        /// <summary>Every machine: a line arrived (sender's client id, text, whether a ghost said it).</summary>
        public static event Action<ulong, string, bool> Received;

        private void Awake() => player = GetComponent<NetworkPlayer>();

        /// <summary>Owner: say something.</summary>
        public void Say(string text)
        {
            if (!IsOwner || string.IsNullOrWhiteSpace(text)) return;
            SayRpc(new FixedString512Bytes(Fit(text)));
        }

        /// <summary>
        /// At most <see cref="MaxLength"/> characters, never more bytes than the wire holds (QA B-15: Arabic,
        /// Chinese or emoji take 2-4 bytes each) and never half a surrogate pair.
        /// </summary>
        public static string Fit(string text)
        {
            text = text.Trim();
            if (text.Length > MaxLength) text = text.Substring(0, MaxLength);
            while (text.Length > 0 && System.Text.Encoding.UTF8.GetByteCount(text) > MaxBytes) text = text.Substring(0, text.Length - 1);
            if (text.Length > 0 && char.IsHighSurrogate(text[text.Length - 1])) text = text.Substring(0, text.Length - 1);
            return text;
        }

        // FixedString512Bytes holds 509 UTF-8 bytes: 120 characters of up to 4 bytes each.
        private const int MaxBytes = 509;

        [Rpc(SendTo.Server)]
        private void SayRpc(FixedString512Bytes text, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || Time.unscaledTime < nextAllowed || text.ToString().Length > MaxLength) return;
            nextAllowed = Time.unscaledTime + MinInterval;
            bool ghost = player != null && player.IsDead;
            if (!ghost)
            {
                HearRpc(text, false, RpcTarget.Everyone);
                return;
            }
            var dead = new List<ulong>();
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.NetworkManager == NetworkManager && p.IsDead) dead.Add(p.OwnerClientId);
            HearRpc(text, true, RpcTarget.Group(dead, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void HearRpc(FixedString512Bytes text, bool ghost, RpcParams rpcParams) => Received?.Invoke(OwnerClientId, text.ToString(), ghost);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Received = null;
    }
}
