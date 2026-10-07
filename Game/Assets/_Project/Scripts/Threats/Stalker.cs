using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Stalker (GDD 9): follows the most isolated player at a distance and stares; while that player
    /// looks at it, it doesn't move; if they're alone and look away too long, it rushes and kills on
    /// contact. Counterplay: stay grouped, keep checking behind you. Host only; state replicated.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Stalker : Threat
    {
        [SerializeField] private StalkerConfig config;

        private readonly NetworkVariable<StalkerState> state = new();
        private NavMeshAgent agent;
        private StalkerBrain brain;
        private NetworkPlayer target;
        private float nextRetarget, nextBreath;

        public override string DisplayName => "Stalker";
        public StalkerState State => state.Value;
        public NetworkPlayer Target => target;
        public StalkerBrain Brain => brain;

        protected override void Awake()
        {
            base.Awake();
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            brain = new StalkerBrain(config);
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        private void Update()
        {
            if (IsSpawned) Breathe();
            if (!IsServer || !IsSpawned || brain == null) return;
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position + Vector3.down * 4f, out NavMeshHit hit, 5f, NavMesh.AllAreas)) agent.Warp(hit.position);
                return;
            }
            bool active = RunState.Current == null || RunState.Current.State.Phase != RunPhase.Departed;
            if (Time.time >= nextRetarget || target == null || target.IsDead) PickTarget();
            if (target == null || !active)
            {
                agent.isStopped = true;
                return;
            }

            Vector3 at = PositionOf(target);
            brain.Config = config;
            // Dread only builds while it has eyes on its prey: behind walls or a floor away it just follows.
            brain.Tick(IsWatchedBy(target), IsIsolated(target) && Sees(target), Time.deltaTime);
            state.Value = brain.State;
            switch (brain.State)
            {
                case StalkerState.Frozen:
                    agent.isStopped = true;
                    break;
                case StalkerState.Follow:
                    agent.isStopped = false;
                    agent.speed = config.FollowSpeed * SpeedScale;
                    // Hang back: walk to a point FollowDistance short of them.
                    Vector3 away = (transform.position - at).normalized;
                    if (away.sqrMagnitude < 0.01f) away = -target.transform.forward;
                    agent.SetDestination(at + away * config.FollowDistance);
                    break;
                case StalkerState.Rush:
                    agent.isStopped = false;
                    agent.speed = config.RushSpeed * SpeedScale;
                    agent.SetDestination(at);
                    // Prey it can't reach (at the truck, off the NavMesh): give up rather than camp the doorway.
                    if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete) brain.Reset();
                    else if (KillWithinReach(config.AttackRange) != null) brain.Reset();
                    break;
            }
            // Frozen, it keeps staring at its target (moving, the agent turns it).
            if (brain.State == StalkerState.Frozen && Flat(at - transform.position).sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(Flat(at - transform.position));
        }

        // The most alone living player (the one furthest from their nearest teammate).
        private void PickTarget()
        {
            nextRetarget = Time.time + config.RetargetInterval;
            var alive = NetworkPlayer.All.Where(p => p != null && p.NetworkManager == NetworkManager && !p.IsDead).ToList();
            // Two players are always equally far from each other: then the nearer one to it.
            NetworkPlayer picked = alive
                .OrderByDescending(p => alive.Where(o => o != p).Select(o => Vector3.Distance(PositionOf(o), PositionOf(p))).DefaultIfEmpty(float.MaxValue).Min())
                .ThenBy(p => Vector3.Distance(PositionOf(p), transform.position))
                .FirstOrDefault();
            // A new prey hasn't been stalked yet: the dread built on someone else doesn't carry over.
            if (picked != target) brain.Reset();
            target = picked;
        }

        private bool Sees(NetworkPlayer p) => LineOfSight(transform.position + Vector3.up * 1.5f, PositionOf(p) + Vector3.up * 1.2f);

        private bool IsIsolated(NetworkPlayer p) =>
            !NetworkPlayer.All.Any(o => o != null && o != p && o.NetworkManager == NetworkManager && !o.IsDead
                                        && Vector3.Distance(PositionOf(o), PositionOf(p)) < config.IsolationRadius);

        // Watching = facing it (horizontally) within the angle, near enough, with nothing in between.
        public bool IsWatchedBy(NetworkPlayer p)
        {
            Vector3 eye = PositionOf(p) + Vector3.up * 1.6f, chest = transform.position + Vector3.up * 1.5f;
            Vector3 to = chest - eye;
            if (to.magnitude > config.WatchRange) return false;
            if (Vector3.Angle(Flat(p.transform.forward), Flat(to)) > config.WatchAngle) return false;
            return LineOfSight(eye, chest);
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        // Every machine: a slow breath when it's close behind you.
        private void Breathe()
        {
            if (Time.time < nextBreath) return;
            nextBreath = Time.time + (state.Value == StalkerState.Rush ? 0.6f : 2.2f);
            AudioSource.PlayClipAtPoint(Audio.PlaceholderAudio.GetStructureClip(Audio.StructureSound.Creak), transform.position + Vector3.up * 1.8f, 0.35f);
        }
    }
}
