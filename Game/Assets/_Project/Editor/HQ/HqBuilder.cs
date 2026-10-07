using System.Collections.Generic;
using System.Linq;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Scenes/HQ.unity (GDD 5: company HQ between runs): a 24 x 16 m one-storey greybox with a
    /// garage (the van that drives everyone to the job, the players' spawn), an office (the contract
    /// board) and a shop corner (the terminal, 6.4). No in-scene network objects: the HQ is where
    /// players join and every level spawns its networked things itself (session travel, M6.0).
    /// </summary>
    public static class HqBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/HQ.unity";
        private const float W = 24f, D = 16f, H = 4f, Wall = 0.2f;
        private static readonly WallOpening GarageDoor = new(8f, 0f, 3.4f);
        private static readonly WallOpening Doorway = new(2f, 0f, 2.4f);

        [MenuItem("Tools/Abandoned/Create HQ")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject("HQ").transform;
            Material floor = GetMaterial("Greybox_HQFloor", new Color(0.42f, 0.43f, 0.45f));
            Material wall = GetMaterial("Greybox_Wall", new Color(0.82f, 0.81f, 0.78f));
            Material frame = GetMaterial("Greybox_DoorFrame", new Color(0.95f, 0.8f, 0.1f));
            Material ground = GetMaterial("Greybox_Ground", new Color(0.36f, 0.4f, 0.32f));

            Transform shell = Group("Shell", root);
            Box("Floor", shell, new Vector3(W / 2f, -0.15f, D / 2f), new Vector3(W, 0.3f, D), floor).AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            Box("Ground", shell, new Vector3(W / 2f, -0.4f, -10f), new Vector3(W + 40f, 0.3f, 20f), ground).AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            Box("Roof", shell, new Vector3(W / 2f, H + 0.15f, D / 2f), new Vector3(W, 0.3f, D), GetMaterial("Greybox_Roof", new Color(0.35f, 0.35f, 0.37f)));
            // Outer walls: the garage's south side is one wide door.
            GreyboxWall.Panel(shell, "Wall_S_Garage", new Vector3(0f, 0f, -Wall / 2f), Vector3.right, 12f, H, Wall, GarageDoor, wall, frame);
            GreyboxWall.Panel(shell, "Wall_S_Office", new Vector3(12f, 0f, -Wall / 2f), Vector3.right, 12f, H, Wall, null, wall, frame);
            GreyboxWall.Panel(shell, "Wall_N", new Vector3(0f, 0f, D + Wall / 2f), Vector3.right, W, H, Wall, null, wall, frame);
            GreyboxWall.Panel(shell, "Wall_W", new Vector3(-Wall / 2f, 0f, 0f), Vector3.forward, D, H, Wall, null, wall, frame);
            GreyboxWall.Panel(shell, "Wall_E", new Vector3(W + Wall / 2f, 0f, 0f), Vector3.forward, D, H, Wall, null, wall, frame);
            // Garage | office/shop, and office | shop.
            GreyboxWall.Panel(shell, "Wall_GarageOffice", new Vector3(12f, 0f, 0f), Vector3.forward, 10f, H, Wall, Doorway, wall, frame);
            GreyboxWall.Panel(shell, "Wall_GarageShop", new Vector3(12f, 0f, 10f), Vector3.forward, 6f, H, Wall, Doorway, wall, frame);
            GreyboxWall.Panel(shell, "Wall_OfficeShop", new Vector3(12f, 0f, 10f), Vector3.right, 12f, H, Wall, Doorway, wall, frame);
            MarkStatic(shell);

            // The contract board on the office's south wall, facing in; the van in the garage; the shop counter.
            GameObject board = Box("ContractBoard", root, new Vector3(18f, 1.7f, 0.25f), new Vector3(4f, 1.8f, 0.15f), GetMaterial("Greybox_Board", new Color(0.55f, 0.4f, 0.25f)));
            board.AddComponent<ContractBoard>();
            GameObject van = Box("Van", root, new Vector3(6f, 1.25f, 9f), new Vector3(2.4f, 2.4f, 5f), GetMaterial("Greybox_Truck", new Color(0.7f, 0.22f, 0.2f)));
            van.AddComponent<HqVan>();
            Box("ShopCounter", root, new Vector3(18f, 0.55f, 13.5f), new Vector3(3f, 1.1f, 0.8f), GetMaterial("Greybox_Prop", new Color(0.55f, 0.42f, 0.3f)));

            Transform spawns = Group("Spawns", root);
            for (int i = 0; i < 4; i++)
            {
                var spawn = new GameObject($"PlayerSpawn_{i}").transform;
                spawn.SetParent(spawns, false);
                spawn.SetPositionAndRotation(new Vector3(3f + i * 1.5f, 0.05f, 3.5f), Quaternion.identity);
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }

            new GameObject("DebugView").AddComponent<DebugViewToggle>();
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                if (camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();
                camera.transform.position = new Vector3(W / 2f, 18f, -14f);
                camera.transform.LookAt(new Vector3(W / 2f, 0f, D / 2f));
            }
            NetworkSceneBuilder.Add();
            var ui = new GameObject("CompanyUI");
            ui.AddComponent<HqHud>();
            ui.AddComponent<ContractBoardScreen>();

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.37f);
            foreach (Vector3 at in new[] { new Vector3(6f, 3.4f, 8f), new Vector3(18f, 3.4f, 5f), new Vector3(18f, 3.4f, 13f) })
            {
                var light = new GameObject("Light").AddComponent<Light>();
                light.transform.SetParent(root, false);
                light.transform.position = at;
                light.type = LightType.Point;
                light.range = 12f;
                light.intensity = 5f;
                light.shadows = LightShadows.None;
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != ScenePath)) EditorBuildSettings.scenes = scenes.Append(new EditorBuildSettingsScene(ScenePath, true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"HQ saved to {ScenePath}.");
        }
    }
}
