using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>HQ crew room (2026 remake): the notice board, E opens how to play and the controls.</summary>
    public class NoticeBoard : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user) => MenuUi.Current != null ? Loc.T("Notice board: how the job works") : null;

        public void Use(GameObject user) => MenuUi.Current?.OpenNotices();
    }
}
