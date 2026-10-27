using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>HQ dispatch office (2026 remake): the crew board, E opens who is here, invites and the host's rules.</summary>
    public class CrewBoard : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user) => MenuUi.Current != null ? Loc.T("Crew board: invite friends, the crew, house rules") : null;

        public void Use(GameObject user) => MenuUi.Current?.OpenCrewBoard();
    }
}
