using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Host-side structural simulation for one level: solves the logical load on every section each
    /// physics step, ticks health and stages, applies stability scaling and seeded pre-damage, and
    /// handles collapse consequences (cascades onto lower sections, waking what was resting on top).
    /// Clients only ever see the resulting section state.
    /// </summary>
    public class StructureSimulation : MonoBehaviour
    {
        [SerializeField] private StructureConfig config;
        [Tooltip("Contract Structural Stability (GDD 6.2). Lower = weaker, faster decay, more pre-damage.")]
        [SerializeField, Range(0f, 1f)] private float stability = 1f;
        [SerializeField] private int seed = 12345;

        private const float RayLift = 0.15f;

        private readonly List<StructuralSection> sections = new();
        private readonly Dictionary<StructuralSection, float> loads = new();
        private readonly List<LoadPoint> points = new();
        private readonly List<StructuralSection> supports = new();
        private readonly Collider[] overlap = new Collider[64];
        // Impacts reported this physics step, per hitting body: one landing is split across every
        // section it touched instead of hitting each of them in full.
        private readonly Dictionary<Rigidbody, (float momentum, List<StructuralSection> sections)> pendingImpacts = new();
        private readonly List<Rigidbody> pendingOrder = new();

        public IReadOnlyList<StructuralSection> Sections => sections;
        public StructureConfig Config => config;
        public float Stability => stability;
        public int Seed => seed;
        public int CollapseCount { get; private set; }

        private int StructureMask => 1 << Mathf.Max(0, GameLayers.StructureLayer);

        private void Awake()
        {
            // Stable ids (hierarchy order by name, then position) so "section X collapsed" means the same
            // piece on every machine and in every run.
            sections.AddRange(FindObjectsByType<StructuralSection>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(s => s.name, System.StringComparer.Ordinal).ThenBy(s => s.transform.position.x).ThenBy(s => s.transform.position.z));
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].Id = i;
                sections[i].Simulation = this;
                sections[i].Collapsed += OnSectionCollapsed;
            }
            StructureSignals.Impact += OnPointImpact;
        }

        private void OnDestroy()
        {
            StructureSignals.Impact -= OnPointImpact;
            foreach (StructuralSection s in sections) if (s != null) s.Collapsed -= OnSectionCollapsed;
        }

        private void Start()
        {
            if (GameAuthority.IsHost) ApplyStability(stability, seed);
        }

        /// <summary>Host: rescales every section for a stability, resets them and re-rolls pre-damage.</summary>
        public void ApplyStability(float newStability, int newSeed)
        {
            stability = Mathf.Clamp01(newStability);
            seed = newSeed;
            float capacityScale = config.CapacityScale(stability);
            float decayScale = config.DecayScale(stability);
            foreach (StructuralSection s in sections)
            {
                s.Configure(capacityScale, decayScale, seed);
                s.ResetState();
            }

            bool[] eligible = sections.Select(s => s.CanCollapse).ToArray();
            float[] plan = StructureMath.PreDamagePlan(eligible, stability, seed, config);
            for (int i = 0; i < sections.Count; i++)
                if (plan[i] > 0f) sections[i].PreDamage(plan[i]);
        }

        private void FixedUpdate()
        {
            if (GameAuthority.IsHost) Step(Time.fixedDeltaTime);
        }

        /// <summary>Host: one simulation step. Public so tests can step deterministically.</summary>
        public void Step(float dt)
        {
            ApplyPendingImpacts();
            SolveLoads();
            foreach (StructuralSection s in sections)
            {
                s.SetLoad(loads.TryGetValue(s, out float kg) ? kg : 0f);
                s.Tick(dt);
            }
        }

        /// <summary>Logical load model: each source's weight split evenly over the sections under its points.</summary>
        public void SolveLoads()
        {
            loads.Clear();
            IReadOnlyList<ILoadSource> all = LoadSources.All;
            for (int i = 0; i < all.Count; i++)
            {
                float weight = all[i].LoadWeight;
                if (weight <= 0f) continue;

                points.Clear();
                all[i].GetLoadPoints(points);
                supports.Clear();
                foreach (LoadPoint p in points)
                {
                    StructuralSection support = SectionBelow(p.Position + Vector3.up * RayLift, config.MaxSupportDistance + RayLift);
                    if (support != null) supports.Add(support);
                }
                if (supports.Count == 0) continue;

                float share = weight / supports.Count;
                foreach (StructuralSection s in supports)
                    loads[s] = (loads.TryGetValue(s, out float kg) ? kg : 0f) + share;
            }
        }

        /// <summary>Host: a rigidbody hit a section this step; applied (shared) at the start of the next step.</summary>
        public void ReportImpact(StructuralSection section, Rigidbody body, float momentum)
        {
            if (!pendingImpacts.TryGetValue(body, out var entry))
            {
                entry = (0f, new List<StructuralSection>());
                pendingOrder.Add(body);
            }
            if (!entry.sections.Contains(section)) entry.sections.Add(section);
            pendingImpacts[body] = (Mathf.Max(entry.momentum, momentum), entry.sections);
        }

        private void ApplyPendingImpacts()
        {
            foreach (Rigidbody body in pendingOrder)
            {
                (float momentum, List<StructuralSection> hit) = pendingImpacts[body];
                foreach (StructuralSection s in hit) s.ApplyImpact(momentum / hit.Count);
            }
            pendingImpacts.Clear();
            pendingOrder.Clear();
        }

        /// <summary>Host: a non-physics impact (a player landing) at a point; hits the section underneath.</summary>
        private void OnPointImpact(Vector3 position, float momentum)
        {
            if (!GameAuthority.IsHost) return;
            StructuralSection below = SectionBelow(position + Vector3.up * RayLift, RayLift + 0.5f);
            if (below != null) below.ApplyImpact(momentum);
        }

        public float LoadOn(StructuralSection section) => loads.TryGetValue(section, out float kg) ? kg : 0f;

        private StructuralSection SectionBelow(Vector3 from, float distance) =>
            Physics.Raycast(from, Vector3.down, out RaycastHit hit, distance, StructureMask, QueryTriggerInteraction.Ignore)
                ? hit.collider.GetComponentInParent<StructuralSection>()
                : null;

        private void OnSectionCollapsed(StructuralSection section)
        {
            CollapseCount++;
            Bounds surface = section.SurfaceBounds;

            // Whatever was resting on it must fall now, even if the physics engine had put it to sleep.
            Vector3 halfExtents = surface.extents + new Vector3(0.1f, 1.5f, 0.1f);
            int count = Physics.OverlapBoxNonAlloc(surface.center + Vector3.up * 1.5f, halfExtents, overlap,
                Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (overlap[i].attachedRigidbody != null) overlap[i].attachedRigidbody.WakeUp();

            NoiseSystem.Emit(surface.center, config.CollapseLoudness, NoiseSource.Collapse);
            StructureSignals.RaiseCollapsed(surface);
            Cascade(section, surface);
        }

        /// <summary>
        /// The host computes what a falling section does to the sections below it (debris is cosmetic
        /// and never deals damage): its mass falling the gap, split over everything it lands on.
        /// </summary>
        private void Cascade(StructuralSection fallen, Bounds surface)
        {
            var hits = new Dictionary<StructuralSection, float>();
            Vector3 c = surface.center, e = surface.extents * 0.8f;
            Vector3[] probes = { c, c + new Vector3(e.x, 0, e.z), c + new Vector3(-e.x, 0, e.z), c + new Vector3(e.x, 0, -e.z), c + new Vector3(-e.x, 0, -e.z) };
            foreach (Vector3 probe in probes)
            {
                Vector3 from = new(probe.x, surface.min.y - 0.05f, probe.z);
                if (!Physics.Raycast(from, Vector3.down, out RaycastHit hit, config.CascadeSearchDistance, StructureMask, QueryTriggerInteraction.Ignore))
                    continue;
                StructuralSection below = hit.collider.GetComponentInParent<StructuralSection>();
                if (below == null || below == fallen || below.IsCollapsed) continue;
                hits[below] = hits.TryGetValue(below, out float d) ? Mathf.Min(d, hit.distance) : hit.distance;
            }
            if (hits.Count == 0) return;

            float mass = config.Profile(fallen.Type).SelfMass * config.CascadeFactor / hits.Count;
            foreach (KeyValuePair<StructuralSection, float> h in hits.OrderBy(h => h.Key.Id))
                h.Key.ApplyImpact(mass * StructureMath.FallSpeed(h.Value));
        }
    }
}
