using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// What every threat shares (GDD 9: each has one rule players can learn): it runs on the host only,
    /// everyone sees it through its NetworkTransform, danger scales it, ghosts see it, and killing is
    /// "touching, not through a wall".
    /// </summary>
    public abstract partial class Threat : NetworkBehaviour
    {
        private static readonly List<Threat> Spawned = new();
        [SerializeField] private ThreatDefinition definition;
        private float nextAttack;
        private float attackReadyAt;
        private NavMeshPath patrolPath;
        // Host (0.16.0): the players it watched climb into a locker (client ids), until they come out.
        private readonly HashSet<ulong> watchedHide = new();
        public float WarningRemaining => Mathf.Max(0f, attackReadyAt - Time.time);
        public ThreatDefinition Definition => definition;
        public virtual ThreatMotion DesiredMotion => ThreatMotion.Idle;
        public float Aggression => 1f + (Extraction.RunState.Current != null ? Extraction.RunState.Current.State.Danger : 0) * (definition != null ? definition.AggressionPerDanger : 0.09f);

        public static IReadOnlyList<Threat> All => Spawned;

        public abstract string DisplayName { get; }
        /// <summary>The death screen's cause line when this kills you (UI step 3).</summary>
        public virtual string DeathLine => $"The {DisplayName} got you.";
        /// <summary>Host: the job's THREAT level scales every non-lethal hit (<see cref="ThreatPacingConfig.LevelDamageScale"/>).</summary>
        public static float ContractDamageScale { get; set; } = 1f;
        public float SpeedScale { get; set; } = 1f;
        public float HearingScale { get; set; } = 1f;
        public int Kills { get; protected set; }
        /// <summary>
        /// Host, <see cref="ThreatPacing"/>: set while nobody has come near it for a while, so its idle wandering
        /// heads for the crew's part of the building. Never a target: it still has to hear or see someone.
        /// </summary>
        public Vector3? RoamHint { get; set; }
        /// <summary>Host: where idle wandering is centred (the drift point if there is one, else where it stands).</summary>
        protected Vector3 RoamCentre => RoamHint ?? transform.position;

        protected int WallMask { get; private set; }

        protected virtual void Awake()
        {
            WallMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            patrolPath = new NavMeshPath();
            if (GetComponent<ThreatPolish>() == null) gameObject.AddComponent<ThreatPolish>(); // 0.21.3
        }

        // 0.21.3: loot thrown or dropped hard into it makes it recoil, on every machine (it isn't hurt).
        private void OnCollisionEnter(Collision collision)
        {
            if (!IsServer || !IsSpawned || collision.rigidbody == null || collision.relativeVelocity.magnitude < 3f) return;
            if (collision.rigidbody.GetComponent<Loot.LootItem>() == null) return;
            FlinchRpc(-collision.relativeVelocity.normalized);
        }

        [Rpc(SendTo.Everyone)]
        private void FlinchRpc(Vector3 from) => GetComponent<ThreatPolish>()?.Flinch(from);

        public override void OnNetworkSpawn()
        {
            Spawned.Add(this);
            attackReadyAt = Time.time + (definition != null ? definition.SpawnWarningSeconds : 0f);
            if (IsServer) NetworkPlayer.HidingChanged += OnHidingChanged;
        }

        public override void OnNetworkDespawn()
        {
            Spawned.Remove(this);
            NetworkPlayer.HidingChanged -= OnHidingChanged;
        }

        public override void OnDestroy()
        {
            // Destroyed without a despawn (session torn down): don't leave a dead listener behind.
            NetworkPlayer.HidingChanged -= OnHidingChanged;
            base.OnDestroy();
        }

        /// <summary>Where a living or downed player is (the body when ragdolled).</summary>
        protected static Vector3 PositionOf(NetworkPlayer p) => p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;

        // ---- Lockers (0.16.0): a hidden player is safe from anything that didn't watch them climb in ----

        // Host: a player climbed in (raised before they moved) or out.
        private void OnHidingChanged(NetworkPlayer p, int was, int now)
        {
            if (p == null || p.NetworkManager != NetworkManager) return;
            if (now < 0) watchedHide.Remove(p.OwnerClientId);
            else if (WatchesHide(p)) watchedHide.Add(p.OwnerClientId);
        }

        /// <summary>Host: is it after this player right now (charging, rushing, fixed on them)? Only then can it watch them hide.</summary>
        protected virtual bool Pursuing(NetworkPlayer p) => false;

        // It was after them, near enough, and had them in sight as they went in.
        private bool WatchesHide(NetworkPlayer p)
        {
            if (!Pursuing(p)) return false;
            Vector3 at = PositionOf(p);
            if (Vector3.Distance(at, transform.position) > Extraction.DoorConfig.Current.WitnessRange) return false;
            return LineOfSight(transform.position + Vector3.up * 1.5f, at + Vector3.up * 1.2f);
        }

        /// <summary>Host: does it know where this hiding player is? By default only if it watched them climb in.</summary>
        protected virtual bool FindsHider(NetworkPlayer p) => watchedHide.Contains(p.OwnerClientId);
        protected void RememberHider(NetworkPlayer p) { if (p.IsHiding) watchedHide.Add(p.OwnerClientId); }

        /// <summary>Host: hiding in a locker, and this threat has no idea.</summary>
        protected bool HiddenFromMe(NetworkPlayer p) => p.IsConcealed && (p.InVent || !FindsHider(p)); // Vents remain concealed; open cover and peeking use normal sight checks.

        /// <summary>
        /// Where to go for a player and reach them from: for one in a locker, the floor in front of its door
        /// (they stand inside something solid).
        /// </summary>
        protected static Vector3 SpotOf(NetworkPlayer p)
        {
            if (!p.IsHiding) return PositionOf(p);
            Extraction.HidingLocker locker = Extraction.HidingLocker.ByIndex(p.HidingIn);
            return locker != null ? locker.ExitPoint : PositionOf(p);
        }

        /// <summary>Host: kills the first living player within reach it can actually touch; returns them or null.</summary>
        protected NetworkPlayer KillWithinReach(float range) => DamageWithinReach(range, 1000f, true);

        /// <param name="senses">Optional: only players this threat can perceive (the Blind One must have heard them).</param>
        protected NetworkPlayer DamageWithinReach(float range, float fallbackDamage, bool lethal = false, System.Func<NetworkPlayer, bool> senses = null)
        {
            if (!IsServer || !IsSpawned || Time.time < nextAttack || Time.time < attackReadyAt) return null;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.NetworkManager != NetworkManager || p.IsDead) continue;
                if (HiddenFromMe(p)) continue;
                if (senses != null && !senses(p)) continue;
                // Someone it knows is in a locker is reached at the locker's door, and dragged out.
                bool hiding = p.IsHiding;
                if (hiding && !FindsHider(p) && !LineOfSight(transform.position + Vector3.up * 1.3f, p.SightPoint)) continue;
                Vector3 at = SpotOf(p);
                Vector3 d = at - transform.position;
                if (Mathf.Abs(d.y) > 1.8f || new Vector2(d.x, d.z).sqrMagnitude > range * range) continue;
                if (Sheltered(at)) continue;
                if (!LineOfSight(transform.position + Vector3.up * 1.3f, at + Vector3.up * 0.8f)) continue;
                if (hiding) p.ServerLeaveHiding();
                float damage = lethal || definition != null && definition.LethalContact ? p.MaxHealth : (definition != null ? definition.Damage : fallbackDamage) * Mathf.Min(Aggression, 1.4f) * ContractDamageScale;
                p.DealDamage(damage, DeathLine);
                if (p.IsDead) Kills++;
                nextAttack = Time.time + (definition != null ? definition.AttackCooldown : 2.5f) / Mathf.Min(Aggression, 1.6f);
                GetComponent<ThreatAnimationSync>()?.ServerAttack();
                Debug.Log($"[Threat] {DisplayName} struck player {p.OwnerClientId}.");
                return p;
            }
            return null;
        }

        /// <summary>Host: safe in the truck (see <see cref="Extraction.RunState.Shelters"/>: armored, or already leaving).</summary>
        protected static bool Sheltered(Vector3 at) => Extraction.RunState.Current != null && Extraction.RunState.Current.Shelters(at);

        // QA P-04: chasing monsters re-asked for a path every frame. Ask again only a few times a second, or
        // at once when the goal has moved a real distance.
        private const float RepathInterval = 0.25f, RepathDistance = 1.5f;
        private float nextRepath;
        private Vector3 lastGoal = new(float.NaN, 0f, 0f);

        /// <summary>Host: head for <paramref name="goal"/>, re-pathing at most a few times a second.</summary>
        protected void Steer(UnityEngine.AI.NavMeshAgent agent, Vector3 goal)
        {
            if (!agent.enabled || !agent.isOnNavMesh) return;
            bool moved = float.IsNaN(lastGoal.x) || (goal - lastGoal).sqrMagnitude > RepathDistance * RepathDistance;
            if (!moved && (Time.time < nextRepath || agent.pathPending)) return;
            nextRepath = Time.time + RepathInterval;
            lastGoal = goal;
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            // Keep an upstairs target upstairs: a wide nearest-point query can pick the floor below it.
            if (NavMesh.SamplePosition(goal, out NavMeshHit hit, 2f, filter) && Mathf.Abs(hit.position.y - goal.y) < 1.75f)
                agent.SetDestination(hit.position);
        }

        /// <summary>Patrol goals must be on the intended floor and connected to where the creature stands.</summary>
        protected bool ReachableGoal(NavMeshAgent agent, Vector3 guess, out Vector3 destination, float radius = 3f)
        {
            destination = default;
            if (!agent.enabled || !agent.isOnNavMesh) return false;
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (!NavMesh.SamplePosition(guess, out NavMeshHit hit, radius, filter) || Mathf.Abs(hit.position.y - guess.y) > 1.75f) return false;
            if (!agent.CalculatePath(hit.position, patrolPath) || patrolPath.status != NavMeshPathStatus.PathComplete) return false;
            destination = hit.position;
            return true;
        }

        private readonly RaycastHit[] sightHits = new RaycastHit[16];

        /// <summary>Nothing solid between two points (its own body doesn't count).</summary>
        protected bool LineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            float length = d.magnitude;
            if (length < 0.01f) return true;
            int n = Physics.RaycastNonAlloc(from, d / length, sightHits, length, WallMask, QueryTriggerInteraction.Ignore);
            if (n == sightHits.Length) return false;
            for (int i = 0; i < n; i++)
                if (!sightHits[i].collider.transform.IsChildOf(transform)) return false;
            return true;
        }

#if UNITY_EDITOR
        public void EditorSetupDefinition(ThreatDefinition value) => definition = value;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetThreats()
        {
            Spawned.Clear();
            ContractDamageScale = 1f;
        }
    }
}
