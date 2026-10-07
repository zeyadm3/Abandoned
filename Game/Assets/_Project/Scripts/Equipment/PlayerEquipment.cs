using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.Core;
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

        public bool Has(EquipmentKind kind) =>
            (InSlot(0) is EquipmentDefinition a && a.Kind == kind) || (InSlot(1) is EquipmentDefinition b && b.Kind == kind);

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
            if (flashlight != null)
            {
                bool on = state.Value.LightOn && Has(EquipmentKind.Flashlight) && !IsDead;
                if (on != flashlight.enabled && !IsDead) Audio.GameAudio.Play(Audio.SoundId.FlashlightClick, flashlight.transform.position, 0.6f);
                flashlight.enabled = on;
            }
            if (carrier != null) carrier.SoloDragAllowed = !CompanyService.RulesApply || Has(EquipmentKind.HandTrolley);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            // The company can appear or change after we spawned (HQ load): keep the effects current.
            if (carrier != null) carrier.SoloDragAllowed = !CompanyService.RulesApply || Has(EquipmentKind.HandTrolley);
            // Ghosts can't help (GDD 11): a light left on goes out with its holder.
            if (IsServer && IsDead && state.Value.LightOn) { EquipState s = state.Value; s.LightOn = false; state.Value = s; }
            if (flashlight != null && flashlight.enabled && IsDead) Apply();
            if (!IsOwner || inputReader == null || IsDead) return;
            PlayerInputFrame input = inputReader.Current;
            if (!GameplayInputAllowed(input)) return;
            if (input.Slot1Pressed) SelectRpc(0);
            if (input.Slot2Pressed) SelectRpc(1);
            if (input.FlashlightPressed && Has(EquipmentKind.Flashlight)) LightRpc(!state.Value.LightOn);
            // Single-use gear: the left mouse button with empty hands.
            if (input.UsePressed && carrier != null && carrier.Held == null && !carrier.IsRagdolled && InHand is EquipmentDefinition d && d.Consumable) UseRpc(state.Value.Active);
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
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1) return;
            if (index >= 0 && (catalog.At(index) == null || state.Value[slot] != index && Available(index, this) - (state.Value[1 - slot] == index ? 1 : 0) <= 0)) return;
            // Gear is changed at the HQ, not mid-run (when a company exists).
            if (CompanyService.RulesApply && Networking.SessionTravel.Current != null && Networking.SessionTravel.Current.Level != CompanyService.HomeLevel) return;
            state.Value = state.Value.With(slot, index);
        }

        [Rpc(SendTo.Server)]
        private void SelectRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1 || IsDead) return;
            EquipState s = state.Value;
            s.Active = (byte)slot;
            state.Value = s;
        }

        [Rpc(SendTo.Server)]
        private void LightRpc(bool on, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || IsDead) return;
            EquipState s = state.Value;
            s.LightOn = on && Has(EquipmentKind.Flashlight);
            state.Value = s;
        }

        [Rpc(SendTo.Server)]
        private void UseRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1) return;
            // A knocked-down player's root stays where they fell from: nothing is used from there.
            if (player == null || player.IsDead || carrier.Held != null || carrier.IsRagdolled) return;
            EquipmentDefinition d = InSlot(slot);
            if (d == null || !d.Consumable) return;
            bool used = d.Kind switch
            {
                EquipmentKind.Medkit => Revive(),
                EquipmentKind.Planks => LayPlanks(),
                EquipmentKind.NoiseMaker => Throw(),
                _ => false,
            };
            if (used) ServerConsume(slot);
        }

        private Vector3 Eye => transform.position + Vector3.up * 1.5f;

        // Host: the plank appears lying lengthwise ahead (walkways are one 4 m tile wide, the plank is
        // 4.4 m), only where it fits: spawned inside a wall, PhysX would shove it through to the far side.
        private bool LayPlanks()
        {
            if (plankPrefab == null || !plankPrefab.TryGetComponent(out Loot.LootItem item) || item.Definition == null) return false;
            Vector3 half = item.Definition.Size * 0.5f;
            Quaternion rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            Vector3 forward = rotation * Vector3.forward;
            Vector3 at = transform.position + forward * (0.6f + half.z) + Vector3.up * 0.4f;
            if (Physics.CheckBox(at, half - Vector3.one * 0.02f, rotation, wallMask, QueryTriggerInteraction.Ignore) ||
                Physics.Linecast(Eye, at, wallMask, QueryTriggerInteraction.Ignore))
            {
                HintRpc("No room to lay the planks here");
                return false;
            }
            return Spawn(plankPrefab, at, rotation) != null;
        }

        // Host: the nearest downed crewmate within reach and in sight gets up.
        private bool Revive()
        {
            NetworkPlayer best = null;
            float bestDistance = reviveReach;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || !p.IsDead) continue;
                Vector3 body = p.Ragdoll.BodyPosition;
                float d = Vector3.Distance(body, transform.position);
                if (d > bestDistance || Physics.Linecast(Eye, body + Vector3.up * 0.3f, wallMask, QueryTriggerInteraction.Ignore)) continue;
                (best, bestDistance) = (p, d);
            }
            if (best == null) return false;
            best.ServerRevive();
            Debug.Log($"[Gear] Player {OwnerClientId} revived player {best.OwnerClientId} with a medkit.");
            return true;
        }

        private bool Throw()
        {
            Transform t = transform;
            // Facing a wall, start just short of it rather than past it in the next room.
            Vector3 at = Eye + t.forward * 0.6f;
            if (Physics.Linecast(Eye, at + t.forward * 0.15f, out RaycastHit hit, wallMask, QueryTriggerInteraction.Ignore))
                at = Eye + t.forward * Mathf.Max(0f, hit.distance - 0.2f);
            NetworkObject device = Spawn(noiseMakerPrefab, at, t.rotation);
            if (device == null) return false;
            device.GetComponent<NoiseMakerDevice>().Launch(t.forward * throwSpeed.x + Vector3.up * throwSpeed.y);
            return true;
        }

        [Rpc(SendTo.Owner)]
        private void HintRpc(string message, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            if (carrier != null) carrier.ShowHint(message);
        }

        private NetworkObject Spawn(NetworkObject prefab, Vector3 at, Quaternion rotation) =>
            prefab == null ? null : NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: rotation);

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
            foreach (PlayerEquipment e in Spawned)
            {
                if (e == null || e.NetworkManager != manager || !e.IsSpawned) continue;
                EquipState s = e.state.Value;
                for (int slot = 0; slot < 2; slot++)
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
