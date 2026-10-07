namespace Abandoned.Networking
{
    /// <summary>
    /// The slice of Steam's client API that SteamBootstrap drives. A seam so tests can run the
    /// bootstrap's lifetime logic with a fake and never touch the real Steam on this machine.
    /// </summary>
    public interface ISteamClient
    {
        bool IsValid { get; }
        ulong SteamId { get; }
        string PlayerName { get; }

        /// <summary>Throws on failure (Steam not running, library missing...), like SteamClient.Init.</summary>
        void Init(uint appId);
        void RunCallbacks();
        void Shutdown();

        /// <summary>The Steam overlay on a store page (the demo's wishlist button).</summary>
        void OpenStoreOverlay(uint appId);

        /// <summary>Unlocks a Steam achievement by its API name (M9.5; needs the real App ID's setup).</summary>
        void SetAchievement(string apiName);
    }
}
