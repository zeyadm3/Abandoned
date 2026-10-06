using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The one correct set of platform import settings for the Facepunch.Steamworks binaries in the
    /// embedded transport fork. A wrong tick here fails silently until a player build or another OS:
    /// two Steamworks DLLs in one build, or a missing native library (the arm64 Mac spike failure).
    /// </summary>
    public static class SteamPluginSettings
    {
        public const string Folder = "Packages/com.community.netcode.transport.facepunch/Runtime/Facepunch";

        private sealed class Rule
        {
            public string Path;
            public string EditorOS;
            public string EditorCPU;
            public string PlayerCPU = "AnyCPU";
            public BuildTarget[] Players;
        }

        private static readonly BuildTarget[] DesktopTargets =
            { BuildTarget.StandaloneOSX, BuildTarget.StandaloneWindows64, BuildTarget.StandaloneWindows, BuildTarget.StandaloneLinux64 };

        private static readonly Rule[] Rules =
        {
            new Rule { Path = Folder + "/Facepunch.Steamworks.Posix.dll", EditorOS = "OSX", EditorCPU = "AnyCPU",
                Players = new[] { BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64 } },
            new Rule { Path = Folder + "/Facepunch.Steamworks.Win64.dll", EditorOS = "Windows", EditorCPU = "x86_64",
                Players = new[] { BuildTarget.StandaloneWindows64 } },
            new Rule { Path = Folder + "/redistributable_bin/osx/libsteam_api.dylib", EditorOS = "OSX", EditorCPU = "AnyCPU",
                Players = new[] { BuildTarget.StandaloneOSX } },
            new Rule { Path = Folder + "/redistributable_bin/win64/steam_api64.dll", EditorOS = "Windows", EditorCPU = "x86_64", PlayerCPU = "x86_64",
                Players = new[] { BuildTarget.StandaloneWindows64 } },
            new Rule { Path = Folder + "/redistributable_bin/linux64/libsteam_api.so", EditorOS = "Linux", EditorCPU = "x86_64", PlayerCPU = "x86_64",
                Players = new[] { BuildTarget.StandaloneLinux64 } },
        };

        [MenuItem("Tools/Abandoned/Fix/Apply Steam Plugin Settings")]
        public static void Apply()
        {
            foreach (Rule rule in Rules)
            {
                if (!(AssetImporter.GetAtPath(rule.Path) is PluginImporter importer))
                {
                    UnityEngine.Debug.LogError($"Steam plugin {rule.Path} not found.");
                    continue;
                }
                if (Matches(importer, rule)) continue;

                importer.ClearSettings();
                importer.SetCompatibleWithAnyPlatform(false);
                importer.SetCompatibleWithEditor(true);
                importer.SetEditorData("OS", rule.EditorOS);
                importer.SetEditorData("CPU", rule.EditorCPU);
                foreach (BuildTarget target in DesktopTargets)
                {
                    bool on = System.Array.IndexOf(rule.Players, target) >= 0;
                    importer.SetCompatibleWithPlatform(target, on);
                    importer.SetPlatformData(target, "CPU", on ? rule.PlayerCPU : "None");
                }
                importer.SaveAndReimport();
                UnityEngine.Debug.Log($"Applied Steam plugin settings to {rule.Path}.");
            }
        }

        /// <summary>Every mismatch between the plugins on disk and the rules (empty = correct).</summary>
        public static List<string> Problems()
        {
            var problems = new List<string>();
            foreach (Rule rule in Rules)
            {
                if (!(AssetImporter.GetAtPath(rule.Path) is PluginImporter importer))
                {
                    problems.Add($"{rule.Path} missing");
                    continue;
                }
                if (!Matches(importer, rule)) problems.Add($"{rule.Path} has wrong platform settings");
            }

            // A second copy (old package, Asset Store Steamworks, 32-bit leftovers) would clash at build time.
            foreach (string path in AllSteamBinaries())
                if (System.Array.FindIndex(Rules, r => r.Path == path) < 0)
                    problems.Add($"unexpected Steam binary {path}");
            return problems;
        }

        private static bool Matches(PluginImporter importer, Rule rule)
        {
            if (importer.GetCompatibleWithAnyPlatform()) return false;
            if (!importer.GetCompatibleWithEditor() || importer.GetEditorData("OS") != rule.EditorOS) return false;
            foreach (BuildTarget target in DesktopTargets)
                if (importer.GetCompatibleWithPlatform(target) != (System.Array.IndexOf(rule.Players, target) >= 0)) return false;
            return true;
        }

        private static IEnumerable<string> AllSteamBinaries()
        {
            foreach (string root in new[] { "Assets", "Packages/com.community.netcode.transport.facepunch" })
            {
                if (!Directory.Exists(root)) continue;
                foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileName(file);
                    if (name.EndsWith(".meta")) continue;
                    if (name.StartsWith("Facepunch.Steamworks") || name.StartsWith("steam_api") || name.StartsWith("libsteam_api"))
                        yield return file.Replace('\\', '/');
                }
            }
        }
    }
}
