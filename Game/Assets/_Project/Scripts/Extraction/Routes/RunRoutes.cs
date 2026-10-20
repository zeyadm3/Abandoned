using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Seeded route changes (0.16.2), on the run state's object. Host, a moment into each job: the run seed
    /// picks a few collapsible floor sections (and sometimes an escalator) and sends them down through the
    /// ordinary structure path, so the sync, rubble mounds, NavMesh carving and cascades are the usual ones
    /// and clients need nothing new. Stairs, protected routes and the ground floor can't collapse, so every
    /// store and the truck stay reachable.
    /// </summary>
    public class RunRoutes : NetworkBehaviour
    {
        private float rollAt = -1f;
        private bool rolled;

        public static RunRoutes Current { get; private set; }
        public IReadOnlyList<string> Fallen => fallen;
        private readonly List<string> fallen = new();

        public override void OnNetworkSpawn()
        {
            Current = this;
            if (IsServer) rollAt = Time.time + RouteConfig.Current.Delay;
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            // Training: the house's floors only fall when a lesson says so.
            if (!IsServer || rolled || rollAt < 0f || Time.time < rollAt || Tutorial.TutorialMode.Active) return;
            RunState run = GetComponent<RunState>();
            StructureSimulation structure = BuildingDecay.Current != null ? BuildingDecay.Current.Structure : null;
            if (run == null || structure == null || run.State.Phase != RunPhase.Running) return;
            rolled = true;
            Roll(structure, run.State.Seed);
        }

        private void Roll(StructureSimulation structure, int seed)
        {
            RouteConfig config = RouteConfig.Current;
            var random = new System.Random(seed ^ 0x0d0162);
            // Sorted by name: the same seed picks the same sections whatever order the scene loaded them in.
            List<StructuralSection> open = structure.Sections.Where(s => s != null && structure.CanDecay(s)).OrderBy(s => s.name).ToList();
            var escalators = open.Where(s => s.name.StartsWith(config.EscalatorPrefix)).ToList();
            open.RemoveAll(s => s.name.StartsWith(config.EscalatorPrefix));
            var picked = new List<StructuralSection>();
            if (escalators.Count > 1 && random.NextDouble() < config.EscalatorChance)
                picked.Add(escalators[random.Next(escalators.Count)]); // never both: one way up stays
            int count = random.Next(config.Fallen.x, config.Fallen.y + 1);
            for (int tries = 0; picked.Count < count + (picked.Count > 0 && escalators.Contains(picked[0]) ? 1 : 0) && tries < 200 && open.Count > 0; tries++)
            {
                StructuralSection s = open[random.Next(open.Count)];
                open.Remove(s);
                if (s.Reinforcement > 1f || picked.Any(p => Vector3.Distance(p.transform.position, s.transform.position) < config.Apart)) continue;
                picked.Add(s);
            }
            foreach (StructuralSection s in picked)
                if (structure.StartFailing(s)) fallen.Add(s.name);
            Debug.Log($"[Run] Route changes: {string.Join(", ", fallen)} start the job fallen.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
