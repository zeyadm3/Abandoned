using System;
using UnityEngine;

namespace Abandoned.Onboarding
{
    /// <summary>One thing the player may say back, and what the speaker answers. It changes a line, never the outcome.</summary>
    [Serializable]
    public class DialogueReply
    {
        [Tooltip("What the player says (English; worded through Loc.T).")]
        public string text;
        [Tooltip("The speaker's answer (English; its voice clip is found by the line's hash).")]
        public string answer;
    }
}
