using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>Shared navigation for the added creatures. Clients never enable their NavMeshAgent.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class RoamingThreat : Threat
    {
        protected NavMeshAgent Agent { get; private set; }
        protected float WalkSpeed => Definition != null ? Definition.WalkSpeed : 1.8f;
        protected float ChaseSpeed => Definition != null ? Definition.ChaseSpeed : 4.8f;
        protected float Reach => Definition != null ? Definition.AttackRange : 1.25f;
        protected float SenseRange => Definition != null ? Definition.SenseRange * Mathf.Min(Aggression, 1.5f) : 22f;
        protected float Interval => (Definition != null ? Definition.SpecialInterval : 8f) / Mathf.Min(Aggression, 1.5f);
        protected float Momentum => Definition != null ? Definition.StructuralMomentum * Aggression : 1800f;

        protected override void Awake()
        {
            base.Awake();
            Agent = GetComponent<NavMeshAgent>();
            Agent.enabled = false;
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            Agent.enabled = true;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 8f, NavMesh.AllAreas)) Agent.Warp(hit.position);
        }
        protected virtual void Update()
        {
            if (!IsServer || !IsSpawned) return;
            if (RunState.Current != null && RunState.Current.State.Phase == RunPhase.Departed) { Stop(); return; }
            if (!Agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position + Vector3.down * 3f, out NavMeshHit hit, 8f, NavMesh.AllAreas)) Agent.Warp(hit.position);
                return;
            }
            HostTick();
        }
        protected abstract void HostTick();
        protected void Stop() { if (Agent.enabled && Agent.isOnNavMesh) Agent.isStopped = true; }
        protected void Move(Vector3 to, float speed)
        {
            if (!Agent.isOnNavMesh) return;
            Agent.isStopped = false;
            Agent.speed = speed * SpeedScale;
            if (NavMesh.SamplePosition(to, out NavMeshHit hit, 4f, NavMesh.AllAreas)) Agent.SetDestination(hit.position);
        }
        protected void Wander()
        {
            if (!Agent.pathPending && (!Agent.hasPath || Agent.remainingDistance < 1f))
                Move(transform.position + Random.insideUnitSphere * 12f, WalkSpeed);
        }
        protected NetworkPlayer Nearest(float range = float.MaxValue)
        {
            NetworkPlayer best = null;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.NetworkManager != NetworkManager || p.IsDead || Sheltered(PositionOf(p))) continue;
                float d = Vector3.Distance(PositionOf(p), transform.position);
                if (d < range) { best = p; range = d; }
            }
            return best;
        }
        protected bool Sees(NetworkPlayer p) => LineOfSight(transform.position + Vector3.up * 1.3f, PositionOf(p) + Vector3.up * 1.1f);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            Camera camera = Camera.main;
            if (camera == null) return;
            Vector3 at = camera.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            if (at.z > 0f) GUI.Label(new Rect(at.x - 110f, Screen.height - at.y, 240f, 22f), $"{DisplayName}: {DesiredMotion} x{Aggression:0.0}");
        }
#endif
        private void OnDrawGizmosSelected()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Reach);
        }
    }
}
