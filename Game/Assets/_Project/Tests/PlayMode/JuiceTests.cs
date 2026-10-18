using System.Collections;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Structure;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M8.4: the floor under you is announced before it goes; loot reaching the truck pops its value.</summary>
    public class JuiceTests
    {
        [UnityTest]
        public IEnumerator ACrackingFloorUnderYouIsAnnounced()
        {
            yield return TestMapScene.Load("Mall");
            GameObject me = TestMapScene.Player;
            var warning = Object.FindAnyObjectByType<FloorWarningHud>();
            Assert.IsNotNull(warning, "the session has the floor warning");
            // Stand on a sound upper-floor tile, then hammer it.
            StructuralSection tile = Object.FindAnyObjectByType<StructureSimulation>().Sections
                .First(s => s.CanCollapse && s.Type != SectionType.Stair && s.transform.position.y > 3f && s.Stage == StructuralStage.Stable
                            && UnityEngine.AI.NavMesh.SamplePosition(s.transform.position, out _, 0.3f, UnityEngine.AI.NavMesh.AllAreas)); // clear floor to stand on
            PlayerTestRig rig = PlayerTestRig.ForExisting(me);
            rig.Teleport(tile.transform.position + Vector3.up * 0.05f);
            rig.Settle();
            yield return null;
            Assert.IsNull(warning.Warning, "a sound floor says nothing");
            // Only a failing floor warns ("FLOOR GIVING WAY · MOVE"); cracks, tint and dust show the rest.
            for (int i = 0; i < 20 && tile.Stage < StructuralStage.Failing; i++) tile.ApplyImpact(100000f);
            Assert.AreEqual(StructuralStage.Failing, tile.Stage);
            yield return null;
            yield return null;
            Assert.AreEqual(StructuralStage.Failing, warning.Warning, "standing on it: warned");
        }

        [UnityTest]
        public IEnumerator LootReachingTheTruckPopsItsValue()
        {
            yield return TestMapScene.Load("Mall");
            GameObject me = TestMapScene.Player;
            TruckCargo truck = TruckCargo.Current;
            NetworkLoot cargo = Object.FindAnyObjectByType<NetworkLootSpawner>().Spawned
                .First(l => l != null && l.Item.Definition.CarryClass == CarryClass.OneHand && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Medium);
            yield return null;
            // Money shows only while you scan (0.11.0 scan-only values).
            me.GetComponent<LootTags>().Scan();
            int before = FloatingTextOverlay.Instance.ActiveCount;
            Vector3 bay = truck.transform.TransformPoint(new Vector3(0f, 1.2f, -1.2f));
            cargo.Grabbable.Body.position = bay;
            cargo.transform.position = bay;
            float until = Time.time + 4f;
            while (Time.time < until && RunState.Current.State.Haul == 0) yield return null;
            Assert.Greater(RunState.Current.State.Haul, 0, "counted as haul");
            yield return null;
            Assert.Greater(FloatingTextOverlay.Instance.ActiveCount, before, "a +$ pop-up over the truck");
        }
    }
}
