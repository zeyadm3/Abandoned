using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Master materials and PBR surface shaders for Jackson P. Burley Middle School in winter.
    /// Fully binds generated BaseColor, Normal, and Mask maps for snow, brick, tile, hardwood, and glass.
    /// </summary>
    public sealed class BurleySchoolArt
    {
        public const string Folder = "Assets/_Project/Art/School";
        private const string MaterialFolder = Folder + "/Materials";

        // Winter Ground, Snow & Ice
        public readonly Material Snow;
        public readonly Material SnowDrift;
        public readonly Material FrostIce;
        public readonly Material SnowyAsphalt;
        public readonly Material PineFoliage;

        // Architecture & Structure
        public readonly Material Brick;
        public readonly Material BrickDark;
        public readonly Material Limestone;
        public readonly Material Concrete;
        public readonly Material RoofGravel;
        public readonly Material Terrazzo;
        public readonly Material FloorTile;
        public readonly Material AcousticCeiling;
        public readonly Material PlasterWall;

        // Interior & Thematic
        public readonly Material GymWood;
        public readonly Material StageWood;
        public readonly Material OakWood;
        public readonly Material LockerGreen;
        public readonly Material GoldAccent;
        public readonly Material Chalkboard;
        public readonly Material LabEpoxy;
        public readonly Material StageCurtain;
        public readonly Material TheaterSeat;
        public readonly Material MetalSteel;
        public readonly Material WindowGlass;

        // Athletics & Exterior
        public readonly Material TrackRubber;
        public readonly Material TurfGrass;
        public readonly Material InfieldClay;
        public readonly Material Sidewalk;
        public readonly Material WhiteStripe;
        public readonly Material YellowStripe;
        public readonly Material FenceChainlink;

        // Lighting & Atmosphere
        public readonly Material FluorescentGlow;
        public readonly Material ExitGlow;
        public readonly Material LampSodium;
        public readonly Material WeatherParticle;
        public readonly Material DampStain;
        public readonly Material Rust;
        public readonly Material Soil;
        public readonly Dictionary<Material, Vector2> Tiling = new();

        public const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public BurleySchoolArt()
        {
            PolishAssets.EnsureFolder(MaterialFolder);
            BurleySchoolTextures.EnsureAll();

            // 1. Winter Surfaces
            Snow = CreatePbr("Winter_Snow", "WinterSnow", new Color(0.96f, 0.98f, 1.0f));
            SnowDrift = CreatePbr("Winter_SnowDrift", "SnowDrift", new Color(0.94f, 0.97f, 1.0f));
            FrostIce = CreatePbr("Winter_FrostIce", "FrostIce", new Color(0.85f, 0.92f, 0.98f, 0.85f), transparent: true);
            SnowyAsphalt = CreatePbr("Winter_SnowyAsphalt", "SnowyAsphalt", Color.white);
            PineFoliage = CreatePbr("Winter_PineFoliage", "PineSnow", Color.white);

            // 2. Architectural Envelope
            Brick = CreatePbr("Burley_Brick", "WinterBrick", Color.white);
            BrickDark = CreatePbr("Burley_BrickDark", "WinterBrickDark", Color.white);
            Limestone = PolishAssets.Material("Burley_Limestone", new Color(0.88f, 0.87f, 0.82f), roughness: 0.65f);
            Concrete = PolishAssets.Material("Burley_Concrete", new Color(0.72f, 0.73f, 0.74f), roughness: 0.75f);
            RoofGravel = PolishAssets.Material("Burley_RoofGravel", new Color(0.42f, 0.44f, 0.46f), roughness: 0.90f);
            Terrazzo = PolishAssets.Material("Burley_Terrazzo", new Color(0.86f, 0.85f, 0.82f), roughness: 0.32f);
            FloorTile = CreatePbr("Burley_FloorTile", "SchoolTile", Color.white);
            AcousticCeiling = CreatePbr("Burley_AcousticCeiling", "AcousticCeiling", Color.white);
            PlasterWall = CreatePbr("Burley_PlasterWall", "SchoolPlaster", Color.white);

            // 3. Interior Facilities & Burley Colors (Kelly Green and Old Gold)
            LockerGreen = CreatePbr("Burley_LockerGreen", "LockerMetal", Color.white);
            GoldAccent = PolishAssets.Material("Burley_GoldAccent", new Color(0.86f, 0.72f, 0.22f), roughness: 0.32f);
            GymWood = CreatePbr("Burley_GymWood", "GymMaple", Color.white);
            StageWood = PolishAssets.Material("Burley_StageWood", new Color(0.32f, 0.20f, 0.12f), roughness: 0.60f);
            OakWood = PolishAssets.Material("Burley_OakWood", new Color(0.52f, 0.36f, 0.22f), roughness: 0.50f);
            Chalkboard = CreatePbr("Burley_Chalkboard", "Chalkboard", Color.white);
            LabEpoxy = PolishAssets.Material("Burley_LabEpoxy", new Color(0.11f, 0.12f, 0.13f), roughness: 0.28f);
            StageCurtain = PolishAssets.Material("Burley_StageCurtain", new Color(0.08f, 0.24f, 0.14f), roughness: 0.90f);
            TheaterSeat = PolishAssets.Material("Burley_TheaterSeat", new Color(0.12f, 0.22f, 0.32f), roughness: 0.82f);
            MetalSteel = PolishAssets.Material("Burley_MetalSteel", new Color(0.35f, 0.37f, 0.39f), roughness: 0.35f);

            // 4. Frosted Winter Windows
            WindowGlass = CreatePbr("Burley_FrostedGlass", "FrostedGlass", new Color(0.85f, 0.92f, 0.96f, 0.55f), transparent: true);

            // 5. Athletics Complex & Surrounding Roads
            TrackRubber = PolishAssets.Material("Burley_TrackRubber", new Color(0.55f, 0.22f, 0.18f), roughness: 0.85f);
            TurfGrass = PolishAssets.Material("Burley_TurfGrass", new Color(0.32f, 0.42f, 0.30f), roughness: 0.85f);
            InfieldClay = PolishAssets.Material("Burley_InfieldClay", new Color(0.60f, 0.38f, 0.28f), roughness: 0.90f);
            Sidewalk = PolishAssets.Material("Burley_Sidewalk", new Color(0.78f, 0.79f, 0.80f), roughness: 0.75f);
            WhiteStripe = PolishAssets.Material("Burley_WhiteStripe", new Color(0.95f, 0.95f, 0.94f), roughness: 0.40f);
            YellowStripe = PolishAssets.Material("Burley_YellowStripe", new Color(0.94f, 0.80f, 0.15f), roughness: 0.40f);
            FenceChainlink = PolishAssets.Material("Burley_Fence", new Color(0.55f, 0.58f, 0.60f), roughness: 0.40f);

            // 6. Lighting & Glow
            FluorescentGlow = PolishAssets.Material("Burley_FluorescentGlow", new Color(0.96f, 0.98f, 0.92f), roughness: 0.30f, emission: 2.4f);
            ExitGlow = PolishAssets.Material("Burley_ExitGlow", new Color(0.20f, 0.88f, 0.36f), roughness: 0.30f, emission: 2.8f);
            LampSodium = PolishAssets.Material("Burley_LampSodium", new Color(1.0f, 0.78f, 0.38f), roughness: 0.30f, emission: 3.2f);
            DampStain = PolishAssets.Stain();
            Rust = PolishAssets.Material("Burley_OxidisedSteel", new Color(.27f, .15f, .09f), roughness: .92f);
            Soil = PolishAssets.Material("Burley_DormantGround", new Color(.24f, .28f, .24f), roughness: .98f);
            WeatherParticle = CreateWeatherParticle();

            // Repeats are metres, not one image stretched across an entire wall or floor slab.
            Tiling[Brick] = Tiling[BrickDark] = new Vector2(1.84f, 1.2f);
            Tiling[FloorTile] = new Vector2(2.4f, 2.4f);
            Tiling[GymWood] = new Vector2(4f, 3.2f);
            Tiling[PlasterWall] = new Vector2(3f, 3f);
            Tiling[AcousticCeiling] = new Vector2(2.4f, 2.4f);
            Tiling[Snow] = Tiling[SnowDrift] = new Vector2(6f, 6f);
            Tiling[SnowyAsphalt] = new Vector2(8f, 8f);
            Tiling[FrostIce] = new Vector2(4f, 4f);
            Tiling[WindowGlass] = new Vector2(2f, 2f);
            Tiling[LockerGreen] = new Vector2(1.2f, 1.8f);
        }

        private static Material CreateWeatherParticle()
        {
            string path = MaterialFolder + "/Burley_WeatherParticle.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (material == null)
            {
                material = new Material(shader) { name = "Burley_WeatherParticle" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", PolishAssets.Texture("Burley_SoftSnowflake", stain: true));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreatePbr(string matName, string textureSet, Color tint, bool transparent = false)
        {
            string path = $"{MaterialFolder}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader required for School materials.");

            if (mat == null)
            {
                mat = new Material(shader) { name = matName };
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = shader;
            Texture2D baseMap = BurleySchoolTextures.Get(textureSet, "BaseColor");
            Texture2D normalMap = BurleySchoolTextures.Get(textureSet, "Normal");
            Texture2D maskMap = BurleySchoolTextures.Get(textureSet, "Mask");

            mat.SetTexture("_BaseMap", baseMap);
            mat.SetColor("_BaseColor", tint);
            if (normalMap != null)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", 1.0f);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (maskMap != null)
            {
                mat.SetTexture("_MetallicGlossMap", maskMap);
                mat.SetTexture("_OcclusionMap", maskMap);
                mat.SetFloat("_Smoothness", 1.0f);
                mat.SetFloat("_OcclusionStrength", 1.0f);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.EnableKeyword("_OCCLUSIONMAP");
            }

            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.SetFloat("_Cull", (float)CullMode.Off);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                mat.SetFloat("_Surface", 0f);
                mat.SetFloat("_ZWrite", 1f);
                mat.SetFloat("_Cull", (float)CullMode.Back);
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = (int)RenderQueue.Geometry;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
