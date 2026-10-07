using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>The truck's start lever (GDD 10: anyone can start the truck). Asks the host through the run state.</summary>
    public class TruckIgnition : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user)
        {
            RunState run = RunState.Current;
            if (run == null) return null;
            RunNetState s = run.State;
            if (s.Phase != RunPhase.Running) return null;
            if (s.Overloaded) return $"Overloaded ({s.CargoVolume:0.0}/{s.CargoCapacity:0} m³) - unload something";
            return $"Start the truck: it leaves in {run.Config.HonkSeconds:0} s (haul ${s.Haul:N0} / quota ${s.Quota:N0})";
        }

        public void Use(GameObject user) => RunState.Current?.RequestDepart();
    }
}
