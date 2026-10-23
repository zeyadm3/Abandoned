using System.Collections.Generic;
using Abandoned.Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The open world's shared state, on the van's NetworkObject (one per world). The host picks the <b>anchor</b>: the
    /// place whose own origin is the game's origin. Only the anchor's scene may be loaded (every map is built around
    /// 0,0,0), so the host moves the anchor to the next place once the crew has driven away and nobody still has the
    /// old one loaded. When it changes, every machine moves the land, and then everything it is in charge of moving
    /// (the host: the van, loot, monsters; each player: themselves), by the same amount. Coordinates stay small too.
    /// Each machine also reports which places it has loaded, so the host spawns a place's objects only once all have it.
    /// </summary>
    public class WorldState : NetworkBehaviour
    {
        private readonly NetworkVariable<sbyte> anchor = new(-1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        // Host-written: the place every machine should have loaded (only ever the anchor), -1 for none.
        private readonly NetworkVariable<sbyte> streamed = new(-1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Host: which places each machine has loaded (bit per atlas index).
        private readonly Dictionary<ulong, int> loaded = new();
        private int appliedAnchor = -1;

        public static WorldState Current { get; private set; }

        public int Anchor => anchor.Value;
        public int Streamed => streamed.Value;

        /// <summary>
        /// Host, before a place is unloaded everywhere: clear what belongs to it (the job's departure, its loot and
        /// monsters). <see cref="WorldJob"/> fills it in.
        /// </summary>
        public static System.Action<int> ServerClearing;
        public WorldAtlas Atlas { get; private set; }
        public WorldPoi AnchorPoi => Atlas != null && anchor.Value >= 0 && anchor.Value < Atlas.Pois.Count ? Atlas.Pois[anchor.Value] : null;
        /// <summary>World position of the game's origin.</summary>
        public Vector3 Offset => AnchorPoi != null ? AnchorPoi.worldPosition : Vector3.zero;
        /// <summary>This machine's loaded places (bits).</summary>
        public int LocalLoaded { get; private set; }

        public Vector3 ToWorld(Vector3 engine) => engine + Offset;
        public Vector3 ToEngine(Vector3 world) => world - Offset;

        private void Awake() => Atlas = WorldAtlas.Load();

        public override void OnNetworkSpawn()
        {
            Current = this;
            anchor.OnValueChanged += OnAnchorChanged;
            if (IsServer)
            {
                if (anchor.Value < 0 && Atlas != null) anchor.Value = (sbyte)Atlas.Home;
                NetworkManager.OnClientDisconnectCallback += OnClientLeft;
                SessionTravel.StreamingReady = StreamingReady;
            }
            ApplyAnchor(false);
        }

        public override void OnNetworkDespawn()
        {
            anchor.OnValueChanged -= OnAnchorChanged;
            if (NetworkManager != null) NetworkManager.OnClientDisconnectCallback -= OnClientLeft;
            if (SessionTravel.StreamingReady == (System.Func<bool>)StreamingReady) SessionTravel.StreamingReady = null;
            if (Current == this) Current = null;
        }

        private void OnClientLeft(ulong client) => loaded.Remove(client);

        private void OnAnchorChanged(sbyte was, sbyte now) => ApplyAnchor(true);

        // Every machine: the land to the new origin, then whatever this machine moves by the same step.
        private void ApplyAnchor(bool shiftMovers)
        {
            if (AnchorPoi == null || appliedAnchor == anchor.Value) return;
            Vector3 before = WorldRoot.Current != null ? WorldRoot.Current.Offset : Offset;
            appliedAnchor = anchor.Value;
            if (WorldRoot.Current != null) WorldRoot.Current.SetOffset(Offset);
            if (shiftMovers) ShiftMovers(before - Offset);
            Debug.Log($"[World] Origin is now {AnchorPoi.scene} (world {Offset:F0}).");
        }

        private void ShiftMovers(Vector3 delta)
        {
            if (delta == Vector3.zero || NetworkManager == null) return;
            foreach (NetworkObject no in NetworkManager.SpawnManager.SpawnedObjectsList)
            {
                if (no == null || !no.TryGetComponent(out NetworkTransform nt) || !nt.CanCommitToTransform) continue;
                if (no.TryGetComponent(out NetworkPlayer player))
                {
                    if (player.IsOwner) player.OwnerTeleport(player.transform.position + delta);
                    continue;
                }
                Vector3 to = no.transform.position + delta;
                // Keep moving bodies moving (the van mid-drive): only the position jumps.
                if (no.TryGetComponent(out Rigidbody body) && !body.isKinematic) body.position = to;
                no.transform.position = to;
                nt.Teleport(to, no.transform.rotation, no.transform.localScale);
            }
            Physics.SyncTransforms();
        }

        /// <summary>This machine's streamer: the places it has loaded now.</summary>
        public void ReportLoaded(int mask)
        {
            LocalLoaded = mask;
            if (!IsSpawned) return;
            if (IsServer) loaded[NetworkManager.LocalClientId] = mask;
            else LoadedRpc(mask);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void LoadedRpc(int mask, RpcParams rpcParams = default) => loaded[rpcParams.Receive.SenderClientId] = mask;

        /// <summary>Host: does every machine in the session have place <paramref name="poi"/> loaded (or none of them)?</summary>
        public bool Everyone(int poi, bool hasIt)
        {
            int bit = 1 << poi;
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
            {
                bool has = loaded.TryGetValue(id, out int mask) && (mask & bit) != 0;
                if (has != hasIt) return false;
            }
            return true;
        }

        // SessionTravel.LevelReady: while the streamed place isn't yet loaded on every machine, the level's directors
        // hold their spawns (structure, run, loot, monsters belong to that scene on every machine).
        private bool StreamingReady()
        {
            int s = streamed.Value;
            return s < 0 || Everyone(s, true);
        }

        // Host: the anchor's scene is loaded while the van is near it and unloaded (after it is cleared) once the van
        // is away; with nothing loaded, the nearest place becomes the anchor.
        private void Update()
        {
            if (!IsSpawned || !IsServer || Atlas == null || WorldVan.Current == null) return;
            int a = anchor.Value;
            WorldPoi current = Atlas.Pois[a];
            Vector3 van = ToWorld(WorldVan.Current.transform.position);
            float d = current.FromGround(van);
            bool loaded = streamed.Value == a;
            bool want = current.built && Application.CanStreamedLevelBeLoaded(current.scene) && d < (loaded ? current.unloadDistance : current.loadDistance);
            if (want && !loaded) streamed.Value = (sbyte)a;
            else if (!want && loaded)
            {
                ServerClearing?.Invoke(a);
                streamed.Value = -1;
            }
            if (streamed.Value >= 0 || !Everyone(a, false)) return;
            int nearest = Nearest(van);
            // The nearest by ground, not by middle: a big site (the mall's district) is near long before its middle.
            if (nearest >= 0 && nearest != a && d > current.unloadDistance) anchor.Value = (sbyte)nearest;
        }

        public int Nearest(Vector3 world)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < Atlas.Pois.Count; i++)
            {
                float d = Atlas.Pois[i].FromGround(world);
                if (d < bestDistance) { bestDistance = d; best = i; }
            }
            return best;
        }

        public static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
