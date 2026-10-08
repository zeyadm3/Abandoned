using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Blind One (GDD 9): can't see, hunts by sound, kills on contact. Runs only on the host (NavMesh
    /// agent, hearing, kills); everyone sees its replicated position and state and hears its clicking,
    /// faster when it hunts. Counterplay: move slowly, crouch, don't drop things, whisper.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BlindOne : Threat
    {
        private const float ArrivedDistance = 1.2f, RepathInterval = 0.25f;
        // Walls between its head and a noise: anything solid except players, loot and debris.
        private const int MaxWallHits = 6;

        [SerializeField] private BlindOneConfig config;

        private readonly NetworkVariable<BlindOneState> state = new();
        private readonly RaycastHit[] wallHits = new RaycastHit[MaxWallHits];
        private readonly List<(Vector3 position, float strength, float time)> heard = new();
        private NavMeshAgent agent;
        private BlindOneBrain brain;
        private int wallMask;
        private float nextRepath, nextClick;
        private Vector3 wanderTarget, lastGoal;
        private bool hasWanderTarget, hasGoal;

        /// <summary>Just the Blind Ones (Threat.All has every threat).</summary>
        public static new readonly List<BlindOne> All = new();

        public override string DisplayName => "Blind One";
        public override string DeathLine => "The Blind One heard you.";

        public BlindOneState State => state.Value;
        public override ThreatMotion DesiredMotion => State == BlindOneState.Attack ? ThreatMotion.Attack : State == BlindOneState.Hunt ? ThreatMotion.Chase : State == BlindOneState.Investigate ? ThreatMotion.Special : ThreatMotion.Idle;
        public BlindOneConfig Config => config;
        public IReadOnlyList<(Vector3 position, float strength, float time)> Heard => heard;
        public BlindOneBrain Brain => brain;

        protected override void Awake()
        {
            base.Awake();
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
            wallMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            All.Add(this);
            if (!IsServer) return;
            brain = new BlindOneBrain(config);
            agent.enabled = true;
            agent.Warp(transform.position);
            NoiseSystem.Emitted += OnNoise;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            All.Remove(this);
            NoiseSystem.Emitted -= OnNoise;
        }

        private void OnNoise(NoiseEvent e)
        {
            Vector3 ear = transform.position + Vector3.up * 1.8f;
            float reach = NoiseSystem.RadiusOf(e) * config.Hearing * HearingScale;
            if ((e.Position - ear).sqrMagnitude > reach * reach) return;
            float strength = Hearing.Perceive(e, ear, Walls(ear, e.Position), config.Hearing * HearingScale, config.WallDamping);
            if (strength <= 0f) return;
            heard.Add((e.Position, strength, Time.time));
            brain.Hear(e.Position, strength, Time.time);
        }

        private int Walls(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            float length = d.magnitude;
            return length < 0.01f ? 0 : Physics.RaycastNonAlloc(from, d / length, wallHits, length, wallMask, QueryTriggerInteraction.Ignore);
        }

        private void Update()
        {
            if (IsSpawned) Click();
            if (!IsServer || !IsSpawned || brain == null) return;
            heard.RemoveAll(h => Time.time - h.time > config.Memory);
            if (!agent.isOnNavMesh)
            {
                RecoverOntoNavMesh();
                return;
            }

            Vector3 goal = brain.State == BlindOneState.Wander || !brain.HasTarget ? wanderTarget : brain.Target;
            // "Arrived" only counts for the goal it is walking to now, not the one before it heard something.
            bool arrived = hasGoal && (goal - lastGoal).sqrMagnitude < 1f && !agent.pathPending && agent.remainingDistance <= ArrivedDistance;
            brain.Tick(transform.position, arrived, Time.time);
            if (brain.State == BlindOneState.Wander && (arrived || !hasWanderTarget)) PickWanderTarget();
            agent.speed = brain.Speed * SpeedScale;
            goal = brain.State == BlindOneState.Wander || !brain.HasTarget ? wanderTarget : brain.Target;
            bool newGoal = !hasGoal || (goal - lastGoal).sqrMagnitude >= 1f;
            if (newGoal || Time.time >= nextRepath)
            {
                nextRepath = Time.time + RepathInterval;
                if (NavMesh.SamplePosition(goal, out NavMeshHit hit, 3f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
                lastGoal = goal;
                hasGoal = true;
            }
            if (RunActive) KillOnContact();
            if (state.Value != brain.State) state.Value = brain.State;
        }

        // The truck has left: it stops hunting (appraisal, extracted players in the bay).
        private static bool RunActive => RunState.Current == null || RunState.Current.State.Phase != RunPhase.Departed;

        // The floor under it collapsed (its NavMesh is carved away): it drops to whatever is below.
        private void RecoverOntoNavMesh()
        {
            Vector3 below = transform.position + Vector3.down * 4f;
            if (NavMesh.SamplePosition(below, out NavMeshHit hit, 5f, NavMesh.AllAreas) ||
                NavMesh.SamplePosition(transform.position, out hit, 6f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                hasGoal = false;
                Debug.Log($"[Threat] The Blind One fell to {hit.position}.");
            }
        }

        private void PickWanderTarget()
        {
            Vector3 random = transform.position + Random.insideUnitSphere * config.WanderRadius;
            if (!NavMesh.SamplePosition(random, out NavMeshHit hit, config.WanderRadius, NavMesh.AllAreas)) return;
            wanderTarget = hit.position;
            hasWanderTarget = true;
        }

        private void KillOnContact()
        {
            if (brain.State == BlindOneState.Attack) return;
            if (DamageWithinReach(config.AttackRange, 65f, senses: HeardRecently) != null) brain.Attacked(Time.time);
        }

        // QA D-07: it's blind. Someone it hasn't heard near where they stand (crouch-walking, holding still,
        // not talking) can slip past within arm's reach; one sound there and they're fair game.
        private const float HeardNearRadius = 3f, HeardWithinSeconds = 2f;

        private bool HeardRecently(NetworkPlayer p)
        {
            Vector3 at = PositionOf(p);
            foreach ((Vector3 position, float _, float time) in heard)
                if (Time.time - time <= HeardWithinSeconds && (position - at).sqrMagnitude <= HeardNearRadius * HeardNearRadius) return true;
            return false;
        }

        // Every machine: its clicking is how players know it's near (faster = hunting).
        private void Click()
        {
            if (Time.time < nextClick) return;
            BlindOneState s = state.Value;
            nextClick = Time.time + (s == BlindOneState.Hunt ? config.HuntClickInterval : config.ClickInterval) * Random.Range(0.85f, 1.15f);
            if (s != BlindOneState.Attack) GameAudio.Play(SoundId.BlindOneClick, transform.position + Vector3.up * 2f, s == BlindOneState.Hunt ? 1f : 0.7f);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.8f);
            if (screen.z <= 0f) return;
            string extra = brain != null ? $" pull {brain.CurrentPull(Time.time):0.00} heard {heard.Count}" : "";
            GUI.Label(new Rect(screen.x - 120f, Screen.height - screen.y - 12f, 240f, 24f), $"BLIND ONE {state.Value}{extra}");
        }
#endif

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            foreach (var h in heard)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f, Mathf.Clamp01(h.strength));
                Gizmos.DrawLine(transform.position + Vector3.up * 2f, h.position);
            }
            if (brain != null && brain.HasTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(brain.Target, 0.5f);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => All.Clear();
    }
}
