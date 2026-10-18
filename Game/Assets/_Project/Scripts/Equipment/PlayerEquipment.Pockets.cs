using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    public partial class PlayerEquipment
    {
        private readonly NetworkList<DroppedToolState> droppedTools = new();
        private readonly Dictionary<int, DroppedTool> droppedModels = new();
        private int nextDroppedTool;
        private float nextToolSnapshot;
        public int DroppedToolCount => droppedTools.Count;

        private void SpawnPockets()
        {
            droppedTools.OnListChanged += OnDroppedToolsChanged;
            ReconcileDroppedTools();
        }
        private void DespawnPockets()
        {
            droppedTools.OnListChanged -= OnDroppedToolsChanged;
            foreach (DroppedTool tool in droppedModels.Values) if (tool != null) Destroy(tool.gameObject);
            droppedModels.Clear();
        }
        private void OnDroppedToolsChanged(NetworkListEvent<DroppedToolState> change) => ReconcileDroppedTools();
        private int DroppedCount(int index)
        {
            int count = 0;
            for (int i = 0; i < droppedTools.Count; i++) if (droppedTools[i].CatalogIndex == index) count++;
            return count;
        }
        private void ReconcileDroppedTools()
        {
            var present = new HashSet<int>();
            for (int i = 0; i < droppedTools.Count; i++)
            {
                DroppedToolState item = droppedTools[i];
                present.Add(item.Id);
                if (!droppedModels.TryGetValue(item.Id, out DroppedTool tool) || tool == null)
                {
                    if (catalog.At(item.CatalogIndex) == null) continue;
                    tool = new GameObject("Dropped " + catalog.At(item.CatalogIndex).DisplayName).AddComponent<DroppedTool>();
                    tool.Setup(this, item, IsServer);
                    droppedModels[item.Id] = tool;
                }
                if (!IsServer) tool.transform.SetPositionAndRotation(item.Position, item.Rotation);
            }
            var removed = new List<int>();
            foreach (var pair in droppedModels)
                if (!present.Contains(pair.Key)) { if (pair.Value != null) Destroy(pair.Value.gameObject); removed.Add(pair.Key); }
            foreach (int id in removed) droppedModels.Remove(id);
        }
        private void TickDroppedTools()
        {
            if (!IsServer || Time.time < nextToolSnapshot) return;
            nextToolSnapshot = Time.time + .1f;
            for (int i = droppedTools.Count - 1; i >= 0; i--)
            {
                DroppedToolState item = droppedTools[i];
                if (!droppedModels.TryGetValue(item.Id, out DroppedTool model) || model == null)
                { droppedTools.RemoveAt(i); continue; }
                item.Position = model.transform.position;
                item.Rotation = model.transform.rotation;
                if (!item.Equals(droppedTools[i])) droppedTools[i] = item;
            }
        }
        public void RequestDiscardPocket()
        {
            if (IsSpawned && IsOwner) DiscardPocketRpc();
        }
        [Rpc(SendTo.Server)]
        private void DiscardPocketRpc(RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || IsDead || carrier.IsRagdolled || player.IsHiding) return;
            if (carrier.Held != null && !carrier.Inventory.Contains(carrier.Held)) return;
            int slot = State.Active;
            int index = State[slot];
            if (index < 0)
            {
                if (carrier.Held != null) InteractionService.Handler.RequestThrow(carrier, carrier.EyeForward * carrier.Config.ThrowSpeedFor(.3f, carrier.Held.Weight));
                else InteractionService.Handler.RequestDropFromPocket(carrier);
                return;
            }
            Vector3 aim = Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;
            Pose pose = carrier.UnpocketPose(aim);
            var item = new DroppedToolState { Id = ++nextDroppedTool, CatalogIndex = index, Position = pose.position, Rotation = pose.rotation };
            state.Value = State.With(slot, -1);
            droppedTools.Add(item);
            if (droppedModels.TryGetValue(item.Id, out DroppedTool model) && model != null)
                model.Body.linearVelocity = aim * 4f + Vector3.up;
        }
        public void RequestRecoverTool(int id)
        {
            if (IsSpawned) RecoverToolRpc(id);
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RecoverToolRpc(int id, RpcParams rpcParams = default)
        {
            PlayerEquipment receiver = Of(NetworkManager, rpcParams.Receive.SenderClientId);
            if (receiver == null || receiver.IsDead || receiver.player.IsHiding || receiver.carrier.IsRagdolled || receiver.FreePocket < 0) return;
            for (int i = 0; i < droppedTools.Count; i++)
            {
                DroppedToolState item = droppedTools[i];
                if (item.Id != id || !droppedModels.TryGetValue(id, out DroppedTool model) || model == null) continue;
                float reach = receiver.carrier.Config.Reach + receiver.carrier.Config.ReachTolerance;
                if (Vector3.Distance(receiver.Eye, model.transform.position) > reach || Physics.Linecast(receiver.Eye, model.transform.position, receiver.wallMask, QueryTriggerInteraction.Ignore)) return;
                int free = receiver.FreePocket;
                droppedTools.RemoveAt(i);
                receiver.state.Value = receiver.State.With(free, item.CatalogIndex);
                return;
            }
        }
    }
}
