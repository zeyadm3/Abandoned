using Abandoned.Audio;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck as a place to breathe: a warm lamp and headlights, the engine idling when you're close, a
    /// rear-view mirror while you stand in the bay, and the doors closing as it pulls away. Local only.
    /// Its materials come from the builder (assets ship their shaders; a runtime shader lookup can find
    /// nothing in a build).
    /// </summary>
    public class TruckSanctuary : MonoBehaviour
    {
        private const float NearDistance = 14f;
        // The mirror is a small, far-off view: refreshing it every few frames is plenty.
        private const int MirrorFrameInterval = 3;

        [SerializeField] private Material doorMaterial;
        [SerializeField] private Material mirrorMaterial;
        [Tooltip("The warm lamp over the bay (intensity, range) and the two headlights (intensity, range, cone).")]
        [SerializeField] private Vector2 bayLamp = new(1.7f, 8f);
        [SerializeField] private Vector3 headlights = new(4f, 17f, 55f);
        [SerializeField, Range(0f, 1f)] private float idleVolume = 0.15f;

        private AudioSource idle;
        private TruckCargo truck;
        private Camera mirrorCamera;
        private RenderTexture reflection;
        private Material mirrorInstance;
        private Transform doors;
        private Vector3 cameraHome;
        private bool leaving;

        private void Awake()
        {
            truck = GetComponent<TruckCargo>();
            idle = gameObject.AddComponent<AudioSource>();
            idle.clip = HorrorAudio.Clip(HorrorAudio.Cue.EngineIdle);
            idle.loop = true;
            idle.playOnAwake = false;
            idle.spatialBlend = 1f;
            idle.minDistance = 2f;
            idle.maxDistance = 22f;
            idle.volume = 0f;
            AddLight(new Vector3(0f, 2.5f, 0f), new Color(1f, 0.65f, 0.29f), bayLamp.x, bayLamp.y);
            foreach (float side in new[] { -0.9f, 0.9f })
            {
                Light head = AddLight(new Vector3(side, 1.25f, 3.7f), new Color(1f, 0.78f, 0.43f), headlights.x, headlights.y);
                head.type = LightType.Spot;
                head.spotAngle = headlights.z;
            }
            BuildDoors();
            BuildMirror();
        }

        private void BuildDoors()
        {
            doors = new GameObject("DepartureDoors").transform;
            doors.SetParent(transform, false);
            doors.localPosition = new Vector3(0f, 1.55f, -2.56f);
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.transform.SetParent(doors, false);
            panel.transform.localScale = new Vector3(2.6f, 2.2f, 0.07f);
            Destroy(panel.GetComponent<Collider>());
            if (doorMaterial != null) panel.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            else if (Shader.Find("Universal Render Pipeline/Lit") is Shader lit) panel.GetComponent<Renderer>().material = new Material(lit) { color = new Color(0.18f, 0.2f, 0.18f) };
            doors.gameObject.SetActive(false);
        }

        private void BuildMirror()
        {
            Material source = mirrorMaterial;
            if (source == null)
            {
                Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlit == null) return; // no mirror rather than a pink one
                source = new Material(unlit);
            }
            reflection = new RenderTexture(256, 128, 16);
            reflection.Create();
            var cameraObject = new GameObject("RearMirrorCamera");
            cameraObject.transform.SetParent(transform, false);
            cameraHome = new Vector3(0f, 2.8f, -3.0f);
            cameraObject.transform.localPosition = cameraHome;
            cameraObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            mirrorCamera = cameraObject.AddComponent<Camera>();
            mirrorCamera.targetTexture = reflection;
            mirrorCamera.fieldOfView = 70f;
            mirrorCamera.farClipPlane = 80f;
            mirrorCamera.nearClipPlane = 0.2f;
            mirrorCamera.depth = -5f;
            mirrorCamera.enabled = false;
            // QA P-06: no shadows or post-processing for a 256 x 128 mirror.
            var data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderShadows = false;
            data.renderPostProcessing = false;
            GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mirror.name = "RearViewMirror";
            mirror.transform.SetParent(transform, false);
            mirror.transform.localPosition = new Vector3(0f, 2.6f, 2.38f);
            mirror.transform.localScale = new Vector3(1.1f, 0.55f, 1f);
            Destroy(mirror.GetComponent<Collider>());
            mirrorInstance = new Material(source) { mainTexture = reflection };
            mirror.GetComponent<Renderer>().sharedMaterial = mirrorInstance;
        }

        private Light AddLight(Vector3 at, Color color, float intensity, float range)
        {
            var go = new GameObject("TruckWarmLamp");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = at;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        private void Update()
        {
            RunState run = RunState.Current;
            Camera eye = Camera.main;
            bool near = eye != null && Vector3.Distance(eye.transform.position, transform.position) < NearDistance;
            if (run != null && run.IsSpawned && run.Elapsed < 7f) near = false;
            if (near && !idle.isPlaying) idle.Play();
            idle.volume = near ? idleVolume * AudioLevels.Sfx : 0f;
            if (mirrorCamera != null)
            {
                bool inBay = near && truck != null && eye != null && truck.Carries(eye.transform.position - Vector3.up);
                mirrorCamera.enabled = inBay && Time.frameCount % MirrorFrameInterval == 0;
            }
            if (run == null)
            {
                leaving = false;
                doors.gameObject.SetActive(false);
                if (mirrorCamera != null) mirrorCamera.transform.localPosition = cameraHome;
                return;
            }
            if (run.State.Phase == RunPhase.Honking && run.HonkRemaining < 2f)
            {
                if (!leaving)
                {
                    leaving = true;
                    HorrorAudio.Play(HorrorAudio.Cue.Door, transform.position, 0.9f);
                }
                doors.gameObject.SetActive(true);
                float progress = 1f - run.HonkRemaining / 2f;
                doors.localScale = new Vector3(Mathf.Max(0.02f, progress), 1f, 1f);
                if (mirrorCamera != null) mirrorCamera.transform.localPosition = cameraHome + Vector3.forward * progress * 8f;
                idle.pitch = 1f + progress;
            }
        }

        private void OnDestroy()
        {
            if (reflection != null)
            {
                reflection.Release();
                Destroy(reflection);
            }
            if (mirrorInstance != null) Destroy(mirrorInstance);
        }

#if UNITY_EDITOR
        public void EditorSetup(Material doorPanel, Material mirrorSurface)
        {
            doorMaterial = doorPanel;
            mirrorMaterial = mirrorSurface;
        }
#endif
    }
}
