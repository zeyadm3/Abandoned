using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Interrupted tenant pack-out and industrial clearance, kept outside permanent carry aisles.</summary>
    public static class SalvageMallDressing
    {
        public static void Build(Transform mall, Transform interiors)
        {
            var fit = new SalvageDressingPlacement(mall);
            Tech(fit, Group("A_ApexCircuit", interiors));
            Furniture(fit, Group("B_NordicHabitat_Ground", interiors), 0);
            Furniture(fit, Group("B_NordicHabitat_OfficeAndSamples", interiors), 1);
            Hardware(fit, Group("C_TitanTools", interiors));
            Apparel(fit, Group("D_VogueThread", interiors));
            Staging(fit, Group("E_SolutionsClearanceStaging", interiors));
        }

        public static void DryFountain(Transform mall)
        {
            Transform publicSpaces = mall.Find("MallPublicSpaces");
            if (publicSpaces == null) return;
            Transform fountain = publicSpaces.Find("Fountain");
            if (fountain == null) return;
            // Blender's meridian_props.py joins StagnantPool into each LOD. Replace its water slot;
            // separate source nodes, when present, can be removed without touching the basin collider.
            foreach (Transform child in fountain.GetComponentsInChildren<Transform>(true))
                if (child != null && child.name.Contains("StagnantPool")) Object.DestroyImmediate(child.gameObject);
            foreach (Renderer renderer in fountain.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] != null && materials[i].name.Contains("Water"))
                        materials[i] = SalvageMaterials.Get("Concrete");
                renderer.sharedMaterials = materials;
            }
            Transform old = fountain.Find("DrySalvageBasin");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform dust = Group("DrySalvageBasin", fountain);
            foreach (float x in new[] { -1.23f, 1.23f })
                Box(dust, "shallow accumulated drywall dust", new Vector3(x, .125f, 0),
                    new Vector3(.75f, .012f, 1.65f), "Concrete");
            Box(dust, "broken board offcut", new Vector3(-1.0f, .15f, .49f),
                new Vector3(.69f, .03f, .24f), "Wood", new Vector3(0, 23, 0));
            Box(dust, "discarded conduit offcut", new Vector3(.46f, .145f, -1.1f),
                new Vector3(.018f, .024f, .79f), "Copper", new Vector3(0, -16, 0));
        }

        private static void Tech(SalvageDressingPlacement fit, Transform room)
        {
            foreach (float x in new[] { 2f, 6f, 10f, 14f, 18f })
                fit.Model("modular_shelving", room, 0, x, .8f, 0);
            Transform intact = fit.DisplayCase(room, 0, 6, 2.4f, 0);
            fit.Inside("pc_tower", intact, new Vector3(-.65f, .35f, .0f));
            fit.Inside("motherboard", intact, new Vector3(.30f, .35f, .0f), 14);
            Transform smashed = fit.DisplayCase(room, 0, 12, 2.4f, 1);
            fit.Inside("led_monitor", smashed, new Vector3(-.52f, .35f, .03f), -14);
            fit.DisplayCase(room, 0, 18, 2.4f, 2);
            fit.Model("broken_glass", room, 0, 12.4f, 3.25f, 35);
            fit.Model("server_rack", room, 0, 1.1f, 9.7f, 90);
            fit.Model("server_rack", room, 0, 1.1f, 12.2f, 90);
            Transform bench = fit.Workbench(room, 0, 4, 10.2f);
            fit.Inside("circuit_board", bench, new Vector3(-.5f, .952f, .0f), 17);
            fit.Inside("motherboard", bench, new Vector3(.28f, .952f, .11f), -11);
            fit.Inside("copper_spool", bench, new Vector3(.61f, .24f, .0f));
            fit.Model("heavy_dolly", room, 0, 2.1f, 14.6f, -18);
            fit.Model("plastic_sorting_crate", room, 0, 3.15f, 14.5f, 0);
            fit.Model("demolition_fence", room, 0, 5.25f, 13.8f);
            fit.Model("demolition_fence", room, 0, 7.5f, 14.65f, 90);
            fit.Model("exposed_partition", room, 0, 11.6f, 15.5f);
            fit.Model("dust_sheet", room, 0, 5.8f, 15.1f);
            fit.Model("orange_netting", room, 0, 11.6f, 14.8f);
            fit.Model("work_floodlight", room, 0, 4, 12, 145);
            fit.Model("cable_bundle", room, 0, .55f, 10.8f);
            fit.Tag(room, 0, 5.22f, 13.51f, "HOLD - RECOVER\nHIGH VALUE", 180);
            fit.Tag(room, 0, 22.6f, 8.9f, "SALVAGE FIRST", 270);
            Transform wires = fit.Assembly(room, "DisconnectedDisplayCables", 0, 12, 2.4f, ceiling: true);
            GameObject cable = fit.Inside("cable_bundle", wires, new Vector3(0, 3.1f, 0));
            if (cable != null) cable.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        private static void Furniture(SalvageDressingPlacement fit, Transform room, int floor)
        {
            foreach (float x in new[] { 34.7f, 40.8f, 46.8f, 52.8f })
                fit.Model("modular_shelving", room, floor, x, .75f);
            foreach (float x in new[] { 34.7f, 46.8f, 52.8f })
                fit.Model("fabric_roll", room, floor, x, .75f, 90, 1.225f);
            fit.Model("flatpack_desk", room, floor, 36, 2.5f);
            fit.Model("flatpack_desk", room, floor, 42, 2.5f);
            fit.Model("ergonomic_chair", room, floor, 36, 3.45f, 180);
            fit.Model("shrinkwrap_roll", room, floor, 44.1f, 1.75f);
            fit.Model("heavy_dolly", room, floor, 55.1f, 14.35f, -20);
            foreach (float x in new[] { 42.8f, 46.8f })
                fit.Model("modular_shelving", room, floor, x, 15.2f, 180);
            fit.Model("exposed_partition", room, floor, 52.8f, 15.5f);
            fit.Model("dust_sheet", room, floor, 49.8f, 15.15f);
            fit.Model("orange_netting", room, floor, 52.8f, 14.5f);
            fit.Model("plastic_sorting_crate", room, floor, 54.3f, 9.2f);
            fit.Model("plastic_sorting_crate", room, floor, 54.3f, 9.2f, 0, .32f);
            fit.Tag(room, floor, 51.9f, 14.20f, "ZONE B\nCLEAR FOR DEMO", 180);
            fit.Tag(room, floor, 33.1f, 8.8f, "NORDIC HABITAT\nSALVAGE FIRST", 90);
            if (floor == 0)
            {
                fit.Model("oak_conference_table", room, floor, 49.6f, 2.5f);
                foreach (float z in new[] { 1.8f, 9.3f, 12.0f })
                    fit.Model("steel_filing_cabinet", room, floor, 55.15f, z, 270);
                // 0.24.9: the tipped executive chair was a loot look-alike; packaging is left where it lay.
                Transform tipped = fit.Assembly(room, "TippedExecutiveChair", floor, 39.5f, 2.5f);
                fit.Inside("executive_chair", tipped, Vector3.zero, 12);
                fit.Tag(room, floor, 49.5f, 1.84f, "HOLD - RECOVER\nHIGH VALUE", 180, .75f);
                fit.Tracks(room, floor, 47.2f, 6.0f, 90);
                fit.Tracks(room, floor, 43.2f, 6.0f, 90);
                fit.Tracks(room, floor, 51.3f, 6.6f, 38);
            }
            else
            {
                fit.Model("flatpack_desk", room, floor, 48, 2.5f);
                fit.Model("ergonomic_chair", room, floor, 42, 3.45f, 180);
                fit.Model("ergonomic_chair", room, floor, 48, 3.45f, 160);
                fit.Model("cargo_crate", room, floor, 54, 14.6f, -6);
                Transform samples = fit.Workbench(room, floor, 53.2f, 2.6f);
                Box(samples, "loose material swatches", new Vector3(.1f, .956f, .1f), new Vector3(.26f, .01f, .19f), "Fabric");
                Box(samples, "interrupted sample order", new Vector3(-.4f, .957f, .05f), new Vector3(.24f, .003f, .32f), "Paper", new Vector3(0, -17, 0));
            }
        }

        private static void Hardware(SalvageDressingPlacement fit, Transform room)
        {
            Transform bench = fit.Workbench(room, 0, 42.8f, 18.2f);
            fit.Inside("angle_grinder", bench, new Vector3(-.65f, .952f, -.05f), 20);
            fit.Inside("angle_grinder", bench, new Vector3(.23f, .952f, .07f), -35);
            fit.Inside("hydraulic_jack", bench, new Vector3(.63f, .24f, .10f), 90);
            fit.Model("tool_chest", room, 0, 49.8f, 18.3f, 180);
            fit.Model("portable_generator", room, 0, 53.0f, 18.2f, 15);
            fit.Model("scaffolding_pipes", room, 0, 54.8f, 27.0f);
            fit.Model("scaffolding_pipes", room, 0, 54.8f, 30.0f);
            fit.Model("demolition_fence", room, 0, 52.8f, 30.85f);
            fit.Model("demolition_fence", room, 0, 54.1f, 29.45f, 90);
            fit.Model("demolition_fence", room, 0, 51.4f, 29.45f, 90);
            foreach (float x in new[] { 52.15f, 52.8f, 53.45f })
                fit.Model("safety_harness", room, 0, x, 30.79f, 180, .85f);
            fit.Model("exposed_partition", room, 0, 46.0f, 31.55f);
            fit.Model("orange_netting", room, 0, 42.7f, 30.8f);
            fit.Model("plastic_sorting_crate", room, 0, 51.85f, 26.0f);
            fit.Model("work_floodlight", room, 0, 53, 29, 215);
            fit.Model("cable_bundle", room, 0, 55.35f, 18.3f);
            fit.Model("cable_bundle", room, 0, 55.35f, 26.0f);
            fit.Tag(room, 0, 52.85f, 30.56f, "TOOL CRIB\nRECOVERY HOLD", 180);
            fit.Tag(room, 0, 40.7f, 18.8f, "TITAN TOOLS\nSALVAGE FIRST", 90);
            Transform marking = fit.Assembly(room, "HardwareHazardFloorBorder", 0, 42.8f, 18.2f);
            foreach (float side in new[] { -1f, 1f })
            {
                Box(marking, "yellow taped bench boundary", new Vector3(0, .007f, side * .69f), new Vector3(2.7f, .004f, .06f), "Safety");
                for (int i = 0; i < 8; i++)
                    Box(marking, "hazard tape diagonal", new Vector3(-1.15f + i * .32f, .010f, side * .69f), new Vector3(.07f, .002f, .073f), "Rubber", new Vector3(0, 35, 0));
            }
        }

        private static void Apparel(SalvageDressingPlacement fit, Transform room)
        {
            fit.Model("garment_rack", room, 0, 4, 18.3f, -4);
            fit.Model("garment_rack", room, 0, 10, 18.3f, 6);
            fit.Model("garment_rack", room, 0, .8f, 26, 90);
            fit.Model("garment_rack", room, 0, .8f, 30, 90);
            foreach (float z in new[] { 18.3f, 19.35f, 26.0f, 27.2f })
                fit.Model("mannequin", room, 0, 14.4f, z, 90);
            fit.Model("laundry_bin", room, 0, 2.6f, 25.8f, -15);
            fit.Model("laundry_bin", room, 0, 2.9f, 27.1f, 9);
            Transform till = fit.Assembly(room, "OverturnedCashWrap", 0, 3.2f, 29.4f, 8);
            GameObject counter = fit.Inside("cashwrap_counter", till, new Vector3(0, .92f, 0));
            if (counter != null) counter.transform.localRotation = Quaternion.Euler(0, 0, 90);
            fit.Inside("tag_detacher", till, new Vector3(.29f, .03f, -.68f), 32);
            foreach (float x in new[] { 12.2f, 14.0f })
            {
                Transform bales = fit.Assembly(room, "HalfPackedTextilePallet", 0, x, 30.3f);
                fit.Inside("shipping_pallet", bales, Vector3.zero);
                fit.Inside("textile_bundle", bales, new Vector3(-.22f, .15f, 0), 90);
                fit.Inside("textile_bundle", bales, new Vector3(.25f, .53f, .03f), 90);
            }
            fit.Model("exposed_partition", room, 0, 8, 31.55f);
            fit.Model("orange_netting", room, 0, 2.7f, 31.15f);
            fit.Model("demolition_fence", room, 0, 12.5f, 28.8f, 90);
            fit.Model("dust_sheet", room, 0, 11.8f, 31.5f);
            fit.Tag(room, 0, 14.65f, 18.6f, "VOGUE THREAD\nTEXTILES / BALE", 90);
            fit.Tag(room, 0, 3.15f, 30.86f, "DEMOLITION\nSIDE BAY", 180);
            fit.Tracks(room, 0, 8, 22, 90);
            fit.Tracks(room, 0, 12, 22, 90);
        }

        private static void Staging(SalvageDressingPlacement fit, Transform room)
        {
            fit.Model("work_floodlight", room, 0, 26, 21.8f, 165);
            fit.Model("work_floodlight", room, 0, 30, 21.8f, 195);
            GameObject hub = fit.Model("generator_hub", room, 0, 29.5f, 19.6f, 0);
            if (hub != null) fit.Sign(hub.transform, new Vector3(-.48f, .89f, .443f), 0, "LIVE TEMP POWER", .39f, .075f);
            foreach (Vector2 at in new[] { new Vector2(25.9f, 19.8f), new Vector2(25.9f, 22.9f) })
            {
                Transform stack = fit.Assembly(room, "PackedExtractionPallet", 0, at.x, at.y);
                fit.Inside("shipping_pallet", stack, Vector3.zero);
                fit.Inside("cargo_crate", stack, new Vector3(0, .15f, 0), 180);
                fit.Sign(stack, new Vector3(-.12f, .93f, -.48f), 180,
                    at.y < 21 ? "TECH / TEST" : "HOLD / FRAGILE", .34f, .14f);
            }
            // Remaining cargo and the second dumpster are recoverable stock at dedicated E pads.
            fit.Model("cargo_crate", room, 0, 32.5f, 27.6f, 180);
            foreach (float lift in new[] { 0f, .15f, .30f, .45f })
                fit.Model("shipping_pallet", room, 0, 31.5f, 21.0f, 0, lift);
            foreach (float lift in new[] { 0f, .32f, .64f })
            {
                fit.Model("plastic_sorting_crate", room, 0, 24.65f, 20.1f, 180, lift);
                fit.Model("plastic_sorting_crate", room, 0, 31.4f, 22.0f, 180, lift);
            }
            fit.Model("pallet_jack", room, 0, 27.9f, 20.7f, 90);
            fit.Model("industrial_dumpster", room, 0, 24.75f, 27.1f, 90);
            fit.Model("broken_glass", room, 0, 27.1f, 29.1f, 15, .14f);
            fit.Model("scaffolding_pipes", room, 0, 28.9f, 29, 0, .14f);
            fit.Model("circuit_board", room, 0, 26.95f, 28.7f, 18, .16f);
            fit.Model("cable_bundle", room, 0, 30.7f, 20.25f, 0);
            fit.Model("cable_bundle", room, 0, 28.7f, 20.7f, 90);
            fit.Tag(room, 0, 25.53f, 27.1f, "METAL / WEIGH", 90, .92f);
            Transform kiosk = fit.Assembly(room, "ClearanceCheckInFascia", 0, 28, 17.8f);
            fit.Sign(kiosk, new Vector3(0, 2.78f, -1.135f), 180,
                "SOLUTIONS INC.\nCLEARANCE DIVISION", 2.65f, .36f);
            fit.Tracks(room, 0, 28, 24.6f);
        }
    }
}
