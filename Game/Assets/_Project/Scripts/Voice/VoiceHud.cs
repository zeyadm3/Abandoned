using System.Collections.Generic;
using System.Text;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Voice
{
    /// <summary>
    /// Voice on the HUD (M8.1), drawn by the local player only: a talking / radio indicator for
    /// ourselves, a speaker mark over every remote player who is talking, a line saying why we can't
    /// talk (no mic, Steam off, muted); with F1 the per-speaker stream stats (OnGUI, debug only).
    /// </summary>
    public class VoiceHud : MonoBehaviour
    {
        [SerializeField] private VoiceTransmitter transmitter;
        [SerializeField] private int fontSize = 15;

        private readonly StringBuilder text = new();
        private readonly Dictionary<NetworkVoice, Label> marks = new();
        private readonly List<NetworkVoice> gone = new();
        private GUIStyle style;
        private Label own;

        /// <summary>Tests: our own voice line.</summary>
        public string OwnLine { get; private set; }

        private void Update()
        {
            if (transmitter == null) return;
            OwnLine = OwnState();
            if (own == null)
            {
                if (UI.HudLayer.Root == null) return;
                own = UI.HudLayer.Label("hud-voice");
            }
            UI.MenuKit.Show(own, true);
            if (own.text != OwnLine) own.text = OwnLine;
            UpdateSpeakers();
        }

        private void OnDisable()
        {
            if (own != null) UI.MenuKit.Show(own, false);
            foreach (Label l in marks.Values) UI.HudLayer.Remove(l);
            marks.Clear();
        }

        private void OnDestroy() => UI.HudLayer.Remove(own);

        private string OwnState()
        {
            IVoiceCapture capture = transmitter.Capture;
            string talk = InputBindings.Display("PushToTalk"), radio = InputBindings.Display("Radio");
            if (VoiceSettings.MicMuted) return "<color=#ff7766>MIC MUTED</color>";
            if (capture != null && capture.Problem.Length > 0) return $"<color=#ff7766>VOICE: {capture.Problem}</color>";
            if (transmitter.Transmitting) return transmitter.OnRadio ? "<color=#ffd24d>● RADIO</color>" : "<color=#7dff7d>● TALKING</color>";
            return VoiceSettings.Mode == VoiceMode.PushToTalk ? $"HOLD {talk} TO TALK, {radio} FOR RADIO" : "OPEN MIC";
        }

        private void UpdateSpeakers()
        {
            Camera cam = Camera.main;
            foreach (NetworkVoice v in NetworkVoice.All)
            {
                if (v == null || v.IsOwner) continue;
                bool speaking = v.IsSpeaking && cam != null;
                if (!marks.TryGetValue(v, out Label mark))
                {
                    if (!speaking) continue;
                    marks[v] = mark = UI.HudLayer.Label("hud-marker");
                    if (mark == null) { marks.Remove(v); continue; }
                }
                UI.MenuKit.Show(mark, speaking);
                if (!speaking) continue;
                string glyph = v.IsOnRadio ? "<color=#ffd24d>((R))</color>" : "<color=#7dff7d>((•))</color>";
                if (mark.text != glyph) mark.text = glyph;
                UI.HudLayer.Place(mark, v.transform.position + Vector3.up * 2.4f, cam);
            }
            // Speakers who left (a reused list: this runs every frame).
            gone.Clear();
            foreach (KeyValuePair<NetworkVoice, Label> m in marks) if (m.Key == null) gone.Add(m.Key);
            foreach (NetworkVoice g in gone) { UI.HudLayer.Remove(marks[g]); marks.Remove(g); }
        }

        private void OnGUI()
        {
            if (transmitter == null || !DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };
            DrawDebug();
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
