using Abandoned.Player;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// The wardrobe's 3D preview (UI step 8; 0.12.5): a mannequin far below the level, wearing the picked
    /// coverall, hat and accessory, facing its own camera with a gentle sway (drag in the wardrobe to turn
    /// it), drawn into a texture the wardrobe shows. Made on first use, off when the wardrobe closes. Local only.
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
        private float yaw, lastTurn = -10f;

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

        /// <summary>The wardrobe's drag: turn the mannequin by hand (the sway waits a moment before resuming).</summary>
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
            // Far below anything: the camera's short far plane keeps the level out of the picture.
            transform.position = new Vector3(0f, -800f, 0f);
            texture = new RenderTexture(640, 800, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4, name = "WardrobePreview" };
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
                // Face the camera (which looks back along -Z at the turntable): the face's eyes must be on +Z.
                foreach (Transform t in mannequin.GetComponentsInChildren<Transform>())
                    if (t.name == "Eye" && turntable.InverseTransformPoint(t.position).z < 0f)
                    {
                        mannequin.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                        break;
                    }
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
            if (cam == null || !cam.enabled) return;
            // Front-on with a slow sway; a hand turn holds where it was left for a few seconds first.
            float idle = Time.unscaledTime - lastTurn - 3f;
            float sway = Mathf.Sin(Time.unscaledTime * 0.5f) * 22f * Mathf.Clamp01(idle / 2f);
            if (idle > 0f) yaw = Mathf.MoveTowardsAngle(yaw, 0f, 40f * Time.unscaledDeltaTime);
            turntable.localRotation = Quaternion.Euler(0f, yaw + sway, 0f);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (texture != null) texture.Release();
        }
    }
}
