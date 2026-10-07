using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.Interaction;
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

        /// <summary>Host: a consumable in this slot was used up (6.4b).</summary>
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
