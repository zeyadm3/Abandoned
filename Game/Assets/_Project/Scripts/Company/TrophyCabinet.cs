using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>HQ break room (2026 remake): the trophy cabinet, E opens this player's achievements.</summary>
    public class TrophyCabinet : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user) => MenuUi.Current != null ? Loc.T("Trophy cabinet: your achievements") : null;

        public void Use(GameObject user) => MenuUi.Current?.OpenTrophies();
    }
}
