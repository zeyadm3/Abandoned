using System;
using System.Collections.Generic;
using Abandoned.Interaction;
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
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private NetworkTransform networkTransform;
        private PlayerLook look;
        [Tooltip("Run only for the local player: camera, input, look, motor, interactor, HUD, camera feel, overlays.")]
        [SerializeField] private Behaviour[] ownerOnlyBehaviours;
        [Tooltip("Objects only the local player needs (e.g. the hit trigger: hits are judged by the owner).")]
        [SerializeField] private GameObject[] ownerOnlyObjects;

        private readonly NetworkVariable<PlayerNetState> state = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Host-written: death is the host's call (a monster's contact, left behind), never the owner's.
        private readonly NetworkVariable<bool> dead = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private static readonly List<NetworkPlayer> Spawned = new();

        public static IReadOnlyList<NetworkPlayer> All => Spawned;

        /// <summary>This machine's own player, or null.</summary>
        public static NetworkPlayer Local { get; private set; }

        public static event Action<NetworkPlayer> LocalPlayerSpawned;

        public PlayerNetState State => state.Value;
        public bool IsDead => dead.Value;

        /// <summary>Every machine: a player died (this one, now dead) or came back (next run).</summary>
        public static event Action<NetworkPlayer, bool> DeathChanged;
        public PlayerMotor Motor => motor;
        public PlayerRagdoll Ragdoll => ragdoll;
        public PlayerCarrier Carrier => carrier;

        public override void OnNetworkSpawn()
        {
            Spawned.Add(this);
            look = GetComponent<PlayerLook>();
            name = $"Player {OwnerClientId}{(IsOwner ? " (local)" : "")}";
            // Players travel with the session from level to level (6.0).
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            if (IsOwner)
            {
                Local = this;
                // The spawn pose was applied after PlayerLook.OnEnable cached yaw; without this the
                // first mouse move would snap every player to face +Z.
                if (look != null) look.SyncYawFromTransform();
                motor.Landed += OnOwnerLanded;
                ragdoll.Ended += OnOwnerGotUp;
                dead.OnValueChanged += OnDeadChanged;
                LocalPlayerSpawned?.Invoke(this);
                return;
            }

            dead.OnValueChanged += OnDeadChanged;
            foreach (Behaviour b in ownerOnlyBehaviours) if (b != null) b.enabled = false;
            foreach (GameObject go in ownerOnlyObjects) if (go != null) go.SetActive(false);
            ragdoll.MakeRemote();
            // Holds are mirrored from each item's replicated state (NetworkLoot); this copy never drives them.
            carrier.MakeRemote();
            state.OnValueChanged += OnStateChanged;
            ApplyRemote(state.Value);
        }

        public override void OnNetworkDespawn()
        {
            Spawned.Remove(this);
            state.OnValueChanged -= OnStateChanged;
            dead.OnValueChanged -= OnDeadChanged;
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
                ragdoll.IsRagdolled, ragdoll.IsBodyResting, ragdoll.BodyPosition, look != null ? look.Pitch : 0f);
        }

        private void OnStateChanged(PlayerNetState previous, PlayerNetState current) => ApplyRemote(current);

        private void ApplyRemote(PlayerNetState s)
        {
            motor.ApplyRemoteState(s.Grounded, s.Sprinting, s.Crouching);
            ragdoll.ApplyRemoteState(s.Ragdolled, s.BodyPosition, s.BodyResting);
            if (!IsOwner && look != null) look.ApplyRemotePitch(s.Pitch);
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

        /// <summary>
        /// Owner: moves this player somewhere else at once (nettest staging, later respawns); every
        /// other machine jumps it there instead of sliding through walls.
        /// </summary>
        public void OwnerTeleport(Vector3 position)
        {
            if (!IsOwner) return;
            var controller = GetComponent<CharacterController>();
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            Physics.SyncTransforms();
            controller.enabled = wasEnabled;
            motor.ResetFallTracking();
            if (networkTransform != null && networkTransform.CanCommitToTransform)
                networkTransform.Teleport(transform.position, transform.rotation, transform.localScale);
        }

        /// <summary>Host: put this player back on a spawn point for a new run (the owner moves itself), alive.</summary>
        public void ServerRespawn(Pose pose)
        {
            if (!IsServer) return;
            dead.Value = false;
            RespawnRpc(pose.position, pose.rotation, false);
        }

        /// <summary>Host: a medkit got this downed player back up where their body lies.</summary>
        public void ServerRevive()
        {
            if (!IsServer || !dead.Value) return;
            dead.Value = false;
            RespawnRpc(ragdoll.BodyPosition + Vector3.up * 0.1f, transform.rotation, true);
        }

        /// <summary>Host: this player dies (monster contact). Their body falls where they stood.</summary>
        public void ServerKill()
        {
            if (!IsServer || dead.Value) return;
            dead.Value = true;
            // GDD 11: a dead player's pocket loot drops where they died, for the others to recover.
            foreach (Grabbable item in new List<Grabbable>(carrier.Inventory.Items))
                if (NetworkLoot.Of(item) is NetworkLoot loot) LootServerActions.FreeOrphan(loot);
        }

        // Dead: the body goes down for good (local ragdoll, GDD 11) and the controls stop. Alive again: back up.
        private void OnDeadChanged(bool was, bool now)
        {
            if (IsOwner)
            {
                if (now) ragdoll.Enter(Vector3.zero);
                ragdoll.HoldDown = now;
                foreach (Behaviour b in ownerOnlyBehaviours)
                    if (b is PlayerMotor || b is PlayerInteractor) b.enabled = !now;
            }
            if (now && IsOwner)
            {
                Core.Achievements.Increment(Core.Achievements.StatDeaths);
                Audio.MusicPlayer.Play(Audio.MusicSting.Death);
            }
            DeathChanged?.Invoke(this, now);
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        private void RespawnRpc(Vector3 position, Quaternion rotation, bool whereBodyLies)
        {
            // A revive stands up on the ragdoll's own checked spot; the body's raw position can be
            // against a wall or inside a prop.
            if (whereBodyLies && ragdoll.IsRagdolled)
            {
                ragdoll.HoldDown = false;
                ragdoll.Recover();
                motor.ResetFallTracking();
                if (look != null) look.SyncYawFromTransform();
                return;
            }
            if (ragdoll.IsRagdolled) ragdoll.Recover();
            transform.rotation = rotation;
            OwnerTeleport(position);
            if (look != null) look.SyncYawFromTransform();
        }

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
            DeathChanged = null;
        }
    }
}
