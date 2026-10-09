using Abandoned.Audio;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// What happens to a salvager's body, heard by everyone near it (0.14.0): a blow landing, and going down
    /// for good. Driven by the replicated health and death state, so every machine plays it without a new
    /// message. Your own are flat in your ears; a crewmate's come from where they stand.
    /// </summary>
    public static class PlayerBodySounds
    {
        // Slow drains (a few points a second) are felt through the vitals, not as a string of blows.
        private const float SmallestBlow = 3f, FullBlow = 40f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            NetworkPlayer.HealthChanged -= OnHealthChanged;
            NetworkPlayer.HealthChanged += OnHealthChanged;
            NetworkPlayer.DeathChanged -= OnDeathChanged;
            NetworkPlayer.DeathChanged += OnDeathChanged;
        }

        private static void OnHealthChanged(NetworkPlayer player, float before, float after)
        {
            float lost = before - after;
            if (player == null || lost < SmallestBlow) return;
            float volume = Mathf.Lerp(0.45f, 1f, Mathf.InverseLerp(SmallestBlow, FullBlow, lost));
            if (player.IsOwner) GameAudio.PlayUi(SoundId.PlayerHurt, volume);
            else GameAudio.Play(SoundId.PlayerHurt, player.transform.position + Vector3.up, volume);
        }

        private static void OnDeathChanged(NetworkPlayer player, bool dead)
        {
            if (player == null || !dead) return;
            if (player.IsOwner) GameAudio.PlayUi(SoundId.PlayerDeath, 0.9f);
            else GameAudio.Play(SoundId.PlayerDeath, player.transform.position + Vector3.up, 1f);
        }
    }
}
