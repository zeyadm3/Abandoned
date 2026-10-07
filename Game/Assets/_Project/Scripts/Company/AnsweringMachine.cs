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
            ? k != heard ? "Play the boss's message (1 new)" : "Play the boss's message again" : null;

        public void Use(GameObject user)
        {
            if (!Current(out key, out text)) return;
            heard = key;
            open = true;
            GameAudio.PlayUi(SoundId.UiOpen);
            GameAudio.Play(SoundId.RadioStatic, transform.position, 0.4f);
        }

        private void Update()
        {
            if (lamp != null) lamp.enabled = HasNew && Mathf.Repeat(Time.time, 1f) < 0.5f;
            if (open && (CompanyService.Current == null || UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame))
                open = false;
            CursorOwner.Set(this, open);
            if (screen == null)
            {
                if (!open || (screen = ScreenPanel.Create(wide: false)) == null) return;
            }
            screen.Show(open);
            if (!open) return;
            screen.Build(key, panel =>
            {
                MenuKit.Text(panel, "VOICEMAIL", "heading");
                MenuKit.Text(panel, "1 message - from: THE BOSS", "subtitle");
                MenuKit.Text(panel, $"\"{text}\"");
                MenuKit.Text(panel, "<i>*beep*</i>", "text").AddToClassList("text--small");
                MenuKit.Button(panel, "Hang up", () => open = false, SoundId.UiBack);
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
