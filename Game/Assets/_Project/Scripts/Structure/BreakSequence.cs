using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// The moment a floor goes: its chunks don't all burst at once. Each holds, sags a few centimetres, then lets go
    /// with its launch velocity, the middle first and the edges a fifth of a second later, so the floor reads as
    /// tearing open under the weight. Cosmetic and local; the launches themselves are the seeded ones DebrisSpawner
    /// recorded, so every machine still plays the same break. (The section's collider is already off: anyone on it
    /// has already started falling.)
    /// </summary>
    public class BreakSequence : MonoBehaviour
    {
        private struct Pending
        {
            public Rigidbody Body;
            public Vector3 From, Velocity, Spin;
            public float Delay;
        }

        private readonly List<Pending> pending = new();
        private float started, sag;

        public void Begin(float sagDepth)
        {
            started = Time.time;
            sag = sagDepth;
        }

        /// <summary>Holds <paramref name="body"/> (kinematic) for <paramref name="delay"/> s, then launches it.</summary>
        public void Add(Rigidbody body, Vector3 velocity, Vector3 spin, float delay)
        {
            body.isKinematic = true;
            pending.Add(new Pending { Body = body, From = body.position, Velocity = velocity, Spin = spin, Delay = delay });
        }

        private void FixedUpdate()
        {
            float elapsed = Time.time - started;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                Pending p = pending[i];
                if (p.Body == null)
                {
                    pending.RemoveAt(i);
                    continue;
                }
                if (elapsed < p.Delay)
                {
                    p.Body.MovePosition(p.From + Vector3.down * sag * (elapsed / p.Delay));
                    continue;
                }
                p.Body.isKinematic = false;
                p.Body.linearVelocity = p.Velocity;
                p.Body.angularVelocity = p.Spin;
                pending.RemoveAt(i);
            }
            if (pending.Count == 0) enabled = false;
        }
    }
}
