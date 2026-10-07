using Abandoned.Audio;
using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A thrown noise maker (GDD 12: distract the Blind One): it lands, ticks for a few seconds, then
    /// shrieks, a loud noise threats hear (host) and a sound everyone hears, then it's spent.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NoiseMakerDevice : NetworkBehaviour
    {
        [SerializeField, Min(0f)] private float delay = 3f;
        [SerializeField, Min(0.5f)] private float duration = 8f;
        [SerializeField, Range(0f, 1f)] private float loudness = 0.9f;
        [SerializeField, Min(0.1f)] private float interval = 0.5f;

        private readonly NetworkVariable<bool> shrieking = new();
        private float spawnedAt, nextNoise, nextSound;

        public bool Shrieking => shrieking.Value;

        public override void OnNetworkSpawn()
        {
            spawnedAt = Time.time;
            // Only the host simulates it; everyone else follows its NetworkTransform.
            GetComponent<Rigidbody>().isKinematic = !IsServer;
        }

        /// <summary>Host: throw it from where the user stands.</summary>
        public void Launch(Vector3 velocity) => GetComponent<Rigidbody>().linearVelocity = velocity;

        private void Update()
        {
            if (!IsSpawned) return;
            if (shrieking.Value && Time.time >= nextSound)
            {
                nextSound = Time.time + interval * 2f;
                AudioSource.PlayClipAtPoint(PlaceholderAudio.Shriek(), transform.position, 1f);
            }
            if (!IsServer) return;
            float age = Time.time - spawnedAt;
            if (!shrieking.Value && age >= delay) shrieking.Value = true;
            if (shrieking.Value && Time.time >= nextNoise)
            {
                nextNoise = Time.time + interval;
                NoiseSystem.Emit(transform.position, loudness, NoiseSource.Other);
            }
            if (age >= delay + duration) NetworkObject.Despawn(true);
        }
    }
}
