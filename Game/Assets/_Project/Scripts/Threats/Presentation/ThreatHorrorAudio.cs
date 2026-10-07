using System.Collections.Generic;
using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>Original synthesised signatures routed through the effects source pool. No score plays during runs.</summary>
    public class ThreatHorrorAudio : MonoBehaviour
    {
        private static readonly Dictionary<ThreatKind, AudioClip> Clips = new();
        private Threat threat;
        private float next;
        private void Awake() => threat = GetComponent<Threat>();
        private void Update()
        {
            if (threat == null || !threat.IsSpawned || threat.Definition == null || Time.time < next) return;
            ThreatDefinition d = threat.Definition;
            bool intense = threat.DesiredMotion == ThreatMotion.Chase || threat.DesiredMotion == ThreatMotion.Attack;
            next = Time.time + d.SignatureInterval * (intense ? 0.55f : 1f);
            if (d.Kind == ThreatKind.BlindOne || d.Kind == ThreatKind.Hunter) return; // Their existing clicks and heavy steps remain the primary warning.
            PlaySignature(d.Kind, transform.position + Vector3.up * (d.Kind == ThreatKind.Weight ? 3.5f : 1f), intense ? 0.95f : 0.62f, d.AudibleRange);
        }
        public static void PlaySignature(ThreatKind kind, Vector3 at, float volume = 1f, float range = 45f) =>
            AudioPool.Play(Clip(kind), at, volume * AudioLevels.Sfx, 1f, 2f, range, true, 54);
        private static AudioClip Clip(ThreatKind kind)
        {
            if (Clips.TryGetValue(kind, out AudioClip clip) && clip != null) return clip;
            const int rate = 22050;
            float duration = kind == ThreatKind.LastHunter ? 2.7f : kind == ThreatKind.Weight ? 2.2f : 1.4f;
            float[] samples = new float[Mathf.CeilToInt(duration * rate)];
            var random = new System.Random(109 + (int)kind);
            float filtered = 0f, phase = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate, u = t / duration;
                float noise = (float)random.NextDouble() * 2f - 1f;
                filtered += (noise - filtered) * (kind == ThreatKind.Weight ? 0.025f : 0.14f);
                float envelope = Mathf.Sin(Mathf.PI * u) * Mathf.Clamp01(t * 20f);
                float signal;
                switch (kind)
                {
                    case ThreatKind.Stalker:
                        signal = filtered * 0.8f + Mathf.Sin(2f * Mathf.PI * 215f * t) * 0.12f * Mathf.Sin(2f * Mathf.PI * 6f * t); break;
                    case ThreatKind.Collector:
                        signal = Mathf.Sin(2f * Mathf.PI * 870f * t) * Mathf.Exp(-8f * Mathf.Repeat(t, 0.24f)) * 0.25f + filtered * 0.3f; break;
                    case ThreatKind.Weight:
                        signal = Mathf.Sin(2f * Mathf.PI * (42f * t - 4f * t * t)) * 0.5f + filtered * 1.8f; break;
                    case ThreatKind.Thing:
                        signal = filtered * 0.4f + Mathf.Sin(2f * Mathf.PI * 390f * t) * Mathf.Sin(2f * Mathf.PI * 393f * t) * 0.25f; break;
                    case ThreatKind.Crawlers:
                        signal = noise * Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 91f)), 12f) * 0.3f + filtered * 0.15f; break;
                    default:
                        phase += (58f + 14f * Mathf.Sin(t * 6f)) / rate;
                        signal = Mathf.Sin(2f * Mathf.PI * phase) * 0.55f + filtered * 0.65f + Mathf.Sin(phase * 2f * Mathf.PI * 2.47f) * 0.2f; break;
                }
                samples[i] = Mathf.Clamp(signal * envelope, -0.8f, 0.8f);
            }
            clip = AudioClip.Create($"Abandoned_{kind}_Signature", samples.Length, 1, rate, false);
            clip.SetData(samples, 0); Clips[kind] = clip;
            return clip;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetClips() => Clips.Clear();
    }
}
