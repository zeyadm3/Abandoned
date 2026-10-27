using Abandoned.Core;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>HQ: the garage's paint-shop desk by the truck (E) opens the truck garage (2026 remake).</summary>
    public class TruckGarageTerminal : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user) => MenuUi.Current != null && Company.CompanyService.Current != null ? Loc.T("Kit out the truck") : null;

        public void Use(GameObject user) => MenuUi.Current?.OpenTruckGarage();
    }
}
