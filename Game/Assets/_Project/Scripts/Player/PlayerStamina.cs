using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Stamina pool for sprinting and jumping. Regenerates after a short delay; once fully drained,
    /// sprint stays locked until the pool refills to a threshold. Ticked by <see cref="PlayerMotor"/>
    /// on its simulation clock rather than Update, so it stays in step with movement.
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private PlayerMovementConfig config;

        private float clock;
        private float lastDrainTime = float.NegativeInfinity;

        public float Current { get; private set; }
        public float Max => config.MaxStamina;
        public bool IsExhausted { get; private set; }
        public bool CanSprint => !IsExhausted && Current > 0f;

        private void Awake() => Current = config.MaxStamina;

        public void Drain(float amount)
        {
            if (amount <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            lastDrainTime = clock;
            if (Current <= 0f) IsExhausted = true;
        }

        public bool TryConsume(float amount)
        {
            if (Current < amount) return false;
            Drain(amount);
            return true;
        }

        public void Tick(float dt)
        {
            clock += dt;
            if (clock - lastDrainTime < config.RegenDelay) return;
            Current = Mathf.Min(config.MaxStamina, Current + config.RegenPerSecond * dt);
            if (IsExhausted && Current >= config.SprintResumeThreshold) IsExhausted = false;
        }
    }
}
