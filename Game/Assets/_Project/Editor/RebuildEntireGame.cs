using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The top of Tools > Abandoned: every Editor-built thing in dependency order. Project settings, then data and
    /// configs, loot, characters and prefabs, sound and UI assets, the network prefab list and the tutorial lessons,
    /// then every map (the same steps as Rebuild All Maps), their look and lighting, and last the loot icons.
    /// Data builders only create what is missing, so tuned assets keep their values. One-off release steps
    /// (Content/Features, map Updates) and the character redesign (needs Blender) are not repeated here.
    /// </summary>
    public static class RebuildEntireGame
    {
        private const string Title = "Rebuild Entire Game";

        [MenuItem(ToolMenu.Root + "Rebuild Entire Game", priority = ToolMenu.RebuildGameP)]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(Title, "Leave Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Same HQ choice as Rebuild All Maps: regenerating the HQ loses hand edits made in its scene.
            int choice = EditorUtility.DisplayDialogComplex(Title,
                "Rebuilds everything the editor builds: settings, data, loot, prefabs, sound, UI, tutorial lessons, " +
                "every map, the look and lighting, and the loot icons. This takes a long time and Unity will look " +
                "frozen between steps.\n\n" +
                "The HQ: \"Keep my HQ\" only adds the training clipboard to the saved HQ. " +
                "\"Rebuild HQ too\" regenerates it from code and loses any hand edits made in the HQ scene.",
                "Keep my HQ", "Cancel", "Rebuild HQ too");
            if (choice == 1) return;

            var steps = ContentSteps();
            steps.AddRange(RebuildAllMaps.MapSteps(rebuildHq: choice == 2));
            steps.Add(("Generated material repair", GeneratedMaterialRepair.Rebuild));
            steps.Add(RebuildAllMaps.AfterMapsStep);
            steps.Add(RebuildAllMaps.OpenWorldStep);
            steps.Add(("Loot icons", LootIcons));
            RebuildAllMaps.RunSteps(Title, steps);
        }

        // Order matters: cosmetics before the player prefab (it references the catalog), every prefab before the
        // network prefab list, materials remapped before the maps so their models already use URP copies.
        private static List<(string name, Action run)> ContentSteps() => new()
        {
            ("Project settings", () =>
            {
                ProjectLayersSetup.Apply();
                BuildScript.ApplyPlayerSettings();
                SteamPluginSettings.Apply();
                RenderPipelineSetup.Apply();
                BuildInfoAsset.CreateMissing();
            }),
            ("Data and configs", () =>
            {
                NetworkContentBuilder.CreateMissing();
                AchievementsBuilder.CreateMissing();
                StructureContentBuilder.CreateMissing();
                HealthContentBuilder.Create();
                EquipmentContentBuilder.CreateMissing();
                Remake.GearModelBuilder.Build();
                CompanyContentBuilder.CreateMissing();
                RunPressureContentBuilder.CreateMissing();
                RoomsContentBuilder.CreateMissing();
                Release02410.Author();
            }),
            ("Loot", () =>
            {
                CustomMallArt.Prepare();
                LootCatalogBuilder.CreateMissing();
                LootModelBuilder.AssignMissing();
                LootPrefabGenerator.GenerateAll();
            }),
            ("Cosmetics", () =>
            {
                CosmeticsBuilder.CreateMissing();
                Remake.CrewLookBuilder.Build();
            }),
            ("Prefabs", () =>
            {
                PlayerPrefabBuilder.Create();
                NetworkContentBuilder.CreateStructureNetPrefab();
                NetworkContentBuilder.CreateRunStatePrefab();
                NetworkContentBuilder.CreateSessionTravelPrefab();
                ThreatContentBuilder.CreateBlindOne();
                ThreatContentBuilder.CreateOthers();
                NewThreatContentBuilder.Create();
                Remake.MonsterInstaller.Install();
                HorrorContentBuilder.Create();
                GarageDoorBuilder.Create();
            }),
            ("Sound", () =>
            {
                SoundBankBuilder.CreateMissing();
                SoundLibrary.Build();
            }),
            ("UI assets", () =>
            {
                UiHorrorTextureBuilder.Create();
                UiContentBuilder.CreateMissing();
                MenuEffectsBuilder.Create();
                UiGearIconBuilder.Create();
            }),
            ("Materials and network prefab list", () =>
            {
                MaterialAudit.RemapThirdPartyMaterials();
                NetworkContentBuilder.RegisterNetworkPrefabs();
            }),
            ("Tutorial lessons", TutorialContentBuilder.Build),
        };

        // Renders only icons that are missing (it opens its own empty scene), then the icon registry picks them up.
        private static void LootIcons()
        {
            if (LootIconRenderer.Render(force: false) > 0) UiContentBuilder.BuildIcons();
            AssetDatabase.SaveAssets();
        }
    }
}
