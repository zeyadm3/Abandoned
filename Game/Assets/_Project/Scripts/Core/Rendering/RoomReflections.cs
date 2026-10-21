using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// 0.24.13: the map's room reflections. Its child probes (placed per room by the Editor's RoomReflectionProbes,
    /// realtime, box-projected) are drawn a few at a time after the map loads, and again once the lights change
    /// (<see cref="LightingEvents"/>), so wet concrete, steel and glass reflect the room they are in instead of the sky,
    /// and stop reflecting lamps that have gone out. Realtime on purpose: the lights change during a job, and baked
    /// light can't follow them (M7.2). F1 shows how many are drawn.
    /// </summary>
    public sealed class RoomReflections : MonoBehaviour
    {
        [Tooltip("Probes started per frame. Each draws its six faces over the following frames (time-sliced).")]
        [SerializeField, Range(1, 8)] private int perFrame = 2;
        [Tooltip("Wait after a lighting change before redrawing, so fixtures have finished fading.")]
        [SerializeField, Min(0f)] private float settle = 1.5f;

        private readonly List<ReflectionProbe> probes = new();
        private readonly Queue<ReflectionProbe> queue = new();
        private readonly Dictionary<ReflectionProbe, int> drawing = new();
        private readonly List<ReflectionProbe> finished = new();
        private float redrawAt = -1f;
        private int drawn;
        private GUIStyle style;

        private void OnEnable()
        {
            probes.Clear();
            GetComponentsInChildren(probes);
            foreach (ReflectionProbe p in probes)
            {
                p.mode = ReflectionProbeMode.Realtime;
                p.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            }
            LightingEvents.Changed += OnLightingChanged;
            // A batch session has nothing to draw into; tests and builds' first frame skip it.
            if (!Application.isBatchMode) Redraw();
        }

        private void OnDisable() => LightingEvents.Changed -= OnLightingChanged;

        private void OnLightingChanged() => redrawAt = Time.unscaledTime + settle;

        private void Redraw()
        {
            queue.Clear();
            drawing.Clear();
            drawn = 0;
            foreach (ReflectionProbe p in probes) if (p != null) queue.Enqueue(p);
        }

        private void Update()
        {
            if (redrawAt >= 0f && Time.unscaledTime >= redrawAt && !Application.isBatchMode)
            {
                redrawAt = -1f;
                Redraw();
            }
            finished.Clear();
            foreach (var kv in drawing)
                if (kv.Key == null || kv.Key.IsFinishedRendering(kv.Value)) finished.Add(kv.Key);
            foreach (ReflectionProbe p in finished)
            {
                drawing.Remove(p);
                drawn++;
            }
            while (drawing.Count < perFrame * 2 && queue.Count > 0)
            {
                ReflectionProbe p = queue.Dequeue();
                if (p == null || !p.isActiveAndEnabled) { drawn++; continue; }
                drawing[p] = p.RenderProbe();
            }
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 620, 72, 610, 22),
                $"room reflections {drawn}/{probes.Count}{(queue.Count + drawing.Count > 0 ? " drawing" : "")}", style);
        }
    }
}
