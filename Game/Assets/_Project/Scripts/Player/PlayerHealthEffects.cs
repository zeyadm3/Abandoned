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
    public class PlayerHealthEffects : MonoBehaviour
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
                return;
            }
            PlayerHealthConfig config = player.HealthConfig;
            float low = 1f - Mathf.Clamp01(player.Health01 / config.LowHealthThreshold);
            damageFlash = Mathf.MoveTowards(damageFlash, 0f, Time.deltaTime / config.DamageFadeSeconds);
            if (damageFlash > 0f || low > 0f)
            {
                if (volume == null) BuildVolume();
                volume.weight = Mathf.Clamp01(Mathf.Max(damageFlash, low));
                vignette.intensity.value = Mathf.Max(damageFlash * config.DamageVignette, low * 0.48f);
                blur.active = low > 0f;
                blur.gaussianMaxRadius.value = low * 0.75f;
            }
            else if (volume != null) volume.weight = 0f;

            RunState run = RunState.Current;
            bool insideJob = run != null && (run.State.Phase == RunPhase.Running || run.State.Phase == RunPhase.Honking);
            bool inTruck = TruckCargo.Current != null && TruckCargo.Current.Carries(player.transform.position);
            if (!insideJob || inTruck) return;
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
            }
            float darkness = equipment == null || !equipment.LightOn ? 0.28f : 0.05f;
            float pressure = Mathf.Max(low, threatPressure, darkness, player.Motor.IsSprinting ? 0.4f : 0f);
            if (Time.time >= nextHeartbeat)
            {
                nextHeartbeat = Time.time + Mathf.Lerp(1.05f, 0.48f, pressure);
                GameAudio.PlayHeartbeat(config.HeartbeatVolume * Mathf.Lerp(0.18f, 1f, pressure));
            }
            if (Time.time >= nextBreath)
            {
                nextBreath = Time.time + Mathf.Lerp(4.2f, 2f, pressure);
                GameAudio.PlayBreath(Mathf.Lerp(0.04f, 0.18f, pressure));
            }
        }

        private void BuildVolume()
        {
            var go = new GameObject("Injury vision");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 80f;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            vignette = profile.Add<Vignette>(true);
            vignette.color.Override(new Color(0.45f, 0.012f, 0.006f));
            vignette.smoothness.Override(0.75f);
            vignette.intensity.Override(0f);
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
        }

        private void OnDestroy()
        {
            if (profile != null) Destroy(profile);
            if (volume != null) Destroy(volume.gameObject);
        }
    }
}
