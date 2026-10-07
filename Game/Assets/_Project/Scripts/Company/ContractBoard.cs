using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The HQ's contract board (GDD 14): press E to read today's three jobs.</summary>
    public class ContractBoard : MonoBehaviour, IUsable
    {
        public static bool Open { get; set; }

        public string UsePrompt(GameObject user) => CompanyService.Current == null ? null
            : CompanyService.Current.DemoOver ? "The demo is over - thanks for playing!" : "Read the contract board";

        public void Use(GameObject user)
        {
            if (CompanyService.Current != null && CompanyService.Current.DemoOver) UI.MenuUi.Current?.OpenDemoEnd();
            else Open = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Open = false;
    }
}
