namespace Abandoned.Networking
{
    /// <summary>Which NGO transport carries the session.</summary>
    public enum TransportMode
    {
        /// <summary>Direct IP over Unity Transport: local testing, LAN, Multiplayer Play Mode.</summary>
        UnityTransport,
        /// <summary>Steam relay over the embedded Facepunch transport (needs Steam running).</summary>
        Steam,
    }
}
