using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Camera and audio "feel" tunables, each with an on/off switch (GDD 20: camera shake and
    /// head bob toggles). The settings menu will write these later.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Player/Feel Settings", fileName = "FeelSettings")]
    public class FeelSettings : ScriptableObject, IValidatable
    {
        [field: Header("Head bob")]
        [field: SerializeField] public bool HeadBobEnabled { get; private set; } = true;
        [field: SerializeField, Min(0f)] public float BobVertical { get; private set; } = 0.035f;
        [field: SerializeField, Min(0f)] public float BobHorizontal { get; private set; } = 0.02f;
        [field: Tooltip("Metres travelled per full bob cycle (two steps).")]
        [field: SerializeField, Min(0.1f)] public float BobCycleLength { get; private set; } = 1.6f;
        [field: SerializeField, Min(0f)] public float SprintBobScale { get; private set; } = 1.4f;
        [field: SerializeField, Min(0f)] public float CrouchBobScale { get; private set; } = 0.5f;
        [field: Tooltip("How fast the bob fades in/out when starting or stopping.")]
        [field: SerializeField, Min(0.1f)] public float BobBlendSpeed { get; private set; } = 8f;

        [field: Header("Landing dip")]
        [field: SerializeField] public bool LandingDipEnabled { get; private set; } = true;
        [field: Tooltip("Dip depth per m/s of landing speed.")]
        [field: SerializeField, Min(0f)] public float DipPerSpeed { get; private set; } = 0.02f;
        [field: SerializeField, Min(0f)] public float MaxDip { get; private set; } = 0.25f;
        [field: SerializeField, Min(0.1f)] public float DipRecoverySpeed { get; private set; } = 10f;
        [field: Tooltip("Landings slower than this don't dip (stepping off a kerb).")]
        [field: SerializeField, Min(0f)] public float MinDipSpeed { get; private set; } = 3f;

        [field: Header("Footsteps")]
        [field: SerializeField] public bool FootstepsEnabled { get; private set; } = true;
        [field: Tooltip("Metres between footsteps.")]
        [field: SerializeField, Min(0.1f)] public float StepLength { get; private set; } = 0.8f;
        [field: SerializeField, Range(0f, 1f)] public float WalkStepVolume { get; private set; } = 0.35f;
        [field: SerializeField, Range(0f, 1f)] public float SprintStepVolume { get; private set; } = 0.6f;
        [field: SerializeField, Range(0f, 1f)] public float CrouchStepVolume { get; private set; } = 0.12f;

        [field: Header("Camera shake")]
        [field: SerializeField] public bool CameraShakeEnabled { get; private set; } = true;
        [field: Tooltip("Impacts below this momentum (kg·m/s) never shake the camera.")]
        [field: SerializeField, Min(0f)] public float ShakeMomentumThreshold { get; private set; } = 200f;
        [field: Tooltip("Momentum that gives full shake at point blank.")]
        [field: SerializeField, Min(1f)] public float ShakeFullMomentum { get; private set; } = 2000f;
        [field: SerializeField, Min(0.5f)] public float ShakeRadius { get; private set; } = 15f;
        [field: Tooltip("Max camera rotation at full trauma (degrees).")]
        [field: SerializeField, Min(0f)] public float ShakeMaxAngle { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float ShakeFrequency { get; private set; } = 18f;
        [field: Tooltip("Trauma lost per second.")]
        [field: SerializeField, Min(0.01f)] public float ShakeDecay { get; private set; } = 1.6f;

        public void Validate(List<string> errors)
        {
            if (ShakeFullMomentum <= ShakeMomentumThreshold) errors.Add($"{name}: ShakeFullMomentum must exceed the threshold.");
        }
    }
}
