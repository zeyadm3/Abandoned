using Abandoned.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The one way builders give a TextMesh its look: a depth-tested, front-only material per font
    /// (Abandoned/WorldText), never the font's built-in GUI/Text material, which draws double-sided and
    /// through walls (signs read mirrored from behind).
    /// </summary>
    public static class WorldTextMaterial
    {
        public const string ShaderName = "Abandoned/WorldText";
        public const string Folder = "Assets/_Project/Art/Generated/WorldText";

        public static Material For(Font font)
        {
            if (font == null) return null;
            string path = $"{Folder}/WorldText_{font.name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[WorldText] Shader {ShaderName} missing (Art/Shaders/WorldText.shader).");
                return null;
            }
            if (material == null)
            {
                PolishAssets.EnsureFolder(Folder);
                material = new Material(shader) { name = "WorldText_" + font.name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetTexture("_MainTex", font.material != null ? font.material.mainTexture : null);
            material.SetColor("_Color", Color.white);
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void Apply(TextMesh text, Font font)
        {
            if (font != null) text.font = font;
            var renderer = text.GetComponent<MeshRenderer>();
            Material material = For(text.font);
            if (material != null) renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (text.GetComponent<WorldTextBinding>() == null) text.gameObject.AddComponent<WorldTextBinding>();
        }
    }
}
