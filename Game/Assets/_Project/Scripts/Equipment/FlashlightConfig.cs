using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Equipment
{
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Flashlight Config", fileName = "FlashlightConfig")]
    public class FlashlightConfig : ScriptableObject, IValidatable
    {
        [field: SerializeField, Min(10f)] public float BatterySeconds { get; private set; } = 420f;
        [field: Tooltip("Battery seconds gained per second while standing in the truck (QA D-01); 0 = no recharge.")]
        [field: SerializeField, Min(0f)] public float TruckRechargePerSecond { get; private set; } = 8f;
        [field: SerializeField, Min(1f)] public float Range { get; private set; } = 26f;
        [field: SerializeField, Min(0f)] public float Intensity { get; private set; } = 28f;
        [field: SerializeField, Range(15f, 100f)] public float SpotAngle { get; private set; } = 68f;
        [field: SerializeField] public Color Color { get; private set; } = new(0.95f, 0.98f, 1f);
        [SerializeField, Range(0f, 1f)] private float innerConeRatio = 0.6f;
        [SerializeField, HideInInspector] private int visibilityRevision;
        public float InnerSpotAngle => SpotAngle * innerConeRatio;
        [field: SerializeField, Min(1f)] public float ThreatFlickerRange { get; private set; } = 15f;
        [field: SerializeField, Range(0f, 1f)] public float MinimumFlickerIntensity { get; private set; } = 0.18f;
        [field: SerializeField, Min(0.1f)] public float FlickerFrequency { get; private set; } = 19f;
        [field: Tooltip("0.24.12: the reflector-pattern cookie (Core.LightCookies).")]
        [field: SerializeField] public bool UseCookie { get; private set; } = true;
        [Tooltip("Makes up the light the cookie's pattern takes away, so the beam is as bright as before 0.24.12.")]
        [SerializeField, Range(1f, 1.5f)] private float cookieGain = 1.2f;

        /// <summary>The intensity the beam is given: <see cref="Intensity"/>, made up for the cookie when it has one.</summary>
        public float BeamIntensity => Intensity * (UseCookie ? cookieGain : 1f);

        private static FlashlightConfig fallback;
        public static FlashlightConfig Default => fallback != null ? fallback : fallback = CreateInstance<FlashlightConfig>();

        public void Validate(List<string> errors)
        {
            if (BatterySeconds <= 0f) errors.Add($"{name}: battery capacity must be positive.");
        }
    }
}
