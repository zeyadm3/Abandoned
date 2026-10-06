using System;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// A piece of loot in the world. The host (value authority) rolls its value from a seed and
    /// applies impact damage. Impacts are judged on the machine that simulates the body; networking
    /// relays them and replays sounds, damage text and shatters on every machine through the Replay*
    /// methods. Presentation lives in <see cref="LootFeedback"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class LootItem : MonoBehaviour, ICarryable, IValuable, ILoadSource, ISharedCarrySpec
    {
        [SerializeField] private LootDefinition definition;
        [SerializeField] private LootDamageConfig damageConfig;
        [Tooltip("Seed for the value/condition roll. 0 = pick one at spawn.")]
        [SerializeField] private int seed;

        private float lastDamageTime = float.NegativeInfinity;
        private bool initialized;
        private Grabbable grabbable;
        private Rigidbody body;
        private bool? valueAuthority;

        public LootDefinition Definition => definition;
        public int FullValue { get; private set; }
        public float Condition { get; private set; }
        public int CurrentValue { get; private set; }
        public bool IsShattered { get; private set; }
        public int Seed => seed;
        public bool IsInitialized => initialized;
        public LootDamageConfig DamageConfig => damageConfig;

        /// <summary>This machine owns value and damage: the host, or offline. Networking sets it per item.</summary>
        public bool HasValueAuthority => valueAuthority ?? GameAuthority.IsHost;

        /// <summary>How a shattered item leaves the world; networking despawns instead of destroying.</summary>
        public Action<LootItem> Remover { get; set; }

        public string DisplayName => definition.DisplayName;
        public CarryClass CarryClass => definition.CarryClass;
        public float GameplayWeight => definition.GameplayWeight;
        public int RequiredCarriersOverride => definition.RequiredCarriers;
        public IReadOnlyList<Vector3> AuthoredCarryPoints => definition.CarryPoints ?? Array.Empty<Vector3>();
        public Vector3 CarrySize => definition.Size;

        /// <summary>An impact to present (sound, shake) on this machine: item, speed along the normal, contact point.</summary>
        public event Action<LootItem, float, Vector3> Impacted;
        /// <summary>This machine's physics hit something (physics authority only); networking reports/relays it.</summary>
        public event Action<LootItem, LootImpact> CollisionImpact;
        /// <summary>Value lost from an impact (item, loss, contact point). Host applies; others replay it.</summary>
        public event Action<LootItem, int, Vector3> Damaged;
        /// <summary>Item shattered to $0 and is about to be removed. Host applies; others replay it.</summary>
        public event Action<LootItem, Vector3> Shattered;
        /// <summary>Value authority: value, condition or shattered changed.</summary>
        public event Action<LootItem> ValueChanged;

        private void Awake()
        {
            grabbable = GetComponent<Grabbable>();
            body = GetComponent<Rigidbody>();
        }

        private void OnEnable() => LoadSources.Register(this);

        private void OnDisable() => LoadSources.Unregister(this);

        /// <summary>
        /// Resting weight on the structure: zero while pocketed, carried in hands or lifted by a crew
        /// (the carriers count it, each their share) or still flying; dragged items press down where they are.
        /// </summary>
        public float LoadWeight
        {
            get
            {
                if (IsShattered || grabbable == null || grabbable.IsPocketed) return 0f;
                if (grabbable.IsLifted || (grabbable.Holder != null && !grabbable.IsDragged)) return 0f;
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
            if (HasValueAuthority) EnsureInitialized();
        }

        /// <summary>null = follow <see cref="GameAuthority"/>; networking sets true on the host, false on clients.</summary>
        public void SetValueAuthority(bool? authority) => valueAuthority = authority;

        public void EnsureInitialized()
        {
            if (!initialized) Initialize(seed != 0 ? seed : GetInstanceID());
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
            ValueChanged?.Invoke(this);
        }

        /// <summary>Client: the host's value state arrived.</summary>
        public void ApplyNetworkValue(int fullValue, int currentValue, float condition, bool shattered)
        {
            FullValue = fullValue;
            CurrentValue = currentValue;
            Condition = condition;
            IsShattered = shattered;
            initialized = true;
        }

        // Presentation of things the host decided, raised on machines that didn't decide them.
        public void ReplayImpact(float speed, Vector3 point) => Impacted?.Invoke(this, speed, point);
        public void ReplayDamaged(int loss, Vector3 point) => Damaged?.Invoke(this, loss, point);
        public void ReplayShattered(Vector3 point) => Shattered?.Invoke(this, point);

        private void OnCollisionEnter(Collision collision)
        {
            if (IsShattered) return;
            // Only the machine simulating the body judges its hits; elsewhere it's a kinematic copy.
            if (grabbable != null && !grabbable.HasPhysicsAuthority) return;
            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            Vector3 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector3.up;
            // Only the component along the normal hurts: sliding along a floor is not an impact.
            float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, normal));

            Impacted?.Invoke(this, speed, point);
            CollisionImpact?.Invoke(this, new LootImpact(speed, point, normal, collision.rigidbody));
            if (!HasValueAuthority) return;
            ApplyImpact(speed, point);
            EmitImpactNoise(speed, point);
        }

        /// <summary>Host: threats hear impacts (also client-reported ones).</summary>
        public void EmitImpactNoise(float speed, Vector3 point)
        {
            if (!HasValueAuthority || speed < damageConfig.MinSoundSpeed) return;
            NoiseSystem.Emit(point, damageConfig.ImpactNoise * definition.Noise * Mathf.Clamp01(speed / damageConfig.FullVolumeSpeed),
                NoiseSource.LootImpact);
        }

        /// <summary>Host: applies one impact's damage, from its own physics or a carrier's report.</summary>
        public void ApplyImpact(float speed, Vector3 point)
        {
            if (IsShattered || !initialized || !HasValueAuthority) return;
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
            ValueChanged?.Invoke(this);
            Damaged?.Invoke(this, loss, point);
        }

        private void Shatter(Vector3 point)
        {
            IsShattered = true;
            CurrentValue = 0;
            ValueChanged?.Invoke(this);
            if (grabbable != null && grabbable.Holder != null)
                InteractionService.Handler.RequestDrop(grabbable.Holder);
            Shattered?.Invoke(this, point);
            if (Remover != null) Remover(this);
            else Destroy(gameObject);
        }
    }
}
