using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>Only the overstay phase spawns it. It knows where living crew are and breaks obstructions; the truck is sanctuary.</summary>
    public class LastHunter : RoamingThreat
    {
        private StructuralSection[] sections;
        private readonly NetworkVariable<bool> hunting = new();
        private float nextRupture, wakesAt;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) wakesAt = Time.time + (Definition != null ? Definition.SpawnWarningSeconds : 8f);
        }
        private Bounds huntBounds;
        public void SetHuntBounds(Bounds bounds) => huntBounds = bounds;
        public override string DisplayName => "The Last Hunter";
        public override string DeathLine => "You stayed too long.";
        public override ThreatMotion DesiredMotion => hunting.Value ? ThreatMotion.Chase : ThreatMotion.Special;
        protected override void HostTick()
        {
            if (Time.time < wakesAt) { hunting.Value = false; Stop(); return; }
            NetworkPlayer prey = null;
            float nearest = float.MaxValue;
            foreach (NetworkPlayer candidate in NetworkPlayer.All)
            {
                if (candidate == null || candidate.IsDead || candidate.NetworkManager != NetworkManager || !huntBounds.Contains(PositionOf(candidate)) || Sheltered(PositionOf(candidate))) continue;
                float distance = Vector3.Distance(transform.position, PositionOf(candidate));
                if (distance < nearest) { nearest = distance; prey = candidate; }
            }
            hunting.Value = prey != null;
            if (prey == null) { Stop(); return; }
            Move(PositionOf(prey), ChaseSpeed);
            KillWithinReach(Reach);
            if (Time.time < nextRupture) return;
            nextRupture = Time.time + Interval;
            // QA P-05: the level's sections are found once per monster, not every few seconds.
            sections ??= FindObjectsByType<StructuralSection>(FindObjectsSortMode.None);
            foreach (StructuralSection section in sections)
                if (section != null && section.CanCollapse && !section.IsCollapsed && Vector3.Distance(section.transform.position, transform.position + transform.forward * 1.5f) < 3f)
                    section.ApplyImpact(Momentum);
            // A collapsed route cannot permanently imprison it: the host takes a nearby connected floor, never past the prey.
            if (!Agent.pathPending && Agent.pathStatus != NavMeshPathStatus.PathComplete && Agent.remainingDistance < 1.5f)
            {
                Vector3 toward = Vector3.MoveTowards(transform.position, PositionOf(prey), 3.5f);
                if (Vector3.Distance(toward, PositionOf(prey)) > 3f && NavMesh.SamplePosition(toward, out NavMeshHit hit, 2f, NavMesh.AllAreas)) Agent.Warp(hit.position);
            }
        }
    }
}
