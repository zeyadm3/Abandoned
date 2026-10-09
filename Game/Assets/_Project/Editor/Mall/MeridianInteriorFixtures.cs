using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Fixed, stripped retail fittings: their silhouettes explain each former tenant.</summary>
    public static class MeridianInteriorFixtures
    {
        public static void Build(string tag, Transform bay, int variant)
        {
            switch (tag)
            {
                case "electronics": Electronics(bay, variant); break;
                case "clothing": Clothing(bay, variant); break;
                case "furniture": Furniture(bay, variant); break;
                case "toys": Toys(bay); break;
                case "jewelry": Jewelry(bay); break;
                case "food": Kitchen(bay, variant); break;
                case "cinema": Cinema(bay, variant); break;
                case "office": Office(bay); break;
                default: Stock(bay); break;
            }
        }

        private static GameObject Part(Transform parent, string name, Vector3 at, Vector3 size,
            string material = "EscalatorSteel", bool solid = true)
        {
            GameObject part = Box(name, parent, at, size, Material(material), solid);
            if (solid) part.AddComponent<Abandoned.Core.SurfaceTag>().EditorSet(
                material == "Wood" || material == "BoardWood" ? Abandoned.Core.SurfaceMaterial.Wood
                : material == "Upholstery" || material == "Carpet" ? Abandoned.Core.SurfaceMaterial.Carpet
                : Abandoned.Core.SurfaceMaterial.Metal);
            return part;
        }

        internal static Material Material(string key)
        {
            string path = $"{MeridianArt.Folder}/Materials/MALL_{key}.mat";
            return AssetDatabase.LoadAssetAtPath<Material>(path) ?? MeridianArt.Material(key);
        }

        private static void Back(Transform bay, string key = "StorefrontTrim")
        {
            Part(bay, "FittedDisplayBack", new Vector3(0, 1.3f, 1.48f), new Vector3(3.25f, 2.6f, .12f), key);
            Part(bay, "Pelmet", new Vector3(0, 2.6f, 1.35f), new Vector3(3.3f, .12f, .4f));
        }

        private static void Electronics(Transform bay, int variant)
        {
            Back(bay);
            for (int i = 0; i < 6; i++)
                Part(bay, "VacantSlatRail", new Vector3(0, .65f + i * .29f, 1.37f), new Vector3(3.1f, .025f, .045f), solid: false);
            foreach (float x in new[] { -1.02f, 1.02f })
            {
                Part(bay, "DemoCabinet", new Vector3(x, .43f, .83f), new Vector3(.92f, .86f, .8f), "FixtureTrim");
                Part(bay, "DeadDisplayShell", new Vector3(x, 1.22f, 1.15f), new Vector3(.75f, .55f, .1f), "Carpet");
                Part(bay, "DisplayStand", new Vector3(x, .99f, 1.15f), new Vector3(.12f, .23f, .12f), solid: false);
                Part(bay, "LooseCableTray", new Vector3(x, .9f, .55f), new Vector3(.66f, .025f, .22f), "Rust", false);
            }
            if (variant % 2 == 0)
                Part(bay, "RemovedScreenMount", new Vector3(0, 1.65f, 1.36f), new Vector3(.4f, .35f, .05f), solid: false);
        }

        private static void Clothing(Transform bay, int variant)
        {
            if (variant % 2 == 0)
            {
                Back(bay, "Wood");
                foreach (float x in new[] { -1.48f, 0f, 1.48f })
                    Part(bay, "FittingCubiclePartition", new Vector3(x, 1.1f, .88f), new Vector3(.075f, 2.2f, 1.25f), "Wood");
                foreach (float x in new[] { -.75f, .75f })
                {
                    Part(bay, "FittingMirror", new Vector3(x, 1.24f, 1.39f), new Vector3(.46f, 1.58f, .045f), "EscalatorSteel", false);
                    Part(bay, "HalfDrawnCurtain", new Vector3(x - .35f, 1.16f, .29f), new Vector3(.32f, 1.92f, .08f), "Upholstery");
                }
            }
            else
            {
                Back(bay, "Wood");
                for (int i = 0; i < 3; i++)
                    Part(bay, "EmptyFoldedStockShelf", new Vector3(0, .5f + i * .67f, 1.14f), new Vector3(3.1f, .06f, .6f), "Wood", false);
                foreach (float x in new[] { -1.3f, 1.3f })
                    Part(bay, "RackLeg", new Vector3(x, .9f, -.65f), new Vector3(.055f, 1.8f, .055f), solid: false);
                Part(bay, "BareGarmentRail", new Vector3(0, 1.8f, -.65f), new Vector3(2.65f, .055f, .055f), solid: false);
                for (int i = 0; i < 7; i++)
                    Part(bay, "RemainingHanger", new Vector3(-1f + i * .28f, 1.62f, -.65f), new Vector3(.18f, .045f, .1f), "Wood", false);
            }
        }

        private static void Furniture(Transform bay, int variant)
        {
            Back(bay, "Wood");
            Part(bay, "ShowroomSofaBase", new Vector3(0, .32f, .5f), new Vector3(2.65f, .5f, 1.05f), "Upholstery");
            Part(bay, "SofaBack", new Vector3(0, .82f, .95f), new Vector3(2.65f, .73f, .22f), "Upholstery");
            foreach (float x in new[] { -1.22f, 1.22f })
                Part(bay, "SofaArm", new Vector3(x, .61f, .5f), new Vector3(.23f, .55f, 1.08f), "Upholstery");
            Part(bay, "CoffeeTableTop", new Vector3(0, .52f, -.87f), new Vector3(1.35f, .09f, .65f), "Wood");
            foreach (float x in new[] { -.55f, .55f })
                Part(bay, "CoffeeTableLeg", new Vector3(x, .26f, -.87f), new Vector3(.1f, .48f, .53f), "Wood");
            Part(bay, "VacantWallFrame", new Vector3(0, 1.88f, 1.36f), new Vector3(1.25f, .75f, .08f), "BoardWood", false);
        }

        private static void Toys(Transform bay)
        {
            Back(bay, "Wood");
            for (int i = 0; i < 4; i++)
                Part(bay, "EmptyToyShelf", new Vector3(0, .25f + i * .5f, 1.1f), new Vector3(3.1f, .075f, .65f), "Wood", false);
            foreach (float x in new[] { -1.05f, 0f, 1.05f })
                Part(bay, "CubbyDivider", new Vector3(x, .98f, 1.1f), new Vector3(.075f, 1.9f, .65f), "Wood");
            foreach (float x in new[] { -.85f, .85f })
            {
                Part(bay, "VacantArcadeCabinet", new Vector3(x, .75f, -.7f), new Vector3(.7f, 1.5f, .68f), "FixtureTrim");
                Part(bay, "ArcadeScreen", new Vector3(x, 1.18f, -1.05f), new Vector3(.53f, .4f, .015f), "Carpet", false);
            }
        }

        private static void Jewelry(Transform bay)
        {
            Back(bay, "Wood");
            Part(bay, "DisplayPlinth", new Vector3(0, .43f, .5f), new Vector3(2.65f, .86f, .82f), "Wood");
            Part(bay, "EmptyVelvetTray", new Vector3(0, .88f, .5f), new Vector3(2.48f, .06f, .72f), "Carpet", false);
            foreach (float x in new[] { -1.3f, 1.3f })
                Part(bay, "DisplayGlassSide", new Vector3(x, 1.09f, .5f), new Vector3(.035f, .4f, .82f), "Glass");
            Part(bay, "DisplayGlassFront", new Vector3(0, 1.09f, .08f), new Vector3(2.65f, .4f, .035f), "Glass");
            Part(bay, "DisplayGlassLid", new Vector3(0, 1.31f, .5f), new Vector3(2.65f, .035f, .82f), "Glass", false);
            for (int i = 0; i < 4; i++)
                Part(bay, "StrippedWallDisplay", new Vector3(-1.05f + i * .7f, 1.87f, 1.35f), new Vector3(.5f, .55f, .08f), "Carpet", false);
        }

        private static void Kitchen(Transform bay, int variant)
        {
            Back(bay, "Tile");
            Part(bay, "ServiceKitchenCabinet", new Vector3(0, .47f, .95f), new Vector3(3.1f, .94f, .85f));
            Part(bay, "ExtractorHood", new Vector3(-.6f, 2.35f, 1.07f), new Vector3(1.7f, .38f, .72f));
            for (int i = 0; i < 4; i++)
                Part(bay, "ColdHob", new Vector3(-1.14f + i * .42f, .96f, .94f), new Vector3(.28f, .022f, .3f), "Carpet", false);
            Part(bay, "DrainedSink", new Vector3(.97f, .96f, .94f), new Vector3(.6f, .025f, .55f), "Carpet", false);
            Part(bay, "SinkTap", new Vector3(.97f, 1.08f, 1.18f), new Vector3(.055f, .23f, .055f), solid: false);
            Part(bay, "ServiceCounter", new Vector3(0, .49f, -.82f), new Vector3(2.65f, .98f, .64f), variant % 2 == 0 ? "Wood" : "Tile");
        }

        private static void Cinema(Transform bay, int variant)
        {
            if (variant == 0)
            {
                Back(bay, "Carpet");
                Part(bay, "ProjectionScreenFrame", new Vector3(0, 1.57f, 1.34f), new Vector3(3.2f, 1.8f, .16f), "FixtureTrim");
                Part(bay, "AgedProjectionScreen", new Vector3(0, 1.57f, 1.24f), new Vector3(3f, 1.62f, .035f), "Plaster", false);
                return;
            }
            foreach (float x in new[] { -.95f, 0f, .95f })
            foreach (float z in new[] { -.7f, .8f })
            {
                Part(bay, "SeatPedestal", new Vector3(x, .22f, z), new Vector3(.18f, .44f, .25f));
                Part(bay, "CinemaSeatCushion", new Vector3(x, .49f, z), new Vector3(.63f, .17f, .62f), "Upholstery");
                Part(bay, "CinemaSeatBack", new Vector3(x, .85f, z + .24f), new Vector3(.63f, .75f, .13f), "Upholstery");
                foreach (float side in new[] { -.35f, .35f })
                    Part(bay, "SeatArm", new Vector3(x + side, .66f, z), new Vector3(.08f, .07f, .55f), "FixtureTrim", false);
            }
        }

        private static void Office(Transform bay)
        {
            Back(bay, "Block");
            Part(bay, "OfficeDesk", new Vector3(-.45f, .74f, .8f), new Vector3(2f, .08f, .8f), "Wood");
            Part(bay, "DeskDrawers", new Vector3(-1.1f, .35f, .8f), new Vector3(.55f, .7f, .75f));
            Part(bay, "DeadTerminal", new Vector3(-.35f, 1f, 1.01f), new Vector3(.55f, .4f, .16f), "Carpet");
            Part(bay, "OpenFilingCabinet", new Vector3(1.02f, .8f, .95f), new Vector3(.7f, 1.6f, .85f));
            for (int i = 0; i < 4; i++)
                Part(bay, "DrawerGap", new Vector3(1.02f, .22f + i * .36f, .51f), new Vector3(.6f, .075f, .035f), "Carpet", false);
        }

        private static void Stock(Transform bay)
        {
            Back(bay, "Block");
            foreach (float x in new[] { -1.45f, 1.45f })
                Part(bay, "StockRackUpright", new Vector3(x, 1.2f, .95f), new Vector3(.085f, 2.4f, .85f));
            for (int i = 0; i < 4; i++)
                Part(bay, "StockRackShelf", new Vector3(0, .2f + i * .64f, .95f), new Vector3(3f, .065f, .9f), "Rust", false);
            for (int i = 0; i < 3; i++)
                Part(bay, "EmptyTransitCrate", new Vector3(-.95f + i * .9f, .43f, .95f), new Vector3(.72f, .43f, .67f), "BoardWood");
        }
    }
}
