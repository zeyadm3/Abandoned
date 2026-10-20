using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The training clipboard on the HQ dispatch bench: E (the host) takes the crew to the practice house. Scene
    /// object at the HQ; the company does the travelling.
    /// </summary>
    public class TrainingClipboard : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company == null || !company.IsSpawned || !CompanyService.TrainingAvailable) return null;
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsSpawned) return null;
            return company.IsServer ? Loc.T("Training job: a practice house, no pay, no risk") : Loc.T("Training job (the host takes the crew)");
        }

        public void Use(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company == null || !company.IsSpawned) return;
            if (company.IsServer) company.StartTraining();
            else if (user != null && user.GetComponentInParent<NetworkPlayer>() is NetworkPlayer me && me.Carrier != null)
                me.Carrier.ShowHint(Loc.Mark("Ask the host to start the training job."));
        }
    }
}
