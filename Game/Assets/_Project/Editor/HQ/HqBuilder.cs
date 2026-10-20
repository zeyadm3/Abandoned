using System.Collections.Generic;
using System.Linq;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>A working salvage warehouse with dispatch, processing and maintenance stations.</summary>
    public static class HqBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/HQ.unity";

        [MenuItem(ToolMenu.Hq + "Rebuild Salvage Hub and Run Audits", priority = ToolMenu.HqP + 1)]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
            => BuildScene(runAudits: true);

        [MenuItem(ToolMenu.Hq + "Rebuild Salvage Hub", priority = ToolMenu.HqP)]
        public static void RebuildSalvageHub()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildScene(runAudits: false);
        }

        /// <summary>Authoring only: generate the HQ without running the validation passes.</summary>
        public static void AuthorSalvageHub() => BuildScene(runAudits: false);

        private static void BuildScene(bool runAudits)
        {
            HqDispatchAudioBuilder.Build();
            CustomMallArt.Prepare();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject("HQ").transform;
            HqIndustrialShell.Build(root);
            HqSalvageStations.Build(root);
            HqDressingBuilder.Build(root);
            HqWarehouseMachinery.Build(root);
            HqSalvageAtmosphere.Build(root);
            PlaceGarageDoor(root);
            HqStaffEntryDoor.Build(root);
            HqExteriorBuilder.Build(root);

            Transform spawns = Group("Spawns", root);
            for (int i = 0; i < 4; i++)
            {
                Transform spawn = new GameObject($"PlayerSpawn_{i}").transform;
                spawn.SetParent(spawns, false);
                spawn.SetPositionAndRotation(new Vector3(26.2f + i * 1.5f, .05f, 1.8f), Quaternion.identity);
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }

            new GameObject("DebugView", typeof(DebugViewToggle), typeof(PerfOverlay));
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                if (camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();
                camera.transform.position = new Vector3(18f, 24f, -21f);
                camera.transform.LookAt(new Vector3(18f, 0f, 14f));
            }
            // The title screen shows the receiving floor, stock aisles and high warehouse roof.
            var vantage = new GameObject("MenuVantage");
            vantage.transform.SetParent(root, false);
            vantage.transform.position = HqLayout.MenuEye;
            vantage.transform.rotation = Quaternion.LookRotation(HqLayout.MenuFocus - vantage.transform.position);
            vantage.AddComponent<Abandoned.UI.MenuVantage>();
            NetworkSceneBuilder.Add();
            var ui = new GameObject("CompanyUI");
            ui.AddComponent<HqHud>();
            ui.AddComponent<ContractBoardScreen>();
            ui.AddComponent<Abandoned.Equipment.GearScreens>();
            LevelAtmosphere.Apply("HQ", HqSalvageAtmosphere.Mood, root);
            AmbienceBuilder.Hq(root);
            HqWorldText.Remove(root);
            // Author collision after all of the operational stock and furniture have been placed.
            SolidObjectsPass.Apply(root, "HQ");
            if (runAudits)
            {
                InvisibleColliderAudit.Run(root, "HQ");
                SolidObjectsPass.Audit(root, "HQ");
            }
            ShadowCasterPass.Apply(root, "HQ");

            // Our own save must not be captured as a hand placement (HqPlacementSync).
            HqPlacementSync.Building = true;
            try { EditorSceneManager.SaveScene(scene, ScenePath); }
            finally { HqPlacementSync.Building = false; }
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != ScenePath)) EditorBuildSettings.scenes = scenes.Append(new EditorBuildSettingsScene(ScenePath, true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"Salvage depot HQ saved to {ScenePath}.");
        }

        /// <summary>The car garage door, wherever it was last placed by hand (HqPlacements; default: the garage opening).</summary>
        private static void PlaceGarageDoor(Transform root)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GarageDoorBuilder.PrefabPath);
            if (prefab == null) prefab = GarageDoorBuilder.Create();
            HqPlacements placements = HqPlacementSync.Load();
            var door = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            door.name = "GarageDoor";
            door.transform.SetPositionAndRotation(placements.GarageDoorPosition, placements.GarageDoorRotation);
            door.transform.localScale = placements.GarageDoorScale;
            door.GetComponent<GarageDoor>().EditorSetOpen(placements.GarageDoorOpen);
        }

    }
}
