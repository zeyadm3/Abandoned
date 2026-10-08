using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public sealed class MeridianArtImport : AssetPostprocessor
    {
        private bool Meridian => assetPath.StartsWith(MeridianArt.Folder+"/");
        public override uint GetVersion()=>1;
        private void OnPreprocessModel()
        {
            if(!Meridian) return;
            var m=(ModelImporter)assetImporter;
            m.globalScale=1;m.useFileScale=true;m.bakeAxisConversion=true;m.importAnimation=false;
            m.animationType=ModelImporterAnimationType.None;m.importCameras=false;m.importLights=false;
            m.addCollider=false;m.isReadable=false;m.importNormals=ModelImporterNormals.Import;
            m.importTangents=ModelImporterTangents.CalculateMikk;m.meshCompression=ModelImporterMeshCompression.Low;
            m.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;m.materialLocation=ModelImporterMaterialLocation.InPrefab;
        }
        private void OnPreprocessTexture()
        {
            if(!Meridian) return;
            var t=(TextureImporter)assetImporter;
            bool normal=assetPath.EndsWith("_Normal.png"),mask=assetPath.EndsWith("_Mask.png");
            t.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
            t.sRGBTexture=!normal&&!mask;t.mipmapEnabled=true;t.isReadable=false;t.wrapMode=TextureWrapMode.Repeat;
            t.filterMode=FilterMode.Trilinear;t.textureCompression=TextureImporterCompression.CompressedHQ;
            t.maxTextureSize=SizeOf(assetPath);
            var desktop=t.GetPlatformTextureSettings("Standalone");
            desktop.overridden=true;desktop.maxTextureSize=t.maxTextureSize;
            desktop.format=normal?TextureImporterFormat.BC5:TextureImporterFormat.BC7;
            desktop.compressionQuality=100;t.SetPlatformTextureSettings(desktop);
        }
        private static int SizeOf(string path)
        {
            foreach(string hero in new[]{"Terrazzo_","Plaster_","StorefrontTrim_","EscalatorSteel_","Ceiling_"})
                if(path.Contains("MALL_"+hero)) return 2048;
            foreach(string small in new[]{"FixtureTrim_","Upholstery_","BoardWood_","Moss_","Leaves_","Graffiti_","Water_","FixtureGlow_","Glass_","Crack_","Scorch_"})
                if(path.Contains("MALL_"+small)) return 512;
            return 1024;
        }
        private Material OnAssignMaterialModel(Material source,Renderer renderer) => Meridian&&source!=null&&source.name.StartsWith("MALL_")?
            AssetDatabase.LoadAssetAtPath<Material>($"{MeridianArt.Folder}/Materials/{source.name}.mat"):null;
    }
}
