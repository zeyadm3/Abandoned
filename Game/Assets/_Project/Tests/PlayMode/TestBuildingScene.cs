using System.Collections;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Tests
{
    /// <summary>
    /// Loads TestBuilding the way a solo player gets it: the scene auto-hosts (editor rule) and NGO
    /// spawns the player at spawn point 0. There's no scene-placed player any more.
    /// </summary>
    public static class TestBuildingScene
    {
        public const string Name = "TestBuilding";
        private const int MaxFramesToSpawn = 120;

        public static GameObject Player => NetworkPlayer.Local != null ? NetworkPlayer.Local.gameObject : null;

        public static IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(Name, LoadSceneMode.Single);
            // NetworkBootstrap hosts in Start; the host's player is spawned during StartHost.
            for (int i = 0; i < MaxFramesToSpawn && Player == null; i++) yield return null;
            Assert.IsNotNull(Player, "TestBuilding didn't auto-host a solo player");
        }
    }
}
