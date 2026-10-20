using System.Collections.Generic;
using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The training's voices on this machine: the dispatcher's radio line when a lesson starts, then the trainer
    /// reading the lesson out. Recordings are found by a hash of their line's English text (made by
    /// Tools/Audio/generate_tutorial_dispatch.py and generate_tutorial_narration.py, the way the boss's voicemails
    /// are), so a line edited since the recordings were made simply goes unspoken; the screen still has it.
    /// A new lesson cuts off whatever was still being said. The player can turn the voices off (pause menu).
    /// </summary>
    public sealed class TutorialRadioVoice
    {
        public const string Folder = "TutorialDispatch", NarrationFolder = "TutorialNarration";
        private const float Gap = 0.35f;

        private readonly Queue<AudioClip> queue = new();
        private AudioSource source;
        private float nextAt;

        /// <summary>Something is being said or is about to be.</summary>
        public bool Speaking => queue.Count > 0 || source != null && source.isPlaying;

        /// <summary>Says a lesson's radio line (if any) and then its spoken instruction, after <paramref name="delay"/> seconds.</summary>
        public void Speak(GameObject owner, string radioLine, string voiceLine, float delay = 0.25f)
        {
            Stop();
            if (owner == null || !TutorialSettings.Voice) return;
            Enqueue(Folder, ClipName(radioLine), radioLine);
            Enqueue(NarrationFolder, NarrationClipName(voiceLine), voiceLine);
            if (queue.Count == 0) return;
            if (source == null)
            {
                source = owner.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // the radio on your own shoulder
            }
            nextAt = Time.unscaledTime + Mathf.Max(0f, delay);
        }

        /// <summary>Every frame: the next line once the last has finished.</summary>
        public void Tick()
        {
            if (source == null) return;
            if (!TutorialSettings.Voice)
            {
                Stop(); // switched off mid-sentence in the pause menu
                return;
            }
            if (queue.Count == 0 || source.isPlaying || Time.unscaledTime < nextAt) return;
            AudioClip clip = queue.Dequeue();
            source.clip = clip;
            source.volume = AudioMix.Gain(AudioBus.Effects);
            source.Play();
            nextAt = Time.unscaledTime + clip.length + Gap;
        }

        public void Stop()
        {
            queue.Clear();
            if (source != null && source.isPlaying) source.Stop();
        }

        private void Enqueue(string folder, string clipName, string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            AudioClip clip = Resources.Load<AudioClip>(folder + "/" + clipName);
            if (clip != null) queue.Enqueue(clip);
        }

        /// <summary>"td_" and the first 8 hex digits of the MD5 of the line (UTF-8), as the generator names it.</summary>
        public static string ClipName(string line) => "td_" + Hash(line);

        /// <summary>The trainer's recording of a lesson's spoken line: "tn_" and the same hash.</summary>
        public static string NarrationClipName(string line) => "tn_" + Hash(line);

        private static string Hash(string line)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(line ?? ""));
            return System.BitConverter.ToString(hash, 0, 4).Replace("-", "").ToLowerInvariant();
        }
    }
}
