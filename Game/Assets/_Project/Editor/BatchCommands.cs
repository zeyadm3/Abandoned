using System;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Entry points for Unity batch mode (-executeMethod), used by Tools/unity.sh so the project
    /// can be rebuilt, validated and screenshotted without opening the editor.
    /// Each method exits Unity with 0 on success and 1 on failure.
    /// </summary>
    public static class BatchCommands
    {
        /// <summary>Regenerates every Editor-built asset in dependency order.</summary>
        public static void RebuildContent() => RunAndExit(() =>
        {
            ProjectLayersSetup.Apply();
            LootCatalogBuilder.CreateMissing();
            LootPrefabGenerator.GenerateAll();
            PlayerPrefabBuilder.Create();
            TestBuildingBuilder.Build();
            return true;
        });

        public static void VerifyAll() => RunAndExit(() =>
        {
            int setupFailures = ProjectSetupVerifier.Run(out string report);
            Log(report, setupFailures == 0);
            List<string> contentErrors = ContentValidator.Run();
            ContentValidator.Report(contentErrors);
            return setupFailures == 0 && contentErrors.Count == 0;
        });

        public static void Screenshots() => RunAndExit(() =>
        {
            EditorSceneManager.OpenScene(TestBuildingBuilder.ScenePath, OpenSceneMode.Single);
            var shots = new List<string>
            {
                ScreenshotCapture.CaptureFrom(new Vector3(10f, 10f, -24f), new Vector3(10f, 2f, 8f), "M1_overview"),
                ScreenshotCapture.CaptureFrom(new Vector3(-6f, 18f, 8f), new Vector3(10f, 0f, 8f), "M1_top"),
                ScreenshotCapture.CaptureFrom(new Vector3(2f, 5.6f, 2f), new Vector3(10f, 2f, 10f), "M1_balcony_view"),
                ScreenshotCapture.CaptureFrom(new Vector3(18f, 1.6f, 1f), new Vector3(18f, 3f, 10f), "M1_stairs"),
            };

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
            if (player != null)
            {
                Transform eye = player.transform.Find("CameraRoot");
                shots.Add(ScreenshotCapture.CaptureFrom(eye.position, eye.position + player.transform.forward * 10f, "M1_player_eye", 75f));
            }

            Debug.Log("Screenshots written:\n" + string.Join("\n", shots));
            return true;
        });

        private static void RunAndExit(Func<bool> command)
        {
            bool ok;
            try
            {
                ok = command();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ok = false;
            }
            EditorApplication.Exit(ok ? 0 : 1);
        }

        private static void Log(string message, bool ok)
        {
            if (ok) Debug.Log(message);
            else Debug.LogError(message);
        }
    }
}
