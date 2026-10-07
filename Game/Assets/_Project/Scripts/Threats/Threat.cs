using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// What every threat shares (GDD 9: each has one rule players can learn): it runs on the host only,
    /// everyone sees it through its NetworkTransform, danger scales it, ghosts see it, and killing is
    /// "touching, not through a wall".
    /// </summary>
    public abstract class Threat : NetworkBehaviour
    {
        private static readonly List<Threat> Spawned = new();
        [SerializeField] private ThreatDefinition definition;
        private float nextAttack;
        public ThreatDefinition Definition => definition;
        public virtual ThreatMotion DesiredMotion => ThreatMotion.Idle;
        public float Aggression => 1f + (Extraction.RunState.Current != null ? Extraction.RunState.Current.State.Danger : 0) * (definition != null ? definition.AggressionPerDanger : 0.09f);

        public static IReadOnlyList<Threat> All => Spawned;

        public abstract string DisplayName { get; }
        /// <summary>The death screen's cause line when this kills you (UI step 3).</summary>
        public virtual string DeathLine => $"The {DisplayName} got you.";
        public float SpeedScale { get; set; } = 1f;
        public float HearingScale { get; set; } = 1f;
        public int Kills { get; protected set; }

        protected int WallMask { get; private set; }

        protected virtual void Awake() =>
            WallMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");

        public override void OnNetworkSpawn() => Spawned.Add(this);

        public override void OnNetworkDespawn() => Spawned.Remove(this);

        /// <summary>Where a living or downed player is (the body when ragdolled).</summary>
        protected static Vector3 PositionOf(NetworkPlayer p) => p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;

        /// <summary>Host: kills the first living player within reach it can actually touch; returns them or null.</summary>
        protected NetworkPlayer KillWithinReach(float range) => DamageWithinReach(range, 1000f, true);

        protected NetworkPlayer DamageWithinReach(float range, float fallbackDamage, bool lethal = false)
        {
            if (!IsServer || !IsSpawned || Time.time < nextAttack) return null;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || p.IsDead) continue;
                Vector3 at = PositionOf(p);
                Vector3 d = at - transform.position;
                if (Mathf.Abs(d.y) > 1.8f || new Vector2(d.x, d.z).sqrMagnitude > range * range) continue;
                if (Sheltered(at)) continue;
                if (Physics.Linecast(transform.position + Vector3.up * 1.3f, at + Vector3.up * 0.8f, WallMask, QueryTriggerInteraction.Ignore)) continue;
                float damage = lethal || definition != null && definition.LethalContact ? p.MaxHealth : (definition != null ? definition.Damage : fallbackDamage) * Mathf.Min(Aggression, 1.4f);
                p.DealDamage(damage, DeathLine);
                if (p.IsDead) Kills++;
                nextAttack = Time.time + (definition != null ? definition.AttackCooldown : 2.5f) / Mathf.Min(Aggression, 1.6f);
                GetComponent<ThreatAnimationSync>()?.ServerAttack();
                Debug.Log($"[Threat] {DisplayName} struck player {p.OwnerClientId}.");
                return p;
            }
            return null;
        }

        /// <summary>Host: inside an armored truck (M10.1) nothing can touch you.</summary>
        protected static bool Sheltered(Vector3 at) => Extraction.TruckCargo.Current != null && Extraction.TruckCargo.Current.Carries(at);

        private readonly RaycastHit[] sightHits = new RaycastHit[8];

        /// <summary>Nothing solid between two points (its own body doesn't count).</summary>
        protected bool LineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            float length = d.magnitude;
            if (length < 0.01f) return true;
            int n = Physics.RaycastNonAlloc(from, d / length, sightHits, length, WallMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
                if (!sightHits[i].collider.transform.IsChildOf(transform)) return false;
            return true;
        }

#if UNITY_EDITOR
        public void EditorSetupDefinition(ThreatDefinition value) => definition = value;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetThreats() => Spawned.Clear();
    }
}
