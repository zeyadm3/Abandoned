using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.UI;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The appraisal after the training truck leaves: the receipt prints as on a real job, and beside it, instead of
    /// a payday, how a real job settles (nothing from training is banked). The host then takes the crew home or runs
    /// the house again.
    /// </summary>
    public static class TutorialAppraisal
    {
        public static void Payday(VisualElement panel, RunResults results)
        {
            MenuKit.Text(panel, Loc.T("TRAINING / NOTHING BANKED"), "section");
            MenuKit.Text(panel, Loc.T("On a real job this haul is paid to the company when the truck gets back."));
            MenuKit.Text(panel, results != null && results.QuotaMet
                ? Loc.T("You met the quota. On a real job, missing it means the shortfall comes off the company's money as a penalty.")
                : Loc.T("You missed the quota. On a real job, the shortfall comes off the company's money as a penalty."));
            MenuKit.Text(panel, Loc.T("Miss three quotas in a row and the company folds: a new one starts with the basic kit."));
            MenuKit.Text(panel, Loc.T("Pick jobs at the dispatch telephone; buy gear at the maintenance bench; read the field guide when something new finds you."));
        }

        /// <summary>The host's choices once the training truck has gone.</summary>
        public static void Buttons(VisualElement box, RunDirector director, bool isHost)
        {
            CompanyService company = CompanyService.Current;
            if (!isHost)
            {
                MenuKit.Text(box, Loc.T("Waiting for the host..."));
                return;
            }
            if (company != null && company.IsSpawned)
                MenuKit.Button(box, Loc.T("Back to HQ"), company.ReturnFromTraining, SoundId.UiConfirm);
            if (director != null)
                MenuKit.Button(box, Loc.T("Run the training again"), director.StartNextRun, company != null && company.IsSpawned ? SoundId.UiClick : SoundId.UiConfirm);
        }
    }
}
