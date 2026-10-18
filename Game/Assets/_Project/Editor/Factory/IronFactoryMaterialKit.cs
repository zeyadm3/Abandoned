using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Makes the Iron Factory's PBR materials (URP Lit) from a texture set: the factory's own code-made maps
    /// (<see cref="IronFactoryTextures"/>) or the mall's kit, copied into a factory material so the mall's own stays
    /// as it is. Re-running updates the same assets in place, so the materials keep their GUIDs and every scene
    /// reference survives (Inspector tweaks to their textures are overwritten by a rebuild).
    /// </summary>
    public static class IronFactoryMaterialKit
    {
        private const string Folder = "Assets/_Project/Art/Factory/Materials";

        public enum Surface { Opaque, Cutout, CutoutTwoSided, Transparent }

        public static Material Textured(string name, string set, Color tint, Surface surface = Surface.Opaque) =>
            Apply(Asset(name), IronFactoryTextures.Get(set, "BaseColor"), IronFactoryTextures.Get(set, "Normal"),
                IronFactoryTextures.Get(set, "Mask"), tint, surface);

        public static Material FromMall(string name, string mallKey, Color tint)
        {
            Material mall = MeridianArt.Material(mallKey);
            return Apply(Asset(name), mall.GetTexture("_BaseMap") as Texture2D, mall.GetTexture("_BumpMap") as Texture2D,
                mall.GetTexture("_MetallicGlossMap") as Texture2D, tint, Surface.Opaque);
        }

        private static Material Asset(string name)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            return m;
        }

        private static Material Apply(Material m, Texture2D colour, Texture2D normal, Texture2D mask, Color tint, Surface surface)
        {
            m.shader = Shader.Find("Universal Render Pipeline/Lit");
            m.SetTexture("_BaseMap", colour);
            m.SetColor("_BaseColor", tint);
            m.SetTextureScale("_BaseMap", Vector2.one);   // the world-UV meshes carry the tiling
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", 1f);
            m.SetTexture("_MetallicGlossMap", mask);
            m.SetTexture("_OcclusionMap", mask);
            m.SetFloat("_Smoothness", mask != null ? 1f : .2f);
            m.SetFloat("_OcclusionStrength", 1f);
            Keyword(m, "_NORMALMAP", normal != null);
            Keyword(m, "_METALLICSPECGLOSSMAP", mask != null);
            Keyword(m, "_OCCLUSIONMAP", mask != null);

            bool cutout = surface is Surface.Cutout or Surface.CutoutTwoSided, transparent = surface == Surface.Transparent;
            m.SetFloat("_Surface", transparent ? 1f : 0f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_AlphaClip", cutout ? 1f : 0f);
            m.SetFloat("_Cutoff", .4f);
            m.SetFloat("_ZWrite", transparent ? 0f : 1f);
            m.SetFloat("_SrcBlend", (float)(transparent ? BlendMode.SrcAlpha : BlendMode.One));
            m.SetFloat("_DstBlend", (float)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
            // Grating and mesh are seen from both sides: from the deck and from the floor below.
            m.SetFloat("_Cull", (float)(surface == Surface.CutoutTwoSided || transparent ? CullMode.Off : CullMode.Back));
            Keyword(m, "_SURFACE_TYPE_TRANSPARENT", transparent);
            Keyword(m, "_ALPHATEST_ON", cutout);
            m.SetOverrideTag("RenderType", transparent ? "Transparent" : cutout ? "TransparentCutout" : "Opaque");
            m.renderQueue = (int)(transparent ? RenderQueue.Transparent : cutout ? RenderQueue.AlphaTest : RenderQueue.Geometry);
            m.SetShaderPassEnabled("ShadowCaster", !transparent);
            m.enableInstancing = true;
            GeneratedMaterialRepair.Apply(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void Keyword(Material m, string keyword, bool on)
        {
            if (on) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }
    }
}
