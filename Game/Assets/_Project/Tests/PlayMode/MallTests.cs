using System.Collections;
using System.Linq;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>
    /// The greybox mall with the real player: every floor piece is a section, and the routes work (main
    /// entrance, both escalators, both service flights) while railings keep players out of the atrium.
    /// Coordinates follow MallLayout: 4 m tiles, x 0..48, z 0..40, floors at y = 0, 4, 8.
    /// </summary>
    public class MallTests
    {
        private const string SceneName = "Mall";
        private PlayerTestRig rig;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestBuildingScene.Load(SceneName);
            rig = ForExisting(TestBuildingScene.Player);
        }

        [Test]
        public void EveryFloorPieceIsASectionAndOnlyTheGroundAndServiceStairsCantFall()
        {
            StructureSimulation sim = Object.FindAnyObjectByType<StructureSimulation>();
            Assert.IsNotNull(sim);
            Assert.AreEqual(328 + 4, sim.Sections.Count, "328 tiles + 2 escalators + 2 service flights");
            foreach (StructuralSection s in sim.Sections)
            {
                bool ground = s.transform.position.y < 0.5f && s.Type != SectionType.Stair;
                if (ground) Assert.IsFalse(s.CanCollapse, $"{s.name}: nothing below the ground floor");
                else if (s.name.StartsWith("Stairs")) Assert.IsFalse(s.CanCollapse, $"{s.name}: the way down never falls (GDD 6.4)");
                else Assert.IsTrue(s.CanCollapse, $"{s.name} should be able to collapse");
            }
            Assert.AreEqual(2, sim.Sections.Count(s => s.name.StartsWith("Escalator") && s.CanCollapse));
            Assert.Greater(sim.Sections.Count(s => s.Type == SectionType.Balcony), 30, "walkways around the atrium");
        }

        [Test]
        public void WalksInThroughTheMainEntrance()
        {
            rig.Teleport(new Vector3(22f, 0.05f, -3f));
            rig.Settle();
            Assert.GreaterOrEqual(rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.z > 3f, 4f), 0f, "blocked at the entrance");
        }

        [Test]
        public void RidesTheEscalatorOntoTheBridge()
        {
            rig.Teleport(new Vector3(18f, 0.05f, 10f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 3.8f && rig.Player.transform.position.z > 20f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void ClimbsTheSecondEscalatorFromTheBridge()
        {
            rig.Teleport(new Vector3(30f, 4.05f, 22f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.down), () => rig.Player.transform.position.y > 7.8f && rig.Player.transform.position.z < 12f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void ClimbsBothServiceFlights()
        {
            rig.Teleport(new Vector3(46f, 0.05f, 38f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.down), () => rig.Player.transform.position.y > 3.8f && rig.Player.transform.position.z < 28f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"first flight: stuck at {rig.Player.transform.position}");

            rig.Teleport(new Vector3(42f, 4.05f, 26f));
            rig.Settle();
            t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 7.8f && rig.Player.transform.position.z > 36f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"second flight: stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void TheAtriumRailingStopsAWalkOffTheEdge()
        {
            rig.Teleport(new Vector3(14f, 4.05f, 18f));
            rig.Settle();
            rig.RunUntil(Frame(Vector2.right), () => false, 2f);
            Assert.Less(rig.Player.transform.position.x, 16f, "walked through the railing into the atrium");
            Assert.AreEqual(4f, rig.Player.transform.position.y, 0.2f);
        }

        [Test]
        public void TheGalleryHasAWideDoorForTheStatue()
        {
            // 3 m wide box through the gallery door (floor 2, x 20..28 at z = 32).
            bool blocked = Physics.CheckBox(new Vector3(22f, 9.6f, 32f), new Vector3(1.5f, 1.2f, 0.4f), Quaternion.identity,
                ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
            Assert.IsFalse(blocked, "the statue can't leave the gallery");
        }

        [Test]
        public void TheLoadingBayDoorOpensOntoTheTruck()
        {
            rig.Teleport(new Vector3(42f, 0.05f, 26f), 90f);
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.x > 50f, 6f);
            Assert.GreaterOrEqual(t, 0f, $"blocked between the loading bay and the truck at {rig.Player.transform.position}");
            Assert.Greater(rig.Player.transform.position.y, 0.2f, "up the truck's ramp");
        }

        [Test]
        public void TheEscalatorTopHasNoGapBesideIt()
        {
            // Floor 2, beside the top of Escalator_12 (z = 12, the escalator spans x 28.8..31.2).
            rig.Teleport(new Vector3(28.4f, 8.05f, 10f));
            rig.Settle();
            rig.RunUntil(Frame(Vector2.up), () => false, 1.5f);
            Assert.Greater(rig.Player.transform.position.y, 7.5f, "fell into the atrium beside the escalator");
        }
    }
}
