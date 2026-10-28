using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// An Iron Factory machine that comes back to life while its power sector's breaker is thrown
    /// (<see cref="RunBreakers"/>): dead until then, running for the breaker's few minutes, and loud. Nothing here
    /// is networked: the breaker state is, and every strike falls on the shared server clock, so every machine
    /// sees the same slam at the same moment. Only the host deals the damage. Scene object on every machine.
    /// </summary>
    public class MachineHazard : MonoBehaviour
    {
        private static readonly List<MachineHazard> Live = new();

        [SerializeField] private MachineHazardKind kind;
        [Tooltip("The danger zone in the machine's own space (striped on the floor). Players inside take the strike.")]
        [SerializeField] private Bounds zone = new(Vector3.up, new Vector3(2f, 2f, 2f));
        [Tooltip("Where in its cycle this machine starts (0-1), so a row of presses doesn't slam as one.")]
        [SerializeField, Range(0f, 1f)] private float phase;
        [Tooltip("Press: the ram that drops on a strike.")]
        [OptionalReference, SerializeField] private Transform mover;
        [SerializeField] private Vector3 moverStrike = new(0f, -1f, 0f);
        [Tooltip("Shown only during a strike (steam jets).")]
        [OptionalReference, SerializeField] private GameObject plume;
        [Tooltip("Flashes amber before a strike and while the machine runs.")]
        [OptionalReference, SerializeField] private Renderer lamp;
        [Tooltip("Belt: the platform players ride, and which way (machine space) it carries them.")]
        [OptionalReference, SerializeField] private MovingPlatform belt;
        [SerializeField] private Vector3 beltDirection = Vector3.forward;

        private int sector = -1;
        private bool running;
        private long lastCycle = long.MinValue;
        private Vector3 moverRest;
        private float shownPose = -1f;
        private int shownLamp = -1;
        private MaterialPropertyBlock block;

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        private void Awake()
        {
            if (mover != null) moverRest = mover.localPosition;
            if (plume != null) plume.SetActive(false);
        }

        private void Update()
        {
            RunState run = RunState.Current;
            bool on = run != null && run.IsSpawned && RunBreakers.Current != null && Sector() >= 0 && RunBreakers.Current.Restored(sector);
            if (on != running)
            {
                running = on;
                lastCycle = long.MinValue;
                if (on) GameAudio.Play(SoundId.MachineStart, transform.position + Vector3.up, 1f);
            }
            MachineHazardConfig config = MachineHazardConfig.Current;
            if (!running)
            {
                Show(0f, 0);
                if (belt != null) belt.Drift(Vector3.zero);
                return;
            }
            if (kind == MachineHazardKind.Belt)
            {
                Show(0f, 1);
                if (belt != null) belt.Drift(transform.TransformDirection(beltDirection.normalized) * config.BeltSpeed * Time.deltaTime);
                return;
            }

            MachineHazardConfig.Cycle cycle = config.For(kind);
            double time = run.Now + phase * cycle.Period;
            long index = (long)System.Math.Floor(time / cycle.Period);
            float into = (float)(time - index * cycle.Period);
            bool striking = into < cycle.Strike;
            bool warning = into >= cycle.Period - cycle.Warning;
            // The ram comes down hard and lifts back over the strike.
            Show(striking ? 1f - into / cycle.Strike : 0f, striking || warning ? 2 : 1);
            if (striking && index != lastCycle)
            {
                lastCycle = index;
                Strike(config, cycle);
            }
        }

        // Which power sector it is on: the same sums the ceiling lights use, once the horror config is there.
        private int Sector()
        {
            if (sector < 0 && RunHorrorDirector.Current != null && RunHorrorDirector.Current.Config != null)
                sector = RunHorrorDirector.Current.Config.SectorOf(transform.position);
            return sector;
        }

        private void Strike(MachineHazardConfig config, MachineHazardConfig.Cycle cycle)
        {
            Vector3 at = transform.TransformPoint(zone.center);
            if (kind == MachineHazardKind.Press)
            {
                GameAudio.Play(SoundId.PressSlam, at, 1f);
                CameraShake.Emit(at, 900f);
            }
            else if (kind == MachineHazardKind.Steam) GameAudio.Play(SoundId.SteamBlast, at, 1f);

            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsServer) return;
            if (cycle.Noise > 0f) NoiseSystem.Emit(at, cycle.Noise, NoiseSource.Other);
            if (cycle.Damage <= 0f) return;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead || p.IsHiding) continue;
                Vector3 feet = p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;
                if (!zone.Contains(transform.InverseTransformPoint(feet))) continue;
                p.DealDamage(cycle.Damage, config.DeathLine(kind));
                Vector3 away = feet - transform.position;
                away.y = 0f;
                if (away.sqrMagnitude > .01f) p.ServerKnock(away.normalized * config.Knock);
            }
        }

        // Pose of the moving parts (0 rest, 1 struck) and the lamp (0 off, 1 running, 2 warning/striking).
        private void Show(float pose, int lampState)
        {
            if (!Mathf.Approximately(pose, shownPose))
            {
                shownPose = pose;
                if (mover != null) mover.localPosition = moverRest + moverStrike * pose;
                if (plume != null) plume.SetActive(pose > 0f);
            }
            if (lamp == null) return;
            // Warning flashes; steady while running; dark when dead.
            int state = lampState == 2 && Mathf.Repeat(Time.time * 4f, 1f) < .5f ? 1 : lampState;
            if (state == shownLamp) return;
            shownLamp = state;
            block ??= new MaterialPropertyBlock();
            lamp.GetPropertyBlock(block);
            Color c = state == 0 ? new Color(.15f, .1f, .05f) : state == 1 ? new Color(1f, .55f, .1f) : new Color(1f, .9f, .4f);
            block.SetColor("_BaseColor", c);
            block.SetColor("_EmissionColor", state == 0 ? Color.black : c * 3f);
            lamp.SetPropertyBlock(block);
        }

#if UNITY_EDITOR
        public void EditorSetup(MachineHazardKind hazardKind, Bounds dangerZone, float cyclePhase, Transform ram, GameObject steam,
            Renderer warningLamp, MovingPlatform platform)
        {
            kind = hazardKind;
            zone = dangerZone;
            phase = cyclePhase;
            mover = ram;
            plume = steam;
            lamp = warningLamp;
            belt = platform;
        }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = running ? new Color(1f, .25f, .1f, .8f) : new Color(.6f, .6f, .6f, .35f);
            Gizmos.DrawWireCube(zone.center, zone.size);
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || Live.Count == 0 || Live[0] != this) return;
            int on = 0;
            foreach (MachineHazard m in Live) if (m != null && m.running) on++;
            GUI.Label(new Rect(Screen.width - 380, 254, 375, 22), $"MACHINES running {on}/{Live.Count} (breakers start their sector)");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
