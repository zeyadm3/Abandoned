using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Structure
{
    /// <summary>Local island reflections follow the job's power, with a single probe capture in flight.</summary>
    public sealed class IslandReflectionRefresh : MonoBehaviour
    {
        [SerializeField] private ReflectionProbe[] probes;
        private int[] shown;
        private int active = -1, renderId = -1, capturingSignature;

        private void OnEnable()
        {
            shown = new int[probes?.Length ?? 0];
            for (int i = 0; i < shown.Length; i++) shown[i] = int.MinValue;
            active = -1;
            renderId = -1;
        }

        private void LateUpdate()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || probes == null || probes.Length == 0) return;
            if (active >= 0)
            {
                ReflectionProbe pending = probes[active];
                if (pending != null && !pending.IsFinishedRendering(renderId)) return;
                shown[active] = capturingSignature;
                active = -1;
                renderId = -1;
            }

            Camera camera = Camera.main;
            if (camera == null) return;
            RunState run = RunState.Current;
            int signature = run != null && run.IsSpawned
                ? (run.State.PowerOff ? 1 : 0) | (run.State.Night ? 2 : 0) | ((int)run.State.Phase << 2) | (run.State.Danger << 4)
                : 0;
            if (RunHorrorDirector.Current != null && RunHorrorDirector.Current.FinalPhase) signature |= 1 << 12;

            float closest = float.MaxValue;
            int selected = -1;
            for (int i = 0; i < probes.Length; i++)
            {
                ReflectionProbe probe = probes[i];
                if (probe == null || !probe.isActiveAndEnabled || shown[i] == signature) continue;
                float distance = probe.bounds.SqrDistance(camera.transform.position);
                if (distance > 900f || distance >= closest) continue;
                selected = i;
                closest = distance;
            }
            if (selected < 0) return;
            capturingSignature = signature;
            renderId = probes[selected].RenderProbe();
            if (renderId >= 0) active = selected;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!Abandoned.Core.DebugView.Visible) return;
            GUI.Label(new Rect(Screen.width - 360f, 326f, 350f, 22f),
                $"ISLAND REFLECTIONS {probes?.Length ?? 0} probes / capturing {active >= 0}");
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(ReflectionProbe[] localProbes) => probes = localProbes;
#endif
    }
}
