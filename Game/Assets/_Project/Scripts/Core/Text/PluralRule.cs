namespace Abandoned.Core
{
    /// <summary>
    /// How a language picks a word form for a number (0.24.8). A counted line lists its forms in this order,
    /// split by " | ": "one | other", or "one | few | many" for Russian and Polish.
    /// </summary>
    public enum PluralRule
    {
        /// <summary>One form for every number (Chinese, Japanese, Korean).</summary>
        None,

        /// <summary>1, then everything else (English, German, Spanish).</summary>
        OneOther,

        /// <summary>0 and 1, then everything else (French, Brazilian Portuguese).</summary>
        ZeroOneOther,

        /// <summary>1, 21, 31... then 2-4, 22-24... then the rest (Russian).</summary>
        Russian,

        /// <summary>1, then 2-4, 22-24... then the rest (Polish).</summary>
        Polish,
    }
}
