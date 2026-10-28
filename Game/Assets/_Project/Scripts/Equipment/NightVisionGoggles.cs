using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

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
            // A click on a screen (pause, gear rack) is not a press of the switch.
            else if (inputReader != null && inputReader.Current.UsePressed && (carrier == null || carrier.Held == null) && battery > 1f && !CursorOwner.UiActive)
            {
                on = !on;
                Audio.GameAudio.Play(on ? Audio.SoundId.NightVisionOn : Audio.SoundId.NightVisionOff, transform.position + Vector3.up * 1.6f, 0.6f);
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
            Structure.LevelStoreys.LightEverything(illuminator);
        }

        private void ShowBattery()
        {
            if (!InHand)
            {
                if (panel != null) UI.MenuKit.Show(panel, false);
                return;
            }
            if (panel == null || panel.panel == null)
            {
                UI.HudLayer.Remove(panel);
                panel = UI.HudLayer.Label("hud-panel", "hud-nightvision", "instrument-hud");
                if (panel == null) return;
            }
            string s = Loc.T("NIGHT VISION {0}  {1:P0}\n<color=#999999>{2}: switch</color>", (on ? Loc.T("ON") : Loc.T("OFF")), Battery01, Core.InputBindings.Display("Use"));
            UI.MenuKit.Show(panel, true);
            if (panel.text != s) panel.text = s;
            PositionBattery();
        }

        private void PositionBattery()
        {
            VisualElement hud = UI.HudLayer.Root;
            if (!UI.HudLayer.TryLocalRect(hud, hud, out Rect area)) return;
            bool found = UI.HudLayer.TryLocalRect(hud.Q(className: "h-inventory"), hud, out Rect anchor);
            if (UI.HudLayer.TryLocalRect(hud.Q(className: "h-inspection"), hud, out Rect inspection) && (!found || inspection.yMin < anchor.yMin))
            {
                anchor = inspection;
                found = true;
            }
            panel.style.top = StyleKeyword.Auto;
            panel.style.right = found ? Mathf.Max(0f, area.width - anchor.xMax) : 28f;
            panel.style.bottom = found ? Mathf.Max(28f, area.height - anchor.yMin + 12f) : 28f;
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
