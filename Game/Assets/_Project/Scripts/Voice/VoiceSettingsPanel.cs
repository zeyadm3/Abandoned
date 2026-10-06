using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Placeholder voice settings (OnGUI, until the settings menu): push-to-talk or open mic, others'
    /// volume, mute. Shown only while the cursor is free (Esc / menu), under the network panel.
    /// </summary>
    public class VoiceSettingsPanel : MonoBehaviour
    {
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Rect area = new(10f, 380f, 330f, 130f);

        private GUIStyle box;

        private void OnGUI()
        {
            if (Cursor.lockState == CursorLockMode.Locked) return;
            box ??= new GUIStyle(GUI.skin.box) { fontSize = fontSize, alignment = TextAnchor.UpperLeft };
            GUILayout.BeginArea(area, box);
            GUILayout.Label("<b>VOICE</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = fontSize });

            GUILayout.BeginHorizontal();
            bool ptt = VoiceSettings.Mode == VoiceMode.PushToTalk;
            if (GUILayout.Toggle(ptt, "Push to talk (V)", GUI.skin.button) && !ptt) VoiceSettings.Mode = VoiceMode.PushToTalk;
            if (GUILayout.Toggle(!ptt, "Open mic", GUI.skin.button) && ptt) VoiceSettings.Mode = VoiceMode.OpenMic;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Voices {VoiceSettings.Volume * 100f:0}%", GUILayout.Width(95f));
            float volume = GUILayout.HorizontalSlider(VoiceSettings.Volume, 0f, 1f);
            if (!Mathf.Approximately(volume, VoiceSettings.Volume)) VoiceSettings.Volume = volume;
            GUILayout.EndHorizontal();

            bool muted = GUILayout.Toggle(VoiceSettings.MicMuted, " Mute my microphone");
            if (muted != VoiceSettings.MicMuted) VoiceSettings.MicMuted = muted;
            GUILayout.EndArea();
        }
    }
}
