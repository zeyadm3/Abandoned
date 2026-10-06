using System;
using System.Collections.Generic;
using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// A player's voice over NGO: the owner sends each packet to the host (unreliable: a late voice
    /// packet is useless), the host checks it really came from this player and relays it to everyone
    /// else; each machine plays it from this player's position. Old or duplicate packets are dropped
    /// by sequence number. The host also turns each speaker's loudness into NoiseEvents (monsters hear voices).
    /// </summary>
    public class NetworkVoice : NetworkBehaviour
    {
        /// <summary>Hard cap on a packet a client may send us (one datagram).</summary>
        public const int MaxPacketBytes = 1200;

        [SerializeField] private VoiceConfig config;
        [SerializeField] private VoicePlayback playback;
        [SerializeField] private RadioPlayback radio;

        private static readonly List<NetworkVoice> Spawned = new();

        private readonly float[] decoded = new float[8192];
        private VoiceNoiseMeter noiseMeter;
        private ushort sendSequence, lastSequence;
        private bool hasSequence;

        public static IReadOnlyList<NetworkVoice> All => Spawned;

        /// <summary>Host: a player's voice packet arrived (speaker, level 0..1, over the radio).</summary>
        public static event Action<NetworkVoice, float, bool> HeardOnHost;

        /// <summary>
        /// Who carries a walkie-talkie (client id -> yes). Unset: <see cref="VoiceConfig.EveryoneHasRadio"/>.
        /// Equipment (M6) plugs in here.
        /// </summary>
        public static Func<ulong, bool> RadioHolder { get; set; }

        public VoiceConfig Config => config;
        public VoicePlayback Playback => playback;
        public RadioPlayback Radio => radio;

        public bool HasRadio(ulong clientId) => RadioHolder?.Invoke(clientId) ?? config.EveryoneHasRadio;
        public float LastLevel { get; private set; }
        public bool LastWasRadio { get; private set; }
        public int PacketsReceived { get; private set; }
        public int PacketsSent { get; private set; }
        public int PacketsRejected { get; private set; }
        private float lastPacketTime = float.NegativeInfinity;

        /// <summary>Talking right now as this machine sees it (the indicator).</summary>
        public bool IsSpeaking => Time.time - lastPacketTime <= config.IndicatorHold;

        public bool IsOnRadio => IsSpeaking && LastWasRadio;

        public override void OnNetworkSpawn()
        {
            Spawned.Add(this);
            // Nobody hears themselves.
            if (IsOwner && playback != null) playback.enabled = false;
            if (IsOwner && radio != null) radio.enabled = false;
        }

        public override void OnNetworkDespawn() => Spawned.Remove(this);

        /// <summary>Owner: one encoded packet from the microphone.</summary>
        public void Send(byte[] packet, int length, VoiceCodecId codec, float level, bool radio)
        {
            if (!IsSpawned || !IsOwner || length <= 0 || length > MaxPacketBytes) return;
            var data = new byte[length];
            Buffer.BlockCopy(packet, 0, data, 0, length);
            MarkSpoke(level, radio);
            PacketsSent++;
            ToHostRpc(data, codec, VoiceMath.ToByte(level), radio, sendSequence++);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        private void ToHostRpc(byte[] data, VoiceCodecId codec, byte level, bool radio, ushort sequence, RpcParams rpcParams = default)
        {
            // Only this player's own client may speak with this player's voice.
            if (rpcParams.Receive.SenderClientId != OwnerClientId || data == null || data.Length == 0 ||
                data.Length > MaxPacketBytes || VoiceBackends.Codec(codec) == null)
            {
                PacketsRejected++;
                return;
            }
            // Only someone carrying a radio can transmit on it (the host's view of who does).
            radio = radio && HasRadio(OwnerClientId);
            if (!IsOwner) MarkSpoke(VoiceMath.FromByte(level), radio);
            HeardOnHost?.Invoke(this, VoiceMath.FromByte(level), radio);
            EmitNoise(VoiceMath.FromByte(level), radio);
            ToListenersRpc(data, codec, level, radio, sequence);
        }

        [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Unreliable)]
        private void ToListenersRpc(byte[] data, VoiceCodecId codec, byte level, bool radio, ushort sequence)
        {
            // Unreliable packets can arrive late or twice; playing them would garble the stream.
            if (hasSequence && (short)(sequence - lastSequence) <= 0) return;
            hasSequence = true;
            lastSequence = sequence;
            PacketsReceived++;
            MarkSpoke(VoiceMath.FromByte(level), radio);
            IVoiceCodec decoder = VoiceBackends.Codec(codec);
            if (decoder == null || decoder.SampleRate <= 0) return;
            int n = decoder.Decode(data, data.Length, decoded);
            // Heard in person (falloff + walls) and, over the radio, in the hand of anyone who carries one.
            if (playback != null && playback.enabled) playback.Push(decoded, n, decoder.SampleRate);
            if (radio && this.radio != null && this.radio.enabled && HasRadio(NetworkManager.LocalClientId))
                this.radio.Push(decoded, n, decoder.SampleRate);
        }

        // Host: monsters hear voice chat (GDD 17). Noise comes from the speaker's mouth; a radio
        // transmission adds its squawk there too.
        private void EmitNoise(float level, bool radio)
        {
            noiseMeter ??= new VoiceNoiseMeter(config.NoiseInterval);
            if (!noiseMeter.Add(VoiceMath.NoiseLoudness(level, radio, config), Time.time, out float loudness)) return;
            Vector3 mouth = playback != null ? playback.transform.position : transform.position;
            NoiseSystem.Emit(mouth, loudness, NoiseSource.Voice);
        }

        private void MarkSpoke(float level, bool radio)
        {
            lastPacketTime = Time.time;
            LastLevel = level;
            LastWasRadio = radio;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Spawned.Clear();
            HeardOnHost = null;
            RadioHolder = null;
        }
    }
}
