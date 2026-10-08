using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>The HQ office's desk computer: press E to read the threat files (<see cref="ThreatFiles"/>).</summary>
    public class ThreatFilesTerminal : MonoBehaviour, IUsable
    {
        public static bool Open { get; set; }

        public string UsePrompt(GameObject user) => "Read the threat files";

        public void Use(GameObject user) => Open = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Open = false;
    }
}
