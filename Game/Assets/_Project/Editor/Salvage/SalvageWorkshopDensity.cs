using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Loaded stock walls and interrupted pack-out pockets for Titan Tools and Vogue Thread.</summary>
    public static class SalvageWorkshopDensity
    {
        public static void Build(SalvageDensityPlacement fit)
        {
            Hardware(fit, fit.Zone("C_TitanTools_DenseRecoveryStock"));
            Apparel(fit, fit.Zone("D_VogueThread_DensePackOut"));
        }

        private static void Hardware(SalvageDensityPlacement fit, Transform room)
        {
            // A full stock wall reads at eye height while the old central forklift lane stays open.
            foreach (Vector3 shelf in new[]
            {
                new Vector3(46.0f, 0, 17.0f), new Vector3(47.5f, 0, 17.0f),
                new Vector3(51.2f, 0, 17.0f), new Vector3(54.4f, 0, 17.0f),
                new Vector3(41.0f, 90, 26.7f), new Vector3(41.0f, 90, 29.0f),
                new Vector3(55.2f, 90, 24.6f), new Vector3(50.0f, 180, 31.1f)
            })
                LoadedToolShelf(fit, room, shelf.x, shelf.z, shelf.y);

            // Supported stock on the original benches makes the pre-existing fixtures feel in use.
            fit.Prop("plastic_sorting_crate", room, 0, 42.9f, 18.25f, 0, .952f, true);
            fit.Prop("safety_harness", room, 0, 43.35f, 18.3f, 160, .952f, true);
            fit.Prop("angle_grinder", room, 0, 42.15f, 18.1f, -34, .952f, true);
            fit.Prop("shrinkwrap_roll", room, 0, 43.68f, 18.0f, 0, .952f, true);
            fit.Prop("plastic_sorting_crate", room, 0, 49.8f, 18.30f, 180, 1.03f, true);
            fit.Prop("angle_grinder", room, 0, 49.8f, 18.30f, 35, 1.08f, true);

            RecoveryPallet(fit, room, 52.4f, 24.8f, true);
            RecoveryPallet(fit, room, 45.7f, 18.9f, false);
            fit.Prop("tool_chest", room, 0, 51.3f, 18.35f, 180);
            fit.Prop("heavy_dolly", room, 0, 41.0f, 25.0f, 100);
            fit.Prop("cargo_crate", room, 0, 50.2f, 25.0f, 6);

            // The caged crib already has a physical perimeter. Fill its inside, leaving its gate pocket.
            if (fit.Prop("plastic_sorting_crate", room, 0, 52.7f, 30.1f, 0) != null)
            {
                fit.Prop("plastic_sorting_crate", room, 0, 52.7f, 30.1f, 0, .32f, true);
                fit.Prop("plastic_sorting_crate", room, 0, 52.7f, 30.1f, 0, .64f, true);
                fit.Prop("angle_grinder", room, 0, 52.7f, 30.1f, -15, .69f, true);
            }
            fit.Prop("hydraulic_jack", room, 0, 53.6f, 30.0f, 0);

            foreach (float z in new[] { 26.9f, 29.9f })
            {
                fit.Prop("scaffolding_pipes", room, 0, 54.8f, z, 0, .23f, true);
                fit.Prop("scaffolding_pipes", room, 0, 54.8f, z, 0, .46f, true);
                fit.Box(room, "Retaining timber beside pipe stack", 0,
                    new Vector3(54.47f, .25f, z), new Vector3(.08f, .5f, 1.1f), "Wood", solid: false);
            }

            // Torn packaging spreads outward from the stock piles, never creating a physics carpet.
            fit.Scatter(room, 0, new Rect(41.2f, 16.7f, 13.7f, 2.7f), 22, 155301);
            fit.Scatter(room, 0, new Rect(40.7f, 24.8f, 14.5f, 6.4f), 34, 155302);
            fit.Box(room, "Abandoned metal shelving side panel", 0,
                new Vector3(43.1f, .065f, 31.0f), new Vector3(1.38f, .05f, .60f), "Metal",
                new Vector3(0, -19, 0), false);
            fit.Box(room, "Detached drawer face", 0,
                new Vector3(50.6f, .035f, 27.0f), new Vector3(.70f, .025f, .25f), "Paint",
                new Vector3(0, 26, 0), false);
        }

        private static void LoadedToolShelf(SalvageDensityPlacement fit, Transform room, float x, float z, float yaw)
        {
            if (fit.Prop("modular_shelving", room, 0, x, z, yaw) == null) return;
            Stock(fit, room, "plastic_sorting_crate", x, z, yaw, 0, 0, .141f);
            Stock(fit, room, "hydraulic_jack", x, z, yaw, -.03f, 0, .681f, 90);
            Stock(fit, room, "angle_grinder", x, z, yaw, -.27f, 0, 1.221f, 20);
            Stock(fit, room, "shrinkwrap_roll", x, z, yaw, .30f, .01f, 1.221f);
            Stock(fit, room, "industrial_battery", x, z, yaw, -.10f, 0, 1.761f);
            Stock(fit, room, "safety_harness", x, z, yaw, .30f, .19f, .89f, 180);
        }

        private static void RecoveryPallet(SalvageDensityPlacement fit, Transform room, float x, float z, bool generator)
        {
            if (fit.Prop("shipping_pallet", room, 0, x, z) == null) return;
            fit.Prop(generator ? "portable_generator" : "hydraulic_jack", room, 0, x, z, 90, .15f, true);
            if (fit.Prop("plastic_sorting_crate", room, 0, x + .36f, z + .23f, 0,
                    generator ? .77f : .50f, true) != null)
                fit.Prop("angle_grinder", room, 0, x + .36f, z + .23f, -20, generator ? .82f : .55f, true);
        }

        private static void Apparel(SalvageDensityPlacement fit, Transform room)
        {
            foreach (Vector2 shelf in new[]
            {
                new Vector2(1.7f, 17.1f), new Vector2(6.8f, 17.1f),
                new Vector2(8.3f, 17.1f), new Vector2(12.5f, 17.1f),
                new Vector2(4.4f, 31.1f)
            })
                LoadedTextileShelf(fit, room, shelf.x, shelf.y);

            // The rails were empty in 0.15.5. Remaining garments now give them a broad broken silhouette.
            RackGarments(fit, room, 4, 18.3f, -4);
            RackGarments(fit, room, 10, 18.3f, 6);
            RackGarments(fit, room, .8f, 26, 90);
            RackGarments(fit, room, .8f, 30, 90);
            foreach (Vector3 rack in new[]
            {
                new Vector3(6.7f, -8, 18.6f), new Vector3(11.8f, 4, 18.6f),
                new Vector3(2.8f, 85, 18.8f)
            })
            {
                if (fit.Prop("garment_rack", room, 0, rack.x, rack.z, rack.y) == null) continue;
                RackGarments(fit, room, rack.x, rack.z, rack.y);
            }

            BalePallet(fit, room, 4.8f, 25.1f, 0);
            BalePallet(fit, room, 13.5f, 24.9f, 90);
            BalePallet(fit, room, 9.0f, 31.05f, 0);

            // Existing pallet stacks gain a third tier and loose small goods awaiting classification.
            foreach (float x in new[] { 12.2f, 14.0f })
            {
                fit.Prop("textile_bundle", room, 0, x + .15f, 30.3f, 90, .91f, true);
                fit.Prop("shrinkwrap_roll", room, 0, x - .35f, 30.3f, 0, .15f, true);
            }
            fit.Prop("textile_bundle", room, 0, 2.6f, 25.8f, -15, .83f, true);
            fit.Prop("textile_bundle", room, 0, 2.9f, 27.1f, 9, .83f, true);

            foreach (Vector2 at in new[] { new Vector2(6.0f, 18.9f), new Vector2(13.25f, 28.05f) })
            {
                if (fit.Prop("plastic_sorting_crate", room, 0, at.x, at.y) == null) continue;
                fit.Prop("plastic_sorting_crate", room, 0, at.x, at.y, 0, .32f, true);
                fit.Prop("plastic_sorting_crate", room, 0, at.x, at.y, 0, .64f, true);
                fit.Prop("textile_bundle", room, 0, at.x, at.y, 90, .96f, true);
            }

            fit.Prop("mannequin", room, 0, 1.6f, 18.5f, 15);
            fit.Prop("mannequin", room, 0, 13.1f, 18.4f, -40);
            fit.Prop("heavy_dolly", room, 0, 15.1f, 29.8f, 75);

            // Thin stripped display backers and leaned counter parts fill the wall without new assets.
            for (int i = 0; i < 5; i++)
                fit.Box(room, "Stripped apparel wall display backer", 0,
                    new Vector3(1.7f + i * 2.4f, 1.46f, 16.55f), new Vector3(1.65f, 1.58f, .025f),
                    i % 2 == 0 ? "Paint" : "Paper", solid: false);
            fit.Box(room, "Detached folding counter slab", 0,
                new Vector3(5.5f, .06f, 30.45f), new Vector3(1.65f, .055f, .65f), "Wood",
                new Vector3(0, -19, 0), false);
            fit.Box(room, "Leaning counter end panel", 0,
                new Vector3(15.42f, .54f, 30.5f), new Vector3(.055f, 1.08f, .72f), "Wood",
                new Vector3(0, 0, -8), false);
            fit.Box(room, "Removed rack price strip", 0,
                new Vector3(8.7f, .012f, 18.8f), new Vector3(.95f, .008f, .065f), "Paper",
                new Vector3(0, 27, 0), false);
            fit.Scatter(room, 0, new Rect(1.1f, 16.8f, 13.9f, 2.7f), 29, 155401);
            fit.Scatter(room, 0, new Rect(1.2f, 24.8f, 13.8f, 6.3f), 39, 155402);
        }

        private static void LoadedTextileShelf(SalvageDensityPlacement fit, Transform room, float x, float z)
        {
            if (fit.Prop("modular_shelving", room, 0, x, z, 180) == null) return;
            foreach (float lift in new[] { .141f, .681f, 1.221f })
                fit.Prop("textile_bundle", room, 0, x, z, 0, lift, true);
            fit.Prop("fabric_roll", room, 0, x, z, 90, 1.761f, true);
            fit.Prop("shrinkwrap_roll", room, 0, x + .35f, z, 0, 1.761f, true);
        }

        private static void BalePallet(SalvageDensityPlacement fit, Transform room, float x, float z, float yaw)
        {
            if (fit.Prop("shipping_pallet", room, 0, x, z, yaw) == null) return;
            for (int layer = 0; layer < 3; layer++)
                for (int column = 0; column < 2; column++)
                    Stock(fit, room, "textile_bundle", x, z, yaw, column == 0 ? -.27f : .27f,
                        layer % 2 == 0 ? -.06f : .05f, .15f + layer * .375f, 90);
            Stock(fit, room, "shrinkwrap_roll", x, z, yaw, -.45f, -.30f, .15f);
        }

        private static void RackGarments(SalvageDensityPlacement fit, Transform room, float x, float z, float yaw)
        {
            Quaternion rotation = Quaternion.Euler(0, yaw, 0);
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = rotation * new Vector3(-.46f + i * .22f, 1.21f + (i % 2) * .055f, 0);
                fit.Box(room, "Uncollected garment on rack", 0, new Vector3(x, 0, z) + offset,
                    new Vector3(.26f, .64f - (i % 3) * .065f, .035f), "Fabric", new Vector3(0, yaw + i * 7, 0), false);
                Vector3 sleeve = rotation * new Vector3(-.46f + i * .22f, 1.45f + (i % 2) * .055f, 0);
                fit.Box(room, "Hanging garment shoulders", 0, new Vector3(x, 0, z) + sleeve,
                    new Vector3(.37f, .11f, .03f), "Fabric", new Vector3(0, yaw + i * 7, 0), false);
            }
        }

        private static void Stock(SalvageDensityPlacement fit, Transform room, string id, float x, float z,
            float yaw, float localX, float localZ, float lift, float facing = 0)
        {
            Vector3 offset = Quaternion.Euler(0, yaw, 0) * new Vector3(localX, 0, localZ);
            fit.Prop(id, room, 0, x + offset.x, z + offset.z, yaw + facing, lift, true);
        }
    }
}
