using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// The HR manager's recorded lines (Tools/Audio/generate_hr_interview.py, Kokoro-82M): each is named "hr_" plus the
    /// first 8 hex digits of the MD5 of its English text, so a line edited in the data simply has no clip until the
    /// script runs again (the subtitle still shows).
    /// </summary>
    public static class DialogueVoice
    {
        private static readonly Dictionary<string, AudioClip> cache = new();

        public static AudioClip ClipFor(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            if (cache.TryGetValue(text, out AudioClip clip)) return clip;
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            var name = new StringBuilder("hr_");
            for (int i = 0; i < 4; i++) name.Append(hash[i].ToString("x2"));
            return cache[text] = Resources.Load<AudioClip>("Onboarding/" + name);
        }
    }
}
