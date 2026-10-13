using System;

namespace Abandoned.Core
{
    /// <summary>Picks the word form a number needs ("1 item", "3 items"; Russian and Polish have three).</summary>
    public static class Plurals
    {
        public const string Separator = " | ";

        /// <summary>Which of a line's forms fits <paramref name="count"/>: 0 is the first.</summary>
        public static int Form(PluralRule rule, long count)
        {
            long n = Math.Abs(count), ones = n % 10, tens = n % 100;
            bool few = ones >= 2 && ones <= 4 && (tens < 12 || tens > 14);
            switch (rule)
            {
                case PluralRule.OneOther: return n == 1 ? 0 : 1;
                case PluralRule.ZeroOneOther: return n <= 1 ? 0 : 1;
                case PluralRule.Russian: return ones == 1 && tens != 11 ? 0 : few ? 1 : 2;
                case PluralRule.Polish: return n == 1 ? 0 : few ? 1 : 2;
                default: return 0;
            }
        }

        /// <summary>The form for <paramref name="count"/> out of "one | few | many". A line with fewer forms gives its last.</summary>
        public static string Pick(string forms, PluralRule rule, long count)
        {
            if (string.IsNullOrEmpty(forms)) return forms;
            int want = Form(rule, count), start = 0;
            for (int i = 0; i < want; i++)
            {
                int next = forms.IndexOf(Separator, start, StringComparison.Ordinal);
                if (next < 0) break;
                start = next + Separator.Length;
            }
            int end = forms.IndexOf(Separator, start, StringComparison.Ordinal);
            return end < 0 ? forms.Substring(start) : forms.Substring(start, end - start);
        }
    }
}
