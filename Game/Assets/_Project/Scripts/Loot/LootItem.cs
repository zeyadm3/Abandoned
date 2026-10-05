using System;
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
    public class LootItem : MonoBehaviour, ICarryable, IValuable
    {
        [SerializeField] private LootDefinition definition;
        [SerializeField] private LootDamageConfig damageConfig;
        [Tooltip("Seed for the value/condition roll. 0 = pick one at spawn.")]
        [SerializeField] private int seed;

        private float lastDamageTime = float.NegativeInfinity;
        private bool initialized;

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
            if (GameAuthority.IsHost) ApplyImpact(speed, point);
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
