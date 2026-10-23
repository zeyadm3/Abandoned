using System.IO;
using System.Linq;
using Abandoned.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Abandoned.EditorTools
{
    public static class SkyscraperReview
    {
        public static void RenderAndExit() => ReleaseKit.RunAndExit(() =>
        {
            EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath, OpenSceneMode.Single);
            DynamicGI.UpdateEnvironment();
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Docs/renders/skyscraper-rebuild"));
            Directory.CreateDirectory(folder);
            var go=new GameObject("ReviewCamera");var cam=go.AddComponent<Camera>();
            cam.fieldOfView=72;cam.nearClipPlane=.05f;cam.farClipPlane=1000;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            cam.GetUniversalAdditionalCameraData().requiresDepthTexture=true;
            cam.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            const string scratch="Assets/_SkyscraperReviewTemp";
            AssetDatabase.DeleteAsset(scratch);
            AssetDatabase.CreateFolder("Assets","_SkyscraperReviewTemp");
            int probeIndex=0;
            var target=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);
            void Shot(string name,Vector3 at,Vector3 toward)
            {
                go.transform.position=at;go.transform.LookAt(toward);
                var initial=new UniversalRenderPipeline.SingleCameraRequest{destination=target};
                if(RenderPipelineManager.currentPipeline==null)RenderPipeline.SubmitRenderRequest(cam,initial);
                foreach(IndirectLighting rig in Object.FindObjectsByType<IndirectLighting>(FindObjectsSortMode.None))rig.Preview(cam);
                foreach(ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)
                    .OrderBy(p=>p.bounds.SqrDistance(at)).ThenBy(p=>(p.transform.position-at).sqrMagnitude).Take(2))
                {
                    if(probe.customBakedTexture!=null && AssetDatabase.GetAssetPath(probe.customBakedTexture).StartsWith(scratch))continue;
                    string path=scratch+"/probe_"+probeIndex+++".exr";
                    probe.mode=ReflectionProbeMode.Realtime;
                    if(Lightmapping.BakeReflectionProbe(probe,path))
                    {
                        AssetDatabase.ImportAsset(path);probe.mode=ReflectionProbeMode.Custom;
                        probe.customBakedTexture=AssetDatabase.LoadAssetAtPath<Texture>(path);
                    }
                }
                VolumeManager.instance.ResetMainStack();
                VolumeManager.instance.Update(cam.transform,cam.GetUniversalAdditionalCameraData().volumeLayerMask);
                var request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};
                for(int i=0;i<4;i++) RenderPipeline.SubmitRenderRequest(cam,request);
                RenderTexture previous=RenderTexture.active;RenderTexture.active=target;
                var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();
                File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
                Object.DestroyImmediate(pixels);RenderTexture.active=previous;
            }
            try
            {
                Shot("01-arrival",new Vector3(-10,1.7f,-25),new Vector3(0,12,-10));
                Shot("02-lobby",new Vector3(0,1.7f,-9),new Vector3(-1,2.2f,-1));
                Shot("03-dining",new Vector3(0,405.7f,5),new Vector3(0,406f,20));
                Shot("04-lounge",new Vector3(6,405.7f,0),new Vector3(16,406f,4));
                Shot("05-kitchen",new Vector3(-10,405.7f,9),new Vector3(-15,405.5f,18));
                Shot("06-stairwell",new Vector3(-10,405.7f,-3.5f),new Vector3(-13.5f,407f,4));
                Shot("07-roof",new Vector3(-7,411.7f,6),new Vector3(0,412f,18));
                Shot("09-cloud-crown",new Vector3(-65,418,-85),new Vector3(0,404f,6));
                Shot("10-tall-tower",new Vector3(-350,220,-370),new Vector3(0,240,25));
                Shot("11-downtown-street",new Vector3(-40,2,-28),new Vector3(-180,80,-28));
                Shot("12-downtown-panorama",new Vector3(0,405.7f,20.5f),new Vector3(20,375,160));
                Shot("08-wine",new Vector3(-11,405.7f,-5.8f),new Vector3(-15,405.6f,-9));
            }
            finally {Object.DestroyImmediate(go);target.Release();Object.DestroyImmediate(target);AssetDatabase.DeleteAsset(scratch);}
            Debug.Log("[SkyscraperReview] Saved twelve review views to "+folder);
        });
    }
}
