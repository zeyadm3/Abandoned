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
    /// On a client it is a mirror (<see cref="SetMirror"/>): sections take the host's state and only
    /// the local consequences of a collapse (wake bodies, drop local players) run here.
    /// </summary>
    public partial class StructureSimulation : MonoBehaviour
    {
        [SerializeField] private StructureConfig config;
        [Tooltip("Contract Structural Stability (GDD 6.2). Lower = weaker, faster decay, more pre-damage.")]
        [SerializeField, Range(0f, 1f)] private float stability = 1f;
        [SerializeField] private int seed = 12345;
        [Tooltip("Only sections under this object (several independent structures in one scene, e.g. in-process network tests). Off = every section in the scene.")]
        [SerializeField] private bool childrenOnly;

        private const float RayLift = 0.15f;

        private readonly List<StructuralSection> sections = new();
        private readonly Dictionary<StructuralSection, float> loads = new();
        private readonly List<LoadPoint> points = new();
        private readonly List<StructuralSection> supports = new();
        private readonly SupportCache supportCache = new();
        private readonly Collider[] overlap = new Collider[64];
        private readonly RaycastHit[] rayHits = new RaycastHit[16];
        // Impacts reported this physics step, per hitting body: one landing is split across every
        // section it touched instead of hitting each of them in full.
        private readonly Dictionary<Rigidbody, (float momentum, List<StructuralSection> sections)> pendingImpacts = new();
        private readonly List<Rigidbody> pendingOrder = new();

        public IReadOnlyList<StructuralSection> Sections => sections;
        public StructureConfig Config => config;
        public float Stability => stability;

        /// <summary>Host: danger (M5.6) makes overloaded sections fail faster; 1 = normal.</summary>
        public float DangerDecay { get; set; } = 1f;
        public int Seed => seed;
        public int CollapseCount { get; private set; }
        /// <summary>Bumped by every <see cref="ApplyStability"/> (start, stability change, re-roll), so clients know to restore.</summary>
        public int Generation { get; private set; }
        /// <summary>Client: the host runs this structure; this copy only mirrors it.</summary>
        public bool IsMirror { get; private set; }
        /// <summary>Runs damage, load, timers and cascades: the host (or solo), never a mirror.</summary>
        public bool HasAuthority => !IsMirror && GameAuthority.IsHost;

        private int StructureMask => 1 << Mathf.Max(0, GameLayers.StructureLayer);

        private void Awake()
        {
            // Stable ids (name, then position: x, z, and height for stacked floors) so "section X
            // collapsed" means the same piece on every machine and in every run.
            StructuralSection[] found = childrenOnly
                ? GetComponentsInChildren<StructuralSection>(true)
                : FindObjectsByType<StructuralSection>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            sections.AddRange(found
                .OrderBy(s => s.name, System.StringComparer.Ordinal).ThenBy(s => s.transform.position.x).ThenBy(s => s.transform.position.z)
                .ThenBy(s => s.transform.position.y));
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
            if (HasAuthority) ApplyStability(stability, seed);
        }

        /// <summary>Client side of a session (on) or back to running it locally (off, e.g. after leaving).</summary>
        public void SetMirror(bool mirror)
        {
            IsMirror = mirror;
            foreach (StructuralSection s in sections) s.IsMirror = mirror;
        }

        /// <summary>
        /// Fingerprint of the section list in id order (names + positions to the cm), so a client can tell
        /// when its building doesn't match the host's and section ids would mean different pieces.
        /// </summary>
        public int LayoutHash()
        {
            unchecked
            {
                int hash = 17 + sections.Count;
                foreach (StructuralSection s in sections)
                {
                    foreach (char c in s.name) hash = hash * 31 + c;
                    Vector3 p = s.transform.position;
                    hash = hash * 31 + Mathf.RoundToInt(p.x * 100f);
                    hash = hash * 31 + Mathf.RoundToInt(p.y * 100f);
                    hash = hash * 31 + Mathf.RoundToInt(p.z * 100f);
                }
                return hash;
            }
        }

        /// <summary>Host: rescales every section for a stability, resets them and re-rolls pre-damage.</summary>
        public void ApplyStability(float newStability, int newSeed)
        {
            if (IsMirror) return;
            Reconfigure(newStability, newSeed);
            bool[] eligible = sections.Select(s => s.CanCollapse).ToArray();
            float[] plan = StructureMath.PreDamagePlan(eligible, stability, seed, config);
            for (int i = 0; i < sections.Count; i++)
                if (plan[i] > 0f) sections[i].PreDamage(plan[i]);
        }

        /// <summary>
        /// Client: the host re-rolled or changed stability. Scales and restores every section (debris
        /// cleared, colliders back); their health and stage then come from the host, not a local roll.
        /// </summary>
        /// <summary>
        /// Host, danger: the building ages on its own. Each picked collapsible section that can still fall
        /// loses <paramref name="fraction"/> of its health, never below <paramref name="floor"/>: it creaks
        /// and cracks, but only weight finishes the job.
        /// </summary>
        public int AgeRandomSections(int count, float fraction, float floor, System.Random random)
        {
            if (!HasAuthority || count <= 0) return 0;
            var candidates = sections.FindAll(s => s.CanCollapse && !s.IsCollapsed && s.HealthFraction > floor);
            int aged = 0;
            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                StructuralSection s = candidates[random.Next(candidates.Count)];
                candidates.Remove(s);
                float amount = Mathf.Min(fraction, s.HealthFraction - floor) * s.MaxHealth;
                if (amount <= 0f) continue;
                s.Damage(amount);
                aged++;
            }
            return aged;
        }

        public void MirrorStability(float newStability, int newSeed, int hostGeneration)
        {
            // The host's generation, not a local count, so F1 shows the same number on every machine.
            if (IsMirror) Reconfigure(newStability, newSeed, hostGeneration);
        }

        private void Reconfigure(float newStability, int newSeed, int? generation = null)
        {
            stability = Mathf.Clamp01(newStability);
            seed = newSeed;
            supportCache.Invalidate();
            Generation = generation ?? Generation + 1;
            float capacityScale = config.CapacityScale(stability);
            float decayScale = config.DecayScale(stability);
            foreach (StructuralSection s in sections)
            {
                s.Configure(capacityScale, decayScale, seed);
                s.ResetState();
            }
        }

        private void FixedUpdate()
        {
            if (HasAuthority) Step(Time.fixedDeltaTime);
        }

        /// <summary>Host: one simulation step. Public so tests can step deterministically.</summary>
        public void Step(float dt)
        {
            if (IsMirror) return;
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
            supportCache.BeginStep();
            IReadOnlyList<ILoadSource> all = LoadSources.All;
            for (int i = 0; i < all.Count; i++)
            {
                float weight = all[i].LoadWeight;
                if (weight <= 0f) continue;

                points.Clear();
                all[i].GetLoadPoints(points);
                if (!supportCache.TryGet(all[i], points, supports))
                {
                    supports.Clear();
                    foreach (LoadPoint p in points)
                    {
                        StructuralSection support = SectionBelow(p.Position + Vector3.up * RayLift, config.MaxSupportDistance + RayLift);
                        if (support != null) supports.Add(support);
                    }
                    supportCache.Store(all[i], points, supports);
                }
                if (supports.Count == 0) continue;

                float share = weight / supports.Count;
                foreach (StructuralSection s in supports)
                    loads[s] = (loads.TryGetValue(s, out float kg) ? kg : 0f) + share;
            }
        }

        public float LoadOn(StructuralSection section) => loads.TryGetValue(section, out float kg) ? kg : 0f;

        private StructuralSection SectionBelow(Vector3 from, float distance) => FirstOwnSectionBelow(from, distance, out _);

        /// <summary>
        /// Nearest section of THIS structure under a point. Another structure's colliders (only ever in
        /// in-process network tests, where every machine's copy shares one physics world) are skipped.
        /// </summary>
        private StructuralSection FirstOwnSectionBelow(Vector3 from, float distance, out float hitDistance)
        {
            hitDistance = 0f;
            int count = Physics.RaycastNonAlloc(from, Vector3.down, rayHits, distance, StructureMask, QueryTriggerInteraction.Ignore);
            StructuralSection best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (rayHits[i].distance >= bestDistance) continue;
                StructuralSection s = rayHits[i].collider.GetComponentInParent<StructuralSection>();
                if (s != null && s.Simulation != null && s.Simulation != this) continue;
                // Anything else on the Structure layer still blocks, as a plain raycast would.
                best = s;
                bestDistance = rayHits[i].distance;
            }
            if (best != null) hitDistance = bestDistance;
            return best;
        }

        private void OnSectionCollapsed(StructuralSection section)
        {
            CollapseCount++;
            supportCache.Invalidate();
            Bounds surface = section.SurfaceBounds;
            // Already down when this machine joined: nothing is falling, nobody is standing on it.
            if (section.CollapsedQuietly) return;

            // Whatever was resting on it must fall now, even if the physics engine had put it to sleep.
            Vector3 halfExtents = surface.extents + new Vector3(0.1f, 1.5f, 0.1f);
            int count = Physics.OverlapBoxNonAlloc(surface.center + Vector3.up * 1.5f, halfExtents, overlap,
                Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (overlap[i].attachedRigidbody != null) overlap[i].attachedRigidbody.WakeUp();

            // Every machine drops its own players (each owns its movement); noise and cascades are host gameplay.
            StructureSignals.RaiseCollapsed(surface);
            if (!HasAuthority) return;
            NoiseSystem.Emit(surface.center, config.CollapseLoudness, NoiseSource.Collapse);
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
                StructuralSection below = FirstOwnSectionBelow(from, config.CascadeSearchDistance, out float distance);
                if (below == null || below == fallen || below.IsCollapsed) continue;
                hits[below] = hits.TryGetValue(below, out float d) ? Mathf.Min(d, distance) : distance;
            }
            if (hits.Count == 0) return;

            float mass = config.Profile(fallen.Type).SelfMass * config.CascadeFactor / hits.Count;
            foreach (KeyValuePair<StructuralSection, float> h in hits.OrderBy(h => h.Key.Id))
                h.Key.ApplyImpact(mass * StructureMath.FallSpeed(h.Value));
        }
    }
}
