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
        private GUIStyle style;

        private static GhostSpectator Ghost(out NetworkPlayer me)
        {
            me = NetworkPlayer.Local;
            return me != null && me.IsDead ? me.GetComponent<GhostSpectator>() : null;
        }

        private void Update()
        {
            GhostSpectator ghost = Ghost(out NetworkPlayer me);
            RunGhosts run = RunGhosts.Current;
            if (ghost == null || !ghost.Spectating || ghost.View == null || run == null || Core.CursorOwner.UiActive) return;
            var reader = me.GetComponent<PlayerInputReader>();
            if (reader == null) return;
            PlayerInputFrame input = reader.Current;
            RunGhosts.Act? act = input.InteractPressed ? RunGhosts.Act.Knock : input.FlashlightPressed ? RunGhosts.Act.Flicker : input.JumpPressed ? RunGhosts.Act.Nudge : null;
            if (act == null) return;
            Transform view = ghost.View;
            Vector3 at = Physics.Raycast(view.position, view.forward, out RaycastHit hit, 20f, ~0, QueryTriggerInteraction.Ignore) ? hit.point : view.position + view.forward * 6f;
            run.Request(act.Value, at, view.forward);
        }

        private void OnGUI()
        {
            GhostSpectator ghost = Ghost(out _);
            if (ghost == null || !ghost.Spectating || RunGhosts.Current == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerCenter };
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.019f);
            GUI.color = new Color(0.75f, 0.85f, 1f, 0.85f);
            GUI.Label(new Rect(0, Screen.height * 0.8f, Screen.width, Screen.height * 0.05f),
                $"{Core.InputBindings.Display("Interact")}: knock    {Core.InputBindings.Display("Flashlight")}: flicker the lights    {Core.InputBindings.Display("Jump")}: nudge something", style);
            GUI.color = Color.white;
        }
    }
}
