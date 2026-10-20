using Abandoned.Networking;

namespace Abandoned.Equipment
{
    /// <summary>The training job's kit crates hand gear straight to a player (the host decides, as at the HQ rack).</summary>
    public partial class PlayerEquipment
    {
        /// <summary>
        /// Host: put catalog item <paramref name="index"/> into a free hand slot, and into the hand when the hands are
        /// empty. Already carried: that slot is chosen instead. False when every slot is taken.
        /// </summary>
        public bool ServerGive(int index)
        {
            if (!IsServer || catalog == null || catalog.At(index) == null || IsDead) return false;
            EquipState s = state.Value;
            int slot = -1;
            for (int i = 0; i < EquipState.SlotCount && slot < 0; i++) if (s[i] == index) slot = i;
            if (slot < 0)
                for (int i = 0; i < EquipState.SlotCount && slot < 0; i++)
                    if (s[i] < 0 && (carrier == null || carrier.Inventory.AtSlot(i) == null)) slot = i;
            if (slot < 0) return false;
            s = s.With(slot, index);
            if (carrier == null || carrier.Held == null && LootServerActions.SelectPocket(carrier, slot)) s.Active = (byte)slot;
            state.Value = s;
            return true;
        }

        /// <summary>
        /// Host, training: just a flashlight in the first slot. The lessons hand out the rest at the kit crates, and
        /// four places hold gear and pockets together, so the radio would only crowd them. Also used when the crew
        /// leaves training, so nothing taken from a training crate reaches a real job.
        /// </summary>
        public void ServerTrainingKit()
        {
            if (!IsServer || catalog == null) return;
            EquipState s = EquipState.Empty;
            int light = catalog.IndexOf("flashlight");
            if (light >= 0 && (carrier == null || carrier.Inventory.AtSlot(0) == null)) s = s.With(0, light);
            state.Value = s;
        }

        /// <summary>Host: a short line on this player's screen (English, worded on their machine).</summary>
        public void ServerHint(string english)
        {
            if (IsServer && IsSpawned) HintRpc(english);
        }
    }
}
