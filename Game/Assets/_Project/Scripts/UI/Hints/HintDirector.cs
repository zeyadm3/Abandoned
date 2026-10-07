using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Structure;
using Abandoned.Threats;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Onboarding (M8.2): watches what the local player is doing and shows each first-time tip once,
    /// at the moment it matters (the first loot in reach, the first heavy item, a cracking floor under
    /// your feet, a monster nearby...). Local and cosmetic; checks run a few times a second.
    /// </summary>
    public class HintDirector : MonoBehaviour
    {
        private const float CheckEvery = 0.25f, ThreatNoticeDistance = 18f, RunGoalDelay = 2f;

        private HintView view;
        private float nextCheck;
        private readonly RaycastHit[] hits = new RaycastHit[4];

        /// <summary>Tests: the tip on screen now.</summary>
        public HintId? Showing => view?.Showing;

        public static HintDirector Current { get; private set; }

        private void OnEnable() => Current = this;

        private void OnDisable()
        {
            if (Current == this) Current = null;
            view?.Hide();
        }

        private void OnDestroy() => view?.Remove();

        private void Update()
        {
            if (view == null || !view.Valid)
            {
                if (HudLayer.Root == null) return;
                view = new HintView();
            }
            view.Tick();
            if (!Hints.Enabled || Time.time < nextCheck) return;
            nextCheck = Time.time + CheckEvery;
            if (view.Showing != null) return; // one tip at a time; the next waits its turn
            if (Next() is HintId id) Show(id);
        }

        /// <summary>Shows a tip now if it hasn't been seen (tests and direct triggers).</summary>
        public bool Show(HintId id)
        {
            if (view == null || !view.Valid || Hints.Seen(id)) return false;
            Hints.MarkSeen(id);
            view.Show(id, Hints.Text(id));
            Audio.GameAudio.PlayUi(Audio.SoundId.UiOpen, 0.5f);
            return true;
        }

        // The most pressing unseen tip for this moment, or null.
        private HintId? Next()
        {
            NetworkPlayer me = NetworkPlayer.Local;
            if (me == null || !me.IsSpawned) return null;
            if (me.IsDead) return Unseen(HintId.Ghost);

            RunState run = RunState.Current;
            bool inRun = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed;
            if (!inRun)
            {
                // HQ tips only where there's a board to read (the company lives on in every level).
                CompanyService company = CompanyService.Current;
                if (company == null || !company.IsSpawned || !AtHq()) return null;
                return company.Selected < 0 ? Unseen(HintId.HqBoard) : Unseen(HintId.HqVan);
            }

            if (run.State.Phase == RunPhase.Honking && Unseen(HintId.TruckLeaving) is HintId leaving) return leaving;
            if (OnCrackingFloor(me.transform.position) && Unseen(HintId.FloorCracking) is HintId cracking) return cracking;
            if (ThreatNear<BlindOne>(me) && Unseen(HintId.BlindOne) is HintId blind) return blind;
            if (ThreatNear<Stalker>(me) && Unseen(HintId.Stalker) is HintId stalker) return stalker;
            if (ThreatNear<Collector>(me) && Unseen(HintId.Collector) is HintId collector) return collector;
            if (run.Elapsed > RunGoalDelay && Unseen(HintId.RunGoal) is HintId goal) return goal;
            if (run.State.PowerOff && Unseen(HintId.PowerOff) is HintId power) return power;

            PlayerCarrier carrier = me.Carrier;
            var interactor = me.GetComponent<PlayerInteractor>();
            if (carrier != null && carrier.Inventory.Count > 0 && Unseen(HintId.Pockets) is HintId pockets) return pockets;
            if (carrier != null && carrier.Held != null && carrier.Held.Shared == null && !carrier.IsDragging && Unseen(HintId.Throw) is HintId toss) return toss;
            if (interactor != null && interactor.Target != null)
            {
                bool heavy = interactor.Target.Shared != null || interactor.Target.CarryClass >= CarryClass.Heavy;
                if (heavy && Unseen(HintId.Heavy) is HintId big) return big;
                if (Unseen(HintId.PickUp) is HintId pick) return pick;
            }
            return null;
        }

        private static HintId? Unseen(HintId id) => Hints.Seen(id) ? null : id;

        private ContractBoard board;
        private int boardScene = -1;

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

        private bool OnCrackingFloor(Vector3 feet)
        {
            int mask = 1 << Mathf.Max(0, GameLayers.StructureLayer);
            int count = Physics.RaycastNonAlloc(feet + Vector3.up * 0.3f, Vector3.down, hits, 1.5f, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                StructuralSection s = hits[i].collider.GetComponentInParent<StructuralSection>();
                if (s != null && s.Stage >= StructuralStage.Cracking && s.Stage != StructuralStage.Collapsed) return true;
            }
            return false;
        }

        private static bool ThreatNear<T>(NetworkPlayer me) where T : Threat
        {
            foreach (Threat t in Threat.All)
                if (t is T && t != null && Vector3.Distance(t.transform.position, me.transform.position) < ThreatNoticeDistance) return true;
            return false;
        }
    }
}
