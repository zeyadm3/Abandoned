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

        private static FlashlightConfig fallback;
        public static FlashlightConfig Default => fallback != null ? fallback : fallback = CreateInstance<FlashlightConfig>();

        public void Validate(List<string> errors)
        {
            if (BatterySeconds <= 0f) errors.Add($"{name}: battery capacity must be positive.");
        }
    }
}
