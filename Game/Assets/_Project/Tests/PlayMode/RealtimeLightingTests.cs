using System.Collections;
using Abandoned.Core;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    public class RealtimeLightingTests
    {
        [UnityTest]
        public IEnumerator DiffuseBounceFollowsPowerAndLosesACollapsedReceiver()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.CreateScene("Bounce lifecycle test");
            SceneManager.SetActiveScene(scene);
            var config = ScriptableObject.CreateInstance<IndirectLightingConfig>();
            try
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 origin = Vector3.right * 10000f;
                floor.transform.position = origin + Vector3.down * .1f;
                floor.transform.localScale = new Vector3(20, .2f, 20);
                var source = new GameObject("Fixture");
                source.transform.SetPositionAndRotation(origin + Vector3.up * 4, Quaternion.Euler(90, 0, 0));
                Light lamp = source.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 12;
                lamp.spotAngle = 110;
                lamp.innerSpotAngle = 60;
                lamp.intensity = 10;
                LightFixture fixture = source.AddComponent<LightFixture>();
                fixture.EditorSetup(lamp, null, false, false, 0);
                Camera camera = new GameObject("Eye").AddComponent<Camera>();
                camera.transform.position = origin + new Vector3(0, 1.65f, -2);
                IndirectLighting rig = new GameObject("Bounce").AddComponent<IndirectLighting>();
                rig.EditorSetup(config);
                Physics.SyncTransforms();
                rig.Preview(camera);
                Light bounce = null;
                foreach (Light light in rig.GetComponentsInChildren<Light>()) if (light.enabled) bounce = light;
                Assert.IsNotNull(bounce, "a floor receiver produces a local diffuse bounce");
                Assert.Greater(bounce.intensity, 0);
                fixture.SetPowered(false);
                rig.Preview(camera);
                Assert.IsFalse(bounce.enabled, "a blackout cannot leave baked-looking illumination behind");
                fixture.SetPowered(true);
                rig.Preview(camera);
                Assert.IsTrue(bounce.enabled);
                floor.GetComponent<Collider>().enabled = false;
                Physics.SyncTransforms();
                rig.Preview(camera);
                Assert.IsFalse(bounce.enabled, "a missing floor cannot keep bouncing light");
                yield return null;
            }
            finally
            {
                Object.Destroy(config);
                SceneManager.SetActiveScene(previous);
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        [UnityTest]
        public IEnumerator AQueuedProbeStartsWithAValidFallbackAndAcceptsARefresh()
        {
            var go = new GameObject("Unpublished probe");
            ReflectionProbe probe = go.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            probe.resolution = 16;
            probe.cullingMask = 0;
            probe.enabled = true;
            var fallback = RoomReflections.CreateFallback();
            probe.mode = ReflectionProbeMode.Custom;
            probe.customBakedTexture = fallback;
            try
            {
                Assert.IsNotNull(probe.texture);
                probe.mode = ReflectionProbeMode.Realtime;
                int request = probe.RenderProbe();
                Assert.GreaterOrEqual(request, 0, "Unity must accept the enabled probe's queued render");
                yield return null;
                // Batch mode doesn't service Unity's native probe scheduler (the production manager skips it too).
                // Interactive test runs additionally verify completion; batch verifies the valid fallback and request.
                if (!Application.isBatchMode)
                {
                    for (int i = 0; i < 90 && !probe.IsFinishedRendering(request); i++) yield return null;
                    Assert.IsTrue(probe.IsFinishedRendering(request));
                    Assert.IsNotNull(probe.texture);
                }
            }
            finally { Object.Destroy(go); Object.Destroy(fallback); }
        }
    }
}
