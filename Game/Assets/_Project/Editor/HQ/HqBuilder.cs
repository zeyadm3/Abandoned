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
    /// <summary>A dim salvage depot: warm departure bay, cold office and worn equipment workshop.</summary>
    public static class HqBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/HQ.unity";
        private const float Width = 24f, Depth = 16f, Height = 4f;

        [MenuItem("Tools/Abandoned/Create HQ")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            CustomMallArt.Prepare();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject("HQ").transform;
            BuildShell(root);
            BuildStations(root);
            HqDressingBuilder.Build(root);
            PlaceGarageDoor(root);

            Transform spawns = Group("Spawns", root);
            for (int i = 0; i < 4; i++)
            {
                Transform spawn = new GameObject($"PlayerSpawn_{i}").transform;
                spawn.SetParent(spawns, false);
                spawn.SetPositionAndRotation(new Vector3(3f + i * 1.5f, .05f, 3.5f), Quaternion.identity);
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }

            new GameObject("DebugView", typeof(DebugViewToggle), typeof(PerfOverlay));
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                if (camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();
                camera.transform.position = new Vector3(12f, 18f, -14f);
                camera.transform.LookAt(new Vector3(12f, 0f, 8f));
            }
            // Title screen: eye height in the garage, looking through the office doorway at its failing tubes.
            var vantage = new GameObject("MenuVantage");
            vantage.transform.SetParent(root, false);
            vantage.transform.position = new Vector3(2.2f, 1.62f, 1.2f);
            vantage.transform.rotation = Quaternion.LookRotation(new Vector3(22f, 1.35f, 8.9f) - vantage.transform.position);
            vantage.AddComponent<Abandoned.UI.MenuVantage>();
            NetworkSceneBuilder.Add();
            var ui = new GameObject("CompanyUI");
            ui.AddComponent<HqHud>();
            ui.AddComponent<ContractBoardScreen>();
            ui.AddComponent<Abandoned.Equipment.GearScreens>();
            LevelAtmosphere.Apply("HQ", LevelAtmosphere.Hq, root);
            AmbienceBuilder.Hq(root);
            InvisibleColliderAudit.Run(root, "HQ");

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

        private static void BuildShell(Transform root)
        {
            Transform shell = Group("Shell", root);
            Material concrete = CustomMallArt.Material("Concrete");
            GameObject slab = Box("Floor", shell, new Vector3(12f, -.15f, 8f), new Vector3(Width, .3f, Depth), concrete);
            slab.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            CustomMallArt.HidePrimitive(slab);
            GameObject roof = Box("Roof", shell, new Vector3(12f, Height + .15f, 8f), new Vector3(Width + .22f, .3f, Depth + .22f), CustomMallArt.Material("Metal"));
            // The visible opaque roof closes the garage above its exposed trusses.
            roof.GetComponent<Renderer>().enabled = true;
            Box("Yard", shell, new Vector3(12f, -.4f, -10f), new Vector3(64f, .3f, 20f), CustomMallArt.Material("Dirt"))
                .AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);

            // Each solid portion is a modular visual over its own authoritative collision box.
            Solid("GaragePierL", new Vector3(1f, 0f, -.1f), 2f);
            Solid("GaragePierR", new Vector3(11f, 0f, -.1f), 2f);
            Solid("GarageHeader", new Vector3(6f, 3.4f, -.1f), 8f, .6f);
            for (int x = 0; x < 6; x++)
            {
                if (x >= 3) Solid($"South_{x}", new Vector3(x * 4f + 2f, 0f, -.1f), 4f);
                Solid($"North_{x}", new Vector3(x * 4f + 2f, 0f, Depth + .1f), 4f);
            }
            for (int z = 0; z < 4; z++)
            {
                Solid($"West_{z}", new Vector3(-.1f, 0f, z * 4f + 2f), 4f, Height, 90f);
                Solid($"East_{z}", new Vector3(Width + .1f, 0f, z * 4f + 2f), 4f, Height, 90f);
            }
            // The three original doorways remain 2 m wide and 2.4 m high, including bulky gear routes.
            Solid("GarageOfficeSouth", new Vector3(12f, 0f, 2f), 4f, Height, 90f);
            Solid("GarageOfficeNorth", new Vector3(12f, 0f, 8f), 4f, Height, 90f);
            Solid("GarageOfficeHeader", new Vector3(12f, 2.4f, 5f), 2f, 1.6f, 90f);
            Solid("GarageShopSouth", new Vector3(12f, 0f, 11f), 2f, Height, 90f);
            Solid("GarageShopNorth", new Vector3(12f, 0f, 15f), 2f, Height, 90f);
            Solid("GarageShopHeader", new Vector3(12f, 2.4f, 13f), 2f, 1.6f, 90f);
            Solid("OfficeShopWest", new Vector3(14f, 0f, 10f), 4f);
            Solid("OfficeShopWestJamb", new Vector3(16.5f, 0f, 10f), 1f);
            Solid("OfficeShopEast", new Vector3(21f, 0f, 10f), 4f);
            Solid("OfficeShopEastJamb", new Vector3(23.5f, 0f, 10f), 1f);
            Solid("OfficeShopHeader", new Vector3(18f, 2.4f, 10f), 2f, 1.6f);
            foreach (Vector3 corner in new[] { new Vector3(-.1f, 0f, -.1f), new Vector3(Width + .1f, 0f, -.1f),
                         new Vector3(-.1f, 0f, Depth + .1f), new Vector3(Width + .1f, 0f, Depth + .1f) })
                Box("CornerClosure", shell, corner + Vector3.up * (Height * .5f), new Vector3(.2f, Height, .2f), CustomMallArt.Material("Plaster"));
            MarkStatic(shell);

            void Solid(string name, Vector3 floor, float length, float height = Height, float yaw = 0f)
            {
                var wall = new GameObject(name);
                wall.transform.SetParent(shell, false);
                wall.transform.SetLocalPositionAndRotation(floor, Quaternion.Euler(0f, yaw, 0f));
                var collider = wall.AddComponent<BoxCollider>();
                collider.center = Vector3.up * height * .5f;
                collider.size = new Vector3(length, height, .2f);
                CustomMallArt.Place("WallSolid", wall.transform, Vector3.zero, Quaternion.identity, new Vector3(length / 4f, height / 3.82f, 1f));
            }
        }

        private static void BuildStations(Transform root)
        {
            Material wood = CustomMallArt.Material("Wood"), metal = CustomMallArt.Material("Metal"), trim = CustomMallArt.Material("Trim");
            GameObject board = Box("ContractBoard", root, new Vector3(18f, 1.7f, .25f), new Vector3(4f, 1.8f, .15f), wood);
            board.AddComponent<ContractBoard>();
            HqDressingBuilder.ContractFace(board.transform);

            GameObject van = Box("Van", root, new Vector3(6f, 1.25f, 9f), new Vector3(2.4f, 2.4f, 5f), trim);
            van.AddComponent<HqVan>();
            Vehicles.Dress(van, Vehicles.Van);
            foreach (Renderer renderer in van.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == null) continue;
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", new Color(.33f, .36f, .31f));
                renderer.SetPropertyBlock(block);
            }

            GameObject counter = Box("ShopCounter", root, new Vector3(18f, .55f, 13.5f), new Vector3(3f, 1.1f, .8f), wood);
            CustomMallArt.Fit(counter, "Counter", Vector3.one);
            GameObject terminal = Box("ShopTerminal", root, new Vector3(18f, 1.35f, 13.6f), new Vector3(.8f, .5f, .3f), trim);
            terminal.AddComponent<Abandoned.Equipment.ShopTerminal>();
            HqDressingBuilder.TerminalFace(terminal.transform);

            Transform rack = Station(root, "GearRack", new Vector3(.65f, 0f, 12f), new Vector3(.62f, 2.4f, 3f));
            rack.gameObject.AddComponent<Abandoned.Equipment.GearRack>();
            CustomMallArt.Place("RetailShelf", rack, Vector3.zero, Quaternion.Euler(0f, 90f, 0f), new Vector3(1.74f, 1.12f, 1.12f));
            Transform lockers = Station(root, "Lockers", new Vector3(.4f, 0f, 6.5f), new Vector3(.55f, 2.1f, 2.4f));
            lockers.gameObject.AddComponent<Abandoned.UI.WardrobeLocker>();
            CustomMallArt.Place("DepotLockers", lockers, Vector3.zero, Quaternion.Euler(0f, 90f, 0f), new Vector3(1.33f, 1f, 1f));

            GameObject desk = Box("OfficeDesk", root, new Vector3(21.5f, .4f, 2.2f), new Vector3(2f, .8f, 1f), wood);
            CustomMallArt.Fit(desk, "Counter", Vector3.one);
            GameObject machine = Box("AnsweringMachine", root, new Vector3(21.5f, .88f, 2.2f), new Vector3(.4f, .15f, .3f), metal);
            Material blink = PolishAssets.Material("HQ_MessageLamp", new Color(.62f, .23f, .075f), emission: 1.6f);
            GameObject lamp = Box("MessageLight", machine.transform, new Vector3(.3f, .6f, 0f), new Vector3(.12f, .4f, .15f), blink, false);
            machine.AddComponent<AnsweringMachine>().EditorSetup(lamp.GetComponent<Renderer>());
        }

        private static Transform Station(Transform parent, string name, Vector3 floor, Vector3 size)
        {
            Transform station = Group(name, parent);
            station.localPosition = floor;
            var collider = station.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.up * size.y * .5f;
            collider.size = size;
            return station;
        }
    }
}
