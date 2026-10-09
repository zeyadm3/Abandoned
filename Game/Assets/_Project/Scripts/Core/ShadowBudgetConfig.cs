using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The shadow budget (0.14.0): how many of the ceiling lights nearest the camera cast shadows at each
    /// Shadows setting, and how they fade as the player moves. The player's own flashlight and the moon
    /// always cast (within the setting's shadow distance) and are not counted here.
    /// The counts are upper limits: the atlas is shared in shadow maps (a spot light is one, a point light
    /// six), so with two point lights casting there is room for fewer spot lights beside them.
    /// Lives at Data/Core/Resources/ShadowBudgetConfig (Tools/Abandoned/Art/Apply Render Budget makes it);
    /// the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Rendering/Shadow Budget", fileName = "ShadowBudgetConfig")]
    public class ShadowBudgetConfig : ScriptableObject
    {
        public const string ResourcePath = "ShadowBudgetConfig";

        [Header("Per Shadows setting: Off, Low, Medium, High")]
        [Tooltip("Ceiling and street lights nearest the camera that cast shadows, at most: fewer when the atlas is full.")]
        [SerializeField] private int[] lights = { 0, 2, 4, 6 };
        [Tooltip("Of those, how many may be point lights (each one costs six shadow maps; the HQ's tubes are points).")]
        [SerializeField] private int[] pointLights = { 0, 1, 2, 2 };
        [Tooltip("Size (pixels) of the shadow atlas the flashlight and these lights share.")]
        [SerializeField] private int[] atlas = { 1024, 4096, 4096, 4096 };
        [Tooltip("Shadow maps always kept free in the atlas for lights outside this budget (the player's own flashlight is one).")]
        [SerializeField, Min(0)] private int reservedMaps = 1;

        [Header("Choosing")]
        [Tooltip("Lights further than this (m) from the camera never cast.")]
        [SerializeField, Min(1f)] private float maxDistance = 22f;
        [Tooltip("A light on another storey counts as this many times further away.")]
        [SerializeField, Min(1f)] private float otherStoreyPenalty = 2.5f;
        [Tooltip("A light behind the camera counts as this many times further away.")]
        [SerializeField, Min(1f)] private float behindPenalty = 1.3f;
        [Tooltip("A light already casting counts as this much nearer, so two lights at the same distance don't swap every step.")]
        [SerializeField, Range(0.5f, 1f)] private float keepBonus = 0.85f;
        [SerializeField, Min(1f)] private float storeyHeight = 4f;

        [Header("Look")]
        [Tooltip("Seconds for a light's shadows to fade in or out.")]
        [SerializeField, Min(0.05f)] private float fadeSeconds = 0.6f;
        [Tooltip("How dark a ceiling light's shadows are (1 = black).")]
        [SerializeField, Range(0f, 1f)] private float strength = 0.9f;

        public float MaxDistance => maxDistance;
        public float OtherStoreyPenalty => otherStoreyPenalty;
        public float BehindPenalty => behindPenalty;
        public float KeepBonus => keepBonus;
        public float StoreyHeight => storeyHeight;
        public float FadeSeconds => fadeSeconds;
        public float Strength => strength;
        public int ReservedMaps => reservedMaps;

        public int Lights(int setting) => Pick(lights, setting, 0);
        public int PointLights(int setting) => Pick(pointLights, setting, 0);
        public int Atlas(int setting) => Mathf.Clamp(Pick(atlas, setting, 2048), 256, 8192);

        private static int Pick(int[] values, int setting, int fallback) =>
            values == null || values.Length == 0 ? fallback : values[Mathf.Clamp(setting, 0, values.Length - 1)];

        private static ShadowBudgetConfig current;

        public static ShadowBudgetConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<ShadowBudgetConfig>(ResourcePath);
                if (current == null) current = CreateInstance<ShadowBudgetConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
