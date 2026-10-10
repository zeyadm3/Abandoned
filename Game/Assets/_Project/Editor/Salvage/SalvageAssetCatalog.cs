using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using static Abandoned.Interaction.CarryClass;
using static Abandoned.Loot.SalvageCategory;

namespace Abandoned.EditorTools
{
    public static class SalvageAssetCatalog
    {
        public static readonly SalvageAssetSpec[] All =
        {
            new("pc_tower", "PC Tower", Tech, 12, 900, 1500, TwoHand, "electronics", SurfaceMaterial.Metal, Fragility.Medium),
            new("motherboard", "Motherboard", Tech, .6f, 180, 450, Pocket, "electronics", SurfaceMaterial.Plastic, Fragility.High, hasLods:false),
            new("crt_monitor", "CRT Monitor", Tech, 24, 300, 800, TwoHand, "electronics", SurfaceMaterial.Glass, Fragility.High),
            new("led_monitor", "LED Monitor", Tech, 5, 200, 600, TwoHand, "electronics", SurfaceMaterial.Glass, Fragility.High),
            new("circuit_board", "Loose Circuit Board", Tech, .2f, 80, 220, Pocket, "electronics", SurfaceMaterial.Plastic, Fragility.High, hasLods:false),
            new("server_rack", "Recoverable Server Rack", Tech, 150, 6000, 10000, Huge, "electronics", SurfaceMaterial.Metal),
            new("copper_spool", "Copper Spool", Material, 22, 1200, 3500, TwoHand, "electronics", SurfaceMaterial.Metal),
            new("industrial_battery", "Industrial Battery", Tech, 55, 1200, 2400, Heavy, "electronics", SurfaceMaterial.Metal),
            new("steel_filing_cabinet", "Steel Filing Cabinet", Furniture, 60, 600, 1500, Heavy, "furniture", SurfaceMaterial.Metal),
            new("ergonomic_chair", "Ergonomic Chair", Furniture, 14, 200, 800, TwoHand, "furniture", SurfaceMaterial.Fabric),
            new("oak_conference_table", "Oak Conference Table", Furniture, 120, 3500, 7000, Huge, "furniture", SurfaceMaterial.Wood),
            new("modular_shelving", "Modular Shelving Unit", Furniture, 50, 500, 1400, Heavy, "furniture", SurfaceMaterial.Metal),
            new("fabric_roll", "Fabric Roll", Material, 18, 150, 500, TwoHand, "furniture", SurfaceMaterial.Fabric),
            new("flatpack_desk", "Half-disassembled Flat-pack Desk", Furniture, 20, 300, 1000, TwoHand, "furniture", SurfaceMaterial.Wood),
            new("executive_chair", "Executive Chair", Furniture, 20, 300, 1000, TwoHand, "furniture", SurfaceMaterial.Fabric),
            new("shrinkwrap_roll", "Shrink-wrap Roll", Material, 2, 40, 90, OneHand, "furniture", SurfaceMaterial.Plastic, hasLods:false),
            new("heavy_dolly", "Heavy-duty Dolly", Hardware, 25, 250, 700, TwoHand, "furniture", SurfaceMaterial.Metal),
            new("hydraulic_jack", "Hydraulic Jack", Hardware, 20, 300, 800, TwoHand, "hardware", SurfaceMaterial.Metal),
            new("portable_generator", "Portable Generator", Hardware, 70, 1800, 4000, Heavy, "hardware", SurfaceMaterial.Metal),
            new("angle_grinder", "Angle Grinder", Hardware, 3, 150, 600, OneHand, "hardware", SurfaceMaterial.Metal),
            new("tool_chest", "Tool Chest", Hardware, 65, 1200, 3000, Heavy, "hardware", SurfaceMaterial.Metal),
            new("scaffolding_pipes", "Scaffolding Pipe Bundle", Material, 26, 500, 1300, TwoHand, "hardware", SurfaceMaterial.Metal),
            new("safety_harness", "Safety Harness", Hardware, 1, 90, 250, OneHand, "hardware", SurfaceMaterial.Fabric),
            new("textile_bundle", "Compressed Textile Bundle", Material, 8, 100, 600, TwoHand, "clothing", SurfaceMaterial.Fabric),
            new("tag_detacher", "Security Tag Detacher", Hardware, .4f, 80, 180, Pocket, "clothing", SurfaceMaterial.Metal, hasLods:false),
            new("floor_safe", "Floor Safe", Hardware, 100, 3000, 7000, Huge, "clothing", SurfaceMaterial.Metal),
            new("laundry_bin", "Commercial Laundry Bin", Material, 12, 100, 350, TwoHand, "clothing", SurfaceMaterial.Fabric),
            new("mannequin", "Stripped Display Mannequin", Furniture, 9, 80, 180, TwoHand, "clothing", SurfaceMaterial.Plastic),
            new("garment_rack", "Garment Rack", Furniture, 18, 100, 400, TwoHand, "clothing", SurfaceMaterial.Metal),
            new("cashwrap_counter", "Cash-wrap Counter", Furniture, 80, 700, 1800, Heavy, "clothing", SurfaceMaterial.Wood),
            new("shipping_pallet", "Heavy Wooden Shipping Pallet", Material, 25, 50, 100, TwoHand, "concourse", SurfaceMaterial.Wood),
            new("industrial_dumpster", "Industrial Dumpster", Hardware, 220, 500, 1200, Huge, "concourse", SurfaceMaterial.Metal),
            new("pallet_jack", "Hydraulic Pallet Jack", Hardware, 70, 800, 1500, Heavy, "concourse", SurfaceMaterial.Metal),
            new("cargo_crate", "Labeled Cargo Crate", Material, 35, 200, 500, TwoHand, "concourse", SurfaceMaterial.Wood),
            new("plastic_sorting_crate", "Plastic Classification Crate", Material, 2, 50, 80, OneHand, "concourse", SurfaceMaterial.Plastic),
            Dressing("work_floodlight", "Mobile Work Floodlight", Hardware),
            Dressing("generator_hub", "Crew Generator Hub", Hardware),
            Dressing("demolition_fence", "Temporary Demolition Fence", Hardware),
            Dressing("orange_netting", "Orange Safety Netting", Material),
            Dressing("dust_sheet", "Heavy Dust Sheet", Material),
            Dressing("exposed_partition", "Exposed Drywall Partition", Material),
            Dressing("cable_bundle", "Zip-tied Power Cable Bundle", Material),
            Dressing("salvage_tag", "Salvage Status Tag", Material, false),
            Dressing("tire_track", "Forklift Tire Track", Material, false),
            Dressing("broken_glass", "Broken Display Glass", Material, false)
        };

        public static SalvageAssetSpec Find(string id) => All.FirstOrDefault(a => a.Id == id || a.DefinitionId == id);
        private static SalvageAssetSpec Dressing(string id, string name, SalvageCategory category, bool lods = true)
            => new(id, name, category, 0, 0, 0, OneHand, "concourse", SurfaceMaterial.Metal,
                isLoot:false, hasLods:lods);
    }
}
