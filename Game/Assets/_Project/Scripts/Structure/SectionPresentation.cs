using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Local look and sound of a section's stage: tint, cracks, dust, creak/groan/snap timing, the
    /// Failing sag, and on collapse the swap to pre-fractured debris. Reads section state only,
    /// so it runs the same on every machine.
    /// </summary>
    [RequireComponent(typeof(StructuralSection))]
    public class SectionPresentation : MonoBehaviour
    {
        [SerializeField] private StructureVisualConfig visuals;
        [Tooltip("Pre-fractured chunks matching the Visual. Empty = break the Visual's own children apart (stairs).")]
        [OptionalReference, SerializeField] private GameObject fracturedPrefab;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private StructuralSection section;
        private Renderer[] renderers;
        private Color[] baseColors;
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
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                baseColors[i] = renderers[i].sharedMaterial != null && renderers[i].sharedMaterial.HasProperty(BaseColor)
                    ? renderers[i].sharedMaterial.GetColor(BaseColor)
                    : Color.white;
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

            if (stage == StructuralStage.Failing)
            {
                // Visibly sagging with a nervous jitter: "get off now".
                float t = Mathf.Clamp01(section.FailingTime / section.Config.FailingDuration);
                Vector3 jitter = new(Jitter(), Jitter(), Jitter());
                section.Visual.localPosition = visualRestPosition + Vector3.down * (visuals.SagDepth * t) + jitter;
                // Cracks sag with the surface rather than floating where it used to be.
                if (cracks != null) cracks.localPosition = section.Visual.localPosition - visualRestPosition;
            }

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
                case StructuralStage.Stressed: PlaceholderAudio.PlayStructure(StructureSound.Creak, at, visuals.CreakVolume); break;
                case StructuralStage.Cracking: PlaceholderAudio.PlayStructure(StructureSound.Groan, at, visuals.GroanVolume); break;
                case StructuralStage.Failing: PlaceholderAudio.PlayStructure(StructureSound.Snap, at, visuals.SnapVolume); break;
            }
        }

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
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].GetPropertyBlock(block);
                block.SetColor(BaseColor, baseColors[i] * tint);
                renderers[i].SetPropertyBlock(block);
            }

            int crackCount = stage switch
            {
                StructuralStage.Cracking => visuals.CracksWhenCracking,
                StructuralStage.Failing => visuals.CracksWhenFailing,
                _ => 0,
            };
            BuildCracks(crackCount);

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

        /// <summary>Thin dark slivers on the walking surface, seeded per section so they're the same each time.</summary>
        private void BuildCracks(int count)
        {
            if (cracks == null)
            {
                cracks = new GameObject("Cracks").transform;
                cracks.SetParent(transform, false);
            }
            cracks.localPosition = Vector3.zero;
            if (count == cracks.childCount) return;
            // Detach first: Destroy is deferred, and childCount must be right straight away.
            for (int i = cracks.childCount - 1; i >= 0; i--)
            {
                Transform old = cracks.GetChild(i);
                old.SetParent(null);
                Destroy(old.gameObject);
            }
            if (count == 0 || section.Type == SectionType.Stair) return;

            Bounds surface = section.SurfaceBounds;
            var crackRandom = new System.Random(section.Id * 101 + 7);
            Material material = CrackMaterial();
            for (int i = 0; i < count; i++)
            {
                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(crack.GetComponent<Collider>());
                crack.name = "Crack";
                crack.transform.SetParent(cracks, true);
                float length = Mathf.Lerp(visuals.CrackLength.x, visuals.CrackLength.y, (float)crackRandom.NextDouble());
                Vector3 at = new(
                    Mathf.Lerp(surface.min.x + 0.3f, surface.max.x - 0.3f, (float)crackRandom.NextDouble()),
                    surface.max.y + 0.004f,
                    Mathf.Lerp(surface.min.z + 0.3f, surface.max.z - 0.3f, (float)crackRandom.NextDouble()));
                crack.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, crackRandom.Next(180), 0f));
                crack.transform.localScale = new Vector3(visuals.CrackWidth, 0.006f, length);
                crack.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static Material crackMaterial;

        private Material CrackMaterial()
        {
            if (crackMaterial != null) return crackMaterial;
            if (renderers.Length == 0 || renderers[0].sharedMaterial == null) return null;
            // Built from an existing material so it uses the same (always included) URP Lit shader.
            crackMaterial = new Material(renderers[0].sharedMaterial) { name = "Crack" };
            crackMaterial.SetColor(BaseColor, visuals.CrackColor);
            return crackMaterial;
        }

        private void CreateDust()
        {
            var go = new GameObject("Dust");
            go.transform.SetParent(transform, false);
            Bounds b = section.SurfaceBounds;
            // Dust trickles from the underside: what you'd see from the room below.
            go.transform.position = new Vector3(b.center.x, b.min.y - 0.05f, b.center.z);
            dust = go.AddComponent<ParticleSystem>();
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = dust.main;
            main.startLifetime = 2.5f;
            main.startSpeed = 0.15f;
            main.startSize = 0.07f;
            main.gravityModifier = 0.08f;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.75f, 0.7f, 0.62f, 0.8f);
            ParticleSystem.ShapeModule shape = dust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(b.size.x * 0.9f, 0.05f, b.size.z * 0.9f);
            ParticleSystem.EmissionModule emission = dust.emission;
            emission.rateOverTime = 0f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = visuals.DustMaterial;
            dust.Play();
        }

        private void OnCollapsed(StructuralSection s)
        {
            foreach (Renderer r in renderers) r.enabled = false;
            BuildCracks(0);
            section.Visual.localPosition = visualRestPosition;
            dust.Emit(visuals.CollapseDustBurst);
            ParticleSystem.EmissionModule emission = dust.emission;
            emission.rateOverTime = 0f;

            Vector3 centre = s.SurfaceBounds.center;
            PlaceholderAudio.PlayStructure(StructureSound.Crash, centre, 1f);
            CameraShake.Emit(centre, visuals.CollapseShake);
            debris = DebrisSpawner.Spawn(section, fracturedPrefab, renderers.Length > 0 ? renderers[0].sharedMaterial : null,
                visuals, s.CollapseSeed);
        }

        private void OnRestored(StructuralSection s)
        {
            if (debris != null) Destroy(debris);
            debris = null;
            foreach (Renderer r in renderers) r.enabled = true;
            section.Visual.localPosition = visualRestPosition;
            ApplyStage(StructuralStage.Stable);
        }

#if UNITY_EDITOR
        public void EditorSetup(StructureVisualConfig visualConfig, GameObject fractured)
        {
            visuals = visualConfig;
            fracturedPrefab = fractured;
        }
#endif
    }
}
