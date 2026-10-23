using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>Every machine: the van's engine (louder and higher with speed, silent when off) and its horn, from the synced state.</summary>
    public class VanSound : MonoBehaviour
    {
        [SerializeField] private WorldVan van;
        [SerializeField] private float hornEvery = .85f;

        private SoundLoop engine;
        private float nextHorn;

        private void Awake()
        {
            if (van == null) van = GetComponent<WorldVan>();
        }

        private void Update()
        {
            if (van == null || !van.IsSpawned) return;
            VanNetState s = van.State;
            engine ??= GameAudio.Loop(SoundId.EngineIdle, transform, "VanEngine", spatial: true);
            float pace = Mathf.Clamp01(van.Drive.SpeedKmh / 90f);
            engine.Level = s.EngineOn ? Mathf.Lerp(.55f, 1f, pace) : 0f;
            engine.Pitch = Mathf.Lerp(.8f, 1.7f, pace);
            if (!s.Horn || Time.time < nextHorn) return;
            nextHorn = Time.time + hornEvery;
            GameAudio.Play(SoundId.Horn, transform.position + transform.forward * 5f, 1f);
        }
    }
}
