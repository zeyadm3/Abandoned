using Abandoned.Core;
using UnityEngine;
namespace Abandoned.UI
{
    /// <summary>The mounted identification signs follow the player's local language.</summary>
    public sealed class MotelWorldSign : MonoBehaviour
    {
        [SerializeField] private TextMesh label;
        [SerializeField] private string english;
        [SerializeField] private Font [] fonts;
        private Font originalFont;
        private float originalSize;
        private void Awake()
        {
            originalSize = label != null ? label.characterSize : 0f;
            originalFont = label != null ? label.font : null;
        }
        private void OnEnable()
        {
            Languages.Changed += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            Languages.Changed -= Refresh;
        }
        private void Refresh()
        {
            if (label == null)
            {
                return;
            }
            int index = Languages.Current.Script switch
            {
                TextScript.Chinese => 1,
                TextScript.Japanese => 2,
                TextScript.Korean => 3,
                _ => 0
            };
            Font font = Languages.Current.IsEnglish ? originalFont : fonts [index];
            if (font != null)
            {
                label.font = font;
                var block = new MaterialPropertyBlock();
                Renderer renderer = label.GetComponent<Renderer>();
                renderer.GetPropertyBlock(block);
                block.SetTexture(Shader.PropertyToID("_MainTex"), font.material.mainTexture);
                renderer.SetPropertyBlock(block);
            }
            label.text = Loc.T(english);
            // Longer translations must still fit the physical sign, without changing its collider.
            float fit = Mathf.Min(1f, english.Length /(float) Mathf.Max(1, label.text.Length));
            if (originalSize> 0f)
            {
                label.characterSize = originalSize * fit;
            }
        }
        #if UNITY_EDITOR
        public void EditorSetup(TextMesh text, string key, Font [] translatedFonts)
        {
            label = text;
            english = key;
            fonts = translatedFonts;
        }
        #endif
    }
}
