using Abandoned.Core;

namespace Abandoned.Networking
{
    /// <summary>Player-readable lobby errors, kept in one place so menus and tests agree on the wording.</summary>
    public static class LobbyMessages
    {
        public const string CreateFailed = "Couldn't create the Steam lobby, so friends can't join by invite. Try hosting again."; // @loc
        public const string AlreadyInGame = "Leave your current game first, then accept the invite again."; // @loc
        public const string Busy = "Already joining a lobby..."; // @loc
        public const string NoHost = "That lobby has no host any more."; // @loc
        public const string Full = "That game is full."; // @loc
        public const string Gone = "That game doesn't exist any more (the host left)."; // @loc

        public static string VersionMismatch(string hostKey, string ourKey) => SessionMessages.VersionMismatch(hostKey, ourKey);

        /// <summary>Steam's RoomEnter result names -> what the player should know.</summary>
        public static string ForRoomEnter(string result) => result switch
        {
            "DoesntExist" => Gone,
            "Full" => Full,
            "NotAllowed" => Loc.Mark("You're not allowed into that lobby (it's friends-only)."),
            "Banned" or "CommunityBan" => Loc.Mark("You can't join that lobby."),
            "Limited" => Loc.Mark("Limited Steam accounts can't join lobbies."),
            "MemberBlockedYou" or "YouBlockedMember" => Loc.Mark("Someone in that lobby has blocked you (or you them)."),
            "RatelimitExceeded" => Loc.Mark("Steam says slow down; try again in a moment."),
            _ => Loc.Wire("Couldn't join the Steam lobby ({0}).", result),
        };
    }
}
