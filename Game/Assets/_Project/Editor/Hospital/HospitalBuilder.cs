using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Hospital
{
    public static class HospitalBuilder
    {
        public const string ScenePath=HospitalLayout.ScenePath;
        [MenuItem(ToolMenu.Hospital+"Rebuild Saint Brigid Hospital",priority=ToolMenu.HospitalP)]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Rebuild Hospital in Edit mode.");
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var kit=new HospitalKit();LootCatalog catalog=HospitalLoot.Build(kit);
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Transform root=new GameObject("Hospital").transform;
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(HospitalLayout.Bounds);
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(HospitalLayout.Bounds,new Color(.14f,.18f,.16f),.009f);
            var setup=new StructureSceneSetup();if(!setup.IsValid)throw new InvalidOperationException("Hospital needs the shared structure content.");
            var floors=new HospitalStructure(kit,setup);floors.Build(root);
            HospitalShell.Build(root,kit,floors);HospitalSite.Build(root,kit);
            List<LootPointSpec> loot=HospitalInteriors.Build(root,kit,floors);
            HorrorConfig horror=HospitalServices.Build(root,kit,floors);
            GameObject runPrefab=HospitalLoot.RunPrefab(horror);
            StructureSimulation sim=setup.AddSimulation(.72f,17023);
            var threats=new List<Vector3>{new(10,0,30),new(40,0,32),new(36,0,12),new(24,0,35),
                new(10,4,30),new(38,4,32),new(12,4,12),new(36,4,12),new(24,4,28),new(24,4,36)};
            LocationPlumbing.Populate(root,HospitalLayout.Crew,loot,threats,sim);
            var bootstrap=UnityEngine.Object.FindFirstObjectByType<NetworkBootstrap>();
            var spawner=UnityEngine.Object.FindFirstObjectByType<NetworkLootSpawner>();
            spawner.Setup(bootstrap,catalog,AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath),false);
            var director=UnityEngine.Object.FindFirstObjectByType<RunDirector>();director.Setup(bootstrap,runPrefab.GetComponent<NetworkObject>(),spawner,sim);
            SerializedWiring.Set(UnityEngine.Object.FindFirstObjectByType<PowerController>(),"sectorsWithoutHorror",horror);
            // Give a standalone scene the same pre-registered content as the persistent world session.
            bootstrap.Manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(PrefabList(catalog,runPrefab));
            var location=SerializedWiring.LoadOrCreateAsset<LocationDefinition>(HospitalLoot.Data+"/HospitalLocation.asset");
            location.EditorSetup("hospital","Saint Brigid Hospital","Two clinical floors, failing wards and heavy diagnostic salvage.","Hospital",3,1,1.2f,1.1f,-.05f,1.15f,.1f);
            EditorUtility.SetDirty(location);
            foreach(GameObject go in scene.GetRootGameObjects())
                if(go.transform!=root&&go.name!="NetworkManager"&&go.name!="Network")go.transform.SetParent(root,true);
            HospitalNavigation.Build(root);
            BuildScenes.ApplyToEditorSettings();NetworkObjectIds.StampScene(scene);AssetDatabase.SaveAssets();
            if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new InvalidOperationException("Hospital scene could not be saved.");
            Debug.Log("[Hospital] Authored Saint Brigid Hospital. Review instructions: Docs/HOSPITAL_PLAN.md.");
        }
        private static NetworkPrefabsList PrefabList(LootCatalog catalog,GameObject run)
        {
            string path=HospitalKit.Art+"/HospitalNetworkPrefabs.asset";
            var list=SerializedWiring.LoadOrCreateAsset<NetworkPrefabsList>(path);
            foreach(var e in catalog.Entries)if(!list.Contains(e.prefab))list.Add(new NetworkPrefab{Prefab=e.prefab});
            if(!list.Contains(run))list.Add(new NetworkPrefab{Prefab=run});EditorUtility.SetDirty(list);return list;
        }
    }
}
