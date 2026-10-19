using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Gallows Rock's daylight finish: local reflections, lit air and a restrained photographic grade.</summary>
    public static class IslandPresentation
    {
        public const string GroupName = "IslandPresentation";

        public static void Apply(Transform root)
        {
            Transform group = Group(GroupName, root);
            RenderSettings.defaultReflectionResolution = 256;
            RenderSettings.reflectionIntensity = .55f;
            if (RenderSettings.sun != null)
            {
                RenderSettings.sun.shadowBias = .045f;
                RenderSettings.sun.shadowNormalBias = .22f;
                RenderSettings.sun.shadowStrength = .88f;
            }
            Transform volumeRoot = root.Find("PostProcessing");
            VolumeProfile profile = volumeRoot != null ? volumeRoot.GetComponent<Volume>()?.sharedProfile : null;
            if (profile != null && profile.TryGet(out ColorAdjustments colour))
            {
                colour.postExposure.Override(.15f);
                EditorUtility.SetDirty(profile);
            }
            Reflections(group);
            Dust(group, new Vector3(54f, 19f, 66f), new Vector3(8f, 10f, 26f), 72);
            Dust(group, new Vector3(40f, 10f, 85.5f), new Vector3(13f, 2.5f, 8f), 24);
            ShapeFixtureShadows(root);
            group.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Local reflection volumes and drifting lit dust have no solid geometry.");
        }

        private static void Reflections(Transform group)
        {
            var probes = new List<ReflectionProbe>();
            Probe("Broadway", new Vector3(54f, 18f, 66f), new Vector3(12f, 14f, 40f), 2.5f);
            Probe("WestAisle", new Vector3(36f, 17f, 67f), new Vector3(10f, 12f, 30f), 1f);
            Probe("EastAisle", new Vector3(72f, 17f, 67f), new Vector3(10f, 12f, 30f), 1f);
            Probe("Laundry", new Vector3(42f, 9.8f, 85f), new Vector3(23f, 3.5f, 11f), .7f);
            Probe("Boiler", new Vector3(69f, 9.8f, 85f), new Vector3(17f, 3.5f, 11f), .7f);
            group.gameObject.AddComponent<IslandReflectionRefresh>().EditorSetup(probes.ToArray());

            void Probe(string name, Vector3 at, Vector3 size, float blend)
            {
                Transform holder = Group("Reflection_" + name, group);
                holder.position = at;
                var probe = holder.gameObject.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probe.resolution = 128;
                probe.hdr = true;
                probe.boxProjection = true;
                probe.size = size;
                probe.blendDistance = blend;
                probe.intensity = .5f;
                probe.importance = name is "Laundry" or "Boiler" ? 2 : 1;
                probe.nearClipPlane = .15f;
                probe.farClipPlane = 65f;
                probe.cullingMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
                probes.Add(probe);
            }
        }

        private static void Dust(Transform group, Vector3 at, Vector3 size, int count)
        {
            const string path = IslandPrisonArt.Folder + "/Materials/Island_Dust.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Particles/Simple Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", PolishAssets.Texture("IslandDustDot", stain: true));
            material.SetColor("_BaseColor", new Color(.9f, .88f, .8f, .16f));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            Transform air = Group("LitDust", group);
            air.position = at;
            var particles = air.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.useAutoRandomSeed = false;
            particles.randomSeed = 5209;
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(14f, 22f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.005f, .025f);
            main.startSize = new ParticleSystem.MinMaxCurve(.01f, .025f);
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = count / 18f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = .025f;
            noise.frequency = .12f;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static void ShapeFixtureShadows(Transform root)
        {
            // Four local shadow casters give the grille and boilerwork depth without making every light render
            // another shadow map. Power/flicker still belongs to each LightFixture.
            int hallRemaining = 2, basementRemaining = 2;
            foreach (LightFixture fixture in root.GetComponentsInChildren<LightFixture>())
            {
                Light lamp = fixture.Lamp;
                if (lamp == null || lamp.type != LightType.Spot || fixture.Dead) continue;
                Vector3 at = fixture.transform.position;
                bool basement = at.y < IslandPrisonLayout.PlateauY && at.z > 80f
                    && (at.x > 40f && at.x < 47f || at.x > 67f && at.x < 72f);
                bool hall = at.x > 50f && at.x < 58f && at.z > 58f && at.z < 70f;
                if (basement ? basementRemaining <= 0 : !hall || hallRemaining <= 0) continue;
                lamp.shadows = LightShadows.Soft;
                lamp.shadowResolution = LightShadowResolution.Low;
                lamp.shadowBias = .04f;
                lamp.shadowNormalBias = .18f;
                lamp.shadowStrength = .72f;
                if (basement) basementRemaining--;
                else hallRemaining--;
            }
        }
    }
}
