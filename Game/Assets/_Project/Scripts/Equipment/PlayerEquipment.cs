using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Voice;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A player's gear (GDD 12: 2 hand slots, keys 1 and 2): what they took from the company's stock at
    /// the HQ gear rack, which slot is in hand, and the flashlight (F). The host decides (stock is
    /// shared); everyone sees the light. Effects: the flashlight beam, the radio (walkie-talkie needs one),
    /// the hand trolley (solo Heavy drags). Outside a company game (a level hosted directly: dev, tests)
    /// nothing is restricted (<see cref="CompanyService.RulesApply"/>).
    /// </summary>
    public class PlayerEquipment : NetworkBehaviour
    {
        [SerializeField] private EquipmentCatalog catalog;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private Light flashlight;
        [Tooltip("Spawned by the Planks gear: a loot-like plank to carry and lay across a hole.")]
        [SerializeField] private NetworkObject plankPrefab;
        [SerializeField] private NetworkObject noiseMakerPrefab;
        [SerializeField, Min(0.5f)] private float reviveReach = 2.5f;
        [SerializeField] private Vector2 throwSpeed = new(9f, 3f);

        private readonly NetworkVariable<EquipState> state = new(EquipState.Empty);
        private static readonly List<PlayerEquipment> Spawned = new();

        public static IReadOnlyList<PlayerEquipment> All => Spawned;
        public EquipState State => state.Value;
        public EquipmentCatalog Catalog => catalog;

        public EquipmentDefinition InSlot(int slot) => catalog.At(state.Value[slot]);
        public EquipmentDefinition InHand => InSlot(state.Value.Active);

        public bool Has(EquipmentKind kind) =>
            (InSlot(0) is EquipmentDefinition a && a.Kind == kind) || (InSlot(1) is EquipmentDefinition b && b.Kind == kind);

        public static PlayerEquipment Of(NetworkManager manager, ulong clientId)
        {
            foreach (PlayerEquipment e in Spawned)
                if (e != null && e.NetworkManager == manager && e.OwnerClientId == clientId) return e;
            return null;
        }

        public override void OnNetworkSpawn()
        {
            Spawned.Add(this);
            state.OnValueChanged += OnChanged;
            // Walkie-talkies need a radio in hand slots once there's a company to buy them from.
            NetworkVoice.RadioHolder = id =>
            {
                if (!CompanyService.RulesApply) return true;
                PlayerEquipment e = Of(NetworkManager.Singleton, id);
                return e == null || e.Has(EquipmentKind.Radio);
            };
            if (IsServer) StartingKit();
            Apply();
        }

        public override void OnNetworkDespawn()
        {
            Spawned.Remove(this);
            state.OnValueChanged -= OnChanged;
        }

        private void OnChanged(EquipState previous, EquipState current) => Apply();

        // Host: everyone starts holding a flashlight and a radio when the company has them spare.
        private void StartingKit()
        {
            EquipState s = EquipState.Empty;
            int light = catalog.IndexOf("flashlight"), radio = catalog.IndexOf("radio");
            if (Available(light, this) > 0) s = s.With(0, light);
            if (Available(radio, this) > 0) s = s.With(1, radio);
            state.Value = s;
        }

        /// <summary>Company stock of an item not in anyone else's hands (no company: unlimited).</summary>
        public int Available(int index, PlayerEquipment except = null)
        {
            CompanyService company = CompanyService.Current;
            if (index < 0) return 0;
            if (!CompanyService.RulesApply) return int.MaxValue;
            int used = 0;
            foreach (PlayerEquipment e in Spawned)
                if (e != null && e != except && e.NetworkManager == NetworkManager)
                    used += (e.State.Slot0 == index ? 1 : 0) + (e.State.Slot1 == index ? 1 : 0);
            return company.OwnedCount(index) - used;
        }

        private void Apply()
        {
            if (flashlight != null) flashlight.enabled = state.Value.LightOn && Has(EquipmentKind.Flashlight);
            if (carrier != null) carrier.SoloDragAllowed = !CompanyService.RulesApply || Has(EquipmentKind.HandTrolley);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            // The company can appear or change after we spawned (HQ load): keep the effects current.
            if (carrier != null) carrier.SoloDragAllowed = !CompanyService.RulesApply || Has(EquipmentKind.HandTrolley);
            if (!IsOwner || inputReader == null) return;
            PlayerInputFrame input = inputReader.Current;
            if (input.Slot1Pressed) SelectRpc(0);
            if (input.Slot2Pressed) SelectRpc(1);
            if (input.FlashlightPressed && Has(EquipmentKind.Flashlight)) LightRpc(!state.Value.LightOn);
            // Single-use gear: the left mouse button with empty hands.
            if (input.UsePressed && carrier != null && carrier.Held == null && InHand is EquipmentDefinition d && d.Consumable) UseRpc(state.Value.Active);
        }

        /// <summary>This player asks to put a catalog item (-1 = nothing) into a slot (the HQ gear rack).</summary>
        public void RequestEquip(int slot, int index) => EquipRpc(slot, index);

        [Rpc(SendTo.Server)]
        private void EquipRpc(int slot, int index, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1) return;
            if (index >= 0 && (catalog.At(index) == null || state.Value[slot] != index && Available(index, this) - (state.Value[1 - slot] == index ? 1 : 0) <= 0)) return;
            // Gear is changed at the HQ, not mid-run (when a company exists).
            if (CompanyService.RulesApply && Networking.SessionTravel.Current != null && Networking.SessionTravel.Current.Level != CompanyService.HomeLevel) return;
            state.Value = state.Value.With(slot, index);
        }

        [Rpc(SendTo.Server)]
        private void SelectRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1) return;
            EquipState s = state.Value;
            s.Active = (byte)slot;
            state.Value = s;
        }

        [Rpc(SendTo.Server)]
        private void LightRpc(bool on, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId) return;
            EquipState s = state.Value;
            s.LightOn = on && Has(EquipmentKind.Flashlight);
            state.Value = s;
        }

        [Rpc(SendTo.Server)]
        private void UseRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1) return;
            NetworkPlayer me = GetComponent<NetworkPlayer>();
            if (me == null || me.IsDead || carrier.Held != null) return;
            EquipmentDefinition d = InSlot(slot);
            if (d == null || !d.Consumable) return;
            Transform t = transform;
            Vector3 front = t.position + t.forward * 1.2f + Vector3.up * 1f;
            bool used = d.Kind switch
            {
                EquipmentKind.Medkit => Revive(),
                EquipmentKind.Planks => Spawn(plankPrefab, front, t.rotation * Quaternion.Euler(0f, 90f, 0f)) != null,
                EquipmentKind.NoiseMaker => Throw(),
                _ => false,
            };
            if (used) ServerConsume(slot);
        }

        // Host: the nearest downed crewmate within reach gets up.
        private bool Revive()
        {
            NetworkPlayer best = null;
            float bestDistance = reviveReach;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || !p.IsDead) continue;
                float d = Vector3.Distance(p.Ragdoll.BodyPosition, transform.position);
                if (d <= bestDistance) (best, bestDistance) = (p, d);
            }
            if (best == null) return false;
            best.ServerRevive();
            Debug.Log($"[Gear] Player {OwnerClientId} revived player {best.OwnerClientId} with a medkit.");
            return true;
        }

        private bool Throw()
        {
            Transform t = transform;
            NetworkObject device = Spawn(noiseMakerPrefab, t.position + t.forward * 0.6f + Vector3.up * 1.5f, t.rotation);
            if (device == null) return false;
            device.GetComponent<NoiseMakerDevice>().Launch(t.forward * throwSpeed.x + Vector3.up * throwSpeed.y);
            return true;
        }

        private NetworkObject Spawn(NetworkObject prefab, Vector3 at, Quaternion rotation) =>
            prefab == null ? null : NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: rotation);

        /// <summary>Host: a consumable in this slot was used up.</summary>
        public void ServerConsume(int slot)
        {
            if (!IsServer) return;
            int index = state.Value[slot];
            if (index < 0 || (CompanyService.RulesApply && !CompanyService.Current.Consume(index))) return;
            state.Value = state.Value.With(slot, -1);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Spawned.Clear();
    }
}
