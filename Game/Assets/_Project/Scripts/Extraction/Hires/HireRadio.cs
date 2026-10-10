using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>The truck's radio to the staffing agency (0.19.1): E calls a replacement for a dead crewmate.</summary>
    public class HireRadio : MonoBehaviour, IUsable, IHintSource
    {
        public HintId Hint => HintId.Hire;

        public string UsePrompt(GameObject user)
        {
            RunHires hires = RunHires.Current;
            if (hires == null || !hires.IsSpawned) return null;
            NetworkPlayer dead = hires.Replaces();
            if (dead == null) return null;
            if (!hires.HiresLeft) return "The agency won't send anyone else today";
            return $"Hire a replacement for {dead.DisplayName} (-${hires.Fee:N0})";
        }

        public void Use(GameObject user) => RunHires.Current?.RequestHire();
    }
}
