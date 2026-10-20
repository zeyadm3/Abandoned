using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Is this machine in the training job (the house at 14 Larch Court)? Decided by the loaded level's name, so
    /// every machine agrees without a message. In training nothing touches the company (no pay, no quota owed, no
    /// strikes), the building never decays or reroutes itself, monsters only come when a lesson calls them, and
    /// nobody dies. Each system that would otherwise interfere asks <see cref="Active"/>.
    /// </summary>
    public static class TutorialMode
    {
        /// <summary>The training level's scene name (Scenes/Tutorial.unity).</summary>
        public const string Scene = "Tutorial";

        private static bool active;

        public static bool Active => active;

        /// <summary>Host: the lowest a player's health can fall in training (the low-health effects still show).</summary>
        public static float HealthFloor => TutorialConfig.Current.HealthFloor;

        /// <summary>Damage after the training floor: what a hit may really take off a player who has <paramref name="health"/>.</summary>
        public static float Clamp(float health, float amount) => Mathf.Min(amount, Mathf.Max(0f, health - HealthFloor));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Hook()
        {
            Set(false);
            SceneManager.activeSceneChanged -= OnActiveScene;
            SceneManager.activeSceneChanged += OnActiveScene;
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
        }

        // Session travel loads levels Single, so the loaded scene becomes the active one; both events are watched
        // because which fires first differs between the editor and players.
        private static void OnActiveScene(Scene previous, Scene next) => Set(next.name == Scene);

        private static void OnLoaded(Scene scene, LoadSceneMode mode) => Set(SceneManager.GetActiveScene().name == Scene);

        // Achievements and play stats never count in the practice house.
        private static void Set(bool training)
        {
            active = training;
            Core.Achievements.Suspended = training;
        }
    }
}
