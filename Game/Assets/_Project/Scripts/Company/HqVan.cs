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
            if (company.Selected < 0 || company.Selected >= company.Board.Count) return Loc.T("Pick a contract at the board first");
            Contract c = company.Board[company.Selected];
            return Loc.T("Drive to {0} ({1}, quota ${2:N0})", Loc.T(c.Location), Loc.T(c.ModifierName), company.QuotaFor(c));
        }

        public void Use(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company != null && company.Selected >= 0) company.RequestDepart();
        }
    }
}
