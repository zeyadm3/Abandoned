using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Every intensity and timing of the horror menus (title backdrop, overlays, tactile buttons, screen
    /// transitions, the pause blur) plus the original procedural textures they draw with. Lives in
    /// Data/UI/Resources so the menus find it without wiring; built by Tools/Abandoned/UI/Create Menu Effects.
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

        [Header("Screen overlay")]
        [field: SerializeField, Range(0f, 1f)] public float GrainOpacity { get; private set; } = 0.09f;
        [field: SerializeField, Min(1f)] public float GrainFps { get; private set; } = 18f;
        [field: SerializeField, Range(0f, 1f)] public float VignetteOpacity { get; private set; } = 0.85f;
        [field: SerializeField, Range(0f, 1f)] public float ScanlineOpacity { get; private set; } = 0.35f;
        [Tooltip("A faint VHS tracking band rolls down the screen every so often (seconds between rolls).")]
        [field: SerializeField] public Vector2 TrackingBandInterval { get; private set; } = new(7f, 16f);
        [field: SerializeField, Range(0f, 1f)] public float TrackingBandOpacity { get; private set; } = 0.06f;

        [Header("Title")]
        [field: SerializeField, Min(0f)] public float ChromaticOffset { get; private set; } = 2f;
        [field: SerializeField, Range(0f, 1f)] public float ChromaticOpacity { get; private set; } = 0.35f;
        [field: SerializeField] public Vector2 TitleGlitchInterval { get; private set; } = new(4f, 11f);
        [field: SerializeField, Min(0f)] public float TitleGlitchShift { get; private set; } = 9f;
        [field: SerializeField, Range(0f, 1f)] public float TitleWearOpacity { get; private set; } = 0.75f;

        [Header("Buttons")]
        [field: SerializeField, Min(0f)] public float HoverJitter { get; private set; } = 1.6f;
        [field: SerializeField, Min(0f)] public float HoverJitterSeconds { get; private set; } = 0.16f;
        [field: SerializeField, Range(0f, 1f)] public float HoverGlowOpacity { get; private set; } = 0.55f;
        [field: SerializeField, Range(0f, 1f)] public float HoverFlickerDip { get; private set; } = 0.45f;
        [field: SerializeField, Range(0.8f, 1f)] public float PressScale { get; private set; } = 0.95f;
        [field: SerializeField, Min(0f)] public float PressOffset { get; private set; } = 3f;
        [field: SerializeField, Range(0f, 1f)] public float PressFlashOpacity { get; private set; } = 0.32f;
        [Tooltip("Screen shake (px) for the important actions: Host, Join, Quit.")]
        [field: SerializeField, Min(0f)] public float ImportantShake { get; private set; } = 7f;
        [field: SerializeField, Min(0f)] public float ShakeSeconds { get; private set; } = 0.22f;

        [Header("Transitions")]
        [field: SerializeField, Min(0f)] public float TransitionSeconds { get; private set; } = 0.18f;
        [field: SerializeField, Range(0f, 1f)] public float TransitionStaticOpacity { get; private set; } = 0.55f;
        [field: SerializeField, Min(0f)] public float PauseFadeSeconds { get; private set; } = 0.3f;

        [Header("Post effects (URP volume over the 3D view)")]
        [field: SerializeField] public float TitleExposure { get; private set; } = -0.9f;
        [field: SerializeField, Range(-100f, 0f)] public float TitleSaturation { get; private set; } = -35f;
        [field: SerializeField] public float PauseExposure { get; private set; } = -1.4f;
        [field: SerializeField, Range(-100f, 0f)] public float PauseSaturation { get; private set; } = -85f;
        [Tooltip("Gaussian depth-of-field end distance behind the pause menu: everything past it is blurred.")]
        [field: SerializeField, Min(0.1f)] public float PauseBlurEnd { get; private set; } = 1.5f;
        [field: SerializeField, Range(0.5f, 1.5f)] public float PauseBlurRadius { get; private set; } = 1.4f;
        [field: SerializeField, Range(0f, 1f)] public float PostVignette { get; private set; } = 0.42f;
        [field: SerializeField, Range(0f, 1f)] public float PostGrain { get; private set; } = 0.45f;
        [field: SerializeField, Range(0f, 1f)] public float PostChromatic { get; private set; } = 0.22f;
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
        public static bool Calm => Core.GameSettings.ReduceMenuEffects;

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
