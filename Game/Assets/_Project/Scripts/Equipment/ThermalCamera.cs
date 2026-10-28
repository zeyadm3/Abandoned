using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Threats;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The thermal camera (0.20.4), local to its owner like night vision: in hand, use switches it on. The
    /// picture goes cold blue and dark, and anything warm in sight (monsters, crewmates) burns orange-white
    /// through darkness and fog, though never through walls. The battery is short and recharges while off.
    /// </summary>
    public class ThermalCamera : MonoBehaviour
    {
        private const float Range = 45f;

        private Volume volume;
        private Texture2D blob;
        private float battery = -1f;
        private bool on;
        // OnGUI runs several times a frame: the mask is looked up once (GetMask allocates).
        private int sightMask;

        private static PlayerEquipment Gear(out NetworkPlayer me)
        {
            me = NetworkPlayer.Local;
            return me != null ? me.GetComponent<PlayerEquipment>() : null;
        }

        private void Update()
        {
            PlayerEquipment gear = Gear(out NetworkPlayer me);
            ToolConfig config = ToolConfig.Current;
            if (battery < 0f) battery = config.ThermalSeconds;
            bool inHand = gear != null && gear.InHand != null && gear.InHand.Kind == EquipmentKind.ThermalCamera;
            if (!inHand || me == null || me.IsDead) on = false;
            else if (me.TryGetComponent(out PlayerInputReader input) && input.Current.UsePressed && battery > 1f && !Core.CursorOwner.UiActive)
            {
                on = !on;
                Audio.GameAudio.Play(on ? Audio.SoundId.NightVisionOn : Audio.SoundId.NightVisionOff, me.transform.position + Vector3.up * 1.6f, 0.6f);
            }
            if (on)
            {
                battery -= Time.deltaTime;
                if (battery <= 0f) { battery = 0f; on = false; }
            }
            else battery = Mathf.Min(config.ThermalSeconds, battery + config.ThermalRecharge * Time.deltaTime);
            if (on && volume == null) Build();
            if (volume != null) volume.weight = on ? 1f : 0f;
            if (on) UI.HudCallout.Set(this, Loc.T("THERMAL  {0:0} s", battery));
            else UI.HudCallout.Clear(this);
        }

        private void Build()
        {
            var go = new GameObject("ThermalVolume");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 51f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.6f);
            color.colorFilter.Override(new Color(0.35f, 0.5f, 1f));
            color.saturation.Override(-85f);
            color.contrast.Override(25f);
            profile.Add<FilmGrain>(true).intensity.Override(0.4f);
            profile.Add<Vignette>(true).intensity.Override(0.5f);
            volume.profile = profile;
            blob = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32f;
                float a = Mathf.Clamp01(1f - d);
                blob.SetPixel(x, y, new Color(1f, Mathf.Lerp(0.35f, 1f, a * a), a * a * 0.6f, Mathf.SmoothStep(0f, 1f, a)));
            }
            blob.Apply();
        }

        private void OnGUI()
        {
            if (!on || blob == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (Threat t in Threat.All) if (t != null) Heat(cam, t.transform.position, 1.2f, 1.9f);
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && !p.IsOwner && !p.IsDead) Heat(cam, p.transform.position, 0.6f, 1.8f);
        }

        private void OnDestroy()
        {
            UI.HudCallout.Clear(this);
            if (volume != null && volume.profile != null) Destroy(volume.profile);
            if (blob != null) Destroy(blob);
        }

        // A warm body: an orange-white blob over its outline, only where the camera could actually see it.
        private void Heat(Camera cam, Vector3 feet, float width, float height)
        {
            Vector3 eye = cam.transform.position, chest = feet + Vector3.up * height * 0.55f;
            if (Vector3.Distance(eye, chest) > Range) return;
            if (sightMask == 0) sightMask = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");
            if (Physics.Linecast(eye, chest, out RaycastHit hit, sightMask, QueryTriggerInteraction.Ignore) && hit.distance < Vector3.Distance(eye, chest) - 1f) return;
            Vector3 top = cam.WorldToScreenPoint(feet + Vector3.up * height), bottom = cam.WorldToScreenPoint(feet);
            if (top.z <= 0f || bottom.z <= 0f) return;
            float h = Mathf.Abs(top.y - bottom.y), w = h * width / height * 1.6f;
            GUI.color = new Color(1f, 1f, 1f, 0.9f);
            GUI.DrawTexture(new Rect(bottom.x - w * 0.5f, Screen.height - top.y, w, h), blob);
        }
    }
}
