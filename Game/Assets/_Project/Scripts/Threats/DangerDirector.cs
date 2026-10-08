using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Structure;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// The run gets worse (GDD 9, PLAYBOOK 5.6). Host: sets the danger level from the run clock (a jump and
    /// double pace once the extraction window closes), makes overloaded floors fail faster, ages random
    /// sections, sharpens the monsters and adds a second Blind One. Every machine: each level-up is felt,
    /// not shown (GDD 9): a building-wide groan, flickering lights and a burst of radio static.
    /// </summary>
    public class DangerDirector : MonoBehaviour
    {
        private const float FlickerSeconds = 1.6f;

        [SerializeField] private DangerConfig config;
        [SerializeField] private StructureSimulation structure;

        private readonly List<(Light light, float intensity)> lights = new();
        private System.Random random = new(1);
        private int shownLevel;
        private float nextAging, flickerUntil;
        private RunState lastRun;

        public DangerConfig Config => config;
        public int Level => RunState.Current != null ? RunState.Current.State.Danger : 0;

        public void Setup(DangerConfig dangerConfig, StructureSimulation simulation)
        {
            config = dangerConfig;
            structure = simulation;
        }

        private void Start()
        {
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type != LightType.Directional && l.gameObject.scene == gameObject.scene && l.GetComponentInParent<LightFixture>() == null && l.GetComponentInParent<Abandoned.Networking.NetworkPlayer>() == null && l.GetComponentInParent<TruckCargo>() == null && l.GetComponentInParent<TruckFloodlights>() == null) lights.Add((l, l.intensity));
        }

        private void Update()
        {
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return;
            if (run != lastRun)
            {
                // A new run: calm again.
                lastRun = run;
                shownLevel = run.State.Danger;
                random = new System.Random(run.State.Seed);
                RestoreLights();
            }
            if (run.IsServer && run.State.Phase == RunPhase.Running) Escalate(run);
            if (run.State.Danger > shownLevel) LevelUpCues();
            shownLevel = run.State.Danger;
            Flicker();
        }

        // ---- Host ----

        private void Escalate(RunState run)
        {
            int level = config.LevelAt(run.Elapsed, run.State.Window > 0f ? run.State.Window : run.Config.WindowSeconds);
            run.SetDanger(level);
            // A storm (M9.2) speeds decay and dulls hearing on top of the danger level.
            if (structure != null) structure.DangerDecay = (1f + level * config.DecayPerLevel) * run.Terms.Decay;
            foreach (Threat t in Threat.All)
            {
                t.HearingScale = (1f + level * config.HearingPerLevel) * run.Terms.Hearing;
                t.SpeedScale = 1f + level * config.SpeedPerLevel;
            }
            if (structure != null && level > 0 && Time.time >= nextAging)
            {
                nextAging = Time.time + config.AgingInterval;
                structure.AgeRandomSections(level, config.AgingDamage, config.AgingFloor, random);
            }

        }

        // ---- Everyone ----

        private void LevelUpCues()
        {
            AudioListener ear = VoiceListener.Current;
            Vector3 at = ear != null ? ear.transform.position : Vector3.zero;
            GameAudio.Play(SoundId.Groan, at + Vector3.up * 3f, 1f);
            GameAudio.Play(SoundId.RadioStatic, at, 0.6f);
            flickerUntil = Time.time + FlickerSeconds;
            LightFixture.Disturb(FlickerSeconds); // fixtures run their own flicker
        }

        private void Flicker()
        {
            if (flickerUntil <= 0f) return;
            if (Time.time >= flickerUntil)
            {
                flickerUntil = 0f;
                RestoreLights();
                return;
            }
            // QA O-01: with flashing reduced, the level-up is one slow dip and recovery, never a strobe.
            float calm = 1f - 0.5f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(1f - (flickerUntil - Time.time) / FlickerSeconds));
            foreach ((Light l, float intensity) in lights)
                if (l != null) l.intensity = Core.GameSettings.ReduceFlashing ? intensity * calm
                    : intensity * (Random.value < 0.35f ? 0.05f : Random.Range(0.6f, 1f));
        }

        private void RestoreLights()
        {
            foreach ((Light l, float intensity) in lights) if (l != null) l.intensity = intensity;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (DebugView.Visible && RunState.Current != null)
                GUI.Label(new Rect(Screen.width - 360f, 30f, 350f, 22f), $"DANGER {Level}  decay x{(structure != null ? structure.DangerDecay : 1f):0.00}");
        }
#endif
    }
}
