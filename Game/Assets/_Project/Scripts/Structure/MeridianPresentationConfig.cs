using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Mall-only presentation tuning; the HQ keeps its independent art and lighting.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Meridian Presentation", fileName = "MeridianPresentationConfig")]
    public sealed class MeridianPresentationConfig : ScriptableObject
    {
        [Header("Ceiling fixtures")]
        [SerializeField, Min(0.1f)] private float fixtureIntensity = 8.4f;
        [SerializeField, Min(1f)] private float fixtureRange = 9.5f;
        [SerializeField, Range(45f, 150f)] private float fixtureAngle = 112f;
        [SerializeField, Range(0f, 0.15f)] private float fixtureCeilingClearance = 0.02f;
        [SerializeField, Range(0f, 1f)] private float basementIntensityScale = 0.6f;
        [SerializeField, Range(0, 100)] private int faultyPercent = 45;
        [SerializeField, Range(0, 100)] private int deadPercent = 22;
        [SerializeField, Range(1, 256)] private int maximumFixtures = 128;
        [SerializeField, Range(0, 12)] private int shadowLights = 4;
        [SerializeField] private Color fluorescent = new(0.72f, 0.88f, 0.92f);
        [SerializeField] private Color sodium = new(1f, 0.76f, 0.44f);
        [SerializeField, Min(0.1f)] private float alarmIntensity = 1.3f;
        [SerializeField, Min(1f)] private float alarmRange = 7f;
        [SerializeField, Min(0.1f)] private float fixtureFallLifetime = 5f;

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
        public Color Fluorescent => fluorescent;
        public Color Sodium => sodium;
        public float AlarmIntensity => alarmIntensity;
        public float AlarmRange => alarmRange;
        public float FixtureFallLifetime => fixtureFallLifetime;
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
