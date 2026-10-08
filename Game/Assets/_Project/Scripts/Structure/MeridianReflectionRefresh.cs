using Abandoned.Extraction;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Refreshes mall reflections after power changes, staggering faces and probes across frames.</summary>
    public sealed class MeridianReflectionRefresh : MonoBehaviour
    {
        [SerializeField] private ReflectionProbe[] probes;
        [SerializeField] private MeridianPresentationConfig config;
        private int shownSignature = int.MinValue, pendingSignature = int.MinValue;
        private int probeIndex, renderId = -1;
        private float nextRefresh;

        private void Update()
        {
            if (config == null || probes == null || probes.Length == 0) return;
            RunState run = RunState.Current;
            int signature = run != null && run.IsSpawned
                ? (run.State.PowerOff ? 1 : 0) | (run.State.Night ? 2 : 0) | ((int)run.State.Phase << 2) | (run.State.Danger << 4)
                : 0;
            if (RunHorrorDirector.Current != null && RunHorrorDirector.Current.FinalPhase) signature |= 1 << 12;
            if (pendingSignature == int.MinValue)
            {
                if (signature == shownSignature || Time.time < nextRefresh) return;
                pendingSignature = signature;
                probeIndex = 0;
                renderId = -1;
            }
            if (probeIndex >= probes.Length)
            {
                shownSignature = pendingSignature;
                pendingSignature = int.MinValue;
                nextRefresh = Time.time + config.ReflectionRefreshSeconds;
                return;
            }
            ReflectionProbe probe = probes[probeIndex];
            if (probe == null) { probeIndex++; renderId = -1; return; }
            if (renderId < 0) renderId = probe.RenderProbe();
            else if (probe.IsFinishedRendering(renderId)) { probeIndex++; renderId = -1; }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!Abandoned.Core.DebugView.Visible) return;
            GUI.Label(new Rect(Screen.width - 360f, 300f, 350f, 22f),
                $"REFLECTIONS {probes?.Length ?? 0} probes / refreshing {pendingSignature != int.MinValue}");
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(ReflectionProbe[] mallProbes, MeridianPresentationConfig settings)
        {
            probes = mallProbes;
            config = settings;
        }
#endif
    }
}
