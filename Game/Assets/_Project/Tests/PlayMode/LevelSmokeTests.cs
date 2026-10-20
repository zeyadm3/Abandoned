using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Threats;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// Smoke run of every level (0.24.11): a real job with real monsters, decay and route changes while the player
    /// moves around the level, scans and handles loot, then the truck leaves and the next job starts. Any error or
    /// exception logged fails it (the test framework does that); warnings are listed as "[Smoke]" lines.
    /// Explicit: run by name, it takes minutes.
    /// </summary>
    [Explicit("Minutes long: Tools/unity.sh playmode LevelSmokeTests")]
    public class LevelSmokeTests
    {
        private const float JobSeconds = 75f, AfterSeconds = 20f;
        private readonly List<string> warnings = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            warnings.Clear();
            Application.logMessageReceived += OnLog;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Application.logMessageReceived -= OnLog;
            yield return null;
        }

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Warning) warnings.Add(message.Split('\n')[0]);
        }

        [UnityTest, Timeout(600000)] public IEnumerator Mall() => Level("Mall");
        [UnityTest, Timeout(600000)] public IEnumerator School() => Level("School");
        [UnityTest, Timeout(600000)] public IEnumerator Factory() => Level("Factory");
        [UnityTest, Timeout(600000)] public IEnumerator Prison() => Level("Prison");
        [UnityTest, Timeout(600000)] public IEnumerator Tutorial() => Level("Tutorial");

        [UnityTest, Timeout(600000)]
        public IEnumerator HQ()
        {
            yield return TestMapScene.Load("HQ");
            Vector3[] walkable = Walkable();
            var random = new System.Random(5);
            float end = Time.time + 40f;
            while (Time.time < end)
            {
                if (walkable.Length > 0) NetworkPlayer.Local.OwnerTeleport(walkable[random.Next(walkable.Length)] + Vector3.up * 0.1f);
                yield return new WaitForSeconds(2f);
            }
            Report("HQ", 0);
        }

        private IEnumerator Level(string scene)
        {
            yield return TestMapScene.Load(scene);
            for (int i = 0; i < 120 && RunState.Current == null; i++) yield return null;
            Assert.IsNotNull(RunState.Current, $"{scene}: a job starts");
            Vector3[] walkable = Walkable();
            Assert.Greater(walkable.Length, 0, $"{scene}: has a NavMesh");
            var random = new System.Random(scene.GetHashCode());
            int jobs = 1;

            float end = Time.time + JobSeconds;
            yield return Wander(walkable, random, end);
            // Leave: from the ignition (the second pull confirms below quota), then the next job.
            TruckCargo truck = TruckCargo.Current;
            if (truck != null)
            {
                NetworkPlayer.Local.OwnerTeleport(truck.Ignition.position + Vector3.up * 0.1f);
                yield return new WaitForSeconds(0.5f);
                if (RunState.Current.State.Phase == RunPhase.Running) RunState.Current.RequestDepart();
                float leave = Time.time + RunState.Current.HonkSeconds + 5f;
                while (RunState.Current.State.Phase != RunPhase.Departed && Time.time < leave) { Heal(); yield return null; }
            }
            if (RunState.Current.State.Phase == RunPhase.Departed)
            {
                Object.FindAnyObjectByType<RunDirector>().StartNextRun();
                jobs++;
                yield return new WaitForSeconds(1f);
                yield return Wander(walkable, random, Time.time + AfterSeconds);
            }
            Report(scene, jobs);
        }

        private IEnumerator Wander(Vector3[] walkable, System.Random random, float until)
        {
            float nextMove = 0f;
            while (Time.time < until)
            {
                RunState run = RunState.Current;
                if (run != null && run.State.Phase == RunPhase.Departed)
                {
                    Object.FindAnyObjectByType<RunDirector>().StartNextRun();
                    yield return new WaitForSeconds(1f);
                }
                Heal();
                NetworkPlayer me = NetworkPlayer.Local;
                if (Time.time >= nextMove && me != null && !me.IsDead)
                {
                    nextMove = Time.time + 4f;
                    me.OwnerTeleport(walkable[random.Next(walkable.Length)] + Vector3.up * 0.1f);
                    yield return new WaitForSeconds(0.3f);
                    me.GetComponent<LootTags>()?.Scan();
                    Handle(me, random);
                }
                yield return null;
            }
        }

        // Pick up something small nearby, then put it down again on the next stop.
        private static void Handle(NetworkPlayer me, System.Random random)
        {
            var carrier = me.GetComponent<PlayerCarrier>();
            if (carrier == null || InteractionService.Handler == null) return;
            if (carrier.Inventory.Count > 0)
            {
                InteractionService.Handler.RequestDropFromPocket(carrier);
                return;
            }
            NetworkLoot near = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .Where(l => l != null && l.IsSpawned && l.Grabbable.CarryClass == CarryClass.Pocket && !l.Grabbable.IsPocketed
                            && Vector3.Distance(l.transform.position, me.transform.position) < 6f)
                .OrderBy(_ => random.Next()).FirstOrDefault();
            if (near == null) return;
            near.Grabbable.Body.position = carrier.EyePosition + carrier.EyeForward * 0.8f;
            near.transform.position = near.Grabbable.Body.position;
            InteractionService.Handler.RequestPickup(carrier, near.Grabbable);
        }

        private static void Heal()
        {
            NetworkPlayer me = NetworkPlayer.Local;
            if (me != null && !me.IsDead && me.Health < me.MaxHealth * 0.5f) me.ServerHeal(me.MaxHealth);
        }

        // Walkable spots on this level's NavMesh, off any noisy ground.
        private static Vector3[] Walkable()
        {
            NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
            var spots = new List<Vector3>();
            for (int i = 0; i + 2 < mesh.indices.Length; i += 3 * 7)
            {
                Vector3 c = (mesh.vertices[mesh.indices[i]] + mesh.vertices[mesh.indices[i + 1]] + mesh.vertices[mesh.indices[i + 2]]) / 3f;
                if (NoisyGround.All.Any(g => g != null && g.Covers(c))) continue;
                spots.Add(c);
            }
            return spots.ToArray();
        }

        private void Report(string scene, int jobs)
        {
            var counts = warnings.GroupBy(w => w).OrderByDescending(g => g.Count()).ToList();
            Debug.Log($"[Smoke] {scene}: {jobs} job(s), {warnings.Count} warnings ({counts.Count} kinds), monsters seen {Threat.All.Count}." +
                      string.Concat(counts.Take(25).Select(g => $"\n[Smoke]   {g.Count()}x {g.Key}")));
        }
    }
}
