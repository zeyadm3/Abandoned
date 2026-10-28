using System;
using System.Collections;
using Abandoned.Core;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// Plays a hiring scene (a <see cref="DialogueDefinition"/>): cuts between the office's Cinemachine shots (shot 0 is
    /// the seated first-person view, the rest are set-ups the brain blends or cuts to), speaks each line with its voice
    /// clip and subtitle, and offers the player's replies. A click moves on; holding Space (or the pad's Y) skips the
    /// rest of the scene, the replies taking their first answer. Local and offline.
    /// </summary>
    public class CutsceneDirector : MonoBehaviour
    {
        private const float SkipHold = 1.2f;

        [SerializeField] private CinemachineCamera[] shots;
        [SerializeField] private SeatedLook seat;
        [SerializeField] private AudioSource voice;

        private DialogueView view;
        private bool skipping, choosing;
        private float held;

        public bool Playing { get; private set; }

        public void Play(DialogueDefinition scene, Action done)
        {
            if (scene == null)
            {
                done?.Invoke();
                return;
            }
            StopAllCoroutines();
            StartCoroutine(Run(scene, done));
        }

        private IEnumerator Run(DialogueDefinition scene, Action done)
        {
            Playing = true;
            skipping = false;
            held = 0f;
            view ??= new DialogueView();
            view.Show(true);
            Debug.Log($"[Onboarding] Playing {scene.name}: {scene.Lines.Count} lines, subtitles {(view.Attached ? "on screen" : "with nowhere to draw")}.");
            foreach (DialogueLine line in scene.Lines)
            {
                if (skipping) break;
                if (!Holds(line.condition)) continue;
                Cut(line.shot);
                yield return Say(Loc.T(scene.Speaker), Loc.T(scene.SpeakerRole), line.text, line.hold);
                if (line.replies == null || line.replies.Count == 0 || skipping) continue;
                int pick = -1;
                choosing = true;
                if (seat != null) seat.Frozen = true;
                CursorOwner.Set(this, true);
                var options = new string[line.replies.Count];
                for (int i = 0; i < options.Length; i++) options[i] = Loc.T(line.replies[i].text);
                view.Replies(options, i => pick = i);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                while (pick < 0 && !skipping)
                {
                    pick = NumberKey(options.Length);
                    yield return null;
                }
                view.Replies(null, null);
                choosing = false;
                CursorOwner.Set(this, false);
                if (seat != null) seat.Frozen = false;
                if (pick < 0) break;
                view.Say(Loc.T("You"), "", options[pick]);
                yield return Wait(1.1f);
                yield return Say(Loc.T(scene.Speaker), Loc.T(scene.SpeakerRole), line.replies[pick].answer, .4f);
            }
            if (voice != null) voice.Stop();
            view.Show(false);
            Playing = false;
            done?.Invoke();
        }

        private IEnumerator Say(string speaker, string role, string text, float hold)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            view.Say(speaker, role, Loc.T(text));
            AudioClip clip = DialogueVoice.ClipFor(text);
            if (clip == null) Debug.LogWarning($"[Onboarding] No voice clip for \"{text}\".");
            float length = clip != null ? clip.length : 1.2f + text.Length * .05f;
            if (clip != null && voice != null)
            {
                voice.clip = clip;
                voice.Play();
            }
            yield return Wait(length + hold);
            if (voice != null) voice.Stop();
        }

        private IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            yield return null;
            while (Time.time < end && !skipping && !Advanced()) yield return null;
        }

        private void Update()
        {
            if (!Playing) return;
            bool hold = (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) ||
                        (Gamepad.current != null && Gamepad.current.buttonNorth.isPressed);
            held = hold ? held + Time.deltaTime : 0f;
            view?.SkipProgress(held / SkipHold);
            if (held >= SkipHold) skipping = true;
            // Between replies the seat looks around with the mouse, so the cursor stays with the view.
            if (!choosing && Cursor.lockState != CursorLockMode.Locked && !CursorOwner.UiActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private static int NumberKey(int count)
        {
            if (Keyboard.current == null) return -1;
            Key[] keys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
            for (int i = 0; i < count && i < keys.Length; i++)
                if (Keyboard.current[keys[i]].wasPressedThisFrame) return i;
            return -1;
        }

        private bool Advanced()
        {
            if (choosing) return false;
            return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                   (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) ||
                   (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        }

        private void Cut(int shot)
        {
            if (shot < 0 || shots == null || shot >= shots.Length) return;
            for (int i = 0; i < shots.Length; i++)
                if (shots[i] != null) shots[i].Priority = i == shot ? 20 : 0;
            if (seat != null) seat.enabled = shot == 0;
        }

        private static bool Holds(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return true;
            float minutes = OnboardingProgress.TrainingMinutes;
            return condition switch
            {
                "quick" => minutes > 0f && minutes < 12f,
                "slow" => minutes >= 25f,
                "steady" => minutes <= 0f || (minutes >= 12f && minutes < 25f),
                _ => true,
            };
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            view?.Show(false);
        }

#if UNITY_EDITOR
        public void EditorSetup(CinemachineCamera[] cameras, SeatedLook look, AudioSource speaker)
        {
            shots = cameras;
            seat = look;
            voice = speaker;
        }
#endif
    }
}
