using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Loot;
using Abandoned.Structure;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallPopulator
    {
        public const float DefaultStability=.75f;
        public const int DefaultSeed=1;
        public static float TruckDoorZ=>LoadingDoor.z;
        public static Vector3 SpawnPosition=>SpawnCentre;
        public static readonly float[] SpawnOffsetsX={0,1.5f,-1.5f,3};
        private static readonly Dictionary<string,(float health,float capacity)> WeakSpots=new() {
            ["Floor_2_4_9"]=(.65f,.55f),["Floor_2_5_9"]=(.60f,.5f),["Floor_2_6_9"]=(.65f,.5f),
            ["Walk_1_6_6"]=(.8f,.6f),["Walk_1_7_6"]=(.8f,.6f)
        };
        public static void Populate(Transform mall)
        {
            new GameObject("DebugView",typeof(DebugViewToggle),typeof(PerfOverlay));
            Camera camera=Object.FindFirstObjectByType<Camera>();
            if(camera!=null&&camera.GetComponent<CinemachineBrain>()==null)camera.gameObject.AddComponent<CinemachineBrain>();
            Transform spawns=GreyboxFactory.Group("Spawns",mall);
            for(int i=0;i<SpawnOffsetsX.Length;i++) {
                Transform spawn=GreyboxFactory.Group($"PlayerSpawn_{i}",spawns);
                Vector3 position=SpawnCentre+new Vector3(SpawnOffsetsX[i],0,(i%2)*.7f);
                spawn.SetPositionAndRotation(position,Quaternion.LookRotation(Vector3.ProjectOnPlane(EntranceTarget-position,Vector3.up)));
                spawn.gameObject.AddComponent<Abandoned.Player.PlayerSpawnPoint>().EditorSetup(i);
            }
            var bootstrap=NetworkSceneBuilder.Add();AddStructure(mall);
            MallLootPoints.Place(GreyboxFactory.Group("LootPoints",mall));
            var spawner=new GameObject("LootSpawner").AddComponent<Abandoned.Networking.NetworkLootSpawner>();
            spawner.Setup(bootstrap,AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath),
                AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath),autoSpawn:false);
            TruckBuilder.Build(GreyboxFactory.Group("Truck",mall),TruckRear,Vector3.right);
            var run=new GameObject("Run");
            var director=run.AddComponent<Abandoned.Extraction.RunDirector>();
            director.Setup(bootstrap,AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.RunStatePrefabPath).GetComponent<Unity.Netcode.NetworkObject>(),
                spawner,Object.FindAnyObjectByType<StructureSimulation>());
            run.AddComponent<Abandoned.Extraction.RunHud>();run.AddComponent<Abandoned.Extraction.PowerController>();
            SerializedWiring.Set(run.AddComponent<Abandoned.Extraction.AppraisalScreen>(),"director",director);
            PlaceThreats(mall,bootstrap);
            var views=new GameObject("DebugViews");views.AddComponent<NoiseDebugView>();views.AddComponent<LootDebugView>();views.AddComponent<Abandoned.Interaction.SharedCarryDebugView>();
        }
        private static void AddStructure(Transform mall)
        {
            MallDamageBuilder.Reset();var setup=new StructureSceneSetup();if(!setup.IsValid)throw new System.InvalidOperationException("Missing structure data.");
            var matched=new HashSet<string>();
            for(int f=-1;f<Floors;f++) foreach(Transform tile in mall.Find($"Tiles/Floor_{f}")) {
                var c=new Vector2Int(Mathf.FloorToInt(tile.position.x/Tile),Mathf.FloorToInt(tile.position.z/Tile));
                (float health,float capacity)=(1,1);
                if(WeakSpots.TryGetValue(tile.name,out var weak)){(health,capacity)=weak;matched.Add(tile.name);}
                bool collapse=!IsProtected(c,f)&&(f>0||f==0&&MallBasementBuilder.AboveBasement(tile.position));
                if(MeridianArt.UseKit)MallDamageBuilder.Floor(setup,tile,IsWalkwayTile(c,f)?SectionType.Balcony:SectionType.Floor,collapse,MallFloorBuilder.Finish(c,f),health,capacity);
                else setup.AddSection(tile,IsWalkwayTile(c,f)?SectionType.Balcony:SectionType.Floor,collapse,health,capacity);
                if(collapse)SectionNavCarver.EditorAdd(tile.gameObject,new Vector3(Tile,.3f,Tile));
            }
            foreach(Flight f in AllFlights) {
                Transform flight=mall.Find($"Flights/{f.Name}");
                if(MeridianArt.UseKit)MallDamageBuilder.Flight(setup,flight,f);else setup.AddSection(flight,SectionType.Stair,f.CanCollapse,fractured:false);
                flight.gameObject.AddComponent<SurfaceTag>().EditorSet(f.Kind==FlightKind.Stair?SurfaceMaterial.Concrete:SurfaceMaterial.Metal);
                if(!f.CanCollapse)continue;
                SectionNavCarver.EditorAdd(flight.gameObject,new Vector3(f.Width,f.Rise,f.Length));
                var o=flight.GetComponent<UnityEngine.AI.NavMeshObstacle>();o.center=new Vector3(0,.4f+f.Rise/2,f.Length/2);
            }
            foreach(Transform turn in mall.Find("Landings")) {
                if(MeridianArt.UseKit)MallDamageBuilder.Floor(setup,turn,SectionType.Floor,false,"Concrete");else setup.AddSection(turn,SectionType.Floor,false);turn.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            }
            foreach(string weak in WeakSpots.Keys)if(!matched.Contains(weak))throw new System.InvalidOperationException($"Weak spot not built: {weak}");
            setup.AddSimulation(DefaultStability,DefaultSeed);
        }
        private static void PlaceThreats(Transform mall,Abandoned.Networking.NetworkBootstrap bootstrap)
        {
            Transform points=GreyboxFactory.Group("ThreatSpawns",mall);
            foreach(var (f,x,z) in new[]{(0,2,3),(0,2,7),(0,12,5),(0,7,10),(1,2,6),(1,7,1),(1,12,4),(1,10,10),
                (2,2,6),(2,6,2),(2,5,10),(2,12,5),(-1,10,6),(-1,12,7)}) {
                var c=new Vector2Int(x,z);if(ZoneAt(c,f)==null)throw new System.InvalidOperationException($"Threat point over void: {f}/{x}/{z}");
                var p=GreyboxFactory.Group($"ThreatSpawn_{f}_{x}_{z}",points);p.position=TileTopCenter(c,f);p.gameObject.AddComponent<Abandoned.Threats.ThreatSpawnPoint>();
            }
            var threats=new GameObject("Threats");
            Unity.Netcode.NetworkObject Threat(string path)=>AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Unity.Netcode.NetworkObject>();
            var director=threats.AddComponent<Abandoned.Threats.ThreatDirector>();
            director.Setup(bootstrap,Threat(ThreatContentBuilder.BlindOnePrefabPath),new[]{Threat(ThreatContentBuilder.BlindOnePrefabPath),
                Threat(ThreatContentBuilder.StalkerPrefabPath),Threat(ThreatContentBuilder.CollectorPrefabPath),Threat(ThreatContentBuilder.HunterPrefabPath)},new[]{.4f,.2f,.2f,.2f});
            director.SetupCatalog(AssetDatabase.LoadAssetAtPath<Abandoned.Threats.ThreatCatalog>(NewThreatContentBuilder.CatalogPath));
            threats.AddComponent<Abandoned.Threats.DangerDirector>().Setup(AssetDatabase.LoadAssetAtPath<Abandoned.Extraction.DangerConfig>(NetworkContentBuilder.DangerConfigPath),Object.FindAnyObjectByType<StructureSimulation>());
        }
    }
}
