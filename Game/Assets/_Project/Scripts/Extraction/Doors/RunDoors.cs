using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Which doors are shut and which are broken this run (0.16.0), on the run state's object: the host
    /// rolls a few shut from the run seed, slides one for a player who presses E at it, and breaks one
    /// down for the Hunter. Everyone sees the two masks; the doors animate locally.
    /// </summary>
    public class RunDoors : NetworkBehaviour
    {
        private readonly NetworkVariable<ulong> shut = new();
        private readonly NetworkVariable<ulong> broken = new();
        // False until the host has rolled this run's doors (they hold still until then).
        private readonly NetworkVariable<bool> ready = new();
        private readonly Dictionary<ulong, float> nextUse = new();
        private bool rolled;

        public static RunDoors Current { get; private set; }

        public bool Ready => ready.Value;
        public bool IsShut(int index) => Ready && Valid(index) && (shut.Value & Bit(index)) != 0;
        public bool IsBroken(int index) => Ready && Valid(index) && (broken.Value & Bit(index)) != 0;
        public ulong ShutMask => shut.Value;
        public ulong BrokenMask => broken.Value;

        private static bool Valid(int index) => index >= 0 && index < 64;

        private static ulong Bit(int index) => 1UL << index;

        public override void OnNetworkSpawn() => Current = this;

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            // Host, once the run state has its seed (its own spawn may run after ours).
            if (IsServer && IsSpawned && !rolled) RollOnce();
        }

        private void RollOnce()
        {
            rolled = true;
            RunState run = GetComponent<RunState>();
            var random = new System.Random((run != null ? run.State.Seed : 0) ^ 0x0d0025);
            DoorConfig config = DoorConfig.Current;
            ulong mask = 0;
            // By index, and one roll per index whether or not a door has it: the same seed shuts the same
            // doors whatever order the scene loaded them in.
            for (int i = 0; i < 64; i++)
            {
                double roll = random.NextDouble();
                SlidingDoor door = SlidingDoor.ByIndex(i);
                if (door == null) continue;
                float chance = door.Kind == DoorKind.Service ? config.ServiceShutChance : config.StoreShutChance;
                if (roll < chance) mask |= Bit(i);
            }
            shut.Value = mask;
            ready.Value = true;
        }

        /// <summary>This machine's player pressed E on a door.</summary>
        public void RequestToggle(int index) => ToggleRpc(index);

        [Rpc(SendTo.Server)]
        private void ToggleRpc(int index, RpcParams rpcParams = default)
        {
            ulong client = rpcParams.Receive.SenderClientId;
            SlidingDoor door = SlidingDoor.ByIndex(index);
            if (!Ready || door == null || IsBroken(index)) return;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(client, out NetworkClient c) ? c.PlayerObject : null;
            if (player == null || !player.TryGetComponent(out NetworkPlayer np) || np.IsDead || np.Ragdoll.IsRagdolled || np.IsHiding) return;
            DoorConfig config = DoorConfig.Current;
            if (door.ReachDistance(player.transform.position) > config.Reach) return;
            if (nextUse.TryGetValue(client, out float next) && Time.time < next) return;
            nextUse[client] = Time.time + config.UseCooldown;
            bool shutIt = !IsShut(index);
            // A door never closes on anyone or anything standing in it.
            if (shutIt && door.SlotBlocked()) return;
            ServerSet(index, shutIt);
        }

        /// <summary>Host: slide a door open or shut. A broken one stays as it is.</summary>
        public void ServerSet(int index, bool shutIt)
        {
            if (!IsServer || !Ready || !Valid(index) || IsBroken(index) || IsShut(index) == shutIt) return;
            SlidingDoor door = SlidingDoor.ByIndex(index);
            if (door == null) return;
            shut.Value = shutIt ? shut.Value | Bit(index) : shut.Value & ~Bit(index);
            NoiseSystem.Emit(door.Centre, DoorConfig.Current.SlideNoise, NoiseSource.Other);
            SoundRpc(shutIt ? SoundId.DoorShut : SoundId.DoorSlide, door.Centre);
        }

        /// <summary>Host: one of the Hunter's blows on a shut door. Everyone hears it.</summary>
        public void ServerBash(int index)
        {
            if (!IsServer || !IsShut(index)) return;
            SlidingDoor door = SlidingDoor.ByIndex(index);
            if (door == null) return;
            NoiseSystem.Emit(door.Centre, DoorConfig.Current.BashNoise, NoiseSource.Other);
            BashRpc(index);
        }

        /// <summary>Host: the door gives way. It flies open and never shuts again this job.</summary>
        public void ServerBreak(int index)
        {
            if (!IsServer || !Ready || !Valid(index) || IsBroken(index)) return;
            SlidingDoor door = SlidingDoor.ByIndex(index);
            if (door == null) return;
            broken.Value |= Bit(index);
            shut.Value &= ~Bit(index);
            NoiseSystem.Emit(door.Centre, 1f, NoiseSource.Other);
            BreakRpc(index);
            Debug.Log($"[Run] Door {index} was broken down.");
        }

        /// <summary>Host (the Last Hunter): every shut door this near, on its storey, is broken down at once.</summary>
        public void ServerBreakNear(Vector3 at, float radius)
        {
            if (!IsServer || !Ready || shut.Value == 0) return;
            foreach (SlidingDoor door in SlidingDoor.All)
            {
                if (door == null || !IsShut(door.Index)) continue;
                if (Mathf.Abs(door.ToLocal(at).y) < 3f && door.DistanceTo(at) <= radius) ServerBreak(door.Index);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SoundRpc(SoundId id, Vector3 at) => GameAudio.Play(id, at, 1f);

        [Rpc(SendTo.Everyone)]
        private void BashRpc(int index)
        {
            SlidingDoor door = SlidingDoor.ByIndex(index);
            if (door == null) return;
            GameAudio.Play(SoundId.DoorBash, door.Centre, 1f);
            CameraShake.Emit(door.Centre, 1200f);
            door.Rattle();
        }

        [Rpc(SendTo.Everyone)]
        private void BreakRpc(int index)
        {
            SlidingDoor door = SlidingDoor.ByIndex(index);
            if (door == null) return;
            GameAudio.Play(SoundId.DoorBreak, door.Centre, 1f);
            CameraShake.Emit(door.Centre, 2600f);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || !IsSpawned) return;
            GUI.Label(new Rect(Screen.width - 360f, 96f, 350f, 22f),
                $"DOORS shut {System.Convert.ToString((long)shut.Value, 2)} broken {System.Convert.ToString((long)broken.Value, 2)} ({SlidingDoor.All.Count} in level)");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
