using UnityEditor;
using UnityEngine;
namespace Abandoned.EditorTools
{
    public sealed class SkyscraperDowntownMaterials
    {
        public const string Folder=SkyscraperArt.Folder+"/Downtown";
        public readonly Material[] Facades=new Material[4];
        public readonly Material Limestone,Sidewalk,Shop,WindowLight;
        public SkyscraperDowntownMaterials()
        {
            PolishAssets.EnsureFolder(Folder);
            var colours=new[]{new Color(.18f,.27f,.34f),new Color(.29f,.25f,.19f),new Color(.15f,.21f,.24f),new Color(.36f,.38f,.4f)};
            for(int style=0;style<4;style++)
            {
                Facades[style]=Mat("DowntownFacade_"+style,colours[style]);
                Facades[style].SetColor("_BaseColor",Color.white);
                Facades[style].SetFloat("_Metallic",.25f);Facades[style].SetFloat("_Smoothness",.65f);
                Facades[style].SetTexture("_BaseMap",Atlas(style,colours[style],false));
                Facades[style].SetTexture("_EmissionMap",Atlas(style,colours[style],true));
                Facades[style].EnableKeyword("_EMISSION");Facades[style].SetColor("_EmissionColor",Color.white);
                EditorUtility.SetDirty(Facades[style]);
            }
            Limestone=Mat("DowntownLimestone",new Color(.48f,.46f,.41f));
            Sidewalk=Mat("DowntownPavement",new Color(.32f,.34f,.35f));
            Shop=Mat("DowntownShopfront",new Color(.12f,.18f,.21f));
            Shop.EnableKeyword("_EMISSION");Shop.SetColor("_EmissionColor",new Color(.025f,.04f,.05f));
            WindowLight=Mat("DowntownWarmLamp",new Color(.8f,.66f,.39f));
            WindowLight.EnableKeyword("_EMISSION");WindowLight.SetColor("_EmissionColor",new Color(1.2f,.72f,.3f));
        }
        private static Material Mat(string name,Color colour)
        {
            string path=Folder+"/"+name+".mat";
            Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",colour);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
        }
        private static Texture2D Atlas(int style,Color colour,bool emissive)
        {
            string path=Folder+$"/Facade_{style}_{(emissive?"Emission":"Colour")}.asset";
            Texture2D texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture!=null)return texture;
            const int width=128,height=256;
            texture=new Texture2D(width,height,TextureFormat.RGB24,true){name=System.IO.Path.GetFileNameWithoutExtension(path),wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                int col=x/32,row=y/32,px=x%32,py=y%32;
                bool frame=px<2 || px>29 || py<3 || py>25;
                bool lit=(col*13+row*7+style*3)%9==0;
                float variation=.72f+((col*11+row*17+style*5)%13)*.035f;
                Color c;
                if(emissive)c=frame?Color.black:(lit?new Color(.36f,.24f,.11f):new Color(.025f,.044f,.067f)*variation);
                else c=frame?colour*.34f:(lit?new Color(.47f,.42f,.29f):colour*variation);
                if(!frame && px%9==0)c*=.78f;
                pixels[x+width*y]=c;
            }
            texture.SetPixels(pixels);texture.Apply(true,true);AssetDatabase.CreateAsset(texture,path);return texture;
        }
    }
}
