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

        [Header("Walls")]
        [Tooltip("Volume kept per wall between speaker and listener (0..1).")]
        [field: SerializeField, Range(0f, 1f)] public float OcclusionVolumePerWall { get; private set; } = 0.55f;
        [Tooltip("Low-pass cutoff (Hz) through one wall; each further wall lowers it more. Muffled, not silent.")]
        [field: SerializeField, Range(200f, 5000f)] public float OcclusionCutoff { get; private set; } = 900f;
        [Tooltip("Walls beyond this many don't muffle further (the falloff still applies).")]
        [field: SerializeField, Range(1, 6)] public int MaxOccludingWalls { get; private set; } = 3;
        [Tooltip("How fast muffling follows the speaker in and out of rooms (1/s).")]
        [field: SerializeField, Range(1f, 30f)] public float OcclusionSmoothing { get; private set; } = 8f;

        [Header("Radio")]
        [Tooltip("Until equipment exists (M6) every player carries a walkie-talkie.")]
        [field: SerializeField] public bool EveryoneHasRadio { get; private set; } = true;
        [field: SerializeField, Range(0f, 1f)] public float RadioVolume { get; private set; } = 0.85f;
        [Tooltip("Static mixed in while a transmission plays (amplitude).")]
        [field: SerializeField, Range(0f, 0.3f)] public float RadioStatic { get; private set; } = 0.035f;
        [Tooltip("Walkie-talkie band (Hz): everything outside is cut.")]
        [field: SerializeField, Range(100f, 1000f)] public float RadioLowCut { get; private set; } = 400f;
        [field: SerializeField, Range(1500f, 6000f)] public float RadioHighCut { get; private set; } = 3200f;

        public void Validate(List<string> errors)
        {
            if (MaxDistance <= MinDistance) errors.Add($"{name}: MaxDistance must be greater than MinDistance.");
            if (MaxBufferMs <= JitterMs) errors.Add($"{name}: MaxBufferMs must be greater than JitterMs.");
            if (RadioHighCut <= RadioLowCut) errors.Add($"{name}: RadioHighCut must be above RadioLowCut.");
        }
    }
}
