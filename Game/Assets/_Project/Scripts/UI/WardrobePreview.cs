using Abandoned.Player;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// The wardrobe's 3D preview (UI step 8): a mannequin far below the level, wearing the picked coverall,
    /// hat and accessory, turning slowly in front of its own camera, which draws into a texture the wardrobe
    /// shows. Made on first use, switched off when the wardrobe closes. Local only.
    /// </summary>
    public class WardrobePreview : MonoBehaviour
    {
        private static WardrobePreview instance;

        private Camera cam;
        private Transform turntable, crown;
        private WorkerRig worker;
        private GameObject hat, accessory;
        private RenderTexture texture;
        private CosmeticChoice shown;
        private bool dressed;

        /// <summary>Dress the mannequin and start filming; returns the picture to show.</summary>
        public static RenderTexture Show(CosmeticCatalog catalog, CosmeticChoice choice)
        {
            if (instance == null)
            {
                var go = new GameObject("WardrobePreview");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<WardrobePreview>();
                instance.Build();
            }
            instance.cam.enabled = true;
            instance.Dress(catalog, choice);
            return instance.texture;
        }

        public static void Hide()
        {
            if (instance != null && instance.cam != null) instance.cam.enabled = false;
        }

        private void Build()
        {
            // Far below anything: the camera's short far plane keeps the level out of the picture.
            transform.position = new Vector3(0f, -800f, 0f);
            texture = new RenderTexture(480, 600, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4, name = "WardrobePreview" };
            turntable = new GameObject("Turntable").transform;
            turntable.SetParent(transform, false);

            GameObject prefab = Resources.Load<GameObject>("WorkerVisual");
            if (prefab != null)
            {
                GameObject mannequin = Instantiate(prefab, turntable, false);
                mannequin.transform.localPosition = Vector3.up * 0.95f;
                worker = mannequin.GetComponent<WorkerRig>();
                crown = worker != null ? worker.HeadAnchor : mannequin.transform;
                foreach (TextMesh mark in mannequin.GetComponentsInChildren<TextMesh>()) mark.text = "";
            }
            else
            {
                crown = new GameObject("Crown").transform;
                crown.SetParent(turntable, false);
                crown.localPosition = Vector3.up * 1.76f;
            }

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(1.2f, 2.6f, 2.2f);
            lightGo.transform.LookAt(transform.position + Vector3.up * 1.1f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 6f;
            light.spotAngle = 60f;
            light.intensity = 9f;
            light.shadows = LightShadows.None;

            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.25f, 3.3f);
            camGo.transform.LookAt(transform.position + Vector3.up * 1.12f);
            cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.075f, 0.085f, 1f);
            cam.fieldOfView = 32f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 8f;
            cam.targetTexture = texture;
            cam.enabled = false;
        }

        private void Dress(CosmeticCatalog catalog, CosmeticChoice choice)
        {
            if (catalog == null || (dressed && shown.Equals(choice))) return;
            dressed = true;
            shown = choice;
            CosmeticDefinition suit = catalog.Coverall(choice.Coverall);
            worker?.Tint(suit != null ? suit.Color : Color.white);
            if (hat != null) Destroy(hat);
            if (accessory != null) Destroy(accessory);
            hat = Wear(catalog.Hat(choice.Hat));
            accessory = Wear(catalog.Accessory(choice.Accessory));
        }

        private GameObject Wear(CosmeticDefinition d)
        {
            if (d == null || d.HatPrefab == null) return null;
            GameObject worn = Instantiate(d.HatPrefab, crown, false);
            foreach (Collider c in worn.GetComponentsInChildren<Collider>()) Destroy(c);
            return worn;
        }

        private void Update()
        {
            if (cam != null && cam.enabled) turntable.Rotate(0f, 16f * Time.unscaledDeltaTime, 0f);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (texture != null) texture.Release();
        }
    }
}
