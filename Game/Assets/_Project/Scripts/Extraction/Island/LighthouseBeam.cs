using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Threats;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The island prison's lighthouse lamp. Thrown like a breaker (its own power sector in <see cref="RunBreakers"/>,
    /// so the host already owns when it runs and every machine already knows); while it runs, the lamp turns and two
    /// opposed beams sweep the outdoor routes. Every machine turns it from the shared server clock, so everyone sees
    /// the same sweep. The host dazzles monsters the beam crosses (<see cref="Threat.Daze"/>): a moment's grace, never
    /// a kill. Scene object, not networked itself; it ignores the building's power (it has its own motor).
    /// </summary>
    public class LighthouseBeam : MonoBehaviour
    {
        [Tooltip("The power sector of the lamp-room switch (a BreakerPanel flagged startsLighthouse).")]
        [SerializeField, Range(0, RunBreakers.MaxSectors - 1)] private int sector = RunBreakers.MaxSectors - 1;
        [Tooltip("Turns about its Y axis; the beams are its children.")]
        [SerializeField] private Transform head;
        [SerializeField] private Light[] beams;
        [Tooltip("Lens and visible beam shafts: shown only while the lamp turns.")]
        [SerializeField] private Renderer[] glow;

        private bool shownOn;
        private float nextCheck;
        private int dazed;
        private SoundLoop motor;
        private readonly RaycastHit[] hits = new RaycastHit[8];

        private bool On => RunBreakers.Current != null && RunBreakers.Current.Restored(sector);

        private void Awake() => Show(false);

        private void Update()
        {
            bool on = On;
            if (on != shownOn) Show(on);
            if (!on) return;
            LighthouseConfig config = LighthouseConfig.Current;
            double now = RunState.Current != null ? RunState.Current.Now : Time.timeAsDouble;
            if (head != null) head.localRotation = Quaternion.Euler(0f, (float)(now * config.TurnSpeed % 360d), 0f);

            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsServer || !net.IsListening || Time.time < nextCheck) return;
            nextCheck = Time.time + config.CheckInterval;
            DazzleMonsters(config);
        }

        private void Show(bool on)
        {
            shownOn = on;
            if (beams != null)
                foreach (Light beam in beams)
                    if (beam != null) { beam.enabled = on; beam.intensity = LighthouseConfig.Current.BeamIntensity; }
            if (glow != null)
                foreach (Renderer r in glow)
                    if (r != null) r.enabled = on;
            if (Application.isBatchMode) return;
            if (on && motor == null)
            {
                motor = GameAudio.Loop(SoundId.LighthouseMotor, transform, "LighthouseMotor", spatial: true);
                motor.Level = .8f;
            }
            if (motor != null) motor.Level = on ? .8f : 0f;
        }

        // Host: any monster the beam is crossing, in the open, within range, gets a moment's daze.
        private void DazzleMonsters(LighthouseConfig config)
        {
            if (head == null) return;
            Vector3 lamp = head.position;
            Vector3 a = Flat(head.forward), b = -a;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            foreach (Threat t in Threat.All)
            {
                if (t == null || !t.IsSpawned) continue;
                Vector3 chest = t.transform.position + Vector3.up * 1.2f;
                Vector3 to = chest - lamp;
                if (to.sqrMagnitude > config.Range * config.Range) continue;
                Vector3 flat = Flat(to);
                if (Vector3.Angle(a, flat) > config.HalfWidth && Vector3.Angle(b, flat) > config.HalfWidth) continue;
                if (!Clear(lamp, chest, t.transform, mask)) continue;
                if (t.Daze(config.DazeSeconds, config.DazeCooldown, flat)) dazed++;
            }
        }

        private bool Clear(Vector3 from, Vector3 to, Transform target, int mask)
        {
            Vector3 d = to - from;
            float length = d.magnitude;
            int n = Physics.RaycastNonAlloc(from, d / length, hits, length, mask, QueryTriggerInteraction.Ignore);
            if (n == hits.Length) return false;
            for (int i = 0; i < n; i++)
            {
                Transform hit = hits[i].collider.transform;
                // The tower's own glass and frame don't count.
                if (!hit.IsChildOf(target) && !hit.IsChildOf(transform)) return false;
            }
            return true;
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.forward;
        }

#if UNITY_EDITOR
        public void EditorSetup(int powerSector, Transform turning, Light[] beamLights, Renderer[] shown)
        {
            sector = powerSector;
            head = turning;
            beams = beamLights;
            glow = shown;
        }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            float left = RunBreakers.Current != null ? RunBreakers.Current.Remaining(sector) : 0f;
            GUI.Label(new Rect(Screen.width - 380, 254, 375, 22), $"LIGHTHOUSE: {(shownOn ? $"turning, {left:0} s left" : "dark")} · {dazed} monsters dazzled");
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible || head == null) return;
            LighthouseConfig config = LighthouseConfig.Current;
            Gizmos.color = shownOn ? new Color(1f, .95f, .6f, .8f) : new Color(.5f, .5f, .5f, .4f);
            Vector3 f = Flat(head.forward) * config.Range;
            foreach (float side in new[] { -config.HalfWidth, config.HalfWidth })
            {
                Gizmos.DrawLine(head.position, head.position + Quaternion.Euler(0f, side, 0f) * f);
                Gizmos.DrawLine(head.position, head.position - Quaternion.Euler(0f, side, 0f) * f);
            }
        }
#endif
    }
}
