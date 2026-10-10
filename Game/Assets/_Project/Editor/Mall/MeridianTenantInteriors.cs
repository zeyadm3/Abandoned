using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Entire room plans: perimeter stock, ordered display aisles and a clear path to each storefront.</summary>
    public static class MeridianTenantInteriors
    {
        public static void Build(Transform mall)
        {
            Transform interiors = Group("TenantInteriors", mall);
            var fit = new MeridianTenantPlacement(mall);
            Office(fit, Group("Upper_ClosedAdministration", interiors), 1, 0, 16, 4);
            SalvageMallDressing.Build(mall, interiors);
            Toys(fit, Group("Upper_ToyShop", interiors));
            MeridianTenantCinema.Build(fit, Group("Upper_Cinema", interiors));
            MeridianTenantFoodCourt.Build(fit, Group("Ground_FoodCourt", interiors), 0);
            MeridianTenantFoodCourt.Build(fit, Group("Upper_DiningTerrace", interiors), 1);
            Stock(fit, Group("Basement_StockAisles", interiors));
        }

        private static void Electronics(MeridianTenantPlacement fit, Transform room)
        {
            for (float x = 2; x <= 23; x += 3) fit.Model("RetailShelf", room, 0, x, .48f);
            foreach (float z in new[] { 2f, 6f, 10f, 14f }) fit.Model("RetailShelf", room, 0, .45f, z, 90);
            foreach (float x in new[] { 6f, 12f, 18f })
            foreach (float z in new[] { 2.5f, 10f }) fit.Model("RetailGondola", room, 0, x, z, 90);
            foreach (float x in new[] { 2f, 8f, 14f }) fit.Model("SecurityDesk", room, 0, x, 14.6f, 180);
            fit.Model("CheckoutCounter", room, 0, 21, 10.2f, 180);
            fit.Model("ServiceCart", room, 0, 22, 2.2f);
            fit.Model("MallBin", room, 0, 22.9f, 8.9f);
        }

        private static void Jewelry(MeridianTenantPlacement fit, Transform room)
        {
            for (float x = 34; x <= 54; x += 4) fit.Model("EmptyDisplayCase", room, 0, x, .6f);
            foreach (float z in new[] { 2f, 6f, 10f, 14f }) fit.Model("EmptyDisplayCase", room, 0, 55.52f, z, 270);
            foreach (float x in new[] { 36f, 42f, 48f, 54f })
            foreach (float z in new[] { 2.8f, 10f }) fit.Model("EmptyDisplayCase", room, 0, x, z);
            for (float x = 42; x <= 54; x += 4) fit.Model("RetailShelf", room, 0, x, 15.5f, 180);
            fit.Model("CheckoutCounter", room, 0, 50, 13.7f, 180);
            fit.Model("AdvertisingLightbox", room, 0, 33.2f, 9.8f, 90);
        }

        private static void Clothing(MeridianTenantPlacement fit, Transform room)
        {
            foreach (float z in new[] { 18f, 26f }) fit.Model("ClothingRack", room, 0, .62f, z, 90);
            foreach (float x in new[] { 5.5f, 10f })
            foreach (float z in new[] { 18.3f, 26f }) fit.Model("ClothingRack", room, 0, x, z);
            foreach (float x in new[] { 2f, 5f, 8f }) MeridianTenantFurniture.FittingRoom(fit, room, 0, x, 30.8f);
            fit.Model("CheckoutCounter", room, 0, 14.1f, 29.9f, 90);
            foreach (float z in new[] { 18f, 26f })
            {
                fit.Model("Mannequin", room, 0, 14.5f, z, 90);
                fit.Model("Mannequin", room, 0, 14.5f, z + 1.2f, 65);
            }
            fit.Model("RetailShelf", room, 0, 2, 16.6f);
        }

        private static void Office(MeridianTenantPlacement fit, Transform room, int floor, float left, float front, int width)
        {
            for (int i = 0; i < width; i++)
            foreach (float z in new[] { front + 2, front + 10 })
            {
                float x = left + 2 + i * 4;
                GameObject desk = fit.Model("SecurityDesk", room, floor, x, z);
                if (desk != null)
                {
                    fit.Model("CinemaSeat", room, floor, x + .3f, z + 1.35f, 180);
                    MeridianTenantFurniture.DeskPartition(fit, room, floor, x, z - .68f);
                }
            }
            float rear = front + 15.5f;
            for (int i = 0; i < width; i++) fit.Model("RetailShelf", room, floor, left + 2 + i * 4, rear, 180);
            fit.Model("ServiceCart", room, floor, left + 1, front + 12.5f);
            fit.Model("MallBin", room, floor, left + 1, front + 8.8f);
        }

        private static void Furniture(MeridianTenantPlacement fit, Transform room)
        {
            foreach (float x in new[] { 3f, 9.5f })
            foreach (float z in new[] { 18f, 26f })
            {
                MeridianTenantFurniture.Sofa(fit, room, 1, x, z);
                MeridianTenantFurniture.CoffeeTable(fit, room, 1, x, z + 1.6f);
                MeridianTenantFurniture.Armchair(fit, room, 1, x + 2.1f, z + 1.2f, 270);
            }
            MeridianTenantFurniture.Sofa(fit, room, 1, 3, 30.4f, 180);
            MeridianTenantFurniture.DiningSet(fit, room, 1, 13.7f, 18.5f);
            MeridianTenantFurniture.Bed(fit, room, 1, 13.7f, 28.5f);
            foreach (float z in new[] { 18f, 26f, 30f }) fit.Model("RetailShelf", room, 1, .45f, z, 90);
        }

        private static void Toys(MeridianTenantPlacement fit, Transform room)
        {
            foreach (float x in new[] { 42f, 46f, 50f, 54f })
            {
                GameObject shelf = fit.Model("RetailShelf", room, 1, x, 31.48f, 180);
                if (shelf != null) MeridianTenantFurniture.EmptyPackaging(shelf.transform);
            }
            foreach (float z in new[] { 18f, 26f }) fit.Model("RetailShelf", room, 1, 55.55f, z, 270);
            foreach (float x in new[] { 44f, 50f })
            foreach (float z in new[] { 18.5f, 26f }) fit.Model("RetailGondola", room, 1, x, z, 90);
            fit.Model("CheckoutCounter", room, 1, 44, 30);
            MeridianTenantFurniture.Arcade(fit, room, 1, 41, 27);
            MeridianTenantFurniture.Arcade(fit, room, 1, 41, 29);
            MeridianTenantFurniture.ActivityTable(fit, room, 1, 53.1f, 29.7f);
        }

        private static void Stock(MeridianTenantPlacement fit, Transform room)
        {
            fit.Model("StockCage", room, -1, 46, 24.75f);
            fit.Model("StockCage", room, -1, 46, 31.25f, 180);
            fit.Model("StockCage", room, -1, 55.1f, 26, 270);
            fit.Model("StockCage", room, -1, 55.1f, 30, 270);
            fit.Model("ServiceCart", room, -1, 45.2f, 29);
        }
    }
}
