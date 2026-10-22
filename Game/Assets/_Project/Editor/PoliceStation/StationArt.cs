using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Kit = Abandoned.EditorTools.IslandPrisonMaterials;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A station's materials: grounded and realistic like the mall and the island. Brick outside, institutional paint
    /// and tile inside, steel and glass, built from the mall's PBR maps and the factory's code-made maps (read only) and
    /// saved as the station's own materials (<c>{ArtFolder}/Materials</c>), so no other level changes. Every tiled
    /// material records its real-world repeat, so <see cref="StationSurfaces"/> keeps textures at true size.
    /// </summary>
    public sealed class StationArt
    {
        public readonly string Folder, Prefix;
        private readonly string materialFolder;
        public readonly Dictionary<Material, Vector2> Tiling = new();

        // Shell and rooms.
        public readonly Material Brick, BrickDark, Trim, Roof, Plaster, Dado, Block, WallTile, Panel, Concrete, Ceiling;
        // Floors.
        public readonly Material Terrazzo, Vinyl, Carpet, FloorConcrete, Epoxy;
        // Metal, wood and fittings.
        public readonly Material Steel, DarkSteel, Bars, Chrome, Brass, Grating, Safety, Wood, DarkWood, Board;
        public readonly Material Glass, Fabric, Mattress, Paper, Rubber, Porcelain, Accent, AccentDark, White, Red, Leather;
        // Light.
        public readonly Material Lamp, LampOff, ExitGreen, WarningRed;
        // Outside.
        public readonly Material Asphalt, Sidewalk, Grass, Dirt, Gravel, Kerb, Paint, Oil;
        // Stains and wear.
        public readonly Material Grime, Wet, Soot;

        /// <param name="folder">The station's art folder (Art/PoliceStation, Art/FireStation).</param>
        /// <param name="brick">Facade brick tint.</param>
        /// <param name="accent">The station's paint colour (police blue, fire red).</param>
        /// <param name="lamp">Ceiling-light colour.</param>
        public StationArt(string folder, string prefix, Color brick, Color accent, Color lamp)
        {
            Folder = folder;
            Prefix = prefix;
            materialFolder = folder + "/Materials";
            PolishAssets.EnsureFolder(materialFolder);
            IronFactoryTextures.EnsureAll();

            Brick = T(Kit.Textured(N("Brick"), "FireBrick", brick, Kit.Surface.Opaque, materialFolder), 2.4f);
            BrickDark = T(Kit.Textured(N("BrickDark"), "SootBrick", Color.Lerp(brick, Color.white, .35f), Kit.Surface.Opaque, materialFolder), 2.4f);
            Trim = T(Kit.FromMall(N("Trim"), "FacadeStone", new Color(.86f, .84f, .78f), materialFolder), 2f);
            Roof = T(Kit.FromMall(N("Roof"), "Concrete", new Color(.42f, .42f, .4f), materialFolder), 4f);
            Plaster = T(Kit.FromMall(N("Plaster"), "Plaster", new Color(.86f, .87f, .82f), materialFolder), 2.5f);
            Dado = T(Kit.FromMall(N("Dado"), "Plaster", Color.Lerp(accent, new Color(.6f, .6f, .58f), .55f), materialFolder), 2.5f);
            Block = T(Kit.FromMall(N("Block"), "Block", new Color(.82f, .82f, .78f), materialFolder), 2.4f);
            WallTile = T(Kit.FromMall(N("WallTile"), "Tile", new Color(.85f, .88f, .86f), materialFolder), 1.2f);
            Panel = T(Kit.FromMall(N("Panel"), "Wood", new Color(.62f, .48f, .34f), materialFolder), 2f);
            Concrete = T(Kit.FromMall(N("Concrete"), "Concrete", new Color(.62f, .61f, .58f), materialFolder), 3f);
            Ceiling = T(Kit.FromMall(N("Ceiling"), "Ceiling", new Color(.86f, .86f, .82f), materialFolder), 1.2f);

            Terrazzo = T(Kit.FromMall(N("Terrazzo"), "Terrazzo", new Color(.84f, .82f, .76f), materialFolder), 3f);
            Vinyl = T(Kit.FromMall(N("Vinyl"), "Tile", new Color(.72f, .73f, .68f), materialFolder), 1.2f);
            Carpet = T(Kit.FromMall(N("Carpet"), "Carpet", Color.Lerp(accent, new Color(.35f, .34f, .32f), .6f), materialFolder), 2f);
            FloorConcrete = T(Kit.FromMall(N("FloorConcrete"), "Concrete", new Color(.55f, .55f, .52f), materialFolder), 4f);
            Epoxy = T(Kit.FromMall(N("Epoxy"), "Concrete", new Color(.62f, .58f, .5f), materialFolder), 4f);

            Steel = T(Kit.Textured(N("Steel"), "RivetedPlate", new Color(.8f, .82f, .82f), Kit.Surface.Opaque, materialFolder), 2f);
            DarkSteel = T(Kit.FromMall(N("DarkSteel"), "BridgeMetal", new Color(.32f, .33f, .34f), materialFolder), 1.5f);
            Bars = T(Kit.FromMall(N("Bars"), "Rust", new Color(.62f, .6f, .58f), materialFolder), 1f);
            Grating = T(Kit.Textured(N("Grating"), "Grating", Color.white, Kit.Surface.Opaque, materialFolder), 1f);
            Safety = T(Kit.Textured(N("Safety"), "WornPaint", new Color(.92f, .72f, .16f), Kit.Surface.Opaque, materialFolder), 1f);
            Wood = T(Kit.FromMall(N("Wood"), "Wood", new Color(.78f, .66f, .52f), materialFolder), 2f);
            DarkWood = T(Kit.FromMall(N("DarkWood"), "Wood", new Color(.4f, .28f, .2f), materialFolder), 2f);
            Board = T(Kit.FromMall(N("Board"), "BoardWood", new Color(.66f, .6f, .52f), materialFolder), 2f);
            Glass = Kit.FromMall(N("Glass"), "Glass", new Color(.7f, .8f, .82f, .28f), materialFolder, Kit.Surface.Transparent);
            Fabric = T(Kit.FromMall(N("Fabric"), "Upholstery", new Color(.36f, .38f, .42f), materialFolder), 1f);
            Mattress = T(Kit.FromMall(N("Mattress"), "Upholstery", new Color(.6f, .58f, .5f), materialFolder), 1f);
            Paint = T(Kit.Textured(N("LinePaint"), "WornPaint", new Color(.95f, .93f, .86f), Kit.Surface.Opaque, materialFolder), 1f);

            Chrome = Plain("Chrome", new Color(.78f, .8f, .82f), .85f, .9f);
            Brass = Plain("Brass", new Color(.72f, .54f, .24f), .6f, .85f);
            Paper = Plain("Paper", new Color(.86f, .83f, .74f), .1f);
            Rubber = Plain("Rubber", new Color(.07f, .07f, .075f), .25f);
            Porcelain = Plain("Porcelain", new Color(.9f, .9f, .87f), .75f);
            Accent = Plain("Accent", accent, .45f, .2f);
            AccentDark = Plain("AccentDark", Color.Lerp(accent, Color.black, .5f), .35f, .2f);
            White = Plain("White", new Color(.88f, .88f, .86f), .4f);
            Red = Plain("Red", new Color(.62f, .07f, .05f), .55f, .2f);
            Leather = Plain("Leather", new Color(.24f, .15f, .09f), .45f);

            Lamp = Plain("Lamp", lamp, .5f, 0f, lamp * 1.6f);
            LampOff = Plain("LampOff", new Color(.3f, .3f, .29f), .4f);
            ExitGreen = Plain("ExitGreen", new Color(.1f, .8f, .3f), .5f, 0f, new Color(.2f, 2.2f, .7f));
            WarningRed = Plain("WarningRed", new Color(.8f, .1f, .08f), .5f, 0f, new Color(1.8f, .15f, .1f));

            Asphalt = T(Kit.FromMall(N("Asphalt"), "Asphalt", new Color(.74f, .74f, .74f), materialFolder), 6f);
            Sidewalk = T(Kit.FromMall(N("Sidewalk"), "Concrete", new Color(.7f, .69f, .65f), materialFolder), 2f);
            Kerb = T(Kit.FromMall(N("Kerb"), "Concrete", new Color(.62f, .61f, .58f), materialFolder), 1.5f);
            Grass = T(Kit.Textured(N("Grass"), "Turf", Color.white, Kit.Surface.Opaque, materialFolder), 2.5f);
            Dirt = T(Kit.Textured(N("Dirt"), "Earth", Color.white, Kit.Surface.Opaque, materialFolder), 3f);
            Gravel = T(Kit.Textured(N("Gravel"), "Cinder", new Color(.75f, .74f, .7f), Kit.Surface.Opaque, materialFolder), 2f);
            Oil = Kit.Textured(N("Oil"), "OilStain", Color.white, Kit.Surface.Transparent, materialFolder);

            Grime = Plain("Grime", new Color(.13f, .12f, .1f), .05f);
            Soot = Plain("Soot", new Color(.05f, .045f, .04f), .02f);
            Wet = Kit.FromMall(N("Wet"), "Water", new Color(.2f, .23f, .24f, .5f), materialFolder, Kit.Surface.Transparent);
            Wet.SetFloat("_Smoothness", .75f);
        }

        /// <summary>The wall material for a room's finish.</summary>
        public Material For(StationPlan.Finish finish) => finish switch
        {
            StationPlan.Finish.Block => Block,
            StationPlan.Finish.Tile => WallTile,
            StationPlan.Finish.Brick => BrickDark,
            StationPlan.Finish.Concrete => Concrete,
            StationPlan.Finish.Panel => Panel,
            _ => Plaster,
        };

        /// <summary>The floor finish under a room's finish (tiled rooms get tile, panelled ones carpet).</summary>
        public Material FloorFor(StationPlan.Finish finish, int floor) => finish switch
        {
            StationPlan.Finish.Tile => Vinyl,
            StationPlan.Finish.Panel => Carpet,
            StationPlan.Finish.Concrete => FloorConcrete,
            StationPlan.Finish.Brick => Epoxy,
            _ => floor < 0 ? FloorConcrete : Terrazzo,
        };

        private string N(string name) => Prefix + "_" + name;

        private Material T(Material m, float metres)
        {
            Tiling[m] = new Vector2(metres, metres);
            return m;
        }

        /// <summary>An untextured URP Lit material in the station's folder; re-running updates it in place.</summary>
        public Material Plain(string name, Color colour, float smoothness, float metallic = 0f, Color? emission = null)
        {
            string path = $"{materialFolder}/{N(name)}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (m == null)
            {
                m = new Material(shader) { name = N(name) };
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission.Value);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                m.DisableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.black);
            }
            m.SetFloat("_Surface", 0f);
            m.renderQueue = (int)RenderQueue.Geometry;
            m.enableInstancing = true;
            GeneratedMaterialRepair.Apply(m);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
