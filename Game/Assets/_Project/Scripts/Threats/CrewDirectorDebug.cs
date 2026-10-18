using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// F1 on the host: the Crew Director's phase and the last two minutes of the highest stress against the
    /// peak line (the video's "highest of the 4" graph), plus a stress bar over each player in the Scene view.
    /// </summary>
    public class CrewDirectorDebug : MonoBehaviour
    {
        private const int Samples = 240;
        private const float SampleEvery = 0.5f;

        private readonly float[] history = new float[Samples];
        private readonly bool[] quiet = new bool[Samples];
        private int head;
        private float nextSample;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            CrewDirector director = CrewDirector.Current;
            if (director == null || !GameAuthority.IsHost || Time.time < nextSample) return;
            nextSample = Time.time + SampleEvery;
            history[head] = director.Highest;
            quiet[head] = director.Cycle.Quiet;
            head = (head + 1) % Samples;
        }

        private void OnGUI()
        {
            CrewDirector director = CrewDirector.Current;
            if (!DebugView.Visible || director == null || !GameAuthority.IsHost) return;
            CrewDirectorConfig config = CrewDirectorConfig.Current;
            CrewDirectorCycle cycle = director.Cycle;
            string phase = !config.Enabled ? "OFF" : cycle.Phase switch
            {
                CrewDirectorPhase.BuildUp => "BUILD UP",
                CrewDirectorPhase.SustainPeak => "PEAK",
                CrewDirectorPhase.PeakFade => "PEAK FADE",
                _ => "RELAX"
            };
            string left = cycle.Remaining > 0f ? $"  {cycle.Remaining:0}s left" : $"  {cycle.InPhase:0}s";
            var label = new Rect(Screen.width - 360f, 74f, 350f, 22f);
            GUI.Label(label, $"DIRECTOR {phase}{left}  stress {director.Highest:0}/{config.Peak:0}{(CrewDirector.Quiet ? "  holding monsters" : "")}");

            var box = new Rect(Screen.width - 360f, 96f, Samples, 60f);
            Color was = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            for (int i = 0; i < Samples; i++)
            {
                int at = (head + i) % Samples;
                float h = box.height * history[at] / IntensityTrack.Max;
                GUI.color = quiet[at] ? new Color(1f, 0.55f, 0.6f, 0.35f) : Color.clear;
                GUI.DrawTexture(new Rect(box.x + i, box.y, 1f, box.height), Texture2D.whiteTexture);
                GUI.color = new Color(1f, 0.25f, 0.5f);
                GUI.DrawTexture(new Rect(box.x + i, box.yMax - h, 1f, Mathf.Max(1f, h)), Texture2D.whiteTexture);
            }
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            GUI.DrawTexture(new Rect(box.x, box.yMax - box.height * config.Peak / IntensityTrack.Max, box.width, 1f), Texture2D.whiteTexture);
            GUI.color = was;

            float y = box.yMax + 2f;
            foreach (var pair in director.Tracks)
            {
                if (pair.Key == null) continue;
                GUI.Label(new Rect(box.x, y, 350f, 20f), $"  {pair.Key.DisplayName}: {pair.Value.Value:0}");
                y += 18f;
            }
        }
#endif

        private void OnDrawGizmos()
        {
            CrewDirector director = CrewDirector.Current;
            if (!DebugView.Visible || director == null) return;
            foreach (var pair in director.Tracks)
            {
                if (pair.Key == null) continue;
                Vector3 head = pair.Key.transform.position + Vector3.up * 2.2f;
                float fill = pair.Value.Value / IntensityTrack.Max;
                Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
                Gizmos.DrawWireCube(head + Vector3.up * 0.5f, new Vector3(0.12f, 1f, 0.12f));
                Gizmos.color = Color.Lerp(Color.green, Color.red, fill);
                Gizmos.DrawCube(head + Vector3.up * fill * 0.5f, new Vector3(0.1f, fill, 0.1f));
            }
        }
    }
}
