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
        private readonly Dictionary<NetworkVoice, VisualElement> marks = new();
        private readonly List<NetworkVoice> gone = new();
        private GUIStyle style;
        private Label own;
        private VisualElement hud, vitals;

        private const float LayoutGap = 12f, ScreenInset = 24f, MaxWidth = 440f;

        /// <summary>Tests: our own voice line.</summary>
        public string OwnLine { get; private set; }

        private void Update()
        {
            VisualElement layer = UI.HudLayer.Root;
            if (transmitter == null || layer == null)
            {
                ReleaseHud();
                return;
            }
            OwnLine = OwnState();
            if (own == null || own.parent != layer || hud != layer)
            {
                ReleaseHud();
                hud = layer;
                hud.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                own = UI.HudLayer.Label("hud-voice", "crew-voice");
                own.enableRichText = false;
            }
            BindVitals();
            UI.MenuKit.Show(own, true);
            own.EnableInClassList("crew-voice--transmitting", transmitter.Transmitting);
            own.EnableInClassList("crew-voice--radio", transmitter.OnRadio);
            own.EnableInClassList("crew-voice--fault", VoiceSettings.MicMuted || transmitter.Capture != null && transmitter.Capture.Problem.Length > 0);
            if (own.text != OwnLine) own.text = OwnLine;
            PositionOwn();
            UpdateSpeakers();
        }

        private void BindVitals()
        {
            VisualElement next = hud.Q(className: "h-vitals");
            if (vitals == next) return;
            vitals?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            vitals = next;
            vitals?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent _) => PositionOwn();

        private void PositionOwn()
        {
            if (own == null || !UI.HudLayer.TryLocalRect(hud, hud, out Rect area) || area.height <= ScreenInset + LayoutGap) return;
            bool anchored = UI.HudLayer.TryLocalRect(vitals, hud, out Rect monitor);
            float left = anchored ? monitor.xMin : ScreenInset;
            left = Mathf.Clamp(left, 0f, Mathf.Max(0f, area.width - ScreenInset));
            float width = Mathf.Min(MaxWidth, Mathf.Max(280f, area.width * .3f), area.width - left - ScreenInset);
            if (!UI.HudLayer.Finite(width) || width <= 0f) return;
            // Anchor the lower edge to the live monitor's top, so LIGHT/LOAD and compact mode cannot cover it.
            own.style.top = StyleKeyword.Auto;
            own.style.bottom = anchored ? Mathf.Clamp(area.height - monitor.yMin + LayoutGap, ScreenInset, area.height - LayoutGap) : ScreenInset;
            own.style.left = left;
            own.style.width = width;
            own.style.maxWidth = width;
        }

        private void ReleaseHud()
        {
            hud?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            vitals?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            UI.HudLayer.Remove(own);
            own = null;
            hud = vitals = null;
            foreach (VisualElement l in marks.Values) UI.HudLayer.Remove(l);
            marks.Clear();
        }

        private void OnDisable() => ReleaseHud();
        private void OnDestroy() => ReleaseHud();

        private string OwnState()
        {
            IVoiceCapture capture = transmitter.Capture;
            string talk = InputBindings.Display("PushToTalk"), radio = InputBindings.Display("Radio");
            if (VoiceSettings.MicMuted) return Loc.T("MIC / MUTED");
            if (capture != null && capture.Problem.Length > 0) return Loc.T("VOICE / {0}", Loc.T(capture.Problem));
            if (transmitter.Transmitting) return transmitter.OnRadio ? Loc.T("\u25CF RADIO TRANSMITTING") : Loc.T("\u25CF VOICE TRANSMITTING");
            return VoiceSettings.Mode == VoiceMode.PushToTalk ? Loc.T("HOLD {0} TO TALK  /  {1} RADIO", talk, radio) : Loc.T("MIC / OPEN");
        }

        private void UpdateSpeakers()
        {
            Camera cam = Camera.main;
            foreach (NetworkVoice v in NetworkVoice.All)
            {
                if (v == null || v.IsOwner) continue;
                bool speaking = v.IsSpeaking && cam != null;
                if (!marks.TryGetValue(v, out VisualElement mark))
                {
                    if (!speaking || UI.HudLayer.World == null) continue;
                    // Over their head (UI step 3): a speaker icon and who it is; yellow on the radio.
                    mark = new VisualElement { pickingMode = PickingMode.Ignore };
                    mark.AddToClassList("speaker-mark");
                    UI.UiKit.Icon(mark, "icon/audioOn", "small");
                    var who = new Label(Networking.NetworkPlayer.NameOf(v.OwnerClientId).ToUpperInvariant()) { pickingMode = PickingMode.Ignore, enableRichText = false };
                    who.AddToClassList("speaker-mark__name");
                    mark.Add(who);
                    UI.HudLayer.World.Add(mark);
                    marks[v] = mark;
                }
                UI.MenuKit.Show(mark, speaking);
                if (!speaking) continue;
                mark.EnableInClassList("speaker-mark--radio", v.IsOnRadio);
                UI.HudLayer.Place(mark, v.transform.position + Vector3.up * 2.3f, cam);
            }
            // Speakers who left (a reused list: this runs every frame).
            gone.Clear();
            foreach (KeyValuePair<NetworkVoice, VisualElement> m in marks) if (m.Key == null) gone.Add(m.Key);
            foreach (NetworkVoice g in gone) { UI.HudLayer.Remove(marks[g]); marks.Remove(g); }
        }

        // F1 stream stats: a debug overlay, so player builds don't carry an OnGUI pass (QA P-08).
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
#endif
    }
}
