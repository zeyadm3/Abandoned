namespace Abandoned.Networking
{
    /// <summary>Player-readable lobby errors, kept in one place so menus and tests agree on the wording.</summary>
    public static class LobbyMessages
    {
        public const string CreateFailed = "Couldn't create the Steam lobby, so friends can't join by invite. Try hosting again.";
        public const string AlreadyInGame = "Leave your current game first, then accept the invite again.";
        public const string Busy = "Already joining a lobby...";
        public const string NoHost = "That lobby has no host any more.";
        public const string Full = "That game is full.";
        public const string Gone = "That game doesn't exist any more (the host left).";

        public static string VersionMismatch(string hostKey, string ourKey) => SessionMessages.VersionMismatch(hostKey, ourKey);

        /// <summary>Steam's RoomEnter result names -> what the player should know.</summary>
        public static string ForRoomEnter(string result) => result switch
        {
            "DoesntExist" => Gone,
            "Full" => Full,
            "NotAllowed" => "You're not allowed into that lobby (it's friends-only).",
            "Banned" or "CommunityBan" => "You can't join that lobby.",
            "Limited" => "Limited Steam accounts can't join lobbies.",
            "MemberBlockedYou" or "YouBlockedMember" => "Someone in that lobby has blocked you (or you them).",
            "RatelimitExceeded" => "Steam says slow down; try again in a moment.",
            _ => $"Couldn't join the Steam lobby ({result}).",
        };
    }
}
