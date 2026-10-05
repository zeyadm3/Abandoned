using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Namespace is EditorTools, not Editor: "Abandoned.Editor" would shadow UnityEditor.Editor
// inside our own code and cause confusing ambiguity errors in custom inspectors.
namespace Abandoned.EditorTools
{
    /// <summary>
    /// One-click sanity check of the project setup rules in CLAUDE.md, so drift
    /// (a stray package, legacy input re-enabled, a missing folder) is caught early.
    /// </summary>
    public static class ProjectSetupVerifier
    {
        private const string Root = "Assets/_Project";

        private static readonly string[] Areas =
        {
            "Core", "Player", "Interaction", "Loot", "Structure", "Networking", "Threats",
            "Extraction", "Company", "Contracts", "Equipment", "Voice", "Audio", "UI"
        };

        private static readonly string[] RemovedPackages = { "com.unity.visualscripting" };

        private static readonly string[] RemovedTemplateAssets =
            { "Assets/TutorialInfo", "Assets/Readme.asset", "Assets/InputSystem_Actions.inputactions" };

        private const string InputAssetPath = Root + "/Data/Core/AbandonedInput.inputactions";
        private static readonly string[] InputMaps = { "Gameplay", "UI", "Debug" };
        private const string ProjectWideActionsKey = "com.unity.input.settings.actions";

        [MenuItem("Tools/Abandoned/Verify Project Setup")]
        public static void Verify()
        {
            int failures = Run(out string report);
            if (failures == 0) Debug.Log(report);
            else Debug.LogError(report);
        }

        /// <summary>Runs every check; returns the failure count and a PASS/FAIL report.</summary>
        public static int Run(out string reportText)
        {
            var report = new StringBuilder("ABANDONED project setup check\n");
            int failures = 0;

            void Check(bool ok, string label)
            {
                report.AppendLine($"{(ok ? "PASS" : "FAIL")}  {label}");
                if (!ok) failures++;
            }

            foreach (string folder in new[] { $"{Root}/Prefabs", $"{Root}/Scenes", $"{Root}/Editor" })
                Check(AssetDatabase.IsValidFolder(folder), $"Folder {folder}");

            foreach (string area in Areas)
            {
                Check(AssetDatabase.IsValidFolder($"{Root}/Scripts/{area}"), $"Folder {Root}/Scripts/{area}");
                Check(AssetDatabase.IsValidFolder($"{Root}/Data/{area}"), $"Folder {Root}/Data/{area}");
            }

            // Look for the .asmdef assets rather than loaded assemblies: Unity skips compiling an
            // assembly that has no scripts yet, so Abandoned.Runtime won't be loaded until 0.2+.
            Check(AsmdefExists("Abandoned.Runtime", $"{Root}/Scripts/"), "Assembly definition Abandoned.Runtime");
            Check(AsmdefExists("Abandoned.Editor", $"{Root}/Editor/"), "Assembly definition Abandoned.Editor");

            // These defines reflect the *active* Player Settings value after the last recompile.
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Check(true, "Active Input Handling = Input System Package only");
#else
            Check(false, "Active Input Handling = Input System Package only (legacy or Both is enabled)");
#endif

            string manifest = File.ReadAllText("Packages/manifest.json");
            foreach (string package in RemovedPackages)
                Check(!manifest.Contains($"\"{package}\""), $"Package {package} removed");

            foreach (string path in RemovedTemplateAssets)
                Check(!AssetDatabase.IsValidFolder(path) && AssetDatabase.LoadMainAssetAtPath(path) == null,
                    $"Template leftover {path} removed");

            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            Check(inputAsset != null, $"Input actions asset {InputAssetPath}");
            foreach (string map in InputMaps)
                Check(inputAsset != null && inputAsset.FindActionMap(map) != null, $"Input map {map}");

            // Looked up by name so this file still compiles if the generated class is missing.
            Check(Type.GetType("Abandoned.Core.AbandonedInput, Abandoned.Runtime") != null,
                "Generated class Abandoned.Core.AbandonedInput");

            // Our code owns its own AbandonedInput instance; a project-wide asset would be a second copy.
            // Checks the key itself: the settings UI shows "None" for a reference to a deleted asset,
            // but the dangling entry stays in EditorBuildSettings until it is removed explicitly.
            Check(!HasProjectWideActionsEntry(), ProjectWideActionsLabel());

            string scene = TestBuildingBuilder.ScenePath;
            Check(File.Exists(scene), $"Scene {scene} (Tools/Abandoned/Create Test Building)");
            Check(Array.Exists(EditorBuildSettings.scenes, s => s.path == scene), $"Scene {scene} in Build Settings");

            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PrefabPath);
            Check(player != null, $"Player prefab {PlayerPrefabBuilder.PrefabPath} (Tools/Abandoned/Create Player Prefab)");
            Check(AssetDatabase.LoadAssetAtPath<Abandoned.Player.PlayerMovementConfig>(PlayerPrefabBuilder.ConfigPath) != null,
                $"Movement config {PlayerPrefabBuilder.ConfigPath}");

            report.Insert(0, failures == 0 ? "[ALL PASS] " : $"[{failures} FAILED] ");
            reportText = report.ToString();
            return failures;
        }

        [MenuItem("Tools/Abandoned/Fix/Clear Project-wide Input Actions")]
        public static void ClearProjectWideActions()
        {
            if (!HasProjectWideActionsEntry())
            {
                Debug.Log("Project-wide Input Actions entry already absent; nothing to clear.");
                return;
            }

            EditorBuildSettings.RemoveConfigObject(ProjectWideActionsKey);
            AssetDatabase.SaveAssets();
            Debug.Log("Removed the Project-wide Input Actions entry from EditorBuildSettings.");
        }

        private static bool HasProjectWideActionsEntry() =>
            Array.IndexOf(EditorBuildSettings.GetConfigObjectNames(), ProjectWideActionsKey) >= 0;

        private static string ProjectWideActionsLabel()
        {
            if (!HasProjectWideActionsEntry()) return "Project-wide Actions = None";

            EditorBuildSettings.TryGetConfigObject(ProjectWideActionsKey, out UnityEngine.Object asset);
            return asset != null
                ? $"Project-wide Actions = None (currently {AssetDatabase.GetAssetPath(asset)}; set it to None)"
                : "Project-wide Actions = None (dangling entry to a deleted asset; run Tools/Abandoned/Fix/Clear Project-wide Input Actions)";
        }

        private static bool AsmdefExists(string assemblyName, string expectedFolder)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{assemblyName} t:AssemblyDefinitionAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == $"{expectedFolder}{assemblyName}.asmdef") return true;
            }
            return false;
        }
    }
}
