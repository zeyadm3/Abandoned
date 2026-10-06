using System;

namespace Abandoned.Networking
{
    /// <summary>
    /// Session command-line flags: <c>-host</c>, <c>-connect address[:port]</c> (or a SteamID64 with
    /// <c>-transport steam</c>), <c>-client</c> (a client that waits for the menu) and
    /// <c>-transport unity|steam</c>. Used by builds, the multi-process nettest and MPPM players.
    /// </summary>
    public readonly struct NetworkLaunchArgs
    {
        public bool Host { get; }
        public bool Client { get; }
        public string ConnectAddress { get; }
        public ushort ConnectPort { get; }
        public TransportMode? Transport { get; }

        /// <summary>Launched to be a client: never auto-host.</summary>
        public bool IsClientLaunch => Client || !string.IsNullOrEmpty(ConnectAddress);

        private NetworkLaunchArgs(bool host, bool client, string address, ushort port, TransportMode? transport)
        {
            Host = host;
            Client = client;
            ConnectAddress = address;
            ConnectPort = port;
            Transport = transport;
        }

        public static NetworkLaunchArgs Parse(string[] args)
        {
            bool host = false, client = false;
            string address = null;
            ushort port = 0;
            TransportMode? transport = null;
            if (args == null) return default;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (Is(a, "-host")) host = true;
                else if (Is(a, "-client")) client = true;
                else if (Is(a, "-connect") && i + 1 < args.Length) SplitAddress(args[++i], out address, out port);
                else if (Is(a, "-transport") && i + 1 < args.Length)
                {
                    string value = args[++i];
                    if (Is(value, "steam")) transport = TransportMode.Steam;
                    else if (Is(value, "unity")) transport = TransportMode.UnityTransport;
                }
            }
            return new NetworkLaunchArgs(host, client, address, port, transport);
        }

        /// <summary>"1.2.3.4:7777" -> address + port; no port (or a SteamID64) leaves port 0.</summary>
        public static void SplitAddress(string text, out string address, out ushort port)
        {
            port = 0;
            address = text?.Trim();
            if (string.IsNullOrEmpty(address)) return;
            int colon = address.LastIndexOf(':');
            // One colon only: IPv6 literals have several and are taken as-is.
            if (colon <= 0 || address.IndexOf(':') != colon) return;
            if (ushort.TryParse(address.Substring(colon + 1), out ushort parsed)) port = parsed;
            address = address.Substring(0, colon);
        }

        private static bool Is(string a, string flag) => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase);
    }
}
