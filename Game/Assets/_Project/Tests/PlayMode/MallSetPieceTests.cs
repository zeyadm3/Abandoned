using System.Collections;
using System.Linq;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// M8.5: the piano on the atrium bridge (GDD 28). The bridge's jackpot point only takes the piano,
    /// and the piano alone rests there without breaking the weak bridge: it's the crew that tips it.
    /// </summary>
    public class MallSetPieceTests
    {
        [UnityTest]
        public IEnumerator ThePianoWaitsOnTheWeakBridge()
        {
            yield return TestBuildingScene.Load("Mall");
            NetworkLootSpawner spawner = Object.FindAnyObjectByType<NetworkLootSpawner>();
            LootSpawnPoint bridge = spawner.Points().FirstOrDefault(p => p.name == "LootPoint_1_06_05_J");
            Assert.IsNotNull(bridge, "a jackpot point on the bridge");
            var definitions = Resources.FindObjectsOfTypeAll<LootDefinition>().Where(d => d.Jackpot).ToList();
            Assert.IsTrue(definitions.Where(bridge.Fits).All(d => d.Id == "grand_piano") && definitions.Any(bridge.Fits),
                "only the piano fits there (a 2-ton statue would break the bridge before anyone arrived)");

            StructuralSection tile = Object.FindAnyObjectByType<StructureSimulation>().Sections.First(s => s.name == "Walk_1_6_5");
            NetworkLoot piano = spawner.Spawned.FirstOrDefault(l => l != null && l.Item.Definition.Id == "grand_piano");
            if (piano == null) Assert.Ignore("this run's seed put no piano in the mall"); // seed 1 does; a changed planner might not
            piano.Grabbable.Body.position = tile.transform.position + Vector3.up * 0.6f;
            piano.transform.position = piano.Grabbable.Body.position;
            yield return new WaitForSeconds(4f);
            Assert.IsFalse(tile.IsCollapsed, $"the piano alone holds (load {tile.Load:0}/{tile.Capacity:0} kg)");
            Assert.Greater(tile.Load, 400f, "and it does weigh on the bridge");
        }
    }
}
