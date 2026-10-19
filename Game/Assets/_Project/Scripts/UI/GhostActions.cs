using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// The local ghost's hands (0.19.2): while spectating, E knocks, F makes the lights stutter, jump nudges
    /// a small object, each where the ghost camera looks (within 20 m). A line on screen says so.
    /// </summary>
    public class GhostActions : MonoBehaviour
    {
        private static GhostSpectator Ghost(out NetworkPlayer me)
        {
            me = NetworkPlayer.Local;
            return me != null && me.IsDead ? me.GetComponent<GhostSpectator>() : null;
        }

        private void Update()
        {
            GhostSpectator ghost = Ghost(out NetworkPlayer me);
            RunGhosts run = RunGhosts.Current;
            if (ghost == null || !ghost.Spectating || ghost.View == null || run == null)
            {
                HudCallout.Clear(this);
                return;
            }
            // 0.22.4: and the way back, when the agency will still send someone.
            string line = Core.Loc.T("{0}  knock      {1}  flicker the lights      {2}  nudge something",
                Core.InputBindings.Display("Interact"), Core.InputBindings.Display("Flashlight"), Core.InputBindings.Display("Jump"));
            if (RunHires.Current != null && RunHires.Current.HiresLeft) line += "      " + (Extraction.TruckCargo.BoatJob ? Core.Loc.T("a crewmate can hire you back at the boat's radio") : Core.Loc.T("a crewmate can hire you back at the truck's radio"));
            HudCallout.Set(this, line);
            if (Core.CursorOwner.UiActive) return;
            var reader = me.GetComponent<PlayerInputReader>();
            if (reader == null) return;
            PlayerInputFrame input = reader.Current;
            RunGhosts.Act? act = input.InteractPressed ? RunGhosts.Act.Knock : input.FlashlightPressed ? RunGhosts.Act.Flicker : input.JumpPressed ? RunGhosts.Act.Nudge : null;
            if (act == null) return;
            Transform view = ghost.View;
            Vector3 at = Physics.Raycast(view.position, view.forward, out RaycastHit hit, 20f, ~0, QueryTriggerInteraction.Ignore) ? hit.point : view.position + view.forward * 6f;
            run.Request(act.Value, at, view.forward);
        }
    }
}
