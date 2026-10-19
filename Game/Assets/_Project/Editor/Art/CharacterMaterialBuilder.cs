using System.Collections.Generic;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using P = Abandoned.EditorTools.CharacterSurfaceTextures.Pattern;

namespace Abandoned.EditorTools
{
    /// <summary>Authors persistent PBR surfaces; prefab authoring never stores a transient runtime material.</summary>
    public static class CharacterMaterialBuilder
    {
        public const string Folder = "Assets/_Project/Art/Characters";
        public const string LibraryPath = Folder + "/Resources/CharacterSurfaceLibrary.asset";
        private static bool prepared, preparing;
        private static readonly Dictionary<string, Material> surfaces = new();

        public static void Prepare()
        {
            if (prepared || preparing) return;
            preparing = true;
            try
            {
                PolishAssets.EnsureFolder(Folder + "/Materials");
                PolishAssets.EnsureFolder(Folder + "/Resources");
                Add("CrewFabric", P.Fabric, new Color(.88f, .39f, .085f));
                Add("CrewTrim", P.Canvas, new Color(.16f, .19f, .2f));
                Add("Rubber", P.Rubber, new Color(.055f, .06f, .065f));
                Add("Leather", P.Leather, new Color(.12f, .1f, .08f));
                Add("Steel", P.Metal, new Color(.42f, .46f, .48f));
                Add("Tape", P.Tape, new Color(.72f, .75f, .67f), emission: .05f);
                Add("Skin", P.Skin, new Color(.63f, .46f, .34f));
                Add("Glass", P.Glass, new Color(.12f, .19f, .2f, .28f), transparent: true);
                Add("Canvas", P.Canvas, new Color(.36f, .34f, .27f));
                Add("Paint", P.Paint, new Color(.68f, .39f, .12f));
                Add("Screen", P.Screen, new Color(.04f, .13f, .115f), emission: .3f);
                Add("Wood", P.Wood, new Color(.45f, .3f, .16f));
                Add("ThreatAsh", P.Ash, new Color(.44f, .43f, .38f), legacy: "Ash");
                Add("ThreatCharcoal", P.Leather, new Color(.065f, .075f, .08f), legacy: "Charcoal");
                Add("ThreatCanvas", P.Canvas, new Color(.27f, .23f, .17f), legacy: "Canvas");
                Add("ThreatBone", P.Bone, new Color(.7f, .66f, .54f), legacy: "Bone");
                Add("ThreatIron", P.Metal, new Color(.22f, .25f, .26f), legacy: "Iron");
                Add("ThreatEyes", P.Glass, new Color(.44f, .62f, .55f), legacy: "Eyes", emission: .32f);
                Add("ThreatRust", P.Rust, new Color(.35f, .17f, .075f), legacy: "Rust");
                var keys = new List<string>(surfaces.Keys);
                var materials = new Material[keys.Count];
                for (int i = 0; i < keys.Count; i++) materials[i] = surfaces[keys[i]];
                CharacterSurfaceLibrary library = SerializedWiring.LoadOrCreateAsset<CharacterSurfaceLibrary>(LibraryPath);
                library.EditorSetup(keys.ToArray(), materials);
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssets();
                prepared = true;
            }
            finally { preparing = false; }
        }

        public static void ConfigureThreat(Material material, string slot)
        {
            Prepare();
            if (!surfaces.TryGetValue("Threat" + slot, out Material source)) source = surfaces["ThreatAsh"];
            if (material != source)
            {
                material.shader = source.shader;
                material.CopyPropertiesFromMaterial(source);
            }
            EditorUtility.SetDirty(material);
        }

        public static Material Tinted(string key, Color colour, string name)
        {
            Prepare();
            PolishAssets.EnsureFolder(Folder + "/Materials/Variants");
            string path = Folder + "/Materials/Variants/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Material source = surfaces[key];
            if (material == null)
            {
                material = new Material(source) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = source.shader;
            material.CopyPropertiesFromMaterial(source);
            material.SetColor("_BaseColor", colour);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Add(string key, P pattern, Color colour, string legacy = null, float emission = 0f, bool transparent = false)
        {
            CharacterSurfaceTextures.Ensure(pattern);
            string path = legacy == null ? $"{Folder}/Materials/{key}.mat" : $"{PolishAssets.Folder}/CustomThreat_{legacy}.mat";
            PolishAssets.EnsureFolder(PolishAssets.Folder);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = legacy == null ? key : "CustomThreat_" + legacy };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", CharacterSurfaceTextures.Load(pattern, "BaseColor"));
            material.SetTextureScale("_BaseMap", Vector2.one);
            material.SetTextureOffset("_BaseMap", Vector2.zero);
            material.SetColor("_BaseColor", colour);
            material.SetTexture("_BumpMap", CharacterSurfaceTextures.Load(pattern, "Normal"));
            material.SetFloat("_BumpScale", pattern is P.Fabric or P.Canvas ? .65f : .5f);
            material.SetTexture("_MetallicGlossMap", CharacterSurfaceTextures.Load(pattern, "Mask"));
            material.SetTexture("_OcclusionMap", CharacterSurfaceTextures.Load(pattern, "Mask"));
            material.SetFloat("_Metallic", 1f);
            material.SetFloat("_Smoothness", 1f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetFloat("_OcclusionStrength", .65f);
            material.SetFloat("_WorkflowMode", 1f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_OCCLUSIONMAP");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
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
            material.SetColor("_EmissionColor", colour * emission);
            if (emission > 0f) material.EnableKeyword("_EMISSION");
            else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            surfaces[key] = material;
        }
    }
}
