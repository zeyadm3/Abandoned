using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    /// <summary>Physical material semantics, artwork retention and surfaces that follow carried objects.</summary>
    public class MaterialRemasterTests
    {
        [TestCase("House_Trim", MaterialRemaster.SurfaceKind.PaintedWood)]
        [TestCase("House_TrimCream", MaterialRemaster.SurfaceKind.PaintedWood)]
        [TestCase("House_StreetSidingShadowBlue", MaterialRemaster.SurfaceKind.Siding)]
        [TestCase("Island_Mattress", MaterialRemaster.SurfaceKind.Canvas)]
        [TestCase("ENV_Cloth", MaterialRemaster.SurfaceKind.Canvas)]
        [TestCase("FactoryLoot_Paper", MaterialRemaster.SurfaceKind.Paper)]
        [TestCase("IslandLoot_Paper", MaterialRemaster.SurfaceKind.Paper)]
        [TestCase("ENV_Brass", MaterialRemaster.SurfaceKind.Brass)]
        [TestCase("HQ_StaffDoorHardware", MaterialRemaster.SurfaceKind.BareMetal)]
        [TestCase("Burley_LockerGreen", MaterialRemaster.SurfaceKind.PaintedMetal)]
        [TestCase("Mall_Floor_concourse_1", MaterialRemaster.SurfaceKind.Terrazzo)]
        [TestCase("MALL_FacadeStone", MaterialRemaster.SurfaceKind.Stone)]
        [TestCase("Island_Facade", MaterialRemaster.SurfaceKind.Plaster)]
        [TestCase("IronFactory_Wall", MaterialRemaster.SurfaceKind.Plaster)]
        [TestCase("IronFactory_Asphalt", MaterialRemaster.SurfaceKind.Asphalt)]
        [TestCase("Winter_Snow", MaterialRemaster.SurfaceKind.Snow)]
        [TestCase("Winter_SnowyAsphalt", MaterialRemaster.SurfaceKind.TroddenSnow)]
        [TestCase("FactoryLoot_flywheel", MaterialRemaster.SurfaceKind.BareMetal)]
        [TestCase("Loot_smartphone", MaterialRemaster.SurfaceKind.Plastic)]
        [TestCase("House_WindowGlass", MaterialRemaster.SurfaceKind.None)]
        [TestCase("Mall_SignFace", MaterialRemaster.SurfaceKind.None)]
        [TestCase("Winter_PineFoliage", MaterialRemaster.SurfaceKind.None)]
        public void NamesDescribeTheSurfaceRatherThanTheMapPrefix(string name, MaterialRemaster.SurfaceKind expected) =>
            Assert.AreEqual(expected, MaterialRemaster.Classify(name));

        [Test]
        public void TerrazzoAggregateAndSnowDetailAreAtBelievableScales()
        {
            Assert.LessOrEqual(MaterialRemaster.MetresFor("Mall_Terrazzo"), .25f);
            Assert.LessOrEqual(MaterialRemaster.MetresFor("Winter_Snow"), 1f);
            var snow = Make("Winter_Snow");
            try
            {
                Assert.IsTrue(MaterialRemaster.Apply(snow, 1f, false, false, Color.white));
                Assert.LessOrEqual(snow.GetFloat("_BumpScale"), .2f, "Snow should have soft grains rather than stone-sized craters.");
                Assert.Zero(snow.GetFloat("_Grime"));
                Assert.Zero(snow.GetFloat("_Dust"));
            }
            finally { Object.DestroyImmediate(snow); }
        }

        [Test]
        public void PaintedTrimIsDielectricAndRetainsReadableFinish()
        {
            var trim = Make("House_TrimCream");
            try
            {
                trim.SetFloat("_Metallic", 1f);
                Assert.IsTrue(MaterialRemaster.Apply(trim, .5f, false, false, new Color(.72f, .69f, .62f)));
                Assert.Zero(trim.GetFloat("_Metallic"));
                Assert.Less(trim.GetFloat("_Grime"), .06f);
                Assert.Less(trim.GetFloat("_Dust"), .08f);
                Assert.IsTrue(trim.IsKeywordEnabled("_NORMALMAP"));
                Assert.IsNotNull(trim.GetTexture("_MetallicGlossMap"));
            }
            finally { Object.DestroyImmediate(trim); }
        }

        [Test]
        public void CarriedSurfaceCannotSlideThroughWorldSpace()
        {
            var material = Make("Loot_Steel");
            try
            {
                Assert.IsTrue(MaterialRemaster.Apply(material, 0f, true, true, Color.gray));
                Assert.IsFalse(material.IsKeywordEnabled("_TRIPLANAR"));
                Assert.IsFalse(material.IsKeywordEnabled("_WEATHERING"));
                Assert.Zero(material.GetFloat("_Triplanar"));
                Assert.Zero(material.GetFloat("_Weathering"));
                Assert.Less(material.GetTextureScale("_BaseMap").x, 10f, "Missing density uses a local fallback, not extreme tiling.");
            }
            finally { Object.DestroyImmediate(material); }
        }

        [Test]
        public void PaletteArtAndItsUvsSurviveIndependentDetailNormals()
        {
            var material = Make("Loot_Plastic");
            var palette = new Texture2D(2, 2);
            try
            {
                material.SetTexture("_BaseMap", palette);
                material.SetTextureScale("_BaseMap", new Vector2(2f, 3f));
                material.SetTextureOffset("_BaseMap", new Vector2(.2f, .3f));
                material.SetColor("_BaseColor", Color.cyan);
                Assert.IsTrue(MaterialRemaster.Apply(material, 2f, true, true, Color.white, preserveArtwork: true));
                Assert.AreSame(palette, material.GetTexture("_BaseMap"));
                Assert.AreEqual(new Vector2(2f, 3f), material.GetTextureScale("_BaseMap"));
                Assert.AreEqual(new Vector2(.2f, .3f), material.GetTextureOffset("_BaseMap"));
                Assert.AreEqual(Color.cyan, material.GetColor("_BaseColor"));
                Assert.IsNotNull(material.GetTexture("_DetailNormalMap"));
                Assert.IsTrue(material.IsKeywordEnabled("_DETAIL_SCALED"));
                Assert.Zero(material.GetFloat("_DetailAlbedoMapScale"), "Printed colours must not be multiplied by an unrelated scan.");
                Assert.IsFalse(material.IsKeywordEnabled("_TRIPLANAR"));
            }
            finally { Object.DestroyImmediate(material); Object.DestroyImmediate(palette); }
        }

        [Test]
        public void ArtworkFinishIgnoresLegacyMetallicAndSmoothnessMasks()
        {
            var material = Make("Loot_Plastic");
            var legacy = new Texture2D(2, 2);
            try
            {
                material.SetTexture("_MetallicGlossMap", legacy);
                material.SetTexture("_SpecGlossMap", legacy);
                material.SetTexture("_BumpMap", legacy);
                material.SetFloat("_BumpScale", 1f);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                Assert.IsTrue(MaterialRemaster.Apply(material, 1f, false, true, Color.white, preserveArtwork: true));
                Assert.IsNull(material.GetTexture("_MetallicGlossMap"));
                Assert.IsNull(material.GetTexture("_SpecGlossMap"));
                Assert.IsFalse(material.IsKeywordEnabled("_METALLICSPECGLOSSMAP"));
                Assert.IsFalse(material.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"));
                Assert.Zero(material.GetFloat("_Metallic"));
                Assert.AreSame(legacy, material.GetTexture("_BumpMap"), "Authored shape normals survive with calibrated strength.");
                Assert.LessOrEqual(material.GetFloat("_BumpScale"), .2f);
            }
            finally { Object.DestroyImmediate(material); Object.DestroyImmediate(legacy); }
        }

        [Test]
        public void StreetRoofVariantsUseShingleArtworkInsteadOfLegacyGravel()
        {
            var material = Make("House_ShinglesBrown");
            try
            {
                Assert.IsTrue(MaterialRemaster.Apply(material, .5f, false, false, Color.white, preserveArtwork: true));
                Assert.IsNotNull(material.GetTexture("_BaseMap"));
                Assert.AreEqual("Shingles_BaseColor", material.GetTexture("_BaseMap").name);
                Assert.AreEqual("Shingles_Normal", material.GetTexture("_BumpMap").name);
                Color tint = material.GetColor("_BaseColor");
                Assert.That(tint.r, Is.EqualTo(.30f).Within(1e-5f));
                Assert.That(tint.g, Is.EqualTo(.22f).Within(1e-5f));
                Assert.That(tint.b, Is.EqualTo(.16f).Within(1e-5f));
                Assert.IsFalse(material.IsKeywordEnabled("_METALLICSPECGLOSSMAP"));
            }
            finally { Object.DestroyImmediate(material); }
        }

        [Test]
        public void ReapplyingWithTheSameSourceColourIsIdempotent()
        {
            var material = Make("House_Wood");
            var source = new Color(.38f, .24f, .12f);
            try
            {
                Assert.IsTrue(MaterialRemaster.Apply(material, .5f, false, false, source));
                Color colour = material.GetColor("_BaseColor");
                Vector2 scale = material.GetTextureScale("_BaseMap");
                Texture texture = material.GetTexture("_BaseMap");
                Assert.IsTrue(MaterialRemaster.Apply(material, .5f, false, false, source));
                Assert.AreEqual(colour, material.GetColor("_BaseColor"));
                Assert.AreEqual(scale, material.GetTextureScale("_BaseMap"));
                Assert.AreSame(texture, material.GetTexture("_BaseMap"));
            }
            finally { Object.DestroyImmediate(material); }
        }

        [Test]
        public void TransparentAndEmissiveSurfacesRemainSpecialized()
        {
            var material = Make("Plastic");
            try
            {
                material.SetFloat("_Surface", 1f);
                Assert.IsFalse(MaterialRemaster.Apply(material, 1f, false, false, Color.white));
                material.SetFloat("_Surface", 0f);
                material.EnableKeyword("_EMISSION");
                Assert.IsFalse(MaterialRemaster.Apply(material, 1f, false, false, Color.white));
            }
            finally { Object.DestroyImmediate(material); }
        }

        private static Material Make(string name)
        {
            Shader shader = Shader.Find(MaterialUpgrade.SurfaceShader);
            Assert.IsNotNull(shader);
            return new Material(shader) { name = name };
        }
    }
}
