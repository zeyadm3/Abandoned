using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// URP Lit materials for remake texture sets. A set is three PNGs from Blender's bake: BaseColor (sRGB), Normal
    /// (OpenGL tangent space, Unity's convention) and Mask (R metallic, G occlusion, A smoothness), which is exactly the
    /// layout URP Lit reads from _MetallicGlossMap (R, A) and _OcclusionMap (G), so one texture feeds both.
    /// </summary>
    public static class RemakeMaterials
    {
        /// <summary>Surfaces that are not baked into an asset's set (Tools/Blender/remake/core/materials.py SHARED).</summary>
        public static readonly string[] SharedNames =
            { "Glass", "GlassDark", "Emit", "EmitWarm", "EmitCold", "EmitRed", "EmitGreen", "Screen", "HairCard", "Decal" };

        public static bool IsShared(string set) => System.Array.IndexOf(SharedNames, set) >= 0;

        /// <summary>The material for a set exported from Blender as "RM_&lt;set&gt;" by a model under areaFolder.</summary>
        public static Material GetOrCreate(string set, string areaFolder)
        {
            bool shared = IsShared(set);
            string folder = shared ? RemakePaths.Shared : areaFolder + "/Materials";
            PolishAssets.EnsureFolder(folder);
            string path = $"{folder}/{set}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = set };
                AssetDatabase.CreateAsset(material, path);
            }
            if (shared) ConfigureShared(material, set);
            else Fill(material, set, areaFolder + "/Textures");
            return material;
        }

        /// <summary>Points a material at its set's textures (whatever has been imported so far).</summary>
        public static void Fill(Material material, string set, string textureFolder)
        {
            var color = AssetDatabase.LoadAssetAtPath<Texture2D>($"{textureFolder}/{set}_BaseColor.png");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{textureFolder}/{set}_Normal.png");
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>($"{textureFolder}/{set}_Mask.png");
            material.SetColor("_BaseColor", Color.white);
            if (color != null) material.SetTexture("_BaseMap", color);
            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.SetFloat("_BumpScale", 1f);
                material.EnableKeyword("_NORMALMAP");
            }
            if (mask != null)
            {
                material.SetTexture("_MetallicGlossMap", mask);
                material.SetFloat("_Metallic", 1f);
                material.SetFloat("_Smoothness", 1f);
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_OcclusionMap", mask);
                material.SetFloat("_OcclusionStrength", 1f);
                material.EnableKeyword("_OCCLUSIONMAP");
            }
            else
            {
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Smoothness", .3f);
            }
            EditorUtility.SetDirty(material);
        }

        private static void ConfigureShared(Material m, string set)
        {
            m.SetFloat("_Metallic", 0f);
            switch (set)
            {
                case "Glass":
                case "GlassDark":
                    m.SetColor("_BaseColor", set == "Glass" ? new Color(.78f, .84f, .84f, .22f) : new Color(.05f, .06f, .06f, .6f));
                    m.SetFloat("_Smoothness", .94f);
                    Transparent(m);
                    break;
                case "Screen":
                    m.SetColor("_BaseColor", new Color(.02f, .03f, .03f));
                    m.SetFloat("_Smoothness", .9f);
                    Emissive(m, new Color(.15f, .4f, .3f) * 1.5f);
                    break;
                case "HairCard":
                    m.SetColor("_BaseColor", new Color(.08f, .06f, .045f));
                    m.SetFloat("_AlphaClip", 1f);
                    m.SetFloat("_Cutoff", .4f);
                    m.SetFloat("_Cull", (float)CullMode.Off);
                    m.EnableKeyword("_ALPHATEST_ON");
                    m.SetFloat("_Smoothness", .35f);
                    break;
                case "Decal":
                    m.SetFloat("_AlphaClip", 1f);
                    m.SetFloat("_Cutoff", .5f);
                    m.EnableKeyword("_ALPHATEST_ON");
                    break;
                default:
                    Color c = set switch
                    {
                        "EmitWarm" => new Color(1f, .7f, .4f),
                        "EmitCold" => new Color(.7f, .85f, 1f),
                        "EmitRed" => new Color(1f, .08f, .03f),
                        "EmitGreen" => new Color(.2f, 1f, .25f),
                        _ => new Color(1f, .95f, .85f),
                    };
                    m.SetColor("_BaseColor", c);
                    Emissive(m, c * 4f);
                    break;
            }
            EditorUtility.SetDirty(m);
        }

        private static void Emissive(Material m, Color c)
        {
            m.SetColor("_EmissionColor", c);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        private static void Transparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        /// <summary>Re-fills every remake material from its textures (after a batch export, textures may import after models).</summary>
        [MenuItem(ToolMenu.Art + "Remake: Refresh Materials", priority = ToolMenu.ArtP + 100)]
        public static void RefreshAll()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { RemakePaths.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null) continue;
                if (path.StartsWith(RemakePaths.Shared + "/")) ConfigureShared(m, m.name);
                else
                {
                    string area = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(path)).Replace('\\', '/');
                    Fill(m, m.name, area + "/Textures");
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Remake] materials refreshed");
        }
    }
}
