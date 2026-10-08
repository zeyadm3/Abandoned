using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Temporary power, lens-relative LOD and torch presentation on the unsaved scene copy.</summary>
    public sealed class MeridianRenderState : IDisposable
    {
        private readonly Transform mall;
        private readonly Camera camera;
        private readonly Dictionary<Camera, bool> cameras = new();
        private readonly Dictionary<Renderer, Material[]> originals = new();
        private readonly Dictionary<Renderer, bool> visible = new();
        private readonly Dictionary<Renderer, MaterialPropertyBlock> propertyBlocks = new();
        private readonly Dictionary<Renderer, MaterialPropertyBlock[]> slotBlocks = new();
        private readonly Dictionary<Light, bool> lamps = new();
        private readonly Dictionary<Light, float> intensities = new();
        private readonly Dictionary<Transform, Vector3> shutterScales = new();
        private readonly Dictionary<Collider, bool> shutterBlockers = new();
        private readonly Dictionary<Behaviour, bool> shutterObstacles = new();
        private readonly List<Material> clones = new();
        private readonly List<Light> torches = new();
        private readonly LightFixture[] fixtures;
        private readonly LODGroup[] lods;
        private readonly Color fog;
        private readonly float density;

        public MeridianRenderState(Transform level, Camera eye)
        {
            mall = level; camera = eye; fog = RenderSettings.fogColor; density = RenderSettings.fogDensity;
            fixtures = mall.GetComponentsInChildren<LightFixture>(true);
            lods = mall.GetComponentsInChildren<LODGroup>(true);
            foreach (Camera other in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (other == camera) continue;
                cameras[other] = other.enabled; other.enabled = false;
            }
            foreach (Renderer renderer in mall.GetComponentsInChildren<Renderer>(true))
            {
                originals[renderer] = renderer.sharedMaterials; visible[renderer] = renderer.enabled;
                var globalBlock = new MaterialPropertyBlock(); renderer.GetPropertyBlock(globalBlock);
                propertyBlocks[renderer] = globalBlock;
                var blocks = new MaterialPropertyBlock[renderer.sharedMaterials.Length];
                for (int i = 0; i < blocks.Length; i++)
                { blocks[i] = new MaterialPropertyBlock(); renderer.GetPropertyBlock(blocks[i], i); }
                slotBlocks[renderer] = blocks;
            }
            foreach (LightFixture fixture in fixtures)
                if (fixture.Lamp != null)
                { lamps[fixture.Lamp] = fixture.Lamp.enabled; intensities[fixture.Lamp] = fixture.Lamp.intensity; }
            OpenDefaultShutters();
            FlashlightConfig config = AssetDatabase.LoadAssetAtPath<FlashlightConfig>("Assets/_Project/Data/Equipment/FlashlightConfig.asset");
            for (int index = 0; index < 4; index++)
            {
                var torch = new GameObject("ArtFlashlight_" + index) { hideFlags = HideFlags.HideAndDontSave }.AddComponent<Light>();
                torch.transform.SetParent(camera.transform, false);
                torch.transform.localPosition = new Vector3((index - 1.5f) * 0.45f, -0.15f, 0.1f);
                torch.transform.localRotation = Quaternion.Euler(0f, (index - 1.5f) * 7f, 0f);
                torch.type = LightType.Spot;
                torch.range = config != null ? config.Range : 12f;
                torch.intensity = config != null ? config.Intensity : 6f;
                torch.spotAngle = config != null ? config.SpotAngle : 52f;
                torch.innerSpotAngle = config != null ? config.InnerSpotAngle : torch.spotAngle * .6f;
                torch.color = config != null ? config.Color : new Color(.83f, .87f, .8f);
                // Match the owner's shadow beam and the three non-shadowed remote beams.
                torch.shadows = index == 0 ? LightShadows.Soft : LightShadows.None;
                torches.Add(torch);
            }
            int seed = 0;
            foreach (ParticleSystem dust in mall.GetComponentsInChildren<ParticleSystem>())
            {
                dust.useAutoRandomSeed = false; dust.randomSeed = (uint)(1300 + seed++);
                dust.Simulate(12f, true, true, true); dust.Pause(true);
            }
        }

        public void Apply(MeridianRenderView view, bool power, int flashlightCount)
        {
            RestoreMaterials();
            camera.transform.SetPositionAndRotation(view.Eye, Quaternion.LookRotation(view.Target - view.Eye));
            for (int i = 0; i < torches.Count; i++) torches[i].enabled = i < flashlightCount;
            foreach (LightFixture fixture in fixtures)
            {
                float brightness = !power || fixture.Dead ? 0f : fixture.Faulty ? .75f : 1f;
                if (fixture.Lamp != null)
                { fixture.Lamp.enabled = brightness > 0; fixture.Lamp.intensity = intensities[fixture.Lamp] * brightness; }
                foreach (Renderer renderer in fixture.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] materials = renderer.sharedMaterials;
                    // Runtime diffuser blocks override shared emission; use the transient clones for this art capture.
                    renderer.SetPropertyBlock(null);
                    for (int i = 0; i < materials.Length; i++)
                    {
                        renderer.SetPropertyBlock(null, i);
                        Material source = materials[i];
                        if (source == null || !source.HasProperty("_EmissionColor") || !source.IsKeywordEnabled("_EMISSION")) continue;
                        var clone = new Material(source) { hideFlags = HideFlags.HideAndDontSave };
                        clone.SetColor("_EmissionColor", source.GetColor("_EmissionColor") * brightness);
                        if (brightness == 0) clone.DisableKeyword("_EMISSION");
                        clones.Add(clone); materials[i] = clone;
                    }
                    renderer.sharedMaterials = materials;
                }
            }
            ApplyFog(view.Eye);
            foreach (LODGroup group in lods)
            {
                if (group == null || !group.enabled || !group.gameObject.activeInHierarchy) continue;
                LOD[] levels = group.GetLODs();
                if (levels.Length == 0) continue;
                float distance = Vector3.Distance(camera.transform.position, group.transform.TransformPoint(group.localReferencePoint));
                Vector3 scale = group.transform.lossyScale;
                float size = group.size * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                float screenHeight = size * QualitySettings.lodBias / Mathf.Max(.01f, 2f * distance * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * .5f));
                int selected = levels.Length - 1;
                for (int i = 0; i < levels.Length; i++)
                    if (screenHeight >= levels[i].screenRelativeTransitionHeight) { selected = i; break; }
                group.ForceLOD(selected);
                if (screenHeight < levels[^1].screenRelativeTransitionHeight)
                    foreach (Renderer renderer in levels.SelectMany(l => l.renderers)) if (renderer != null) renderer.enabled = false;
            }
            foreach (ReflectionProbe probe in mall.GetComponentsInChildren<ReflectionProbe>())
            {
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
                probe.RenderProbe();
            }
        }

        private void ApplyFog(Vector3 point)
        {
            BuildingInteriorAtmosphere atmosphere = mall.GetComponent<BuildingInteriorAtmosphere>();
            if (atmosphere == null) return;
            var data = new SerializedObject(atmosphere);
            Bounds building = data.FindProperty("building").boundsValue;
            float blend = 0f;
            if (building.Contains(point))
            {
                Vector3 min = building.min, max = building.max;
                float depth = Mathf.Min(point.x - min.x, max.x - point.x, point.z - min.z, max.z - point.z);
                blend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(depth / 2.5f));
            }
            RenderSettings.fogColor = Color.Lerp(fog, data.FindProperty("interiorFog").colorValue, blend);
            RenderSettings.fogDensity = Mathf.Lerp(density, data.FindProperty("interiorDensity").floatValue, blend);
        }

        private void RestoreMaterials()
        {
            foreach (var entry in originals)
            {
                if (entry.Key == null) continue;
                entry.Key.sharedMaterials = entry.Value;
                entry.Key.SetPropertyBlock(propertyBlocks[entry.Key]);
                MaterialPropertyBlock[] blocks = slotBlocks[entry.Key];
                for (int i = 0; i < blocks.Length; i++) entry.Key.SetPropertyBlock(blocks[i], i);
            }
            foreach (var entry in visible) if (entry.Key != null) entry.Key.enabled = entry.Value;
            foreach (Material clone in clones) UnityEngine.Object.DestroyImmediate(clone);
            clones.Clear();
        }

        private void OpenDefaultShutters()
        {
            foreach (RollerShutter shutter in mall.GetComponentsInChildren<RollerShutter>(true))
            {
                var data = new SerializedObject(shutter);
                var curtain = data.FindProperty("curtain").objectReferenceValue as Transform;
                var blocker = data.FindProperty("blocker").objectReferenceValue as Collider;
                var obstacle = data.FindProperty("obstacle").objectReferenceValue as Behaviour;
                // Match RollerShutter.Awake/Show(1): the unloaded default run has every shutter open.
                if (curtain != null)
                { shutterScales[curtain] = curtain.localScale; curtain.localScale = new Vector3(1f, .05f, 1f); }
                if (blocker != null) { shutterBlockers[blocker] = blocker.enabled; blocker.enabled = false; }
                if (obstacle != null) { shutterObstacles[obstacle] = obstacle.enabled; obstacle.enabled = false; }
            }
        }

        public void Dispose()
        {
            RestoreMaterials();
            foreach (var entry in cameras) if (entry.Key != null) entry.Key.enabled = entry.Value;
            foreach (var entry in lamps) if (entry.Key != null) { entry.Key.enabled = entry.Value; entry.Key.intensity = intensities[entry.Key]; }
            foreach (var entry in shutterScales) if (entry.Key != null) entry.Key.localScale = entry.Value;
            foreach (var entry in shutterBlockers) if (entry.Key != null) entry.Key.enabled = entry.Value;
            foreach (var entry in shutterObstacles) if (entry.Key != null) entry.Key.enabled = entry.Value;
            foreach (LODGroup group in lods)
                if (group != null && group.enabled && group.gameObject.activeInHierarchy) group.ForceLOD(-1);
            foreach (Light torch in torches) UnityEngine.Object.DestroyImmediate(torch.gameObject);
            RenderSettings.fogColor = fog; RenderSettings.fogDensity = density;
        }
    }
}
