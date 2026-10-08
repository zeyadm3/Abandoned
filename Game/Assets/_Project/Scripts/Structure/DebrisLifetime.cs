using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// One break's chunks, from launch to rubble. They tumble for a few seconds (or until they all sleep),
    /// then settle: kinematic, kept where they landed for the rest of the job (2026-10-08: rubble stays).
    /// Settled chunks still sit on the Debris layer, so they never touch players or loot. If the floor under
    /// them falls later they drop again. Gives its chunks back to the moving budget when they settle, and
    /// remembers each chunk's launch velocity so machines can be compared for the same seed.
    /// </summary>
    public class DebrisLifetime : MonoBehaviour
    {
        private const float MinTumble = 1.5f, SleepCheckInterval = 0.25f;

        private readonly List<Vector3> launches = new();
        private Rigidbody[] bodies = System.Array.Empty<Rigidbody>();
        private int moving, resting, restingCap = int.MaxValue;
        private float tumbleUntil, tumbleFrom, nextCheck;

        /// <summary>Launch velocity of each chunk, in spawn order.</summary>
        public IReadOnlyList<Vector3> Launches => launches;
        public bool Resting => resting > 0;

        public void Track(int count) => moving = count;

        internal void RecordLaunch(Vector3 velocity) => launches.Add(velocity);

        /// <summary>Chunks settle after at most <paramref name="seconds"/>; kept only while the world holds fewer than <paramref name="cap"/> resting chunks.</summary>
        internal void Settle(float seconds, int cap)
        {
            restingCap = cap;
            tumbleFrom = Time.time;
            tumbleUntil = Time.time + Mathf.Max(MinTumble, seconds);
            bodies = GetComponentsInChildren<Rigidbody>(true);
        }

        private void Update()
        {
            if (resting > 0 || tumbleUntil <= 0f) return;
            if (Time.time < tumbleUntil)
            {
                if (Time.time < tumbleFrom + MinTumble || Time.time < nextCheck) return;
                nextCheck = Time.time + SleepCheckInterval;
                foreach (Rigidbody b in bodies) if (b != null && !b.isKinematic && !b.IsSleeping()) return;
            }
            Rest();
        }

        private void Rest()
        {
            int count = 0;
            foreach (Rigidbody b in bodies) if (b != null) count++;
            DebrisSpawner.Release(moving);
            moving = 0;
            tumbleUntil = 0f;
            if (count == 0 || DebrisSpawner.RestingChunks + count > restingCap)
            {
                // Over the rubble budget: this break's loose chunks clear; its solid mound stays.
                Destroy(gameObject);
                return;
            }
            foreach (Rigidbody b in bodies)
            {
                if (b == null) continue;
                b.linearVelocity = Vector3.zero;
                b.angularVelocity = Vector3.zero;
                b.isKinematic = true;
            }
            resting = count;
            DebrisSpawner.Rest(this, count);
        }

        /// <summary>The floor under some of these chunks fell: they tumble again and settle lower down.</summary>
        internal bool Unsettle(Bounds region)
        {
            if (resting == 0) return false;
            bool any = false;
            foreach (Rigidbody b in bodies)
                if (b != null && region.Contains(b.position)) { any = true; break; }
            if (!any) return false;
            DebrisSpawner.Unrest(this, resting);
            resting = 0;
            foreach (Rigidbody b in bodies)
            {
                if (b == null) continue;
                b.isKinematic = false;
                b.WakeUp();
            }
            tumbleFrom = Time.time;
            tumbleUntil = Time.time + MinTumble * 3f;
            return true;
        }

        private void OnDestroy()
        {
            DebrisSpawner.Release(moving);
            if (resting > 0) DebrisSpawner.Unrest(this, resting);
        }
    }
}
