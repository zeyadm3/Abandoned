using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.World
{
    /// <summary>
    /// A scene's sky, ambient light, reflections and fog: Unity only applies these from the active scene, and the world
    /// stays the active scene, so each place's are captured when the world is built and applied while it is loaded
    /// (each map keeps its own hour and weather; the world's own look comes back when it unloads).
    /// </summary>
    [System.Serializable]
    public class PlaceLook
    {
        public bool captured;
        public Material skybox;
        public AmbientMode ambientMode;
        public Color ambientSky, ambientEquator, ambientGround, ambientLight;
        public float ambientIntensity = 1f, reflectionIntensity = 1f;
        public bool fog;
        public FogMode fogMode;
        public Color fogColor;
        public float fogDensity, fogStart, fogEnd;

        /// <summary>What the active scene shows now.</summary>
        public static PlaceLook Capture() => new()
        {
            captured = true,
            skybox = RenderSettings.skybox,
            ambientMode = RenderSettings.ambientMode,
            ambientSky = RenderSettings.ambientSkyColor,
            ambientEquator = RenderSettings.ambientEquatorColor,
            ambientGround = RenderSettings.ambientGroundColor,
            ambientLight = RenderSettings.ambientLight,
            ambientIntensity = RenderSettings.ambientIntensity,
            reflectionIntensity = RenderSettings.reflectionIntensity,
            fog = RenderSettings.fog,
            fogMode = RenderSettings.fogMode,
            fogColor = RenderSettings.fogColor,
            fogDensity = RenderSettings.fogDensity,
            fogStart = RenderSettings.fogStartDistance,
            fogEnd = RenderSettings.fogEndDistance,
        };

        public void Apply()
        {
            if (!captured) return;
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSky;
            RenderSettings.ambientEquatorColor = ambientEquator;
            RenderSettings.ambientGroundColor = ambientGround;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.fog = fog;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
            DynamicGI.UpdateEnvironment();
        }
    }
}
