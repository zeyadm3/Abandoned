using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The 0.24.12 sound and picture overhaul in one place. <see cref="AfterMaps"/> is what Rebuild All Maps runs last
    /// (the map builders recreate their materials, so the upgrade has to follow them); <see cref="AuthorAll"/> is the
    /// batch entry (Tools/unity.sh exec Abandoned.EditorTools.AvOverhaul.AuthorAll): rebuild the maps (the HQ is kept,
    /// never regenerated), then the look, surfaces, sound and decals. Docs/AV_OVERHAUL_0.24.12.md.
    /// </summary>
    public static class AvOverhaul
    {
        public static readonly string[] MapScenes =
        {
            HqBuilder.ScenePath, MallBuilder.ScenePath, SchoolBuilder.ScenePath, FactoryBuilder.ScenePath,
            PrisonBuilder.ScenePath, TutorialHouseLayout.ScenePath,
        };

        private static readonly List<string> failures = new();

        [MenuItem(ToolMenu.Art + "0.24.12 Look, Surfaces, Sound and Decals", priority = ToolMenu.ArtP + 65)]
        public static void AfterMapsMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AfterMaps();
        }

        /// <summary>Render settings, each map's look, surfaces, sound and wear decals.</summary>
        public static void AfterMaps()
        {
            Step("Render pipeline", RenderPipelineSetup.Apply);
            Step("Map looks", AvLook.ApplyToSaved);
            Step("Scanned surfaces", MaterialUpgrade.Run);
            Step("Decal set", DecalAssets.MakeAll);
            Step("Sound bank", SoundBankBuilder.CreateMissing);
            Step("Recorded library", SoundLibrary.Build);
            foreach (string path in MapScenes)
                Step($"Wear decals: {System.IO.Path.GetFileNameWithoutExtension(path)}", () =>
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    int n = DecalScatter.Scatter(scene.name);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[AvOverhaul] {scene.name}: {n} wear decals.");
                });
            AssetDatabase.SaveAssets();
        }

        /// <summary>For Rebuild All Maps: <see cref="AfterMaps"/>, throwing if any of its steps failed so the rebuild
        /// reports it instead of "All steps finished".</summary>
        public static void AfterMapsOrThrow()
        {
            int before = failures.Count;
            AfterMaps();
            if (failures.Count > before)
                throw new InvalidOperationException("0.24.12 steps failed: " + string.Join(", ", failures.GetRange(before, failures.Count - before)));
        }

        /// <summary>Batch: <see cref="AfterMaps"/> on the saved maps (no rebuild), then exit.</summary>
        public static void AfterMapsAndExit()
        {
            failures.Clear();
            AfterMaps();
            Exit();
        }

        /// <summary>Batch: the render pipeline, the looks and the surfaces again (no scene changes), then exit.</summary>
        public static void RefreshAndExit()
        {
            failures.Clear();
            Step("Render pipeline", RenderPipelineSetup.Apply);
            Step("Map looks", AvLook.ApplyToSaved);
            Step("Scanned surfaces", MaterialUpgrade.Run);
            Exit();
        }

        /// <summary>Batch: rebuild the maps, then the whole overhaul, then exit (1 if any step failed).</summary>
        public static void AuthorAll()
        {
            failures.Clear();
            var maps = new List<(string, Action)>
            {
                ("Mall and Parking", MeridianMallOverhaul.Author),
                ("School", BurleySchoolBuilder.Build),
                ("Iron Factory", IronFactoryBuilder.Build),
                ("Island Prison", IslandPrisonBuilder.Build),
                ("Tutorial House", TutorialHouseBuilder.Build),
                ("Training clipboard in the HQ", TutorialHqClipboard.AddToSavedHq),
            };
            foreach (var (name, run) in maps) Step(name, run);
            BuildScenes.ApplyToEditorSettings();
            AfterMaps();
            Exit();
        }

        private static void Exit()
        {
            Debug.Log(failures.Count == 0 ? "[AvOverhaul] All steps finished." : "[AvOverhaul] Failed: " + string.Join(", ", failures));
            if (Application.isBatchMode) EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
        }

        private static void Step(string name, Action run)
        {
            try
            {
                run();
                Debug.Log($"[AvOverhaul] {name}: done.");
            }
            catch (Exception e)
            {
                failures.Add(name);
                Debug.LogError($"[AvOverhaul] {name} failed.");
                Debug.LogException(e);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }
}
