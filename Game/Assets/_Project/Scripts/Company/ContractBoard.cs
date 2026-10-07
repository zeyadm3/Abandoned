using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The HQ's contract board (GDD 14): press E to read today's three jobs.</summary>
    public class ContractBoard : MonoBehaviour, IUsable
    {
        public static bool Open { get; set; }

        public string UsePrompt(GameObject user) => CompanyService.Current == null ? null : "Read the contract board";

        public void Use(GameObject user) => Open = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Open = false;
    }
}
