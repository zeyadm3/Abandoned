using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Site = Abandoned.EditorTools.StationSite;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Cloudline Tower's city (Docs/SKYSCRAPER_DOWNTOWN.md): a 720 m grid of 48 high-rise blocks round the tower's own
    /// block, with clean streets, kerbed pavements and zebras at every junction (StreetGrid), towers with mullion fins,
    /// spandrel bands, podium shops and varied crowns, then, as the scene's own backdrop under <c>Exterior/Skyline</c>
    /// (the open world hides it, and the builder's occlusion pass leaves it out), the street life and two outer rings of
    /// mid-rise city. The tower's block, its plaza, Broadway in front of it and the truck are never touched here.
    /// </summary>
    public static class SkyscraperDowntown
    {
        /// <summary>The downtown's ground: the street grid the open world fits its land to (the rings beyond are scene-only).</summary>
        public static readonly Rect Ground = Rect.MinMaxRect(-360, -338, 360, 382);
        public const int TowerCount = 48;
        /// <summary>The roads' top; block pavements stand 6 cm over it, at 0.</summary>
        public const float StreetTop = -.06f;

        public static void Build(Transform exterior, SkyscraperArt art)
        {
            Transform city = Group("Downtown", exterior);
            var palette = new SkyscraperDowntownMaterials();
            var kit = new StationArt(SkyscraperDowntownMaterials.Folder, "SkyTown", new Color(.55f, .4f, .33f), new Color(.16f, .22f, .28f), new Color(1f, .8f, .55f));
            var stock = new StationRingStock(kit);
            float previous = Site.GroundTop;
            Site.GroundTop = StreetTop;
            try
            {
                SkyscraperDowntownStreets.Build(city, art, palette, stock);
                for (int z = -3; z <= 3; z++)
                for (int x = -3; x <= 3; x++)
                {
                    if (x == 0 && z == 0) continue;
                    int seed = (x + 4) * 173 + (z + 4) * 71;
                    bool close = Mathf.Abs(x) <= 1 && Mathf.Abs(z) <= 1;
                    float height = close ? new[] { 388f, 366f, 342f, 396f, 372f, 354f, 382f, 326f }[seed % 8] : 108f + seed * 37 % 205;
                    if (x == 0 && z == 1) height = 396f;
                    SkyscraperDowntownBuildings.Tower(city, art, palette, new Vector3(x * 100f, 0f, z * 100f + 22f), height, seed);
                }
                // Scene-only backdrop: the world streams its own land here and hides this group.
                Transform skyline = Group("Skyline", exterior);
                SkyscraperDowntownStreetscape.Build(skyline, art, palette, kit, stock);
                SkyscraperDowntownRings.Build(skyline, palette, kit, stock);
                StationSurfaces.Apply(skyline, kit);
            }
            finally
            {
                Site.GroundTop = previous;
            }
            Debug.Log("[Skyscraper] Downtown: 48 neighbouring towers on a 720 m grid, street life, and two outer rings of mid-rise city.");
        }
    }
}
