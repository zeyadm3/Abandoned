using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// What a monster sounds like on every machine: its signature at intervals (quicker when it is chasing),
    /// and its blow landing when it turns to attack. The Blind One and the Hunter keep their own click and
    /// tread as the warning, so they have no interval signature. Each monster's song is <see cref="ThreatMusic"/>.
    /// Reads only replicated state; the sounds are local.
    /// </summary>
    public class ThreatHorrorAudio : MonoBehaviour
    {
        private Threat threat;
        private float next;
        private ThreatMotion heard;

        private void Awake() => threat = GetComponent<Threat>();

        private void Update()
        {
            if (threat == null || !threat.IsSpawned || threat.Definition == null) return;
            ThreatDefinition d = threat.Definition;
            ThreatMotion motion = threat.DesiredMotion;
            if (motion != heard)
            {
                if (motion == ThreatMotion.Attack) GameAudio.Play(SoundId.ThreatStrike, transform.position + Vector3.up, 1f);
                heard = motion;
            }
            if (Time.time < next) return;
            bool intense = motion == ThreatMotion.Chase || motion == ThreatMotion.Attack;
            next = Time.time + d.SignatureInterval * (intense ? 0.55f : 1f);
            if (d.Kind == ThreatKind.BlindOne || d.Kind == ThreatKind.Hunter) return;
            PlaySignature(d.Kind, transform.position + Vector3.up * (d.Kind == ThreatKind.Weight ? 3.5f : 1f), intense ? 0.95f : 0.62f, d.AudibleRange);
        }

        public static SoundId Signature(ThreatKind kind) => kind switch
        {
            ThreatKind.BlindOne => SoundId.BlindOneClick,
            ThreatKind.Hunter => SoundId.HunterStep,
            ThreatKind.Collector => SoundId.CollectorSignature,
            ThreatKind.Weight => SoundId.WeightSignature,
            ThreatKind.Thing => SoundId.ThingSignature,
            ThreatKind.Crawlers => SoundId.CrawlersSignature,
            ThreatKind.LastHunter => SoundId.LastHunterSignature,
            _ => SoundId.StalkerSignature,
        };

        public static void PlaySignature(ThreatKind kind, Vector3 at, float volume = 1f, float range = 45f) =>
            GameAudio.Play(Signature(kind), at, volume, range);
    }
}
