using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Loot;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck's cargo bay (GDD 10): loot whose centre is inside counts toward the haul when the truck
    /// leaves; players inside the bay or cab leave with it. The bay's size is the physical limit; the
    /// cargo volume (sum of the items' bounding boxes) is checked against the run's capacity.
    /// </summary>
    public class TruckCargo : MonoBehaviour
    {
        [Tooltip("Trigger box covering the cargo bay interior.")]
        [SerializeField] private BoxCollider bay;
        [Tooltip("Where someone has to stand to ride along (bay + cab), in this object's space.")]
        [SerializeField] private Bounds rideZone = new(Vector3.zero, new Vector3(8f, 3f, 3f));
        [SerializeField] private Transform ignition;

        private readonly Collider[] hits = new Collider[256];
        private readonly HashSet<LootItem> found = new();

        public Transform Ignition => ignition;

        public static TruckCargo Current { get; private set; }

        private void OnEnable() => Current = this;

        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

        /// <summary>Loot in the bay (not pocketed, not shattered).</summary>
        public IReadOnlyCollection<LootItem> ItemsInside()
        {
            found.Clear();
            Vector3 center = bay.transform.TransformPoint(bay.center);
            Vector3 half = Vector3.Scale(bay.size, bay.transform.lossyScale) / 2f;
            int n = Physics.OverlapBoxNonAlloc(center, half, hits, bay.transform.rotation, LayerMask.GetMask(GameLayers.Loot), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                LootItem item = hits[i].GetComponentInParent<LootItem>();
                if (item == null || item.IsShattered || !item.gameObject.activeInHierarchy) continue;
                // A pocketed item keeps its colliders where it was picked up; it rides in the pocket, not the bay.
                if (item.TryGetComponent(out Interaction.Grabbable grabbable) && grabbable.IsPocketed) continue;
                Vector3 local = bay.transform.InverseTransformPoint(item.transform.position) - bay.center;
                if (Mathf.Abs(local.x) <= bay.size.x / 2f && Mathf.Abs(local.y) <= bay.size.y / 2f + 0.5f && Mathf.Abs(local.z) <= bay.size.z / 2f)
                    found.Add(item);
            }
            return found;
        }

        public static float VolumeOf(LootItem item) => item.Definition.Size.x * item.Definition.Size.y * item.Definition.Size.z;

        public bool Carries(Vector3 worldPosition) => rideZone.Contains(transform.InverseTransformPoint(worldPosition));

        public void EditorSetup(BoxCollider cargoBay, Bounds zone, Transform ignitionPoint)
        {
            bay = cargoBay;
            rideZone = zone;
            ignition = ignitionPoint;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(rideZone.center, rideZone.size);
        }
    }
}
