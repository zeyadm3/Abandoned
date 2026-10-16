using System.IO;
using System.Linq;
using Abandoned.Contracts;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;

namespace Abandoned.Tests
{
    /// <summary>Contract data and the player build must agree about every playable destination.</summary>
    public class LocationBuildWiringTests
    {
        [Test]
        public void EveryContractLocationIsBuiltAndEveryBuiltDestinationHasContractData()
        {
            ContractConfig config = AssetDatabase.LoadAssetAtPath<ContractConfig>(CompanyContentBuilder.ContractConfigPath);
            Assert.IsNotNull(config, "the contract config exists");
            Assert.IsNotNull(config.Locations, "the contract config wires its location catalog");

            string[] catalogScenes = config.Locations.All.Where(location => location != null)
                .Select(location => location.Scene).OrderBy(scene => scene).ToArray();
            string[] builtScenes = BuildScenes.SalvageScenes.Select(Path.GetFileNameWithoutExtension)
                .OrderBy(scene => scene).ToArray();

            CollectionAssert.AreEqual(builtScenes, catalogScenes,
                "a board destination must have a matching player-build scene, and vice versa");
            CollectionAssert.AllItemsAreUnique(config.Locations.All.Where(location => location != null).Select(location => location.Id));
            CollectionAssert.AllItemsAreUnique(catalogScenes);
            CollectionAssert.Contains(catalogScenes, ContractGenerator.MallScene, "the original destination remains the fallback");
        }
    }
}
