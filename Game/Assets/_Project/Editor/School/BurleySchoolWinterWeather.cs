using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Exterior-only world-space snowfall and a soft, upright chimney plume.</summary>
    public static class BurleySchoolWinterWeather
    {
        public static void Build(Transform parent, BurleySchoolArt art)
        {
            // Gaps beside the envelope account for the full lifetime's wind drift; there is no emitter over rooms.
            Snow(parent, "FrontCampusSnow", new Vector3(50f, 13f, 3f), new Vector3(120f, .1f, 26f), 32f, art);
            Snow(parent, "WestCampusSnow", new Vector3(-12f, 13f, 57f), new Vector3(16f, .1f, 82f), 22f, art);
            Snow(parent, "EastCampusSnow", new Vector3(162f, 13f, 57f), new Vector3(108f, .1f, 82f), 46f, art);
            Snow(parent, "NorthCampusSnow", new Vector3(52f, 13f, 130f), new Vector3(112f, .1f, 64f), 32f, art);
            Steam(parent, art);
        }

        private static void Snow(Transform parent, string name, Vector3 at, Vector3 area, float rate, BurleySchoolArt art)
        {
            Transform emitter = Group(name, parent);
            emitter.position = at;
            var particles = emitter.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = true; main.playOnAwake = true; main.duration = 8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 8f);
            main.startSpeed = 0f; main.startSize = new ParticleSystem.MinMaxCurve(.028f, .065f);
            main.startColor = new Color(.9f, .95f, 1f, .8f);
            main.maxParticles = 450; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            var emission = particles.emission;
            emission.rateOverTime = rate;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box; shape.scale = area;
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-.6f, -.15f);
            velocity.y = new ParticleSystem.MinMaxCurve(-2.4f, -1.65f);
            velocity.z = new ParticleSystem.MinMaxCurve(-.4f, -.12f);
            var collision = particles.collision;
            collision.enabled = true; collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.collidesWith = LayerMask.GetMask("Default");
            collision.quality = ParticleSystemCollisionQuality.Low;
            collision.lifetimeLoss = 1f; collision.bounce = 0f;
            var colour = particles.colorOverLifetime;
            colour.enabled = true; colour.color = Fade(.8f);
            Renderer(particles, art);
        }

        private static void Steam(Transform parent, BurleySchoolArt art)
        {
            Transform emitter = Group("SmokestackSteamPlume", parent);
            emitter.position = L.SmokestackPosition + Vector3.up * (L.SmokestackHeight + 1.25f);
            emitter.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var particles = emitter.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = true; main.playOnAwake = true; main.duration = 8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 10f);
            main.startSpeed = 0f; main.startSize = new ParticleSystem.MinMaxCurve(.7f, 1.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(.91f, .94f, .98f, .28f);
            main.maxParticles = 120; main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 9f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 12f; shape.radius = .55f;
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-.75f, -.35f);
            velocity.y = new ParticleSystem.MinMaxCurve(1.6f, 2.4f);
            velocity.z = new ParticleSystem.MinMaxCurve(-.35f, -.12f);
            var size = particles.sizeOverLifetime;
            size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 3.8f));
            var colour = particles.colorOverLifetime;
            colour.enabled = true; colour.color = Fade(.75f);
            var noise = particles.noise;
            noise.enabled = true; noise.strength = .2f; noise.frequency = .13f; noise.scrollSpeed = .16f;
            Renderer(particles, art);
        }

        private static Gradient Fade(float alpha)
        {
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(alpha, .12f), new GradientAlphaKey(alpha * .8f, .65f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }

        private static void Renderer(ParticleSystem particles, BurleySchoolArt art)
        {
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = art.WeatherParticle;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off; renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.maxParticleSize = .08f;
        }
    }
}
