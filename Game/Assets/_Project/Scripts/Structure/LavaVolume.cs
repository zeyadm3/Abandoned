using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Molten iron (Iron Factory). Host only: a player who goes in dies, and loot that goes in is despawned, so
    /// it can never reach the truck. The box is checked by position rather than as a trigger because the host's
    /// copies of client-owned players don't simulate physics; their synced positions are always there.
    /// Everyone else only sees the glow (<see cref="LavaSurface"/>): the surface has no collider, so bodies
    /// and loot sink out of sight onto the channel bed.
    /// </summary>
    public class LavaVolume : MonoBehaviour
    {
        private static readonly List<LavaVolume> Volumes = new();
        private static readonly Collider[] Hits = new Collider[64];

        /// <summary>The island prison's sea: the same rules as the lava, its own death line.</summary>
        public static readonly string SeaDeathLine = Loc.Mark("Lost to the sea.");

        [Tooltip("World-space box: anything whose position is inside is in the lava.")]
        [SerializeField] private Bounds volume = new(Vector3.zero, new Vector3(20f, 2.4f, 60f));

        [Tooltip("Death line for this volume (English, worded where shown). Empty: LavaConfig's (the molten iron).")]
        [SerializeField] private string deathLine = "";

        private float nextCheck;
        private int kills, burned;

        public Bounds Volume => volume;

        /// <summary>Is this point in any molten channel? (Rubble never piles up in one.)</summary>
        public static bool Covers(Vector3 at)
        {
            foreach (LavaVolume v in Volumes)
                if (v != null && v.volume.Contains(at)) return true;
            return false;
        }

        private void OnEnable() => Volumes.Add(this);
        private void OnDisable() => Volumes.Remove(this);

        private void Update()
        {
            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsServer || !net.IsListening || Time.time < nextCheck) return;
            LavaConfig config = LavaConfig.Current;
            nextCheck = Time.time + config.CheckInterval;
            Players(config);
            Loot();
        }

        private void Players(LavaConfig config)
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                // A vent crawl parks the player far under the level; that's inside the ducts, not the lava.
                if (p == null || !p.IsSpawned || p.IsDead || p.InVent) continue;
                Vector3 at = p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;
                if (!volume.Contains(at)) continue;
                p.ServerKill(string.IsNullOrEmpty(deathLine) ? config.DeathLine : deathLine);
                kills++;
                Debug.Log($"[Lava] {p.DisplayName} fell in.");
            }
        }

        private void Loot()
        {
            int lootLayer = GameLayers.LootLayer;
            if (lootLayer < 0) return;
            int count = Physics.OverlapBoxNonAlloc(volume.center, volume.extents, Hits, Quaternion.identity,
                1 << lootLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                NetworkLoot loot = Hits[i] != null ? Hits[i].GetComponentInParent<NetworkLoot>() : null;
                // An item with several colliders shows up more than once; the first despawn ends it.
                if (loot == null || !loot.IsSpawned || !volume.Contains(loot.transform.position)) continue;
                // Despawn tears down any hand, pocket or shared hold on every machine.
                loot.NetworkObject.Despawn(destroy: true);
                burned++;
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Bounds worldBox) => volume = worldBox;

        public void EditorSetup(Bounds worldBox, string line)
        {
            volume = worldBox;
            deathLine = line;
        }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            GUI.Label(new Rect(Screen.width - 380, 232, 375, 22), $"LAVA: {kills} players lost · {burned} items burned");
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = new Color(1f, .35f, .05f, .5f);
            Gizmos.DrawWireCube(volume.center, volume.size);
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Volumes.Clear();
    }
}
