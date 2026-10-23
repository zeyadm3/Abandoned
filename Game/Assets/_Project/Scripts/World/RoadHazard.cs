using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// Something lying across the road (a fallen tree, a wreck): solid, so the van has to slow down and squeeze past on
    /// the verge or hit it (the van's own knocks do the damage). Host: cleared once the van is well past it.
    /// </summary>
    public class RoadHazard : NetworkBehaviour
    {
        [SerializeField] private float clearBehind = 450f, minLife = 30f;

        private float born;

        public override void OnNetworkSpawn() => born = Time.time;

        private void Update()
        {
            if (!IsSpawned || !IsServer || WorldVan.Current == null || Time.time - born < minLife) return;
            if (Vector3.Distance(WorldVan.Current.transform.position, transform.position) > clearBehind) NetworkObject.Despawn(true);
        }
    }
}
