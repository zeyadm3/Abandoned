using System.Collections.Generic;
using System.IO;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Kit = Abandoned.EditorTools.IslandPrisonMaterials;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gallows Rock's materials: grounded and realistic like the mall. Weathered concrete and stucco outside, an
    /// institutional two-tone (pale green over a darker dado) inside, rusted bars, sea-worn rock, wet asphalt and the
    /// bay itself. Built from the mall's PBR kit and the factory's code-made maps, copied into the island's own
    /// materials (Art/Prison/Materials) so no other level changes. Every tiled material records its real-world
    /// repeat, so the world-UV pass (<see cref="IslandPrisonSurfaces"/>) keeps textures at true size.
    /// </summary>
    public sealed class IslandPrisonArt
    {
        public const string Folder = "Assets/_Project/Art/Prison";
        private const string MaterialFolder = Folder + "/Materials";
        private const string LighthouseConfigPath = "Assets/_Project/Data/Extraction/Resources/LighthouseConfig.asset";

        public readonly Dictionary<Material, Vector2> Tiling = new();

        // Shell and ground.
        public readonly Material Facade, FacadeDark, Plaster, Dado, Ceiling, Floor, Terrazzo, Tile, Block, Brick, BurntBrick;
        public readonly Material Rock, RockDark, Ground, Grass, Sand, Asphalt, Gravel, Pier, Sea, Foam;
        // Metal and wood.
        public readonly Material Bars, Steel, Plate, Rust, Wood, DarkWood, Board, Railing;
        // Fittings.
        public readonly Material Porcelain, Mattress, Blanket, Canvas, Paper, Books, Glass, Lamp, LampOff, Ember, Warning, Hull, HullTrim, Brass;
        // Decals (the mall's).
        public readonly Material Moss, Crack, Scorch, Leaves, Graffiti, Water, Stain, Ivy, LightShaft;

        public IslandPrisonArt()
        {
            IronFactoryTextures.EnsureAll();
            Facade = T(Kit.FromMall("Island_Facade", "Concrete", new Color(.94f, .91f, .83f), MaterialFolder), 3.5f);
            FacadeDark = T(Kit.FromMall("Island_FacadeDark", "Concrete", new Color(.67f, .65f, .59f), MaterialFolder), 3f);
            Plaster = T(Kit.FromMall("Island_Plaster", "Plaster", new Color(.85f, .89f, .8f), MaterialFolder), 2.5f);
            Dado = T(Kit.FromMall("Island_Dado", "Plaster", new Color(.43f, .53f, .46f), MaterialFolder), 2.5f);
            Ceiling = T(Kit.FromMall("Island_Ceiling", "Concrete", new Color(.8f, .81f, .76f), MaterialFolder), 3f);
            Floor = T(Kit.FromMall("Island_Floor", "Concrete", new Color(.77f, .76f, .7f), MaterialFolder), 4f);
            Terrazzo = T(Kit.FromMall("Island_Terrazzo", "Terrazzo", new Color(.88f, .86f, .8f), MaterialFolder), 3f);
            Tile = T(Kit.FromMall("Island_Tile", "Tile", new Color(.82f, .84f, .8f), MaterialFolder), 1.2f);
            Block = T(Kit.FromMall("Island_Block", "Block", new Color(.7f, .7f, .66f), MaterialFolder), 2.5f);
            Brick = T(Kit.Textured("Island_Brick", "FireBrick", new Color(.95f, .78f, .7f), Kit.Surface.Opaque, MaterialFolder), 3f);
            BurntBrick = T(Kit.Textured("Island_BurntBrick", "SootBrick", new Color(.9f, .85f, .82f), Kit.Surface.Opaque, MaterialFolder), 3f);
            // Ground made for the island (IronFactoryTextures.Island): never the mall's cut-stone facade or leaf decals.
            Rock = T(Kit.Textured("Island_Rock", "SeaRock", Color.white, Kit.Surface.Opaque, MaterialFolder), 7f);
            RockDark = T(Kit.Textured("Island_RockDark", "SeaRock", new Color(.62f, .62f, .64f), Kit.Surface.Opaque, MaterialFolder), 5f);
            Ground = T(Kit.Textured("Island_Ground", "Earth", Color.white, Kit.Surface.Opaque, MaterialFolder), 3f);
            Grass = T(Kit.Textured("Island_Grass", "Turf", Color.white, Kit.Surface.Opaque, MaterialFolder), 2.5f);
            Sand = T(Kit.Textured("Island_Shingle", "Shingle", Color.white, Kit.Surface.Opaque, MaterialFolder), 1.5f);
            Asphalt = T(Kit.FromMall("Island_Asphalt", "Asphalt", new Color(.78f, .78f, .78f), MaterialFolder), 6f);
            Gravel = T(Kit.FromMall("Island_Gravel", "Concrete", new Color(.42f, .41f, .38f), MaterialFolder), 2f);
            Pier = T(Kit.FromMall("Island_Pier", "BoardWood", new Color(.55f, .5f, .44f), MaterialFolder), 2f);
            // Bay water at noon: a grey-green blue under a pale sky.
            Sea = T(Kit.FromMall("Island_Sea", "Water", new Color(.2f, .3f, .34f), MaterialFolder), 12f);
            Sea.SetFloat("_BumpScale", .45f);
            Sea.SetFloat("_Smoothness", .85f);
            Foam = Translucent(GetMaterial("Island_Foam", new Color(.7f, .74f, .74f, .32f)));
            Foam.SetTexture("_BaseMap", PolishAssets.Texture("Island_FoamFade", stain: true));

            Bars = T(Kit.FromMall("Island_Bars", "Rust", new Color(.68f, .63f, .58f), MaterialFolder), 1f);
            Steel = T(Kit.Textured("Island_Steel", "RivetedPlate", new Color(.92f, .95f, .94f), Kit.Surface.Opaque, MaterialFolder), 2f);
            Plate = T(Kit.Textured("Island_GalleryPlate", "RivetedPlate", new Color(.8f, .86f, .83f), Kit.Surface.Opaque, MaterialFolder), 1.5f);
            Rust = T(Kit.FromMall("Island_Rust", "Rust", Color.white, MaterialFolder), 2f);
            Wood = T(Kit.FromMall("Island_Wood", "Wood", new Color(.8f, .7f, .58f), MaterialFolder), 2f);
            DarkWood = T(Kit.FromMall("Island_DarkWood", "Wood", new Color(.42f, .3f, .22f), MaterialFolder), 2f);
            Board = T(Kit.FromMall("Island_Board", "BoardWood", new Color(.7f, .66f, .6f), MaterialFolder), 2f);
            Railing = T(Kit.Textured("Island_Railing", "WornPaint", new Color(.55f, .6f, .56f), Kit.Surface.Opaque, MaterialFolder), 1f);

            Porcelain = Grain("Island_Porcelain", new Color(.89f, .89f, .84f), .22f);
            Mattress = T(Kit.FromMall("Island_Mattress", "Upholstery", new Color(.56f, .52f, .44f), MaterialFolder), 1f);
            Blanket = T(Kit.FromMall("Island_Blanket", "Upholstery", new Color(.4f, .46f, .48f), MaterialFolder), .6f);
            Canvas = T(Kit.FromMall("Island_Canvas", "Upholstery", new Color(.78f, .74f, .61f), MaterialFolder), .6f);
            Paper = Grain("Island_Paper", new Color(.82f, .78f, .68f), .9f);
            Books = Grain("Island_Books", new Color(.4f, .25f, .19f), .7f);
            Glass = Kit.FromMall("Island_Glass", "Glass", new Color(.8f, .89f, .88f, .22f), MaterialFolder, Kit.Surface.Transparent);
            Brass = Grain("Island_Brass", new Color(.68f, .5f, .22f), .3f);
            Brass.SetFloat("_Metallic", .8f);
            Hull = Grain("Island_Hull", new Color(.22f, .29f, .31f), .55f);
            Hull.SetFloat("_Metallic", .35f);
            HullTrim = Grain("Island_HullTrim", new Color(.67f, .18f, .12f), .52f);

            Lamp = GetMaterial("Island_Lamp", new Color(1f, .93f, .78f));
            Lamp.SetColor("_EmissionColor", new Color(1.6f, 1.4f, 1f));
            Lamp.EnableKeyword("_EMISSION");
            LampOff = GetMaterial("Island_LampOff", new Color(.3f, .3f, .28f));
            Ember = GetMaterial("Island_Ember", new Color(1f, .45f, .15f));
            Ember.SetColor("_EmissionColor", new Color(2.4f, .8f, .2f));
            Ember.EnableKeyword("_EMISSION");
            Warning = GetMaterial("Island_Warning", new Color(.8f, .62f, .12f));

            // Stains and leftovers are the island's own soft-edged patches (IslandKit.Patch), never the mall's alpha
            // decals: on a box those render as hard rectangles.
            Moss = Grain("Island_Moss", new Color(.1f, .14f, .07f), .9f);
            Crack = Grain("Island_Grime", new Color(.12f, .11f, .1f), .9f);
            Scorch = Grain("Island_Soot", new Color(.04f, .035f, .03f), .95f);
            Leaves = Grain("Island_LeafLitter", new Color(.24f, .18f, .1f), .95f);
            Graffiti = Crack;
            // Wet patches let the actual floor show through instead of covering it with a blank reflective disc.
            Water = Kit.FromMall("Island_Wet", "Water", new Color(.19f, .23f, .24f, .5f), MaterialFolder, Kit.Surface.Transparent);
            Water.SetFloat("_BumpScale", .25f);
            Water.SetFloat("_Smoothness", .7f);
            Stain = Crack;
            Ivy = Grain("Island_Ivy", new Color(.08f, .13f, .06f), .85f);
            LightShaft = Kit.Textured("Island_LightShaft", "LightShaft", Color.white, Kit.Surface.Transparent, MaterialFolder);
            GeneratedMaterialRepair.Apply(Foam);
            IslandMeshKit.ConfigureSurfaces(Tiling);
            EnsureLighthouseConfig();
        }

        private Material Grain(string name, Color colour, float roughness) =>
            T(PolishAssets.Material(name, colour, roughness, PolishAssets.Texture("Island_SurfaceGrain")), .5f);

        private Material T(Material m, float metres)
        {
            Tiling[m] = new Vector2(metres, metres);
            return m;
        }

        // URP Lit, transparent alpha blend: foam you see the water through.
        private static Material Translucent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_Smoothness", .2f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void EnsureLighthouseConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<LighthouseConfig>(LighthouseConfigPath) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(LighthouseConfigPath));
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LighthouseConfig>(), LighthouseConfigPath);
        }
    }
}
