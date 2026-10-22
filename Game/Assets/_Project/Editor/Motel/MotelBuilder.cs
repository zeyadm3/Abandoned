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

namespace Abandoned.Motel
{
    public static class MotelBuilder
    {
        public const string ScenePath=MotelLayout.ScenePath;
        [MenuItem(ToolMenu.Motel+"Rebuild Last Light Motor Lodge",priority=ToolMenu.MotelP)]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Rebuild Motel in Edit mode.");
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var kit=new MotelKit();LootCatalog catalog=MotelLoot.Build(kit);
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Transform root=new GameObject("Motel").transform;
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(MotelLayout.Bounds);
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(MotelLayout.Bounds,new Color(.18f,.13f,.1f),.008f);
            var setup=new StructureSceneSetup();if(!setup.IsValid)throw new InvalidOperationException("Motel needs the shared structure content.");
            var floors=new MotelStructure(kit,setup);floors.Build(root);
            MotelShell.Build(root,kit,floors);MotelSite.Build(root,kit);
            List<LootPointSpec> loot=MotelInteriors.Build(root,kit,floors);
            HorrorConfig horror=MotelServices.Build(root,kit,floors);
            GameObject runPrefab=MotelLoot.RunPrefab(horror);
            StructureSimulation sim=setup.AddSimulation(.76f,17523);
            var threats=new List<Vector3>();
            foreach(int room in new[]{0,2,3,4,5,7})for(int level=0;level<2;level++)
                threats.Add(new Vector3(MotelLayout.RoomX(room)+3.5f,level*4,17.4f));
            LocationPlumbing.Populate(root,MotelLayout.Crew,loot,threats,sim);
            var bootstrap=UnityEngine.Object.FindFirstObjectByType<NetworkBootstrap>();
            var spawner=UnityEngine.Object.FindFirstObjectByType<NetworkLootSpawner>();
            spawner.Setup(bootstrap,catalog,AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath),false);
            var director=UnityEngine.Object.FindFirstObjectByType<RunDirector>();director.Setup(bootstrap,runPrefab.GetComponent<NetworkObject>(),spawner,sim);
            SerializedWiring.Set(UnityEngine.Object.FindFirstObjectByType<PowerController>(),"sectorsWithoutHorror",horror);
            bootstrap.Manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(PrefabList(catalog,runPrefab));
            var location=SerializedWiring.LoadOrCreateAsset<LocationDefinition>(MotelLoot.Data+"/MotelLocation.asset");
            location.EditorSetup("motel","Last Light Motor Lodge","Sixteen guest rooms around a drained pool; two protected gallery exits.","Motel",2,1,1.05f,1f,-.025f,1.1f,.05f);
            EditorUtility.SetDirty(location);
            foreach(GameObject go in scene.GetRootGameObjects())
                if(go.transform!=root&&go.name!="NetworkManager"&&go.name!="Network")go.transform.SetParent(root,true);
            MotelNavigation.Build(root);
            BuildScenes.ApplyToEditorSettings();NetworkObjectIds.StampScene(scene);AssetDatabase.SaveAssets();
            if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new InvalidOperationException("Motel scene could not be saved.");
            Debug.Log("[Motel] Authored Last Light Motor Lodge. Review instructions: Docs/MOTEL_PLAN.md.");
        }
        private static NetworkPrefabsList PrefabList(LootCatalog catalog,GameObject run)
        {
            var list=SerializedWiring.LoadOrCreateAsset<NetworkPrefabsList>(MotelKit.Art+"/MotelNetworkPrefabs.asset");
            foreach(var e in catalog.Entries)if(!list.Contains(e.prefab))list.Add(new NetworkPrefab{Prefab=e.prefab});
            if(!list.Contains(run))list.Add(new NetworkPrefab{Prefab=run});EditorUtility.SetDirty(list);return list;
        }
    }
}
