using Abandoned.Company;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck's start lever (GDD 10). Below quota it wants a second pull to be sure; while the truck honks,
    /// pulling it again stops the departure (QA B-09). Asks the host through the run state.
    /// </summary>
    public class TruckIgnition : MonoBehaviour, IUsable
    {
        private const float ConfirmSeconds = 4f;

        // This machine only: the first pull below quota arms the confirmation.
        private static float confirmUntil;

        private static bool Confirming => Time.time < confirmUntil;

        public string UsePrompt(GameObject user)
        {
            RunState run = RunState.Current;
            if (run == null) return null;
            RunNetState s = run.State;
            if (s.Phase == RunPhase.Honking)
                return run.CanCancel ? Loc.T("Stop the truck (leaving in {0:0} s)", run.HonkRemaining) : null;
            if (s.Phase != RunPhase.Running) return null;
            if (s.Overloaded) return Loc.T("Overloaded ({0:0.0}/{1:0} m³) - unload something", s.CargoVolume, s.CargoCapacity);
            if (s.Haul < s.Quota && Confirming)
                return Loc.T("BELOW QUOTA (${0:N0} / ${1:N0}): pull again to leave anyway", s.Haul, s.Quota);
            return Loc.T("Start the truck: it leaves in {0:0} s (haul ${1:N0} / quota ${2:N0})", run.HonkSeconds, s.Haul, s.Quota);
        }

        public void Use(GameObject user)
        {
            RunState run = RunState.Current;
            if (run == null) return;
            RunNetState s = run.State;
            if (s.Phase == RunPhase.Honking)
            {
                run.RequestCancelDepart();
                return;
            }
            if (s.Phase != RunPhase.Running) return;
            if (s.Haul < s.Quota && !s.Overloaded && !Confirming)
            {
                confirmUntil = Time.time + ConfirmSeconds;
                Audio.GameAudio.PlayUi(Audio.SoundId.UiError, 0.6f);
                return;
            }
            confirmUntil = 0f;
            run.RequestDepart();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => confirmUntil = 0f;
    }
}
