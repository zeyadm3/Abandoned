using Abandoned.Core;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Hospital
{
    public static class HospitalNavigation
    {
        public static void Build(Transform root)
        {
            var surface=root.gameObject.AddComponent<NavMeshSurface>();surface.collectObjects=CollectObjects.Children;
            surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.ignoreNavMeshObstacle=false;
            surface.layerMask=~LayerMask.GetMask(GameLayers.Player,GameLayers.Loot,GameLayers.Debris,"Ignore Raycast");
            Transform roof=root.Find("Roof");if(roof!=null)roof.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild=true;
            // Fitted furniture is solid cover, never an alternative walkable floor on top of a cabinet.
            foreach(var prop in root.GetComponentsInChildren<Abandoned.Structure.SectionProp>())
            {
                var mod=prop.gameObject.AddComponent<NavMeshModifier>();mod.overrideArea=true;mod.area=1;
            }
            surface.BuildNavMesh();
            string path=HospitalKit.Art+"/HospitalNavigation.asset";
            var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if(saved==null)AssetDatabase.CreateAsset(surface.navMeshData,path);
            else
            {
                surface.RemoveData();var generated=surface.navMeshData;EditorUtility.CopySerialized(generated,saved);
                Object.DestroyImmediate(generated);surface.navMeshData=saved;surface.AddData();EditorUtility.SetDirty(saved);
            }
        }
    }
}
