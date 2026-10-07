using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>M9.4 gear in the mall: the backpack's pockets, the support jack's brace, the crowbar's strikes.</summary>
    public class StructureGearTests
    {
        private PlayerTestRig rig;
        private PlayerEquipment gear;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestBuildingScene.Load("Mall");
            rig = ForExisting(TestBuildingScene.Player);
            gear = TestBuildingScene.Player.GetComponent<PlayerEquipment>();
        }

        private IEnumerator Equip(string id)
        {
            gear.RequestEquip(0, gear.Catalog.IndexOf(id));
            float until = Time.time + 3f;
            while (Time.time < until && (gear.InSlot(0) == null || gear.InSlot(0).Id != id)) yield return null;
            Assert.AreEqual(id, gear.InSlot(0)?.Id, $"{id} in hand");
        }

        private IEnumerator PressUse()
        {
            // The rig turns the reader off (it drives the motor by hand): on just for the press.
            var reader = gear.GetComponent<PlayerInputReader>();
            reader.enabled = true;
            reader.Override = new PlayerInputFrame(default, default, false, false, false, false, true, true, false, false, false, false);
            yield return null;
            reader.Override = default(PlayerInputFrame);
            yield return null;
            reader.Override = null;
            reader.enabled = false;
        }

        // A sound upper-floor tile with NavMesh-free middle ground: stand in its centre.
        private StructuralSection StandOnUpperTile()
        {
            StructuralSection tile = Object.FindAnyObjectByType<StructureSimulation>().Sections
                .Where(s => s.CanCollapse && s.Type == SectionType.Floor && s.transform.position.y > 3f && s.Stage == StructuralStage.Stable && s.Load < 1f)
                .OrderBy(s => s.name).First();
            rig.Teleport(tile.transform.position + Vector3.up * 0.05f);
            rig.Settle();
            return tile;
        }

        [UnityTest]
        public IEnumerator TheBackpackAddsPockets()
        {
            int before = rig.Player.GetComponent<Interaction.PlayerCarrier>().Inventory.Capacity;
            yield return Equip("backpack");
            yield return null;
            Assert.AreEqual(before + 2, rig.Player.GetComponent<Interaction.PlayerCarrier>().Inventory.Capacity);
        }

        [UnityTest]
        public IEnumerator TheSupportJackBracesTheFloorYouStandOn()
        {
            StructuralSection tile = StandOnUpperTile();
            float capacity = tile.Capacity;
            yield return Equip("support_jack");
            yield return PressUse();
            float until = Time.time + 3f;
            while (Time.time < until && Object.FindAnyObjectByType<SupportJack>() == null) yield return null;
            SupportJack jack = Object.FindAnyObjectByType<SupportJack>();
            Assert.IsNotNull(jack, "a jack post appeared");
            Assert.AreSame(tile, jack.Section);
            Assert.AreEqual(capacity * jack.CapacityMultiplier, tile.Capacity, 1f, "the floor holds more");
            Assert.Less(jack.transform.position.y, tile.transform.position.y - 2f, "standing on the floor below");
            Assert.IsNull(gear.InSlot(0), "single use");
            tile.ResetState();
            Assert.AreEqual(capacity, tile.Capacity, 1f, "a restore (the next run) takes the brace away");
        }

        [UnityTest]
        public IEnumerator TheCrowbarBreaksAFloorThroughACrackingWarning()
        {
            StructuralSection tile = StandOnUpperTile();
            rig.Look.ApplyLook(new Vector2(0f, -2000f)); // look straight down
            rig.Run(Frame(), 0.05f);
            yield return Equip("crowbar");
            bool cracked = false;
            for (int i = 0; i < 20 && !tile.IsCollapsed && tile.Stage != StructuralStage.Failing; i++)
            {
                yield return PressUse();
                cracked |= tile.Stage == StructuralStage.Cracking;
                yield return new WaitForSeconds(0.75f);
            }
            Assert.Less(tile.HealthFraction, 0.5f, $"strikes hurt it (health {tile.HealthFraction:P0}, stage {tile.Stage})");
            Assert.IsTrue(cracked, "it cracked before it could fail");
            Assert.AreEqual("crowbar", gear.InSlot(0)?.Id, "a tool: still in hand");
        }
    }
}
