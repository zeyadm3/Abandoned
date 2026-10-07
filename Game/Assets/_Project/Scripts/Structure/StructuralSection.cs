using System;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// One breakable piece (floor tile, balcony, stair segment). Holds the host-authoritative
    /// state — capacity, health, load, stage — and switches its colliders off when it collapses.
    /// Visuals and sound react to its events in <see cref="SectionPresentation"/>.
    /// On a client the section is a mirror: the host's state arrives through
    /// <see cref="ApplyReplicated"/> (StructureNetSync) and nothing here simulates damage.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-20)] // state must exist before presentation and the simulation read it
    public class StructuralSection : MonoBehaviour
    {
        [SerializeField] private StructureConfig config;
        [SerializeField] private SectionType type = SectionType.Floor;
        [Tooltip("Off for pieces that must never fall (e.g. a ground slab with nothing below). Per section, not a global rule.")]
        [SerializeField] private bool canCollapse = true;
        [Tooltip("Authored starting health (1 = intact), applied before stability pre-damage.")]
        [SerializeField, Range(0.05f, 1f)] private float initialHealth = 1f;
        [Tooltip("Authored weakness: rotten boards, a cracked slab. Multiplies the type's capacity.")]
        [SerializeField, Range(0.05f, 1f)] private float capacityMultiplier = 1f;
        [Tooltip("Child holding the renderers: sags while Failing and is replaced by debris on collapse.")]
        [SerializeField] private Transform visual;

        private Collider[] colliders;
        private float creakTimer;
        private int collapseSeedBase;

        public int Id { get; internal set; } = -1;
        public SectionType Type => type;
        public bool CanCollapse => canCollapse;
        public StructureConfig Config => config;
        public Transform Visual => visual;
        /// <summary>Load it holds before overload drains it: the profile, stability, authoring, and any support jack.</summary>
        public float Capacity => baseCapacity * Reinforcement;
        /// <summary>Host: a support jack under it (M9.4) multiplies its capacity until the next restore.</summary>
        public float Reinforcement { get; private set; } = 1f;
        private float baseCapacity;
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public float Load { get; private set; }
        public float DecayScale { get; private set; } = 1f;
        public StructuralStage Stage { get; private set; }
        public float FailingTime { get; private set; }
        public int CollapseSeed { get; private set; }
        /// <summary>Client copy: state comes from the host, never from local load, impacts or timers.</summary>
        public bool IsMirror { get; internal set; }
        /// <summary>The last collapse was applied on joining (already down): no break, sound or ragdolls.</summary>
        public bool CollapsedQuietly { get; private set; }

        public float HealthFraction => MaxHealth > 0f ? Health / MaxHealth : 0f;
        public float LoadRatio => Capacity > 0f ? Load / Capacity : 0f;
        public bool IsCollapsed => Stage == StructuralStage.Collapsed;

        public event Action<StructuralSection, StructuralStage> StageChanged;
        public event Action<StructuralSection, float> Damaged;
        public event Action<StructuralSection> Collapsed;
        public event Action<StructuralSection> Restored;

        private void Awake()
        {
            colliders = GetComponentsInChildren<Collider>(true);
            Configure(1f, 1f, 0);
            ResetState();
        }

        /// <summary>World bounds of the walking surface (top of the colliders).</summary>
        public Bounds SurfaceBounds
        {
            get
            {
                colliders ??= GetComponentsInChildren<Collider>(true);
                if (colliders.Length == 0) return new Bounds(transform.position, Vector3.zero);
                Bounds b = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++) b.Encapsulate(colliders[i].bounds);
                return b;
            }
        }

        /// <summary>True while any of the section's colliders is on (off from the moment it collapses).</summary>
        public bool CollidersEnabled
        {
            get
            {
                foreach (Collider c in colliders) if (c != null && c.enabled) return true;
                return false;
            }
        }

        // ---- Host-only from here: called by StructureSimulation or host physics callbacks. ----

        public void Configure(float capacityScale, float decayScale, int seedBase)
        {
            SectionProfile profile = config.Profile(type);
            baseCapacity = profile.Capacity * capacityScale * capacityMultiplier;
            MaxHealth = profile.MaxHealth;
            DecayScale = decayScale;
            collapseSeedBase = seedBase;
        }

        /// <summary>Back to authored health with colliders on; used at start and by the debug reroll.</summary>
        /// <summary>Host: braced from below (a support jack); a restore takes it away again.</summary>
        public void Reinforce(float multiplier) => Reinforcement = Mathf.Max(Reinforcement, multiplier);

        public void ResetState()
        {
            Reinforcement = 1f;
            Health = MaxHealth * initialHealth;
            Load = 0f;
            FailingTime = 0f;
            creakTimer = 0f;
            SetColliders(true);
            // Restored resets visuals to Stable; UpdateStage then raises StageChanged if load/health say otherwise.
            Stage = StructuralStage.Stable;
            CollapsedQuietly = false;
            Restored?.Invoke(this);
            // A mirror waits for the host's stage instead of deriving its own.
            if (!IsMirror) UpdateStage();
        }

        /// <summary>Host: seeded starting damage. Never pushes a section below MinStartHealth (or its authored health if lower).</summary>
        public void PreDamage(float fraction)
        {
            float floor = Mathf.Min(Health, MaxHealth * config.MinStartHealth);
            Damage(Mathf.Min(MaxHealth * fraction, Health - floor));
        }

        /// <summary>The level's simulation; impacts go through it so one landing is shared between sections.</summary>
        public StructureSimulation Simulation { get; internal set; }

        public void SetLoad(float kg) => Load = kg;

        public void Tick(float dt)
        {
            if (IsCollapsed || IsMirror) return;

            if (Stage == StructuralStage.Failing)
            {
                // ~2 s to get off, then it goes regardless (GDD 6.1): readable, not reversible.
                FailingTime += dt;
                EmitCreaks(dt);
                if (FailingTime >= config.FailingDuration) Collapse();
                return;
            }

            float danger = Simulation != null ? Simulation.DangerDecay : 1f;
            float drain = StructureMath.OverloadDrain(Load, Capacity, config.OverloadDrainPerSecond, DecayScale * danger, dt);
            if (drain > 0f) Damage(drain);
            else UpdateStage();
            EmitCreaks(dt);
        }

        public void ApplyImpact(float momentum)
        {
            if (IsMirror) return;
            float damage = StructureMath.ImpactDamage(momentum, config.ImpactThreshold, config.ImpactDamagePerMomentum);
            if (damage <= 0f) return;
            // Readable: a section that hasn't shown cracks yet can't be knocked straight into Failing.
            if (Stage < StructuralStage.Cracking)
                damage = Mathf.Min(damage, Health - MaxHealth * config.WarningFloor);
            Damage(damage);
        }

        public void Collapse()
        {
            if (IsCollapsed || !canCollapse || IsMirror) return;
            BreakNow(collapseSeedBase ^ (Id * 7919 + 17), quiet: false);
        }

        /// <summary>
        /// Client: the host's state for this section. A collapse flips the colliders off right here, so
        /// this machine's own player drops at the moment the news arrives; <paramref name="quiet"/> skips
        /// the break and ragdolls for sections that were already down when this machine joined.
        /// </summary>
        public void ApplyReplicated(StructuralStage stage, float healthFraction, float load, float failingTime, int seed, bool quiet)
        {
            if (!IsMirror) return;
            Load = load;
            if (stage == StructuralStage.Collapsed)
            {
                if (!IsCollapsed) BreakNow(seed, quiet);
                return;
            }
            if (IsCollapsed)
            {
                // Only a host restore brings a section back; normally ResetState already did.
                SetColliders(true);
                Stage = StructuralStage.Stable;
                CollapsedQuietly = false;
                Restored?.Invoke(this);
            }
            Health = Mathf.Clamp01(healthFraction) * MaxHealth;
            FailingTime = failingTime;
            if (stage == Stage) return;
            StructuralStage previous = Stage;
            Stage = stage;
            StageChanged?.Invoke(this, previous);
        }

        private void BreakNow(int seed, bool quiet)
        {
            StructuralStage previous = Stage;
            Stage = StructuralStage.Collapsed;
            Health = 0f;
            CollapseSeed = seed;
            CollapsedQuietly = quiet;
            SetColliders(false);
            Physics.SyncTransforms();
            StageChanged?.Invoke(this, previous);
            Collapsed?.Invoke(this);
        }

        internal void Damage(float amount)
        {
            if (IsCollapsed || IsMirror || Stage == StructuralStage.Failing || amount <= 0f) return;
            // Non-collapsible sections can crack but always keep a sliver of health.
            float floor = canCollapse ? 0f : MaxHealth * 0.01f;
            Health = Mathf.Max(floor, Health - amount);
            Damaged?.Invoke(this, amount);
            UpdateStage();
        }

        private void UpdateStage()
        {
            if (IsCollapsed || Stage == StructuralStage.Failing) return;
            StructuralStage next = StructureMath.StageFor(HealthFraction, LoadRatio, config);
            if (!canCollapse && next == StructuralStage.Failing) next = StructuralStage.Cracking;
            if (next == Stage) return;
            StructuralStage previous = Stage;
            Stage = next;
            if (next == StructuralStage.Failing) FailingTime = 0f;
            if (next > previous && next >= StructuralStage.Cracking)
                NoiseSystem.Emit(SurfaceBounds.center, config.CrackLoudness, NoiseSource.Creak);
            StageChanged?.Invoke(this, previous);
        }

        private void EmitCreaks(float dt)
        {
            if (Stage == StructuralStage.Stable) { creakTimer = 0f; return; }
            float interval = Stage switch
            {
                StructuralStage.Stressed => config.CreakInterval,
                StructuralStage.Cracking => config.CreakInterval * 0.5f,
                _ => config.CreakInterval * 0.25f,
            };
            creakTimer += dt;
            if (creakTimer < interval) return;
            creakTimer = 0f;
            NoiseSystem.Emit(SurfaceBounds.center, config.CreakLoudness, NoiseSource.Creak);
        }

        private void SetColliders(bool enabled)
        {
            foreach (Collider c in colliders) if (c != null) c.enabled = enabled;
        }

        private void OnCollisionEnter(Collision collision) => HandleCollision(collision);

        /// <summary>
        /// Host: an object hit one of this section's colliders. Called directly for a collider on the
        /// root and via <see cref="SectionColliderRelay"/> for child colliders (stair ramps).
        /// </summary>
        public void HandleCollision(Collision collision)
        {
            bool authority = Simulation != null ? Simulation.HasAuthority : GameAuthority.IsHost;
            if (!authority || IsMirror || IsCollapsed) return;
            Rigidbody body = collision.rigidbody;
            if (body == null || body.gameObject.layer == GameLayers.DebrisLayer) return;

            Vector3 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector3.up;
            float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, normal));
            if (speed < config.MinImpactSpeed) return;
            float weight = body.TryGetComponent(out IWeighted weighted) ? weighted.GameplayWeight : body.mass;
            if (Simulation != null) Simulation.ReportImpact(this, body, weight * speed);
            else ApplyImpact(weight * speed);
        }

#if UNITY_EDITOR
        public void EditorSetup(StructureConfig structureConfig, SectionType sectionType, bool collapsible,
            float startHealth, float capacityScale, Transform visualRoot)
        {
            capacityMultiplier = capacityScale;
            config = structureConfig;
            type = sectionType;
            canCollapse = collapsible;
            initialHealth = startHealth;
            visual = visualRoot;
        }
#endif
    }
}
