using Abandoned.Company;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// Gear moving between crewmates mid-job (QA D-10): the give-gear key hands the item in your active slot
    /// to the crewmate in front of you, or takes a fallen crewmate's gear into your empty slots. And a solo
    /// player doesn't spend a hand slot on the trolley (QA D-09): if the company owns one, it rides along.
    /// The host checks reach, sight and slots; company stock is unchanged (the item only changes hands).
    /// </summary>
    public partial class PlayerEquipment
    {
        [Tooltip("How close (m) a crewmate must be to hand gear over or take a fallen one's.")]
        [SerializeField, Min(0.5f)] private float giveReach = 2.5f;

        /// <summary>Solo Heavy drags: a trolley in hand, or (alone) one the company owns (QA D-09).</summary>
        private bool TrolleyAvailable
        {
            get
            {
                if (!CompanyService.RulesApply || Has(EquipmentKind.HandTrolley)) return true;
                CompanyService company = CompanyService.Current;
                return company != null && CompanyService.CrewSize <= 1 && company.OwnedCount(catalog.IndexOf("hand_trolley")) > 0;
            }
        }

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
                HintRpc("Face a crewmate to hand them your gear, or a fallen one to take theirs.");
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
                HintRpc("Nothing in your hand to give. Pick a slot with 1 or 2.");
                return;
            }
            int free = other.state.Value[0] < 0 ? 0 : other.state.Value[1] < 0 ? 1 : -1;
            if (free < 0)
            {
                HintRpc("Their hands are full.");
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
            for (int from = 0; from < 2; from++)
            {
                int index = fallen.state.Value[from];
                if (index < 0) continue;
                int free = state.Value[0] < 0 ? 0 : state.Value[1] < 0 ? 1 : -1;
                if (free < 0) break;
                state.Value = state.Value.With(free, index);
                EquipState theirs = fallen.state.Value.With(from, -1);
                if (catalog.At(index) is EquipmentDefinition d && d.Kind == EquipmentKind.Flashlight) theirs.LightOn = false;
                fallen.state.Value = theirs;
                took = true;
            }
            HintRpc(took ? "You took their gear." : fallen.state.Value[0] < 0 && fallen.state.Value[1] < 0 ? "They had nothing on them." : "Your hands are full.");
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
