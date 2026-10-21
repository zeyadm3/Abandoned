using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    public class VisualEffectsRemasterTests
    {
        private GameObject root;
        private VisualEffectsConfig config;
        private Material original;

        [SetUp]
        public void Setup()
        {
            root = new GameObject("EffectsRemasterTest");
            config = ScriptableObject.CreateInstance<VisualEffectsConfig>();
            original = new Material(Shader.Find("Universal Render Pipeline/Particles/Lit")) { name = "QA_Original" };
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(original);
        }

        [TestCase("Snow_Exterior", "snow")]
        [TestCase("DEMOLITIONDust_1", "dust")]
        [TestCase("LitMotes", "dust")]
        [TestCase("MoltenSparks", "spark")]
        [TestCase("Embers", "spark")]
        [TestCase("SmokestackSteamPlume", "steam")]
        [TestCase("Smoke", "steam")]
        [TestCase("Plume", "steam")]
        [TestCase("ColdBreath", null)]
        [TestCase("TorchFlame", null)]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void ClassificationRecognizesAuthoredEffectFamilies(string name, string expected) =>
            Assert.AreEqual(expected, VisualEffectsRemaster.Kind(name));

        [TestCase("DispatchAir", "ShopDust")]
        [TestCase("StockAisleWest", "ShopDust")]
        [TestCase("Area_1_Cinema", "Dust")]
        [TestCase("AtriumAir", "Dust")]
        public void LocalizedRoomPoolsUseTheirAuthoredMaterialForClassification(string name, string materialName)
        {
            original.name = materialName;
            ParticleSystem particles = Emitter(name);
            Assert.AreEqual(1, VisualEffectsRemaster.ApplyToRoot(root.transform, config));
            Assert.AreEqual("dust", particles.GetComponent<ParticleSystemRenderer>().sharedMaterial.name);
            Assert.AreEqual(config.DustSize.x, particles.main.startSize.constantMin);
        }

        [Test]
        public void ExplicitEmitterFamilyTakesPrecedenceOverSharedMaterialName()
        {
            original.name = "Dust";
            ParticleSystem particles = Emitter("SmokestackSteam");
            Assert.AreEqual(1, VisualEffectsRemaster.ApplyToRoot(root.transform, config));
            Assert.AreEqual("steam", particles.GetComponent<ParticleSystemRenderer>().sharedMaterial.name);
        }

        [Test]
        public void SerializedConfigValuesRemainUsableOutsideTheInspector()
        {
            var data = new SerializedObject(config);
            data.FindProperty("snowflakeSize").vector2Value = new Vector2(.05f, .02f);
            data.FindProperty("dustSize").vector2Value = new Vector2(-.5f, -.1f);
            data.FindProperty("dustOpacity").floatValue = -2f;
            data.FindProperty("steamOpacity").floatValue = 3f;
            data.FindProperty("dustPerEmitter").intValue = -10;
            data.FindProperty("snowPerEmitter").intValue = 0;
            data.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(new Vector2(.02f, .05f), config.SnowflakeSize);
            Assert.Greater(config.DustSize.x, 0f);
            Assert.GreaterOrEqual(config.DustSize.y, config.DustSize.x);
            Assert.Zero(config.DustOpacity);
            Assert.AreEqual(1f, config.SteamOpacity);
            Assert.AreEqual(1, config.DustPerEmitter);
            Assert.AreEqual(1, config.SnowPerEmitter);
        }

        [TestCase("Dust", 120)]
        [TestCase("Snow", 600)]
        public void ParticleBudgetIsCappedWithoutIncreasingSmallAuthoredPools(string name, int defaultLimit)
        {
            ParticleSystem small = Emitter(name + "Small"), large = Emitter(name + "Large");
            var smallMain = small.main;
            smallMain.maxParticles = 16;
            var largeMain = large.main;
            largeMain.maxParticles = 2000;
            Assert.AreEqual(2, VisualEffectsRemaster.ApplyToRoot(root.transform, config));
            Assert.AreEqual(16, small.main.maxParticles);
            Assert.AreEqual(defaultLimit, large.main.maxParticles);
            Material material = large.GetComponent<ParticleSystemRenderer>().sharedMaterial;
            Assert.AreEqual(2, VisualEffectsRemaster.ApplyToRoot(root.transform, config));
            Assert.AreSame(material, large.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            Assert.AreEqual(defaultLimit, large.main.maxParticles);
        }

        [TestCase("Dust", false)]
        [TestCase("Snow", false)]
        [TestCase("Steam", false)]
        [TestCase("Spark", true)]
        public void ParticleMaterialsHaveTransparentDepthAndLightingState(string name, bool additive)
        {
            ParticleSystem particles = Emitter(name);
            VisualEffectsRemaster.ApplyToRoot(root.transform, config);
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            Material material = renderer.sharedMaterial;
            Assert.AreEqual(additive ? "Universal Render Pipeline/Particles/Unlit" : "Universal Render Pipeline/Particles/Lit", material.shader.name);
            Assert.AreEqual((int)RenderQueue.Transparent, material.renderQueue);
            Assert.AreEqual("Transparent", material.GetTag("RenderType", false));
            Assert.Zero(material.GetFloat("_ZWrite"));
            Assert.AreEqual((float)BlendMode.SrcAlpha, material.GetFloat("_SrcBlend"));
            Assert.AreEqual((float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha), material.GetFloat("_DstBlend"));
            Assert.IsTrue(material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"));
            Assert.IsTrue(material.IsKeywordEnabled("_SOFTPARTICLES_ON"));
            Assert.IsTrue(material.IsKeywordEnabled("_FADING_ON"));
            Assert.AreEqual(ShadowCastingMode.Off, renderer.shadowCastingMode);
            Assert.IsTrue(renderer.receiveShadows);
            var streams = new List<ParticleSystemVertexStream>();
            renderer.GetActiveVertexStreams(streams);
            CollectionAssert.Contains(streams, ParticleSystemVertexStream.Normal);
            CollectionAssert.Contains(streams, ParticleSystemVertexStream.Color);
            var texture = (Texture2D)material.GetTexture("_BaseMap");
            Assert.IsNotNull(texture);
            Assert.AreEqual(TextureWrapMode.Clamp, texture.wrapMode);
            Assert.Greater(texture.mipmapCount, 1);
            Vector4 fade = material.GetVector("_SoftParticleFadeParams");
            Assert.Greater(fade.y, 0f);
        }

        [Test]
        public void AuthoringPreservesPlacementEmissionTriggersAndSimulation()
        {
            root.transform.SetPositionAndRotation(new Vector3(12, 3, -7), Quaternion.Euler(5, 45, 0));
            root.transform.localScale = new Vector3(2, 1.5f, 3);
            ParticleSystem particles = Emitter("SectionDust");
            particles.transform.localPosition = new Vector3(.4f, 2, -.6f);
            particles.transform.localRotation = Quaternion.Euler(12, 35, 7);
            Vector3 worldPosition = particles.transform.position;
            Quaternion rotation = particles.transform.rotation;
            var main = particles.main;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.1f, .5f);
            main.gravityModifier = .27f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            particles.useAutoRandomSeed = false;
            particles.randomSeed = 2309;
            var emission = particles.emission;
            emission.rateOverTime = 18f;
            emission.rateOverDistance = 3f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(.4f, (short)12) });
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3, 2, 4);
            shape.position = new Vector3(.2f, .4f, .6f);
            var collision = particles.collision;
            collision.enabled = true;
            collision.collidesWith = 1 << 4;
            var triggerObject = new GameObject("GameplayTrigger");
            triggerObject.transform.SetParent(root.transform, false);
            var volume = triggerObject.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            var trigger = particles.trigger;
            trigger.enabled = true;
            trigger.inside = ParticleSystemOverlapAction.Kill;
            trigger.SetCollider(0, volume);

            Assert.AreEqual(1, VisualEffectsRemaster.ApplyToRoot(root.transform, config));
            Assert.AreEqual(worldPosition, particles.transform.position);
            Assert.AreEqual(rotation, particles.transform.rotation);
            Assert.IsFalse(particles.main.loop);
            Assert.AreEqual(2f, particles.main.startLifetime.constantMin);
            Assert.AreEqual(8f, particles.main.startLifetime.constantMax);
            Assert.AreEqual(.1f, particles.main.startSpeed.constantMin);
            Assert.AreEqual(.5f, particles.main.startSpeed.constantMax);
            Assert.AreEqual(.27f, particles.main.gravityModifier.constant);
            Assert.AreEqual(ParticleSystemSimulationSpace.World, particles.main.simulationSpace);
            Assert.IsFalse(particles.useAutoRandomSeed);
            Assert.AreEqual(2309u, particles.randomSeed);
            Assert.AreEqual(18f, particles.emission.rateOverTime.constant);
            Assert.AreEqual(3f, particles.emission.rateOverDistance.constant);
            Assert.AreEqual(1, particles.emission.burstCount);
            Assert.AreEqual(.4f, particles.emission.GetBurst(0).time);
            Assert.AreEqual(12f, particles.emission.GetBurst(0).count.constant);
            Assert.AreEqual(ParticleSystemShapeType.Box, particles.shape.shapeType);
            Assert.AreEqual(new Vector3(3, 2, 4), particles.shape.scale);
            Assert.AreEqual(new Vector3(.2f, .4f, .6f), particles.shape.position);
            Assert.IsTrue(particles.collision.enabled);
            Assert.AreEqual(1 << 4, (int)particles.collision.collidesWith);
            Assert.IsTrue(particles.trigger.enabled);
            Assert.AreEqual(ParticleSystemOverlapAction.Kill, particles.trigger.inside);
            Assert.AreSame(volume, particles.trigger.GetCollider(0));
            Assert.AreEqual(1, root.GetComponentsInChildren<Collider>().Length);
        }

        [Test]
        public void UnrecognizedChildEffectRetainsItsMaterialAndPlayback()
        {
            ParticleSystem dust = Emitter("Dust"), flame = Emitter("TorchFlame");
            flame.transform.SetParent(dust.transform, false);
            flame.Play(false);
            Assert.IsTrue(flame.isPlaying);
            Assert.AreEqual(1, VisualEffectsRemaster.ApplyToRoot(root.transform, config));
            Assert.AreSame(original, flame.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            Assert.IsTrue(flame.isPlaying, "Refining a parent must not stop unrelated child effects.");
        }

        private ParticleSystem Emitter(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = original;
            return particles;
        }
    }
}
