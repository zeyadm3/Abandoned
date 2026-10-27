using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// One spoken line of a scene: the English text (the key for its translation and its voice clip,
    /// Resources/Onboarding/hr_&lt;md5&gt;), the shot to cut to, how long to hold after it, an optional condition and
    /// optional replies for the player.
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [TextArea] public string text;
        [Tooltip("Index into the scene's shots (0 is the seated first-person view); -1 keeps the current one.")]
        public int shot = -1;
        [Tooltip("Seconds to wait after the line (or, without a voice clip, after reading time).")]
        public float hold = .5f;
        [Tooltip("Empty: always. Otherwise only when it holds: quick, slow or steady (the hiring's training time).")]
        public string condition = "";
        public List<DialogueReply> replies = new();
    }
}
