using System.Text;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Placeholder voice UI (OnGUI until the UI milestone), drawn by the local player only: a talking /
    /// radio indicator for ourselves, a speaker mark over every remote player who is talking, a line
    /// saying why we can't talk (no mic, Steam off, muted), and with F1 the per-speaker stream stats.
    /// </summary>
    public class VoiceHud : MonoBehaviour
    {
        [SerializeField] private VoiceTransmitter transmitter;
        [SerializeField] private int fontSize = 15;

        private readonly StringBuilder text = new();
        private GUIStyle style, mark;

        private void OnGUI()
        {
            if (transmitter == null) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };
            mark ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize + 3, alignment = TextAnchor.MiddleCenter, richText = true };

            DrawOwnState();
            DrawSpeakers();
            if (DebugView.Visible) DrawDebug();
        }

        private void DrawOwnState()
        {
            string line;
            IVoiceCapture capture = transmitter.Capture;
            if (VoiceSettings.MicMuted) line = "<color=#ff7766>Mic muted</color>";
            else if (capture != null && capture.Problem.Length > 0) line = $"<color=#ff7766>Voice: {capture.Problem}</color>";
            else if (transmitter.Transmitting) line = transmitter.OnRadio ? "<color=#ffd24d>● RADIO</color>" : "<color=#7dff7d>● TALKING</color>";
            else line = VoiceSettings.Mode == VoiceMode.PushToTalk ? "<color=#bbbbbb>Hold V to talk, R for radio</color>" : "<color=#bbbbbb>Open mic</color>";
            GUI.Label(new Rect(12f, Screen.height - 64f, 420f, 24f), line, style);
        }

        private void DrawSpeakers()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (NetworkVoice v in NetworkVoice.All)
            {
                if (v == null || v.IsOwner || !v.IsSpeaking) continue;
                Vector3 screen = cam.WorldToScreenPoint(v.transform.position + Vector3.up * 2.4f);
                if (screen.z <= 0f) continue;
                string glyph = v.IsOnRadio ? "<color=#ffd24d>((R))</color>" : "<color=#7dff7d>((•))</color>";
                GUI.Label(new Rect(screen.x - 40f, Screen.height - screen.y - 14f, 80f, 28f), glyph, mark);
            }
        }

        private void DrawDebug()
        {
            text.Clear().AppendLine($"<b>VOICE</b>  {transmitter.Capture?.Name ?? "-"}  mode {VoiceSettings.Mode}  vol {VoiceSettings.Volume:0.00}  level {transmitter.Level:0.000}");
            foreach (NetworkVoice v in NetworkVoice.All)
            {
                if (v == null) continue;
                VoicePlayback p = v.Playback;
                VoiceJitterBuffer b = p != null ? p.Buffer : null;
                text.AppendLine($"  p{v.OwnerClientId}{(v.IsOwner ? " (me)" : "")}  {(v.IsSpeaking ? (v.IsOnRadio ? "RADIO" : "talk") : "-")}  " +
                                $"sent {v.PacketsSent} recv {v.PacketsReceived} rej {v.PacketsRejected}" +
                                (b != null ? $"  buf {b.Count} under {b.Underruns} drop {b.Dropped}  gain {p.Gain:0.00} walls {p.Walls} lp {p.Cutoff:0}Hz" : "") +
                                (v.Radio != null && v.Radio.SamplesReceived > 0 ? $"  radio {v.Radio.SamplesReceived}" : ""));
            }
            GUI.Label(new Rect(Screen.width - 620f, Screen.height - 160f, 610f, 150f), text.ToString(), style);
        }
    }
}
