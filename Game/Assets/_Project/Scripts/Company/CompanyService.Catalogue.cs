using System;
using Abandoned.Extraction;
using Unity.Collections;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>
    /// The loot catalogue (0.18.7): every kind of loot this company has sold, kept in the save and mirrored to
    /// the crew so the HQ's catalogue book and trophy shelf can show it.
    /// </summary>
    public partial class CompanyService
    {
        private readonly NetworkList<FixedString64Bytes> catalogue = new();

        /// <summary>Every machine: the catalogue changed (after a sale, or once synced on joining).</summary>
        public static event Action CatalogueChanged;

        public int CatalogueCount => catalogue.Count;

        public bool Catalogued(string lootId)
        {
            foreach (FixedString64Bytes id in catalogue) if (id.ToString() == lootId) return true;
            return false;
        }

        // Host: newly sold kinds into the save and the mirror.
        private void RecordCatalogue(RunResults results)
        {
            if (save == null) return;
            foreach (RunResults.Item item in results.Items)
                if (!string.IsNullOrEmpty(item.Id) && item.FinalValue > 0 && !save.catalogue.Contains(item.Id)) save.catalogue.Add(item.Id);
            SyncCatalogue();
        }

        // Host, from Update: the mirror catches up with the save (on load, and after a sale).
        private void SyncCatalogue()
        {
            if (!IsServer || !IsSpawned || save == null || catalogue.Count == save.catalogue.Count) return;
            catalogue.Clear();
            foreach (string id in save.catalogue) catalogue.Add(new FixedString64Bytes(id));
        }

        private void HookCatalogue() => catalogue.OnListChanged += OnCatalogueChanged;
        private void UnhookCatalogue() => catalogue.OnListChanged -= OnCatalogueChanged;
        private static void OnCatalogueChanged(NetworkListEvent<FixedString64Bytes> _) => CatalogueChanged?.Invoke();
    }
}
