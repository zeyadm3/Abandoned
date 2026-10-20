using System.Collections.Generic;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The building gets worse the longer the crew stays (user, 2026-10-08; <see cref="BuildingDecayConfig"/>).
    /// Host: on the config's shrinking interval a section is cracked, groans for a few seconds, then fails and
    /// falls through the normal structure path (synced, cascades, NavMesh carving); sections near the crew are
    /// preferred. At the last minute the total collapse starts: everything that can fall comes down top floor
    /// first, and whoever is still inside when the countdown ends dies (if that is everyone, the job is lost).
    /// Every machine: the alarm, rumble and countdown on the radio. Added beside the level's DangerDirector.
    /// </summary>
    public class BuildingDecay : MonoBehaviour
    {
        private const float RumbleInterval = 2.5f, RadioInterval = 5f, WarningLead = 120f;

        [SerializeField] private StructureSimulation structure;

        private readonly List<(StructuralSection section, float failAt)> pending = new();
        private readonly List<StructuralSection> candidates = new(), near = new();
        private readonly List<Vector3> crew = new();
        private List<StructuralSection> totalOrder;
        private System.Random random = new(1);
        private RunState lastRun;
        private float nextFall, totalStartedAt, nextRumble, nextRadio, interval;
        private int totalIndex;
        private bool crushed, shownTotal, warned;

        public static BuildingDecay Current { get; private set; }
        public StructureSimulation Structure => structure;

        public void Setup(StructureSimulation simulation) => structure = simulation;

        private void OnEnable() => Current = this;
        private void OnDisable() { if (Current == this) Current = null; }

        private void Update()
        {
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return;
            if (run != lastRun) NewRun(run);
            if (structure == null) structure = FindAnyObjectByType<StructureSimulation>();
            Cues(run);
            if (!run.IsServer || structure == null || !structure.HasAuthority) return;
            if (run.State.Phase != RunPhase.Running && run.State.Phase != RunPhase.Honking) return;
            // Training: the practice house never decays and never comes down on the crew.
            if (Tutorial.TutorialMode.Active) return;

            BuildingDecayConfig config = BuildingDecayConfig.Current;
            TickPending();
            if (!run.TotalCollapse && run.Elapsed >= config.TotalCollapseAt) BeginTotal(run, config);
            if (run.TotalCollapse)
            {
                TickTotal(run, config);
                return;
            }
            interval = config.IntervalAt(run.Elapsed);
            if (interval <= 0f) return;
            // The first failure comes as a stage begins; a shorter stage pulls the next one in.
            nextFall = nextFall <= 0f ? Time.time : Mathf.Min(nextFall, Time.time + interval);
            if (Time.time < nextFall) return;
            nextFall = Time.time + interval;
            PickAndCrack(config);
        }

        private void NewRun(RunState run)
        {
            lastRun = run;
            pending.Clear();
            totalOrder = null;
            totalIndex = 0;
            nextFall = 0f;
            crushed = shownTotal = warned = false;
            random = new System.Random(run.State.Seed ^ 0x2C011A);
        }

        // ---- Host: the timed failures ----

        private void PickAndCrack(BuildingDecayConfig config)
        {
            candidates.Clear();
            near.Clear();
            CrewPositions();
            foreach (StructuralSection s in structure.Sections)
            {
                // A support jack under it keeps the slow decay off (the total collapse still takes it).
                if (!structure.CanDecay(s) || s.Reinforcement > 1f || IsPending(s)) continue;
                candidates.Add(s);
                if (NearCrew(s.SurfaceBounds.center, config.NearCrewRadius)) near.Add(s);
            }
            if (candidates.Count == 0) return;
            List<StructuralSection> pool = near.Count > 0 && random.NextDouble() < config.NearCrewChance ? near : candidates;
            StructuralSection pick = pool[random.Next(pool.Count)];
            if (!structure.Weaken(pick, config.CrackedHealth)) return;
            pending.Add((pick, Time.time + config.WarningSeconds));
            Debug.Log($"[Decay] {pick.name} is giving way ({interval:0} s interval).");
        }

        private void TickPending()
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                (StructuralSection s, float failAt) = pending[i];
                if (s == null || s.IsCollapsed || s.Stage == StructuralStage.Failing) { pending.RemoveAt(i); continue; }
                if (Time.time < failAt) continue;
                pending.RemoveAt(i);
                structure.StartFailing(s);
            }
        }

        private bool IsPending(StructuralSection s)
        {
            foreach (var (section, _) in pending) if (section == s) return true;
            return false;
        }

        private void CrewPositions()
        {
            crew.Clear();
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned && !p.IsDead) crew.Add(p.transform.position);
        }

        private bool NearCrew(Vector3 at, float radius)
        {
            foreach (Vector3 p in crew) if ((p - at).sqrMagnitude <= radius * radius) return true;
            return false;
        }

        // ---- Host: the total collapse ----

        private void BeginTotal(RunState run, BuildingDecayConfig config)
        {
            run.ServerStartTotalCollapse(config.TotalCollapseSeconds);
            totalStartedAt = Time.time;
            totalIndex = 0;
            Vector3 truck = TruckCargo.Current != null ? TruckCargo.Current.transform.position : Vector3.zero;
            // Top floor first, and on each floor the far side before the side the truck waits on.
            totalOrder = structure.Sections.Where(s => s != null && s.CanCollapse && !s.IsCollapsed)
                .OrderByDescending(s => Mathf.Round(s.transform.position.y))
                .ThenByDescending(s => Vector3.Distance(Flat(s.transform.position), Flat(truck)))
                .ThenBy(s => s.Id).ToList();
        }

        private void TickTotal(RunState run, BuildingDecayConfig config)
        {
            if (totalOrder == null) BeginTotal(run, config); // the host took over mid-collapse (never in practice)
            float spread = Mathf.Max(1f, config.TotalCollapseSeconds * config.TotalCollapseSpread);
            int due = Mathf.CeilToInt(Mathf.Clamp01((Time.time - totalStartedAt) / spread) * totalOrder.Count);
            while (totalIndex < due)
            {
                StructuralSection s = totalOrder[totalIndex++];
                if (structure.CanDecay(s)) structure.StartFailing(s);
            }
            if (!crushed && run.CollapseRemaining <= 0f) Crush(run, config);
        }

        private void Crush(RunState run, BuildingDecayConfig config)
        {
            crushed = true;
            Bounds fallback = BuildingBounds();
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead || p.NetworkManager != run.NetworkManager) continue;
                Vector3 at = p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;
                // A vent crawl parks the player far under the level (NetworkPlayer.Vents): the ducts are inside.
                bool inside = p.InVent || (RunHorrorDirector.Current != null ? RunHorrorDirector.Current.Inside(at)
                    : fallback.Contains(at) && (TruckCargo.Current == null || !TruckCargo.Current.Carries(at)));
                if (!inside) continue;
                // Marked first: if that was everyone, the departure reads it as a lost job.
                run.ServerMarkCrushed();
                p.ServerKill(config.CrushedLine);
                Debug.Log($"[Decay] {p.DisplayName} was inside when the building came down.");
            }
        }

        private Bounds BuildingBounds()
        {
            var bounds = new Bounds();
            bool first = true;
            foreach (StructuralSection s in structure.Sections)
            {
                if (s == null) continue;
                if (first) { bounds = new Bounds(s.transform.position, Vector3.one * 4f); first = false; }
                else bounds.Encapsulate(s.transform.position);
            }
            bounds.Expand(new Vector3(2f, 6f, 2f));
            return bounds;
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        // ---- Every machine: what the crew hears and reads ----

        private void Cues(RunState run)
        {
            if (run.State.Phase == RunPhase.Departed) return;
            BuildingDecayConfig config = BuildingDecayConfig.Current;
            // The run's start in server time, so every machine warns at the same moment.
            double started = run.State.WindowEnd - run.State.Window;
            float elapsed = (float)(run.Now - started);
            if (!warned && !run.TotalCollapse && elapsed >= config.TotalCollapseAt - WarningLead && elapsed < config.TotalCollapseAt)
            {
                warned = true;
                RunHorrorDirector.Say(Loc.Mark("DISPATCH: That building won't last two more minutes. Get out."), 8f);
            }
            if (!run.TotalCollapse) return;
            Vector3 ear = Ear();
            if (!shownTotal)
            {
                shownTotal = true;
                nextRadio = 0f;
                GameAudio.PlayUi(SoundId.Siren, 0.8f);
                GameAudio.Play(SoundId.DeepRumble, ear + Vector3.up * 6f, 1f);
                LightFixture.Disturb(config.TotalCollapseSeconds);
            }
            float remaining = run.CollapseRemaining;
            if (Time.time >= nextRadio && remaining > 0f)
            {
                nextRadio = Time.time + RadioInterval;
                RunHorrorDirector.Say(Loc.Wire("STRUCTURE FAILING / GET OUT NOW / {0} s", Mathf.CeilToInt(remaining)), RadioInterval + 0.5f);
                GameAudio.PlayUi(SoundId.Siren, 0.6f);
            }
            if (Time.time >= nextRumble && remaining > 0f)
            {
                nextRumble = Time.time + RumbleInterval;
                // The whole frame working loose: louder as the minute runs out.
                GameAudio.Play(SoundId.DeepRumble, ear + Vector3.up * 3f, Mathf.Lerp(0.5f, 1f, 1f - remaining / config.TotalCollapseSeconds));
                CameraShake.EmitCollapse(ear + Vector3.up * 3f, Mathf.Lerp(1500f, 4500f, 1f - remaining / config.TotalCollapseSeconds));
            }
        }

        private static Vector3 Ear()
        {
            AudioListener ear = VoiceListener.Current;
            return ear != null ? ear.transform.position : Vector3.zero;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            RunState run = RunState.Current;
            if (!DebugView.Visible || run == null || !run.IsSpawned) return;
            string text;
            if (run.TotalCollapse) text = $"TOTAL COLLAPSE {run.CollapseRemaining:0}s  sections {totalIndex}/{totalOrder?.Count ?? 0}";
            else if (!run.IsServer) text = "DECAY (host runs it)";
            else
            {
                BuildingDecayConfig config = BuildingDecayConfig.Current;
                int down = structure != null ? structure.Sections.Count(s => s != null && s.IsCollapsed) : 0;
                text = interval <= 0f ? $"DECAY starts at {config.Stages.FirstOrDefault().FromMinute:0}:00  run {run.Elapsed / 60f:0.0} min"
                    : $"DECAY every {interval:0}s  next {Mathf.Max(0f, nextFall - Time.time):0}s  cracking {pending.Count}  down {down}  total at {config.TotalCollapseMinute:0}:00";
            }
            GUI.Label(new Rect(Screen.width - 360f, 344f, 350f, 22f), text);
        }
#endif

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = new Color(1f, 0.2f, 0.1f);
            foreach (var (s, _) in pending)
                if (s != null) Gizmos.DrawWireCube(s.SurfaceBounds.center, s.SurfaceBounds.size + Vector3.one * 0.2f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
