using System.Linq;
using Abandoned.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Abandoned.EditorTools
{
    /// <summary>Refreshes only Cloudline's atlas look and distant model; only its expanded district terrain is fitted.</summary>
    public static class SkyscraperWorld
    {
        public static void RefreshAndExit() => ReleaseKit.RunAndExit(Refresh);
        public static void Refresh()
        {
            var atlas=AssetDatabase.LoadAssetAtPath<WorldAtlas>(WorldBuilder.AtlasPath);
            if(atlas==null || !System.IO.File.Exists(WorldBuilder.ScenePath))return;
            int index=atlas.IndexOf("Skyscraper");if(index<0)return;
            var scene=EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath,OpenSceneMode.Single);
            atlas=AssetDatabase.LoadAssetAtPath<WorldAtlas>(WorldBuilder.AtlasPath);
            _=new SkyscraperArt();
            WorldPoi poi=atlas.Pois[index];
            poi.look=PlaceLook.Capture();
            poi.ground=SkyscraperDowntown.Ground;
            poi.groundHeight=-.18f;
            poi.hideInWorld=poi.hideInWorld.Where(path=>!path.Contains("SummitClouds") && !path.Contains("Downtown")).ToArray();
            // The distant copy is rebuilt with fresh references in the World scene in the same operation.
            AssetDatabase.DeleteAsset(WorldProxies.Folder+"/Skyscraper");
            WorldProxies.Bake(scene,poi);
            EditorUtility.SetDirty(atlas);AssetDatabase.SaveAssets();
            Scene world=EditorSceneManager.OpenScene(WorldBuilder.ScenePath,OpenSceneMode.Single);
            WorldRoot root=Object.FindFirstObjectByType<WorldRoot>();
            if(root==null)throw new System.InvalidOperationException("WorldRoot is missing from the saved world.");
            var serialized=new SerializedObject(root);
            var sites=serialized.FindProperty("standIns");
            if(index>=sites.arraySize || sites.GetArrayElementAtIndex(index).objectReferenceValue is not GameObject site)
                throw new System.InvalidOperationException("The skyscraper stand-in is missing from the world.");
            Transform old=site.transform.Find("FarCopy");if(old!=null)Object.DestroyImmediate(old.gameObject);
            WorldProxies.Place(site.transform,poi);
            Transform pad=site.transform.Find("Pad");
            if(pad!=null)
            {
                pad.position=poi.worldPosition+new Vector3(poi.ground.center.x,poi.groundHeight-1f,poi.ground.center.y);
                pad.localScale=new Vector3(poi.ground.width,2,poi.ground.height);
                pad.GetComponent<Renderer>().enabled=false;
            }
            SkyscraperWorldTerrain.Fit(poi);
            AssetDatabase.SaveAssets();
            if(!EditorSceneManager.SaveScene(world))throw new System.InvalidOperationException("Could not save the refreshed skyscraper world model.");
            EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath,OpenSceneMode.Single);
            Debug.Log("[Skyscraper] World distant model refreshed to 410m with its downtown district; summit clouds preserved during streaming.");
        }
    }
}
