namespace Abandoned.Tutorial
{
    /// <summary>
    /// How a lesson's progress bar words its numbers (the guide's Value and Goal), worded on each machine in the
    /// player's language. Sent as a byte: append only.
    /// </summary>
    public enum TutorialReadout : byte
    {
        /// <summary>The bar alone.</summary>
        None,
        /// <summary>"12 m to go" (Value = metres left).</summary>
        MetresLeft,
        /// <summary>"1.4 / 2 m" (moved or walked).</summary>
        Metres,
        /// <summary>"1.2 / 2.0 s".</summary>
        Seconds,
        /// <summary>"80° / 120°" (turned).</summary>
        Degrees,
        /// <summary>"$4,200 / $6,000" (the haul).</summary>
        Dollars,
        /// <summary>"Floor load 85%".</summary>
        LoadPercent,
        /// <summary>"2 / 3 aboard".</summary>
        Aboard,
    }
}
