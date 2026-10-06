using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>Proximity voice tuning (one asset in Data/Voice). Per-player choices live in <see cref="VoiceSettings"/>.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Voice/Voice Config", fileName = "VoiceConfig")]
    public class VoiceConfig : ScriptableObject, IValidatable
    {
        [Header("Proximity")]
        [Tooltip("Full volume inside this distance (m).")]
        [field: SerializeField, Min(0.1f)] public float MinDistance { get; private set; } = 1.5f;
        [Tooltip("Silent beyond this distance (m). A big room is ~20 m; shouting across a mall atrium shouldn't carry.")]
        [field: SerializeField, Min(1f)] public float MaxDistance { get; private set; } = 25f;

        [Header("Network")]
        [Tooltip("Bytes per voice packet; must fit one unreliable datagram (~1.2 KB).")]
        [field: SerializeField, Range(160, 1200)] public int PacketBytes { get; private set; } = 640;
        [Tooltip("Audio queued before a speaker starts playing (ms). Higher = smoother over bad connections, more delay.")]
        [field: SerializeField, Range(20, 400)] public int JitterMs { get; private set; } = 120;
        [Tooltip("Most audio kept per speaker (ms); older audio is dropped so delay can't build up.")]
        [field: SerializeField, Range(100, 2000)] public int MaxBufferMs { get; private set; } = 500;

        [Header("Speaking")]
        [Tooltip("Open mic: RMS level that counts as talking (0..1).")]
        [field: SerializeField, Range(0f, 0.5f)] public float OpenMicThreshold { get; private set; } = 0.02f;
        [Tooltip("Open mic keeps sending this long after the level drops, so word endings aren't clipped (s).")]
        [field: SerializeField, Range(0f, 2f)] public float OpenMicHangTime { get; private set; } = 0.4f;
        [Tooltip("The speaking indicator stays on this long after the last packet (s).")]
        [field: SerializeField, Range(0f, 1f)] public float IndicatorHold { get; private set; } = 0.25f;

        public void Validate(List<string> errors)
        {
            if (MaxDistance <= MinDistance) errors.Add($"{name}: MaxDistance must be greater than MinDistance.");
            if (MaxBufferMs <= JitterMs) errors.Add($"{name}: MaxBufferMs must be greater than JitterMs.");
        }
    }
}
