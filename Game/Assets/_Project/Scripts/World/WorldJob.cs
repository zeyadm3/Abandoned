using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// A job in the open world, start to payday (Docs/OPEN_WORLD_PLAN.md). The host's crew picks a contract at the HQ's
    /// board and drives out: the job is taken as the van leaves the HQ (as the van used to leave). The place stays empty
    /// until the van arrives on its site, then the run starts as it always has (quota, decay, monsters). The van's cargo
    /// box is the job's truck: the driver's horn starts the departure countdown, or driving off the site leaves at once;
    /// whoever and whatever is aboard counts. Anyone left behind alive is fetched into the cargo box (their pockets were
    /// already lost). The appraisal and payday wait until the van is back at the HQ; "Back to HQ" there sells the load.
    /// Plugs into the company and run through their open-world hooks; does nothing outside the world scene.
    /// </summary>
    public class WorldJob : MonoBehaviour
    {
        [Tooltip("How far past a place's ground (m) the van counts as having left it.")]
        [SerializeField] private float leaveMargin = 60f;
        [Tooltip("How far past a place's ground (m) the van counts as there (the run starts, the HQ counts as home).")]
        [SerializeField] private float arriveMargin = 25f;

        private bool leavingHome, wasHome = true;
        private RunState hookedRun;

        public static WorldJob Current { get; private set; }

        private void OnEnable()
        {
            Current = this;
            CompanyService.WorldAtHome = AtHome;
            CompanyService.WorldReturnHome = ServerReturnHome;
            RunDirector.MayStart = MayStart;
            AppraisalScreen.HoldBack = HoldBack;
            WorldState.ServerClearing = ServerClear;
        }

        private void OnDisable()
        {
            if (Current == this) Current = null;
            if (CompanyService.WorldAtHome == (System.Func<bool>)AtHome) CompanyService.WorldAtHome = null;
            if (CompanyService.WorldReturnHome == (System.Action)ServerReturnHome) CompanyService.WorldReturnHome = null;
            if (RunDirector.MayStart == (System.Func<GameObject, bool>)MayStart) RunDirector.MayStart = null;
            if (AppraisalScreen.HoldBack == (System.Func<bool>)HoldBack) AppraisalScreen.HoldBack = null;
            if (WorldState.ServerClearing == (System.Action<int>)ServerClear) WorldState.ServerClearing = null;
        }

        // The open world is the HQ's way out whenever its scene is in the build (decision 2: driving is the only way).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void UseWorldWhenBuilt()
        {
            if (!Application.CanStreamedLevelBeLoaded(WorldAtlas.WorldScene)) return;
            CompanyService.HomeTravel = WorldAtlas.WorldScene;
            UI.TravelScreen.NoDrive = level => level == WorldAtlas.WorldScene;
        }

        /// <summary>World distance (m) from the van to a place's ground (0 on it), or infinity without a van.</summary>
        public static float VanFrom(int place)
        {
            WorldState state = WorldState.Current;
            if (state == null || WorldVan.Current == null || place < 0 || place >= state.Atlas.Pois.Count) return float.PositiveInfinity;
            return state.Atlas.Pois[place].FromGround(state.ToWorld(WorldVan.Current.transform.position));
        }

        /// <summary>The place the active job is at (-1 for none).</summary>
        public static int JobPlace
        {
            get
            {
                CompanyService company = CompanyService.Current;
                WorldState state = WorldState.Current;
                return company != null && company.Active.IsValid && state != null ? state.Atlas.IndexOf(company.Active.Scene) : -1;
            }
        }

        private bool AtHome() => leavingHome || (WorldState.Current != null && WorldState.Current.Anchor == WorldState.Current.Atlas.Home
                                                  && VanFrom(WorldState.Current.Atlas.Home) <= arriveMargin);

        // A streamed place's run starts only if it is the job's place and the crew has driven onto it.
        private bool MayStart(GameObject director)
        {
            int job = JobPlace;
            return job >= 0 && WorldState.Current.Atlas.Pois[job].scene == director.scene.name && VanFrom(job) <= arriveMargin;
        }

        private bool HoldBack() => WorldState.Current != null && !AtHome();

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            WorldState state = WorldState.Current;
            if (company == null || state == null || !state.IsSpawned || !state.IsServer) return;
            HookRun();
            // Leaving the HQ with a contract picked: the job is taken now (its quota is owed from here, as before).
            bool home = AtHome();
            if (wasHome && !home && company.Selected >= 0 && !company.Active.IsValid)
            {
                leavingHome = true;
                company.RequestDepart();
                leavingHome = false;
            }
            wasHome = home;
            // Driving off the job's site with the run still going: it leaves now, with whoever and whatever is aboard.
            RunState run = RunState.Current;
            int job = JobPlace;
            if (run != null && run.IsSpawned && job >= 0 && (run.State.Phase is RunPhase.Running or RunPhase.Honking)
                && state.Streamed == job && VanFrom(job) > leaveMargin && WorldVan.Current.Seats.HasDriver)
                run.ServerDepartNow();
        }

        private void HookRun()
        {
            if (RunState.Current == hookedRun) return;
            if (hookedRun != null) hookedRun.Departed -= OnDeparted;
            hookedRun = RunState.Current;
            if (hookedRun != null) hookedRun.Departed += OnDeparted;
        }

        // Host: whoever is still standing at the place goes into the cargo box (a left-behind crewmate isn't stranded
        // in a place that is about to unload under them; the pockets they had are already counted as lost).
        private void OnDeparted(RunResults results)
        {
            WorldVan van = WorldVan.Current;
            if (van == null || van.Cargo == null || !van.IsServer) return;
            int n = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead || van.Cargo.Carries(p.transform.position)) continue;
                if (van.Seats.Seating.SeatOf(p.OwnerClientId) >= 0) continue;
                Vector3 spot = van.transform.TransformPoint(new Vector3(-.8f + .55f * (n++ % 4), 1.1f, -1.2f));
                p.ServerRespawn(new Pose(spot, van.transform.rotation));
            }
        }

        /// <summary>
        /// Host, before a place unloads everywhere: a job still running there leaves now; then everything spawned for it
        /// goes (its structure sync, monsters, loot), except what rides home: the van, its cargo, the crew's hands and
        /// pockets, and the run itself (its results wait for the HQ).
        /// </summary>
        private void ServerClear(int place)
        {
            NetworkManager manager = NetworkBootstrap.Resolve(null)?.Manager;
            if (manager == null || !manager.IsServer) return;
            RunState run = RunState.Current;
            if (run != null && run.IsSpawned && place == JobPlace && (run.State.Phase is RunPhase.Running or RunPhase.Honking)) run.ServerDepartNow();
            var keep = new HashSet<NetworkObject>();
            WorldVan van = WorldVan.Current;
            if (van != null && van.Cargo != null)
                foreach (LootItem item in van.Cargo.ItemsInside())
                    if (item.TryGetComponent(out NetworkObject no)) keep.Add(no);
            var gone = new List<NetworkObject>();
            foreach (NetworkObject no in manager.SpawnManager.SpawnedObjectsList)
            {
                if (no == null || no.IsPlayerObject || keep.Contains(no) || no.gameObject.scene.name == "DontDestroyOnLoad") continue;
                if (no.GetComponent<WorldVan>() != null || no.GetComponent<RunState>() != null) continue;
                if (no.TryGetComponent(out Interaction.Grabbable g) && (g.Holder != null || g.IsPocketed)) continue;
                gone.Add(no);
            }
            foreach (NetworkObject no in gone) no.Despawn(true);
            Debug.Log($"[World] Cleared {WorldState.Current.Atlas.Pois[place].scene}: {gone.Count} objects despawned, {keep.Count} aboard the van.");
        }

        // Host, the appraisal's "Back to HQ" at home: the load is sold (it was paid for as the van left the job),
        // the run is over. The company resets the board itself.
        private void ServerReturnHome()
        {
            NetworkManager manager = NetworkBootstrap.Resolve(null)?.Manager;
            if (manager == null || !manager.IsServer) return;
            var gone = new List<NetworkObject>();
            foreach (NetworkObject no in manager.SpawnManager.SpawnedObjectsList)
                if (no != null && (no.GetComponent<NetworkLoot>() != null || no.GetComponent<RunState>() != null)) gone.Add(no);
            foreach (NetworkObject no in gone) no.Despawn(true);
            Debug.Log($"[World] Home: the van unloaded ({gone.Count} objects).");
        }
    }
}
