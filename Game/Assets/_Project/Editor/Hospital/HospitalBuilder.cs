using System;
using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Contracts;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Abandoned.Hospital
{
    public static class HospitalBuilder
    {
        public const string ScenePath = HospitalLayout.ScenePath;
        // Overcast late afternoon: a flat grey-green sky, the clinical haze of a shut ward block.
        private static readonly LevelAtmosphere.Mood Mood = new(sky : new Color(.1f, .115f, .12f), equator : new Color(.065f, .072f, .07f), ground : new Color(.03f, .03f, .028f), fog : new Color(.26f, .29f, .28f), fogDensity : .012f, bloom : .2f, vignette : .22f, grain : .14f, saturation : - 26f, contrast : 10f, sunColor : new Color(.9f, .93f, 1f), sunIntensity : .3f, sunEuler : new Vector3(34f, - 140f, 0f));
        [MenuItem(ToolMenu.Hospital + "Rebuild Saint Brigid Hospital", priority = ToolMenu.HospitalP)] public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Rebuild Hospital in Edit mode.");
            }
            if (! EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            var kit = new HospitalKit();
            LootCatalog catalog = HospitalLoot.Build(kit);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject("Hospital").transform;
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(HospitalLayout.Bounds);
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(HospitalLayout.Bounds, new Color(.14f, .18f, .16f), .009f);
            var setup = new StructureSceneSetup();
            if (! setup.IsValid)
            {
                throw new InvalidOperationException("Hospital needs the shared structure content.");
            }
            var floors = new HospitalStructure(kit, setup);
            floors.Build(root);
            HospitalShell.Build(root, kit, floors);
            HospitalSite.Build(root, kit);
            List<LootPointSpec> loot = HospitalInteriors.Build(root, kit, floors);
            HorrorConfig horror = HospitalServices.Build(root, kit, floors);
            GameObject runPrefab = HospitalLoot.RunPrefab(horror);
            StructureSimulation sim = setup.AddSimulation(.72f, 17023);
            var threats = floors.Context.Threats;
            LocationPlumbing.Populate(root, HospitalLayout.Crew, loot, threats, sim);
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<NetworkBootstrap>();
            var spawner = UnityEngine.Object.FindFirstObjectByType<NetworkLootSpawner>();
            spawner.Setup(bootstrap, catalog, AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath), false);
            var director = UnityEngine.Object.FindFirstObjectByType<RunDirector>();
            director.Setup(bootstrap, runPrefab.GetComponent<NetworkObject>(), spawner, sim);
            SerializedWiring.Set(UnityEngine.Object.FindFirstObjectByType<PowerController>(), "sectorsWithoutHorror", horror);
            // Give a standalone scene the same pre-registered content as the persistent world session.
            bootstrap.Manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(PrefabList(catalog, runPrefab));
            var location = SerializedWiring.LoadOrCreateAsset<LocationDefinition>(HospitalLoot.Data + "/HospitalLocation.asset");
            location.EditorSetup("hospital", "Saint Brigid Hospital", "An abandoned medical city with a glass atrium, stepped wards and a basement morgue.", "Hospital", 1, 1, 1.2f, 1.1f, - .05f, 1.15f, .1f);
            EditorUtility.SetDirty(location);
            HospitalRegistration.Include(catalog, location);
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.transform != root && go.name != "NetworkManager" && go.name != "Network")
                {
                    go.transform.SetParent(root, true);
                }
            }
            HospitalSolids.Fit(floors);
            StationSurfaces.Apply(root, floors.Context.Art);
            HospitalNavigation.Build(root);
            // Solid props (0.24.11), guarded by the first bake so no box cuts a route; then bake round them.
            LevelSolidity.Apply(root, "Hospital");
            foreach (NavMeshSurface first in root.GetComponents<NavMeshSurface>())
            {
                UnityEngine.Object.DestroyImmediate(first);
            }
            HospitalNavigation.Build(root);
            // The map's sky light, fog and grade (its Hospital_Volume profile also takes the 0.24.12 look), and its sound bed.
            LevelAtmosphere.Apply("Hospital", Mood, root);
            // Wind round the ward block, a faint ballast hum, the building settling now and then.
            var ambience = new GameObject("Ambience");
            ambience.transform.SetParent(root, false);
            ambience.AddComponent<LevelAmbience>().EditorSetup(.22f, .12f, .03f, new Vector2(20f, 50f));
            Camera reviewCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (reviewCamera != null)
            {
                reviewCamera.transform.position = new Vector3(82f, 30f, - 40f);
                reviewCamera.transform.LookAt(new Vector3(28f, 6f, 24f));
            }
            BuildScenes.ApplyToEditorSettings();
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            if (! EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException("Hospital scene could not be saved.");
            }
            if (! OcclusionBake.Bake())
            {
                throw new InvalidOperationException("Hospital occlusion bake failed.");
            }
            if (! EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException("Hospital could not be saved after occlusion.");
            }
            Debug.Log("[Hospital] Authored Saint Brigid Hospital. Review instructions: Docs/HOSPITAL_PLAN.md.");
        }
        private static NetworkPrefabsList PrefabList(LootCatalog catalog, GameObject run)
        {
            string path = HospitalKit.Art + "/HospitalNetworkPrefabs.asset";
            var list = SerializedWiring.LoadOrCreateAsset<NetworkPrefabsList>(path);
            foreach (var e in catalog.Entries)
            {
                if (! list.Contains(e.prefab))
                {
                    list.Add(new NetworkPrefab
                    {
                        Prefab = e.prefab
                    });
                }
            }
            if (! list.Contains(run))
            {
                list.Add(new NetworkPrefab
                {
                    Prefab = run
                });
            }
            EditorUtility.SetDirty(list);
            return list;
        }
    }
}
