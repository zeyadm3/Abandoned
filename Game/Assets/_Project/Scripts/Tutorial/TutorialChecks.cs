using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Host: has the world done what the current lesson asks? One question per <see cref="TutorialCheck"/>, read
    /// from what the host already knows (players, items, sections, doors, the truck). Personal lessons are
    /// answered on each player's own machine instead (<see cref="TutorialWatch"/>).
    /// </summary>
    public static class TutorialChecks
    {
        public static bool Done(RunTraining training, TutorialStepDefinition s)
        {
            RunState run = training.Run;
            switch (s.Check)
            {
                case TutorialCheck.Read:
                    return training.StepElapsed >= (s.Amount > 0f ? s.Amount : TutorialConfig.Current.ReadSeconds);
                case TutorialCheck.EnterArea:
                    return AnyoneIn(training, TutorialArea.Find(s.Area));
                case TutorialCheck.Flashlight:
                    foreach (PlayerEquipment e in PlayerEquipment.All)
                        if (Ours(training, e) && e.LightOn) return true;
                    return false;
                case TutorialCheck.HoldItem:
                    return RunTraining.IsHeld(training.Item(s.Key));
                case TutorialCheck.PocketItem:
                    return string.IsNullOrEmpty(s.Key) ? AnyPockets(training) : Pocketed(training.Item(s.Key));
                case TutorialCheck.DropItem:
                    // Down already (or gone) counts: a player who emptied their pockets early isn't made to do it twice.
                    return training.Item(s.Key) is not LootItem dropped || !RunTraining.IsHeld(dropped);
                case TutorialCheck.MoveItem:
                    return training.Item(s.Key) is LootItem moved && Vector3.Distance(moved.transform.position, training.ItemStart(s.Key)) >= Mathf.Max(0.5f, s.Amount);
                case TutorialCheck.ItemInArea:
                    return training.Item(s.Key) is LootItem placed && TutorialArea.Find(s.Area) is TutorialArea area && area.Contains(placed.transform.position);
                case TutorialCheck.ItemInTruck:
                    return InTruck(training, training.Item(s.Key));
                case TutorialCheck.SwitchSlot:
                    return training.SlotChangedSinceStart;
                case TutorialCheck.HasGear:
                    foreach (PlayerEquipment e in PlayerEquipment.All)
                        if (Ours(training, e) && Carries(e, s.Key)) return true;
                    return false;
                case TutorialCheck.DoorShut:
                    return RunDoors.Current != null && RunDoors.Current.IsShut(Mathf.RoundToInt(s.Amount));
                case TutorialCheck.DoorOpen:
                    return RunDoors.Current != null && RunDoors.Current.Ready && !RunDoors.Current.IsShut(Mathf.RoundToInt(s.Amount));
                case TutorialCheck.SectionStage:
                    return TutorialSection.Find(s.Key) is StructuralSection staged && (int)staged.Stage >= Mathf.RoundToInt(s.Amount);
                case TutorialCheck.OffSection:
                    return training.SecondsOffSection >= Mathf.Max(0.5f, s.Amount);
                case TutorialCheck.SectionBraced:
                    return TutorialSection.Find(s.Key) is StructuralSection braced && (braced.Reinforcement > 1f || braced.IsCollapsed);
                case TutorialCheck.SectionDown:
                    return TutorialSection.Find(s.Key) is not StructuralSection down || down.IsCollapsed;
                case TutorialCheck.Breaker:
                    return RunBreakers.Current != null && RunBreakers.Current.RestoredMask != 0;
                case TutorialCheck.Hide:
                    foreach (NetworkPlayer p in NetworkPlayer.All)
                        if (Ours(training, p) && p.IsHiding) return true;
                    return false;
                case TutorialCheck.ThreatGone:
                    return training.VisitorGone;
                case TutorialCheck.Haul:
                    return run != null && run.IsSpawned && run.State.Haul >= (s.Amount > 0f ? s.Amount : run.State.Quota);
                case TutorialCheck.Lever:
                    return run != null && run.IsSpawned && run.State.Phase != RunPhase.Running;
                case TutorialCheck.FreeSlot:
                    foreach (NetworkPlayer p in NetworkPlayer.All)
                        if (Ours(training, p) && !p.IsDead && p.Carrier != null && p.Carrier.Inventory.HasSpace) return true;
                    return false;
                default:
                    return false; // personal: reported by the player's own machine
            }
        }

        private static bool Ours(RunTraining training, Unity.Netcode.NetworkBehaviour b) =>
            b != null && b.IsSpawned && b.NetworkManager == training.NetworkManager;

        private static bool AnyoneIn(RunTraining training, TutorialArea area)
        {
            if (area == null) return false;
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (Ours(training, p) && !p.IsDead && area.Contains(p.transform.position + Vector3.up * 0.5f)) return true;
            return false;
        }

        private static bool AnyPockets(RunTraining training)
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (Ours(training, p) && p.Carrier != null && p.Carrier.Inventory.Count > 0) return true;
            return false;
        }

        private static bool Pocketed(LootItem item) =>
            item != null && item.TryGetComponent(out Interaction.Grabbable g) && g.IsPocketed;

        private static bool Carries(PlayerEquipment e, string id)
        {
            for (int slot = 0; slot < EquipState.SlotCount; slot++)
                if (e.InSlot(slot) is EquipmentDefinition d && d.Id == id) return true;
            return false;
        }

        // Aboard: in the bay, or in the hands or pockets of someone standing in it (what the truck's tally counts).
        private static bool InTruck(RunTraining training, LootItem item)
        {
            TruckCargo truck = TruckCargo.Current;
            if (truck == null || item == null) return false;
            foreach (LootItem inside in truck.ItemsInside()) if (inside == item) return true;
            if (!item.TryGetComponent(out Interaction.Grabbable g)) return false;
            Interaction.PlayerCarrier holder = g.Holder != null ? g.Holder : g.PocketHolder;
            return holder != null && truck.Carries(holder.transform.position);
        }
    }
}
