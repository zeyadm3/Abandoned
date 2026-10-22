using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.World
{
    /// <summary>
    /// Every machine: loads the anchor place's scene additively when the van comes within its load distance and unloads
    /// it once the van is past its unload distance (hysteresis, so it doesn't flicker at the edge). Only the anchor is
    /// ever loaded (<see cref="WorldState"/>). A loaded place is fitted into the world (<see cref="PoiAdapter"/>) and
    /// its flat stand-in is hidden. Reports what it has loaded to the host. Its NetworkObjects are spawned by the host's
    /// directors in that scene once every machine has it (SessionTravel.LevelReady).
    /// </summary>
    public class PoiStreamer : MonoBehaviour
    {
        private readonly HashSet<int> loaded = new();
        private readonly HashSet<int> busy = new();
        private int reported = -1;

        public static PoiStreamer Current { get; private set; }

        public bool IsLoaded(int poi) => loaded.Contains(poi);
        public bool IsBusy => busy.Count > 0;

        private void Awake() => Current = this;

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            WorldState state = WorldState.Current;
            if (state == null || state.Atlas == null) return;
            Vector3? focus = Focus(state);
            for (int i = 0; i < state.Atlas.Pois.Count; i++)
            {
                if (busy.Contains(i)) continue;
                WorldPoi poi = state.Atlas.Pois[i];
                bool isLoaded = loaded.Contains(i);
                bool want = false;
                if (i == state.Anchor && poi.built && focus.HasValue && Application.CanStreamedLevelBeLoaded(poi.scene))
                {
                    float d = WorldState.Flat(focus.Value, poi.Centre);
                    want = d < (isLoaded ? poi.unloadDistance : poi.loadDistance);
                }
                if (want && !isLoaded) Load(i, poi);
                else if (!want && isLoaded) Unload(i, poi);
            }
            int mask = 0;
            foreach (int i in loaded) mask |= 1 << i;
            if (mask != reported)
            {
                reported = mask;
                state.ReportLoaded(mask);
            }
        }

        // What the streaming follows: the van (the crew travels in it); before it exists, this machine's player.
        private static Vector3? Focus(WorldState state)
        {
            if (WorldVan.Current != null && WorldVan.Current.IsSpawned) return state.ToWorld(WorldVan.Current.transform.position);
            var local = Networking.NetworkPlayer.Local;
            return local != null ? state.ToWorld(local.transform.position) : null;
        }

        private void Load(int index, WorldPoi poi)
        {
            busy.Add(index);
            Debug.Log($"[World] Loading {poi.scene}.");
            AsyncOperation op = SceneManager.LoadSceneAsync(poi.scene, LoadSceneMode.Additive);
            if (op == null)
            {
                busy.Remove(index);
                return;
            }
            op.completed += _ =>
            {
                busy.Remove(index);
                Scene scene = SceneManager.GetSceneByName(poi.scene);
                if (!scene.IsValid()) return;
                PoiAdapter.Adopt(scene, poi);
                loaded.Add(index);
                if (WorldRoot.Current != null) WorldRoot.Current.ShowStandIn(index, false);
                Loaded?.Invoke(index, true);
            };
        }

        private void Unload(int index, WorldPoi poi)
        {
            busy.Add(index);
            Unloading?.Invoke(index);
            Debug.Log($"[World] Unloading {poi.scene}.");
            AsyncOperation op = SceneManager.UnloadSceneAsync(poi.scene);
            if (op == null)
            {
                busy.Remove(index);
                loaded.Remove(index);
                return;
            }
            op.completed += _ =>
            {
                busy.Remove(index);
                loaded.Remove(index);
                if (WorldRoot.Current != null) WorldRoot.Current.ShowStandIn(index, true);
                Loaded?.Invoke(index, false);
            };
        }

        /// <summary>This machine: a place finished loading (true) or unloading (false).</summary>
        public event System.Action<int, bool> Loaded;
        /// <summary>This machine: a place is about to unload (the host clears its objects first).</summary>
        public event System.Action<int> Unloading;
    }
}
