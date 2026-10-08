using Abandoned.Audio;
using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Threats;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Player
{
    /// <summary>Owner-only injury feedback and pressure audible only inside this salvager's head.</summary>
    public partial class PlayerHealthEffects : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerEquipment equipment;
        private Volume volume;
        private VolumeProfile profile;
        private Vignette vignette;
        private DepthOfField blur;
        private float damageFlash, nextHeartbeat, nextBreath, nextThreatScan, threatPressure;

        private void OnEnable() => NetworkPlayer.HealthChanged += OnHealthChanged;

        private void OnHealthChanged(NetworkPlayer changed, float before, float after)
        {
            if (changed != player || !player.IsOwner || after >= before) return;
            damageFlash = Mathf.Max(damageFlash, Mathf.Clamp01((before - after) / player.MaxHealth * 2f + 0.25f));
        }

        private void Update()
        {
            if (player == null || !player.IsSpawned || !player.IsOwner) return;
            if (player.IsDead)
            {
                damageFlash = 0f;
                if (volume != null) volume.weight = 0f;
                StopTrauma();
                return;
            }
            PlayerHealthConfig config = player.HealthConfig;
            float low = 1f - Mathf.Clamp01(player.Health01 / config.LowHealthThreshold);
            damageFlash = Mathf.MoveTowards(damageFlash, 0f, Time.deltaTime / config.DamageFadeSeconds);
            // QA B-30: the player sets how loud their own heartbeat and breath are (0 = off).
            float vitals = Core.GameSettings.VitalsVolume;
            // Low health by threshold (PlayerHealthEffects.Trauma): it runs wherever the player is, job or not.
            int tier = TickTrauma(config, vitals);
            if (damageFlash > 0f || severe > 0f)
            {
                if (volume == null) BuildVolume(config);
                ApplyVision(config);
            }
            else if (volume != null) volume.weight = 0f;

            RunState run = RunState.Current;
            bool insideJob = run != null && (run.State.Phase == RunPhase.Running || run.State.Phase == RunPhase.Honking);
            bool inTruck = TruckCargo.Current != null && TruckCargo.Current.Carries(player.transform.position);
            if (!insideJob || inTruck || vitals <= 0f) return;
            if (Time.time >= nextThreatScan)
            {
                nextThreatScan = Time.time + 0.25f;
                threatPressure = 0f;
                foreach (Threat threat in Threat.All)
                {
                    if (threat == null || threat.NetworkManager != player.NetworkManager) continue;
                    float distance = Vector3.Distance(threat.transform.position, player.transform.position);
                    threatPressure = Mathf.Max(threatPressure, 1f - Mathf.Clamp01(distance / config.ThreatHeartbeatRange));
                }
                // A Stalker working up to a rush on this player is felt before it is seen (2026-10-09).
                Stalker stalker = Stalker.PressingOn(player);
                if (stalker != null) threatPressure = Mathf.Max(threatPressure, stalker.Dread01);
            }
            float darkness = equipment == null || !equipment.LightOn ? config.DarknessFear.x : config.DarknessFear.y;
            float pressure = Mathf.Max(low, threatPressure, darkness, player.Motor.IsSprinting ? config.SprintFear : 0f);
            // From critical down the trauma pulse owns the heartbeat (one steady tempo per threshold), and near
            // death it owns the breathing too; fear only sets the pace while the player is still above 30 HP.
            if (tier == 0 && Time.time >= nextHeartbeat)
            {
                nextHeartbeat = Time.time + Mathf.Lerp(config.HeartbeatInterval.x, config.HeartbeatInterval.y, pressure);
                GameAudio.PlayHeartbeat(vitals * config.HeartbeatVolume * Mathf.Lerp(0.18f, 1f, pressure));
            }
            if (tier < 3 && Time.time >= nextBreath)
            {
                nextBreath = Time.time + Mathf.Lerp(config.BreathInterval.x, config.BreathInterval.y, pressure);
                GameAudio.PlayBreath(vitals * Mathf.Lerp(0.04f, 0.18f, pressure));
            }
        }

        private void BuildVolume(PlayerHealthConfig config)
        {
            var go = new GameObject("Injury vision");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 80f;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            vignette = profile.Add<Vignette>(true);
            vignette.color.Override(config.TraumaVignetteColor);
            vignette.smoothness.Override(config.TraumaVignetteSmoothness.x);
            vignette.intensity.Override(0f);
            fringe = profile.Add<ChromaticAberration>(true);
            fringe.intensity.Override(0f);
            warp = profile.Add<LensDistortion>(true);
            warp.intensity.Override(0f);
            warp.scale.Override(1.02f); // hides the stretched border the warp would show
            blur = profile.Add<DepthOfField>(true);
            blur.mode.Override(DepthOfFieldMode.Gaussian);
            blur.gaussianStart.Override(0f);
            blur.gaussianEnd.Override(0.7f);
            blur.gaussianMaxRadius.Override(0f);
            volume.sharedProfile = profile;
        }

        private void OnDisable()
        {
            NetworkPlayer.HealthChanged -= OnHealthChanged;
            damageFlash = 0f;
            if (volume != null) volume.weight = 0f;
            StopTrauma();
        }

        private void OnDestroy()
        {
            if (profile != null) Destroy(profile);
            if (volume != null) Destroy(volume.gameObject);
            DestroyTrauma();
        }
    }
}
