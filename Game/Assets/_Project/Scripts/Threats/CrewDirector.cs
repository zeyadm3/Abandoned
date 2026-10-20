using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Host, beside the <see cref="ThreatDirector"/> (0.24.10, after Left 4 Dead's AI Director): each living
    /// player carries a hidden stress number that rises when they are hurt, knocked down, chased, near a failing
    /// floor or near a crewmate's death. The highest one drives <see cref="CrewDirectorCycle"/>; while it is
    /// <see cref="Quiet"/>, no new monster arrives and <see cref="ThreatPacing"/> sends idle ones away from the
    /// crew. It changes when monsters come, never how hard they hit: the opening wave, the danger level, decay,
    /// the total collapse and the Last Hunter are untouched. Nothing is networked.
    /// </summary>
    public class CrewDirector : MonoBehaviour
    {
        private const float TickInterval = 0.25f;

        private readonly Dictionary<NetworkPlayer, IntensityTrack> tracks = new();
        private readonly Dictionary<NetworkPlayer, bool> wasDown = new();
        private readonly List<NetworkPlayer> stale = new();
        private StructureSimulation structure;
        private RunState run;
        private float nextTick;
        private bool lookedForStructure;

        public static CrewDirector Current { get; private set; }
        public CrewDirectorCycle Cycle { get; private set; } = new(0);
        public float Highest { get; private set; }
        public IReadOnlyDictionary<NetworkPlayer, IntensityTrack> Tracks => tracks;
        private static CrewDirectorConfig Config => CrewDirectorConfig.Current;

        /// <summary>Fade or relax on the host: hold new arrivals, keep idle monsters away.</summary>
        public static bool Quiet => Current != null && Config.Enabled && GameAuthority.IsHost && Current.Cycle.Quiet;

        private void OnEnable()
        {
            Current = this;
            RunDirector.RunStarted += OnRunStarted;
            NetworkPlayer.HealthChanged += OnHealthChanged;
            NetworkPlayer.DeathChanged += OnDeathChanged;
        }

        private void OnDisable()
        {
            if (Current == this) Current = null;
            RunDirector.RunStarted -= OnRunStarted;
            NetworkPlayer.HealthChanged -= OnHealthChanged;
            NetworkPlayer.DeathChanged -= OnDeathChanged;
            Watch(null);
        }

        private void OnRunStarted(int seed) => Restart(seed);

        private void Restart(int seed)
        {
            Cycle = new CrewDirectorCycle(seed);
            Highest = 0f;
            lookedForStructure = false;
            tracks.Clear();
            wasDown.Clear();
        }

        private void Update()
        {
            if (!GameAuthority.IsHost || Time.time < nextTick || Tutorial.TutorialMode.Active) return;
            nextTick = Time.time + TickInterval;
            RunState current = RunState.Current;
            if (current == null || !current.IsSpawned) return;
            if (current != run) { run = current; Restart(current.State.Seed); }
            if (current.State.Phase != RunPhase.Running) return;
            // Once a job: a level without a structure must not search the scene four times a second.
            if (!lookedForStructure) { lookedForStructure = true; if (structure == null) Watch(FindStructure()); }

            float now = Time.time, highest = 0f;
            stale.Clear();
            foreach (NetworkPlayer p in tracks.Keys) if (p == null || !p.IsSpawned) stale.Add(p);
            foreach (NetworkPlayer p in stale) { tracks.Remove(p); wasDown.Remove(p); }
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead) continue;
                IntensityTrack track = Track(p);
                bool down = p.State.Ragdolled;
                if (down && !(wasDown.TryGetValue(p, out bool before) && before)) track.Add(Config.KnockedDown, now);
                wasDown[p] = down;
                track.Tick(TickInterval, Chased(p), now, Config);
                highest = Mathf.Max(highest, track.Value);
            }
            Highest = highest;
            CrewDirectorPhase was = Cycle.Phase;
            Cycle.Tick(TickInterval, highest, Config);
            if (Cycle.Phase != was && Config.Enabled) Debug.Log($"[CrewDirector] {was} -> {Cycle.Phase} at {current.Elapsed:0}s (stress {highest:0}).");
        }

        private IntensityTrack Track(NetworkPlayer p)
        {
            if (!tracks.TryGetValue(p, out IntensityTrack track)) tracks[p] = track = new IntensityTrack();
            return track;
        }

        private bool Chased(NetworkPlayer p)
        {
            float range = Config.ChaseRange;
            foreach (Threat t in Threat.All)
            {
                // The Collector "chases" only when running off with loot: away from the crew, not after it.
                if (t == null || !t.IsSpawned || t is Collector) continue;
                ThreatMotion motion = t.DesiredMotion;
                if (motion != ThreatMotion.Chase && motion != ThreatMotion.Attack) continue;
                Vector3 offset = t.transform.position - p.transform.position;
                if (Mathf.Abs(offset.y) < 3f && offset.sqrMagnitude <= range * range) return true;
            }
            return false;
        }

        private void OnHealthChanged(NetworkPlayer p, float before, float after)
        {
            if (!GameAuthority.IsHost || p == null || p.IsDead || after >= before) return;
            Track(p).Add((before - after) * Config.PerHealthLost, Time.time);
        }

        private void OnDeathChanged(NetworkPlayer who, bool dead)
        {
            if (!GameAuthority.IsHost || !dead || who == null) return;
            if (tracks.TryGetValue(who, out IntensityTrack own)) own.Reset();
            Nearby(who.transform.position, Config.CrewmateRange, Config.CrewmateDied);
        }

        private void Nearby(Vector3 at, float range, float amount)
        {
            float now = Time.time;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead) continue;
                if ((p.transform.position - at).sqrMagnitude <= range * range) Track(p).Add(amount, now);
            }
        }

        // ---- The building ----

        private StructureSimulation FindStructure()
        {
            foreach (StructureSimulation s in FindObjectsByType<StructureSimulation>(FindObjectsSortMode.None))
                if (s.gameObject.scene == gameObject.scene) return s;
            return null;
        }

        private void Watch(StructureSimulation simulation)
        {
            if (structure != null)
                foreach (StructuralSection s in structure.Sections)
                    if (s != null) s.StageChanged -= OnStageChanged;
            structure = simulation;
            if (structure == null) return;
            foreach (StructuralSection s in structure.Sections)
                if (s != null) s.StageChanged += OnStageChanged;
        }

        // The event passes the stage before. Only a worsening from Cracking up counts: Stressed comes and goes with
        // the load on it (stepping on and off would pump stress), and a re-roll restoring Stable is not a scare.
        private void OnStageChanged(StructuralSection section, StructuralStage previous)
        {
            if (!GameAuthority.IsHost || section == null) return;
            StructuralStage stage = section.Stage;
            if (stage <= previous || stage < StructuralStage.Cracking) return;
            float amount = stage == StructuralStage.Collapsed ? Config.SectionCollapsed : Config.SectionCracked;
            Nearby(section.transform.position, Config.StructureRange, amount);
        }
    }
}
