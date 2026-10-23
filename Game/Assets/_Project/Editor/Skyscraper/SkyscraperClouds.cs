using Abandoned.Skyscraper;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperClouds
    {
        public static void ResizeExisting(Transform root)
        {
            var cloud=root.GetComponentInChildren<SkyscraperCloudLayer>();
            if(cloud==null)return;
            cloud.transform.localScale=new Vector3(1200,180,1200);
            cloud.EditorSetup(root);
        }
        public static void Build(Transform root)
        {
            const string folder=SkyscraperArt.Folder;
            const string noisePath=folder+"/SummitCloudNoise.asset";
            Texture3D noise=AssetDatabase.LoadAssetAtPath<Texture3D>(noisePath);
            if(noise==null)
            {
                const int size=64;
                noise=new Texture3D(size,size,size,TextureFormat.R8,true){name="SummitCloudNoise",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};
                var pixels=new Color[size*size*size];
                for(int z=0;z<size;z++)for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    // Periodic projections avoid a hard seam when the wind wraps the noise volume.
                    float u=x*Mathf.PI*2/size,v=y*Mathf.PI*2/size,w=z*Mathf.PI*2/size;
                    float n=.5f*Mathf.PerlinNoise(5+Mathf.Cos(u)*1.5f+Mathf.Sin(w),5+Mathf.Sin(u)*1.5f+Mathf.Cos(v))
                        +.5f*Mathf.PerlinNoise(12+Mathf.Cos(v)*1.7f+Mathf.Sin(u),12+Mathf.Sin(v)*1.7f+Mathf.Cos(w));
                    pixels[x+size*(y+size*z)]=new Color(n,n,n,1);
                }
                noise.SetPixels(pixels);noise.Apply(true,true);AssetDatabase.CreateAsset(noise,noisePath);
            }
            string path=folder+"/Materials/Summit_Clouds.mat";
            Material material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Abandoned/SummitClouds"));AssetDatabase.CreateAsset(material,path);}
            material.SetTexture("_CloudNoise",noise);
            Transform parent=Group("SummitClouds",root);
            GameObject volume=Box("CloudBank",parent,new Vector3(0,SkyscraperLayout.RestaurantY-40f,6),new Vector3(1200,180,1200),material,false);
            var renderer=volume.GetComponent<Renderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            volume.AddComponent<SkyscraperCloudLayer>().EditorSetup(root);
            EditorUtility.SetDirty(material);
        }
    }
}
