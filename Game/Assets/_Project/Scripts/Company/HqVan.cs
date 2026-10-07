using Abandoned.Contracts;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The company van in the HQ garage: once the host picked a job, anyone can drive everyone there.</summary>
    public class HqVan : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company == null) return null;
            if (company.Selected < 0) return "Pick a contract at the board first";
            Contract c = company.Board[company.Selected];
            return $"Drive to {c.Location} ({c.ModifierName}, quota ${company.QuotaFor(c):N0})";
        }

        public void Use(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company != null && company.Selected >= 0) company.RequestDepart();
        }
    }
}
