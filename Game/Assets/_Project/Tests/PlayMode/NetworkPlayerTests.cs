using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Host + clients in one process over loopback Unity Transport: spawning, ownership, movement,
    /// footsteps, ragdoll and structural load replication of the networked Player prefab.
    /// </summary>
    public class NetworkPlayerTests
    {
        private NetTestHarness net;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator EachPlayerSpawnsAtItsOwnSpawnPoint()
        {
            yield return net.StartSession(clients: 3);

            var seen = new HashSet<int>();
            foreach (NetworkBootstrap machine in net.Machines)
            {
                NetworkPlayer own = OwnPlayer(machine);
                Assert.IsTrue(net.Host.Slots.TryGetSlot(machine.Manager.LocalClientId, out int slot), $"{machine.name} has no slot");
                Assert.IsTrue(seen.Add(slot), $"slot {slot} given twice");
                Vector3 expected = SpawnPosition(slot);
                Assert.Less(Vector3.Distance(Flat(own.transform.position), Flat(expected)), 0.3f,
                    $"{machine.name}'s player at {own.transform.position}, its point is {expected}");
            }
            Assert.AreEqual(0, net.Host.Slots.TryGetSlot(net.Host.Manager.LocalClientId, out int hostSlot) ? hostSlot : -1, "host takes slot 0");
            ScreenshotCapture.CaptureFrom(new Vector3(3f, 2.5f, -6f), new Vector3(3f, 1f, 0f), "M3_2_four_players_spawned");
        }

        [UnityTest]
        public IEnumerator OnlyTheOwnerRunsCameraInputLookAndMotor()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();

            foreach (NetworkBootstrap machine in net.Machines)
            foreach (NetworkPlayer p in PlayersOn(machine))
            {
                bool own = p.OwnerClientId == machine.Manager.LocalClientId;
                string who = $"{machine.name}'s copy of player {p.OwnerClientId}";
                Assert.AreEqual(own, p.IsOwner, who);
                Assert.AreEqual(own, p.GetComponentInChildren<CinemachineCamera>(true).enabled, $"{who}: camera");
                Assert.AreEqual(own, p.GetComponent<PlayerInputReader>().enabled, $"{who}: input");
                Assert.AreEqual(own, p.GetComponent<PlayerLook>().enabled, $"{who}: look");
                Assert.AreEqual(own, p.GetComponent<PlayerMotor>().enabled, $"{who}: motor");
                Assert.AreEqual(own, p.GetComponent<PlayerInteractor>().enabled, $"{who}: interactor");
                Assert.AreEqual(own, p.GetComponent<InteractionHud>().enabled, $"{who}: HUD");
                Assert.AreEqual(own, p.GetComponentInChildren<PlayerHitDetector>(true).gameObject.activeSelf, $"{who}: hit trigger");
                Assert.AreEqual(!own, p.Ragdoll.IsRemote, who);
                Assert.IsTrue(p.GetComponentInChildren<MeshRenderer>().enabled, $"{who}: still renders");
                Assert.IsNull(p.GetComponentInChildren<AudioListener>(true), "the listener lives on the scene camera, not the player");
            }
            Assert.AreSame(OwnPlayer(client), PlayersOn(client).Single(p => p.IsLocalPlayer));
        }

        [UnityTest]
        public IEnumerator OwnerMovementReplicatesToHostAndOtherClients()
        {
            yield return net.StartSession(clients: 2);
            NetworkBootstrap mover = net.Clients.First();
            NetworkPlayer own = OwnPlayer(mover);
            Vector3 start = own.transform.position;

            // Walk forward ~1.5 s, driving the motor ourselves (its own Update would add a zero-input step).
            own.Motor.enabled = false;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                own.Motor.Simulate(PlayerTestRig.Frame(Vector2.up), Time.deltaTime);
                yield return null;
            }
            Vector3 end = own.transform.position;
            Assert.Greater(Vector3.Distance(start, end), 3f, "the owner should have walked");

            ulong ownerId = mover.Manager.LocalClientId;
            foreach (NetworkBootstrap other in net.Machines.Where(m => m != mover))
            {
                NetworkPlayer copy = PlayerOf(other, ownerId);
                yield return WaitFor(() => Vector3.Distance(copy.transform.position, end) < 0.1f,
                    $"{other.name} to see the move (at {copy.transform.position}, owner at {end})", 3f);
            }
        }

        [UnityTest]
        public IEnumerator RemotePlayersMakeFootstepsAndLoadFloorsOnTheHost()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();
            NetworkPlayer own = OwnPlayer(client);
            NetworkPlayer onHost = PlayerOf(net.Host, client.Manager.LocalClientId);
            var hostSteps = onHost.GetComponent<PlayerFootsteps>();

            yield return WaitFor(() => onHost.Motor.IsGrounded, "the host to see the client grounded", 3f);
            var load = onHost.GetComponent<CarrierLoad>();
            Assert.AreEqual(own.Motor.Config.BodyWeight, load.LoadWeight, 0.01f, "a standing remote player weighs on the floor");

            int before = hostSteps.StepCount;
            own.Motor.enabled = false;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                own.Motor.Simulate(PlayerTestRig.Frame(Vector2.up, sprint: true), Time.deltaTime);
                yield return null;
            }
            yield return WaitSeconds(0.3f);
            Assert.Greater(hostSteps.StepCount - before, 2, "the host hears the remote player's footsteps");
            Assert.AreEqual(own.GetComponent<PlayerFootsteps>().LastVolume, hostSteps.LastVolume, 0.001f, "sprint volume replicated");

            var points = new List<LoadPoint>();
            load.GetLoadPoints(points);
            Assert.Less(Vector3.Distance(points[0].Position, own.transform.position), 0.2f, "the load follows the replicated position");
        }

        [UnityTest]
        public IEnumerator RagdollIsLocalButOthersSeeTheBodyLyingDown()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();
            NetworkPlayer own = OwnPlayer(client);
            NetworkPlayer onHost = PlayerOf(net.Host, client.Manager.LocalClientId);
            Transform hostBody = onHost.transform.Find("Body");

            own.Ragdoll.Enter(Vector3.zero);
            yield return WaitFor(() => onHost.Ragdoll.IsRagdolled, "the host to see the ragdoll", 3f);
            Assert.IsFalse(onHost.Ragdoll.Pelvis.gameObject.activeInHierarchy, "no physics ragdoll on the remote copy");
            Assert.Less(Mathf.Abs(Vector3.Dot(hostBody.up, Vector3.up)), 0.2f, "the remote capsule lies on its side");
            yield return WaitFor(() => onHost.Ragdoll.IsBodyResting, "the body to come to rest", 4f);
            Assert.Less(Vector3.Distance(onHost.Ragdoll.BodyPosition, own.Ragdoll.Pelvis.position), 0.3f);
            // Owner's physics ragdoll and the host's lying capsule overlap in this one-process world.
            Vector3 at = onHost.Ragdoll.BodyPosition;
            ScreenshotCapture.CaptureFrom(at + new Vector3(2f, 1.5f, -2f), at, "M3_2_remote_ragdoll_lying");

            own.Ragdoll.Recover();
            yield return WaitFor(() => !onHost.Ragdoll.IsRagdolled, "the host to see them get up", 3f);
            Assert.Greater(Vector3.Dot(hostBody.up, Vector3.up), 0.99f, "the capsule stands again");
        }

        [UnityTest]
        public IEnumerator RemoteCopyDropsItsStandingColliderWhileTheOwnerIsRagdolled()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();
            NetworkPlayer own = OwnPlayer(client);
            NetworkPlayer onHost = PlayerOf(net.Host, client.Manager.LocalClientId);
            var hostController = onHost.GetComponent<CharacterController>();
            // The harness switches remote colliders off (shared physics world); real machines keep them.
            hostController.enabled = true;
            try
            {
                own.Ragdoll.Enter(Vector3.zero);
                yield return WaitFor(() => onHost.Ragdoll.IsRagdolled, "the host to see the ragdoll", 3f);
                Assert.IsFalse(hostController.enabled, "no invisible upright capsule where the owner fell");
                own.Ragdoll.Recover();
                yield return WaitFor(() => !onHost.Ragdoll.IsRagdolled, "the host to see them get up", 3f);
                Assert.IsTrue(hostController.enabled, "the standing collider comes back with the player");
            }
            finally { hostController.enabled = false; }
        }

        [UnityTest]
        public IEnumerator PlayersFaceTheirSpawnPointsForwardAfterLooking()
        {
            float[] yaws = { 135f, 90f };
            for (int i = 0; i < yaws.Length; i++)
                GameObject.Find($"Spawn_{i}").transform.rotation = Quaternion.Euler(0f, yaws[i], 0f);
            yield return net.StartSession(clients: 1);

            foreach (NetworkBootstrap machine in net.Machines)
            {
                NetworkPlayer own = OwnPlayer(machine);
                Assert.IsTrue(net.Host.Slots.TryGetSlot(machine.Manager.LocalClientId, out int slot));
                // What a first captured-cursor frame does; it used to snap the body back to yaw 0.
                own.GetComponent<PlayerLook>().ApplyLook(Vector2.zero);
                Assert.AreEqual(0f, Mathf.DeltaAngle(own.transform.eulerAngles.y, yaws[slot]), 1f,
                    $"{machine.name}'s player faces {own.transform.eulerAngles.y}, its point faces {yaws[slot]}");
            }
        }

        [UnityTest]
        public IEnumerator ClientLandingsReachTheHostAsImpacts()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();
            NetworkPlayer own = OwnPlayer(client);

            var impacts = new List<float>();
            void OnImpact(Vector3 position, float momentum) => impacts.Add(momentum);
            StructureSignals.Impact += OnImpact;
            try
            {
                // Raised on the client's own player; the host must hear it from its copy too.
                own.Motor.RaiseRemoteLanding(2f, 6f);
                yield return WaitFor(() => impacts.Count >= 2, "the host copy to report the landing", 3f);
            }
            finally { StructureSignals.Impact -= OnImpact; }
            Assert.AreEqual(impacts[0], impacts[1], 0.01f, "same momentum on host and client");
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);
    }
}
