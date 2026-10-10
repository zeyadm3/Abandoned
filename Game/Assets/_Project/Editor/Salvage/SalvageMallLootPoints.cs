using System.Collections.Generic;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Tenant-specific recoverable stock, with hand-authored facing and clearance pockets.</summary>
    public static class SalvageMallLootPoints
    {
        private readonly struct Pad
        {
            public readonly string Id;
            public readonly Vector3 Position;
            public readonly float Yaw;
            public Pad(string id, float x, float y, float z, float yaw = 0)
            { Id = id; Position = new Vector3(x, y, z); Yaw = yaw; }
        }

        // Floors and pockets match SalvageMallDressing; aisles, ramps and demolition fences stay clear.
        private static readonly Pad[] Inventory =
        {
            new("pc_tower", 8, 0, 10, 180),
            new("motherboard", 12, 0, 11, 16),
            new("crt_monitor", 18, 0, 10, 180),
            new("led_monitor", 20, 0, 10, 180),
            new("circuit_board", 14, 0, 12, -12),
            new("server_rack", 15.1f, 0, 10, 180),
            new("copper_spool", 6, 0, 12, 0),
            new("industrial_battery", 8, 0, 12, 0),
            new("steel_filing_cabinet", 52, 0, 10, 180),
            new("ergonomic_chair", 36, 0, 10, -25),
            new("oak_conference_table", 44, 0, 10.4f, 0),
            new("modular_shelving", 52, 0, 12, 180),
            new("fabric_roll", 54, 0, 10, 90),
            new("flatpack_desk", 38, 0, 11.5f, 180),
            new("executive_chair", 40, 0, 10, 15),
            new("shrinkwrap_roll", 54, 0, 12, 0),
            new("heavy_dolly", 48, 0, 11.5f, 180),
            new("hydraulic_jack", 44, 0, 26, 90),
            new("portable_generator", 48, 0, 26, 180),
            new("angle_grinder", 44, 0, 30, 22),
            new("tool_chest", 48, 0, 30, 180),
            new("scaffolding_pipes", 44, 0, 28, 90),
            new("safety_harness", 50, 0, 28, 180),
            new("textile_bundle", 6, 0, 26, 90),
            new("tag_detacher", 8.5f, 0, 27, -12),
            new("floor_safe", 5.1f, 0, 28.5f, 90),
            new("laundry_bin", 12, 0, 26, -8),
            new("mannequin", 11, 0, 28, 90),
            new("garment_rack", 7, 0, 29.3f, 0),
            new("cashwrap_counter", 10, 0, 29.3f, 90),
            new("shipping_pallet", 25.5f, 0, 24.3f, 0),
            new("industrial_dumpster", 31.1f, 0, 25.4f, 90),
            new("pallet_jack", 28.3f, 0, 25.9f, 0),
            new("cargo_crate", 28.5f, 0, 24.15f, 0),
            new("plastic_sorting_crate", 30.25f, 0, 24.0f, 0),
            // B spans two floors; upstairs holds office furnishings and lighter material handling.
            new("ergonomic_chair", 36, 4, 10, -15),
            new("executive_chair", 40, 4, 10, 15),
            new("flatpack_desk", 44, 4, 10, 180),
            new("modular_shelving", 52, 4, 10, 180),
            new("fabric_roll", 54, 4, 10, 90),
            new("shrinkwrap_roll", 52, 4, 12, 0),
        };

        public static int Place(Transform parent)
        {
            int count = 0;
            Transform salvage = GreyboxFactory.Group("DedicatedSalvageInventory", parent);
            foreach (Pad pad in Inventory)
            {
                SalvageAssetSpec spec = SalvageAssetCatalog.Find(pad.Id);
                Point(salvage, $"Salvage_{count:00}_{spec.Id}", pad.Position, pad.Yaw,
                    spec.Tag, spec.Carry, spec.Carry, false, new[] { spec.DefinitionId });
                count++;
            }
            Transform prizes = GreyboxFactory.Group("RetainedRiskPrizes", parent);
            // Neither a former furniture jackpot nor a gallery statue sits in the relocated closed offices.
            Point(prizes, "Basement_MilitaryGenerator_Prize", new Vector3(50, -4, 30), 0,
                "stock", CarryClass.Huge, CarryClass.Huge, true, new[] { "military_generator" }, true);
            Point(prizes, "Gallery_GrandPiano_Prize", new Vector3(26, 4, 26), 90,
                "gallery", CarryClass.Huge, CarryClass.Huge, true, new[] { "grand_piano" }, true);
            count += 2;
            return count + RemainingTenantScatter(GreyboxFactory.Group("RemainingTenantScatter", parent));
        }

        private static void Point(Transform parent, string name, Vector3 position, float yaw, string tag,
            CarryClass minimum, CarryClass maximum, bool jackpot, string[] ids = null, bool featured = false)
        {
            Transform marker = GreyboxFactory.Group(name, parent);
            marker.localPosition = position;
            marker.localRotation = Quaternion.Euler(0, yaw, 0);
            marker.gameObject.AddComponent<LootSpawnPoint>().EditorSetup(tag, minimum, maximum,
                jackpot, ids, featured, useRotation: true);
        }

        private static int RemainingTenantScatter(Transform parent)
        {
            var allotted = new Dictionary<string, int>
            {
                ["cinema"] = 10,
                ["toys"] = 8,
                ["food"] = 14,
                ["office"] = 10,
                ["stock"] = 8,
            };
            var used = new Dictionary<string, int>();
            int count = 0;
            Collider[] fittings = parent.root.GetComponentsInChildren<Collider>(true);
            for (int floor = -1; floor < Floors; floor++)
            for (int z = 0; z < TilesZ; z++)
            for (int x = 0; x < TilesX; x++)
            {
                var cell = new Vector2Int(x, z);
                Zone? zone = ZoneAt(cell, floor);
                if (!zone.HasValue) continue;
                string tag = zone.Value.Tag;
                if (!allotted.TryGetValue(tag, out int cap) || (tag == "stock" && floor >= 0)) continue;
                used.TryGetValue(tag, out int population);
                if (population >= cap || IsProtected(cell, floor) || UnderFlight(cell, floor)) continue;
                if (floor == -1 && cell.x == 12 && cell.y == 7) continue;
                // Repeatable shelf-side pockets, rather than general scattering through the five salvage zones.
                Vector3 center = TileTopCenter(cell, floor);
                Vector3 selected = center;
                bool openPocket = false;
                foreach (Vector2 offset in new[] { Vector2.zero, new Vector2(1.05f, .6f),
                    new Vector2(-1.05f, -.6f), new Vector2(.65f, -1.05f), new Vector2(-.65f, 1.05f) })
                {
                    Vector3 candidate = center + new Vector3(offset.x, 0, offset.y);
                    if (!PocketAvailable(fittings, candidate, .48f)) continue;
                    selected = candidate;
                    openPocket = true;
                    break;
                }
                if (!openPocket) continue;
                Point(parent, $"SmallStock_{tag}_{floor}_{x:00}_{z:00}", selected,
                    ((x * 17 + z * 23 + floor * 11) % 4) * 90f, tag,
                    CarryClass.Pocket, CarryClass.TwoHand, false);
                used[tag] = population + 1;
                count++;
            }
            return count;
        }

        private static bool UnderFlight(Vector2Int cell, int floor) => AllFlights.Any(flight =>
            (flight.Floor == floor || flight.Floor == floor - 1) && flight.Tiles.Contains(cell));

        // This is the placement reservation policy, applied while authoring pockets beside fixed fittings.
        private static bool PocketAvailable(Collider[] fittings, Vector3 at, float radius)
        {
            foreach (Collider fitting in fittings)
            {
                if (fitting == null || fitting.isTrigger || fitting.GetComponentInParent<StructuralSection>() != null) continue;
                Bounds bounds = fitting.bounds;
                if (bounds.max.y < at.y + .15f || bounds.min.y > at.y + 1.4f) continue;
                Vector3 edge = bounds.ClosestPoint(at + Vector3.up * .5f);
                if (new Vector2(edge.x - at.x, edge.z - at.z).sqrMagnitude < radius * radius) return false;
            }
            return true;
        }
    }
}
