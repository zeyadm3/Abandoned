using System.Linq;

namespace Abandoned.EditorTools
{
    public static partial class MaterialUpgrade
    {
        /// <summary>How a kind of surface ages: big light/dark variation, grime in cavities and streaks, dust on top.</summary>
        public enum Wear { Floor, Wall, Ceiling, Metal, Wood, Fabric, Street, Nature, Plain }

        private static (float variation, float grime, float dust) Weathering(Wear wear) => wear switch
        {
            Wear.Floor => (0.12f, 0.45f, 0.65f),
            Wear.Wall => (0.12f, 0.55f, 0.35f),
            Wear.Ceiling => (0.10f, 0.45f, 0f),
            Wear.Metal => (0.10f, 0.45f, 0.5f),
            Wear.Wood => (0.12f, 0.40f, 0.5f),
            Wear.Fabric => (0.10f, 0.35f, 0.55f),
            Wear.Street => (0.16f, 0.30f, 0.1f),
            Wear.Nature => (0.18f, 0f, 0f), // rain and snow keep the outdoors from greying
            _ => (0.10f, 0.40f, 0.45f),
        };

        private readonly struct Rule
        {
            public readonly string[] Keys;
            public readonly string Set, DarkSet;
            public readonly float Metres, Tint;
            public readonly bool Bare, AntiTile;
            public readonly Wear Wear;

            /// <param name="metres">How much surface one texture repeat covers.</param>
            /// <param name="tint">How much of the material's own hue the scan takes (paint 1, brick 0.3).</param>
            /// <param name="bare">Keep the scan's metalness map. Off for plain painted metal: with no reflections to show, a
            /// metallic locker in a dark room renders black. On for chipped paint, whose map is metal only at the chips.</param>
            /// <param name="antiTile">Random surfaces only: offsetting a grid (tiles, bricks, planks) doubles its joints.</param>
            public Rule(string set, float metres, float tint, bool bare, bool antiTile, Wear wear, params string[] keys)
            { Set = set; DarkSet = null; Metres = metres; Tint = tint; Bare = bare; AntiTile = antiTile; Wear = wear; Keys = keys; }

            public Rule(string set, string darkSet, float metres, float tint, bool antiTile, Wear wear, params string[] keys)
            { Set = set; DarkSet = darkSet; Metres = metres; Tint = tint; Bare = false; AntiTile = antiTile; Wear = wear; Keys = keys; }
        }

        // Matched against "<name without its map prefix> <old code-drawn map's name>", lower case; first match wins, so
        // the specific words come before the general ones (snowy asphalt before asphalt, brick before wall).
        private static readonly Rule[] Rules =
        {
            new("Snow013", 2.5f, 0.3f, false, true, Wear.Nature, "snowyasphalt", "snowypath", "trodden"),
            new("Snow010A", 3f, 0.25f, false, true, Wear.Nature, "snowdrift", "snow"),
            new("Rock035", 4f, 0.35f, false, true, Wear.Nature, "rockdark", "searock", "wetrock"),
            new("Rock051", 4f, 0.4f, false, true, Wear.Nature, "rock", "cliff"),
            new("Grass004", 3f, 0.6f, false, true, Wear.Nature, "grass", "turf", "lawn"),
            new("Ground037", 3f, 0.5f, false, true, Wear.Nature, "dirt", "earth", "ground", "soil", "clay", "infield", "dormant", "mud"),
            new("Gravel023", 2f, 0.45f, false, true, Wear.Street, "gravel", "ballast", "cinder_basecolor", "_cinder", "slag"),
            new("Asphalt031", 1.6f, 0.3f, false, true, Wear.Street, "asphalt", "road", "carpark", "streetrecess", "tarmac"),
            new("PavingStones126A", 3.5f, 0.4f, false, false, Wear.Street, "pavers", "paving", "forecourt"),
            new("Concrete034", 2.5f, 0.45f, false, true, Wear.Street, "sidewalk", "pavement", "curb", "kerb", "footing", "floor_stock"),
            new("Terrazzo018", 0.75f, 0.4f, false, true, Wear.Floor, "terrazzo", "floor_concourse", "floor_walkway", "floor_food", "floor_gallery", "floor_jewelry"),
            new("Carpet016", 1.5f, 0.85f, false, true, Wear.Fabric, "carpet", "rug", "floor_cinema", "floor_office"),
            new("Rubber004", 1f, 1f, false, true, Wear.Floor, "rubber"),
            new("DiamondPlate008A", 1f, 0.5f, true, false, Wear.Metal, "tread", "diamondplate", "checkerplate"),
            new("MetalWalkway014", 1.2f, 0.4f, true, false, Wear.Metal, "grating", "walkway", "galleryplate", "catwalk"),
            new("OfficeCeiling001", 3.6f, 1f, false, false, Wear.Ceiling, "acoustic", "mall_ceiling", "ceilingtile"),
            new("Tiles036", 1.2f, 0.8f, false, false, Wear.Wall, "walltile", "glazed", "washroom", "restroom", "bathroom", "shower"),
            new("Tiles141", 2f, 0.6f, false, false, Wear.Floor, "kitchen"),
            new("Tiles107", 1.2f, 0.6f, false, false, Wear.Floor, "floortile", "schooltile", "mall_tile", "tile", "lino", "epoxy", "vinyl"),
            new("WoodFloor051", 2f, 0.4f, false, false, Wear.Wood, "woodfloor", "oakfloor", "gymwood", "gymmaple", "stagewood", "maple", "parquet"),
            new("Metal063", 2f, 0.3f, true, true, Wear.Metal, "rust", "oxide", "oxidised", "corroded", "wetiron", "moldiron", "coolingiron"),
            new("CorrugatedSteel009", 2.5f, 0.7f, true, false, Wear.Metal, "corrugated", "cladding"),
            new("Planks037A", 2f, 0.85f, false, false, Wear.Wood, "fenceweathered", "picket", "boardwood", "planks", "timber", "dunnage", "deck", "pallet"),
            new("Chipboard004", 1.5f, 0.6f, false, true, Wear.Wood, "boardedshop", "chipboard", "plywood", "hoarding", "board"),
            new("Metal041B", 1.5f, 0.75f, true, true, Wear.Metal, "bars", "railing", "handrail", "rail", "fence", "pipe"),
            new("PaintedMetal012", 1.2f, 1f, true, true, Wear.Metal, "locker", "cabinet", "filing", "appliance", "console", "wornpaint", "safety", "machine"),
            new("Metal016", 1.5f, 1f, true, true, Wear.Metal, "fixturetrim", "storefronttrim", "escalator", "bridgemetal"),
            new("Plastic010", "Plastic011", 1f, 1f, true, Wear.Plain, "plastic", "acrylic"),
            new("Fabric030", 0.6f, 1f, false, true, Wear.Fabric, "upholstery", "fabric", "seat", "cushion", "sofa", "couch"),
            new("Leather037", 1f, 0.8f, false, true, Wear.Fabric, "leather"),
            new("Marble012", 2f, 0.5f, false, true, Wear.Floor, "marble"),
            new("WoodSiding009", 2f, 1f, false, false, Wear.Wood, "siding"),
            new("PaintedBricks004", 1.55f, 1f, false, false, Wear.Wall, "paintedbrick", "mall_block", "block", "cmu"),
            new("Bricks106A", 1.6f, 0.35f, false, false, Wear.Wall, "brick"),
            new("Concrete042A", 3f, 0.45f, false, true, Wear.Wall, "dampconcrete", "saltconcrete", "basement"),
            new("Concrete036", 3f, 0.45f, false, true, Wear.Wall, "tunnel", "cellar", "vault", "sewer"),
            new("Concrete046", 3f, 0.6f, false, true, Wear.Wall, "castconcrete", "smoothconcrete", "precast"),
            new("Concrete031", 3f, 0.45f, false, true, Wear.Floor, "concrete", "pier", "floor", "slab"),
            new("PaintedPlaster006", 3f, 1f, false, true, Wear.Wall, "facade", "render", "stucco"),
            new("PaintedPlaster017", 2.5f, 1f, false, true, Wear.Wall, "hallpaint", "institutionalpaint", "paintedplaster"),
            new("Plaster001", 2.5f, 1f, false, true, Wear.Wall, "limestone", "stone"),
            new("Plaster007", 2.5f, 1f, false, true, Wear.Wall, "brokenplaster", "damagedplaster", "oldplaster", "crumbling"),
            // Ordinary walls are clean paint over plaster: the weathering ages them, a mouldy scan made every room a cellar.
            new("PaintedPlaster017", 2.5f, 1f, false, true, Wear.Wall, "plaster", "wall", "dado", "ceiling", "interior"),
            new("Metal049A", 1.5f, 0.75f, false, true, Wear.Metal, "steel", "metal", "iron", "pole", "guard", "beam", "girder", "column", "truss", "riveted"),
            new("WoodFloor051", 2f, 0.5f, false, false, Wear.Wood, "oak", "wood"),
            // Last: plain "paint" on fittings and props (painted plaster and halls are caught above).
            new("Metal016", 1.5f, 1f, true, true, Wear.Metal, "paint"),
        };

        // The map prefix carries no meaning ("IronFactory" is not iron, "Island" not a kind of ground).
        private static readonly string[] Prefixes =
        {
            "ironfactory_", "island_", "burley_", "hq_", "mall_", "house_", "winter_", "meridian_", "salvage_", "larch_",
            "tutorial_", "school_", "factory_", "prison_", "env_",
        };

        // Never touched: light, glass, liquids, effects, foliage, printed and signed things, characters and kit colours,
        // vehicles (they drive away, and the world-space grime would slide), roof shingles (no scan fits). Matched against
        // the start of whole words of the name ("Street" is not "tree"), after the matching rule's own key is taken out
        // (so "WetIron" is iron, not a wet overlay).
        private static readonly string[] Skip =
        {
            "glow", "glass", "glazing", "lamp", "light", "led", "crt", "emiss", "flame", "spark", "lava", "steam", "dust", "shaft",
            "sky", "sea", "water", "foam", "smoke", "veil", "particle", "outline", "ink", "stripe", "lane_paint", "road_paint",
            "window", "mirror", "chrome", "gold", "brass", "copper", "bronze", "porcelain", "ornament", "tree", "leaves", "leaf",
            "bark", "branch", "needle", "crown", "ivy", "moss", "scrub", "ice", "frost", "plant", "pine", "foliage", "conifer",
            "broadleaf", "guano", "grime", "stain", "mould", "chalk", "label", "sign", "screen", "lens", "backdrop", "greybox",
            "hat_", "acc_", "mattress", "pillow", "bedding", "curtain", "blanket", "canvas", "cloth", "wool", "denim", "felt",
            "tape", "paper", "card", "book", "blueprint", "clipboard", "cork", "pegboard", "marker", "warning", "hazard",
            "caution", "beacon", "signal", "display", "mug", "drum", "bin_", "forklift", "pallet_jack", "toolbox", "loot",
            "threat", "crew", "skin", "cosmetic", "decal", "puddle", "wet", "soot", "vacant_windows", "distant_mass", "mass",
            "silhouette", "icon", "ui_", "shingles", "graffiti", "scorch", "crack", "truck", "van", "boat", "launch", "vehicle",
            "car_kit", "carkit", "colormap", "mountain", "slope", "ridge", "cable",
        };

        private static string StripPrefix(string lower)
        {
            foreach (string p in Prefixes)
                if (lower.StartsWith(p)) return lower.Substring(p.Length);
            return lower;
        }

        /// <summary>The scan a material of this name (and old drawn map, if any) gets, or null. For tests and the audit.</summary>
        public static string ScanFor(string materialName, string oldMapName = "")
        {
            string core = StripPrefix(materialName.ToLowerInvariant());
            Rule? rule = Match(core + " " + oldMapName.ToLowerInvariant(), out string key);
            if (rule == null) return null;
            string rest = Words(core.Contains(key) ? core.Remove(core.IndexOf(key, System.StringComparison.Ordinal), key.Length) : core);
            return Skip.Any(s => rest.Contains("_" + s)) ? null : rule.Value.Set;
        }

        private static Rule? Match(string text, out string key)
        {
            foreach (Rule r in Rules)
            {
                key = r.Keys.FirstOrDefault(text.Contains);
                if (key != null) return r;
            }
            key = null;
            return null;
        }
    }
}
