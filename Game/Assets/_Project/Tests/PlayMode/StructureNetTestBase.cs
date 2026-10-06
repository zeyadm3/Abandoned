using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Host + two clients in one process, each with its own copy of a three-tile building 3 m above
    /// the arena floor (away from the spawn points): A healthy, B weak, C on the ground and never
    /// collapsing. The host's copy simulates; the clients' copies mirror it through StructureNetSync.
    /// </summary>
    public abstract class StructureNetTestBase
    {
        protected const string A = "A", B = "B", C = "C";
        protected static readonly Vector3 TileA = new(0f, NetStructureKit.Top, 10f);
        protected static readonly Vector3 TileB = new(5f, NetStructureKit.Top, 10f);
        protected static readonly Vector3 TileC = new(10f, 0.3f, 10f);

        protected NetTestHarness net;
        protected NetStructureKit kit;
        protected NetworkBootstrap client1, client2;

        protected static readonly NetStructureKit.Tile[] Tiles =
        {
            new(A, TileA),
            new(B, TileB, health: 0.6f, capacity: 0.05f), // 150 kg: a 300 kg rack breaks it in a few seconds
            new(C, TileC, collapsible: false),
        };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
            yield return Connect();
        }

        /// <summary>Starts the session and the structure; tests that need a late joiner override this.</summary>
        protected virtual IEnumerator Connect()
        {
            yield return net.StartSession(clients: 2);
            client1 = net.Clients.ElementAt(0);
            client2 = net.Clients.ElementAt(1);
            kit = NetStructureKit.Build(net, Tiles);
            NetStructureSeparator.Create(net, kit);
            yield return kit.WaitForSync();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            kit?.Destroy();
            kit = null;
            net.Destroy();
            yield return null;
        }

        protected StructuralSection HostSection(string name) => kit.Section(net.Host, name);

        /// <summary>Waits until every client shows <paramref name="name"/> at the host's stage.</summary>
        protected IEnumerator WaitForClientsToMatch(string name, string what) =>
            WaitFor(() => net.Clients.All(c => kit.Section(c, name).Stage == HostSection(name).Stage), what, 5f);

        /// <summary>Two hits: the first stops at the warning floor (Cracking), the second ends it (Failing).</summary>
        protected static void BreakToFailing(StructuralSection section)
        {
            section.ApplyImpact(1e6f);
            section.ApplyImpact(1e6f);
            Assert.AreEqual(StructuralStage.Failing, section.Stage);
        }

        /// <summary>Owner-side teleport (CharacterController off while moving), replicated like a real one.</summary>
        protected static IEnumerator StandOn(NetworkBootstrap machine, Vector3 tileTop, float sideways = 0f)
        {
            NetworkPlayer player = OwnPlayer(machine);
            player.OwnerTeleport(tileTop + new Vector3(sideways, 0.05f, 0f));
            yield return WaitSeconds(0.5f);
            Assert.IsTrue(player.Motor.IsGrounded, $"{machine.name}'s player stands on the tile");
            Assert.AreEqual(tileTop.y, player.transform.position.y, 0.15f);
        }
    }
}
