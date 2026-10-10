using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Additional recoverable stock beside the saved displays, with actual seeded pickup allocation.</summary>
    public static class SalvageLootPlacement0157
    {
        public const string GroupName = "_PICKABLE_STOCK_0157";
        public const float OrdinaryFillChance = .82f;

        private readonly struct Department
        {
            public readonly string Name, Tag;
            public readonly int Floor, Limit;
            public Department(string name, string tag, int floor, int limit)
            { Name = name; Tag = tag; Floor = floor; Limit = limit; }
        }

        private static readonly Department[] Departments =
        {
            new("ApexCircuit", "electronics", 0, 18),
            new("NordicHabitat_Ground", "furniture", 0, 12),
            new("NordicHabitat_Upper", "furniture", 1, 14),
            new("TitanTools", "hardware", 0, 14),
            new("VogueThread", "clothing", 0, 12),
            new("SalvageStaging", "concourse", 0, 8),
            new("BasementRecovery", "stock", -1, 12),
            new("ClosedOffices", "office", 1, 6),
            new("FoodCourtRecovery", "food", 0, 6),
            new("CinemaRecovery", "cinema", 1, 4),
            new("ToyArchiveRecovery", "toys", 1, 4),
        };

        /// <summary>Call after authoring the 0.15.7 loot catalog; only this pass's markers are replaced.</summary>
        public static int Build(Transform mall)
        {
            if (mall == null) throw new ArgumentNullException(nameof(mall));
            Transform previous = mall.Find(GroupName);
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            Transform root = GreyboxFactory.Group(GroupName, mall);
            LootCatalog catalog = AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Author the loot catalog before placing pickable stock.");
            LootDefinition[] definitions = catalog.Entries.Where(e => e.definition != null && e.prefab != null)
                .Select(e => e.definition).Where(d => !d.Utility && !d.Jackpot && d.Rarity > 0f)
                .OrderBy(d => d.Id, StringComparer.Ordinal).ToArray();
            var placement = new Placement(mall, root);
            int added = 0;
            foreach (Department department in Departments)
            {
                LootDefinition[] available = definitions.Where(d => d.HasSpawnTag(department.Tag)
                    && d.CarryClass <= CarryClass.Heavy && (d.CarryClass < CarryClass.Heavy || NewStock(d.Id)))
                    .OrderBy(d => NewStock(d.Id) ? 0 : 1)
                    .ThenBy(d => d.CarryClass >= CarryClass.Heavy ? 1 : 0)
                    .ThenBy(d => d.Id, StringComparer.Ordinal).ToArray();
                int count = placement.Fill(department, available);
                added += count;
                Debug.Log($"[Pickables0157] {department.Name}: authored {count} additional recoverable stock pockets.");
            }
            LootSpawnConfig config = AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath);
            if (config == null) throw new InvalidOperationException("The shared loot spawn configuration is missing.");
            var serialized = new SerializedObject(config);
            serialized.FindProperty("<FillChance>k__BackingField").floatValue = OrdinaryFillChance;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            int total = mall.GetComponentsInChildren<LootSpawnPoint>(true).Length;
            Debug.Log($"[Pickables0157] Authored {added} additional points, {total} total. Ordinary fill is {OrdinaryFillChance:P0}; "
                + "existing prize rolls and contract fill modifiers remain active. F1 displays the actual host-spawned count/value.");
            return added;
        }

        private static bool NewStock(string id) => id is
            "salvage_receipt_printer" or "salvage_card_terminal" or "salvage_headphone_carton" or "salvage_network_switch"
            or "salvage_showroom_sofa" or "salvage_floor_lamp" or "salvage_office_printer" or "salvage_air_compressor"
            or "salvage_welding_trolley" or "salvage_workshop_vacuum" or "salvage_shoe_cartons" or "salvage_cash_register"
            or "salvage_extension_reel" or "salvage_mobile_planning_board" or "salvage_duct_stack" or "salvage_recovery_cart";

        private sealed class Placement
        {
            private readonly Transform mall, root;
            private readonly MallRouteClearance routes;
            private readonly List<Bounds> fittings = new(), claimed = new();
            private readonly Dictionary<string, int> copies = new();
            private int heavyCount;

            public Placement(Transform mallRoot, Transform markerRoot)
            {
                mall = mallRoot;
                root = markerRoot;
                Physics.SyncTransforms();
                routes = new MallRouteClearance(mall);
                foreach (Collider collider in mall.GetComponentsInChildren<Collider>(true))
                {
                    if (!collider.enabled || collider.isTrigger || collider.GetComponentInParent<StructuralSection>() != null) continue;
                    fittings.Add(collider.bounds);
                }
            }

            public int Fill(Department department, LootDefinition[] available)
            {
                if (available.Length == 0) return 0;
                Transform group = GreyboxFactory.Group(department.Name, root);
                List<Vector3> candidates = Candidates(department);
                int count = 0;
                // The first sweep gives newly recoverable families their own locations before repeats consume bays.
                foreach (LootDefinition definition in available)
                {
                    if (count >= department.Limit) break;
                    if (TryPlace(group, department, definition, candidates, count)) count++;
                }
                LootDefinition[] light = available.Where(d => d.CarryClass < CarryClass.Heavy).ToArray();
                bool placed = true;
                while (count < department.Limit && placed)
                {
                    placed = false;
                    foreach (LootDefinition definition in light.OrderBy(d => copies.GetValueOrDefault(d.Id)))
                    {
                        if (count >= department.Limit) break;
                        if (!TryPlace(group, department, definition, candidates, count)) continue;
                        count++;
                        placed = true;
                    }
                }
                return count;
            }

            private bool TryPlace(Transform parent, Department department, LootDefinition definition,
                List<Vector3> candidates, int index)
            {
                bool heavy = definition.CarryClass >= CarryClass.Heavy;
                if (heavy && (heavyCount >= 10 || copies.GetValueOrDefault(definition.Id) >= 2)) return false;
                foreach (Vector3 at in candidates)
                foreach (float yaw in new[] { 0f, 90f })
                {
                    Vector3 size = yaw == 0f ? definition.Size : new Vector3(definition.Size.z, definition.Size.y, definition.Size.x);
                    Bounds pocket = new(at + Vector3.up * (size.y * .5f + .02f), size);
                    pocket.Expand(new Vector3(.22f, .035f, .22f));
                    if (!FitsFloor(pocket, department) || routes.Conflict(pocket) != null) continue;
                    if (fittings.Any(b => b.Intersects(pocket)) || claimed.Any(b => b.Intersects(pocket))) continue;
                    Transform point = GreyboxFactory.Group($"Recoverable_{department.Name}_{index:00}_{definition.Id}", parent);
                    point.localPosition = at;
                    point.localRotation = Quaternion.Euler(0, yaw, 0);
                    point.gameObject.AddComponent<LootSpawnPoint>().EditorSetup(department.Tag, definition.CarryClass,
                        definition.CarryClass, false, new[] { definition.Id }, useRotation: true);
                    // Leave a hand's breadth and breathing room between independent physics bodies.
                    pocket.Expand(new Vector3(.4f, 0, .4f));
                    claimed.Add(pocket);
                    copies[definition.Id] = copies.GetValueOrDefault(definition.Id) + 1;
                    if (heavy) heavyCount++;
                    return true;
                }
                return false;
            }

            private List<Vector3> Candidates(Department department)
            {
                var candidates = new List<(Vector3 at, int rank)>();
                for (int z = 0; z < MallLayout.TilesZ; z++)
                for (int x = 0; x < MallLayout.TilesX; x++)
                {
                    var cell = new Vector2Int(x, z);
                    MallLayout.Zone? zone = MallLayout.ZoneAt(cell, department.Floor);
                    if (!zone.HasValue || zone.Value.Tag != department.Tag || MallLayout.IsProtected(cell, department.Floor)) continue;
                    if (MeridianDressingUtility.Support(mall, cell, department.Floor) == null) continue;
                    // Side bays hold recoverable boxes/equipment; the authored lanes and forty threat pockets are reserved.
                    for (int row = 0; row < 4; row++)
                    for (int column = 0; column < 4; column++)
                    {
                        Vector3 at = new(x * MallLayout.Tile + .65f + column * .9f,
                            MallLayout.FloorY(department.Floor), z * MallLayout.Tile + .65f + row * .9f);
                        int rank = MeridianDressingUtility.Hash(cell, department.Floor) ^ (row * 677 + column * 359);
                        candidates.Add((at, rank));
                    }
                }
                return candidates.OrderBy(p => p.rank).Select(p => p.at).ToList();
            }

            private bool FitsFloor(Bounds bounds, Department department)
            {
                foreach (Vector3 corner in new[] { bounds.min, bounds.max,
                    new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                    new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) })
                {
                    var cell = new Vector2Int(Mathf.FloorToInt(corner.x / MallLayout.Tile), Mathf.FloorToInt(corner.z / MallLayout.Tile));
                    MallLayout.Zone? zone = MallLayout.ZoneAt(cell, department.Floor);
                    if (!zone.HasValue || zone.Value.Tag != department.Tag || MallLayout.IsProtected(cell, department.Floor)
                        || MeridianDressingUtility.Support(mall, cell, department.Floor) == null) return false;
                }
                return true;
            }
        }
    }
}
