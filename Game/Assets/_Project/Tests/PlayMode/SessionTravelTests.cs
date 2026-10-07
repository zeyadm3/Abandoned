using System.Collections;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>One session across levels (M6.0): the host travels, the session and its players come along.</summary>
    public class SessionTravelTests
    {
        private static IEnumerator WaitUntil(System.Func<bool> condition, float seconds, string what)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(condition(), what);
        }

        [UnityTest]
        public IEnumerator TheHostTravelsToTheMallAndBackWithoutDroppingTheSession()
        {
            yield return TestBuildingScene.Load("TestBuilding");
            NetworkManager manager = NetworkManager.Singleton;
            NetworkPlayer me = NetworkPlayer.Local;
            yield return WaitUntil(() => SessionTravel.Current != null, 3f, "the session spawned its travel object");
            Assert.AreEqual("TestBuilding", SessionTravel.Current.Level);
            Assert.AreSame(NetworkBootstrap.Persistent, NetworkBootstrap.Instance, "the first scene's session is the game's session");

            SessionTravel.Current.Travel("Mall");
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Mall" && RunState.Current != null && RunState.Current.IsSpawned,
                10f, "the mall loaded and its run started");
            Assert.AreSame(manager, NetworkManager.Singleton, "same NetworkManager");
            Assert.IsTrue(manager.IsListening && manager.IsHost, "still hosting");
            Assert.AreSame(me, NetworkPlayer.Local, "the same player object came along");
            Assert.AreEqual(1, Object.FindObjectsByType<NetworkBootstrap>(FindObjectsSortMode.None).Length, "the mall's own session copy removed itself");
            yield return WaitUntil(() => Vector3.Distance(me.transform.position, PlayerSpawnPoint.PoseFor(0).position) < 1f, 3f, "placed on the mall's spawn");
            Assert.Greater(Object.FindAnyObjectByType<NetworkLootSpawner>().Spawned.Count, 40, "the mall's loot spawned once everyone was there");

            SessionTravel.Current.Travel("TestBuilding");
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "TestBuilding" && Object.FindAnyObjectByType<StructureNetSync>() != null,
                10f, "back in the test building with its structure sync");
            Assert.IsNull(RunState.Current, "the mall's run didn't come back with us");
            Assert.AreSame(me, NetworkPlayer.Local);
            yield return WaitUntil(() => Vector3.Distance(me.transform.position, PlayerSpawnPoint.PoseFor(0).position) < 1f, 3f, "placed on the test building's spawn");
        }
    }
}
