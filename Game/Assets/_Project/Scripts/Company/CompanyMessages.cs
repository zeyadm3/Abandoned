using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The company's voice (GDD 4: "dry, bleak humor from the company"): the boss's voicemails by
    /// situation and the one-liners on the contract board. Text only, so adding lines needs no code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Company/Company Messages", fileName = "CompanyMessages")]
    public class CompanyMessages : ScriptableObject, IValidatable
    {
        [field: SerializeField, TextArea(2, 6)] public string[] Welcome { get; private set; } = System.Array.Empty<string>();
        [field: SerializeField, TextArea(2, 6)] public string[] QuotaMet { get; private set; } = System.Array.Empty<string>();
        [field: Tooltip("Twice the quota or more.")]
        [field: SerializeField, TextArea(2, 6)] public string[] BigHaul { get; private set; } = System.Array.Empty<string>();
        [field: SerializeField, TextArea(2, 6)] public string[] QuotaMissed { get; private set; } = System.Array.Empty<string>();
        [field: Tooltip("One more miss and the company goes under.")]
        [field: SerializeField, TextArea(2, 6)] public string[] LastWarning { get; private set; } = System.Array.Empty<string>();
        [field: SerializeField, TextArea(2, 6)] public string[] Bankrupt { get; private set; } = System.Array.Empty<string>();
        [field: SerializeField, TextArea(2, 6)] public string[] LevelUp { get; private set; } = System.Array.Empty<string>();
        [field: Tooltip("One per contract card, picked by its seed.")]
        [field: SerializeField] public string[] BoardQuips { get; private set; } = System.Array.Empty<string>();

        public void Validate(List<string> errors)
        {
            if (Welcome.Length == 0 || QuotaMet.Length == 0 || QuotaMissed.Length == 0)
                errors.Add($"{name}: needs at least a welcome, a quota-met and a quota-missed voicemail.");
        }

        /// <summary>
        /// Today's voicemail for the company as it stands (same on every machine): the last job's outcome
        /// first (bankruptcy, a last warning, a miss, a big haul, a level, a plain pass), a welcome before any.
        /// </summary>
        public (string key, string text) Voicemail(CompanyNetState s, OutcomeNet last)
        {
            int pick = s.Runs + s.Bankruptcies * 7;
            if (s.Runs == 0 && last.Run == 0) return Line("welcome", s.Bankruptcies > 0 ? Bankrupt : Welcome, pick);
            if (last.Bankrupt) return Line("bankrupt", Bankrupt, pick);
            if (!last.QuotaMet && last.MissedInARow >= 2) return Line("warning", LastWarning, pick);
            if (!last.QuotaMet) return Line("missed", QuotaMissed, pick);
            if (last.LevelledUp && LevelUp.Length > 0) return Line("level", LevelUp, pick);
            if (last.Quota > 0 && last.Haul >= 2 * last.Quota && BigHaul.Length > 0) return Line("big", BigHaul, pick);
            return Line("met", QuotaMet, pick);
        }

        private static (string, string) Line(string kind, string[] lines, int pick) =>
            lines == null || lines.Length == 0 ? (kind, "") : ($"{kind}:{pick}", lines[Mathf.Abs(pick) % lines.Length]);

        public string Quip(int seed) => BoardQuips.Length == 0 ? "" : BoardQuips[Mathf.Abs(seed) % BoardQuips.Length];

#if UNITY_EDITOR
        public void EditorSetup(string[] welcome, string[] met, string[] big, string[] missed, string[] warning, string[] bankrupt, string[] level, string[] quips)
        {
            Welcome = welcome;
            QuotaMet = met;
            BigHaul = big;
            QuotaMissed = missed;
            LastWarning = warning;
            Bankrupt = bankrupt;
            LevelUp = level;
            BoardQuips = quips;
        }
#endif
    }
}
