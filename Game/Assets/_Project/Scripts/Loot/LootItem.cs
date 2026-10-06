using System;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// A piece of loot in the world. The host rolls its value from a seed and applies impact
    /// damage; every machine raises <see cref="Impacted"/> for local sound. Presentation lives in
    /// <see cref="LootFeedback"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class LootItem : MonoBehaviour, ICarryable, IValuable, ILoadSource
    {
        [SerializeField] private LootDefinition definition;
        [SerializeField] private LootDamageConfig damageConfig;
        [Tooltip("Seed for the value/condition roll. 0 = pick one at spawn.")]
        [SerializeField] private int seed;

        private float lastDamageTime = float.NegativeInfinity;
        private bool initialized;
        private Grabbable grabbable;
        private Rigidbody body;

        public LootDefinition Definition => definition;
        public int FullValue { get; private set; }
        public float Condition { get; private set; }
        public int CurrentValue { get; private set; }
        public bool IsShattered { get; private set; }
        public int Seed => seed;

        public string DisplayName => definition.DisplayName;
        public CarryClass CarryClass => definition.CarryClass;
        public float GameplayWeight => definition.GameplayWeight;

        /// <summary>Any collision worth a sound (all machines): item, speed along the normal, contact point.</summary>
        public event Action<LootItem, float, Vector3> Impacted;
        /// <summary>Host: value lost from an impact (item, loss, contact point).</summary>
        public event Action<LootItem, int, Vector3> Damaged;
        /// <summary>Host: item shattered to $0 and is about to be removed.</summary>
        public event Action<LootItem, Vector3> Shattered;

        private void Awake()
        {
            grabbable = GetComponent<Grabbable>();
            body = GetComponent<Rigidbody>();
        }

        private void OnEnable() => LoadSources.Register(this);

        private void OnDisable() => LoadSources.Unregister(this);

        /// <summary>
        /// Resting weight on the structure: zero while pocketed, carried in hands (the carrier
        /// counts it) or still flying; dragged items press down where they are.
        /// </summary>
        public float LoadWeight
        {
            get
            {
                if (IsShattered || grabbable == null || grabbable.IsPocketed) return 0f;
                if (grabbable.Holder != null && !grabbable.IsDragged) return 0f;
                if (!grabbable.IsDragged && body.linearVelocity.sqrMagnitude > damageConfig.LoadRestingSpeed * damageConfig.LoadRestingSpeed)
                    return 0f;
                return definition.GameplayWeight;
            }
        }

        public void GetLoadPoints(List<LoadPoint> points)
        {
            // Footprint centre and corners, so a piano across two tiles loads both. Rays start at the
            // top of the item: on a ramp the uphill corners' surface is above the item's lowest point.
            Bounds b = grabbable.GetBounds();
            float y = b.max.y;
            Vector3 c = b.center, e = b.extents * 0.8f;
            points.Add(new LoadPoint(new Vector3(c.x, y, c.z)));
            points.Add(new LoadPoint(new Vector3(c.x + e.x, y, c.z + e.z)));
            points.Add(new LoadPoint(new Vector3(c.x - e.x, y, c.z + e.z)));
            points.Add(new LoadPoint(new Vector3(c.x + e.x, y, c.z - e.z)));
            points.Add(new LoadPoint(new Vector3(c.x - e.x, y, c.z - e.z)));
        }

        private void Start()
        {
            if (!initialized && GameAuthority.IsHost)
                Initialize(seed != 0 ? seed : GetInstanceID());
        }

        /// <summary>Host: rolls value and condition. Same definition + seed = same result.</summary>
        public void Initialize(int rollSeed)
        {
            seed = rollSeed;
            LootMath.Roll roll = LootMath.RollValue(definition.ValueMin, definition.ValueMax,
                definition.ConditionMin, definition.ConditionMax, rollSeed);
            FullValue = roll.Value;
            Condition = roll.Condition;
            CurrentValue = FullValue;
            initialized = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsShattered) return;
            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            Vector3 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector3.up;
            // Only the component along the normal hurts: sliding along a floor is not an impact.
            float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, normal));

            Impacted?.Invoke(this, speed, point);
            if (!GameAuthority.IsHost) return;
            ApplyImpact(speed, point);
            if (speed >= damageConfig.MinSoundSpeed)
                NoiseSystem.Emit(point, damageConfig.ImpactNoise * definition.Noise * Mathf.Clamp01(speed / damageConfig.FullVolumeSpeed),
                    NoiseSource.LootImpact);
        }

        /// <summary>Host: applies one impact's damage. Public so M3 can apply client-reported impacts.</summary>
        public void ApplyImpact(float speed, Vector3 point)
        {
            if (IsShattered || !initialized) return;
            if (Time.time - lastDamageTime < damageConfig.ImpactCooldown) return;

            int loss = LootMath.ImpactLoss(damageConfig.Profile(definition.Fragility), FullValue, speed, out bool shatters);
            if (shatters)
            {
                lastDamageTime = Time.time;
                Shatter(point);
                return;
            }
            if (loss <= 0) return;

            lastDamageTime = Time.time;
            loss = Mathf.Min(loss, CurrentValue);
            CurrentValue -= loss;
            Damaged?.Invoke(this, loss, point);
        }

        private void Shatter(Vector3 point)
        {
            IsShattered = true;
            CurrentValue = 0;
            if (TryGetComponent(out Grabbable grabbable) && grabbable.Holder != null)
                InteractionService.Handler.RequestDrop(grabbable.Holder);
            Shattered?.Invoke(this, point);
            Destroy(gameObject);
        }
    }
}
