using Abandoned.Audio;
using Abandoned.Voice;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>Settings (GDD 20): sound and voice. Saved as they change; written to disk on Back.</summary>
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

            MenuKit.Text(scroll, "SOUND", "section");
            MenuKit.Percent(scroll, "Master volume", AudioLevels.Master, v => AudioLevels.Master = v);
            MenuKit.Percent(scroll, "Effects", AudioLevels.Sfx, v => AudioLevels.Sfx = v);
            MenuKit.Percent(scroll, "Ambience", AudioLevels.Ambience, v => AudioLevels.Ambience = v);
            MenuKit.Percent(scroll, "Voices", VoiceSettings.Volume, v => VoiceSettings.Volume = v);

            MenuKit.Text(scroll, "VOICE", "section");
            MenuKit.Switch(scroll, "Microphone", VoiceSettings.Mode == VoiceMode.PushToTalk, "Push to talk (V)", "Open mic",
                ptt => VoiceSettings.Mode = ptt ? VoiceMode.PushToTalk : VoiceMode.OpenMic);
            MenuKit.Toggle(scroll, "Mute my microphone", VoiceSettings.MicMuted, m => VoiceSettings.MicMuted = m);

            MenuKit.Button(panel, "Back", () =>
            {
                VoiceSettings.Flush();
                PlayerPrefs.Save();
                menu.Back();
            }, SoundId.UiBack);
        }
    }
}
