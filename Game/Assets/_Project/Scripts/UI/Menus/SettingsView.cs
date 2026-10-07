using System.Collections.Generic;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Voice;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Settings (GDD 20; tabs in UI step 5): Gameplay (look, comfort, tips, UI scale), Video (window,
    /// resolution, VSync, frame cap, quality, shadows, brightness), Audio (volumes, microphone with a live
    /// level, push to talk), Controls (rebinding) and Accessibility. Saved as they change; written to disk on Back.
    /// </summary>
    public class SettingsView
    {
        private static readonly string[] Tabs = { "Gameplay", "Video", "Audio", "Controls", "Accessibility" };

        private readonly MenuUi menu;
        private readonly VisualElement body;
        private readonly List<Button> tabButtons = new();
        private VisualElement micBar;
        private int tab;

        public VisualElement Root { get; }

        public SettingsView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            panel.AddToClassList("settings");
            MenuKit.Text(panel, "SETTINGS", "heading");
            VisualElement bar = MenuKit.Row(panel);
            bar.AddToClassList("tabs");
            for (int i = 0; i < Tabs.Length; i++)
            {
                int index = i;
                Button b = MenuKit.Button(bar, Tabs[i].ToUpperInvariant(), () => Show(index), SoundId.UiClick, small: true);
                b.AddToClassList("tab");
                tabButtons.Add(b);
            }
            var scroll = MenuKit.Scroll(panel, "settings__body");
            body = scroll;
            MenuKit.Button(panel, "Back", () =>
            {
                VoiceSettings.Flush();
                Prefs.Save();
                menu.Back();
            }, SoundId.UiBack);
            Show(0);
        }

        private void Show(int index)
        {
            tab = index;
            for (int i = 0; i < tabButtons.Count; i++) tabButtons[i].EnableInClassList("menu-button--on", i == tab);
            body.Clear();
            micBar = null;
            switch (tab)
            {
                case 0: Gameplay(); break;
                case 1: Video(); break;
                case 2: Audio(); break;
                case 3: Controls(); break;
                default: Accessibility(); break;
            }
        }

        private void Gameplay()
        {
            MenuKit.Text(body, "LOOK AND COMFORT", "section");
            MenuKit.Range(body, "Mouse sensitivity", GameSettings.MinSensitivity, GameSettings.MaxSensitivity, GameSettings.Sensitivity, "0.00x",
                v => GameSettings.Sensitivity = v);
            MenuKit.Range(body, "Field of view", GameSettings.MinFov, GameSettings.MaxFov, GameSettings.FieldOfView, "0",
                v => GameSettings.FieldOfView = Mathf.Round(v));
            MenuKit.Toggle(body, "Head bob", GameSettings.HeadBob, v => GameSettings.HeadBob = v);
            MenuKit.Toggle(body, "Camera shake", GameSettings.CameraShake, v => GameSettings.CameraShake = v);
            MenuKit.Text(body, "INTERFACE", "section");
            // Applied when the slider is let go: resizing the whole UI under the pointer mid-drag is unusable.
            Slider scale = MenuKit.Range(body, "UI scale", GameSettings.MinUiScale, GameSettings.MaxUiScale, GameSettings.UiScale, "0.00x", _ => { });
            scale.RegisterCallback<PointerCaptureOutEvent>(_ => GameSettings.UiScale = Mathf.Round(scale.value * 20f) / 20f);
            MenuKit.Toggle(body, "Show tips", Hints.Enabled, v => Hints.Enabled = v);
            MenuKit.Button(body, "Show all tips again", Hints.ResetSeen, small: true);
        }

        private void Video()
        {
            MenuKit.Text(body, "DISPLAY", "section");
            var modes = new[] { (FullScreenMode.FullScreenWindow, "Borderless fullscreen"), (FullScreenMode.ExclusiveFullScreen, "Fullscreen"), (FullScreenMode.Windowed, "Windowed") };
            MenuKit.Choice(body, "Window mode", modes.Select(m => m.Item2).ToArray(), System.Array.FindIndex(modes, m => m.Item1 == VideoSettings.WindowMode),
                i => VideoSettings.WindowMode = modes[i].Item1);
            List<Vector2Int> sizes = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().OrderByDescending(r => r.x * r.y).ToList();
            sizes.Insert(0, Vector2Int.zero);
            string[] names = sizes.Select(r => r == Vector2Int.zero ? "Desktop" : $"{r.x} x {r.y}").ToArray();
            MenuKit.Choice(body, "Resolution", names, Mathf.Max(0, sizes.IndexOf(VideoSettings.Resolution)), i => VideoSettings.Resolution = sizes[i]);
            MenuKit.Toggle(body, "VSync", VideoSettings.VSync, v => VideoSettings.VSync = v);
            MenuKit.Choice(body, "Frame cap (VSync off)", VideoSettings.FrameCaps.Select(c => c == 0 ? "Unlimited" : $"{c} fps").ToArray(),
                Mathf.Max(0, System.Array.IndexOf(VideoSettings.FrameCaps, VideoSettings.FrameCap)), i => VideoSettings.FrameCap = VideoSettings.FrameCaps[i]);
            MenuKit.Text(body, "GRAPHICS", "section");
            string[] quality = QualitySettings.names;
            if (quality.Length > 1)
                MenuKit.Choice(body, "Quality", quality, VideoSettings.Quality >= 0 ? VideoSettings.Quality : QualitySettings.GetQualityLevel(), i => VideoSettings.Quality = i);
            MenuKit.Choice(body, "Shadows", VideoSettings.ShadowNames, VideoSettings.Shadows, i => VideoSettings.Shadows = i);
            MenuKit.Range(body, "Brightness", -1f, 1.5f, VideoSettings.Brightness, "+0.0;-0.0;0", v => VideoSettings.Brightness = v);
            MenuKit.Text(body, "Window and resolution changes apply to the built game (the editor's Game view keeps its own size).", "text")
                .AddToClassList("text--small");
        }

        private void Audio()
        {
            MenuKit.Text(body, "VOLUME", "section");
            MenuKit.Percent(body, "Master", AudioLevels.Master, v => AudioLevels.Master = v);
            MenuKit.Percent(body, "Effects", AudioLevels.Sfx, v => AudioLevels.Sfx = v);
            MenuKit.Percent(body, "Ambience", AudioLevels.Ambience, v => AudioLevels.Ambience = v);
            MenuKit.Percent(body, "Music", AudioLevels.Music, v => AudioLevels.Music = v);
            MenuKit.Percent(body, "Voices", VoiceSettings.Volume, v => VoiceSettings.Volume = v);

            MenuKit.Text(body, "MICROPHONE", "section");
            string[] devices = Microphone.devices;
            var options = new List<string> { "System default" };
            options.AddRange(devices);
            int current = Mathf.Max(0, options.IndexOf(VoiceSettings.MicDevice));
            MenuKit.Choice(body, "Input device", options.ToArray(), current, i => VoiceSettings.MicDevice = i == 0 ? "" : options[i]);
            MenuKit.Switch(body, "Talk", VoiceSettings.Mode == VoiceMode.PushToTalk, $"Push to talk ({InputBindings.Display("PushToTalk")})", "Open mic",
                ptt => VoiceSettings.Mode = ptt ? VoiceMode.PushToTalk : VoiceMode.OpenMic);
            MenuKit.Toggle(body, "Mute my microphone", VoiceSettings.MicMuted, m => VoiceSettings.MicMuted = m);
            VisualElement meter = MenuKit.Row(body);
            meter.AddToClassList("setting");
            MenuKit.Text(meter, "Mic level", "setting__label");
            micBar = UiKit.Bar(meter, "mic-meter");
            Label note = MenuKit.Text(body, "", "text");
            note.AddToClassList("text--small");
            VisualElement bar = micBar;
            bar.schedule.Execute(() =>
            {
                VoiceTransmitter me = NetworkPlayer.Local != null ? NetworkPlayer.Local.GetComponent<VoiceTransmitter>() : null;
                UiKit.SetBar(bar, me != null ? Mathf.Clamp01(me.Level * 4f) : 0f, me != null && me.Transmitting ? "good" : null);
                note.text = me == null ? "Join or host a game to see your mic level. Steam voice always uses the system's default microphone; the input device applies next game."
                    : me.Transmitting ? "Sending: your crew hears you." : $"Not sending. Hold {InputBindings.Display("PushToTalk")} to talk (or switch to open mic).";
            }).Every(60);
        }

        private void Controls()
        {
            MenuKit.Text(body, "KEYS", "section");
            MenuKit.Text(body, "Rebind any action. Prompts in the game show your own keys.");
            MenuKit.Button(body, "Change keys", () => menu.Push(MenuScreen.Controls));
        }

        private void Accessibility()
        {
            MenuKit.Text(body, "ACCESSIBILITY", "section");
            MenuKit.Toggle(body, "Subtitles (warnings and threats)", GameSettings.Subtitles, v => GameSettings.Subtitles = v);
            MenuKit.Toggle(body, "Colourblind-safe scanner", GameSettings.ColorblindScanner, v => GameSettings.ColorblindScanner = v);
            MenuKit.Toggle(body, "Camera shake", GameSettings.CameraShake, v => GameSettings.CameraShake = v);
            MenuKit.Toggle(body, "Head bob", GameSettings.HeadBob, v => GameSettings.HeadBob = v);
        }
    }
}
