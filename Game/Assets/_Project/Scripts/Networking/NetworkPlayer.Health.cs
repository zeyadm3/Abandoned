using System;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    public partial class NetworkPlayer
    {
        [SerializeField] private PlayerHealthConfig healthConfig;
        private readonly NetworkVariable<float> health = new(100f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private float nextLandingReport;
        private bool trackingServerFall;
        private float serverFallPeak, serverFallWeight, previousCarriedWeight;

        public PlayerHealthConfig HealthConfig => healthConfig != null ? healthConfig : PlayerHealthConfig.Default;
        public float Health => health.Value;
        public float MaxHealth => HealthConfig.MaxHealth;
        public float Health01 => Mathf.Clamp01(Health / MaxHealth);
        public static event Action<NetworkPlayer, float, float> HealthChanged;

        private void SpawnHealth()
        {
            health.OnValueChanged += OnHealthChanged;
            if (IsServer) health.Value = MaxHealth;
        }

        private void OnHealthChanged(float before, float after) => HealthChanged?.Invoke(this, before, after);

        /// <summary>The single host damage entry point: hit amount and death cause never come from an owner RPC.</summary>
        public void DealDamage(float amount, string cause)
        {
            if (!IsServer || dead.Value || !float.IsFinite(amount) || amount <= 0f) return;
            health.Value = Mathf.Clamp(health.Value - amount, 0f, MaxHealth);
            if (health.Value <= 0f) Die(cause);
        }

        /// <summary>Host: heal a living player. Returns false when no consumable should be spent.</summary>
        public bool ServerHeal(float amount)
        {
            if (!IsServer || dead.Value || !float.IsFinite(amount) || amount <= 0f || health.Value >= MaxHealth) return false;
            health.Value = Mathf.Min(MaxHealth, health.Value + amount);
            return true;
        }

        private void ResetHealth()
        {
            health.Value = MaxHealth;
            deathCause.Value = default;
            trackingServerFall = false;
            serverFallWeight = 0f;
            previousCarriedWeight = 0f;
            nextLandingReport = Time.time + HealthConfig.LandingReportInterval;
            GetComponent<Equipment.PlayerEquipment>()?.ServerResetBattery();
        }

        private void TrackServerFall()
        {
            if (!IsServer || IsDead) return;
            bool inAir = ragdoll.IsRagdolled ? !ragdoll.IsBodyResting : !motor.IsGrounded;
            if (!inAir)
            {
                previousCarriedWeight = carrier != null ? carrier.CarriedWeight : 0f;
                return;
            }
            float y = ragdoll.IsRagdolled ? ragdoll.BodyPosition.y : transform.position.y;
            float weight = carrier != null ? carrier.CarriedWeight : 0f;
            if (!trackingServerFall)
            {
                trackingServerFall = true;
                serverFallPeak = y;
                // A collapse drops hand loot before the next frame; remember the load at its start.
                serverFallWeight = Mathf.Max(weight, previousCarriedWeight);
            }
            else
            {
                serverFallPeak = Mathf.Max(serverFallPeak, y);
                serverFallWeight = Mathf.Max(serverFallWeight, weight);
            }
        }

        private bool AcceptLanding(ref float fallHeight, ref float impactSpeed, Vector3 landingPosition)
        {
            if (!IsServer || IsDead || !float.IsFinite(fallHeight) || !float.IsFinite(impactSpeed) ||
                fallHeight < 0f || impactSpeed < 0f || Time.time < nextLandingReport) return false;
            // Owners simulate movement, but a landing must be near solid ground and may not replay every frame.
            Vector3 tracked = ragdoll.IsRagdolled ? ragdoll.BodyPosition : transform.position;
            if (!float.IsFinite(landingPosition.x) || !float.IsFinite(landingPosition.y) || !float.IsFinite(landingPosition.z) ||
                Vector3.Distance(tracked, landingPosition) > HealthConfig.LandingPoseTolerance) return false;
            Vector3 at = landingPosition;
            int groundMask = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");
            if (!Physics.Raycast(at + Vector3.up * 0.2f, Vector3.down, HealthConfig.LandingGroundTolerance + 0.2f,
                    groundMask, QueryTriggerInteraction.Ignore)) return false;
            nextLandingReport = Time.time + HealthConfig.LandingReportInterval;
            fallHeight = Mathf.Clamp(fallHeight, 0f, MaxReportedFall);
            impactSpeed = Mathf.Clamp(impactSpeed, 0f, MaxReportedImpactSpeed);
            if (trackingServerFall)
                fallHeight = Mathf.Min(fallHeight, Mathf.Max(0f, serverFallPeak - at.y) + 2f);
            // Reports cannot exceed the height implied by their capped impact speed plus a small step tolerance.
            float gravity = motor.Config.Gravity * motor.Config.FallGravityMultiplier;
            fallHeight = Mathf.Min(fallHeight, impactSpeed * impactSpeed / Mathf.Max(1f, 2f * gravity) + 1f);
            return true;
        }

        private void ApplyLandingDamage(float fallHeight)
        {
            float weight = Mathf.Max(serverFallWeight, carrier != null ? carrier.CarriedWeight : 0f);
            trackingServerFall = false;
            serverFallWeight = 0f;
            DealDamage(HealthConfig.FallDamage(fallHeight, weight), "Fell");
        }
    }
}
