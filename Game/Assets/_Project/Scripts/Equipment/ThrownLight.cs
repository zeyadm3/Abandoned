using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A thrown glow stick or flare (0.17.7): host-simulated, everyone follows its NetworkTransform; a small
    /// point light (no shadows) burns for its life, fading at the end. A flare is bright, hisses, and is a
    /// little noisy; a glow stick is dim, silent and lasts long.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownLight : NetworkBehaviour
    {
        [SerializeField] private Light lamp;
        [SerializeField, Min(1f)] private float life = 600f;
        [SerializeField] private bool flare;

        private float spawnedAt, baseIntensity, nextHiss, nextNoise;

        public override void OnNetworkSpawn()
        {
            spawnedAt = Time.time;
            GetComponent<Rigidbody>().isKinematic = !IsServer;
            if (lamp == null) return;
            baseIntensity = lamp.intensity;
            lamp.shadows = LightShadows.None;
            LevelStoreys.LightEverything(lamp);
            if (flare) GameAudio.Play(SoundId.FlareIgnite, transform.position, 1f);
        }

        public void Launch(Vector3 velocity) => GetComponent<Rigidbody>().linearVelocity = velocity;

        private void Update()
        {
            if (!IsSpawned) return;
            float age = Time.time - spawnedAt, left = life - age;
            if (lamp != null)
            {
                float flicker = flare ? 0.85f + 0.15f * Mathf.PerlinNoise(Time.time * 9f, 0.3f) : 1f;
                lamp.intensity = baseIntensity * flicker * Mathf.Clamp01(left / 10f);
            }
            if (flare && Time.time >= nextHiss && left > 0f)
            {
                nextHiss = Time.time + 1.4f;
                GameAudio.Play(SoundId.FlareHiss, transform.position, 1f);
            }
            if (!IsServer) return;
            if (flare && Time.time >= nextNoise)
            {
                nextNoise = Time.time + 3f;
                NoiseSystem.Emit(transform.position, 0.2f, NoiseSource.Other);
            }
            if (left <= 0f) NetworkObject.Despawn(true);
        }

#if UNITY_EDITOR
        public void EditorSetup(Light light, float seconds, bool isFlare)
        {
            lamp = light;
            life = seconds;
            flare = isFlare;
        }
#endif
    }
}
