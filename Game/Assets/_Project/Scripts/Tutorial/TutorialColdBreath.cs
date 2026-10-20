using Abandoned.Networking;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// December in an unheated house: this machine's player breathes out a faint cloud every few seconds, quicker
    /// when they're out of breath. Local and cosmetic, on the training level's root; nothing networked.
    /// </summary>
    public class TutorialColdBreath : MonoBehaviour
    {
        [SerializeField] private Material puffMaterial;
        [SerializeField, Min(1f)] private float interval = 3.8f;
        [SerializeField, Range(4, 40)] private int particles = 12;

        private ParticleSystem puff;
        private float next;

        private void Update()
        {
            if (Application.isBatchMode || puffMaterial == null || Time.time < next) return;
            NetworkPlayer me = NetworkPlayer.Local;
            Camera eye = Camera.main;
            if (me == null || !me.IsSpawned || me.IsDead || me.IsHiding || eye == null) return;
            if (puff == null) Build();
            // Sprinting (or just having sprinted) quickens it.
            bool winded = me.Motor != null && me.Motor.IsSprinting;
            next = Time.time + interval * (winded ? .45f : 1f) * Random.Range(.85f, 1.2f);
            Transform view = eye.transform;
            puff.transform.SetPositionAndRotation(view.position + view.forward * .32f + Vector3.down * .14f, view.rotation);
            var emit = new ParticleSystem.EmitParams { velocity = view.forward * .35f + Vector3.up * .05f, applyShapeToPosition = true };
            puff.Emit(emit, particles);
        }

        private void Build()
        {
            var go = new GameObject("ColdBreath");
            go.transform.SetParent(transform, false);
            puff = go.AddComponent<ParticleSystem>();
            puff.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = puff.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.7f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(.06f, .14f);
            main.startColor = new Color(.92f, .94f, .97f, .16f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;
            ParticleSystem.EmissionModule emission = puff.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = puff.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .03f;
            ParticleSystem.SizeOverLifetimeModule size = puff.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, .6f, 1f, 2.6f));
            ParticleSystem.ColorOverLifetimeModule colour = puff.colorOverLifetime;
            colour.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .15f), new GradientAlphaKey(0f, 1f) });
            colour.color = fade;
            var renderer = puff.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = puffMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

#if UNITY_EDITOR
        public void EditorSetup(Material material) => puffMaterial = material;
#endif
    }
}
