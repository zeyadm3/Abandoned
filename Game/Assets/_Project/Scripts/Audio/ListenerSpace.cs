using Abandoned.Core;
using Abandoned.Structure;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The room the listener stands in (0.14.0). A few rays measure how big and how enclosed the space
    /// around the ear is, and a reverb on the listener follows it: tight and dry in a stock room, long in
    /// the atrium, almost nothing in the parking lot. Needs no authored zones, so it works in any level and
    /// changes when floors fall. Also owns the world's low-pass (the low-health muffle), so one object
    /// decides what sits on the listener. 2D sounds, music and crew voices bypass both.
    /// One per game, made on first scene load (never in batch mode).
    /// </summary>
    public class ListenerSpace : MonoBehaviour
    {
        public const float OpenCutoff = 22000f;
        private const float ProbeInterval = 0.25f, Reach = 30f;

        // Up, four ways along the floor, and four ways up at an angle.
        private static readonly Vector3[] Rays =
        {
            Vector3.up, Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            new Vector3(1f, 1f, 0f).normalized, new Vector3(-1f, 1f, 0f).normalized,
            new Vector3(0f, 1f, 1f).normalized, new Vector3(0f, 1f, -1f).normalized,
        };
        private const int UpwardRays = 5;

        private static ListenerSpace instance;
        private static float muffleTarget = OpenCutoff;

        private GameObject host;
        private AudioLowPassFilter muffle;
        private AudioReverbFilter reverb;
        private float size = 6f, openness = 1f, targetSize = 6f, targetOpen = 1f, cutoff = OpenCutoff, nextProbe;
        private int mask;

        /// <summary>The world's low-pass cutoff (Hz) asked for by the player's condition; <see cref="OpenCutoff"/> = none.</summary>
        public static float Muffle
        {
            get => muffleTarget;
            set => muffleTarget = Mathf.Clamp(value, 200f, OpenCutoff);
        }

        /// <summary>Where the low-pass is right now (it eases toward <see cref="Muffle"/>).</summary>
        public static float MuffleNow => instance != null ? instance.cutoff : muffleTarget;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("ListenerSpace");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ListenerSpace>();
        }

        private void Awake() =>
            mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");

        private void Update()
        {
            AudioListener ear = VoiceListener.Current;
            if (ear == null) return;
            if (host != ear.gameObject) MoveTo(ear.gameObject);
            AudioConfig config = AudioConfig.Current;
            if (Time.unscaledTime >= nextProbe)
            {
                nextProbe = Time.unscaledTime + ProbeInterval;
                Probe(ear.transform.position);
            }
            float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime / config.RoomBlend);
            size = Mathf.Lerp(size, targetSize, k);
            openness = Mathf.Lerp(openness, targetOpen, k);
            ApplyRoom(config);
            ApplyMuffle();
        }

        // The filters live on whichever object holds the live listener (the player's camera, then the ghost's).
        private void MoveTo(GameObject listener)
        {
            if (muffle != null) Destroy(muffle);
            if (reverb != null) Destroy(reverb);
            host = listener;
            // Added in this order: the room is heard through the muffle, not the other way round.
            muffle = listener.AddComponent<AudioLowPassFilter>();
            muffle.lowpassResonanceQ = 1f;
            muffle.enabled = false;
            reverb = listener.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.User;
            reverb.dryLevel = 0f;
            reverb.roomLF = 0f;
            reverb.roomHF = -700f;
            reverb.decayHFRatio = 0.55f;
            reverb.reflectionsDelay = 0.02f;
            reverb.reverbLevel = 0f;
            reverb.reverbDelay = 0.03f;
            reverb.hfReference = 5000f;
            reverb.lfReference = 250f;
            reverb.diffusion = 100f;
            reverb.density = 100f;
            reverb.enabled = false;
        }

        private void Probe(Vector3 at)
        {
            float sum = 0f;
            int sky = 0, walls = 0;
            for (int i = 0; i < Rays.Length; i++)
            {
                if (Physics.Raycast(at, Rays[i], out RaycastHit hit, Reach, mask, QueryTriggerInteraction.Ignore))
                {
                    sum += hit.distance;
                    if (i >= 1 && i <= 4) walls++;
                }
                else
                {
                    sum += Reach;
                    if (i == 0 || i > 4) sky++;
                }
            }
            targetSize = sum / Rays.Length;
            float open = sky / (float)UpwardRays;
            // Walls all round is a courtyard at most, never open ground.
            if (walls == 4) open *= 0.5f;
            // The atrium's skylight has nothing to hit: inside the building counts as roofed whatever the rays say.
            if (BuildingInteriorAtmosphere.Present) open = Mathf.Min(open, 1f - BuildingInteriorAtmosphere.InsideBlend);
            targetOpen = open;
        }

        private void ApplyRoom(AudioConfig config)
        {
            if (reverb == null) return;
            reverb.enabled = config.Reverb;
            if (!config.Reverb) return;
            float room = Mathf.InverseLerp(config.RoomSize.x, config.RoomSize.y, size);
            reverb.decayTime = Mathf.Lerp(config.Decay.x, config.Decay.y, room) * Mathf.Lerp(1f, 0.6f, openness);
            float level = Mathf.Lerp(Mathf.Lerp(config.WetLevel.x, config.WetLevel.y, room), config.OutdoorLevel, openness);
            reverb.room = level;
            // Small rooms are mostly their first echoes; big ones mostly their tail.
            reverb.reflectionsLevel = Mathf.Lerp(-900f, -1500f, room);
        }

        private void ApplyMuffle()
        {
            cutoff = Mathf.Lerp(cutoff, muffleTarget, 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
            if (muffle == null) return;
            bool on = cutoff < OpenCutoff - 250f;
            muffle.enabled = on;
            if (on) muffle.cutoffFrequency = cutoff;
        }

        public static string Describe()
        {
            if (instance == null) return "SPACE none";
            return $"SPACE size {instance.size:0.0} m  open {instance.openness:0.00}  " +
                   $"decay {(instance.reverb != null && instance.reverb.enabled ? instance.reverb.decayTime : 0f):0.0} s  " +
                   $"muffle {instance.cutoff:0} Hz  walls {AudioOcclusion.LastWalls}";
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            muffleTarget = OpenCutoff;
        }
    }
}
