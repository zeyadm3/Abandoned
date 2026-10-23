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
        private static readonly string[] AllMapScenes =
        {
            HqBuilder.ScenePath, MallBuilder.ScenePath, SchoolBuilder.ScenePath, FactoryBuilder.ScenePath,
            PrisonBuilder.ScenePath, TutorialHouseLayout.ScenePath,
            Abandoned.Hospital.HospitalBuilder.ScenePath, Abandoned.Motel.MotelBuilder.ScenePath, SkyscraperBuilder.ScenePath,
            PoliceStationLayout.ScenePath, FireStationLayout.ScenePath,
        };

        /// <summary>The open-world places built by the map agents on 2026-10-23 (Docs/AGENT_REVIEW.md).</summary>
        public static readonly string[] NewMapScenes =
        {
            Abandoned.Hospital.HospitalBuilder.ScenePath, Abandoned.Motel.MotelBuilder.ScenePath, SkyscraperBuilder.ScenePath,
            PoliceStationLayout.ScenePath, FireStationLayout.ScenePath,
        };

        private static string[] scope;

        /// <summary>Every map the passes work on: only scenes saved on disk (a map not built yet is skipped, not an
        /// error), and only <see cref="NewMapScenes"/> while <see cref="AfterNewMaps"/> runs.</summary>
        public static string[] MapScenes => Array.FindAll(scope ?? AllMapScenes, System.IO.File.Exists);

        /// <summary>True while a pass is limited to some maps (the remaster then writes its own report).</summary>
        public static bool Scoped => scope != null;

        private static readonly List<string> failures = new();

        [MenuItem(ToolMenu.Art + "0.24.12 Look, Surfaces, Sound and Decals", priority = ToolMenu.ArtP + 65)]
        public static void AfterMapsMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AfterMaps();
        }

        /// <summary>Render settings, each map's look, surfaces, sound and wear decals; since 0.24.13 also each map's
        /// weathering and the coplanar-face fix (Docs/GRAPHICS_0.24.13.md).</summary>
        public static void AfterMaps()
        {
            Step("Render pipeline", RenderPipelineSetup.Apply);
            Step("Map looks", AvLook.ApplyToSaved);
            Step("Scanned surfaces", MaterialUpgrade.Run);
            Step("Decal set", DecalAssets.MakeAll);
            Step("Sound bank", SoundBankBuilder.CreateMissing);
            Step("Recorded library", SoundLibrary.Build);
            foreach (string path in MapScenes)
                Step($"Weathering, wear decals and coplanar faces: {System.IO.Path.GetFileNameWithoutExtension(path)}", () =>
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    float weather = SurfaceAging.Place(scene);
                    AmbientBalance.Apply(scene);
                    int probes = RoomReflectionProbes.Place(scene);
                    int n = DecalScatter.Scatter(scene.name);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    // 0.24.13: last, so it sees everything the map now draws; saves the scene itself.
                    var (moved, faces, left) = CoplanarFix.FixOpenScene();
                    Debug.Log($"[AvOverhaul] {scene.name}: weathering {weather:0.00}, {probes} room reflections, {n} wear decals, " +
                              $"coplanar faces: {moved} moved, {faces} lifted, {left} left.");
                });
            AssetDatabase.SaveAssets();
            Step("Map art, object geometry, materials and effects", VisualRemaster.ApplySaved);
            // 2026-10-22: last, on top of the remaster: each map's sky, scanned trees, grass and outdoor sound.
            Step("Map sites: sky, trees, grass and sound", MapSite.ApplySaved);
        }

        [MenuItem(ToolMenu.Art + "Upgrade New Maps (Hospital, Motel, Skyscraper, Stations)", priority = ToolMenu.ArtP + 66)]
        public static void AfterNewMapsMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AfterNewMaps();
        }

        /// <summary>
        /// <see cref="AfterMaps"/> on the five new places only, after their builders have run: look, scanned surfaces,
        /// weathering, reflections, decals, coplanar faces, the remaster, lighting and the map site (sky, trees, grass,
        /// outdoor sound). The six older maps are left as saved.
        /// </summary>
        public static void AfterNewMaps()
        {
            scope = NewMapScenes;
            try { AfterMaps(); }
            finally { scope = null; }
        }

        /// <summary>For Rebuild New Maps and World: <see cref="AfterNewMaps"/>, throwing if any of its steps failed.</summary>
        public static void AfterNewMapsOrThrow()
        {
            int before = failures.Count;
            AfterNewMaps();
            if (failures.Count > before)
                throw new InvalidOperationException("New-map steps failed: " + string.Join(", ", failures.GetRange(before, failures.Count - before)));
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
