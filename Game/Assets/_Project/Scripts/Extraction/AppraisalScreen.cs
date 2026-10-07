using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// After the truck leaves (PLAYBOOK 5.4, placeholder OnGUI): every item with its starting value,
    /// what damage cost and what it's worth now; haul vs quota; who made it out; the run's funny stats.
    /// The host starts the next run from here; everyone else waits for them.
    /// </summary>
    public class AppraisalScreen : MonoBehaviour
    {
        [SerializeField] private RunDirector director;
        [SerializeField] private int fontSize = 15;

        private GUIStyle box, label, title, right;
        private Vector2 scroll;

        public bool Showing => RunState.Current != null && RunState.Current.State.Phase == RunPhase.Departed && RunState.Current.Results != null;

        // The appraisal owns the mouse while it's up, so a click is a click on the button.
        private void Update() => Core.CursorOwner.UiActive = Showing;

        private void OnDisable() => Core.CursorOwner.UiActive = false;

        private void OnGUI()
        {
            if (!Showing) return;
            RunState run = RunState.Current;
            RunResults r = run.Results;
            box ??= new GUIStyle(GUI.skin.box) { fontSize = fontSize };
            label ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };
            title ??= new GUIStyle(label) { fontSize = fontSize + 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            right ??= new GUIStyle(label) { alignment = TextAnchor.MiddleRight };

            float w = Mathf.Min(760f, Screen.width - 40f), h = Mathf.Min(620f, Screen.height - 40f);
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), box);
            GUILayout.Label("APPRAISAL", title);
            string verdict = r.QuotaMet ? "<color=#7dff7d>QUOTA MET</color>" : "<color=#ff7766>QUOTA MISSED</color>";
            GUILayout.Label($"Haul <b>${r.Haul:N0}</b> / quota ${r.Quota:N0}   {verdict}   ({(int)r.Seconds / 60}:{(int)r.Seconds % 60:00})", title);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(h * 0.45f));
            Row("<b>Item</b>", "<b>Found at</b>", "<b>Damage</b>", "<b>Worth now</b>");
            foreach (RunResults.Item item in r.Items)
                Row(item.Name + (item.Pocketed ? " (pocket)" : ""), $"${item.StartValue:N0}",
                    item.DamageLost > 0 ? $"<color=#ff7766>-${item.DamageLost:N0}</color>" : "-", $"${item.FinalValue:N0}");
            if (r.Items.Length == 0) GUILayout.Label("The truck left empty.", label);
            GUILayout.EndScrollView();

            foreach (RunResults.Player p in r.Players)
                GUILayout.Label(p.Extracted ? $"{p.Name}: made it out" : p.Died ? $"<color=#ff7766>{p.Name}: died in there</color>" :
                    $"<color=#ff7766>{p.Name}: left behind{(p.PocketValueLost > 0 ? $" (lost ${p.PocketValueLost:N0} in their pockets)" : "")}</color>", label);
            foreach (string line in r.Stats) GUILayout.Label("• " + line, label);

            GUILayout.FlexibleSpace();
            if (director != null && run.IsServer)
            {
                if (GUILayout.Button("Next run", GUILayout.Height(40f))) director.StartNextRun();
            }
            else GUILayout.Label("Waiting for the host to start the next run...", label);
            GUILayout.EndArea();
        }

        private void Row(string name, string found, string damage, string now)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, label, GUILayout.Width(300f));
            GUILayout.Label(found, right, GUILayout.Width(130f));
            GUILayout.Label(damage, right, GUILayout.Width(130f));
            GUILayout.Label(now, right, GUILayout.Width(130f));
            GUILayout.EndHorizontal();
        }
    }
}
