using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A store's alarm box (0.16.5), inside by the shopfront: a blinking red lamp when the store is wired,
    /// a bell and a flashing lamp while it rings. Bolt cutters in hand + E disarm it. Scene object on every
    /// machine; the state is <see cref="RunAlarms"/>.
    /// </summary>
    public class StoreAlarm : MonoBehaviour, IUsable, IHintSource
    {
        private static readonly List<StoreAlarm> Live = new();
        private const float BellInterval = 1.1f;

        [SerializeField] private RollerShutter shutter;
        [SerializeField] private Renderer lamp;

        private MaterialPropertyBlock block;
        private float nextBell;

        public static IReadOnlyList<StoreAlarm> All => Live;
        public RollerShutter Shutter => shutter;
        public int Store => shutter != null ? shutter.Index : -1;
        public HintId Hint => HintId.Alarm;

        public static StoreAlarm ForStore(int store)
        {
            foreach (StoreAlarm a in Live) if (a != null && a.Store == store) return a;
            return null;
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        private void Update()
        {
            RunAlarms run = RunAlarms.Current;
            bool wired = run != null && run.Wired(Store) && !run.Disarmed(Store);
            bool ringing = run != null && run.Ringing(Store);
            float glow = ringing ? (Mathf.Repeat(Time.time * 4f, 1f) < 0.5f ? 4f : 0.2f)
                : wired && run.Armed(Store) ? (Mathf.Repeat(Time.time, 2f) < 0.12f ? 3f : 0.1f) : 0f;
            if (lamp != null)
            {
                block ??= new MaterialPropertyBlock();
                lamp.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", new Color(1f, 0.08f, 0.05f) * glow);
                block.SetColor("_BaseColor", glow > 0.5f ? new Color(1f, 0.2f, 0.15f) : new Color(0.3f, 0.05f, 0.05f));
                lamp.SetPropertyBlock(block);
            }
            if (ringing && Time.time >= nextBell)
            {
                nextBell = Time.time + BellInterval;
                GameAudio.Play(SoundId.AlarmBell, transform.position, 1f);
            }
        }

        public string UsePrompt(GameObject user)
        {
            RunAlarms run = RunAlarms.Current;
            if (run == null || !run.Wired(Store) || run.Disarmed(Store)) return null;
            PlayerEquipment gear = user != null ? user.GetComponentInParent<PlayerEquipment>() : null;
            if (gear == null || !gear.Has(EquipmentKind.BoltCutters)) return run.Ringing(Store) ? "Alarm ringing (bolt cutters cut it)" : "Wired alarm (bolt cutters cut it)";
            return "Cut the alarm wires";
        }

        public void Use(GameObject user)
        {
            PlayerEquipment gear = user != null ? user.GetComponentInParent<PlayerEquipment>() : null;
            if (gear != null && gear.Has(EquipmentKind.BoltCutters)) RunAlarms.Current?.RequestDisarm(Store);
        }

#if UNITY_EDITOR
        public void EditorSetup(RollerShutter store, Renderer indicator)
        {
            shutter = store;
            lamp = indicator;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
