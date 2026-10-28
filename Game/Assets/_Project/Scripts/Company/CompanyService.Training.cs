using Abandoned.Equipment;
using Abandoned.Networking;
using Abandoned.Tutorial;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The training job (the house at 14 Larch Court): the host takes the crew there from the HQ and back. It is not
    /// a board contract, so nothing about it is owed, paid, struck or saved; the board waits as it was.
    /// </summary>
    public partial class CompanyService
    {
        /// <summary>The training level is in this build (dev builds made before it existed don't have it).</summary>
        public static bool TrainingAvailable => Application.CanStreamedLevelBeLoaded(TutorialMode.Scene);

        /// <summary>Host, at the HQ: everyone to the training house. No contract is taken.</summary>
        public void StartTraining()
        {
            if (!IsServer || SessionTravel.Current == null || !AtHome || !TrainingAvailable) return;
            active.Value = default;
            Debug.Log("[Company] The crew goes to the training house (nothing is owed or paid).");
            SessionTravel.Current.Travel(TutorialMode.Scene);
        }

        /// <summary>Host, from the training house: everyone home, the board as it was.</summary>
        public void ReturnFromTraining()
        {
            if (!IsServer || SessionTravel.Current == null) return;
            active.Value = default;
            SessionTravel.Current.Travel(HomeLevel);
        }

        private bool trainedThisVisit;

        // Host, every frame: back at the HQ after training, hands hold only what the company owns again.
        private void TrainingHousekeeping()
        {
            if (TutorialMode.Active)
            {
                trainedThisVisit = true;
                return;
            }
            if (!trainedThisVisit || SessionTravel.Current == null || !AtHome || !SessionTravel.LevelReady) return;
            trainedThisVisit = false;
            PlayerEquipment.ServerTrimToStock(NetworkManager);
        }
    }
}
