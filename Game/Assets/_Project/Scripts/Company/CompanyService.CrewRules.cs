using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>
    /// The host's rules for the crew (QA B-08): may they spend company money, start the van, pull the truck
    /// lever? Kept as the host's own preference (Prefs) and shown to everyone; every request is checked on the
    /// host with the sender's id. By default the crew may pull the lever but not spend or drive.
    /// </summary>
    public partial class CompanyService
    {
        private const string CrewRulesKey = "host.crewrules";
        private const CrewRule DefaultCrewRules = CrewRule.Lever;

        private readonly NetworkVariable<byte> crewRules = new((byte)DefaultCrewRules);

        /// <summary>Every machine: does the host let the crew do this?</summary>
        public bool CrewMay(CrewRule rule) => (crewRules.Value & (byte)rule) == (byte)rule;

        /// <summary>Host: may this client do it (the host always may)?</summary>
        public bool Allows(ulong clientId, CrewRule rule) => clientId == NetworkManager.ServerClientId || CrewMay(rule);

        /// <summary>This machine may do it (the host, or a crew member the host allows).</summary>
        public bool LocalMay(CrewRule rule) => IsServer || CrewMay(rule);

        /// <summary>Host: change a rule; remembered for the next game this machine hosts.</summary>
        public void SetCrewRule(CrewRule rule, bool allowed)
        {
            if (!IsServer) return;
            byte value = allowed ? (byte)(crewRules.Value | (byte)rule) : (byte)(crewRules.Value & ~(byte)rule);
            crewRules.Value = value;
            Prefs.SetInt(CrewRulesKey, value);
            Prefs.Save();
        }

        private void SpawnCrewRules()
        {
            if (IsServer) crewRules.Value = (byte)Prefs.GetInt(CrewRulesKey, (int)DefaultCrewRules);
        }

        /// <summary>What a crew member is told when the host keeps something to themselves.</summary>
        public static string Refusal(CrewRule rule) => rule switch
        {
            CrewRule.Spend => "The host keeps the company card. Ask them, or they can allow crew spending in the pause menu.",
            CrewRule.Drive => "Only the host can start the van. They can allow it in the pause menu.",
            CrewRule.Lever => "Only the host can pull the lever on this crew.",
            _ => "The host hasn't allowed that.",
        };

        // Client side: say no at once instead of sending a request the host will refuse.
        private bool RefuseLocally(CrewRule rule)
        {
            if (LocalMay(rule)) return false;
            UI.ToastFeed.Show("HOST ONLY", Refusal(rule), null, UI.ToastFeed.Kind.Warn);
            return true;
        }

        /// <summary>Host: remove a player from the game (the pause menu's Kick).</summary>
        public void Kick(ulong clientId)
        {
            if (!IsServer || clientId == NetworkManager.ServerClientId || !NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            UnityEngine.Debug.Log($"[Company] The host removed {NetworkPlayer.NameOf(clientId)} (client {clientId}).");
            NetworkManager.DisconnectClient(clientId, SessionMessages.Kicked);
        }
    }
}
