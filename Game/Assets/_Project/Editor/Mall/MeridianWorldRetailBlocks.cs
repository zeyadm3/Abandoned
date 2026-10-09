using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MeridianWorldSurroundings;

namespace Abandoned.EditorTools
{
    /// <summary>Shuttered street shops and former offices establish the mall's larger retail neighbourhood.</summary>
    public static class MeridianWorldRetailBlocks
    {
        private static Material stone, plaster, steel, glazing, timber, moss;
        private static readonly Color[] colors =
        {
            new(.25f, .35f, .34f), new(.38f, .26f, .22f), new(.40f, .37f, .24f),
            new(.27f, .29f, .39f), new(.38f, .30f, .32f), new(.31f, .34f, .27f)
        };

        public static void Build(Transform parent)
        {
            stone = Material("NeighbourhoodStone", "FacadeStone", new Color(.46f, .47f, .43f));
            plaster = Material("NeighbourhoodPlaster", "Plaster", new Color(.48f, .47f, .41f));
            steel = Material("NeighbourhoodSteel", "FixtureTrim", new Color(.21f, .24f, .23f));
            glazing = Material("VacantShopWindows", "FixtureTrim", new Color(.035f, .060f, .065f));
            timber = Material("BoardedShopTimber", "BoardWood", new Color(.37f, .30f, .22f));
            moss = Material("RetailDampCourse", "Moss", new Color(.20f, .25f, .20f));
            Transform retail = Scenery("VacantRetailNeighbourhood", parent);
            Terrace(retail, new Vector3(28f, -.46f, -90.5f), 0f, 6, 11f, 12f, 8.7f, 0);
            Terrace(retail, new Vector3(-57f, -.46f, 16f), 90f, 4, 10f, 11f, 13.1f, 6);
            Terrace(retail, new Vector3(28f, -.46f, 84.6f), 180f, 5, 12f, 13f, 7.7f, 10);
        }

        private static void Terrace(Transform parent, Vector3 at, float yaw, int shops, float bay, float depth, float height, int seed)
        {
            Transform terrace = Group("ClosedRetailTerrace_" + seed, parent);
            terrace.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            float width = bay * shops;
            Panel("VacantRetailBlock", terrace, new Vector3(0f, height * .5f, 0f), new Vector3(width, height, depth), plaster);
            Panel("RaisedShopPavement", terrace, new Vector3(0f, .16f, depth * .5f + 1f), new Vector3(width + 1f, .32f, 2.3f), stone);
            Panel("DampGroundCourse", terrace, new Vector3(0f, .18f, depth * .5f + .013f), new Vector3(width, .36f, .04f), moss);
            Panel("UpperShopStringCourse", terrace, new Vector3(0f, 3.87f, depth * .5f + .15f), new Vector3(width + .25f, .21f, .38f), stone);
            Panel("FlatRoof", terrace, new Vector3(0f, height + .10f, 0f), new Vector3(width + .3f, .20f, depth + .3f), steel);
            Panel("FrontParapet", terrace, new Vector3(0f, height + .44f, depth * .5f), new Vector3(width + .3f, .7f, .3f), stone);
            Panel("RearParapet", terrace, new Vector3(0f, height + .35f, -depth * .5f), new Vector3(width + .3f, .5f, .25f), stone);
            for (int shop = 0; shop < shops; shop++)
            {
                int variant = seed + shop;
                float x = (shop - (shops - 1) * .5f) * bay;
                Material fascia = Material("VacantShopFascia_" + variant % colors.Length, "StorefrontTrim", colors[variant % colors.Length]);
                Shop(terrace, x, depth * .5f, bay, fascia, variant);
                int floors = Mathf.FloorToInt((height - 3.9f) / 3.1f);
                for (int storey = 0; storey < floors; storey++)
                {
                    float y = 5.35f + storey * 3.1f;
                    foreach (float side in new[] { -1f, 1f })
                        Window(terrace, new Vector3(x + side * bay * .24f, y, depth * .5f + .06f), 2.1f, 1.7f, variant % 4 == 1);
                }
                Panel("RetailRoofVent", terrace, new Vector3(x, height + .61f, -.8f), new Vector3(2f, 1f, 1.4f), steel);
                if (variant % 2 == 0)
                    Panel("ColdRoofFlue", terrace, new Vector3(x + bay * .27f, height + 1.65f, -depth * .24f), new Vector3(.52f, 3.3f, .52f), stone);
                Panel("DrainDownpipe", terrace, new Vector3(x - bay * .48f, height * .48f, depth * .5f + .18f), new Vector3(.11f, height * .94f, .11f), steel);
            }
            foreach (float side in new[] { -1f, 1f })
                Panel("CornerPilaster", terrace, new Vector3(side * width * .5f, height * .5f, depth * .5f + .1f), new Vector3(.35f, height, .25f), stone);
        }

        private static void Shop(Transform parent, float x, float front, float bay, Material fascia, int variant)
        {
            Transform shop = Group("EmptyStreetShop_" + variant, parent);
            shop.localPosition = new Vector3(x, 0f, front);
            Panel("DarkRetailRecess", shop, new Vector3(0f, 1.76f, .025f), new Vector3(bay - 1f, 3.4f, .05f), glazing);
            Panel("BlankTenantFascia", shop, new Vector3(0f, 3.34f, .16f), new Vector3(bay - .36f, .7f, .25f), fascia);
            foreach (float side in new[] { -1f, 1f })
                Panel("StoneShopPier", shop, new Vector3(side * (bay * .5f - .24f), 1.75f, .13f), new Vector3(.48f, 3.5f, .34f), stone);
            if (variant % 3 != 1)
            {
                Panel("LoweredRetailShutter", shop, new Vector3(0f, 1.46f, .09f), new Vector3(bay - 1.1f, 2.85f, .08f), steel);
                for (int slat = 0; slat < 10; slat++)
                    Panel("RollerShutterSeam", shop, new Vector3(0f, .17f + slat * .27f, .14f), new Vector3(bay - 1.2f, .026f, .025f), glazing);
            }
            else
            {
                foreach (float divider in new[] { -.28f, .12f, .38f })
                    Panel("OldShopfrontMullion", shop, new Vector3(bay * divider, 1.52f, .13f), new Vector3(.08f, 2.9f, .13f), steel);
                foreach (float y in new[] { .47f, 1.37f, 2.2f })
                {
                    GameObject board = Panel("BoardedShopfront", shop, new Vector3(-bay * .11f, y, .24f), new Vector3(bay * .68f, .24f, .08f), timber);
                    board.transform.localRotation = Quaternion.Euler(0f, 0f, y < 1f ? 6f : -4f);
                }
                Panel("ClosedRetailDoor", shop, new Vector3(bay * .39f, 1.42f, .15f), new Vector3(bay * .18f, 2.8f, .10f), steel);
            }
            if (variant % 2 == 0)
            {
                GameObject awning = Panel("FadedShopAwning", shop, new Vector3(0f, 2.89f, .79f), new Vector3(bay - 1f, .08f, 1.4f), fascia);
                awning.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
                Panel("AwningValance", shop, new Vector3(0f, 2.72f, 1.48f), new Vector3(bay - 1f, .22f, .06f), fascia);
            }
        }

        private static void Window(Transform parent, Vector3 at, float width, float height, bool boarded)
        {
            Panel("UnlitOfficeWindow", parent, at, new Vector3(width, height, .035f), glazing);
            foreach (float x in new[] { -width * .5f, 0f, width * .5f })
                Panel("UpperWindowFrame", parent, at + new Vector3(x, 0f, .06f), new Vector3(.075f, height + .12f, .10f), steel);
            Panel("UpperWindowSill", parent, at + new Vector3(0f, -height * .5f - .07f, .09f), new Vector3(width + .25f, .12f, .28f), stone);
            if (!boarded) return;
            foreach (float y in new[] { -.40f, .32f })
                Panel("UpperWindowBoard", parent, at + new Vector3(0f, y, .12f), new Vector3(width * 1.02f, .24f, .05f), timber);
        }
    }
}
