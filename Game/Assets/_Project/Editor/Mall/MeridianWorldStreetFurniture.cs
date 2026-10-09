using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MeridianWorldSurroundings;

namespace Abandoned.EditorTools
{
    /// <summary>Ordinary civic remnants connect the abandoned car park to the streets around it.</summary>
    public static class MeridianWorldStreetFurniture
    {
        public static void Build(Transform parent)
        {
            Transform streets = Scenery("RetailEstateStreetRemnants", parent);
            Material asphalt = Material("EstateConnectorAsphalt", "Asphalt", new Color(.76f, .78f, .74f));
            Material stone = Material("EstatePavement", "FacadeStone", new Color(.55f, .56f, .50f));
            Material paint = Material("EstateFadedPaint", "Ceiling", new Color(.61f, .59f, .41f));
            Material steel = Material("EstateStreetSteel", "FixtureTrim", new Color(.25f, .28f, .25f));
            Material dark = Material("EstateStreetRecess", "FixtureTrim", new Color(.05f, .07f, .065f));
            Material wood = Material("WeatheredStreetBench", "Wood", new Color(.35f, .30f, .24f));
            Material blue = Material("OldBusShelterEnamel", "StorefrontTrim", new Color(.18f, .27f, .31f));

            Panel("CarParkStreetConnection", streets, new Vector3(28f, -.39f, -52f), new Vector3(18f, .10f, 32f), asphalt);
            foreach (float side in new[] { -1f, 1f })
            {
                Panel("ConnectionFootpath", streets, new Vector3(28f + side * 10.7f, -.30f, -50.5f), new Vector3(3f, .22f, 29f), stone);
                Panel("ConnectionKerb", streets, new Vector3(28f + side * 9.13f, -.235f, -50.5f), new Vector3(.22f, .31f, 29f), stone);
            }
            for (int crossing = 0; crossing < 7; crossing++)
                Panel("StreetPedestrianCrossing", streets, new Vector3(28f, -.325f, -58f - crossing * 1.4f), new Vector3(7f, .012f, .7f), paint);
            BusShelter(streets, new Vector3(58f, -.46f, -55.8f), steel, wood, blue, dark);
            foreach (float x in new[] { -4f, 69f })
                EmptyBillboard(streets, new Vector3(x, -.46f, -44.5f), steel, dark);
            Cabinet(streets, new Vector3(92f, -.46f, 49f), steel, stone, dark);
        }

        private static void BusShelter(Transform parent, Vector3 at, Material steel, Material wood, Material blue, Material dark)
        {
            Transform shelter = Group("DisusedBusShelter", parent);
            shelter.localPosition = at;
            Panel("ShelterRoof", shelter, new Vector3(0f, 2.7f, 0f), new Vector3(6.6f, .15f, 2.8f), blue);
            Panel("ShelterRoofEdge", shelter, new Vector3(0f, 2.61f, -1.33f), new Vector3(6.6f, .22f, .10f), steel);
            foreach (float x in new[] { -3f, 0f, 3f })
                Panel("ShelterSteelPost", shelter, new Vector3(x, 1.32f, .99f), new Vector3(.11f, 2.64f, .11f), steel);
            foreach (float x in new[] { -3f, 3f })
                Panel("ShelterSidePost", shelter, new Vector3(x, 1.32f, -1f), new Vector3(.11f, 2.64f, .11f), steel);
            for (int slat = 0; slat < 6; slat++)
                Panel("ShelterRearSlat", shelter, new Vector3(0f, .45f + slat * .34f, 1.01f), new Vector3(6.1f, .075f, .08f), steel);
            Panel("ShelterBlankRouteCase", shelter, new Vector3(2f, 1.5f, .89f), new Vector3(1.25f, 1.4f, .10f), blue);
            Panel("EmptyRouteCaseInset", shelter, new Vector3(2f, 1.5f, .83f), new Vector3(1.12f, 1.24f, .018f), dark);
            for (int slat = 0; slat < 4; slat++)
                Panel("BenchSeatSlat", shelter, new Vector3(-.65f, .51f, .45f - slat * .15f), new Vector3(3.6f, .065f, .12f), wood);
            foreach (float x in new[] { -2f, .7f })
            {
                Panel("BenchLeg", shelter, new Vector3(x, .25f, .23f), new Vector3(.10f, .50f, .48f), steel);
                Panel("BenchBackUpright", shelter, new Vector3(x, .72f, .60f), new Vector3(.10f, .86f, .09f), steel);
            }
            foreach (float y in new[] { .80f, 1f })
                Panel("BenchBackSlat", shelter, new Vector3(-.65f, y, .60f), new Vector3(3.6f, .12f, .06f), wood);
            Transform bin = Group("StreetLitterBin", shelter);
            bin.localPosition = new Vector3(-3.8f, 0f, .05f);
            Panel("BinBody", bin, new Vector3(0f, .52f, 0f), new Vector3(.63f, 1.04f, .63f), blue);
            Panel("BinTop", bin, new Vector3(0f, 1.07f, 0f), new Vector3(.7f, .07f, .7f), steel);
            Panel("BinAperture", bin, new Vector3(0f, .85f, -.319f), new Vector3(.41f, .22f, .012f), dark);
        }

        private static void EmptyBillboard(Transform parent, Vector3 at, Material steel, Material dark)
        {
            Transform sign = Group("EmptyRetailAdvertisingFrame", parent);
            sign.localPosition = at;
            foreach (float side in new[] { -1f, 1f })
                Panel("AdvertFramePost", sign, new Vector3(side * .81f, 1.9f, 0f), new Vector3(.10f, 3.8f, .13f), steel);
            Panel("WeatheredEmptyAdvertPanel", sign, new Vector3(0f, 2.49f, 0f), new Vector3(1.62f, 2.52f, .08f), dark);
            foreach (float y in new[] { 1.2f, 3.8f })
                Panel("AdvertFrameRail", sign, new Vector3(0f, y, 0f), new Vector3(1.82f, .11f, .16f), steel);
        }

        private static void Cabinet(Transform parent, Vector3 at, Material steel, Material stone, Material dark)
        {
            Transform utility = Group("VacantEstateSwitchCabin", parent);
            utility.localPosition = at;
            Panel("CabinConcreteBase", utility, new Vector3(0f, .17f, 0f), new Vector3(4.7f, .34f, 3.6f), stone);
            Panel("CabinHousing", utility, new Vector3(0f, 1.8f, 0f), new Vector3(4.3f, 3.25f, 3.25f), steel);
            Panel("CabinOverhangingRoof", utility, new Vector3(0f, 3.46f, 0f), new Vector3(4.65f, .16f, 3.55f), steel);
            Panel("ClosedCabinDoor", utility, new Vector3(-.9f, 1.63f, -1.65f), new Vector3(1.45f, 2.45f, .055f), dark);
            for (int louvre = 0; louvre < 9; louvre++)
                Panel("CabinVentLouvre", utility, new Vector3(1.07f, 1.07f + louvre * .16f, -1.66f), new Vector3(1.15f, .055f, .07f), dark);
        }
    }
}
