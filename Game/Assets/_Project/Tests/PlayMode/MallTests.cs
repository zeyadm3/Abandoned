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
    /// The saved mall with the real player: every floor piece is a section with the right collapse rule, and the routes
    /// work (the front entrance, both escalators, the service stairs, the haul ramps, the truck's ramp) while railings
    /// keep players out of the atrium. Coordinates follow the 0.15 plan (MallLayout): 4 m tiles, x 0..56, z 0..48,
    /// floors at y = -4 (basement), 0 and 4; the crew arrives in the front court (z about -13).
    /// </summary>
    public class MallTests
    {
        private const string SceneName = "Mall";
        private PlayerTestRig rig;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load(SceneName);
            rig = ForExisting(TestMapScene.Player);
        }

        [Test]
        public void EveryFloorPieceIsASectionAndOnlyTheWayDownAndSolidGroundCantFall()
        {
            StructureSimulation sim = Object.FindAnyObjectByType<StructureSimulation>();
            Assert.IsNotNull(sim);
            Assert.Greater(sim.Sections.Count, 300, "every floor tile, walkway, escalator, stair and ramp");
            var basement = sim.Sections.Where(s => s.transform.position.y < -3.5f && s.Type == SectionType.Floor)
                .Select(s => new Vector2(s.transform.position.x, s.transform.position.z)).ToList();
            foreach (StructuralSection s in sim.Sections)
            {
                Vector3 p = s.transform.position;
                if (s.name.StartsWith("Escalator")) Assert.IsTrue(s.CanCollapse, $"{s.name}: escalators can fall");
                else if (s.Type == SectionType.Stair || s.name.StartsWith("RampTurn"))
                    Assert.IsFalse(s.CanCollapse, $"{s.name}: the way down never falls (GDD 6.4)");
                else if (p.y < -3.5f) Assert.IsFalse(s.CanCollapse, $"{s.name}: nothing below the basement");
                else if (Mathf.Abs(p.y) < 0.5f)
                {
                    // Only into the basement, and never the permanent service aisle above it.
                    bool overBasement = basement.Any(b => Vector2.Distance(b, new Vector2(p.x, p.z)) < 0.5f);
                    if (!overBasement) Assert.IsFalse(s.CanCollapse, $"{s.name}: solid ground never falls");
                }
            }
            Assert.AreEqual(2, sim.Sections.Count(s => s.name.StartsWith("Escalator") && s.CanCollapse));
            Assert.Greater(sim.Sections.Count(s => Mathf.Abs(s.transform.position.y) < 0.5f && s.CanCollapse), 0, "some of the ground floor can drop into the basement");
            Assert.Greater(sim.Sections.Count(s => s.transform.position.y > 3.5f && s.CanCollapse), 50, "the upper floor can fall");
            Assert.Greater(sim.Sections.Count(s => s.transform.position.y > 3.5f && !s.CanCollapse), 20, "but its permanent aisle can't");
            Assert.Greater(sim.Sections.Count(s => s.Type == SectionType.Balcony), 20, "walkways around the atrium");
            Assert.Greater(sim.Sections.Count(s => s.Type == SectionType.Balcony && s.CanCollapse), 0, "and the bridge across it is a risk");
        }

        [Test]
        public void WalksInThroughTheMainEntrance()
        {
            rig.Teleport(new Vector3(28f, 0.05f, -4f));
            rig.Settle();
            Assert.GreaterOrEqual(rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.z > 3f, 5f), 0f,
                $"blocked at the entrance at {rig.Player.transform.position}");
        }

        [Test]
        public void RidesTheWestEscalatorUpstairs()
        {
            rig.Teleport(new Vector3(22f, 0.05f, 13f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 3.8f && rig.Player.transform.position.z > 24f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void RidesTheEastEscalatorUpstairs()
        {
            rig.Teleport(new Vector3(34f, 0.05f, 13f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 3.8f && rig.Player.transform.position.z > 24f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void ClimbsTheServiceStairs()
        {
            // Stairs_G1 climbs north beside the service core (x 45.55, z 36 -> 44).
            rig.Teleport(new Vector3(45.55f, 0.05f, 34f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 3.8f && rig.Player.transform.position.z > 43f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void ClimbsOutOfTheBasement()
        {
            // BasementServiceStairs climbs north from the basement (x 45.55, y -4, z 36 -> 44) to the ground floor.
            rig.Teleport(new Vector3(45.55f, -3.95f, 34f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > -0.2f && rig.Player.transform.position.z > 43f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void HaulsUpFromTheBasement()
        {
            // HaulRamp_-1_A north to the half-storey turn, HaulRamp_-1_B south to the ground floor.
            rig.Teleport(new Vector3(49.65f, -3.95f, 34f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > -2.2f && rig.Player.transform.position.z > 44.5f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"first flight: stuck at {rig.Player.transform.position}");
            rig.Teleport(new Vector3(53.85f, -1.95f, 46f));
            rig.Settle();
            t = rig.RunUntil(Frame(Vector2.down), () => rig.Player.transform.position.y > -0.2f && rig.Player.transform.position.z < 37f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"second flight: stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void ClimbsBothHaulRampFlights()
        {
            // HaulRamp_0_A runs north to the half-storey turn (z 36 -> 44); HaulRamp_0_B comes back south to floor 1.
            rig.Teleport(new Vector3(49.65f, 0.05f, 34f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 1.8f && rig.Player.transform.position.z > 44.5f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"first flight: stuck at {rig.Player.transform.position}");

            rig.Teleport(new Vector3(53.85f, 2.05f, 46f));
            rig.Settle();
            t = rig.RunUntil(Frame(Vector2.down), () => rig.Player.transform.position.y > 3.8f && rig.Player.transform.position.z < 37f, 8f);
            Assert.GreaterOrEqual(t, 0f, $"second flight: stuck at {rig.Player.transform.position}");
        }

        [Test]
        public void TheAtriumRailingStopsAWalkOffTheEdge()
        {
            // The west walkway beside the open atrium (Walk_1_4_7); east of it is the drop.
            rig.Teleport(new Vector3(18f, 4.05f, 30f));
            rig.Settle();
            rig.RunUntil(Frame(Vector2.right), () => false, 2f);
            Assert.Less(rig.Player.transform.position.x, 20.5f, "walked through the railing into the atrium");
            Assert.AreEqual(4f, rig.Player.transform.position.y, 0.3f);
        }

        [Test]
        public void TheEscalatorWellHasARailing()
        {
            // Floor 1, east of the east escalator's opening (x 32..36, z 16..24): walk west into it.
            rig.Teleport(new Vector3(38f, 4.05f, 20f), -90f);
            rig.Settle();
            rig.RunUntil(Frame(Vector2.up), () => false, 2f);
            Assert.Greater(rig.Player.transform.position.y, 3.5f, $"fell into the escalator well at {rig.Player.transform.position}");
        }

        [Test]
        public void StoreDoorwaysFitHugeLoot()
        {
            // The four storefront doorways on each floor are 4 m: a 3 m wide crate passes (0.16.0 doors slide aside).
            int blocking = ~LayerMask.GetMask("Player", "Loot", "Debris", "Ignore Raycast");
            foreach ((Vector3 at, bool alongX) in new[] { (new Vector3(24f, 0f, 6f), true), (new Vector3(32f, 0f, 6f), true),
                         (new Vector3(16f, 0f, 22f), true), (new Vector3(40f, 0f, 22f), true) })
            foreach (float y in new[] { 0f, 4f })
            {
                Vector3 centre = at + Vector3.up * (y + 1.3f);
                Vector3 half = alongX ? new Vector3(0.3f, 1f, 1.5f) : new Vector3(1.5f, 1f, 0.3f);
                Collider[] hits = Physics.OverlapBox(centre, half, Quaternion.identity, blocking, QueryTriggerInteraction.Ignore)
                    .Where(c => c.GetComponentInParent<Abandoned.Extraction.SlidingDoor>() == null && c.GetComponentInParent<Abandoned.Extraction.RollerShutter>() == null).ToArray();
                Assert.IsEmpty(hits, $"doorway at {centre}: {string.Join(", ", hits.Select(h => h.name))}");
            }
        }

        [Test]
        public void TheTruckRampIsReachableFromTheEntrance()
        {
            // The collection truck stands in the front court, rear (z about -11) toward the mall, ramp down to the court.
            rig.Teleport(new Vector3(36f, 0.05f, -3f), 180f);
            rig.Settle();
            float t = rig.RunUntil(Frame(Vector2.up), () => rig.Player.transform.position.y > 0.6f, 6f);
            Assert.GreaterOrEqual(t, 0f, $"couldn't walk up the truck's ramp (at {rig.Player.transform.position})");
        }
    }
}
