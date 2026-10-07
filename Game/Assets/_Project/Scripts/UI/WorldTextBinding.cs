using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Keeps a world-space TextMesh's single-sided material sampling its font's live glyph texture.
    /// A dynamic font can rebuild that texture when new characters are needed (another language, a
    /// crew number); the shared material asset only knows the texture it was saved with.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class WorldTextBinding : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private TextMesh text;
        private Renderer textRenderer;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            text = GetComponent<TextMesh>();
            textRenderer = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            Font.textureRebuilt += OnTextureRebuilt;
            Bind();
        }

        private void OnDisable() => Font.textureRebuilt -= OnTextureRebuilt;

        private void OnTextureRebuilt(Font font)
        {
            if (text != null && font == text.font) Bind();
        }

        private void Bind()
        {
            if (text == null || text.font == null || text.font.material == null || textRenderer == null) return;
            Texture glyphs = text.font.material.mainTexture;
            if (glyphs == null) return;
            textRenderer.GetPropertyBlock(block);
            block.SetTexture(MainTex, glyphs);
            textRenderer.SetPropertyBlock(block);
        }
    }
}
