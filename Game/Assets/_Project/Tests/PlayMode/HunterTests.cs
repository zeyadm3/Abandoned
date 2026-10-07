using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>M9.1: the Hunter chases what it sees, can't see you crouching far off, and is heavy enough to fall through floors.</summary>
    public class HunterTests
    {
        private PlayerTestRig rig;
        private Hunter hunter;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
            rig = ForExisting(TestMapScene.Player);
            for (int i = 0; i < 60 && ThreatDirector.Current == null; i++) yield return null;
            hunter = (Hunter)ThreatDirector.Current.SpawnOf<Hunter>();
            Assert.IsNotNull(hunter, "the Hunter is on the mall's roster");
            yield return new WaitForSeconds(0.3f);
        }

        [UnityTest]
        public IEnumerator ItChasesWhatItSeesAndKillsOnContact()
        {
            // The open ground floor under the atrium: it faces the player 7 m away.
            Place(new Vector3(24f, 0f, 9f), Vector3.forward);
            rig.Teleport(new Vector3(24f, 0.05f, 16f));
            rig.Settle();
            bool chased = false;
            float end = Time.time + 8f;
            while (!NetworkPlayer.Local.IsDead && Time.time < end)
            {
                chased |= hunter.State == HunterState.Chase;
                yield return null;
            }
            Assert.IsTrue(chased, "it charged " + Why());
            Assert.IsTrue(NetworkPlayer.Local.IsDead, $"and caught a player standing still (state {hunter.State}, {Vector3.Distance(hunter.transform.position, rig.Player.transform.position):0.0} m)");
        }

        [UnityTest]
        public IEnumerator ACrouchingPlayerFarOffStaysHidden()
        {
            Vector3 at = new(24f, 0f, 6f), spot = new(24f, 0f, 19f); // 13 m across the open atrium floor
            Place(at, Vector3.back);
            // Crouch first, out of its sight (it faces away), then turn it to look.
            Face(at - (spot - at));
            rig.Teleport(new Vector3(spot.x, at.y + 0.05f, spot.z));
            rig.Run(Frame(Vector2.zero, crouchHeld: true), 0.5f); // the rig drives the motor by hand
            float settle = Time.time + 1f;
            while (Time.time < settle || !NetworkPlayer.Local.State.Crouching)
            {
                Face(at - (rig.Player.transform.position - at));
                if (Time.time > settle + 3f) Assert.Fail("never crouched " + Why());
                yield return null;
            }
            Assert.AreNotEqual(HunterState.Chase, hunter.State, "it didn't see the approach");
            float end = Time.time + 2f;
            var agent = hunter.GetComponent<NavMeshAgent>();
            while (Time.time < end)
            {
                agent.Warp(at); // it patrols; keep the 13 m
                Face(rig.Player.transform.position);
                Assert.AreNotEqual(HunterState.Chase, hunter.State, "crouched 13 m away: unseen " + Why());
                yield return null;
            }
            rig.Run(Frame(Vector2.zero), 0.5f); // stand up
            end = Time.time + 2f;
            while (hunter.State != HunterState.Chase && Time.time < end)
            {
                Face(rig.Player.transform.position);
                yield return null;
            }
            Assert.AreEqual(HunterState.Chase, hunter.State, "standing up in its sight: seen");
        }

        [UnityTest]
        public IEnumerator ItsWeightLoadsTheFloorAndAFallStunsIt()
        {
            // An upper floor tile it can stand on (NavMesh at its centre) with nothing else weighing on it.
            StructuralSection tile = Object.FindAnyObjectByType<StructureSimulation>().Sections
                .Where(t => t.CanCollapse && t.Type == SectionType.Floor && t.transform.position.y > 3f && !t.IsCollapsed && t.Load < 1f
                            && NavMesh.SamplePosition(t.transform.position, out NavMeshHit h, 0.3f, NavMesh.AllAreas))
                .OrderBy(t => t.transform.position.y).ThenBy(t => t.name).First();
            rig.Teleport(new Vector3(24f, 0.05f, -8f)); // out of its way
            var agent = hunter.GetComponent<NavMeshAgent>();
            Assert.IsTrue(agent.Warp(tile.transform.position), $"onto {tile.name}");
            // Hold it on the tile (it would patrol away) until the floor has felt it.
            float hold = Time.time + 1f;
            while (Time.time < hold)
            {
                agent.Warp(tile.transform.position);
                yield return null;
            }
            Assert.Greater(tile.Load, hunter.Config.Weight * 0.5f, $"it weighs on the floor (load {tile.Load:0} kg)");
            float before = hunter.transform.position.y;
            tile.Collapse();
            float end = Time.time + 3f;
            while (hunter.State != HunterState.Stunned && Time.time < end) yield return null;
            Assert.AreEqual(HunterState.Stunned, hunter.State, "the floor went: it fell " + Why() + $" tile {tile.name} at {tile.transform.position} collapsed {tile.IsCollapsed}, it stood on {(hunter.StandingOn != null ? hunter.StandingOn.name : "nothing")}");
            yield return new WaitForSeconds(1.5f);
            Assert.Less(hunter.transform.position.y, before - 2f, "down to the floor below");
        }

        private string Why()
        {
            var agent = hunter.GetComponent<NavMeshAgent>();
            return $"[hunter {hunter.State} at {hunter.transform.position} fwd {hunter.transform.forward}, onMesh {agent.isOnNavMesh}, enabled {agent.enabled}; " +
                   $"me at {rig.Player.transform.position} dead {NetworkPlayer.Local.IsDead} crouch {NetworkPlayer.Local.State.Crouching}; threats {Threat.All.Count}: " +
                   $"{string.Join(",", Threat.All.Select(t => t.DisplayName))}]";
        }

        private void Place(Vector3 at, Vector3 facing)
        {
            hunter.GetComponent<NavMeshAgent>().Warp(at);
            hunter.transform.rotation = Quaternion.LookRotation(facing);
        }

        private void Face(Vector3 point)
        {
            Vector3 d = point - hunter.transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.01f) hunter.transform.rotation = Quaternion.LookRotation(d);
        }
    }
}
