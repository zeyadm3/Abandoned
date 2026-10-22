using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Floor 2 (y = 44m): Sky Lobby reception, Grand Dining Room with panoramic skyline windows,
    /// cantilevered floor sections (collapsible), Cocktail Bar & Lounge, and outdoor Observation Terrace.
    /// </summary>
    public static class SkyscraperRestaurantFloor
    {
        public static List<Transform> Build(Transform root, SkyscraperArt art, out List<Transform> collapsibleDiningTiles)
        {
            Transform floorGroup = Group("RestaurantFloor_44m", root);
            collapsibleDiningTiles = new List<Transform>();
            var allSlabs = new List<Transform>();

            float y = L.RestaurantY;

            // 1. Structural Flooring Grid (4m x 4m sections)
            Transform slabsGroup = Group("FloorSlabs", floorGroup);
            for (float x = -14f; x <= 14f; x += 4f)
            {
                for (float z = -6f; z <= 18f; z += 4f)
                {
                    // Skip the central elevator shaft opening
                    if (Mathf.Abs(x) < 2.0f && Mathf.Abs(z) < 2.0f) continue;
                    // Skip the stairwell tower
                    if (x < -8.5f && Mathf.Abs(z) < 3.2f) continue;

                    Transform tile = Group($"Tile_{x:00}_{z:00}", slabsGroup);
                    tile.position = new Vector3(x, y, z);
                    BoxCollider col = tile.gameObject.AddComponent<BoxCollider>();
                    col.center = new Vector3(0f, -0.2f, 0f);
                    col.size = new Vector3(3.9f, 0.4f, 3.9f);

                    // Parquet in dining & lounge, Marble in lobby & terrace
                    Material surfaceMat = (z > 4f) ? art.Parquet : art.Marble;
                    Box("Visual", tile, col.center, col.size, surfaceMat, false);
                    tile.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
                    allSlabs.Add(tile);

                    // Slabs adjacent to northern window ledge are deteriorating cantilevered tiles!
                    if (z >= 18f && Mathf.Abs(x) <= 6f)
                        collapsibleDiningTiles.Add(tile);
                }
            }

            // Ceiling with acoustic tiles and recessed fixtures overhead at y = 50m
            Box("RestaurantCeiling", floorGroup, new Vector3(0f, L.RoofY, 6f), new Vector3(34f, 0.4f, 30f), art.CeilingTile);

            // 2. Sky Lobby & Maître D' Reception (z: -8 to 0)
            Transform lobby = Group("SkyLobby", floorGroup);
            Transform podium = Group("MaitreDPodium", lobby);
            podium.position = new Vector3(0f, y, -5.5f);
            Box("PodiumStand", podium, new Vector3(0f, 0.6f, 0f), new Vector3(0.8f, 1.2f, 0.6f), art.Brass);
            Box("GuestLedger", podium, new Vector3(0f, 1.25f, 0f), new Vector3(0.5f, 0.08f, 0.4f), art.Leather, false);

            // Velvet Rope Stanchions guiding arriving guests
            for (float x = -2.5f; x <= 2.5f; x += 1.25f)
            {
                if (Mathf.Abs(x) < 0.5f) continue;
                Primitive(PrimitiveType.Cylinder, "StanchionPost", lobby, new Vector3(x, y + 0.5f, -4f),
                    new Vector3(0.08f, 1.0f, 0.08f), art.Brass);
            }

            // 3. Grand Dining Room (North half, z: 2 to 20, x: -16 to +16)
            Transform dining = Group("GrandDiningRoom", floorGroup);

            // Panoramic Floor-to-Ceiling North Curtain Glass Wall
            Box("NorthGlassCurtain", dining, new Vector3(0f, y + 3f, 21.8f), new Vector3(32f, 5.8f, 0.2f), art.Glass);

            // Central Grand Plinth / Statue Base (Loot spawn location)
            Transform plinth = Group("CentralMarblePlinth", dining);
            plinth.position = new Vector3(0f, y, 10f);
            Box("PlinthBase", plinth, new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.8f, 3.2f), art.Marble);
            Box("PlinthTier", plinth, new Vector3(0f, 0.9f, 0f), new Vector3(2.4f, 0.3f, 2.4f), art.Brass);

            // Banquet Dining Tables & Seating Clusters
            Vector3[] tablePositions = {
                new(-10f, y, 6f), new(-6f, y, 6f), new(6f, y, 6f), new(10f, y, 6f),
                new(-12f, y, 12f), new(-6f, y, 14f), new(6f, y, 14f), new(12f, y, 12f),
                new(-8f, y, 18f), new(8f, y, 18f)
            };

            for (int t = 0; t < tablePositions.Length; t++)
            {
                Vector3 pos = tablePositions[t];
                Transform table = Group($"DiningTable_{t}", dining);
                table.position = pos;

                // Round Table with Linen Tablecloth
                Primitive(PrimitiveType.Cylinder, "Tabletop", table, new Vector3(0f, 0.75f, 0f),
                    new Vector3(1.8f, 0.08f, 1.8f), art.Marble);
                Primitive(PrimitiveType.Cylinder, "Pedestal", table, new Vector3(0f, 0.38f, 0f),
                    new Vector3(0.3f, 0.72f, 0.3f), art.Brass);

                // 4 Dining Chairs around each table
                for (int c = 0; c < 4; c++)
                {
                    float angle = c * Mathf.PI * 0.5f;
                    Vector3 chairOffset = new(Mathf.Cos(angle) * 1.1f, 0f, Mathf.Sin(angle) * 1.1f);
                    Transform chair = Group($"Chair_{c}", table);
                    chair.localPosition = chairOffset;
                    chair.localRotation = Quaternion.LookRotation(-chairOffset);
                    Box("Seat", chair, new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.1f, 0.5f), art.Leather);
                    Box("Back", chair, new Vector3(0f, 0.85f, -0.2f), new Vector3(0.5f, 0.7f, 0.08f), art.Leather);
                    Box("Legs", chair, new Vector3(0f, 0.22f, 0f), new Vector3(0.45f, 0.44f, 0.45f), art.Brass, false);
                }
            }

            // 4. Cocktail Lounge & Curved Bar (East Wing, x: 4 to 16, z: -8 to 2)
            Transform bar = Group("CocktailLoungeBar", floorGroup);

            // Curved Bar Counter
            Transform counter = Group("BarCounter", bar);
            counter.position = new Vector3(8.5f, y, -2f);
            Box("CounterBase", counter, new Vector3(0f, 0.55f, 0f), new Vector3(8f, 1.1f, 1.2f), art.Mahogany);
            Box("CounterTop", counter, new Vector3(0f, 1.15f, 0f), new Vector3(8.4f, 0.1f, 1.5f), art.Marble);
            Box("BrassFootrail", counter, new Vector3(0f, 0.2f, -0.7f), new Vector3(8f, 0.06f, 0.06f), art.Brass, false);

            // Back-Bar Mirrored Spirit Shelving
            Transform backBar = Group("BackBarShelves", bar);
            backBar.position = new Vector3(8.5f, y, 0.5f);
            Box("WallBacking", backBar, new Vector3(0f, 1.8f, 0f), new Vector3(8f, 3.2f, 0.4f), art.Mahogany);
            Box("MirrorBack", backBar, new Vector3(0f, 2.0f, -0.18f), new Vector3(7.6f, 2.6f, 0.04f), art.Glass, false);
            for (float s = 1.0f; s <= 3.0f; s += 0.6f)
                Box($"Shelf_{s:0.0}", backBar, new Vector3(0f, s, -0.2f), new Vector3(7.6f, 0.05f, 0.35f), art.Glass, false);

            // Bar Stools along the counter
            for (float bx = -3.2f; bx <= 3.2f; bx += 1.6f)
            {
                Transform stool = Group($"BarStool_{bx:0.0}", counter);
                stool.localPosition = new Vector3(bx, 0f, -1.1f);
                Primitive(PrimitiveType.Cylinder, "Cushion", stool, new Vector3(0f, 0.75f, 0f),
                    new Vector3(0.42f, 0.12f, 0.42f), art.Leather);
                Primitive(PrimitiveType.Cylinder, "Column", stool, new Vector3(0f, 0.36f, 0f),
                    new Vector3(0.1f, 0.7f, 0.1f), art.Brass);
            }

            // 5. Outdoor Observation Terrace (West / Northwest Balcony)
            Transform terrace = Group("ObservationTerrace", floorGroup);
            terrace.position = new Vector3(-16f, y, 14f);
            Box("TerraceFloor", terrace, Vector3.zero, new Vector3(4f, 0.3f, 12f), art.Concrete);
            Box("SafetyGlassRailingW", terrace, new Vector3(-2f, 0.7f, 0f), new Vector3(0.1f, 1.3f, 12f), art.Glass);
            Box("SafetyGlassRailingN", terrace, new Vector3(0f, 0.7f, 6f), new Vector3(4f, 1.3f, 0.1f), art.Glass);

            return allSlabs;
        }
    }
}
