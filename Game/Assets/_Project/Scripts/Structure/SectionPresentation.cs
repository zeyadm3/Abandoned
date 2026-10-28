using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Local look and sound of a section's stage: tint, cracks, dust, creak/groan/snap timing, the
    /// Failing sag, and on collapse the swap to pre-fractured debris. Reads section state only,
    /// so it runs the same on every machine: on clients that state is the host's, replicated.
    /// </summary>
    [RequireComponent(typeof(StructuralSection))]
    public partial class SectionPresentation : MonoBehaviour
    {
        [SerializeField] private StructureVisualConfig visuals;
        [Tooltip("Pre-fractured chunks matching the Visual. Empty = break the Visual's own children apart (stairs).")]
        [OptionalReference, SerializeField] private GameObject fracturedPrefab;
        [OptionalReference, SerializeField] private StructureStageVisuals stageVisuals;
        [SerializeField] private bool preserveFractureMaterials;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private StructuralSection section;
        private Renderer[] renderers;
        private Color[][] baseColors;
        private bool[] rendererRestEnabled;
        private MaterialPropertyBlock block;
        private Vector3 visualRestPosition;
        private Transform cracks;
        private ParticleSystem dust;
        private GameObject debris;
        private System.Random random;
        private float nextSoundIn;

        public GameObject Debris => debris;
        public int CrackCount => cracks != null ? cracks.childCount : 0;
        public float DustRate => dust != null ? dust.emission.rateOverTime.constant : 0f;

        private void Awake()
        {
            section = GetComponent<StructuralSection>();
            renderers = section.Visual.GetComponentsInChildren<Renderer>(true);
            baseColors = new Color[renderers.Length][];
            rendererRestEnabled = new bool[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                rendererRestEnabled[i] = renderers[i].enabled;
                // Per material: a remodelled slab (remodel v2) carries its nosing or ceiling grid in further slots.
                Material[] materials = renderers[i].sharedMaterials;
                baseColors[i] = new Color[materials.Length];
                for (int j = 0; j < materials.Length; j++)
                    baseColors[i][j] = materials[j] != null && materials[j].HasProperty(BaseColor) ? materials[j].GetColor(BaseColor) : Color.white;
            }
            block = new MaterialPropertyBlock();
            visualRestPosition = section.Visual.localPosition;
            random = new System.Random(name.GetHashCode());
            CreateDust();
        }

        private void OnEnable()
        {
            section.StageChanged += OnStageChanged;
            section.Collapsed += OnCollapsed;
            section.Restored += OnRestored;
            ApplyStage(section.Stage);
        }

        private void OnDisable()
        {
            section.StageChanged -= OnStageChanged;
            section.Collapsed -= OnCollapsed;
            section.Restored -= OnRestored;
        }

        private void Update()
        {
            StructuralStage stage = section.Stage;
            if (stage == StructuralStage.Stable || stage == StructuralStage.Collapsed) return;

            if (stage == StructuralStage.Failing && (stageVisuals == null || !stageVisuals.HasFailing))
            {
                // Visibly sagging with a nervous jitter: "get off now".
                float t = Mathf.Clamp01(section.FailingTime / section.Config.FailingDuration);
                Vector3 jitter = new(Jitter(), Jitter(), Jitter());
                section.Visual.localPosition = visualRestPosition + Vector3.down * (visuals.SagDepth * t) + jitter;
                // Cracks sag with the surface rather than floating where it used to be.
                if (cracks != null) cracks.localPosition = section.Visual.localPosition - visualRestPosition;
            }

            // Only a failing section makes a sound now: stressed/cracked floors creaking and groaning on a loop
            // became a constant drone across a decaying mall (user, 2026-10-08). Cracks, tint and dust still show it.
            if (stage != StructuralStage.Failing) return;
            nextSoundIn -= Time.deltaTime;
            if (nextSoundIn > 0f) return;
            PlayStageSound(stage);
            nextSoundIn = Gap(stage);
        }

        private float Jitter() => ((float)random.NextDouble() * 2f - 1f) * visuals.FailingJitter;

        private float Gap(StructuralStage stage)
        {
            Vector2 range = stage switch
            {
                StructuralStage.Stressed => visuals.CreakGapStressed,
                StructuralStage.Cracking => visuals.CreakGapCracking,
                _ => visuals.CreakGapFailing,
            };
            return Mathf.Lerp(range.x, range.y, (float)random.NextDouble());
        }

        private void PlayStageSound(StructuralStage stage)
        {
            Vector3 at = section.SurfaceBounds.center;
            switch (stage)
            {
                case StructuralStage.Failing:
                    // 0.17.2: glass, timber and tile fail in their own voice; concrete snaps.
                    if (section.FloorKind == FloorKind.Concrete) GameAudio.PlayStructure(StructureSound.Snap, at, visuals.SnapVolume);
                    else GameAudio.Play(FloorFailSound(section.FloorKind), at, visuals.SnapVolume);
                    break;
                case StructuralStage.Cracking when section.FloorKind != FloorKind.Concrete:
                    // One warning as it starts to go: a glass floor ticks and spiders, tiles pop, boards split.
                    GameAudio.Play(FloorCrackSound(section.FloorKind), at, visuals.SnapVolume * 0.8f);
                    break;
            }
        }

        private static SoundId FloorFailSound(FloorKind kind) => kind switch
        {
            FloorKind.Glass => SoundId.GlassFloorFail,
            FloorKind.Timber => SoundId.TimberFloorFail,
            _ => SoundId.TileFloorFail,
        };

        private static SoundId FloorCrackSound(FloorKind kind) => kind switch
        {
            FloorKind.Glass => SoundId.GlassFloorCrack,
            FloorKind.Timber => SoundId.TimberFloorCrack,
            _ => SoundId.TileFloorCrack,
        };

        private void OnStageChanged(StructuralSection s, StructuralStage previous)
        {
            ApplyStage(s.Stage);
            // Getting worse always announces itself straight away; the stage loop takes over after.
            if (s.Stage > previous && s.Stage != StructuralStage.Collapsed)
            {
                PlayStageSound(s.Stage);
                nextSoundIn = Gap(s.Stage);
            }
        }

        private void ApplyStage(StructuralStage stage)
        {
            Color tint = stage switch
            {
                StructuralStage.Stressed => visuals.StressedTint,
                StructuralStage.Cracking => visuals.CrackingTint,
                StructuralStage.Failing => visuals.FailingTint,
                _ => Color.white,
            };
            // Authored PBR variants contain damage already; property blocks would also override trim slots.
            if (stageVisuals == null) for (int i = 0; i < renderers.Length; i++)
            for (int j = 0; j < baseColors[i].Length; j++)
            {
                renderers[i].GetPropertyBlock(block, j);
                block.SetColor(BaseColor, baseColors[i][j] * tint);
                renderers[i].SetPropertyBlock(block, j);
            }

            int crackCount = stage switch
            {
                StructuralStage.Cracking => visuals.CracksWhenCracking,
                StructuralStage.Failing => visuals.CracksWhenFailing,
                _ => 0,
            };
            if (stageVisuals != null) stageVisuals.Show(stage);
            else BuildCracks(crackCount);

            ParticleSystem.EmissionModule emission = dust.emission;
            emission.rateOverTime = stage switch
            {
                StructuralStage.Stressed => visuals.StressedDust,
                StructuralStage.Cracking => visuals.CrackingDust,
                StructuralStage.Failing => visuals.FailingDust,
                _ => 0f,
            };
            if (stage != StructuralStage.Failing) section.Visual.localPosition = visualRestPosition;
        }

        private void OnCollapsed(StructuralSection s)
        {
            foreach (Renderer r in renderers) r.enabled = false;
            if (stageVisuals != null) stageVisuals.Show(StructuralStage.Collapsed);
            else BuildCracks(0);
            section.Visual.localPosition = visualRestPosition;
            if (!s.CollapsedQuietly) dust.Emit(visuals.CollapseDustBurst);
            ParticleSystem.EmissionModule emission = dust.emission;
            emission.rateOverTime = 0f;
            // Already down when we joined: just gone, no break replayed out of nowhere.
            if (s.CollapsedQuietly) return;

            Vector3 centre = s.SurfaceBounds.center;
            GameAudio.PlayStructure(StructureSound.Crash, centre, 1f);
            CameraShake.EmitCollapse(centre, visuals.CollapseShake);
            Achievements.Increment(Achievements.StatCollapses); // witnessed on this machine (M9.5)
            debris = DebrisSpawner.Spawn(section, fracturedPrefab, renderers.Length > 0 ? renderers[0].sharedMaterial : null,
                visuals, s.CollapseSeed, preserveFractureMaterials);
        }

        private void OnRestored(StructuralSection s)
        {
            if (debris != null) Destroy(debris);
            debris = null;
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = stageVisuals == null && !preserveFractureMaterials || rendererRestEnabled[i];
            section.Visual.localPosition = visualRestPosition;
            ApplyStage(StructuralStage.Stable);
            if (stageVisuals != null) stageVisuals.RestoreLods();
        }

#if UNITY_EDITOR
        public void EditorSetup(StructureVisualConfig visualConfig, GameObject fractured,
            StructureStageVisuals authoredStages = null, bool keepFractureMaterials = false)
        {
            visuals = visualConfig;
            fracturedPrefab = fractured;
            stageVisuals = authoredStages;
            preserveFractureMaterials = keepFractureMaterials;
        }
#endif
    }
}
