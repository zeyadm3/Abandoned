using System;
using System.Text.RegularExpressions;

namespace Abandoned.EditorTools
{
    public static partial class MaterialRemaster
    {
        public enum SurfaceKind
        {
            None, Snow, TroddenSnow, Grass, Soil, Rock, Gravel, Asphalt, Paving, Concrete, CastConcrete,
            Terrazzo, FloorTile, GlazedTile, AcousticCeiling, Plaster, Carpet, WoodFloor, Timber, Siding,
            PaintedWood, PaintedMetal, BareMetal, Rust, Brass, Copper, Chrome, Fabric, Canvas, Leather,
            Paper, Cardboard, Rubber, Plastic, Cork, Shingle, Brick, PaintedBlock, Stone, CorrugatedSteel, TreadPlate, Grating
        }

        private readonly struct Profile
        {
            public readonly string Scan;
            public readonly float Metres, Normal, Smoothness, Variation, Grime, Dust;
            public readonly bool Metal, AntiTile;
            public Profile(string scan, float metres, float normal, float smoothness, bool metal = false,
                bool antiTile = true, float variation = .055f, float grime = .12f, float dust = .14f)
            {
                Scan = scan; Metres = metres; Normal = normal; Smoothness = smoothness; Metal = metal;
                AntiTile = antiTile; Variation = variation; Grime = grime; Dust = dust;
            }
        }

        private static readonly Regex WordBreak = new(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|[_\s]+|(?<=\D)(?=\d)|(?<=\d)(?=\D)");
        private static bool Has(string name, params string[] words)
        {
            foreach (string word in words) if (name.Contains("_" + word + "_", StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>Semantic material categories, independent of map prefixes and names like IronFactory.</summary>
        public static SurfaceKind Classify(string materialName)
        {
            if (materialName != null && materialName.StartsWith("IronFactory_", StringComparison.OrdinalIgnoreCase))
                materialName = materialName.Substring("IronFactory_".Length);
            string n = "_" + WordBreak.Replace(materialName ?? "", "_").ToLowerInvariant() + "_";
            if (Has(n, "glow", "glass", "glazing", "window", "mirror", "sky", "sea", "water", "wet", "puddle", "oil",
                "particle", "dust", "smoke", "soot", "flame", "spark", "shaft", "outline", "ink", "logo", "sign", "label",
                "screen", "crt", "led", "lens", "stripe", "hazard", "lane", "graffiti", "crack", "stain", "grime",
                "mould", "scorch", "leaves", "leaf", "bark", "foliage", "tree", "branch", "needle", "ivy", "moss", "scrub",
                "pine", "crown", "plant", "ornament", "crest", "emblem", "threat", "crew", "skin", "cosmetic", "silhouette")) return SurfaceKind.None;
            if (Has(n, "snowy", "trodden")) return SurfaceKind.TroddenSnow;
            if (Has(n, "snow", "snowdrift")) return SurfaceKind.Snow;
            if (Has(n, "terrazzo") || Has(n, "concourse", "walkway", "food", "gallery", "jewelry") && Has(n, "floor")) return SurfaceKind.Terrazzo;
            if (Has(n, "roofing", "shingle", "shingles")) return SurfaceKind.Shingle;
            if (Has(n, "grass", "turf", "lawn")) return SurfaceKind.Grass;
            if (Has(n, "dirt", "earth", "soil", "ground", "clay", "infield", "dormant")) return SurfaceKind.Soil;
            if (Has(n, "rock", "cliff", "stone") && !Has(n, "facade", "limestone")) return SurfaceKind.Rock;
            if (Has(n, "gravel", "ballast", "cinder", "slag")) return SurfaceKind.Gravel;
            if (Has(n, "asphalt", "road", "carpark", "tarmac", "recess")) return SurfaceKind.Asphalt;
            if (Has(n, "corrugated", "cladding") && !Has(n, "siding")) return SurfaceKind.CorrugatedSteel;
            if (Has(n, "pavers", "paving", "forecourt", "pavement", "sidewalk", "kerb", "curb")) return SurfaceKind.Paving;
            if (Has(n, "brick") && !Has(n, "painted")) return SurfaceKind.Brick;
            if (Has(n, "block", "cmu") || Has(n, "brick", "paintedbrick") && Has(n, "painted")) return SurfaceKind.PaintedBlock;
            if (Has(n, "limestone") || Has(n, "facade") && Has(n, "stone")) return SurfaceKind.Stone;
            if (Has(n, "paper", "pages", "books", "book") || Has(n, "ivory") && !Has(n, "porcelain")) return SurfaceKind.Paper;
            if (Has(n, "cardboard", "carton", "cartons", "packing")) return SurfaceKind.Cardboard;
            if (Has(n, "corkboard", "cork", "masonite", "pegboard")) return SurfaceKind.Cork;
            if (Has(n, "rubber", "tire", "tyre", "cable", "seal", "net")) return SurfaceKind.Rubber;
            if (Has(n, "chrome", "silver")) return SurfaceKind.Chrome;
            if (Has(n, "brass", "gold", "bronze")) return SurfaceKind.Brass;
            if (Has(n, "copper")) return SurfaceKind.Copper;
            if (Has(n, "leather")) return SurfaceKind.Leather;
            if (Has(n, "canvas", "cloth", "blanket", "curtain", "bedding", "pillow", "mattress", "denim", "wool", "felt", "wrap")) return SurfaceKind.Canvas;
            if (Has(n, "fabric", "upholstery", "seat", "chair", "cushion", "sofa", "couch", "fur")) return SurfaceKind.Fabric;
            if (Has(n, "carpet", "rug", "cinema", "office") && !Has(n, "wall")) return SurfaceKind.Carpet;
            if (Has(n, "acoustic", "ceilingtile") || Has(n, "ceiling") && Has(n, "mall", "burley", "school", "fallen")) return SurfaceKind.AcousticCeiling;
            if (Has(n, "hall", "institutional", "interior", "wall") && Has(n, "paint", "painted", "plaster")) return SurfaceKind.Plaster;
            if (Has(n, "bath", "bathroom", "shower", "glazed", "washroom", "restroom")) return SurfaceKind.GlazedTile;
            if (Has(n, "floor", "gym", "stage", "maple", "parquet") && Has(n, "oak", "wood", "woodfloor", "gymwood", "stagewood", "maple", "parquet")) return SurfaceKind.WoodFloor;
            if (Has(n, "siding", "cladding") && !Has(n, "metal", "corrugated", "architecture")) return SurfaceKind.Siding;
            if (Has(n, "trim", "frontdoor", "door", "blind", "slats", "porcelain", "white") &&
                !Has(n, "metal", "steel", "fixture", "storefront", "van", "vehicle", "hull", "truck", "hardware")) return SurfaceKind.PaintedWood;
            if (Has(n, "wood", "oak", "timber", "walnut", "planks", "pallet", "dunnage", "board", "plywood", "chipboard")) return SurfaceKind.Timber;
            if (Has(n, "floortile", "tile", "lino", "vinyl", "epoxy", "kitchen", "countertop")) return SurfaceKind.FloorTile;
            if (Has(n, "rust", "oxide", "oxidised", "oxidized", "corroded")) return SurfaceKind.Rust;
            if (Has(n, "tread", "diamondplate", "checkerplate")) return SurfaceKind.TreadPlate;
            if (Has(n, "grating", "catwalk", "galleryplate")) return SurfaceKind.Grating;
            if (Has(n, "locker", "cabinet", "filing", "appliance", "console", "enamel", "paint", "painted", "safety", "machine", "housing", "bin", "toolbox", "forklift")) return SurfaceKind.PaintedMetal;
            if (Has(n, "steel", "metal", "iron", "bars", "rail", "railing", "handrail", "pipe", "pole", "guard", "beam", "girder", "column", "truss", "fence", "grating", "plate", "clip", "hardware", "darkmetal", "track")) return SurfaceKind.BareMetal;
            if (Has(n, "cast", "smooth", "precast") && Has(n, "concrete")) return SurfaceKind.CastConcrete;
            if (Has(n, "concrete", "pier", "slab", "basement", "tunnel", "cellar", "foundation", "lining", "floor", "stairs")) return SurfaceKind.Concrete;
            if (Has(n, "plaster", "wall", "dado", "limestone", "facade", "render", "stucco", "ceiling", "interior", "block", "brick")) return SurfaceKind.Plaster;
            if (Has(n, "plastic", "acrylic", "battery", "casing", "black")) return SurfaceKind.Plastic;
            // A few authored props are named for the object rather than its surface. Their print/palette is retained.
            if (Has(n, "flywheel", "gear", "anvil", "safe", "vice", "generator", "lathe", "motor", "welding", "tank", "radiator", "reel", "pump", "gauge", "gramophone", "watch")) return SurfaceKind.BareMetal;
            if (Has(n, "loot", "islandloot", "factoryloot", "furniture", "electronics", "clothing", "toys", "display", "hull", "van", "vehicle", "chalk", "orange")) return SurfaceKind.Plastic;
            return SurfaceKind.None;
        }

        private static Profile For(SurfaceKind kind) => kind switch
        {
            SurfaceKind.Snow => new("Snow010A", .9f, .15f, .12f, variation: .025f, grime: 0, dust: 0),
            SurfaceKind.TroddenSnow => new("Snow013", 1.2f, .24f, .14f, variation: .03f, grime: 0, dust: 0),
            SurfaceKind.Grass => new("Grass004", 1.5f, .2f, .1f, variation: .08f, grime: 0, dust: 0),
            SurfaceKind.Soil => new("Ground037", 1.5f, .35f, .18f, variation: .08f, grime: 0, dust: 0),
            SurfaceKind.Rock => new("Rock051", 2f, .65f, .25f, variation: .09f, grime: .08f, dust: 0),
            SurfaceKind.Gravel => new("Gravel023", 1.25f, .55f, .2f, variation: .08f, grime: .08f, dust: 0),
            SurfaceKind.Asphalt => new("Asphalt031", .85f, .2f, .3f, variation: .07f, grime: .08f, dust: .02f),
            SurfaceKind.Paving => new("Concrete034", 1.4f, .45f, .45f, variation: .06f, grime: .14f, dust: .05f),
            SurfaceKind.Terrazzo => new("Terrazzo018", .18f, .22f, .85f, variation: .025f, grime: .09f, dust: .1f),
            SurfaceKind.FloorTile => new("Tiles107", 1.2f, .32f, .8f, antiTile: false, variation: .035f, grime: .1f, dust: .1f),
            SurfaceKind.GlazedTile => new("Tiles036", 1.2f, .3f, .95f, antiTile: false, variation: .025f, grime: .06f, dust: .03f),
            SurfaceKind.AcousticCeiling => new("OfficeCeiling001", 2.4f, .35f, .18f, antiTile: false, variation: .04f, grime: .09f, dust: 0),
            SurfaceKind.Carpet => new("Carpet016", .7f, .45f, .16f, variation: .055f, grime: .12f, dust: .18f),
            SurfaceKind.WoodFloor => new("WoodFloor051", 1.2f, .38f, .78f, antiTile: false, variation: .04f, grime: .1f, dust: .1f),
            SurfaceKind.Timber => new("WoodFloor051", .8f, .35f, .62f, antiTile: false, variation: .05f, grime: .1f, dust: .12f),
            SurfaceKind.Siding => new("WoodSiding009", 1.6f, .32f, .4f, antiTile: false, variation: .04f, grime: .1f, dust: .04f),
            SurfaceKind.PaintedWood => new("Plastic010", .5f, .15f, .85f, variation: .025f, grime: .035f, dust: .055f),
            SurfaceKind.PaintedMetal => new("Metal049A", .6f, .16f, .82f, variation: .025f, grime: .065f, dust: .08f),
            SurfaceKind.BareMetal => new("Metal049A", .65f, .28f, .86f, metal: true, variation: .04f, grime: .1f, dust: .1f),
            SurfaceKind.Rust => new("Metal063", .8f, .5f, .5f, metal: true, variation: .08f, grime: .17f, dust: .08f),
            SurfaceKind.Brass => new("Metal049A", .45f, .14f, .9f, metal: true, variation: .025f, grime: .06f, dust: .06f),
            SurfaceKind.Copper => new("Metal049A", .45f, .2f, .85f, metal: true, variation: .03f, grime: .09f, dust: .05f),
            SurfaceKind.Chrome => new("Metal049A", .35f, .08f, 1f, metal: true, variation: .015f, grime: .025f, dust: .025f),
            SurfaceKind.Fabric => new("Fabric030", .28f, .4f, .14f, variation: .04f, grime: .1f, dust: .16f),
            SurfaceKind.Canvas => new("Fabric030", .4f, .5f, .1f, variation: .045f, grime: .1f, dust: .16f),
            SurfaceKind.Leather => new("Leather037", .45f, .32f, .8f, variation: .045f, grime: .1f, dust: .08f),
            SurfaceKind.Paper => new("Paper001", .35f, .12f, .12f, variation: .015f, grime: .025f, dust: .04f),
            SurfaceKind.Cardboard => new("Cardboard001", .6f, .35f, .15f, variation: .04f, grime: .07f, dust: .08f),
            SurfaceKind.Rubber => new("Rubber004", .35f, .25f, .27f, variation: .035f, grime: .1f, dust: .1f),
            SurfaceKind.Plastic => new("Plastic010", .4f, .18f, .7f, variation: .025f, grime: .06f, dust: .07f),
            SurfaceKind.Cork => new("Chipboard004", .35f, .32f, .16f, variation: .04f, grime: .08f, dust: .1f),
            SurfaceKind.Shingle => new("Asphalt031", .4f, .3f, .18f, variation: .06f, grime: .05f, dust: .02f),
            SurfaceKind.CastConcrete => new("Concrete046", 1.5f, .32f, .7f, variation: .06f, grime: .12f, dust: .1f),
            SurfaceKind.Brick => new("Bricks106A", 1.6f, .55f, .3f, antiTile: false, variation: .04f, grime: .13f, dust: .04f),
            SurfaceKind.PaintedBlock => new("PaintedBricks004", 1.55f, .45f, .35f, antiTile: false, variation: .035f, grime: .1f, dust: .04f),
            SurfaceKind.Stone => new("Rock051", 2f, .45f, .3f, variation: .05f, grime: .1f, dust: .04f),
            SurfaceKind.CorrugatedSteel => new("CorrugatedSteel009", 2.5f, .5f, .75f, metal: true, antiTile: false),
            SurfaceKind.TreadPlate => new("DiamondPlate008A", .85f, .45f, .75f, metal: true, antiTile: false),
            SurfaceKind.Grating => new("MetalWalkway014", 1.2f, .4f, .7f, metal: true, antiTile: false),
            SurfaceKind.Plaster => new("PaintedPlaster017", 1.5f, .28f, .25f, variation: .035f, grime: .1f, dust: .04f),
            _ => new("Concrete034", 1.6f, .45f, .5f, variation: .06f, grime: .16f, dust: .12f)
        };

        public static float MetresFor(string materialName) => For(Classify(materialName)).Metres;
    }
}
