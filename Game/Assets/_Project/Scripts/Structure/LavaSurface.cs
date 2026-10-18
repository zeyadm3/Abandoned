using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Every machine, cosmetic only: the crust drifts along the channel and the glow lights breathe. Not
    /// networked and not in step with anyone else's; the lava's rule lives in <see cref="LavaVolume"/>.
    /// The lights don't follow the building's power: molten iron glows whether the breakers are on or not.
    /// </summary>
    public class LavaSurface : MonoBehaviour
    {
        [SerializeField] private Renderer surface;
        [SerializeField] private Light[] glows;

        private Material instance;
        private float[] baseIntensity;
        private Vector2 offset;

        private void Awake()
        {
            if (surface != null) instance = surface.material;
            baseIntensity = new float[glows != null ? glows.Length : 0];
            for (int i = 0; i < baseIntensity.Length; i++) baseIntensity[i] = glows[i] != null ? glows[i].intensity : 0f;
            // The roar comes from along the channel, one source at each glow, so walking its length you hear it
            // move with you. Batch runs have no audio.
            if (Application.isBatchMode) return;
            for (int i = 0; i < baseIntensity.Length; i++)
            {
                if (glows[i] == null) continue;
                Audio.SoundLoop loop = Audio.GameAudio.Loop(Audio.SoundId.LavaRumble, glows[i].transform, "LavaRumble", spatial: true);
                loop.Level = .7f;
            }
        }

        private void Update()
        {
            LavaConfig config = LavaConfig.Current;
            if (instance != null)
            {
                offset += config.CrustDrift * Time.deltaTime;
                offset.x %= 1f;
                offset.y %= 1f;
                instance.mainTextureOffset = offset;
            }
            float t = Time.time * config.FlickerSpeed;
            for (int i = 0; i < baseIntensity.Length; i++)
            {
                if (glows[i] == null) continue;
                // Each light on its own noise track, so the channel shimmers instead of pulsing as one.
                float n = Mathf.PerlinNoise(t + i * 1.73f, i * 0.37f) * 2f - 1f;
                glows[i].intensity = baseIntensity[i] * (1f + n * config.FlickerAmount);
            }
        }

        private void OnDestroy()
        {
            if (instance != null) Destroy(instance);
        }

#if UNITY_EDITOR
        public void EditorSetup(Renderer lavaSurface, Light[] glowLights)
        {
            surface = lavaSurface;
            glows = glowLights;
        }
#endif
    }
}
