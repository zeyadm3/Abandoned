using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Equipment
{
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Flashlight Config", fileName = "FlashlightConfig")]
    public class FlashlightConfig : ScriptableObject, IValidatable
    {
        [field: SerializeField, Min(10f)] public float BatterySeconds { get; private set; } = 180f;
        [field: SerializeField, Min(1f)] public float Range { get; private set; } = 12f;
        [field: SerializeField, Min(0f)] public float Intensity { get; private set; } = 6f;
        [field: SerializeField, Range(15f, 100f)] public float SpotAngle { get; private set; } = 52f;
        [field: SerializeField] public Color Color { get; private set; } = new(0.83f, 0.87f, 0.8f);
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
