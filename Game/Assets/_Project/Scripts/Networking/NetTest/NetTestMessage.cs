#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
namespace Abandoned.Networking
{
    /// <summary>One scenario message: a kind ("move", "report"...) and a payload (usually JSON).</summary>
    public readonly struct NetTestMessage
    {
        public ulong Sender { get; }
        public string Kind { get; }
        public string Payload { get; }

        public NetTestMessage(ulong sender, string kind, string payload)
        {
            Sender = sender;
            Kind = kind;
            Payload = payload ?? string.Empty;
        }
    }
}

#endif
