using UnityEngine;
using static Abandoned.EditorTools.MeridianTenantFurniture;

namespace Abandoned.EditorTools
{
    public static class MeridianTenantFoodCourt
    {
        private static readonly float[] Columns = { 2, 6, 10, 14, 18, 22, 34, 38, 42 };

        public static void Build(MeridianTenantPlacement fit, Transform room, int floor)
        {
            foreach (float x in Columns)
            {
                foreach (float z in new[] { 38.2f, 41.4f }) fit.Model("FoodCourtTable", room, floor, x, z);
                if (floor == 0) Stall(fit, room, x);
                else fit.Model("FoodBooth", room, floor, x, 46.4f);
            }
            fit.Model("MallBin", room, floor, 23.2f, 43.2f);
            fit.Model("MallBin", room, floor, 32.8f, 43.2f);
            if (floor == 1)
            {
                fit.Model("FoodCourtCounter", room, floor, .85f, 38.2f, 90);
                fit.Model("FoodServiceStation", room, floor, .85f, 42.2f, 90);
            }
        }

        private static void Stall(MeridianTenantPlacement fit, Transform room, float x)
        {
            fit.Model("FoodCourtCounter", room, 0, x, 44.25f, 180);
            fit.Model("FoodServiceStation", room, 0, x, 47.05f, 180);
            // The upper fascia, extractor and menu frames sit over their own kitchen, never across the dining route.
            Transform overhead = fit.Assembly("KitchenHoodAndMenu", room, 0, x, 46.25f, new Vector3(3.35f, 1.3f, 1.5f), bottom: 2.2f);
            if (overhead == null) return;
            Part(overhead, "ExtractionHood", new(0, 2.85f, .28f), new(2.8f, .4f, .9f), "EscalatorSteel", false);
            Part(overhead, "ExtractionDuct", new(0, 3.28f, .28f), new(.5f, .46f, .55f), "EscalatorSteel", false);
            Part(overhead, "BlankMenuFascia", new(0, 2.55f, -.43f), new(3.35f, .65f, .13f), "StorefrontTrim", false);
            foreach (float side in new[] { -1.03f, 0f, 1.03f })
                Part(overhead, "FadedMenuPanel", new(side, 2.55f, -.505f), new(.89f, .51f, .014f), "Plaster", false);
        }
    }
}
