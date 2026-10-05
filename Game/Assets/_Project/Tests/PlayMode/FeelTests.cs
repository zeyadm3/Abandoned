using System.Collections;
using Abandoned.Core;
using Abandoned.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>Head bob, landing dip, footsteps by surface and camera shake, plus their off switches.</summary>
    public class FeelTests
    {
        private PlayerTestRig rig;
        private PlayerCameraFeel feel;
        private PlayerFootsteps footsteps;
        private FeelSettings settings;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            rig = OnFlatGround(new Vector3(0f, 0.05f, 0f));
            feel = rig.Player.GetComponent<PlayerCameraFeel>();
            footsteps = rig.Player.GetComponent<PlayerFootsteps>();
            feel.enabled = true;
            settings = feel.Settings;
            yield return null;
            rig.Settle();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            rig.Destroy();
            yield return null;
        }

        /// <summary>Simulates movement and ticks feel components in lockstep.</summary>
        private void Run(PlayerInputFrame frame, float seconds, System.Action<int> afterStep = null)
        {
            int steps = Mathf.RoundToInt(seconds / Dt);
            for (int i = 0; i < steps; i++)
            {
                rig.Motor.Simulate(frame, Dt);
                feel.Tick(Dt);
                footsteps.Tick();
                afterStep?.Invoke(i);
            }
        }

        /// <summary>Copies the settings asset with one bool switched, so tests never edit the real asset.</summary>
        private void WithSetting(string property, bool value)
        {
            FeelSettings copy = Object.Instantiate(settings);
#if UNITY_EDITOR
            var so = new SerializedObject(copy);
            so.FindProperty($"<{property}>k__BackingField").boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif
            foreach (MonoBehaviour b in new MonoBehaviour[] { feel, footsteps })
            {
                var sb = new SerializedObject(b);
                sb.FindProperty("settings").objectReferenceValue = copy;
                sb.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        [Test]
        public void NoBobWhenStandingStill()
        {
            Run(Frame(), 1f);
            Assert.Less(feel.BobOffset.magnitude, 1e-4f);
        }

        [Test]
        public void BobsWhileWalking()
        {
            float min = 0f, max = 0f;
            Run(Frame(Vector2.up), 2f, _ => { min = Mathf.Min(min, feel.BobOffset.y); max = Mathf.Max(max, feel.BobOffset.y); });
            Assert.Greater(max - min, settings.BobVertical * 0.5f);
        }

        [Test]
        public void HeadBobCanBeTurnedOff()
        {
            WithSetting("HeadBobEnabled", false);
            float peak = 0f;
            Run(Frame(Vector2.up), 2f, _ => peak = Mathf.Max(peak, feel.BobOffset.magnitude));
            Assert.Less(peak, 1e-4f);
        }

        [Test]
        public void HardLandingDipsThenRecovers()
        {
            rig.Teleport(new Vector3(0f, 2.5f, 0f));
            float deepest = 0f;
            Run(Frame(), 1.5f, _ => deepest = Mathf.Max(deepest, feel.Dip));
            Assert.Greater(deepest, 0.05f, "a 2.5 m drop should dip the camera");
            Assert.LessOrEqual(deepest, settings.MaxDip + 1e-4f);
            Assert.Less(feel.Dip, 0.01f, "recovered");
        }

        [Test]
        public void LandingDipCanBeTurnedOff()
        {
            WithSetting("LandingDipEnabled", false);
            rig.Teleport(new Vector3(0f, 2.5f, 0f));
            float deepest = 0f;
            Run(Frame(), 1.5f, _ => deepest = Mathf.Max(deepest, feel.Dip));
            Assert.AreEqual(0f, deepest);
        }

        [Test]
        public void FootstepCadenceMatchesStepLength()
        {
            Run(Frame(Vector2.up), 3f);
            float walked = rig.Player.transform.position.z;
            int expected = Mathf.FloorToInt(walked / settings.StepLength);
            Assert.That(footsteps.StepCount, Is.InRange(expected - 1, expected + 1));
        }

        [Test]
        public void CrouchedStepsAreQuieterThanSprintSteps()
        {
            Run(Frame(Vector2.up, crouchHeld: true), 2f);
            float crouch = footsteps.LastVolume;
            Run(Frame(Vector2.up, sprint: true), 1.5f);
            float sprint = footsteps.LastVolume;
            Assert.Less(crouch, sprint);
        }

        [Test]
        public void NoFootstepsInTheAir()
        {
            rig.Teleport(new Vector3(0f, 30f, 0f));
            rig.Motor.Simulate(Frame(), Dt);
            int before = footsteps.StepCount;
            for (int i = 0; i < 30; i++)
            {
                rig.Motor.Simulate(Frame(Vector2.up), Dt);
                footsteps.Tick();
            }
            Assert.AreEqual(before, footsteps.StepCount);
        }

        [Test]
        public void HeavyImpactNearbyShakesTheCamera()
        {
            CameraShake.Emit(rig.Player.transform.position + Vector3.forward * 2f, settings.ShakeFullMomentum);
            Assert.Greater(feel.Trauma, 0.5f);
            Run(Frame(), 2f);
            Assert.AreEqual(0f, feel.Trauma, 1e-4f, "shake decays away");
        }

        [Test]
        public void FarOrLightImpactsDoNotShake()
        {
            CameraShake.Emit(rig.Player.transform.position + Vector3.forward * (settings.ShakeRadius + 5f), settings.ShakeFullMomentum);
            CameraShake.Emit(rig.Player.transform.position, settings.ShakeMomentumThreshold * 0.5f);
            Assert.AreEqual(0f, feel.Trauma);
        }

        [Test]
        public void CameraShakeCanBeTurnedOff()
        {
            WithSetting("CameraShakeEnabled", false);
            CameraShake.Emit(rig.Player.transform.position, settings.ShakeFullMomentum);
            Assert.AreEqual(0f, feel.Trauma);
        }
    }
}
