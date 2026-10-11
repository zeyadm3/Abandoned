using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A scan reveals haul and quota; the compact clock and departure countdown remain readable without
    /// covering exploration. The truck's own haul board is always available at the loading bay.
    /// </summary>
    public class RunHud : MonoBehaviour
    {
        private static readonly Color GainColor = new(0.76f, 0.65f, 0.45f);

        private VisualElement top, haulBar, clock, leaving;
        private Label amount, quota, cargo, met, time, leavingCount, leavingStatus, goals, clockState, haulState;
        private RunState shownRun;
        private int shownHaul;
        private bool metShown;

        private void Update()
        {
            RunState run = RunState.Current;
            bool showing = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed;
            if (top == null || top.panel == null)
            {
                if (!showing || HudLayer.Root == null) return;
                Build();
            }
            // The inspection ledger and tool readouts share this corner; the haul returns when it closes.
            bool inspecting = HudLayer.TryLocalRect(HudLayer.Root?.Q(className: "h-inspection"), HudLayer.Root, out _);
            MenuKit.Show(top, showing && LootTags.ValuesVisible && !inspecting);
            MenuKit.Show(clock, showing && LootTags.ValuesVisible && !inspecting);
            if (HudLayer.Root != null)
                top.EnableInClassList("h-recovery--compact", Mathf.Min(340f, HudLayer.Root.layout.width * .28f) < 320f);
            if (!showing)
            {
                MenuKit.Show(leaving, false);
                return;
            }

            RunNetState s = run.State;
            HaulGain(run, s.Haul);
            bool quotaMet = s.Haul >= s.Quota;
            SetText(haulState, quotaMet ? "TARGET RECOVERED" : "RECOVERY TARGET");
            SetText(amount, $"${s.Haul:N0}");
            SetText(quota, $"/ ${s.Quota:N0}");
            UiKit.SetBar(haulBar, s.Quota > 0 ? (float)s.Haul / s.Quota : 1f, quotaMet ? "good" : null);
            if (quotaMet != metShown)
            {
                metShown = quotaMet;
                met.EnableInClassList("run-haul__met--on", quotaMet);
                if (quotaMet && LootTags.ValuesVisible) UiKit.Jolt(met);
            }
            MenuKit.Show(met, quotaMet);
            SetText(cargo, s.Overloaded ? $"CARGO / {s.CargoVolume:0.0} : {s.CargoCapacity:0} m\u00b3 / OVERLOADED" : $"CARGO / {s.CargoVolume:0.0} : {s.CargoCapacity:0} m\u00b3");
            cargo.EnableInClassList("run-haul__cargo--bad", s.Overloaded);
            string goalText = RunGoals.Text();
            SetText(goals, goalText);
            MenuKit.Show(goals, goalText.Length > 0);
            float w = run.WindowRemaining;
            SetText(time, run.WindowClosed ? "+00:00" : $"{(int)w / 60:00}:{(int)w % 60:00}");
            SetText(clockState, run.WindowClosed ? "RECOVERY WINDOW CLOSED" : "RECOVERY WINDOW");
            clock.EnableInClassList("run-clock--late", run.WindowClosed);

            // The dead don't need the countdown (their death card and ghost bar have the screen).
            Networking.NetworkPlayer local = Networking.NetworkPlayer.Local;
            bool honking = s.Phase == RunPhase.Honking && (local == null || !local.IsDead);
            MenuKit.Show(leaving, honking);
            if (honking) Leaving(run);
        }

        // The truck is going: how long, and whether you're on it (the death card covers the dead).
        private void Leaving(RunState run)
        {
            SetText(leavingCount, Mathf.CeilToInt(run.HonkRemaining).ToString());
            Networking.NetworkPlayer me = Networking.NetworkPlayer.Local;
            bool aboard = me != null && TruckCargo.Current != null &&
                          TruckCargo.Current.Carries(me.Ragdoll.IsRagdolled ? me.Ragdoll.BodyPosition : me.transform.position);
            bool dead = me == null || me.IsDead;
            SetText(leavingStatus, dead ? "" : aboard ? "ABOARD / HOLD POSITION" : "GET TO THE TRUCK");
            leavingStatus.EnableInClassList("truck-banner__status--good", aboard);
            MenuKit.Show(leavingStatus, !dead);
        }

        // Loot landing in the truck: "+$X" over the cargo bay and the till (every machine).
        private void HaulGain(RunState run, int haulNow)
        {
            if (run != shownRun)
            {
                shownRun = run;
                shownHaul = haulNow;
                return;
            }
            if (haulNow > shownHaul && TruckCargo.Current != null)
            {
                Vector3 at = TruckCargo.Current.transform.position + Vector3.up * 2.6f;
                FloatingText.Show(at, $"+${haulNow - shownHaul:N0}", GainColor);
                Audio.GameAudio.Play(Audio.SoundId.Coins, at, 0.6f);
            }
            shownHaul = haulNow;
        }

        private void Build()
        {
            HudLayer.Remove(top);
            HudLayer.Remove(clock);
            HudLayer.Remove(leaving);
            top = HudLayer.Add(new VisualElement(), "run-haul", "h-recovery");
            haulState = Text(top, "RECOVERY TARGET", "h-instrument-title");
            VisualElement row = new() { pickingMode = PickingMode.Ignore };
            row.AddToClassList("run-haul__row");
            top.Add(row);
            amount = Text(row, "", "run-haul__amount");
            quota = Text(row, "", "run-haul__quota");
            haulBar = UiKit.Bar(top, "run-haul__bar");
            cargo = Text(top, "", "run-haul__cargo");
            goals = Text(top, "", "h-recovery__goals"); // 0.18+: boss request, rush order, sets, overtime
            met = UiKit.Tag(top, "QUOTA MET / RETURN TO THE TRUCK");
            met.AddToClassList("run-haul__met");
            clock = new VisualElement { pickingMode = PickingMode.Ignore };
            clock.AddToClassList("run-clock");
            clock.AddToClassList("h-window");
            top.Add(clock);
            clockState = Text(clock, "RECOVERY WINDOW", "h-micro");
            time = Text(clock, "", "run-clock__time");
            leaving = HudLayer.Add(new VisualElement(), "truck-banner", "h-departure");
            Text(leaving, "EVACUATION", "h-instrument-title");
            Text(leaving, "THE TRUCK IS LEAVING", "truck-banner__title");
            leavingCount = Text(leaving, "", "truck-banner__count");
            Text(leaving, "SECONDS TO DEPARTURE", "h-micro");
            leavingStatus = Text(leaving, "", "truck-banner__status");
        }

        private static Label Text(VisualElement parent, string text, string cls)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(cls);
            parent.Add(label);
            return label;
        }

        // Labels re-layout on every text set; only touch them when the text changed.
        private static void SetText(Label label, string text)
        {
            if (label.text != text) label.text = text;
        }

        private void OnDestroy()
        {
            HudLayer.Remove(top);
            HudLayer.Remove(clock);
            HudLayer.Remove(leaving);
        }

        /// <summary>Tests: what the top bar says.</summary>
        public string HaulText => amount != null ? $"HAUL {amount.text} {quota.text}" : null;
    }
}
