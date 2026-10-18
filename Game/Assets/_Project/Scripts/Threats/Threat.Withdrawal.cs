using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    public abstract partial class Threat
    {
        private float encounterBegan = -1f, withdrawalEnds, nextWithdrawalPick, nextIdlePick;
        private Vector3 withdrawalOrigin, withdrawalGoal;
        private bool hasWithdrawalGoal;

        /// <summary>Host: this creature is leaving an encounter; pacing must let it leave.</summary>
        public bool IsWithdrawing => Time.time < withdrawalEnds;
        public float WithdrawalRemaining => Mathf.Max(0f, withdrawalEnds - Time.time);
        public Vector3? WithdrawalDestination => IsWithdrawing && hasWithdrawalGoal ? withdrawalGoal : null;

        /// <summary>Bounds ordinary encounters, including ones repeatedly refreshed by sight or noise.</summary>
        protected bool TickWithdrawal(NavMeshAgent agent, float speed, bool engaged, Vector3 from)
        {
            if (!IsWithdrawing)
            {
                if (!engaged) { encounterBegan = -1f; return false; }
                if (encounterBegan < 0f) encounterBegan = Time.time;
                if (Time.time - encounterBegan < ThreatPacingConfig.Current.MaxEncounterSeconds) return false;
                BeginWithdrawal(agent, from);
            }
            if (!agent.enabled || !agent.isOnNavMesh) return true;
            agent.isStopped = false;
            agent.speed = speed * SpeedScale;
            bool arrived = !agent.pathPending && (!agent.hasPath || agent.remainingDistance < 1.25f
                || agent.pathStatus != NavMeshPathStatus.PathComplete);
            if ((!hasWithdrawalGoal || arrived) && Time.time >= nextWithdrawalPick)
            {
                nextWithdrawalPick = Time.time + 1f;
                PickWithdrawalGoal(agent);
            }
            if (hasWithdrawalGoal) Steer(agent, withdrawalGoal);
            return true;
        }

        protected void BeginWithdrawal(NavMeshAgent agent, Vector3 from)
        {
            if (IsWithdrawing) return;
            withdrawalEnds = Time.time + ThreatPacingConfig.Current.RecoverySeconds;
            withdrawalOrigin = from;
            encounterBegan = -1f;
            hasWithdrawalGoal = false;
            nextWithdrawalPick = 0f;
            RoamHint = null;
            watchedHide.Clear();
            if (agent.enabled && agent.isOnNavMesh) agent.ResetPath();
        }

        // A complete path away from the encounter, preferably around a corner. Avoid retreating into
        // another crew member: their positions are used only to choose an escape route, never a target.
        private void PickWithdrawalGoal(NavMeshAgent agent)
        {
            float radius = ThreatPacingConfig.Current.WithdrawalDistance;
            float best = float.NegativeInfinity;
            hasWithdrawalGoal = false;
            for (int i = 0; i < 24; i++)
            {
                // Smaller fallback steps let it get out of cramped rooms before choosing a long route.
                float stepRadius = i < 16 ? radius : radius * 0.35f;
                Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(stepRadius * 0.5f, stepRadius);
                Vector3 guess = transform.position + new Vector3(offset.x, 0f, offset.y);
                if (!ReachableGoal(agent, guess, out Vector3 destination)) continue;
                if ((destination - transform.position).sqrMagnitude < 9f) continue;
                float separation = Vector3.Distance(destination, withdrawalOrigin);
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.IsSpawned && p.NetworkManager == NetworkManager && !p.IsDead)
                        separation = Mathf.Min(separation, Vector3.Distance(destination, PositionOf(p)));
                float score = separation;
                if (separation < radius * 0.35f) score -= radius;
                if (!LineOfSight(withdrawalOrigin + Vector3.up * 1.3f, destination + Vector3.up * 1.3f)) score += radius * 0.5f;
                if (score <= best) continue;
                best = score;
                withdrawalGoal = destination;
                hasWithdrawalGoal = true;
            }
        }

        protected void IdleRoam(NavMeshAgent agent, float speed, float radius)
        {
            agent.isStopped = false;
            agent.speed = speed * SpeedScale;
            if (agent.pathPending || Time.time < nextIdlePick
                || agent.hasPath && agent.remainingDistance > 1f && agent.pathStatus == NavMeshPathStatus.PathComplete) return;
            nextIdlePick = Time.time + 0.75f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                if (!ReachableGoal(agent, RoamCentre + new Vector3(offset.x, 0f, offset.y), out Vector3 destination)) continue;
                Steer(agent, destination);
                return;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || !IsServer || !IsWithdrawing || Camera.main == null) return;
            Vector3 at = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
            if (at.z > 0f) GUI.Label(new Rect(at.x - 100f, Screen.height - at.y, 240f, 22f), $"WITHDRAWING {WithdrawalRemaining:0}s");
        }
#endif
    }
}
