using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// F1 (0.24.12): what the renderer and the mixer are doing, top right. While F1 is up, V shows the
    /// volumetric fog alone over black. Editor and Development builds only, like every F1 view.
    /// </summary>
    public class RenderDebugOverlay : MonoBehaviour
    {
        /// <summary>One line the audio side adds (the mixer snapshot and live voices); set by Audio.GameMixer.</summary>
        public static System.Func<string> AudioLine;

        private GUIStyle style;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || !DevTools.Enabled) return;
            var go = new GameObject("RenderDebugOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<RenderDebugOverlay>();
        }

        private void Update()
        {
            AutoExposureFeature.WantShownStops = DebugView.Visible;
            if (!DebugView.Visible)
            {
                VolumetricFogFeature.DebugFogOnly = false;
                return;
            }
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
                VolumetricFogFeature.DebugFogOnly = !VolumetricFogFeature.DebugFogOnly;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperRight };
            var fog = VolumeManager.instance.stack.GetComponent<VolumetricFogOverride>();
            string fogText = fog != null && fog.IsActive && VideoSettings.Volumetrics > 0
                ? $"fog {VideoSettings.VolumetricNames[VideoSettings.Volumetrics]} density {fog.density.value:0.000} 1/{VideoSettings.VolumetricDivisor[VideoSettings.Volumetrics]} res {VideoSettings.VolumetricSteps[VideoSettings.Volumetrics]} steps"
                : "fog off";
            var exposure = VolumeManager.instance.stack.GetComponent<AutoExposureOverride>();
            string eye = exposure != null && exposure.IsActive && VideoSettings.EyeAdaptation ? $"eye {AutoExposurePass.ShownStops:+0.00;-0.00} EV" : "eye off";
            string text = $"Forward+  {fogText}{(VolumetricFogFeature.DebugFogOnly ? " [V: fog only]" : " [V]")}\n{eye}  probes {(ProbeReferenceVolume.instance.isInitialized ? ProbeReferenceVolume.instance.lightingScenario ?? "-" : "off")}";
            if (AudioLine != null) text += "\n" + AudioLine();
            GUI.Label(new Rect(Screen.width - 620, 8, 610, 60), text, style);
        }
    }
}
