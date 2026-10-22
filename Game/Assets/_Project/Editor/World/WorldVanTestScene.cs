using Abandoned.Player;
using Abandoned.World;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Scenes/WorldVanTest: a flat asphalt pad with a ramp, speed bumps, a slalom and a wall to drive into, the session
    /// objects (press Play to host alone) and the van. For trying the van's handling, seats, ramp and damage.
    /// </summary>
    public static class WorldVanTestScene
    {
        public const string ScenePath = "Assets/_Project/Scenes/WorldVanTest.unity";

        [MenuItem(ToolMenu.OpenWorld + "Build Van Test Scene", priority = ToolMenu.OpenWorldP + 21)]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            GameObject vanPrefab = WorldVanBuilder.Build();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCameraAndDebug();

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 2.2f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.55f, .6f, .68f);
            RenderSettings.ambientEquatorColor = new Color(.42f, .42f, .4f);
            RenderSettings.ambientGroundColor = new Color(.22f, .2f, .18f);

            Material asphalt = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Polish/HQ_District_Asphalt.mat");
            Material concrete = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Greybox/Greybox_Asphalt.mat");
            Transform course = new GameObject("Course").transform;
            GreyboxFactory.Box("Ground", course, new Vector3(0f, -.5f, 0f), new Vector3(500f, 1f, 500f), asphalt);
            // A loading-dock ramp, two speed bumps, a slalom of posts and a wall to test damage against.
            var ramp = GreyboxFactory.Box("Ramp", course, new Vector3(0f, .9f, 70f), new Vector3(8f, .4f, 14f), concrete);
            ramp.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            for (int i = 0; i < 2; i++)
                GreyboxFactory.Box($"Bump_{i}", course, new Vector3(-20f, .06f, 30f + i * 12f), new Vector3(9f, .12f, .6f), concrete);
            for (int i = 0; i < 6; i++)
                GreyboxFactory.Box($"Post_{i}", course, new Vector3(30f + (i % 2) * 4f, .6f, 20f + i * 14f), new Vector3(.4f, 1.2f, .4f), concrete);
            GreyboxFactory.Box("CrashWall", course, new Vector3(-60f, 1.5f, 60f), new Vector3(12f, 3f, 1f), concrete);
            foreach (Transform t in course) GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);

            NetworkSceneBuilder.Add();
            Transform spawns = new GameObject("PlayerSpawns").transform;
            for (int i = 0; i < 4; i++)
            {
                var spawn = new GameObject($"PlayerSpawn_{i}").transform;
                spawn.SetParent(spawns, false);
                spawn.localPosition = new Vector3(-4f + i * 1.6f, 0f, -4f);
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }
            var spawner = new GameObject("VanSpawn").AddComponent<WorldVanSpawner>();
            spawner.transform.position = new Vector3(-6f, .1f, 6f);
            spawner.EditorSetup(vanPrefab.GetComponent<NetworkObject>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[WorldVan] Built {ScenePath}.");
        }

        /// <summary>The scene's real camera (the player prefab brings only a Cinemachine camera) and the F1 switch.</summary>
        public static void AddCameraAndDebug()
        {
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(Unity.Cinemachine.CinemachineBrain));
            camera.tag = "MainCamera";
            camera.GetComponent<Camera>().farClipPlane = 3000f;
            new GameObject("DebugView", typeof(Abandoned.Core.DebugViewToggle), typeof(Abandoned.Core.PerfOverlay));
        }
    }
}
