using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class MeridianRetailArchitecture
    {
        public const string GroupName = "RetailArchitecture015";

        public static void Build(Transform mall)
        {
            Transform previous = mall.Find(GroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform root = Group(GroupName, mall);
            Portals(root, mall);
            TenantWalls(root, mall);
            MeridianRetailFacade.Build(root, mall);
        }

        private static void Portals(Transform root, Transform mall)
        {
            foreach (MallLayout.Door door in MallLayout.Doors)
            {
                MallLayout.Zone? a = MallLayout.ZoneAt(door.A, door.Floor);
                MallLayout.Zone? b = MallLayout.ZoneAt(door.B, door.Floor);
                if (!a.HasValue || !b.HasValue || MallLayout.SameSpace(a.Value, b.Value)) continue;
                bool isA = a.Value.Kind == MallLayout.Kind.Store;
                if (!isA && b.Value.Kind != MallLayout.Kind.Store) continue;
                MallLayout.Zone tenant = isA ? a.Value : b.Value;
                Vector2Int cell = isA ? door.A : door.B, outside = isA ? door.B : door.A;
                Vector3 at = (MallLayout.TileTopCenter(cell, door.Floor) + MallLayout.TileTopCenter(outside, door.Floor)) * .5f;
                Transform portal = Oriented($"{tenant.Name}_Portal_{door.Floor}_{cell.x}_{cell.y}", root, at, outside - cell);
                Bind(portal, mall, cell, door.Floor);
                Material accent = MeridianRetailMaterials.Accent(tenant.Tag);
                float width = door.Wide ? MallWalls.WideWidth : 1.8f;
                float height = door.Wide ? MallWalls.WideHeight : 2.5f;
                // The frame surrounds the authored opening; the full hauling aperture remains untouched.
                foreach (float sign in new[] { -1f, 1f })
                {
                    Box("PortalReveal", portal, new Vector3(sign * (width / 2 + .09f), height / 2, .17f),
                        new Vector3(.16f, height, .19f), MeridianRetailMaterials.Dark);
                    Box("TenantPilaster", portal, new Vector3(sign * (width / 2 + .2f), height / 2, .17f),
                        new Vector3(.13f, height, .22f), MeridianRetailMaterials.Stone);
                    Box("AccentInlay", portal, new Vector3(sign * (width / 2 + .2f), height / 2, .293f),
                        new Vector3(.045f, height - .15f, .025f), accent, false);
                }
                Box("StorefrontFascia", portal, new Vector3(0, height + .18f, .2f),
                    new Vector3(width + .5f, .32f, .26f), accent);
                Box("RaisedCornice", portal, new Vector3(0, height + .34f, .22f),
                    new Vector3(width + .64f, .055f, .36f), MeridianRetailMaterials.Pale);
                Box("DarkShutterPelmet", portal, new Vector3(0, height + .04f, .1f),
                    new Vector3(width, .07f, .25f), MeridianRetailMaterials.Dark);
                for (int n = 0; n < 5; n++)
                    Box("FormerLogoMount", portal, new Vector3((n - 2) * .19f, height + .18f, .343f),
                        new Vector3(.065f, .14f - (n % 2) * .04f, .022f), MeridianRetailMaterials.Brass, false);
            }
        }

        private static void TenantWalls(Transform root, Transform mall)
        {
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            for (int x = 0; x < MallLayout.TilesX; x++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                MallLayout.Zone? zone = MallLayout.ZoneAt(cell, floor);
                if (!zone.HasValue || zone.Value.Kind != MallLayout.Kind.Store) continue;
                foreach (Vector2Int direction in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                {
                    Vector2Int neighbour = cell + direction;
                    MallLayout.Zone? next = MallLayout.ZoneAt(neighbour, floor);
                    if (!next.HasValue || MallLayout.SameSpace(zone.Value, next.Value) ||
                        MallLayout.DoorBetween(cell, neighbour, floor).HasValue || MallLayout.IsVoid(neighbour, floor)) continue;
                    Vector3 at = MallLayout.TileTopCenter(cell, floor) + new Vector3(direction.x, 0, direction.y) * 2;
                    // A wall-mounted shallow fixture adds depth without taking over the circulation grid.
                    Transform panel = Oriented($"{zone.Value.Name}_DisplayWall_{floor}_{x}_{z}_{direction}", root, at, -direction);
                    Bind(panel, mall, cell, floor);
                    Material accent = MeridianRetailMaterials.Accent(zone.Value.Tag);
                    Box("TenantCeilingRibbon", panel, new Vector3(0, 3.46f, .145f),
                        new Vector3(3.9f, .32f, .055f), accent);
                    Box("PictureRail", panel, new Vector3(0, 2.94f, .155f),
                        new Vector3(3.85f, .055f, .07f), MeridianRetailMaterials.Brass);
                    if ((x + z + floor) % 2 != 0) continue;
                    Display(panel, zone.Value.Tag, accent);
                }
            }
        }

        private static void Display(Transform panel, string tag, Material accent)
        {
            float depth = .195f;
            Box("DisplayBack", panel, new Vector3(0, 1.95f, .145f),
                new Vector3(2.8f, 1.74f, .035f), MeridianRetailMaterials.Dark);
            foreach (float side in new[] { -1f, 1f })
                Box("DisplayEdge", panel, new Vector3(side * 1.43f, 1.95f, depth),
                    new Vector3(.06f, 1.85f, .1f), accent);
            foreach (float h in new[] { 1.06f, 2.85f })
                Box("DisplayRail", panel, new Vector3(0, h, depth),
                    new Vector3(2.92f, .06f, .1f), accent);
            switch (tag)
            {
                case "electronics":
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box("DeadScreenCase", panel, new Vector3(side * .68f, 2.02f, .207f),
                            new Vector3(1.15f, .78f, .08f), MeridianRetailMaterials.Rust);
                        Box("DeadScreen", panel, new Vector3(side * .68f, 2.02f, .251f),
                            new Vector3(1.04f, .67f, .012f), MeridianRetailMaterials.Dark, false);
                    }
                    break;
                case "cinema":
                case "gallery":
                    for (int n = 0; n < 3; n++)
                    {
                        Box("EmptyPosterFrame", panel, new Vector3((n - 1) * .87f, 1.95f, .21f),
                            new Vector3(.72f, 1.38f, .09f), MeridianRetailMaterials.Brass);
                        Box("FadedPosterBacking", panel, new Vector3((n - 1) * .87f, 1.95f, .26f),
                            new Vector3(.63f, 1.28f, .012f), accent, false);
                    }
                    break;
                case "food":
                    Box("TileSplashback", panel, new Vector3(0, 1.95f, .18f),
                        new Vector3(2.68f, 1.65f, .04f), MeridianRetailMaterials.Tile);
                    for (int n = 0; n < 4; n++)
                        Box("ExtractorLouvre", panel, new Vector3(0, 2.2f + n * .13f, .235f),
                            new Vector3(2.15f, .075f, .075f), MeridianRetailMaterials.Dark);
                    break;
                case "office":
                    for (int n = 0; n < 4; n++)
                        Box("OfficeBlind", panel, new Vector3(0, 1.6f + n * .25f, .205f),
                            new Vector3(2.64f, .2f, .04f), MeridianRetailMaterials.Pale);
                    break;
                default:
                    for (int n = 0; n < 5; n++)
                        Box("WallDisplaySlat", panel, new Vector3((n - 2) * .48f, 1.95f, .2f),
                            new Vector3(.16f, 1.55f, .065f), tag == "jewelry" ? MeridianRetailMaterials.Brass : MeridianRetailMaterials.Wood);
                    Box("AbandonedDisplayRail", panel, new Vector3(0, 1.54f, .235f),
                        new Vector3(2.62f, .065f, .06f), MeridianRetailMaterials.Dark);
                    break;
            }
        }

        private static Transform Oriented(string name, Transform parent, Vector3 at, Vector2Int direction)
        {
            Transform group = Group(name, parent);
            group.localPosition = at;
            group.localRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
            return group;
        }

        private static void Bind(Transform group, Transform mall, Vector2Int cell, int floor)
        {
            StructuralSection support = MeridianDressingUtility.Support(mall, cell, floor);
            if (support != null) group.gameObject.AddComponent<SectionProp>().EditorSetup(support);
        }
    }
}
