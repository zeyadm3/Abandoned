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
        [SerializeField] private NetworkObject supportJackPrefab;
        [SerializeField] private NetworkObject pulleyPrefab;
        [Tooltip("Pocket slots a backpack adds.")]
        [SerializeField, Range(0, 6)] private int backpackSlots = 2;
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
                bool on = LightOn;
                if (on != flashlight.enabled && !IsDead) Audio.GameAudio.Play(Audio.SoundId.FlashlightClick, flashlight.transform.position, 0.6f);
                flashlight.enabled = on;
            }
            if (carrier != null) carrier.SoloDragAllowed = !CompanyService.RulesApply || Has(EquipmentKind.HandTrolley);
            if (carrier != null) carrier.FlatbedDrag = Has(EquipmentKind.Flatbed);
            if (carrier != null && carrier.Inventory != null) carrier.Inventory.ExtraSlots = Has(EquipmentKind.Backpack) ? backpackSlots : 0;
        }

        private void Update()
        {
            if (!IsSpawned) return;
            // The company can appear or change after we spawned (HQ load): keep the effects current.
            if (carrier != null) carrier.SoloDragAllowed = !CompanyService.RulesApply || Has(EquipmentKind.HandTrolley);
            if (carrier != null) carrier.FlatbedDrag = Has(EquipmentKind.Flatbed);
            TickFlashlight();
            // Preserve the switch position while hands are occupied: setting down restores the beam.
            // Automatic hand changes are silent; the click belongs to deliberately using the switch.
            if (flashlight != null)
                flashlight.enabled = LightOn;
            if (!IsOwner || inputReader == null || IsDead) return;
            PlayerInputFrame input = inputReader.Current;
            if (!GameplayInputAllowed(input)) return;
            if (input.Slot1Pressed) SelectRpc(0);
            if (input.Slot2Pressed) SelectRpc(1);
            if (input.FlashlightPressed && Has(EquipmentKind.Flashlight))
            {
                if (HandsFreeForLight) LightRpc(!state.Value.LightOn);
                else carrier.ShowHint("Your hands are full. Set the loot down to use your flashlight.");
            }
            // Single-use gear: the left mouse button with empty hands.
            if (input.UsePressed && carrier != null && carrier.Held == null && !carrier.IsRagdolled && InHand is EquipmentDefinition d && (d.Consumable || d.Kind == EquipmentKind.Crowbar)) UseRpc(state.Value.Active);
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
            s.LightOn = on && Has(EquipmentKind.Flashlight) && BatterySeconds > 0f;
            state.Value = s;
        }

        [Rpc(SendTo.Server)]
        private void UseRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot > 1) return;
            // A knocked-down player's root stays where they fell from: nothing is used from there.
            if (player == null || player.IsDead || carrier.Held != null || carrier.IsRagdolled) return;
            EquipmentDefinition d = InSlot(slot);
            if (d == null) return;
            if (d.Kind == EquipmentKind.Crowbar) { Pry(); return; } // a tool: never used up
            if (!d.Consumable) return;
            bool used = d.Kind switch
            {
                EquipmentKind.Medkit => TreatInjury(),
                EquipmentKind.Battery => RefillBattery(),
                EquipmentKind.Planks => LayPlanks(),
                EquipmentKind.NoiseMaker => Throw(),
                EquipmentKind.SupportJack => PlaceJack(),
                EquipmentKind.RopePulley => RigPulley(),
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

        // Host (M10.3): rig a rope and pulley over the hole in front of you.
        private bool RigPulley()
        {
            Vector3 forward = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward;
            if (pulleyPrefab == null || !Pulley.FindHole(transform.position, forward, 3.2f, wallMask, out Vector3 at, out float drop))
            {
                HintRpc("Face a hole in the floor (a drop of 2 m or more) to rig the pulley");
                return false;
            }
            NetworkObject rig = Spawn(pulleyPrefab, at, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
            if (rig == null) return false;
            rig.GetComponent<Pulley>().Rig(drop);
            SoundRpc(Audio.SoundId.JackPlaced, at + Vector3.up);
            Debug.Log($"[Gear] Player {OwnerClientId} rigged a pulley over a {drop:0.0} m drop.");
            return true;
        }

        // Host (M9.4): brace the floor you're standing on from below: the post runs down to whatever is under it.
        private bool PlaceJack()
        {
            StructuralSection section = SectionQuery.Under(transform.position);
            if (supportJackPrefab == null || section == null || !section.CanCollapse || section.Reinforcement > 1f)
            {
                HintRpc(section != null && section.Reinforcement > 1f ? "This floor is already braced" : "Stand on a floor that could give way to brace it");
                return false;
            }
            Vector3 centre = section.transform.position;
            float underside = centre.y - 0.35f;
            if (!Physics.Raycast(new Vector3(centre.x, underside - 0.05f, centre.z), Vector3.down, out RaycastHit below, 8f, wallMask, QueryTriggerInteraction.Ignore))
            {
                HintRpc("Nothing below to brace it against");
                return false;
            }
            NetworkObject jack = Spawn(supportJackPrefab, below.point, Quaternion.identity);
            if (jack == null) return false;
            jack.GetComponent<SupportJack>().Brace(section, underside - below.point.y);
            SoundRpc(Audio.SoundId.JackPlaced, below.point + Vector3.up);
            Debug.Log($"[Gear] Player {OwnerClientId} braced {section.name} (capacity now {section.Capacity:0} kg).");
            return true;
        }

        // Host (M9.4): strike the section in front of you; enough strikes break even a sound floor (always
        // through a Cracking warning first). Loud: monsters hear it.
        private void Pry()
        {
            if (Time.time < nextPry) return;
            nextPry = Time.time + crowbarCooldown;
            int mask = 1 << Mathf.Max(0, GameLayers.StructureLayer);
            Vector3 eye = player.transform.position + Vector3.up * 1.5f;
            // Where its owner is looking: body yaw and the replicated camera pitch (the host has no remote camera).
            Vector3 aim = Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;
            if (!Physics.Raycast(eye, aim, out RaycastHit hit, crowbarReach, mask, QueryTriggerInteraction.Ignore)) return;
            StructuralSection section = hit.collider.GetComponentInParent<StructuralSection>();
            if (section == null || !section.CanCollapse) return;
            section.ApplyImpact(crowbarMomentum);
            NoiseSystem.Emit(hit.point, 0.7f, NoiseSource.Other);
            SoundRpc(Audio.SoundId.CrowbarHit, hit.point);
        }

        // The reviver's own machine counts it (achievements are personal).
        [Rpc(SendTo.Owner)]
        private void RevivedSomeoneRpc() => Achievements.Increment(Achievements.StatRevives);

        [Rpc(SendTo.Everyone)]
        private void SoundRpc(Audio.SoundId id, Vector3 at) => Audio.GameAudio.Play(id, at, 1f);

        // Host: the nearest downed crewmate within reach and in sight gets up.
        private bool TreatInjury()
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
            if (best != null)
            {
                best.ServerRevive();
                RevivedSomeoneRpc();
                return true;
            }
            // Aim at an injured teammate to help them; an empty sightline treats yourself.
            bestDistance = reviveReach;
            Vector3 aim = Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p == player || p.NetworkManager != NetworkManager || p.IsDead || p.Health >= p.MaxHealth) continue;
                Vector3 at = p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position + Vector3.up;
                Vector3 offset = at - Eye;
                float distance = offset.magnitude;
                if (distance > bestDistance || Vector3.Dot(offset.normalized, aim) < 0.7f ||
                    Physics.Linecast(Eye, at, wallMask, QueryTriggerInteraction.Ignore)) continue;
                (best, bestDistance) = (p, distance);
            }
            if (best != null) return best.ServerHeal(best.HealthConfig.MedkitHeal);
            if (player.ServerHeal(player.HealthConfig.MedkitHeal)) return true;
            HintRpc("No injury to treat. Aim at an injured crewmate, or use the medkit when hurt.");
            return false;
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
