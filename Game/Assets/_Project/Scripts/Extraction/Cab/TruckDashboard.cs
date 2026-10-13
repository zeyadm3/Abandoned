using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck cab, made to work (0.21.4): the 0.15.7b cab already has seats, a dashboard, a wheel and
    /// mirrors; now its two gauges read the haul against quota and how full the bay is (seen through the
    /// windscreen), a lamp shows green once quota is met, the dome light comes on while the truck honks to
    /// leave, and a horn button beside the ignition gives a short toot (RunCab) to call the crew back.
    /// </summary>
    public class TruckDashboard : MonoBehaviour, IUsable
    {
        [SerializeField] private Transform haulNeedle, cargoNeedle;
        [SerializeField] private Renderer quotaLamp;
        [SerializeField] private Light dome;

        private MaterialPropertyBlock block;
        private bool shownMet;

        private void Start()
        {
            if (dome != null) Structure.LevelStoreys.LightEverything(dome);
            SetLamp(false);
        }

        private void Update()
        {
            RunState run = RunState.Current;
            bool live = run != null && run.IsSpawned;
            float haul = live && run.State.Quota > 0 ? Mathf.Clamp01((float)run.State.Haul / run.State.Quota) : 0f;
            float cargo = live && run.State.CargoCapacity > 0f ? Mathf.Clamp01(run.State.CargoVolume / run.State.CargoCapacity) : 0f;
            Needle(haulNeedle, haul);
            Needle(cargoNeedle, cargo);
            bool met = live && run.State.Haul >= run.State.Quota && run.State.Quota > 0;
            if (met != shownMet) SetLamp(met);
            // The cab lights up once the truck is about to go: one more sign it's leaving.
            if (dome != null) dome.enabled = live && run.State.Phase == RunPhase.Honking;
        }

        // A needle sweeps from -120° (empty) to +120° (full), with a little tremble from the engine.
        private static void Needle(Transform needle, float value)
        {
            if (needle == null) return;
            float target = Mathf.Lerp(-120f, 120f, value) + Mathf.Sin(Time.time * 31f) * 0.6f;
            float z = Mathf.MoveTowardsAngle(needle.localEulerAngles.z, -target, 180f * Time.deltaTime);
            needle.localRotation = Quaternion.Euler(0f, 0f, z);
        }

        private void SetLamp(bool met)
        {
            shownMet = met;
            if (quotaLamp == null) return;
            block ??= new MaterialPropertyBlock();
            quotaLamp.GetPropertyBlock(block);
            Color c = met ? new Color(0.2f, 1f, 0.3f) : new Color(0.6f, 0.1f, 0.05f);
            block.SetColor("_BaseColor", c);
            block.SetColor("_EmissionColor", c * (met ? 3f : 0.6f));
            quotaLamp.SetPropertyBlock(block);
        }

        public string UsePrompt(GameObject user) => RunCab.Current != null && RunCab.Current.IsSpawned ? Loc.T("Toot the horn (calls the crew, and anything else)") : null;

        public void Use(GameObject user) => RunCab.Current?.RequestToot();

#if UNITY_EDITOR
        public void EditorSetup(Transform haul, Transform fill, Renderer lamp, Light light)
        {
            haulNeedle = haul;
            cargoNeedle = fill;
            quotaLamp = lamp;
            dome = light;
        }
#endif
    }
}
