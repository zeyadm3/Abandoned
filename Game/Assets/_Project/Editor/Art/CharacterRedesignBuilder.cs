using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Abandoned.Threats;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Abandoned.EditorTools
{
    /// <summary>Character-only authoring: exports the original monster rigs and updates existing visual prefabs.</summary>
    public static class CharacterRedesignBuilder
    {
        private static readonly string[] Monsters = { "BlindOne", "Stalker", "Collector", "Hunter", "Weight", "Thing", "Crawlers", "LastHunter" };
        private static readonly ConcurrentQueue<string> Output = new();
        private static Process exporter;
        private static bool refreshHeld;

        [MenuItem("Tools/Abandoned/Characters/Generate and Apply Full Redesign")]
        public static void GenerateAndApply()
        {
            RequireEditMode();
            if (exporter != null) return;
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string blender = Environment.GetEnvironmentVariable("BLENDER");
            if (string.IsNullOrWhiteSpace(blender)) blender = EditorPrefs.GetString("Abandoned.BlenderExecutable", "/Applications/Blender.app/Contents/MacOS/Blender");
            if (!File.Exists(blender))
            {
                blender = EditorUtility.OpenFilePanel("Select the Blender executable", "/Applications", "");
                if (string.IsNullOrEmpty(blender)) return;
                EditorPrefs.SetString("Abandoned.BlenderExecutable", blender);
            }
            CharacterMaterialBuilder.Prepare();
            string script = Path.Combine(repository, "Tools/Blender/threats.py");
            var start = new ProcessStartInfo
            {
                FileName = blender,
                Arguments = "--background --python-exit-code 1 --python " + Quote(script) + " -- " + Quote(repository),
                WorkingDirectory = repository,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            try
            {
                // Partial FBX exports must not be imported while Blender is still writing the set.
                AssetDatabase.DisallowAutoRefresh();
                refreshHeld = true;
                exporter = new Process { StartInfo = start };
                exporter.OutputDataReceived += Capture;
                exporter.ErrorDataReceived += Capture;
                exporter.Start();
                exporter.BeginOutputReadLine();
                exporter.BeginErrorReadLine();
                EditorApplication.update += ExportTick;
                AssemblyReloadEvents.beforeAssemblyReload += ReleaseExporter;
                Debug.Log("[CharacterArt] Generating the eight redesigned monster models. The character prefabs will be authored when export finishes.");
            }
            catch
            {
                ReleaseExporter();
                throw;
            }
        }

        [MenuItem("Tools/Abandoned/Characters/Apply Redesign from Exported Models")]
        public static void ApplyExported()
        {
            RequireEditMode();
            CharacterMaterialBuilder.Prepare();
            foreach (string name in Monsters)
                AssetDatabase.ImportAsset($"{ThreatRigBuilder.ArtFolder}/{name}.fbx", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            foreach (string name in Monsters)
            {
                string path = $"{ThreatContentBuilder.Folder}/{name}.prefab";
                var definition = AssetDatabase.LoadAssetAtPath<ThreatDefinition>($"{ThreatContentBuilder.DataFolder}/{name}Definition.asset");
                if (definition == null) throw new InvalidOperationException("Create the base threat content before applying character art: " + name);
                // Upgrade only the visual/animation assembly: existing speed, damage, AI config and catalog order stay authored in place.
                GameObject prefab = ThreatRigBuilder.Upgrade(path, name, definition);
                NetworkObjectIds.StampPrefab(prefab);
            }
            WardrobeArtDetails.UpgradeSaved();
            PlayerPrefabBuilder.Create();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterArt] Authored monster, worker, wardrobe and first-person resources. No scenes, gameplay checks or review renders were run.");
        }

        private static void ExportTick()
        {
            while (Output.TryDequeue(out string line)) Debug.Log("[CharacterArt/Blender] " + line);
            if (exporter == null || !exporter.HasExited) return;
            int code = exporter.ExitCode;
            ReleaseExporter();
            if (code != 0)
            {
                Debug.LogError("[CharacterArt] Blender export stopped with code " + code + ". Character prefab authoring was not started; see the Blender output above.");
                return;
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            try { ApplyExported(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private static void Capture(object sender, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.Data)) Output.Enqueue(args.Data);
        }

        private static void ReleaseExporter()
        {
            EditorApplication.update -= ExportTick;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleaseExporter;
            // A domain reload detaches the UI; leave an already-running authoring export intact.
            exporter?.Dispose();
            exporter = null;
            if (refreshHeld) { refreshHeld = false; AssetDatabase.AllowAutoRefresh(); }
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Apply character art in Edit mode.");
        }

        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
