using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>The building's total collapse (BuildingDecay): its clock for every machine, and the lost job.</summary>
    public partial class RunState
    {
        /// <summary>Host: the total collapse killed someone; if nobody is left, the job is lost outright.</summary>
        public bool Crushed { get; private set; }

        /// <summary>Every machine: the total collapse is under way.</summary>
        public bool TotalCollapse => State.CollapseAt > 0d;

        /// <summary>Every machine: seconds until the building is down (0 when it is, or before it starts).</summary>
        public float CollapseRemaining => TotalCollapse ? Mathf.Max(0f, (float)(State.CollapseAt - Now)) : 0f;

        /// <summary>Host: start the total collapse; it is down <paramref name="seconds"/> from now.</summary>
        public void ServerStartTotalCollapse(float seconds)
        {
            if (!IsServer || TotalCollapse) return;
            RunNetState s = State;
            s.CollapseAt = Now + Mathf.Max(1f, seconds);
            state.Value = s;
            Debug.Log($"[Run] Total collapse: the building is down in {seconds:0} s.");
        }

        /// <summary>Host: someone was still inside when it came down.</summary>
        public void ServerMarkCrushed()
        {
            if (IsServer) Crushed = true;
        }
    }
}
