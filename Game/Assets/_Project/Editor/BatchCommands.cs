using System;
using System.Collections.Generic;
using System.Linq;
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
            BuildScript.ApplyPlayerSettings();
            SteamPluginSettings.Apply();
            NetworkContentBuilder.CreateMissing();
            BuildInfoAsset.CreateMissing();
            LootCatalogBuilder.CreateMissing();
            LootPrefabGenerator.GenerateAll();
            StructureContentBuilder.CreateMissing();
            PlayerPrefabBuilder.Create();
            NetworkContentBuilder.CreateStructureNetPrefab();
            NetworkContentBuilder.CreateRunStatePrefab();
            NetworkContentBuilder.CreateSessionTravelPrefab();
            ThreatContentBuilder.CreateBlindOne();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            TestBuildingBuilder.Build();
            MallBuilder.Build();
            BuildScenes.ApplyToEditorSettings();
            return true;
        });

        public static void BuildMac() => RunAndExit(() => BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Shareable));
        public static void BuildWindows() => RunAndExit(() => BuildScript.Build(BuildPlatform.Windows, BuildFlavor.Shareable));
        public static void BuildBoth() => RunAndExit(BuildScript.BuildBoth);
        public static void BuildMacDev() => RunAndExit(() => BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Dev));

        public static void VerifyAll() => RunAndExit(() =>
        {
            int setupFailures = ProjectSetupVerifier.Run(out string report);
            Log(report, setupFailures == 0);
            List<string> contentErrors = ContentValidator.Run();
            ContentValidator.Report(contentErrors);
            return setupFailures == 0 && contentErrors.Count == 0;
        });

        private const float EyeHeight = 1.6f;

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

            // Players are spawned by NGO at runtime; the host's spawn point shows what they first see.
            PlayerSpawnPoint spawn = UnityEngine.Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None)
                .OrderBy(s => s.Index).FirstOrDefault();
            if (spawn != null)
            {
                Vector3 eye = spawn.transform.position + Vector3.up * EyeHeight;
                shots.Add(ScreenshotCapture.CaptureFrom(eye, eye + spawn.transform.forward * 10f, "M1_player_eye", 75f));
            }

            EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            shots.Add(ScreenshotCapture.CaptureFrom(new Vector3(24f, 34f, -34f), new Vector3(24f, 4f, 20f), "M5_mall_overview"));
            shots.Add(ScreenshotCapture.CaptureFrom(new Vector3(24f, 1.6f, -6f), new Vector3(24f, 3f, 20f), "M5_mall_entrance"));
            shots.Add(ScreenshotCapture.CaptureFrom(new Vector3(13f, 5.6f, 9f), new Vector3(26f, 5f, 22f), "M5_mall_walkway1"));
            shots.Add(ScreenshotCapture.CaptureFrom(new Vector3(20f, 1.6f, 6f), new Vector3(22f, 6f, 24f), "M5_mall_atrium_up"));
            shots.Add(ScreenshotCapture.CaptureFrom(new Vector3(-20f, 40f, 20f), new Vector3(24f, 0f, 20f), "M5_mall_side"));

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
