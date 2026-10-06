using System;
using System.Collections.Generic;
using Abandoned.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Makes the Player prefab a networked player. The owner simulates movement (owner-authoritative
    /// NetworkTransform) and publishes <see cref="PlayerNetState"/>; on every other machine the local
    /// control parts (camera, input, look, motor, interaction, HUD) are switched off and the body is
    /// driven from replicated state, so remote players render, weigh on floors (host) and make footsteps.
    /// Without a network session (single-player tests) nothing changes.
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerRagdoll ragdoll;
        [SerializeField] private NetworkTransform networkTransform;
        [Tooltip("Run only for the local player: camera, input, look, motor, interactor, HUD, camera feel, overlays.")]
        [SerializeField] private Behaviour[] ownerOnlyBehaviours;
        [Tooltip("Objects only the local player needs (e.g. the hit trigger: hits are judged by the owner).")]
        [SerializeField] private GameObject[] ownerOnlyObjects;

        private readonly NetworkVariable<PlayerNetState> state = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private static readonly List<NetworkPlayer> Spawned = new();

        public static IReadOnlyList<NetworkPlayer> All => Spawned;

        /// <summary>This machine's own player, or null.</summary>
        public static NetworkPlayer Local { get; private set; }

        public static event Action<NetworkPlayer> LocalPlayerSpawned;

        public PlayerNetState State => state.Value;
        public PlayerMotor Motor => motor;
        public PlayerRagdoll Ragdoll => ragdoll;

        public override void OnNetworkSpawn()
        {
            Spawned.Add(this);
            name = $"Player {OwnerClientId}{(IsOwner ? " (local)" : "")}";
            if (IsOwner)
            {
                Local = this;
                // The spawn pose was applied after PlayerLook.OnEnable cached yaw; without this the
                // first mouse move would snap every player to face +Z.
                if (TryGetComponent(out PlayerLook look)) look.SyncYawFromTransform();
                motor.Landed += OnOwnerLanded;
                ragdoll.Ended += OnOwnerGotUp;
                LocalPlayerSpawned?.Invoke(this);
                return;
            }

            foreach (Behaviour b in ownerOnlyBehaviours) if (b != null) b.enabled = false;
            foreach (GameObject go in ownerOnlyObjects) if (go != null) go.SetActive(false);
            ragdoll.MakeRemote();
            state.OnValueChanged += OnStateChanged;
            ApplyRemote(state.Value);
        }

        public override void OnNetworkDespawn()
        {
            Spawned.Remove(this);
            state.OnValueChanged -= OnStateChanged;
            if (!IsOwner) return;
            motor.Landed -= OnOwnerLanded;
            ragdoll.Ended -= OnOwnerGotUp;
            if (Local == this) Local = null;
        }

        public override void OnDestroy()
        {
            // Destroyed without a despawn (manager torn down abruptly): don't leave a dead entry behind.
            Spawned.Remove(this);
            base.OnDestroy();
        }

        private void LateUpdate()
        {
            if (!IsSpawned || !IsOwner) return;
            // NetworkVariable only sends when the value actually changed.
            state.Value = PlayerNetState.From(motor.IsGrounded, motor.IsSprinting, motor.IsCrouching,
                ragdoll.IsRagdolled, ragdoll.IsBodyResting, ragdoll.BodyPosition);
        }

        private void OnStateChanged(PlayerNetState previous, PlayerNetState current) => ApplyRemote(current);

        private void ApplyRemote(PlayerNetState s)
        {
            motor.ApplyRemoteState(s.Grounded, s.Sprinting, s.Crouching);
            ragdoll.ApplyRemoteState(s.Ragdolled, s.BodyPosition, s.BodyResting);
        }

        // The host owns structural damage; a client's landing only exists on the client, so report it.
        private void OnOwnerLanded(float fallHeight, float impactSpeed)
        {
            if (!IsServer) ReportLandingRpc(fallHeight, impactSpeed);
        }

        // The owner is trusted with its movement anyway; the clamp only stops a bad value from flattening a building.
        private const float MaxReportedFall = 60f, MaxReportedImpactSpeed = 35f;

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void ReportLandingRpc(float fallHeight, float impactSpeed) =>
            motor.RaiseRemoteLanding(Mathf.Clamp(fallHeight, 0f, MaxReportedFall), Mathf.Clamp(impactSpeed, 0f, MaxReportedImpactSpeed));

        // Getting up moves the root to where the body lies; others should jump there, not slide.
        private void OnOwnerGotUp()
        {
            if (networkTransform != null && networkTransform.CanCommitToTransform)
                networkTransform.Teleport(transform.position, transform.rotation, transform.localScale);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Spawned.Clear();
            Local = null;
            LocalPlayerSpawned = null;
        }
    }
}
