using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Company
{
    /// <summary>The HQ depot unloads already appraised cargo; only the run's ledger grants a payday.</summary>
    public partial class CompanyService
    {
        private readonly NetworkVariable<float> lastLoadedWeight = new(-1f);
        private readonly NetworkVariable<int> lastLoadedItems = new();
        private readonly NetworkVariable<int> intakeItems = new();
        private readonly NetworkVariable<float> intakeWeight = new();

        public float LastLoadedWeight => lastLoadedWeight.Value;
        public int LastLoadedItems => lastLoadedItems.Value;
        public int IntakeItems => intakeItems.Value;
        public float IntakeWeight => intakeWeight.Value;

        public void RequestScrapIntake()
        {
            if (!IsSpawned) return;
            if (IsServer) ReceiveScrap(NetworkManager.LocalClientId);
            else ReceiveScrapRpc();
        }

        [Rpc(SendTo.Server)]
        private void ReceiveScrapRpc(RpcParams rpcParams = default) => ReceiveScrap(rpcParams.Receive.SenderClientId);

        private void ReceiveScrap(ulong sender)
        {
            ScrapDepot intake = ScrapDepot.Intake;
            if (!IsServer || intake == null || intake.gameObject.scene.name != HomeLevel || !SessionTravel.LevelReady) return;
            if (SessionTravel.Current != null ? !AtHome : SceneManager.GetActiveScene().name != HomeLevel) return;
            if (!NetworkManager.ConnectedClients.TryGetValue(sender, out NetworkClient client) || client.PlayerObject == null) return;
            NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();
            if (player == null || player.IsDead || player.Carrier == null || player.Carrier.IsRagdolled) return;
            PlayerCarrier carrier = player.Carrier;
            float reach = carrier.Config.Reach + carrier.Config.ReachTolerance;
            if ((carrier.EyePosition - intake.transform.position).sqrMagnitude > reach * reach) return;
            int mask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris);
            if (Physics.Linecast(carrier.EyePosition, intake.transform.position, out RaycastHit obstruction, mask, QueryTriggerInteraction.Ignore)
                && obstruction.collider.GetComponentInParent<ScrapDepot>() != intake
                && !obstruction.collider.transform.IsChildOf(intake.ReceivingBay.transform)) return;

            var candidates = new HashSet<LootItem>(intake.ItemsInside());
            if (carrier.Held != null && carrier.Held.TryGetComponent(out LootItem held)) candidates.Add(held);
            foreach (Grabbable pocket in carrier.Inventory.Items)
                if (pocket != null && pocket.TryGetComponent(out LootItem item)) candidates.Add(item);

            int count = 0;
            float weight = 0f;
            foreach (LootItem item in candidates)
            {
                if (item == null || item.IsShattered || item.Definition == null) continue;
                NetworkLoot loot = item.GetComponent<NetworkLoot>();
                if (loot == null || !loot.IsSpawned || !loot.IsServer || loot.NetworkManager != NetworkManager) continue;
                Grabbable grabbable = loot.Grabbable;
                if (grabbable.IsPocketed && grabbable.PocketHolder != carrier) continue;
                if (grabbable.Holder != null && grabbable.Holder != carrier) continue;
                if (grabbable.Shared != null && grabbable.Shared.CarrierCount > 0 &&
                    (grabbable.Shared.CarrierCount > 1 || !grabbable.Shared.IsCarriedBy(carrier))) continue;
                if (grabbable.Holder != carrier && grabbable.PocketHolder != carrier &&
                    !(grabbable.Shared != null && grabbable.Shared.IsCarriedBy(carrier)) && !intake.Contains(item.transform.position)) continue;

                count++;
                weight += item.Definition.GameplayWeight;
                // Despawn tears down mirrored hand/pocket/shared holds on every machine. Repeated requests
                // cannot unload the same object again, and never apply the settled haul to the ledger twice.
                loot.NetworkObject.Despawn(destroy: true);
            }
            if (count == 0) return;
            intakeItems.Value += count;
            intakeWeight.Value += weight;
            Debug.Log($"[Depot] Unloaded {count} items ({weight:0.0} kg); cargo credit remains on its existing receipt.");
        }
    }
}
