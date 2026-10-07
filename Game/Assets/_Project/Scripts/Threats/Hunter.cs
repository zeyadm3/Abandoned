using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Hunter (GDD 9): patrols, chases whoever it sees, kills on contact. Counterplay: break line of
    /// sight, crouch to hide, outrun it while your stamina lasts, or lure it onto a weak floor: it's
    /// heavy (a load source on the structure), and when its floor gives way it falls and lies stunned.
    /// Host only; its state and heavy footsteps reach every machine.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Hunter : Threat, ILoadSource
    {
        [SerializeField] private HunterConfig config;

        private readonly NetworkVariable<HunterState> state = new();
        private NavMeshAgent agent;
        private NetworkPlayer target;
        private Vector3 lastSeen;
        private float lostAt, searchUntil, stunnedUntil, nextStep, fallSpeed;
        private bool falling;
        private Vector3 lastFooting;
        private StructuralSection standingOn;

        public override string DisplayName => "Hunter";
        public HunterState State => state.Value;
        public NetworkPlayer Target => target;
        public HunterConfig Config => config;
        /// <summary>Tests/F1: the section it last stood on.</summary>
        public StructuralSection StandingOn => standingOn;

        public float LoadWeight => IsServer && IsSpawned && !falling ? config.Weight : 0f;

        public void GetLoadPoints(List<LoadPoint> points) => points.Add(new LoadPoint(transform.position + Vector3.up * 0.2f));

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
            agent.enabled = true;
            agent.Warp(transform.position);
            LoadSources.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            LoadSources.Unregister(this);
        }

        private void Update()
        {
            if (IsSpawned) Footsteps();
            if (!IsServer || !IsSpawned) return;
            if (falling || state.Value == HunterState.Stunned) { Fall(); return; }
            if (FloorGaveWay()) { StartFall(); return; }
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position + Vector3.down * 4f, out NavMeshHit hit, 5f, NavMesh.AllAreas)) agent.Warp(hit.position);
                return;
            }
            bool active = RunState.Current == null || RunState.Current.State.Phase != RunPhase.Departed;
            if (!active) { agent.isStopped = true; return; }

            NetworkPlayer seen = Spotted();
            if (seen != null)
            {
                target = seen;
                lastSeen = PositionOf(seen);
                lostAt = Time.time;
                Set(HunterState.Chase);
            }
            switch (state.Value)
            {
                case HunterState.Chase:
                    agent.isStopped = false;
                    agent.speed = config.ChaseSpeed * SpeedScale;
                    agent.SetDestination(lastSeen);
                    if (KillWithinReach(config.AttackRange) != null) { target = null; Set(HunterState.Search); searchUntil = Time.time + config.SearchSeconds; }
                    else if (seen == null && Time.time - lostAt > config.LoseAfter) { Set(HunterState.Search); searchUntil = Time.time + config.SearchSeconds; }
                    break;
                case HunterState.Search:
                    agent.isStopped = false;
                    agent.speed = config.PatrolSpeed * SpeedScale;
                    if (!agent.pathPending && agent.remainingDistance < 1f) transform.Rotate(0f, 90f * Time.deltaTime, 0f); // looking around
                    if (agent.destination != lastSeen && Vector3.Distance(agent.destination, lastSeen) > 0.5f) agent.SetDestination(lastSeen);
                    if (Time.time > searchUntil) Set(HunterState.Patrol);
                    break;
                default:
                    agent.isStopped = false;
                    agent.speed = config.PatrolSpeed * SpeedScale;
                    if (!agent.pathPending && (agent.remainingDistance < 1f || !agent.hasPath)) PatrolSomewhere();
                    break;
            }
            // Walking into it is fatal too, not only being chased down.
            if (state.Value != HunterState.Chase) KillWithinReach(config.AttackRange);
        }

        private void Set(HunterState s)
        {
            if (state.Value != s) state.Value = s;
        }

        // The living player it sees best: in its cone, in range (crouchers only up close), in the open; or anyone right next to it.
        private NetworkPlayer Spotted()
        {
            NetworkPlayer best = null;
            float bestDistance = float.MaxValue;
            Vector3 eye = transform.position + Vector3.up * 2f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || p.IsDead || !p.IsSpawned) continue;
                Vector3 at = PositionOf(p) + Vector3.up * 1.2f;
                Vector3 to = at - eye;
                float distance = to.magnitude;
                bool close = distance < config.CloseSense;
                float range = p.State.Crouching ? config.CrouchSightRange : config.SightRange;
                if (!close && (distance > range || Vector3.Angle(transform.forward, new Vector3(to.x, 0f, to.z)) > config.SightAngle / 2f)) continue;
                if (!LineOfSight(eye, at)) continue;
                if (distance < bestDistance) { best = p; bestDistance = distance; }
            }
            return best;
        }

        private void PatrolSomewhere()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 r = Random.insideUnitCircle * config.PatrolRadius;
                Vector3 guess = transform.position + new Vector3(r.x, Random.Range(-4f, 4f), r.y);
                if (NavMesh.SamplePosition(guess, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        // The section it stood on collapsed. Checked on the section, not by looking down: the collapse carves
        // the NavMesh at once and the agent gets pushed out of the hole onto the next tile in the same frame.
        private bool FloorGaveWay()
        {
            if (standingOn != null && standingOn.IsCollapsed) return true;
            StructuralSection under = SectionQuery.Under(transform.position, 1f);
            if (under != null)
            {
                standingOn = under;
                lastFooting = transform.position;
            }
            return false;
        }

        private void StartFall()
        {
            // Back over the hole (the NavMesh may already have shoved it onto the neighbouring tile).
            transform.position = lastFooting;
            standingOn = null;
            falling = true;
            fallSpeed = 0f;
            agent.enabled = false;
            target = null;
            Set(HunterState.Stunned);
            stunnedUntil = Time.time + config.StunSeconds;
            Debug.Log("[Threat] The Hunter fell through the floor.");
        }

        // Down to whatever is below, then lie there; then find the NavMesh again and patrol.
        private void Fall()
        {
            if (falling)
            {
                fallSpeed = Mathf.Min(fallSpeed + 20f * Time.deltaTime, 30f);
                float step = fallSpeed * Time.deltaTime;
                if (Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, step + 0.3f, WallMask, QueryTriggerInteraction.Ignore))
                {
                    transform.position = hit.point;
                    falling = false;
                    stunnedUntil = Time.time + config.StunSeconds;
                    standingOn = hit.collider.GetComponentInParent<StructuralSection>();
                    CameraShake.Emit(hit.point, config.Weight * fallSpeed);
                    LandedRpc(hit.point);
                }
                else transform.position += Vector3.down * step;
                return;
            }
            if (Time.time < stunnedUntil) return;
            agent.enabled = true;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit nav, 3f, NavMesh.AllAreas)) agent.Warp(nav.position);
            Set(HunterState.Search);
            lastSeen = transform.position;
            searchUntil = Time.time + config.SearchSeconds;
        }

        [Rpc(SendTo.Everyone)]
        private void LandedRpc(Vector3 at) => Audio.GameAudio.Play(Audio.SoundId.HunterLanding, at, 1f);

        // Every machine: heavy steps, quicker when it charges (its signature, GDD 9: one clear rule).
        private void Footsteps()
        {
            HunterState s = state.Value;
            if (s == HunterState.Stunned || Time.time < nextStep) return;
            nextStep = Time.time + (s == HunterState.Chase ? 0.32f : 0.62f);
            Audio.GameAudio.Play(Audio.SoundId.HunterStep, transform.position, s == HunterState.Chase ? 1f : 0.75f);
        }

#if UNITY_EDITOR
        public void EditorSetup(HunterConfig hunterConfig) => config = hunterConfig;
#endif
    }
}
