using System.Collections;
using Abandoned.Player;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>Verifies first-person and third-person perspective switching (F5), camera boom tracking, and visual presentation.</summary>
    public class PerspectiveTests
    {
        private PlayerTestRig rig;
        private PlayerPerspective perspective;
        private WorkerPresentation presentation;
        private CinemachineCamera cinemachine;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return NetTestHarness.CleanWorld();
            rig = OnFlatGround(new Vector3(0f, 0.05f, 0f));
            perspective = rig.Player.GetComponent<PlayerPerspective>();
            presentation = rig.Player.GetComponent<WorkerPresentation>();
            cinemachine = rig.Player.GetComponentInChildren<CinemachineCamera>(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (perspective != null) perspective.SetThirdPerson(false);
            rig.Destroy();
            yield return null;
        }

        [Test]
        public void PlayerPrefabCarriesPerspectiveComponent()
        {
            Assert.IsNotNull(perspective, "Player prefab must carry PlayerPerspective component.");
        }

        [Test]
        public void StartsInFirstPerson()
        {
            Assert.IsFalse(PlayerPerspective.IsThirdPerson, "Should start in first-person view.");
            Assert.IsFalse(presentation.ThirdPerson, "WorkerPresentation should start in first-person mode.");
            Transform eye = rig.Player.transform.Find("CameraRoot/Eye");
            Assert.AreEqual(eye, cinemachine.Target.TrackingTarget, "Cinemachine camera should track Eye in first-person view.");
        }

        [UnityTest]
        public IEnumerator TogglePerspectiveSwitchesToThirdPersonAndBack()
        {
            perspective.Toggle();
            yield return null;

            Assert.IsTrue(PlayerPerspective.IsThirdPerson, "Should be in third-person view after toggle.");
            Assert.IsTrue(presentation.ThirdPerson, "WorkerPresentation should be in third-person mode.");
            Transform cameraTarget = rig.Player.transform.Find("CameraRoot/CameraTarget");
            Assert.IsNotNull(cameraTarget, "CameraTarget should exist under CameraRoot.");
            Assert.AreEqual(cameraTarget, cinemachine.Target.TrackingTarget, "Cinemachine camera should track CameraTarget in third-person.");

            perspective.Toggle();
            yield return null;

            Assert.IsFalse(PlayerPerspective.IsThirdPerson, "Should be back in first-person view after second toggle.");
            Assert.IsFalse(presentation.ThirdPerson, "WorkerPresentation should return to first-person mode.");
            Transform eye = rig.Player.transform.Find("CameraRoot/Eye");
            Assert.AreEqual(eye, cinemachine.Target.TrackingTarget, "Cinemachine camera should track Eye again in first-person.");
        }
    }
}
