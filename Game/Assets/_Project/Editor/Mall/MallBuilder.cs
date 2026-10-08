using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallBuilder
    {
        public const string ScenePath="Assets/_Project/Scenes/Mall.unity",TilesGroup="Tiles",FlightsGroup="Flights";
        public const string NavMeshPath="Assets/_Project/Scenes/Mall_NavMesh.asset";
        [MenuItem("Tools/Abandoned/Create Mall")]
        public static void Create(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())Build();}
        public static void Build()
        {
            MeridianArt.Prepare();MallGameplayData.Apply();
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            Transform mall=new GameObject("Mall").transform;
            mall.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(BuildingBounds);
            int tiles=MallFloorBuilder.Build(Group(TilesGroup,mall));
            Transform flights=Group(FlightsGroup,mall);
            foreach(Flight flight in AllFlights)MallFlights.Build(flight,flights);
            MallFlights.Landings(Group("Landings",mall));
            MallArchitectureBuilder.Build(mall);
            MallExteriorBuilder.Build(mall);MallExteriorBuilder.Exits(mall);
            MallPopulator.Populate(mall);
            var setup=new StructureSceneSetup();
            foreach(Transform e in mall.Find("Exits")) {
                if(MeridianArt.UseKit)MallDamageBuilder.Floor(setup,e.Find("Ledge"),SectionType.Balcony,false,"Concrete");
                else setup.AddSection(e.Find("Ledge"),SectionType.Balcony,false);
            }
            MallArchitectureBuilder.BindRails(mall);
            MeridianAtmosphere.Apply(mall);
            if(MeridianArt.UseKit)MeridianArchitectureDetails.Place(mall);
            if(MeridianArt.Dress)MeridianDressing.Place(mall);else MallArchitectureBuilder.Ceilings(mall);
            int fixtures=MeridianArt.Illuminate?MeridianLighting.Place(mall):MallFixtures.Place(Group("Lights",mall),mall.Find(TilesGroup));
            MallSurfaceBuilder.Assign(mall);
            BakeNavMesh(mall);
            int shutters=MallShutters.Place(mall);
            int problems=MallFlightValidator.Run(mall)+InvisibleColliderAudit.Run(mall,"Mall");
            if(problems!=0)throw new System.InvalidOperationException($"Mall rebuild has {problems} validation problem(s).");
            EditorSceneManager.SaveScene(scene,ScenePath);BuildScenes.ApplyToEditorSettings();NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Meridian] Saved shaped mall: {tiles} tiles, {System.Linq.Enumerable.Count(AllFlights)} flights, {fixtures} fixtures, {shutters} shutters.");
        }
        public static string TileName(Vector2Int c,int floor)=>$"{(IsWalkwayTile(c,floor)?"Walk":"Floor")}_{floor}_{c.x}_{c.y}";
        private static void BakeNavMesh(Transform mall)
        {
            var surface=mall.gameObject.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects=Unity.AI.Navigation.CollectObjects.Children;surface.useGeometry=UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            surface.ignoreNavMeshObstacle=false;
            surface.layerMask=~LayerMask.GetMask(Abandoned.Core.GameLayers.Player,Abandoned.Core.GameLayers.Loot,Abandoned.Core.GameLayers.Debris,"Ignore Raycast");
            foreach(string name in new[]{"Exterior","Truck","Roof","Exits"})mall.Find(name).gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild=true;
            surface.BuildNavMesh();
            // Reuse the data asset GUID so repeated rebuilds do not invalidate external references.
            var previous=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(NavMeshPath);
            if(previous!=null){surface.RemoveData();EditorUtility.CopySerialized(surface.navMeshData,previous);Object.DestroyImmediate(surface.navMeshData);surface.navMeshData=previous;surface.AddData();EditorUtility.SetDirty(previous);}
            else AssetDatabase.CreateAsset(surface.navMeshData,NavMeshPath);
        }
    }
}
