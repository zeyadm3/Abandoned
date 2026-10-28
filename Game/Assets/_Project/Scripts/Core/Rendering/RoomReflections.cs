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
        private Cubemap fallback;

        private void OnEnable()
        {
            probes.Clear();
            foreach (ReflectionProbe probe in FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
                if (probe.gameObject.scene == gameObject.scene) probes.Add(probe);
            foreach (ReflectionProbe p in probes)
            {
                p.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                // URP 17.3 subtracts every texture-less visible probe from its capped probe count. Over 64 cold
                // probes can produce a negative cluster allocation and a black frame. Waiting probes use a valid cube.
                if (p.mode != ReflectionProbeMode.Realtime || p.texture == null)
                {
                    fallback ??= CreateFallback();
                    p.mode = ReflectionProbeMode.Custom;
                    p.customBakedTexture = fallback;
                }
                p.enabled = true;
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
            drawn = 0;
            Camera eye = Camera.main;
            if (eye != null) probes.Sort((a, b) => Distance(a, eye).CompareTo(Distance(b, eye)));
            foreach (ReflectionProbe p in probes)
                if (p != null && !drawing.ContainsKey(p)) queue.Enqueue(p);
        }

        private static float Distance(ReflectionProbe probe, Camera eye) =>
            probe == null ? float.MaxValue : probe.bounds.SqrDistance(eye.transform.position);

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
                if (p == null || !p.gameObject.activeInHierarchy) { drawn++; continue; }
                // Unity refuses RenderProbe on a disabled component. Only the bounded in-flight set goes cold;
                // every waiting probe keeps a valid fallback cube, avoiding URP's negative cluster allocation.
                p.mode = ReflectionProbeMode.Realtime;
                p.enabled = true;
                drawing[p] = p.RenderProbe();
            }
        }

        public static Cubemap CreateFallback()
        {
            var cube = new Cubemap(4, TextureFormat.RGBAHalf, false) { name = "Unlit room reflection", hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black;
            for (int face = 0; face < 6; face++) cube.SetPixels(pixels, (CubemapFace)face);
            cube.Apply(false, true);
            return cube;
        }

        private void OnDestroy()
        {
            if (fallback == null) return;
            if (Application.isPlaying) Destroy(fallback);
            else DestroyImmediate(fallback);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // F1 only, and F1 never opens in a player build: there the GUI loop doesn't run for it at all.
        private GUIStyle style;

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 620, 72, 610, 22),
                $"room reflections {drawn}/{probes.Count}{(queue.Count + drawing.Count > 0 ? " drawing" : "")}", style);
        }
#endif
    }
}
