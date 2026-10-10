using Abandoned.Company;
using Abandoned.Extraction;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Steam rich presence (0.19.11): what friends see under your name ("At the HQ with a crew of 3", "On a
    /// salvage job: $12,400 of $20,000") and the crew grouped together. Needs the game's own App ID: the
    /// "#Status_*" tokens come from Tools/Steam/rich_presence_english.vdf, uploaded on the partner site.
    /// Under App ID 480 Steam shows nothing; the plain "status" key still fills "view game info".
    /// </summary>
    public class SteamPresence : MonoBehaviour
    {
        private const float Interval = 5f;
        private float next;
        private string shown;
        private SteamLobby lobby;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode) return;
            var go = new GameObject("SteamPresence");
            DontDestroyOnLoad(go);
            go.AddComponent<SteamPresence>();
        }

        private void Update()
        {
            if (Time.unscaledTime < next) return;
            next = Time.unscaledTime + Interval;
            SteamBootstrap steam = SteamBootstrap.Instance;
            if (steam == null || !steam.IsAvailable) return;
            string token, status;
            int crew = NetworkPlayer.All.Count;
            RunState run = RunState.Current;
            if (run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed)
            {
                token = "#Status_Job";
                status = $"On a salvage job: ${run.State.Haul:N0} of ${run.State.Quota:N0}";
                Set("haul", run.State.Haul.ToString("N0"));
                Set("quota", run.State.Quota.ToString("N0"));
            }
            else if (CompanyService.Current != null && CompanyService.Current.IsSpawned)
            {
                token = "#Status_HQ";
                status = $"At the HQ with a crew of {Mathf.Max(1, crew)}";
            }
            else
            {
                token = "#Status_Menu";
                status = "In the menus";
            }
            Set("crew", Mathf.Max(1, crew).ToString());
            if (status == shown) return;
            shown = status;
            Set("status", status);
            Set("steam_display", token);
            if (lobby == null) lobby = FindAnyObjectByType<SteamLobby>();
            SteamLobbyFlow flow = lobby != null ? lobby.Flow : null;
            if (flow != null && flow.InLobby)
            {
                Set("steam_player_group", flow.LobbyId.ToString());
                Set("steam_player_group_size", flow.Members.Count.ToString());
            }
            else Set("steam_player_group", "");
        }

        private static void Set(string key, string value)
        {
            try { Steamworks.SteamFriends.SetRichPresence(key, value); }
            catch (System.Exception e) { Debug.LogWarning($"[Steam] Rich presence {key}: {e.Message}"); }
        }

        private void OnApplicationQuit()
        {
            SteamBootstrap steam = SteamBootstrap.Instance;
            if (steam != null && steam.IsAvailable) Steamworks.SteamFriends.ClearRichPresence();
        }
    }
}
