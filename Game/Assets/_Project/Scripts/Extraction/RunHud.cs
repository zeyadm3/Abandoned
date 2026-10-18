using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Extraction
{
    /// <summary>Top-centre objective/extraction; bottom-right appraised cargo and carried load.</summary>
    public class RunHud : MonoBehaviour
    {
        private VisualElement mission, cargo, loadBar;
        private Label objective, clockTitle, clock, status, haul, weight;
        private void Update()
        {
            RunState run = RunState.Current;
            NetworkPlayer me = NetworkPlayer.Local;
            bool show = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed && me != null && !me.IsDead;
            if (mission == null || mission.panel == null)
            {
                if (!show || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(mission, show); MenuKit.Show(cargo, show);
            if (!show) return;
            RunNetState state = run.State;
            bool leaving = state.Phase == RunPhase.Honking;
            mission.EnableInClassList("field-mission--departing", leaving);
            string goal = RunGoals.Text();
            FieldHud.Set(objective, string.IsNullOrEmpty(goal) ? Loc.T("Locate and extract salvage") : goal);
            FieldHud.Set(clockTitle, leaving ? Loc.T("DEPARTING") : Loc.T("EXTRACT"));
            float remaining = leaving ? run.HonkRemaining : run.WindowRemaining;
            int seconds = Mathf.CeilToInt(remaining);
            FieldHud.Set(clock, $"{seconds / 60:00}:{seconds % 60:00}");
            bool aboard = TruckCargo.Current != null && TruckCargo.Current.Carries(me.Ragdoll.IsRagdolled ? me.Ragdoll.BodyPosition : me.transform.position);
            FieldHud.Set(status, aboard ? Loc.T("ABOARD / HOLD POSITION") : Loc.T("GET TO THE TRUCK"));
            MenuKit.Show(status, leaving);
            FieldHud.Set(haul, "$" + state.Haul.ToString("N0"));
            float load = me.Carrier.HandlingWeight;
            FieldHud.Set(weight, Loc.T("{0:0.#} kg", load));
            UiKit.SetBar(loadBar, me.Carrier.Config.WeightFraction(load));
        }
        private void Build()
        {
            HudLayer.Remove(mission); HudLayer.Remove(cargo);
            mission = HudLayer.Add(new VisualElement(), "field-mission");
            VisualElement row = FieldHud.Element(mission, "field-objective", "field-panel");
            FieldHud.Text(row, Loc.T("OBJECTIVE"), "field-objective__caption");
            objective = FieldHud.Text(row, "", "field-objective__text");
            row = FieldHud.Element(mission, "field-extraction", "field-panel");
            clockTitle = FieldHud.Text(row, Loc.T("EXTRACT"), "field-extraction__caption");
            clock = FieldHud.Text(row, "", "field-extraction__time");
            status = FieldHud.Text(mission, "", "field-departure-status");
            cargo = HudLayer.Add(new VisualElement(), "field-cargo", "field-panel");
            row = FieldHud.Element(cargo, "field-cargo__row");
            FieldHud.Text(row, Loc.T("CARGO"), "field-cargo__caption");
            haul = FieldHud.Text(row, "", "field-cargo__value");
            row = FieldHud.Element(cargo, "field-cargo__row", "field-cargo__row--last");
            weight = FieldHud.Text(row, "", "field-cargo__weight");
            loadBar = UiKit.Bar(row, "field-meter", "field-load");
        }
        private void OnDestroy() { HudLayer.Remove(mission); HudLayer.Remove(cargo); }
        public string HaulText => haul != null ? Loc.T("HAUL {0} {1}", haul.text, "") : null;
    }
}
