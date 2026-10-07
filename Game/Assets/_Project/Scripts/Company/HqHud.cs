using Abandoned.Contracts;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>Placeholder HQ HUD: the company's money (red = debt), level and experience, the missed-quota streak, today's job.</summary>
    public class HqHud : MonoBehaviour
    {
        [SerializeField] private int fontSize = 17;

        private GUIStyle style;

        private void OnGUI()
        {
            CompanyService company = CompanyService.Current;
            if (company == null || !company.IsSpawned) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, alignment = TextAnchor.UpperCenter, richText = true };
            CompanyNetState s = company.State;
            int next = s.Level - 1 < company.Config.LevelXp.Length ? company.Config.LevelXp[s.Level - 1] : s.Xp;
            string money = s.Money < 0 ? $"<color=#ff7766>DEBT ${-s.Money:N0}</color>" : $"${s.Money:N0}";
            string streak = s.MissedQuotas > 0
                ? $"   <color=#ff7766>Missed quotas: {s.MissedQuotas}/{company.Config.MissesToBankruptcy}</color>" : "";
            string job = company.Selected >= 0 ? $"   Job: {company.Board[company.Selected].ModifierName} at {company.Board[company.Selected].Location} - take the van"
                : "   No job yet: read the contract board";
            GUI.Label(new Rect(0f, 8f, Screen.width, 28f), $"{money}   Level {s.Level} ({s.Xp}/{next} xp){streak}{job}", style);
        }
    }
}
