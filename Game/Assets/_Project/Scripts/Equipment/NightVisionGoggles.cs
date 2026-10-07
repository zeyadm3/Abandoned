using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Equipment
{
    /// <summary>
    /// Night vision in hand (GDD 12, M10.5): use it (empty hands) to switch it on: the picture goes green,
    /// bright and grainy, and a short-range infrared lamp lights what's near. The battery lasts a couple of
    /// minutes and recharges while it's off. Purely local to its owner: nobody else sees the IR lamp.
    /// </summary>
    public class NightVisionGoggles : MonoBehaviour
    {
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private Transform eye;
        [SerializeField, Min(5f)] private float batterySeconds = 120f;
        [Tooltip("Battery seconds regained per second while off.")]
        [SerializeField, Min(0f)] private float rechargeRate = 0.35f;
        [SerializeField, Range(0f, 5f)] private float exposure = 2.4f;

        private Volume volume;
        private Light illuminator;
        private UnityEngine.UIElements.Label panel;
        private float battery = -1f;
        private bool on;

        public bool On => on;
        public float Battery01 => battery < 0f ? 1f : battery / batterySeconds;

        private bool InHand => equipment != null && equipment.InHand != null && equipment.InHand.Kind == EquipmentKind.NightVision;

        private void Update()
        {
            if (equipment == null || !equipment.IsOwner) return;
            if (battery < 0f) battery = batterySeconds;
            bool dead = TryGetComponent(out NetworkPlayer player) && player.IsDead;
            if (!InHand || dead) on = false;
            else if (inputReader != null && inputReader.Current.UsePressed && (carrier == null || carrier.Held == null) && battery > 1f)
            {
                on = !on;
                Audio.GameAudio.Play(Audio.SoundId.FlashlightClick, transform.position + Vector3.up * 1.6f, 0.5f);
            }
            if (on)
            {
                battery -= Time.deltaTime;
                if (battery <= 0f) { battery = 0f; on = false; }
            }
            else battery = Mathf.Min(batterySeconds, battery + rechargeRate * Time.deltaTime);
            Apply();
            ShowBattery();
        }

        private void Apply()
        {
            if (on && volume == null) Build();
            if (volume != null) volume.weight = on ? 1f : 0f;
            if (illuminator != null) illuminator.enabled = on;
        }

        // Made on first use, on the owner's machine only.
        private void Build()
        {
            var go = new GameObject("NightVisionVolume");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(exposure);
            color.colorFilter.Override(new Color(0.45f, 1f, 0.5f));
            color.saturation.Override(-70f);
            color.contrast.Override(15f);
            profile.Add<FilmGrain>(true).intensity.Override(0.7f);
            profile.Add<Vignette>(true).intensity.Override(0.45f);
            volume.sharedProfile = profile;

            var lamp = new GameObject("InfraredLamp");
            lamp.transform.SetParent(eye != null ? eye : transform, false);
            illuminator = lamp.AddComponent<Light>();
            illuminator.type = LightType.Point;
            illuminator.range = 14f;
            illuminator.intensity = 1.6f;
            illuminator.color = new Color(0.75f, 1f, 0.75f);
            illuminator.shadows = LightShadows.None;
        }

        private void ShowBattery()
        {
            if (!InHand)
            {
                if (panel != null) UI.MenuKit.Show(panel, false);
                return;
            }
            if (panel == null && (panel = UI.HudLayer.Label("hud-panel", "hud-nightvision")) == null) return;
            string s = $"NIGHT VISION {(on ? "ON" : "OFF")}  {Battery01:P0}\n<color=#999999>{Core.InputBindings.Display("Use")}: switch</color>";
            UI.MenuKit.Show(panel, true);
            if (panel.text != s) panel.text = s;
        }

        private void OnDisable()
        {
            on = false;
            Apply();
            if (panel != null) UI.MenuKit.Show(panel, false);
        }

        private void OnDestroy() => UI.HudLayer.Remove(panel);
    }
}
