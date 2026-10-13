namespace Abandoned.Core
{
    /// <summary>
    /// The writing system a language needs (0.24.8): it decides which fallback font leads. Chinese, Japanese and
    /// Korean share thousands of characters but draw them differently, so each must find its own font first.
    /// </summary>
    public enum TextScript
    {
        Latin,
        Cyrillic,
        Chinese,
        Japanese,
        Korean,
    }
}
