using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Core
{
    /// <summary>
    /// 0.24.13: how weathered this map's environment surfaces look (the Abandoned/Surface shader's grime, dust and
    /// variation multiply by it), and the colours its grime and dust take. One per map scene, placed by the
    /// Editor's SurfaceShaderUpgrade; a scene without one draws its surfaces clean (the globals read 0). While F1 is up,
    /// B (bare) toggles the weathering off and on to compare.
    /// </summary>
    [ExecuteAlways]
    public sealed class SurfaceWeathering : MonoBehaviour
    {
        private static readonly int Weather = Shader.PropertyToID("_AbandonedWeather"),
            GrimeColor = Shader.PropertyToID("_AbandonedGrimeColor"), DustColor = Shader.PropertyToID("_AbandonedDustColor");

        [SerializeField, Range(0f, 1.5f)] private float strength = 1f;
        [SerializeField] private Color grime = new(0.42f, 0.38f, 0.31f, 1f);
        [SerializeField] private Color dust = new(0.36f, 0.34f, 0.31f, 1f);

        private static bool debugOff;

        private void OnEnable() => Push();

        private void OnValidate() => Push();

        private void OnDisable() => Shader.SetGlobalFloat(Weather, 0f);

        private void Update()
        {
            if (Application.isPlaying && DebugView.Visible && Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                debugOff = !debugOff;
                Push();
            }
        }

        private void Push()
        {
            Shader.SetGlobalFloat(Weather, debugOff ? 0f : strength);
            Shader.SetGlobalColor(GrimeColor, grime);
            Shader.SetGlobalColor(DustColor, dust);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // F1 only, and F1 never opens in a player build: there the GUI loop doesn't run for it at all.
        private GUIStyle style;

        private void OnGUI()
        {
            if (!Application.isPlaying || !DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 620, 50, 610, 22),
                $"surfaces: weathering {(debugOff ? "off" : $"{strength:0.00}")} [B]", style);
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(float weather, Color grimeTint, Color dustTint)
        {
            strength = weather;
            grime = grimeTint;
            dust = dustTint;
            Push();
        }
#endif
    }
}
