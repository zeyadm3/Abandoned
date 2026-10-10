using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Light player pushing (0.21.2), on this machine for its own player: walking briskly into a crewmate
    /// shoves them a little in the direction you're going.
    /// </summary>
    public class PlayerShove : MonoBehaviour
    {
        private const float Contact = 0.75f, MinSpeed = 1.2f, Share = 0.6f;

        private Vector3 last;
        private float next;

        private void Update()
        {
            NetworkPlayer me = NetworkPlayer.Local;
            if (me == null || me.IsDead || me.IsHiding || Time.deltaTime <= 0f) return;
            Vector3 velocity = (me.transform.position - last) / Time.deltaTime;
            last = me.transform.position;
            velocity.y = 0f;
            if (velocity.magnitude < MinSpeed || velocity.magnitude > 12f || Time.time < next) return;
            foreach (NetworkPlayer other in NetworkPlayer.All)
            {
                if (other == null || other == me || other.IsDead || !other.IsSpawned) continue;
                Vector3 gap = other.transform.position - me.transform.position;
                if (Mathf.Abs(gap.y) > 1f) continue;
                gap.y = 0f;
                if (gap.magnitude > Contact || Vector3.Dot(gap.normalized, velocity.normalized) < 0.5f) continue;
                other.RequestShove(velocity * Share);
                Audio.GameAudio.Play(Audio.SoundId.Shove, other.transform.position + Vector3.up, 1f);
                next = Time.time + 0.5f;
                break;
            }
        }
    }
}
