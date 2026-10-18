using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.9: a model that is loot never stands in a level as scenery. Players read an identical copy as a
    /// pickup and lose trust in every item when it won't lift, so each static copy is swapped
    /// (<see cref="LootLookalikeStandIns"/>). Details: Docs/LOOT_LOOKALIKES_0.24.9.md.
    /// </summary>
    public static class LootLookalikes
    {
        // These hold the rest of the stock, so they stay in place as purpose-built fixed versions
        // rather than disappearing under a sheet. The loot items themselves are unchanged.
        private static readonly Dictionary<string, string> Fixtures = new()
        {
            ["modular_shelving"] = "fixed_gondola_shelving",
            ["garment_rack"] = "fixed_garment_rail",
            ["cashwrap_counter"] = "fixed_service_counter",
            ["shipping_pallet"] = "plastic_pallet",
            ["industrial_dumpster"] = "waste_compactor"
        };

        private static HashSet<string> ids;

        public static IEnumerable<string> FixtureIds => Fixtures.Values;

        public static bool Is(string id) => id != null && Ids.Contains(id);

        public static string FixtureFor(string id) => id != null && Fixtures.TryGetValue(id, out string fixture) ? fixture : null;

        /// <summary>The loot id a scenery prefab (asset or placed instance) was made from, or null.</summary>
        public static string IdOf(GameObject prefab)
        {
            if (prefab == null) return null;
            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            if (string.IsNullOrEmpty(path)) return null;
            // Only the scenery folders: the loot prefabs themselves live elsewhere and must never match.
            bool scenery = path.StartsWith(SalvageAssetBuilder.DressingFolder + "/") ||
                path.StartsWith(SalvageExpansionModels0156.Folder + "/Prefabs/");
            string id = System.IO.Path.GetFileNameWithoutExtension(path);
            return scenery && Is(id) ? id : null;
        }

        /// <summary>The prefab the scenery copies of <paramref name="id"/> were placed from.</summary>
        public static GameObject Source(string id)
        {
            GameObject expansion = SalvageExpansionModels0156.Load(id);
            return expansion != null ? expansion
                : AssetDatabase.LoadAssetAtPath<GameObject>(SalvageAssetBuilder.DressingPath(id));
        }

        private static HashSet<string> Ids => ids ??= new HashSet<string>(
            SalvageAssetCatalog.All.Where(spec => spec.IsLoot).Select(spec => spec.Id)
                .Concat(SalvagePickables0157.Specs.Select(spec => spec.Id)));
    }
}
