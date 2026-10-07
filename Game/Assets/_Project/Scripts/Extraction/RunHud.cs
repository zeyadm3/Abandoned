using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The run HUD (M8.1): haul vs quota along the top so "one more floor" always has a number (GDD 10),
    /// cargo space and the extraction window beside it; big banners for the truck's departure countdown
    /// and for your own death.
    /// </summary>
    public class RunHud : MonoBehaviour
    {
        private static readonly Color GainColor = new(0.5f, 1f, 0.5f);

        private VisualElement top;
        private Label haul, cargo, window, leaving, died;
        private RunState shownRun;
        private int shownHaul;

        private void Update()
        {
            RunState run = RunState.Current;
            bool showing = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed;
            if (top == null)
            {
                if (!showing || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(top, showing);
            if (!showing)
            {
                MenuKit.Show(leaving, false);
                MenuKit.Show(died, false);
                return;
            }

            RunNetState s = run.State;
            HaulGain(run, s.Haul);
            SetText(haul, $"HAUL ${s.Haul:N0} / ${s.Quota:N0}");
            haul.EnableInClassList("hud-chip--good", s.Haul >= s.Quota);
            SetText(cargo, s.Overloaded ? $"CARGO {s.CargoVolume:0.0}/{s.CargoCapacity:0} M3 - OVERLOADED" : $"CARGO {s.CargoVolume:0.0}/{s.CargoCapacity:0} M3");
            cargo.EnableInClassList("hud-chip--bad", s.Overloaded);
            float w = run.WindowRemaining;
            SetText(window, run.WindowClosed ? "WINDOW CLOSED - DANGER RISING" : $"WINDOW {(int)w / 60}:{(int)w % 60:00}");
            window.EnableInClassList("hud-chip--bad", run.WindowClosed);

            bool honking = s.Phase == RunPhase.Honking;
            MenuKit.Show(leaving, honking);
            if (honking) SetText(leaving, $"TRUCK LEAVES IN {Mathf.CeilToInt(run.HonkRemaining)} - GET IN!");
            Networking.NetworkPlayer me = Networking.NetworkPlayer.Local;
            MenuKit.Show(died, me != null && me.IsDead);
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
            top = HudLayer.Add(new VisualElement(), "hud-top");
            haul = Chip();
            cargo = Chip();
            window = Chip();
            leaving = HudLayer.Label("hud-banner");
            died = HudLayer.Label("hud-banner", "hud-banner--dead");
            died.text = "YOU DIED";
        }

        private Label Chip()
        {
            var chip = new Label { pickingMode = PickingMode.Ignore };
            chip.AddToClassList("hud-chip");
            top.Add(chip);
            return chip;
        }

        // Labels re-layout on every text set; only touch them when the text changed.
        private static void SetText(Label label, string text)
        {
            if (label.text != text) label.text = text;
        }

        private void OnDestroy()
        {
            HudLayer.Remove(top);
            HudLayer.Remove(leaving);
            HudLayer.Remove(died);
        }

        /// <summary>Tests: what the top bar says.</summary>
        public string HaulText => haul != null ? haul.text : null;
    }
}
