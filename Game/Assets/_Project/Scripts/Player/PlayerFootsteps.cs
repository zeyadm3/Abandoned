using System;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Plays a footstep every StepLength metres walked on the ground, by surface material, louder
    /// when sprinting and quieter when crouched. Raises <see cref="Stepped"/> for the noise system.
    /// </summary>
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private FeelSettings settings;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerRagdoll ragdoll;

        private const float ProbeHeight = 0.3f;
        private const float ProbeDistance = 0.8f;

        private Vector3 lastPosition;
        private float distance;

        public int StepCount { get; private set; }
        public SurfaceMaterial LastSurface { get; private set; }
        public float LastVolume { get; private set; }

        /// <summary>Position and volume (0–1) of each footstep.</summary>
        public event Action<Vector3, float> Stepped;

        private void OnEnable()
        {
            lastPosition = transform.position;
            ragdoll.Ended += OnGotUp;
        }

        private void OnDisable() => ragdoll.Ended -= OnGotUp;

        // Getting up teleports the root to where the body landed; that isn't walking.
        private void OnGotUp()
        {
            lastPosition = transform.position;
            distance = 0f;
        }

        private void LateUpdate() => Tick();

        public void Tick()
        {
            Vector3 position = transform.position;
            Vector3 delta = position - lastPosition;
            lastPosition = position;
            if (ragdoll.IsRagdolled || !motor.IsGrounded) return;

            distance += new Vector2(delta.x, delta.z).magnitude;
            if (distance < settings.StepLength) return;
            distance -= settings.StepLength;
            // Teleports and ragdoll recovery shouldn't fire a burst of steps.
            if (distance > settings.StepLength) distance = 0f;
            Step(position);
        }

        private void Step(Vector3 position)
        {
            int mask = ~((1 << Mathf.Max(0, GameLayers.PlayerLayer)) | (1 << Mathf.Max(0, GameLayers.DebrisLayer)));
            LastSurface = Physics.Raycast(position + Vector3.up * ProbeHeight, Vector3.down, out RaycastHit hit,
                ProbeDistance, mask, QueryTriggerInteraction.Ignore)
                ? SurfaceTag.Of(hit.collider)
                : SurfaceMaterial.Concrete;

            LastVolume = motor.IsSprinting ? settings.SprintStepVolume
                : motor.IsCrouching ? settings.CrouchStepVolume
                : settings.WalkStepVolume;
            StepCount++;
            if (settings.FootstepsEnabled) GameAudio.PlayFootstep(LastSurface, position, LastVolume);
            // Threats hear steps whether or not the player has footstep audio turned on.
            PlayerMovementConfig c = motor.Config;
            float noise = motor.IsSprinting ? c.SprintNoise : motor.IsCrouching ? c.CrouchNoise : c.WalkNoise;
            NoiseSystem.Emit(position, noise, NoiseSource.Footstep);
            Stepped?.Invoke(position, LastVolume);
        }
    }
}
