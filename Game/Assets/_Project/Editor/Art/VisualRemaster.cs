using System;
using System.IO;
using System.Linq;
using System.Text;
using Abandoned.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Complete repeatable asset and scene art pass. Uses Editor APIs to keep asset GUIDs and game state.</summary>
    public static class VisualRemaster
    {
        private const string DiagnosticsName = "_ArtRemaster_Diagnostics";

        [MenuItem(ToolMenu.Art + "Remaster All Map Art and Objects", priority = ToolMenu.ArtP + 82)]
        public static void ApplyMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try { ApplySaved(); }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        [MenuItem(ToolMenu.Art + "Remaster All Map Art and Objects", true)]
        private static bool CanApply() => !EditorApplication.isPlayingOrWillChangePlaymode;

        public static void ApplyAndExit()
        {
            try { ApplySaved(); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void PruneAndExit()
        {
            try { Debug.Log($"[VisualRemaster] Removed {ObjectRemaster.PruneUnusedCache()} unreferenced generated mesh assets."); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void RestoreVehicleAndExit()
        {
            try { CollectionTruckMaterials.Prepare(); AssetDatabase.SaveAssets(); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void ApplySaved()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Apply the art remaster in Edit Mode.");
            var report = new StringBuilder("ABANDONED visual remaster\n");
            // The former material pass can still be run by old map builders; the remaster must always finish last.
            LightingRebuild.ApplySaved();
            CollectionTruckMaterials.Prepare();
            MaterialRemaster.Run();
            EnvironmentRemaster.ResetSession();
            int prefabs = RemasterPrefabs(report);
            foreach (string path in AvOverhaul.MapScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                ObjectRemaster.Result objects = ObjectRemaster.ApplyToScene(scene);
                EnvironmentRemaster.Result environment = EnvironmentRemaster.ApplyToScene(scene);
                int effects = VisualEffectsRemaster.ApplyToScene(scene);
                int refined = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MeshFilter>(true))
                    .Count(filter => filter.sharedMesh != null && AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith(ObjectRemaster.Folder + "/Prop_", StringComparison.Ordinal));
                var diagnostics = scene.GetRootGameObjects().FirstOrDefault(go => go.name == DiagnosticsName);
                if (diagnostics == null)
                {
                    diagnostics = new GameObject(DiagnosticsName);
                    SceneManager.MoveGameObjectToScene(diagnostics, scene);
                }
                string summary = $"{refined} model renderers refined · {environment.Junctions} fitted junctions · {effects} particle emitters";
                (Abandoned.EditorTools.SerializedWiring.GetOrAdd<ArtRemasterDiagnostics>(diagnostics)).EditorSetup(summary);
                EditorSceneManager.MarkSceneDirty(scene);
                AssetDatabase.SaveAssets();
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Couldn't save remastered scene " + path);
                string line = $"{scene.name}: {refined} refined prop renderers present; changes this pass: {objects}; {environment}; {effects} particle emitters";
                report.AppendLine(line);
                Debug.Log("[VisualRemaster] " + line);
            }
            report.AppendLine($"Prefabs updated: {prefabs}");
            report.AppendLine($"Unused generated meshes removed: {ObjectRemaster.PruneUnusedCache()}");
            string reportFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/renders/art-remaster"));
            Directory.CreateDirectory(reportFolder);
            File.WriteAllText(Path.Combine(reportFolder, AvOverhaul.Scoped ? "AuthoringReport-NewMaps.txt" : "AuthoringReport.txt"), report.ToString());
            AssetDatabase.SaveAssets();
            Debug.Log($"[VisualRemaster] Saved {AvOverhaul.MapScenes.Length} maps, material assets and object prefabs. Report: Docs/renders/art-remaster/{(AvOverhaul.Scoped ? "AuthoringReport-NewMaps.txt" : "AuthoringReport.txt")}");
        }

        private static int RemasterPrefabs(StringBuilder report)
        {
            int changed = 0;
            string[] folders = { "Assets/_Project/Prefabs/Loot", "Assets/_Project/Prefabs/Equipment",
                "Assets/_Project/Prefabs/HQ", "Assets/_Project/Prefabs/Meridian", "Assets/_Project/Prefabs/SalvageDressing",
                // The open-world places keep their loot and fittings in their own art folders (2026-10-23).
                "Assets/_Project/Art/Hospital", "Assets/_Project/Art/Motel", "Assets/_Project/Art/Skyscraper",
                "Assets/_Project/Art/PoliceStation", "Assets/_Project/Art/FireStation" };
            folders = Array.FindAll(folders, AssetDatabase.IsValidFolder);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    ObjectRemaster.Result result = ObjectRemaster.ApplyToRoot(root.transform);
                    int effects = VisualEffectsRemaster.ApplyToRoot(root.transform);
                    if (result.ChangedRendererCount == 0 && effects == 0) continue;
                    PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
                    if (!success) throw new InvalidOperationException("Couldn't save remastered prefab " + path);
                    report.AppendLine($"Prefab {path}: {result}; {effects} particle emitters");
                    changed++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return changed;
        }
    }
}
