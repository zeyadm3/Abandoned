using Abandoned.Company;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// The truck garage's 3D preview (2026 remake): the company truck (Resources/TruckPreview, built by the editor from
    /// the remade body) far below the level, wearing the fittings being tried, on a slow turntable, drawn into a texture
    /// the garage shows. Made on first use, off when the garage closes. Local only.
    /// </summary>
    public class TruckGaragePreview : MonoBehaviour
    {
        private static TruckGaragePreview instance;

        private Camera cam;
        private Transform turntable;
        private TruckLook look;
        private RenderTexture texture;
        private float yaw = 210f, lastTurn = -10f;

        public static RenderTexture Show(CompanyNetState state, TruckUpgradeCatalog upgrades)
        {
            if (instance == null)
            {
                var go = new GameObject("TruckGaragePreview");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<TruckGaragePreview>();
                instance.Build();
            }
            instance.cam.enabled = true;
            if (instance.look != null) instance.look.Show(state, upgrades);
            return instance.texture;
        }

        public static void Turn(float degrees)
        {
            if (instance == null) return;
            instance.yaw += degrees;
            instance.lastTurn = Time.unscaledTime;
        }

        public static void Hide()
        {
            if (instance != null && instance.cam != null) instance.cam.enabled = false;
        }

        private void Build()
        {
            transform.position = new Vector3(0f, -900f, 0f);
            texture = new RenderTexture(960, 600, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4, name = "TruckGaragePreview" };
            turntable = new GameObject("Turntable").transform;
            turntable.SetParent(transform, false);
            GameObject prefab = Resources.Load<GameObject>("TruckPreview");
            if (prefab != null)
            {
                GameObject truck = Instantiate(prefab, turntable, false);
                truck.transform.localPosition = new Vector3(0f, 0f, -1.2f);
                look = truck.GetComponent<TruckLook>();
            }
            foreach (var (pos, intensity) in new[] { (new Vector3(5f, 6f, 7f), 2.2f), (new Vector3(-6f, 3f, -4f), 1.0f) })
            {
                var lightGo = new GameObject("PreviewLight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = pos;
                lightGo.transform.LookAt(transform.position + Vector3.up * 1.5f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = intensity;
                light.shadows = LightShadows.None;
            }
            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.localPosition = new Vector3(0f, 3.4f, 11.5f);
            camGo.transform.LookAt(transform.position + Vector3.up * 1.6f);
            cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.075f, 0.085f, 1f);
            cam.fieldOfView = 34f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 30f;
            cam.targetTexture = texture;
            cam.enabled = false;
        }

        private void Update()
        {
            if (cam == null || !cam.enabled) return;
            if (Time.unscaledTime - lastTurn > 3f) yaw += 12f * Time.unscaledDeltaTime;
            turntable.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (texture != null) texture.Release();
        }
    }
}
