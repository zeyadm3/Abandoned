using Steamworks;

namespace Abandoned.Networking
{
    /// <summary>The real Steam client through Facepunch.Steamworks.</summary>
    public sealed class FacepunchSteamClient : ISteamClient
    {
        public bool IsValid => SteamClient.IsValid;
        public ulong SteamId => SteamClient.IsValid ? SteamClient.SteamId.Value : 0UL;
        public string PlayerName => SteamClient.IsValid ? SteamClient.Name : string.Empty;

        // asyncCallbacks off: SteamBootstrap pumps RunCallbacks on the main thread so callbacks
        // (lobby joins, invites) can touch Unity objects safely.
        public void Init(uint appId)
        {
            // A previous editor Play session may have left Steam up; Init would throw "already initialized".
            if (SteamClient.IsValid) return;
            SteamClient.Init(appId, false);
        }

        public void RunCallbacks() => SteamClient.RunCallbacks();
        public void Shutdown() => SteamClient.Shutdown();
        public void OpenStoreOverlay(uint appId) => Steamworks.SteamFriends.OpenStoreOverlay(appId);
        public void SetAchievement(string apiName) => new Steamworks.Data.Achievement(apiName).Trigger();
    }
}
