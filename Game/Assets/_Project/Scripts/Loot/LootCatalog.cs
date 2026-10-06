using System;
using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// Every loot definition with its generated prefab, for spawning at runtime (players have no
    /// AssetDatabase). Written by the loot prefab generator; one asset in Data/Loot.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Loot/Loot Catalog", fileName = "LootCatalog")]
    public class LootCatalog : ScriptableObject, IValidatable
    {
        [Serializable]
        public struct Entry
        {
            public LootDefinition definition;
            public GameObject prefab;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        public GameObject PrefabFor(LootDefinition definition)
        {
            foreach (Entry e in entries) if (e.definition == definition) return e.prefab;
            return null;
        }

        public void Validate(List<string> errors)
        {
            if (entries.Count == 0) errors.Add($"{name}: empty (run Tools/Abandoned/Generate Loot Prefabs).");
            foreach (Entry e in entries)
                if (e.definition == null || e.prefab == null) errors.Add($"{name}: an entry is missing its definition or prefab.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<Entry> newEntries) => entries = newEntries;
#endif
    }
}
