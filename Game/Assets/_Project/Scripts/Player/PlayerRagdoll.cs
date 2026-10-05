using System;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Switches the player between controlled movement and a physics ragdoll: on big falls, heavy
    /// hits, or the K debug key. Local only (not networked); others see the synced root.
    /// Gets back up where the body settles.
    /// </summary>
    public class PlayerRagdoll : MonoBehaviour
    {
        [SerializeField] private PlayerRagdollConfig config;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform cameraRoot;
        [Tooltip("Normal (non-ragdoll) body visual, hidden while ragdolled.")]
        [SerializeField] private GameObject body;
        [Tooltip("Inactive container holding the ragdoll parts.")]
        [SerializeField] private Transform ragdollRoot;
        [SerializeField] private Rigidbody pelvis;
        [SerializeField] private Transform head;
        [Tooltip("Switched off while ragdolled: movement, look, interaction.")]
        [SerializeField] private Behaviour[] disableWhileRagdolled;

        private const float GroundProbeHeight = 1.5f;

        private CharacterController controller;
        private Rigidbody[] parts;
        private Vector3[] partPositions;
        private Quaternion[] partRotations;
        private Vector3 cameraRootDefault;
        private float ragdollTime;

        public bool IsRagdolled { get; private set; }
        public Transform Head => head;
        public Rigidbody Pelvis => pelvis;
        public PlayerRagdollConfig Config => config;

        /// <summary>Optional filter for hits (e.g. ignore the object we're carrying). Return false to ignore.</summary>
        public Predicate<Rigidbody> HitFilter { get; set; }

        public event Action Started;
        public event Action Ended;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            parts = ragdollRoot.GetComponentsInChildren<Rigidbody>(true);
            partPositions = new Vector3[parts.Length];
            partRotations = new Quaternion[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                partPositions[i] = parts[i].transform.localPosition;
                partRotations[i] = parts[i].transform.localRotation;
            }
            cameraRootDefault = cameraRoot.localPosition;
            ragdollRoot.gameObject.SetActive(false);
        }

        private void OnEnable() => motor.Landed += OnLanded;

        private void OnDisable() => motor.Landed -= OnLanded;

        private void Update()
        {
            if (inputReader != null && inputReader.isActiveAndEnabled && inputReader.Current.DebugRagdollPressed)
            {
                if (IsRagdolled) Recover();
                else Enter(motor.MovementVelocity);
            }
            if (IsRagdolled) Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (IsRagdolled) cameraRoot.position = head.position;
        }

        /// <summary>Advances recovery timing; public so tests can step it.</summary>
        public void Tick(float dt)
        {
            ragdollTime += dt;
            bool settled = ragdollTime >= config.MinRagdollTime && pelvis.linearVelocity.magnitude < config.SettledSpeed;
            if (settled || ragdollTime >= config.MaxRagdollTime) Recover();
        }

        private void OnLanded(float fallHeight, float impactSpeed)
        {
            if (fallHeight > config.FallHeight) Enter(Vector3.down * impactSpeed);
        }

        /// <summary>Called by the hit detector when a moving rigidbody touches the player.</summary>
        public void ReportHit(Rigidbody other, float weight, Vector3 otherVelocity)
        {
            if (IsRagdolled || (HitFilter != null && !HitFilter(other))) return;
            Vector3 toPlayer = transform.position + Vector3.up - other.worldCenterOfMass;
            Vector3 relative = otherVelocity - motor.MovementVelocity;
            float closing = toPlayer.sqrMagnitude > 1e-4f ? Vector3.Dot(relative, toPlayer.normalized) : relative.magnitude;
            if (closing < config.MinHitSpeed || weight * closing < config.HitMomentum) return;
            Enter(otherVelocity * config.HitVelocityTransfer);
        }

        public void Enter(Vector3 velocity)
        {
            if (IsRagdolled) return;
            IsRagdolled = true;
            ragdollTime = 0f;
            foreach (Behaviour b in disableWhileRagdolled) if (b != null) b.enabled = false;
            controller.enabled = false;
            body.SetActive(false);

            ragdollRoot.localPosition = Vector3.zero;
            ragdollRoot.localRotation = Quaternion.identity;
            for (int i = 0; i < parts.Length; i++)
                parts[i].transform.SetLocalPositionAndRotation(partPositions[i], partRotations[i]);
            ragdollRoot.gameObject.SetActive(true);
            Physics.SyncTransforms();
            foreach (Rigidbody part in parts)
            {
                part.linearVelocity = velocity;
                part.angularVelocity = Vector3.zero;
            }
            Started?.Invoke();
        }

        public void Recover()
        {
            if (!IsRagdolled) return;
            Vector3 p = pelvis.position;
            int mask = ~((1 << Mathf.Max(0, GameLayers.PlayerLayer)) | (1 << Mathf.Max(0, GameLayers.DebrisLayer)));
            float y = Physics.Raycast(p + Vector3.up * GroundProbeHeight, Vector3.down, out RaycastHit hit,
                GroundProbeHeight + 3f, mask, QueryTriggerInteraction.Ignore)
                ? hit.point.y
                : p.y - 0.9f;

            ragdollRoot.gameObject.SetActive(false);
            transform.position = new Vector3(p.x, y + 0.05f, p.z);
            Physics.SyncTransforms();
            controller.enabled = true;
            body.SetActive(true);
            cameraRoot.localPosition = cameraRootDefault;
            motor.ResetFallTracking();
            foreach (Behaviour b in disableWhileRagdolled) if (b != null) b.enabled = true;
            IsRagdolled = false;
            Ended?.Invoke();
        }
    }
}
