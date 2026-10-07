using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Offsets the Eye transform (under CameraRoot, followed by the Cinemachine camera) with head
    /// bob, a landing dip and shake from nearby heavy impacts. Purely local presentation.
    /// </summary>
    public class PlayerCameraFeel : MonoBehaviour
    {
        [SerializeField] private FeelSettings settings;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerRagdoll ragdoll;
        [SerializeField] private Transform eye;

        private const float TwoPi = Mathf.PI * 2f;

        private float bobPhase;
        private float bobWeight;
        private float noiseTime;

        public float Dip { get; private set; }
        public float Trauma { get; private set; }
        public Vector3 BobOffset { get; private set; }
        public FeelSettings Settings => settings;

        private void OnEnable()
        {
            motor.Landed += OnLanded;
            CameraShake.Impact += OnImpact;
            CameraShake.Collapse += OnCollapse;
        }

        private void OnDisable()
        {
            motor.Landed -= OnLanded;
            CameraShake.Impact -= OnImpact;
            CameraShake.Collapse -= OnCollapse;
            eye.localPosition = Vector3.zero;
            eye.localRotation = Quaternion.identity;
        }

        private void LateUpdate() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            UpdateBob(dt);
            Dip = GameSettings.HeadBob ? Mathf.Lerp(Dip, 0f, 1f - Mathf.Exp(-settings.DipRecoverySpeed * dt)) : 0f;
            if (!GameSettings.CameraShake) Trauma = 0f;
            Trauma = Mathf.Max(0f, Trauma - settings.ShakeDecay * dt);

            eye.localPosition = BobOffset + Vector3.down * Dip;
            eye.localRotation = ShakeRotation(dt);
        }

        private void UpdateBob(float dt)
        {
            // While ragdolled the motor is off and its state is stale; no bob.
            float speed = !ragdoll.IsRagdolled && motor.IsGrounded ? motor.HorizontalSpeed : 0f;
            float scale = motor.IsSprinting ? settings.SprintBobScale : motor.IsCrouching ? settings.CrouchBobScale : 1f;
            float target = settings.HeadBobEnabled && GameSettings.HeadBob && speed > 0.1f
                ? Mathf.Clamp01(speed / motor.Config.WalkSpeed) * scale
                : 0f;
            bobWeight = Mathf.MoveTowards(bobWeight, target, settings.BobBlendSpeed * dt);
            if (!settings.HeadBobEnabled || !GameSettings.HeadBob) bobWeight = 0f;
            bobPhase = (bobPhase + speed * dt / settings.BobCycleLength * TwoPi) % TwoPi;

            // Side-to-side once per cycle, up-and-down twice (once per step).
            BobOffset = new Vector3(Mathf.Sin(bobPhase) * settings.BobHorizontal,
                -Mathf.Abs(Mathf.Sin(bobPhase)) * settings.BobVertical, 0f) * bobWeight;
        }

        private Quaternion ShakeRotation(float dt)
        {
            if (Trauma <= 0f) return Quaternion.identity;
            noiseTime += dt * settings.ShakeFrequency;
            float amount = Trauma * Trauma * settings.ShakeMaxAngle; // squared: small hits stay subtle
            return Quaternion.Euler(
                (Mathf.PerlinNoise(noiseTime, 0.1f) * 2f - 1f) * amount,
                (Mathf.PerlinNoise(0.7f, noiseTime) * 2f - 1f) * amount,
                (Mathf.PerlinNoise(noiseTime, 3.3f) * 2f - 1f) * amount * 0.5f);
        }

        private void OnLanded(float fallHeight, float impactSpeed)
        {
            if (!settings.LandingDipEnabled || !GameSettings.HeadBob || impactSpeed < settings.MinDipSpeed) return;
            Dip = Mathf.Min(settings.MaxDip, Dip + impactSpeed * settings.DipPerSpeed);
        }

        private void OnImpact(Vector3 position, float momentum)
        {
            if (!settings.CameraShakeEnabled || !GameSettings.CameraShake || momentum < settings.ShakeMomentumThreshold) return;
            float distance = Vector3.Distance(eye.position, position);
            if (distance >= settings.ShakeRadius) return;
            float strength = Mathf.Clamp01((momentum - settings.ShakeMomentumThreshold) /
                                           (settings.ShakeFullMomentum - settings.ShakeMomentumThreshold));
            Trauma = Mathf.Max(Trauma, Mathf.Min(settings.ImpactShakeLimit,
                strength * settings.ImpactShakeLimit * (1f - distance / settings.ShakeRadius)));
        }

        private void OnCollapse(Vector3 position, float momentum)
        {
            if (!settings.CameraShakeEnabled || !GameSettings.CameraShake) return;
            float distance = Vector3.Distance(eye.position, position);
            if (distance >= settings.CollapseShakeRadius) return;
            Trauma = Mathf.Max(Trauma, 0.95f * (1f - distance / settings.CollapseShakeRadius));
        }
    }
}
