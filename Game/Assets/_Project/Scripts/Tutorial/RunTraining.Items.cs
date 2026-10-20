using System.Collections.Generic;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>Host: the training salvage (spawned by spot, found by key, put back on a chapter reset) and what the lessons watch.</summary>
    public partial class RunTraining
    {
        private readonly Dictionary<string, LootItem> items = new();
        private readonly Dictionary<string, Vector3> itemStarts = new();
        private readonly HashSet<string> heldThisStep = new();
        private readonly Dictionary<ulong, int> slotAtStart = new();
        private float lastOnSection;

        /// <summary>Host: the training item named <paramref name="key"/> (null when missing).</summary>
        public LootItem Item(string key) => key != null && items.TryGetValue(key, out LootItem item) && item != null ? item : null;

        public Vector3 ItemStart(string key) => itemStarts.TryGetValue(key, out Vector3 at) ? at : Vector3.zero;

        public bool HeldThisStep(string key) => heldThisStep.Contains(key);

        public float SecondsOffSection => Time.time - lastOnSection;

        public bool SlotChangedSinceStart
        {
            get
            {
                foreach (Equipment.PlayerEquipment e in Equipment.PlayerEquipment.All)
                    if (e != null && e.IsSpawned && e.NetworkManager == NetworkManager &&
                        slotAtStart.TryGetValue(e.OwnerClientId, out int slot) && slot != e.State.Active) return true;
                return false;
            }
        }

        // chapter -1: every spot (the run's start).
        private void SpawnItems(int chapter)
        {
            NetworkLootSpawner spawner = NetworkLootSpawner.Current;
            if (spawner == null || spawner.Catalog == null)
            {
                Debug.LogError("[Training] No loot spawner in the level: the lessons have nothing to carry.");
                return;
            }
            int seed = TutorialConfig.Current.Seed;
            foreach (TutorialLootSpot spot in TutorialLootSpot.All)
            {
                if (spot == null || chapter >= 0 && spot.Chapter != chapter) continue;
                SpawnAt(spawner, spot, seed);
            }
        }

        private void SpawnAt(NetworkLootSpawner spawner, TutorialLootSpot spot, int seed)
        {
            LootDefinition definition = Definition(spawner.Catalog, spot.DefinitionId);
            if (definition == null)
            {
                Debug.LogError($"[Training] '{spot.Key}' wants loot '{spot.DefinitionId}', which isn't in the loot catalog.");
                return;
            }
            Vector3 at = spot.transform.position + Vector3.up * (definition.Size.y * 0.5f + 0.02f);
            // A stable value per item, so every run of the training house pays the same.
            int valueSeed = (seed ^ StableHash(spot.Key)) & 0x7fffffff;
            LootItem item = spawner.SpawnExtra(definition, at, Quaternion.Euler(0f, spot.transform.eulerAngles.y, 0f), valueSeed);
            if (item == null) return;
            items[spot.Key] = item;
            itemStarts[spot.Key] = at;
        }

        // FNV-1a: string.GetHashCode may differ between runtimes, and a seed must not.
        private static int StableHash(string text)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in text) hash = (hash ^ c) * 16777619;
                return (int)hash;
            }
        }

        private static LootDefinition Definition(LootCatalog catalog, string id)
        {
            foreach (LootCatalog.Entry e in catalog.Entries)
                if (e.definition != null && e.definition.Id == id) return e.definition;
            return null;
        }

        // A reset puts back what this chapter needs: its own salvage, and anything its lessons name that was lost or
        // broken. What is already aboard the truck stays aboard.
        private void ResetChapterItems(int chapter)
        {
            NetworkLootSpawner spawner = NetworkLootSpawner.Current;
            if (spawner == null) return;
            var named = new HashSet<string>();
            foreach (TutorialStepDefinition s in Catalog.Steps)
                if (s != null && s.Chapter == chapter && !string.IsNullOrEmpty(s.Key)) named.Add(s.Key);
            foreach (TutorialLootSpot spot in TutorialLootSpot.All)
            {
                if (spot == null) continue;
                LootItem existing = Item(spot.Key);
                bool own = spot.Chapter == chapter, needed = named.Contains(spot.Key) && (existing == null || existing.IsShattered);
                if (!own && !needed) continue;
                if (existing != null && Aboard(existing)) continue;
                if (existing != null && existing.TryGetComponent(out Unity.Netcode.NetworkObject no) && no.IsSpawned) no.Despawn(true);
                SpawnAt(spawner, spot, TutorialConfig.Current.Seed);
            }
        }

        private static bool Aboard(LootItem item)
        {
            Extraction.TruckCargo truck = Extraction.TruckCargo.Current;
            return truck != null && truck.ItemsInside() is IReadOnlyCollection<LootItem> inside && Contains(inside, item);
        }

        private static bool Contains(IReadOnlyCollection<LootItem> list, LootItem item)
        {
            foreach (LootItem l in list) if (l == item) return true;
            return false;
        }

        /// <summary>Host: is this item in hands or pockets (alone or shared)?</summary>
        public static bool IsHeld(LootItem item)
        {
            if (item == null || !item.TryGetComponent(out Grabbable g)) return false;
            return g.Holder != null || g.IsPocketed || g.Shared != null && (g.Shared.IsLifted || g.Shared.IsDragged);
        }

        // Each lesson watches from its own start: who holds what, which slot each hand is on, who stands where.
        private void CaptureBaselines(TutorialStepDefinition s)
        {
            heldThisStep.Clear();
            foreach (KeyValuePair<string, LootItem> pair in items)
                if (IsHeld(pair.Value)) heldThisStep.Add(pair.Key);
            slotAtStart.Clear();
            foreach (Equipment.PlayerEquipment e in Equipment.PlayerEquipment.All)
                if (e != null && e.IsSpawned && e.NetworkManager == NetworkManager) slotAtStart[e.OwnerClientId] = e.State.Active;
            lastOnSection = Time.time;
        }

        private void TickWorldState()
        {
            foreach (KeyValuePair<string, LootItem> pair in items)
                if (IsHeld(pair.Value)) heldThisStep.Add(pair.Key);
            TutorialStepDefinition s = CurrentStep;
            if (s != null && s.Check == TutorialCheck.OffSection && AnyoneOn(TutorialSection.Find(s.Key))) lastOnSection = Time.time;
            CatchFallers(s);
        }

        // Nobody is lost in training: anyone who falls out of the world is put back where the chapter starts.
        private void CatchFallers(TutorialStepDefinition s)
        {
            const float Lowest = -8f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.NetworkManager != NetworkManager) continue;
                Vector3 at = p.Ragdoll != null && p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;
                if (at.y > Lowest) continue;
                TutorialSpot spot = TutorialSpot.ForChapter(s != null ? s.Chapter : 0);
                if (spot != null) p.ServerRespawn(spot.Pose);
            }
        }

        /// <summary>Host: is a living player standing on this section?</summary>
        public bool AnyoneOn(StructuralSection section)
        {
            if (section == null || section.IsCollapsed) return false;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.NetworkManager != NetworkManager || p.IsDead) continue;
                Vector3 feet = p.Ragdoll != null && p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;
                if (SectionQuery.Under(feet) == section) return true;
            }
            return false;
        }

        private string DebugState()
        {
            int present = 0;
            foreach (LootItem item in items.Values) if (item != null) present++;
            string visitorState = visitor != null && visitor.IsSpawned ? leaveAt >= 0f ? "leaving" : "inside" : visitorCame ? "gone" : "none";
            return $"items {present}/{items.Count}  held {heldThisStep.Count}  visitor {visitorState}  armed {(armed != null ? armed.name : "-")}  off-section {SecondsOffSection:0.0}s";
        }
    }
}
