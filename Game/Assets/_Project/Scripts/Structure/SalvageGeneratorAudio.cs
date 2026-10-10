using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Crew generator bed through the existing sound catalog, mix and occlusion pipeline.</summary>
    public sealed class SalvageGeneratorAudio : MonoBehaviour
    {
        [SerializeField] private SalvagePresentationConfig presentation;
        [SerializeField] private bool running = true;
        private bool attached = true;
        private SoundLoop engine;
        private AudioLowPassFilter lowPass;
        private float occlusionGain = 1f;
        private float nextOcclusion;

        private float Near => presentation != null ? presentation.GeneratorNearDistance : 1.5f;
        private float Far => presentation != null ? presentation.GeneratorFarDistance : 15f;

        private void Start()
        {
            if (Application.isBatchMode) return;
            engine = GameAudio.Loop(SoundId.EngineIdle, transform, "ClearanceGeneratorLoop", spatial: true);
            engine.SetRange(Near, Far, AudioRolloffMode.Logarithmic);
            engine.Source.transform.localPosition = Vector3.up * .45f;
            lowPass = engine.Source.gameObject.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = AudioVoice.OpenCutoff;
        }

        private void Update()
        {
            if (engine == null) return;
            if (!running || !attached) { engine.Level = 0f; return; }
            if (Time.unscaledTime >= nextOcclusion)
            {
                nextOcclusion = Time.unscaledTime + .33f;
                if (GameAudio.TryEar(out Vector3 ear))
                {
                    AudioOcclusion.Evaluate(SoundOcclusion.Full, ear, engine.Source.transform.position,
                        Far, out occlusionGain, out float cutoff);
                    if (lowPass != null) lowPass.cutoffFrequency = cutoff;
                }
                else occlusionGain = 0f;
            }
            engine.Level = (presentation != null ? presentation.GeneratorVolume : .18f) * occlusionGain;
            engine.Pitch = presentation != null ? presentation.GeneratorPitch : .87f;
        }

        private void OnDisable() { if (engine != null) engine.Level = 0f; }
        private void OnDestroy() { if (engine != null) engine.Level = 0f; }

        public void SetAttached(bool on)
        {
            attached = on;
            if (!on && engine != null) engine.Level = 0f;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = new Color(.95f, .58f, .12f, .45f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * .45f, Far);
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(SalvagePresentationConfig config) => presentation = config;
#endif
    }
}
