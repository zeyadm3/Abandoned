using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Mall-only presentation tuning; the HQ keeps its independent art and lighting.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Meridian Presentation", fileName = "MeridianPresentationConfig")]
    public sealed class MeridianPresentationConfig : ScriptableObject
    {
        [Header("Ceiling fixtures")]
        [SerializeField, Min(0.1f)] private float fixtureIntensity = 18f;
        [SerializeField, Min(1f)] private float fixtureRange = 7.5f;
        [SerializeField, Range(45f, 150f)] private float fixtureAngle = 116f;
        [SerializeField, Range(0f, 0.15f)] private float fixtureCeilingClearance = 0.02f;
        [SerializeField, Range(0f, 1f)] private float basementIntensityScale = 0.8f;
        [SerializeField, Range(0, 100)] private int faultyPercent = 28;
        [SerializeField, Range(0, 100)] private int deadPercent = 10;
        [SerializeField, Range(1, 512)] private int maximumFixtures = 384;
        [SerializeField, Range(0, 4)] private int shadowLights = 4;
        [Tooltip("One fixture per tile at 1; larger values thin the diagonal grid deterministically.")]
        [SerializeField, Range(1, 4)] private int storeFixtureStride = 1;
        [SerializeField, Range(1, 4)] private int serviceFixtureStride = 1;
        [SerializeField, Range(1, 4)] private int concourseFixtureStride = 2;
        [SerializeField] private Color fluorescent = new(0.72f, 0.88f, 0.92f);
        [SerializeField] private Color sodium = new(1f, 0.76f, 0.44f);
        [SerializeField, Min(0.1f)] private float alarmIntensity = 1.3f;
        [SerializeField, Min(1f)] private float alarmRange = 7f;
        [SerializeField, Min(0.1f)] private float fixtureFallLifetime = 5f;

        [Header("Skylight-suspended atrium fixtures")]
        [SerializeField, Range(0, 4)] private int atriumFixtures = 4;
        [SerializeField, Range(6.2f, 10f)] private float atriumFixtureHeight = 6.4f;
        [SerializeField, Min(0.1f)] private float atriumFixtureIntensity = 42f;
        [SerializeField, Range(6f, 16f)] private float atriumFixtureRange = 12f;

        [Header("Parking utility lights")]
        [SerializeField, Min(0f)] private float parkingLampIntensity = 10f;
        [SerializeField, Min(1f)] private float parkingLampRange = 16f;
        [SerializeField] private Color parkingLampColor = new(0.95f, 0.80f, 0.58f);

        [Header("Atmosphere")]
        [SerializeField, Min(0f)] private float skyExposure = 1f;
        [SerializeField] private Color ambientSky = new(0.065f, 0.080f, 0.100f);
        [SerializeField] private Color ambientEquator = new(0.038f, 0.049f, 0.051f);
        [SerializeField] private Color ambientGround = new(0.018f, 0.021f, 0.018f);
        [SerializeField] private Color exteriorFog = new(0.10f, 0.13f, 0.16f);
        [SerializeField] private Color interiorFog = new(0.045f, 0.056f, 0.055f);
        [SerializeField, Range(0f, 0.1f)] private float exteriorFogDensity = 0.010f;
        [SerializeField, Range(0f, 0.1f)] private float interiorFogDensity = 0.018f;
        [SerializeField] private Color moonColor = new(0.55f, 0.68f, 0.82f);
        [SerializeField, Min(0f)] private float moonIntensity = 0.22f;
        [SerializeField] private Vector3 moonEuler = new(58f, -28f, 0f);
        [SerializeField, Range(0f, 1f)] private float bloom = 0.18f;
        [SerializeField, Range(0f, 1f)] private float vignette = 0.14f;
        [SerializeField, Range(0f, 1f)] private float grain = 0.10f;
        [SerializeField, Range(-100f, 100f)] private float saturation = -24f;
        [SerializeField, Range(-100f, 100f)] private float contrast = 10f;
        [SerializeField, Range(-2f, 2f)] private float postExposure = 0.2f;
        [SerializeField, HideInInspector] private int readabilityRevision;

        [Header("Dust and reflections")]
        [SerializeField, Range(0f, 0.2f)] private float dustPerCubicMetre = 0.035f;
        [SerializeField, Range(0, 160)] private int dustPerZoneLimit = 56;
        [SerializeField, Range(32, 256)] private int reflectionResolution = 128;
        [SerializeField, Range(0f, 1f)] private float reflectionIntensity = 0.28f;
        [SerializeField, Min(1f)] private float reflectionRefreshSeconds = 8f;

        public float FixtureIntensity => fixtureIntensity;
        public float FixtureRange => fixtureRange;
        public float FixtureAngle => fixtureAngle;
        public float FixtureCeilingClearance => fixtureCeilingClearance;
        public float BasementIntensityScale => basementIntensityScale;
        public int FaultyPercent => faultyPercent;
        public int DeadPercent => deadPercent;
        public int MaximumFixtures => maximumFixtures;
        public int ShadowLights => shadowLights;
        public int StoreFixtureStride => storeFixtureStride;
        public int ServiceFixtureStride => serviceFixtureStride;
        public int ConcourseFixtureStride => concourseFixtureStride;
        public Color Fluorescent => fluorescent;
        public Color Sodium => sodium;
        public float AlarmIntensity => alarmIntensity;
        public float AlarmRange => alarmRange;
        public float FixtureFallLifetime => fixtureFallLifetime;
        public int AtriumFixtures => atriumFixtures;
        public float AtriumFixtureHeight => atriumFixtureHeight;
        public float AtriumFixtureIntensity => atriumFixtureIntensity;
        public float AtriumFixtureRange => atriumFixtureRange;
        public float ParkingLampIntensity => parkingLampIntensity;
        public float ParkingLampRange => parkingLampRange;
        public Color ParkingLampColor => parkingLampColor;
        public float SkyExposure => skyExposure;
        public Color AmbientSky => ambientSky;
        public Color AmbientEquator => ambientEquator;
        public Color AmbientGround => ambientGround;
        public Color ExteriorFog => exteriorFog;
        public Color InteriorFog => interiorFog;
        public float ExteriorFogDensity => exteriorFogDensity;
        public float InteriorFogDensity => interiorFogDensity;
        public Color MoonColor => moonColor;
        public float MoonIntensity => moonIntensity;
        public Vector3 MoonEuler => moonEuler;
        public float Bloom => bloom;
        public float Vignette => vignette;
        public float Grain => grain;
        public float Saturation => saturation;
        public float Contrast => contrast;
        public float PostExposure => postExposure;
        public float DustPerCubicMetre => dustPerCubicMetre;
        public int DustPerZoneLimit => dustPerZoneLimit;
        public int ReflectionResolution => reflectionResolution;
        public float ReflectionIntensity => reflectionIntensity;
        public float ReflectionRefreshSeconds => reflectionRefreshSeconds;
    }
}
