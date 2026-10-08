using Unity.Profiling;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// F1 performance readout (M7.6) for profiling a real 4-player session: frame time (average and
    /// worst over the last second), draw batches, set-pass calls, triangles, awake physics bodies,
    /// managed allocation per frame and memory. Counters only run while F1 is on.
    /// </summary>
    public class PerfOverlay : MonoBehaviour
    {
        private ProfilerRecorder batches, setPass, triangles, gcAlloc, mainThread;
        private float windowStart, worst, sum;
        private int frames, awakeBodies;
        private string line = "";
        private GUIStyle style;

        private void OnEnable()
        {
            DebugView.Changed += OnDebugChanged;
            OnDebugChanged(DebugView.Visible);
        }

        private void OnDisable()
        {
            DebugView.Changed -= OnDebugChanged;
            Stop();
        }

        private void OnDebugChanged(bool visible)
        {
            Stop();
            if (!visible) return;
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        }

        private void Stop()
        {
            batches.Dispose();
            setPass.Dispose();
            triangles.Dispose();
            gcAlloc.Dispose();
            mainThread.Dispose();
        }

        private void Update()
        {
            if (!DebugView.Visible) return;
            float dt = Time.unscaledDeltaTime;
            sum += dt;
            worst = Mathf.Max(worst, dt);
            frames++;
            if (Time.unscaledTime - windowStart < 1f) return;
            // Once a second: the expensive count and the text.
            awakeBodies = 0;
            foreach (Rigidbody b in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
                if (!b.isKinematic && !b.IsSleeping()) awakeBodies++;
            float avg = frames > 0 ? sum / frames : 0f;
            line = $"PERF {1f / Mathf.Max(avg, 1e-5f):0} fps  {avg * 1000f:0.0} ms (worst {worst * 1000f:0.0})  " +
                   $"main {Ms(mainThread):0.0} ms  batches {Value(batches)}  setpass {Value(setPass)}  tris {Value(triangles) / 1000}k  " +
                   $"bodies awake {awakeBodies}  GC {Value(gcAlloc) / 1024f:0.0} KB/frame  mem {System.GC.GetTotalMemory(false) / (1024 * 1024)} MB";
            windowStart = Time.unscaledTime;
            sum = worst = 0f;
            frames = 0;
        }

        private static long Value(ProfilerRecorder r) => r.Valid ? r.LastValue : -1;

        private static double Ms(ProfilerRecorder r)
        {
            if (!r.Valid || r.Count == 0) return -1;
            double total = 0;
            for (int i = 0; i < r.Count; i++) total += r.GetSample(i).Value;
            return total / r.Count * 1e-6;
        }

        // QA P-08: a debug overlay only; in player builds it never draws, so it doesn't sit in the GUI loop either.
        private void Start()
        {
            if (!Abandoned.Core.DevTools.Enabled) enabled = false;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || line.Length == 0) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 13 };
            GUI.Label(new Rect(10f, Screen.height - 26f, Screen.width - 20f, 22f), line, style);
        }
    }
}
