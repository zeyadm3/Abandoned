using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.Structure
{
    /// <summary>
    /// Which lights cast shadows (0.14.0). A level has hundreds of ceiling lights and only a handful can
    /// afford shadow maps, so instead of a fixed few (four in the whole mall, before this) the lit ones
    /// nearest this machine's camera cast, and the set follows the player: shadows fade in on a light as it
    /// becomes one of the nearest and fade out as it drops away. How many is the Shadows setting
    /// (<see cref="ShadowBudgetConfig"/>). The player's own flashlight and the moon are not managed here:
    /// they always cast.
    ///
    /// Also keeps lights made at run time on every rendering layer in a level split by storey
    /// (<see cref="LevelStoreys"/>). Local and cosmetic: nothing is networked, every machine has its own
    /// set. One per game, made on first scene load (never in batch mode).
    /// </summary>
    public class ShadowDirector : MonoBehaviour
    {
        private const float ChooseEvery = 0.2f, LayersEvery = 1f;

        private class Casting
        {
            public Light Light;
            public float Strength;
            public bool Wanted;
        }

        private struct Candidate
        {
            public Light Light;
            public float Score;
        }

        private static ShadowDirector instance;
        private readonly List<Casting> casting = new();
        private readonly List<Candidate> candidates = new();
        private float nextChoose, nextLayers;
        private int budget, atlas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("ShadowDirector");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ShadowDirector>();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // A new level: its lights are checked on the next frame, not a second later.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            nextChoose = 0f;
            nextLayers = 0f;
        }

        private void Update()
        {
            ShadowBudgetConfig config = ShadowBudgetConfig.Current;
            Camera eye = Camera.main;
            if (eye != null && Time.unscaledTime >= nextChoose)
            {
                nextChoose = Time.unscaledTime + ChooseEvery;
                Choose(eye.transform, config);
            }
            Fade(config);
            if (Time.unscaledTime >= nextLayers)
            {
                nextLayers = Time.unscaledTime + LayersEvery;
                KeepLayers();
            }
        }

        private void Choose(Transform eye, ShadowBudgetConfig config)
        {
            int setting = VideoSettings.Shadows;
            budget = config.Lights(setting);
            atlas = config.Atlas(setting);
            candidates.Clear();
            foreach (LightFixture fixture in LightFixture.All)
                if (fixture != null && fixture.Lit) Consider(fixture.Lamp, eye, config);
            foreach (ShadowBudgetLight extra in ShadowBudgetLight.All)
                if (extra != null && extra.Lamp != null && extra.Lamp.enabled) Consider(extra.Lamp, eye, config);
            candidates.Sort((a, b) => a.Score.CompareTo(b.Score));

            foreach (Casting c in casting) c.Wanted = false;
            int points = config.PointLights(setting), taken = 0;
            for (int i = 0; i < candidates.Count && taken < budget; i++)
            {
                Light light = candidates[i].Light;
                Casting c = Find(light);
                if (light.type == LightType.Point)
                {
                    // A point light is six shadow maps. One that is still fading out keeps its six until it is
                    // done, so a new one waits for it: the atlas is never asked for more than the budget's worth.
                    if (points <= 0 || (c == null && PointsCasting() >= config.PointLights(setting))) continue;
                    points--;
                }
                taken++;
                if (c == null)
                {
                    c = new Casting { Light = light };
                    casting.Add(c);
                    Unblock(light);
                    light.shadowStrength = 0f;
                    light.shadows = LightShadows.Soft;
                }
                c.Wanted = true;
            }
        }

        // Every managed light is seen here on every pass, so one that carries shadows from the scene file
        // (the old fixed four) loses them unless it is one of ours.
        private void Consider(Light light, Transform eye, ShadowBudgetConfig config)
        {
            if (light == null || (light.type != LightType.Spot && light.type != LightType.Point)) return;
            Casting current = Find(light);
            if (current == null && light.shadows != LightShadows.None) light.shadows = LightShadows.None;
            Vector3 to = light.transform.position - eye.position;
            float distance = to.magnitude;
            if (distance > config.MaxDistance) return;
            float score = distance;
            // Lights hang from the ceiling: the same storey is anything from just below the eye to a storey above it.
            if (to.y < -1f || to.y > config.StoreyHeight) score *= config.OtherStoreyPenalty;
            if (distance > 0.5f && Vector3.Dot(eye.forward, to / distance) < -0.2f) score *= config.BehindPenalty;
            if (current != null && current.Wanted) score *= config.KeepBonus;
            candidates.Add(new Candidate { Light = light, Score = score });
        }

        private void Fade(ShadowBudgetConfig config)
        {
            float step = Time.unscaledDeltaTime / config.FadeSeconds * config.Strength;
            for (int i = casting.Count - 1; i >= 0; i--)
            {
                Casting c = casting[i];
                if (c.Light == null)
                {
                    casting.RemoveAt(i);
                    continue;
                }
                c.Strength = Mathf.MoveTowards(c.Strength, c.Wanted ? config.Strength : 0f, step);
                c.Light.shadowStrength = c.Strength;
                if (c.Wanted || c.Strength > 0f) continue;
                c.Light.shadows = LightShadows.None;
                casting.RemoveAt(i);
            }
        }

        private int PointsCasting()
        {
            int count = 0;
            foreach (Casting c in casting)
                if (c.Light != null && c.Light.type == LightType.Point) count++;
            return count;
        }

        // A fixture's housing and diffuser sit round its lamp: casting, they would shadow their own light.
        // The rebuild switches them off (ShadowCasterPass); this covers a level saved before it did.
        private static void Unblock(Light light)
        {
            var fixture = light.GetComponentInParent<LightFixture>();
            if (fixture == null) return;
            foreach (Renderer renderer in fixture.GetComponentsInChildren<Renderer>(true))
                if (renderer.shadowCastingMode == ShadowCastingMode.On) renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private Casting Find(Light light)
        {
            foreach (Casting c in casting)
                if (c.Light == light) return c;
            return null;
        }

        // In a level split by storey, a light made after the rebuild's pass (a truck lamp, a flashlight, the
        // night-vision illuminator) would only reach the default layer. Everything that isn't a ceiling fixture
        // lights every layer.
        private static void KeepLayers()
        {
            LevelStoreys storeys = LevelStoreys.Current;
            if (storeys == null) return;
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                var fixture = light.GetComponentInParent<LightFixture>();
                storeys.Apply(light, fixture != null && fixture.Lamp == light);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            int fading = 0;
            foreach (Casting c in casting) if (!c.Wanted) fading++;
            GUI.Label(new Rect(Screen.width - 360f, 162f, 350f, 22f),
                $"SHADOWS {VideoSettings.ShadowNames[VideoSettings.Shadows]}: {casting.Count - fading}/{budget} lights ({fading} fading)  " +
                $"atlas {atlas}  storeys {(LevelStoreys.Current != null ? "on" : "off")}");
        }

        // Lines from the camera to the lights casting now (Scene view, with F1 on).
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Camera eye = Camera.main;
            if (eye == null) return;
            foreach (Casting c in casting)
            {
                if (c.Light == null) continue;
                Gizmos.color = c.Wanted ? new Color(1f, 0.9f, 0.3f, 0.8f) : new Color(1f, 0.4f, 0.2f, 0.4f);
                Gizmos.DrawLine(eye.transform.position, c.Light.transform.position);
                Gizmos.DrawWireSphere(c.Light.transform.position, 0.2f);
            }
        }
#endif

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;
    }
}
