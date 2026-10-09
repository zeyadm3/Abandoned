using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class MeridianRetailFacade
    {
        public static void Build(Transform root, Transform mall)
        {
            Transform facade = Group("RetailEnvelope", root);
            Entrance(facade);
            for (int x = 0; x < MallLayout.TilesX; x++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                if (!MallLayout.InFootprint(cell)) continue;
                foreach (Vector2Int direction in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                {
                    if (MallLayout.InFootprint(cell + direction)) continue;
                    Vector3 at = MallLayout.TileTopCenter(cell, 0) + new Vector3(direction.x, 0, direction.y) * 2;
                    Transform bay = Group($"FacadeBay_{x}_{z}_{direction}", facade);
                    bay.localPosition = at;
                    bay.localRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
                    Box("RetailParapetCoping", bay, new Vector3(0, 12.35f, .23f),
                        new Vector3(4.02f, .24f, .5f), MeridianRetailMaterials.Stone);
                    Box("UpperFacadeBand", bay, new Vector3(0, 11.72f, .145f),
                        new Vector3(3.95f, .78f, .06f), MeridianRetailMaterials.Accent("electronics"));
                    Box("LowerCornice", bay, new Vector3(0, 7.62f, .19f),
                        new Vector3(3.98f, .2f, .19f), MeridianRetailMaterials.Stone);
                    Box("CorniceShadowLine", bay, new Vector3(0, 7.46f, .17f),
                        new Vector3(3.98f, .055f, .13f), MeridianRetailMaterials.Dark);
                    if ((x + z) % 2 == 0 && !UpperExit(cell, direction)) Fins(bay);
                }
            }
        }

        private static bool UpperExit(Vector2Int cell, Vector2Int direction)
        {
            char side = direction == Vector2Int.left ? 'W' : direction == Vector2Int.right ? 'E' :
                direction == Vector2Int.up ? 'N' : 'S';
            int column = direction.x == 0 ? cell.x : cell.y;
            foreach (var exit in MallLayout.Exterior)
                if (exit.floor == 2 && exit.side == side && exit.column == column) return true;
            return false;
        }

        private static void Entrance(Transform parent)
        {
            Transform entrance = Group("EntryMarquee", parent);
            entrance.localPosition = new Vector3(28, 0, 4);
            // A cantilevered entrance roof leaves the original arrival and haul lanes fully open.
            Box("CantileverCanopy", entrance, new Vector3(0, 3.57f, -1.2f),
                new Vector3(11.8f, .23f, 3.2f), MeridianRetailMaterials.Stone);
            Box("CanopyFascia", entrance, new Vector3(0, 3.64f, -2.82f),
                new Vector3(12.15f, .4f, .11f), MeridianRetailMaterials.Accent("electronics"));
            Box("MarqueeBottomLip", entrance, new Vector3(0, 3.45f, -2.83f),
                new Vector3(12.2f, .055f, .25f), MeridianRetailMaterials.Brass);
            for (int n = 0; n < 7; n++)
                Box("CanopySoffitRib", entrance, new Vector3((n - 3) * 1.7f, 3.425f, -1.1f),
                    new Vector3(.065f, .07f, 2.9f), MeridianRetailMaterials.Dark, false);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("CanopySuspension", entrance, new Vector3(side * 5.2f, 5.16f, -.22f),
                    new Vector3(.07f, 2.95f, .08f), MeridianRetailMaterials.Rust);
                Box("MarqueeAnchor", entrance, new Vector3(side * 5.2f, 6.61f, -.2f),
                    new Vector3(.38f, .23f, .09f), MeridianRetailMaterials.Dark);
            }
        }

        private static void Fins(Transform bay)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Box("FacadePilaster", bay, new Vector3(side * 1.85f, 9.75f, .2f),
                    new Vector3(.14f, 3.45f, .17f), MeridianRetailMaterials.Stone);
                Box("PilasterInlay", bay, new Vector3(side * 1.85f, 9.75f, .292f),
                    new Vector3(.05f, 3.18f, .015f), MeridianRetailMaterials.Brass, false);
            }
            Box("InfilledClerestory", bay, new Vector3(0, 9.65f, .15f),
                new Vector3(2.8f, 1.92f, .04f), MeridianRetailMaterials.Dark);
            for (int n = 0; n < 3; n++)
                Box("ClerestoryMullion", bay, new Vector3((n - 1) * .92f, 9.65f, .205f),
                    new Vector3(.055f, 1.95f, .085f), MeridianRetailMaterials.Stone);
        }
    }
}
