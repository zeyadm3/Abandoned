using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// What every threat shares (GDD 9: each has one rule players can learn): it runs on the host only,
    /// everyone sees it through its NetworkTransform, danger scales it, ghosts see it, and killing is
    /// "touching, not through a wall".
    /// </summary>
    public abstract class Threat : NetworkBehaviour
    {
        private static readonly List<Threat> Spawned = new();

        public static IReadOnlyList<Threat> All => Spawned;

        public abstract string DisplayName { get; }
        /// <summary>The death screen's cause line when this kills you (UI step 3).</summary>
        public virtual string DeathLine => $"The {DisplayName} got you.";
        public float SpeedScale { get; set; } = 1f;
        public float HearingScale { get; set; } = 1f;
        public int Kills { get; protected set; }

        protected int WallMask { get; private set; }

        protected virtual void Awake() =>
            WallMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");

        public override void OnNetworkSpawn() => Spawned.Add(this);

        public override void OnNetworkDespawn() => Spawned.Remove(this);

        /// <summary>Where a living or downed player is (the body when ragdolled).</summary>
        protected static Vector3 PositionOf(NetworkPlayer p) => p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;

        /// <summary>Host: kills the first living player within reach it can actually touch; returns them or null.</summary>
        protected NetworkPlayer KillWithinReach(float range)
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || p.IsDead) continue;
                Vector3 at = PositionOf(p);
                Vector3 d = at - transform.position;
                if (Mathf.Abs(d.y) > 1.8f || new Vector2(d.x, d.z).sqrMagnitude > range * range) continue;
                if (Sheltered(at)) continue;
                if (Physics.Linecast(transform.position + Vector3.up * 1.3f, at + Vector3.up * 0.8f, WallMask, QueryTriggerInteraction.Ignore)) continue;
                p.ServerKill(DeathLine);
                Kills++;
                Debug.Log($"[Threat] {DisplayName} killed player {p.OwnerClientId}.");
                return p;
            }
            return null;
        }

        /// <summary>Host: inside an armored truck (M10.1) nothing can touch you.</summary>
        protected static bool Sheltered(Vector3 at) => Extraction.RunState.Current != null && Extraction.RunState.Current.Shelters(at);

        private readonly RaycastHit[] sightHits = new RaycastHit[8];

        /// <summary>Nothing solid between two points (its own body doesn't count).</summary>
        protected bool LineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            float length = d.magnitude;
            if (length < 0.01f) return true;
            int n = Physics.RaycastNonAlloc(from, d / length, sightHits, length, WallMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
                if (!sightHits[i].collider.transform.IsChildOf(transform)) return false;
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetThreats() => Spawned.Clear();
    }
}
