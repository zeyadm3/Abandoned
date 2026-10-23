using Abandoned.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.World
{
    /// <summary>
    /// Every machine: keeps loaded exactly the place the host says (<see cref="WorldState.Streamed"/>, the anchor while
    /// the van is near it), additively, the world staying the active scene. A loaded place is fitted into the world
    /// (<see cref="PoiAdapter"/>), takes over the sky (<see cref="WorldRoot.UsePlaceLook"/>) and its flat stand-in is
    /// hidden. Reports what it has loaded, so the host knows when every machine has a place (its spawns wait for that)
    /// and when none still has it (the anchor may move). The host clears a place's objects before it says unload.
    /// </summary>
    public class PoiStreamer : MonoBehaviour
    {
        private readonly HashSet<int> loaded = new();
        private readonly HashSet<int> busy = new();
        private int reported = -1;

        public static PoiStreamer Current { get; private set; }

        public bool IsLoaded(int poi) => loaded.Contains(poi);

        /// <summary>This machine: a place finished loading (true) or unloading (false).</summary>
        public event System.Action<int, bool> Loaded;

        private void Awake() => Current = this;

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            WorldState state = WorldState.Current;
            if (state == null || state.Atlas == null) return;
            int target = state.Streamed;
            for (int i = 0; i < state.Atlas.Pois.Count; i++)
            {
                if (busy.Contains(i)) continue;
                bool isLoaded = loaded.Contains(i);
                if (i == target && !isLoaded) Load(i, state.Atlas.Pois[i]);
                else if (i != target && isLoaded) Unload(i, state.Atlas.Pois[i]);
            }
            int mask = 0;
            foreach (int i in loaded) mask |= 1 << i;
            if (mask != reported && busy.Count == 0)
            {
                reported = mask;
                state.ReportLoaded(mask);
            }
        }

        private void Load(int index, WorldPoi poi)
        {
            if (!Application.CanStreamedLevelBeLoaded(poi.scene)) return;
            busy.Add(index);
            Debug.Log($"[World] Loading {poi.scene}.");
            // The scene's own F1 switch sets the overlays as it wakes; the world's state wins.
            bool debug = DebugView.Visible;
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
                if (WorldRoot.Current != null)
                {
                    WorldRoot.Current.UsePlaceLook(poi);
                    WorldRoot.Current.ShowStandIn(index, false);
                }
                PoiAdapter.Adopt(scene, poi);
                DebugView.SetVisible(debug);
                loaded.Add(index);
                // The place's own collection truck claimed the cargo role as it woke and was then hidden: the van takes it back.
                if (WorldVan.Current != null) WorldVan.Current.ClaimCargo();
                Loaded?.Invoke(index, true);
            };
        }

        private void Unload(int index, WorldPoi poi)
        {
            busy.Add(index);
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
                if (WorldRoot.Current != null)
                {
                    WorldRoot.Current.ShowStandIn(index, true);
                    WorldRoot.Current.UsePlaceLook(null);
                }
                Loaded?.Invoke(index, false);
            };
        }
    }
}
