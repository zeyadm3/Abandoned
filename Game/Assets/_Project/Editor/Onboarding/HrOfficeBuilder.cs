using System;
using Abandoned.EditorTools.Remake;
using Abandoned.Onboarding;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools.Hiring
{
    /// <summary>
    /// The HR office of the hiring (2026 remake, phase 7): a small office at Abandoned Salvaging's head office, the HR
    /// manager behind her desk, a visitor's chair for you, six Cinemachine shots (the seated first-person view and five
    /// set-ups), and the scene's director. Offline only, so nothing here is networked. Furniture is the map kit's
    /// (Art/Remake/Maps/Kit); the manager is the crew's figure in her own look (Art/Remake/Onboarding/Characters).
    /// Origin: the middle of the desk; you sit at -Z facing +Z, she sits at +Z facing you.
    /// </summary>
    public static class HrOfficeBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/HrOffice.unity";
        private const string LookFolder = "Assets/_Project/Data/Onboarding/Looks";
        private const float W = 5f, D = 6f, H = 2.8f;

        [MenuItem(ToolMenu.Hq + "Rebuild HR Office (hiring)", priority = ToolMenu.HqP + 80)]
        public static void BuildMenu()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build the HR office in Edit mode.");
            OnboardingDialogue.Build();
            KitSwap.Prepare();
            (CosmeticDefinition person, CosmeticDefinition outfit) = CrewLookBuilder.BuildNpc(RemakePaths.Onboarding + "/Characters", "whitfield", "office", LookFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("HrOffice").transform;
            Room(Group("Room", root));
            Transform furniture = Group("Furniture", root);
            Kit("Kit_desk_b", furniture, Vector3.zero, 0f);
            Kit("Kit_office_chair_a", furniture, new Vector3(0f, 0f, .95f), 180f);
            Kit("Kit_chair_b", furniture, new Vector3(0f, 0f, -1.08f), 0f);
            Kit("Kit_filing_cabinet_a", furniture, new Vector3(-1.4f, 0f, D / 2 - .36f), 180f);
            Kit("Kit_filing_cabinet_a", furniture, new Vector3(-.85f, 0f, D / 2 - .36f), 180f);
            Kit("Kit_bookcase_a", furniture, new Vector3(-W / 2 + .2f, 0f, .9f), 90f);
            Kit("Kit_cabinet_a", furniture, new Vector3(1.2f, 0f, D / 2 - .27f), 180f);
            Kit("Kit_whiteboard_a", furniture, new Vector3(.2f, 1.1f, D / 2 - .03f), 180f, .6f);
            Kit("Kit_trash_can_a", furniture, new Vector3(.95f, 0f, .7f), 0f, .45f);
            Kit("Kit_chair_b", furniture, new Vector3(1.65f, 0f, -2.2f), -120f);
            Desk(Group("DeskTop", root));

            Transform people = Group("People", root);
            var manager = new GameObject("HrManager");
            manager.transform.SetParent(people, false);
            manager.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, .95f), Quaternion.Euler(0f, 180f, 0f));
            var voiceGo = new GameObject("Voice");
            voiceGo.transform.SetParent(manager.transform, false);
            voiceGo.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            var voice = voiceGo.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = .55f;
            voice.minDistance = 1.5f;
            var actor = manager.AddComponent<HrManagerActor>();

            Light(root);
            CinemachineCamera[] shots = Shots(Group("Shots", root), out SeatedLook seat);
            actor.EditorSetup(person, outfit, voice, shots[0].transform);

            var director = root.gameObject.AddComponent<CutsceneDirector>();
            director.EditorSetup(shots, seat, voice);
            var office = root.gameObject.AddComponent<HrOffice>();
            office.EditorSetup(director, AssetDatabase.LoadAssetAtPath<DialogueDefinition>(OnboardingDialogue.InterviewPath),
                AssetDatabase.LoadAssetAtPath<DialogueDefinition>(OnboardingDialogue.HiredPath),
                AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CosmeticsBuilder.CatalogPath));

            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Unity could not save the HR office.");
            BuildScenes.ApplyToEditorSettings();
            Debug.Log("[Onboarding] HR office built: " + ScenePath);
        }

        private static void Room(Transform room)
        {
            Material floor = PolishAssets.Material("HR_Carpet", new Color(.2f, .21f, .23f), .95f);
            Material wall = PolishAssets.Material("HR_Wall", new Color(.62f, .6f, .55f), .85f);
            Material ceiling = PolishAssets.Material("HR_Ceiling", new Color(.78f, .78f, .75f), .9f);
            Material skirting = PolishAssets.Material("HR_Skirting", new Color(.25f, .2f, .16f), .6f);
            Material door = PolishAssets.Material("HR_Door", new Color(.36f, .27f, .19f), .55f);
            Material outside = PolishAssets.Material("HR_Outside", new Color(.55f, .6f, .66f), .9f, null, .6f);
            Box("Floor", room, new Vector3(0f, -.05f, 0f), new Vector3(W, .1f, D), floor);
            Box("Ceiling", room, new Vector3(0f, H + .05f, 0f), new Vector3(W, .1f, D), ceiling);
            Box("WallNorth", room, new Vector3(0f, H / 2, D / 2 + .05f), new Vector3(W, H, .1f), wall);
            Box("WallWest", room, new Vector3(-W / 2 - .05f, H / 2, 0f), new Vector3(.1f, H, D), wall);
            // South wall with the door (x -1.6, 1 m wide, 2.1 m high), standing ajar.
            Box("WallSouthWest", room, new Vector3(-2.3f, H / 2, -D / 2 - .05f), new Vector3(.4f, H, .1f), wall);
            Box("WallSouthEast", room, new Vector3(.45f, H / 2, -D / 2 - .05f), new Vector3(4.1f, H, .1f), wall);
            Box("WallSouthHead", room, new Vector3(-1.6f, 2.45f, -D / 2 - .05f), new Vector3(1f, .7f, .1f), wall);
            GameObject leaf = Box("Door", room, Vector3.zero, new Vector3(.95f, 2.08f, .045f), door);
            leaf.transform.localPosition = new Vector3(-1.95f, 1.04f, -D / 2 + .3f);
            leaf.transform.localRotation = Quaternion.Euler(0f, -65f, 0f);
            // East wall with the window (z 0.4 to 2.0, sill 0.9 m, 1.4 m tall).
            Box("WallEastSouth", room, new Vector3(W / 2 + .05f, H / 2, -1.3f), new Vector3(.1f, H, 3.4f), wall);
            Box("WallEastNorth", room, new Vector3(W / 2 + .05f, H / 2, 2.5f), new Vector3(.1f, H, 1f), wall);
            Box("WallEastSill", room, new Vector3(W / 2 + .05f, .45f, 1.2f), new Vector3(.1f, .9f, 1.6f), wall);
            Box("WallEastHead", room, new Vector3(W / 2 + .05f, 2.55f, 1.2f), new Vector3(.1f, .5f, 1.6f), wall);
            Kit("Kit_window_a", room, new Vector3(W / 2 + .05f, .9f, 1.2f), -90f, 1f, new Vector3(1f, .7f, .6f));
            Box("Outside", room, new Vector3(W / 2 + 6f, 1.5f, 1.2f), new Vector3(.1f, 8f, 14f), outside, false);
            foreach ((string n, Vector3 c, Vector3 s) in new[]
                     {
                         ("SkirtingNorth", new Vector3(0f, .05f, D / 2 - .01f), new Vector3(W, .1f, .02f)),
                         ("SkirtingWest", new Vector3(-W / 2 + .01f, .05f, 0f), new Vector3(.02f, .1f, D)),
                     })
                Box(n, room, c, s, skirting, false);
        }

        private static void Desk(Transform top)
        {
            Material paper = PolishAssets.Material("HR_Paper", new Color(.88f, .87f, .82f), .9f);
            Material plastic = PolishAssets.Material("HR_Plastic", new Color(.08f, .08f, .09f), .45f);
            Material screen = PolishAssets.Material("HR_Screen", new Color(.05f, .09f, .1f), .2f, null, .4f);
            Material ink = PolishAssets.Material("HR_Pen", new Color(.05f, .07f, .2f), .4f);
            const float y = .76f;
            // The contract, turned to you, and a pen.
            Box("Contract", top, new Vector3(0f, y + .003f, -.18f), new Vector3(.21f, .004f, .297f), paper, false).transform.localRotation = Quaternion.Euler(0f, 4f, 0f);
            Box("ContractPage2", top, new Vector3(.01f, y + .006f, -.17f), new Vector3(.21f, .002f, .297f), paper, false).transform.localRotation = Quaternion.Euler(0f, -3f, 0f);
            Box("Pen", top, new Vector3(.16f, y + .007f, -.2f), new Vector3(.14f, .01f, .01f), ink, false).transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            // Her monitor, keyboard and files.
            Box("MonitorStand", top, new Vector3(-.45f, y + .06f, .2f), new Vector3(.18f, .12f, .14f), plastic, false);
            GameObject monitor = Box("Monitor", top, new Vector3(-.45f, y + .3f, .22f), new Vector3(.52f, .32f, .04f), plastic, false);
            monitor.transform.localRotation = Quaternion.Euler(0f, -18f, 0f);
            Box("MonitorScreen", monitor.transform, new Vector3(0f, 0f, .52f), new Vector3(.94f, .9f, .1f), screen, false);
            Box("Keyboard", top, new Vector3(-.2f, y + .012f, .18f), new Vector3(.44f, .02f, .15f), plastic, false);
            for (int i = 0; i < 3; i++)
                Box("Folder", top, new Vector3(.5f, y + .01f + i * .018f, .15f), new Vector3(.24f, .016f, .32f),
                    PolishAssets.Material("HR_Folder" + i, new[] { new Color(.55f, .45f, .25f), new Color(.2f, .3f, .45f), new Color(.5f, .2f, .15f) }[i], .8f), false);
        }

        private static void Light(Transform root)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.32f, .31f, .3f);
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(root, false);
            sunGo.transform.localRotation = Quaternion.Euler(28f, -110f, 0f);
            var sun = sunGo.AddComponent<UnityEngine.Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            sun.color = new Color(1f, .93f, .82f);
            sun.shadows = LightShadows.Soft;
            foreach ((string name, Vector3 at, float intensity, float range, Color color) in new[]
                     {
                         ("CeilingPanel", new Vector3(0f, H - .15f, -.4f), 2.4f, 7f, new Color(1f, .96f, .9f)),
                         ("CeilingPanelBack", new Vector3(0f, H - .15f, 2f), 1.8f, 6f, new Color(1f, .96f, .9f)),
                         ("DeskLamp", new Vector3(-.75f, 1.2f, .45f), 1.2f, 2.5f, new Color(1f, .8f, .55f)),
                     })
            {
                var go = new GameObject(name);
                go.transform.SetParent(root, false);
                go.transform.localPosition = at;
                var light = go.AddComponent<UnityEngine.Light>();
                light.type = LightType.Point;
                light.intensity = intensity;
                light.range = range;
                light.color = color;
                light.shadows = name == "CeilingPanel" ? LightShadows.Soft : LightShadows.None;
            }
        }

        private static CinemachineCamera[] Shots(Transform parent, out SeatedLook seat)
        {
            var mainGo = new GameObject("Main Camera") { tag = "MainCamera" };
            mainGo.transform.SetParent(parent, false);
            var camera = mainGo.AddComponent<Camera>();
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 60f;
            mainGo.AddComponent<AudioListener>();
            var brain = mainGo.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, .9f);
            var head = new Vector3(0f, 1.22f, .95f);
            (string name, Vector3 at, Vector3 look, float fov)[] specs =
            {
                ("Shot0_Seat", new Vector3(0f, 1.18f, -1.08f), head, 62f),
                ("Shot1_Door", new Vector3(-.85f, 1.72f, -2.75f), new Vector3(.1f, .95f, .4f), 54f),
                ("Shot2_OverShoulder", new Vector3(.42f, 1.36f, -1.62f), head, 40f),
                ("Shot3_Contract", new Vector3(.22f, 1.22f, -.5f), new Vector3(0f, .77f, -.17f), 36f),
                ("Shot4_Handshake", new Vector3(2.05f, 1.3f, -.05f), new Vector3(0f, 1.02f, -.05f), 45f),
                ("Shot5_Window", new Vector3(2.0f, 1.85f, 2.5f), new Vector3(-.6f, .95f, -1.1f), 56f),
            };
            var shots = new CinemachineCamera[specs.Length];
            seat = null;
            for (int i = 0; i < specs.Length; i++)
            {
                var go = new GameObject(specs[i].name);
                go.transform.SetParent(parent, false);
                go.transform.localPosition = specs[i].at;
                go.transform.localRotation = Quaternion.LookRotation(specs[i].look - specs[i].at, Vector3.up);
                var shot = go.AddComponent<CinemachineCamera>();
                LensSettings lens = shot.Lens;
                lens.FieldOfView = specs[i].fov;
                lens.NearClipPlane = .05f;
                shot.Lens = lens;
                shot.Priority = i == 1 ? 20 : 0;
                shots[i] = shot;
                if (i == 0) seat = go.AddComponent<SeatedLook>();
            }
            return shots;
        }

        private static GameObject Kit(string model, Transform parent, Vector3 at, float yaw, float scale = 1f, Vector3? stretch = null)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{KitSwap.KitFolder}/{model}.fbx");
            if (asset == null)
            {
                Debug.LogWarning($"[Onboarding] No kit model {model} (run Tools/Blender/remake/maps/kit.py).");
                return null;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.transform.SetParent(parent, false);
            go.transform.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            go.transform.localScale = (stretch ?? Vector3.one) * scale;
            Bounds b = ModelFit.LocalBounds(go, go.transform);
            var box = go.AddComponent<BoxCollider>();
            box.center = b.center;
            box.size = b.size;
            return go;
        }
    }
}
