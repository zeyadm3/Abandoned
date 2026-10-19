using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Master materials, colors, and textures for Jackson P. Burley Middle School.
    /// Incorporates the school's historic colors (Kelly Green and Old Gold, Home of the Bears),
    /// 1951 mid-century red brick architecture, polished hardwood gymnasium court, and athletic facilities.
    /// </summary>
    public sealed class BurleySchoolArt
    {
        public const string Folder = "Assets/_Project/Art/School";
        private const string MaterialFolder = Folder + "/Materials";

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
        public readonly Material Asphalt;
        public readonly Material TrackRubber;
        public readonly Material TurfGrass;
        public readonly Material InfieldClay;
        public readonly Material Sidewalk;
        public readonly Material RoadAsphalt;
        public readonly Material WhiteStripe;
        public readonly Material YellowStripe;
        public readonly Material FenceChainlink;

        // Lighting & Atmosphere
        public readonly Material FluorescentGlow;
        public readonly Material ExitGlow;
        public readonly Material LampSodium;

        // Font
        public const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public BurleySchoolArt()
        {
            PolishAssets.EnsureFolder(MaterialFolder);
            IronFactoryTextures.EnsureAll();

            // School Building Facades & Structural Envelopes
            Brick = PolishAssets.Material("Burley_Brick", new Color(0.68f, 0.32f, 0.22f), roughness: 0.82f);
            BrickDark = PolishAssets.Material("Burley_BrickDark", new Color(0.48f, 0.22f, 0.16f), roughness: 0.85f);
            Limestone = PolishAssets.Material("Burley_Limestone", new Color(0.86f, 0.84f, 0.78f), roughness: 0.72f);
            Concrete = PolishAssets.Material("Burley_Concrete", new Color(0.65f, 0.64f, 0.62f), roughness: 0.80f);
            RoofGravel = PolishAssets.Material("Burley_RoofGravel", new Color(0.38f, 0.37f, 0.35f), roughness: 0.92f);
            Terrazzo = PolishAssets.Material("Burley_Terrazzo", new Color(0.82f, 0.80f, 0.75f), roughness: 0.38f);
            FloorTile = PolishAssets.Material("Burley_FloorTile", new Color(0.74f, 0.73f, 0.68f), roughness: 0.45f);
            AcousticCeiling = PolishAssets.Material("Burley_Ceiling", new Color(0.88f, 0.87f, 0.84f), roughness: 0.88f);
            PlasterWall = PolishAssets.Material("Burley_PlasterWall", new Color(0.82f, 0.83f, 0.79f), roughness: 0.85f);

            // Burley School Colors & Interior Materials
            // Historic Burley High / Middle School colors: Kelly Green and Old Gold
            LockerGreen = PolishAssets.Material("Burley_LockerGreen", new Color(0.11f, 0.35f, 0.21f), roughness: 0.42f);
            GoldAccent = PolishAssets.Material("Burley_GoldAccent", new Color(0.82f, 0.68f, 0.20f), roughness: 0.35f);
            GymWood = PolishAssets.Material("Burley_GymWood", new Color(0.88f, 0.72f, 0.48f), roughness: 0.25f);
            StageWood = PolishAssets.Material("Burley_StageWood", new Color(0.35f, 0.22f, 0.12f), roughness: 0.65f);
            OakWood = PolishAssets.Material("Burley_OakWood", new Color(0.55f, 0.38f, 0.22f), roughness: 0.55f);
            Chalkboard = PolishAssets.Material("Burley_Chalkboard", new Color(0.18f, 0.28f, 0.22f), roughness: 0.88f);
            LabEpoxy = PolishAssets.Material("Burley_LabEpoxy", new Color(0.12f, 0.13f, 0.14f), roughness: 0.30f);
            StageCurtain = PolishAssets.Material("Burley_StageCurtain", new Color(0.08f, 0.26f, 0.15f), roughness: 0.92f);
            TheaterSeat = PolishAssets.Material("Burley_TheaterSeat", new Color(0.14f, 0.24f, 0.32f), roughness: 0.80f);
            MetalSteel = PolishAssets.Material("Burley_MetalSteel", new Color(0.32f, 0.34f, 0.35f), roughness: 0.40f);

            // Transparent glass
            WindowGlass = PolishAssets.Material("Burley_Glass", new Color(0.65f, 0.82f, 0.88f, 0.35f), roughness: 0.10f);
            ConfigureTransparent(WindowGlass);

            // Athletics & Exterior Grounds
            Asphalt = PolishAssets.Material("Burley_Asphalt", new Color(0.24f, 0.24f, 0.25f), roughness: 0.85f);
            TrackRubber = PolishAssets.Material("Burley_TrackRubber", new Color(0.56f, 0.22f, 0.18f), roughness: 0.88f);
            TurfGrass = PolishAssets.Material("Burley_TurfGrass", new Color(0.24f, 0.38f, 0.16f), roughness: 0.85f);
            InfieldClay = PolishAssets.Material("Burley_InfieldClay", new Color(0.65f, 0.38f, 0.25f), roughness: 0.92f);
            Sidewalk = PolishAssets.Material("Burley_Sidewalk", new Color(0.72f, 0.70f, 0.66f), roughness: 0.80f);
            RoadAsphalt = PolishAssets.Material("Burley_RoadAsphalt", new Color(0.20f, 0.20f, 0.21f), roughness: 0.80f);
            WhiteStripe = PolishAssets.Material("Burley_WhiteStripe", new Color(0.92f, 0.92f, 0.90f), roughness: 0.45f);
            YellowStripe = PolishAssets.Material("Burley_YellowStripe", new Color(0.92f, 0.78f, 0.12f), roughness: 0.45f);
            FenceChainlink = PolishAssets.Material("Burley_Fence", new Color(0.48f, 0.50f, 0.52f), roughness: 0.40f);

            // Lighting & Diffusers
            FluorescentGlow = PolishAssets.Material("Burley_FluorescentGlow", new Color(0.96f, 0.98f, 0.92f), roughness: 0.30f, emission: 2.2f);
            ExitGlow = PolishAssets.Material("Burley_ExitGlow", new Color(0.22f, 0.85f, 0.38f), roughness: 0.30f, emission: 2.5f);
            LampSodium = PolishAssets.Material("Burley_LampSodium", new Color(1.0f, 0.75f, 0.35f), roughness: 0.30f, emission: 3.0f);
        }

        private static void ConfigureTransparent(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
        }
    }
}
