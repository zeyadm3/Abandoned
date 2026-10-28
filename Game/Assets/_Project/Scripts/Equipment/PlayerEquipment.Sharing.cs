using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// Gear moving between crewmates mid-job (QA D-10): the give-gear key hands the item in your active slot
    /// to the crewmate in front of you, or takes a fallen crewmate's gear into your empty slots.
    /// The host checks reach, sight and slots; company stock is unchanged (the item only changes hands).
    /// </summary>
    public partial class PlayerEquipment
    {
        [Tooltip("How close (m) a crewmate must be to hand gear over or take a fallen one's.")]
        [SerializeField, Min(0.5f)] private float giveReach = 2.5f;

        private void TickGiveGear(Player.PlayerInputFrame input)
        {
            if (input.GiveGearPressed) GiveGearRpc();
        }

        [Rpc(SendTo.Server)]
        private void GiveGearRpc(RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || IsDead) return;
            PlayerEquipment other = CrewmateInFront();
            if (other == null)
            {
                HintRpc(Loc.Mark("Face a crewmate to hand them your gear, or a fallen one to take theirs."));
                return;
            }
            if (other.IsDead) TakeFrom(other);
            else GiveTo(other);
        }

        private void GiveTo(PlayerEquipment other)
        {
            int slot = state.Value.Active;
            int index = state.Value[slot];
            if (index < 0)
            {
                HintRpc(Loc.Mark("Select a tool pocket to give gear."));
                return;
            }
            int free = other.FreePocket;
            if (free < 0)
            {
                HintRpc(Loc.Mark("Their hands are full."));
                return;
            }
            EquipState mine = state.Value.With(slot, -1);
            if (catalog.At(index) is EquipmentDefinition d && d.Kind == EquipmentKind.Flashlight) mine.LightOn = false;
            state.Value = mine;
            other.state.Value = other.state.Value.With(free, index);
            Debug.Log($"[Gear] {NetworkPlayer.NameOf(OwnerClientId)} handed {catalog.At(index)?.DisplayName} to {NetworkPlayer.NameOf(other.OwnerClientId)}.");
        }

        private void TakeFrom(PlayerEquipment fallen)
        {
            bool took = false;
            for (int from = 0; from < EquipState.SlotCount; from++)
            {
                int index = fallen.state.Value[from];
                if (index < 0) continue;
                int free = FreePocket;
                if (free < 0) break;
                state.Value = state.Value.With(free, index);
                EquipState theirs = fallen.state.Value.With(from, -1);
                if (catalog.At(index) is EquipmentDefinition d && d.Kind == EquipmentKind.Flashlight) theirs.LightOn = false;
                fallen.state.Value = theirs;
                took = true;
            }
            HintRpc(took ? Loc.Mark("You took their gear.") : fallen.OccupiedCount == 0 ? Loc.Mark("They had nothing on them.") : Loc.Mark("Your hands are full."));
        }

        // Host: the nearest crewmate (alive or fallen) within reach, roughly where this player is looking, in sight.
        private PlayerEquipment CrewmateInFront()
        {
            Vector3 aim = Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;
            PlayerEquipment best = null;
            float bestDistance = giveReach;
            foreach (PlayerEquipment e in Spawned)
            {
                if (e == null || e == this || e.NetworkManager != NetworkManager || e.player == null) continue;
                Vector3 at = e.player.Ragdoll.IsRagdolled ? e.player.Ragdoll.BodyPosition : e.transform.position + Vector3.up;
                Vector3 offset = at - Eye;
                float distance = offset.magnitude;
                if (distance > bestDistance || Vector3.Angle(aim, offset) > 45f) continue;
                if (Physics.Linecast(Eye, at, wallMask, QueryTriggerInteraction.Ignore)) continue;
                (best, bestDistance) = (e, distance);
            }
            return best;
        }
    }
}
