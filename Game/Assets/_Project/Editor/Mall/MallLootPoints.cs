using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Loot spawn points for the mall: about every other tile of each store (tagged with its kind), a few
    /// in the open concourse and walkways, one heavy-item spot per store, and the jackpot points (GDD 7.4:
    /// the gallery behind its weak floor, the furniture floor upstairs, the loading bay).
    /// </summary>
    public static class MallLootPoints
    {
        private static readonly (int floor, int x, int z, string tag)[] Jackpots =
        {
            (2, 5, 9, "gallery"),
            (1, 1, 8, "furniture"),
            (0, 10, 9, "stock"),
        };

        public static int Place(Transform parent)
        {
            int count = 0;
            // Heavy spots only where some heavy item can actually turn up (else they'd always be empty).
            var heavyTags = new System.Collections.Generic.HashSet<string>(UnityEditor.AssetDatabase.FindAssets("t:LootDefinition", new[] { LootCatalogBuilder.Folder })
                .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<LootDefinition>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
                .Where(d => d != null && !d.Jackpot && d.CarryClass == CarryClass.Heavy && d.SpawnTags != null)
                .SelectMany(d => d.SpawnTags));
            foreach ((int floor, int x, int z, string tag) in Jackpots)
                count += Point(parent, floor, new Vector2Int(x, z), tag, CarryClass.Huge, CarryClass.Huge, true, Vector2.zero);

            for (int f = 0; f < Floors; f++)
                foreach (Zone zone in ZonesByFloor[f])
                {
                    bool store = zone.Kind == Kind.Store;
                    bool heavyPlaced = !heavyTags.Contains(zone.Tag);
                    foreach (Vector2Int c in Tiles(zone))
                    {
                        if (IsVoid(c, f) || UnderFlight(c, f) || IsJackpotTile(c, f)) continue;
                        // Stores: every other tile; open areas: one in three, so they read as emptied.
                        if (store ? (c.x + c.y + f) % 2 != 0 : (c.x + 2 * c.y + f) % 3 != 0) continue;
                        Vector2 jitter = new((Hash(c, f) % 7 - 3) * 0.3f, (Hash(c, f) / 7 % 7 - 3) * 0.3f);
                        bool heavy = !heavyPlaced && !IsWalkwayTile(c, f);
                        heavyPlaced |= heavy;
                        count += Point(parent, f, c, IsWalkwayTile(c, f) ? "walkway" : zone.Tag,
                            heavy ? CarryClass.OneHand : CarryClass.Pocket, heavy ? CarryClass.Heavy : CarryClass.TwoHand, false,
                            heavy ? Vector2.zero : jitter);
                    }
                }
            return count;
        }

        private static int Point(Transform parent, int floor, Vector2Int c, string tag, CarryClass min, CarryClass max, bool jackpot, Vector2 offset)
        {
            var go = new GameObject($"LootPoint_{floor}_{c.x:00}_{c.y:00}{(jackpot ? "_J" : "")}");
            go.transform.SetParent(parent, false);
            go.transform.position = TileTopCenter(c, floor) + new Vector3(offset.x, 0f, offset.y);
            go.AddComponent<LootSpawnPoint>().EditorSetup(tag, min, max, jackpot);
            return 1;
        }

        private static System.Collections.Generic.IEnumerable<Vector2Int> Tiles(Zone zone)
        {
            for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
            for (int z = zone.Area.yMin; z < zone.Area.yMax; z++)
                yield return new Vector2Int(x, z);
        }

        private static bool UnderFlight(Vector2Int c, int floor) => Flights.Any(f => f.Floor == floor && f.Tiles.Contains(c));

        public static bool IsJackpotTile(Vector2Int c, int floor) => Jackpots.Any(j => j.floor == floor && j.x == c.x && j.z == c.y);

        private static int Hash(Vector2Int c, int floor) => Mathf.Abs((c.x * 73856093) ^ (c.y * 19349663) ^ (floor * 83492791));
    }
}
