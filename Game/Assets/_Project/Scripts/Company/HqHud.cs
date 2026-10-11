using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Company
{
    /// <summary>
    /// A compact HQ ledger: money, clearance, quota failures and the crew's next assignment.
    /// </summary>
    public class HqHud : MonoBehaviour
    {
        private VisualElement top, account, failures;
        private Label accountCaption, money, level, experience, streak, dispatchCaption, job, departure;

        /// <summary>Tests: the visible assignment or next step.</summary>
        public string JobText => job != null ? job.text : null;

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            bool showing = company != null && company.IsSpawned;
            if (top == null || top.panel == null)
            {
                if (!showing || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(top, showing);
            if (!showing) return;

            CompanyNetState s = company.State;
            int index = s.Level - 1;
            bool capped = index >= company.Config.LevelXp.Length;
            int next = index >= 0 && !capped ? company.Config.LevelXp[index] : s.Xp;
            bool debt = s.Money < 0;
            Set(accountCaption, debt ? "DEBT" : "BALANCE");
            Set(money, $"${(debt ? -(long)s.Money : s.Money):N0}");
            account.EnableInClassList("hq-brief__metric--warning", debt);
            Set(level, $"{s.Level:00}");
            Set(experience, capped ? $"{s.Xp} XP / MAX" : $"{s.Xp}/{next} XP");
            MenuKit.Show(failures, s.MissedQuotas > 0);
            Set(streak, $"{s.MissedQuotas} / {company.Config.MissesToBankruptcy}");

            bool assigned = company.Selected >= 0 && company.Selected < company.Board.Count;
            Set(dispatchCaption, assigned ? "ASSIGNED" : "NEXT STEP");
            Set(job, assigned
                ? $"{company.Board[company.Selected].ModifierName.ToUpperInvariant()} / {company.Board[company.Selected].Location.ToUpperInvariant()}"
                : "READ THE CONTRACT BOARD");
            MenuKit.Show(departure, assigned);
        }

        private void Build()
        {
            HudLayer.Remove(top);
            top = HudLayer.Add(new VisualElement(), "hq-brief");
            top.RegisterCallback<GeometryChangedEvent>(_ =>
                top.EnableInClassList("hq-brief--compact", top.layout.width < 520f));

            VisualElement metrics = Element(top, "hq-brief__metrics");
            account = Element(metrics, "hq-brief__metric", "hq-brief__account");
            accountCaption = Text(account, "BALANCE", "hq-brief__caption");
            money = Text(account, "", "hq-brief__value");

            VisualElement clearance = Element(metrics, "hq-brief__metric", "hq-brief__clearance");
            Text(clearance, "CLEARANCE", "hq-brief__caption");
            VisualElement progress = Element(clearance, "hq-brief__progress");
            level = Text(progress, "", "hq-brief__value");
            experience = Text(progress, "", "hq-brief__experience");

            failures = Element(metrics, "hq-brief__metric", "hq-brief__failures", "hq-brief__metric--warning");
            Text(failures, "MISSED QUOTAS", "hq-brief__caption");
            streak = Text(failures, "", "hq-brief__value");

            VisualElement assignment = Element(top, "hq-brief__assignment");
            dispatchCaption = Text(assignment, "NEXT STEP", "hq-brief__dispatch-caption");
            VisualElement detail = Element(assignment, "hq-brief__detail");
            job = Text(detail, "", "hq-brief__job");
            departure = Text(detail, "TAKE THE VAN", "hq-brief__departure");
            MenuKit.Show(departure, false);
        }

        private static VisualElement Element(VisualElement parent, params string[] classes)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            foreach (string c in classes) element.AddToClassList(c);
            parent.Add(element);
            return element;
        }

        private static Label Text(VisualElement parent, string value, string cls)
        {
            var label = new Label(value) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(cls);
            parent.Add(label);
            return label;
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
