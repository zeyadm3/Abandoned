using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The guided first job (tester feedback, 2026-10-09: tips that show once for nine seconds didn't teach the
    /// loop). One instruction at a time, from the contract board to the truck's lever; each step ends when this
    /// player actually does it, and the two that only explain something end after a read. Local and cosmetic,
    /// on the session object; it runs once per player and follows the Tips setting. The host side of the first
    /// job (the first monster holds off) is <see cref="RunState.RunTerms.FirstJob"/>.
    /// </summary>
    public class FirstDayGuide : MonoBehaviour
    {
        private enum Step { Board, Van, Light, Scan, Grab, Truck, Floors, Quiet, Quota, Lever, Count }

        private const float CheckEvery = 0.25f, ReadSeconds = 12f, LightGrace = 25f;

        private VisualElement box, progress;
        private Label title, text;
        private Step step;
        private float nextCheck, stepSince, runSince;
        private bool wasInRun;
        private ContractBoard board;
        private int boardScene = -1;

        private void Awake()
        {
            // Never in batch runs (tests, nettests, captures): it would sit over the screens they check.
            if (Application.isBatchMode) enabled = false;
        }

        private void Update()
        {
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + CheckEvery;
            NetworkPlayer me = NetworkPlayer.Local;
            if (FirstDay.Done || !Hints.Enabled || me == null || !me.IsSpawned || HudLayer.Root == null)
            {
                Show(false);
                return;
            }
            RunState run = RunState.Current;
            bool spawned = run != null && run.IsSpawned;
            bool inRun = spawned && run.State.Phase != RunPhase.Departed;
            if (inRun && !wasInRun) runSince = Time.time;
            wasInRun = inRun;
            CompanyService company = CompanyService.Current;

            // The truck is already leaving (someone pulled the lever): the day is over, taught or not.
            if (spawned && run.State.Phase != RunPhase.Running && step >= Step.Light) step = Step.Lever;
            bool advanced = false;
            while (step < Step.Count && Complete(step, me, run, spawned, inRun, company))
            {
                step++;
                stepSince = Time.time;
                advanced = true;
            }
            if (step >= Step.Count)
            {
                FirstDay.MarkDone();
                Show(false);
                ToastFeed.Show("FIRST DAY DONE", "That's the job. Tips can be replayed from Settings > Gameplay.");
                return;
            }
            bool visible = !me.IsDead && (step <= Step.Van ? !inRun && company != null && company.IsSpawned && AtHq() : inRun);
            Show(visible);
            if (!visible) return;
            if (advanced) GameAudio.PlayUi(SoundId.UiConfirm, 0.4f);
            Set(title, $"FIRST DAY  {(int)step + 1}/{(int)Step.Count}");
            Set(text, TextFor(company));
            UiKit.SetSegments(progress, (float)step / (int)Step.Count);
        }

        private bool Complete(Step s, NetworkPlayer me, RunState run, bool spawned, bool inRun, CompanyService company)
        {
            switch (s)
            {
                case Step.Board: return inRun || company != null && company.IsSpawned && company.Selected >= 0;
                case Step.Van: return inRun;
                // Never stuck on a crew member without a flashlight: the step lets go after a while.
                case Step.Light: return inRun && (LightOn() || Time.time - runSince > LightGrace);
                case Step.Scan: return inRun && LootTags.ValuesVisible;
                case Step.Grab: return inRun && me.Carrier != null && (me.Carrier.Held != null || me.Carrier.Inventory.Count > 0);
                case Step.Truck: return inRun && run.State.Haul > 0;
                case Step.Floors:
                case Step.Quiet: return inRun && Time.time - stepSince > ReadSeconds;
                case Step.Quota: return inRun && (run.State.Haul >= run.State.Quota || run.State.Phase == RunPhase.Honking);
                case Step.Lever: return spawned && run.State.Phase != RunPhase.Running;
                default: return true;
            }
        }

        private string TextFor(CompanyService company)
        {
            string K(string action) => InputBindings.Display(action);
            switch (step)
            {
                case Step.Board:
                    return company != null && !company.IsServer
                        ? "Wait for the host to take a job from the contract board."
                        : $"Walk to the contract board, press {K("Interact")} and take a job.";
                case Step.Van: return $"Job taken. Get in the van ({K("Interact")}) to drive there. Gear and the monster files are here at the HQ.";
                case Step.Light: return $"Turn your flashlight on ({K("Flashlight")}). Health and breath are bottom left; the compass is up top.";
                case Step.Scan: return $"Scan ({K("Scan")}) to see what things are worth and how much you have hauled.";
                case Step.Grab: return $"Pick something up ({K("Interact")}). Heavy things slow you down; fragile things lose value when they hit anything.";
                case Step.Truck: return $"Carry it out to the truck in the loading bay and put it in the cargo bay ({K("Drop")} drops it).";
                case Step.Floors: return "Floors have a load limit. Creaks and cracks mean too much weight: spread out, or get off before it gives way.";
                case Step.Quiet: return $"You are not alone in here. Noise draws things: crouch ({K("Crouch")}) and don't run when something is close.";
                case Step.Quota: return "Keep hauling until the quota is met. The truck's board shows it, and so does a scan.";
                default: return "Pull the yellow lever on the truck to leave, and stay in the cargo bay until it goes.";
            }
        }

        private static bool LightOn()
        {
            foreach (PlayerEquipment e in PlayerEquipment.All) if (e != null && e.IsOwner) return e.LightOn;
            return false;
        }

        // The HQ is wherever there's a contract board (the company lives on in every level).
        private bool AtHq()
        {
            int scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
            if (scene != boardScene)
            {
                boardScene = scene;
                board = FindAnyObjectByType<ContractBoard>();
            }
            return board != null;
        }

        private void Show(bool visible)
        {
            FirstDay.Active = visible;
            if (!visible)
            {
                if (box != null) MenuKit.Show(box, false);
                return;
            }
            if (box == null || box.panel == null)
            {
                HudLayer.Remove(box);
                box = HudLayer.Add(new VisualElement(), "hud-panel", "hud-hint", "first-day");
                title = MenuKit.Text(box, "FIRST DAY", "section");
                title.pickingMode = PickingMode.Ignore;
                text = MenuKit.Text(box, "", "text");
                text.pickingMode = PickingMode.Ignore;
                text.style.whiteSpace = WhiteSpace.Normal;
                progress = UiKit.Segments(box, (int)Step.Count);
            }
            MenuKit.Show(box, true);
        }

        private static void Set(Label label, string value)
        {
            if (label != null && label.text != value) label.text = value;
        }

        private void OnDisable()
        {
            FirstDay.Active = false;
            if (box != null) MenuKit.Show(box, false);
        }

        private void OnDestroy() => HudLayer.Remove(box);
    }
}
