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

            MenuKit.Text(panel, Loc.T("PERSONAL RECORD / FIELD CONFIGURATION"), "menu-record");
            MenuKit.Text(panel, Loc.T("SETTINGS"), "heading");
            MenuKit.Text(panel, Loc.T("Changes apply straight away and are saved when you leave this screen."), "subtitle");
            var columns = new VisualElement();
            columns.AddToClassList("settings__columns");
            panel.Add(columns);

            ScrollView list = MenuKit.Scroll(columns, "settings__nav");
            list.style.flexGrow = 0;
            list.style.flexShrink = 0;
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
            footer.style.flexWrap = Wrap.Wrap;
            MenuKit.Button(footer, Loc.T("Back"), () =>
            {
                VoiceSettings.Flush();
                Prefs.Save();
                menu.Back();
            }, SoundId.UiBack).AddToClassList("settings__back");
            MenuKit.Button(footer, Loc.T("Restore this page's defaults"), () =>
            {
                pages[open].Reset();
                Show(open);
            }, SoundId.UiBack, small: true).AddToClassList("settings__reset");
            Show(0);
        }

        private void Define()
        {
            Add(Loc.T("Gameplay"), Loc.T("How the game looks and feels while you play."), () =>
            {
                // 0.24.8. First on the first page and each name in its own script, so it can be found in a
                // language the player can't read; the menus are rebuilt on a pick and open here again.
                Section(Loc.T("LANGUAGE"));
                VisualElement languages = MenuKit.Row(body);
                languages.AddToClassList("language-list");
                foreach (LanguageInfo language in Languages.Offered)
                {
                    LanguageInfo pick = language;
                    Button button = MenuKit.Button(languages, language.NativeName, () => Languages.Choose(pick.Code), small: true);
                    button.AddToClassList("language-list__button");
                    button.EnableInClassList("language-list__button--current", language == Languages.Current);
                }
                NoteLast(Loc.T("Menus, the HUD and captions. Spoken lines stay in English. Every player picks their own."));
                Section(Loc.T("VIEW"));
                Note(MenuKit.Range(body, Loc.T("Field of view"), GameSettings.MinFov, GameSettings.MaxFov, GameSettings.FieldOfView, "0",
                    v => GameSettings.FieldOfView = Mathf.Round(v)), Loc.T("Wider shows more of the building; narrower feels closer."));
                Note(Toggle(Loc.T("Head bob"), GameSettings.HeadBob, v => GameSettings.HeadBob = v), Loc.T("The camera sways as you walk."));
                Note(Toggle(Loc.T("Camera shake"), GameSettings.CameraShake, v => GameSettings.CameraShake = v), Loc.T("Collapses, landings and big hits shake the view."));
                Note(Toggle(Loc.T("Crosshair"), GameSettings.ShowCrosshair, v => GameSettings.ShowCrosshair = v), Loc.T("Off hides the dot; the ring still shows on things you can use."));
                Section(Loc.T("YOU"));
                VisualElement nameRow = Row(Loc.T("Your name"));
                var nameField = new TextField { value = GameSettings.PlayerName, maxLength = Networking.NetworkPlayer.MaxNameLength };
                nameField.AddToClassList("field");
                nameField.AddToClassList("setting__input");
                nameField.RegisterValueChangedCallback(e => GameSettings.PlayerName = e.newValue);
                nameRow.Add(nameField);
                Note(nameRow, Loc.T("What the crew sees when you play without Steam (Steam uses your Steam name). Applies from the next game."));
                Section(Loc.T("INTERFACE"));
                // Applied when the slider is let go: resizing the whole UI under the pointer mid-drag is unusable.
                Slider scale = MenuKit.Range(body, Loc.T("UI scale"), GameSettings.MinUiScale, GameSettings.MaxUiScale, GameSettings.UiScale, "0.00x", _ => { });
                scale.RegisterCallback<PointerCaptureOutEvent>(_ => GameSettings.UiScale = Mathf.Round(scale.value * 20f) / 20f);
                Note(scale, Loc.T("Menus and HUD size. Applies when you let go of the slider."));
                Note(Toggle(Loc.T("Tips"), Hints.Enabled, v => Hints.Enabled = v), Loc.T("The guided first job and short first-time tips during play."));
                VisualElement again = Row(Loc.T("Seen tips"));
                MenuKit.Button(again, Loc.T("Show them again"), Hints.ResetSeen, small: true);
            }, () =>
            {
                GameSettings.FieldOfView = GameSettings.DefaultFov;
                GameSettings.HeadBob = GameSettings.CameraShake = GameSettings.ShowCrosshair = true;
                GameSettings.UiScale = GameSettings.DefaultUiScale;
                Hints.Enabled = true;
            });

            Add(Loc.T("Controls"), Loc.T("Mouse and movement. Every key can be rebound."), () =>
            {
                Section(Loc.T("MOUSE"));
                Note(MenuKit.Range(body, Loc.T("Mouse sensitivity"), GameSettings.MinSensitivity, GameSettings.MaxSensitivity, GameSettings.Sensitivity, "0.00x",
                    v => GameSettings.Sensitivity = v), Loc.T("How far the view turns for a given mouse movement."));
                Note(Toggle(Loc.T("Invert vertical look"), GameSettings.InvertY, v => GameSettings.InvertY = v), Loc.T("Push the mouse forward to look down."));
                Section(Loc.T("MOVEMENT"));
                MenuKit.Switch(body, Loc.T("Crouch"), GameSettings.CrouchToggle, Loc.T("Toggle"), Loc.T("Hold"), v => GameSettings.CrouchToggle = v);
                NoteLast(Loc.T("Toggle: press once to crouch, again to stand."));
                Section(Loc.T("KEYS"));
                VisualElement keys = Row(Loc.T("Key bindings"));
                MenuKit.Button(keys, Loc.T("Change keys"), () => menu.Push(MenuScreen.Controls), small: true);
            }, () =>
            {
                GameSettings.Sensitivity = 1f;
                GameSettings.InvertY = GameSettings.CrouchToggle = false;
            });

            Add(Loc.T("Video"), Loc.T("The window and how often it's drawn."), () =>
            {
                Section(Loc.T("DISPLAY"));
                var modes = new[] { (FullScreenMode.FullScreenWindow, Loc.T("Borderless fullscreen")), (FullScreenMode.ExclusiveFullScreen, Loc.T("Fullscreen")), (FullScreenMode.Windowed, Loc.T("Windowed")) };
                MenuKit.Choice(body, Loc.T("Window mode"), modes.Select(m => m.Item2).ToArray(), Array.FindIndex(modes, m => m.Item1 == VideoSettings.WindowMode),
                    i => VideoSettings.WindowMode = modes[i].Item1);
                List<Vector2Int> sizes = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().OrderByDescending(r => r.x * r.y).ToList();
                sizes.Insert(0, Vector2Int.zero);
                string[] names = sizes.Select(r => r == Vector2Int.zero ? Loc.T("Desktop") : $"{r.x} x {r.y}").ToArray();
                MenuKit.Choice(body, Loc.T("Resolution"), names, Mathf.Max(0, sizes.IndexOf(VideoSettings.Resolution)), i => VideoSettings.Resolution = sizes[i]);
                NoteLast(Loc.T("Window and resolution apply to the built game; the editor's Game view keeps its own size."));
                // 0.23.4
                var displays = new List<DisplayInfo>();
                Screen.GetDisplayLayout(displays);
                if (displays.Count > 1)
                    MenuKit.Choice(body, Loc.T("Monitor"), displays.Select((d, i) => $"{i + 1}: {d.name}").ToArray(), Mathf.Clamp(VideoSettings.Monitor, 0, displays.Count - 1), i => VideoSettings.Monitor = i);
                Note(Toggle(Loc.T("Show FPS"), VideoSettings.ShowFps, v => VideoSettings.ShowFps = v), Loc.T("A small frame counter in the corner."));
                Section(Loc.T("FRAME RATE"));
                Note(Toggle(Loc.T("VSync"), VideoSettings.VSync, v => VideoSettings.VSync = v), Loc.T("Matches your monitor's refresh: no tearing, a little more input delay."));
                MenuKit.Choice(body, Loc.T("Frame cap"), VideoSettings.FrameCaps.Select(c => c == 0 ? Loc.T("Unlimited") : Loc.T("{0} fps", c)).ToArray(),
                    Mathf.Max(0, Array.IndexOf(VideoSettings.FrameCaps, VideoSettings.FrameCap)), i => VideoSettings.FrameCap = VideoSettings.FrameCaps[i]);
                NoteLast(Loc.T("Only used while VSync is off."));
                Section(Loc.T("PICTURE"));
                Note(MenuKit.Range(body, Loc.T("Brightness"), -1f, 1.5f, VideoSettings.Brightness, "+0.0;-0.0;0", v => VideoSettings.Brightness = v),
                    Loc.T("Raise it if Power Off jobs are too dark to read on your screen."));
            }, () =>
            {
                VideoSettings.WindowMode = FullScreenMode.FullScreenWindow;
                VideoSettings.Resolution = Vector2Int.zero;
                VideoSettings.VSync = true;
                VideoSettings.FrameCap = 0;
                VideoSettings.Brightness = 0f;
            });

            Add(Loc.T("Graphics"), Loc.T("Detail against speed. Lower these first on a slow machine."), () =>
            {
                Section(Loc.T("QUALITY"));
                string[] quality = QualitySettings.names; // the project's quality levels. @loc "Mobile", "PC"
                if (quality.Length > 1)
                    MenuKit.Choice(body, Loc.T("Preset"), Words(quality), VideoSettings.Quality >= 0 ? VideoSettings.Quality : QualitySettings.GetQualityLevel(), i => VideoSettings.Quality = i);
                MenuKit.Choice(body, Loc.T("Shadows"), Words(VideoSettings.ShadowNames, "quality"), VideoSettings.Shadows, i => VideoSettings.Shadows = i);
                MenuKit.Choice(body, Loc.T("Textures"), Words(VideoSettings.TextureNames), VideoSettings.TextureQuality, i => VideoSettings.TextureQuality = i);
                Section(Loc.T("EFFECTS"));
                Note(Toggle(Loc.T("Fog"), VideoSettings.Fog, v => VideoSettings.Fog = v), Loc.T("Off: see further. The building is meant to be murky."));
                Note(Toggle(Loc.T("Bloom"), VideoSettings.Bloom, v => VideoSettings.Bloom = v), Loc.T("The glow round bright lights."));
                Note(Toggle(Loc.T("Film grain"), VideoSettings.Grain, v => VideoSettings.Grain = v), Loc.T("Off also calms night vision's noise."));
                Note(Toggle(Loc.T("Post-processing"), VideoSettings.PostProcessing, v => VideoSettings.PostProcessing = v), Loc.T("Everything above, plus colour grading, brightness, and the low-health and night-vision looks. Off is fastest."));
                NoteLast(Loc.T("How far shadows reach, and how many of the lights around you cast them (2, 4 or 6)."));
                Note(MenuKit.Percent(body, Loc.T("Render scale"), Mathf.InverseLerp(VideoSettings.MinRenderScale, 1f, VideoSettings.RenderScale),
                    v => VideoSettings.RenderScale = Mathf.Lerp(VideoSettings.MinRenderScale, 1f, v)), Loc.T("Draws the 3D view smaller and stretches it (0% = half resolution). Menus stay sharp."));
                MenuKit.Choice(body, Loc.T("Anti-aliasing"), Words(VideoSettings.AntiAliasingNames, "quality"), VideoSettings.AntiAliasing, i => VideoSettings.AntiAliasing = i);
                NoteLast(Loc.T("Smooths jagged edges. 4x costs the most."));
            }, () =>
            {
                VideoSettings.Quality = -1;
                VideoSettings.Shadows = VideoSettings.ShadowNames.Length - 1;
                VideoSettings.RenderScale = 1f;
                VideoSettings.AntiAliasing = 1;
            });

            Add(Loc.T("Audio"), Loc.T("Volume of everything except voice chat."), () =>
            {
                Section(Loc.T("VOLUME"));
                MenuKit.Percent(body, Loc.T("Master"), AudioLevels.Master, v => AudioLevels.Master = v);
                MenuKit.Percent(body, Loc.T("Effects"), AudioLevels.Sfx, v => AudioLevels.Sfx = v);
                NoteLast(Loc.T("Footsteps, loot, creaks, the monsters."));
                MenuKit.Percent(body, Loc.T("Ambience"), AudioLevels.Ambience, v => AudioLevels.Ambience = v);
                MenuKit.Percent(body, Loc.T("Music"), AudioLevels.Music, v => AudioLevels.Music = v);
                Section(Loc.T("WINDOW"));
                Note(Toggle(Loc.T("Mute when in the background"), GameSettings.MuteWhenUnfocused, v => GameSettings.MuteWhenUnfocused = v),
                    Loc.T("Silences the game while you're in another window."));
            }, () =>
            {
                AudioLevels.Master = AudioLevels.Sfx = AudioLevels.Ambience = 1f;
                AudioLevels.Music = 0.7f;
                GameSettings.MuteWhenUnfocused = false;
            });

            Add(Loc.T("Voice"), Loc.T("Proximity voice chat and your microphone."), Voice, () =>
            {
                VoiceSettings.Volume = 1f;
                VoiceSettings.MicDevice = "";
                VoiceSettings.Mode = VoiceMode.PushToTalk;
                VoiceSettings.MicMuted = false;
            });

            Add(Loc.T("Controls+"), Loc.T("Look speed, hold or toggle, and the mouse wheel."), () =>
            {
                Section(Loc.T("LOOK"));
                Note(MenuKit.Percent(body, Loc.T("Horizontal look speed"), GameSettings.LookX / 2f, v => GameSettings.LookX = v * 2f), Loc.T("50 % is normal."));
                Note(MenuKit.Percent(body, Loc.T("Vertical look speed"), GameSettings.LookY / 2f, v => GameSettings.LookY = v * 2f), Loc.T("50 % is normal."));
                Note(MenuKit.Percent(body, Loc.T("Controller look speed"), GameSettings.PadLookSpeed / 1800f, v => GameSettings.PadLookSpeed = Mathf.Max(100f, v * 1800f)), Loc.T("The right stick at full tilt."));
                Section(Loc.T("HOLD OR TOGGLE"));
                Note(Toggle(Loc.T("Toggle sprint"), GameSettings.SprintToggle, v => GameSettings.SprintToggle = v), Loc.T("Press once to run, again to walk."));
                Note(Toggle(Loc.T("Toggle push to talk"), GameSettings.TalkToggle, v => GameSettings.TalkToggle = v), Loc.T("Press once to open your mic, again to close it."));
                Note(Toggle(Loc.T("Toggle radio"), GameSettings.RadioToggle, v => GameSettings.RadioToggle = v), Loc.T("Press once to key the radio, again to let go."));
                Section(Loc.T("HANDS"));
                Note(Toggle(Loc.T("Mouse wheel switches hand slots"), GameSettings.WheelSwitchesSlots, v => GameSettings.WheelSwitchesSlots = v), Loc.T("With empty hands. Hold Inventory and scroll to pick the pocket item Drop takes out."));
            }, () =>
            {
                GameSettings.LookX = GameSettings.LookY = 1f;
                GameSettings.PadLookSpeed = 900f;
                GameSettings.SprintToggle = GameSettings.TalkToggle = GameSettings.RadioToggle = false;
                GameSettings.WheelSwitchesSlots = true;
            });

            Add(Loc.T("Accessibility"), Loc.T("Captions, colours and calmer effects."), () =>
            {
                Section(Loc.T("CAPTIONS"));
                Note(Toggle(Loc.T("Subtitles"), GameSettings.Subtitles, v => GameSettings.Subtitles = v), Loc.T("Names warning and monster sounds, and who makes them (never where from)."));
                MenuKit.Choice(body, Loc.T("Subtitle size"), Words(GameSettings.SubtitleSizeNames, "size"), GameSettings.SubtitleSize, i => GameSettings.SubtitleSize = i);
                Section(Loc.T("COLOUR AND MOTION"));
                Note(Toggle(Loc.T("Colourblind-safe scanner"), GameSettings.ColorblindScanner, v => GameSettings.ColorblindScanner = v), Loc.T("Blue / orange / vermilion stress colours."));
                Note(Toggle(Loc.T("Reduce flashing lights"), GameSettings.ReduceFlashing, v => GameSettings.ReduceFlashing = v), Loc.T("Lights in the game dim and recover smoothly instead of flickering or strobing."));
                Note(Toggle(Loc.T("Reduce menu effects"), GameSettings.ReduceMenuEffects, v => GameSettings.ReduceMenuEffects = v), Loc.T("No flicker, glitches, jitter or static in the menus."));
                Note(Toggle(Loc.T("Fewer jump scares"), GameSettings.FewerJumpScares, v => GameSettings.FewerJumpScares = v), Loc.T("The building stops showing you sudden figures. Sounds and monsters stay."));
                Note(Toggle(Loc.T("Arachnophobia mode"), GameSettings.Arachnophobia, v => GameSettings.Arachnophobia = v), Loc.T("The Crawlers look like scurrying lumps of grit, with no legs. They behave the same."));
                Section(Loc.T("PRIVACY"));
                Note(Toggle(Loc.T("Streamer mode"), GameSettings.StreamerMode, v => GameSettings.StreamerMode = v), Loc.T("Crewmates show as \"Crewmate 1-4\" on your screen, and your address and port are hidden."));
                Note(MenuKit.Percent(body, Loc.T("Camera shake"), GameSettings.ShakeAmount, v => GameSettings.ShakeAmount = v), Loc.T("0 turns it off."));
                Note(MenuKit.Percent(body, Loc.T("Head bob and landing dip"), GameSettings.BobAmount, v => GameSettings.BobAmount = v), Loc.T("0 turns it off."));
                Note(Toggle(Loc.T("Low-health vision effects"), GameSettings.LowHealthVision, v => GameSettings.LowHealthVision = v), Loc.T("The closing vignette, blur and warp at low health. The heartbeat and the blood stay."));
                Section(Loc.T("SOUND"));
                Note(MenuKit.Percent(body, Loc.T("Heartbeat and breathing"), GameSettings.VitalsVolume, v => GameSettings.VitalsVolume = v), Loc.T("Your own heartbeat and breath during a job. 0 turns them off."));
            }, () =>
            {
                GameSettings.Subtitles = true;
                GameSettings.ShakeAmount = GameSettings.BobAmount = 1f;
                GameSettings.LowHealthVision = true;
                GameSettings.VitalsVolume = 1f;
                GameSettings.ColorblindScanner = GameSettings.ReduceMenuEffects = GameSettings.ReduceFlashing = GameSettings.FewerJumpScares = false;
                GameSettings.SubtitleSize = 1;
            });
        }

        private void Voice()
        {
            Section(Loc.T("HEARING"));
            Note(MenuKit.Percent(body, Loc.T("Voices"), VoiceSettings.Volume, v => VoiceSettings.Volume = v), Loc.T("Everyone's voice; per-player volume is in the pause menu's crew list."));
            Section(Loc.T("MICROPHONE"));
            // 0.23.3
            Note(MenuKit.Percent(body, Loc.T("Microphone gain"), VoiceSettings.MicGain / 3f, v => VoiceSettings.MicGain = v * 3f), Loc.T("How loud your mic counts for the open-mic gate (33 % is normal)."));
            Note(MenuKit.Percent(body, Loc.T("Open mic threshold"), VoiceSettings.OpenMicThreshold / 4f, v => VoiceSettings.OpenMicThreshold = v * 4f), Loc.T("Higher: you must speak louder before open mic sends (25 % is normal)."));
            Note(MenuKit.Percent(body, Loc.T("Radio volume"), VoiceSettings.RadioVolume, v => VoiceSettings.RadioVolume = v), Loc.T("The walkie-talkie, on top of the voice volume."));
            Note(Toggle(Loc.T("Voice chat off"), VoiceSettings.VoiceOff, v => VoiceSettings.VoiceOff = v), Loc.T("Send nothing and hear nobody. Per-crewmate volumes are in the pause menu and are remembered."));
            Note(Toggle(Loc.T("Text chat off"), VoiceSettings.TextChatOff, v => VoiceSettings.TextChatOff = v), Loc.T("The chat key does nothing and nobody's lines show."));
            var options = new List<string> { Loc.T("System default") };
            options.AddRange(Microphone.devices);
            MenuKit.Choice(body, Loc.T("Input device"), options.ToArray(), Mathf.Max(0, options.IndexOf(VoiceSettings.MicDevice)),
                i => VoiceSettings.MicDevice = i == 0 ? "" : options[i]);
            NoteLast(Loc.T("Steam voice always uses the system default; this applies from the next game."));
            MenuKit.Switch(body, Loc.T("Talk"), VoiceSettings.Mode == VoiceMode.PushToTalk, Loc.T("Push to talk ({0})", InputBindings.Display("PushToTalk")), Loc.T("Open mic"),
                ptt => VoiceSettings.Mode = ptt ? VoiceMode.PushToTalk : VoiceMode.OpenMic);
            Note(Toggle(Loc.T("Mute my microphone"), VoiceSettings.MicMuted, m => VoiceSettings.MicMuted = m), Loc.T("Nobody hears you, in any mode."));
            VisualElement meter = Row(Loc.T("Mic level"));
            micBar = UiKit.Bar(meter, "mic-meter");
            Label status = MenuKit.Text(body, "", "settings__note");
            VisualElement bar = micBar;
            bar.schedule.Execute(() =>
            {
                VoiceTransmitter me = NetworkPlayer.Local != null ? NetworkPlayer.Local.GetComponent<VoiceTransmitter>() : null;
                UiKit.SetBar(bar, me != null ? Mathf.Clamp01(me.Level * 4f) : 0f, me != null && me.Transmitting ? "good" : null);
                status.text = me == null ? Loc.T("Join or host a game to see your level.")
                    : me.Transmitting ? Loc.T("Sending: your crew hears you.") : Loc.T("Not sending. Hold {0} to talk, or switch to open mic.", InputBindings.Display("PushToTalk"));
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

        // Option names kept in English where they are defined (static lists), worded here. A context keeps
        // "Medium" the size apart from "Medium" the quality.
        private static string[] Words(string[] english, string context = null) =>
            english.Select(e => context != null ? Loc.C(context, e) : Loc.T(e)).ToArray();

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
