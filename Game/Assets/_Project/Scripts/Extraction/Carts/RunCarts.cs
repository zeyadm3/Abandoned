using System.Linq;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Shopping carts (0.18.4), on the run state's object: a moment into each job the host stands a cart at
    /// every cart bay. A cart is wheeled utility loot (worth nothing, never in the planner): one player drags
    /// it, and what rides in its basket is host-simulated like the cart, so it comes along (and rattles).
    /// </summary>
    public class RunCarts : NetworkBehaviour
    {
        public const string CartId = "shopping_cart";
        private float spawnAt = -1f;

        public override void OnNetworkSpawn()
        {
            if (IsServer) spawnAt = Time.time + 2f;
        }

        private void Update()
        {
            if (!IsServer || spawnAt < 0f || Time.time < spawnAt) return;
            spawnAt = -1f;
            NetworkLootSpawner spawner = NetworkLootSpawner.Current;
            LootDefinition cart = spawner != null && spawner.Catalog != null
                ? spawner.Catalog.Entries.Select(e => e.definition).FirstOrDefault(d => d != null && d.Id == CartId) : null;
            if (cart == null) return;
            int n = 0;
            foreach (CartBay bay in FindObjectsByType<CartBay>(FindObjectsSortMode.None).OrderBy(b => b.name))
                spawner.SpawnExtra(cart, bay.transform.position + Vector3.up * (cart.Size.y * 0.5f + 0.05f), bay.transform.rotation, n++);
            Debug.Log($"[Run] {n} shopping carts stand ready.");
        }
    }
}
