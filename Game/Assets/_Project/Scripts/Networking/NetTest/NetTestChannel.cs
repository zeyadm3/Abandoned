using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>
    /// Scenario coordination over NGO named messages: lets the host tell clients when to act and
    /// collect what each client saw, without adding test-only RPCs to gameplay objects. One channel
    /// per NetworkManager, so the in-process PlayMode test can run four "machines" side by side.
    /// </summary>
    public sealed class NetTestChannel
    {
        public const string MessageName = "Abandoned.NetTest";
        private const int InitialSize = 1024, MaxSize = 512 * 1024;

        private readonly NetworkManager manager;
        private readonly List<NetTestMessage> inbox = new();
        private bool registered;

        public NetTestChannel(NetworkManager manager) => this.manager = manager;

        /// <summary>Call once the manager is listening (the messaging manager exists only then).</summary>
        public void Open()
        {
            if (registered || manager.CustomMessagingManager == null) return;
            manager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, OnMessage);
            registered = true;
        }

        public void Close()
        {
            if (!registered) return;
            manager.CustomMessagingManager?.UnregisterNamedMessageHandler(MessageName);
            registered = false;
        }

        public void SendToHost(string kind, string payload = "") => Send(NetworkManager.ServerClientId, kind, payload);

        /// <summary>Host only: every connected client except the host's own.</summary>
        public void SendToClients(string kind, string payload = "")
        {
            foreach (ulong id in manager.ConnectedClientsIds)
                if (id != manager.LocalClientId) Send(id, kind, payload);
        }

        public void Send(ulong target, string kind, string payload)
        {
            using var writer = new FastBufferWriter(InitialSize, Allocator.Temp, MaxSize);
            writer.WriteValueSafe(kind);
            writer.WriteValueSafe(payload ?? string.Empty);
            manager.CustomMessagingManager.SendNamedMessage(MessageName, target, writer, NetworkDelivery.ReliableFragmentedSequenced);
        }

        /// <summary>Removes and returns the first waiting message of this kind (from anyone).</summary>
        public bool TryTake(string kind, out NetTestMessage message)
        {
            for (int i = 0; i < inbox.Count; i++)
            {
                if (inbox[i].Kind != kind) continue;
                message = inbox[i];
                inbox.RemoveAt(i);
                return true;
            }
            message = default;
            return false;
        }

        private void OnMessage(ulong sender, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string kind);
            reader.ReadValueSafe(out string payload);
            inbox.Add(new NetTestMessage(sender, kind, payload));
        }
    }
}
