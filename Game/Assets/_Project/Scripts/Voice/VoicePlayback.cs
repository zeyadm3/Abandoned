using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Plays one remote player's voice from their head. Unity only pans it (flat rolloff curve); the
    /// volume is our proximity falloff, times the walls between us (each one also lowers a low-pass
    /// cutoff, so a voice next door is muffled, not just quieter), times the player's voice volume.
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
    public class VoicePlayback : MonoBehaviour
    {
        // A wall is anything solid that isn't a player, loot or debris.
        private const int MaxHits = 8;

        [SerializeField] private VoiceConfig config;

        private AudioSource source;
        private AudioLowPassFilter lowPass;
        private VoiceStream stream;
        private readonly RaycastHit[] hits = new RaycastHit[MaxHits];
        private int wallMask;
        private float occlusion = 1f, cutoff = VoiceMath.OpenCutoff;

        public VoiceJitterBuffer Buffer => stream?.Buffer;
        public int SamplesReceived => stream?.SamplesReceived ?? 0;
        public float Gain { get; private set; }

        private Unity.Netcode.NetworkObject owner;
        /// <summary>Whose voice this plays (per-player volume and mute, UI step 4).</summary>
        private ulong Speaker => (owner ??= GetComponentInParent<Unity.Netcode.NetworkObject>()) != null ? owner.OwnerClientId : ulong.MaxValue;
        public int Walls { get; private set; }
        public float Cutoff => cutoff;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            lowPass = GetComponent<AudioLowPassFilter>();
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Constant(0f, 1f, 1f));
            source.maxDistance = config.MaxDistance;
            source.volume = 0f;
            stream = new VoiceStream(source, config);
            wallMask = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");
        }

        public void Push(float[] pcm, int count, int rate) => stream.Push(pcm, count, rate);

        private void Update()
        {
            AudioListener listener = VoiceListener.Current;
            Vector3 ear = listener != null ? listener.transform.position : transform.position;
            float distance = Vector3.Distance(ear, transform.position);
            Walls = distance < config.MaxDistance ? CountWalls(ear) : 0;

            float k = 1f - Mathf.Exp(-config.OcclusionSmoothing * Time.deltaTime);
            occlusion = Mathf.Lerp(occlusion, VoiceMath.OcclusionGain(Walls, config.OcclusionVolumePerWall, config.MaxOccludingWalls), k);
            cutoff = Mathf.Lerp(cutoff, VoiceMath.OcclusionCutoff(Walls, config.OcclusionCutoff, config.MaxOccludingWalls), k);
            lowPass.cutoffFrequency = cutoff;

            Gain = VoiceMath.DistanceGain(distance, config.MinDistance, config.MaxDistance) * occlusion;
            source.volume = Gain * VoiceSettings.Volume * VoiceSettings.PlayerGain(Speaker);
            stream.Tick();
        }

        public int CountWalls(Vector3 ear)
        {
            Vector3 to = transform.position - ear;
            float length = to.magnitude;
            if (length < 0.01f) return 0;
            int n = Physics.RaycastNonAlloc(ear, to / length, hits, length, wallMask, QueryTriggerInteraction.Ignore);
            return Mathf.Min(n, config.MaxOccludingWalls);
        }

        private void OnDisable() => stream?.Stop();

        private void OnDestroy() => stream?.Dispose();

        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, config.MinDistance);
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, config.MaxDistance);
            AudioListener listener = VoiceListener.Current;
            if (listener == null) return;
            Gizmos.color = Walls > 0 ? Color.red : Color.green;
            Gizmos.DrawLine(listener.transform.position, transform.position);
        }
    }
}
