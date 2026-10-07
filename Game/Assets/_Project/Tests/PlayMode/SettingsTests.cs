using System.Collections;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Player;
using Abandoned.UI;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M7.4b: the player's settings take effect live (and are reset after each test).</summary>
    public class SettingsTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GameSettings.ResetAll();
            yield return TestMapScene.Load("HQ");
        }

        [TearDown]
        public void TearDown() => GameSettings.ResetAll();

        [Test]
        public void SensitivityScalesTheLook()
        {
            PlayerLook look = TestMapScene.Player.GetComponent<PlayerLook>();
            float before = look.transform.eulerAngles.y;
            look.ApplyLook(new Vector2(10f, 0f));
            float normal = Mathf.DeltaAngle(before, look.transform.eulerAngles.y);
            GameSettings.Sensitivity = 2f;
            before = look.transform.eulerAngles.y;
            look.ApplyLook(new Vector2(10f, 0f));
            Assert.AreEqual(normal * 2f, Mathf.DeltaAngle(before, look.transform.eulerAngles.y), 1e-3f);
        }

        [Test]
        public void FieldOfViewFollowsTheSetting()
        {
            var cam = TestMapScene.Player.GetComponentInChildren<CinemachineCamera>();
            Assert.AreEqual(GameSettings.DefaultFov, cam.Lens.FieldOfView, 1e-3f);
            GameSettings.FieldOfView = 90f;
            Assert.AreEqual(90f, cam.Lens.FieldOfView, 1e-3f);
        }

        [UnityTest]
        public IEnumerator CameraShakeCanBeSwitchedOff()
        {
            var feel = TestMapScene.Player.GetComponent<PlayerCameraFeel>();
            Vector3 at = TestMapScene.Player.transform.position;
            GameSettings.CameraShake = false;
            CameraShake.Emit(at, 5000f);
            Assert.AreEqual(0f, feel.Trauma, "shake off: nothing");
            GameSettings.CameraShake = true;
            CameraShake.Emit(at, 5000f);
            Assert.Greater(feel.Trauma, 0f, "shake on: the same hit shakes");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SubtitlesCaptionWarningsWithTheirDirection()
        {
            Camera ear = Camera.main;
            Assert.IsNotNull(ear);
            MenuUi menu = MenuUi.Current;
            Vector3 behind = ear.transform.position - ear.transform.forward * 6f;
            GameAudio.Play(SoundId.Creak, behind);
            yield return null;
            Assert.AreEqual(0, menu.Subtitles.Count, "subtitles are off by default");

            GameSettings.Subtitles = true;
            string heard = null;
            void Listen(string s) => heard = s;
            SubtitleFeed.Heard += Listen;
            GameAudio.Play(SoundId.Creak, behind);
            GameAudio.Play(SoundId.LootPickup, behind); // not a warning: no caption
            SubtitleFeed.Heard -= Listen;
            yield return null;
            Assert.AreEqual("[floor creaks - behind]", heard);
            Assert.AreEqual(1, menu.Subtitles.Count);
            GameAudio.Play(SoundId.Creak, ear.transform.position + Vector3.right * 500f);
            yield return null;
            Assert.AreEqual(1, menu.Subtitles.Count, "out of earshot: no caption");
        }
    }
}
