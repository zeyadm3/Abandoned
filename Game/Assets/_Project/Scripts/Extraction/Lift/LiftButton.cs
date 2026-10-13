using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A freight lift button (0.16.3): one at each landing calls the lift there; the one on the deck sends
    /// it to the other floor. The prompt says when it has no power and when the deck is overloaded.
    /// </summary>
    public class LiftButton : MonoBehaviour, IUsable, IHintSource
    {
        [Tooltip("The landing this button calls the lift to (0 ground, 1 upper), or -1 on the deck: the other floor.")]
        [SerializeField, Range(-1, 1)] private int floor = -1;

        public HintId Hint => HintId.Lift;

        public string UsePrompt(GameObject user)
        {
            RunLift run = RunLift.Current;
            FreightLift lift = FreightLift.Active;
            if (run == null || lift == null || !run.IsSpawned) return null;
            LiftNetState s = run.State;
            if (s.Broken) return Loc.T("The lift has given way");
            if (run.Moving) return Loc.T("The lift is moving");
            if (!run.Powered(lift)) return Loc.T("No power to the lift (throw its breaker)");
            int target = Target(s);
            if (target < 0) return Loc.T("The lift is here");
            float load = RunLift.LoadOn(lift), limit = LiftConfig.Current.LoadLimit;
            if (floor >= 0) return Loc.T("Call the lift");
            string send = target == 1 ? Loc.T("Send the lift up") : Loc.T("Send the lift down");
            return load > limit ? Loc.T("{0}  OVERLOADED {1:0} / {2:0} kg", send, load, limit) : Loc.T("{0}  {1:0} / {2:0} kg", send, load, limit);
        }

        // The floor this press would send the lift to, or -1 when it's already there.
        private int Target(LiftNetState s)
        {
            int here = s.Start > 0d ? s.To : s.From;
            int to = floor < 0 ? 1 - here : floor;
            return to == here ? -1 : to;
        }

        public void Use(GameObject user)
        {
            RunLift run = RunLift.Current;
            if (run == null) return;
            int target = Target(run.State);
            if (target >= 0) run.RequestSend(target);
        }

#if UNITY_EDITOR
        public void EditorSetup(int landing) => floor = landing;
#endif
    }
}
