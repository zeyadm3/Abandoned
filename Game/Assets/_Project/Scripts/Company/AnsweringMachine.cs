using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The HQ's answering machine (GDD 4): the boss leaves a voicemail about the last job. Its light
    /// blinks while there's one this machine hasn't played; E plays it (a panel with the transcript). The
    /// message is picked from the replicated company state, so everyone hears the same one. Local only.
    /// </summary>
    public class AnsweringMachine : MonoBehaviour, IUsable
    {
        [OptionalReference, SerializeField] private Renderer lamp;

        // Per session (not saved): a restart replays the latest message, which is fine for a voicemail.
        private static string heard;
        private ScreenPanel screen;
        private bool open;
        private string key, text;

        public bool HasNew => Current(out string k, out _) && k != heard;

        private bool Current(out string k, out string t)
        {
            k = t = null;
            CompanyService company = CompanyService.Current;
            if (company == null || !company.IsSpawned || company.Messages == null) return false;
            (k, t) = company.Messages.Voicemail(company.State, company.LastOutcome);
            return !string.IsNullOrEmpty(t);
        }

        public string UsePrompt(GameObject user) => Current(out string k, out _)
            ? k != heard ? Loc.T("Play the boss's message (1 new)") : Loc.T("Play the boss's message again") : null;

        public void Use(GameObject user)
        {
            if (!Current(out key, out text)) return;
            heard = key;
            open = true;
            GameAudio.Play(SoundId.VoicemailBeep, transform.position, 1f);
            GameAudio.Play(SoundId.RadioStatic, transform.position, 0.4f);
            Speak(text);
        }

        // 0.24.2: the boss's own voice on the tape (Art/Audio/Voicemail, named by a hash of the line's text).
        private AudioSource tapeVoice;

        private void Speak(string line)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(line));
            string name = "vm_" + System.BitConverter.ToString(hash, 0, 4).Replace("-", "").ToLowerInvariant();
            AudioClip clip = Resources.Load<AudioClip>("Voicemail/" + name);
            if (clip == null) return; // a line edited since the recordings were made: the text still shows
            if (tapeVoice == null)
            {
                tapeVoice = gameObject.AddComponent<AudioSource>();
                tapeVoice.playOnAwake = false;
                tapeVoice.spatialBlend = 0.6f;
                tapeVoice.minDistance = 1.5f;
                tapeVoice.maxDistance = 14f;
            }
            tapeVoice.clip = clip;
            tapeVoice.volume = Audio.AudioMix.Gain(Audio.AudioBus.Effects);
            tapeVoice.PlayDelayed(0.7f);
        }

        private void Update()
        {
            if (lamp != null) lamp.enabled = HasNew && Mathf.Repeat(Time.time, 1f) < 0.5f;
            if (open && (CompanyService.Current == null || Core.PadInput.BackPressed))
                open = false;
            if (!open && tapeVoice != null && tapeVoice.isPlaying) tapeVoice.Stop();
            CursorOwner.Set(this, open);
            if (screen == null)
            {
                if (!open || (screen = ScreenPanel.Create(wide: false)) == null) return;
            }
            screen.Show(open);
            if (!open) return;
            screen.Build(key, panel =>
            {
                MenuKit.Text(panel, Loc.T("VOICEMAIL"), "heading");
                MenuKit.Text(panel, Loc.T("1 message - from: THE BOSS"), "subtitle");
                MenuKit.Text(panel, $"\"{Loc.T(text)}\"");
                MenuKit.Text(panel, "<i>*beep*</i>", "text").AddToClassList("text--small");
                MenuKit.Button(panel, Loc.T("Hang up"), () => open = false, SoundId.UiBack);
            });
        }

        private void OnDisable()
        {
            open = false;
            CursorOwner.Set(this, false);
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();

#if UNITY_EDITOR
        public void EditorSetup(Renderer blinkingLamp) => lamp = blinkingLamp;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => heard = null;
    }
}
