using System;
using System.Globalization;

namespace Abandoned.Networking
{
    /// <summary>
    /// Command-line flags of the multi-process localhost nettest (Tools/nettest.sh):
    /// <c>-nettest host|client -nettestPort N -nettestScenario name -nettestOut file.json</c>,
    /// optionally <c>-nettestClients N</c> (clients the host waits for, default 3) and
    /// <c>-nettestTimeout seconds</c>. Without <c>-nettest</c> the game ignores all of them.
    /// </summary>
    public readonly struct NetTestArgs
    {
        public const int DefaultClients = 3;
        public const float DefaultTimeout = 90f;
        public const string DefaultScenario = "basic";

        public NetTestRole Role { get; }
        public ushort Port { get; }
        public string Scenario { get; }
        public string OutPath { get; }
        public int Clients { get; }
        public float Timeout { get; }

        public bool Active => Role != NetTestRole.None;

        public NetTestArgs(NetTestRole role, ushort port, string scenario, string outPath, int clients, float timeout)
        {
            Role = role;
            Port = port;
            Scenario = scenario;
            OutPath = outPath;
            Clients = clients;
            Timeout = timeout;
        }

        public static NetTestArgs Parse(string[] args)
        {
            NetTestRole role = NetTestRole.None;
            ushort port = 0;
            string scenario = DefaultScenario, outPath = null;
            int clients = DefaultClients;
            float timeout = DefaultTimeout;
            if (args == null) return default;

            for (int i = 0; i < args.Length - 1; i++)
            {
                string flag = args[i], value = args[i + 1];
                if (Is(flag, "-nettest"))
                {
                    if (Is(value, "host")) role = NetTestRole.Host;
                    else if (Is(value, "client")) role = NetTestRole.Client;
                }
                else if (Is(flag, "-nettestPort")) ushort.TryParse(value, out port);
                else if (Is(flag, "-nettestScenario")) scenario = value;
                else if (Is(flag, "-nettestOut")) outPath = value;
                else if (Is(flag, "-nettestClients") && int.TryParse(value, out int n)) clients = Math.Max(0, n);
                else if (Is(flag, "-nettestTimeout") &&
                         float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float t) && t > 0f)
                    timeout = t;
            }
            return new NetTestArgs(role, port, scenario, outPath, clients, timeout);
        }

        /// <summary>Why these args can't run, or null when they can.</summary>
        public string Problem()
        {
            if (!Active) return "not a nettest launch";
            if (Port == 0) return "-nettestPort is missing or 0 (clients need the host's real port)";
            if (string.IsNullOrWhiteSpace(Scenario)) return "-nettestScenario is empty";
            return null;
        }

        private static bool Is(string a, string flag) => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase);
    }
}
