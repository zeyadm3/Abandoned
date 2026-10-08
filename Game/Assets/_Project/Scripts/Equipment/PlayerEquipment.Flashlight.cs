using Abandoned.Extraction;
using Abandoned.Threats;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    public partial class PlayerEquipment
    {
        [SerializeField] private FlashlightConfig flashlightConfig;
        private readonly NetworkVariable<float> battery = new(420f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private float drainAccumulator, rechargeAccumulator, nextThreatLightScan, flickerPressure;

        public FlashlightConfig FlashlightConfig => flashlightConfig != null ? flashlightConfig : Abandoned.Equipment.FlashlightConfig.Default;
        public float BatterySeconds => battery.Value;
        public float Battery01 => Mathf.Clamp01(BatterySeconds / FlashlightConfig.BatterySeconds);
        public bool LightSwitchedOn => state.Value.LightOn && Has(EquipmentKind.Flashlight) && BatterySeconds > 0f;
        public bool LightOn => LightSwitchedOn && !IsDead && HandsFreeForLight;
        public Light Flashlight => flashlight;

        private void InitializeFlashlight()
        {
            if (IsServer) ServerResetBattery();
            if (flashlight == null) return;
            flashlight.range = FlashlightConfig.Range;
            flashlight.intensity = FlashlightConfig.Intensity;
            flashlight.spotAngle = FlashlightConfig.SpotAngle;
            flashlight.color = FlashlightConfig.Color;
        }

        public void ServerResetBattery()
        {
            if (!IsServer) return;
            battery.Value = FlashlightConfig.BatterySeconds;
            drainAccumulator = 0f;
        }

        private bool RefillBattery()
        {
            if (!IsServer || IsDead || !Has(EquipmentKind.Flashlight) || Battery01 >= 0.999f)
            {
                HintRpc("Keep a flashlight in your other slot; a fresh cell replaces a used battery.");
                return false;
            }
            ServerResetBattery();
            SoundRpc(Audio.SoundId.FlashlightClick, Eye);
            return true;
        }

        private void TickFlashlight()
        {
            RunState run = RunState.Current;
            bool activeRun = run != null && (run.State.Phase == RunPhase.Running || run.State.Phase == RunPhase.Honking);
            // QA D-01: the truck's lamp socket tops the battery up while you stand in the bay.
            if (IsServer && activeRun && !IsDead && Has(EquipmentKind.Flashlight) && Battery01 < 1f && FlashlightConfig.TruckRechargePerSecond > 0f
                && TruckCargo.Current != null && TruckCargo.Current.Carries(transform.position))
            {
                rechargeAccumulator += Time.deltaTime;
                if (rechargeAccumulator >= 0.25f)
                {
                    battery.Value = Mathf.Min(FlashlightConfig.BatterySeconds, battery.Value + rechargeAccumulator * FlashlightConfig.TruckRechargePerSecond);
                    rechargeAccumulator = 0f;
                }
            }
            else if (IsServer && LightOn && activeRun)
            {
                drainAccumulator += Time.deltaTime;
                if (drainAccumulator >= 0.25f)
                {
                    battery.Value = Mathf.Max(0f, battery.Value - drainAccumulator);
                    drainAccumulator = 0f;
                    if (battery.Value <= 0f)
                    {
                        EquipState s = state.Value;
                        s.LightOn = false;
                        state.Value = s;
                    }
                }
            }
            if (flashlight == null || !LightOn) return;
            if (Time.time >= nextThreatLightScan)
            {
                nextThreatLightScan = Time.time + 0.2f;
                flickerPressure = 0f;
                foreach (Threat threat in Threat.All)
                {
                    if (threat == null || threat.NetworkManager != NetworkManager) continue;
                    float distance = Vector3.Distance(threat.transform.position, transform.position);
                    flickerPressure = Mathf.Max(flickerPressure, 1f - Mathf.Clamp01(distance / FlashlightConfig.ThreatFlickerRange));
                }
            }
            float flicker = Mathf.PerlinNoise(OwnerClientId * 1.7f, Time.time * FlashlightConfig.FlickerFrequency);
            float dip = flicker < 0.38f ? FlashlightConfig.MinimumFlickerIntensity : Mathf.Lerp(0.65f, 1f, flicker);
            // QA O-01: the warning is still there with flashing reduced, as a slow fade rather than a strobe.
            if (Core.GameSettings.ReduceFlashing)
                dip = Mathf.Lerp(0.45f, 0.8f, Mathf.PerlinNoise(OwnerClientId * 1.7f, Time.time * 0.6f));
            flashlight.intensity = FlashlightConfig.Intensity * Mathf.Lerp(1f, dip, flickerPressure);
        }
    }
}
