using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using Abandoned.EditorTools;
using Abandoned.Loot;
using UnityEditor;
namespace Abandoned.Hospital
{
    /// <summary>Merge only this map's entries when its builder runs; preserve other agents' catalog content.</summary>
    public static class HospitalRegistration
    {
        public static void Include(LootCatalog local, LocationDefinition location)
        {
            var global = SerializedWiring.LoadOrCreateAsset<LootCatalog>(LootCatalogBuilder.CatalogPath);
            string prefix = location.Id + "_";
            var entries = new List<LootCatalog.Entry>(global.Entries);
            entries.RemoveAll(e => e.definition != null && e.definition.Id.StartsWith(prefix, StringComparison.Ordinal));
            entries.AddRange(local.Entries);
            global.EditorSet(entries);
            EditorUtility.SetDirty(global);
            string [] ids = AssetDatabase.FindAssets("t:LocationCatalog", new []
            {
                "Assets/_Project/Data/Contracts"
            });
            if (ids.Length != 1)
            {
                throw new InvalidOperationException("The shared contract LocationCatalog must exist before rebuilding a map.");
            }
            var contracts = AssetDatabase.LoadAssetAtPath<LocationCatalog>(AssetDatabase.GUIDToAssetPath(ids [0]));
            var places = new List<LocationDefinition>(contracts.All);
            places.RemoveAll(p => p != null && p.Id == location.Id);
            places.Add(location);
            contracts.EditorSet(places.ToArray());
            EditorUtility.SetDirty(contracts);
        }
    }
}
