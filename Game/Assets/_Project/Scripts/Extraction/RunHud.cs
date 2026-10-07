using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Placeholder run HUD (OnGUI until the UI milestone): haul vs quota so "one more floor" always has a
    /// number (GDD 10), cargo space, the extraction window, and the truck's departure countdown.
    /// </summary>
    public class RunHud : MonoBehaviour
    {
        [SerializeField] private int fontSize = 18;

        private GUIStyle style, big;

        private void OnGUI()
        {
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, alignment = TextAnchor.UpperCenter, richText = true };
            big ??= new GUIStyle(style) { fontSize = fontSize * 2, fontStyle = FontStyle.Bold };

            RunNetState s = run.State;
            if (s.Phase == RunPhase.Departed) return;
            string haulColor = s.Haul >= s.Quota ? "#7dff7d" : "#ffffff";
            string cargo = s.Overloaded ? $"<color=#ff7766>Cargo {s.CargoVolume:0.0}/{s.CargoCapacity:0} m³ OVERLOADED</color>"
                : $"Cargo {s.CargoVolume:0.0}/{s.CargoCapacity:0} m³";
            float w = run.WindowRemaining;
            string window = run.WindowClosed ? "<color=#ff7766>WINDOW CLOSED - DANGER RISING</color>" : $"Window {(int)w / 60}:{(int)w % 60:00}";
            string line = $"<color={haulColor}>Haul ${s.Haul:N0}</color> / Quota ${s.Quota:N0}     {cargo}     {window}";
            GUI.Label(new Rect(0f, 8f, Screen.width, 28f), line, style);

            Networking.NetworkPlayer me = Networking.NetworkPlayer.Local;
            if (me != null && me.IsDead)
                GUI.Label(new Rect(0f, Screen.height * 0.4f, Screen.width, 60f), "<color=#ff5544>YOU DIED</color>", big);

            if (s.Phase == RunPhase.Honking)
                GUI.Label(new Rect(0f, Screen.height * 0.25f, Screen.width, 60f),
                    $"<color=#ffd24d>TRUCK LEAVES IN {Mathf.CeilToInt(run.HonkRemaining)} - GET IN!</color>", big);
        }
    }
}
