using System.Text;
using Abandoned.Core;
using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>
    /// The host's door rules for a connecting machine, free of NGO's callback types so they are
    /// unit-tested: other builds are turned away first (with both versions in the reason), then a
    /// full game. The host's own local client always gets in.
    /// </summary>
    public static class ConnectionGate
    {
        // A compatibility key is a few dozen bytes; anything much larger isn't one of ours.
        public const int MaxPayloadBytes = 256;

        public static byte[] Payload(string compatibilityKey) => Encoding.UTF8.GetBytes(compatibilityKey ?? string.Empty);

        public static string KeyOf(byte[] payload) =>
            payload != null && payload.Length <= MaxPayloadBytes ? Encoding.UTF8.GetString(payload) : string.Empty;

        /// <summary>True = let them in on <paramref name="slot"/>; false = refuse with <paramref name="reason"/>.</summary>
        public static bool Admit(ulong clientId, byte[] payload, string hostKey, SpawnSlots slots, int maxPlayers,
            out int slot, out string reason)
        {
            slot = -1;
            reason = string.Empty;
            if (clientId != NetworkManager.ServerClientId)
            {
                string theirs = KeyOf(payload);
                if (!VersionInfo.AreCompatible(hostKey, theirs))
                {
                    reason = SessionMessages.VersionMismatch(hostKey, theirs);
                    return false;
                }
            }
            if (slots.TryAssign(clientId, out slot)) return true;
            reason = SessionMessages.Full(maxPlayers);
            return false;
        }
    }
}
