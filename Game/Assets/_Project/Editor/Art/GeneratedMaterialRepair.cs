using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    // Runtime emission must survive URP's material inspector/import keyword rebuild.
    public static class GeneratedMaterialRepair
    {
        public static void Apply(Material material)
        {
            if(material==null||material.shader==null||!material.shader.name.StartsWith("Universal Render Pipeline/"))return;
            if(material.HasProperty("_BaseMap")&&material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex",material.GetTexture("_BaseMap"));
                material.SetTextureScale("_MainTex",material.GetTextureScale("_BaseMap"));
                material.SetTextureOffset("_MainTex",material.GetTextureOffset("_BaseMap"));
            }
            if(material.HasProperty("_BaseColor")&&material.HasProperty("_Color"))material.SetColor("_Color",material.GetColor("_BaseColor"));
            if(material.HasProperty("_EmissionColor"))
            {
                bool glows=material.GetColor("_EmissionColor").maxColorComponent>0.001f;
                material.globalIlluminationFlags=glows?MaterialGlobalIlluminationFlags.RealtimeEmissive:MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                if(glows)material.EnableKeyword("_EMISSION");else material.DisableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(material);
        }
        public static void Rebuild()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Project/Art/Custom","Assets/_Project/Art/Polish","Assets/_Project/Art/Lighting","Assets/_Project/Art/Greybox"}))
                Apply(AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)));
            AssetDatabase.SaveAssets();
        }
    }
}
