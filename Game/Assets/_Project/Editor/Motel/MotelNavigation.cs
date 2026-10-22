using Abandoned.Core;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Motel
{
    public static class MotelNavigation
    {
        public static void Build(Transform root)
        {
            var surface=root.gameObject.AddComponent<NavMeshSurface>();surface.collectObjects=CollectObjects.Children;
            surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.ignoreNavMeshObstacle=false;
            surface.layerMask=~LayerMask.GetMask(GameLayers.Player,GameLayers.Loot,GameLayers.Debris,"Ignore Raycast");
            Transform roof=root.Find("Roof");if(roof!=null)Modifier(roof.gameObject).ignoreFromBuild=true;
            // Fitted furniture is solid cover, never an alternative walkable floor on top of a cabinet.
            foreach(var prop in root.GetComponentsInChildren<Abandoned.Structure.SectionProp>())
            {
                var mod=Modifier(prop.gameObject);mod.overrideArea=true;mod.area=1;
            }
            surface.BuildNavMesh();
            string path=MotelKit.Art+"/MotelNavigation.asset";
            var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if(saved==null)AssetDatabase.CreateAsset(surface.navMeshData,path);
            else
            {
                surface.RemoveData();var generated=surface.navMeshData;EditorUtility.CopySerialized(generated,saved);
                Object.DestroyImmediate(generated);surface.navMeshData=saved;surface.AddData();EditorUtility.SetDirty(saved);
            }
        }

        // The builder bakes twice (before and after the solid-props pass); keep one modifier per object.
        private static NavMeshModifier Modifier(GameObject go)=>go.TryGetComponent(out NavMeshModifier mod)?mod:go.AddComponent<NavMeshModifier>();
    }
}
