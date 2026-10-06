using System.Collections;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// M3.5 review fixes over in-process NGO: grips turn with the item, so a crew can swing a long load
    /// round a corner; and an under-crewed Huge item only nudges, however long you pull or regrab.
    /// </summary>
    public class SharedCarryTurnAndNudgeTests : SharedCarryTestBase
    {
        [UnityTest]
        public IEnumerator CarriersWalkingAroundEachOtherTurnTheRackAQuarter()
        {
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            // Stand at the rack's two ends, in line with it, so each grip points along its length.
            Transform rack = NetLootKit.CopyOn(net.Host, id).transform;
            yield return Drive(3f, () => Own(client1).transform.position.z >= rack.position.z && Own(client2).transform.position.z >= rack.position.z,
                (client1, Vector2.up, false), (client2, Vector2.up, false));
            yield return Grab(client1, id);
            yield return Grab(client2, id);
            yield return WaitForCrew(id, 2, lifted: true);
            yield return Stand(0.5f, client1, client2);
            // Back away from the platform edge first so there's room to circle.
            yield return Drive(4f, () => rack.position.z < -1.2f, (client1, Vector2.down, false), (client2, Vector2.down, false));
            yield return Stand(0.5f, client1, client2);

            float startYaw = rack.eulerAngles.y;
            float Turned() => Mathf.Abs(Mathf.DeltaAngle(startYaw, rack.eulerAngles.y));
            float CarrierLine()
            {
                Vector3 d = Own(client2).transform.position - Own(client1).transform.position;
                return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            }
            float startLine = CarrierLine();
            OwnPlayer(client1).Motor.enabled = false;
            OwnPlayer(client2).Motor.enabled = false;
            float end = Time.time + 8f;
            // The carriers swap round a quarter; the rack should come with them.
            while (Time.time < end && Mathf.Abs(Mathf.DeltaAngle(startLine, CarrierLine())) < 90f)
            {
                foreach (NetworkBootstrap c in new[] { client1, client2 })
                {
                    // Each walks round the rack's centre the same way, like turning a sofa in a hallway.
                    Transform me = Own(c).transform;
                    Vector3 radial = Vector3.ProjectOnPlane(me.position - NetLootKit.CopyOn(c, id).transform.position, Vector3.up);
                    Vector3 local = Quaternion.Inverse(me.rotation) * Vector3.Cross(Vector3.up, radial).normalized;
                    OwnPlayer(c).Motor.Simulate(PlayerTestRig.Frame(new Vector2(local.x, local.z)), Time.deltaTime);
                }
                yield return null;
            }
            yield return Stand(1.5f, client1, client2);

            Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(startLine, CarrierLine())), 80f, "the carriers went round a quarter");
            Assert.Greater(Turned(), 75f, "the rack turned about a quarter with its carriers (world-fixed grips jammed it short)");
            SharedCarryable host = SharedOn(net.Host, id);
            Assert.IsTrue(host.IsLifted, "still carried");
            for (int i = 0; i < host.PointCount; i++)
                Assert.Less(host.LastPull[i].magnitude, 0.5f, $"handle {i} settles where its carrier wants it (no residual strain)");
            foreach (NetworkBootstrap c in new[] { client1, client2 })
            {
                SharedCarryable mine = SharedOn(c, id);
                Vector3 off = Own(c).transform.position - mine.AnchorFor(mine.IndexOf(Own(c)));
                Assert.Less(new Vector2(off.x, off.z).magnitude, mine.Config.TetherSlack, $"{c.name} stays at their end of the turned rack");
            }
        }

        [UnityTest]
        public IEnumerator OnePlayerOnlyNudgesAHugeItemEvenAfterRegrabbing()
        {
            ulong id = 0;
            yield return SpawnPiano(i => id = i);
            SharedCarryable piano = SharedOn(net.Host, id);
            float budget = piano.Config.NudgeRadius;
            Vector3 start = NetLootKit.CopyOn(net.Host, id).transform.position;
            float Moved() => Vector3.ProjectOnPlane(NetLootKit.CopyOn(net.Host, id).transform.position - start, Vector3.up).magnitude;

            yield return Grab(client1, id);
            yield return WaitForCrew(id, 1, lifted: false);
            Assert.IsTrue(piano.IsNudgeOnly, "one person on a Huge item can only nudge it");
            yield return Drive(5f, null, (client1, Vector2.down, false));
            yield return Stand(0.5f, client1);
            float first = Moved();
            Assert.Greater(first, 0.2f, "it does budge");
            Assert.Less(first, budget + 0.15f, $"but no further than the {budget} m nudge (it used to creep {piano.Config.CreepSpeed} m/s for ever)");

            // Letting go and grabbing again must not refill the budget.
            yield return LetGo(client1, id);
            yield return Grab(client1, id);
            yield return Drive(4f, null, (client1, Vector2.down, false));
            yield return Stand(0.5f, client1);
            Assert.Less(Moved(), budget + 0.15f, "regrabbing doesn't give a fresh nudge");
            Assert.IsTrue(net.Machines.All(m => Vector3.Distance(NetLootKit.CopyOn(m, id).transform.position,
                NetLootKit.CopyOn(net.Host, id).transform.position) < 0.3f), "every machine agrees where it is");
        }
    }
}
