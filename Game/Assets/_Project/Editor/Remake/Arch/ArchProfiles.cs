using System.Text.RegularExpressions;
using UnityEngine;
using Surface = Abandoned.EditorTools.Remake.ArchStyle.Surface;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The trim profiles every style draws from (metres; x away from the wall, y along it, starting and ending on the
    /// wall with the solid on the left, see <see cref="ArchMesh.Sweep"/>), and the material-name rules shared by all maps
    /// (a map's own rules are read first).
    /// </summary>
    public static class ArchProfiles
    {
        public static Vector2 V(float x, float y) => new(x, y);

        // ── Bases ──────────────────────────────────────────────────────────────
        public static Vector2[] KickPlate(float h) => new[] { V(0, 0), V(.006f, 0), V(.006f, h), V(0, h) };
        public static Vector2[] SquareBase(float h, float t) =>
            new[] { V(0, 0), V(t, 0), V(t, h - .004f), V(t - .004f, h), V(0, h) };
        public static Vector2[] Colonial(float h) =>
            new[] { V(0, 0), V(.018f, 0), V(.018f, h - .035f), V(.014f, h - .022f), V(.009f, h - .008f), V(0, h) };
        public static Vector2[] RubberCove(float h) =>
            new[] { V(0, 0), V(.024f, 0), V(.013f, .006f), V(.007f, .018f), V(.006f, h), V(0, h) };
        public static Vector2[] Plinth(float h, float d) => new[] { V(0, 0), V(d, 0), V(d, h - d), V(0, h) };

        // ── Tops (y ≤ 0, hanging from the line they follow) ─────────────────────
        public static Vector2[] Cove(float s) =>
            new[] { V(0, -s), V(s * .15f, -s * .86f), V(s * .5f, -s * .42f), V(s * .88f, -s * .1f), V(s, 0), V(0, 0) };
        public static Vector2[] Crown(float s) =>
            new[] { V(0, -s), V(.012f, -s), V(.012f, -s * .72f), V(s * .45f, -s * .58f), V(s * .62f, -s * .3f), V(s, -s * .16f), V(s, 0), V(0, 0) };
        public static Vector2[] ShadowBead(float s) => new[] { V(0, -s), V(.008f, -s), V(.008f, 0), V(0, 0) };

        // ── Runs along a face (centred on a height) ──────────────────────────────
        public static Vector2[] Rail(float h, float d) =>
            new[] { V(0, -h * .5f), V(d * .6f, -h * .5f), V(d, -h * .2f), V(d, h * .2f), V(d * .6f, h * .5f), V(0, h * .5f) };
        public static Vector2[] Band(float h, float d) =>
            new[] { V(0, -h * .5f), V(d, -h * .5f), V(d, h * .5f - d * .6f), V(d * .4f, h * .5f), V(0, h * .5f) };

        // ── Casings (y runs from the opening's edge into the wall) ───────────────
        public static Vector2[] Architrave(float w, float d) =>
            new[] { V(0, 0), V(d * .75f, 0), V(d, w * .12f), V(d, w * .7f), V(d * .55f, w), V(0, w) };
        public static Vector2[] SteelJamb(float w, float d) => new[] { V(0, 0), V(d, 0), V(d, w), V(0, w) };
        public static Vector2[] Reveal(float w, float d) => new[] { V(0, 0), V(d, w * .4f), V(d, w), V(0, w) };

        /// <summary>Material names no style rebuilds: markings, lamps, vehicles, props, foliage, decals.</summary>
        public static readonly Regex Never = ArchStyle.R(
            @"grime|oil|linepaint|stripe|safety|marking|lamp|glow|lens|car(trim|paint|chrome|under|well)|^car_|truck|van|wire|snow|drift|" +
            @"rock|bars|railing|lattice|townframe|guard|curtain(?!_glass)|linen|fabric|bedding|blind|salvage|distant|ballast|cinder|" +
            @"lava|shadowjoint|wayfinding|amber|hazard|sign|paper|stain|rust|dunnage|void|gutter|downpipe|hose|weathering|vacant|" +
            @"deaddisplay|windowframe|trim$|_trim|brass|chrome|paint(red|cream|blue|green|black)|kerb|curb|grass|gravel|dirt|soil|" +
            @"clay|mold|ingot|bed$|boxcar|copperbus|bushing|dumpster|barrier|ornament|sofa|mirror|rubber|locker|stage|vehicle|" +
            @"pier$|fence|doorsteel|doorwood|wood_?door|display|rail|^env_|scrap|lane|canvas|cargo|mat$");

        /// <summary>Shared rules, read after the map's own.</summary>
        public static readonly (Regex, Surface)[] Shared =
        {
            (ArchStyle.R(@"glass|glazing|shopfront|tower_cladding|window"), Surface.Glass),
            (ArchStyle.R(@"ceiling|acoustic|soffit|popcorn"), Surface.Ceiling),
            (ArchStyle.R(@"walltile|wall_tile|kitchen_tiles|dado"), Surface.Tile),
            (ArchStyle.R(@"floor|vinyl|carpet|terrazzo|parquet|sidewalk|pavement|paving|asphalt|grating|epoxy|gymwood|tarroof|roof|plate$|_board$"), Surface.Floor),
            (ArchStyle.R(@"brick|block|masonry"), Surface.Masonry),
            (ArchStyle.R(@"concrete|core|cement|facade"), Surface.Concrete),
            (ArchStyle.R(@"limestone|granite|marble|stone|porcelain|colonnade"), Surface.Stone),
            (ArchStyle.R(@"cladding|corrugat"), Surface.Ribbed),
            (ArchStyle.R(@"siding|oak|wood|walnut|mahogany|timber|panel"), Surface.Boards),
            (ArchStyle.R(@"plaster|ivory|white|wall|lining|interior"), Surface.Flat),
            (ArchStyle.R(@"steel|metal|iron|champagne|lift"), Surface.Steel),
        };

        public static Surface Classify(ArchStyle style, Material material)
        {
            if (material == null) return Surface.Skip;
            string name = material.name;
            foreach ((Regex pattern, Surface surface) in style.Rules)
                if (pattern.IsMatch(name)) return surface;
            if (Never.IsMatch(name)) return Surface.Skip;
            foreach ((Regex pattern, Surface surface) in Shared)
                if (pattern.IsMatch(name)) return surface;
            return Surface.Skip;
        }
    }
}
