using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Mall worksite lighting, local dust and generator sound tuning for 0.15.5.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Salvage Presentation", fileName = "SalvagePresentationConfig")]
    public sealed class SalvagePresentationConfig : ScriptableObject
    {
        [Header("Remaining retail power")]
        [SerializeField, Range(0, 100)] private int deadFixturePercent = 75;
        [SerializeField, Range(0, 100)] private int faultyFixturePercent = 10;
        [SerializeField, Min(.1f)] private float retailFixtureIntensity = 5.4f;
        [SerializeField, Min(.1f)] private float atriumFixtureIntensity = 10f;
        [SerializeField, Range(0f, .1f)] private float interiorFogDensity = .021f;

        [Header("Independent work lights")]
        [SerializeField, Min(.1f)] private float workLightIntensity = 24f;
        [SerializeField, Min(1f)] private float workLightRange = 13f;
        [SerializeField, Range(25f, 100f)] private float workLightAngle = 68f;
        [SerializeField] private Color workLightColor = new(1f, .93f, .80f);
        [SerializeField, Range(0f, .12f)] private float beamAlpha = .042f;
        [SerializeField, Range(1, 12)] private int maximumWorkLights = 8;

        [Header("Battery exit guides")]
        [SerializeField, Min(0f)] private float emergencyIntensity = 2.8f;
        [SerializeField, Min(1f)] private float emergencyRange = 5.5f;
        [SerializeField] private Color emergencyColor = new(1f, .50f, .12f);

        [Header("Demolition dust pockets")]
        [SerializeField, Range(0, 80)] private int dustParticlesPerPocket = 38;
        [SerializeField, Range(0, 320)] private int maximumLocalDustParticles = 190;
        [SerializeField, Range(.006f, .06f)] private float dustMinimumSize = .016f;
        [SerializeField, Range(.01f, .09f)] private float dustMaximumSize = .038f;
        [SerializeField, Range(0f, .2f)] private float dustDrift = .045f;

        [Header("Crew generator audio")]
        [SerializeField, Range(0f, 1f)] private float generatorVolume = .18f;
        [SerializeField, Min(.1f)] private float generatorNearDistance = 1.5f;
        [SerializeField, Min(1f)] private float generatorFarDistance = 15f;
        [SerializeField, Range(.5f, 1.5f)] private float generatorPitch = .87f;

        public int DeadFixturePercent => deadFixturePercent;
        public int FaultyFixturePercent => faultyFixturePercent;
        public float RetailFixtureIntensity => retailFixtureIntensity;
        public float AtriumFixtureIntensity => atriumFixtureIntensity;
        public float InteriorFogDensity => interiorFogDensity;
        public float WorkLightIntensity => workLightIntensity;
        public float WorkLightRange => workLightRange;
        public float WorkLightAngle => workLightAngle;
        public Color WorkLightColor => workLightColor;
        public float BeamAlpha => beamAlpha;
        public int MaximumWorkLights => maximumWorkLights;
        public float EmergencyIntensity => emergencyIntensity;
        public float EmergencyRange => emergencyRange;
        public Color EmergencyColor => emergencyColor;
        public int DustParticlesPerPocket => dustParticlesPerPocket;
        public int MaximumLocalDustParticles => maximumLocalDustParticles;
        public float DustMinimumSize => dustMinimumSize;
        public float DustMaximumSize => dustMaximumSize;
        public float DustDrift => dustDrift;
        public float GeneratorVolume => generatorVolume;
        public float GeneratorNearDistance => generatorNearDistance;
        public float GeneratorFarDistance => generatorFarDistance;
        public float GeneratorPitch => generatorPitch;
    }
}
