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
            text = text.Trim();
            if (text.Length > MaxLength) text = text.Substring(0, MaxLength);
            SayRpc(new FixedString128Bytes(text));
        }

        [Rpc(SendTo.Server)]
        private void SayRpc(FixedString128Bytes text, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || Time.unscaledTime < nextAllowed) return;
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
        private void HearRpc(FixedString128Bytes text, bool ghost, RpcParams rpcParams) => Received?.Invoke(OwnerClientId, text.ToString(), ghost);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Received = null;
    }
}
