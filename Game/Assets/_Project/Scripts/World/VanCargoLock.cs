using System.Collections.Generic;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// Host: while someone is at the wheel, loot lying in the cargo box rides fixed to the van (held by hand, like the
    /// Collector carries things), so the road doesn't throw it about or break it; when the van is parked again it is
    /// let go, moving with the van. Anyone who picks a piece up takes it out of the lock.
    /// </summary>
    public class VanCargoLock : MonoBehaviour
    {
        [SerializeField] private WorldVan van;

        private readonly Dictionary<Grabbable, Pose> locked = new();
        private readonly List<Grabbable> gone = new();
        private int nextScan;

        public int Locked => locked.Count;

        private void Awake()
        {
            if (van == null) van = GetComponent<WorldVan>();
        }

        private void FixedUpdate()
        {
            if (van == null || !van.IsSpawned || !van.IsServer || van.Cargo == null) return;
            if (van.State.RampDown)
            {
                Release();
                return;
            }
            if (--nextScan <= 0)
            {
                nextScan = 10;
                foreach (LootItem item in van.Cargo.ItemsInside())
                {
                    if (!item.TryGetComponent(out Grabbable g) || locked.ContainsKey(g) || !Free(g)) continue;
                    locked[g] = new Pose(transform.InverseTransformPoint(g.transform.position), Quaternion.Inverse(transform.rotation) * g.transform.rotation);
                    g.SetExternallyHeld(true);
                }
            }
            gone.Clear();
            foreach (KeyValuePair<Grabbable, Pose> pair in locked)
            {
                Grabbable g = pair.Key;
                if (g == null || !Free(g))
                {
                    gone.Add(g);
                    continue;
                }
                g.Body.MovePosition(transform.TransformPoint(pair.Value.position));
                g.Body.MoveRotation(transform.rotation * pair.Value.rotation);
            }
            foreach (Grabbable g in gone)
            {
                locked.Remove(g);
                if (g != null) g.SetExternallyHeld(false);
            }
        }

        private static bool Free(Grabbable g) => g.Holder == null && !g.IsPocketed && (g.Shared == null || g.Shared.CarrierCount == 0);

        private void Release()
        {
            if (locked.Count == 0) return;
            Vector3 v = van.Drive.Body.linearVelocity;
            foreach (Grabbable g in locked.Keys)
            {
                if (g == null) continue;
                g.SetExternallyHeld(false);
                if (!g.Body.isKinematic) g.Body.linearVelocity = v;
            }
            locked.Clear();
        }
    }
}
