using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The training job's lessons as everyone sees them, on the run state's object (inert in every other level).
    /// Host: which lesson the crew is on, judging the world for it (items, floors, doors, the truck), running each
    /// lesson's scripted beat, handing out kit-crate gear, and skipping or resetting on the host's word. Each
    /// player's own machine judges the personal lessons (look, sprint, scan) and reports them. The crew moves
    /// through the lessons together.
    /// </summary>
    public partial class RunTraining : NetworkBehaviour
    {
        private const float CheckEvery = 0.2f;

        private readonly NetworkVariable<int> step = new(-1);
        private readonly NetworkVariable<double> stepStarted = new();
        private readonly NetworkVariable<bool> finished = new();

        private readonly HashSet<ulong> reported = new();
        private RunState run;
        private float nextCheck;

        public static RunTraining Current { get; private set; }

        /// <summary>The lesson index everyone is on (the catalog's count once training is complete).</summary>
        public int Step => step.Value;
        public bool Finished => finished.Value;
        public TutorialCatalog Catalog => TutorialCatalog.Current;
        public TutorialStepDefinition CurrentStep => Catalog.At(step.Value);
        public double Now => NetworkManager != null ? NetworkManager.ServerTime.Time : 0d;
        /// <summary>Seconds into the current lesson (server time, the same on every machine).</summary>
        public float StepElapsed => Mathf.Max(0f, (float)(Now - stepStarted.Value));
        public RunState Run => run;

        /// <summary>Every machine: the crew moved to another lesson (the new index).</summary>
        public static event System.Action<int> StepChanged;

        public override void OnNetworkSpawn()
        {
            if (!TutorialMode.Active)
            {
                enabled = false;
                return;
            }
            Current = this;
            run = GetComponent<RunState>();
            step.OnValueChanged += OnStep;
            if (IsServer) ServerBegin();
            else if (step.Value >= 0) StepChanged?.Invoke(step.Value);
        }

        public override void OnNetworkDespawn()
        {
            step.OnValueChanged -= OnStep;
            if (IsServer && enabled) ServerEnd();
            if (Current == this) Current = null;
        }

        private void OnStep(int before, int now) => StepChanged?.Invoke(now);

        private void ServerBegin()
        {
            Threats.Threat.ContractDamageScale = TutorialConfig.Current.ThreatDamageScale;
            foreach (PlayerEquipment e in PlayerEquipment.All)
                if (e != null && e.IsSpawned && e.NetworkManager == NetworkManager) e.ServerTrainingKit();
            SpawnItems(-1);
            step.Value = 0;
            stepStarted.Value = Now;
            StartStep(CurrentStep);
            Debug.Log($"[Training] Started: {Catalog.Count} lessons, {items.Count} items in the house.");
        }

        // Leaving (home, or the house again): nobody keeps gear from a training crate. At the HQ the company's stock
        // rules trim hands back to what it owns (CompanyService.Training).
        private void ServerEnd()
        {
            DismissVisitor();
            foreach (PlayerEquipment e in PlayerEquipment.All)
                if (e != null && e.IsSpawned && e.NetworkManager == NetworkManager) e.ServerTrainingKit();
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || finished.Value) return;
            TickBeats();
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + CheckEvery;
            TickWorldState();
            // Several lessons may already be done at once (the clock went straight into the truck).
            for (int guard = 0; guard < 8 && !finished.Value; guard++)
            {
                TutorialStepDefinition s = CurrentStep;
                if (s == null) { Finish(); break; }
                if (s.Personal || !TutorialChecks.Done(this, s)) break;
                Advance();
            }
            TickGuide();
        }

        // ---- Host ----

        private void Advance()
        {
            if (!IsServer || finished.Value) return;
            int next = step.Value + 1;
            reported.Clear();
            if (next >= Catalog.Count)
            {
                Finish();
                return;
            }
            step.Value = next;
            stepStarted.Value = Now;
            StartStep(CurrentStep);
        }

        private void Finish()
        {
            if (finished.Value) return;
            finished.Value = true;
            step.Value = Catalog.Count;
            Debug.Log("[Training] Every lesson done.");
        }

        private void JumpTo(int index)
        {
            reported.Clear();
            step.Value = Mathf.Clamp(index, 0, Mathf.Max(0, Catalog.Count - 1));
            stepStarted.Value = Now;
            StartStep(CurrentStep);
        }

        // ---- Requests (any machine) ----

        /// <summary>This machine's player did the current personal lesson.</summary>
        public void ReportDone(int index) => ReportRpc(index);

        /// <summary>The host skips the current lesson (pause menu).</summary>
        public void RequestSkip() => SkipRpc(step.Value);

        /// <summary>The host puts the crew back at the start of this chapter, its salvage back in place.</summary>
        public void RequestResetChapter() => ResetRpc(step.Value);

        /// <summary>This machine's player took gear from a kit crate.</summary>
        public void RequestGear(int crate) => GearRpc(crate);

        public bool CanSkip => CurrentStep is TutorialStepDefinition s && s.Skippable && !finished.Value;

        [Rpc(SendTo.Server)]
        private void ReportRpc(int index, RpcParams rpcParams = default)
        {
            TutorialStepDefinition s = CurrentStep;
            if (finished.Value || index != step.Value || s == null || !s.Personal) return;
            reported.Add(rpcParams.Receive.SenderClientId);
            if (TutorialConfig.Current.AnyPlayerCompletes || EveryoneReported()) Advance();
        }

        private bool EveryoneReported()
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned && p.NetworkManager == NetworkManager && !p.IsDead && !reported.Contains(p.OwnerClientId)) return false;
            return true;
        }

        [Rpc(SendTo.Server)]
        private void SkipRpc(int index, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || index != step.Value || !CanSkip) return;
            Debug.Log($"[Training] Skipped '{CurrentStep.Id}'.");
            Advance();
        }

        [Rpc(SendTo.Server)]
        private void ResetRpc(int index, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || index != step.Value || finished.Value) return;
            TutorialStepDefinition s = CurrentStep;
            if (s == null) return;
            int start = Catalog.ChapterStart(step.Value);
            DismissVisitor();
            ResetChapterItems(s.Chapter);
            if (TutorialSpot.ForChapter(s.Chapter) is TutorialSpot spot)
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.IsSpawned && p.NetworkManager == NetworkManager) p.ServerRespawn(Spread(spot.Pose, p.CrewSeat));
            Debug.Log($"[Training] Chapter {s.Chapter} reset.");
            JumpTo(start);
        }

        // Side by side, so a crew put back at one spot doesn't stand inside each other.
        private static Pose Spread(Pose pose, int seat)
        {
            int n = Mathf.Max(0, seat);
            Vector3 right = pose.rotation * Vector3.right;
            float offset = (n % 4 - 1.5f) * 0.9f;
            return new Pose(pose.position + right * offset, pose.rotation);
        }

        [Rpc(SendTo.Server)]
        private void GearRpc(int crateIndex, RpcParams rpcParams = default)
        {
            TutorialGearCrate crate = TutorialGearCrate.ByIndex(crateIndex);
            PlayerEquipment gear = PlayerEquipment.Of(NetworkManager, rpcParams.Receive.SenderClientId);
            if (crate == null || gear == null || gear.Catalog == null) return;
            if (Vector3.Distance(gear.transform.position, crate.transform.position) > 3.5f) return;
            int index = gear.Catalog.IndexOf(crate.EquipmentId);
            if (index < 0) return;
            if (!gear.ServerGive(index)) gear.ServerHint(Loc.Mark("Your hand slots are full. Drop something (hold the inventory key and press drop) to make room."));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || !IsSpawned) return;
            TutorialStepDefinition s = CurrentStep;
            string line = finished.Value ? "TRAINING done" : s == null ? "TRAINING (no lesson)"
                : $"TRAINING {step.Value + 1}/{Catalog.Count} ch{s.Chapter} '{s.Id}' {s.Check} {s.Key}{(s.Area.Length > 0 ? " @" + s.Area : "")} {s.Amount:0.#}  {StepElapsed:0}s";
            GUI.Label(new Rect(15f, 215f, 900f, 22f), line);
            TutorialGuide g = guide.Value;
            GUI.Label(new Rect(15f, 259f, 900f, 22f), $"  guide {g.Kind} {g.Target}:{g.Key} {(g.Follow != 0 ? "follow #" + g.Follow + " " : "")}" +
                $"{g.Fraction:P0} {g.Readout} {g.Value:0.#}/{g.Goal:0.#}{(g.Danger != 0 ? "  DANGER #" + g.Danger : "")}");
            if (IsServer) GUI.Label(new Rect(15f, 237f, 900f, 22f), "  " + DebugState());
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            StepChanged = null;
        }
    }
}
