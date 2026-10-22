using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// One menu item for every map's own rebuild, in the order they are safe to run: the HQ goes before the
    /// tutorial's clipboard step so the clipboard always ends up on the bench. Each map still runs exactly the
    /// code its own menu runs, so nothing here can drift from the per-map rebuilds.
    /// </summary>
    public static class RebuildAllMaps
    {
        [MenuItem(ToolMenu.Root + "Rebuild All Maps", priority = ToolMenu.RebuildAllP)]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Rebuild All Maps", "Leave Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // The HQ is opt-in: a full HQ regeneration discards hand edits made in Scenes/HQ.unity.
            int choice = EditorUtility.DisplayDialogComplex("Rebuild All Maps",
                "Rebuilds the Mall, School, Iron Factory, Island Prison and Tutorial House, one after another. " +
                "Also rebuilds Saint Brigid Hospital and Last Light Motor Lodge. " +
                "This takes a while and Unity will look frozen between maps.\n\n" +
                "The HQ: \"Keep my HQ\" only adds the training clipboard to the saved HQ. " +
                "\"Rebuild HQ too\" regenerates it from code and loses any hand edits made in the HQ scene.",
                "Keep my HQ", "Cancel", "Rebuild HQ too");
            if (choice == 1) return;
            bool rebuildHq = choice == 2;

            var steps = new List<(string name, Action run)>
            {
                ("Mall and Parking", MeridianMallOverhaul.Author),
                ("Jackson P Burley Middle School", BurleySchoolBuilder.Build),
                ("Iron Factory", IronFactoryBuilder.Build),
                ("Island Prison", IslandPrisonBuilder.Build),
                ("Saint Brigid Hospital", Abandoned.Hospital.HospitalBuilder.Build),
                ("Last Light Motor Lodge", Abandoned.Motel.MotelBuilder.Build),
                ("Skyscraper Restaurant", SkyscraperBuilder.Build),
                ("Millbrook 3rd Precinct", PoliceStationBuilder.Build),
                ("Millbrook Fire Station No. 4", FireStationBuilder.Build),
            };
            if (rebuildHq) steps.Add(("HQ (Salvage Hub)", HqBuilder.AuthorSalvageHub));
            steps.Add(("Tutorial House", TutorialHouseBuilder.Build));
            steps.Add(("Training clipboard in the HQ", TutorialHqClipboard.AddToSavedHq));
            // 0.24.12: the builders recreate their materials, so the scanned surfaces, looks, sound and decals follow them.
            steps.Add(("0.24.12 look, surfaces, sound and decals", AvOverhaul.AfterMapsOrThrow));

            var failed = new List<string>();
            try
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Rebuild All Maps", $"{steps[i].name} ({i + 1}/{steps.Count})", (float)i / steps.Count);
                    try
                    {
                        steps[i].run();
                        Debug.Log($"[RebuildAllMaps] {steps[i].name}: done.");
                    }
                    catch (Exception e)
                    {
                        failed.Add(steps[i].name);
                        Debug.LogError($"[RebuildAllMaps] {steps[i].name} failed; its saved scene is unchanged.");
                        Debug.LogException(e);
                        // Drop the half-built scene unsaved, or the next map's save prompt would offer to keep it.
                        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    }
                }
                BuildScenes.ApplyToEditorSettings();
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            string summary = failed.Count == 0
                ? $"All {steps.Count} steps finished."
                : $"{steps.Count - failed.Count} of {steps.Count} steps finished. Failed (see the Console):\n- " + string.Join("\n- ", failed);
            Debug.Log("[RebuildAllMaps] " + summary.Replace("\n", " "));
            EditorUtility.DisplayDialog("Rebuild All Maps", summary, "OK");
        }
    }
}
