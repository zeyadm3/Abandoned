using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// The living view behind the title screen: the level's scene camera stands at its MenuVantage and
    /// drifts and breathes slowly through dust. Every so often, never on a schedule you can learn,
    /// something happens: a light in view sputters and dies for a while, a dark figure stands at the far
    /// end and is gone on the next flicker, or the building groans and the camera shudders. All of it is
    /// undone the moment a game starts. "Reduce menu effects" keeps the drift, the figure and the dying
    /// light (no sputter) but drops shudders and flicker.
    /// </summary>
    public class MenuBackdrop
    {
        private enum Beat { LightDies, Figure, Groan }

        private readonly MenuEffectsConfig config;
        private readonly List<(LightFixture fixture, float until)> killed = new();
        private Camera camera;
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private float nextEvent, shudderStart = -1f, figureUntil = -1f, figureFlickerAt = -1f;
        private GameObject figure;
        private ParticleSystem dust;

        public MenuBackdrop(MenuEffectsConfig effects) => config = effects;

        public void Tick()
        {
            Camera main = Camera.main;
            if (main == null) return;
            if (main != camera) Take(main);
            float now = Time.unscaledTime;
            Drift(now);
            if (now >= nextEvent)
            {
                nextEvent = now + MenuEffectsConfig.Range(config.EventInterval);
                Happen();
            }
            TickFigure(now);
            for (int i = killed.Count - 1; i >= 0; i--)
            {
                if (now < killed[i].until) continue;
                if (killed[i].fixture != null) killed[i].fixture.SetHorrorPower(true);
                killed.RemoveAt(i);
            }
        }

        /// <summary>A game started (or the menu went away): put every light back and clear the stage.</summary>
        public void Stop()
        {
            foreach ((LightFixture fixture, float _) in killed)
                if (fixture != null) fixture.SetHorrorPower(true);
            killed.Clear();
            if (figure != null) figure.SetActive(false);
            figureUntil = -1f;
            if (dust != null) Object.Destroy(dust.gameObject);
            dust = null;
            camera = null;
        }

        private void Take(Camera main)
        {
            camera = main;
            MenuVantage vantage = Object.FindAnyObjectByType<MenuVantage>();
            if (vantage != null)
            {
                camera.transform.SetPositionAndRotation(vantage.transform.position, vantage.transform.rotation);
                camera.fieldOfView = vantage.FieldOfView;
            }
            basePosition = camera.transform.position;
            baseRotation = camera.transform.rotation;
            nextEvent = Time.unscaledTime + MenuEffectsConfig.Range(config.EventInterval) * 0.6f;
            MakeDust();
        }

        private void Drift(float now)
        {
            // Two slow, unrelated sines so the look never quite repeats, plus a breath forward and back.
            float yaw = Mathf.Sin(now * 0.071f) * config.DriftYaw + Mathf.Sin(now * 0.023f + 1.3f) * config.DriftYaw * 0.4f;
            float pitch = Mathf.Sin(now * 0.11f + 0.6f) * config.DriftPitch;
            Quaternion look = baseRotation * Quaternion.Euler(pitch, yaw, 0f);
            if (shudderStart >= 0f)
            {
                float t = (now - shudderStart) / Mathf.Max(0.01f, config.ShudderSeconds);
                if (t >= 1f) shudderStart = -1f;
                else
                {
                    float a = config.ShudderDegrees * (1f - t);
                    look *= Quaternion.Euler((Mathf.PerlinNoise(now * 23f, 0f) - 0.5f) * 2f * a, (Mathf.PerlinNoise(0f, now * 19f) - 0.5f) * 2f * a, (Mathf.PerlinNoise(now * 17f, 3f) - 0.5f) * a);
                }
            }
            Vector3 forward = baseRotation * Vector3.forward;
            camera.transform.SetPositionAndRotation(basePosition + forward * (Mathf.Sin(now * 0.05f) * config.DriftDolly), look);
        }

        private void Happen()
        {
            bool calm = MenuEffectsConfig.Calm;
            var beats = new List<Beat> { Beat.LightDies, Beat.Figure };
            if (!calm) beats.Add(Beat.Groan);
            switch (beats[Random.Range(0, beats.Count)])
            {
                case Beat.LightDies:
                    LightFixture victim = LitFixtureInView();
                    if (victim == null) { Groan(calm); return; }
                    if (!calm) LightFixture.Disturb(0.6f);
                    victim.SetHorrorPower(false);
                    killed.Add((victim, Time.unscaledTime + MenuEffectsConfig.Range(config.LightDeadSeconds)));
                    break;
                case Beat.Figure:
                    if (!ShowFigure(calm)) Groan(calm);
                    break;
                default:
                    Groan(calm);
                    break;
            }
        }

        private void Groan(bool calm)
        {
            if (calm) return;
            shudderStart = Time.unscaledTime;
            LightFixture.Disturb(0.5f);
            GameAudio.PlayStructure(StructureSound.Groan, camera.transform.position + camera.transform.forward * 8f + Vector3.up * 2f, config.GroanVolume);
        }

        private LightFixture LitFixtureInView()
        {
            LightFixture best = null;
            float bestScore = float.MaxValue;
            foreach (LightFixture fixture in LightFixture.All)
            {
                if (fixture == null || !fixture.Lit || killed.Exists(k => k.fixture == fixture)) continue;
                Vector3 v = camera.WorldToViewportPoint(fixture.transform.position);
                if (v.z < 1f || v.z > 30f || v.x < 0.1f || v.x > 0.9f || v.y < 0.05f || v.y > 0.95f) continue;
                // Prefer one off-centre and some way in: noticed a beat late, not shoved in your face.
                float score = Mathf.Abs(v.x - 0.5f) < 0.15f ? v.z + 10f : v.z;
                if (score < bestScore) { bestScore = score; best = fixture; }
            }
            return best;
        }

        private bool ShowFigure(bool calm)
        {
            Transform eye = camera.transform;
            Vector3 dir = Quaternion.AngleAxis(Random.Range(-7f, 7f), Vector3.up) * Vector3.ProjectOnPlane(eye.forward, Vector3.up).normalized;
            int mask = ~LayerMask.GetMask("Ignore Raycast", Core.GameLayers.Player, Core.GameLayers.Debris);
            if (!Physics.Raycast(eye.position, dir, out RaycastHit wall, 45f, mask, QueryTriggerInteraction.Ignore)) return false;
            if (wall.distance < config.SilhouetteMinDistance) return false;
            Vector3 spot = wall.point - dir * 0.9f;
            if (!Physics.Raycast(spot + Vector3.up * 0.5f, Vector3.down, out RaycastHit floor, 4f, mask, QueryTriggerInteraction.Ignore)) return false;
            if (figure == null) figure = BuildFigure();
            figure.transform.SetPositionAndRotation(floor.point, Quaternion.LookRotation(-dir, Vector3.up) * Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f));
            // It arrives under cover of a sputter and leaves on the next one.
            if (!calm) LightFixture.Disturb(0.35f);
            figure.SetActive(true);
            figureUntil = Time.unscaledTime + config.SilhouetteSeconds;
            figureFlickerAt = figureUntil - 0.2f;
            return true;
        }

        private void TickFigure(float now)
        {
            if (figureUntil < 0f) return;
            if (figureFlickerAt >= 0f && now >= figureFlickerAt)
            {
                figureFlickerAt = -1f;
                if (!MenuEffectsConfig.Calm) LightFixture.Disturb(0.4f);
            }
            if (now < figureUntil) return;
            figureUntil = -1f;
            figure.SetActive(false);
        }

        // A tall, too-thin shape in matte black: no face, arms a little too long. Made from primitives so
        // the menu needs no model; no colliders, so nothing in the world can bump it.
        private GameObject BuildFigure()
        {
            var root = new GameObject("MenuFigure") { layer = LayerMask.NameToLayer("Ignore Raycast") };
            Material skin = config.SilhouetteMaterial;
            Part(PrimitiveType.Capsule, new Vector3(0f, 1.05f, 0f), new Vector3(0.36f, 0.62f, 0.26f), Vector3.zero);
            Part(PrimitiveType.Sphere, new Vector3(0.02f, 1.86f, 0.02f), new Vector3(0.22f, 0.27f, 0.23f), new Vector3(0f, 0f, -9f));
            Part(PrimitiveType.Capsule, new Vector3(-0.27f, 0.98f, 0f), new Vector3(0.09f, 0.52f, 0.09f), new Vector3(0f, 0f, -4f));
            Part(PrimitiveType.Capsule, new Vector3(0.27f, 0.95f, 0.02f), new Vector3(0.09f, 0.55f, 0.09f), new Vector3(0f, 0f, 5f));
            Part(PrimitiveType.Capsule, new Vector3(-0.1f, 0.42f, 0f), new Vector3(0.12f, 0.45f, 0.12f), Vector3.zero);
            Part(PrimitiveType.Capsule, new Vector3(0.1f, 0.42f, 0f), new Vector3(0.12f, 0.45f, 0.12f), Vector3.zero);
            root.SetActive(false);
            return root;

            void Part(PrimitiveType type, Vector3 at, Vector3 size, Vector3 euler)
            {
                GameObject part = GameObject.CreatePrimitive(type);
                Object.DestroyImmediate(part.GetComponent<Collider>());
                part.layer = root.layer;
                part.transform.SetParent(root.transform, false);
                part.transform.SetLocalPositionAndRotation(at, Quaternion.Euler(euler));
                part.transform.localScale = size;
                var renderer = part.GetComponent<MeshRenderer>();
                if (skin != null) renderer.sharedMaterial = skin;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        // Motes hanging in front of the lens; the level's own dust and fog do the rest.
        private void MakeDust()
        {
            if (config.DustMaterial == null || config.DustParticles <= 0 || dust != null) return;
            var go = new GameObject("MenuDust");
            go.transform.SetParent(camera.transform, false);
            go.transform.localPosition = Vector3.forward * 4f;
            dust = go.AddComponent<ParticleSystem>();
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = dust.main;
            main.loop = true;
            main.duration = 10f;
            main.startLifetime = 12f;
            main.startSpeed = 0.02f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.025f);
            main.startColor = new Color(0.85f, 0.8f, 0.7f, 0.35f);
            main.maxParticles = config.DustParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true;
            ParticleSystem.EmissionModule emission = dust.emission;
            emission.rateOverTime = config.DustParticles / 12f;
            ParticleSystem.ShapeModule shape = dust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(6f, 3.5f, 6f);
            ParticleSystem.NoiseModule noise = dust.noise;
            noise.enabled = true;
            noise.strength = 0.05f;
            noise.frequency = 0.25f;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = config.DustMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dust.Simulate(8f, true, true);
            dust.Play();
        }
    }
}
