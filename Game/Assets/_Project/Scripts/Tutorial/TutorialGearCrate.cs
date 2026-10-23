using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// A kit crate in the training house: E takes one piece of gear (the hand trolley, a support jack, a medkit)
    /// straight into a free hand slot. On a real job gear comes from the HQ rack before you leave; training hands
    /// it out where the lesson needs it. Scene object; the host does the giving (<see cref="RunTraining"/>).
    /// </summary>
    public class TutorialGearCrate : MonoBehaviour, IUsable
    {
        private static readonly List<TutorialGearCrate> Live = new();

        [SerializeField, Range(0, 31)] private int index;
        [Tooltip("The equipment catalog id it hands out (hand_trolley, medkit...).")]
        [SerializeField] private string equipmentId = "hand_trolley";

        public static IReadOnlyList<TutorialGearCrate> All => Live;
        public int Index => index;
        public string EquipmentId => equipmentId;

        public static TutorialGearCrate ByIndex(int crateIndex)
        {
            foreach (TutorialGearCrate c in Live) if (c != null && c.index == crateIndex) return c;
            return null;
        }

        public string UsePrompt(GameObject user)
        {
            RunTraining training = RunTraining.Current;
            PlayerEquipment gear = user != null ? user.GetComponentInParent<PlayerEquipment>() : null;
            if (training == null || gear == null || gear.Catalog == null) return null;
            EquipmentDefinition item = gear.Catalog.At(gear.Catalog.IndexOf(equipmentId));
            if (item == null) return null;
            // "Take: Hand Trolley", not "take the ...": every language can put a name after a colon.
            return Has(gear, item) ? Loc.T("Already carried: {0}", item.LocalName) : Loc.T("Take: {0}", item.LocalName);
        }

        public void Use(GameObject user)
        {
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsSpawned || me.IsDead) return;
            RunTraining.Current?.RequestGear(index);
        }

        private static bool Has(PlayerEquipment gear, EquipmentDefinition item)
        {
            for (int slot = 0; slot < EquipState.SlotCount; slot++)
                if (gear.InSlot(slot) == item) return true;
            return false;
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

#if UNITY_EDITOR
        public void EditorSetup(int crateIndex, string id)
        {
            index = crateIndex;
            equipmentId = id;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
