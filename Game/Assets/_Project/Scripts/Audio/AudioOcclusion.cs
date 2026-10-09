using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// What stands between a sound and the listener (0.14.0): each wall or floor slab on the straight line
    /// dulls the sound and, for most sounds, quietens it; far sounds lose their top end to the air. One ray
    /// per sound when it starts. A collapsed section has no collider, so sound comes through the hole.
    /// </summary>
    public static class AudioOcclusion
    {
        private static readonly RaycastHit[] hits = new RaycastHit[8];
        private static int mask;

        /// <summary>For the F1 overlay: walls counted for the last occluded sound.</summary>
        public static int LastWalls { get; private set; }

        public static void Evaluate(SoundOcclusion mode, Vector3 ear, Vector3 source, float maxDistance, out float gain, out float cutoff)
        {
            gain = 1f;
            cutoff = AudioVoice.OpenCutoff;
            AudioConfig config = AudioConfig.Current;
            Vector3 to = source - ear;
            float distance = to.magnitude;
            if (distance > maxDistance) return; // silent anyway
            float far = Mathf.InverseLerp(config.FarFrom, 1f, distance / Mathf.Max(0.1f, maxDistance));
            if (far > 0f) cutoff = Mathf.Lerp(AudioVoice.OpenCutoff, config.FarCutoff, far * far);
            if (mode == SoundOcclusion.None || distance < 1f) return;

            int walls = CountWalls(ear, to / distance, distance - 0.4f, config.MaxWalls);
            LastWalls = walls;
            if (walls == 0) return;
            float behind = Mathf.Lerp(config.WallCutoff.x, config.WallCutoff.y, Mathf.InverseLerp(1f, config.MaxWalls, walls));
            cutoff = Mathf.Min(cutoff, behind);
            gain = Mathf.Pow(config.WallGain, walls);
            if (mode == SoundOcclusion.Soft) gain = Mathf.Max(gain, config.SoftFloor);
        }

        // The ray stops short of the source: a sound placed on a surface shouldn't be hidden by that surface.
        private static int CountWalls(Vector3 ear, Vector3 direction, float length, int most)
        {
            if (length <= 0f) return 0;
            if (mask == 0) mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            int n = Physics.RaycastNonAlloc(ear, direction, hits, length, mask, QueryTriggerInteraction.Ignore);
            int walls = 0;
            for (int i = 0; i < n && walls < most; i++)
            {
                // Monsters, gear and doors that move are things in the room, not the room.
                if (hits[i].collider.GetComponentInParent<NetworkObject>() != null) continue;
                walls++;
            }
            return walls;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            mask = 0;
            LastWalls = 0;
        }
    }
}
