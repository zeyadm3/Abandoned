using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>One press starts departure; only its starter can cancel by pressing the ignition again.</summary>
    public class TruckIgnition : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user)
        {
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return null;
            if (run.State.Phase == RunPhase.Honking)
                return !run.CanCancel ? null : TruckCargo.BoatJob
                    ? Loc.T("Stop the boat (casting off in {0:0} s)", run.HonkRemaining)
                    : Loc.T("Stop the truck (leaving in {0:0} s)", run.HonkRemaining);
            if (run.State.Phase != RunPhase.Running) return null;
            if (run.State.Overloaded) return Loc.T("Overloaded ({0:0.0}/{1:0} m³) - unload something", run.State.CargoVolume, run.State.CargoCapacity);
            return Loc.T("Start departure · {0:0} seconds", run.HonkSeconds);
        }
        public void Use(GameObject user)
        {
            RunState run = RunState.Current;
            if (run == null) return;
            if (run.State.Phase == RunPhase.Honking) { if (run.CanCancel) run.RequestCancelDepart(); }
            else if (run.State.Phase == RunPhase.Running) run.RequestDepart();
        }
    }
}
