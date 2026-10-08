using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.UI;
using Abandoned.Voice;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Player
{
    // Low-health feedback by threshold (user spec, 2026-10-09). Every number is in PlayerHealthConfig.
    //   30 HP and below: a steady heartbeat, about 75 BPM, until healed above 30.
    //   20 HP and below: about 105 BPM and louder; the world's sound is muffled; a soft dark-red vignette with
    //                    chromatic aberration creeps in from the edges; blood splatter in the screen corners.
    //   10 HP and below: about 145 BPM at full volume with heavy breathing; the vignette closes toward the
    //                    centre and swells on every beat; heavy, layered, dripping blood and a slight blur and warp.
    // The beat itself is PlayerPulse, so the vitals bar, the vignette and the blood all move with the sound.
    public partial class PlayerHealthEffects
    {
        private const float OpenCutoff = 22000f, BeatDecay = 5.5f;

        private ChromaticAberration fringe;
        private LensDistortion warp;
        private AudioSource pulseSource, breathSource;
        private AudioLowPassFilter muffle;
        private TraumaOverlay overlay;
        private float severe, nearDeath, drip, nextBeat, nextLabored, cutoff = OpenCutoff;
        private int traumaTier;

        /// <summary>Runs the thresholds for this frame and returns the tier (0 = above critical).</summary>
        private int TickTrauma(PlayerHealthConfig config, float vitals)
        {
            int tier = traumaTier = config.TraumaTier(player.Health);
            float step = Time.deltaTime / config.TraumaFadeSeconds;
            severe = Mathf.MoveTowards(severe, tier >= 2 ? 1f : 0f, step);
            nearDeath = Mathf.MoveTowards(nearDeath, tier >= 3 ? 1f : 0f, step);
            // The drips keep running while the player stays near death, and start over once the blood has cleared.
            if (tier >= 3) drip = Mathf.MoveTowards(drip, 1f, Time.deltaTime / config.DripSeconds);
            else if (nearDeath <= 0f) drip = 0f;

            float beat = PlayerPulse.Beat * Mathf.Exp(-BeatDecay * Time.deltaTime);
            if (tier >= 1)
            {
                float interval = 60f / Mathf.Max(30f, PlayerHealthConfig.Pick(config.HeartbeatBpm, tier));
                // Just crossed into a threshold, or the tempo rose: the next beat is now, not on the old schedule.
                if (nextBeat <= 0f || nextBeat > Time.time + interval) nextBeat = Time.time;
                if (Time.time >= nextBeat)
                {
                    // Added to the schedule, not to "now": the tempo stays even whatever the frame rate does.
                    nextBeat = Mathf.Max(nextBeat + interval, Time.time + interval * 0.5f);
                    beat = 1f;
                    if (vitals > 0f) Heartbeat(config, tier, vitals);
                }
            }
            else nextBeat = 0f;
            PlayerPulse.Beat = beat < 0.002f ? 0f : beat;
            PlayerPulse.Tier = traumaTier;

            if (tier >= 3 && vitals > 0f && Time.time >= nextLabored)
            {
                nextLabored = Time.time + config.LaboredBreathInterval;
                InnerSources();
                breathSource.PlayOneShot(PlayerVitalAudio.LaboredBreath, Mathf.Clamp01(vitals * config.LaboredBreathVolume) * AudioLevels.Sfx);
            }
            Muffle(tier >= 2 ? config.MuffleCutoff : OpenCutoff);
            Blood(config);
            return tier;
        }

        private void Heartbeat(PlayerHealthConfig config, int tier, float vitals)
        {
            InnerSources();
            pulseSource.pitch = PlayerHealthConfig.Pick(config.TraumaHeartbeatPitch, tier);
            pulseSource.PlayOneShot(PlayerVitalAudio.Heartbeat,
                Mathf.Clamp01(vitals * PlayerHealthConfig.Pick(config.TraumaHeartbeatVolume, tier)) * AudioLevels.Sfx);
        }

        // The player's own pulse and breath: their own sources, so they never pass through the muffle or a reverb.
        private void InnerSources()
        {
            if (pulseSource != null) return;
            var go = new GameObject("Trauma audio");
            go.transform.SetParent(transform, false);
            pulseSource = InnerSource(go, 40);
            breathSource = InnerSource(go, 60);
        }

        private static AudioSource InnerSource(GameObject go, int priority)
        {
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = priority;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            return source;
        }

        // A low-pass on the listener muffles everything in the world. It sits on whichever listener is live.
        private void Muffle(float target)
        {
            cutoff = Mathf.Lerp(cutoff, target, 1f - Mathf.Exp(-4f * Time.deltaTime));
            bool on = cutoff < OpenCutoff - 250f;
            if (!on && muffle == null) return;
            AudioListener ear = VoiceListener.Current;
            if (muffle == null || ear == null || muffle.gameObject != ear.gameObject)
            {
                if (muffle != null) Destroy(muffle);
                muffle = ear != null ? ear.gameObject.AddComponent<AudioLowPassFilter>() : null;
                if (muffle == null) return;
                muffle.lowpassResonanceQ = 1f;
            }
            muffle.enabled = on;
            if (on) muffle.cutoffFrequency = cutoff;
        }

        private void Blood(PlayerHealthConfig config)
        {
            // The splatter textures are built in code, one a frame, long before a player can be hurt this badly.
            if (!Application.isBatchMode && !BloodSplatter.Ready) BloodSplatter.WarmStep();
            if (severe <= 0f && nearDeath <= 0f)
            {
                overlay?.Hide();
                return;
            }
            if (overlay == null || !overlay.Valid) overlay = TraumaOverlay.Create();
            if (overlay == null) return;
            float beat = GameSettings.ReduceFlashing ? 0f : PlayerPulse.Beat;
            overlay.Set(severe * config.BloodOpacity.x, nearDeath * config.BloodOpacity.y, drip * config.DripStretch, beat * nearDeath);
        }

        /// <summary>The injury volume for this frame: the hit flash as before, and the low-health vision on top.</summary>
        private void ApplyVision(PlayerHealthConfig config)
        {
            float health = player.Health;
            float beat = GameSettings.ReduceFlashing ? 0f : PlayerPulse.Beat;
            // How deep into each band: 20 -> 11 HP creeps the vignette inward, 10 -> 0 closes it further.
            float sinkSevere = Mathf.InverseLerp(config.SevereHp, config.NearDeathHp + 1f, health);
            float sinkDeath = Mathf.InverseLerp(config.NearDeathHp, 0f, health);
            float severeRing = Mathf.Lerp(config.SevereVignette.x, config.SevereVignette.y, sinkSevere);
            // Near death the ring also swells on every beat, harder the lower the health.
            float deathRing = Mathf.Lerp(config.NearDeathVignette.x, config.NearDeathVignette.y, sinkDeath)
                              + beat * Mathf.Lerp(config.VignettePulse.x, config.VignettePulse.y, sinkDeath);
            float ring = Mathf.Lerp(severeRing, deathRing, nearDeath) * severe;

            volume.weight = Mathf.Clamp01(Mathf.Max(damageFlash, severe));
            vignette.intensity.value = Mathf.Clamp01(Mathf.Max(damageFlash * config.DamageVignette, ring));
            vignette.smoothness.value = Mathf.Lerp(config.TraumaVignetteSmoothness.x, config.TraumaVignetteSmoothness.y, nearDeath);
            fringe.intensity.value = Mathf.Clamp01(
                Mathf.Lerp(config.TraumaChromaticAberration.x, config.TraumaChromaticAberration.y, nearDeath) * severe + beat * 0.12f * nearDeath);
            blur.active = nearDeath > 0f;
            blur.gaussianMaxRadius.value = nearDeath * config.NearDeathBlur;
            warp.intensity.value = nearDeath * config.NearDeathDistortion * (1f + 0.35f * beat);
        }

        /// <summary>Dead, disabled or healed away: everything this adds is switched off at once.</summary>
        private void StopTrauma()
        {
            severe = nearDeath = drip = 0f;
            nextBeat = 0f;
            traumaTier = 0;
            cutoff = OpenCutoff;
            PlayerPulse.Beat = 0f;
            PlayerPulse.Tier = 0;
            if (muffle != null) muffle.enabled = false;
            overlay?.Hide();
        }

        private void DestroyTrauma()
        {
            StopTrauma();
            if (muffle != null) Destroy(muffle);
            overlay?.Remove();
            overlay = null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || player == null || !player.IsSpawned || !player.IsOwner || player.IsDead) return;
            float bpm = traumaTier > 0 ? PlayerHealthConfig.Pick(player.HealthConfig.HeartbeatBpm, traumaTier) : 0f;
            GUI.Label(new Rect(Screen.width - 360f, 74f, 350f, 22f),
                $"TRAUMA tier {traumaTier}  {player.Health:0.0} HP  {bpm:0} BPM  muffle {cutoff:0} Hz  vision {severe:0.00}/{nearDeath:0.00}");
        }
#endif
    }
}
