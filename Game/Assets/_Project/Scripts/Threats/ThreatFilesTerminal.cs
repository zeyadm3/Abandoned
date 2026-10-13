using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>The worn HQ field guide: press E to inspect the monster notes (<see cref="ThreatFiles"/>).</summary>
    public class ThreatFilesTerminal : MonoBehaviour, IUsable
    {
        public static bool Open { get; set; }

        public string UsePrompt(GameObject user) => Loc.T("Inspect the field guide / monster manual");

        public void Use(GameObject user) => Open = true;

        private void OnDisable() => Open = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Open = false;
    }
}
