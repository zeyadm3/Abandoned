using UnityEditor;
using UnityEngine;
namespace Abandoned.Hospital
{
    public sealed class HospitalPalette
    {
        public readonly Material Stone,
        Bronze,
        Blue,
        Glass,
        White,
        Dark;
        public HospitalPalette(HospitalKit kit)
        {
            Stone = Make(kit.Wall, "CityLimestone", new Color(.82f, .78f, .66f), .22f);
            White = Make(kit.Wall, "CityPorcelain", new Color(.91f, .9f, .84f), .3f);
            Bronze = Make(kit.Metal, "CityChampagne", new Color(.57f, .43f, .23f), .6f);
            Blue = Make(kit.Glass, "CityPrivacyGlazing", new Color(.16f, .36f, .39f), .82f);
            Dark = Make(kit.Dark, "CityShadowJoint", new Color(.07f, .1f, .11f), .15f);
            Glass = Make(kit.Glass, "CityClearGlazing", new Color(.35f, .65f, .68f, .22f), .9f);
            Glass.SetFloat("_Surface", 1f);
            Glass.SetFloat("_ZWrite", 0f);
            Glass.SetFloat("_SrcBlend",(float) UnityEngine.Rendering.BlendMode.SrcAlpha);
            Glass.SetFloat("_DstBlend",(float) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            Glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            Glass.renderQueue = 3000;
            EditorUtility.SetDirty(Glass);
        }
        private static Material Make(Material source, string name, Color tint, float smooth)
        {
            string path = HospitalKit.Art + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source);
                material.name = name;
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Smoothness", smooth);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
