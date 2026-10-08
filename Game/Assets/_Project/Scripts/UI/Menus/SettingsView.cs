using System;
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
    /// Settings (GDD 20; redone in 0.12.5): a landscape screen with the pages listed down the left
    /// (Gameplay, Controls, Video, Graphics, Audio, Voice, Accessibility) and the open page on the right,
    /// every option with a one-line explanation. Changes apply at once and are written to disk on Back;
    /// each page can be put back to its defaults.
    /// </summary>
    public class SettingsView
    {
        private sealed class Page
        {
            public string Name, Blurb;
            public Action Build, Reset;
        }

        private readonly MenuUi menu;
        private readonly List<Page> pages = new();
        private readonly List<Button> nav = new();
        private readonly Label pageTitle, pageBlurb;
        private readonly ScrollView body;
        private VisualElement micBar;
        private int open;

        public VisualElement Root { get; }

        public SettingsView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            panel.AddToClassList("settings");
            // MenuKit.Panel sizes inline; the landscape layout needs the room.
            panel.style.width = 1360f;
            panel.style.maxWidth = Length.Percent(96f);

            MenuKit.Text(panel, "SETTINGS", "heading");
            MenuKit.Text(panel, "Changes apply straight away and are saved when you leave this screen.", "subtitle");
            var columns = new VisualElement();
            columns.AddToClassList("settings__columns");
            panel.Add(columns);

            var list = new VisualElement();
            list.AddToClassList("settings__nav");
            columns.Add(list);
            var page = new VisualElement();
            page.AddToClassList("settings__page");
            columns.Add(page);
            pageTitle = MenuKit.Text(page, "", "settings__page-title");
            pageBlurb = MenuKit.Text(page, "", "settings__page-blurb");
            body = MenuKit.Scroll(page, "settings__body");

            Define();
            for (int i = 0; i < pages.Count; i++)
            {
                int index = i;
                Button b = MenuKit.Button(list, pages[i].Name, () => Show(index));
                b.AddToClassList("settings__nav-button");
                nav.Add(b);
            }

            VisualElement footer = MenuKit.Row(panel);
            footer.AddToClassList("settings__footer");
            MenuKit.Button(footer, "Back", () =>
            {
                VoiceSettings.Flush();
                Prefs.Save();
                menu.Back();
            }, SoundId.UiBack).AddToClassList("settings__back");
            MenuKit.Button(footer, "Restore this page's defaults", () =>
            {
                pages[open].Reset();
                Show(open);
            }, SoundId.UiBack, small: true).AddToClassList("settings__reset");
            Show(0);
        }

        private void Define()
        {
            Add("Gameplay", "How the game looks and feels while you play.", () =>
            {
                Section("VIEW");
                Note(MenuKit.Range(body, "Field of view", GameSettings.MinFov, GameSettings.MaxFov, GameSettings.FieldOfView, "0",
                    v => GameSettings.FieldOfView = Mathf.Round(v)), "Wider shows more of the building; narrower feels closer.");
                Note(Toggle("Head bob", GameSettings.HeadBob, v => GameSettings.HeadBob = v), "The camera sways as you walk.");
                Note(Toggle("Camera shake", GameSettings.CameraShake, v => GameSettings.CameraShake = v), "Collapses, landings and big hits shake the view.");
                Note(Toggle("Crosshair", GameSettings.ShowCrosshair, v => GameSettings.ShowCrosshair = v), "Off hides the dot; the ring still shows on things you can use.");
                Section("YOU");
                VisualElement nameRow = Row("Your name");
                var nameField = new TextField { value = GameSettings.PlayerName, maxLength = Networking.NetworkPlayer.MaxNameLength };
                nameField.AddToClassList("field");
                nameField.RegisterValueChangedCallback(e => GameSettings.PlayerName = e.newValue);
                nameRow.Add(nameField);
                Note(nameRow, "What the crew sees when you play without Steam (Steam uses your Steam name). Applies from the next game.");
                Section("INTERFACE");
                // Applied when the slider is let go: resizing the whole UI under the pointer mid-drag is unusable.
                Slider scale = MenuKit.Range(body, "UI scale", GameSettings.MinUiScale, GameSettings.MaxUiScale, GameSettings.UiScale, "0.00x", _ => { });
                scale.RegisterCallback<PointerCaptureOutEvent>(_ => GameSettings.UiScale = Mathf.Round(scale.value * 20f) / 20f);
                Note(scale, "Menus and HUD size. Applies when you let go of the slider.");
                Note(Toggle("Tips", Hints.Enabled, v => Hints.Enabled = v), "Short first-time tips during play.");
                VisualElement again = Row("Seen tips");
                MenuKit.Button(again, "Show them again", Hints.ResetSeen, small: true);
            }, () =>
            {
                GameSettings.FieldOfView = GameSettings.DefaultFov;
                GameSettings.HeadBob = GameSettings.CameraShake = GameSettings.ShowCrosshair = true;
                GameSettings.UiScale = 1f;
                Hints.Enabled = true;
            });

            Add("Controls", "Mouse and movement. Every key can be rebound.", () =>
            {
                Section("MOUSE");
                Note(MenuKit.Range(body, "Mouse sensitivity", GameSettings.MinSensitivity, GameSettings.MaxSensitivity, GameSettings.Sensitivity, "0.00x",
                    v => GameSettings.Sensitivity = v), "How far the view turns for a given mouse movement.");
                Note(Toggle("Invert vertical look", GameSettings.InvertY, v => GameSettings.InvertY = v), "Push the mouse forward to look down.");
                Section("MOVEMENT");
                MenuKit.Switch(body, "Crouch", GameSettings.CrouchToggle, "Toggle", "Hold", v => GameSettings.CrouchToggle = v);
                NoteLast("Toggle: press once to crouch, again to stand.");
                Section("KEYS");
                VisualElement keys = Row("Key bindings");
                MenuKit.Button(keys, "Change keys", () => menu.Push(MenuScreen.Controls), small: true);
            }, () =>
            {
                GameSettings.Sensitivity = 1f;
                GameSettings.InvertY = GameSettings.CrouchToggle = false;
            });

            Add("Video", "The window and how often it's drawn.", () =>
            {
                Section("DISPLAY");
                var modes = new[] { (FullScreenMode.FullScreenWindow, "Borderless fullscreen"), (FullScreenMode.ExclusiveFullScreen, "Fullscreen"), (FullScreenMode.Windowed, "Windowed") };
                MenuKit.Choice(body, "Window mode", modes.Select(m => m.Item2).ToArray(), Array.FindIndex(modes, m => m.Item1 == VideoSettings.WindowMode),
                    i => VideoSettings.WindowMode = modes[i].Item1);
                List<Vector2Int> sizes = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().OrderByDescending(r => r.x * r.y).ToList();
                sizes.Insert(0, Vector2Int.zero);
                string[] names = sizes.Select(r => r == Vector2Int.zero ? "Desktop" : $"{r.x} x {r.y}").ToArray();
                MenuKit.Choice(body, "Resolution", names, Mathf.Max(0, sizes.IndexOf(VideoSettings.Resolution)), i => VideoSettings.Resolution = sizes[i]);
                NoteLast("Window and resolution apply to the built game; the editor's Game view keeps its own size.");
                Section("FRAME RATE");
                Note(Toggle("VSync", VideoSettings.VSync, v => VideoSettings.VSync = v), "Matches your monitor's refresh: no tearing, a little more input delay.");
                MenuKit.Choice(body, "Frame cap", VideoSettings.FrameCaps.Select(c => c == 0 ? "Unlimited" : $"{c} fps").ToArray(),
                    Mathf.Max(0, Array.IndexOf(VideoSettings.FrameCaps, VideoSettings.FrameCap)), i => VideoSettings.FrameCap = VideoSettings.FrameCaps[i]);
                NoteLast("Only used while VSync is off.");
                Section("PICTURE");
                Note(MenuKit.Range(body, "Brightness", -1f, 1.5f, VideoSettings.Brightness, "+0.0;-0.0;0", v => VideoSettings.Brightness = v),
                    "Raise it if Power Off jobs are too dark to read on your screen.");
            }, () =>
            {
                VideoSettings.WindowMode = FullScreenMode.FullScreenWindow;
                VideoSettings.Resolution = Vector2Int.zero;
                VideoSettings.VSync = true;
                VideoSettings.FrameCap = 0;
                VideoSettings.Brightness = 0f;
            });

            Add("Graphics", "Detail against speed. Lower these first on a slow machine.", () =>
            {
                Section("QUALITY");
                string[] quality = QualitySettings.names;
                if (quality.Length > 1)
                    MenuKit.Choice(body, "Preset", quality, VideoSettings.Quality >= 0 ? VideoSettings.Quality : QualitySettings.GetQualityLevel(), i => VideoSettings.Quality = i);
                MenuKit.Choice(body, "Shadows", VideoSettings.ShadowNames, VideoSettings.Shadows, i => VideoSettings.Shadows = i);
                NoteLast("How far flashlight and ceiling-light shadows reach.");
                Note(MenuKit.Percent(body, "Render scale", Mathf.InverseLerp(VideoSettings.MinRenderScale, 1f, VideoSettings.RenderScale),
                    v => VideoSettings.RenderScale = Mathf.Lerp(VideoSettings.MinRenderScale, 1f, v)), "Draws the 3D view smaller and stretches it (0% = half resolution). Menus stay sharp.");
                MenuKit.Choice(body, "Anti-aliasing", VideoSettings.AntiAliasingNames, VideoSettings.AntiAliasing, i => VideoSettings.AntiAliasing = i);
                NoteLast("Smooths jagged edges. 4x costs the most.");
            }, () =>
            {
                VideoSettings.Quality = -1;
                VideoSettings.Shadows = VideoSettings.ShadowNames.Length - 1;
                VideoSettings.RenderScale = 1f;
                VideoSettings.AntiAliasing = 1;
            });

            Add("Audio", "Volume of everything except voice chat.", () =>
            {
                Section("VOLUME");
                MenuKit.Percent(body, "Master", AudioLevels.Master, v => AudioLevels.Master = v);
                MenuKit.Percent(body, "Effects", AudioLevels.Sfx, v => AudioLevels.Sfx = v);
                NoteLast("Footsteps, loot, creaks, the monsters.");
                MenuKit.Percent(body, "Ambience", AudioLevels.Ambience, v => AudioLevels.Ambience = v);
                MenuKit.Percent(body, "Music", AudioLevels.Music, v => AudioLevels.Music = v);
                Section("WINDOW");
                Note(Toggle("Mute when in the background", GameSettings.MuteWhenUnfocused, v => GameSettings.MuteWhenUnfocused = v),
                    "Silences the game while you're in another window.");
            }, () =>
            {
                AudioLevels.Master = AudioLevels.Sfx = AudioLevels.Ambience = 1f;
                AudioLevels.Music = 0.7f;
                GameSettings.MuteWhenUnfocused = false;
            });

            Add("Voice", "Proximity voice chat and your microphone.", Voice, () =>
            {
                VoiceSettings.Volume = 1f;
                VoiceSettings.MicDevice = "";
                VoiceSettings.Mode = VoiceMode.PushToTalk;
                VoiceSettings.MicMuted = false;
            });

            Add("Accessibility", "Captions, colours and calmer effects.", () =>
            {
                Section("CAPTIONS");
                Note(Toggle("Subtitles", GameSettings.Subtitles, v => GameSettings.Subtitles = v), "Captions for structural warnings and threat sounds.");
                MenuKit.Choice(body, "Subtitle size", GameSettings.SubtitleSizeNames, GameSettings.SubtitleSize, i => GameSettings.SubtitleSize = i);
                Section("COLOUR AND MOTION");
                Note(Toggle("Colourblind-safe scanner", GameSettings.ColorblindScanner, v => GameSettings.ColorblindScanner = v), "Blue / orange / vermilion stress colours.");
                Note(Toggle("Reduce flashing lights", GameSettings.ReduceFlashing, v => GameSettings.ReduceFlashing = v), "Lights in the game dim and recover smoothly instead of flickering or strobing.");
                Note(Toggle("Reduce menu effects", GameSettings.ReduceMenuEffects, v => GameSettings.ReduceMenuEffects = v), "No flicker, glitches, jitter or static in the menus.");
                Note(Toggle("Fewer jump scares", GameSettings.FewerJumpScares, v => GameSettings.FewerJumpScares = v), "The building stops showing you sudden figures. Sounds and monsters stay.");
                Note(Toggle("Camera shake", GameSettings.CameraShake, v => GameSettings.CameraShake = v), "Also on the Gameplay page.");
                Note(Toggle("Head bob", GameSettings.HeadBob, v => GameSettings.HeadBob = v), "Also on the Gameplay page.");
                Section("SOUND");
                Note(MenuKit.Percent(body, "Heartbeat and breathing", GameSettings.VitalsVolume, v => GameSettings.VitalsVolume = v), "Your own heartbeat and breath during a job. 0 turns them off.");
            }, () =>
            {
                GameSettings.Subtitles = true;
                GameSettings.VitalsVolume = 1f;
                GameSettings.ColorblindScanner = GameSettings.ReduceMenuEffects = GameSettings.ReduceFlashing = GameSettings.FewerJumpScares = false;
                GameSettings.SubtitleSize = 1;
            });
        }

        private void Voice()
        {
            Section("HEARING");
            Note(MenuKit.Percent(body, "Voices", VoiceSettings.Volume, v => VoiceSettings.Volume = v), "Everyone's voice; per-player volume is in the pause menu's crew list.");
            Section("MICROPHONE");
            var options = new List<string> { "System default" };
            options.AddRange(Microphone.devices);
            MenuKit.Choice(body, "Input device", options.ToArray(), Mathf.Max(0, options.IndexOf(VoiceSettings.MicDevice)),
                i => VoiceSettings.MicDevice = i == 0 ? "" : options[i]);
            NoteLast("Steam voice always uses the system default; this applies from the next game.");
            MenuKit.Switch(body, "Talk", VoiceSettings.Mode == VoiceMode.PushToTalk, $"Push to talk ({InputBindings.Display("PushToTalk")})", "Open mic",
                ptt => VoiceSettings.Mode = ptt ? VoiceMode.PushToTalk : VoiceMode.OpenMic);
            Note(Toggle("Mute my microphone", VoiceSettings.MicMuted, m => VoiceSettings.MicMuted = m), "Nobody hears you, in any mode.");
            VisualElement meter = Row("Mic level");
            micBar = UiKit.Bar(meter, "mic-meter");
            Label status = MenuKit.Text(body, "", "settings__note");
            VisualElement bar = micBar;
            bar.schedule.Execute(() =>
            {
                VoiceTransmitter me = NetworkPlayer.Local != null ? NetworkPlayer.Local.GetComponent<VoiceTransmitter>() : null;
                UiKit.SetBar(bar, me != null ? Mathf.Clamp01(me.Level * 4f) : 0f, me != null && me.Transmitting ? "good" : null);
                status.text = me == null ? "Join or host a game to see your level."
                    : me.Transmitting ? "Sending: your crew hears you." : $"Not sending. Hold {InputBindings.Display("PushToTalk")} to talk, or switch to open mic.";
            }).Every(60);
        }

        private void Add(string name, string blurb, Action build, Action reset) =>
            pages.Add(new Page { Name = name.ToUpperInvariant(), Blurb = blurb, Build = build, Reset = reset });

        private void Show(int index)
        {
            open = index;
            for (int i = 0; i < nav.Count; i++) nav[i].EnableInClassList("settings__nav-button--open", i == open);
            pageTitle.text = pages[open].Name;
            pageBlurb.text = pages[open].Blurb;
            body.Clear();
            micBar = null;
            pages[open].Build();
            body.scrollOffset = Vector2.zero;
        }

        private void Section(string title) => MenuKit.Text(body, title, "section");

        private VisualElement Toggle(string label, bool value, Action<bool> commit)
        {
            MenuKit.Toggle(body, label, value, commit);
            return body.contentContainer[body.contentContainer.childCount - 1];
        }

        private VisualElement Row(string label)
        {
            VisualElement row = MenuKit.Row(body);
            row.AddToClassList("setting");
            MenuKit.Text(row, label, "setting__label");
            return row;
        }

        // A short explanation under the option it belongs to.
        private void Note(VisualElement _, string text) => NoteLast(text);

        private void NoteLast(string text) => MenuKit.Text(body, text, "settings__note");
    }
}
