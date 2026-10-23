using System.Text;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>F1: the open world's anchor, the van's world position and each place's distance and streaming state.</summary>
    public class WorldDebugView : MonoBehaviour
    {
        private GUIStyle style;
        private readonly StringBuilder text = new();

        private void OnGUI()
        {
            WorldState state = WorldState.Current;
            if (!DebugView.Visible || state == null || state.Atlas == null) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(.6f, .9f, 1f) } };
            text.Clear();
            Vector3 van = WorldVan.Current != null ? state.ToWorld(WorldVan.Current.transform.position) : Vector3.zero;
            text.AppendLine($"WORLD origin {state.AnchorPoi?.scene} {state.Offset:F0}  van world {van:F0}");
            PoiStreamer streamer = PoiStreamer.Current;
            for (int i = 0; i < state.Atlas.Pois.Count; i++)
            {
                WorldPoi poi = state.Atlas.Pois[i];
                string mark = i == state.Anchor ? ">" : " ";
                string load = streamer != null && streamer.IsLoaded(i) ? "LOADED" : poi.built ? "-" : "planned";
                text.AppendLine($"{mark} {poi.scene,-12} {poi.FromGround(van),6:0} m  {load}");
            }
            GUI.Label(new Rect(Screen.width - 330f, 120f, 320f, 20f * (state.Atlas.Pois.Count + 2)), text.ToString(), style);
        }
    }
}
