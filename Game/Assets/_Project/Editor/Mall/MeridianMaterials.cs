using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    public static class MeridianMaterials
    {
        private static readonly Dictionary<string,Material> Cache=new();
        private static readonly Dictionary<string,string> Aliases=new() {
            ["Bridge"]="BridgeMetal",["Metal"]="EscalatorSteel",["SignFace"]="FixtureTrim",
            ["PaperAge"]="BoardWood",["Stain"]="Moss",["DecalGraffiti"]="Graffiti",
            ["FluorescentGlow"]="FixtureGlow",["Brass"]="EscalatorSteel",["Trim"]="StorefrontTrim",
            ["Cloth"]="Upholstery",["Black"]="Carpet",["WaterStain"]="Moss"
        };
        private static readonly string[] Sets={"Terrazzo","Plaster","StorefrontTrim","EscalatorSteel","Ceiling","Tile","Carpet","Concrete","Wood",
            "BridgeMetal","Block","Asphalt","FacadeStone","Rust","FixtureTrim","Upholstery","BoardWood","Moss","Leaves","Graffiti","Water","FixtureGlow","Glass","Crack","Scorch"};
        public static void Prepare() { Cache.Clear(); foreach(string key in Sets) Get(key); }
        public static Material Get(string key)
        {
            key=key.Replace("MALL_","").Replace(" (Instance)","");
            key=Aliases.GetValueOrDefault(key,key);
            if(Cache.TryGetValue(key,out var existing)&&existing!=null) return existing;
            string folder=MeridianArt.Folder+"/Materials",path=$"{folder}/MALL_{key}.mat";
            PolishAssets.EnsureFolder(folder);
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null) {m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="MALL_"+key};AssetDatabase.CreateAsset(m,path);}
            m.shader=Shader.Find("Universal Render Pipeline/Lit");
            string stem=$"{MeridianArt.Folder}/Textures/MALL_{key}";
            Texture2D colour=AssetDatabase.LoadAssetAtPath<Texture2D>(stem+"_BaseColor.png");
            Texture2D normal=AssetDatabase.LoadAssetAtPath<Texture2D>(stem+"_Normal.png");
            Texture2D mask=AssetDatabase.LoadAssetAtPath<Texture2D>(stem+"_Mask.png");
            m.SetTexture("_BaseMap",colour!=null?colour:PolishAssets.Texture("MeridianGreybox"));
            m.SetColor("_BaseColor",colour!=null?Color.white:GreyColour(key));
            m.SetTexture("_BumpMap",normal);m.SetFloat("_BumpScale",1);
            m.SetTexture("_MetallicGlossMap",mask);m.SetTexture("_OcclusionMap",mask);
            m.SetFloat("_Smoothness",mask!=null?1:.25f);m.SetFloat("_OcclusionStrength",1);
            Keyword(m,"_NORMALMAP",normal!=null);Keyword(m,"_METALLICSPECGLOSSMAP",mask!=null);Keyword(m,"_OCCLUSIONMAP",mask!=null);
            m.enableInstancing=true;m.SetFloat("_Cull",(float)CullMode.Back);
            bool transparent=key=="Glass"||key=="Water";
            bool cutout=key=="Moss"||key=="Leaves"||key=="Graffiti"||key=="Scorch";
            m.SetFloat("_Surface",transparent?1:0);m.SetFloat("_Blend",0);m.SetFloat("_AlphaClip",cutout?1:0);
            m.SetFloat("_Cutoff",.25f);m.SetFloat("_ZWrite",transparent?0:1);
            m.SetFloat("_SrcBlend",(float)(transparent?BlendMode.SrcAlpha:BlendMode.One));
            m.SetFloat("_DstBlend",(float)(transparent?BlendMode.OneMinusSrcAlpha:BlendMode.Zero));
            Keyword(m,"_SURFACE_TYPE_TRANSPARENT",transparent);Keyword(m,"_ALPHATEST_ON",cutout);
            m.SetOverrideTag("RenderType",transparent?"Transparent":cutout?"TransparentCutout":"Opaque");
            m.renderQueue=(int)(transparent?RenderQueue.Transparent:cutout?RenderQueue.AlphaTest:RenderQueue.Geometry);
            m.SetShaderPassEnabled("ShadowCaster",!transparent);
            if(key=="FixtureGlow") {m.SetColor("_EmissionColor",new Color(.72f,.85f,.74f)*2);m.EnableKeyword("_EMISSION");m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;}
            GeneratedMaterialRepair.Apply(m);EditorUtility.SetDirty(m);Cache[key]=m;return m;
        }
        private static void Keyword(Material m,string keyword,bool on) {if(on)m.EnableKeyword(keyword);else m.DisableKeyword(keyword);}
        private static Color GreyColour(string key) => key switch {
            "Terrazzo"=>new Color(.56f,.53f,.45f),"Carpet"=>new Color(.25f,.13f,.16f),"Asphalt"=>new Color(.11f,.12f,.12f),
            "Block"=>new Color(.31f,.36f,.33f),"Wood"=>new Color(.31f,.23f,.16f),"Water"=>new Color(.08f,.16f,.14f),
            _=>new Color(.42f,.44f,.41f)};
    }
}
