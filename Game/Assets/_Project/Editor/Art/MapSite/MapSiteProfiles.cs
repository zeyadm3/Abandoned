namespace Abandoned.EditorTools
{
    /// <summary>
    /// What the site pass (MapSite) gives each map, chosen to keep the map's own hour and weather (user, 2026-10-22):
    /// a photographed sky of the same mood, which trees, whether grass grows on its ground, whether a wooded terrain
    /// closes off the edge of its ground, and how its outdoors sounds.
    /// </summary>
    public readonly struct MapSiteProfile
    {
        public readonly string Scene;
        /// <summary>A Poly Haven sky in Art/Skies, and where its sun is across the panorama (0..1, measured at fetch).</summary>
        public readonly string Sky;
        public readonly float SkySunU;
        /// <summary>The sky's mean upper-hemisphere radiance (measured at fetch) and how bright it is drawn: about as bright
        /// as the map's old sky, or the eye adaptation, metering a far brighter sky, darkens the whole map.</summary>
        public readonly float SkyLevel, SkyExposure;
        /// <summary>Snow on the ground: evergreen trees and snags only, no grass, a snow terrain.</summary>
        public readonly bool Winter;
        public readonly bool Grass;
        /// <summary>A terrain ring with forest past the edge of the map's ground (maps that end in a hard slab edge).</summary>
        public readonly bool Ring;
        public readonly float Birds, Wind, Crickets;
        public readonly bool Crows;

        public MapSiteProfile(string scene, string sky, float skySunU, float skyLevel, float skyExposure, bool winter, bool grass,
            bool ring, float birds, float wind, float crickets, bool crows)
        {
            Scene = scene; Sky = sky; SkySunU = skySunU; SkyLevel = skyLevel; SkyExposure = skyExposure;
            Winter = winter; Grass = grass; Ring = ring;
            Birds = birds; Wind = wind; Crickets = crickets; Crows = crows;
        }
    }

    public static class MapSiteProfiles
    {
        public static readonly MapSiteProfile[] All =
        {
            // The company's yard at dusk under cloud.
            new("HQ", "kloppenheim_01_puresky", .5981f, .914f, .22f, winter: false, grass: true, ring: false, birds: .2f, wind: .4f, crickets: .3f, crows: true),
            // The mall at night: the glow of a town on the clouds, crickets, no birds.
            new("Mall", "kloppenheim_07_puresky", .2524f, .327f, .7f, winter: false, grass: true, ring: false, birds: 0f, wind: .35f, crickets: .4f, crows: false),
            // Winter afternoon at the school: a low sun under broken cloud, snow everywhere.
            new("School", "kloppenheim_06_puresky", .6118f, .781f, 1f, winter: true, grass: false, ring: true, birds: 0f, wind: .5f, crickets: 0f, crows: true),
            // The iron works at a smoky dusk.
            new("Factory", "industrial_sunset_02_puresky", .5996f, .749f, .3f, winter: false, grass: true, ring: false, birds: 0f, wind: .35f, crickets: .2f, crows: true),
            // The island at noon: a clear sky over the bay (the surf is the island's own soundscape).
            new("Prison", "kloofendal_43d_clear_puresky", .6003f, 1.254f, 1f, winter: false, grass: true, ring: false, birds: .3f, wind: .45f, crickets: 0f, crows: false),
            // A December afternoon in Michigan: misty, snow on the ground.
            new("Tutorial", "kloofendal_misty_morning_puresky", .6191f, 1.157f, .9f, winter: true, grass: false, ring: true, birds: 0f, wind: .45f, crickets: 0f, crows: true),
        };

        public static bool TryGet(string scene, out MapSiteProfile profile)
        {
            foreach (MapSiteProfile p in All)
                if (p.Scene == scene)
                {
                    profile = p;
                    return true;
                }
            profile = default;
            return false;
        }
    }
}
