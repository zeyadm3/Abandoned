using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Networking
{
    /// <summary>
    /// When a game this machine was in ends (host left, kicked, connection lost, or we left), reload the
    /// scene: a game's world (spawned loot, a mirrored building, no local player) isn't fit to keep
    /// playing solo in. <see cref="SessionEndNotice"/> carries the reason across the reload and stops
    /// the fresh scene auto-hosting. Placeholder for the HQ/menu flow (M5).
    /// </summary>
    public sealed class ReturnToMenu : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;

        private bool pending;

        private void OnEnable()
        {
            if (bootstrap != null) bootstrap.SessionEnded += OnSessionEnded;
        }

        private void OnDisable()
        {
            if (bootstrap != null) bootstrap.SessionEnded -= OnSessionEnded;
        }

        private void OnSessionEnded(bool onPurpose, string reason)
        {
            SessionEndNotice.Set(onPurpose ? string.Empty : reason);
            pending = true;
        }

        // Deferred: NGO is still inside its shutdown when the stop event fires.
        private void Update()
        {
            if (!pending) return;
            pending = false;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogWarning($"[Net] Session ended; '{scene.name}' isn't in the build list, so it isn't reloaded.");
                return;
            }
            string why = SessionEndNotice.Message.Length > 0 ? SessionEndNotice.Message : "left";
            Debug.Log($"[Net] Session ended ({why}); back to the menu.");
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
