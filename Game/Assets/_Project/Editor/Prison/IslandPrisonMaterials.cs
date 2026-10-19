using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Prison-owned PBR materials; source maps are read directly without reauthoring the mall's materials.</summary>
    public static class IslandPrisonMaterials
    {
        public enum Surface { Opaque, Transparent }

        public static Material FromMall(string name, string set, Color tint, string folder, Surface surface = Surface.Opaque) =>
            Apply(name, folder, $"{MeridianArt.Folder}/Textures/MALL_{set}", tint, surface);

        public static Material Textured(string name, string set, Color tint, Surface surface, string folder) =>
            Apply(name, folder, $"{IronFactoryTextures.Folder}/{set}", tint, surface);

        private static Material Apply(string name, string folder, string stem, Color tint, Surface surface)
        {
            // A null map silently renders as a flat-colour surface. Keep all three channels bound together.
            Texture2D Read(string channel)
            {
                string path = $"{stem}_{channel}.png";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null) throw new InvalidOperationException($"Prison surface '{name}' needs {path}.");
                return texture;
            }
            Texture2D colour = Read("BaseColor"), normal = Read("Normal"), mask = Read("Mask");
            PolishAssets.EnsureFolder(folder);
            string materialPath = $"{folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The prison needs the URP Lit shader.");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", colour);
            material.SetTextureScale("_BaseMap", Vector2.one);
            material.SetTextureOffset("_BaseMap", Vector2.zero);
            material.SetColor("_BaseColor", tint);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", .8f);
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTexture("_OcclusionMap", mask);
            material.SetFloat("_Smoothness", 1f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetFloat("_OcclusionStrength", .8f);
            material.SetFloat("_WorkflowMode", 1f);
            material.SetColor("_EmissionColor", Color.black);
            material.DisableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_OCCLUSIONMAP");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            bool transparent = surface == Surface.Transparent;
            material.SetFloat("_Surface", transparent ? 1f : 0f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_AlphaClip", 0f);
            material.SetFloat("_ZWrite", transparent ? 0f : 1f);
            material.SetFloat("_SrcBlend", (float)(transparent ? BlendMode.SrcAlpha : BlendMode.One));
            material.SetFloat("_DstBlend", (float)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
            material.SetFloat("_Cull", (float)(transparent ? CullMode.Off : CullMode.Back));
            if (transparent) material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            else material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", transparent ? "Transparent" : "Opaque");
            material.renderQueue = (int)(transparent ? RenderQueue.Transparent : RenderQueue.Geometry);
            material.SetShaderPassEnabled("ShadowCaster", !transparent);
            material.enableInstancing = true;
            GeneratedMaterialRepair.Apply(material);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
