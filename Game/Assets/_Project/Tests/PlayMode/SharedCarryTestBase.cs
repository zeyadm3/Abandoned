using System;
using System.Collections;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Shared carrying over in-process NGO (host + 2 clients). Everyone starts on a 5 m high platform
    /// whose north edge is at z = <see cref="Edge"/>: host at x 0, client 1 at x 2, client 2 at x 4,
    /// all facing +Z. Items spawn on the platform in front of them; carrying one past the edge and
    /// letting go drops it 5 m onto the ground.
    /// </summary>
    public abstract class SharedCarryTestBase : NetworkLootTestBase
    {
        protected const float Height = 5f, Edge = 2f;
        protected const float RackWeight = 300f, PianoWeight = 500f;

        protected override void ArrangeArena()
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.position = new Vector3(2f, Height / 2f, (Edge - 8f) / 2f);
            platform.transform.localScale = new Vector3(12f, Height, 8f + Edge);
            net.Track(platform);
            for (int i = 0; i < net.Config.MaxPlayers; i++)
            {
                GameObject spawn = GameObject.Find($"Spawn_{i}");
                spawn.transform.position += Vector3.up * Height;
            }
            Physics.SyncTransforms();
        }

        /// <summary>The server rack (Heavy, needs 2) between clients 1 and 2, its 1 m long axis along x: handles at x 2.5 and 3.5.</summary>
        protected IEnumerator SpawnRack(Action<ulong> spawned) =>
            SpawnAndSettle("server_rack", new Vector3(3f, Height + 1.05f, 1f), 90f, spawned);

        /// <summary>The grand piano (Huge, needs 3) in front of all three: handles at x 0.8 / 3.2 (z 1.2) and z 0.45 / 1.95 (x 2).</summary>
        protected IEnumerator SpawnPiano(Action<ulong> spawned) =>
            SpawnAndSettle("grand_piano", new Vector3(2f, Height + 0.55f, 1.2f), 90f, spawned);

        private IEnumerator SpawnAndSettle(string lootId, Vector3 at, float yaw, Action<ulong> spawned)
        {
            ulong id = 0;
            yield return NetLootKit.Spawn(net, lootId, at, i => id = i, yaw);
            yield return NetLootKit.Settle();
            spawned(id);
        }

        protected static SharedCarryable SharedOn(NetworkBootstrap machine, ulong id) => NetLootKit.CopyOn(machine, id).Grabbable.Shared;

        protected static LootItem ItemOn(NetworkBootstrap machine, ulong id) => NetLootKit.CopyOn(machine, id).Item;

        protected static float Bottom(NetworkBootstrap machine, ulong id) => NetLootKit.CopyOn(machine, id).Grabbable.GetBounds().min.y;

        /// <summary>The real request path (E on the item); returns once every machine sees them on a handle.</summary>
        protected IEnumerator Grab(NetworkBootstrap who, ulong id)
        {
            Handler.RequestPickup(Own(who), NetLootKit.CopyOn(who, id).Grabbable);
            yield return WaitFor(() => net.Machines.All(m => SharedOn(m, id).IsCarriedBy(CarrierOf(m, who))),
                $"every machine to see {who.name} on a handle of #{id} (hint '{Own(who).Hint}')", 5f);
        }

        protected IEnumerator LetGo(NetworkBootstrap who, ulong id)
        {
            Handler.RequestDrop(Own(who));
            yield return WaitFor(() => net.Machines.All(m => !SharedOn(m, id).IsCarriedBy(CarrierOf(m, who))),
                $"every machine to see {who.name} let go of #{id}", 5f);
        }

        protected IEnumerator WaitForCrew(ulong id, int carriers, bool lifted) =>
            WaitFor(() => net.Machines.All(m => SharedOn(m, id).CarrierCount == carriers && SharedOn(m, id).IsLifted == lifted),
                $"every machine to see #{id} with {carriers} carrier(s), lifted={lifted}", 5f);

        /// <summary>Drives clients' own motors for real (one Simulate per frame) until <paramref name="done"/> or the time runs out.</summary>
        protected static IEnumerator Drive(float seconds, Func<bool> done, params (NetworkBootstrap who, Vector2 move, bool crouch)[] walkers)
        {
            foreach (var w in walkers) OwnPlayer(w.who).Motor.enabled = false;
            float end = Time.time + seconds;
            while (Time.time < end && (done == null || !done()))
            {
                foreach (var w in walkers)
                    OwnPlayer(w.who).Motor.Simulate(PlayerTestRig.Frame(w.move, crouchHeld: w.crouch), Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>Lets the given clients stand still (motor stepped with no input) for a while.</summary>
        protected static IEnumerator Stand(float seconds, params NetworkBootstrap[] who) =>
            Drive(seconds, null, who.Select(w => (w, Vector2.zero, false)).ToArray());

        protected static void AssertUpright(NetworkBootstrap machine, ulong id, float maxDegrees, string what) =>
            Assert.Less(Vector3.Angle(NetLootKit.CopyOn(machine, id).transform.up, Vector3.up), maxDegrees, what);
    }
}
