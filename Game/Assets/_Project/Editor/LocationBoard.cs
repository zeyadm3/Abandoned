using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Puts a map on the contract board: adds (or replaces, by id) its <see cref="LocationDefinition"/> in the shared
    /// LocationCatalog. The newer maps were reached by driving through the open world; since it was removed
    /// (2026-10-28) the board is the only way to them, so every map's builder registers here.
    /// </summary>
    public static class LocationBoard
    {
        public static void Register(LocationDefinition location)
        {
            if (location == null) return;
            string[] ids = AssetDatabase.FindAssets("t:LocationCatalog", new[] { "Assets/_Project/Data/Contracts" });
            if (ids.Length != 1) throw new InvalidOperationException("The shared contract LocationCatalog must exist before rebuilding a map.");
            var catalog = AssetDatabase.LoadAssetAtPath<LocationCatalog>(AssetDatabase.GUIDToAssetPath(ids[0]));
            var places = new List<LocationDefinition>(catalog.All);
            places.RemoveAll(p => p == null || p.Id == location.Id);
            places.Add(location);
            catalog.EditorSet(places.ToArray());
            EditorUtility.SetDirty(catalog);
        }
    }
}
