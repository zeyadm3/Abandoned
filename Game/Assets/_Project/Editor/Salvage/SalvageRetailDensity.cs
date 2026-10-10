using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Remaining shop inventory and interrupted room sets, outside the clearance lanes.</summary>
    public static class SalvageRetailDensity
    {
        public static void Build(SalvageDensityPlacement fit)
        {
            Tech(fit, fit.Zone("A_ApexRemainingStock"));
            Furniture(fit, fit.Zone("B_NordicGroundRoomSets"), 0);
            Furniture(fit, fit.Zone("B_NordicUpperOfficeSamples"), 1);
        }

        private static void Tech(SalvageDensityPlacement fit, Transform room)
        {
            // The shelf tops come from the authored modular shelving recipe, not floating stock.
            foreach (float x in new[] { 2f, 6f, 10f, 14f, 18f })
            {
                Stock(fit, room, "pc_tower", 0, x - .28f, .8f, .141f, 180);
                Stock(fit, room, "pc_tower", 0, x + .28f, .8f, .141f, 180);
                Stock(fit, room, "crt_monitor", 0, x - .255f, .8f, .681f, 180);
                Stock(fit, room, "crt_monitor", 0, x + .255f, .8f, .681f, 180);
                Stock(fit, room, "led_monitor", 0, x - .22f, .8f, 1.221f, 180);
                Stock(fit, room, "circuit_board", 0, x + .32f, .79f, 1.221f, 12);
                Stock(fit, room, "motherboard", 0, x - .25f, .8f, 1.761f, -8);
                Stock(fit, room, "motherboard", 0, x + .25f, .8f, 1.761f, 6);
            }
            Stock(fit, room, "copper_spool", 0, 6.78f, 2.4f, .35f);
            Stock(fit, room, "motherboard", 0, 12.84f, 2.36f, .35f, -18);
            Stock(fit, room, "crt_monitor", 0, 17.36f, 2.4f, .35f, 180);
            Stock(fit, room, "pc_tower", 0, 18.02f, 2.4f, .35f, 180);
            Stock(fit, room, "motherboard", 0, 18.65f, 2.42f, .35f, 15);
            Stock(fit, room, "industrial_battery", 0, 3.75f, 10.2f, .952f, 180);

            foreach (Vector2 at in new[] { new Vector2(22.8f, 1.2f), new Vector2(22.8f, 2.4f),
                new Vector2(22.8f, 10.9f), new Vector2(10.8f, 13.2f), new Vector2(14.5f, 14.3f) })
            {
                GameObject crate = fit.Prop("cargo_crate", room, 0, at.x, at.y, 180);
                if (crate == null) continue;
                Stock(fit, room, "pc_tower", 0, at.x - .27f, at.y, 1.108f, 180);
                Stock(fit, room, "pc_tower", 0, at.x + .27f, at.y, 1.108f, 180);
                fit.Box(room, "cut tech packing film", 0, new Vector3(at.x, .55f, at.y - .46f),
                    new Vector3(1.0f, .72f, .018f), "Wrap", solid: false);
            }
            foreach (Vector2 at in new[] { new Vector2(.95f, 1.9f), new Vector2(.95f, 3.1f),
                new Vector2(21.8f, 9.1f), new Vector2(10.9f, 14.4f) })
                fit.Prop("server_rack", room, 0, at.x, at.y, at.x < 2 ? 90 : 180);

            fit.Box(room, "interrupted stockroom divider", 0, new Vector3(12.5f, .95f, 14.9f),
                new Vector3(2.15f, 1.9f, .12f), "Paint", new Vector3(0, -7, 0));
            foreach (Vector2 at in new[] { new Vector2(3.1f, 12.9f), new Vector2(4.0f, 13.2f),
                new Vector2(8.6f, 14.2f), new Vector2(20.5f, 2.2f) })
                RecoveryBin(fit, room, 0, at, "copper_spool");
            fit.Scatter(room, 0, new Rect(.6f, .4f, 22.6f, 3.1f), 38, 15501);
            fit.Scatter(room, 0, new Rect(.7f, 8.5f, 14.5f, 6.7f), 50, 15502);
            fit.Scatter(room, 0, new Rect(16.5f, 8.5f, 6.7f, 2.9f), 24, 15503);
        }

        private static void Furniture(SalvageDensityPlacement fit, Transform room, int floor)
        {
            foreach (float x in new[] { 34.7f, 40.8f, 46.8f, 52.8f })
            {
                Stock(fit, room, "plastic_sorting_crate", floor, x, .75f, .141f, 180);
                Stock(fit, room, "fabric_roll", floor, x, .75f, .681f, 90);
                Stock(fit, room, "shrinkwrap_roll", floor, x - .33f, .75f, 1.761f);
                Stock(fit, room, "shrinkwrap_roll", floor, x + .33f, .75f, 1.761f);
                if (x > 40 && x < 41)
                    Stock(fit, room, "fabric_roll", floor, x, .75f, 1.221f, 90);
            }

            foreach (float x in new[] { 37.8f, 43.8f, 49.8f })
            {
                GameObject shelving = fit.Prop("modular_shelving", room, floor, x, .75f);
                if (shelving == null) continue;
                Stock(fit, room, "fabric_roll", floor, x, .75f, .141f, 90);
                Stock(fit, room, "plastic_sorting_crate", floor, x, .75f, .681f, 180);
                Stock(fit, room, "shrinkwrap_roll", floor, x - .30f, .75f, 1.761f);
                Stock(fit, room, "shrinkwrap_roll", floor, x + .30f, .75f, 1.761f);
            }
            foreach (float x in floor == 0 ? new[] { 36f, 42f } : new[] { 36f, 42f, 48f })
            {
                Stock(fit, room, "shrinkwrap_roll", floor, x - .45f, 2.7f, .75f);
                fit.Box(room, "desk sample catalogue", floor, new Vector3(x + .13f, .755f, 2.28f),
                    new Vector3(.30f, .01f, .22f), "Paper", new Vector3(0, 15, 0), false);
                fit.Box(room, "unpacked desk trim", floor, new Vector3(x + .3f, .785f, 2.68f),
                    new Vector3(.42f, .06f, .14f), "Wood", new Vector3(0, -11, 0), false);
            }
            foreach (float x in new[] { 38.9f, 44.8f, 51.5f })
                fit.Box(room, "remaining showroom partition", floor, new Vector3(x, .95f, 2.2f),
                    new Vector3(.11f, 1.9f, 2.0f), "Paint", new Vector3(0, -6, 0));

            foreach (Vector2 at in new[] { new Vector2(33.4f, 1.7f), new Vector2(33.4f, 2.8f),
                new Vector2(55.15f, 2.8f), new Vector2(55.15f, 11.1f), new Vector2(44.8f, 14.5f) })
                fit.Prop("steel_filing_cabinet", room, floor, at.x, at.y, at.x < 34 ? 90 : 270);
            foreach (Vector2 at in new[] { new Vector2(33.6f, 9.6f), new Vector2(48.9f, 9.3f),
                new Vector2(42.7f, 13.4f), new Vector2(46.7f, 13.4f) })
                FlatpackPile(fit, room, floor, at);
            foreach (Vector2 at in new[] { new Vector2(55.15f, 13.1f), new Vector2(50.0f, 13.4f) })
            {
                GameObject pallet = fit.Prop("shipping_pallet", room, floor, at.x, at.y);
                if (pallet == null) continue;
                Stock(fit, room, "textile_bundle", floor, at.x, at.y, .146f, 90);
                Stock(fit, room, "textile_bundle", floor, at.x + .06f, at.y - .025f, .521f, 90);
                fit.Box(room, "half wrapped upholstery stack", floor, new Vector3(at.x, .53f, at.y),
                    new Vector3(.75f, .72f, .55f), "Wrap", solid: false);
            }

            if (floor == 0)
            {
                fit.Prop("executive_chair", room, floor, 48.2f, 3.4f, 155);
                fit.Prop("executive_chair", room, floor, 50.6f, 3.4f, 190);
                Stock(fit, room, "fabric_roll", floor, 49.55f, 2.45f, .788f, 90);
            }
            else
            {
                foreach (Vector2 at in new[] { new Vector2(34.3f, 11.4f), new Vector2(37.8f, 13.2f),
                    new Vector2(48.8f, 11.9f), new Vector2(54.3f, 3.0f) })
                    fit.Prop("ergonomic_chair", room, floor, at.x, at.y, 130);
            }
            fit.Scatter(room, floor, new Rect(32.6f, .4f, 22.6f, 3.1f), 42, 15510 + floor);
            fit.Scatter(room, floor, new Rect(32.6f, 8.6f, 22.5f, 2.8f), 40, 15520 + floor);
            fit.Scatter(room, floor, new Rect(40.5f, 12.2f, 14.6f, 3.0f), 38, 15530 + floor);
        }

        private static void Stock(SalvageDensityPlacement fit, Transform room, string id, int floor,
            float x, float z, float height, float yaw = 0)
            => fit.Prop(id, room, floor, x, z, yaw, height, surfaceStock: true);

        private static void RecoveryBin(SalvageDensityPlacement fit, Transform room, int floor,
            Vector2 at, string id)
        {
            if (fit.Prop("plastic_sorting_crate", room, floor, at.x, at.y, 180) == null) return;
            Stock(fit, room, id, floor, at.x, at.y, .041f, 0);
        }

        private static void FlatpackPile(SalvageDensityPlacement fit, Transform room, int floor, Vector2 at)
        {
            GameObject pile = fit.Box(room, "stacked flat pack cartons", floor,
                new Vector3(at.x, .27f, at.y), new Vector3(1.45f, .54f, .9f), "Paper", new Vector3(0, -5, 0));
            if (pile == null) return;
            fit.Box(room, "opened desktop panel", floor, new Vector3(at.x, .572f, at.y),
                new Vector3(1.36f, .064f, .78f), "Wood", new Vector3(0, 7, 0), false);
            fit.Box(room, "discarded retaining strap", floor, new Vector3(at.x + .35f, .61f, at.y),
                new Vector3(.038f, .012f, .8f), "Fabric", new Vector3(0, 7, 0), false);
            Stock(fit, room, "shrinkwrap_roll", floor, at.x - .38f, at.y, .604f);
        }
    }
}
