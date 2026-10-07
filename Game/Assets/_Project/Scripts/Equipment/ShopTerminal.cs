using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>HQ: the shop (GDD 12). Anyone can buy with the company's money; the host checks.</summary>
    public class ShopTerminal : MonoBehaviour, IUsable
    {
        public static bool Open { get; set; }

        public string UsePrompt(GameObject user) => Company.CompanyService.Current == null ? null : "Shop for gear";

        public void Use(GameObject user) => Open = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Open = false;
    }
}
