using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using Abandoned.Threats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The building's mood during a job, on the run state's object. Host: the arrival beats when the crew
    /// first walks in, pressure sounds near a random player, scares (an apparition, from a danger level on a
    /// shutter slam that can never trap anyone) and the final phase after the window closes. Every machine:
    /// lights lose power sector by sector as danger rises (some always stay lit), the dispatch radio lines,
    /// and the lockdown sirens. All places and timings come from the level's <see cref="HorrorConfig"/>.
    /// </summary>
    public class RunHorrorDirector : NetworkBehaviour
    {
        private const string AlarmPrefix = "HorrorAlarm";

        [SerializeField] private HorrorConfig config;

        private readonly NetworkVariable<double> enteredAt = new(-1);
        private readonly NetworkVariable<bool> final = new();
        private readonly List<NetworkPlayer> inside = new();
        private RunState run;
        private int arrivalBeat, shownDanger = -1, shownLightsDanger = -1;
        private bool shownLightsFinal, arrivalWaveDone;
        private float nextPressure, nextScare, nextSiren;
        private bool announced, arrivalPlayed, departurePlayed;
        private System.Random random;
        private LightFixture[] fixtures;
        private int[] fixtureSectors;
        private Light[] alarms;

        public static RunHorrorDirector Current { get; private set; }
        public static string RadioLine { get; private set; }
        public static float RadioUntil { get; private set; }
        public bool FinalPhase => final.Value;

        public bool Inside(Vector3 at) =>
            config != null && config.Building.Contains(at) && (TruckCargo.Current == null || !TruckCargo.Current.Carries(at));

        public override void OnNetworkSpawn()
        {
            if (config == null || !config.RunsIn(gameObject.scene.name) && !config.RunsIn(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            {
                enabled = false;
                return;
            }
            Current = this;
            run = GetComponent<RunState>();
            random = new System.Random(run.State.Seed ^ 713);
            fixtures = FindObjectsByType<LightFixture>(FindObjectsSortMode.None);
            fixtureSectors = new int[fixtures.Length];
            for (int i = 0; i < fixtures.Length; i++) fixtureSectors[i] = fixtures[i] != null ? config.SectorOf(fixtures[i].transform.position) : 0;
            // Once per run, lights only (not every object in the scene).
            var found = new List<Light>();
            foreach (Light l in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (l != null && l.name.StartsWith(AlarmPrefix)) found.Add(l);
            alarms = found.ToArray();
            nextPressure = Time.time + 25f;
            nextScare = Time.time + config.ScareInterval;
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
            if (fixtures != null)
                foreach (LightFixture f in fixtures)
                    if (f != null)
                    {
                        f.SetHorrorPower(true);
                        f.SetAlarm(false);
                    }
            if (alarms != null) foreach (Light l in alarms) if (l != null) l.enabled = false;
            RadioLine = null;
        }

        private void Update()
        {
            if (!IsSpawned || run == null || config == null) return;
            if (!arrivalPlayed && run.State.Phase == RunPhase.Running)
            {
                arrivalPlayed = true;
                HorrorAudio.Play(HorrorAudio.Cue.EngineStop, TruckCargo.Current != null ? TruckCargo.Current.transform.position : Vector3.zero, 0.7f);
                Say("DISPATCH: Don't stay past dark. Last crew didn't.", 8f);
            }
            if (run.State.Phase != RunPhase.Running && run.State.Phase != RunPhase.Honking) return;
            if (IsServer) HostPressure();
            Lights();
            if (final.Value && !announced)
            {
                announced = true;
                Say("LOCKDOWN / IT IS HERE / GET TO THE TRUCK", 12f);
                HorrorAudio.Play(HorrorAudio.Cue.Roar, config.RoarPoint, 1f, false);
                nextSiren = 0f;
                LightFixture.Disturb(8f);
            }
            if (final.Value && Time.time >= nextSiren)
            {
                nextSiren = Time.time + 9f;
                HorrorAudio.Play(HorrorAudio.Cue.Siren, Vector3.zero, 0.7f, false);
            }
            if (run.State.Phase == RunPhase.Running) departurePlayed = false; // the lever can call a departure off
            if (run.State.Phase == RunPhase.Honking && !departurePlayed && TruckCargo.Current != null)
            {
                departurePlayed = true;
                Vector3 truck = TruckCargo.Current.transform.position;
                HorrorAudio.Play(HorrorAudio.Cue.Door, truck, 1f);
                HorrorAudio.Play(HorrorAudio.Cue.EngineEscape, truck, 0.9f);
                Say("DOORS SHUT. HOLD ON.", 4f);
            }
        }

        // ---- Host ----

        private void HostPressure()
        {
            inside.Clear();
            int crew = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager) continue;
                crew++;
                if (!p.IsDead && Inside(p.transform.position)) inside.Add(p);
            }
            if (enteredAt.Value < 0 && inside.Count > 0) enteredAt.Value = run.Now;
            ArrivalBeats();

            // A lone player gets longer before the final hunt (QA D-03).
            float finalDelay = config.FinalDelay + (crew <= 1 ? config.SoloFinalDelayBonus : 0f);
            if (!final.Value && run.Now >= run.State.WindowEnd + finalDelay)
            {
                final.Value = true;
                ThreatDirector.Current?.SpawnFinalHunter();
            }
            if (inside.Count == 0) return;
            int danger = run.State.Danger;
            if (Time.time >= nextPressure) Pressure(danger);
            if (Time.time >= nextScare) ScareSomeone(danger);
        }

        private void ArrivalBeats()
        {
            if (enteredAt.Value < 0) return;
            float[] beats = config.ArrivalBeats;
            double age = run.Now - enteredAt.Value;
            if (beats == null || arrivalBeat >= beats.Length || age < beats[arrivalBeat]) return;
            Vector3 at = inside.Count > 0 ? inside[0].transform.position : config.ArrivalFallback;
            EventRpc(arrivalBeat, at, run.State.Seed + arrivalBeat);
            arrivalBeat++;
        }

        // A sound close to a random player inside; closer as the danger rises.
        private void Pressure(int danger)
        {
            nextPressure = Time.time + Mathf.Max(6f, config.PressureInterval - danger * 2.5f);
            NetworkPlayer p = inside[random.Next(inside.Count)];
            float distance = Mathf.Lerp(18f, 3f, danger / 6f);
            var direction = new Vector3((float)random.NextDouble() * 2f - 1f, 0f, (float)random.NextDouble() * 2f - 1f);
            Vector3 at = ClampToBuilding(p.transform.position + direction.normalized * distance, 1f);
            EventRpc(5 + random.Next(3), at + Vector3.up * (2 + random.Next(2)), random.Next());
        }

        private void ScareSomeone(int danger)
        {
            nextScare = Time.time + config.ScareInterval + random.Next(45);
            NetworkPlayer p = inside[random.Next(inside.Count)];
            Vector3 at = ClampToBuilding(p.transform.position + p.transform.forward * 9f, 2f);
            if (NavMesh.SamplePosition(at, out NavMeshHit floor, 2.5f, NavMesh.AllAreas))
                EventRpc(8 + random.Next(3), floor.position + Vector3.up * 0.04f, random.Next());
            if (danger < config.SlamFromDanger || RunShutters.Current == null) return;
            var open = new List<RollerShutter>();
            foreach (RollerShutter s in RollerShutter.All) if (s != null && !s.Entrance && !s.IsDown) open.Add(s);
            // RunShutters refuses a slam that could trap anyone.
            if (open.Count > 0) RunShutters.Current.ServerSlam(open[random.Next(open.Count)].Index);
        }

        private Vector3 ClampToBuilding(Vector3 at, float margin)
        {
            Bounds b = config.Building;
            at.x = Mathf.Clamp(at.x, b.min.x + margin, b.max.x - margin);
            at.z = Mathf.Clamp(at.z, b.min.z + margin, b.max.z - margin);
            return at;
        }

        // ---- Every machine ----

        private void Lights()
        {
            int danger = run.State.Danger;
            double arrival = enteredAt.Value >= 0 ? run.Now - enteredAt.Value : -1;
            Vector2 wave = config.ArrivalBlackout;
            bool inWave = arrival >= wave.x && arrival < wave.y;
            // QA P-03: only touch every fixture when something changed (or the arrival wave is rolling).
            if (inWave || (arrival >= wave.y && !arrivalWaveDone) || danger != shownLightsDanger || final.Value != shownLightsFinal)
            {
                if (arrival >= wave.y) arrivalWaveDone = true;
                shownLightsDanger = danger;
                shownLightsFinal = final.Value;
                for (int i = 0; i < fixtures.Length; i++)
                {
                    LightFixture f = fixtures[i];
                    if (f == null) continue;
                    bool power = config.SectorPowered(fixtureSectors[i], danger);
                    if (inWave) power &= Mathf.Abs((float)arrival - wave.x - f.transform.position.z / config.ArrivalBlackoutSpeed) >= 1.2f;
                    f.SetHorrorPower(power);
                    f.SetAlarm(final.Value);
                }
                foreach (Light l in alarms) if (l != null) l.enabled = final.Value;
            }
            if (shownDanger != danger)
            {
                shownDanger = danger;
                if (danger > 0) Say(danger >= 5 ? "SIGNAL FAILING / LEAVE" : "BUILDING STATUS " + new string('|', danger) + " / deteriorating", 4f);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void EventRpc(int kind, Vector3 at, int seed)
        {
            switch (kind)
            {
                case 0: GameAudio.Play(SoundId.Groan, at + Vector3.up * 6f, 1f); LightFixture.Disturb(3f); break;
                case 1: GameAudio.Play(SoundId.RadioStatic, at, 0.35f); break;
                case 2: HorrorAudio.Play(HorrorAudio.Cue.Scream, at + Vector3.forward * 22f + Vector3.up * 5f, 0.7f); break;
                case 3: Scare(0, config.ArrivalFigure, seed); HorrorAudio.Play(HorrorAudio.Cue.Door, config.ArrivalDoor, 0.8f); break;
                case 4: HorrorAudio.Play(HorrorAudio.Cue.Ceiling, at + Vector3.up * 4f, 0.7f); break;
                case 5: HorrorAudio.Play(HorrorAudio.Cue.Whisper, at, 0.45f); break;
                case 6: HorrorAudio.Play(HorrorAudio.Cue.Knock, at, 0.6f); break;
                case 7: HorrorAudio.Play(HorrorAudio.Cue.Ceiling, at, 0.65f); break;
                default: Scare(kind - 8, at + (kind == 9 ? Vector3.up * 2f : Vector3.zero), seed); HorrorAudio.Play(HorrorAudio.Cue.Door, at, 0.6f); break;
            }
        }

        private void Scare(int kind, Vector3 at, int seed)
        {
            // The door bang still plays; only the figure is skipped for players who asked for fewer scares.
            if (config.ScareModel == null || GameSettings.FewerJumpScares) return;
            GameObject shape = Instantiate(config.ScareModel, at, Quaternion.Euler(0f, seed % 360, kind == 1 ? 180f : 0f));
            foreach (Collider c in shape.GetComponentsInChildren<Collider>()) Destroy(c);
            shape.AddComponent<HorrorScare>().Setup(kind);
        }

        private static void Say(string text, float seconds)
        {
            RadioLine = text;
            RadioUntil = Time.time + seconds;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (DebugView.Visible)
                GUI.Label(new Rect(15f, 190f, 650f, 25f), $"HORROR arrival {arrivalBeat}/{config.ArrivalBeats?.Length ?? 0} final {final.Value} next scare {nextScare - Time.time:0}s");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            RadioLine = null;
            RadioUntil = 0f;
        }
    }
}
