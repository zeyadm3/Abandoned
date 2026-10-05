using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

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

        private static readonly string[] RemovedTemplateAssets = { "Assets/TutorialInfo", "Assets/Readme.asset" };

        [MenuItem("Tools/Abandoned/Verify Project Setup")]
        public static void Verify()
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

            report.Insert(0, failures == 0 ? "[ALL PASS] " : $"[{failures} FAILED] ");
            if (failures == 0) Debug.Log(report.ToString());
            else Debug.LogError(report.ToString());
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
