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

        public static IEnumerator Load() => Load(Name);

        /// <summary>Any session scene (TestBuilding, Mall): load it and wait for the auto-hosted solo player.</summary>
        public static IEnumerator Load(string sceneName, bool randomRun = false)
        {
            // Each test gets its scene's own session (the persistent one from the last test goes).
            NetworkBootstrap.DestroyPersistent();
            // Tests replay the same run (seed 1) unless they ask for a random one.
            Abandoned.Extraction.RunDirector.ForcedSeed = randomRun ? 0 : 1;
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            // NetworkBootstrap hosts in Start; the host's player is spawned during StartHost.
            for (int i = 0; i < MaxFramesToSpawn && Player == null; i++) yield return null;
            Assert.IsNotNull(Player, $"{sceneName} didn't auto-host a solo player");
        }
    }
}
