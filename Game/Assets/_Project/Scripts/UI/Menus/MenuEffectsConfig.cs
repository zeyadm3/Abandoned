using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// The menus' 3D side: the post volume behind them and the drifting title backdrop. The overlay, button and
    /// transition tunables went with the 2026-10-28 UI redo; the textures stay until their builder is cut with a
    /// Unity compile. Lives in Data/UI/Resources so the menus find it without wiring.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/UI/Menu Effects Config")]
    public class MenuEffectsConfig : ScriptableObject
    {
        public const string ResourcePath = "MenuEffectsConfig";

        [Header("Textures (original, procedural)")]
        [SerializeField] private Texture2D grain;
        [SerializeField] private Texture2D vignette;
        [SerializeField] private Texture2D scratches;
        [SerializeField] private Texture2D stain;
        [SerializeField] private Texture2D glow;
        [SerializeField] private Texture2D scanlines;
        [SerializeField] private Material silhouetteMaterial;
        [SerializeField] private Material dustMaterial;

        [Header("Post effects (URP volume over the 3D view)")]
        [field: SerializeField] public float TitleExposure { get; private set; } = -0.9f;
        [field: SerializeField, Range(-100f, 0f)] public float TitleSaturation { get; private set; } = -35f;
        [field: SerializeField] public float PauseExposure { get; private set; } = -1.4f;
        [field: SerializeField, Range(-100f, 0f)] public float PauseSaturation { get; private set; } = -85f;
        [field: SerializeField, Range(0f, 1f)] public float PostVignette { get; private set; } = 0.42f;
        [field: SerializeField, Min(0.01f)] public float PostBlendSeconds { get; private set; } = 0.35f;

        [Header("Title backdrop (the drifting 3D view)")]
        [field: SerializeField, Min(0f)] public float DriftYaw { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float DriftPitch { get; private set; } = 1.6f;
        [field: SerializeField, Min(0f)] public float DriftDolly { get; private set; } = 0.25f;
        [Tooltip("Seconds between unsettling events (a light dies, a figure, a groan). Rare on purpose.")]
        [field: SerializeField] public Vector2 EventInterval { get; private set; } = new(14f, 32f);
        [field: SerializeField] public Vector2 LightDeadSeconds { get; private set; } = new(6f, 14f);
        [field: SerializeField, Min(0f)] public float SilhouetteMinDistance { get; private set; } = 9f;
        [field: SerializeField, Min(0f)] public float SilhouetteSeconds { get; private set; } = 2.2f;
        [field: SerializeField, Min(0f)] public float ShudderDegrees { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float ShudderSeconds { get; private set; } = 0.7f;
        [field: SerializeField, Range(0f, 1f)] public float GroanVolume { get; private set; } = 0.35f;
        [field: SerializeField, Min(0)] public int DustParticles { get; private set; } = 140;

        public Texture2D Grain => grain;
        public Texture2D Vignette => vignette;
        public Texture2D Scratches => scratches;
        public Texture2D Stain => stain;
        public Texture2D Glow => glow;
        public Texture2D Scanlines => scanlines;
        public Material SilhouetteMaterial => silhouetteMaterial;
        public Material DustMaterial => dustMaterial;

        private static MenuEffectsConfig current;

        /// <summary>The project's config, or code defaults (no textures) if the asset is missing.</summary>
        public static MenuEffectsConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<MenuEffectsConfig>(ResourcePath);
                if (current == null) current = CreateInstance<MenuEffectsConfig>();
                return current;
            }
        }

        /// <summary>Flicker, glitches, jitter, shakes and static cuts are off when the player asked for calm menus.</summary>
        public static bool Calm => Core.GameSettings.ReduceMenuEffects || Core.GameSettings.ReduceFlashing;

        public static float Range(Vector2 range) => UnityEngine.Random.Range(range.x, Mathf.Max(range.x, range.y));

#if UNITY_EDITOR
        public void EditorSetup(Texture2D grainTexture, Texture2D vignetteTexture, Texture2D scratchTexture, Texture2D stainTexture,
            Texture2D glowTexture, Texture2D scanlineTexture, Material silhouette, Material dust)
        {
            grain = grainTexture;
            vignette = vignetteTexture;
            scratches = scratchTexture;
            stain = stainTexture;
            glow = glowTexture;
            scanlines = scanlineTexture;
            silhouetteMaterial = silhouette;
            dustMaterial = dust;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
