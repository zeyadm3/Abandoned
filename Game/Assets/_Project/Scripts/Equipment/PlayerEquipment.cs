using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
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
    public partial class PlayerEquipment : NetworkBehaviour
    {
        [SerializeField] private EquipmentCatalog catalog;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private Light flashlight;
        [Tooltip("Spawned by the Planks gear: a loot-like plank to carry and lay across a hole.")]
        [SerializeField] private NetworkObject plankPrefab;
        [SerializeField] private NetworkObject noiseMakerPrefab;
        [Tooltip("Crowbar: reach (m), strike momentum (kg·m/s, before the structure's threshold) and seconds between strikes.")]
        [SerializeField] private float crowbarReach = 2.6f;
        [SerializeField] private float crowbarMomentum = 1500f;
        [SerializeField] private float crowbarCooldown = 0.7f;
        private float nextPry;
        [SerializeField, Min(0.5f)] private float reviveReach = 2.5f;
        [SerializeField] private Vector2 throwSpeed = new(9f, 3f);

        private readonly NetworkVariable<EquipState> state = new(EquipState.Empty);
        private static readonly List<PlayerEquipment> Spawned = new();
        private static readonly Dictionary<int, int> Remaining = new();
        private PlayerLook look;
        private NetworkPlayer player;
        private bool suppressUseUntilRelease;
        private int wallMask;

        public static IReadOnlyList<PlayerEquipment> All => Spawned;
        public EquipState State => state.Value;
        public EquipmentCatalog Catalog => catalog;

        public EquipmentDefinition InSlot(int slot) => catalog.At(state.Value[slot]);
        public EquipmentDefinition InHand => InSlot(state.Value.Active);

        /// <summary>One-hand loot leaves a hand for the light; larger carries need both hands.</summary>
        public bool HandsFreeForLight => carrier == null || carrier.Held == null ||
            (carrier.Held.CarryClass < CarryClass.TwoHand && !carrier.IsSharing && !carrier.IsDragging);

        public bool Has(EquipmentKind kind)
        {
            for (int slot = 0; slot < EquipState.SlotCount; slot++)
                if (InSlot(slot) is EquipmentDefinition item && item.Kind == kind) return true;
            return false;
        }
        public int OccupiedCount
        {
            get { int count = 0; for (int slot = 0; slot < EquipState.SlotCount; slot++) if (State[slot] >= 0) count++; return count; }
        }
        public int FreePocket => carrier != null ? carrier.Inventory.FirstFreeSlot : -1;
        public void RequestSelect(int slot) => SelectRpc(slot);

        public static PlayerEquipment Of(NetworkManager manager, ulong clientId)
        {
            foreach (PlayerEquipment e in Spawned)
                if (e != null && e.NetworkManager == manager && e.OwnerClientId == clientId) return e;
            return null;
        }

        private void Awake()
        {
            look = GetComponent<PlayerLook>();
            player = GetComponent<NetworkPlayer>();
            wallMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
        }

        private bool IsDead => player != null && player.IsDead;

        public override void OnNetworkSpawn()
        {
            Spawned.Add(this);
            SpawnPockets();
            InitializeFlashlight();
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
            DespawnPockets();
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
                if (e != null && e.NetworkManager == NetworkManager) used += e.DroppedCount(index);
            foreach (PlayerEquipment e in Spawned)
                if (e != null && e != except && e.NetworkManager == NetworkManager)
                    for (int slot = 0; slot < EquipState.SlotCount; slot++) if (e.State[slot] == index) used++;
            return company.OwnedCount(index) - used;
        }

        private void Apply()
        {
            if (flashlight != null)
            {
                bool on = LightOn;
                if (on != flashlight.enabled && !IsDead) Audio.GameAudio.Play(Audio.SoundId.FlashlightClick, flashlight.transform.position, 0.6f);
                flashlight.enabled = on;
            }
            if (carrier != null) carrier.TrolleyDrag = Has(EquipmentKind.HandTrolley);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            TickDroppedTools();
            // The company can appear or change after we spawned (HQ load): keep the effects current.
            if (carrier != null) carrier.TrolleyDrag = Has(EquipmentKind.HandTrolley);
            TickFlashlight();
            // Preserve the switch position while hands are occupied: setting down restores the beam.
            // Automatic hand changes are silent; the click belongs to deliberately using the switch.
            if (flashlight != null)
                flashlight.enabled = LightOn;
            if (!IsOwner || inputReader == null || IsDead) return;
            PlayerInputFrame input = inputReader.Current;
            if (!GameplayInputAllowed(input)) return;
            TickGiveGear(input);
            if (input.Slot1Pressed) SelectRpc(0);
            if (input.Slot2Pressed) SelectRpc(1);
            if (input.Slot3Pressed) SelectRpc(2);
            if (input.Slot4Pressed) SelectRpc(3);
            // 0.23.2: the wheel flips between the hand slots while the hands are empty (and not sorting pockets).
            if (input.Scroll != 0f && !input.RotateHeld && carrier != null)
                SelectRpc((state.Value.Active + (input.Scroll > 0f ? 3 : 1)) % EquipState.SlotCount);
            if (input.InventoryHeld)
            {
                if (input.DropPressed) RequestDiscardPocket();
                return;
            }
            if (input.FlashlightPressed && Has(EquipmentKind.Flashlight))
            {
                if (HandsFreeForLight) LightRpc(!state.Value.LightOn);
                else carrier.ShowHint(Loc.Mark("Your hands are full. Set the loot down to use your flashlight."));
            }
            // Single-use gear: the left mouse button with empty hands.
            if (input.UsePressed && carrier != null && carrier.Held == null && !carrier.IsRagdolled && InHand is EquipmentDefinition d && (d.Consumable || d.Kind == EquipmentKind.Crowbar || d.Kind >= EquipmentKind.Sledgehammer)) UseRpc(state.Value.Active);
        }

        // Like the interactor: clicks on a screen (gear rack, shop, pause) and the click that
        // re-captures the cursor must not also use the gear in hand.
        private bool GameplayInputAllowed(PlayerInputFrame input)
        {
            if (!input.UseHeld) suppressUseUntilRelease = false;
            if (CursorOwner.UiActive) { suppressUseUntilRelease = true; return false; }
            if (look != null && look.isActiveAndEnabled)
            {
                if (!look.CursorCaptured) { suppressUseUntilRelease = true; return false; }
                if (look.CaptureFrame == Time.frameCount) suppressUseUntilRelease = true;
            }
            return !suppressUseUntilRelease || !input.UsePressed;
        }

        /// <summary>This player asks to put a catalog item (-1 = nothing) into a slot (the HQ gear rack).</summary>
        public void RequestEquip(int slot, int index) => EquipRpc(slot, index);

        [Rpc(SendTo.Server)]
        private void EquipRpc(int slot, int index, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot >= EquipState.SlotCount) return;
            if (carrier != null && carrier.Inventory.AtSlot(slot) != null) return;
            int usedHere = 0;
            for (int other = 0; other < EquipState.SlotCount; other++) if (other != slot && state.Value[other] == index) usedHere++;
            if (index >= 0 && (catalog.At(index) == null || state.Value[slot] != index && Available(index, this) - usedHere <= 0)) return;
            // Gear is changed at the HQ, not mid-run (when a company exists).
            if (CompanyService.RulesApply && Networking.SessionTravel.Current != null && !CompanyService.AtHome) return;
            state.Value = state.Value.With(slot, index);
        }

        [Rpc(SendTo.Server)]
        private void SelectRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot >= EquipState.SlotCount || IsDead || carrier.IsRagdolled) return;
            if (!LootServerActions.SelectPocket(carrier, slot)) return;
            EquipState s = state.Value;
            s.Active = (byte)slot;
            state.Value = s;
        }

        [Rpc(SendTo.Server)]
        private void LightRpc(bool on, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || IsDead) return;
            EquipState s = state.Value;
            s.LightOn = on && Has(EquipmentKind.Flashlight) && BatterySeconds > 0f;
            state.Value = s;
        }

        /// <summary>Host: a consumable in this slot was used up.</summary>
        public void ServerConsume(int slot)
        {
            if (!IsServer) return;
            int index = state.Value[slot];
            if (index < 0) return;
            // The slot empties even if the stock had no such item left (bankruptcy reset it): a used
            // item must never stay in hand for free.
            if (CompanyService.RulesApply) CompanyService.Current.Consume(index);
            state.Value = state.Value.With(slot, -1);
        }

        /// <summary>
        /// Host: the company's stock changed (bought, used, bankruptcy reset). Anything in hands beyond
        /// what it now owns goes back, first come first kept (spawn order).
        /// </summary>
        public static void ServerTrimToStock(NetworkManager manager)
        {
            CompanyService company = CompanyService.Current;
            if (manager == null || !manager.IsServer || company == null || !CompanyService.RulesApply) return;
            Remaining.Clear();
            // Ground tools reserve stock too, including after a stock reset or bankruptcy.
            foreach (PlayerEquipment e in Spawned)
            {
                if (e == null || e.NetworkManager != manager || !e.IsSpawned) continue;
                for (int i = e.droppedTools.Count - 1; i >= 0; i--)
                {
                    int index = e.droppedTools[i].CatalogIndex;
                    if (!Remaining.TryGetValue(index, out int left)) left = company.OwnedCount(index);
                    if (left <= 0) e.droppedTools.RemoveAt(i);
                    else Remaining[index] = left - 1;
                }
            }
            foreach (PlayerEquipment e in Spawned)
            {
                if (e == null || e.NetworkManager != manager || !e.IsSpawned) continue;
                EquipState s = e.state.Value;
                for (int slot = 0; slot < EquipState.SlotCount; slot++)
                {
                    int index = s[slot];
                    if (index < 0) continue;
                    if (!Remaining.TryGetValue(index, out int left)) left = company.OwnedCount(index);
                    if (left <= 0) s = s.With(slot, -1);
                    else Remaining[index] = left - 1;
                }
                if (!s.Equals(e.state.Value)) e.state.Value = s;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Spawned.Clear();
    }
}
