using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Company
{
    /// <summary>
    /// The HQ HUD (M8.1b): the company's money (red = debt), level and experience, the missed-quota
    /// streak, and today's job (or what to do next).
    /// </summary>
    public class HqHud : MonoBehaviour
    {
        private VisualElement top;
        private Label money, level, streak, job;

        /// <summary>Tests: the job chip.</summary>
        public string JobText => job != null ? job.text : null;

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            bool showing = company != null && company.IsSpawned;
            if (top == null)
            {
                if (!showing || HudLayer.Root == null) return;
                top = HudLayer.Add(new VisualElement(), "hud-top");
                money = Chip();
                level = Chip();
                streak = Chip();
                job = Chip();
            }
            MenuKit.Show(top, showing);
            if (!showing) return;

            CompanyNetState s = company.State;
            int next = s.Level - 1 < company.Config.LevelXp.Length ? company.Config.LevelXp[s.Level - 1] : s.Xp;
            Set(money, s.Money < 0 ? $"DEBT ${-s.Money:N0}" : $"${s.Money:N0}");
            money.EnableInClassList("hud-chip--bad", s.Money < 0);
            Set(level, $"LEVEL {s.Level}  ({s.Xp}/{next} XP)");
            MenuKit.Show(streak, s.MissedQuotas > 0);
            Set(streak, $"MISSED QUOTAS {s.MissedQuotas}/{company.Config.MissesToBankruptcy}");
            streak.AddToClassList("hud-chip--bad");
            Set(job, company.Selected >= 0
                ? $"JOB: {company.Board[company.Selected].ModifierName} AT {company.Board[company.Selected].Location} - TAKE THE VAN"
                : "NO JOB YET: READ THE CONTRACT BOARD");
            job.EnableInClassList("hud-chip--good", company.Selected >= 0);
        }

        private Label Chip()
        {
            var chip = new Label { pickingMode = PickingMode.Ignore };
            chip.AddToClassList("hud-chip");
            top.Add(chip);
            return chip;
        }

        private static void Set(Label label, string text)
        {
            if (label.text != text) label.text = text;
        }

        private void OnDisable()
        {
            if (top != null) MenuKit.Show(top, false);
        }

        private void OnDestroy() => HudLayer.Remove(top);
    }
}
