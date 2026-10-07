using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// M7.5: a player's outfit is seen by everyone (coverall tint and hat), their own hat only casts a
    /// shadow; the HQ wardrobe opens from the lockers and closes back into the game.
    /// </summary>
    public class CosmeticsTests
    {
        private string savedCoverall, savedHat;

        [SetUp]
        public void SaveProfile()
        {
            savedCoverall = PlayerProfile.Coverall;
            savedHat = PlayerProfile.Hat;
        }

        [TearDown]
        public void RestoreProfile() => PlayerProfile.Wear(savedCoverall, savedHat);

        [UnityTest]
        public IEnumerator TheCrewSeesWhatAClientWears()
        {
            yield return CleanWorld();
            var net = new NetTestHarness();
            try
            {
                net.BuildArena();
                yield return net.StartSession(clients: 1);
                NetworkBootstrap client = net.Clients.First();
                PlayerCosmetics mine = OwnPlayer(client).GetComponent<PlayerCosmetics>();
                CosmeticCatalog catalog = mine.Catalog;
                Assert.IsNotNull(catalog, "the player prefab has the catalog");
                int navy = CosmeticCatalog.IndexOf(catalog.Coveralls, "navy"), cap = CosmeticCatalog.IndexOf(catalog.Hats, "beanie");
                mine.Wear(new CosmeticChoice((byte)navy, (byte)cap));

                PlayerCosmetics seen = PlayerOf(net.Host, client.Manager.LocalClientId).GetComponent<PlayerCosmetics>();
                yield return WaitFor(() => seen.Choice.Coverall == navy && seen.Choice.Hat == cap, "the host to see the client's outfit", 5f);
                yield return null;
                Assert.IsNotNull(seen.HatInstance, "the host sees the hat");
                Assert.IsTrue(seen.HatInstance.GetComponentsInChildren<Renderer>().All(r => r.shadowCastingMode == ShadowCastingMode.On), "visible on others");
                Assert.IsTrue(mine.HatInstance.GetComponentsInChildren<Renderer>().All(r => r.shadowCastingMode == ShadowCastingMode.ShadowsOnly), "only a shadow on its wearer");

                var body = seen.transform.Find("Body").GetComponent<Renderer>();
                var block = new MaterialPropertyBlock();
                body.GetPropertyBlock(block);
                Color shown = block.GetColor("_BaseColor"), navyColor = catalog.Coverall(navy).Color;
                Assert.Less(Mathf.Abs(shown.r - navyColor.r) + Mathf.Abs(shown.g - navyColor.g) + Mathf.Abs(shown.b - navyColor.b), 1e-3f,
                    $"the coverall is navy on the host (shows {shown})");
                Assert.AreEqual("navy", PlayerProfile.Coverall, "the pick is remembered");
                Transform t = seen.transform;
                Core.ScreenshotCapture.CaptureFrom(t.position + t.forward * 2.2f + Vector3.up * 1.7f, t.position + Vector3.up * 1.4f, "M7_cosmetics_player", 50f);
            }
            finally
            {
                net.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator TheWardrobeOpensFromTheLockersAndClosesIntoTheGame()
        {
            yield return TestMapScene.Load("HQ");
            var lockers = Object.FindAnyObjectByType<WardrobeLocker>();
            Assert.IsNotNull(lockers, "the HQ has lockers");
            Assert.IsNotNull(lockers.UsePrompt(TestMapScene.Player));
            lockers.Use(TestMapScene.Player);
            yield return null;
            Assert.AreEqual(MenuScreen.Wardrobe, MenuUi.Current.Showing);
            Assert.IsTrue(Core.CursorOwner.UiActive);
            yield return MenuTests.Capture(MenuUi.Current, "M7_menu_wardrobe");
            MenuUi.Current.Back();
            yield return null;
            yield return null;
            Assert.AreEqual(MenuScreen.None, MenuUi.Current.Showing, "Done goes straight back to the game");
            Assert.IsFalse(Core.CursorOwner.UiActive);
        }
    }
}
