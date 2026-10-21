using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Tests
{
    public class LightingRebuildTests
    {
        [SetUp]
        public void Setup() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        [TearDown]
        public void Cleanup() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        private static LightFixture Fixture(Vector3 at)
        {
            var holder = new GameObject("Test fixture");
            var child = new GameObject("Lamp");
            child.transform.SetParent(holder.transform);
            child.transform.SetPositionAndRotation(at, Quaternion.Euler(90f, 0, 0));
            Light lamp = child.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = 12f;
            lamp.intensity = 2f;
            lamp.spotAngle = 110f;
            lamp.innerSpotAngle = 60f;
            var fixture = holder.AddComponent<LightFixture>();
            fixture.EditorSetup(lamp, null, false, false, 42);
            return fixture;
        }

        private static GameObject Floor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = Vector3.down * .1f;
            floor.transform.localScale = new Vector3(24f, .2f, 24f);
            return floor;
        }

        [Test]
        public void ASpotDoesNotContributeOutsideItsConeOrRange()
        {
            Light lamp = Fixture(Vector3.up * 4).Lamp;
            Assert.Greater(LightingMath.Contribution(lamp, Vector3.zero), 0f);
            Assert.AreEqual(0f, LightingMath.Contribution(lamp, Vector3.up * 8));
            Assert.AreEqual(0f, LightingMath.Contribution(lamp, Vector3.down * 20));
            lamp.enabled = false;
            Assert.AreEqual(0f, LightingMath.Contribution(lamp, Vector3.zero));
        }

        [Test]
        public void ReceiverCalibrationIsRepeatableAndCompensatesForHeight()
        {
            Floor();
            Light lamp = Fixture(Vector3.up * 4).Lamp;
            var config = ScriptableObject.CreateInstance<IndirectLightingConfig>();
            try
            {
                LightingRebuild.ApplyScene(lamp.gameObject.scene, config);
                float intensity = lamp.intensity;
                float received = LightingMath.Contribution(lamp, Vector3.zero);
                Assert.That(received, Is.InRange(1.37f, 1.63f));
                LightingRebuild.ApplyScene(lamp.gameObject.scene, config);
                Assert.AreEqual(intensity, lamp.intensity, 1e-5f, "reapplying must not compound brightness");
                lamp.transform.position = Vector3.up * 8;
                LightingRebuild.ApplyScene(lamp.gameObject.scene, config);
                Assert.Greater(lamp.intensity, intensity * 2f);
                Assert.AreEqual(received, LightingMath.Contribution(lamp, Vector3.zero), .01f);
            }
            finally { Object.DestroyImmediate(config); }
        }

        [Test]
        public void AnOccludedFixtureDoesNotDisplaceVisibleFixtureShadows()
        {
            int saved = VideoSettings.Shadows;
            try
            {
                VideoSettings.Shadows = 1;
                Light hidden = Fixture(new Vector3(0, 3, 2)).Lamp;
                Light left = Fixture(new Vector3(-3, 3, 3)).Lamp;
                Light right = Fixture(new Vector3(3, 3, 3)).Lamp;
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = new Vector3(0, 1.5f, 1);
                wall.transform.localScale = new Vector3(1.5f, 3, .2f);
                var go = new GameObject("Eye");
                var camera = go.AddComponent<Camera>();
                camera.transform.position = new Vector3(0, 1.65f, 0);
                Physics.SyncTransforms();
                go.AddComponent<ShadowDirector>().Preview(camera);
                Assert.AreEqual(LightShadows.None, hidden.shadows);
                Assert.AreEqual(LightShadows.Soft, left.shadows);
                Assert.AreEqual(LightShadows.Soft, right.shadows);
            }
            finally { VideoSettings.Shadows = saved; }
        }

        [Test]
        public void ThinOpaqueArchitectureCastsButFixturePartsStayUnchanged()
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.localScale = new Vector3(4, 3, .01f);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Test_Wall" };
            Renderer renderer = wall.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var config = ScriptableObject.CreateInstance<IndirectLightingConfig>();
            try
            {
                LightingRebuild.ApplyScene(wall.scene, config);
                Assert.AreEqual(ShadowCastingMode.TwoSided, renderer.shadowCastingMode);
            }
            finally { Object.DestroyImmediate(material); Object.DestroyImmediate(config); }
        }

        [Test]
        public void ColdReflectionProbesReceiveValidFallbackTextures()
        {
            for (int i = 0; i < 70; i++) new GameObject("Cold probe").AddComponent<ReflectionProbe>();
            var go = new GameObject("Reflections");
            var rig = go.AddComponent<RoomReflections>();
            // The initialization callback isn't automatic in EditMode.
            typeof(RoomReflections).GetMethod("OnEnable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(rig, null);
            foreach (ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
                Assert.IsNotNull(probe.texture, "waiting probes must have a valid cube for URP's cluster allocation");
            typeof(RoomReflections).GetMethod("OnDisable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(rig, null);
        }
    }
}
