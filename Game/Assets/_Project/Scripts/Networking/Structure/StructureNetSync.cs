using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The building's structural state over the network: ONE NetworkObject per level holding a
    /// NetworkList with an entry per section (index = section id) plus building-wide globals. NGO
    /// sends only the entries that changed, so a ~300-section mall costs one ~6 KB initial sync and
    /// ~20 bytes per section change, instead of 300 NetworkObjects to spawn, id and track.
    /// The host's StructureSimulation runs everything (load, damage, pre-damage, timers, cascades) and
    /// this publishes the result each frame; clients turn their structure into a mirror and apply it.
    /// A collapse entry carries the seed, and applying it flips that machine's colliders off at once.
    /// </summary>
    [DisallowMultipleComponent]
    public class StructureNetSync : NetworkBehaviour
    {
        private static readonly List<StructureNetSync> Live = new();

        private readonly NetworkVariable<StructureNetGlobals> globals = new();
        private NetworkList<SectionNetState> states;

        private StructureSimulation simulation;
        // Host: server time each section entered Failing (< 0 = not failing), so its entry stays stable.
        private double[] failingSince;
        private bool dirty;
        private int appliedGeneration = int.MinValue;
        private bool layoutMismatch;

        public StructureSimulation Simulation => simulation;
        public int EntryCount => states.Count;
        public StructureNetGlobals Globals => globals.Value;
        /// <summary>Client: the building doesn't match the host's, so nothing is applied (logged once).</summary>
        public bool LayoutMismatch => layoutMismatch;

        public SectionNetState Entry(int sectionId) => states[sectionId];

        /// <summary>The sync spawned on <paramref name="manager"/>'s session, if any.</summary>
        public static StructureNetSync For(NetworkManager manager) =>
            Live.FirstOrDefault(s => s != null && s.IsSpawned && s.NetworkManager == manager);

        // NGO needs NetworkLists constructed before the behaviour is initialised.
        private void Awake() => states = new NetworkList<SectionNetState>();

        public override void OnNetworkSpawn()
        {
            Live.Add(this);
            simulation = StructureNetBinding.Resolve(NetworkManager);
            if (simulation == null)
            {
                Debug.LogWarning("[Net] StructureNetSync spawned but this machine has no StructureSimulation.", this);
                return;
            }
            if (IsServer)
            {
                PublishAll();
                return;
            }
            simulation.SetMirror(true);
            states.OnListChanged += OnStatesChanged;
            globals.OnValueChanged += OnGlobalsChanged;
            // Whatever is already down was down before we arrived: hide it, don't replay the break.
            Reconcile(quiet: true);
        }

        public override void OnNetworkDespawn()
        {
            Live.Remove(this);
            states.OnListChanged -= OnStatesChanged;
            globals.OnValueChanged -= OnGlobalsChanged;
            // Left the session: the building is this machine's own again (solo rules).
            if (!IsServer && simulation != null) simulation.SetMirror(false);
        }

        private void LateUpdate()
        {
            if (!IsSpawned || simulation == null) return;
            if (IsServer) Publish();
            else if (dirty) Reconcile(quiet: false);
            else TickFailingClocks();
        }

        // ---- Host ----

        private void PublishAll()
        {
            IReadOnlyList<StructuralSection> sections = simulation.Sections;
            failingSince = Enumerable.Repeat(-1d, sections.Count).ToArray();
            globals.Value = CurrentGlobals();
            states.Clear();
            for (int i = 0; i < sections.Count; i++) states.Add(EntryFor(i));
        }

        // Runs after the physics steps of this frame and before NGO sends its tick, so a collapse
        // goes out in the same frame the host's colliders went off.
        private void Publish()
        {
            if (globals.Value.Generation != simulation.Generation) globals.Value = CurrentGlobals();
            IReadOnlyList<StructuralSection> sections = simulation.Sections;
            for (int i = 0; i < sections.Count && i < states.Count; i++)
            {
                SectionNetState next = EntryFor(i);
                if (!next.Equals(states[i])) states[i] = next;
            }
        }

        private StructureNetGlobals CurrentGlobals() => new()
        {
            Stability = simulation.Stability,
            Seed = simulation.Seed,
            Generation = simulation.Generation,
            SectionCount = simulation.Sections.Count,
            LayoutHash = simulation.LayoutHash(),
        };

        private SectionNetState EntryFor(int id)
        {
            StructuralSection s = simulation.Sections[id];
            if (s.Stage != StructuralStage.Failing) failingSince[id] = -1d;
            else if (failingSince[id] < 0d) failingSince[id] = NetworkManager.ServerTime.Time - s.FailingTime;
            return SectionNetState.Of(s, failingSince[id], simulation.Generation);
        }

        // ---- Client ----

        private void OnStatesChanged(NetworkListEvent<SectionNetState> change) => dirty = true;

        private void OnGlobalsChanged(StructureNetGlobals previous, StructureNetGlobals next) => dirty = true;

        /// <summary>
        /// Applied after NGO has read the whole tick, so a re-roll (globals) and the entries it rewrote
        /// arrive together in whichever order: restore first, then take every entry of the new generation.
        /// </summary>
        private void Reconcile(bool quiet)
        {
            dirty = false;
            StructureNetGlobals g = globals.Value;
            if (!LayoutMatches(g)) return;
            if (g.Generation != appliedGeneration)
            {
                simulation.MirrorStability(g.Stability, g.Seed);
                appliedGeneration = g.Generation;
            }
            byte generation = (byte)g.Generation;
            IReadOnlyList<StructuralSection> sections = simulation.Sections;
            double now = NetworkManager.ServerTime.Time;
            float failing = simulation.Config.FailingDuration;
            for (int i = 0; i < sections.Count; i++)
            {
                SectionNetState e = states[i];
                if (e.Generation != generation) continue;
                sections[i].ApplyReplicated(e.StageValue, e.HealthFraction, e.LoadKg, e.FailingTimeAt(now, failing), e.Seed, quiet);
            }
        }

        // The sag and creaks of a Failing section follow the host's clock between updates.
        private void TickFailingClocks()
        {
            if (layoutMismatch) return;
            IReadOnlyList<StructuralSection> sections = simulation.Sections;
            double now = NetworkManager.ServerTime.Time;
            float failing = simulation.Config.FailingDuration;
            for (int i = 0; i < sections.Count && i < states.Count; i++)
            {
                if (sections[i].Stage != StructuralStage.Failing) continue;
                SectionNetState e = states[i];
                sections[i].ApplyReplicated(e.StageValue, e.HealthFraction, e.LoadKg, e.FailingTimeAt(now, failing), e.Seed, false);
            }
        }

        private bool LayoutMatches(StructureNetGlobals g)
        {
            bool matches = g.SectionCount == simulation.Sections.Count && g.SectionCount == states.Count &&
                           g.LayoutHash == simulation.LayoutHash();
            if (!matches && !layoutMismatch)
                Debug.LogError($"[Net] This building ({simulation.Sections.Count} sections) doesn't match the host's " +
                               $"({g.SectionCount}); structure sync is off. Are both on the same game version?", this);
            layoutMismatch = !matches;
            return matches;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
