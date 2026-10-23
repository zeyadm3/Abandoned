using UnityEditor;
using UnityEngine;
using Kit = Abandoned.EditorTools.IslandPrisonMaterials;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood's materials, saved in the station's own material folder: photographed brick, corrugated iron and
    /// weathered concrete (the Poly Haven scans in Art/TestMap/Ground, read only), the code-made brick, clapboard siding,
    /// stucco and roofing, and plain paints for window frames, doors, awnings and shop signs. Every tiled material is
    /// registered with <see cref="StationArt.Tiling"/>, so <see cref="StationSurfaces"/> keeps it at true size.
    /// </summary>
    public sealed class StationTownArt
    {
        public readonly Material[] Bricks, Sidings, Paints, Awnings, Signs;
        public readonly Material ScanBrick, Corrugated, OldConcrete, Container, Stucco, Shingles, TarRoof, Stone;
        public readonly Material Window, Void, Frame, FrameDark, DoorWood, DoorSteel, Gutter, WaterTank, Lattice, Neon, SignLight;

        private static StationArt last;
        private static StationTownArt cached;

        public static StationTownArt For(StationArt a)
        {
            if (cached == null || last != a)
            {
                cached = new StationTownArt(a);
                last = a;
            }
            return cached;
        }

        private StationTownArt(StationArt a)
        {
            string folder = a.Folder + "/Materials";
            string N(string n) => a.Prefix + "_Town" + n;
            Material T(Material m, float metres) { a.Tiling[m] = new Vector2(metres, metres); return m; }

            Bricks = new[]
            {
                T(Kit.Textured(N("BrickRed"), "FireBrick", new Color(.74f, .44f, .34f), Kit.Surface.Opaque, folder), 2.4f),
                T(Kit.Textured(N("BrickBrown"), "SootBrick", new Color(.92f, .78f, .7f), Kit.Surface.Opaque, folder), 2.4f),
                T(Kit.Textured(N("BrickBuff"), "FireBrick", new Color(1f, .9f, .72f), Kit.Surface.Opaque, folder), 2.4f),
                T(Kit.Textured(N("BrickDark"), "SootBrick", new Color(.7f, .58f, .52f), Kit.Surface.Opaque, folder), 2.4f),
            };
            Sidings = new[]
            {
                T(Kit.FromMall(N("SidingWhite"), "BoardWood", new Color(.86f, .85f, .8f), folder), 2f),
                T(Kit.FromMall(N("SidingBlue"), "BoardWood", new Color(.55f, .63f, .68f), folder), 2f),
                T(Kit.FromMall(N("SidingSage"), "BoardWood", new Color(.58f, .64f, .52f), folder), 2f),
                T(Kit.FromMall(N("SidingYellow"), "BoardWood", new Color(.86f, .78f, .52f), folder), 2f),
            };
            Stucco = T(Kit.FromMall(N("Stucco"), "Plaster", new Color(.8f, .74f, .62f), folder), 3f);
            Shingles = T(Kit.FromMall(N("Shingles"), "Tile", new Color(.32f, .3f, .29f), folder), 1.6f);
            TarRoof = T(Kit.FromMall(N("TarRoof"), "Concrete", new Color(.3f, .3f, .3f), folder), 4f);
            Stone = T(Kit.FromMall(N("Stone"), "FacadeStone", new Color(.78f, .76f, .7f), folder), 2f);

            ScanBrick = Scanned(a, N("ScanBrick"), "factory_brick", 1.5f, Bricks[0]);
            Corrugated = Scanned(a, N("Corrugated"), "corrugated_iron_02", 2f, a.Steel);
            OldConcrete = Scanned(a, N("OldConcrete"), "concrete_moss", 3f, a.Concrete);
            Container = Scanned(a, N("ContainerSide"), "container_side", 2.5f, a.Steel);

            Window = a.Plain("TownWindow", new Color(.06f, .075f, .085f), .92f, .15f);
            Void = a.Plain("TownVoid", new Color(.015f, .015f, .015f), .05f);
            Frame = a.Plain("TownFrame", new Color(.84f, .83f, .78f), .45f);
            FrameDark = a.Plain("TownFrameDark", new Color(.12f, .13f, .12f), .45f, .2f);
            DoorWood = a.Plain("TownDoorWood", new Color(.32f, .17f, .1f), .4f);
            DoorSteel = a.Plain("TownDoorSteel", new Color(.25f, .3f, .3f), .45f, .4f);
            Gutter = a.Plain("TownGutter", new Color(.2f, .21f, .2f), .4f, .5f);
            WaterTank = a.Plain("TownWaterTank", new Color(.32f, .24f, .17f), .2f);
            Lattice = a.Plain("TownLattice", new Color(.14f, .14f, .14f), .3f, .6f);
            Neon = a.Plain("TownNeonDead", new Color(.5f, .55f, .6f), .8f, .1f);
            SignLight = a.Plain("TownSignFace", new Color(.82f, .8f, .72f), .5f);
            Paints = new[]
            {
                a.Plain("TownPaintGreen", new Color(.14f, .27f, .2f), .45f), a.Plain("TownPaintRed", new Color(.45f, .1f, .08f), .45f),
                a.Plain("TownPaintBlue", new Color(.12f, .2f, .34f), .45f), a.Plain("TownPaintCream", new Color(.82f, .78f, .64f), .45f),
                a.Plain("TownPaintBlack", new Color(.06f, .06f, .065f), .5f),
            };
            Awnings = new[]
            {
                a.Plain("TownAwningRed", new Color(.5f, .09f, .07f), .2f), a.Plain("TownAwningGreen", new Color(.1f, .3f, .16f), .2f),
                a.Plain("TownAwningBlue", new Color(.1f, .2f, .42f), .2f), a.Plain("TownAwningTan", new Color(.66f, .55f, .38f), .2f),
                a.Plain("TownAwningWhite", new Color(.82f, .8f, .74f), .2f),
            };
            Signs = new[]
            {
                a.Plain("TownSignRed", new Color(.62f, .1f, .07f), .5f), a.Plain("TownSignYellow", new Color(.85f, .68f, .12f), .5f),
                a.Plain("TownSignBlue", new Color(.1f, .26f, .55f), .5f), a.Plain("TownSignGreen", new Color(.12f, .4f, .22f), .5f),
                a.Plain("TownSignOrange", new Color(.85f, .38f, .08f), .5f),
            };
        }

        public Material Brick(int seed) => Bricks[Mathf.Abs(seed) % Bricks.Length];
        public Material Siding(int seed) => Sidings[Mathf.Abs(seed) % Sidings.Length];
        public Material Paint(int seed) => Paints[Mathf.Abs(seed) % Paints.Length];
        public Material Awning(int seed) => Awnings[Mathf.Abs(seed) % Awnings.Length];
        public Material Sign(int seed) => Signs[Mathf.Abs(seed) % Signs.Length];

        // A URP Lit material on a photographed scan (colour, normal and packed mask); the fallback if the scan is missing.
        private static Material Scanned(StationArt a, string name, string scan, float metres, Material fallback)
        {
            string stem = $"{ShowcaseTerrain.GroundFolder}/{scan}";
            var colour = AssetDatabase.LoadAssetAtPath<Texture2D>(stem + "_diff.jpg");
            if (colour == null) return fallback;
            Material m = a.Plain(name.Substring(a.Prefix.Length + 1), Color.white, .55f);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(stem + "_nor_gl.jpg");
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(stem + "_mask.png");
            m.SetTexture("_BaseMap", colour);
            m.SetTextureScale("_BaseMap", Vector2.one);
            if (normal != null)
            {
                m.SetTexture("_BumpMap", normal);
                m.SetFloat("_BumpScale", 1f);
                m.EnableKeyword("_NORMALMAP");
            }
            if (mask != null)
            {
                m.SetTexture("_MetallicGlossMap", mask);
                m.SetTexture("_OcclusionMap", mask);
                m.SetFloat("_Smoothness", 1f);
                m.SetFloat("_OcclusionStrength", 1f);
                m.EnableKeyword("_METALLICSPECGLOSSMAP");
                m.EnableKeyword("_OCCLUSIONMAP");
            }
            EditorUtility.SetDirty(m);
            a.Tiling[m] = new Vector2(metres, metres);
            return m;
        }
    }
}
