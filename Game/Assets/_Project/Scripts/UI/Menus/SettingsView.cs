using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Voice;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Settings (GDD 20): look and camera comfort, sound, voice, accessibility. Saved as they change;
    /// written to disk on Back. Key rebinding has its own screen.
    /// </summary>
    public class SettingsView
    {
        public VisualElement Root { get; }

        public SettingsView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            MenuKit.Text(panel, "SETTINGS", "heading");
            var scroll = new ScrollView();
            scroll.AddToClassList("scroll");
            panel.Add(scroll);

            MenuKit.Text(scroll, "CONTROLS AND CAMERA", "section");
            MenuKit.Range(scroll, "Mouse sensitivity", GameSettings.MinSensitivity, GameSettings.MaxSensitivity, GameSettings.Sensitivity, "0.00x",
                v => GameSettings.Sensitivity = v);
            MenuKit.Range(scroll, "Field of view", GameSettings.MinFov, GameSettings.MaxFov, GameSettings.FieldOfView, "0",
                v => GameSettings.FieldOfView = Mathf.Round(v));
            MenuKit.Toggle(scroll, "Head bob", GameSettings.HeadBob, v => GameSettings.HeadBob = v);
            MenuKit.Toggle(scroll, "Camera shake", GameSettings.CameraShake, v => GameSettings.CameraShake = v);
            MenuKit.Button(scroll, "Keys...", () => menu.Push(MenuScreen.Controls), small: true);

            MenuKit.Text(scroll, "SOUND", "section");
            MenuKit.Percent(scroll, "Master volume", AudioLevels.Master, v => AudioLevels.Master = v);
            MenuKit.Percent(scroll, "Effects", AudioLevels.Sfx, v => AudioLevels.Sfx = v);
            MenuKit.Percent(scroll, "Ambience", AudioLevels.Ambience, v => AudioLevels.Ambience = v);
            MenuKit.Percent(scroll, "Voices", VoiceSettings.Volume, v => VoiceSettings.Volume = v);

            MenuKit.Text(scroll, "VOICE", "section");
            MenuKit.Switch(scroll, "Microphone", VoiceSettings.Mode == VoiceMode.PushToTalk, $"Push to talk ({InputBindings.Display("PushToTalk")})", "Open mic",
                ptt => VoiceSettings.Mode = ptt ? VoiceMode.PushToTalk : VoiceMode.OpenMic);
            MenuKit.Toggle(scroll, "Mute my microphone", VoiceSettings.MicMuted, m => VoiceSettings.MicMuted = m);

            MenuKit.Text(scroll, "ACCESSIBILITY", "section");
            MenuKit.Toggle(scroll, "Subtitles (warnings and threats)", GameSettings.Subtitles, v => GameSettings.Subtitles = v);
            MenuKit.Toggle(scroll, "Colourblind-safe scanner", GameSettings.ColorblindScanner, v => GameSettings.ColorblindScanner = v);

            MenuKit.Button(panel, "Back", () =>
            {
                VoiceSettings.Flush();
                PlayerPrefs.Save();
                menu.Back();
            }, SoundId.UiBack);
        }
    }
}
