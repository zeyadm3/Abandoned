using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// What a quirky item does (0.18.2), from its definition's <see cref="LootQuirk"/> flags, on every machine:
    /// sounds play everywhere from the item's own (synced) motion and the server clock; the host adds the
    /// noise monsters hear and the value a leak costs. Added at run time by LootItem, so no prefab changes.
    /// </summary>
    public class LootQuirks : MonoBehaviour
    {
        private const float RingPeriod = 75f, RingLength = 4f;

        private LootItem item;
        private Grabbable grab;
        private Vector3 last;
        private float nextRattle, nextDrip, leakCarry, nextRingSound;
        // The current ring window's roll: worked out once per window, not every frame.
        private long ringPeriod = -1;
        private bool ringsThisPeriod;
        private double ringStart;
        private NetworkObject netObject;

        private void Start()
        {
            item = GetComponent<LootItem>();
            grab = GetComponent<Grabbable>();
            netObject = GetComponent<NetworkObject>();
            last = transform.position;
            // Its weight sits high: it falls over at a nudge.
            if (Has(LootQuirk.TopHeavy) && TryGetComponent(out Rigidbody body) && item.Definition != null)
                body.centerOfMass = Vector3.up * item.Definition.Size.y * 0.35f;
        }

        private bool Has(LootQuirk q) => item != null && item.Definition != null && (item.Definition.Quirks & q) != 0;

        private void Update()
        {
            if (item == null || item.IsShattered) return;
            float speed = Time.deltaTime > 0f ? (transform.position - last).magnitude / Time.deltaTime : 0f;
            last = transform.position;
            bool host = GameAuthority.IsHost;
            if (Has(LootQuirk.Rattles) && speed > 0.6f && Time.time >= nextRattle)
            {
                nextRattle = Time.time + 0.45f;
                GameAudio.Play(SoundId.LootRattle, transform.position, Mathf.Clamp01(speed / 3f));
                if (host) NoiseSystem.Emit(transform.position, 0.25f, NoiseSource.LootImpact);
            }
            bool carried = grab != null && (grab.Holder != null || grab.IsLifted || grab.IsDragged);
            if (Has(LootQuirk.Leaks) && carried && speed > 0.3f)
            {
                if (Time.time >= nextDrip)
                {
                    nextDrip = Time.time + 0.7f;
                    GameAudio.Play(SoundId.LootDrip, transform.position, 1f);
                }
                if (host)
                {
                    // About 1 % of its full value every five seconds of being hauled about.
                    leakCarry += item.FullValue * 0.002f * Time.deltaTime;
                    if (leakCarry >= 1f)
                    {
                        item.Drain(Mathf.FloorToInt(leakCarry));
                        leakCarry -= Mathf.Floor(leakCarry);
                    }
                }
            }
            if (Has(LootQuirk.Rings)) Ring(host);
        }

        // Same answer on every machine: whether it rings now comes from its seed and the server clock.
        private void Ring(bool host)
        {
            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsListening) return;
            double now = net.ServerTime.Time;
            long period = (long)(now / RingPeriod);
            if (period != ringPeriod)
            {
                ringPeriod = period;
                // Keyed by the network id, which every machine shares. The item's seed is only set on the
                // host (clients get the rolled value, not the seed), so with it every ringing item on a
                // client rang at the same moment, and never when the host's monsters heard it.
                int key = netObject != null && netObject.IsSpawned ? (int)netObject.NetworkObjectId : item.Seed;
                var roll = new System.Random(key ^ (int)period * 7919);
                ringsThisPeriod = roll.NextDouble() <= 0.6;
                ringStart = period * RingPeriod + roll.NextDouble() * (RingPeriod - RingLength);
            }
            if (!ringsThisPeriod) return;
            double start = ringStart;
            if (now < start || now > start + RingLength || Time.time < nextRingSound) return;
            nextRingSound = Time.time + 1f;
            GameAudio.Play(SoundId.LootRing, transform.position, 1f);
            if (host) NoiseSystem.Emit(transform.position, 0.6f, NoiseSource.Other);
        }
    }
}
