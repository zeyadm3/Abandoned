using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Loaded clearance stacks and scrap gathered inside existing staging footprints.</summary>
    public static class SalvageConcourseDensity
    {
        public static void Build(SalvageDensityPlacement fit)
        {
            Transform staging = fit.Zone("E_LoadedClearanceStaging");
            LoadedPallets(fit, staging);
            SortingStacks(fit, staging);
            FountainScrap(fit, staging);
            InterruptedPacking(fit, staging);
        }

        private static void LoadedPallets(SalvageDensityPlacement fit, Transform parent)
        {
            // The first release already contains the pallet and lower crate in these two bays.
            // Extending those stacks gives volume without taking another carry pocket.
            foreach (float z in new[] { 19.8f, 22.9f })
            {
                fit.Prop("cargo_crate", parent, 0, 25.9f, z, 180, 1.27f, true);
                fit.Prop("plastic_sorting_crate", parent, 0, 25.68f, z, 180, 2.395f, true);
                fit.Prop("plastic_sorting_crate", parent, 0, 26.16f, z, 180, 2.395f, true);
                fit.Prop("shrinkwrap_roll", parent, 0, 25.7f, z, 0, 2.71f, true);
                fit.Prop("copper_spool", parent, 0, 26.15f, z, 0, 2.71f, true);
                fit.Box(parent, "Pallet load stretch film", 0, new Vector3(25.9f, 1.74f, z),
                    new Vector3(1.17f, 1.30f, .94f), "Wrap", solid: false);
                fit.Box(parent, "Crossed packing strap", 0, new Vector3(25.68f, 2.41f, z),
                    new Vector3(.042f, .012f, .91f), "Fabric", solid: false);
            }

            // Four empty pallets at this location become a loaded dispatch stack.
            fit.Prop("cargo_crate", parent, 0, 31.5f, 21, 0, .60f, true);
            fit.Prop("cargo_crate", parent, 0, 31.5f, 21, 0, 1.73f, true);
            fit.Prop("textile_bundle", parent, 0, 31.5f, 21, 0, 2.86f, true);
            fit.Box(parent, "Film around dispatch load", 0, new Vector3(31.5f, 1.72f, 21),
                new Vector3(1.17f, 2.24f, .94f), "Wrap", solid: false);

            fit.Prop("cargo_crate", parent, 0, 32.5f, 27.6f, 180, 1.13f, true);
            fit.Prop("plastic_sorting_crate", parent, 0, 32.25f, 27.6f, 180, 2.26f, true);
            fit.Prop("plastic_sorting_crate", parent, 0, 32.76f, 27.6f, 180, 2.26f, true);
            fit.Prop("textile_bundle", parent, 0, 32.5f, 27.6f, 0, 2.58f, true);
        }

        private static void SortingStacks(SalvageDensityPlacement fit, Transform parent)
        {
            // The original three crates support three more instead of spreading bins across aisles.
            foreach (Vector2 at in new[] { new Vector2(24.65f, 20.1f), new Vector2(31.4f, 22) })
            {
                for (int level = 0; level < 3; level++)
                    fit.Prop("plastic_sorting_crate", parent, 0, at.x, at.y, 180,
                        .96f + level * .32f, true);
                fit.Prop("circuit_board", parent, 0, at.x - .12f, at.y, 15, 1.74f, true);
                fit.Prop("motherboard", parent, 0, at.x + .08f, at.y - .04f, -8, 1.77f, true);
                fit.Prop("tag_detacher", parent, 0, at.x + .15f, at.y + .06f, 0, 1.82f, true);
            }

            // Recovery stock is staged on the closed kiosk counter, clear of its check-in frontage.
            fit.Prop("plastic_sorting_crate", parent, 0, 27.36f, 17.95f, 180, 1.10f, true);
            fit.Prop("plastic_sorting_crate", parent, 0, 28.09f, 17.95f, 180, 1.10f, true);
            fit.Prop("plastic_sorting_crate", parent, 0, 28.82f, 17.95f, 180, 1.10f, true);
            fit.Prop("motherboard", parent, 0, 27.32f, 17.95f, 12, 1.36f, true);
            fit.Prop("circuit_board", parent, 0, 28.11f, 17.95f, -22, 1.37f, true);
            fit.Prop("copper_spool", parent, 0, 28.85f, 17.95f, 0, 1.40f, true);
        }

        private static void FountainScrap(SalvageDensityPlacement fit, Transform parent)
        {
            // Both sides of the dry basin collect mixed stripped fittings; its centre pillar stays clear.
            foreach (float x in new[] { 26.82f, 29.18f })
            {
                fit.Prop("plastic_sorting_crate", parent, 0, x, 28.32f, 12, .14f, true);
                fit.Prop("plastic_sorting_crate", parent, 0, x, 28.32f, -4, .46f, true);
                fit.Prop("copper_spool", parent, 0, x, 28.32f, 0, .70f, true);
                fit.Prop("pc_tower", parent, 0, x, 29.2f, x < 28 ? 65 : -65, .14f, true);
                fit.Prop("circuit_board", parent, 0, x + .17f, 29.65f, 26, .19f, true);
                fit.Prop("motherboard", parent, 0, x - .12f, 29.85f, -13, .17f, true);
                fit.Prop("broken_glass", parent, 0, x, 29.5f, 17, .18f, true);
                fit.Prop("cable_bundle", parent, 0, x, 29.5f, 0, .22f, true);
            }
            fit.Prop("scaffolding_pipes", parent, 0, 28, 30.15f, 90, .16f, true);
            fit.Prop("fabric_roll", parent, 0, 28, 27.86f, 90, .14f, true);
            fit.Prop("textile_bundle", parent, 0, 28.87f, 30.02f, 15, .15f, true);
            fit.Prop("shrinkwrap_roll", parent, 0, 27.14f, 30.03f, 0, .14f, true);

            for (int i = 0; i < 8; i++)
            {
                float x = 26.94f + (i % 4) * .68f;
                float z = i < 4 ? 28.03f : 30.14f;
                fit.Box(parent, "Broken basin board offcut", 0, new Vector3(x, .18f + i % 3 * .025f, z),
                    new Vector3(.46f, .045f, .19f), i % 2 == 0 ? "Wood" : "Concrete",
                    new Vector3(0, i * 19 - 26, i % 2 * 7), false);
            }
        }

        private static void InterruptedPacking(SalvageDensityPlacement fit, Transform parent)
        {
            // Broad, flat traces tell the clearance story without additional blocking rubble.
            fit.Scatter(parent, 0, new Rect(24.3f, 19.2f, 2.1f, 4.2f), 26, 15521);
            fit.Scatter(parent, 0, new Rect(30.9f, 20.2f, 2.0f, 2.3f), 18, 15522);
            fit.Scatter(parent, 0, new Rect(24.0f, 26.9f, 1.7f, 1.9f), 17, 15523);
            fit.Scatter(parent, 0, new Rect(24.2f, 1.0f, .55f, 8.8f), 16, 15524);
            fit.Scatter(parent, 0, new Rect(31.25f, 1.0f, .55f, 7.2f), 16, 15525);
            fit.Prop("tire_track", parent, 0, 26.1f, 23.65f, 10);
            fit.Prop("tire_track", parent, 0, 30.55f, 22.65f, 15);
            fit.Prop("cable_bundle", parent, 0, 30.85f, 20.5f, 15);
            fit.Prop("broken_glass", parent, 0, 24.7f, 28.25f, 36);
        }
    }
}
