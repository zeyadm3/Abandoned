using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.Remake.ArchProfiles;
using Surface = Abandoned.EditorTools.Remake.ArchStyle.Surface;
using Finish = Abandoned.EditorTools.Remake.ArchStyle.Finish;
using Trim = Abandoned.EditorTools.Remake.ArchStyle.Trim;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// One <see cref="ArchStyle"/> per map (scene name), from the art bible's palettes (Docs/REMAKE_2026.md). Trim
    /// finishes are copies of each map's own surfaces, tinted, so they carry its scans, grime and weathering.
    /// </summary>
    public static class ArchStyles
    {
        private static Dictionary<string, ArchStyle> all;

        public static ArchStyle For(string scene)
        {
            all ??= Build();
            return all.TryGetValue(scene, out ArchStyle style) ? style : null;
        }

        private static readonly Color Galv = new(.86f, .87f, .88f), Yellow = new(1f, .74f, .12f), Oxide = new(.55f, .2f, .14f),
            Dark = new(.18f, .18f, .19f), Cream = new(.93f, .9f, .82f), White = new(.95f, .95f, .94f), Mint = new(.7f, .86f, .78f),
            PaleGreen = new(.66f, .8f, .68f), Navy = new(.2f, .25f, .38f), Red = new(.72f, .1f, .08f), Brass = new(.85f, .68f, .38f);

        private static Dictionary<string, ArchStyle> Build()
        {
            var d = new Dictionary<string, ArchStyle>();
            void Add(ArchStyle s) => d[s.Map] = s;

            // HQ: oxide red steel, galvanised grey, yellow safety paint, tired fluorescent white.
            Add(new ArchStyle
            {
                Map = "HQ",
                Finishes = new[]
                {
                    new Finish("Metal", "HQ_Architecture_Steel", Galv, .45f, .6f), new Finish("Safety", "HQ_Architecture_Steel", Yellow, .35f),
                    new Finish("Oxide", "HQ_Architecture_Steel", Oxide, .3f, .3f), new Finish("Soffit", "HQ_Architecture_Concrete", Color.white),
                    new Finish("Stone", "HQ_Architecture_Concrete", new Color(.8f, .8f, .78f)),
                    new Finish("Sheet", "HQ_Architecture_Steel", new Color(.46f, .49f, .5f), .38f, .55f, paint: true),
                },
                Bodies = { [Surface.Ribbed] = "Sheet" },
                Skirting = new Trim("Metal", KickPlate(.15f)),
                Casing = new Trim("Metal", SteelJamb(.06f, .012f)),
                Coping = new Trim("Metal"),
                Nosing = new Trim("Safety"),
                Plinth = new Trim("Stone", Plinth(.35f, .03f)),
                Columns = ArchStyle.ColumnKind.SteelH, ColumnTrim = "Oxide", FrameMaterial = "Metal",
            });

            // Mall: terrazzo cream, brushed steel, faded brand colours, dusty glass.
            Add(new ArchStyle
            {
                Map = "Mall",
                Finishes = new[]
                {
                    new Finish("Metal", "NeighbourhoodSteel", Galv, .55f, .7f), new Finish("Stone", "AgedLimestone", Cream),
                    new Finish("Plaster", "FadedIvory", Color.white), new Finish("Soffit", "FadedIvory", new Color(.9f, .89f, .86f)),
                },
                Skirting = new Trim("Metal", KickPlate(.1f)),
                Cornice = new Trim("Plaster", Cove(.09f)),
                Casing = new Trim("Metal", SteelJamb(.05f, .01f)),
                Coping = new Trim("Stone"),
                Nosing = new Trim("Metal"),
                Plinth = new Trim("Stone", Plinth(.45f, .035f)),
                StringCourse = new Trim("Stone", Band(.22f, .05f)), StoreyBands = 4f,
                Columns = ArchStyle.ColumnKind.Clad, ColumnTrim = "Metal",
            });

            // School: pale green tile, varnished oak, chalk dust, winter light.
            Add(new ArchStyle
            {
                Map = "School",
                Finishes = new[]
                {
                    new Finish("Wood", "Burley_OakWood", Color.white, .45f), new Finish("Metal", "Burley_MetalSteel", Galv, .4f, .5f),
                    new Finish("Tile", "Burley_FloorTile", PaleGreen, .6f), new Finish("Stone", "Burley_Limestone", Color.white),
                    new Finish("Rubber", "Burley_FloorTile", Dark, .15f), new Finish("Soffit", "Burley_Concrete", Color.white),
                },
                Skirting = new Trim("Rubber", RubberCove(.1f)),
                Casing = new Trim("Wood", Architrave(.075f, .02f)),
                ChairRail = new Trim("Wood", Rail(.06f, .022f)), ChairRailHeight = 1.25f,
                Wainscot = 1.2f, WainscotMaterial = "Tile", WainscotCap = new Trim("Tile", Rail(.03f, .014f)),
                Coping = new Trim("Stone"), Nosing = new Trim("Metal"), Plinth = new Trim("Stone", Plinth(.5f, .04f)),
                StringCourse = new Trim("Stone", Band(.2f, .045f)), StoreyBands = 4f,
                Ceiling = ArchStyle.CeilingKind.Grid, GridMaterial = "Metal",
                Rules = new[] { (ArchStyle.R("PlasterWall"), Surface.Flat) },
            });

            // Iron Factory: soot black, rust orange, molten amber, cold steel blue.
            Add(new ArchStyle
            {
                Map = "Factory",
                Finishes = new[]
                {
                    new Finish("Metal", "IronFactory_Steel", new Color(.72f, .76f, .82f), .35f, .6f),
                    new Finish("Safety", "IronFactory_Steel", Yellow, .3f), new Finish("Stone", "IronFactory_Concrete", Color.white),
                    new Finish("Soffit", "IronFactory_Concrete", new Color(.7f, .7f, .7f)),
                    new Finish("Sheet", "IronFactory_Steel", new Color(.3f, .31f, .32f), .3f, .5f, paint: true),
                },
                Bodies = { [Surface.Ribbed] = "Sheet" },
                Skirting = new Trim("Stone", SquareBase(.2f, .03f)),
                Casing = new Trim("Metal", SteelJamb(.08f, .015f)),
                Coping = new Trim("Metal"), Nosing = new Trim("Safety"), Plinth = new Trim("Stone", Plinth(1.2f, .05f)),
                Columns = ArchStyle.ColumnKind.SteelH, ColumnTrim = "Metal", RibPitch = .3f, RibDepth = .045f,
                Rules = new[]
                {
                    (ArchStyle.R("IronFactory_Wall$"), Surface.Ribbed), (ArchStyle.R("RoomWall"), Surface.Masonry),
                    (ArchStyle.R("Lining|Guard"), Surface.Skip),
                },
            });

            // Island Prison: salt-bleached concrete, flaking institutional green, iron bars, sea grey.
            Add(new ArchStyle
            {
                Map = "Prison",
                Finishes = new[]
                {
                    new Finish("Metal", "Island_Steel", new Color(.62f, .65f, .66f), .3f, .5f), new Finish("Green", "Island_Dado", Color.white, .35f),
                    new Finish("Stone", "Island_Facade", new Color(.88f, .88f, .85f)), new Finish("Wood", "Island_DarkWood", Color.white),
                    new Finish("Soffit", "Island_Plaster", new Color(.9f, .9f, .88f)),
                },
                Skirting = new Trim("Green", SquareBase(.15f, .02f)),
                Casing = new Trim("Metal", SteelJamb(.07f, .014f)),
                ChairRail = new Trim("Metal", Rail(.04f, .016f)), ChairRailHeight = 1.2f,
                Coping = new Trim("Stone"), Nosing = new Trim("Metal"), Plinth = new Trim("Stone", Plinth(.6f, .05f)),
                StringCourse = new Trim("Stone", Band(.3f, .06f)), StoreyBands = 4f,
                Rules = new[] { (ArchStyle.R("Island_Facade|Island_Pier"), Surface.Concrete), (ArchStyle.R("Island_Board"), Surface.Floor) },
            });

            // Tutorial House: 70s ranch, wood panelling, beige carpet, Michigan winter.
            Add(new ArchStyle
            {
                Map = "Tutorial",
                Finishes = new[]
                {
                    new Finish("Wood", "House_OakDark", Color.white, .4f), new Finish("Plaster", "House_InteriorPlaster", Color.white),
                    new Finish("Stone", "Burley_Concrete", Color.white), new Finish("Metal", "House_DarkMetal", Color.white, .3f, .5f),
                    new Finish("Soffit", "House_CeilingPopcorn", Color.white),
                },
                Skirting = new Trim("Wood", Colonial(.09f)),
                Cornice = new Trim("Plaster", Cove(.06f)),
                Casing = new Trim("Wood", Architrave(.065f, .018f)),
                Coping = new Trim("Wood"), Nosing = new Trim("Wood"), Plinth = new Trim("Stone", Plinth(.3f, .025f)),
                BoardPitch = .2f,
                Rules = new[] { (ArchStyle.R("House_Trim|Fence"), Surface.Skip) },
            });

            // Hospital: clinical white, mint green, stainless steel, stained ceiling tiles.
            Add(Civic("Hospital", "Hospital_", Mint, ArchStyle.CeilingKind.Grid, stainless: true));
            // Motel: sun-bleached teal and coral, nicotine yellow, neon.
            Add(new ArchStyle
            {
                Map = "Motel",
                Finishes = new[]
                {
                    new Finish("Wood", "Wood", new Color(.8f, .74f, .62f), .35f), new Finish("Metal", "Steel", Galv, .4f, .5f),
                    new Finish("Plaster", "Plaster", new Color(.95f, .88f, .7f)), new Finish("Stone", "MotelTown_TownOldConcrete", Color.white),
                    new Finish("Soffit", "Plaster", new Color(.92f, .87f, .72f)), new Finish("Teal", "Plaster", new Color(.35f, .62f, .6f)),
                },
                Skirting = new Trim("Teal", SquareBase(.1f, .015f)),
                Cornice = new Trim("Plaster", ShadowBead(.03f)),
                Casing = new Trim("Wood", Architrave(.06f, .016f)),
                Coping = new Trim("Stone"), Nosing = new Trim("Metal"), Plinth = new Trim("Stone", Plinth(.3f, .03f)),
                StringCourse = new Trim("Teal", Band(.18f, .03f)), StoreyBands = 3f,
            });
            // Skyscraper: glass, black granite, brushed brass, smoked mirror.
            Add(new ArchStyle
            {
                Map = "Skyscraper",
                Finishes = new[]
                {
                    new Finish("Metal", "Lift_BrushedChampagne", Color.white, .6f, .8f), new Finish("Brass", "Brass_Trim", Color.white, .55f, .8f),
                    new Finish("Stone", "Granite_DarkColonnade", Color.white, .55f), new Finish("Marble", "Marble_Carrara", Color.white, .6f),
                    new Finish("Wood", "Lobby_Walnut", Color.white, .5f), new Finish("Soffit", "Ceiling_Office", Color.white),
                },
                Skirting = new Trim("Stone", SquareBase(.1f, .012f)),
                Cornice = new Trim("Metal", ShadowBead(.025f)),
                Casing = new Trim("Brass", SteelJamb(.04f, .01f)),
                Coping = new Trim("Metal"), Nosing = new Trim("Brass"), Plinth = new Trim("Stone", Plinth(.6f, .03f)),
                StringCourse = new Trim("Metal", Band(.12f, .03f)), StoreyBands = 4f,
                Ceiling = ArchStyle.CeilingKind.Grid, GridMaterial = "Metal",
                Columns = ArchStyle.ColumnKind.Clad, ColumnTrim = "Brass", StonePanel = new Vector2(1.5f, .75f),
            });
            Add(Civic("PoliceStation", "Police_", Navy, ArchStyle.CeilingKind.Grid, stainless: false));
            Add(Civic("FireStation", "Fire_", Red, ArchStyle.CeilingKind.Grid, stainless: false));
            return d;
        }

        /// <summary>The three newer civic buildings share a build: block and brick, tiled wet rooms, suspended ceilings, a
        /// rubber cove base and a crash rail in the building's colour.</summary>
        private static ArchStyle Civic(string map, string prefix, Color accent, ArchStyle.CeilingKind ceiling, bool stainless) => new()
        {
            Map = map,
            Finishes = new[]
            {
                new Finish("Metal", stainless ? "Steel" : prefix + "Steel", Galv, stainless ? .6f : .4f, .7f),
                new Finish("Wood", prefix == "Hospital_" ? "HospitalTown_Board" : prefix + "DarkWood", Color.white, .4f),
                new Finish("Tile", prefix + "WallTile", Color.white, .6f), new Finish("Stone", prefix + "Concrete", Color.white),
                new Finish("Rubber", prefix + "Vinyl", Dark, .2f), new Finish("Soffit", prefix + "Ceiling", Color.white),
                new Finish("Accent", prefix + "Vinyl", accent, .35f),
            },
            Skirting = new Trim("Rubber", RubberCove(.1f)),
            Casing = new Trim(stainless ? "Metal" : "Wood", stainless ? SteelJamb(.05f, .012f) : Architrave(.07f, .018f)),
            ChairRail = new Trim(stainless ? "Metal" : "Accent", Rail(.12f, .03f)), ChairRailHeight = .95f,
            Coping = new Trim("Stone"), Nosing = new Trim("Metal"), Plinth = new Trim("Stone", Plinth(.45f, .035f)),
            StringCourse = new Trim("Stone", Band(.25f, .05f)), StoreyBands = 4f,
            Ceiling = ceiling, GridMaterial = "Metal",
        };
    }
}
