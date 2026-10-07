using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Blind One's decisions, free of Unity objects so they're unit-tested: it wanders until it hears
    /// something, walks to the loudest recent noise (a heard noise's pull fades over Memory), stands and
    /// listens there, and hunts at a run when a noise is strong or keeps coming. Contact kills are the
    /// body's job; it tells the brain (<see cref="Attacked"/>) and the brain pauses.
    /// </summary>
    public sealed class BlindOneBrain
    {
        private readonly BlindOneConfig c;
        private readonly Queue<float> recentHeard = new();
        private Vector3 target;
        private float targetStrength, targetTime, lastHeard = float.NegativeInfinity, arrivedAt = -1f, attackUntil;

        public BlindOneState State { get; private set; } = BlindOneState.Wander;
        public bool HasTarget { get; private set; }
        public Vector3 Target => target;
        public float Speed => State == BlindOneState.Hunt ? c.HuntSpeed : State == BlindOneState.Attack ? 0f : c.WalkSpeed;

        public BlindOneBrain(BlindOneConfig config) => c = config;

        /// <summary>A noise it perceived with this strength (0..1), at time t.</summary>
        public void Hear(Vector3 position, float strength, float t)
        {
            if (strength <= 0f) return;
            recentHeard.Enqueue(t);
            while (recentHeard.Count > 0 && t - recentHeard.Peek() > c.HuntWindow) recentHeard.Dequeue();
            lastHeard = t;

            // The current lead fades; a new noise takes over when it's louder than what's left of it.
            if (!HasTarget || strength >= CurrentPull(t))
            {
                target = position;
                targetStrength = strength;
                targetTime = t;
                HasTarget = true;
                arrivedAt = -1f;
            }
            if (State == BlindOneState.Attack) return;
            bool hunt = strength >= c.HuntThreshold || recentHeard.Count >= c.HuntNoiseCount;
            State = hunt || State == BlindOneState.Hunt ? BlindOneState.Hunt : BlindOneState.Investigate;
        }

        public float CurrentPull(float t) => HasTarget ? targetStrength * Mathf.Clamp01(1f - (t - targetTime) / c.Memory) : 0f;

        /// <summary>It killed someone: it stops to... eat. Then carries on hunting.</summary>
        public void Attacked(float t)
        {
            State = BlindOneState.Attack;
            attackUntil = t + c.AttackPause;
        }

        /// <summary>Every frame: where it is and whether it reached its destination.</summary>
        public void Tick(Vector3 position, bool arrived, float t)
        {
            switch (State)
            {
                case BlindOneState.Attack:
                    if (t >= attackUntil) State = HasTarget ? BlindOneState.Hunt : BlindOneState.Wander;
                    break;
                case BlindOneState.Hunt:
                    // Silence long enough: go and listen where it last heard something.
                    if (t - lastHeard > c.HuntForget) State = BlindOneState.Investigate;
                    else if (arrived) arrivedAt = arrivedAt < 0f ? t : arrivedAt;
                    break;
                case BlindOneState.Investigate:
                    if (!HasTarget) { State = BlindOneState.Wander; break; }
                    if (arrived && arrivedAt < 0f) arrivedAt = t;
                    if (arrivedAt >= 0f && t - arrivedAt >= c.Linger)
                    {
                        State = BlindOneState.Wander;
                        HasTarget = false;
                    }
                    break;
            }
        }
    }
}
