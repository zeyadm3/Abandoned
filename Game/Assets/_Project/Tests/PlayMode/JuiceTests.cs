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
            yield return TestBuildingScene.Load("Mall");
            GameObject me = TestBuildingScene.Player;
            var warning = Object.FindAnyObjectByType<FloorWarningHud>();
            Assert.IsNotNull(warning, "the session has the floor warning");
            // Stand on a sound upper-floor tile, then hammer it.
            StructuralSection tile = Object.FindAnyObjectByType<StructureSimulation>().Sections
                .First(s => s.CanCollapse && s.Type != SectionType.Stair && s.transform.position.y > 3f && s.Stage == StructuralStage.Stable);
            PlayerTestRig rig = PlayerTestRig.ForExisting(me);
            rig.Teleport(tile.transform.position + Vector3.up * 0.05f);
            rig.Settle();
            yield return null;
            Assert.IsNull(warning.Warning, "a sound floor says nothing");
            for (int i = 0; i < 20 && tile.Stage < StructuralStage.Cracking; i++) tile.ApplyImpact(100000f);
            Assert.GreaterOrEqual(tile.Stage, StructuralStage.Cracking);
            yield return null;
            yield return null;
            Assert.GreaterOrEqual(warning.Warning, StructuralStage.Cracking, "standing on it: warned");
        }

        [UnityTest]
        public IEnumerator LootReachingTheTruckPopsItsValue()
        {
            yield return TestBuildingScene.Load("Mall");
            TruckCargo truck = TruckCargo.Current;
            NetworkLoot cargo = Object.FindAnyObjectByType<NetworkLootSpawner>().Spawned
                .First(l => l != null && l.Item.Definition.CarryClass == CarryClass.OneHand && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Medium);
            yield return null;
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
