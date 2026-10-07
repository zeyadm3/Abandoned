using System;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Switches the player between controlled movement and a physics ragdoll: on big falls, heavy
    /// hits, or the K debug key. The physics ragdoll is local only (not networked): on other machines
    /// the player is "remote" and just lays its capsule down where the owner's body is (replicated).
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
        private Vector3 bodyLocalPosition;
        private Quaternion bodyLocalRotation;
        private float ragdollTime;
        private Vector3 remoteBodyPosition;
        private bool remoteResting;
        // Whether the remote copy's controller was on when its owner went down, so getting up restores
        // exactly that (something else, e.g. a test world, may have switched it off on purpose).
        private bool remoteControllerWasEnabled;

        public bool IsRagdolled { get; private set; }

        /// <summary>Another machine's player: never ragdolls itself, mirrors the owner's state instead.</summary>
        public bool IsRemote { get; private set; }

        /// <summary>Dead: stay down (no getting up on its own) until something calls <see cref="Recover"/>.</summary>
        public bool HoldDown { get; set; }

        /// <summary>Where the fallen body is (only meaningful while ragdolled).</summary>
        public Vector3 BodyPosition => IsRemote ? remoteBodyPosition : pelvis.position;

        /// <summary>The fallen body has stopped tumbling and lies on something.</summary>
        public bool IsBodyResting => IsRemote
            ? remoteResting
            : pelvis.linearVelocity.sqrMagnitude < config.RestingSpeed * config.RestingSpeed;
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
            bodyLocalPosition = body.transform.localPosition;
            bodyLocalRotation = body.transform.localRotation;
            ragdollRoot.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            motor.Landed += OnLanded;
            StructureSignals.SectionCollapsed += OnSectionCollapsed;
        }

        private void OnDisable()
        {
            motor.Landed -= OnLanded;
            StructureSignals.SectionCollapsed -= OnSectionCollapsed;
        }

        /// <summary>Turns this copy into another machine's player (network spawn, non-owner).</summary>
        public void MakeRemote()
        {
            if (IsRagdolled) Recover();
            IsRemote = true;
        }

        /// <summary>Mirrors the owner's ragdoll: the capsule lies where their body is.</summary>
        public void ApplyRemoteState(bool ragdolled, Vector3 bodyPosition, bool resting)
        {
            if (!IsRemote) return;
            remoteBodyPosition = bodyPosition;
            remoteResting = resting;
            // The owner's root stays where they fell while only the ragdoll moves; a standing collider
            // left there would be an invisible pillar others bump into and loot rests on.
            if (ragdolled && !IsRagdolled)
            {
                remoteControllerWasEnabled = controller.enabled;
                controller.enabled = false;
            }
            else if (!ragdolled && IsRagdolled)
                controller.enabled = remoteControllerWasEnabled;
            IsRagdolled = ragdolled;
            Transform t = body.transform;
            if (ragdolled)
                // Capsule on its side, centred on the pelvis, lying along the player's facing.
                t.SetPositionAndRotation(bodyPosition, Quaternion.Euler(90f, transform.eulerAngles.y, 0f));
            else
                t.SetLocalPositionAndRotation(bodyLocalPosition, bodyLocalRotation);
        }

        private void OnSectionCollapsed(Bounds surface)
        {
            if (IsRemote) return;
            // Standing on it when it goes: you go down with it (GDD 15: ragdoll from collapses).
            Vector3 feet = transform.position;
            float m = config.CollapseMargin;
            bool above = feet.x >= surface.min.x - m && feet.x <= surface.max.x + m &&
                         feet.z >= surface.min.z - m && feet.z <= surface.max.z + m &&
                         feet.y >= surface.max.y - 0.3f && feet.y <= surface.max.y + config.CollapseStandingBand;
            if (above) Enter(Vector3.down * config.CollapseDropSpeed + motor.MovementVelocity);
        }

        private void Update()
        {
            if (IsRemote) return;
            if (inputReader != null && inputReader.isActiveAndEnabled && inputReader.Current.DebugRagdollPressed)
            {
                if (IsRagdolled) Recover();
                else Enter(motor.MovementVelocity);
            }
            if (IsRagdolled) Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (IsRagdolled && !IsRemote) cameraRoot.position = head.position;
        }

        /// <summary>Advances recovery timing; public so tests can step it.</summary>
        public void Tick(float dt)
        {
            ragdollTime += dt;
            if (HoldDown) return;
            bool settled = ragdollTime >= config.MinRagdollTime && pelvis.linearVelocity.magnitude < config.SettledSpeed;
            if (settled || ragdollTime >= config.MaxRagdollTime) Recover();
        }

        private void OnLanded(float fallHeight, float impactSpeed)
        {
            if (IsRemote) return;
            if (fallHeight > config.FallHeight) Enter(Vector3.down * impactSpeed);
        }

        /// <summary>Called by the hit detector when a moving rigidbody touches the player.</summary>
        public void ReportHit(Rigidbody other, float weight, Vector3 otherVelocity)
        {
            if (IsRemote || IsRagdolled || (HitFilter != null && !HitFilter(other))) return;
            // Only the object's own motion toward us counts: walking into a resting safe is not "being hit".
            Vector3 toPlayer = transform.position + Vector3.up - other.worldCenterOfMass;
            float closing = toPlayer.sqrMagnitude > 1e-4f ? Vector3.Dot(otherVelocity, toPlayer.normalized) : otherVelocity.magnitude;
            if (closing < config.MinHitSpeed || weight * closing < config.HitMomentum) return;
            Enter(otherVelocity * config.HitVelocityTransfer);
        }

        public void Enter(Vector3 velocity)
        {
            if (IsRagdolled || IsRemote) return;
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

        private static readonly Vector3[] RecoverOffsets =
        {
            Vector3.zero, new(0.6f, 0, 0), new(-0.6f, 0, 0), new(0, 0, 0.6f), new(0, 0, -0.6f),
            new(1.2f, 0, 0), new(-1.2f, 0, 0), new(0, 0, 1.2f), new(0, 0, -1.2f),
        };

        /// <summary>Floor under the body (ignoring loot and debris) where the standing capsule fits.</summary>
        private Vector3 FindStandingSpot(Vector3 body)
        {
            int ignore = (1 << Mathf.Max(0, GameLayers.PlayerLayer)) | (1 << Mathf.Max(0, GameLayers.DebrisLayer)) |
                         (1 << Mathf.Max(0, GameLayers.LootLayer));
            int floorMask = ~ignore;
            float radius = controller.radius, height = motor.Config.StandingHeight;
            foreach (Vector3 offset in RecoverOffsets)
            {
                Vector3 probe = body + offset;
                if (!Physics.Raycast(probe + Vector3.up * GroundProbeHeight, Vector3.down, out RaycastHit hit,
                        GroundProbeHeight + 3f, floorMask, QueryTriggerInteraction.Ignore))
                    continue;
                Vector3 feet = new(probe.x, hit.point.y + 0.05f, probe.z);
                Vector3 bottom = feet + Vector3.up * (radius + 0.05f), top = feet + Vector3.up * (height - radius);
                if (!Physics.CheckCapsule(bottom, top, radius * 0.95f, ~(1 << Mathf.Max(0, GameLayers.PlayerLayer)),
                        QueryTriggerInteraction.Ignore))
                    return feet;
            }
            return new Vector3(body.x, body.y - 0.9f, body.z);
        }

        public void Recover()
        {
            if (!IsRagdolled || IsRemote) return;
            Vector3 standAt = FindStandingSpot(pelvis.position);
            ragdollRoot.gameObject.SetActive(false);
            transform.position = standAt;
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
