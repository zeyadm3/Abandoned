using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Tutorial;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// The first launch, on the session object (2026 remake): the HR interview (offline, in the HR office), straight to
    /// the training job (solo host, as the menu's old Training card did), back to HR for "you're hired" once it is
    /// finished, the character creator, then the HQ. Each step is saved (<see cref="OnboardingProgress"/>), so quitting
    /// resumes at it. A Steam invite accepted before being hired waits until the end, then joins that crew if it can.
    /// Off in batch mode and while the editor auto-hosts.
    /// </summary>
    public class OnboardingFlow : MonoBehaviour
    {
        private const string Home = "HQ";
        private const float SettleSeconds = 1.5f;

        private static OnboardingFlow instance;
        private static bool inOffice;
        private static ulong waitingLobby;

        private MenuUi menu;
        private bool hostAtHome, trainAtHome;
        private float homeSince = -1f;

        /// <summary>The flow exists (the game was started from the HQ, so the session object is up).</summary>
        public static bool Running => instance != null;

        /// <summary>The menus stay out of the way: the hiring owns the screen (no title menu behind the office).</summary>
        public static bool OwnsScreen => inOffice || (instance != null && instance.hostAtHome);

        private void Awake()
        {
            instance = this;
            menu = GetComponent<MenuUi>();
            SteamLobbyFlow.InviteGate = Defer;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (SteamLobbyFlow.InviteGate == (System.Func<ulong, bool>)Defer) SteamLobbyFlow.InviteGate = null;
        }

        private static bool Available => !Application.isBatchMode && Application.CanStreamedLevelBeLoaded(HrOffice.Scene);

        private void Update()
        {
            if (!Available || menu == null || menu.Bootstrap == null) return;
            NetworkBootstrap bootstrap = menu.Bootstrap;
            if (!bootstrap.IsRunning)
            {
                homeSince = -1f;
                if (hostAtHome && SceneManager.GetActiveScene().name == Home) Host(bootstrap);
                else if (!inOffice && !hostAtHome && OnboardingProgress.Pending && menu.Showing == MenuScreen.Main && !EarlyAccessNotice.Open) Begin();
                return;
            }
            hostAtHome = false;
            if (!OnboardingProgress.Pending) return;
            OnboardingStage stage = OnboardingProgress.Stage;
            if (stage == OnboardingStage.Interviewed)
            {
                if (TutorialMode.Active) OnboardingProgress.TrainingStarted();
                if (TutorialProgress.Done) OnboardingProgress.TrainingFinished();
            }
            CompanyService company = CompanyService.Current;
            bool home = company != null && company.IsSpawned && CompanyService.AtHome && !TutorialMode.Active && SessionTravel.LevelReady
                        && NetworkPlayer.Local != null && NetworkPlayer.Local.IsSpawned;
            if (!home || !company.IsServer)
            {
                homeSince = -1f;
                return;
            }
            if (homeSince < 0f) homeSince = Time.unscaledTime;
            if (Time.unscaledTime - homeSince < SettleSeconds || TutorialLaunch.Pending) return;
            homeSince = -1f;
            stage = OnboardingProgress.Stage;
            if (stage == OnboardingStage.Interviewed && CompanyService.TrainingAvailable) company.StartTraining();
            else if (stage != OnboardingStage.Interviewed)
            {
                // Back from training, or a host that skipped the title screen (the editor auto-hosts on Play): the
                // hiring's office scenes are offline, in the HR office.
                ReturnToMenu.Next = HrOffice.Scene;
                inOffice = true;
                bootstrap.Disconnect();
            }
        }

        private void Begin()
        {
            if (OnboardingProgress.Stage == OnboardingStage.Interviewed) GoTrain();
            else
            {
                inOffice = true;
                SceneManager.LoadScene(HrOffice.Scene);
            }
        }

        /// <summary>The office has loaded: play what this player has still to see there.</summary>
        public static void EnteredOffice(HrOffice office)
        {
            inOffice = true;
            if (instance == null) return;
            switch (OnboardingProgress.Stage)
            {
                case OnboardingStage.New:
                    office.PlayInterview(() =>
                    {
                        OnboardingProgress.Stage = OnboardingStage.Interviewed;
                        instance.GoTrain();
                    });
                    break;
                case OnboardingStage.Trained:
                    office.PlayHired(() => instance.Create(office));
                    break;
                case OnboardingStage.Hired when !OnboardingProgress.CreatorDone:
                    instance.Create(office);
                    break;
                default:
                    instance.GoHome();
                    break;
            }
        }

        private void Create(HrOffice office)
        {
            OnboardingProgress.Stage = OnboardingStage.Hired;
            menu.OpenCreator(office.Catalog, () =>
            {
                OnboardingProgress.CreatorDone = true;
                GoHome();
            });
        }

        private void GoTrain()
        {
            hostAtHome = trainAtHome = true;
            inOffice = false;
            SceneManager.LoadScene(Home);
        }

        private void GoHome()
        {
            hostAtHome = true;
            trainAtHome = false;
            inOffice = false;
            SceneManager.LoadScene(Home);
        }

        private void Host(NetworkBootstrap bootstrap)
        {
            hostAtHome = false;
            if (trainAtHome)
            {
                trainAtHome = false;
                TutorialLaunch.Request();
                bootstrap.StartHost();
                return;
            }
            ulong lobby = waitingLobby;
            waitingLobby = 0UL;
            SteamLobbyFlow flow = menu.Lobby != null ? menu.Lobby.Flow : null;
            if (lobby != 0UL && flow != null) JoinOrHost(flow, lobby, bootstrap);
            else bootstrap.StartHost();
        }

        private static async void JoinOrHost(SteamLobbyFlow flow, ulong lobby, NetworkBootstrap bootstrap)
        {
            bool joined = false;
            try { joined = await flow.JoinAsync(lobby); }
            catch (System.Exception e) { Debug.LogWarning($"[Onboarding] The invite's lobby couldn't be joined: {e.Message}"); }
            if (joined) return;
            ToastFeed.Show(Loc.T("THE CREW HAS GONE"), Loc.T("The crew that invited you has moved on. Your own HQ is ready."));
            if (!bootstrap.IsRunning) bootstrap.StartHost();
        }

        /// <summary>A Steam invite before being hired: remembered, joined once the hiring is done.</summary>
        private static bool Defer(ulong lobbyId)
        {
            if (!Available || !OnboardingProgress.Pending) return false;
            waitingLobby = lobbyId;
            ToastFeed.Show(Loc.T("INVITE SAVED"), Loc.T("You'll join your friends once HR has signed you on."));
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            inOffice = false;
            waitingLobby = 0UL;
            instance = null;
        }
    }
}
