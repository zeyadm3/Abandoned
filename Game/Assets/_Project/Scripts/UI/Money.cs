namespace Abandoned.UI
{
    /// <summary>Money as the HUD writes it.</summary>
    public static class Money
    {
        /// <summary>"$950", "$1.2k", "$35k", "$1.1M": short enough for a pocket slot.</summary>
        public static string Short(int value)
        {
            if (value < 1000) return $"${value}";
            if (value < 10000) return $"${value / 1000f:0.#}k";
            if (value < 1000000) return $"${value / 1000}k";
            return $"${value / 1000000f:0.#}M";
        }

        /// <summary>A value's tier for colour: 0 small change, 1 decent, 2 good, 3 a big one.</summary>
        public static int Tier(int value) => value >= 20000 ? 3 : value >= 5000 ? 2 : value >= 1000 ? 1 : 0;
    }
}
