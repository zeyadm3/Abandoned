using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>HQ: the company's gear (E): each player fills their two hand slots from what the company owns.</summary>
    public class GearRack : MonoBehaviour, IUsable
    {
        public static bool Open { get; set; }

        public string UsePrompt(GameObject user) => Company.CompanyService.Current == null ? null : "Take gear (hand slots 1 and 2)";

        public void Use(GameObject user) => Open = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Open = false;
    }
}
