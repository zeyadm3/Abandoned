using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    /// The lights share one shadow atlas, and it is counted in shadow maps, not lights: a spot light is one
    /// map and a point light six. The pipeline halves every map (and says so in the console) the moment
    /// they don't all fit, so a light only starts casting when the atlas has room for it at full size.
    ///
    /// Also keeps lights made at run time on every rendering layer in a level split by storey
    /// (<see cref="LevelStoreys"/>). Local and cosmetic: nothing is networked, every machine has its own
    /// set. One per game, made on first scene load (never in batch mode).
    /// </summary>
    public class ShadowDirector : MonoBehaviour
    {
        private const float ChooseEvery = 0.2f, SweepEvery = 1f;

        private class Casting
        {
            public Light Light;
            public float Strength, Score;
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
        private readonly HashSet<Light> considered = new();
        private float nextChoose, nextSweep;
        private int budget, room, outside;

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
            nextSweep = 0f;
        }

        private void Update()
        {
            ShadowBudgetConfig config = ShadowBudgetConfig.Current;
            // Before choosing: in a new level the maps set aside must be that level's count, not the last one's.
            if (Time.unscaledTime >= nextSweep)
            {
                nextSweep = Time.unscaledTime + SweepEvery;
                Sweep();
            }
            budget = config.Lights(VideoSettings.Shadows);
            room = Room(config);
            // Quality and atlas changes apply before rendering, even between camera selection passes.
            Trim(config.PointLights(VideoSettings.Shadows));
            Camera eye = Camera.main;
            if (eye != null && Time.unscaledTime >= nextChoose)
            {
                nextChoose = Time.unscaledTime + ChooseEvery;
                Choose(eye.transform, config);
            }
            Fade(config);
        }

        private void Choose(Transform eye, ShadowBudgetConfig config)
        {
            int setting = VideoSettings.Shadows;
            candidates.Clear();
            considered.Clear();
            foreach (LightFixture fixture in LightFixture.All)
                if (fixture != null && fixture.Lit) Consider(fixture.Lamp, eye, config);
            foreach (ShadowBudgetLight extra in ShadowBudgetLight.All)
                if (extra != null && extra.Lamp != null && extra.Lamp.enabled) Consider(extra.Lamp, eye, config);
            foreach (SalvageWorkLight work in SalvageWorkLight.All)
                if (work != null && work.Lit) Consider(work.Lamp, eye, config);
            candidates.Sort((a, b) => a.Score.CompareTo(b.Score));

            foreach (Casting c in casting) c.Wanted = false;
            // chosen = the maps the lights picked so far need; held = the maps in the atlas right now, which
            // still includes every light that is fading out.
            int points = config.PointLights(setting), taken = 0, chosen = 0, held = MapsCasting();
            for (int i = 0; i < candidates.Count && taken < budget; i++)
            {
                Light light = candidates[i].Light;
                bool point = light.type == LightType.Point;
                int maps = Maps(light);
                if ((point && points <= 0) || chosen + maps > room) continue;
                if (point) points--;
                taken++;
                chosen += maps;
                Casting c = Find(light);
                if (c == null)
                {
                    // Its place is kept, so the lights it replaces fade out, but it starts only once they have
                    // let go of their maps (and a point light only once another point light has finished).
                    if (casting.Count >= budget || held + maps > room ||
                        (point && PointsCasting() >= config.PointLights(setting))) continue;
                    held += maps;
                    c = new Casting { Light = light };
                    casting.Add(c);
                    Unblock(light);
                    light.shadowStrength = 0f;
                    light.shadows = LightShadows.Soft;
                }
                c.Score = candidates[i].Score;
                c.Wanted = true;
            }
        }

        // Every managed light is seen here on every pass, so one that carries shadows from the scene file
        // (the old fixed four) loses them unless it is one of ours.
        private void Consider(Light light, Transform eye, ShadowBudgetConfig config)
        {
            if (light == null || !light.isActiveAndEnabled || !considered.Add(light) ||
                (light.type != LightType.Spot && light.type != LightType.Point)) return;
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

        // Fading lights still consume atlas space. Normal replacements wait; lowering quality must release
        // surplus maps immediately because the smaller atlas is already active this frame.
        private void Trim(int pointLimit)
        {
            for (int i = casting.Count - 1; i >= 0; i--)
                if (casting[i].Light == null || !casting[i].Light.isActiveAndEnabled || Maps(casting[i].Light) == 0) Stop(i);
            int points = PointsCasting(), maps = MapsCasting();
            while (casting.Count > budget || points > pointLimit || maps > room)
            {
                int remove = -1;
                for (int i = 0; i < casting.Count; i++)
                {
                    Casting c = casting[i];
                    if (points > pointLimit && c.Light.type != LightType.Point) continue;
                    if (remove < 0 || (!c.Wanted && casting[remove].Wanted) ||
                        (c.Wanted == casting[remove].Wanted && c.Score > casting[remove].Score)) remove = i;
                }
                if (remove < 0) break;
                Light light = casting[remove].Light;
                if (light.type == LightType.Point) points--;
                maps -= Maps(light);
                Stop(remove);
            }
        }

        private void Stop(int index)
        {
            Light light = casting[index].Light;
            if (light != null) { light.shadowStrength = 0f; light.shadows = LightShadows.None; }
            casting.RemoveAt(index);
        }

        private void Fade(ShadowBudgetConfig config)
        {
            float step = Time.unscaledDeltaTime / config.FadeSeconds * config.Strength;
            for (int i = casting.Count - 1; i >= 0; i--)
            {
                Casting c = casting[i];
                c.Strength = Mathf.MoveTowards(c.Strength, c.Wanted ? config.Strength : 0f, step);
                c.Light.shadowStrength = c.Strength;
                if (c.Wanted || c.Strength > 0f) continue;
                Stop(i);
            }
        }

        private int PointsCasting()
        {
            int count = 0;
            foreach (Casting c in casting)
                if (c.Light != null && c.Light.type == LightType.Point) count++;
            return count;
        }

        private int MapsCasting()
        {
            int maps = 0;
            foreach (Casting c in casting)
                if (c.Light != null) maps += Maps(c.Light);
            return maps;
        }

        private static int Maps(Light light) => light.type == LightType.Point ? 6 : light.type == LightType.Spot ? 1 : 0;

        // The shadow maps the atlas holds at full size, less those set aside for lights outside the budget.
        // Every light asks for the pipeline's High tier: it is the default and nothing in the project sets another.
        private int Room(ShadowBudgetConfig config)
        {
            UniversalRenderPipelineAsset pipeline = UniversalRenderPipeline.asset;
            if (pipeline == null) return 0;
            int across = pipeline.additionalLightsShadowmapResolution / Mathf.Max(1, pipeline.additionalLightsShadowResolutionTierHigh);
            return Mathf.Max(0, across * across - Mathf.Max(outside, config.ReservedMaps));
        }

        // A fixture's housing and diffuser sit round its lamp: casting, they would shadow their own light.
        // The rebuild switches them off (ShadowCasterPass); this covers a level saved before it did.
        private static void Unblock(Light light)
        {
            var fixture = light.GetComponentInParent<LightFixture>();
            var work = light.GetComponent<SalvageWorkLight>();
            Transform housing = fixture != null ? fixture.transform
                : work != null ? (work.Housing != null ? work.Housing : light.transform.parent) : null;
            if (housing == null) return;
            foreach (Renderer renderer in housing.GetComponentsInChildren<Renderer>(true))
                if (renderer.shadowCastingMode == ShadowCastingMode.On) renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private Casting Find(Light light)
        {
            foreach (Casting c in casting)
                if (c.Light == light) return c;
            return null;
        }

        // Once a second, over every light. A light that casts shadows without being one of ours (the player's
        // own flashlight) has its maps set aside, switched on or not, so it never has to wait for room.
        // And in a level split by storey, a light made after the rebuild's pass (a truck lamp, a flashlight,
        // the night-vision illuminator) would only reach the default layer: everything that isn't a ceiling
        // fixture lights every layer.
        private void Sweep()
        {
            LevelStoreys storeys = LevelStoreys.Current;
            int maps = 0;
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                var fixture = light.GetComponentInParent<LightFixture>();
                var extra = light.GetComponent<ShadowBudgetLight>();
                var work = light.GetComponent<SalvageWorkLight>();
                bool managed = (fixture != null && fixture.isActiveAndEnabled && fixture.Lamp == light) ||
                               (extra != null && extra.isActiveAndEnabled) ||
                               (work != null && work.isActiveAndEnabled && work.Lamp == light);
                if (Find(light) == null)
                {
                    if (managed) light.shadows = LightShadows.None;
                    else if (light.shadows != LightShadows.None) maps += Maps(light);
                }
                if (storeys == null) continue;
                storeys.Apply(light, fixture != null && fixture.Lamp == light);
            }
            outside = maps;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            int fading = 0;
            foreach (Casting c in casting) if (!c.Wanted) fading++;
            GUI.Label(new Rect(Screen.width - 360f, 162f, 350f, 22f),
                $"SHADOWS {VideoSettings.ShadowNames[VideoSettings.Shadows]}: {casting.Count - fading}/{budget} lights ({fading} fading)  " +
                $"maps {MapsCasting()}/{room}  storeys {(LevelStoreys.Current != null ? "on" : "off")}");
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
