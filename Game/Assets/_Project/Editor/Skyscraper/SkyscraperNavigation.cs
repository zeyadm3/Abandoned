using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperNavigation
    {
        // A single surface drops stacked stair layers above ~252m. Each bounded bake stays below that span.
        private const float JoinY=201f;
        private static readonly string[] Ignored={"Exterior/Truck","Exterior/Skyline","Exterior/Downtown",
            "Exterior/TowerFacade/PorteCochere","Lobby/LobbyCeiling",
            "IntermediateFloors/FloodedFloor_120m/OfficeCeiling","IntermediateFloors/LootedOffice_280m/OfficeCeiling",
            "MechanicalFloor_200m/MechanicalCeiling","Lights","ElevatorShaft/ShaftHoist","ElevatorShaft/ElevatorCab"};
        public static void Build(Transform root)
        {
            foreach(string path in Ignored)
            {
                Transform t=root.Find(path);if(t==null)continue;
                var modifier=Abandoned.EditorTools.SerializedWiring.GetOrAdd<NavMeshModifier>(t.gameObject);modifier.ignoreFromBuild=true;
            }
            var lower=Abandoned.EditorTools.SerializedWiring.GetOrAdd<NavMeshSurface>(root.gameObject);
            Transform upperRoot=root.Find("UpperNavigation")??Group("UpperNavigation",root);
            var upper=Abandoned.EditorTools.SerializedWiring.GetOrAdd<NavMeshSurface>(upperRoot.gameObject);
            Bake(lower,-4f,JoinY,"Assets/_Project/Scenes/Skyscraper_NavMesh.asset");
            Bake(upper,JoinY,SkyscraperLayout.RoofY+6f,"Assets/_Project/Scenes/Skyscraper_Upper_NavMesh.asset");
            Transform linkRoot=root.Find("StairNavigationJoin")??Group("StairNavigationJoin",root);
            var link=Abandoned.EditorTools.SerializedWiring.GetOrAdd<NavMeshLink>(linkRoot.gameObject);
            // Whole intersecting ramp colliders appear in both bakes. Use the two unique end landings,
            // across this one continuous flight, so endpoint selection survives scene reloads.
            link.startPoint=new Vector3(-13.65f,200f,-3.5f);
            link.endPoint=new Vector3(-13.65f,202f,5.5f);
            link.width=0f;link.bidirectional=true;link.agentTypeID=lower.agentTypeID;link.UpdateLink();
            Debug.Log("[SkyscraperNav] Two vertical navigation surfaces joined along the mechanical-floor stair flight.");
        }
        private static void Bake(NavMeshSurface surface,float bottom,float top,string path)
        {
            surface.collectObjects=CollectObjects.Volume;
            surface.center=new Vector3(0,(bottom+top)*.5f,-5);
            surface.size=new Vector3(60,top-bottom,70);
            surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;
            surface.ignoreNavMeshObstacle=false;
            surface.layerMask=~LayerMask.GetMask(Abandoned.Core.GameLayers.Player,Abandoned.Core.GameLayers.Loot,Abandoned.Core.GameLayers.Debris,"Ignore Raycast");
            surface.overrideTileSize=false;
            // Assign short colliders to one band only. Volume collection includes entire intersecting
            // ramp shapes, producing overlapping islands that a persisted link can bind to incorrectly.
            var root=surface.transform.root;
            var ignored=Ignored.Select(path=>root.Find(path)).Where(t=>t!=null).ToArray();
            var sources=new List<NavMeshBuildSource>();
            Physics.SyncTransforms();
            foreach(Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if(!collider.enabled || collider.isTrigger || !collider.gameObject.activeInHierarchy)continue;
                if((surface.layerMask.value & (1<<collider.gameObject.layer))==0)continue;
                if(ignored.Any(t=>collider.transform==t || collider.transform.IsChildOf(t)))continue;
                Bounds bounds=collider.bounds;
                bool lower=bottom<0;
                if(bounds.size.y<=5f && (bounds.center.y<=JoinY)!=lower)continue;
                if(bounds.max.y<bottom || bounds.min.y>top)continue;
                var source=new NavMeshBuildSource{area=0};
                if(bounds.size.y>5f)
                {
                    float min=Mathf.Max(bottom,bounds.min.y),max=Mathf.Min(top,bounds.max.y);
                    source.shape=NavMeshBuildSourceShape.Box;
                    source.transform=Matrix4x4.TRS(new Vector3(bounds.center.x,(min+max)*.5f,bounds.center.z),Quaternion.identity,Vector3.one);
                    source.size=new Vector3(bounds.size.x,max-min,bounds.size.z);
                }
                else if(collider is BoxCollider box)
                {
                    source.shape=NavMeshBuildSourceShape.Box;source.size=box.size;
                    source.transform=box.transform.localToWorldMatrix*Matrix4x4.Translate(box.center);
                }
                else if(collider is MeshCollider mesh && mesh.sharedMesh!=null)
                {
                    source.shape=NavMeshBuildSourceShape.Mesh;source.sourceObject=mesh.sharedMesh;
                    source.transform=mesh.transform.localToWorldMatrix;
                }
                else
                {
                    source.shape=NavMeshBuildSourceShape.Box;source.size=bounds.size;
                    source.transform=Matrix4x4.TRS(bounds.center,Quaternion.identity,Vector3.one);
                }
                sources.Add(source);
            }
            var baked=UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(),sources,new Bounds(surface.center,surface.size),Vector3.zero,Quaternion.identity);
            surface.RemoveData();
            surface.navMeshData=baked;surface.AddData();
            Debug.Log($"[SkyscraperNav] {bottom}–{top}m: {sources.Count} explicitly partitioned collider sources.");
            var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if(saved==null){baked.name=System.IO.Path.GetFileNameWithoutExtension(path);AssetDatabase.CreateAsset(baked,path);}
            else
            {
                surface.RemoveData();EditorUtility.CopySerialized(baked,saved);Object.DestroyImmediate(baked);
                surface.navMeshData=saved;surface.AddData();EditorUtility.SetDirty(saved);
            }
        }
        public static void CheckSavedAndExit()=>ReleaseKit.RunAndExit(()=>
        {
            EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath,OpenSceneMode.Single);
            foreach(float y in new[]{120f,200f,280f,404f,SkyscraperLayout.UpperRestaurantY,SkyscraperLayout.RoofY})
            {
                bool found=NavMesh.SamplePosition(new Vector3(-7,y,y==SkyscraperLayout.UpperRestaurantY?5.5f:-3.5f),out NavMeshHit end,.65f,NavMesh.AllAreas);
                NavMesh.SamplePosition(new Vector3(-4,0,-18),out NavMeshHit start,.65f,NavMesh.AllAreas);
                var path=new NavMeshPath();NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path);
                if(!found || path.status!=NavMeshPathStatus.PathComplete)
                    throw new System.InvalidOperationException("Saved skyscraper route is disconnected at "+y+"m.");
            }
            Debug.Log("[SkyscraperNav] Reloaded scene: complete street routes to both restaurant floors and the 416m roof.");
        });
        public static void RebakeAndExit()=>ReleaseKit.RunAndExit(()=>
        {
            var scene=EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath,OpenSceneMode.Single);
            Transform root=GameObject.Find("Skyscraper").transform;
            Build(root);SkyscraperClouds.ResizeExisting(root);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
            foreach(float y in new[]{120f,200f,280f,404f,SkyscraperLayout.UpperRestaurantY,SkyscraperLayout.RoofY})
            {
                bool sampled=NavMesh.SamplePosition(new Vector3(-7,y,y==SkyscraperLayout.UpperRestaurantY?5.5f:-3.5f),out NavMeshHit end,.65f,NavMesh.AllAreas);
                NavMesh.SamplePosition(new Vector3(-4,0,-18),out NavMeshHit start,.65f,NavMesh.AllAreas);
                var path=new NavMeshPath();NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path);
                Debug.Log($"[SkyscraperNav] Destination {y}: sampled={sampled}, path={path.status}.");
            }
        });
    }
}
