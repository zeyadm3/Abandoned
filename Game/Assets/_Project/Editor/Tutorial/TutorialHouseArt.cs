using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Materials for the training house. The winter set (snow, frosted glass, brick, plaster, the snowflake) is the
    /// school's PBR kit, shared rather than copied; the house's own finishes (siding, shingles, carpet, oak, laminate,
    /// porcelain) use the original Larch Court PBR kit. The school's shared finishes remain unchanged.
    /// </summary>
    public sealed class TutorialHouseArt
    {
        public readonly BurleySchoolArt School;

        public readonly Material Siding, SidingDark, Trim, Shingles, Brick, Plaster, Ceiling, Concrete, BasementWall;
        public readonly Material Carpet, Oak, OakDark, Tile, Linoleum, Laminate, Cabinet, Porcelain, Chrome, Mirror;
        public readonly Material SofaFabric, Bedding, BeddingB, Mattress, Pillow, Rug, Curtain, Blind, Cardboard, Tape;
        public readonly Material Glass, Snow, SnowDrift, Ice, Asphalt, Sidewalk, Bark, Branch, Pine, FenceWood, DeadGrass;
        public readonly Material Steel, Rust, DarkMetal, Rubber, Paint, Brass, BulbGlow, Ember, TreeGreen, OrnamentRed, OrnamentGold;
        public readonly Material VanBody, VanTrim, Window;
        public readonly Material WeatherParticle;
        /// <summary>The street copies' shut front doors (repainted per house from <see cref="StreetDoors"/>).</summary>
        public readonly Material FrontDoor;
        /// <summary>
        /// The street's paint chart: each copy of the house picks a siding (with its own darker lap shadow, same
        /// index), a roof, a front door and a trim. Index 0 of each is the training house's own.
        /// </summary>
        public readonly Material[] StreetSidings, StreetSidingShadows, StreetShingles, StreetDoors, StreetTrims;
        public readonly Dictionary<Material, Vector2> Tiling = new();

        public TutorialHouseArt()
        {
            School = new BurleySchoolArt();
            Snow = School.Snow;
            SnowDrift = School.SnowDrift;
            Ice = School.FrostIce;
            Brick = School.Brick;
            Plaster = M("InteriorPlaster", new Color(.79f, .77f, .70f), .94f);
            Glass = M("WindowGlass", new Color(.70f, .81f, .86f, .18f), .18f);
            Concrete = School.Concrete;
            Asphalt = School.SnowyAsphalt;
            Sidewalk = School.Sidewalk;
            Pine = School.PineFoliage;
            Steel = School.MetalSteel;
            Rust = School.Rust;
            WeatherParticle = School.WeatherParticle;

            Siding = M("Siding", new Color(.71f, .74f, .72f), .82f);
            SidingDark = M("SidingShadow", new Color(.52f, .55f, .54f), .85f);
            Trim = M("Trim", new Color(.86f, .85f, .80f), .6f);
            Shingles = M("Shingles", new Color(.19f, .19f, .20f), .95f);
            Ceiling = M("CeilingPopcorn", new Color(.84f, .83f, .79f), .95f);
            BasementWall = M("BasementBlock", new Color(.55f, .55f, .52f), .92f);
            Carpet = M("CarpetBeige", new Color(.55f, .49f, .40f), .98f);
            Oak = M("OakFloor", new Color(.52f, .36f, .21f), .55f);
            OakDark = M("OakDark", new Color(.33f, .22f, .13f), .6f);
            Tile = M("BathTile", new Color(.82f, .83f, .80f), .3f);
            Linoleum = M("KitchenLino", new Color(.69f, .64f, .52f), .55f);
            Laminate = M("Countertop", new Color(.78f, .74f, .64f), .35f);
            Cabinet = M("CabinetOak", new Color(.47f, .32f, .18f), .6f);
            Porcelain = M("Porcelain", new Color(.93f, .93f, .90f), .2f);
            Chrome = M("Chrome", new Color(.70f, .72f, .74f), .15f);
            Mirror = M("Mirror", new Color(.55f, .60f, .63f), .05f);
            SofaFabric = M("SofaFabric", new Color(.29f, .35f, .28f), .97f);
            Bedding = M("BeddingFaded", new Color(.42f, .49f, .58f), .97f);
            BeddingB = M("BeddingFloral", new Color(.62f, .45f, .44f), .97f);
            Mattress = M("Mattress", new Color(.80f, .78f, .71f), .97f);
            Pillow = M("Pillow", new Color(.88f, .87f, .82f), .97f);
            Rug = M("RugRed", new Color(.45f, .14f, .12f), .98f);
            Curtain = M("CurtainMustard", new Color(.60f, .48f, .20f), .97f);
            Blind = M("BlindSlats", new Color(.85f, .83f, .75f), .7f);
            Cardboard = M("Cardboard", new Color(.60f, .46f, .29f), .95f);
            Tape = M("PackingTape", new Color(.72f, .63f, .40f), .4f);
            Bark = M("WinterBark", new Color(.20f, .17f, .14f), .97f);
            Branch = M("WinterBranch", new Color(.24f, .21f, .18f), .97f);
            FenceWood = M("FenceWeathered", new Color(.40f, .35f, .29f), .95f);
            DeadGrass = M("DeadGrass", new Color(.43f, .40f, .30f), .98f);
            DarkMetal = M("DarkMetal", new Color(.14f, .15f, .16f), .45f);
            Rubber = M("Rubber", new Color(.06f, .06f, .06f), .9f);
            Paint = M("FurnaceEnamel", new Color(.70f, .66f, .55f), .55f);
            Brass = M("Brass", new Color(.72f, .56f, .26f), .35f);
            BulbGlow = PolishAssets.Material("House_BulbGlow", new Color(1f, .82f, .56f), .3f, emission: 2.6f);
            Ember = PolishAssets.Material("House_PilotFlame", new Color(.35f, .55f, 1f), .3f, emission: 3f);
            TreeGreen = M("ChristmasTree", new Color(.12f, .25f, .14f), .95f);
            OrnamentRed = M("OrnamentRed", new Color(.62f, .07f, .07f), .25f);
            OrnamentGold = M("OrnamentGold", new Color(.78f, .62f, .22f), .25f);
            VanBody = M("VanWhite", new Color(.80f, .80f, .78f), .5f);
            VanTrim = M("VanTrim", new Color(.16f, .17f, .18f), .6f);
            Window = M("DarkWindow", new Color(.08f, .10f, .12f), .1f);
            FrontDoor = M("FrontDoor", new Color(.45f, .10f, .09f), .5f);

            (string name, Color colour)[] sidings =
            {
                ("Grey", new Color(.71f, .74f, .72f)), ("Blue", new Color(.48f, .57f, .64f)), ("Cream", new Color(.78f, .74f, .60f)),
                ("Green", new Color(.47f, .54f, .45f)), ("Tan", new Color(.62f, .56f, .47f)), ("White", new Color(.86f, .86f, .82f)),
                ("Slate", new Color(.40f, .43f, .47f)), ("Yellow", new Color(.80f, .72f, .48f)),
            };
            StreetSidings = new Material[sidings.Length];
            StreetSidingShadows = new Material[sidings.Length];
            for (int i = 0; i < sidings.Length; i++)
            {
                Color c = sidings[i].colour;
                StreetSidings[i] = i == 0 ? Siding : M("StreetSiding" + sidings[i].name, c, .82f);
                StreetSidingShadows[i] = i == 0 ? SidingDark : M("StreetSidingShadow" + sidings[i].name, new Color(c.r * .72f, c.g * .72f, c.b * .72f), .85f);
            }
            StreetShingles = new[]
            {
                Shingles, M("ShinglesBrown", new Color(.30f, .22f, .16f), .95f), M("ShinglesWeathered", new Color(.36f, .36f, .35f), .95f),
                M("ShinglesGreen", new Color(.20f, .26f, .21f), .95f),
            };
            StreetDoors = new[]
            {
                FrontDoor, M("FrontDoorGreen", new Color(.12f, .26f, .18f), .5f), M("FrontDoorNavy", new Color(.12f, .17f, .30f), .5f),
                M("FrontDoorOak", new Color(.40f, .26f, .14f), .55f), M("FrontDoorWhite", new Color(.88f, .87f, .83f), .5f),
                M("FrontDoorBlack", new Color(.08f, .08f, .09f), .45f),
            };
            StreetTrims = new[] { Trim, M("TrimCream", new Color(.85f, .80f, .66f), .6f), M("TrimBrown", new Color(.30f, .22f, .15f), .6f) };

            // Metre-scale repeats for the school's textured surfaces (Surfaces.Apply re-UVs the cubes that use them).
            foreach (KeyValuePair<Material, Vector2> pair in School.Tiling) Tiling[pair.Key] = pair.Value;
            TutorialHouseMaterials.Apply(this);
        }

        private static Material M(string name, Color color, float roughness) => PolishAssets.Material("House_" + name, color, roughness);
    }
}
