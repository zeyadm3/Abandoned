using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Additional stock families and estate buildings, retaining every previous mall layer.</summary>
    public static class MallExpansion0156
    {
        public const string GroupName = "_SALVAGE_EXPANSION_0156";

        public static void Build(Transform mall)
        {
            Transform previous = mall.Find(GroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            SalvageExpansionModels0156.Author();
            Transform root = GreyboxFactory.Group(GroupName, mall);
            var fit = new SalvageDensityPlacement(mall, root);
            Retail(fit);
            Bulk(fit, "B_NordicNewRoomSets_Ground", 0, new Rect(32.5f, .5f, 23, 15),
                new[] { "showroom_sofa", "floor_lamp", "office_printer", "recovery_cart" }, 14);
            Bulk(fit, "B_NordicNewRoomSets_Upper", 1, new Rect(32.5f, .5f, 23, 15),
                new[] { "office_printer", "locker_bank", "showroom_sofa", "floor_lamp" }, 16);
            Bulk(fit, "C_TitanSpecialistEquipment", 0, new Rect(40.5f, 16.5f, 15, 15),
                new[] { "air_compressor", "welding_trolley", "workshop_vacuum", "duct_stack" }, 14);
            Bulk(fit, "D_VogueFootwearPackout", 0, new Rect(.5f, 16.5f, 15, 15),
                new[] { "shoe_cartons", "recovery_cart", "cash_register", "workshop_vacuum" }, 12);
            Bulk(fit, "E_ClearanceSpecialistKit", 0, new Rect(20.5f, 16.5f, 15, 15),
                new[] { "extension_reel", "mobile_planning_board", "workshop_vacuum", "duct_stack", "clearance_barricade" }, 8);
            Bulk(fit, "UpperAdministrationPackout", 1, new Rect(.5f, 16.5f, 15, 15),
                new[] { "office_printer", "locker_bank", "mobile_planning_board", "recovery_cart" }, 12);
            Bulk(fit, "BasementRecoveredPlant", -1, new Rect(40.5f, 24.5f, 15, 11),
                new[] { "air_compressor", "welding_trolley", "duct_stack", "recovery_cart" }, 10);
            int batches = SalvageDensityBatcher.Build(root, SalvageExpansionModels0156.Folder + "/SectionMeshes");
            root.gameObject.AddComponent<SalvageDensityDebug>().EditorSetup(fit.Props, fit.Details, batches);
            int buildings = MallExpansionDistrict0156.Build(root);
            Debug.Log($"[MallExpansion0156] Added {fit.Props} stock/equipment props, {fit.Details} details, " +
                $"{batches} independently supported batches and {buildings} surrounding buildings; retained 0.15.5b dressing.");
        }

        private static void Retail(SalvageDensityPlacement fit)
        {
            Transform tech = fit.Zone("A_ApexAccessoriesAndPOS");
            foreach (float x in new[] { 2f, 6f, 10f, 14f, 18f })
            {
                for (int i = 0; i < 3; i++)
                    Stock(fit, tech, "headphone_carton", 0, x - .34f + i * .34f, .80f, 1.221f);
                Stock(fit, tech, "network_switch", 0, x, .79f, 1.761f);
            }
            Stock(fit, tech, "receipt_printer", 0, 6.95f, 2.40f, .35f);
            Stock(fit, tech, "card_terminal", 0, 12.55f, 2.38f, .35f);
            Stock(fit, tech, "cash_register", 0, 18.8f, 2.4f, .35f);
            Stock(fit, tech, "network_switch", 0, 4.12f, 10.15f, .952f);
            Bulk(fit, "A_ApexAdditionalServiceStock", 0, new Rect(.5f, .5f, 23, 15),
                new[] { "recovery_cart", "office_printer", "extension_reel", "locker_bank" }, 12);
            for (int floor = 0; floor < 2; floor++)
            {
                Transform samples = fit.Zone("B_NordicOfficeAccessories_" + floor);
                foreach (float x in new[] { 36f, 42f })
                {
                    Stock(fit, samples, "receipt_printer", floor, x + .45f, 2.5f, .75f);
                    Stock(fit, samples, "card_terminal", floor, x - .2f, 2.52f, .75f);
                }
            }
            Transform apparel = fit.Zone("D_VogueCounterRemnants");
            Stock(fit, apparel, "card_terminal", 0, 3.4f, 29.0f, .03f);
            Stock(fit, apparel, "receipt_printer", 0, 3.6f, 29.2f, .03f);
            Transform workshop = fit.Zone("C_TitanElectricalStock");
            Stock(fit, workshop, "extension_reel", 0, 42.7f, 18.2f, .952f);
            Stock(fit, workshop, "network_switch", 0, 49.8f, 18.3f, 1.03f);
        }

        private static void Stock(SalvageDensityPlacement fit, Transform parent, string id, int floor,
            float x, float z, float lift)
            => fit.Model(SalvageExpansionModels0156.Load(id), parent, floor, x, z, 180, lift, true);

        private static void Bulk(SalvageDensityPlacement fit, string name, int floor, Rect area, string[] ids, int limit)
        {
            Transform zone = fit.Zone(name);
            int placed = 0, candidate = 0;
            // Placement constraints reserve the original carrying lanes, newly authored threat pockets and fittings.
            for (float z = area.yMin + .65f; z < area.yMax - .45f && placed < limit; z += 1.7f)
            for (float x = area.xMin + .65f; x < area.xMax - .45f && placed < limit; x += 1.8f)
            {
                string id = ids[candidate++ % ids.Length];
                float yaw = candidate % 2 == 0 ? 180 : 90;
                if (fit.Model(SalvageExpansionModels0156.Load(id), zone, floor, x, z, yaw) != null) placed++;
            }
        }
    }
}
